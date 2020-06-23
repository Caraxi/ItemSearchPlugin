using Lumina.Excel.GeneratedSheets;
using ImGuiNET;
using System;
using System.Text.RegularExpressions;

namespace ItemSearchPlugin.Filters {
    class ItemNameSearchFilter : SearchFilter {
        private string searchText;
        private string lastSearchText;

        private Regex searchRegex;

        public ItemNameSearchFilter(ItemSearchPluginConfig config, string startingValue = "") : base(config) {
            searchText = startingValue;
            lastSearchText = string.Empty;
        }
        
        public override string Name => "Search";

        public override string NameLocalizationKey => "DalamudItemSearchVerb";

        public override bool IsSet => !string.IsNullOrEmpty(searchText);

        public override bool CanBeDisabled => false;

        public override bool HasChanged {
            get {
                if (searchText != lastSearchText) {
                    searchRegex = null;
                    if (searchText.Length >= 3 && searchText.StartsWith("/") && searchText.EndsWith("/")) {
                        try {
                            searchRegex = new Regex(searchText[1..^1], RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        } catch (Exception) {
                            searchRegex = null;
                        }
                    }

                    lastSearchText = searchText;
                    return true;
                }

                return false;
            }
        }

        public override bool CheckFilter(Item item) {
            if (searchRegex != null) {
                return searchRegex.IsMatch(item.Name);
            }

            return
                item.Name.ToLower().Contains(searchText.ToLower())
                || (int.TryParse(searchText, out var parsedId) && parsedId == item.RowId);
        }

        public override void DrawEditor() {
            ImGui.PushItemWidth(-1);
            ImGui.InputText("##ItemNameSearchFilter", ref searchText, 256);
            ImGui.PopItemWidth();
        }
    }
}
