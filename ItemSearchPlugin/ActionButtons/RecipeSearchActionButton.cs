using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.ActionButtons {
    class RecipeSearchActionButton : IActionButton {
        private readonly CraftingRecipeFinder craftingRecipeFinder;

        public RecipeSearchActionButton(CraftingRecipeFinder craftingRecipeFinder) {
            this.craftingRecipeFinder = craftingRecipeFinder;
        }

        public override ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public override void Dispose() { }

        public override string GetButtonText(Item selectedItem) {
            return Loc.Localize("RecipeSearchButton", "Search for Crafting Recipe");
        }

        public override bool GetShowButton(Item selectedItem) {
            return selectedItem != null;
        }

        public override void OnButtonClicked(Item selectedItem) {
            craftingRecipeFinder.SearchRecipesByItem(selectedItem);
        }
    }
}
