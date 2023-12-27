global using static ItemSearchPlugin.ItemSearchPlugin;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using ImGuiNET;
using ItemSearchPlugin.DataSites;
using Lumina.Excel.GeneratedSheets;
using HousingUnitedExterior = Lumina.Excel.GeneratedSheets2.HousingUnitedExterior;

namespace ItemSearchPlugin {
    public class ItemSearchPlugin : IDalamudPlugin {
        public string Name => "Item Search";


        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static IDataManager Data { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IKeyState KeyState { get; private set; } = null!;
        [PluginService] public static IChatGui Chat { get; private set; } = null!;
        [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static IGameGui GameGui { get; private set; } = null!;
        [PluginService] public static IFramework Framework { get; private set; } = null!;
        [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] public static IPluginLog PluginLog { get; private set; }

        public ItemSearchPluginConfig PluginConfig { get; private set; }

        public TryOn TryOn { get; private set; }

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
            CraftingRecipeFinder?.Dispose();
            itemSearchWindow?.Dispose();
            TryOn?.Dispose();
            RemoveCommands();
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

            TryOn = new TryOn(this);

            CraftingRecipeFinder = new CraftingRecipeFinder();

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
                this.TryOn.OpenFittingRoom();
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

#if DEBUG
            if (PluginInterface.IsDebugging && PluginInterface.IsDev) {
                ImGui.BeginMainMenuBar();
                if (ImGui.MenuItem("ItemSearch")) {
                    itemSearchWindow?.Dispose();
                    itemSearchWindow = new ItemSearchWindow(this);
                    drawItemSearchWindow = true;
                }

                ImGui.EndMainMenuBar();
            }
#endif

        }

        internal void LinkItem(GenericItem item) {
            if (item == null) {
                PluginLog.Warning("Tried to link NULL item.");
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

            Chat.Print(new XivChatEntry {
                Message = payload
            });
        }

        internal void DrawIcon(ushort icon, Vector2 size) {
            if (icon < 65000) {
                var tex = TextureProvider.GetIcon(icon);
                if (tex == null || tex.ImGuiHandle == nint.Zero) {
                    ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1));
                    ImGui.BeginChild("FailedTexture", size, true);
                    ImGui.Text(icon.ToString());
                    ImGui.EndChild();
                    ImGui.PopStyleColor();
                } else {
                    ImGui.Image(tex.ImGuiHandle, size);
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

            //var itemActionUnlockedAddress = SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 A9");
            //itemActionUnlocked = Marshal.GetDelegateForFunctionPointer<ItemActionUnlockedDelegate>(itemActionUnlockedAddress);
        }


        private delegate byte ItemActionUnlockedDelegate(IntPtr data);
        private delegate bool CardUnlockedDelegate(IntPtr a1, ushort card);

        private ItemActionUnlockedDelegate itemActionUnlocked;
        private CardUnlockedDelegate cardUnlocked;
        private IntPtr cardUnlockedStatic;

        internal bool IsCardOwned(ushort cardId) {
            return cardUnlocked(cardUnlockedStatic, cardId);
        }

        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct LayoutWorld {
            [FieldOffset(0x20)] public LayoutManagerStruct* LayoutManager;


            [StructLayout(LayoutKind.Explicit)]
            public struct LayoutManagerStruct {
                [FieldOffset(0x90)] public void* IndoorAreaData;
            }
        }

        public unsafe LayoutWorld** LayoutWorldPtr;
        public unsafe delegate void SetInteriorFixture(LayoutWorld.LayoutManagerStruct* layoutManager, int floor, int part, int fixture, byte unknown = 255);

        private SetInteriorFixture setInteriorFixture;
        public unsafe delegate void SetExteriorFixture(void* housingController, uint plot, int part, ushort fixture);
        private SetExteriorFixture setExteriorFixture;
        public unsafe delegate void StainExteriorFixture(void* housingController, uint plot, int part, byte stain);
        private StainExteriorFixture stainExteriorFixture;
        
        [StructLayout(LayoutKind.Explicit)]
        private struct OutdoorTerritoryExtension {
            [FieldOffset(0x00)] public HousingOutdoorTerritory Base;
            [FieldOffset(0x96A8)] public sbyte StandingInPlot;
            [FieldOffset(0x96AA)] public sbyte EditingFixturesOfPlot;
        }
        
        internal unsafe void PreviewExteriorHousingItem(GenericItem gItem, uint stainId) {
            if (gItem.GenericItemType != GenericItem.ItemType.Item) return;
            var item = (Item)gItem;
            var part = -1;
            var fixtureId = item.AdditionalData;
            
            part = item.ItemUICategory.Row switch {
                65 => 0, // Roof
                66 => 1, // Exterior Wall
                67 => 2, // Window
                68 => 3, // Door
                69 => 4, // Roof Decoration
                70 => 5, // Exterior Wall Decoration
                71 => 6, // Placard
                72 => 7, // Fence
                _ => -1    
            };

            if (part == -1) return;
            
#if DEBUG
            // Bypass HousingEditExterior requirement in debug.
            if (!(ImGui.GetIO().KeysDown[(int)VirtualKey.SHIFT] || KeyState[VirtualKey.SHIFT]) && GameGui.GetAddonByName("HousingEditExterior", 1) == IntPtr.Zero) return;
#else
            if (GameGui.GetAddonByName("HousingEditExterior", 1) == IntPtr.Zero) return;
#endif
            
            PluginLog.Debug($"Preview Housing Exterior: {item.Name.ToDalamudString().TextValue}");
            
            if (setExteriorFixture == null) {
                setExteriorFixture = Marshal.GetDelegateForFunctionPointer<SetExteriorFixture>(SigScanner.ScanText("E8 ?? ?? ?? ?? 44 0F B6 0E 41 80 F9 FF"));
                if (setExteriorFixture == null) return;
            }
            
            if (stainExteriorFixture == null) {
                stainExteriorFixture = Marshal.GetDelegateForFunctionPointer<StainExteriorFixture>(SigScanner.ScanText("40 55 48 83 EC 30 41 0F B6 E9"));
                if (stainExteriorFixture == null) return;
            }

            var layout = FFXIVClientStructs.FFXIV.Client.LayoutEngine.LayoutWorld.Instance();
            if (layout == null) return;
            var active = layout->ActiveLayout;
            if (active == null) return;
            var controller = active->HousingController;
            if (controller == null) return;
            var manager = HousingManager.Instance();
            if (manager == null) return;
            var territory = (OutdoorTerritoryExtension*) manager->OutdoorTerritory;
            if (territory == null) return;
            
            var plotIndex = territory->EditingFixturesOfPlot >= 0 ? territory->EditingFixturesOfPlot : territory->StandingInPlot;
            if (plotIndex is < 0 or >= 60) return;

            var plot = (uint)plotIndex + 1;
            var plotSize = territory->Base.PlotSpan[plotIndex].Size;
            
            var unitedExterior = Data.GetExcelSheet<HousingUnitedExterior>()?.GetRow(fixtureId);
            if (unitedExterior != null) {
                if ((PlotSize)unitedExterior.PlotSize != plotSize) {
                    PluginLog.Debug("Fail: Incorrect Plot Size");
                    return;
                }
                setExteriorFixture(controller, plot, 0, (ushort)unitedExterior.Roof.Row);
                setExteriorFixture(controller, plot, 1, (ushort)unitedExterior.Walls.Row);
                setExteriorFixture(controller, plot, 2, (ushort)unitedExterior.Windows.Row);
                setExteriorFixture(controller, plot, 3, (ushort)unitedExterior.Door.Row);
                setExteriorFixture(controller, plot, 4, (ushort)unitedExterior.OptionalRoof.Row);
                setExteriorFixture(controller, plot, 5, (ushort)unitedExterior.OptionalWall.Row);
                setExteriorFixture(controller, plot, 6, (ushort)unitedExterior.OptionalSignboard.Row);
                setExteriorFixture(controller, plot, 7, (ushort)unitedExterior.Fence.Row);
                for (var i = 0; i < 8; i++) stainExteriorFixture(controller, plot, i, (byte)stainId);
            } else {
                if (fixtureId > ushort.MaxValue) return;
                var fixture = Data.GetExcelSheet<HousingExterior>()?.GetRow(fixtureId);
                if (fixture == null) return; // Didn't Exist?
                if (fixture.HousingSize != 254 && (PlotSize) fixture.HousingSize != plotSize) {
                    PluginLog.Debug("Fail: Incorrect Plot Size");
                    return; // Invalid Size
                }
                setExteriorFixture(controller, plot, part, (ushort)fixtureId);
                stainExteriorFixture(controller, plot, part, (byte)stainId);
            }
        }
        
        internal unsafe void PreviewHousingItem(GenericItem gItem) {
            if (gItem.GenericItemType != GenericItem.ItemType.Item) return;
            var item = (Item)gItem;


            var part = -1;
            var fixtureId = (int) item.AdditionalData;

            part = item.ItemUICategory.Row switch {
                73 => 0, // Walls
                74 => 3, // Floors
                75 => 4, // Lights
                _ => -1
            };

            if (part == -1) return;

            #if DEBUG
            // Bypass HousingEditInterior requirement in debug.
            if (!(ImGui.GetIO().KeysDown[(int)VirtualKey.SHIFT] || KeyState[VirtualKey.SHIFT]) && GameGui.GetAddonByName("HousingEditInterior", 1) == IntPtr.Zero) return;
            #else
            if (GameGui.GetAddonByName("HousingEditInterior", 1) == IntPtr.Zero) return;
            #endif
            
            
            if (LayoutWorldPtr == null) {
                LayoutWorldPtr = (LayoutWorld**) SigScanner.GetStaticAddressFromSig("48 89 05 ?? ?? ?? ?? 48 8B 00");
                if (LayoutWorldPtr == null) return;
            }

            if (setInteriorFixture == null) {
                setInteriorFixture = Marshal.GetDelegateForFunctionPointer<SetInteriorFixture>(SigScanner.ScanText("E8 ?? ?? ?? ?? 33 C0 48 83 C4 38 C3 33 C0"));
                if (setInteriorFixture == null) return;
            }
            
            var lWorld = LayoutWorldPtr[0];
            if (lWorld == null) return;
            var lManager = lWorld->LayoutManager;
            if (lManager == null) return;
            if (lManager->IndoorAreaData == null) return;

            PluginLog.Debug($"Preview Housing Fixture: {item.Name.RawString}");
            setInteriorFixture(lManager, 0, part, fixtureId);
            setInteriorFixture(lManager, 1, part, fixtureId);
            setInteriorFixture(lManager, 2, part, fixtureId);
        }
        

        internal unsafe bool ItemActionUnlocked(Item item) {
            return false;
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
