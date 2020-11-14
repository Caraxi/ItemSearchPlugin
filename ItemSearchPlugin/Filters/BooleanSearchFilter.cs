using System;
using System.Numerics;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.Filters {
    class BooleanSearchFilter : SearchFilter {
        private readonly string trueString;
        private readonly string falseString;
        private readonly Func<Item, bool> checkFunction;
        private bool showTrue = true;
        private bool showFalse = true;

        private static float _trueWidth;

        public BooleanSearchFilter(ItemSearchPluginConfig pluginConfig, string name, string trueString, string falseString, Func<Item, bool> checkFunction) : base(pluginConfig) {
            this.Name = name;
            this.trueString = trueString;
            this.falseString = falseString;
            this.checkFunction = checkFunction;
        }

        public override string Name { get; }

        public override string NameLocalizationKey => $"{Name}SearchFilter";
        public override bool IsSet => showTrue == false || showFalse == false;

        public override bool CheckFilter(Item item) {
            return checkFunction.Invoke(item);
        }

        public override void DrawEditor() {
            var x = ImGui.GetCursorPosX();
            if (ImGui.Checkbox(trueString, ref showTrue)) {
                if (!showTrue) showFalse = true;
                Modified = true;
            }

            ImGui.SameLine();

            var x2 = ImGui.GetCursorPosX() - x;
            if (x2 > _trueWidth) {
                _trueWidth = x2;
            }

            ImGui.SetCursorPosX(x + _trueWidth);

            if (ImGui.Checkbox(falseString, ref showFalse)) {
                if (!showFalse) showTrue = true;
                Modified = true;
            }
        }

        public override string ToString() {
            return showTrue ? trueString : falseString;
        }

        public override void Hide() {
            _trueWidth = 0;
        }
    }
}
