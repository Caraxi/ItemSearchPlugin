using Lumina.Excel.GeneratedSheets;
using ImGuiNET;
using System;

namespace ItemSearchPlugin.Filters {
    class LevelItemSearchFilter : SearchFilter {
        private static readonly uint MIN_LEVEL = 1;
        private static uint MAX_LEVEL = 505;

        private uint minLevel;
        private uint maxLevel;

        private uint last_minLevel;
        private uint last_maxLevel;

        public LevelItemSearchFilter(ItemSearchPluginConfig config) : base(config) {
            minLevel = last_minLevel = MIN_LEVEL;
            maxLevel = last_maxLevel = MAX_LEVEL;

            MAX_LEVEL = Math.Max(MAX_LEVEL, config.MaxItemLevel);
        }


        public override string Name => "Item Level";

        public override string NameLocalizationKey => "SearchFilterLevelItem";

        public override bool IsSet => minLevel != MIN_LEVEL || maxLevel != MAX_LEVEL;

        public override bool HasChanged {
            get {
                if (minLevel != last_minLevel || maxLevel != last_maxLevel) {
                    last_maxLevel = maxLevel;
                    last_minLevel = minLevel;
                    return true;
                }

                return false;
            }
        }

        public override bool CheckFilter(Item item) {
            if (item.LevelItem.Row > MAX_LEVEL) {
                if (maxLevel == MAX_LEVEL) {
                    maxLevel = MAX_LEVEL = item.LevelItem.Row;
                } else {
                    MAX_LEVEL = item.LevelItem.Row;
                }
            }

            return item.LevelItem.Row >= minLevel && item.LevelItem.Row <= maxLevel;
        }

        public override void DrawEditor() {
            ImGui.PushItemWidth(-1);
            int minLevel = (int) this.minLevel;
            int maxLevel = (int) this.maxLevel;
            if (ImGui.DragIntRange2("##LevelItemSearchFilterRange", ref minLevel, ref maxLevel, 1f, (int) MIN_LEVEL, (int) MAX_LEVEL)) {
                // Force ImGui to behave
                // https://cdn.discordapp.com/attachments/653504487352303619/713825323967447120/ehS7GdAHKG.gif
                if (minLevel > maxLevel && minLevel != last_minLevel) minLevel = maxLevel;
                if (maxLevel < minLevel && maxLevel != last_maxLevel) maxLevel = minLevel;
                if (minLevel < MIN_LEVEL) this.minLevel = MIN_LEVEL;
                if (maxLevel > MAX_LEVEL) this.maxLevel = MAX_LEVEL;

                this.minLevel = (uint) minLevel;
                this.maxLevel = (uint) maxLevel;
            }

            ImGui.PopItemWidth();
        }
    }
}
