using HarmonyLib;
using UnityEngine;

namespace DirectIrrigation;

[HarmonyPatch(typeof(Instance), "SetStorage")]
internal static class IrrigationMonitor_Instance_SetStorage_Patch_LiquidPEC
{
	private static void Postfix(Instance __instance, object obj)
	{
		Storage val = (Storage)((obj is Storage) ? obj : null);
		if (!((Object)(object)val == (Object)null) && __instance != null)
		{
			LiquidConsumerWireUtil.WireLiquidPECs(((Instance)__instance).gameObject, val);
		}
	}
}
