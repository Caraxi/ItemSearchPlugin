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
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using ItemSearchPlugin.ActionButtons;
using ItemSearchPlugin.Filters;
using Serilog;
using Item = Dalamud.Data.TransientSheet.Item;

namespace ItemSearchPlugin {
    class ItemSearchWindow : IDisposable {
        private readonly ItemSearchPlugin plugin;
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

        public List<ISearchFilter> searchFilters;
        public List<IActionButton> actionButtons;

        private bool autoTryOn;
        private int debounceKeyPress;
        private bool doSearchScroll;

        public ItemSearchWindow(ItemSearchPlugin plugin, string searchText = "") {
            this.pluginInterface = plugin.PluginInterface;
            this.data = pluginInterface.Data;
            this.builder = pluginInterface.UiBuilder;
            this.pluginConfig = plugin.PluginConfig;
            this.plugin = plugin;

            while (!data.IsDataReady)
                Thread.Sleep(1);

            searchFilters = new List<ISearchFilter>();
            searchFilters.Add(new ItemNameSearchFilter(searchText));
            searchFilters.Add(new ItemUICategorySearchFilter(data));
            searchFilters.Add(new LevelEquipSearchFilter(pluginConfig));
            searchFilters.Add(new LevelItemSearchFilter(pluginConfig));
            searchFilters.Add(new EquipAsSearchFilter(pluginConfig, data));
            searchFilters.Add(new RaceSexSearchFilter(pluginConfig, data));

            actionButtons = new List<IActionButton>();
            actionButtons.Add(new MarketBoardActionButton(pluginInterface, pluginConfig));
            actionButtons.Add(new DataSiteActionButton(pluginConfig));

            Task.Run(() => this.data.GetExcelSheet<Item>().GetRows()).ContinueWith(t => this.luminaItems = t.Result);
        }

        public bool Draw() {
            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);

            var isOpen = true;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(350, 400));

            if (!ImGui.Begin(Loc.Localize("DalamudItemSelectHeader", "Select an item"), ref isOpen, ImGuiWindowFlags.NoCollapse)) {
                ImGui.PopStyleVar();
                ImGui.End();
                return false;
            }

            ImGui.PopStyleVar();

            // Main window
            ImGui.AlignTextToFramePadding();

            if (this.selectedItemTex != null) {
                ImGui.SetCursorPosY(200f);
                ImGui.SameLine();
                ImGui.Image(this.selectedItemTex.ImGuiHandle, new Vector2(45, 45));

                if (selectedItem != null) {
                    ImGui.SameLine();
                    ImGui.BeginGroup();

                    ImGui.Text(selectedItem.Name);

                    if (pluginConfig.ShowItemID) {
                        ImGui.SameLine();
                        ImGui.Text($"(ID: {selectedItem.RowId})");
                    }

                    var imGuiStyle = ImGui.GetStyle();
                    var imGuiWindowPos = ImGui.GetWindowPos();
                    var windowVisible = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;

                    float currentX = ImGui.GetCursorPosX();

                    IActionButton[] buttons = this.actionButtons.Where(ab => ab.ButtonPosition == ActionButtonPosition.TOP).ToArray();

                    for (int i = 0; i < buttons.Length; i++) {
                        IActionButton button = buttons[i];

                        if (button.GetShowButton(selectedItem)) {
                            string buttonText = button.GetButtonText(selectedItem);
                            ImGui.PushID($"TopActionButton{i}");
                            if (ImGui.Button(buttonText)) {
                                button.OnButtonClicked(selectedItem);
                            }

                            if (i < buttons.Length - 1) {
                                float l_x2 = ImGui.GetItemRectMax().X;
                                float nbw = ImGui.CalcTextSize(buttons[i + 1].GetButtonText(selectedItem)).X + imGuiStyle.ItemInnerSpacing.X * 2;
                                float n_x2 = l_x2 + (imGuiStyle.ItemSpacing.X * 2) + nbw;
                                if (n_x2 < windowVisible) {
                                    ImGui.SameLine();
                                }
                            }

                            ImGui.PopID();
                        }
                    }

                    ImGui.EndGroup();
                }
            } else {
                ImGui.BeginChild("NoTextureBox", new Vector2(200, 45));
                ImGui.Text(Loc.Localize("ItemSearchSelectItem", "Please select an item."));
                ImGui.EndChild();
            }


            ImGui.Separator();

            ImGui.Columns(2);
            float filterNameWidth = searchFilters.Where(f => f.ShowFilter).Select(f => ImGui.CalcTextSize(Loc.Localize(f.NameLocalizationKey, $"{f.Name}: ")).X).Max();

            ImGui.SetColumnWidth(0, filterNameWidth + ImGui.GetStyle().ItemSpacing.X * 2);
            Vector4 filterInUseColour = new Vector4(0, 1, 0, 1);
            foreach (ISearchFilter filter in searchFilters) {
                if (filter.ShowFilter) {
                    if (filter.IsSet) {
                        ImGui.TextColored(filterInUseColour, Loc.Localize(filter.NameLocalizationKey, $"{filter.Name}: "));
                    } else {
                        ImGui.Text(Loc.Localize(filter.NameLocalizationKey, $"{filter.Name}: "));
                    }

                    ImGui.NextColumn();
                    filter.DrawEditor();
                    while (ImGui.GetColumnIndex() != 0)
                        ImGui.NextColumn();
                }
            }

            ImGui.Columns(1);
            var windowSize = ImGui.GetWindowSize();
            ImGui.BeginChild("scrolling", new Vector2(0, Math.Max(100, windowSize.Y - ImGui.GetCursorPosY() - 40)), true, ImGuiWindowFlags.HorizontalScrollbar);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

            if (this.luminaItems != null) {
                if (searchFilters.Any(x => x.ShowFilter && x.IsSet)) {
                    if (searchFilters.Any(x => x.ShowFilter && x.HasChanged)) {
                        this.searchCancelTokenSource?.Cancel();

                        this.searchCancelTokenSource = new CancellationTokenSource();

                        var asyncEnum = this.luminaItems.ToAsyncEnumerable();

                        if (!pluginConfig.ShowLegacyItems) {
                            asyncEnum = asyncEnum.Where(x => {
                                if (x.RowId >= 100 && x.RowId <= 1600) return false;
                                return true;
                            });
                        }

                        foreach (ISearchFilter filter in searchFilters) {
                            if (filter.ShowFilter && filter.IsSet) {
                                asyncEnum = asyncEnum.Where(x => filter.CheckFilter(x));
                            }
                        }

                        this.selectedItemIndex = -1;
                        this.selectedItemTex?.Dispose();
                        this.selectedItemTex = null;

                        this.searchTask = asyncEnum.ToListAsync(this.searchCancelTokenSource.Token);
                    }

                    if (this.searchTask.IsCompletedSuccessfully) {
                        for (var i = 0; i < this.searchTask.Result.Count; i++) {
                            if (ImGui.Selectable(this.searchTask.Result[i].Name, this.selectedItemIndex == i, ImGuiSelectableFlags.AllowDoubleClick)) {
                                this.selectedItem = this.searchTask.Result[i];
                                this.selectedItemIndex = i;

                                try {
                                    var iconTex = this.data.GetIcon(this.searchTask.Result[i].Icon);
                                    this.selectedItemTex?.Dispose();

                                    this.selectedItemTex =
                                        this.builder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width,
                                            iconTex.Header.Height, 4);
                                } catch (Exception ex) {
                                    Log.Error(ex, "Failed loading item texture");
                                    this.selectedItemTex?.Dispose();
                                    this.selectedItemTex = null;
                                }

                                if (ImGui.IsMouseDoubleClicked(0)) {
                                    if (this.selectedItemTex != null) {
                                        OnItemChosen?.Invoke(this, this.searchTask.Result[i]);
                                        if (pluginConfig.CloseOnChoose) {
                                            this.selectedItemTex?.Dispose();
                                            isOpen = false;
                                        }
                                    }
                                }

                                if ((autoTryOn = autoTryOn && pluginConfig.ShowTryOn) && plugin.FittingRoomUI.CanUseTryOn && pluginInterface.ClientState.LocalPlayer != null) {
                                    if (selectedItem.ClassJobCategory != 0) {
                                        plugin.FittingRoomUI.TryOnItem(selectedItem);
                                    }
                                }
                            }

                            if (doSearchScroll && selectedItemIndex == i) {
                                doSearchScroll = false;
                                ImGui.SetScrollHereY(0.5f);
                            }
                        }

                        var keyStateDown = ImGui.GetIO().KeysDown[0x28] || pluginInterface.ClientState.KeyState[0x28];
                        var keyStateUp = ImGui.GetIO().KeysDown[0x26] || pluginInterface.ClientState.KeyState[0x26];

                        var hotkeyUsed = false;
                        if (keyStateUp && !keyStateDown) {
                            if (debounceKeyPress == 0) {
                                debounceKeyPress = 5;
                                if (selectedItemIndex > 0) {
                                    hotkeyUsed = true;
                                    selectedItemIndex -= 1;
                                }
                            }
                        } else if (keyStateDown && !keyStateUp) {
                            if (debounceKeyPress == 0) {
                                debounceKeyPress = 5;
                                if (selectedItemIndex < searchTask.Result.Count - 1) {
                                    selectedItemIndex += 1;
                                    hotkeyUsed = true;
                                }
                            }
                        } else if (debounceKeyPress > 0) {
                            debounceKeyPress -= 1;
                            if (debounceKeyPress < 0) {
                                debounceKeyPress = 5;
                            }
                        }

                        if (hotkeyUsed) {
                            doSearchScroll = true;
                            this.selectedItem = this.searchTask.Result[selectedItemIndex];
                            try {
                                var iconTex = this.data.GetIcon(this.searchTask.Result[selectedItemIndex].Icon);
                                this.selectedItemTex?.Dispose();

                                this.selectedItemTex =
                                    this.builder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width,
                                        iconTex.Header.Height, 4);
                            } catch (Exception ex) {
                                Log.Error(ex, "Failed loading item texture");
                                this.selectedItemTex?.Dispose();
                                this.selectedItemTex = null;
                            }

                            if ((autoTryOn = autoTryOn && pluginConfig.ShowTryOn) && plugin.FittingRoomUI.CanUseTryOn && pluginInterface.ClientState.LocalPlayer != null) {
                                if (selectedItem.ClassJobCategory != 0) {
                                    plugin.FittingRoomUI.TryOnItem(selectedItem);
                                }
                            }
                        }
                    }
                } else {
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
                if (ImGui.Button(Loc.Localize("Close", "Close"))) {
                    this.selectedItemTex?.Dispose();
                    isOpen = false;
                }
            }

            if (this.selectedItemIndex >= 0 && this.selectedItemTex == null) {
                ImGui.SameLine();
                ImGui.Text(Loc.Localize("DalamudItemNotLinkable", "This item is not linkable."));
            }

            if (pluginConfig.ShowTryOn && pluginInterface.ClientState.LocalPlayer != null) {
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
            foreach (ISearchFilter f in searchFilters) {
                f?.Dispose();
            }

            foreach (IActionButton b in actionButtons) {
                b?.Dispose();
            }

            this.selectedItemTex?.Dispose();
        }
    }
}
