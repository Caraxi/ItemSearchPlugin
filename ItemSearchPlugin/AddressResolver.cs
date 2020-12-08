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
        public IntPtr InventoryManager { get; set; }
        public IntPtr ExamineValid { get; set; }
        public IntPtr GetContainerSlot { get; set; }
        public IntPtr GetInventoryContainer { get; set; }
        public IntPtr UpdateCharacterPreview { get; set; }

        protected override void Setup64Bit(SigScanner sig) {
            this.TryOn = sig.ScanText("E8 ?? ?? ?? ?? EB 35 BA ?? ?? ?? ??");
            this.GetTryOnArrayLocation = sig.ScanText("E8 ?? ?? ?? ?? 48 8B 93 ?? ?? ?? ?? 49 8B CD");
            this.GetUIObject = sig.ScanText("E8 ?? ?? ?? ?? 48 8B C8 48 8B 10 FF 52 40 80 88 ?? ?? ?? ?? 01 E9");
            this.GetAgentObject = sig.ScanText("E8 ?? ?? ?? ?? 83 FF 0D");
            this.SearchItemByCraftingMethod = sig.ScanText("E8 ?? ?? ?? ?? EB 7A 48 83 F8 06");
            this.InventoryManager = sig.GetStaticAddressFromSig("BA ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B F8 48 85 C0");
            this.GetInventoryContainer = sig.ScanText("E8 ?? ?? ?? ?? 8B 55 BB");
            this.GetContainerSlot = sig.ScanText("E8 ?? ?? ?? ?? 8B 5B 0C");
            this.ExamineValid = sig.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 C7 43 ?? ?? ?? ?? ??");
            this.UpdateCharacterPreview = sig.ScanText("E8 ?? ?? ?? ?? 83 7B 08 05");
        }
    }
}
