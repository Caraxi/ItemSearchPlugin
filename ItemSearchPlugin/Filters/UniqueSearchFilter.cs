using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.Filters {
    class UniqueSearchFilter : SearchFilter {
        private bool showUnique = true;
        private bool showNotUnique = true;

        public UniqueSearchFilter(ItemSearchPluginConfig pluginConfig) : base(pluginConfig) { }

        public override string Name { get; } = "Unique";
        public override string NameLocalizationKey { get; } = "UniqueSearchFilter";
        public override bool IsSet => showUnique == false || showNotUnique == false;

        public override bool CheckFilter(Item item) {
            return item.IsUnique ? showUnique : showNotUnique;
        }

        public override void DrawEditor() {
            if (ImGui.Checkbox("Unique", ref showUnique)) {
                if (!showUnique) showNotUnique = true;
                Modified = true;
            }

            ImGui.SameLine();
            if (ImGui.Checkbox("Not Unique", ref showNotUnique)) {
                if (!showNotUnique) showUnique = true;
                Modified = true;
            }
        }

        public override string ToString() {
            return showUnique ? "Yes" : "No";
        }
    }
}
