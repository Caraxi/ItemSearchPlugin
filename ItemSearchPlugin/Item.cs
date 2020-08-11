using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin {
    [Sheet("Item")]
    public class ItemTemp : IExcelRow {
        public string Name;
        public ushort Icon;
        public LazyRow<ItemLevel> LevelItem;
        public byte Rarity;
        public LazyRow<Lumina.Excel.GeneratedSheets.ItemUICategory> ItemUICategory;
        public LazyRow<Lumina.Excel.GeneratedSheets.ItemSearchCategory> ItemSearchCategory;
        public LazyRow<Lumina.Excel.GeneratedSheets.EquipSlotCategory> EquipSlotCategory;
        public bool CanBeHq;
        public bool IsDyeable;
        public byte LevelEquip;
        public byte EquipRestriction;
        public LazyRow<Lumina.Excel.GeneratedSheets.ClassJobCategory> ClassJobCategory;
        public UnkStruct59Struct[] UnkStruct59;

        public uint RowId { get; set; }

        public uint SubRowId { get; set; }

        public void PopulateData(RowParser parser, Lumina.Lumina lumina, Language language) {
            this.RowId = parser.Row;
            this.SubRowId = parser.SubRow;
            this.Name = parser.ReadColumn<string>(9);
            this.Icon = parser.ReadColumn<ushort>(10);
            this.LevelItem = new LazyRow<ItemLevel>(lumina, (int) parser.ReadColumn<ushort>(11), language);
            this.Rarity = parser.ReadColumn<byte>(12);
            this.ItemUICategory = new LazyRow<Lumina.Excel.GeneratedSheets.ItemUICategory>(lumina, (int) parser.ReadColumn<byte>(15), language);
            this.ItemSearchCategory = new LazyRow<Lumina.Excel.GeneratedSheets.ItemSearchCategory>(lumina, (int) parser.ReadColumn<byte>(16), language);
            this.EquipSlotCategory = new LazyRow<Lumina.Excel.GeneratedSheets.EquipSlotCategory>(lumina, (int) parser.ReadColumn<byte>(17), language);
            this.CanBeHq = parser.ReadColumn<bool>(26);
            this.LevelEquip = parser.ReadColumn<byte>(40);
            this.EquipRestriction = parser.ReadColumn<byte>(42);
            this.ClassJobCategory = new LazyRow<Lumina.Excel.GeneratedSheets.ClassJobCategory>(lumina, (int) parser.ReadColumn<byte>(43), language);
            this.UnkStruct59 = new UnkStruct59Struct[6];
            for (int index = 0; index < 6; ++index) {
                this.UnkStruct59[index] = new UnkStruct59Struct();
                this.UnkStruct59[index].BaseParam = parser.ReadColumn<byte>(59 + index * 2);
                this.UnkStruct59[index].BaseParamValue = parser.ReadColumn<short>(59 + (index * 2 + 1));
            }
        }

        public struct UnkStruct59Struct {
            public byte BaseParam;
            public short BaseParamValue;
        }

    }
}
