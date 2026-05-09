using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using System;
using Dalamud.Bindings.ImGui;

namespace ItemSearchPlugin.Input;

public class UpDownKeyBlocker : IDisposable
{
    private readonly ItemSearchPluginConfig _pluginConfig;
    
    private unsafe delegate void HandleInputDelegate(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, KeyboardFrame* keyboardState);
    private readonly Hook<HandleInputDelegate> handleInputHook = null!;

    public unsafe UpDownKeyBlocker(ItemSearchPluginConfig pluginPluginConfig, ISigScanner scanner, IGameInteropProvider hooking)
    {
        _pluginConfig = pluginPluginConfig;
        
        var inputHandleSig = "E8 ?? ?? ?? ?? ?? 8B ?? ?? ?? ?? 8B 87 ?? ?? ?? ?? 89 45";
        handleInputHook = hooking.HookFromAddress<HandleInputDelegate>(scanner.ScanText(inputHandleSig), HandleInputDetour);
        handleInputHook.Enable();
    }

    public unsafe void HandleInputDetour(IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, KeyboardFrame* keyboardFrame)
    {
        handleInputHook.Original(arg1, arg2, arg3, arg4, keyboardFrame);

        if (_pluginConfig.CaptureUpDownKeys) {
            keyboardFrame->HandleKey(VirtualKey.UP);
            keyboardFrame->HandleKey(VirtualKey.DOWN);
        }
    }

    public void Dispose()
    {
        handleInputHook.Dispose();
    }
}
