using Dalamud.Configuration;
using Dalamud.Plugin;
using ImGuiNET;
using Newtonsoft.Json;
using System;

namespace ItemSearch {
	public class ItemSearchPluginConfig : IPluginConfiguration {

		[NonSerialized]
		private DalamudPluginInterface pluginInterface;

		public int Version { get; set; }

		public string Language { get; set; }

		public bool CloseOnChoose { get; set; }

		public ItemSearchPluginConfig() {
			LoadDefaults();
		}

		public void LoadDefaults() {
			CloseOnChoose = false;
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

			ImGui.End();
			return drawConfig;
		}

	}
}