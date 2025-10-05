global using static ItemSearchPlugin.ItemSearchPlugin;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Textures;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Dalamud.Bindings.ImGui;
using ItemSearchPlugin.DataSites;
using Lumina.Excel.Sheets;
using HousingUnitedExterior = Lumina.Excel.Sheets.HousingUnitedExterior;

namespace ItemSearchPlugin {
    public class ItemSearchPlugin : IDalamudPlugin {
        public string Name => "Item Search";


        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static IDataManager Data { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IKeyState KeyState { get; private set; } = null!;
        [PluginService] public static IChatGui Chat { get; private set; } = null!;
        [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static IGameGui GameGui { get; private set; } = null!;
        [PluginService] public static IFramework Framework { get; private set; } = null!;
        [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
		[PluginService] public static IPluginLog PluginLog { get; private set; } = null!;

        public ItemSearchPluginConfig PluginConfig { get; private set; }

        public TryOn TryOn { get; private set; }

        public CraftingRecipeFinder CraftingRecipeFinder { get; private set; }

        internal ItemSearchWindow itemSearchWindow;
        private bool drawItemSearchWindow;

        private bool drawConfigWindow;

        internal List<GenericItem> LuminaItems { get; set; }
        internal ClientLanguage LuminaItemsClientLanguage { get; set; }
        
        public static DataSite[] DataSites { get; private set; } = new DataSite[] { new GarlandToolsDataSite(ClientState) }; 
        public string Version { get; private set; }

        public void Dispose() {
            PluginInterface.UiBuilder.Draw -= BuildUI;
            CraftingRecipeFinder?.Dispose();
            itemSearchWindow?.Dispose();
            TryOn?.Dispose();
            RemoveCommands();
        }

        public ItemSearchPlugin() {
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            PluginConfig = (ItemSearchPluginConfig) PluginInterface.GetPluginConfig() ?? new ItemSearchPluginConfig();

            DataSites = [
                new GarlandToolsDataSite(ClientState),
                new TeamcraftDataSite(PluginConfig),
                new GamerEscapeDatasite()
            ];

            PluginConfig.Init(PluginInterface, this);

            ReloadLocalization();

            TryOn = new TryOn(this);

            CraftingRecipeFinder = new CraftingRecipeFinder();

            PluginInterface.UiBuilder.Draw += BuildUI;
            SetupCommands();

#if DEBUG
            OnItemSearchCommand("", "");
#endif

            PluginInterface.UiBuilder.OpenMainUi += () => OnItemSearchCommand(string.Empty, string.Empty);
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigWindow;

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
                TryOn.OpenFittingRoom();
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
            
            drawConfigWindow = drawConfigWindow && PluginConfig.DrawConfigUI();
            
            if (drawItemSearchWindow) {

                drawItemSearchWindow = itemSearchWindow != null && itemSearchWindow.Draw();
                

                if (drawItemSearchWindow == false) {
                    itemSearchWindow?.Dispose();
                    itemSearchWindow = null;
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

        private Stopwatch iconFailTime = new();
        internal void DrawIcon(ushort icon, Vector2 size) {
            if (icon < 65000) {
                var tex = TextureProvider.GetFromGameIcon(new GameIconLookup(icon, false, false, ClientLanguage.English)).GetWrapOrDefault();
                if (tex == null || tex.Handle == nint.Zero) {
                    if (iconFailTime.ElapsedMilliseconds > 1000) {
                        ImGui.BeginChild("FailedTexture", size, true);
                        ImGui.Text(icon.ToString());
                        ImGui.EndChild();
                    } else {
                        if (!iconFailTime.IsRunning) iconFailTime.Restart();
                        ImGui.Dummy(size);
                    }
                } else {
                    iconFailTime.Reset();
                    ImGui.Image(tex.Handle, size);
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

        internal unsafe bool IsCardOwned(ushort cardId) {
            return UIState.Instance()->IsTripleTriadCardUnlocked(cardId);
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
            [FieldOffset(0x00)] public OutdoorTerritory Base;
            [FieldOffset(0x96A8)] public sbyte StandingInPlot;
            [FieldOffset(0x96AA)] public sbyte EditingFixturesOfPlot;
        }
        
        internal unsafe void PreviewExteriorHousingItem(GenericItem gItem, uint stainId) {
            if (gItem.GenericItemType != GenericItem.ItemType.Item) return;
            var item = (Item)gItem;
            var part = -1;
            var fixtureId = item.AdditionalData.RowId;
            
            part = item.ItemUICategory.RowId switch {
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
            var outdoorAreaData = active->OutdoorAreaData;
            if (outdoorAreaData == null) return;
            var manager = HousingManager.Instance();
            if (manager == null) return;
            var territory = (OutdoorTerritoryExtension*) manager->OutdoorTerritory;
            if (territory == null) return;
            
            var plotIndex = territory->EditingFixturesOfPlot >= 0 ? territory->EditingFixturesOfPlot : territory->StandingInPlot;
            if (plotIndex is < 0 or >= 60) return;

            var plot = (uint)plotIndex + 1;
            var plotSize = territory->Base.Plots[plotIndex].Size;
            
            var unitedExteriorMaybe = Data.GetExcelSheet<HousingUnitedExterior>()?.GetRow(fixtureId);
            if (unitedExteriorMaybe.HasValue) {
                var unitedExterior = unitedExteriorMaybe.Value;
                if ((PlotSize)unitedExterior.PlotSize != plotSize) {
                    PluginLog.Debug("Fail: Incorrect Plot Size");
                    return;
                }
                setExteriorFixture(outdoorAreaData, plot, 0, (ushort)unitedExterior.Roof.RowId);
                setExteriorFixture(outdoorAreaData, plot, 1, (ushort)unitedExterior.Walls.RowId);
                setExteriorFixture(outdoorAreaData, plot, 2, (ushort)unitedExterior.Windows.RowId);
                setExteriorFixture(outdoorAreaData, plot, 3, (ushort)unitedExterior.Door.RowId);
                setExteriorFixture(outdoorAreaData, plot, 4, (ushort)unitedExterior.OptionalRoof.RowId);
                setExteriorFixture(outdoorAreaData, plot, 5, (ushort)unitedExterior.OptionalWall.RowId);
                setExteriorFixture(outdoorAreaData, plot, 6, (ushort)unitedExterior.OptionalSignboard.RowId);
                setExteriorFixture(outdoorAreaData, plot, 7, (ushort)unitedExterior.Fence.RowId);
                for (var i = 0; i < 8; i++) stainExteriorFixture(outdoorAreaData, plot, i, (byte)stainId);
            } else {
                if (fixtureId > ushort.MaxValue) return;
                var fixtureMaybe = Data.GetExcelSheet<HousingExterior>()?.GetRow(fixtureId);
                if (!fixtureMaybe.HasValue) return; // Didn't Exist?
                var fixture = fixtureMaybe.Value;
                if (fixture.HousingSize != 254 && (PlotSize) fixture.HousingSize != plotSize) {
                    PluginLog.Debug("Fail: Incorrect Plot Size");
                    return; // Invalid Size
                }
                setExteriorFixture(outdoorAreaData, plot, part, (ushort)fixtureId);
                stainExteriorFixture(outdoorAreaData, plot, part, (byte)stainId);
            }
        }
        
        internal unsafe void PreviewHousingItem(GenericItem gItem) {
            if (gItem.GenericItemType != GenericItem.ItemType.Item) return;
            var item = (Item)gItem;


            var part = -1;
            var fixtureId = (int) item.AdditionalData.RowId;

            part = item.ItemUICategory.RowId switch {
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

            PluginLog.Debug($"Preview Housing Fixture: {item.Name}");
            setInteriorFixture(lManager, 0, part, fixtureId);
            setInteriorFixture(lManager, 1, part, fixtureId);
            setInteriorFixture(lManager, 2, part, fixtureId);
        }
    }
}
