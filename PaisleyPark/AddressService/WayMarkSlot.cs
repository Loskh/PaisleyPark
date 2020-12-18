using PaisleyPark.MemoryCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaisleyPark.Address
{
    class WayMarkSlot
    {
        private readonly MemoryService MemoryService;

        private IntPtr UIModulePtrPtr;
        private Int32 WayMarkSlotOffset;
        //private IntPtr g_Framework_2_Ptr { get; set; }

        public WayMarkSlot(MemoryService memoryService) {
            MemoryService = memoryService;
            //g_Framework_2_Ptr = MemoryService.Scanner.GetStaticAddressFromSig("48 8B 0D ?? ?? ?? ?? 0F B7 DA E8 ?? ?? ?? ?? 4C 8B C0");
            UIModulePtrPtr = MemoryService.Scanner.GetStaticAddressFromSig("48 8B 05 ?? ?? ?? ?? 48 8B D9 8B 40 14 85 C0");
            //Console.WriteLine($"UIModulePtrPtr:{UIModulePtrPtr.ToOffset()}");
            GetWayMarkSlotOffset();
        }

        private void GetWayMarkSlotOffset() {
            Console.WriteLine($"GetWayMarkSlotOffset..");
            var UIModuleSwitch = new SwitchParser(MemoryService, "8B 94 98 ?? ?? ?? ?? 48 03 D0", 3);

            //.text:000000014063AE36                        loc_14063AE36:; CODE XREF: sub_14063ACD0 + 32↑j
            //.text:000000014063AE36
            //.text:000000014063AE36 49 8B 00                               mov rax, [r8]; jumptable 000000014063AD02 case 17
            //.text:000000014063AE39 49 8B C8                               mov rcx, r8
            //.text:000000014063AE3C 48 83 C4 20                            add rsp, 20h
            //.text:000000014063AE40 5B                                     pop     rbx
            //.text:000000014063AE41 48 FF A0 70 01 00 00                   jmp qword ptr[rax + 170h]
            var Case0x11 = UIModuleSwitch.Case(0x11);
            Console.WriteLine($"{Case0x11.ToOffset()}");
            var offset = MemoryService.Read<int>(Case0x11 + 14);

            var UIModulePtr = MemoryService.Read<IntPtr>(UIModulePtrPtr);
            Console.WriteLine($"UIModulePtr:{UIModulePtr.ToHex()}");
            var UIModule = MemoryService.Read<IntPtr>(UIModulePtr);
            Console.WriteLine($"UIModule:{UIModule.ToHex()}");
            var FastCallAddressPtr = MemoryService.Read<IntPtr>(UIModule) + offset;
            Console.WriteLine($"FastCallAddressPtr:{UIModule.ToHex()}");

            var FastCallAddress = MemoryService.Read<IntPtr>(FastCallAddressPtr);
            Console.WriteLine($"FastCallAddress:{FastCallAddress.ToOffset()}");
            //.text:00000001405BA9A0
            //.text:00000001405BA9A0                        sub_1405BA9A0   proc near; DATA XREF: .rdata: 0000000141649E10↓o
            //.text:00000001405BA9A0 48 8D 81 30 05 09 00                   lea rax, [rcx + 90530h]
            //.text:00000001405BA9A7 C3                                     retn
            //.text:00000001405BA9A7                        sub_1405BA9A0   endp
            WayMarkSlotOffset = MemoryService.Read<Int32>(FastCallAddress + 3);
            Console.WriteLine($"WayMarkSlotOffset:0x{WayMarkSlotOffset:X}");
        }


        public IntPtr GetWaymarkDataPointerForSlot(uint slotNum) {
            //var g_Framework_2 = MemoryService.Read<IntPtr>(g_Framework_2_Ptr);
            //var UIModule = MemoryService.Read<IntPtr>(g_Framework_2 + 0x29F8);
            var UIModulePtr = MemoryService.Read<IntPtr>(UIModulePtrPtr);
            var UIModule = MemoryService.Read<IntPtr>(UIModulePtr);

            var WayMarkSlotPtr = UIModule + WayMarkSlotOffset;
            Console.WriteLine($"WayMarkSlotPtr:{WayMarkSlotPtr.ToHex()}");
            var WaymarkDataPointer = WayMarkSlotPtr + 64 + (int)(104 * (slotNum - 1));
            Console.WriteLine($"Slot {slotNum} WaymarkDataPointer:{WaymarkDataPointer.ToHex()}");
            //logger.Debug($"WaymarkDataPointer={WaymarkDataPointer.ToUint64().AsHex()}");
            return WaymarkDataPointer;
        }
    }
}
