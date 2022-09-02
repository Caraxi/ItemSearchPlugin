using Dalamud.Game;
using Dalamud.Game.Internal;
using System;

namespace ItemSearchPlugin {
    class AddressResolver : BaseAddressResolver {
        public IntPtr TryOn { get; private set; }
        protected override void Setup64Bit(SigScanner sig) {
            this.TryOn = sig.ScanText("E8 ?? ?? ?? ?? EB 35 BA ?? ?? ?? ??");
        }
    }
}
