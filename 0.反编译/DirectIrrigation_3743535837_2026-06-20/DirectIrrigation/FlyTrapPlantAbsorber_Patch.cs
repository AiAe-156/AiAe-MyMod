using HarmonyLib;
using UnityEngine;

namespace DirectIrrigation;

[HarmonyPatch(typeof(FlyTrapPlantConfig), "CreatePrefab")]
internal static class FlyTrapPlantAbsorber_Patch
{
	private static void Postfix(ref GameObject __result)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		PassiveElementConsumer val = __result.AddComponent<PassiveElementConsumer>();
		((ElementConsumer)val).elementToConsume = (SimHashes)1832607973;
		((ElementConsumer)val).consumptionRate = 0.5f;
		((ElementConsumer)val).consumptionRadius = 1;
		((ElementConsumer)val).showDescriptor = false;
		((ElementConsumer)val).showInStatusPanel = false;
		((ElementConsumer)val).capacityKG = 100f;
		((ElementConsumer)val).storeOnConsume = true;
		((Behaviour)val).enabled = false;
		((ElementConsumer)val).sampleCellOffset = new Vector3(0f, 2f, 0f);
	}
}
