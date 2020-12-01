using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Chat;
using ImGuiNET;

namespace ItemSearchPlugin {
    class SpecialShopTestUi {
        private ItemSearchPlugin plugin;

        public SpecialShopTestUi(ItemSearchPlugin plugin) {
            this.plugin = plugin;

            selectedSpecialShop = plugin.PluginInterface.Data.Excel.GetSheet<SpecialShopCustom>().GetRow(1769721);

        }

        public SpecialShopCustom selectedSpecialShop;

        private string searchInput = string.Empty;
        private bool focused = false;
        private readonly Vector2 popupSize = new Vector2(-1, 120);

        public void Draw() {
            
            ImGui.Begin("Special Shop Test");
            try {
                if (ImGui.BeginCombo("##ItemUiCategorySearchFilterBox", selectedSpecialShop?.Name ?? "Not Selected")) {
                    try {
                        ImGui.SetNextItemWidth(-1);
                        ImGui.InputTextWithHint("###ItemUiCategorySearchFilterFilter", "Filter", ref searchInput, 60);
                        var isFocused = ImGui.IsItemActive();
                        if (!focused) {
                            ImGui.SetKeyboardFocusHere();
                        }

                        ImGui.BeginChild("###ItemUiCategorySearchFilterDisplay", popupSize, true);
                        try {
                            if (!focused) {
                                ImGui.SetScrollY(0);
                                focused = true;
                            }

                            var sheet = plugin.PluginInterface.Data.Excel.GetSheet<SpecialShopCustom>();
                            foreach (var s in sheet) {
                                if (!string.IsNullOrEmpty(searchInput)) {
                                    if (!s.Name.ToString().ToLower().Contains(searchInput.ToLower())) {
                                        continue;
                                    }
                                }

                                if (ImGui.Selectable($"[{s.RowId}] {s.Name}", selectedSpecialShop != null && s == selectedSpecialShop)) {
                                    selectedSpecialShop = s;
                                    ImGui.CloseCurrentPopup();
                                }
                            }
                        } catch {
                            // Ignored
                        }
                       

                        ImGui.EndChild();

                    } catch {
                        // Ignored
                    }
                   
                    ImGui.EndCombo();
                } else if (focused) {
                    focused = false;
                    searchInput = string.Empty;
                }

                ImGui.Separator();

                if (selectedSpecialShop != null) {
                    ImGui.TextDisabled($"[{selectedSpecialShop.RowId}]");
                    ImGui.SameLine();
                    ImGui.Text(selectedSpecialShop.Name);

                    if (selectedSpecialShop.QuestUnlock.Row > 0) {
                        ImGui.TextDisabled("Unlock Quest: ");
                        ImGui.SameLine();
                        ImGui.Text(selectedSpecialShop.QuestUnlock.Value.Name);
                    }

                    if (selectedSpecialShop.CompleteText.Row > 0) {
                        ImGui.TextDisabled("Complete Text: ");
                        ImGui.Indent(25);
                        ImGui.BeginGroup();
                        foreach (var s in selectedSpecialShop.CompleteText.Value.Text) {
                            if (s == "0") continue;
                            ImGui.Text(s);
                        }
                        ImGui.EndGroup();
                        ImGui.Indent(-25);

                    }
                    if (selectedSpecialShop.NotCompleteText.Row > 0) {
                        ImGui.TextDisabled("Not Complete Text: ");
                        ImGui.Indent(25);
                        ImGui.BeginGroup();
                        foreach (var s in selectedSpecialShop.NotCompleteText.Value.Text) {
                            if (s == "0") continue;
                            ImGui.Text(s);
                        }
                        ImGui.EndGroup();
                        ImGui.Indent(-25);

                    }

                    ImGui.Separator();

                    ImGui.Columns(3);
                    ImGui.SetColumnWidth(0, 40);
                    ImGui.Text("Entry#");
                    ImGui.NextColumn();
                    ImGui.Text("Result");
                    ImGui.NextColumn();
                    ImGui.Text("Cost");
                    ImGui.NextColumn();
                    ImGui.Separator();
                    ImGui.Separator();

                    for (var i = 0; i < selectedSpecialShop.Entries.Length; i++) {
                        var e = selectedSpecialShop.Entries[i];
                        if (e.Result[0].Item.Row == 0) continue;
                        ImGui.Text($"#{i}");
                        ImGui.NextColumn();

                        foreach (var r in e.Result) {
                            if (r.Item.Row == 0) continue;

                            if (r.HQ) {
                                ImGui.Text($"{(char)SeIconChar.HighQuality}");
                                ImGui.SameLine();
                            }

                            ImGui.Text(r.Item.Value.Name);
                            if (r.Count > 1) {
                                ImGui.SameLine();
                                ImGui.Text($"(x{r.Count})");
                            }
                        }

                        ImGui.NextColumn();

                        foreach (var c in e.Cost) {
                            if (c.Item.Row == 0) continue;
                            if (c.HQ) {
                                ImGui.Text($"{(char)SeIconChar.HighQuality}");
                                ImGui.SameLine();
                            }
                            ImGui.Text(c.Item.Value.Name);
                            if (c.Count > 1) {
                                ImGui.SameLine();
                                ImGui.Text($"(x{c.Count})");
                            }

                        }

                        ImGui.NextColumn();

                        ImGui.Separator();
                    }

                    ImGui.Columns();
                }
            } catch {
                // Ignored
            }
           
            
            ImGui.End();
        }


    }
}
