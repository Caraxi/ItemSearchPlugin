using Lumina.Excel.GeneratedSheets;
using ImGuiNET;
using Item = ItemSearchPlugin.ItemTemp;
namespace ItemSearchPlugin.Filters {
    class LevelEquipSearchFilter : SearchFilter {
        private const int MinLevel = 1;
        private const int MaxLevel = 80;


        private int minLevel;
        private int maxLevel;

        private int lastMinLevel;
        private int lastMaxLevel;

        public LevelEquipSearchFilter(ItemSearchPluginConfig config) : base(config) {
            minLevel = lastMinLevel = MinLevel;
            maxLevel = lastMaxLevel = MaxLevel;
        }


        public override string Name => "Equip Level";

        public override string NameLocalizationKey => "SearchFilterLevelEquip";

        public override bool IsSet => minLevel != MinLevel || maxLevel != MaxLevel;

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
            return item.LevelEquip >= minLevel && item.LevelEquip <= maxLevel;
        }

        public override void DrawEditor() {
            ImGui.PushItemWidth(-1);
            if (ImGui.DragIntRange2("##LevelEquipSearchFilterRange", ref minLevel, ref maxLevel, 1f, MinLevel, MaxLevel)) {
                // Force ImGui to behave
                // https://cdn.discordapp.com/attachments/653504487352303619/713825323967447120/ehS7GdAHKG.gif
                if (minLevel > maxLevel && minLevel != lastMinLevel) minLevel = maxLevel;
                if (maxLevel < minLevel && maxLevel != lastMaxLevel) maxLevel = minLevel;
                if (minLevel < MinLevel) minLevel = MinLevel;
                if (maxLevel > MaxLevel) maxLevel = MaxLevel;
            }

            ImGui.PopItemWidth();
        }
    }
}
