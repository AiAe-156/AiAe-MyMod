using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using STRINGS;
using UnityEngine;

namespace ConstantTemperatureCooler;

public static class AirConditionerPatches
{
    // ==========================================
    // 添加 AirConditionerAdjustable 组件到两种空调
    // ==========================================
    [HarmonyPatch(typeof(AirConditionerConfig), "ConfigureBuildingTemplate")]
    private static class Patch_AirConditionerConfig_ConfigureBuildingTemplate
    {
        public static void Postfix(GameObject go) =>
            EntityTemplateExtensions.AddOrGet<AirConditionerAdjustable>(go);
    }

    [HarmonyPatch(typeof(LiquidConditionerConfig), "ConfigureBuildingTemplate")]
    private static class Patch_LiquidConditionerConfig_ConfigureBuildingTemplate
    {
        public static void Postfix(GameObject go) =>
            EntityTemplateExtensions.AddOrGet<AirConditionerAdjustable>(go);
    }

    // ==========================================
    // 设置最大功率上限
    // ==========================================
    [HarmonyPatch(typeof(AirConditionerConfig), "CreateBuildingDef")]
    private static class Patch_AirConditionerConfig_CreateBuildingDef
    {
        public static void Postfix(BuildingDef __result) =>
            __result.EnergyConsumptionWhenActive = 340f;
    }

    [HarmonyPatch(typeof(LiquidConditionerConfig), "CreateBuildingDef")]
    private static class Patch_LiquidConditionerConfig_CreateBuildingDef
    {
        public static void Postfix(BuildingDef __result) =>
            __result.EnergyConsumptionWhenActive = 1700f;
    }

    // ==========================================
    // 修复 CapacityControlSideScreen 标题
    // 仅当目标是我们的空调时将标题改为"目标温度"
    // ==========================================
    [HarmonyPatch(typeof(CapacityControlSideScreen), "SetTarget")]
    private static class Patch_CapacityControlSideScreen_SetTarget
    {
        private static readonly FieldInfo TitleKeyField =
            typeof(SideScreenContent).GetField("titleKey", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            ?? typeof(SideScreenContent).GetField("_titleKey", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        public static void Postfix(CapacityControlSideScreen __instance, GameObject new_target)
        {
            if (new_target == null) return;
            if (new_target.GetComponent<AirConditionerAdjustable>() == null) return;

            // 将标题字段改为"目标温度"的 key
            TitleKeyField?.SetValue(__instance, "STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.TARGET_TITLE");
        }
    }

    // ==========================================
    // 调整 SideScreen 面板顺序:
    // 让 CapacityControlSideScreen 排在 MultiSliderSideScreen 之前
    // (视觉上目标温度在上，调控限制在下)
    // ==========================================
    [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
    private static class Patch_DetailsScreen_ReorderSideScreens
    {
        private static readonly FieldInfo SideScreensField =
            typeof(DetailsScreen).GetField("sideScreens", BindingFlags.Instance | BindingFlags.NonPublic);

        // SideScreenRef 是 DetailsScreen 的内部类，需用反射获取 screenPrefab
        private static readonly FieldInfo ScreenPrefabField =
            typeof(DetailsScreen).GetNestedType("SideScreenRef", BindingFlags.NonPublic | BindingFlags.Public)
                                 ?.GetField("screenPrefab", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        public static void Postfix(DetailsScreen __instance)
        {
            if (SideScreensField == null || ScreenPrefabField == null) return;

            var screens = SideScreensField.GetValue(__instance) as System.Collections.IList;
            if (screens == null) return;

            int capacityIdx = -1;
            int multiSliderIdx = -1;

            for (int i = 0; i < screens.Count; i++)
            {
                var prefab = ScreenPrefabField.GetValue(screens[i]) as SideScreenContent;
                if (prefab is CapacityControlSideScreen) capacityIdx = i;
                else if (prefab is MultiSliderSideScreen) multiSliderIdx = i;
            }

            // 若 MultiSlider 在 Capacity 前面，则交换（让目标温度在上，调控限制在下）
            if (capacityIdx >= 0 && multiSliderIdx >= 0 && multiSliderIdx < capacityIdx)
            {
                var tmp = screens[capacityIdx];
                screens[capacityIdx] = screens[multiSliderIdx];
                screens[multiSliderIdx] = tmp;
            }
        }
    }


    // ==========================================
    // 核心：替换 AirConditioner.UpdateState，使用动态 maxDelta
    // ==========================================
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
                throw new InvalidOperationException("未找到 UpdateStateCbDelegate 字段");
            return _updateStateCbDelegateField.GetValue(null) as Func<int, object, bool>;
        }

        static Patch_AirConditioner_UpdateState()
        {
            _updateStateCbDelegateField = typeof(AirConditioner).GetField("UpdateStateCbDelegate", BindingFlags.Static | BindingFlags.NonPublic);
            _updateStatusMethod = typeof(AirConditioner).GetMethod("UpdateStatus", BindingFlags.Instance | BindingFlags.NonPublic);
            _setlastEnvTemp = typeof(AirConditioner).GetMethod("set_lastEnvTemp", BindingFlags.Instance | BindingFlags.NonPublic);
            _setlastGasTemp = typeof(AirConditioner).GetMethod("set_lastGasTemp", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static bool Prefix(
            AirConditioner __instance,
            ref ConduitConsumer ___consumer,
            ref float ___targetTemperature,
            ref float ___envTemp,
            ref int ___cellCount,
            ref OccupyArea ___occupyArea,
            ref Storage ___storage,
            ref float ___lowTempLag,
            ref bool ___showingLowTemp,
            ref bool ___isLiquidConditioner,
            ref float ___temperatureDelta,
            ref float ___lastSampleTime,
            ref KBatchedAnimHeatPostProcessingEffect ___heatEffect,
            ref HandleVector<int>.Handle ___structureTemperature,
            ref Operational ___operational,
            ref int ___cooledAirOutputCell,
            ref float dt)
        {
            AirConditionerAdjustable adjustable = ((Component)__instance).GetComponent<AirConditionerAdjustable>();
            float effectiveMaxDelta = adjustable != null ? adjustable.GetEffectiveMaxDelta() : 20f;
            float negMaxDelta = -effectiveMaxDelta;

            bool flag = ___consumer.IsSatisfied;
            ___envTemp = 0f;
            ___cellCount = 0;

            if (___occupyArea != null && ((Component)__instance).gameObject != null)
            {
                ___occupyArea.TestArea(Grid.PosToCell(((Component)__instance).gameObject), __instance, GetDelegate());
                ___envTemp /= ___cellCount;
            }

            _setlastEnvTemp.Invoke(__instance, new object[] { ___envTemp });

            List<GameObject> items = ___storage.items;
            for (int i = 0; i < items.Count; i++)
            {
                PrimaryElement component = items[i].GetComponent<PrimaryElement>();
                if (component.Mass > 0f
                    && (!___isLiquidConditioner || !component.Element.IsGas)
                    && (___isLiquidConditioner || !component.Element.IsLiquid))
                {
                    flag = true;
                    _setlastGasTemp.Invoke(__instance, new object[] { component.Temperature });

                    float outputTemp;
                    if (___targetTemperature < component.Temperature)
                    {
                        ___temperatureDelta = Math.Max(___targetTemperature - component.Temperature, negMaxDelta);
                        outputTemp = Math.Max(component.Temperature + negMaxDelta, ___targetTemperature);
                    }
                    else
                    {
                        ___temperatureDelta = 0f;
                        outputTemp = component.Temperature;
                    }

                    if (outputTemp < 1f)
                    {
                        outputTemp = 1f;
                        ___lowTempLag = Mathf.Min(___lowTempLag + dt / 5f, 1f);
                    }
                    else
                    {
                        ___lowTempLag = Mathf.Min(___lowTempLag - dt / 5f, 0f);
                    }

                    float addedMass = (___isLiquidConditioner ? Game.Instance.liquidConduitFlow : Game.Instance.gasConduitFlow)
                        .AddElement(___cooledAirOutputCell, component.ElementID, component.Mass, outputTemp, component.DiseaseIdx, component.DiseaseCount);

                    component.KeepZeroMassObject = true;
                    float ratio = addedMass / component.Mass;
                    int diseaseRemoved = (int)(component.DiseaseCount * ratio);
                    component.Mass -= addedMass;
                    component.ModifyDiseaseCount(-diseaseRemoved, "AirConditioner.UpdateState");

                    float energyTransferred = (outputTemp - component.Temperature) * component.Element.specificHeatCapacity * addedMass;
                    float timeDelta = (___lastSampleTime > 0f) ? (Time.time - ___lastSampleTime) : 1f;
                    ___lastSampleTime = Time.time;

                    ___heatEffect.SetHeatBeingProducedValue(Mathf.Abs(energyTransferred));
                    GameComps.StructureTemperatures.ProduceEnergy(
                        ___structureTemperature, -energyTransferred,
                        BUILDING.STATUSITEMS.OPERATINGENERGY.PIPECONTENTS_TRANSFER, timeDelta);

                    break;
                }
            }

            if (Time.time - ___lastSampleTime > 2f)
            {
                GameComps.StructureTemperatures.ProduceEnergy(
                    ___structureTemperature, 0f,
                    BUILDING.STATUSITEMS.OPERATINGENERGY.PIPECONTENTS_TRANSFER,
                    Time.time - ___lastSampleTime);
                ___lastSampleTime = Time.time;
            }

            ___operational.SetActive(flag, false);
            _updateStatusMethod.Invoke(__instance, null);
            return false;
        }
    }
}
