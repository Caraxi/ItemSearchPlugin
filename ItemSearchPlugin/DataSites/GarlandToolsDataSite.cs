using Dalamud.Game;
using Lumina.Excel.Sheets;
using Dalamud.Plugin.Services;

namespace ItemSearchPlugin.DataSites {
    public class GarlandToolsDataSite(IClientState clientState) : DataSite {
        public override string Name => "Garland Tools";

        public override string NameTranslationKey => "GarlandToolsDataSite";

        private bool Language => clientState.ClientLanguage == ClientLanguage.ChineseSimplified;
        private string Suffix => Language ? "cn" : "org";

        public override string GetItemUrl(Item item) => $"https://www.garlandtools.{Suffix}/db/#item/{item.RowId}";
    }
}
