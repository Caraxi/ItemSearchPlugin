using Lumina.Excel.GeneratedSheets;
using Dalamud.Plugin;
using System;
using System.Dynamic;

namespace ItemSearchPlugin.ActionButtons {
    class MarketBoardActionButton : IActionButton {
        private readonly DalamudPluginInterface pluginInterface;
        private readonly ItemSearchPluginConfig pluginConfig;

        private bool marketBoardResponsed = false;


        public override ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public MarketBoardActionButton(Dalamud.Plugin.DalamudPluginInterface pluginInterface, ItemSearchPluginConfig pluginConfig) {
            this.pluginInterface = pluginInterface;
            this.pluginConfig = pluginConfig;

            try {
                pluginInterface.Subscribe("MarketBoardPlugin", (o) => {
                    PluginLog.Log("Recieved Message from MarketBoardPlugin");
                    dynamic msg = o;
                    if (msg.Target == "ItemSearchPlugin" && msg.Action == "pong") {
                        marketBoardResponsed = true;
                    }
                });
            } catch (Exception ex) {
                PluginLog.LogError($"Exception Subscribing to MarketBoardPlugin: {ex.Message}");
            }

            dynamic areYouThereMarketBoard = new ExpandoObject();
            areYouThereMarketBoard.Target = "MarketBoardPlugin";
            areYouThereMarketBoard.Action = "ping";
            pluginInterface.SendMessage(areYouThereMarketBoard);
        }

        public override string GetButtonText(Item selectedItem) {
            return Loc.Localize("ItemSearchMarketButton", "Market");
        }

        public override bool GetShowButton(Item selectedItem) {
            return this.pluginConfig.MarketBoardPluginIntegration && marketBoardResponsed && selectedItem.ItemSearchCategory.Row > 0;
        }

        public override void OnButtonClicked(Item selectedItem) {
            dynamic itemMessage = new ExpandoObject();
            itemMessage.Target = "MarketBoardPlugin";
            itemMessage.Action = "OpenMarketBoard";
            itemMessage.ItemId = selectedItem.RowId;
            pluginInterface.SendMessage(itemMessage);
        }

        public override void Dispose() {
            pluginInterface.Unsubscribe("MarketBoardPlugin");
        }
    }
}
