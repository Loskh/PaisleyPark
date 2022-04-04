using PaisleyPark.MemoryCore;
using System;

namespace PaisleyPark.Address
{
    public class Offsets
    {
        private readonly MemoryService MemoryService;

        private readonly WayMarkSlot WayMarkSlot;
        //public int WaymarkClassPtr { get; set; }
        //public int WaymarkFunc { get; set; }
        public IntPtr Waymarks { get; set; }
        //public int ActorTable { get; set; }
        public IntPtr MapID;

        public IntPtr Slot1 => WayMarkSlot.GetWaymarkDataPointerForSlot(1);
        public IntPtr Slot2 => WayMarkSlot.GetWaymarkDataPointerForSlot(2);
        public IntPtr Slot3 => WayMarkSlot.GetWaymarkDataPointerForSlot(3);
        public IntPtr Slot4 => WayMarkSlot.GetWaymarkDataPointerForSlot(4);
        public IntPtr Slot5 => WayMarkSlot.GetWaymarkDataPointerForSlot(5);

        public Offsets(MemoryService memoryService) {
            MemoryService = memoryService;
            WayMarkSlot = new WayMarkSlot(MemoryService);

            //MapID = MemoryService.Scanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? 45 8B CD 48 89 7C 24 ??") + 0x05C0 + 0x06;
            var mapIDOffset = MemoryService.Read<UInt16>(MemoryService.Scanner.ScanText("66 89 81 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 44 8B CF")+3);
            MapID = MemoryService.Scanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? 0F B6 55 ??") + mapIDOffset;
            Waymarks = MemoryService.Scanner.GetStaticAddressFromSig("48 8B 94 24 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 41 B0 01") + 432;
        }
    }
}