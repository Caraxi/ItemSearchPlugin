using Dalamud.Data.TransientSheet;

namespace ItemSearchPlugin.DataSites {
    public class GamerEscapeDatasite : DataSite {
        public override string Name => "Gamer Escape";

        public override string NameTranslationKey => "GamerEscapeDataSite";

        public override string Note => "Some items may link to incorrect pages due to using names.";

        public override string GetItemUrl(Item item) => $"https://ffxiv.gamerescape.com/wiki/{item.Name.Replace(' ', '_')}";
    }
}
