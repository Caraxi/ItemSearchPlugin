using Dalamud.Game;
using Dalamud.Game.Internal;
using System;

namespace ItemSearchPlugin {
    class AddressResolver : BaseAddressResolver {
        public IntPtr TryOn { get; private set; }
        public IntPtr GetTryOnArrayLocation { get; private set; }
        public IntPtr GetUIObject { get; private set; }
        public IntPtr GetAgentObject { get; private set; }
        public IntPtr SearchItemByCraftingMethod { get; private set; }

        protected override void Setup64Bit(SigScanner sig) {
            this.TryOn = sig.ScanText("E8 ?? ?? ?? ?? EB 35 BA ?? ?? ?? ??");
            this.GetTryOnArrayLocation = sig.ScanText("E8 ?? ?? ?? ?? 48 8B 97 ?? ?? ?? ?? 49 8B CD");
            this.GetUIObject = sig.ScanText("E8 ?? ?? ?? ?? 48 8B C8 48 8B 10 FF 52 40 80 88 ?? ?? ?? ?? 01 E9");
            this.GetAgentObject = sig.ScanText("E8 ?? ?? ?? ?? 8D 56 24");
            this.SearchItemByCraftingMethod = sig.ScanText("E8 ?? ?? ?? ?? EB 7A 48 83 F8 05");
        }
    }
}
