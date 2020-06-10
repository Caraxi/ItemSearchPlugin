using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CheapLoc;
using Dalamud.Data;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using static ItemSearchPlugin.ExcelExtensions;

namespace ItemSearchPlugin.Filters {
    internal class RaceSexSearchFilter : ISearchFilter {
        private readonly ItemSearchPluginConfig pluginConfig;
        private int selectedIndex = 0;
        private int lastIndex = 0;
        private bool broken; // Only fail once...
        private readonly List<(string text, uint raceId, CharacterSex sex)> options;
        private readonly List<EquipRaceCategory> equipRaceCategories;

        public RaceSexSearchFilter(ItemSearchPluginConfig pluginConfig, DataManager data) {
            this.pluginConfig = pluginConfig;

            while (!data.IsDataReady) Thread.Sleep(1);

            equipRaceCategories = data.GetExcelSheet<EquipRaceCategory>().GetRows();

            options = new List<(string text, uint raceId, CharacterSex sex)> {
                (Loc.Localize("NotSelected", "Not Selected"), 0, CharacterSex.Female)
            };

            foreach (var race in data.GetExcelSheet<Race>().GetRows()) {
                if (race.RSEMBody.Row > 0 && race.RSEFBody.Row > 0) {
                    string male = string.Format(Loc.Localize("RaceSexMale", "Male {0}"), race.Masculine);
                    string female = string.Format(Loc.Localize("RaceSexFemale", "Female {0}"), race.Feminine);
                    options.Add((male, race.RowId, CharacterSex.Male));
                    options.Add((female, race.RowId, CharacterSex.Female));
                } else if (race.RSEMBody.Row > 0) {
                    options.Add((race.Masculine, race.RowId, CharacterSex.Male));
                } else if (race.RSEFBody.Row > 0) {
                    options.Add((race.Feminine, race.RowId, CharacterSex.Female));
                }
            }
        }

        public string Name => "Sex / Race";

        public string NameLocalizationKey => "RaceSexSearchFilter";

        public bool ShowFilter => !broken && pluginConfig.ExtraFilters;

        public bool IsSet => !broken && selectedIndex > 0;

        public bool HasChanged {
            get {
                if (lastIndex == selectedIndex) return false;
                lastIndex = selectedIndex;
                return true;
            }
        }

        public bool CheckFilter(Item item) {
            if (broken) return true;
            try {
                var (text, raceId, sex) = options[selectedIndex];
                var erc = equipRaceCategories[item.EquipRestriction];
                return erc.AllowsRaceSex(raceId, sex);
            } catch (Exception ex) {
                PluginLog.LogError(ex.ToString());
                broken = true;
                return true;
            }
        }

        public void Dispose() { }

        public void DrawEditor() {
            ImGui.PushItemWidth(-1);
            ImGui.Combo("##RaceSexSearchFilter", ref this.selectedIndex, options.Select(a => a.text).ToArray(), options.Count);
            ImGui.PopItemWidth();
        }
    }
}
