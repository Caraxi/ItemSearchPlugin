using System;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Concurrent;
using Dalamud.Game;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace ItemSearchPlugin {
    public class CraftingRecipeFinder : IDisposable {
        private readonly ConcurrentQueue<uint> searchQueue = new();

        private bool disposed;

        private unsafe void OnFrameworkUpdate(IFramework framework) {
            try {
                if (disposed) return;
                if (ItemSearchPlugin.ClientState.LocalContentId == 0) return;
                if (!searchQueue.TryDequeue(out var itemID)) {
                    ItemSearchPlugin.Framework.Update -= OnFrameworkUpdate;
                    return;
                }

                AgentRecipeNote.Instance()->OpenRecipeByItemId(itemID);
            } catch (NullReferenceException) { }
        }

        public void SearchRecipesByItem(Item item) {
            if (disposed) return;
            if (item == null) {
                PluginLog.Log("Tried to find recipe for NULL item.");
                return;
            }

            searchQueue.Enqueue(item.RowId);
            ItemSearchPlugin.Framework.Update -= OnFrameworkUpdate;
            ItemSearchPlugin.Framework.Update += OnFrameworkUpdate;
        }

        public void Dispose() {
            disposed = true;
            ItemSearchPlugin.Framework.Update -= OnFrameworkUpdate;
        }
    }
}
