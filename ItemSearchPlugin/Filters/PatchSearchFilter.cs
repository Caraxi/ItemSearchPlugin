using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;

namespace ItemSearchPlugin.Filters
{
    class PatchSearchFilter : SearchFilter
    {
        private readonly IDalamudPluginInterface pluginInterface;
        private bool Altered = false;
        private List<Patch> selectedPatches;

        public class Patch
        {
            public int Id; // Auto-incrementing ID
            public int Index; // Original ID field
            public string Name;
            public string ShortName;
            public bool Expansion = false;
        }


        // List of patches under shared keys (by expansion name and shorthand)
        private static readonly List<Patch> ExpansionsPatches = new List<Patch>
        {
            // Final Fantasy XIV (1.0)
            new Patch { Id = 0, Index = 0, Name = "Select a patch...", ShortName = "999"},

            // Final Fantasy XIV (1.0)
            new Patch { Id = 1, Index = 10, Name = "Final Fantasy XIV", ShortName = "1.0", Expansion = false },

            // A Realm Reborn (ARR)
            new Patch { Id = 2, Index = 20, Name = "A Realm Reborn", ShortName = "ARR", Expansion = true },
            new Patch { Id = 3, Index = 21, Name = "A Realm Reborn", ShortName = "2.0" },
            new Patch { Id = 4, Index = 22, Name = "A Realm Awoken", ShortName = "2.1" },
            new Patch { Id = 5, Index = 23, Name = "Through the Maelstrom", ShortName = "2.2" },
            new Patch { Id = 6, Index = 24, Name = "Defenders of Eorzea", ShortName = "2.3" },
            new Patch { Id = 7, Index = 25, Name = "Dreams of Ice", ShortName = "2.4" },
            new Patch { Id = 8, Index = 26, Name = "Before the Fall", ShortName = "2.5" },

            // Heavensward (HW)
            new Patch { Id = 9, Index = 30, Name = "Heavensward", ShortName = "HW", Expansion = true },
            new Patch { Id = 10, Index = 31, Name = "Heavensward", ShortName = "3.0" },
            new Patch { Id = 11, Index = 32, Name = "As Goes Light, So Goes Darkness", ShortName = "3.1" },
            new Patch { Id = 12, Index = 33, Name = "The Gears of Change", ShortName = "3.2" },
            new Patch { Id = 13, Index = 34, Name = "Revenge of the Horde", ShortName = "3.3" },
            new Patch { Id = 14, Index = 35, Name = "Soul Surrender", ShortName = "3.4" },
            new Patch { Id = 15, Index = 36, Name = "The Far Edge of Fate", ShortName = "3.5" },

            // Stormblood (SB)
            new Patch { Id = 16, Index = 40, Name = "Stormblood", ShortName = "SB", Expansion = true },
            new Patch { Id = 17, Index = 41, Name = "Stormblood", ShortName = "4.0" },
            new Patch { Id = 18, Index = 42, Name = "The Legend Returns", ShortName = "4.1" },
            new Patch { Id = 19, Index = 43, Name = "Rise of a New Sun", ShortName = "4.2" },
            new Patch { Id = 20, Index = 44, Name = "Under the Moonlight", ShortName = "4.3" },
            new Patch { Id = 21, Index = 45, Name = "Prelude in Violet", ShortName = "4.4" },
            new Patch { Id = 22, Index = 46, Name = "A Requiem for Heroes", ShortName = "4.5" },

            // Shadowbringers (ShB)
            new Patch { Id = 23, Index = 50, Name = "Shadowbringers", ShortName = "ShB", Expansion = true },
            new Patch { Id = 24, Index = 51, Name = "Shadowbringers", ShortName = "5.0" },
            new Patch { Id = 25, Index = 52, Name = "Vows of Virtue, Deeds of Cruelty", ShortName = "5.1" },
            new Patch { Id = 26, Index = 53, Name = "Echoes of a Fallen Star", ShortName = "5.2" },
            new Patch { Id = 27, Index = 54, Name = "Reflections in Crystal", ShortName = "5.3" },
            new Patch { Id = 28, Index = 55, Name = "Futures Rewritten", ShortName = "5.4" },
            new Patch { Id = 29, Index = 56, Name = "Death Unto Dawn", ShortName = "5.5" },

            // Endwalker (EW)
            new Patch { Id = 30, Index = 60, Name = "Endwalker", ShortName = "EW", Expansion = true },
            new Patch { Id = 31, Index = 61, Name = "Endwalker", ShortName = "6.0" },
            new Patch { Id = 32, Index = 62, Name = "Newfound Adventure", ShortName = "6.1" },
            new Patch { Id = 33, Index = 63, Name = "Buried Memory", ShortName = "6.2" },
            new Patch { Id = 34, Index = 64, Name = "Gods Revel, Lands Tremble", ShortName = "6.3" },
            new Patch { Id = 35, Index = 65, Name = "The Dark Throne", ShortName = "6.4" },
            new Patch { Id = 36, Index = 66, Name = "Growing Light", ShortName = "6.5" },

            // Dawntrail (DT)
            new Patch { Id = 37, Index = 70, Name = "Dawntrail", ShortName = "DT", Expansion = true },
            new Patch { Id = 38, Index = 71, Name = "Dawntrail", ShortName = "7.0" },
            new Patch { Id = 39, Index = 72, Name = "Crossroads", ShortName = "7.1" },
            new Patch { Id = 40, Index = 73, Name = "Seekers of Eternity", ShortName = "7.2" },
            new Patch { Id = 41, Index = 74, Name = "Promises of Tomorrow", ShortName = "7.3" },
            new Patch { Id = 42, Index = 75, Name = "Into the Mist", ShortName = "7.4" }
        };



        public PatchSearchFilter(ItemSearchPluginConfig config) : base(config)
        {
            this.selectedPatches = new();
        }
        public override string Name => "From Patch";
        public override string NameLocalizationKey => "PatchSearchFilter";


        public override bool IsSet => selectedPatches.Count() >= 1 && selectedPatches.FirstOrDefault().Index != 0;

        public override bool HasChanged
        {
            get
            {
                if (Altered)
                {
                    Altered = false;
                    return true;
                }

                return false;
            }
        }

        public override bool CheckFilter(Item item)
        {
            {
                foreach (Patch selectedPatch in selectedPatches)
                {
                    string stringTruePatch = ClassExtensions.GetPatch(item.RowId);
                    string stringPatch = stringTruePatch.Length > 3 ? stringTruePatch.Substring(0, 3) : stringTruePatch;
#if DEBUG
                    //Patch patch = ExpansionsPatches.FirstOrDefault(p => p.ShortName == stringPatch);
                    //if (patch.Expansion) selectedPatch.ShortName.Substring(0, 1);
                    //PluginLog.Debug($"{ClassExtensions.GetPatch(item.RowId)} - {stringTruePatch} -> {selectedPatch.ShortName} = {patch.ShortName}");
#endif
                    if (stringPatch == selectedPatch.ShortName && selectedPatches.Contains(selectedPatch)) return true;
                }
            }
            return false;
        }

        public override void DrawEditor()
        {
            var btnSize = new Vector2(24 * ImGui.GetIO().FontGlobalScale);

            if (selectedPatches == null)
            {
                // Still loading
                ImGui.Text("");
                return;
            }

            Patch doRemove = null;
            List<Patch> tempPatchList = new List<Patch>();
            var i = 0;

            foreach (var patch in selectedPatches)
            {
                if (ImGui.Button($"-###PatchSearchFilterRemove{i++}", btnSize))
                {
                    doRemove = patch;
                }
                var selectedParam = patch.Id;
                ImGui.SetNextItemWidth(200);
                
                ImGui.SameLine();

                if (ImGui.Combo(
                $"###PatchSearchFilterSelectStat{i++}",
                ref selectedParam,
                ExpansionsPatches.Select(p => p.Id == 0 ? Loc.Localize("PatchSearchFilterSelectStat", "Select a patch...")
                : $"{p.Name} ({p.ShortName})".ToString()).ToArray(),
                ExpansionsPatches.Count()))
                    {
                    Patch tempPatch = ExpansionsPatches.First(p => p.Id == selectedParam);
                    // 1.0 is an exception and doesn't count as its own expansion.
                    if (tempPatch.Expansion == true)
                    {
                        foreach (Patch extensionPatch in ExpansionsPatches.Where(p => p.Index.ToString().Substring(0,1) == tempPatch.Index.ToString().Substring(0,1)))
                        {
                            Patch newPatch = new Patch();
                            newPatch.Id = extensionPatch.Id;
                            newPatch.Index = extensionPatch.Index;
                            newPatch.Name = extensionPatch.Name;
                            newPatch.ShortName = extensionPatch.ShortName;
                            newPatch.Expansion = extensionPatch.Expansion;
                            if (selectedPatches.Contains(newPatch)) continue;
                            tempPatchList.Add(newPatch);
                        }
                    }
                    else
                    {
                        if (selectedPatches.Contains(tempPatch)) continue;
                        patch.Id = tempPatch.Id;
                        patch.Index = tempPatch.Index;
                        patch.Name = tempPatch.Name;
                        patch.ShortName = tempPatch.ShortName;
                        patch.Expansion = tempPatch.Expansion;
                    }
                    Altered = true;
                    //if(!selectedPatches.Contains(tempPatch)) selectedPatches.Add(tempPatch);
                }
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    doRemove = patch;
                }
            }

            if(tempPatchList.Count > 0)
            {
                foreach (Patch patch in tempPatchList)
                {
                    selectedPatches.Add(patch);
                }
                Altered = true;
            }

            if (doRemove!= null)
            {
                selectedPatches.Remove(doRemove);
                Altered = true;
            }

            if(selectedPatches.RemoveAll(p => p.Expansion == true) > 0) selectedPatches.RemoveAll(p => p.Id == 0);
            selectedPatches = selectedPatches.DistinctBy(p => p.Name).OrderByDescending(p => p.ShortName).ToList();

            if (ImGui.Button("+###PatchSearchFilterPlus", btnSize)) {
                selectedPatches.Add(new Patch { Id = 0});
                Altered = true;
            }

            if (selectedPatches.Count > 1)
            {
                ImGui.SameLine();
            }
        }

    }
}