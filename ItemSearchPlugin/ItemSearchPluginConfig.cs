using Dalamud.Configuration;
using Dalamud.Plugin;
using ImGuiNET;
using Newtonsoft.Json;
using System;

namespace ItemSearchPlugin {
	public class ItemSearchPluginConfig : IPluginConfiguration {

		[NonSerialized]
		private DalamudPluginInterface pluginInterface;

		public int Version { get; set; }

		public string Language { get; set; }

		public bool CloseOnChoose { get; set; }

		public bool ShowItemID { get; set; }

		public bool ExtraFilters { get; set; }

		public ItemSearchPluginConfig() {
			LoadDefaults();
		}

		public void LoadDefaults() {
			CloseOnChoose = false;
			ShowItemID = false;
			ExtraFilters = false;
		}

		public void Init(DalamudPluginInterface pluginInterface) {
			this.pluginInterface = pluginInterface;
		}

		public void Save() {
			this.pluginInterface.SavePluginConfig(this);
		}


		public bool DrawConfigUI() {
			ImGuiWindowFlags windowFlags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse;
			bool drawConfig = true;
			ImGui.Begin("Item Search Config", ref drawConfig, windowFlags);

			bool closeOnChoose = CloseOnChoose;

			if (ImGui.Checkbox("Close window after linking item", ref closeOnChoose)){
				CloseOnChoose = closeOnChoose;
				Save();
			}

			bool showItemId = ShowItemID;
			if (ImGui.Checkbox("Show Item IDs", ref showItemId)){
				ShowItemID = showItemId;
				Save();
			}

			bool extraFilters = ExtraFilters;
			if (ImGui.Checkbox("Enable Extra Filters", ref extraFilters)){
				ExtraFilters = extraFilters;
				Save();
			}

			ImGui.End();
			return drawConfig;
		}

	}
}