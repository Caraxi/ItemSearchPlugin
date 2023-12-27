using Lumina.Excel.GeneratedSheets;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ItemSearchPlugin.Filters {
    class ItemNameSearchFilter : SearchFilter {
        private string searchText;
        private string lastSearchText;
        private string[] searchTokens;

        private Regex searchRegex;

        private string parsedSearchText = string.Empty;
        private ItemSearchWindow window;

        public ItemNameSearchFilter(ItemSearchPluginConfig config, ItemSearchWindow window, string startingValue = "") : base(config) {
            searchText = startingValue;
            lastSearchText = string.Empty;
            this.window = window;
        }
        
        public override string Name => "Search";

        public override string NameLocalizationKey => "DalamudItemSearchVerb";

        public override bool IsSet => !string.IsNullOrEmpty(searchText);

        public override bool CanBeDisabled => false;

        private const char BeginTag = '[';
        private const char EndTag = ']';


        public override bool HasChanged {
            get {
                if (searchText != lastSearchText) {
                    ParseInputText(); 
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
                item.Name.ToString().ToLower().Contains(parsedSearchText.ToLower())
                || (searchTokens != null && searchTokens.Length > 0 && searchTokens.All(t => item.Name.ToString().ToLower().Contains(t)))
                || (int.TryParse(parsedSearchText, out var parsedId) && parsedId == item.RowId)
                || searchText.StartsWith("$") && item.Description.ToString().ToLower().Contains(parsedSearchText.Substring(1).ToLower());
        }
        
        public override bool CheckFilter(EventItem item) {
            if (searchRegex != null) {
                return searchRegex.IsMatch(item.Name);
            }

            return
                item.Name.ToString().ToLower().Contains(parsedSearchText.ToLower())
                || (searchTokens != null && searchTokens.Length > 0 && searchTokens.All(t => item.Name.ToString().ToLower().Contains(t)))
                || (int.TryParse(parsedSearchText, out var parsedId) && parsedId == item.RowId);
                //|| searchText.StartsWith("$") && item.Description.ToString().ToLower().Contains(parsedSearchText.Substring(1).ToLower());
        }
        
        public override void DrawEditor() {
            ImGui.SetNextItemWidth(-20 * ImGui.GetIO().FontGlobalScale);
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

        public void ParseInputText() {
            window.SearchFilters.ForEach(f => f.ClearTags());
            
            searchRegex = null;
            if (searchText.Length >= 3 && searchText.StartsWith("/") && searchText.EndsWith("/")) {
                try {
                    searchRegex = new Regex(searchText.Substring(1, searchText.Length - 2), RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    return;
                } catch (Exception) {
                    searchRegex = null;
                }
            }

            searchTokens = searchText.Trim().ToLower().Split(' ').Where(t => !string.IsNullOrEmpty(t)).ToArray();

            parsedSearchText = string.Empty;
            string currentTag = null;
            var tags = new List<string>();

            foreach (var c in searchText) {
                switch (c) {
                    case BeginTag: {
                        if (currentTag == null) {
                            currentTag = "";
                        } else {
                            if (currentTag == "") {
                                parsedSearchText += BeginTag;
                            } else {
                                parsedSearchText += $"{BeginTag}{currentTag}{BeginTag}";
                            }
                            currentTag = null;
                        }

                        break;
                    }
                    case EndTag: {
                        if (currentTag == null) {
                            parsedSearchText += EndTag;
                        } else {
                            if (currentTag.Length > 0) {
                                tags.Add(currentTag.Trim());
                            } else {
                                parsedSearchText += $"{BeginTag}{EndTag}";
                            }
                            currentTag = null;
                        }
                        break;
                    }
                    default: {
                        if (currentTag == null) {
                            parsedSearchText += c;
                        } else {
                            currentTag += c;
                        }
                        break;
                    }
                }
            }

            if (currentTag != null) {
                parsedSearchText += $"{BeginTag}{currentTag}";
            }

            foreach (var t in tags) {
                window.SearchFilters.ForEach(f => f.ParseTag(t));
            }

            parsedSearchText = parsedSearchText.Trim();
        }


    }
}
