using Dalamud.Configuration;
using Dalamud.Plugin;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Numerics;

namespace ItemSearchPlugin {
	public class ItemSearchPluginConfig : IPluginConfiguration {

		[NonSerialized]
		private DalamudPluginInterface pluginInterface;

		public int Version { get; set; }

		public string Language { get; set; }

		public bool CloseOnChoose { get; set; }

		public bool ShowItemID { get; set; }

		public bool ExtraFilters { get; set; }

		public int MaxItemLevel { get; set; }

		public string DataSite { get; set; }

		[NonSerialized]
		private DataSite lastDataSite = null;

		[JsonIgnore]
		public DataSite SelectedDataSite {
			get {
				if (lastDataSite == null || (lastDataSite.Name != this.DataSite)) {
					if (string.IsNullOrEmpty(this.DataSite)){
						return null;
					}
					lastDataSite = ItemSearchPlugin.DataSites.Where(ds => ds.Name == this.DataSite).FirstOrDefault();
				}
				return lastDataSite;
			}
		}

		public ItemSearchPluginConfig() {
			LoadDefaults();
		}

		public void LoadDefaults() {
			CloseOnChoose = false;
			ShowItemID = false;
			ExtraFilters = false;
			MaxItemLevel = 505;
			DataSite = ItemSearchPlugin.DataSites.FirstOrDefault()?.Name;
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

			int dataSiteIndex = Array.IndexOf(ItemSearchPlugin.DataSites, this.SelectedDataSite);
			if (ImGui.Combo("External Data Site", ref dataSiteIndex, ItemSearchPlugin.DataSites.Select(t => t.Name + (string.IsNullOrEmpty(t.Note) ? "":"*")).ToArray(), ItemSearchPlugin.DataSites.Length)) {
				this.DataSite = ItemSearchPlugin.DataSites[dataSiteIndex].Name;
				Save();
			}
			if (!string.IsNullOrEmpty(SelectedDataSite.Note)){
				ImGui.TextColored(new Vector4(1, 1, 1, 0.5f), $"*{SelectedDataSite.Note}");
			}

			ImGui.End();
			return drawConfig;
		}

	}
}