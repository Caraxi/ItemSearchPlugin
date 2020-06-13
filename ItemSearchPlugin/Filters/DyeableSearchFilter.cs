using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.Filters {
    class DyeableSearchFilter : SearchFilter {
        private bool showDyeable = true;
        private bool showNotDyeable = true;

        private bool changed;

        public DyeableSearchFilter(ItemSearchPluginConfig pluginConfig) : base(pluginConfig) { }

        public override string Name { get; } = "Dyeability";
        public override string NameLocalizationKey { get; } = "DyeableSearchFilter";
        public override bool ShowFilter => PluginConfig.ExtraFilters;
        public override bool IsSet => showDyeable == false || showNotDyeable == false;

        public override bool HasChanged {
            get {
                if (changed) {
                    changed = false;
                    return true;
                }

                return false;
            }
        }

        public override bool CheckFilter(Item item) {
            return item.IsDyeable ? showDyeable : showNotDyeable;
        }

        public override void DrawEditor() {
            if (ImGui.Checkbox("Dyeable", ref showDyeable)) {
                changed = true;
            }

            ImGui.SameLine();
            if (ImGui.Checkbox("Not Dyeable", ref showNotDyeable)) {
                changed = true;
            }
        }
    }
}
