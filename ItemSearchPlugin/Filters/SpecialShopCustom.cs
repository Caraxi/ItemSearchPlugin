// Kept for further reference of the logic in SoldByNPCSearchFilter.cs
//
//
//using Lumina.Data;
//using Lumina.Excel;
//using Lumina.Excel.GeneratedSheets;

//namespace ItemSearchPlugin {

//    [Sheet("SpecialShop")]
//    internal class SpecialShopCustom : SpecialShop {
//        public override void PopulateData(RowParser parser, Lumina.GameData lumina, Language language) {
//            base.PopulateData(parser, lumina, language);

//            Entries = new Entry[60];
            
//            for (var i = 0; i < Entries.Length; i++) {
//                Entries[i] = new Entry {
//                    Result = new[] {
//                        new ResultEntry {
//                            Item = new LazyRow<Item>(lumina, parser.ReadColumn<int>(1 + i), language),
//                            Count = parser.ReadColumn<uint>(61 + i),
//                            SpecialShopItemCategory = new LazyRow<SpecialShopItemCategory>(lumina, parser.ReadColumn<int>(121 + i), language),
//                            HQ = parser.ReadColumn<bool>(181 + i)
//                        },
//                        new ResultEntry {
//                            Item = new LazyRow<Item>(lumina, parser.ReadColumn<int>(241 + i), language),
//                            Count = parser.ReadColumn<uint>(301 + i),
//                            SpecialShopItemCategory = new LazyRow<SpecialShopItemCategory>(lumina, parser.ReadColumn<int>(361 + i), language),
//                            HQ = parser.ReadColumn<bool>(421 + i)
//                        }
//                    },
//                    Cost = new[] {
//                        new CostEntry {
//                            Item = new LazyRow<Item>(lumina, parser.ReadColumn<int>(481 + i), language),
//                            Count = parser.ReadColumn<uint>(541 + i),
//                            HQ = parser.ReadColumn<bool>(601 + i),
//                            Collectability = parser.ReadColumn<ushort>(661 + i)
//                        },
//                        new CostEntry {
//                            Item = new LazyRow<Item>(lumina, parser.ReadColumn<int>(721 + i), language),
//                            Count = parser.ReadColumn<uint>(781 + i),
//                            HQ = parser.ReadColumn<bool>(841 + i),
//                            Collectability = parser.ReadColumn<ushort>(901 + i)
//                        },
//                        new CostEntry {
//                            Item = new LazyRow<Item>(lumina, parser.ReadColumn<int>(961 + i), language),
//                            Count = parser.ReadColumn<uint>(1021 + i),
//                            HQ = parser.ReadColumn<bool>(1081 + i),
//                            Collectability = parser.ReadColumn<ushort>(1141 + i)
//                        }
//                    },
//                    Quest = new LazyRow<Quest>(lumina, parser.ReadColumn<int>(1201 + i), language),
//                    Unknown6 = parser.ReadColumn<int>(1261 + i),
//                    Unknown7 = parser.ReadColumn<byte>(1321 + i),
//                    Unknown8 = parser.ReadColumn<byte>(1381 + i),
//                    AchievementUnlock = new LazyRow<Achievement>(lumina, parser.ReadColumn<int>(1441 + i), language),
//                    Unknown10 = parser.ReadColumn<byte>(1501 + i),
//                    PatchNumber = parser.ReadColumn<ushort>(1561 + i)
//                };
//            }
//        }

//        public struct Entry {
//            public ResultEntry[] Result;
//            public CostEntry[] Cost;
//            public LazyRow<Quest> Quest;
//            public int Unknown6;
//            public byte Unknown7;
//            public byte Unknown8;
//            public LazyRow<Achievement> AchievementUnlock;
//            public int Unknown10;
//            public ushort PatchNumber;
//        }

//        public struct ResultEntry {
//            public LazyRow<Item> Item;
//            public uint Count;
//            public LazyRow<SpecialShopItemCategory> SpecialShopItemCategory;
//            public bool HQ;
//        }

//        public struct CostEntry {
//            public LazyRow<Item> Item;
//            public uint Count;
//            public bool HQ;
//            public ushort Collectability;
//        }

//        public Entry[] Entries;
//    }
//}
