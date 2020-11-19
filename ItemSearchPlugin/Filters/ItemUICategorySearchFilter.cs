using System;
using Dalamud.Data;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin;

namespace ItemSearchPlugin.Filters {
    internal class ItemUICategorySearchFilter : SearchFilter {
        public override string Name => "Category";
        public override string NameLocalizationKey => "DalamudItemSelectCategory";

        public override bool IsSet => (usingTag ? taggedCategory : selectedCategory) != 0;

        public override bool HasChanged {
            get {
                if (lastCategory != selectedCategory) {
                    lastCategory = selectedCategory;
                    return true;
                }

                return false;
            }
        }

        private readonly List<ItemUICategory> uiCategories;
        private readonly string[] uiCategoriesArray;

        private int selectedCategory;
        private int lastCategory;
        private string categorySearchInput = string.Empty;
        private bool focused;
        private readonly Vector2 popupSize = new Vector2(-1, 120);

        public ItemUICategorySearchFilter(ItemSearchPluginConfig config, DataManager data) : base(config) {
            uiCategories = new List<ItemUICategory> {null};
            uiCategories.AddRange(data.GetExcelSheet<ItemUICategory>().ToList().Where(x => !string.IsNullOrEmpty(x.Name)).OrderBy(x => x.Name));
            string nullName = Loc.Localize("ItemUiCategorySearchFilterAll", "All");
            uiCategoriesArray = uiCategories.Select(x => x == null ? nullName : x.Name.Replace("\u0002\u001F\u0001\u0003", "-")).ToArray();
        }


        public override bool CheckFilter(Item item) {
            return item.ItemUICategory.Row == uiCategories[usingTag ? taggedCategory : selectedCategory].RowId;
        }

        public override void DrawEditor() {
            ImGui.PushItemWidth(-1);
            if (usingTag) {
                var str = uiCategoriesArray[taggedCategory];
                ImGui.InputText("##ItemUiCategorySearchFilterBox", ref str, 100, ImGuiInputTextFlags.ReadOnly);
                ImGui.PopItemWidth();
                return;
            }


            
            if (ImGui.BeginCombo("##ItemUiCategorySearchFilterBox", uiCategoriesArray[usingTag ? this.taggedCategory : this.selectedCategory])) {
                ImGui.SetNextItemWidth(-1);
                ImGui.InputTextWithHint("###ItemUiCategorySearchFilterFilter", "Filter", ref categorySearchInput,  60);
                var isFocused = ImGui.IsItemActive();
                if (!focused) {
                    ImGui.SetKeyboardFocusHere();
                }

                ImGui.BeginChild("###ItemUiCategorySearchFilterDisplay", popupSize, true);

                if (!focused) {
                    ImGui.SetScrollY(0);
                    focused = true;
                }

                var c = 0;
                var l = 0;
                for (var i = 0; i < uiCategoriesArray.Length; i++) {
                    if (i > 0 && categorySearchInput.Length > 0 && !uiCategoriesArray[i].ToLowerInvariant().Contains(categorySearchInput.ToLowerInvariant())) continue;
                    if (i != 0) {
                        c++;
                        l = i;
                    }
                    if (!ImGui.Selectable(uiCategoriesArray[i], selectedCategory == i)) continue;
                    selectedCategory = i;
                    
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndChild();
                if (!isFocused && c <= 1) {
                    selectedCategory = l;
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.EndCombo();
            } else if (focused) {
                focused = false;
                categorySearchInput = string.Empty;
            }
            
            ImGui.PopItemWidth();
        }

        private bool usingTag = false;

        public override bool IsFromTag => usingTag;

        private int taggedCategory = 0;

        public override void ClearTags() {
            usingTag = false;
            taggedCategory = 0;
        }

        public override bool ParseTag(string tag) {
            var t = tag.Trim().ToLower();

            for (var i = 1; i < uiCategoriesArray.Length; i++) {
                var c = uiCategoriesArray[i];
                if (c.ToLower() == t) {
                    taggedCategory = i;
                    usingTag = true;
                    Modified = true;
                    return true;
                }
            }

            return false;
        }

        public override string ToString() {
            return uiCategoriesArray[usingTag ? taggedCategory : selectedCategory];
        }
    }
}
