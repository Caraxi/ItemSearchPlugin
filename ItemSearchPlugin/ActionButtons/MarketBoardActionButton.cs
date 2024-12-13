using Lumina.Excel.Sheets;
using System.Linq;

namespace ItemSearchPlugin.ActionButtons {
    class MarketBoardActionButton : IActionButton {
        private readonly ItemSearchPluginConfig pluginConfig;



        public override ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public MarketBoardActionButton(ItemSearchPluginConfig pluginConfig) {
            this.pluginConfig = pluginConfig;
        }

        public override string GetButtonText(Item selectedItem) {
            return Loc.Localize("ItemSearchMarketButton", "Market");
        }

        public override bool GetShowButton(Item selectedItem) {
            return pluginConfig.MarketBoardPluginIntegration && selectedItem.ItemSearchCategory.RowId > 0 && PluginInterface.InstalledPlugins.Any(p => p.InternalName == "MarketBoardPlugin" && p.IsLoaded);
        }

        public override void OnButtonClicked(Item selectedItem) {

            CommandManager.ProcessCommand($"/pmb {selectedItem.RowId}");
        }

        public override void Dispose() { }
    }
}
