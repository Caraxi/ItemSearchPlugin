using System.Linq;
using System.Text;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;

namespace ItemSearchPlugin.ActionButtons {
    class CopyItemAsJson : IActionButton {
        private readonly ItemSearchPlugin plugin;

        public CopyItemAsJson(ItemSearchPlugin plugin) {
            this.plugin = plugin;
        }

        public override ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public override void Dispose() { }

        public override string GetButtonText(Item selectedItem) {
            return Loc.Localize("ItemSearchCopyAsJson", "Copy Information");
        }

        public override bool GetShowButton(Item selectedItem) {
            #if DEBUG
            return true;
            #else
            return false;
            #endif
        }

        public override void OnButtonClicked(Item selectedItem) {
            
            var sb = new StringBuilder();

            foreach (var f in typeof(Item).GetFields()) {

                sb.AppendLine($"{f.Name}: {f.GetValue(selectedItem)}");
            }


            

            var recipes = Data.GetExcelSheet<Recipe>().Where(a => a.ItemResult.RowId == selectedItem.RowId).ToList();

            if (recipes.Count == 0) {
                sb.Append("Recipes: NONE");
            } else {
                sb.AppendLine("Recipes:");
                foreach (var r in recipes) {

                    sb.AppendLine($"  Recipe: {r.RowId}");
                    sb.AppendLine("    Ingredients:");
                    for (var i = 0; i < r.Ingredient.Count; i++) {
                        var ri = r.Ingredient[i];
                        var amount = r.AmountIngredient[i];

                        sb.AppendLine($"      [{ri.RowId}*{amount}] {ri.Value.Name} x {amount}");


                    } 
                    foreach (var rf in typeof(Recipe).GetFields()) {
                        sb.AppendLine($"    {rf.Name}: {rf.GetValue(r)}");
                    }
                }
            }







            ImGui.SetClipboardText(sb.ToString());


        }
    }
}
