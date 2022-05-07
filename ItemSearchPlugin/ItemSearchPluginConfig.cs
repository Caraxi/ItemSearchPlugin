using Dalamud.Configuration;
using Dalamud.Plugin;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud;
using ItemSearchPlugin.ActionButtons;

namespace ItemSearchPlugin {

    public class ItemSearchPluginConfig : IPluginConfiguration {
        [NonSerialized] private DalamudPluginInterface pluginInterface;
        [NonSerialized] private ItemSearchPlugin plugin;
        [JsonIgnore] internal List<(string localizationKey, string englishName)> FilterNames { get; } = new List<(string localizationKey, string englishName)>();

        public int Version { get; set; }

        public string Language { get; set; }

        public bool CloseOnChoose { get; set; }

        public bool ShowItemID { get; set; }

        public uint MaxItemLevel { get; set; }

        public bool ShowTryOn { get; set; }
        
        public bool ShowPreviewHousing { get; set; }

        public string DataSite { get; set; }

        public SortedSet<uint> Favorites { get; set; } = new SortedSet<uint>();
        
        public bool MarketBoardPluginIntegration { get; set; }

        public bool ShowLegacyItems { get; set; }

        public byte SelectedLanguage { get; set; }

        public bool PrependFilterListWithCopy { get; set; }
        public List<string> DisabledFilters { get; set; }
        
        [NonSerialized] private DataSite lastDataSite;

        public uint SelectedStain { get; set; } = 0;

        public bool ExpandedFilters { get; set; } = false;

        [JsonIgnore]
        public DataSite SelectedDataSite {
            get {
                if (lastDataSite == null || (lastDataSite.Name != this.DataSite)) {
                    if (string.IsNullOrEmpty(this.DataSite)) {
                        return null;
                    }

                    lastDataSite = ItemSearchPlugin.DataSites.FirstOrDefault(ds => ds.Name == this.DataSite);
                }

                return lastDataSite;
            }
        }

        [JsonIgnore]
        public ClientLanguage SelectedClientLanguage {
            get {
                return SelectedLanguage switch {
                    0 => ItemSearchPlugin.ClientState.ClientLanguage,
                    1 => ClientLanguage.English,
                    2 => ClientLanguage.Japanese,
                    3 => ClientLanguage.French,
                    4 => ClientLanguage.German,
                    _ => ItemSearchPlugin.ClientState.ClientLanguage,
                };
            }
        }

        public bool HideKofi { get; set; } = false;
        public bool TryOnEnabled { get; set; } = false;
        public bool PreviewHousingEnabled { get; set; } = false;
        public bool AutoFocus { get; set; } = true;
        public bool SuppressTryOnMessage { get; set; } = true;
        public bool TeamcraftForceBrowser { get; set; } = false;

        public bool EnableFFXIVStore { get; set; } = false;

        public ItemSearchPluginConfig() {
            LoadDefaults();
        }

        public void LoadDefaults() {
            CloseOnChoose = false;
            ShowItemID = false;
            MarketBoardPluginIntegration = false;
            MaxItemLevel = 505;
            ShowTryOn = false;
            ShowPreviewHousing = false;
            SuppressTryOnMessage = true;
            ShowLegacyItems = false;
            DataSite = ItemSearchPlugin.DataSites.FirstOrDefault()?.Name;
            SelectedLanguage = 0;
            DisabledFilters = new List<string>();
            PrependFilterListWithCopy = false;
            AutoFocus = true;
            HideKofi = false;
            TeamcraftForceBrowser = false;
        }


        public void Init(DalamudPluginInterface pluginInterface, ItemSearchPlugin plugin) {
            this.pluginInterface = pluginInterface;
            this.plugin = plugin;
        }

        public void Save() {
            this.pluginInterface.SavePluginConfig(this);
        }


        public bool DrawConfigUI() {
            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse;
            bool drawConfig = true;
            ImGui.Begin("Item Search Config", ref drawConfig, windowFlags);

            int selectedLanguage = SelectedLanguage;
            if (ImGui.BeginCombo(Loc.Localize("ItemSearchConfigItemLanguage", "Item Language") + "###ItemSearchConfigLanguageSelect", SelectedLanguage == 0 ? Loc.Localize("LanguageAutomatic", "Automatic") : SelectedClientLanguage.ToString())) {
                if (ImGui.Selectable(Loc.Localize("LanguageAutomatic", "Automatic"), selectedLanguage == 0)) SelectedLanguage = 0;
                if (ImGui.Selectable("English##itemLanguageOption", selectedLanguage == 1)) SelectedLanguage = 1;
                if (ImGui.Selectable("日本語##itemLanguageOption", selectedLanguage == 2)) SelectedLanguage = 2;
                if (ImGui.Selectable("Français##itemLanguageOption", selectedLanguage == 3)) SelectedLanguage = 3;
                if (ImGui.Selectable("Deutsch##itemLanguageOption", selectedLanguage == 4)) SelectedLanguage = 4;
                if (SelectedLanguage != selectedLanguage) {
                    Save();
                }

                ImGui.EndCombo();
            }

            /*
            string uiLanguage = Language;
            string selectedLanguageString = Loc.Localize("LanguageDefault", "Default");
            if (!string.IsNullOrEmpty(uiLanguage)) {
                selectedLanguageString = uiLanguage switch {
                    "en" => "English",
                    "jp" => "日本語",
                    "de" => "Deutsch",
                    "fr" => "Français",
                    _ => "Unknown Language"
                };
            }

            if (ImGui.BeginCombo(Loc.Localize("ItemSearchConfigUILanguage", "UI Language") + "###ItemSearchConfigUiLanguageSelect", selectedLanguageString)) {
                if (ImGui.Selectable(Loc.Localize("LanguageDefault", "Default"), string.IsNullOrEmpty(uiLanguage))) Language = null;
                if (ImGui.Selectable("English##uiLanguageOption", uiLanguage == "en")) Language = "en";
                if (ImGui.Selectable("日本語##uiLanguageOption", uiLanguage == "jp")) Language = "jp";
                if (ImGui.Selectable("Français##uiLanguageOption", uiLanguage == "fr")) Language = "fr";
                if (ImGui.Selectable("Deutsch##uiLanguageOption", uiLanguage == "de")) Language = "de";
                if (Language != uiLanguage) {
                    Save();
                    plugin.ReloadLocalization();
                }

                ImGui.EndCombo();
            }
            */
            bool closeOnChoose = CloseOnChoose;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigCloseAfterLink", "Close window after linking item"), ref closeOnChoose)) {
                CloseOnChoose = closeOnChoose;
                Save();
            }

            bool autoFocus = AutoFocus;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigAutoFocus", "Auto focus search box"), ref autoFocus)) {
                AutoFocus = autoFocus;
                Save();
            }

            bool showItemId = ShowItemID;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigShowItemId", "Show Item IDs"), ref showItemId)) {
                ShowItemID = showItemId;
                Save();
            }

            bool mbpIntegration = MarketBoardPluginIntegration;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigEnableMarketBoard", "Market Board Plugin Integration"), ref mbpIntegration)) {
                MarketBoardPluginIntegration = mbpIntegration;
                Save();
            }

            bool showTryOn = ShowTryOn;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigEnableTryOn", "Enable Try On Feature"), ref showTryOn)) {
                ShowTryOn = showTryOn;
                Save();
            }

            bool suppressTryOnMessage = SuppressTryOnMessage;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigSuppressTryOnMessage", "Surppress Try On Message"), ref suppressTryOnMessage)) {
                SuppressTryOnMessage = suppressTryOnMessage;
                Save();
            }

            bool previewHousing = ShowPreviewHousing;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigPreviewHousing", "Enable Housing Fixture Preview Feature"), ref previewHousing)) {
                ShowPreviewHousing = previewHousing;
                Save();
            }

            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip(Loc.Localize("ItemSearchPreviewHousingNote", "Note: To preview hosuing items you must have the 'Remodel Interior' window open."));
            }
            
            var prependFilterListWithCopy = PrependFilterListWithCopy;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigPrependFilterListWithCopy", "Add filters when copying results to clipboard"), ref prependFilterListWithCopy)) {
                PrependFilterListWithCopy = prependFilterListWithCopy;
                Save();
            }

            bool showLegacyItems = ShowLegacyItems;
            if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigShowLegacyItems", "Show Legacy Items"), ref showLegacyItems)) {
                ShowLegacyItems = showLegacyItems;
                Save();
            }

            bool hideKofi = HideKofi;
            if (ImGui.Checkbox(Loc.Localize("HideKofi", "Don't show Ko-fi link"), ref hideKofi)) {
                HideKofi = hideKofi;
                Save();
            }

            int dataSiteIndex = Array.IndexOf(ItemSearchPlugin.DataSites, this.SelectedDataSite);
            if (ImGui.Combo(Loc.Localize("ItemSearchConfigExternalDataSite", "External Data Site"), ref dataSiteIndex, ItemSearchPlugin.DataSites.Select(t => Loc.Localize(t.NameTranslationKey, t.Name) + (string.IsNullOrEmpty(t.Note) ? "" : "*")).ToArray(), ItemSearchPlugin.DataSites.Length)) {
                this.DataSite = ItemSearchPlugin.DataSites[dataSiteIndex].Name;
                Save();
            }

            if (this.DataSite == "Teamcraft") {
                var teamcraftBroswer = TeamcraftForceBrowser;
                if (ImGui.Checkbox(Loc.Localize("ItemSearchTeamcraftForceBrowser", "Only use browser for Teamcraft"), ref teamcraftBroswer)) {
                    TeamcraftForceBrowser = teamcraftBroswer;
                    Save();
                }
            }

            if (!string.IsNullOrEmpty(SelectedDataSite.Note)) {
                ImGui.TextColored(new Vector4(1, 1, 1, 0.5f), $"*{SelectedDataSite.Note}");
            }

            var storeEnabled = EnableFFXIVStore;
            if (ImGui.Checkbox("FFXIV Store", ref storeEnabled)) {
                EnableFFXIVStore = storeEnabled;
                FfxivStoreActionButton.BeginUpdate();
                Save();
            }

            if (!storeEnabled && ImGui.IsItemHovered()) {
                ImGui.BeginTooltip();
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Warning: FFXIV Store");
                ImGui.Separator();
                ImGui.TextWrapped("Enabling FFXIV Store will cause Item Search Plugin to contact the FFXIV Store website to determine which items are available.");
                ImGui.Text("If this concerns you, you probably shouldn't enable it.");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            ImGui.TextDisabled($"{FfxivStoreActionButton.UpdateStatus}");

            ImGui.Text("Show Filters: ");

            ImGui.BeginChild("###scrollingFilterSelection", new Vector2(0, 180), true);

            ImGui.Columns(2, "###itemSearchToggleFilters", false);
            foreach (var (localizationKey, englishName) in FilterNames) {
                var enabled = !DisabledFilters.Contains(localizationKey);
                if (ImGui.Checkbox(Loc.Localize(localizationKey, englishName) + "##checkboxToggleFilterEnabled", ref enabled)) {
                    if (enabled) {
                        DisabledFilters.RemoveAll(a => a == localizationKey);
                        plugin.itemSearchWindow.SearchFilters.FirstOrDefault(f => f.NameLocalizationKey == localizationKey)?.Show();
                    } else {
                        DisabledFilters.Add(localizationKey);
                        plugin.itemSearchWindow.SearchFilters.FirstOrDefault(f => f.NameLocalizationKey == localizationKey)?.Hide();
                    }

                    Save();
                }

                ImGui.NextColumn();
            }

            ImGui.Columns(1);
            ImGui.EndChild();

            ImGui.End();
            return drawConfig;
        }
    }
}
