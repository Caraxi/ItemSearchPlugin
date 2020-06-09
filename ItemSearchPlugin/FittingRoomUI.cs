using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using CheapLoc;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.Internal;
using Dalamud.Hooking;
using Dalamud.Plugin;
using ImGuiNET;

namespace ItemSearchPlugin {
    public class FittingRoomUI : IDisposable {
        private readonly ItemSearchPlugin plugin;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetBaseUIObjDelegate();

        private readonly GetBaseUIObjDelegate getBaseUIObj;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr GetUI2ObjByNameDelegate(IntPtr getBaseUIObj, string uiName, int index);

        private readonly GetUI2ObjByNameDelegate getUI2ObjByName;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte TryOnDelegate(uint unknownSomethingToDoWithBeingEquipable, uint itemBaseId, byte stainColor, uint itemGlamourId, byte unknownByte);

        private readonly TryOnDelegate tryOn;

        private delegate IntPtr GetFittingRoomArrayLocation(IntPtr fittingRoomBaseAddress, byte unknownByte1, byte unknownByte2);

        private readonly Hook<GetFittingRoomArrayLocation> getFittingLocationHook;
        private IntPtr fittingRoomBaseAddress = IntPtr.Zero;

        private UIObject? tryonUIObject;

        private float lastUiWidth = 0;


        private string saveNameInput = "";
        private FittingRoomSave selectedSave = null;
        private string saveErrorMessage = "";

        private int tryOnDelay = 10;
        private bool windowCollapsed = false;

        private bool deletingSelectedSave = false;

        private readonly Queue<(uint itemid, byte stain)> tryOnQueue = new Queue<(uint itemid, byte stain)>();


        public FittingRoomUI(ItemSearchPlugin plugin) {
            this.plugin = plugin;

            plugin.PluginInterface.Framework.OnUpdateEvent += OnFrameworkUpdate;

            try {
                var address = new AddressResolver();
                address.Setup(plugin.PluginInterface.TargetModuleScanner);
                tryOn = Marshal.GetDelegateForFunctionPointer<TryOnDelegate>(address.TryOn);

                getBaseUIObj = Marshal.GetDelegateForFunctionPointer<GetBaseUIObjDelegate>(address.GetBaseUIObject);
                getUI2ObjByName = Marshal.GetDelegateForFunctionPointer<GetUI2ObjByNameDelegate>(address.GetUI2ObjByName);

                getFittingLocationHook = new Hook<GetFittingRoomArrayLocation>(address.GetTryOnArrayLocation, new GetFittingRoomArrayLocation(GetFittingRoomArrayLocationDetour));
                getFittingLocationHook.Enable();
                CanUseTryOn = true;
            } catch (Exception ex) {
                PluginLog.LogError(ex.ToString());
            }
        }

        private void OnFrameworkUpdate(Framework framework) {
            try {
                IntPtr baseUIObject = getBaseUIObj();
                if (baseUIObject != IntPtr.Zero) {
                    IntPtr tryonUIPtr = getUI2ObjByName(Marshal.ReadIntPtr(baseUIObject, 0x20), "Tryon", 1);
                    if (tryonUIPtr != IntPtr.Zero) {
                        tryonUIObject = Marshal.PtrToStructure<UIObject>(tryonUIPtr);
                    } else {
                        tryonUIObject = null;
                    }
                } else {
                    tryonUIObject = null;
                }
            } catch (NullReferenceException) {
                tryonUIObject = null;
            }
        }

        private IntPtr GetFittingRoomArrayLocationDetour(IntPtr fittingRoomBaseAddress, byte unknownByte1, byte unknownByte2) {
            if (unknownByte1 == 0 && unknownByte2 == 1) {
                this.fittingRoomBaseAddress = fittingRoomBaseAddress;
            }

            return getFittingLocationHook.Original(fittingRoomBaseAddress, unknownByte1, unknownByte2);
        }

        public bool CanUseTryOn { get; }

        public FittingRoomSaveItem[] GetFittingRoomItems() {
            if (fittingRoomBaseAddress == IntPtr.Zero) {
                return null;
            }

            List<FittingRoomSaveItem> arr = new List<FittingRoomSaveItem>();

            for (int i = 0; i < 14; i++) {
                IntPtr itemOffset = fittingRoomBaseAddress + 0x0D8 + (0x20 * i);
                IntPtr stainOffset = fittingRoomBaseAddress + 0x2C2 + (0x18 * i);


                int itemBase = Marshal.ReadInt32(itemOffset);
                int itemGlamour = Marshal.ReadInt32(itemOffset, 0x4);

                if (itemBase > 1000000) itemBase -= 1000000;
                if (itemGlamour > 1000000) itemGlamour -= 1000000;

                if (itemBase == 0 && itemGlamour == 0) {
                    continue;
                }

                byte stainBase = Marshal.ReadByte(stainOffset);
                byte stainPreview = Marshal.ReadByte(stainOffset, 0x1);

                arr.Add(new FittingRoomSaveItem() {
                    ItemID = itemGlamour == 0 ? itemBase : itemGlamour,
                    Stain = stainPreview == 0 ? stainBase : stainPreview
                });
            }

            return arr.ToArray();
        }

        public void TryOnItem(Item item, byte stain = 0, bool HQ = false) {
            if (item.EquipSlotCategory.Row > 0 && item.EquipSlotCategory.Row != 6 && item.EquipSlotCategory.Row != 17) {
                tryOnQueue.Enqueue((item.RowId + (uint) (HQ ? 1000000 : 0), stain));
            }
        }

        public void SetSaveDeleteButton(bool enabled) {
            if (fittingRoomBaseAddress != IntPtr.Zero) {
                Marshal.WriteByte(fittingRoomBaseAddress, 0x2BA, enabled ? (byte) 1 : (byte) 0);
            }
        }

        public void Draw() {
            if (CanUseTryOn && tryOnQueue.Count > 0 && (tryOnDelay-- <= 0)) {
                tryOnDelay = 1;
                try {
                    var (itemid, stain) = tryOnQueue.Dequeue();

                    tryOn(0xFF, itemid, stain, 0, 0);
                } catch {
                    // ignored
                }
            }

            if (fittingRoomBaseAddress != IntPtr.Zero && tryonUIObject?.Visible == true) {
                UIObject ui = tryonUIObject.Value;

                Vector2 pos = ui.Position;
                pos.Y += 20 * ui.Scale;
                if (pos.X < lastUiWidth + 20) {
                    pos.X += ui.Scale * 340;
                } else {
                    pos.X -= lastUiWidth;
                }

                Vector2 hiddenPos = ui.Position;

                hiddenPos.Y -= 24;

                ImGui.SetNextWindowPos(windowCollapsed ? hiddenPos : pos, ImGuiCond.Always);
                ImGui.SetNextWindowSize(new Vector2(220, ui.Scale * 200 + 160), ImGuiCond.Always);

                windowCollapsed = !ImGui.Begin(Loc.Localize("FittingRoomUIHeader", "Saved Outfits") + "###ItemSearchPluginFittingRoomUI", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

                ImGui.SetNextItemWidth(150f);
                ImGui.InputText("###FittingRoomUI_SaveNameInput", ref saveNameInput, 32);
                ImGui.SameLine();
                if (ImGui.Button(Loc.Localize("SaveButton", "Save") + "##FittingRoomUI_SaveButton")) {
                    saveErrorMessage = "";
                    // Check for duplicate names

                    if (plugin.PluginConfig.FittingRoomSaves.Any(a => a.Name.ToLower() == saveNameInput.ToLower())) {
                        saveErrorMessage = Loc.Localize("FittingRoomUI_Error_SaveAlreadyExists", "A save with that name already exists.");
                    } else {
                        FittingRoomSave save = new FittingRoomSave {
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

                ImGui.Separator();
                ImGui.Text(Loc.Localize("FittingRoomUI_SelectOutfit", "Select outfit to load:"));
                float w = ImGui.GetWindowWidth();
                ImGui.BeginChild("###FittingRoomUI_LoadSelect", new Vector2(0, 200 * ui.Scale), true, ImGuiWindowFlags.HorizontalScrollbar);

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

                foreach (FittingRoomSave save in plugin.PluginConfig.FittingRoomSaves) {
                    if (ImGui.Selectable($"{save.Name}##fittingRoomSave", selectedSave == save)) {
                        deletingSelectedSave = false;
                        selectedSave = save;
                    }
                }

                ImGui.PopStyleVar();

                ImGui.EndChild();
                if (selectedSave != null) {
                    float buttonWidth = w - (ImGui.GetStyle().ItemSpacing.X * 2);

                    Vector2 buttonSize = new Vector2(buttonWidth, 26);

                    if (deletingSelectedSave) {
                        if (ImGui.Button(Loc.Localize("FittingRoomUI_ConfirmDelete", "Confirm Delete"), buttonSize)) {
                            deletingSelectedSave = false;
                            plugin.PluginConfig.FittingRoomSaves.Remove(selectedSave);
                            selectedSave = null;
                            plugin.PluginConfig.Save();
                        }

                        if (ImGui.Button(Loc.Localize("FittingRoomUI_CancelDelete", "Don't Delete"), buttonSize)) {
                            deletingSelectedSave = false;
                        }
                    } else {
                        if (ImGui.Button(string.Format(Loc.Localize("FittingRoomUI_LoadButton", "Load '{0}'"), selectedSave.Name), buttonSize)) {
                            foreach (FittingRoomSaveItem item in selectedSave.Items) {
                                SetSaveDeleteButton(true);
                                tryOnQueue.Enqueue(((uint) item.ItemID, item.Stain));
                            }
                        }

                        if (ImGui.Button(string.Format(Loc.Localize("FittingRoomUI_DeleteButton", "Delete '{0}'"), selectedSave.Name), buttonSize)) {
                            deletingSelectedSave = true;
                        }
                    }
                }

                lastUiWidth = ImGui.GetWindowWidth();
                ImGui.End();
            }
        }

        public void Dispose() {
            getFittingLocationHook?.Disable();
            plugin.PluginInterface.Framework.OnUpdateEvent -= OnFrameworkUpdate;
        }
    }
}
