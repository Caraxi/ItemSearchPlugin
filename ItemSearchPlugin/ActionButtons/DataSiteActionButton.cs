using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.ActionButtons {
    class DataSiteActionButton : IActionButton {
        private readonly ItemSearchPluginConfig pluginConfig;

        public DataSiteActionButton(ItemSearchPluginConfig pluginConfig) {
            this.pluginConfig = pluginConfig;
        }

        public override ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public override void Dispose() { }

        public override string GetButtonText(Item selectedItem) {
            return string.Format(
                Loc.Localize("ItemSearchDataSiteViewButton", "View on {0}"),
                Loc.Localize(pluginConfig.SelectedDataSite.NameTranslationKey, pluginConfig.SelectedDataSite.Name)
            );
        }

        public override bool GetShowButton(Item selectedItem) {
            return pluginConfig.SelectedDataSite != null;
        }

        public override void OnButtonClicked(Item selectedItem) {
            pluginConfig.SelectedDataSite.OpenItem(selectedItem);
        }
    }
}
