using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace ItemSearchPlugin {
    public class TryOn : IDisposable {

        private readonly ItemSearchPlugin plugin;

        private int tryOnDelay = 10;

        private readonly Queue<(uint itemid, uint stain, uint stain2)> tryOnQueue = new();

        private enum TryOnControlID : uint {
            SuppressLog = uint.MaxValue - 10,
        }

        public TryOn(ItemSearchPlugin plugin) {
            this.plugin = plugin;
            CanUseTryOn = true;
            Framework.Update += FrameworkUpdate;
        
        }

        public bool CanUseTryOn { get; }

        public void TryOnItem(Item item, uint stain = 0, uint stain2 = 0, bool hq = false) {
#if DEBUG
            PluginLog.Debug($"Try On: {item.Name}");
#endif
            if (item.EquipSlotCategory?.Value == null) return;
            if (item.EquipSlotCategory.Row > 0 && item.EquipSlotCategory.Row != 6 && item.EquipSlotCategory.Row != 17 && (item.EquipSlotCategory.Value.OffHand <=0 || item.ItemUICategory.Row == 11)) {
                if (plugin.PluginConfig.SuppressTryOnMessage) tryOnQueue.Enqueue(((uint) TryOnControlID.SuppressLog, 1, 0));
                tryOnQueue.Enqueue((item.RowId + (uint) (hq ? 1000000 : 0), stain, stain2));
                if (plugin.PluginConfig.SuppressTryOnMessage) tryOnQueue.Enqueue(((uint)TryOnControlID.SuppressLog, 0, 0));
            }
#if DEBUG
            else {
                PluginLog.Warning($"Cancelled Try On: Invalid Item. ({item.EquipSlotCategory.Row}, {item.EquipSlotCategory.Value.OffHand}, {item.EquipSlotCategory.Value.Waist}, {item.EquipSlotCategory.Value.SoulCrystal})");
            }
#endif
        }

        public void OpenFittingRoom() {
            tryOnQueue.Enqueue((0, 0, 0));
        }

        
        public void FrameworkUpdate(IFramework framework) {
            
            while (CanUseTryOn && tryOnQueue.Count > 0 && (tryOnDelay <= 0 || tryOnDelay-- <= 0)) {
                try {
                    var (itemId, stain, stain2) = tryOnQueue.Dequeue();

                    switch ((TryOnControlID) itemId) {
                        case TryOnControlID.SuppressLog: {
                            if (stain == 1) {
                                Chat.CheckMessageHandled += HandleChat;
                            } else {
                                Chat.CheckMessageHandled -= HandleChat;
                            }
                            break;
                        }
                        default: {
                            tryOnDelay = 1;
                            AgentTryon.TryOn(0, itemId, (byte)stain, (byte) stain2);
                            break;
                        }
                    }

                } catch {
                    tryOnDelay = 5;
                    break;
                }
            }
        }

        private void HandleChat(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled) {
            if (type != XivChatType.SystemMessage || message.Payloads.Count <= 1 || (ClientState.ClientLanguage == ClientLanguage.Japanese ? message.Payloads[message.Payloads.Count - 1] : message.Payloads[0]) is not TextPayload a) return;
            var handle = ClientState.ClientLanguage switch {
                ClientLanguage.English => a.Text?.StartsWith("You try on ") ?? false,
                ClientLanguage.German => a.Text?.StartsWith("Da hast ") ?? false,
                ClientLanguage.French => a.Text?.StartsWith("Vous essayez ") ?? false,
                ClientLanguage.Japanese => a.Text?.EndsWith("を試着した。") ?? false,
                _ => false,
            };
            if (handle) isHandled = true;
        }

        public void Dispose() {
            Framework.Update -= FrameworkUpdate;
            Chat.CheckMessageHandled -= HandleChat;
        }
    }
}
