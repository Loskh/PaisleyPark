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

        public IntPtr UIModule { get; set; }
        public IntPtr Slot1 => GetWaymarkDataPointerForSlot(1);
        public IntPtr Slot2 => GetWaymarkDataPointerForSlot(2);
        public IntPtr Slot3 => GetWaymarkDataPointerForSlot(3);
        public IntPtr Slot4 => GetWaymarkDataPointerForSlot(4);
        public IntPtr Slot5 => GetWaymarkDataPointerForSlot(5);

        public Offsets(MemoryService memoryService) {
            MemoryService = memoryService;
            MapID = MemoryService.Scanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? 45 8B CD 48 89 7C 24 ??")+ 0x05C0 + 0x06;
            IntPtr WayMarkPtr = IntPtr.Add(MemoryService.Scanner.GetStaticAddressFromSig("48 8B 94 24 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 41 B0 01"), 432);
            UIModule = MemoryService.Scanner.GetStaticAddressFromSig("48 8B 0D ?? ?? ?? ?? 0F B7 DA E8 ?? ?? ?? ?? 4C 8B C0");
            Waymarks = WayMarkPtr;
            //MapID = MapIDPtr.ToInt64();
        }

        private IntPtr GetWaymarkDataPointerForSlot(uint slotNum) {
            IntPtr step1 = MemoryService.Read<IntPtr>(UIModule) + 0x29F8;
            //Console.WriteLine($"step1={step1.ToUint64().AsHex()}");
            IntPtr step2 = MemoryService.Read<IntPtr>(step1) + 0x90530;
            IntPtr WaymarkDataPointer = step2 + 64 + (int)(104 * (slotNum - 1));
            //logger.Debug($"WaymarkDataPointer={WaymarkDataPointer.ToUint64().AsHex()}");
            return WaymarkDataPointer;
        }
    }
}
