using System;
using System.Linq;
using System.Numerics;
using Lumina.Excel.GeneratedSheets;
using ImGuiNET;

namespace ItemSearchPlugin.Filters {
    class LevelItemSearchFilter : SearchFilter {
        private int MinLevel = 1;
        private int MaxLevel = 600;

        private int minLevel;
        private int maxLevel;

        private int lastMinLevel;
        private int lastMaxLevel;

        public LevelItemSearchFilter(ItemSearchPluginConfig config) : base(config) {
            minLevel = lastMinLevel = MinLevel;
            maxLevel = lastMaxLevel = MaxLevel;
        }

        public override string Name => "Item Level";

        public override string NameLocalizationKey => "SearchFilterLevelItem";

        public override bool IsSet => usingTag || minLevel != MinLevel || maxLevel != MaxLevel;

        public override bool HasChanged {
            get {
                if (Modified || minLevel != lastMinLevel || maxLevel != lastMaxLevel) {
                    lastMaxLevel = maxLevel;
                    lastMinLevel = minLevel;
                    Modified = false;
                    return true;
                }

                return false;
            }
        }

        public override bool CheckFilter(Item item) {
            return item.LevelItem.Row >= (usingTag ? taggedMin : minLevel) && item.LevelItem.Row <= (usingTag ? taggedMax : maxLevel);
        }

        public override void DrawEditor() {
            ImGui.BeginChild($"{NameLocalizationKey}Child", new Vector2(-1, 23 * ImGui.GetIO().FontGlobalScale), false, usingTag ? ImGuiWindowFlags.NoInputs : ImGuiWindowFlags.None);

            ImGui.PushItemWidth(-1);
            var min = usingTag ? taggedMin : minLevel;
            var max = usingTag ? taggedMax : maxLevel;


            if (ImGui.DragIntRange2("##LevelItemSearchFilterRange", ref min, ref max, 1f, MinLevel, MaxLevel)) {
                if (!usingTag) {
                    minLevel = min;
                    maxLevel = max;
                    // Force ImGui to behave
                    // https://cdn.discordapp.com/attachments/653504487352303619/713825323967447120/ehS7GdAHKG.gif
                    if (minLevel > maxLevel && minLevel != lastMinLevel) minLevel = maxLevel;
                    if (maxLevel < minLevel && maxLevel != lastMaxLevel) maxLevel = minLevel;
                    if (minLevel < MinLevel) minLevel = MinLevel;
                    if (maxLevel > MaxLevel) maxLevel = MaxLevel;
                }
            }

            ImGui.PopItemWidth();

            ImGui.EndChild();

        }

        private bool usingTag = false;

        private int taggedMin;
        private int taggedMax;

        public override bool IsFromTag => usingTag;

        public override void ClearTags() {
            usingTag = false;
            taggedMin = 1;
            taggedMax = MaxLevel;
        }

        public override bool ParseTag(string tag) {

            var t = tag.ToLower().Trim();

            var tags = new string[] { "ilvl", "ilevel", "itemlevel", "item level", "itemlvl", "item lvl"};

            if (t.Contains(":")) {
                var k = t.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (k.Length >= 2 && tags.Contains(k[0])) {
                    t = k[1].Trim();
                    var s = t.Split('-');

                    if (s.Length > 0) {
                        var plus = false;
                        if (s.Length == 1 && s[0].EndsWith("+")) {
                            plus = true;
                            s[0] = s[0].Substring(0, s[0].Length - 1);
                        }
                        if (!int.TryParse(s[0], out var min)) {
                            return false;
                        }

                        int max;
                        if (s.Length == 1) {
                            max = plus ? MaxLevel : min;
                        } else {
                            if (!int.TryParse(s[1], out max)) {
                                return false;
                            }
                        }

                        if (max < min) {
                            var swap = max;
                            max = min;
                            min = swap;
                        }

                        taggedMax = max;
                        taggedMin = min;
                        usingTag = true;
                        Modified = true;

                    }
                }
            }

            return false;

        }

        public override string ToString() {
            return $"{(usingTag ? taggedMin : minLevel)} - {(usingTag ? taggedMax : maxLevel)}";
        }
    }
}
