using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using CheapLoc;
using Dalamud.Data;
using Dalamud.Data.LuminaExtensions;
using Dalamud.Interface;
using ImGuiNET;
using ImGuiScene;
using ItemSearchPlugin.Filters;
using Serilog;
using Item = Dalamud.Data.TransientSheet.Item;

namespace ItemSearchPlugin {
    class ItemSearchWindow : IDisposable
    {
        private readonly DataManager data;
        private readonly UiBuilder builder;

        private string lastSearchText = string.Empty;
        private string searchText = string.Empty;

        private Item selectedItem;
        private int selectedItemIndex = -1;
        private TextureWrap selectedItemTex;

        private CancellationTokenSource searchCancelTokenSource;
        private ValueTask<List<Item>> searchTask;
        private List<Item> luminaItems;

        private ItemSearchPluginConfig pluginConfig;

        public event EventHandler<Item> OnItemChosen;
        public event EventHandler<bool> OnConfigButton;

        public List<ISearchFilter> searchFilters;


        public ItemSearchWindow(DataManager data, UiBuilder builder, ItemSearchPluginConfig pluginConfig, string searchText = "") {
            this.data = data;
            this.builder = builder;
            this.pluginConfig = pluginConfig;
            this.searchText = searchText;

            while (!data.IsDataReady)
                Thread.Sleep(1);

            searchFilters = new List<ISearchFilter>();
            searchFilters.Add(new ItemUICategorySearchFilter(data));

            Task.Run(() => this.data.GetExcelSheet<Item>().GetRows()).ContinueWith(t => this.luminaItems = t.Result);
        }

        public bool Draw() {
            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);

            var isOpen = true;
            
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(250, 300));

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
            ImGui.SetColumnWidth(0, 80);
            ImGui.Text(Loc.Localize("DalamudItemSearchVerb", "Search: "));
            ImGui.NextColumn();
            ImGui.PushItemWidth(-1);
            ImGui.InputText("##searchbox", ref this.searchText, 32);
            ImGui.PopItemWidth();

            foreach(ISearchFilter filter in searchFilters) {
                if (filter.ShowFilter) {
                    ImGui.NextColumn();
                    ImGui.Text(Loc.Localize(filter.NameLocalizationKey, $"{filter.Name}: "));
                    ImGui.NextColumn();
                    filter.DrawEditor();
                }
            }

            ImGui.Columns(1);
            var windowSize = ImGui.GetWindowSize();
            ImGui.BeginChild("scrolling", new Vector2(0, windowSize.Y - ImGui.GetCursorPosY() - 40), true, ImGuiWindowFlags.HorizontalScrollbar);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

            if (this.luminaItems != null) {
                if (!string.IsNullOrEmpty(this.searchText) || searchFilters.Where(x => x.ShowFilter && x.IsSet).Any())
                {
                    if (this.lastSearchText != this.searchText || searchFilters.Where(x => x.ShowFilter && x.HasChanged).Any())
                    {
                        this.lastSearchText = this.searchText;

                        this.searchCancelTokenSource?.Cancel();

                        this.searchCancelTokenSource = new CancellationTokenSource();

                        var asyncEnum = this.luminaItems.ToAsyncEnumerable();

                        if (!string.IsNullOrEmpty(this.searchText))
                        {
                            Log.Debug("Searching for " + this.searchText);
                            asyncEnum = asyncEnum.Where(
                                x => (x.Name.ToLower().Contains(this.searchText.ToLower()) ||
                                      int.TryParse(this.searchText, out var parsedId) &&
                                      parsedId == x.RowId) && x.Icon < 65000);
                        }

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

            string configText = Loc.Localize("ItemSearchConfigButton", "Config");
            ImGui.SameLine(ImGui.GetWindowWidth() - (ImGui.CalcTextSize(configText).X + ImGui.GetStyle().ItemSpacing.X * 2));
            if (ImGui.Button(configText)) {
                OnConfigButton.Invoke(this, true);
            }

            ImGui.End();

            return isOpen;
        }

        public void Dispose() {

            foreach(ISearchFilter f in searchFilters){
                f?.Dispose();
            }

            this.selectedItemTex?.Dispose();
        }
    }
}
