using System.Runtime.InteropServices;

namespace ItemSearchPlugin {

    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public unsafe struct FittingRoomItem {

        [FieldOffset(0x00)] public byte Slot;

        [FieldOffset(0x03)] public byte Stain;
        [FieldOffset(0x04)] public byte StainAgain;

        [FieldOffset(0x08)] public uint BaseItem;
        [FieldOffset(0x0C)] public uint GlamourItem;

        // [FieldOffset(0x10)] public ulong Unknown16;
        // [FieldOffset(0x18)] public ulong Unknown24;

    }


    [StructLayout(LayoutKind.Explicit, Size = 0x298)]
    public unsafe struct FittingRoomStruct {

        [FieldOffset(0xD0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public FittingRoomItem[] Items;

        [FieldOffset(0x291)] public byte ReRender;


    }
}
