using Dalamud.Game;
using Dalamud.Game.Internal;
using System;

namespace ItemSearchPlugin {
	class AddressResolver : BaseAddressResolver {
		public IntPtr TryOn { get; private set; }
		public IntPtr GetTryOnArrayLocation { get; private set; }
		public IntPtr GetBaseUIObject { get; private set; }
		public IntPtr GetUI2ObjByName { get; private set; }


		protected override void Setup64Bit(SigScanner sig)
        {
            this.TryOn = sig.ScanText("E8 ?? ?? ?? ?? EB 39 48 8B 49 08");
            this.GetTryOnArrayLocation = sig.ScanText("E8 ?? ?? ?? ?? 48 8B 97 ?? ?? ?? ?? 49 8B CD");
            this.GetBaseUIObject = sig.ScanText("E8 ?? ?? ?? ?? 41 b8 01 00 00 00 48 8d 15 ?? ?? ?? ?? 48 8b 48 20 e8 ?? ?? ?? ?? 48 8b cf");
            this.GetUI2ObjByName = sig.ScanText("e8 ?? ?? ?? ?? 48 8b cf 48 89 87 ?? ?? 00 00 e8 ?? ?? ?? ?? 41 b8 01 00 00 00");
        }

	}
}
