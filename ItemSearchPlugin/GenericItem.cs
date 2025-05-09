using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace ItemSearchPlugin {
    public class GenericItem {
        public enum ItemType {
            Item,
            EventItem
        }

        private EventItem eventItem = default;
        private Item item = default;
        private ItemType itemType;

        public GenericItem(EventItem eventItem) {
            this.eventItem = eventItem;

            itemType = ItemType.EventItem;
        }

        public GenericItem(Item item) {
            this.item = item;
            itemType = ItemType.Item;
        }

        public ItemType GenericItemType => itemType;
        
        public string Name {
            get {
                return itemType switch {
                    ItemType.EventItem => eventItem.Name.ToString(),
                    ItemType.Item => item.Name.ExtractText().Replace("\u00AD", string.Empty), //Remove the SoftHyphens from FR/DE
                    _ => string.Empty
                };
            }
        }

        public uint RowId {
            get {
                return itemType switch {
                    ItemType.EventItem => eventItem.RowId,
                    ItemType.Item => item.RowId,
                    _ => 0
                };
            }
        }
        
        public ushort Icon {
            get {
                return itemType switch {
                    ItemType.EventItem => eventItem.Icon,
                    ItemType.Item => item.Icon,
                    _ => 0
                };
            }
        }
        
        public byte Rarity {
            get {
                return itemType switch {
                    ItemType.EventItem => 1,
                    ItemType.Item => item.Rarity,
                    _ => 0
                };
            }
        }

        public uint LevelItem {
            get {
                return itemType switch {
                    ItemType.EventItem => 1,
                    ItemType.Item => item.LevelItem.RowId,
                    _ => 0
                };
            }
        }
        public byte LevelEquip {
            get {
                return itemType switch {
                    ItemType.EventItem => 255,
                    ItemType.Item => item.LevelEquip,
                    _ => 0
                };
            }
        }

        public bool CanBeHq {
            get {
                return itemType switch {
                    ItemType.EventItem => false,
                    ItemType.Item => item.CanBeHq,
                    _ => false
                };
            }
        }

        public RowRef<ClassJobCategory> ClassJobCategory {
            get {
                return itemType switch {
                    ItemType.EventItem => default,
                    ItemType.Item => item.ClassJobCategory,
                    _ => default
                };
            }
        }
        

        public static explicit operator Item(GenericItem genericItem) => genericItem.itemType == ItemType.Item ? genericItem.item : default;
        public static explicit operator EventItem(GenericItem genericItem) => genericItem.itemType == ItemType.EventItem ? genericItem.eventItem : default;
        public static implicit operator GenericItem(EventItem eventItem) => new GenericItem(eventItem);
        public static implicit operator GenericItem(Item item) => new GenericItem(item);


    }
}
