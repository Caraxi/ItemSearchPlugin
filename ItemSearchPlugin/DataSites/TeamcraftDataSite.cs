using Dalamud.Data.TransientSheet;

namespace ItemSearchPlugin.DataSites {
    public class TeamcraftDataSite : DataSite {
        public override string Name => "Teamcraft";

        public override string NameTranslationKey => "TeamcraftDataSite";

        public override string GetItemUrl(Item item) => $"https://ffxivteamcraft.com/db/en/item/{item.RowId}/{item.Name.Replace(' ', '-')}";
    }
}
