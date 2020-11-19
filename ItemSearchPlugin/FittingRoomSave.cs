using Newtonsoft.Json;

namespace ItemSearchPlugin {

    [JsonObject(ItemTypeNameHandling = TypeNameHandling.None)]
    public class FittingRoomSaveItem {
        public uint ItemID { get; set; } = 0;

        public byte Stain { get; set; } = 0;
    }

    [JsonObject(ItemTypeNameHandling = TypeNameHandling.None)]
    public class FittingRoomSave {
        public string Name { get; set; }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.None)]
        public FittingRoomSaveItem[] Items { get; set; }
    }
}
