using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.ActionButtons {
    class RecipeSearchActionButton : IActionButton {
        private readonly CraftingRecipeFinder craftingRecipeFinder;

        public RecipeSearchActionButton(CraftingRecipeFinder craftingRecipeFinder) {
            this.craftingRecipeFinder = craftingRecipeFinder;
        }

        public ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public void Dispose() { }

        public string GetButtonText(Item selectedItem) {
            return Loc.Localize("RecipeSearchButton", "Search for Crafting Recipe");
        }

        public bool GetShowButton(Item selectedItem) {
            return selectedItem != null;
        }

        public void OnButtonClicked(Item selectedItem) {
            craftingRecipeFinder.SearchRecipesByItem(selectedItem);
        }
    }
}
