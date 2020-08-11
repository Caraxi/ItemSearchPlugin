using Lumina.Excel.GeneratedSheets;
using ImGuiNET;
using System;
using Item = ItemSearchPlugin.ItemTemp;
namespace ItemSearchPlugin.Filters {
    class LevelItemSearchFilter : SearchFilter {
        private const uint MinItemLevel = 1;
        private uint maxItemLevel = 505;

        private uint minLevel;
        private uint maxLevel;

        private uint lastMinLevel;
        private uint lastMaxLevel;

        public LevelItemSearchFilter(ItemSearchPluginConfig config) : base(config) {
            minLevel = lastMinLevel = MinItemLevel;
            maxLevel = lastMaxLevel = maxItemLevel;

            maxItemLevel = Math.Max(maxItemLevel, config.MaxItemLevel);
        }


        public override string Name => "Item Level";

        public override string NameLocalizationKey => "SearchFilterLevelItem";

        public override bool IsSet => minLevel != MinItemLevel || maxLevel != maxItemLevel;

        public override bool HasChanged {
            get {
                if (minLevel != lastMinLevel || maxLevel != lastMaxLevel) {
                    lastMaxLevel = maxLevel;
                    lastMinLevel = minLevel;
                    return true;
                }

                return false;
            }
        }

        public override bool CheckFilter(Item item) {
            if (item.LevelItem.Row > maxItemLevel) {
                if (maxLevel == maxItemLevel) {
                    maxLevel = maxItemLevel = item.LevelItem.Row;
                } else {
                    maxItemLevel = item.LevelItem.Row;
                }
            }

            return item.LevelItem.Row >= minLevel && item.LevelItem.Row <= maxLevel;
        }

        public override void DrawEditor() {
            ImGui.PushItemWidth(-1);
            var minLevelInt = (int) minLevel;
            var maxLevelInt = (int) maxLevel;
            if (ImGui.DragIntRange2("##LevelItemSearchFilterRange", ref minLevelInt, ref maxLevelInt, 1f, (int) MinItemLevel, (int) maxItemLevel)) {
                // Force ImGui to behave
                // https://cdn.discordapp.com/attachments/653504487352303619/713825323967447120/ehS7GdAHKG.gif
                if (minLevelInt > maxLevelInt && minLevelInt != lastMinLevel) minLevelInt = maxLevelInt;
                if (maxLevelInt < minLevelInt && maxLevelInt != lastMaxLevel) maxLevelInt = minLevelInt;
                if (minLevelInt < MinItemLevel) minLevel = MinItemLevel;
                if (maxLevelInt > maxItemLevel) maxLevel = maxItemLevel;

                minLevel = (uint) minLevelInt;
                maxLevel = (uint) maxLevelInt;
            }

            ImGui.PopItemWidth();
        }
    }
}
