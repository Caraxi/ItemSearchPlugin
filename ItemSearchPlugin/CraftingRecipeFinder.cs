using System;
using System.Runtime.InteropServices;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using System.Collections.Concurrent;

namespace ItemSearchPlugin {
    public class CraftingRecipeFinder : IDisposable {
        private readonly ItemSearchPlugin plugin;

        private readonly AddressResolver Address;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetUIObjectDelegate();
        private readonly GetUIObjectDelegate getUIObject;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetUIAgentModuleDelegate(IntPtr UIObject);
        private GetUIAgentModuleDelegate getUIAgentModule;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetAgentObjectDelegate(IntPtr AgentModule, uint agentID);
        private readonly GetAgentObjectDelegate getAgentObject;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr SearchItemByCraftingMethodDelegate(IntPtr RecipeAgentObject, ushort itemID);
        private readonly SearchItemByCraftingMethodDelegate searchItemByCraftingMethod;

        private readonly ConcurrentQueue<uint> searchQueue = new ConcurrentQueue<uint>();

        private bool disposed;
        
        public CraftingRecipeFinder(ItemSearchPlugin plugin) {
            this.plugin = plugin;

            try {
                Address = new AddressResolver();
                Address.Setup(plugin.PluginInterface.TargetModuleScanner);

                getUIObject = Marshal.GetDelegateForFunctionPointer<GetUIObjectDelegate>(Address.GetUIObject);
                getAgentObject = Marshal.GetDelegateForFunctionPointer<GetAgentObjectDelegate>(Address.GetAgentObject);
                searchItemByCraftingMethod = Marshal.GetDelegateForFunctionPointer<SearchItemByCraftingMethodDelegate>(Address.SearchItemByCraftingMethod);
            } catch (Exception ex) {
                PluginLog.LogError(ex.ToString());
            }
        }

        private void OnFrameworkUpdate(Framework framework) {
            try {
                if (disposed) return;
                if (plugin.PluginInterface.ClientState.LocalPlayer == null) return;
                if (!searchQueue.TryDequeue(out var itemID)) {
                    plugin.PluginInterface.Framework.OnUpdateEvent -= OnFrameworkUpdate;
                    return;
                }
                var uiObjectPtr = getUIObject();
                if (uiObjectPtr.Equals(IntPtr.Zero)) {
                    PluginLog.LogError("CraftingRecipeFinder: Null pointer returned from GetUIObject()");
                    return;
                }
                getUIAgentModule = Address.GetVirtualFunction<GetUIAgentModuleDelegate>(uiObjectPtr, 0, 34);
                var uiAgentModulePtr = getUIAgentModule(uiObjectPtr);
                if (uiAgentModulePtr.Equals(IntPtr.Zero)) {
                    PluginLog.LogError("CraftingRecipeFinder: Null pointer returned from GetUIAgentModule()");
                    return;
                }
                var recipeAgentPtr = getAgentObject(uiAgentModulePtr, 23);
                if (recipeAgentPtr.Equals(IntPtr.Zero)) {
                    PluginLog.LogError("CraftingRecipeFinder: Null pointer returned from GetAgentObject()");
                    return;
                }

                searchItemByCraftingMethod(recipeAgentPtr, (ushort) itemID);

            } catch (NullReferenceException) { }
        }

        public void SearchRecipesByItem(Item item) {
            if (disposed) return;
            if (item == null) {
                PluginLog.Log("Tried to find recipe for NULL item.");
                return;
            }

            searchQueue.Enqueue(item.RowId);
            plugin.PluginInterface.Framework.OnUpdateEvent -= OnFrameworkUpdate;
            plugin.PluginInterface.Framework.OnUpdateEvent += OnFrameworkUpdate;
        }

        public void Dispose() {
            disposed = true;
            plugin.PluginInterface.Framework.OnUpdateEvent -= OnFrameworkUpdate;
        }
    }
}
