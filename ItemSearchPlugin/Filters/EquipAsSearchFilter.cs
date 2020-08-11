using Dalamud.Data;
using Dalamud.Plugin;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Item = ItemSearchPlugin.ItemTemp;
namespace ItemSearchPlugin.Filters {
    class EquipAsSearchFilter : SearchFilter {
        private readonly List<uint> selectedClassJobs;
        private readonly List<ClassJobCategory> classJobCategories;
        private readonly List<ClassJob> classJobs;
        private bool changed;
        private bool selectingClasses;
        private int selectedMode;

        public EquipAsSearchFilter(ItemSearchPluginConfig config, DataManager data) : base(config) {
            this.selectedClassJobs = new List<uint>();
            this.classJobCategories = data.GetExcelSheet<ClassJobCategory>().ToList();
            this.classJobs = data.GetExcelSheet<ClassJob>()
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
                if (item.ClassJobCategory.Row != 0) {
                    ClassJobCategory cjc = classJobCategories[(int) item.ClassJobCategory.Row];

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
                if (cj != null) {
                    sb.Append(cj.Abbreviation);
                }
            }

            if (first) {
                sb.Append(Loc.Localize("EquipAsSearchFilterSelectClasses", "None. Click here to select classes"));
            }

            return sb.ToString();
        }

        public override void DrawEditor() {
            ImGui.SetNextItemWidth(60);
            if (ImGui.Combo("###equipAsSearchFilterModeCombo", ref selectedMode, new[] {
                Loc.Localize("SearchFilterAny", "Any"),
                Loc.Localize("SearchFilterAll", "All"),
            }, 2)) {
                changed = true;
            }

            ImGui.SameLine();
            if (ImGui.SmallButton($"{(selectingClasses ? Loc.Localize("EquipAsSearchFilterFinishedSelectingClasses", "Done") : SelectedClassString())}###equipAsChangeClassButton")) {
                selectingClasses = !selectingClasses;
                changed = true;
            }

            if (selectingClasses) {
                float wWidth = ImGui.GetWindowWidth();

                float firstColumnWith = ImGui.GetColumnWidth(0);

                ImGui.SameLine();

                if (ImGui.SmallButton("Select All")) {
                    foreach (ClassJob cj in classJobs) {
                        if (cj.RowId != 0 && !selectedClassJobs.Contains(cj.RowId)) {
                            selectedClassJobs.Add(cj.RowId);
                        }
                    }
                }

                ImGui.SameLine();
                if (ImGui.SmallButton("Select None")) {
                    selectedClassJobs.Clear();
                }
                
                ImGui.Columns(Math.Max(3, (int) wWidth / 70), "###equipAsClassList", false);
                ImGui.SetColumnWidth(0, firstColumnWith);
                try {
                    foreach (ClassJob cj in classJobs) {
                        if (cj.RowId != 0) {
                            if (ImGui.GetColumnIndex() == 0) {
                                ImGui.NextColumn();
                            }

                            bool selected = selectedClassJobs.Contains(cj.RowId);
                            if (ImGui.Checkbox(cj.Abbreviation, ref selected)) {
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
                    PluginLog.LogError(nre.ToString());
                }

                ImGui.Columns(2);
                ImGui.SetColumnWidth(0, firstColumnWith);
            }
        }
    }
}
