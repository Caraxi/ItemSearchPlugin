using Dalamud.Data.TransientSheet;
using ImGuiNET;

namespace ItemSearchPlugin.Filters {
	class LevelEquipSearchFilter : ISearchFilter {

		private static readonly int MIN_LEVEL = 1;
		private static readonly int MAX_LEVEL = 80;


		private int minLevel;
		private int maxLevel;

		private int last_minLevel;
		private int last_maxLevel;

		private ItemSearchPluginConfig config;

		public LevelEquipSearchFilter(ItemSearchPluginConfig config) {
			minLevel = last_minLevel = MIN_LEVEL;
			maxLevel = last_maxLevel = MAX_LEVEL;

			this.config = config;
		}


		public string Name => "Equip Level";

		public string NameLocalizationKey => "SearchFilterLevelEquip";

		public bool ShowFilter => config.ExtraFilters;

		public bool IsSet => minLevel != MIN_LEVEL || maxLevel != MAX_LEVEL;

		public bool HasChanged {
			get {
				if (minLevel != last_minLevel || maxLevel != last_maxLevel){
					last_maxLevel = maxLevel;
					last_minLevel = minLevel;
					return true;
				}
				return false;
			}
		}

		public bool CheckFilter(Item item) {
			return item.LevelEquip >= minLevel && item.LevelEquip <= maxLevel;
		}

		public void Dispose() {
			
		}

		public void DrawEditor() {
			ImGui.PushItemWidth(-1);
			if (ImGui.DragIntRange2("##LevelEquipSearchFilterRange", ref minLevel, ref maxLevel, 1f, MIN_LEVEL, MAX_LEVEL)){
				// Force ImGui to behave
				// https://cdn.discordapp.com/attachments/653504487352303619/713825323967447120/ehS7GdAHKG.gif
				if (minLevel > maxLevel && minLevel != last_minLevel) minLevel = maxLevel;
				if (maxLevel < minLevel && maxLevel != last_maxLevel) maxLevel = minLevel;
				if (minLevel < MIN_LEVEL) minLevel = MIN_LEVEL;
				if (maxLevel > MAX_LEVEL) maxLevel = MAX_LEVEL;
			}
			ImGui.PopItemWidth();
		}
	}
}
