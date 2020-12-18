using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaisleyPark.MemoryCore;

namespace PaisleyPark.Address
{
    class SwitchParser
    {
        private readonly MemoryService MemoryService;
        private readonly List<IntPtr> jumpAddress;

        public SwitchParser(MemoryService memoryService, string sig, ushort offset = 0) {
            MemoryService = memoryService;
            jumpAddress = new List<IntPtr>();
            var jumpTableAddress = MemoryService.Scanner.ScanText(sig);
            var jumpTableIndexPtr = MemoryService.BaseAddress + MemoryService.Read<Int32>(jumpTableAddress + offset);
            GetCases(jumpTableIndexPtr);

        }

        public IntPtr Case(ushort i) {
            IntPtr Address = IntPtr.Zero;
            if (i < jumpAddress.Count)
                Address = jumpAddress[i];
            Console.WriteLine($"jumpAddress:{Address.ToOffset():X}");
            return Address;
        }

        private void GetCases(IntPtr jumpTableIndexPtr) {
            ushort maxTry = 0x50;
            ushort tryNum = 0;
            while (tryNum < maxTry) {
                var CaseOffset = MemoryService.Read<uint>(jumpTableIndexPtr);
                if (CaseOffset == 0xCCCCCCCC)
                    break;
                var address = MemoryService.BaseAddress + (int)CaseOffset;
                jumpAddress.Add(address);
                //Console.WriteLine($"GetJumpAddress:{address.ToOffset()}");
                tryNum++;
                jumpTableIndexPtr += 4;
            }

        }
    }
}
