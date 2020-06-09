using Lumina.Excel.GeneratedSheets;
using ImGuiNET;
using System;
using System.Text.RegularExpressions;

namespace ItemSearchPlugin.Filters {
    class ItemNameSearchFilter : ISearchFilter {
        private string searchText;
        private string lastSearchText;

        private Regex searchRegex;

        public ItemNameSearchFilter(string startingValue = "") {
            searchText = startingValue;
            lastSearchText = string.Empty;
        }


        public string Name => "Search";

        public string NameLocalizationKey => "DalamudItemSearchVerb";

        public bool ShowFilter => true;

        public bool IsSet => !string.IsNullOrEmpty(searchText);

        public bool HasChanged {
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

        public bool CheckFilter(Item item) {
            if (searchRegex != null) {
                return searchRegex.IsMatch(item.Name);
            }

            return item.Name.ToLower().Contains(searchText.ToLower()) || int.TryParse(searchText, out var parsedId) && parsedId == item.RowId && item.Icon < 65000;
        }

        public void Dispose() { }

        public void DrawEditor() {
            ImGui.PushItemWidth(-1);
            ImGui.InputText("##ItemNameSearchFilter", ref searchText, 256);
            ImGui.PopItemWidth();
        }
    }
}
