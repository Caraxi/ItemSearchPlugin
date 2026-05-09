using Dalamud.Game.ClientState.Keys;
using System.Runtime.InteropServices;

namespace ItemSearchPlugin.Input;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct KeyboardFrame
{
    private const int KeyStateLength = 254;

    public byte Unknown1;
    public fixed uint KeyState[KeyStateLength];

    public void HandleKey(VirtualKey virtualKey)
        => KeyState[(int)virtualKey] = 0;
}
