using HarmonyLib;
using KMod;

namespace ConstantTemperatureCooler;

public class AdjustableCoolersMod : UserMod2
{
    public override void OnLoad(Harmony harmony)
    {
        base.OnLoad(harmony);
        LocString.CreateLocStringKeys(typeof(STRINGS.UI), "STRINGS.");
    }
}
