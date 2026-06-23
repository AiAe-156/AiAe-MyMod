using HarmonyLib;
using KMod;
using PeterHan.PLib.Options;
using UnityEngine;

namespace DirectIrrigationAdjacency
{
    public sealed class Mod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            new POptions().RegisterOptions(this, typeof(ModOptions));
            AdjacencySupply.Options = POptions.ReadSettings<ModOptions>() ?? new ModOptions();
            harmony.PatchAll();
            ElementConsumerExtender.Register();
            Debug.Log("[DirectIrrigationAdjacency] Loaded");
        }
    }
}
