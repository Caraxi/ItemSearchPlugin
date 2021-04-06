using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.Filters {
    class CollectableSearchFilter : SearchFilter {

        public enum Mode {
            [Description("Not Selected")]
            NotSelected,
            [Description("Any Collectable")]
            AnyCollectable,
            [Description("Not Collectable")]
            NotCollectable,
            [Description("Owned Collectable")]
            OwnedCollectable,
            [Description("Unowned Collectable")]
            UnownedCollectable
        }

        private Mode SelectedMode = Mode.NotSelected;

        public CollectableSearchFilter(ItemSearchPluginConfig config, ItemSearchPlugin plugin) : base(config) {
            this.plugin = plugin;
        }
        public override string Name => "Collectable";
        public override string NameLocalizationKey => "CollectableSearchFilter";
        public override bool IsSet => plugin.PluginInterface.ClientState.LocalContentId != 0 && SelectedMode != Mode.NotSelected;
        public override bool ShowFilter => plugin.PluginInterface.ClientState.LocalContentId != 0 && base.ShowFilter;

        private ushort[] collectableActionType = { 853, 1013, 1322, 2136, 2633, 3357, 4107, 5845, 20086 };
        private ItemSearchPlugin plugin;

        private bool faultState = false;

        public override bool CheckFilter(Item item) {
            if (faultState) return true;
            if (SelectedMode == Mode.NotSelected) return true;
            var (isCollectable, isOwned) = GetCollectable(item);

            return SelectedMode switch {
                Mode.NotCollectable => !isCollectable,
                Mode.AnyCollectable => isCollectable,
                Mode.OwnedCollectable => isCollectable && isOwned,
                Mode.UnownedCollectable => isCollectable && !isOwned,
                _ => true
            };
        }

        public (bool isCollectable, bool isOwned) GetCollectable(Item item) {
            
            var isCollectable = false;
            var isOwned = false;

            if (item == null) return (false, false);
            if (item.ItemAction == null || item.ItemAction.Row == 0) return (false, false);

            var actionId = item.ItemAction.Row;
            var actionType = item.ItemAction.Value.Type;
            
            if (collectableActionType.Contains(actionType)) {
                isCollectable = true;
                isOwned = actionType == 3357 ? plugin.IsCardOwned((ushort) item.AdditionalData) : plugin.IsCollectableOwned(actionId);
            }

            return (isCollectable, isOwned);
        }


        public override void DrawEditor() {

            if (ImGui.BeginCombo("###CollectableSearchFilterCombo", SelectedMode.DescriptionAttr())) {
                foreach (var v in Enum.GetValues(typeof(Mode))) {
                    if (ImGui.Selectable(v.DescriptionAttr(), SelectedMode == (Mode) v)) {
                        SelectedMode = (Mode) v;
                        Modified = true;
                    }
                }
                ImGui.EndCombo();
            }
        }
    }
}
