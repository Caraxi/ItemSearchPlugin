using CheapLoc;
using Dalamud.Game.Chat;
using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Plugin;


namespace ItemSearchPlugin {
	public class ItemSearchPlugin : IDalamudPlugin {

		public string Name => "Item Search";
		public DalamudPluginInterface PluginInterface { get; private set; }
		public ItemSearchPluginConfig PluginConfig { get; private set; }

		private ItemSearchWindow itemSearchWindow;
		private bool drawItemSearchWindow;

		private Localization localization;
		private bool replacedOriginalCommand = false;
		internal bool drawConfigWindow = false;

		public void Dispose() {
			RemoveCommands();
		}

		public void Initialize(DalamudPluginInterface pluginInterface) {
			this.PluginInterface = pluginInterface;
			this.PluginConfig = (ItemSearchPluginConfig)pluginInterface.GetPluginConfig() ?? new ItemSearchPluginConfig();
			this.PluginConfig.Init(pluginInterface);

			localization = new Localization();

			if (!string.IsNullOrEmpty(PluginConfig.Language)) {
				localization.SetupWithLangCode(PluginConfig.Language);
			} else {
				localization.SetupWithUiCulture();
			}

			PluginInterface.UiBuilder.OnBuildUi += this.BuildUI;
			SetupCommands();

			#if DEBUG
			OnItemSearchCommand("","");
			#endif

		}

		public void SetupCommands() {
			
			// Move the original xlitem
			if (PluginInterface.CommandManager.Commands.ContainsKey("/xlitem")){
				PluginInterface.CommandManager.AddHandler("/xlitem_original", PluginInterface.CommandManager.Commands["/xlitem"]);
				PluginInterface.CommandManager.Commands["/xlitem_original"].ShowInHelp = false;
				PluginInterface.CommandManager.RemoveHandler("/xlitem");
				replacedOriginalCommand = true;
			}

			PluginInterface.CommandManager.AddHandler("/xlitem", new Dalamud.Game.Command.CommandInfo(OnItemSearchCommand) {
				HelpMessage = Loc.Localize("ItemSearchCommandHelp", "Open a window you can use to link any specific item to chat."),
				ShowInHelp = true
			});

		}

		public void OnItemSearchCommand(string command, string args) {			
			itemSearchWindow = new ItemSearchWindow(PluginInterface.Data, PluginInterface.UiBuilder, PluginConfig, args);
			itemSearchWindow.OnItemChosen += (sender, item) => {
				PluginInterface.Framework.Gui.Chat.PrintChat(new XivChatEntry{
					MessageBytes = SeStringUtils.CreateItemLink(item, false).Encode()
				});
			};
			itemSearchWindow.OnConfigButton += (s,c) => {
				drawConfigWindow = !drawConfigWindow;
			};
			drawItemSearchWindow = true;
		}

		public void RemoveCommands() {
			PluginInterface.CommandManager.RemoveHandler("/xlitems");

			// Put the original xlitem back
			if (replacedOriginalCommand) {
				PluginInterface.CommandManager.Commands["/xlitem_original"].ShowInHelp = true;
				PluginInterface.CommandManager.AddHandler("/xlitem", PluginInterface.CommandManager.Commands["/xlitem_original"]);
				PluginInterface.CommandManager.RemoveHandler("/xlitem_original");
				replacedOriginalCommand = false;
			}


		}

		private void BuildUI() {
			if (drawItemSearchWindow) {
				drawItemSearchWindow = itemSearchWindow != null && itemSearchWindow.Draw();
				drawConfigWindow = drawItemSearchWindow && drawConfigWindow && PluginConfig.DrawConfigUI();

				if (drawItemSearchWindow == false) {
					itemSearchWindow?.Dispose();
					itemSearchWindow = null;
					drawConfigWindow = false;
				}

			}

			
		}
	}


}
