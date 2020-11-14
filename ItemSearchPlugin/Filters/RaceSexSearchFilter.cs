using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dalamud.Data;
using Dalamud.Game.ClientState.Actors;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using static ItemSearchPlugin.ExcelExtensions;

namespace ItemSearchPlugin.Filters {
    internal class RaceSexSearchFilter : SearchFilter {
        private readonly DalamudPluginInterface pluginInterface;
        private int selectedIndex;
        private int lastIndex;
        private readonly List<(string text, uint raceId, CharacterSex sex)> options;
        private readonly List<EquipRaceCategory> equipRaceCategories;

        public RaceSexSearchFilter(ItemSearchPluginConfig pluginConfig, DataManager data, DalamudPluginInterface pluginInterface) : base(pluginConfig) {
            this.pluginInterface = pluginInterface;
            while (!data.IsDataReady) Thread.Sleep(1);

            equipRaceCategories = data.GetExcelSheet<EquipRaceCategory>().ToList();

            options = new List<(string text, uint raceId, CharacterSex sex)> {
                (Loc.Localize("NotSelected", "Not Selected"), 0, CharacterSex.Female)
            };

            foreach (var race in data.GetExcelSheet<Race>().ToList()) {
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

        public override string Name => "Sex / Race";

        public override string NameLocalizationKey => "RaceSexSearchFilter";

        public override bool IsSet => selectedIndex > 0;

        public override bool HasChanged {
            get {
                if (lastIndex == selectedIndex) return false;
                lastIndex = selectedIndex;
                return true;
            }
        }

        public override bool CheckFilter(Item item) {
            try {
                var (_, raceId, sex) = options[selectedIndex];
                var erc = equipRaceCategories[item.EquipRestriction];
                return erc.AllowsRaceSex(raceId, sex);
            } catch (Exception ex) {
                PluginLog.LogError(ex.ToString());
                return true;
            }
        }

        public override void DrawEditor() {
            if (pluginInterface.ClientState?.LocalPlayer != null) {
                ImGui.SetNextItemWidth(-80 * ImGui.GetIO().FontGlobalScale);
            } else {
                ImGui.SetNextItemWidth(-1);
            }
            
            ImGui.Combo("##RaceSexSearchFilter", ref this.selectedIndex, options.Select(a => a.text).ToArray(), options.Count);

            if (pluginInterface.ClientState?.LocalPlayer != null) {
                ImGui.SameLine();
                
                if (ImGui.SmallButton($"Current")) {
                    var race = pluginInterface.ClientState.LocalPlayer.Customize[(int)CustomizeIndex.Race];
                    var sex = pluginInterface.ClientState.LocalPlayer.Customize[(int)CustomizeIndex.Gender] == 0 ? CharacterSex.Male : CharacterSex.Female;

                    for (var i = 0; i < options.Count; i++) {
                        if (options[i].sex == sex && options[i].raceId == race) {
                            selectedIndex = i;
                            break;
                        } 
                    }
                }
            }

            
        }

        public override string ToString() {
            return options[selectedIndex].text;
        }
    }
}
