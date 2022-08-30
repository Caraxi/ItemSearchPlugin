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
        public override bool IsSet => usingTag || selectedValue > 0;


        public Dictionary<uint, Vector4> rarityColorMap = new Dictionary<uint, Vector4>() {
            {1, new Vector4(1f, 1f, 1f, 1)},
            {2, new Vector4(0.6F, 0.76F, 0.56F, 1)},
            {3, new Vector4(0.36F, 0.52F, 0.82F, 1)},
            {4, new Vector4(0.56F, 0.47F, 0.75F, 1)},
            {7, new Vector4(0.83F, 0.51F, 0.64F, 1)},
        };

        public override bool CheckFilter(Item item) {
            return item.Rarity == (usingTag ? taggedValue : selectedValue);
        }

        public override void DrawEditor() {
            ImGui.SetNextItemWidth(100 * ImGui.GetIO().FontGlobalScale);

            var sv = usingTag ? taggedValue : selectedValue;

            var setBg = false;
            if (sv != 0 && rarityColorMap.ContainsKey(sv)) {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, rarityColorMap[sv]);
                ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, rarityColorMap[sv]);
                ImGui.PushStyleColor(ImGuiCol.FrameBgActive, rarityColorMap[sv]);
                setBg = true;
            }

            if (usingTag) {
                var t = sv == 0 ? "Any" : "";
                ImGui.InputText("###raritySelect", ref t, 3, ImGuiInputTextFlags.ReadOnly);
                ImGui.PopStyleColor(3);
                return;
            }

            if (ImGui.BeginCombo("###raritySelect", !usingTag && selectedValue == 0 ? "Any" : "", ImGuiComboFlags.HeightLargest)) {

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

                ImGui.BeginChild($"###colorBoxNone", new Vector2(-1, 20 * ImGui.GetIO().FontGlobalScale), false);
                if (ImGui.Selectable($"Any###optionNone", selectedValue == 0, ImGuiSelectableFlags.None, new Vector2((ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X), 20 * ImGui.GetIO().FontGlobalScale))) {
                    selectedValue = 0;
                    Modified = true;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndChild();

                foreach (var v in rarityColorMap) {

                    var x = ImGui.GetCursorPos();
                    if (ImGui.Selectable($"###optionRarity{v.Key}", (usingTag ? taggedValue : selectedValue) == v.Key, ImGuiSelectableFlags.None, new Vector2(100, 20) * ImGui.GetIO().FontGlobalScale)) {
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
                    
                    ImGui.BeginChild($"###colorBox{v.Key}", new Vector2(100, 20) * ImGui.GetIO().FontGlobalScale, false, ImGuiWindowFlags.NoMouseInputs);
                    
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

        
        private bool usingTag = false;

        public override bool IsFromTag => usingTag;

        private uint taggedValue = 0;

        public override void ClearTags() {
            usingTag = false;
            taggedValue = 0;
        }

        public override bool ParseTag(string tag) {
            var t = tag.Trim().ToLower();

            switch (t) {
                case "white": {
                    taggedValue = 1;
                    return usingTag = true;
                }
                case "green": {
                    taggedValue = 2;
                    return usingTag = true;
                }
                case "blue": {
                    taggedValue = 3;
                    return usingTag = true;
                }
                case "purple": {
                    taggedValue = 4;
                    return usingTag = true;
                }
                case "pink": {
                    taggedValue = 7;
                    return usingTag = true;
                }
            }


            return false;
        }


    }
}
