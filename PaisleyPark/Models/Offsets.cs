using PaisleyPark.MemoryCore;
using System;

namespace PaisleyPark.Models
{
    public class Offsets
    {
        private readonly MemoryService MemoryService;
        //public int WaymarkClassPtr { get; set; }
        //public int WaymarkFunc { get; set; }
        public IntPtr Waymarks { get; set; }
        //public int ActorTable { get; set; }
        public IntPtr MapID;
        private IntPtr UIModulePtrPtr;
        private Int32 WayMarkSlotOffset;

        //private IntPtr g_Framework_2_Ptr { get; set; }
        public IntPtr Slot1 => GetWaymarkDataPointerForSlot(1);
        public IntPtr Slot2 => GetWaymarkDataPointerForSlot(2);
        public IntPtr Slot3 => GetWaymarkDataPointerForSlot(3);
        public IntPtr Slot4 => GetWaymarkDataPointerForSlot(4);
        public IntPtr Slot5 => GetWaymarkDataPointerForSlot(5);

        public Offsets(MemoryService memoryService) {
            MemoryService = memoryService;
            MapID = MemoryService.Scanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? 45 8B CD 48 89 7C 24 ??") + 0x05C0 + 0x06;
            var WaymarkPtr = MemoryService.Scanner.GetStaticAddressFromSig("48 8B 94 24 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 41 B0 01") + 432;
            Waymarks = WaymarkPtr;
            //g_Framework_2_Ptr = MemoryService.Scanner.GetStaticAddressFromSig("48 8B 0D ?? ?? ?? ?? 0F B7 DA E8 ?? ?? ?? ?? 4C 8B C0");
            UIModulePtrPtr = MemoryService.Scanner.GetStaticAddressFromSig("48 8B 05 ?? ?? ?? ?? 48 8B D9 8B 40 14 85 C0");
            GetWayMarkSlotOffset();
        }

        private void GetWayMarkSlotOffset() {
            var UIModulePtr = MemoryService.Read<IntPtr>(UIModulePtrPtr);
            var UIModule = MemoryService.Read<IntPtr>(UIModulePtr);

            //.text:000000014063AE36                        loc_14063AE36:; CODE XREF: sub_14063ACD0 + 32¡üj
            //.text:000000014063AE36
            //.text:000000014063AE36 49 8B 00                               mov rax, [r8]; jumptable 000000014063AD02 case 17
            //.text:000000014063AE39 49 8B C8                               mov rcx, r8
            //.text:000000014063AE3C 48 83 C4 20                            add rsp, 20h
            //.text:000000014063AE40 5B                                     pop     rbx
            //.text:000000014063AE41 48 FF A0 70 01 00 00                   jmp qword ptr[rax + 170h]
            var FastCallAddressPtr = MemoryService.Read<IntPtr>(UIModule) + 0x170;
            var FastCallAddress = MemoryService.Read<IntPtr>(FastCallAddressPtr);

            //.text:00000001405BA9A0
            //.text:00000001405BA9A0                        sub_1405BA9A0   proc near; DATA XREF: .rdata: 0000000141649E10¡ýo
            //.text:00000001405BA9A0 48 8D 81 30 05 09 00                   lea rax, [rcx + 90530h]
            //.text:00000001405BA9A7 C3                                     retn
            //.text:00000001405BA9A7                        sub_1405BA9A0   endp
            WayMarkSlotOffset = MemoryService.Read<Int32>(FastCallAddress + 3);
        }

        private IntPtr GetWaymarkDataPointerForSlot(uint slotNum) {
            //var g_Framework_2 = MemoryService.Read<IntPtr>(g_Framework_2_Ptr);
            //var UIModule = MemoryService.Read<IntPtr>(g_Framework_2 + 0x29F8);
            var UIModulePtr = MemoryService.Read<IntPtr>(UIModulePtrPtr);
            var UIModule = MemoryService.Read<IntPtr>(UIModulePtr);

            var WayMarkSlotPtr = UIModule + WayMarkSlotOffset;
            var WaymarkDataPointer = WayMarkSlotPtr + 64 + (int)(104 * (slotNum - 1));
            //logger.Debug($"WaymarkDataPointer={WaymarkDataPointer.ToUint64().AsHex()}");
            return WaymarkDataPointer;
        }
    }
}