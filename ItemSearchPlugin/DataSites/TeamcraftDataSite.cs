using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.DataSites {
    public class TeamcraftDataSite : DataSite {
        public override string Name => "Teamcraft";

        public override string NameTranslationKey => "TeamcraftDataSite";

        public override string GetItemUrl(Item item) => $"https://ffxivteamcraft.com/db/en/item/{item.RowId}/{item.Name.ToString().Replace(' ', '-')}";

        private static bool teamcraftLocalFailed = false;
        private ItemSearchPluginConfig config;

        public TeamcraftDataSite(ItemSearchPluginConfig config) {
            this.config = config;
        }

        public override void OpenItem(Item item) {
            if (!(teamcraftLocalFailed || config.TeamcraftForceBrowser)) {
                Task.Run(() => {
                    try {
                        var wr = WebRequest.CreateHttp($"http://localhost:14500/db/en/item/{item.RowId}");
                        wr.Timeout = 500;
                        wr.Method = "GET";
                        wr.GetResponse().Close();
                    } catch {
                        try {
                            if (System.IO.Directory.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ffxiv-teamcraft"))) {
                                Process.Start($"teamcraft://db/en/item/{item.RowId}");
                            } else {
                                teamcraftLocalFailed = true;
                                Process.Start($"https://ffxivteamcraft.com/db/en/item/{item.RowId}");
                            }
                        } catch {
                            teamcraftLocalFailed = true;
                            Process.Start($"https://ffxivteamcraft.com/db/en/item/{item.RowId}");
                        }
                    }
                });
                return;
            }

            base.OpenItem(item);
        }
    }
}
