using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.Filters {
    class RaritySearchFilter : SearchFilter {
        private uint selectedValue = 0;

        public RaritySearchFilter(ItemSearchPluginConfig config) : base(config) { }
        public override string Name => "Rarity";
        public override string NameLocalizationKey => "RaritySearchFilter";
        public override bool IsSet => selectedValue > 0;


        public Dictionary<uint, Vector4> rarityColorMap = new Dictionary<uint, Vector4>() {
            {1, new Vector4(1f, 1f, 1f, 1)},
            {2, new Vector4(153/255f, 193/255f, 143/255f, 1)},
            {3, new Vector4(90/255f, 132/255f, 209/255f, 1)},
            {4, new Vector4(144/255f, 119/255f, 192/255f, 1)},
            {7, new Vector4(211/255f, 130/255f, 164/255f, 1)},
        };

        public override bool CheckFilter(Item item) {
            return item.Rarity == selectedValue;
        }

        public override void DrawEditor() {
            ImGui.SetNextItemWidth(100);

            var setBg = false;
            if (selectedValue != 0 && rarityColorMap.ContainsKey(selectedValue)) {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, rarityColorMap[selectedValue]);
                ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, rarityColorMap[selectedValue]);
                ImGui.PushStyleColor(ImGuiCol.FrameBgActive, rarityColorMap[selectedValue]);
                setBg = true;
            }
            if (ImGui.BeginCombo("###raritySelect", selectedValue == 0 ? "Any" : "", ImGuiComboFlags.HeightLargest)) {

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

                ImGui.BeginChild($"###colorBoxNone", new Vector2(-1, 20), false);
                if (ImGui.Selectable($"Any###optionNone", selectedValue == 0, ImGuiSelectableFlags.None, new Vector2(ImGui.GetWindowContentRegionWidth(), 20))) {
                    selectedValue = 0;
                    Modified = true;
                }
                ImGui.EndChild();

                foreach (var v in rarityColorMap) {

                    var x = ImGui.GetCursorPos();
                    if (ImGui.Selectable($"###optionRarity{v.Key}", selectedValue == v.Key, ImGuiSelectableFlags.None, new Vector2(100, 20))) {
                        selectedValue = v.Key;
                        Modified = true;
                    }

                    var h = ImGui.IsItemHovered();

                    ImGui.SameLine(0);
                    ImGui.SetCursorPos(x);
                    if (h) {
                        ImGui.PushStyleColor(ImGuiCol.ChildBg, v.Value - new Vector4(0, 0, 0, 0.2f));
                    } else {
                        ImGui.PushStyleColor(ImGuiCol.ChildBg, v.Value);
                    }
                    
                    ImGui.BeginChild($"###colorBox{v.Key}", new Vector2(100, 20), false, ImGuiWindowFlags.NoMouseInputs);
                    
                    ImGui.EndChild();
                    ImGui.PopStyleColor();

                }

                ImGui.PopStyleVar();
                ImGui.EndCombo();
            }

            if (setBg) {
                ImGui.PopStyleColor(3);
            }

        }
    }
}
