using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Data;
using Dalamud.Data.LuminaExtensions;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using ItemSearchPlugin.ActionButtons;
using ItemSearchPlugin.Filters;
using Serilog;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin {
    internal class ItemSearchWindow : IDisposable {
        private readonly ItemSearchPlugin plugin;
        private readonly DalamudPluginInterface pluginInterface;
        private readonly DataManager data;
        private readonly UiBuilder builder;
        private Item selectedItem;
        private int selectedItemIndex = -1;

        private readonly Dictionary<ushort, TextureWrap> textureDictionary = new Dictionary<ushort, TextureWrap>();

        private CancellationTokenSource searchCancelTokenSource;
        private ValueTask<List<Item>> searchTask;

        private readonly ItemSearchPluginConfig pluginConfig;
        public List<SearchFilter> SearchFilters;
        public List<IActionButton> ActionButtons;

        private bool autoTryOn;
        private int debounceKeyPress;
        private bool doSearchScroll;
        private bool forceReload;

        private bool errorLoadingItems;


        private int styleCounter;

        private void PushStyle(ImGuiStyleVar styleVar, Vector2 val) {
            ImGui.PushStyleVar(styleVar, val);
            styleCounter += 1;
        }

        private void PushStyle(ImGuiStyleVar styleVar, float val) {
            ImGui.PushStyleVar(styleVar, val);
            styleCounter += 1;
        }

        private void PopStyle() {
            if (styleCounter <= 0) return;
            ImGui.PopStyleVar();
            styleCounter -= 1;
        }

        private void PopStyle(int count) {
            if (count > styleCounter) count = styleCounter;
            ImGui.PopStyleVar(count);
            styleCounter -= count;
        }

        private void ResetStyle() {
            if (styleCounter <= 0) return;
            ImGui.PopStyleVar(styleCounter);
            styleCounter = 0;
        }

        public ItemSearchWindow(ItemSearchPlugin plugin, string searchText = "") {
            this.pluginInterface = plugin.PluginInterface;
            this.data = pluginInterface.Data;
            this.builder = pluginInterface.UiBuilder;
            this.pluginConfig = plugin.PluginConfig;
            this.plugin = plugin;

            while (!data.IsDataReady)
                Thread.Sleep(1);

            SearchFilters = new List<SearchFilter> {
                new ItemNameSearchFilter(pluginConfig, searchText),
                new ItemUICategorySearchFilter(pluginConfig, data),
                new LevelEquipSearchFilter(pluginConfig),
                new LevelItemSearchFilter(pluginConfig),
                new EquipAsSearchFilter(pluginConfig, data),
                new RaceSexSearchFilter(pluginConfig, data),
                new CraftableSearchFilter(pluginConfig, data),
                new DyeableSearchFilter(pluginConfig),
                new StatSearchFilter(pluginConfig, data),
            };

            ActionButtons = new List<IActionButton> {
                new MarketBoardActionButton(pluginInterface, pluginConfig),
                new DataSiteActionButton(pluginConfig),
                new RecipeSearchActionButton(plugin.CraftingRecipeFinder),
            };
        }

        private void UpdateItemList(int delay = 100) {
            errorLoadingItems = false;
            plugin.LuminaItems = null;
            plugin.LuminaItemsClientLanguage = pluginConfig.SelectedClientLanguage;
            Task.Run(async () => {
                await Task.Delay(delay);
                try {
                    return this.data.GetExcelSheet<Item>(pluginConfig.SelectedClientLanguage).Where(i => !string.IsNullOrEmpty(i.Name)).ToList();
                } catch (Exception ex) {
                    errorLoadingItems = true;
                    PluginLog.LogError("Failed loading Items");
                    PluginLog.LogError(ex.ToString());
                    return new List<Item>();
                }
            }).ContinueWith(t => {
                if (errorLoadingItems) {
                    return plugin.LuminaItems;
                }

                forceReload = true;
                return plugin.LuminaItems = t.Result;
            });
        }

        public bool Draw() {
            var isOpen = true;
            try {
                var isSearch = false;
                if (pluginConfig.SelectedClientLanguage != plugin.LuminaItemsClientLanguage) UpdateItemList(1000);


                if ((selectedItemIndex < 0 && selectedItem != null) || (selectedItemIndex >= 0 && selectedItem == null)) {
                    // Should never happen, but just incase
                    selectedItemIndex = -1;
                    selectedItem = null;
                    return true;
                }

                ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
                
                PushStyle(ImGuiStyleVar.WindowMinSize, new Vector2(350, 400));

                if (!ImGui.Begin(Loc.Localize("ItemSearchPlguinMainWindowHeader", "Item Search") + "###itemSearchPluginMainWindow", ref isOpen, ImGuiWindowFlags.NoCollapse)) {
                    ResetStyle();
                    ImGui.End();
                    return false;
                }

                PopStyle();

                // Main window
                ImGui.AlignTextToFramePadding();

                if (selectedItem != null) {
                    var icon = selectedItem.Icon;

                    if (icon < 65000) {
                        if (textureDictionary.ContainsKey(icon)) {
                            var tex = textureDictionary[icon];
                            if (tex == null || tex.ImGuiHandle == IntPtr.Zero) {
                                ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1));
                                ImGui.BeginChild("FailedTexture", new Vector2(45, 45), true);
                                ImGui.Text(icon.ToString());
                                ImGui.EndChild();
                                ImGui.PopStyleColor();
                            } else {
                                ImGui.Image(textureDictionary[icon].ImGuiHandle, new Vector2(45, 45));
                            }
                        } else {
                            ImGui.BeginChild("WaitingTexture", new Vector2(45, 45), true);
                            ImGui.EndChild();

                            textureDictionary[icon] = null;

                            Task.Run(() => {
                                try {
                                    var iconTex = this.data.GetIcon(icon);
                                    var tex = this.builder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width, iconTex.Header.Height, 4);
                                    if (tex != null && tex.ImGuiHandle != IntPtr.Zero) {
                                        textureDictionary[icon] = tex;
                                    }
                                } catch {
                                    // Ignore
                                }
                            });
                        }
                    } else {
                        ImGui.BeginChild("NoIcon", new Vector2(45, 45), true);
                        if (pluginConfig.ShowItemID) {
                            ImGui.Text(icon.ToString());
                        }

                        ImGui.EndChild();
                    }


                    ImGui.SameLine();
                    ImGui.BeginGroup();

                    ImGui.Text(selectedItem.Name);

                    if (pluginConfig.ShowItemID) {
                        ImGui.SameLine();
                        ImGui.Text($"(ID: {selectedItem.RowId}) (Rarity: {selectedItem.Rarity})");
                    }

                    var imGuiStyle = ImGui.GetStyle();
                    var windowVisible = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;

                    IActionButton[] buttons = this.ActionButtons.Where(ab => ab.ButtonPosition == ActionButtonPosition.TOP).ToArray();

                    for (var i = 0; i < buttons.Length; i++) {
                        var button = buttons[i];

                        if (button.GetShowButton(selectedItem)) {
                            var buttonText = button.GetButtonText(selectedItem);
                            ImGui.PushID($"TopActionButton{i}");
                            if (ImGui.Button(buttonText)) {
                                button.OnButtonClicked(selectedItem);
                            }

                            if (i < buttons.Length - 1) {
                                var lX2 = ImGui.GetItemRectMax().X;
                                var nbw = ImGui.CalcTextSize(buttons[i + 1].GetButtonText(selectedItem)).X + imGuiStyle.ItemInnerSpacing.X * 2;
                                var nX2 = lX2 + (imGuiStyle.ItemSpacing.X * 2) + nbw;
                                if (nX2 < windowVisible) {
                                    ImGui.SameLine();
                                }
                            }

                            ImGui.PopID();
                        }
                    }

                    ImGui.EndGroup();
                } else {
                    ImGui.BeginChild("NoSelectedItemBox", new Vector2(200, 45));
                    ImGui.Text(Loc.Localize("ItemSearchSelectItem", "Please select an item."));
                    ImGui.EndChild();
                }

                ImGui.Separator();

                ImGui.Columns(2);
                var filterNameMax = SearchFilters.Where(x => x.IsEnabled && x.ShowFilter).Select(x => {
                    x._LocalizedName = Loc.Localize(x.NameLocalizationKey, x.Name);
                    x._LocalizedNameWidth = ImGui.CalcTextSize($"{x._LocalizedName}").X;
                    return x._LocalizedNameWidth;
                }).Max();

                ImGui.SetColumnWidth(0, filterNameMax + ImGui.GetStyle().ItemSpacing.X * 2);
                var filterInUseColour = new Vector4(0, 1, 0, 1);
                foreach (var filter in SearchFilters.Where(x => x.IsEnabled && x.ShowFilter)) {
                    ImGui.SetCursorPosX((filterNameMax + ImGui.GetStyle().ItemSpacing.X) - filter._LocalizedNameWidth);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3);
                    if (filter.IsSet) {
                        ImGui.TextColored(filterInUseColour, $"{filter._LocalizedName}: ");
                    } else {
                        ImGui.Text($"{filter._LocalizedName}: ");
                    }

                    ImGui.NextColumn();
                    filter.DrawEditor();
                    while (ImGui.GetColumnIndex() != 0)
                        ImGui.NextColumn();
                }

                ImGui.Columns(1);
                var windowSize = ImGui.GetWindowSize();
                var childSize = new Vector2(0, Math.Max(100, windowSize.Y - ImGui.GetCursorPosY() - 40));
                ImGui.BeginChild("scrolling", childSize, true, ImGuiWindowFlags.HorizontalScrollbar);

                PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

                if (errorLoadingItems) {
                    ImGui.TextColored(new Vector4(1f, 0.1f, 0.1f, 1.00f), Loc.Localize("ItemSearchListLoadFailed", "Error loading item list."));
                    if (ImGui.SmallButton("Retry")) {
                        UpdateItemList();
                    }
                } else if (plugin.LuminaItems != null) {
                    if (SearchFilters.Any(x => x.IsEnabled && x.ShowFilter && x.IsSet)) {
                        isSearch = true;
                        if (SearchFilters.Any(x => x.IsEnabled && x.ShowFilter && x.HasChanged) || forceReload) {
                            forceReload = false;
                            this.searchCancelTokenSource?.Cancel();
                            this.searchCancelTokenSource = new CancellationTokenSource();
                            var asyncEnum = plugin.LuminaItems.ToAsyncEnumerable();

                            if (!pluginConfig.ShowLegacyItems) {
                                asyncEnum = asyncEnum.Where(x => x.RowId < 100 || x.RowId > 1600);
                            }

                            asyncEnum = SearchFilters.Where(filter => filter.IsEnabled && filter.ShowFilter && filter.IsSet).Aggregate(asyncEnum, (current, filter) => current.Where(filter.CheckFilter));
                            this.selectedItemIndex = -1;
                            selectedItem = null;
                            this.searchTask = asyncEnum.ToListAsync(this.searchCancelTokenSource.Token);
                        }

                        if (this.searchTask.IsCompletedSuccessfully) {
                            var itemSize = Vector2.Zero;
                            float cursorPosY = 0;
                            var scrollY = ImGui.GetScrollY();
                            var style = ImGui.GetStyle();
                            for (var i = 0; i < this.searchTask.Result.Count; i++) {
                                if (i == 0 && itemSize == Vector2.Zero) {
                                    itemSize = ImGui.CalcTextSize(this.searchTask.Result[i].Name);
                                    if (!doSearchScroll) {
                                        var sizePerItem = itemSize.Y + style.ItemSpacing.Y;
                                        var skipItems = (int) Math.Floor(scrollY / sizePerItem);
                                        cursorPosY = skipItems * sizePerItem;
                                        ImGui.SetCursorPosY(cursorPosY + style.ItemSpacing.X);
                                        i = skipItems;
                                    }
                                }

                                if (!(doSearchScroll && selectedItemIndex == i) && (cursorPosY < scrollY - itemSize.Y || cursorPosY > scrollY + childSize.Y)) {
                                    ImGui.SetCursorPosY(cursorPosY + itemSize.Y + style.ItemSpacing.Y);
                                } else if (ImGui.Selectable(this.searchTask.Result[i].Name, this.selectedItemIndex == i, ImGuiSelectableFlags.AllowDoubleClick)) {
                                    this.selectedItem = this.searchTask.Result[i];
                                    this.selectedItemIndex = i;

                                    if (ImGui.IsMouseDoubleClicked(0)) {
                                        if (this.selectedItem != null && selectedItem.Icon < 65000) {
                                            try {
                                                plugin.LinkItem(selectedItem);
                                                if (pluginConfig.CloseOnChoose) {
                                                    isOpen = false;
                                                }
                                            } catch (Exception ex) {
                                                PluginLog.LogError(ex.ToString());
                                            }
                                        }
                                    }

                                    if ((autoTryOn = autoTryOn && pluginConfig.ShowTryOn) && plugin.FittingRoomUI.CanUseTryOn && pluginInterface.ClientState.LocalPlayer != null) {
                                        if (selectedItem.ClassJobCategory.Row != 0) {
                                            plugin.FittingRoomUI.TryOnItem(selectedItem);
                                        }
                                    }
                                }

                                if (doSearchScroll && selectedItemIndex == i) {
                                    doSearchScroll = false;
                                    ImGui.SetScrollHereY(0.5f);
                                }

                                cursorPosY = ImGui.GetCursorPosY();

                                if (cursorPosY > scrollY + childSize.Y && !doSearchScroll) {
                                    var c = this.searchTask.Result.Count - i;
                                    ImGui.BeginChild("###scrollFillerBottom", new Vector2(0, c * (itemSize.Y + style.ItemSpacing.Y)), false);
                                    ImGui.EndChild();
                                    break;
                                }
                            }

                            var keyStateDown = ImGui.GetIO().KeysDown[0x28] || pluginInterface.ClientState.KeyState[0x28];
                            var keyStateUp = ImGui.GetIO().KeysDown[0x26] || pluginInterface.ClientState.KeyState[0x26];

#if DEBUG
                            // Random up/down if both are pressed
                            if (keyStateUp && keyStateDown) {
                                debounceKeyPress = 0;

                                var r = new Random().Next(0, 5);

                                switch (r) {
                                    case 1:
                                        keyStateUp = true;
                                        keyStateDown = false;
                                        break;
                                    case 0:
                                        keyStateUp = false;
                                        keyStateDown = false;
                                        break;
                                    default:
                                        keyStateUp = false;
                                        keyStateDown = true;
                                        break;
                                }
                            }
#endif

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
                                if ((autoTryOn = autoTryOn && pluginConfig.ShowTryOn) && plugin.FittingRoomUI.CanUseTryOn && pluginInterface.ClientState.LocalPlayer != null) {
                                    if (selectedItem.ClassJobCategory.Row != 0) {
                                        plugin.FittingRoomUI.TryOnItem(selectedItem);
                                    }
                                }
                            }
                        }
                    } else {
                        ImGui.TextColored(new Vector4(0.86f, 0.86f, 0.86f, 1.00f), Loc.Localize("DalamudItemSelectHint", "Type to start searching..."));

                        this.selectedItemIndex = -1;
                        selectedItem = null;
                    }
                } else {
                    ImGui.TextColored(new Vector4(0.86f, 0.86f, 0.86f, 1.00f), Loc.Localize("DalamudItemSelectLoading", "Loading item list..."));
                }

                PopStyle();

                ImGui.EndChild();

                // Darken choose button if it shouldn't be clickable
                PushStyle(ImGuiStyleVar.Alpha, this.selectedItemIndex < 0 || selectedItem == null || selectedItem.Icon >= 65000 ? 0.25f : 1);

                if (ImGui.Button(Loc.Localize("Choose", "Choose"))) {
                    try {
                        if (selectedItem != null && selectedItem.Icon < 65000) {
                            plugin.LinkItem(selectedItem);
                            if (pluginConfig.CloseOnChoose) {
                                isOpen = false;
                            }
                        }
                    } catch (Exception ex) {
                        Log.Error($"Exception in Choose: {ex.Message}");
                    }
                }

                PopStyle();

                if (!pluginConfig.CloseOnChoose) {
                    ImGui.SameLine();
                    if (ImGui.Button(Loc.Localize("Close", "Close"))) {
                        selectedItem = null;
                        isOpen = false;
                    }
                }

                if (this.selectedItemIndex >= 0 && this.selectedItem != null && selectedItem.Icon >= 65000) {
                    ImGui.SameLine();
                    ImGui.Text(Loc.Localize("DalamudItemNotLinkable", "This item is not linkable."));
                }

                if (pluginConfig.ShowTryOn && pluginInterface.ClientState.LocalPlayer != null) {
                    ImGui.SameLine();
                    ImGui.Checkbox(Loc.Localize("ItemSearchTryOnButton", "Try On"), ref autoTryOn);
                }

                var configText = Loc.Localize("ItemSearchConfigButton", "Config");
                var itemCountText = isSearch ? string.Format(Loc.Localize("ItemCount", "{0} Items"), this.searchTask.Result.Count) : $"v{plugin.Version}";
                ImGui.SameLine(ImGui.GetWindowWidth() - (ImGui.CalcTextSize(configText).X + ImGui.GetStyle().ItemSpacing.X) - (ImGui.CalcTextSize(itemCountText).X + ImGui.GetStyle().ItemSpacing.X * (isSearch ? 3 : 2)));
                if (isSearch) {
                    if (ImGui.Button(itemCountText)) {
                        PluginLog.Log("Copying results to Clipboard");

                        var sb = new StringBuilder();

                        if (pluginConfig.PrependFilterListWithCopy) {
                            foreach (var f in SearchFilters.Where(f => f.IsSet)) {
                                sb.AppendLine($"{f.Name}: {f}");
                            }

                            sb.AppendLine();
                        }

                        foreach (var i in this.searchTask.Result) {
                            sb.AppendLine(i.Name);
                        }

                        System.Windows.Forms.Clipboard.SetText(sb.ToString());
                    }

                    if (ImGui.IsItemHovered()) {
                        ImGui.SetTooltip("Copy results to clipboard");
                    }
                } else {
                    ImGui.Text(itemCountText);
                }

                ImGui.SameLine(ImGui.GetWindowWidth() - (ImGui.CalcTextSize(configText).X + ImGui.GetStyle().ItemSpacing.X * 2));
                if (ImGui.Button(configText)) {
                    plugin.ToggleConfigWindow();
                }

                ImGui.End();

                return isOpen;
            } catch (Exception ex) {
                ResetStyle();
                PluginLog.LogError(ex.ToString());
                selectedItem = null;
                selectedItemIndex = -1;
                return isOpen;
            }
        }

        public void Dispose() {
            foreach (var f in SearchFilters) {
                f?.Dispose();
            }

            foreach (var b in ActionButtons) {
                b?.Dispose();
            }

            foreach (var t in textureDictionary) {
                t.Value?.Dispose();
            }

            textureDictionary.Clear();
        }
    }
}
