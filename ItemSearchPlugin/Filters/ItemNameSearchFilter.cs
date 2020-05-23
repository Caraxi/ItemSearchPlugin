using Dalamud.Data.TransientSheet;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearchPlugin.Filters {
	class ItemNameSearchFilter : ISearchFilter {

		private string searchText;
		private string lastSearchText;

		public ItemNameSearchFilter(string startingValue = "") {
			searchText = startingValue;
			lastSearchText = string.Empty;
		}


		public string Name => "Search";

		public string NameLocalizationKey => "DalamudItemSearchVerb";

		public bool ShowFilter => true;

		public bool IsSet => !string.IsNullOrEmpty(searchText);

		public bool HasChanged {
			get {
				if (searchText != lastSearchText){
					lastSearchText = searchText;
					return true;
				}
				return false;
			}
		} 

		public bool CheckFilter(Item item) {
			return item.Name.ToLower().Contains(searchText.ToLower()) || int.TryParse(searchText, out var parsedId) && parsedId == item.RowId && item.Icon < 65000;
		}

		public void Dispose() {
			
		}

		public void DrawEditor() {
			ImGui.PushItemWidth(-1);
            ImGui.InputText("##ItemNameSearchFilter", ref searchText, 32);
            ImGui.PopItemWidth();
		}
	}
}
