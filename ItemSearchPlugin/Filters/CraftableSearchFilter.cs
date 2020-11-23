using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Data;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.Filters {
    internal class CraftableSearchFilter : SearchFilter {
        private int selectedOption = 0;

        private readonly string[] options;

        private readonly Dictionary<uint, RecipeLookup> craftableItems;

        private bool finishedLoading = false;

        private DataManager data;

        public CraftableSearchFilter(ItemSearchPluginConfig pluginConfig, DataManager data) : base(pluginConfig) {
            this.craftableItems = new Dictionary<uint, RecipeLookup>();
            this.data = data;
            string craftableJobFormat = Loc.Localize("CraftableJobFormat", "Craftable: {0}");

            options = new string[11];

            options[0] = "";
            options[1] = Loc.Localize("NotCraftable", "Not Craftable");
            options[2] = string.Format(craftableJobFormat, Loc.Localize("SearchFilterAny", "Any"));
            
            Task.Run(() => {
                var cj = data.GetExcelSheet<ClassJob>();
                
                for (uint i = 0; i < 8; i++) {
                    var job = cj.GetRow(i + 8);
                    options[3 + i] = string.Format(craftableJobFormat, job.Abbreviation);
                }

                foreach (var recipeLookup in data.GetExcelSheet<RecipeLookup>()) {
                    craftableItems.Add(recipeLookup.RowId, recipeLookup);
                }

                finishedLoading = true;
                Modified = true;
            });
        }

        public override string Name { get; } = "Craftable";
        public override string NameLocalizationKey { get; } = "CraftableSearchFilter";
        public override bool IsSet => selectedOption > 0;

        public override bool ShowFilter => base.ShowFilter && finishedLoading;

        public override bool CheckFilter(Item item) {
            if (item == null) return false;
            if (!finishedLoading) return true;

            var isCraftable = craftableItems.ContainsKey(item.RowId);
            return selectedOption switch {
                1 => !isCraftable,
                2 => isCraftable,
                3 => isCraftable && craftableItems[item.RowId].CRP.Row > 0,
                4 => isCraftable && craftableItems[item.RowId].BSM.Row > 0,
                5 => isCraftable && craftableItems[item.RowId].ARM.Row > 0,
                6 => isCraftable && craftableItems[item.RowId].GSM.Row > 0,
                7 => isCraftable && craftableItems[item.RowId].LTW.Row > 0,
                8 => isCraftable && craftableItems[item.RowId].WVR.Row > 0,
                9 => isCraftable && craftableItems[item.RowId].ALC.Row > 0,
                10 => isCraftable && craftableItems[item.RowId].CUL.Row > 0,
                _ => true
            };
        }

        public override void DrawEditor() {
            ImGui.BeginChild($"###{NameLocalizationKey}Child", new Vector2(-1, 23 * ImGui.GetIO().FontGlobalScale), false, usingTags ? ImGuiWindowFlags.NoInputs : ImGuiWindowFlags.None);
            ImGui.SetNextItemWidth(-1);
            if (ImGui.Combo("###craftableSearchFilter_selection", ref selectedOption, options, options.Length, 14)) {
                Modified = true;
            }
            ImGui.EndChild();
        }


        private bool usingTags = false;

        private int nonTagSelection;

        public override void ClearTags() {
            if (usingTags) {
                selectedOption = nonTagSelection;
                usingTags = false;
            }
        }

        public override bool IsFromTag => usingTags;

        public override bool ParseTag(string tag) {
            var t = tag.ToLower().Trim();

            var split = t.Split(':');
            split[0] = split[0].Trim();
            
            if (split[0] == "craftable" || split[0] == "not craftable") {
                Modified = true;
                if (!usingTags) {
                    nonTagSelection = selectedOption;
                    usingTags = true;
                }

                if (split[0] == "not craftable") {
                    selectedOption = 1;
                    return true;
                }

                if (split.Length > 1) {
                    split[1] = split[1].Trim();
                    var cj = data.GetExcelSheet<ClassJob>();

                    for (uint i = 0; i < 8; i++) {
                        var job = cj.GetRow(i + 8);
                        if (job.Abbreviation.ToString().ToLower() == split[1] || job.Name.ToString().ToLower() == split[1]) {
                            selectedOption = (int) (3 + i);
                            return true;
                        }
                    }

                    usingTags = false;
                    return false;



                } else {
                    selectedOption = 2;
                    return true;
                }

            }

            return false;
        }



        public override string ToString() {
            return options[selectedOption].Replace("Craftable: ", "");
        }
    }
}
