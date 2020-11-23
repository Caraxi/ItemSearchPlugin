using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private bool triedLoadingItems = false;

        private int styleCounter;

        private Stain selectedStain;
        private readonly List<Stain> stains;
        private bool showStainSelector;
        private Vector4 selectedStainColor = Vector4.Zero;
        private readonly Dictionary<byte, Vector4> stainShadeHeaders;

        private bool extraFiltersExpanded;
        
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
            extraFiltersExpanded = pluginConfig.ExpandedFilters;
            autoTryOn = pluginConfig.ShowTryOn && pluginConfig.TryOnEnabled;

            while (!data.IsDataReady)
                Thread.Sleep(1);

            stains = data.Excel.GetSheet<Stain>().ToList();
            FixStainsOrder();

            if (pluginConfig.SelectedStain > 0) {
                selectedStain = stains.FirstOrDefault(s => s.RowId == pluginConfig.SelectedStain);
                if (selectedStain != null) {
                    var b = selectedStain.Color & 255;
                    var g = (selectedStain.Color >> 8) & 255;
                    var r = (selectedStain.Color >> 16) & 255;
                    selectedStainColor = new Vector4(r / 255f, g / 255f, b / 255f, 1f);
                }
            }


            stainShadeHeaders = new Dictionary<byte, Vector4> {
                {2, new Vector4(1, 1, 1, 1)},
                {4, new Vector4(1, 0, 0, 1)},
                {5, new Vector4(0.75f, 0.5f, 0.3f, 1)},
                {6, new Vector4(1f, 1f, 0.1f, 1)},
                {7, new Vector4(0.5f, 1f, 0.25f, 1f)},
                {8, new Vector4(0.3f, 0.5f, 1f, 1f)},
                {9, new Vector4(0.7f, 0.45f, 0.9f, 1)},
                {10, new Vector4(1f, 1f, 1f, 1f)}
            };
            
            SearchFilters = new List<SearchFilter> {
                new ItemNameSearchFilter(pluginConfig, this, searchText),
                new ItemUICategorySearchFilter(pluginConfig, data),
                new LevelEquipSearchFilter(pluginConfig),
                new LevelItemSearchFilter(pluginConfig),
                new RaritySearchFilter(pluginConfig),
                new EquipAsSearchFilter(pluginConfig, data, pluginInterface),
                new RaceSexSearchFilter(pluginConfig, data, pluginInterface),
                new CraftableSearchFilter(pluginConfig, data),
                new DesynthableSearchFilter(pluginConfig, data),
                new BooleanSearchFilter(pluginConfig, "Dyeable", "Dyeable", "Not Dyeable", BooleanSearchFilter.CheckFunc("IsDyeable")),
                new BooleanSearchFilter(pluginConfig, "Unique", "Unique", "Not Unique", BooleanSearchFilter.CheckFunc("IsUnique")),
                new BooleanSearchFilter(pluginConfig, "Tradable", "Tradable", "Not Tradable", BooleanSearchFilter.CheckFunc("IsUntradable", true)),
                new StatSearchFilter(pluginConfig, data),
            };

            SearchFilters.ForEach(a => a.ConfigSetup());

            ActionButtons = new List<IActionButton> {
                new MarketBoardActionButton(pluginInterface, pluginConfig),
                new DataSiteActionButton(pluginConfig),
                new RecipeSearchActionButton(plugin.CraftingRecipeFinder),
                new CopyItemAsJson(plugin),
            };
        }

        private void UpdateItemList(int delay = 100) {
            PluginLog.Log("Loading Item List");
            triedLoadingItems = true;
            errorLoadingItems = false;
            plugin.LuminaItems = null;
            plugin.LuminaItemsClientLanguage = pluginConfig.SelectedClientLanguage;
#if DEBUG
            var sw = new Stopwatch();
#endif
            Task.Run(async () => {

                await Task.Delay(delay);
#if DEBUG
                sw.Start();
#endif
                try {
                    return this.data.GetExcelSheet<Item>(pluginConfig.SelectedClientLanguage).Where(i => !string.IsNullOrEmpty(i.Name)).ToList();
                } catch (Exception ex) {
                    errorLoadingItems = true;
                    PluginLog.LogError("Failed loading Items");
                    PluginLog.LogError(ex.ToString());
                    return new List<Item>();
                }
            }).ContinueWith(t => {
#if DEBUG
                sw.Stop();
                PluginLog.Log($"Loaded Item List in: {sw.ElapsedMilliseconds}ms");
#endif
                if (errorLoadingItems) {
                    return plugin.LuminaItems;
                }

                forceReload = true;
                return plugin.LuminaItems = t.Result;
            });
        }

        public Vector4 HSVtoRGB(Vector4 hsv) {
            
            ImGui.ColorConvertHSVtoRGB(hsv.X, hsv.Y, hsv.Z, out var r, out var g, out var b);
            return new Vector4(r, g, b, hsv.W);
        }
        
        public bool Draw() {
            var isOpen = true;

            try {
                var isSearch = false;
                if (triedLoadingItems == false || pluginConfig.SelectedClientLanguage != plugin.LuminaItemsClientLanguage) UpdateItemList(1000);

                if ((selectedItemIndex < 0 && selectedItem != null) || (selectedItemIndex >= 0 && selectedItem == null)) {
                    // Should never happen, but just incase
                    selectedItemIndex = -1;
                    selectedItem = null;
                    return true;
                }

                ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
                
                PushStyle(ImGuiStyleVar.WindowMinSize, new Vector2(350, 400));

                if (!ImGui.Begin(Loc.Localize("ItemSearchPlguinMainWindowHeader", $"Item Search") + "###itemSearchPluginMainWindow", ref isOpen, ImGuiWindowFlags.NoCollapse)) {
                    ResetStyle();
                    ImGui.End();
                    return false;
                }

                if (ImGui.IsWindowAppearing()) {
                    SearchFilters.ForEach(f => f._ForceVisible = false);
                }

                PopStyle();

                // Main window
                ImGui.AlignTextToFramePadding();

                if (selectedItem != null) {
                    var icon = selectedItem.Icon;

                    plugin.DrawIcon(icon, new Vector2(45 * ImGui.GetIO().FontGlobalScale));


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
                    ImGui.BeginChild("NoSelectedItemBox", new Vector2(-1, 45) * ImGui.GetIO().FontGlobalScale);
                    ImGui.Text(Loc.Localize("ItemSearchSelectItem", "Please select an item."));


                    if (!pluginConfig.HideKofi) {
                        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF5E5BFF);
                        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xFF5E5BAA);
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xFF5E5BDD);
                        ImGui.SameLine(ImGui.GetWindowWidth() - ImGui.CalcTextSize("Support on Ko-fi").X - ImGui.GetStyle().FramePadding.X * 3);
                        if (ImGui.Button("Support on Ko-Fi")) {
                            Process.Start("https://ko-fi.com/Caraxi");
                        }
                        ImGui.PopStyleColor(3);
                    }
                    
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
                var filterUsingTagColour = new Vector4(0.4f, 0.7f, 1, 1);
                foreach (var filter in SearchFilters.Where(x => x.IsEnabled && x.ShowFilter)) {
                    if (!extraFiltersExpanded && filter.CanBeDisabled && !filter.IsSet && !filter._ForceVisible) continue;
                    ImGui.SetCursorPosX((filterNameMax + ImGui.GetStyle().ItemSpacing.X) - filter._LocalizedNameWidth);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3);
                    if (filter.IsSet) {
                        ImGui.TextColored(filter.IsFromTag ? filterUsingTagColour : filterInUseColour, $"{filter._LocalizedName}: ");
                    } else {
                        ImGui.Text($"{filter._LocalizedName}: ");
                    }

                    ImGui.NextColumn();
                    ImGui.BeginGroup();
                    if (filter.IsFromTag && filter.GreyWithTags) ImGui.PushStyleColor(ImGuiCol.Text, 0xFF888888);
                    filter.DrawEditor();
                    if (filter.IsFromTag && filter.GreyWithTags) ImGui.PopStyleColor();
                    ImGui.EndGroup();
                    while (ImGui.GetColumnIndex() != 0)
                        ImGui.NextColumn();
                }
                
                ImGui.Columns(1);

                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, -5 * ImGui.GetIO().FontGlobalScale));
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                if (ImGui.Button($"{(extraFiltersExpanded ? (char) FontAwesomeIcon.CaretUp : (char) FontAwesomeIcon.CaretDown)}", new Vector2(-1, 10 * ImGui.GetIO().FontGlobalScale))) {
                    extraFiltersExpanded = !extraFiltersExpanded;
                    SearchFilters.ForEach(f => f._ForceVisible = f.IsEnabled && f.ShowFilter && f.IsSet);
                    pluginConfig.ExpandedFilters = extraFiltersExpanded;
                    pluginConfig.Save();
                }
                ImGui.PopStyleVar(2);
                ImGui.PopFont();

                var windowSize = ImGui.GetWindowSize();
                var childSize = new Vector2(0, Math.Max(100 * ImGui.GetIO().FontGlobalScale, windowSize.Y - ImGui.GetCursorPosY() - 45 * ImGui.GetIO().FontGlobalScale));
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
                                            plugin.FittingRoomUI.TryOnItem(selectedItem, selectedStain?.RowId ?? 0);
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
                                        plugin.FittingRoomUI.TryOnItem(selectedItem, selectedStain?.RowId ?? 0);
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
                    if (ImGui.Checkbox(Loc.Localize("ItemSearchTryOnButton", "Try On"), ref autoTryOn)) {
                        pluginConfig.TryOnEnabled = autoTryOn;
                        pluginConfig.Save();
                    }

                    ImGui.SameLine();


                    ImGui.PushStyleColor(ImGuiCol.Border, selectedStain != null && selectedStain.Unknown4 ? new Vector4(1, 1, 0, 1) : new Vector4(1, 1, 1, 1));
                    PushStyle(ImGuiStyleVar.FrameBorderSize, 2f);
                    if (ImGui.ColorButton("X", selectedStainColor, ImGuiColorEditFlags.NoTooltip)) {
                        showStainSelector = true;
                    }

                    if (ImGui.IsItemClicked(1)) {
                        selectedStainColor = Vector4.Zero;
                        selectedStain = null;
                        pluginConfig.SelectedStain = 0;
                        pluginConfig.Save();
                    }

                    PopStyle();

                    ImGui.PopStyleColor();

                    if (ImGui.IsItemHovered()) {
                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                        ImGui.SetTooltip(selectedStain == null ? "No Dye Selected" : selectedStain.Name);
                    }
                }
                ImGui.PushFont(UiBuilder.IconFont);
                var configText = $"{(char)FontAwesomeIcon.Cog}";
                var configTextSize = ImGui.CalcTextSize(configText);
                ImGui.PopFont();
                var itemCountText = isSearch ? string.Format(Loc.Localize("ItemCount", "{0} Items"), this.searchTask.Result.Count) : $"v{plugin.Version}";
                ImGui.SameLine(ImGui.GetWindowWidth() - (configTextSize.X + ImGui.GetStyle().ItemSpacing.X) - (ImGui.CalcTextSize(itemCountText).X + ImGui.GetStyle().ItemSpacing.X * (isSearch ? 3 : 2)));
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

                ImGui.SameLine(ImGui.GetWindowWidth() - (configTextSize.X + ImGui.GetStyle().ItemSpacing.X * 2));
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(configText)) {
                    plugin.ToggleConfigWindow();
                }
                ImGui.PopFont();

                var mainWindowPos = ImGui.GetWindowPos();
                var mainWindowSize = ImGui.GetWindowSize();

                ImGui.End();


                if (showStainSelector) {
                    ImGui.SetNextWindowSize(new Vector2(210, 180));
                    ImGui.SetNextWindowPos(mainWindowPos + mainWindowSize - new Vector2(0, 180));
                    ImGui.Begin("Select Dye", ref showStainSelector, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
                    
                    ImGui.BeginTabBar("stainShadeTabs");

                    var unselectedModifier = new Vector4(0, 0, 0, 0.7f);

                    foreach (var shade in stainShadeHeaders) {
                        ImGui.PushStyleColor(ImGuiCol.TabActive, shade.Value);
                        ImGui.PushStyleColor(ImGuiCol.TabHovered, shade.Value);
                        ImGui.PushStyleColor(ImGuiCol.TabUnfocused, shade.Value);
                        ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, shade.Value);
                        ImGui.PushStyleColor(ImGuiCol.Tab, shade.Value - unselectedModifier);
                        
                        if (ImGui.BeginTabItem($"    ###StainShade{shade.Key}")) {
                            var c = 0;

                            PushStyle(ImGuiStyleVar.FrameBorderSize, 2f);
                            foreach (var stain in stains.Where(s => s.Shade == shade.Key && !string.IsNullOrEmpty(s.Name))) {
                                var b = stain.Color & 255;
                                var g = (stain.Color >> 8) & 255;
                                var r = (stain.Color >> 16) & 255;

                                var stainColor = new Vector4(r / 255f, g / 255f, b / 255f, 1f);

                                ImGui.PushStyleColor(ImGuiCol.Border, stain.Unknown4 ? new Vector4(1, 1, 0, 1) : new Vector4(1, 1, 1, 1));

                                if (ImGui.ColorButton($"###stain{stain.RowId}", stainColor, ImGuiColorEditFlags.NoTooltip)) {
                                    selectedStain = stain;
                                    selectedStainColor = stainColor;
                                    showStainSelector = false;
                                    pluginConfig.SelectedStain = stain.RowId;
                                    pluginConfig.Save();
                                }

                                ImGui.PopStyleColor(1);

                                if (ImGui.IsItemHovered()) {
                                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                                    ImGui.SetTooltip(stain.Name);
                                }
  
                                if (c++ < 5) {
                                    ImGui.SameLine();
                                } else {
                                    c = 0;
                                }
                            }

                            PopStyle(1);
                            
                            ImGui.EndTabItem();
                        }

                        ImGui.PopStyleColor(5);
                    }

                    ImGui.EndTabBar();
                    ImGui.End();
                }


                return isOpen;
            } catch (Exception ex) {
                ResetStyle();
                PluginLog.LogError(ex.ToString());
                selectedItem = null;
                selectedItemIndex = -1;
                return isOpen;
            }
        }

        private void FixStainsOrder() {
            var move = stains.GetRange(92, 3);
            stains.RemoveRange(92, 3);
            stains.AddRange(move);
        }

        public void Dispose() {
            foreach (var f in SearchFilters) {
                f?.Dispose();
            }

            foreach (var b in ActionButtons) {
                b?.Dispose();
            }

        }
    }
}
