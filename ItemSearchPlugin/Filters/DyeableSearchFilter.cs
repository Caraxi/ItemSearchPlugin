using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.Filters {
    class DyeableSearchFilter : SearchFilter {
        private bool showDyeable = true;
        private bool showNotDyeable = true;

        public DyeableSearchFilter(ItemSearchPluginConfig pluginConfig) : base(pluginConfig) { }

        public override string Name { get; } = "Dyeability";
        public override string NameLocalizationKey { get; } = "DyeableSearchFilter";
        public override bool IsSet => showDyeable == false || showNotDyeable == false;

        public override bool CheckFilter(Item item) {
            return item.IsDyeable ? showDyeable : showNotDyeable;
        }

        public override void DrawEditor() {
            if (ImGui.Checkbox("Dyeable", ref showDyeable)) {
                if (!showDyeable) showNotDyeable = true;
                Modified = true;
            }

            ImGui.SameLine();
            if (ImGui.Checkbox("Not Dyeable", ref showNotDyeable)) {
                if (!showNotDyeable) showDyeable = true;
                Modified = true;
            }
        }
    }
}
