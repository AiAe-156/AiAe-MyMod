using HarmonyLib;
using UnityEngine;

namespace DirectIrrigation;

[HarmonyPatch(typeof(TubeWormConfig), "CreatePrefab")]
internal static class TubeWormAbsorber_Patch
{
	private static void Postfix(ref GameObject __result)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		PassiveElementConsumer val = __result.AddComponent<PassiveElementConsumer>();
		((ElementConsumer)val).elementToConsume = (SimHashes)(-2044124200);
		((ElementConsumer)val).consumptionRate = 0.5f;
		((ElementConsumer)val).consumptionRadius = 2;
		((ElementConsumer)val).showDescriptor = false;
		((ElementConsumer)val).showInStatusPanel = false;
		((ElementConsumer)val).capacityKG = 100f;
		((ElementConsumer)val).storeOnConsume = true;
		((Behaviour)val).enabled = false;
	}
}
