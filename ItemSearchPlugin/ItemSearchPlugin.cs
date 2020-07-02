using System.Collections.Generic;
using Dalamud;
using Dalamud.Game.Chat;
using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Plugin;
using ImGuiNET;
using ItemSearchPlugin.DataSites;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin {
    public class ItemSearchPlugin : IDalamudPlugin {
        public string Name => "Item Search";
        public DalamudPluginInterface PluginInterface { get; private set; }
        public ItemSearchPluginConfig PluginConfig { get; private set; }

        public FittingRoomUI FittingRoomUI { get; private set; }

        private SeStringManager seStringManager;
        private ItemSearchWindow itemSearchWindow;
        private bool drawItemSearchWindow;

        private bool replacedOriginalCommand;
        private bool drawConfigWindow;

        internal List<Item> LuminaItems { get; set; }
        internal ClientLanguage LuminaItemsClientLanguage { get; set; }
        
        public static DataSite[] DataSites { get; } = new DataSite[] {
            new GarlandToolsDataSite(), new TeamcraftDataSite(), new GamerEscapeDatasite(),
        };

        public void Dispose() {
            PluginInterface.UiBuilder.OnBuildUi -= this.BuildUI;
            FittingRoomUI?.Dispose();
            itemSearchWindow?.Dispose();
            RemoveCommands();
            PluginInterface.Dispose();
        }

        public void Initialize(DalamudPluginInterface pluginInterface) {
            this.PluginInterface = pluginInterface;
            this.PluginConfig = (ItemSearchPluginConfig) pluginInterface.GetPluginConfig() ?? new ItemSearchPluginConfig();
            this.PluginConfig.Init(pluginInterface, this);
            seStringManager = new SeStringManager(pluginInterface.Data);

            ReloadLocalization();

            FittingRoomUI = new FittingRoomUI(this);

            PluginInterface.UiBuilder.OnBuildUi += this.BuildUI;
            SetupCommands();

#if DEBUG
            OnItemSearchCommand("", "");
#endif
        }

        public void ReloadLocalization() {
            if (!string.IsNullOrEmpty(PluginConfig.Language)) {
                Loc.LoadLanguage(PluginConfig.Language);
            } else {
                Loc.LoadDefaultLanguage();
            }
        }


        public void SetupCommands() {
            // Move the original xlitem
            if (PluginInterface.CommandManager.Commands.ContainsKey("/xlitem")) {
                PluginInterface.CommandManager.AddHandler("/xlitem_original", PluginInterface.CommandManager.Commands["/xlitem"]);
                PluginInterface.CommandManager.Commands["/xlitem_original"].ShowInHelp = false;
                PluginInterface.CommandManager.RemoveHandler("/xlitem");
                replacedOriginalCommand = true;
            }

            PluginInterface.CommandManager.AddHandler("/xlitem", new Dalamud.Game.Command.CommandInfo(OnItemSearchCommand) {
                HelpMessage = Loc.Localize("ItemSearchCommandHelp", "Open a window you can use to link any specific item to chat."),
                ShowInHelp = true
            });

#if DEBUG
            PluginInterface.CommandManager.AddHandler("/itemsearchdumploc", new Dalamud.Game.Command.CommandInfo(((command, arguments) => {
                Loc.ExportLoadedDictionary();
            })) {ShowInHelp = false, HelpMessage = ""});
#endif
        }

        public void OnItemSearchCommand(string command, string args) {
            itemSearchWindow?.Dispose();
            itemSearchWindow = new ItemSearchWindow(this, args);
            drawItemSearchWindow = true;
        }

        public void RemoveCommands() {
            PluginInterface.CommandManager.RemoveHandler("/xlitem");

            // Put the original xlitem back
            if (replacedOriginalCommand) {
                PluginInterface.CommandManager.Commands["/xlitem_original"].ShowInHelp = true;
                PluginInterface.CommandManager.AddHandler("/xlitem", PluginInterface.CommandManager.Commands["/xlitem_original"]);
                PluginInterface.CommandManager.RemoveHandler("/xlitem_original");
                replacedOriginalCommand = false;
            }
#if DEBUG
            PluginInterface.CommandManager.RemoveHandler("/itemsearchdumploc");
#endif
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

            if (PluginConfig.EnableFittingRoomSaves || PluginConfig.ShowItemID) {
                if (FittingRoomUI == null) {
                    FittingRoomUI = new FittingRoomUI(this);
                } else {
                    if (PluginConfig.EnableFittingRoomSaves) {
                        FittingRoomUI?.Draw();
                    }
                }
            }

#if DEBUG
            ImGui.BeginMainMenuBar();
            if (ImGui.MenuItem("ItemSearch")) {
                itemSearchWindow?.Dispose();
                itemSearchWindow = new ItemSearchWindow(this);
                drawItemSearchWindow = true;
            }

            ImGui.EndMainMenuBar();
#endif
        }

        internal void LinkItem(Item item) {
            if (item == null) {
                PluginLog.Log("Tried to link NULL item.");
                return;
            }

            PluginInterface.Framework.Gui.Chat.PrintChat(new XivChatEntry {
                MessageBytes = seStringManager.CreateItemLink(item.RowId, item.CanBeHq && PluginInterface.ClientState.KeyState[0x11], item.Name).Encode()
            });
        }

        internal void ToggleConfigWindow() {
            drawConfigWindow = !drawConfigWindow;
        }
    }
}
