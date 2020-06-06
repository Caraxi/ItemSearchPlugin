using Dalamud.Data.TransientSheet;
using ImGuiNET;
using System;

namespace ItemSearchPlugin.Filters {
    class LevelItemSearchFilter : ISearchFilter {
        private static readonly int MIN_LEVEL = 1;
        private static int MAX_LEVEL = 505;


        private int minLevel;
        private int maxLevel;

        private int last_minLevel;
        private int last_maxLevel;

        private ItemSearchPluginConfig config;

        public LevelItemSearchFilter(ItemSearchPluginConfig config) {
            minLevel = last_minLevel = MIN_LEVEL;
            maxLevel = last_maxLevel = MAX_LEVEL;

            MAX_LEVEL = Math.Max(MAX_LEVEL, config.MaxItemLevel);

            this.config = config;
        }


        public string Name => "Item Level";

        public string NameLocalizationKey => "SearchFilterLevelItem";

        public bool ShowFilter => config.ExtraFilters;

        public bool IsSet => minLevel != MIN_LEVEL || maxLevel != MAX_LEVEL;

        public bool HasChanged {
            get {
                if (minLevel != last_minLevel || maxLevel != last_maxLevel) {
                    last_maxLevel = maxLevel;
                    last_minLevel = minLevel;
                    return true;
                }

                return false;
            }
        }

        public bool CheckFilter(Item item) {
            if (item.LevelItem > MAX_LEVEL) {
                if (maxLevel == MAX_LEVEL) {
                    maxLevel = MAX_LEVEL = item.LevelItem;
                } else {
                    MAX_LEVEL = item.LevelItem;
                }
            }

            return item.LevelItem >= minLevel && item.LevelItem <= maxLevel;
        }

        public void Dispose() { }

        public void DrawEditor() {
            ImGui.PushItemWidth(-1);
            if (ImGui.DragIntRange2("##LevelItemSearchFilterRange", ref minLevel, ref maxLevel, 1f, MIN_LEVEL, MAX_LEVEL)) {
                // Force ImGui to behave
                // https://cdn.discordapp.com/attachments/653504487352303619/713825323967447120/ehS7GdAHKG.gif
                if (minLevel > maxLevel && minLevel != last_minLevel) minLevel = maxLevel;
                if (maxLevel < minLevel && maxLevel != last_maxLevel) maxLevel = minLevel;
                if (minLevel < MIN_LEVEL) minLevel = MIN_LEVEL;
                if (maxLevel > MAX_LEVEL) maxLevel = MAX_LEVEL;
            }

            ImGui.PopItemWidth();
        }
    }
}
