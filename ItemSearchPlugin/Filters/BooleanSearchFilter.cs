using System;
using System.Numerics;
using Dalamud.Plugin;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.Filters {
    class BooleanSearchFilter : SearchFilter {

        public static Func<Item, bool, bool, bool> CheckFunc(string n, bool invert = false) {
            var p = typeof(Item).GetProperty(n);
            if (p == null) {
                PluginLog.LogError($"Invalid Check Function for BooleanSearchFilter: {n}");
            }
            return (i, t, f) => {
                if (p == null) return true;
                var v = (bool) p.GetValue(i);
                return (invert ? !v : v) ? t : f;
            };
        }


        private readonly string trueString;
        private readonly string falseString;
        private readonly Func<Item, bool, bool, bool> checkFunction;
        private readonly Func<EventItem, bool, bool, bool> keyCheckFunction;
        private bool showTrue = true;
        private bool showFalse = true;

        private bool usingTag = false;
        private bool taggedTrue = false;
        private bool taggedFalse = false;

        private static float _trueWidth;

        public BooleanSearchFilter(ItemSearchPluginConfig pluginConfig, string name, string trueString, string falseString, Func<Item, bool, bool, bool> checkFunction, Func<EventItem, bool, bool, bool> keyCheckFunction = null) : base(pluginConfig) {
            this.Name = name;
            this.trueString = trueString;
            this.falseString = falseString;
            this.checkFunction = checkFunction;
            this.keyCheckFunction = keyCheckFunction;
        }

        public override string Name { get; }

        public override string NameLocalizationKey => $"{Name}SearchFilter";
        public override bool IsSet => usingTag || showTrue == false || showFalse == false;

        public override bool CheckFilter(Item item) {
            return checkFunction.Invoke(item, usingTag ? taggedTrue : showTrue, usingTag ? taggedFalse : showFalse);
        }
        
        public override bool CheckFilter(EventItem item) {
            return keyCheckFunction != null && keyCheckFunction.Invoke(item, usingTag ? taggedTrue : showTrue, usingTag ? taggedFalse : showFalse);
        }

        public override void DrawEditor() {
            ImGui.BeginChild($"BooleanSearchFilter-{NameLocalizationKey}-Editor", new Vector2(-1, 24 * ImGui.GetIO().FontGlobalScale), false, usingTag ? ImGuiWindowFlags.NoMouseInputs : ImGuiWindowFlags.None);
            var x = ImGui.GetCursorPosX();

            var t = usingTag ? taggedTrue : showTrue;
            var f = usingTag ? taggedFalse : showFalse;


            if (ImGui.Checkbox(trueString, ref t)) {
                if (!usingTag) {
                    showTrue = t;
                    if (!showTrue) showFalse = true;
                    Modified = true;
                }
            }

            ImGui.SameLine();

            var x2 = ImGui.GetCursorPosX() - x;
            if (x2 > _trueWidth) {
                _trueWidth = x2;
            }

            ImGui.SetCursorPosX(x + _trueWidth);

            if (ImGui.Checkbox(falseString, ref f)) {
                if (!usingTag) {
                    showFalse = f;
                    if (!showFalse) showTrue = true;
                    Modified = true;
                }
            }
            ImGui.EndChild();
        }

        public override string ToString() {
            return showTrue ? trueString : falseString;
        }

        public override void Hide() {
            _trueWidth = 0;
        }

        public override void ClearTags() {
            usingTag = false;
        }

        public override bool IsFromTag => usingTag;

        public override bool ParseTag(string tag) {
            var t = tag.ToLower().Trim();

            if (t == $"not {Name}".ToLower()) {
                taggedFalse = true;
                taggedTrue = false;
                usingTag = true;
                Modified = true;
                return true;
            }

            if (t == $"{Name}".ToLower()) {
                taggedFalse = false;
                taggedTrue = true;
                usingTag = true;
                Modified = true;
                return true;
            }

            return false;
        }
    }
}
