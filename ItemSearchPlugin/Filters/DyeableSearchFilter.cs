using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.Filters {
    class DyeableSearchFilter : ISearchFilter {
        private readonly ItemSearchPluginConfig pluginConfig;

        public void Dispose() { }

        private bool showDyeable = true;
        private bool showNotDyeable = true;

        private bool changed;

        public DyeableSearchFilter(ItemSearchPluginConfig pluginConfig) {
            this.pluginConfig = pluginConfig;
        }

        public string Name { get; } = "Dyeability";
        public string NameLocalizationKey { get; } = "DyeableSearchFilter";
        public bool ShowFilter => pluginConfig.ExtraFilters;
        public bool IsSet => showDyeable == false || showNotDyeable == false;

        public bool HasChanged {
            get {
                if (changed) {
                    changed = false;
                    return true;
                }

                return false;
            }
        }

        public bool CheckFilter(Item item) {
            return item.IsDyeable ? showDyeable : showNotDyeable;
        }

        public void DrawEditor() {
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
