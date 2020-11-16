namespace ItemSearchPlugin {
    public class FittingRoomSaveItem {
        public uint ItemID { get; set; } = 0;
        public byte Stain { get; set; } = 0;
    }

    public class FittingRoomSave {
        public string Name { get; set; }
        public FittingRoomSaveItem[] Items { get; set; }
    }
}
