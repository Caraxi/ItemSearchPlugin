using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CheapLoc;
using Dalamud.Data;
using Dalamud.Data.LuminaExtensions;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using ItemSearchPlugin.Filters;
using Serilog;
using Item = Dalamud.Data.TransientSheet.Item;

namespace ItemSearchPlugin {
    class ItemSearchWindow : IDisposable
    {
        private readonly DalamudPluginInterface pluginInterface;
        private readonly DataManager data;
        private readonly UiBuilder builder;
        private Item selectedItem;
        private int selectedItemIndex = -1;
        private TextureWrap selectedItemTex;

        private CancellationTokenSource searchCancelTokenSource;
        private ValueTask<List<Item>> searchTask;
        private List<Item> luminaItems;

        private ItemSearchPluginConfig pluginConfig;

        public event EventHandler<Item> OnItemChosen;
        public event EventHandler<bool> OnConfigButton;
        public event EventHandler<Item> OnMarketboardOpen;

        public List<ISearchFilter> searchFilters;

        private bool marketBoardResponsed = false;

        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte TryOnDelegate(uint unknownSomethingToDoWithBeingEquipable, uint itemBaseId, byte stainColor, uint itemGlamourId, byte unknownByte);
        private TryOnDelegate TryOn;
        private bool autoTryOn = false;

        private AddressResolver address;

        public ItemSearchWindow(DalamudPluginInterface pluginInterface, ItemSearchPluginConfig pluginConfig, string searchText = "") {
            this.pluginInterface = pluginInterface;
            this.data = pluginInterface.Data;
            this.builder = pluginInterface.UiBuilder;
            this.pluginConfig = pluginConfig;

            while (!data.IsDataReady)
                Thread.Sleep(1);

            searchFilters = new List<ISearchFilter>();
            searchFilters.Add(new ItemNameSearchFilter(searchText));
            searchFilters.Add(new ItemUICategorySearchFilter(data));
            searchFilters.Add(new LevelEquipSearchFilter(pluginConfig));
            searchFilters.Add(new LevelItemSearchFilter(pluginConfig));
            searchFilters.Add(new EquipAsSearchFilter(pluginConfig, data));

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

            Task.Run(() => this.data.GetExcelSheet<Item>().GetRows()).ContinueWith(t => this.luminaItems = t.Result);

            try {
                address = new AddressResolver();
                address.Setup(pluginInterface.TargetModuleScanner);
                TryOn = Marshal.GetDelegateForFunctionPointer<TryOnDelegate>(address.TryOn);
            } catch (Exception ex) {
                PluginLog.LogError(ex.ToString());
            }

        }

        public bool Draw() {
            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);

            var isOpen = true;
            
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(350, 400));

            if (!ImGui.Begin(Loc.Localize("DalamudItemSelectHeader", "Select an item"), ref isOpen, ImGuiWindowFlags.NoCollapse))
            {
                ImGui.PopStyleVar();
                ImGui.End();
                return false;
            }

            ImGui.PopStyleVar();

            // Main window
            ImGui.AlignTextToFramePadding();

            ImGui.Text(Loc.Localize("DalamudItemSelect", "Please select an item."));
            
            if (this.selectedItemTex != null) {
                ImGui.Text(" ");

                ImGui.SetCursorPosY(200f);
                ImGui.SameLine();
                ImGui.Image(this.selectedItemTex.ImGuiHandle, new Vector2(40, 40));

                if (selectedItem != null ){
                    ImGui.SameLine();
                    ImGui.Text(selectedItem.Name);

                    if (pluginConfig.ShowItemID){
                        ImGui.SameLine();
                        ImGui.Text($"(ID: {selectedItem.RowId})");
                    }

                }
            } else {
                ImGui.Text(" ");
            }


            ImGui.Separator();
            
            ImGui.Columns(2);
            float filterNameWidth = searchFilters.Where(f => f.ShowFilter).Select(f => ImGui.CalcTextSize(Loc.Localize(f.NameLocalizationKey, $"{f.Name}: ")).X).Max();

            ImGui.SetColumnWidth(0, filterNameWidth + ImGui.GetStyle().ItemSpacing.X * 2);

            foreach(ISearchFilter filter in searchFilters) {
                if (filter.ShowFilter) {
                    ImGui.Text(Loc.Localize(filter.NameLocalizationKey, $"{filter.Name}: "));
                    ImGui.NextColumn();
                    filter.DrawEditor();
                    while(ImGui.GetColumnIndex() != 0)
                    ImGui.NextColumn();
                }
            }

            ImGui.Columns(1);
            var windowSize = ImGui.GetWindowSize();
            ImGui.BeginChild("scrolling", new Vector2(0, Math.Max(100, windowSize.Y - ImGui.GetCursorPosY() - 40)), true, ImGuiWindowFlags.HorizontalScrollbar);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

            if (this.luminaItems != null) {
                if (searchFilters.Where(x => x.ShowFilter && x.IsSet).Any())
                {
                    if (searchFilters.Where(x => x.ShowFilter && x.HasChanged).Any())
                    {

                        this.searchCancelTokenSource?.Cancel();

                        this.searchCancelTokenSource = new CancellationTokenSource();

                        var asyncEnum = this.luminaItems.ToAsyncEnumerable();

                        foreach(ISearchFilter filter in searchFilters) {
                            if (filter.ShowFilter && filter.IsSet) {
                                asyncEnum = asyncEnum.Where(x => filter.CheckFilter(x));
                            }
                        }

                        this.selectedItemIndex = -1;
                        this.selectedItemTex?.Dispose();
                        this.selectedItemTex = null;

                        this.searchTask = asyncEnum.ToListAsync(this.searchCancelTokenSource.Token);
                    }

                    if (this.searchTask.IsCompletedSuccessfully)
                    {
                        for (var i = 0; i < this.searchTask.Result.Count; i++)
                        {
                            if (ImGui.Selectable(this.searchTask.Result[i].Name, this.selectedItemIndex == i, ImGuiSelectableFlags.AllowDoubleClick))
                            {
                                this.selectedItem = this.searchTask.Result[i];
                                this.selectedItemIndex = i;

                                try
                                {
                                    var iconTex = this.data.GetIcon(this.searchTask.Result[i].Icon);
                                    this.selectedItemTex?.Dispose();
                                
                                    this.selectedItemTex =
                                       this.builder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width,
                                                                 iconTex.Header.Height, 4);
                                } catch (Exception ex)
                                {
                                    Log.Error(ex, "Failed loading item texture");
                                    this.selectedItemTex?.Dispose();
                                    this.selectedItemTex = null;
                                }

                                if (ImGui.IsMouseDoubleClicked(0))
                                {
                                    if (this.selectedItemTex != null){
                                        OnItemChosen?.Invoke(this, this.searchTask.Result[i]);
                                        if (pluginConfig.CloseOnChoose)
                                        {
                                            this.selectedItemTex?.Dispose();
                                            isOpen = false;
                                        }
                                    }
                                }

                                if ((autoTryOn = autoTryOn && pluginConfig.ShowTryOn) && TryOn != null) {
                                    if (selectedItem.ClassJobCategory != 0) {
                                        TryOn(0xFF, (uint)selectedItem.RowId, 0 , 0, 0);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    ImGui.TextColored(new Vector4(0.86f, 0.86f, 0.86f, 1.00f), Loc.Localize("DalamudItemSelectHint", "Type to start searching..."));

                    this.selectedItemIndex = -1;
                    this.selectedItemTex?.Dispose();
                    this.selectedItemTex = null;
                }
            } else {
                ImGui.TextColored(new Vector4(0.86f, 0.86f, 0.86f, 1.00f), Loc.Localize("DalamudItemSelectLoading", "Loading item list..."));
            }

            ImGui.PopStyleVar();

            ImGui.EndChild();

            // Darken choose button if it shouldn't be clickable
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, this.selectedItemIndex < 0 || this.selectedItemTex == null ? 0.25f : 1);

            if (ImGui.Button(Loc.Localize("Choose", "Choose"))) {
                try {
                    if (this.selectedItemTex != null) {
                        OnItemChosen?.Invoke(this, this.searchTask.Result[this.selectedItemIndex]);
                        if (pluginConfig.CloseOnChoose) {
                            this.selectedItemTex?.Dispose();
                            isOpen = false;
                        }
                    }
                } catch (Exception ex) {
                    Log.Error($"Exception in Choose: {ex.Message}");
                }
            }
            ImGui.PopStyleVar();

            if (pluginConfig.MarketBoardPluginIntegration && marketBoardResponsed && this.selectedItemIndex >= 0 && this.searchTask.Result[this.selectedItemIndex].ItemSearchCategory > 0){
                ImGui.SameLine();
                if (ImGui.Button(Loc.Localize("ItemSearchMarketButton", "Market"))){
                    OnMarketboardOpen?.Invoke(this, this.searchTask.Result[this.selectedItemIndex]);
                }
            }


            if (pluginConfig.SelectedDataSite != null) {
                ImGui.SameLine();
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, this.selectedItemIndex < 0 ? 0.25f : 1);
                if (ImGui.Button(string.Format(Loc.Localize("ItemSearchDataSiteViewButton", "View on {0}"), Loc.Localize(pluginConfig.SelectedDataSite.NameTranslationKey, pluginConfig.SelectedDataSite.Name)))) {
                    if (this.selectedItemIndex >= 0) {
                        pluginConfig.SelectedDataSite.OpenItem(this.searchTask.Result[this.selectedItemIndex]);
                    }
                }
                ImGui.PopStyleVar();
            }
            
            if (!pluginConfig.CloseOnChoose) {
                ImGui.SameLine();
                if (ImGui.Button(Loc.Localize("Close", "Close")))
                {
                    this.selectedItemTex?.Dispose();
                    isOpen = false;
                }
            }

            if (this.selectedItemIndex >= 0 && this.selectedItemTex == null) {
                ImGui.SameLine();
                ImGui.Text(Loc.Localize("DalamudItemNotLinkable", "This item is not linkable."));
            }

            if (pluginConfig.ShowTryOn) {
                ImGui.SameLine();
                ImGui.Checkbox(Loc.Localize("ItemSearchTryOnButton", "Try On"), ref autoTryOn);
            }
            string configText = Loc.Localize("ItemSearchConfigButton", "Config");
            ImGui.SameLine(ImGui.GetWindowWidth() - (ImGui.CalcTextSize(configText).X + ImGui.GetStyle().ItemSpacing.X * 2));
            if (ImGui.Button(configText)) {
                OnConfigButton.Invoke(this, true);
            }

            ImGui.End();

            return isOpen;
        }

        public void Dispose() {
            pluginInterface.Unsubscribe("MarketBoardPlugin");
            foreach(ISearchFilter f in searchFilters){
                f?.Dispose();
            }

            this.selectedItemTex?.Dispose();
        }
    }
}
