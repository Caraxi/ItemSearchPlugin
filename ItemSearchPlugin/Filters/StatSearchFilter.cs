using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Lumina.Excel.Sheets;
using static ItemSearchPlugin.Filters.PatchSearchFilter;

namespace ItemSearchPlugin.Filters {
    internal class StatSearchFilter : SearchFilter {
        public class Stat {
            public BaseParam BaseParam;
            public int BaseParamIndex;
        }

        private BaseParam[] baseParams;

        private bool modeAny;


        public List<Stat> Stats = new List<Stat>();

        private Dictionary<string, string> StatAlias = new Dictionary<string, string>() {
            { "str", "strength" },
            { "dex", "dexterity" },
            { "vit", "vitality" },
            { "int", "intelligence" },
            { "crit", "critical hit" },
            { "det", "determination" },
            { "dh", "direct hit rate" },
            { "def", "defence" },
            { "mdef", "magic defence" },
            { "sks", "skill speed" },
            { "sps", "spell speed" },
            { "ten", "tenacity" },
        };

        public StatSearchFilter(ItemSearchPluginConfig config, IDataManager data) : base(config) {
            Task.Run(() => {
                var baseParamCounts = new Dictionary<uint, int>();

                foreach (var p in data.GetExcelSheet<Item>().ToList().SelectMany(i => i.BaseParam)) {
                    if (!baseParamCounts.ContainsKey(p.RowId)) {
                        baseParamCounts.Add(p.RowId, 0);
                    }

                    baseParamCounts[p.RowId] += 1;
                }

                var sheet = data.GetExcelSheet<BaseParam>();
                baseParams = baseParamCounts.OrderBy(p => p.Value).Reverse().Select(pair => sheet.GetRow(pair.Key)).ToArray();
            });
        }

        public override string Name => "Has Stats";
        public override string NameLocalizationKey => "StatSearchFilter";
        public override bool IsSet => Stats.Count > 0 && Stats.Any(s => s.BaseParam.RowId != 0);

        public override bool CheckFilter(Item item) {
            if (baseParams == null) return true;
            if (modeAny) {
                // Match Any
                foreach (var s in Stats.Where(s => s.BaseParam.RowId != 0)) {
                    foreach (var p in item.BaseParam) {
                        if (p.RowId == s.BaseParam.RowId) {
                            return true;
                        }
                    }
                }

                return false;
            } else {
                // Match All

                foreach (var s in Stats.Where(s => s.BaseParam.RowId != 0)) {
                    bool foundMatch = false;
                    foreach (var p in item.BaseParam) {
                        if (p.RowId == s.BaseParam.RowId) {
                            foundMatch = true;
                        }
                    }

                    if (!foundMatch) {
                        return false;
                    }
                }
            }

            return true;
        }

        public override void DrawEditor() {
            var btnSize = new Vector2(24 * ImGui.GetIO().FontGlobalScale);

            if (baseParams == null) {
                // Still loading
                ImGui.Text("");
                return;
            }


            Stat doRemove = null;
            var i = 0;
            foreach (var stat in Stats) {
                if (!usingTags && ImGui.Button($"-###statSearchFilterRemove{i++}", btnSize)) doRemove = stat;
                var selectedParam = stat.BaseParamIndex;
                ImGui.SetNextItemWidth(200);

                if (usingTags) {
                    string str = stat.BaseParam.Name.ToString();
                    ImGui.InputText($"###statSearchFilterSelectStat{i++}", ref str, 50, ImGuiInputTextFlags.ReadOnly);
                } else {
                    ImGui.SameLine();
                    if (ImGui.Combo($"###statSearchFilterSelectStat{i++}", ref selectedParam, baseParams.Select(bp => bp.RowId == 0 ? Loc.Localize("StatSearchFilterSelectStat", "Select a stat...") : bp.Name.ToString()).ToArray(), baseParams.Length, 20)) {
                        stat.BaseParamIndex = selectedParam;
                        stat.BaseParam = baseParams[selectedParam];
                        Modified = true;
                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        doRemove = stat;
                    }
                }
            }

            if (doRemove != null) {
                Stats.Remove(doRemove);
                Modified = true;
            }

            if (!usingTags && ImGui.Button("+###StatSearchFilterPlus", btnSize)) {
                var stat = new Stat();
                Stats.Add(stat);
                Modified = true;
            }

            Stats = Stats.DistinctBy(p => p.BaseParam).OrderByDescending(p => p.BaseParamIndex).ToList();

            if (Stats.Count > 1) {
                ImGui.SameLine();
                if (ImGui.Checkbox($"{Loc.Localize("StatSearchFilterMatchAny", "Match Any")}###StatSearchFilterShowAny", ref modeAny)) {
                    Modified = true;
                }
            }
        }

        private bool usingTags = false;

        private List<Stat> nonTagStats;

        public override void ClearTags() {
            if (usingTags) {
                Stats = nonTagStats;
                usingTags = false;
            }
        }

        public override bool IsFromTag => usingTags;

        public override bool ParseTag(string tag) {
            var t = tag.ToLower().Trim();
            if (StatAlias.ContainsKey(t)) t = StatAlias[t];
            foreach (var bp in baseParams) {
                if (bp.Name.ToString().ToLower() == t) {
                    var stat = new Stat() { BaseParam = bp };

                    if (!usingTags) {
                        nonTagStats = Stats;
                        usingTags = true;
                        Stats = new List<Stat>();
                    }

                    Stats.Add(stat);
                }
            }

            return false;
        }

        public override string ToString() {
            return string.Join(", ", Stats.Select(s => s.BaseParam.Name));
        }
    }
}
