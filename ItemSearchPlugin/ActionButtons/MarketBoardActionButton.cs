﻿using CheapLoc;
using Dalamud.Data.TransientSheet;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearchPlugin.ActionButtons {
	class MarketBoardActionButton : IActionButton {
		private DalamudPluginInterface pluginInterface;
		private ItemSearchPluginConfig pluginConfig;

		private bool marketBoardResponsed = false;


		public ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

		public MarketBoardActionButton(Dalamud.Plugin.DalamudPluginInterface pluginInterface, ItemSearchPluginConfig pluginConfig) {
			this.pluginInterface = pluginInterface;
			this.pluginConfig = pluginConfig;

			pluginInterface.Subscribe("MarketBoardPlugin", (o) => {
                PluginLog.Log("Recieved Message from MarketBoardPlugin");
                dynamic msg = o;
                if (msg.Target == "ItemSearchPlugin" && msg.Action == "pong") {
                    marketBoardResponsed = true;
                }
            });

			dynamic areYouThereMarketBoard = new ExpandoObject();
            areYouThereMarketBoard.Target = "MarketBoardPlugin";
            areYouThereMarketBoard.Action = "ping";
			pluginInterface.SendMessage(areYouThereMarketBoard);

		}

		public string GetButtonText(Item selectedItem) {
			return Loc.Localize("ItemSearchMarketButton", "Market");
		}

		public bool GetShowButton(Item selectedItem) {
			return this.pluginConfig.MarketBoardPluginIntegration && marketBoardResponsed && selectedItem.ItemSearchCategory > 0;
		}

		public void OnButtonClicked(Item selectedItem) {
			dynamic itemMessage = new ExpandoObject();
			itemMessage.Target = "MarketBoardPlugin";
			itemMessage.Action = "OpenMarketBoard";
			itemMessage.ItemId = selectedItem.RowId;
			pluginInterface.SendMessage(itemMessage);
		}

		public void Dispose() {
			pluginInterface.Unsubscribe("MarketBoardPlugin");
		}

	}
}