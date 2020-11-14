using Lumina.Excel.GeneratedSheets;
using ImGuiNET;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ItemSearchPlugin.Filters {
    class ItemNameSearchFilter : SearchFilter {
        private string searchText;
        private string lastSearchText;
        private string[] searchTokens;

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
                            searchRegex = new Regex(searchText.Substring(1, searchText.Length - 2), RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        } catch (Exception) {
                            searchRegex = null;
                        }
                    }

                    searchTokens = searchText.Trim().ToLower().Split(' ').Where(t => !string.IsNullOrEmpty(t)).ToArray();
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
                || (searchTokens != null && searchTokens.Length > 0 && searchTokens.All(t => item.Name.ToLower().Contains(t)))
                || (int.TryParse(searchText, out var parsedId) && parsedId == item.RowId)
                || searchText.StartsWith("$") && item.Description.ToLower().Contains(searchText.Substring(1).ToLower());
        }

        public override void DrawEditor() {
            ImGui.PushItemWidth(-1);
            if (PluginConfig.AutoFocus && ImGui.IsWindowAppearing()) {
                ImGui.SetKeyboardFocusHere();
            }
            ImGui.InputText("##ItemNameSearchFilter", ref searchText, 256);
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered()) {
                ImGui.BeginTooltip();
                ImGui.Text("Type an item name to search for items by name.");
                ImGui.SameLine();
                ImGui.TextDisabled("\"OMG\"");
                ImGui.Text("Type an item id to search for item by its ID.");
                ImGui.SameLine();
                ImGui.TextDisabled("\"23991\"");
                ImGui.Text("Start input with '$' to search for an item by its description.");
                ImGui.SameLine();
                ImGui.TextDisabled("\"$Weird.\"");
                ImGui.Text("Start and end with '/' to search using regex.");
                ImGui.SameLine();
                ImGui.TextDisabled("\"/^.M.$/\"");


                ImGui.EndTooltip();
            }

        }

        public override string ToString() {
            return searchText;
        }
    }
}
