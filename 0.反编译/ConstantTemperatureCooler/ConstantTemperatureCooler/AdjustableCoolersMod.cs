using ConstantTemperatureCooler.STRINGS;
using HarmonyLib;
using KMod;

namespace ConstantTemperatureCooler;

public class AdjustableCoolersMod : UserMod2
{
	public override void OnLoad(Harmony harmony)
	{
		((UserMod2)this).OnLoad(harmony);
		LocString.CreateLocStringKeys(typeof(UI), "STRINGS.");
	}
}
