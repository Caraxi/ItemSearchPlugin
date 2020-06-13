using CheapLoc;
using Dalamud.Data;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.Linq;

namespace ItemSearchPlugin.Filters {
    class ItemUICategorySearchFilter : SearchFilter {
        public override string Name => "Category";
        public override string NameLocalizationKey => "DalamudItemSelectCategory";

        public override bool IsSet => selectedCategory != 0;

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

        private int selectedCategory = 0;
        private int lastCategory = 0;

        public ItemUICategorySearchFilter(DataManager data) {
            uiCategories = new List<ItemUICategory> {null};
            uiCategories.AddRange(data.GetExcelSheet<ItemUICategory>().GetRows().Where(x => !string.IsNullOrEmpty(x.Name)).OrderBy(x => x.Name));
            string nullName = Loc.Localize("ItemUiCategorySearchFilterAll", "All");
            uiCategoriesArray = uiCategories.Select(x => x == null ? nullName : x.Name.Replace("\u0002\u001F\u0001\u0003", "-")).ToArray();
        }


        public override bool CheckFilter(Item item) {
            return item.ItemUICategory.Row == uiCategories[selectedCategory].RowId;
        }

        public override void DrawEditor() {
            ImGui.PushItemWidth(-1);
            ImGui.Combo("##ItemUiCategorySearchFilterBox", ref this.selectedCategory, uiCategoriesArray, uiCategories.Count);
            ImGui.PopItemWidth();
        }
    }
}
