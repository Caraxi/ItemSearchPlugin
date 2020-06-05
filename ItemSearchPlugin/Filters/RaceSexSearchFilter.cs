using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheapLoc;
using Dalamud.Data;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using static ItemSearchPlugin.ExcelExtensions;
using Item = Dalamud.Data.TransientSheet.Item;

namespace ItemSearchPlugin.Filters {
	class RaceSexSearchFilter : ISearchFilter
	{
		private readonly ItemSearchPluginConfig pluginConfig;
		private readonly DataManager data;
		private int selectedIndex = 0;
		private int lastIndex = 0;

		private List<(string text, int raceId, CharacterSex sex)> options;

		private List<EquipRaceCategory> equipRaceCategories;

		public RaceSexSearchFilter(ItemSearchPluginConfig pluginConfig, DataManager data)
		{
			this.pluginConfig = pluginConfig;
			this.data = data;

			while (!data.IsDataReady) Thread.Sleep(1);

			equipRaceCategories = data.GetExcelSheet<EquipRaceCategory>().GetRows();

			options = new List<(string text, int raceId, CharacterSex sex)> {
				("Not Selected", 0, CharacterSex.Female)
			};

			foreach (Race race in data.GetExcelSheet<Race>().GetRows())
			{
				if (race.RSEMBody > 0 && race.RSEFBody > 0)
				{
					string male = string.Format(Loc.Localize("RaceSexMale", "Male {0}"), race.Masculine);
					string female = string.Format(Loc.Localize("RaceSexFemale", "Female {0}"), race.Feminine);
					options.Add((male, race.RowId, CharacterSex.Male));
					options.Add((female, race.RowId, CharacterSex.Female));
				} else if (race.RSEMBody > 0)
				{
					options.Add((race.Masculine, race.RowId, CharacterSex.Male));
				} else if (race.RSEFBody > 0)
				{
					options.Add((race.Feminine, race.RowId, CharacterSex.Female));
				}
			}

		}


		public string Name => "Sex / Race";

		public string NameLocalizationKey => "RaceSexSearchFilter";

		public bool ShowFilter => pluginConfig.ExtraFilters;

		public bool IsSet => selectedIndex > 0;

		public bool HasChanged {
			get {
				if (lastIndex != selectedIndex) {
					lastIndex = selectedIndex;
					return true;
				}
				return false;
			}
		}

		public bool CheckFilter(Item item)
		{
			try
			{
				var selectedOption = options[selectedIndex];
				EquipRaceCategory erc = equipRaceCategories[item.EquipRestriction];

				return erc.AllowsRaceSex(selectedOption.raceId, selectedOption.sex);
			} catch {
				return false;
			}
		}

		public void Dispose() {
			
		}

		public void DrawEditor() {
			
			ImGui.PushItemWidth(-1);
			ImGui.Combo("##RaceSexSearchFilter", ref this.selectedIndex, options.Select(a => a.text).ToArray(), options.Count);
			ImGui.PopItemWidth();


		}
	}
}
