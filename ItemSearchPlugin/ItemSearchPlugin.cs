using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using ItemSearchPlugin.DataSites;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin {
    public class ItemSearchPlugin : IDalamudPlugin {
        public string Name => "Item Search";


        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static DataManager Data { get; private set; } = null!;
        [PluginService] public static CommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static KeyState KeyState { get; private set; } = null!;
        [PluginService] public static ChatGui Chat { get; private set; } = null!;
        [PluginService] public static SigScanner SigScanner { get; private set; } = null!;
        [PluginService] public static ClientState ClientState { get; private set; } = null!;
        [PluginService] public static GameGui GameGui { get; private set; } = null!;
        [PluginService] public static Framework Framework { get; private set; } = null!;

        public ItemSearchPluginConfig PluginConfig { get; private set; }

        public FittingRoomUI FittingRoomUI { get; private set; }

        public readonly Dictionary<ushort, TextureWrap> textureDictionary = new Dictionary<ushort, TextureWrap>();

        public CraftingRecipeFinder CraftingRecipeFinder { get; private set; }

        internal ItemSearchWindow itemSearchWindow;
        private bool drawItemSearchWindow;

        private bool drawConfigWindow;

        internal List<GenericItem> LuminaItems { get; set; }
        internal ClientLanguage LuminaItemsClientLanguage { get; set; }
        
        public static DataSite[] DataSites { get; private set; } = new DataSite[] { new GarlandToolsDataSite() }; 
        public string Version { get; private set; }

        public void Dispose() {
            PluginInterface.UiBuilder.Draw -= this.BuildUI;
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

        public ItemSearchPlugin() {
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.PluginConfig = (ItemSearchPluginConfig) PluginInterface.GetPluginConfig() ?? new ItemSearchPluginConfig();

            ItemSearchPlugin.DataSites = new DataSite[] {
                new GarlandToolsDataSite(),
                new TeamcraftDataSite(PluginConfig),
                new GamerEscapeDatasite(),
            };

            this.PluginConfig.Init(PluginInterface, this);


            SetupGameFunctions();

            ReloadLocalization();

            FittingRoomUI = new FittingRoomUI(this);

            CraftingRecipeFinder = new CraftingRecipeFinder(this);

            PluginInterface.UiBuilder.Draw += this.BuildUI;
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
            CommandManager.AddHandler("/xlitem", new Dalamud.Game.Command.CommandInfo(OnItemSearchCommand) {
                HelpMessage = Loc.Localize("ItemSearchCommandHelp", "Open a window you can use to link any specific item to chat."),
                ShowInHelp = true
            });

            CommandManager.AddHandler("/fittingroom", new Dalamud.Game.Command.CommandInfo((command, arguments) => {
                this.FittingRoomUI.OpenFittingRoom();
            }) {
                HelpMessage = Loc.Localize("ItemSearchFittingRoomCommand", "Open the fitting room."),
                ShowInHelp = true
            });

#if DEBUG
            CommandManager.AddHandler("/itemsearchdumploc", new Dalamud.Game.Command.CommandInfo(((command, arguments) => {
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
            CommandManager.RemoveHandler("/xlitem");
            CommandManager.RemoveHandler("/fittingroom");
#if DEBUG
            CommandManager.RemoveHandler("/itemsearchdumploc");
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

        internal void LinkItem(GenericItem item) {
            if (item == null) {
                PluginLog.Log("Tried to link NULL item.");
                return;
            }

            var payloadList = new List<Payload> {
                new UIForegroundPayload((ushort) (0x223 + item.Rarity * 2)),
                new UIGlowPayload((ushort) (0x224 + item.Rarity * 2)),
                new ItemPayload(item.RowId, item.CanBeHq && KeyState[0x11]),
                new UIForegroundPayload(500),
                new UIGlowPayload(501),
                new TextPayload($"{(char) SeIconChar.LinkMarker}"),
                new UIForegroundPayload(0),
                new UIGlowPayload(0),
                new TextPayload(item.Name + (item.CanBeHq && KeyState[0x11] ? $" {(char)SeIconChar.HighQuality}" : "")),
                new RawPayload(new byte[] {0x02, 0x27, 0x07, 0xCF, 0x01, 0x01, 0x01, 0xFF, 0x01, 0x03}),
                new RawPayload(new byte[] {0x02, 0x13, 0x02, 0xEC, 0x03})
            };

            var payload = new SeString(payloadList);

            Chat.PrintChat(new XivChatEntry {
                Message = payload
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
                            var iconTex = Data.GetIcon(icon);
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
            cardUnlockedStatic = SigScanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? 45 33 C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 0F B6 93");
            var cardUnlockedAddress = SigScanner.ScanText("E8 ?? ?? ?? ?? 8D 7B 78");
            cardUnlocked = Marshal.GetDelegateForFunctionPointer<CardUnlockedDelegate>(cardUnlockedAddress);

            var itemActionUnlockedAddress = SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 A9");
            itemActionUnlocked = Marshal.GetDelegateForFunctionPointer<ItemActionUnlockedDelegate>(itemActionUnlockedAddress);
        }


        private delegate byte ItemActionUnlockedDelegate(IntPtr data);
        private delegate bool CardUnlockedDelegate(IntPtr a1, ushort card);

        private ItemActionUnlockedDelegate itemActionUnlocked;
        private CardUnlockedDelegate cardUnlocked;
        private IntPtr cardUnlockedStatic;

        internal bool IsCardOwned(ushort cardId) {
            return cardUnlocked(cardUnlockedStatic, cardId);
        }

        internal unsafe bool ItemActionUnlocked(Item item) {
            var itemAction = item.ItemAction.Value;
            if (itemAction == null) {
                return false;
            }

            var type = itemAction.Type;

            var mem = Marshal.AllocHGlobal(256);
            *(uint*) (mem + 142) = itemAction.RowId;

            if (type == 25183) {
                *(uint*) (mem + 112) = item.AdditionalData;
            }

            var ret = this.itemActionUnlocked(mem) == 1;

            Marshal.FreeHGlobal(mem);

            return ret;
        }
    }
}
