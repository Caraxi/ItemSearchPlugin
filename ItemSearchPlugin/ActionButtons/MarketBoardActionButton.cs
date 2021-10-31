using Lumina.Excel.GeneratedSheets;
using Dalamud.Plugin;
using System;
using System.Dynamic;

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
            return this.pluginConfig.MarketBoardPluginIntegration && selectedItem.ItemSearchCategory.Row > 0 && ItemSearchPlugin.PluginInterface.PluginInternalNames.Contains("MarketBoardPlugin");
        }

        public override void OnButtonClicked(Item selectedItem) {

            ItemSearchPlugin.CommandManager.ProcessCommand($"/pmb {selectedItem.RowId}");
        }

        public override void Dispose() { }
    }
}
