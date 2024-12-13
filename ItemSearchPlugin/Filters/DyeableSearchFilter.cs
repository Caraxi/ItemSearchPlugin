using System.Linq;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace ItemSearchPlugin.Filters;

internal class DyeableSearchFilter : SearchFilter {
    const int MaxDyes = 2;
    private readonly bool[] toggles = new bool[MaxDyes + 1];
    
    public DyeableSearchFilter(ItemSearchPluginConfig config) : base(config) {
        for (var i = 0; i < toggles.Length; i++) toggles[i] = true;
    }

    public override string Name => "Dyeable";
    public override string NameLocalizationKey => "DyeableSearchFilter";

    public override bool IsSet => toggles.Any(a => a == false);

    public override void DrawEditor() {
        for (var i = 0; i < toggles.Length; i++) {
            if (i != 0) ImGui.SameLine();
            if (ImGui.Checkbox($"{i}##ToggleDyeable{i}", ref toggles[i])) {
                Modified = true;
            }
        }
    }
    
    public override bool CheckFilter(Item item) {
        if (item.DyeCount < toggles.Length) return toggles[item.DyeCount];
        return true;
    }
}
