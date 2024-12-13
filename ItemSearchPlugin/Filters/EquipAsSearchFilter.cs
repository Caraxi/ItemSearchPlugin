using Dalamud.Plugin;
using ImGuiNET;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dalamud.Plugin.Services;

namespace ItemSearchPlugin.Filters {
    class EquipAsSearchFilter : SearchFilter {
        private readonly IDalamudPluginInterface pluginInterface;
        private List<uint> selectedClassJobs;
        private readonly List<ClassJobCategory> classJobCategories;
        private readonly List<ClassJob> classJobs;
        private bool changed;
        private bool selectingClasses;
        private int selectedMode;

        public EquipAsSearchFilter(ItemSearchPluginConfig config, IDataManager data, IDalamudPluginInterface pluginInterface) : base(config) {
            this.pluginInterface = pluginInterface;
            selectedClassJobs = new List<uint>();
            classJobCategories = data.GetExcelSheet<ClassJobCategory>().ToList();
            classJobs = data.GetExcelSheet<ClassJob>()
                .Where(cj => cj.RowId != 0)
                .OrderBy(cj => {
                    return cj.Role switch {
                        0 => 3,
                        1 => 0,
                        2 => 2,
                        3 => 2,
                        4 => 1,
                        _ => 4
                    };
                }).ToList();
            changed = false;
        }

        public override string Name => "Equip as";

        public override string NameLocalizationKey => "EquipAsSearchFilter";

        public override bool IsSet => selectedClassJobs.Count >= 1;

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
            try {
                if (item.ClassJobCategory.RowId != 0) {
                    ClassJobCategory cjc = classJobCategories[(int) item.ClassJobCategory.RowId];

                    if (selectedMode == 0) {
                        foreach (uint cjid in selectedClassJobs) {
                            if (cjc.HasClass(cjid)) {
                                return true;
                            }
                        }

                        return false;
                    } else {
                        foreach (uint cjid in selectedClassJobs) {
                            if (!cjc.HasClass(cjid)) {
                                return false;
                            }
                        }

                        return true;
                    }
                } else {
                    return false;
                }
            } catch (Exception) {
                return false;
            }
        }

        private string SelectedClassString() {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var i in selectedClassJobs) {
                if (!first) {
                    sb.Append(", ");
                }

                first = false;
                var cj = classJobs.FirstOrDefault(c => c.RowId == i);
                sb.Append(cj.Abbreviation);
            }

            if (first) {
                sb.Append(Loc.Localize("EquipAsSearchFilterSelectClasses", "None. Click here to select classes"));
            }

            return sb.ToString();
        }

        public override void DrawEditor() {
            ImGui.SetNextItemWidth(60 * ImGui.GetIO().FontGlobalScale);
            if (ImGui.Combo("###equipAsSearchFilterModeCombo", ref selectedMode, new[] {
                Loc.Localize("SearchFilterAny", "Any"),
                Loc.Localize("SearchFilterAll", "All"),
            }, 2)) {
                changed = true;
            }

            ImGui.SameLine();

            if (usingTags) {
                ImGui.Text(SelectedClassString());
            }


            if (usingTags == false && ImGui.SmallButton($"{(selectingClasses ? Loc.Localize("EquipAsSearchFilterFinishedSelectingClasses", "Done") : SelectedClassString())}###equipAsChangeClassButton")) {
                selectingClasses = !selectingClasses;
                changed = true;
            }

            if (usingTags == false && selectingClasses) {
                float wWidth = ImGui.GetWindowWidth();

                float firstColumnWith = ImGui.GetColumnWidth(0);

                ImGui.SameLine();

                if (ImGui.SmallButton("Select All")) {
                    foreach (ClassJob cj in classJobs) {
                        if (cj.RowId != 0 && !selectedClassJobs.Contains(cj.RowId)) {
                            selectedClassJobs.Add(cj.RowId);
                            changed = true;
                        }
                    }
                }

                ImGui.SameLine();
                if (ImGui.SmallButton("Select None")) {
                    selectedClassJobs.Clear();
                    changed = true;
                }
                
                ImGui.Columns(Math.Max(3, (int) (wWidth / (70 * ImGui.GetIO().FontGlobalScale))), "###equipAsClassList", false);
                ImGui.SetColumnWidth(0, firstColumnWith);
                try {
                    foreach (ClassJob cj in classJobs) {
                        if (cj.RowId != 0) {
                            if (ImGui.GetColumnIndex() == 0) {
                                ImGui.NextColumn();
                            }

                            bool selected = selectedClassJobs.Contains(cj.RowId);
                            if (ImGui.Checkbox(cj.Abbreviation.ToString(), ref selected)) {
                                if (selected) {
                                    if (!selectedClassJobs.Contains(cj.RowId)) {
                                        selectedClassJobs.Add(cj.RowId);
                                    }
                                } else {
                                    if (selectedClassJobs.Contains(cj.RowId)) {
                                        selectedClassJobs.Remove(cj.RowId);
                                    }
                                }

                                changed = true;
                            }

                            ImGui.NextColumn();
                        }
                    }
                } catch (NullReferenceException nre) {
                    PluginLog.Error(nre.ToString());
                }

                ImGui.Columns(2);
                ImGui.SetColumnWidth(0, firstColumnWith);
            } else if(usingTags == false && ClientState.LocalContentId != 0) {
                ImGui.SameLine();
                if (ImGui.SmallButton("Current Class")) {
                    if (ClientState?.LocalPlayer != null) {
                        selectedClassJobs.Clear();
                        selectedClassJobs.Add(ClientState.LocalPlayer.ClassJob.RowId);
                        changed = true;
                    }
                }
            }
        }


        private bool usingTags = false;

        private List<uint> nonTagSelection;

        public override void ClearTags() {
            if (usingTags) {
                selectedClassJobs = nonTagSelection;
                usingTags = false;
            }
        }

        public override bool IsFromTag => usingTags;

        public override bool ParseTag(string tag) {
            var t = tag.ToLower().Trim();
            var selfTag = false;
            if (t == "self" && ClientState?.LocalPlayer != null) {
                t = ClientState.LocalPlayer.ClassJob.Value.Abbreviation.ToString().ToLower();
                selfTag = true;
            }

            foreach (var bp in classJobs) {
                if (bp.Abbreviation.ToString().ToLower() == t) {

                    if (!usingTags) {
                        nonTagSelection = selectedClassJobs;
                        selectedClassJobs = new List<uint>();
                    }

                    usingTags = true;
                    selectedClassJobs.Add(bp.RowId);
                    return !selfTag;
                }
            }

            return false;
        }

        public override bool GreyWithTags => false;

        public override string ToString() {
            return $"{(selectedMode == 0 ? "Any of" : "All of")} [{string.Join(", ", classJobs.Where(cj => selectedClassJobs.Contains(cj.RowId)).Select(cj => cj.Abbreviation))}]";
        }
    }
}
