using CheapLoc;
using Dalamud.Data;
using Dalamud.Plugin;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Text;

namespace ItemSearchPlugin.Filters {
    class EquipAsSearchFilter : SearchFilter {
        private readonly List<uint> selectedClassJobs;
        private readonly List<ClassJobCategory> classJobCategories;
        private readonly List<ClassJob> classJobs;
        private bool changed;
        private bool modeAny;
        private bool selectingClasses = false;

        public EquipAsSearchFilter(ItemSearchPluginConfig config, DataManager data) : base(config) {
            this.modeAny = true;
            this.selectedClassJobs = new List<uint>();
            this.classJobCategories = data.GetExcelSheet<ClassJobCategory>().GetRows();
            this.classJobs = data.GetExcelSheet<ClassJob>().GetRows();
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

                    if (modeAny) {
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
            foreach (int i in selectedClassJobs) {
                if (!first) {
                    sb.Append(", ");
                }

                first = false;
                sb.Append(classJobs[i].Abbreviation);
            }

            if (first) {
                sb.Append(Loc.Localize("EquipAsSearchFilterSelectClasses", "None. Click here to select classes"));
            }

            return sb.ToString();
        }

        public override void DrawEditor() {
            if (ImGui.Checkbox($"{(modeAny ? Loc.Localize("SearchFilterAny", "Any") : Loc.Localize("SearchFilterAll", "All"))}: ##equipAsSearchFilterShowAny", ref modeAny)) {
                changed = true;
            }

            ImGui.SameLine();
            if (ImGui.Button($"{(selectingClasses ? Loc.Localize("EquipAsSearchFilterFinishedSelectingClasses", "Done") : SelectedClassString())}###equipAsChangeClassButton")) {
                selectingClasses = !selectingClasses;
                changed = true;
            }

            if (selectingClasses) {
                float wWidth = ImGui.GetWindowWidth();

                float firstColumnWith = ImGui.GetColumnWidth(0);

                ImGui.Columns(Math.Max(3, (int) wWidth / 80));
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
