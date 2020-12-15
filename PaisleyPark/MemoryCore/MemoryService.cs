﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace PaisleyPark.MemoryCore
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    public class MemoryService
    {
        private static Dictionary<Type, bool[]> structMasks = new Dictionary<Type, bool[]>();
        private static ulong memoryTickCount = 0;
        private readonly Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();

        public static IntPtr Handle { get; private set; }
        public static SignatureScanner Scanner { get; private set; }
        public static Process Process { get; private set; }
        public static bool IsProcessAlive { get; private set; }

        public MemoryService(Process process) {
            OpenProcess(process);
        }

        public static string GamePath {
            get {
                if (Process == null)
                    throw new Exception("No game process");

                if (Process.MainModule == null)
                    throw new Exception("Process has no main module");

                return Path.GetDirectoryName(Process.MainModule.FileName) + "\\..\\";
            }
        }

        public static bool GetIsProcessAlive() {
            if (Process == null || Process.HasExited)
                return false;

            if (!Process.Responding)
                return false;

            return true;
        }

        public static async Task WaitForMemoryTick() {
            ulong waitTillTick = memoryTickCount += 2;

            while (memoryTickCount <= waitTillTick) {
                await Task.Delay(16);
            }
        }

        public static IntPtr ReadPtr(IntPtr address) {
            byte[] d = new byte[8];
            ReadProcessMemory(Handle, address, d, 8, out _);
            long i = BitConverter.ToInt64(d, 0);
            IntPtr ptr = (IntPtr)i;
            return ptr;
        }

        public static T? Read<T>(UIntPtr address)
            where T : struct {
            unsafe {
                IntPtr ptr = (IntPtr)address.ToPointer();
                return Read<T>(ptr);
            }
        }

        public static T Read<T>(IntPtr address)
            where T : struct {
            if (address == IntPtr.Zero)
                throw new Exception("Invalid address");

            int attempt = 0;
            while (attempt < 10) {
                int size = Marshal.SizeOf(typeof(T));
                IntPtr mem = Marshal.AllocHGlobal(size);
                ReadProcessMemory(Handle, address, mem, size, out _);
                T? val = Marshal.PtrToStructure<T>(mem);
                Marshal.FreeHGlobal(mem);
                attempt++;

                if (val != null)
                    return (T)val;

                Thread.Sleep(100);
            }

            throw new Exception($"Failed to read memory {typeof(T)} from address {address}");
        }
        /// <summary>
        /// Write a value to the specified offset, determined by type.
        /// </summary>
        /// <param name="offset">Offset to write to.</param>
        /// <param name="data">Value to write.</param>
        /// <exception cref="ArgumentException">Gets thrown, when the type to write is unsupported.</exception>
        public void Write(IntPtr offset, object data) {
            var @writeMethods = new Dictionary<Type, Action>
            {
                {typeof(byte[]), () => WriteBytes(offset, (byte[]) data)},
                {typeof(byte), () => WriteBytes(offset, new byte[] {(byte) data})},

                {typeof(char), () => WriteBytes(offset, new byte[] {(byte) data})},
                {typeof(short), () => WriteBytes(offset, BitConverter.GetBytes((short) data))},
                {typeof(ushort), () => WriteBytes(offset, BitConverter.GetBytes((ushort) data))},
                {typeof(int), () => WriteBytes(offset, BitConverter.GetBytes((int) data))},
                {typeof(uint), () => WriteBytes(offset, BitConverter.GetBytes((uint) data))},
                {typeof(long), () => WriteBytes(offset, BitConverter.GetBytes((long) data))},
                {typeof(ulong), () => WriteBytes(offset, BitConverter.GetBytes((ulong) data))},
                {typeof(float), () => WriteBytes(offset, BitConverter.GetBytes((float) data))},
                {typeof(double), () => WriteBytes(offset, BitConverter.GetBytes((double) data))},
            };

            if (@writeMethods.ContainsKey(data.GetType()))
                @writeMethods[data.GetType()]();
            else
                throw new ArgumentException("Unsupported type.");
        }

        public static void Write<T>(IntPtr address, T value)
            where T : struct {
            if (address == IntPtr.Zero)
                return;

            // Read the existing memory to oldBuffer
            int size = Marshal.SizeOf(typeof(T));
            byte[] oldBuffer = new byte[size];
            ReadProcessMemory(Handle, address, oldBuffer, size, out _);

            // Marshal the struct to newBuffer
            byte[] newbuffer = new byte[size];
            IntPtr mem = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr<T>(value, mem, false);
            Marshal.Copy(mem, newbuffer, 0, size);
            Marshal.FreeHGlobal(mem);

            // Apply only memory that is allowed by the mask.
            // this prevents writing memory for values that we dont have in our structs.
            bool[] mask = GetMask<T>();
            int diff = 0;
            for (int i = 0; i < size; i++) {
                if (mask[i] && oldBuffer[i] != newbuffer[i]) {
                    oldBuffer[i] = newbuffer[i];
                    diff++;
                }
            }

            // No change, nothing to write.
            if (diff <= 0)
                return;

            ////Log.Write("Writing: " + diff + " bytes, " + value.GetType().Name + " to " + address);

            // Write the oldBuffer (which has now had newBuffer merged over it) to the process
            WriteProcessMemory(Handle, address, oldBuffer, size, out _);
        }

        public static bool Read(UIntPtr address, byte[] buffer, UIntPtr size) {
            return ReadProcessMemory(Handle, address, buffer, size, IntPtr.Zero);
        }

        public static bool Read(IntPtr address, byte[] buffer, int size = -1) {
            if (size <= 0)
                size = buffer.Length;

            return ReadProcessMemory(Handle, address, buffer, size, out _);
        }

        public static bool WriteBytes(IntPtr address, byte[] buffer) {
            return WriteProcessMemory(Handle, address, buffer, buffer.Length, out _);
        }


        /// <summary>
        /// Open the PC game process with all security and access rights.
        /// </summary>
        public void OpenProcess(Process process) {
            Process = process;

            if (!Process.Responding)
                throw new Exception("Target process id not responding");

            if (process.MainModule == null)
                throw new Exception("Process has no main module");

            Process.EnterDebugMode();
            int debugPrivilegeCheck = CheckSeDebugPrivilege(out bool isDebugEnabled);
            if (debugPrivilegeCheck != 0) {
                throw new Exception($"ERROR: CheckSeDebugPrivilege failed with error: {debugPrivilegeCheck}");
            }
            else if (!isDebugEnabled) {
                throw new Exception("ERROR: SeDebugPrivilege not enabled. Please report this!");
            }

            Handle = OpenProcess(0x001F0FFF, true, process.Id);
            if (Handle == IntPtr.Zero) {
                int eCode = Marshal.GetLastWin32Error();
            }

            // Set all modules
            this.modules.Clear();
            foreach (ProcessModule module in Process.Modules) {
                if (module == null)
                    continue;

                if (string.IsNullOrEmpty(module.ModuleName))
                    continue;

                if (this.modules.ContainsKey(module.ModuleName))
                    continue;

                this.modules.Add(module.ModuleName, module.BaseAddress);
            }

            Scanner = new SignatureScanner(process.MainModule);
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll")]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool PrivilegeCheck(IntPtr clientToken, ref PRIVILEGE_SET requiredPrivileges, out bool pfResult);

        [DllImport("kernel32.dll")]
        private static extern int CloseHandle(IntPtr hObject);

        /// <summary>
        /// Gets or generates a new mask for the given struct.
        /// The mask indicates which bytes of memory the struct uses, and which bytes should not be
        /// changed in memory.
        /// </summary>
        private static bool[] GetMask<T>()
            where T : struct {
            Type type = typeof(T);

            if (structMasks.ContainsKey(type))
                return structMasks[type];

            int size = Marshal.SizeOf(type);
            byte[] buffer = new byte[size];
            byte[] buffer2 = new byte[size];

            // Write 255 to all bytes in the buffer
            for (int i = 0; i < size; i++)
                buffer[i] = 255;

            // read buffer2 to a struct
            IntPtr mem = Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, mem, size);
            T val = Marshal.PtrToStructure<T>(mem);
            Marshal.FreeHGlobal(mem);

            // write the struct to buffer2
            mem = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(val, mem, false);
            Marshal.Copy(mem, buffer2, 0, size);
            Marshal.FreeHGlobal(mem);

            // generate a mask fore ach bit
            bool[] mask = new bool[size];
            for (int i = 0; i < size; i++) {
                // if the buffer bit (255) has not been changed to the default bit (0) then
                // the bit was written to by the marshaling.
                mask[i] = buffer[i] == buffer2[i];
            }

            structMasks.Add(type, mask);
            return mask;
        }

        private static int CheckSeDebugPrivilege(out bool isDebugEnabled) {
            isDebugEnabled = false;

            if (!OpenProcessToken(GetCurrentProcess(), 0x8 /*TOKEN_QUERY*/, out IntPtr tokenHandle))
                return Marshal.GetLastWin32Error();

            LUID luidDebugPrivilege = default;
            if (!LookupPrivilegeValue(null, "SeDebugPrivilege", ref luidDebugPrivilege))
                return Marshal.GetLastWin32Error();

            PRIVILEGE_SET requiredPrivileges = new PRIVILEGE_SET {
                PrivilegeCount = 1,
                Control = 1 /* PRIVILEGE_SET_ALL_NECESSARY */,
                Privilege = new LUID_AND_ATTRIBUTES[1],
            };

            requiredPrivileges.Privilege[0].Luid = luidDebugPrivilege;
            requiredPrivileges.Privilege[0].Attributes = 2 /* SE_PRIVILEGE_ENABLED */;

            if (!PrivilegeCheck(tokenHandle, ref requiredPrivileges, out bool bResult))
                return Marshal.GetLastWin32Error();

            // bResult == true => SeDebugPrivilege is on; otherwise it's off
            isDebugEnabled = bResult;

            CloseHandle(tokenHandle);

            return 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PRIVILEGE_SET
        {
            public uint PrivilegeCount;
            public uint Control;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public LUID_AND_ATTRIBUTES[] Privilege;
        }

        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }
    }
}
