using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.Internal;
using Dalamud.Hooking;
using Dalamud.Plugin;
using System.Collections.Concurrent;
using System.Linq.Expressions;

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
        private delegate void OpenRecipeLogDelegate(IntPtr RecipeAgentObject);
        private readonly OpenRecipeLogDelegate openRecipeLog;

        private readonly ConcurrentQueue<uint> searchQueue = new ConcurrentQueue<uint>();

        public CraftingRecipeFinder(ItemSearchPlugin plugin) {
            this.plugin = plugin;

            try {
                Address = new AddressResolver();
                Address.Setup(plugin.PluginInterface.TargetModuleScanner);

                getUIObject = Marshal.GetDelegateForFunctionPointer<GetUIObjectDelegate>(Address.GetUIObject);
                getAgentObject = Marshal.GetDelegateForFunctionPointer<GetAgentObjectDelegate>(Address.GetAgentObject);
                openRecipeLog = Marshal.GetDelegateForFunctionPointer<OpenRecipeLogDelegate>(Address.OpenRecipeLog);

                plugin.PluginInterface.Framework.OnUpdateEvent += OnFrameworkUpdate;
            } catch (Exception ex) {
                PluginLog.LogError(ex.ToString());
            }
        }

        private void OnFrameworkUpdate(Framework framework) {
            try {
                if (plugin.PluginInterface.ClientState.LocalPlayer == null) return;
                uint itemID;
                if (!searchQueue.TryDequeue(out itemID)) return;
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
                var recipeAgentPtr = getAgentObject(uiAgentModulePtr, 22);
                if (recipeAgentPtr.Equals(IntPtr.Zero)) {
                    PluginLog.LogError("CraftingRecipeFinder: Null pointer returned from GetAgentObject()");
                    return;
                }

                PluginLog.Log("CraftingRecipeFinder ptr trail: {0:X16} {1:X16} {2:X16}", (ulong)uiObjectPtr, (ulong)uiAgentModulePtr, (ulong)recipeAgentPtr);

                openRecipeLog(recipeAgentPtr);
                Marshal.WriteInt32(recipeAgentPtr, 0x3A8, 3);
                Marshal.WriteInt32(recipeAgentPtr, 0x3AC, unchecked((int)itemID));

            } catch (NullReferenceException) { }
        }

        public void SearchRecipesByItem(Item item) {
            searchQueue.Enqueue(item.RowId);
        }

        public void Dispose() {
            plugin.PluginInterface.Framework.OnUpdateEvent -= OnFrameworkUpdate;
        }
    }
}
