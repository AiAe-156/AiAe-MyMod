using HarmonyLib;
using UnityEngine;

namespace DirectIrrigation;

[HarmonyPatch(typeof(ForestTreeConfig), "CreatePrefab")]
internal static class ForestTreeConfig_CreatePrefab_PressurePatch
{
	private static void Postfix(ref GameObject __result)
	{
		if (!((Object)(object)__result == (Object)null))
		{
			PressureVulnerable component = __result.GetComponent<PressureVulnerable>();
			if (!((Object)(object)component == (Object)null))
			{
				component.pressure_sensitive = true;
				component.pressureWarning_High *= 10f;
				component.pressureLethal_High *= 10f;
			}
		}
	}
}
