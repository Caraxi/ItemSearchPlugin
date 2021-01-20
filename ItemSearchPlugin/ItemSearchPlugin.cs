using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud;
using Dalamud.Data.LuminaExtensions;
using Dalamud.Game.Chat;
using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Game.Chat.SeStringHandling.Payloads;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using ItemSearchPlugin.DataSites;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin {
    public class ItemSearchPlugin : IDalamudPlugin {
        public string Name => "Item Search";
        public DalamudPluginInterface PluginInterface { get; private set; }
        public ItemSearchPluginConfig PluginConfig { get; private set; }

        public FittingRoomUI FittingRoomUI { get; private set; }

        public readonly Dictionary<ushort, TextureWrap> textureDictionary = new Dictionary<ushort, TextureWrap>();

        public CraftingRecipeFinder CraftingRecipeFinder { get; private set; }

        internal ItemSearchWindow itemSearchWindow;
        private bool drawItemSearchWindow;

        private bool drawConfigWindow;

        internal List<Item> LuminaItems { get; set; }
        internal ClientLanguage LuminaItemsClientLanguage { get; set; }
        
        public static DataSite[] DataSites { get; private set; } = new DataSite[] { new GarlandToolsDataSite() }; 
        public string Version { get; private set; }

        public void Dispose() {
            PluginInterface.UiBuilder.OnBuildUi -= this.BuildUI;
            FittingRoomUI?.Dispose();
            CraftingRecipeFinder?.Dispose();
            itemSearchWindow?.Dispose();
            RemoveCommands();
            PluginInterface.Dispose();
            

            foreach (var t in textureDictionary) {
                t.Value?.Dispose();
            }

            textureDictionary.Clear();
        }

        public void Initialize(DalamudPluginInterface pluginInterface) {
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.PluginInterface = pluginInterface;
            this.PluginConfig = (ItemSearchPluginConfig) pluginInterface.GetPluginConfig() ?? new ItemSearchPluginConfig();

            ItemSearchPlugin.DataSites = new DataSite[] {
                new GarlandToolsDataSite(),
                new TeamcraftDataSite(PluginConfig),
                new GamerEscapeDatasite(),
            };

            this.PluginConfig.Init(pluginInterface, this);


            SetupGameFunctions();

            ReloadLocalization();

            FittingRoomUI = new FittingRoomUI(this);

            CraftingRecipeFinder = new CraftingRecipeFinder(this);

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
            PluginInterface.CommandManager.AddHandler("/xlitem", new Dalamud.Game.Command.CommandInfo(OnItemSearchCommand) {
                HelpMessage = Loc.Localize("ItemSearchCommandHelp", "Open a window you can use to link any specific item to chat."),
                ShowInHelp = true
            });

            PluginInterface.CommandManager.AddHandler("/fittingroom", new Dalamud.Game.Command.CommandInfo((command, arguments) => {
                this.FittingRoomUI.OpenFittingRoom();
            }) {
                HelpMessage = Loc.Localize("ItemSearchFittingRoomCommand", "Open the fitting room."),
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
            PluginInterface.CommandManager.RemoveHandler("/fittingroom");
#if DEBUG
            PluginInterface.CommandManager.RemoveHandler("/itemsearchdumploc");
#endif
        }
        
        private Stopwatch debugStopwatch = new Stopwatch();
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


            debugStopwatch.Restart();
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

            var payloadList = new List<Payload> {
                new UIForegroundPayload(PluginInterface.Data, (ushort) (0x223 + item.Rarity * 2)),
                new UIGlowPayload(PluginInterface.Data, (ushort) (0x224 + item.Rarity * 2)),
                new ItemPayload(PluginInterface.Data, item.RowId, item.CanBeHq && PluginInterface.ClientState.KeyState[0x11]),
                new UIForegroundPayload(PluginInterface.Data, 500),
                new UIGlowPayload(PluginInterface.Data, 501),
                new TextPayload($"{(char) SeIconChar.LinkMarker}"),
                new UIForegroundPayload(PluginInterface.Data, 0),
                new UIGlowPayload(PluginInterface.Data, 0),
                new TextPayload(item.Name),
                new RawPayload(new byte[] {0x02, 0x27, 0x07, 0xCF, 0x01, 0x01, 0x01, 0xFF, 0x01, 0x03}),
                new RawPayload(new byte[] {0x02, 0x13, 0x02, 0xEC, 0x03})
            };

            var payload = new SeString(payloadList);

            PluginInterface.Framework.Gui.Chat.PrintChat(new XivChatEntry {
                MessageBytes = payload.Encode()
            });
        }

        internal void DrawIcon(ushort icon, Vector2 size) {
            if (icon < 65000) {
                if (textureDictionary.ContainsKey(icon)) {
                    var tex = textureDictionary[icon];
                    if (tex == null || tex.ImGuiHandle == IntPtr.Zero) {
                        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1));
                        ImGui.BeginChild("FailedTexture", size, true);
                        ImGui.Text(icon.ToString());
                        ImGui.EndChild();
                        ImGui.PopStyleColor();
                    } else {
                        ImGui.Image(textureDictionary[icon].ImGuiHandle, size);
                    }
                } else {
                    ImGui.BeginChild("WaitingTexture", size, true);
                    ImGui.EndChild();

                    textureDictionary[icon] = null;

                    Task.Run(() => {
                        try {
                            var iconTex = PluginInterface.Data.GetIcon(icon);
                            var tex = PluginInterface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width, iconTex.Header.Height, 4);
                            if (tex != null && tex.ImGuiHandle != IntPtr.Zero) {
                                textureDictionary[icon] = tex;
                            }
                        } catch {
                            // Ignore
                        }
                    });
                }
            } else {
                ImGui.BeginChild("NoIcon", size, true);
                if (PluginConfig.ShowItemID) {
                    ImGui.Text(icon.ToString());
                }

                ImGui.EndChild();
            }
        }

        internal void ToggleConfigWindow() {
            drawConfigWindow = !drawConfigWindow;
        }


        private void SetupGameFunctions() {
            cardUnlockedStatic = PluginInterface.TargetModuleScanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? 45 33 C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 0F B6 93");
            var cardUnlockedAddress = PluginInterface.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 8D 7B 78");
            cardUnlocked = Marshal.GetDelegateForFunctionPointer<CardUnlockedDelegate>(cardUnlockedAddress);

            var itemActionUnlockedAddress = PluginInterface.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 A5");
            itemActionUnlocked = Marshal.GetDelegateForFunctionPointer<ItemActionUnlockledDelegate>(itemActionUnlockedAddress);
        }


        private delegate bool ItemActionUnlockledDelegate(long itemActionId);
        private delegate bool CardUnlockedDelegate(IntPtr a1, ushort card);

        private ItemActionUnlockledDelegate itemActionUnlocked;
        private CardUnlockedDelegate cardUnlocked;
        private IntPtr cardUnlockedStatic;

        internal bool IsCardOwned(ushort cardId) {
            return cardUnlocked(cardUnlockedStatic, cardId);
        }

        internal bool IsCollectableOwned(uint actionId) {
            return itemActionUnlocked(actionId);
        }
    }
}
