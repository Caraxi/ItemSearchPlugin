using CheapLoc;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Plugin;
using System;

namespace ItemSearchPlugin.ActionButtons {
    class DataSiteActionButton : IActionButton {
        private readonly ItemSearchPluginConfig pluginConfig;

        public DataSiteActionButton(ItemSearchPluginConfig pluginConfig) {
            this.pluginConfig = pluginConfig;
        }

        public ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public void Dispose() { }

        public string GetButtonText(Item selectedItem) {
            return string.Format(
                Loc.Localize("ItemSearchDataSiteViewButton", "View on {0}"),
                Loc.Localize(pluginConfig.SelectedDataSite.NameTranslationKey, pluginConfig.SelectedDataSite.Name)
            );
        }

        public bool GetShowButton(Item selectedItem) {
            return this.pluginConfig.SelectedDataSite != null;
        }

        public void OnButtonClicked(Item selectedItem) {
            pluginConfig.SelectedDataSite.OpenItem(selectedItem);
        }
    }
}
