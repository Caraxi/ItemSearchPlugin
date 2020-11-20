using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dalamud;
using Dalamud.Game.Chat;
using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Game.Chat.SeStringHandling.Payloads;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.Internal;
using Dalamud.Hooking;
using Dalamud.Plugin;
using ImGuiNET;
using Addon = Dalamud.Game.Internal.Gui.Addon.Addon;

namespace ItemSearchPlugin {
    public class FittingRoomUI : IDisposable {
        private readonly ItemSearchPlugin plugin;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte TryOnDelegate(uint unknownSomethingToDoWithBeingEquipable, uint itemBaseId, ulong stainColor, uint itemGlamourId, byte unknownByte);

        private readonly TryOnDelegate tryOn;

        private delegate IntPtr GetFittingRoomArrayLocation(IntPtr fittingRoomBaseAddress, byte unknownByte1, byte unknownByte2);
        private delegate void UpdateCharacterPreview(IntPtr a1, uint a2);

        private readonly Hook<GetFittingRoomArrayLocation> getFittingLocationHook;
        private Hook<UpdateCharacterPreview> updateCharacterPreviewHook;

        private IntPtr fittingRoomBaseAddress = IntPtr.Zero;

        private Addon tryOnUi;
        private Addon examineUi;

        private float lastUiWidth;


        private string saveNameInput = "";
        private FittingRoomSave selectedSave;
        private string saveErrorMessage = "";

        private int tryOnDelay = 10;
        private bool windowCollapsed;

        private bool deletingSelectedSave;

        private readonly Queue<(uint itemid, uint stain)> tryOnQueue = new Queue<(uint itemid, uint stain)>();


        private readonly AddressResolver address;

        private delegate IntPtr GetInventoryContainer(IntPtr inventoryManager, int inventoryId);
        private delegate IntPtr GetContainerSlot(IntPtr inventoryContainer, int slotId);

        private GetInventoryContainer getInventoryContainer;
        private GetContainerSlot getContainerSlot;
        
        private enum TryOnControlID : uint {
            SetSaveDeleteButton = uint.MaxValue - 10,
            SuppressLog,
        }



        public FittingRoomUI(ItemSearchPlugin plugin) {
            this.plugin = plugin;

            try {
                address = new AddressResolver();
                address.Setup(plugin.PluginInterface.TargetModuleScanner);
                tryOn = Marshal.GetDelegateForFunctionPointer<TryOnDelegate>(address.TryOn);

                getInventoryContainer = Marshal.GetDelegateForFunctionPointer<GetInventoryContainer>(address.GetInventoryContainer);
                getContainerSlot = Marshal.GetDelegateForFunctionPointer<GetContainerSlot>(address.GetContainerSlot);

                getFittingLocationHook = new Hook<GetFittingRoomArrayLocation>(address.GetTryOnArrayLocation, new GetFittingRoomArrayLocation(GetFittingRoomArrayLocationDetour));
                getFittingLocationHook.Enable();


                byte previewHookCounter = 0;
                updateCharacterPreviewHook = new Hook<UpdateCharacterPreview>(address.UpdateCharacterPreview, new UpdateCharacterPreview((a1, a2) => {
                    unsafe {
                        var visibleFlag = *(uint*) (a1 + 8);
                        var previewId = *(uint*) (a1 + 16);
                        if (visibleFlag == 5 && previewId == 2) {
                            // Visible and Fitting Room
                            fittingRoomBaseAddress = a1;
                            updateCharacterPreviewHook?.Original(a1, a2);
                            updateCharacterPreviewHook?.Disable();
                            updateCharacterPreviewHook?.Dispose();
                            updateCharacterPreviewHook = null;
                            return;
                        }
                    }
                    updateCharacterPreviewHook?.Original(a1, a2);
                    if (previewHookCounter++ <= 10) return;
                    // Fitting room probably isn't open, so can stop checking
                    updateCharacterPreviewHook?.Disable();
                    updateCharacterPreviewHook?.Dispose();
                    updateCharacterPreviewHook = null;
                }));

                updateCharacterPreviewHook.Enable();

                CanUseTryOn = true;
                plugin.PluginInterface.Framework.OnUpdateEvent += OnFrameworkUpdate;
            } catch (Exception ex) {
                PluginLog.LogError(ex.ToString());
            }


        }

        private void OnFrameworkUpdate(Framework framework) {
            tryOnUi = null;
            examineUi = null;
            if (plugin.PluginInterface.ClientState.LocalPlayer == null) return;
            try {
                tryOnUi = plugin.PluginInterface.Framework.Gui.GetAddonByName("Tryon", 1);
            } catch (NullReferenceException) {
                tryOnUi = null;
            }

            try {
                examineUi = plugin.PluginInterface.Framework.Gui.GetAddonByName("CharacterInspect", 1);
            } catch (NullReferenceException) {
                examineUi = null;
            }
        }

        private IntPtr GetFittingRoomArrayLocationDetour(IntPtr fittingRoomBaseAddress, byte unknownByte1, byte unknownByte2) {
            if (unknownByte1 == 0 && unknownByte2 == 1) {
                this.fittingRoomBaseAddress = fittingRoomBaseAddress;
                updateCharacterPreviewHook?.Disable();
                updateCharacterPreviewHook?.Dispose();
                updateCharacterPreviewHook = null;
            }

            return getFittingLocationHook.Original(fittingRoomBaseAddress, unknownByte1, unknownByte2);
        }

        public bool CanUseTryOn { get; }

        public FittingRoomSaveItem[] GetFittingRoomItems() {
            if (fittingRoomBaseAddress == IntPtr.Zero) {
                return null;
            }

            var fittingRoom = Marshal.PtrToStructure<FittingRoomStruct>(fittingRoomBaseAddress);

            return 
                fittingRoom.Items
                    .Where(i => i.BaseItem != 0 && i.Slot != 5 && i.Slot < 14)
                    .OrderBy(i => i.Slot)
                    .Select(i => new FittingRoomSaveItem {
                        ItemID = (i.GlamourItem > 0 ? i.GlamourItem : i.BaseItem) % 500000,
                        Stain = i.Stain,
                    })
                    .ToArray();
        }
        
        public void DebugFittingRoom() {
            if (fittingRoomBaseAddress == IntPtr.Zero) {
                return;
            }
            
            PluginLog.Log($"FittingRoom: {fittingRoomBaseAddress.ToInt64():X}");


            var empty = new byte[0x20];

            var fittingRoom = Marshal.PtrToStructure<FittingRoomStruct>(fittingRoomBaseAddress);


            foreach (var i in fittingRoom.Items) {
                PluginLog.Log($"{i.BaseItem}, {i.GlamourItem}, {i.Stain}");
            }
        }



        public void TryOnItem(Item item, uint stain = 0, bool hq = false) {
            if (item.EquipSlotCategory.Row > 0 && item.EquipSlotCategory.Row != 6 && item.EquipSlotCategory.Row != 17 && (item.EquipSlotCategory.Value.OffHand == 0 || item.ItemUICategory.Row == 11)) {
                tryOnQueue.Enqueue((item.RowId + (uint) (hq ? 1000000 : 0), stain));
            }
        }

        public void OpenFittingRoom() {
            tryOnQueue.Enqueue((0, 0));
        }

        public void SetSaveDeleteButton(bool enabled) {
            if (fittingRoomBaseAddress != IntPtr.Zero) {
                Marshal.WriteByte(fittingRoomBaseAddress, 0x2BA, enabled ? (byte) 1 : (byte) 0);
            }
        }
        
        public void Draw() {
            
            
            while (CanUseTryOn && tryOnQueue.Count > 0 && (tryOnDelay <= 0 || tryOnDelay-- <= 0)) {
                try {
                    var (itemid, stain) = tryOnQueue.Dequeue();

                    switch ((TryOnControlID) itemid) {
                        case TryOnControlID.SetSaveDeleteButton: {
                            SetSaveDeleteButton(stain == 1);
                            break;
                        }
                        case TryOnControlID.SuppressLog: {
                            if (stain == 1) {
                                plugin.PluginInterface.Framework.Gui.Chat.OnChatMessage += ChatOnOnChatMessage;
                            } else {
                                plugin.PluginInterface.Framework.Gui.Chat.OnChatMessage -= ChatOnOnChatMessage;
                            }
                            break;
                        }
                        default: {
                            tryOnDelay = 1;
                            tryOn(0xFF, itemid, stain, 0, 0);
                            break;
                        }
                    }

                } catch {
                    tryOnDelay = 5;
                    break;
                }
            }
            
            if (fittingRoomBaseAddress != IntPtr.Zero && tryOnUi != null) {
                var pos = new Vector2(tryOnUi.X, tryOnUi.Y);
                pos.Y += 20 * tryOnUi.Scale;
                if (pos.X < lastUiWidth + 20) {
                    pos.X += tryOnUi.Scale * 340;
                } else {
                    pos.X -= lastUiWidth;
                }

                var buttonSize = new Vector2(-1, 22 * ImGui.GetIO().FontGlobalScale);

                var hiddenPos = new Vector2(tryOnUi.X, tryOnUi.Y);

                hiddenPos.Y -= 19 * ImGui.GetIO().FontGlobalScale;

                ImGui.SetNextWindowPos(windowCollapsed ? hiddenPos : pos, ImGuiCond.Always);
                ImGui.SetNextWindowSize(new Vector2(220 * ImGui.GetIO().FontGlobalScale, tryOnUi.Scale * 300 + buttonSize.Y * 3), ImGuiCond.Always);

                windowCollapsed = !ImGui.Begin(Loc.Localize("FittingRoomUIHeader", "Saved Outfits") + "###ItemSearchPluginFittingRoomUI", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

                ImGui.SetNextItemWidth(-50 * ImGui.GetIO().FontGlobalScale);
                ImGui.InputText("###FittingRoomUI_SaveNameInput", ref saveNameInput, 32);
                ImGui.SameLine();

                if (ImGui.Button(Loc.Localize("SaveButton", "Save") + "##FittingRoomUI_SaveButton", buttonSize)) {
                    saveErrorMessage = "";
                    // Check for duplicate names

                    if (plugin.PluginConfig.FittingRoomSaves.Any(a => a.Name.ToLower() == saveNameInput.ToLower())) {
                        saveErrorMessage = Loc.Localize("FittingRoomUI_Error_SaveAlreadyExists", "A save with that name already exists.");
                    } else {
                        var save = new FittingRoomSave {
                            Name = saveNameInput,
                            Items = GetFittingRoomItems()
                        };

                        if (save.Items.Length == 0) {
                            saveErrorMessage = Loc.Localize("FittingRoomUI_Error_NoItems", "No items in fitting room.");
                        } else {
                            plugin.PluginConfig.FittingRoomSaves.Add(save);
                            plugin.PluginConfig.Save();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(saveErrorMessage)) {
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), saveErrorMessage);
                }

#if DEBUG
                if (ImGui.Button(string.Format(Loc.Localize("FittingRoomUI_PrintOutButton", "Debug")), buttonSize)) {
                    DebugFittingRoom();
                }
#endif

                ImGui.Separator();
                ImGui.Text(Loc.Localize("FittingRoomUI_SelectOutfit", "Select outfit to load:"));
                
                ImGui.BeginChild("###FittingRoomUI_LoadSelect", new Vector2(0, -buttonSize.Y * 2 - ImGui.GetStyle().ItemSpacing.Y * 3), true, ImGuiWindowFlags.HorizontalScrollbar);

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

                foreach (var save in plugin.PluginConfig.FittingRoomSaves) {
                    if (ImGui.Selectable($"{save.Name}##fittingRoomSave", selectedSave == save, ImGuiSelectableFlags.AllowDoubleClick)) {
                        deletingSelectedSave = false;
                        selectedSave = save;
                        if (ImGui.IsMouseDoubleClicked(0)) {
                            LoadSelectedSave(plugin.PluginInterface.ClientState.KeyState[0x10]);
                        }
                    }

                    if (ImGui.IsItemHovered()) {
                        
                        var itemSize = new Vector2(45) * ImGui.GetIO().FontGlobalScale;
                        var stainDotSize = 4 * ImGui.GetIO().FontGlobalScale;
                        var stainDotOffset = new Vector2(itemSize.X - stainDotSize, stainDotSize);
                        ImGui.BeginTooltip();
                        
                        ImGui.Text(save.Name);
                        ImGui.BeginChild("itemsPreview", new Vector2(itemSize.X * 4, itemSize.Y * 3) + ImGui.GetStyle().ItemSpacing * 3 + ImGui.GetStyle().WindowPadding * 2, true);
                        var dl = ImGui.GetWindowDrawList();
                        var c = 0;
                        foreach (var i in save.Items) {
                            var item = plugin.PluginInterface.Data.Excel.GetSheet<Item>().GetRow(i.ItemID % 500000);
                            if (item != null) {
                                var p = ImGui.GetCursorScreenPos();
                                plugin.DrawIcon(item.Icon, itemSize);

                                if (i.Stain > 0) {
                                    var stain = plugin.PluginInterface.Data.Excel.GetSheet<Stain>().GetRow(i.Stain);

                                    var b = stain.Color & 255;
                                    var g = (stain.Color >> 8) & 255;
                                    var r = (stain.Color >> 16) & 255;
                                    
                                    dl.AddCircleFilled(p + stainDotOffset, stainDotSize + 1, stain.Unknown4 ? 0xFF005555 : 0xFF000000);
                                    dl.AddCircleFilled(p + stainDotOffset,  stainDotSize, ImGui.ColorConvertFloat4ToU32(new Vector4(r / 255F, g / 255F, b / 255F, 1)));
                                }

                                if (++c < 4) {
                                    ImGui.SameLine();
                                } else {
                                    c = 0;
                                }

                            }
                        }


                        ImGui.EndChild();
                        ImGui.EndTooltip();

                    }


                }

                ImGui.PopStyleVar();

                ImGui.EndChild();
                if (selectedSave != null) {

                    if (plugin.PluginInterface.ClientState.KeyState[0x10]) {
                        if (ImGui.Button(string.Format(Loc.Localize("FittingRoomUI_MergeButton", "Merge '{0}'"), selectedSave.Name), buttonSize)) {
                            LoadSelectedSave(true);
                        }

                        if (plugin.PluginConfig.DeletedFittingRoomSaves.Count > 0) {
                            if (ImGui.Button(string.Format(Loc.Localize("FittingRoomUI_UndoDelete", "Undelete '{0}'"), plugin.PluginConfig.DeletedFittingRoomSaves.Peek().Name), buttonSize)) {
                                selectedSave = plugin.PluginConfig.DeletedFittingRoomSaves.Pop();
                                plugin.PluginConfig.FittingRoomSaves.Add(selectedSave);
                                plugin.PluginConfig.Save();
                            }
                        } else {
                            ImGui.PushStyleColor(ImGuiCol.Button, 0x44444444);
                            ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0x44444444);
                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0x44444444);
                            ImGui.PushStyleColor(ImGuiCol.Text, 0x88888888);
                            ImGui.Button(string.Format(Loc.Localize("FittingRoomUI_UndoDeleteNone", "Undelete")), buttonSize);
                            ImGui.PopStyleColor(4);
                        }

                    } else {
                        if (ImGui.Button(string.Format(Loc.Localize("FittingRoomUI_LoadButton", "Load '{0}'"), selectedSave.Name), buttonSize)) {
                            LoadSelectedSave();
                        }

                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip("Hold SHIFT to merge with current outfit.");
                        }

                        if (ImGui.Button(string.Format(Loc.Localize("FittingRoomUI_DeleteButton", "Delete '{0}'"), selectedSave.Name), buttonSize)) {
                            plugin.PluginConfig.FittingRoomSaves.Remove(selectedSave);
                            plugin.PluginConfig.DeletedFittingRoomSaves.Push(selectedSave);
                            selectedSave = null;
                            plugin.PluginConfig.Save();
                        }

                    }

                } else if (plugin.PluginConfig.DeletedFittingRoomSaves.Count > 0) {
                    if (ImGui.Button(string.Format(Loc.Localize("FittingRoomUI_UndoDelete", "Undelete '{0}'"), plugin.PluginConfig.DeletedFittingRoomSaves.Peek().Name), buttonSize)) {
                        selectedSave = plugin.PluginConfig.DeletedFittingRoomSaves.Pop();
                        plugin.PluginConfig.FittingRoomSaves.Add(selectedSave);
                        plugin.PluginConfig.Save();
                    }
                }

                lastUiWidth = ImGui.GetWindowWidth();
                ImGui.End();
            }

            unsafe {
                if (plugin.PluginConfig.ShowTryOn && CanUseTryOn && examineUi != null) {
                    if (address.ExamineValid == IntPtr.Zero  || *(byte*)(address.ExamineValid + 0x2A8) == 0) return;
                    var container = getInventoryContainer(address.InventoryManager, 2009);
                    if (container == IntPtr.Zero) return;

                    ImGui.SetNextWindowPos(new Vector2(examineUi.X + 15, examineUi.Y + 490 * examineUi.Scale));
                    ImGui.Begin("TryOn###examineTryOn", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground);
                    ImGui.SetWindowFontScale(examineUi.Scale * (1 / ImGui.GetIO().FontGlobalScale));
                    if (ImGui.Button("Try On Outfit")) {
                        tryOnQueue.Enqueue(((uint) TryOnControlID.SetSaveDeleteButton, 0));
                        tryOnQueue.Enqueue(((uint)TryOnControlID.SuppressLog, 1));
                        for (var i = 0; i < 13; i++) {
                            var slot = getContainerSlot(container, i);

                            if (slot == IntPtr.Zero) continue;
                            var itemid = *(uint*) (slot + 0x08);
                            var glamourId = *(uint*) (slot + 0x30);
                            var stain = *(byte*) (slot + 0x2F);
                            #if DEBUG
                            PluginLog.Log($"{slot.ToInt64():X}: {itemid}, {glamourId} {stain}");
                            #endif
                            var id = glamourId == 0 ? itemid : glamourId;
                            
                            if (id != 0) {

                                var item = plugin.PluginInterface.Data.Excel.GetSheet<Item>().GetRow(id);
                                if (item.EquipSlotCategory.Value.OffHand != 1 || item.ItemUICategory.Row == 11) {
                                    tryOnQueue.Enqueue((id, stain));
                                    tryOnQueue.Enqueue(((uint)TryOnControlID.SetSaveDeleteButton, 1));
                                }
                            }
                        }
                        tryOnQueue.Enqueue(((uint)TryOnControlID.SuppressLog, 0));
                    }
                    ImGui.End();

                }

            }
        }

        private void ChatOnOnChatMessage(XivChatType type, uint senderid, ref SeString sender, ref SeString message, ref bool ishandled) {
            if (type == XivChatType.SystemMessage && message.Payloads.Count > 1 && (plugin.PluginInterface.ClientState.ClientLanguage == ClientLanguage.Japanese ? message.Payloads[message.Payloads.Count - 1] : message.Payloads[0]) is TextPayload a) {

                bool handle = plugin.PluginInterface.ClientState.ClientLanguage switch {
                    ClientLanguage.English => a.Text.StartsWith("You try on "),
                    ClientLanguage.German => a.Text.StartsWith("Da hast "),
                    ClientLanguage.French => a.Text.StartsWith("Vous essayez "),
                    ClientLanguage.Japanese => a.Text.EndsWith("を試着した。"),
                    _ => false,
                };

                if (handle) {
                    ishandled = true;
                }
            }
        }

        private void LoadSelectedSave(bool merge = false) {
            tryOnQueue.Enqueue(((uint)TryOnControlID.SuppressLog, 1));
            tryOnQueue.Enqueue(((uint)TryOnControlID.SetSaveDeleteButton, merge ? 1U : 0U));
            foreach (var item in selectedSave.Items) {
                tryOnQueue.Enqueue((item.ItemID, item.Stain));
                SetSaveDeleteButton(true);
                tryOnQueue.Enqueue(((uint)TryOnControlID.SetSaveDeleteButton, 1));
            }
            tryOnQueue.Enqueue(((uint)TryOnControlID.SuppressLog, 0));
        }

        public void Dispose() {
            plugin.PluginInterface.Framework.OnUpdateEvent -= OnFrameworkUpdate;
            getFittingLocationHook?.Disable();
            updateCharacterPreviewHook?.Disable();
        }

    }
}
