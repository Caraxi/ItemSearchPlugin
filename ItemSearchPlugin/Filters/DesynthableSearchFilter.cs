using System.Threading.Tasks;
using Dalamud.Data;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.Filters {
    internal class DesynthableSearchFilter : SearchFilter {
        private int selectedOption = 0;

        private readonly string[] options;

        private bool finishedLoading = false;

        public DesynthableSearchFilter(ItemSearchPluginConfig pluginConfig, DataManager data) : base(pluginConfig) {

            string craftableJobFormat = Loc.Localize("DesynthableJobFormat", "Desynthable: {0}");

            options = new string[11];

            options[0] = "";
            options[1] = Loc.Localize("NotDesynthable", "Not Desynthable");
            options[2] = string.Format(craftableJobFormat, Loc.Localize("SearchFilterAny", "Any"));
            
            Task.Run(() => {
                var cj = data.GetExcelSheet<ClassJob>();
                
                for (uint i = 0; i < 8; i++) {
                    var job = cj.GetRow(i + 8);
                    options[3 + i] = string.Format(craftableJobFormat, job.Abbreviation);
                }

                finishedLoading = true;
                Modified = true;
            });
        }

        public override string Name { get; } = "Desynthable";
        public override string NameLocalizationKey { get; } = "DesynthableSearchFilter";
        public override bool IsSet => selectedOption > 0;

        public override bool ShowFilter => base.ShowFilter && finishedLoading;

        public override bool CheckFilter(Item item) {
            if (item == null) return false;
            if (!finishedLoading) return true;

            var isDesynthable = item.Unknown36 > 0;
            return selectedOption switch {
                1 => !isDesynthable,
                2 => isDesynthable,
                3 => isDesynthable && item.ClassJobRepair.Row == 8,
                4 => isDesynthable && item.ClassJobRepair.Row == 9,
                5 => isDesynthable && item.ClassJobRepair.Row == 10,
                6 => isDesynthable && item.ClassJobRepair.Row == 11,
                7 => isDesynthable && item.ClassJobRepair.Row == 12,
                8 => isDesynthable && item.ClassJobRepair.Row == 13,
                9 => isDesynthable && item.ClassJobRepair.Row == 14,
                10 => isDesynthable && item.ClassJobRepair.Row == 15,
                _ => true
            };
        }

        public override void DrawEditor() {
            if (ImGui.Combo("###desynthableSearchFilter_selection", ref selectedOption, options, options.Length, 14)) {
                Modified = true;
            }
        }

        public override string ToString() {
            return options[selectedOption].Replace("Desynthable: ", "");
        }
    }
}
