using HarmonyLib;
using KMod;
using UtilLibs;

namespace DirectIrrigation;

public class Mod : UserMod2
{
	public override void OnLoad(Harmony harmony)
	{
		((UserMod2)this).OnLoad(harmony);
		SgtLogger.LogVersion((UserMod2)(object)this, harmony);
	}
}
