using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using STRINGS;
using UnityEngine;

namespace ConstantTemperatureCooler;

public static class AirConditionerPatches
{
	[HarmonyPatch(typeof(AirConditionerConfig), "ConfigureBuildingTemplate")]
	private static class Patch_AirConditionerConfig_ConfigureBuildingTemplate
	{
		public static void Postfix(GameObject go)
		{
			EntityTemplateExtensions.AddOrGet<AirConditionerAdjustable>(go);
		}
	}

	[HarmonyPatch(typeof(LiquidConditionerConfig), "ConfigureBuildingTemplate")]
	private static class Patch_LiquidConditionerConfig_ConfigureBuildingTemplate
	{
		public static void Postfix(GameObject go)
		{
			EntityTemplateExtensions.AddOrGet<AirConditionerAdjustable>(go);
		}
	}

	[HarmonyPatch(typeof(AirConditionerConfig), "CreateBuildingDef")]
	private static class Patch_AirConditionerConfig_CreateBuildingDef
	{
		public static void Postfix(BuildingDef __result)
		{
			__result.EnergyConsumptionWhenActive = 340f;
		}
	}

	[HarmonyPatch(typeof(LiquidConditionerConfig), "CreateBuildingDef")]
	private static class Patch_LiquidConditionerConfig_CreateBuildingDef
	{
		public static void Postfix(BuildingDef __result)
		{
			__result.EnergyConsumptionWhenActive = 1700f;
		}
	}

	[HarmonyPatch(typeof(AirConditioner), "UpdateState")]
	private static class Patch_AirConditioner_UpdateState
	{
		private static MethodInfo _updateStatusMethod;

		private static MethodInfo _setlastEnvTemp;

		private static MethodInfo _setlastGasTemp;

		private static readonly FieldInfo _updateStateCbDelegateField;

		private static Func<int, object, bool> GetDelegate()
		{
			if (_updateStateCbDelegateField == null)
			{
				throw new InvalidOperationException("未找到 UpdateStateCbDelegate 字段");
			}
			return _updateStateCbDelegateField.GetValue(null) as Func<int, object, bool>;
		}

		static Patch_AirConditioner_UpdateState()
		{
			_updateStateCbDelegateField = typeof(AirConditioner).GetField("UpdateStateCbDelegate", BindingFlags.Static | BindingFlags.NonPublic);
			_updateStatusMethod = typeof(AirConditioner).GetMethod("UpdateStatus", BindingFlags.Instance | BindingFlags.NonPublic);
			_setlastEnvTemp = typeof(AirConditioner).GetMethod("set_lastEnvTemp", BindingFlags.Instance | BindingFlags.NonPublic);
			_setlastGasTemp = typeof(AirConditioner).GetMethod("set_lastGasTemp", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public static bool Prefix(AirConditioner __instance, ref ConduitConsumer ___consumer, ref float ___targetTemperature, ref float ___envTemp, ref int ___cellCount, ref OccupyArea ___occupyArea, ref Storage ___storage, ref float ___lowTempLag, ref bool ___showingLowTemp, ref bool ___isLiquidConditioner, ref float ___temperatureDelta, ref float ___lastSampleTime, ref KBatchedAnimHeatPostProcessingEffect ___heatEffect, ref Handle<int> ___structureTemperature, ref Operational ___operational, ref int ___cooledAirOutputCell, ref float dt)
		{
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
			bool flag = ___consumer.IsSatisfied;
			___envTemp = 0f;
			___cellCount = 0;
			if ((Object)(object)___occupyArea != (Object)null && (Object)(object)((Component)__instance).gameObject != (Object)null)
			{
				___occupyArea.TestArea(Grid.PosToCell(((Component)__instance).gameObject), (object)__instance, GetDelegate());
				___envTemp /= ___cellCount;
			}
			object[] parameters = new object[1] { ___envTemp };
			_setlastEnvTemp.Invoke(__instance, parameters);
			List<GameObject> items = ___storage.items;
			for (int i = 0; i < items.Count; i++)
			{
				PrimaryElement component = items[i].GetComponent<PrimaryElement>();
				if (component.Mass > 0f && (!___isLiquidConditioner || !component.Element.IsGas) && (___isLiquidConditioner || !component.Element.IsLiquid))
				{
					flag = true;
					object[] parameters2 = new object[1] { component.Temperature };
					_setlastGasTemp.Invoke(__instance, parameters2);
					float num;
					if (___targetTemperature < component.Temperature)
					{
						___temperatureDelta = Math.Max(___targetTemperature - component.Temperature, -20f);
						num = Math.Max(component.Temperature + -20f, ___targetTemperature);
					}
					else
					{
						___temperatureDelta = 0f;
						num = component.Temperature;
					}
					Debug.Log((object)$"{component.Temperature} {___temperatureDelta} {num} {___targetTemperature}");
					if (num < 1f)
					{
						num = 1f;
						___lowTempLag = Mathf.Min(___lowTempLag + dt / 5f, 1f);
					}
					else
					{
						___lowTempLag = Mathf.Min(___lowTempLag - dt / 5f, 0f);
					}
					float num2 = (___isLiquidConditioner ? Game.Instance.liquidConduitFlow : Game.Instance.gasConduitFlow).AddElement(___cooledAirOutputCell, component.ElementID, component.Mass, num, component.DiseaseIdx, component.DiseaseCount);
					component.KeepZeroMassObject = true;
					float num3 = num2 / component.Mass;
					int num4 = (int)((float)component.DiseaseCount * num3);
					component.Mass -= num2;
					component.ModifyDiseaseCount(-num4, "AirConditioner.UpdateState");
					float num5 = (num - component.Temperature) * component.Element.specificHeatCapacity * num2;
					float num6 = ((___lastSampleTime > 0f) ? (Time.time - ___lastSampleTime) : 1f);
					___lastSampleTime = Time.time;
					___heatEffect.SetHeatBeingProducedValue(Mathf.Abs(num5));
					GameComps.StructureTemperatures.ProduceEnergy(___structureTemperature, 0f - num5, LocString.op_Implicit(OPERATINGENERGY.PIPECONTENTS_TRANSFER), num6);
					break;
				}
			}
			if (Time.time - ___lastSampleTime > 2f)
			{
				GameComps.StructureTemperatures.ProduceEnergy(___structureTemperature, 0f, LocString.op_Implicit(OPERATINGENERGY.PIPECONTENTS_TRANSFER), Time.time - ___lastSampleTime);
				___lastSampleTime = Time.time;
			}
			___operational.SetActive(flag, false);
			_updateStatusMethod.Invoke(__instance, null);
			return false;
		}
	}

	public const float ENERGY_MODIFIER = 1.4285715f;

	public const float MIN_TEMPERATURE_DELTA = -20f;
}
