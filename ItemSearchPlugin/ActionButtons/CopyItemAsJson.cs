using System.Linq;
using System.Text;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace ItemSearchPlugin.ActionButtons {
    class CopyItemAsJson : IActionButton {
        private readonly ItemSearchPlugin plugin;

        public CopyItemAsJson(ItemSearchPlugin plugin) {
            this.plugin = plugin;
        }

        public ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public void Dispose() { }

        public string GetButtonText(Item selectedItem) {
            return Loc.Localize("ItemSearchCopyAsJson", "Copy Information");
        }

        public bool GetShowButton(Item selectedItem) {
            #if DEBUG
            return true;
            #else
            return false;
            #endif
        }

        public void OnButtonClicked(Item selectedItem) {
            
            var sb = new StringBuilder();

            foreach (var f in typeof(Item).GetFields()) {

                sb.AppendLine($"{f.Name}: {f.GetValue(selectedItem)}");
            }


            

            var recipes = plugin.PluginInterface.Data.GetExcelSheet<Recipe>().Where(a => a.ItemResult.Row == selectedItem.RowId).ToList();

            if (recipes.Count == 0) {
                sb.Append("Recipes: NONE");
            } else {
                sb.AppendLine("Recipes:");
                foreach (var r in recipes) {

                    sb.AppendLine($"  Recipe: {r.RowId}");
                    sb.AppendLine("    Ingredients:");
                    foreach (var ri in r.UnkStruct5) {

                        sb.AppendLine($"      [{ri.ItemIngredient}*{ri.AmountIngredient}] {plugin.PluginInterface.Data.GetExcelSheet<Item>().GetRow((uint) ri.ItemIngredient).Name} x {ri.AmountIngredient}");


                    } 
                    foreach (var rf in typeof(Recipe).GetFields()) {
                        sb.AppendLine($"    {rf.Name}: {rf.GetValue(r)}");
                    }
                }
            }







            System.Windows.Forms.Clipboard.SetText(sb.ToString());


        }
    }
}
