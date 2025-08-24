using System;
using Lumina.Excel.Sheets;
using System.Collections.Concurrent;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace ItemSearchPlugin {
    public class CraftingRecipeFinder : IDisposable {
        private readonly ConcurrentQueue<uint> searchQueue = new();

        private bool disposed;

        private unsafe void OnFrameworkUpdate(IFramework framework) {
            try {
                if (disposed) return;
                if (ClientState.LocalContentId == 0) return;
                if (!searchQueue.TryDequeue(out var itemID)) {
                    Framework.Update -= OnFrameworkUpdate;
                    return;
                }

                AgentRecipeNote.Instance()->SearchRecipeByItemId(itemID);
            } catch (NullReferenceException) { }
        }

        public void SearchRecipesByItem(Item item) {
            if (disposed) return;

            searchQueue.Enqueue(item.RowId);
            Framework.Update -= OnFrameworkUpdate;
            Framework.Update += OnFrameworkUpdate;
        }

        public void Dispose() {
            disposed = true;
            Framework.Update -= OnFrameworkUpdate;
        }
    }
}
