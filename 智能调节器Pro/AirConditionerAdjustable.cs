using System;
using System.Reflection;
using KSerialization;
using UnityEngine;

namespace ConstantTemperatureCooler;

[SerializationConfig(MemberSerialization.OptIn)]
public class AirConditionerAdjustable : KMonoBehaviour, IUserControlledCapacity, IMultiSliderControl, ICheckboxControl
{
    private static readonly EventSystem.IntraObjectHandler<AirConditionerAdjustable> OnCopySettingsDelegate =
        new EventSystem.IntraObjectHandler<AirConditionerAdjustable>((Action<AirConditionerAdjustable, object>)OnCopySettings);

    [MyCmpReq] public AirConditioner airConditioner;
    [MyCmpReq] public EnergyConsumer energyConsumer;
    [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;

    [Serialize] private float targetTemperature = 0f;
    [Serialize] private float maxDelta = 20f;
    [Serialize] private float maxPower = -1f;
    [Serialize] private bool usePowerLimitMode = false;

    private ISliderControl[] _sliderControls;

    private const float WATER_SPECIFIC_HEAT = 4.179f;
    private const float EXAMPLE_FLOW_RATE = 10f;

    // ==========================================
    // IUserControlledCapacity (目标温度)
    // ==========================================
    public float UserMaxCapacity
    {
        get => GameUtil.GetTemperatureConvertedFromKelvin(targetTemperature, GameUtil.temperatureUnit);
        set
        {
            targetTemperature = GameUtil.GetTemperatureConvertedToKelvin(value);
            UpdateState();
        }
    }
    public float AmountStored => UserMaxCapacity;
    public bool ControlEnabled() => true;
    public float MinCapacity => GameUtil.GetTemperatureConvertedFromKelvin(0f, GameUtil.temperatureUnit);
    public float MaxCapacity => GameUtil.GetTemperatureConvertedFromKelvin(573.15f, GameUtil.temperatureUnit);
    public bool WholeValues => false;
    public LocString CapacityUnits => (LocString)GameUtil.GetTemperatureUnitSuffix();

    // ==========================================
    // IMultiSliderControl (调控限制: 最大降温 + 最大功率)
    // ==========================================
    public string SidescreenTitleKey => "STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.TITLE";
    public bool SidescreenEnabled() => true;
    public ISliderControl[] sliderControls => _sliderControls;

    // ==========================================
    // ICheckboxControl (功率限制模式)
    // ==========================================
    public string CheckboxTitleKey => "STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.CHECKBOX_TITLE";
    public string CheckboxLabel => STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.CHECKBOX_LABEL;
    public string CheckboxTooltip => STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.CHECKBOX_TOOLTIP;
    public bool GetCheckboxValue() => usePowerLimitMode;
    public void SetCheckboxValue(bool value)
    {
        usePowerLimitMode = value;
        UpdateState();
    }

    // ==========================================
    // 核心逻辑
    // ==========================================
    public float GetEffectiveMaxDelta()
    {
        if (usePowerLimitMode)
        {
            float wMax = GetBuildingMaxPower();
            if (wMax <= 0f) return 20f;
            return Mathf.Clamp(maxPower / wMax * 20f, 0.1f, 20f);
        }
        return Mathf.Clamp(maxDelta, 0.1f, 20f);
    }

    public float GetBuildingMaxPower() => energyConsumer.WattsNeededWhenActive;

    public float GetWattsConsumed() =>
        Mathf.Abs(GetBuildingMaxPower() * (airConditioner.temperatureDelta / 20f));

    public static string GetHeatTransferInfo(float effectiveDelta)
    {
        float kdtuPerSec = effectiveDelta * WATER_SPECIFIC_HEAT * EXAMPLE_FLOW_RATE;
        return $"\n\n以 {EXAMPLE_FLOW_RATE:F0} kg/s 水为例:\n最大可转移 {kdtuPerSec:F1} kDTU/s 的热量";
    }

    private static void SetTargetTemperatureDirect(AirConditioner instance, float value)
    {
        typeof(AirConditioner)
            .GetField("targetTemperature", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(instance, value);
    }

    private static void OnCopySettings(AirConditionerAdjustable comp, object data)
    {
        AirConditionerAdjustable other = ((GameObject)data).GetComponent<AirConditionerAdjustable>();
        if (other == null) return;
        comp.targetTemperature = other.targetTemperature;
        comp.maxDelta = other.maxDelta;
        comp.maxPower = other.maxPower;
        comp.usePowerLimitMode = other.usePowerLimitMode;
    }

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        Subscribe(-905833192, OnCopySettingsDelegate);
    }

    protected override void OnSpawn()
    {
        if (maxPower < 0f) maxPower = GetBuildingMaxPower();

        _sliderControls = new ISliderControl[]
        {
            new MaxDeltaSliderController(this),
            new MaxPowerSliderController(this)
        };

        UpdateState();
    }

    internal void UpdateState()
    {
        if (targetTemperature != airConditioner.TargetTemperature)
            SetTargetTemperatureDirect(airConditioner, targetTemperature);
        energyConsumer.BaseWattageRating = GetWattsConsumed();
    }

    // ==========================================
    // 滑块0: 最大降温幅度
    // ==========================================
    private class MaxDeltaSliderController : ISingleSliderControl, ISliderControl
    {
        private readonly AirConditionerAdjustable owner;
        public MaxDeltaSliderController(AirConditionerAdjustable o) { owner = o; }

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.MAXDELTA_TITLE";
        public string SliderUnits => "°C";
        public int SliderDecimalPlaces(int index) => 1;
        public float GetSliderMin(int index) => 1f;
        public float GetSliderMax(int index) => 20f;
        public float GetSliderValue(int index) => owner.maxDelta;

        public void SetSliderValue(float value, int index)
        {
            owner.maxDelta = Mathf.Clamp(value, 1f, 20f);
            owner.UpdateState();
        }

        public string GetSliderTooltipKey(int index) => "";
        public string GetSliderTooltip(int index)
        {
            float effectiveDelta = owner.GetEffectiveMaxDelta();
            float effectiveW = owner.GetBuildingMaxPower() * effectiveDelta / 20f;
            string status = owner.usePowerLimitMode
                ? STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.INACTIVE_SUFFIX
                : STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.ACTIVE_SUFFIX;

            return string.Format(STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.MAXDELTA_TOOLTIP,
                       owner.maxDelta.ToString("F1")) + status
                   + $"\n当前生效: 最大降温 {effectiveDelta:F1}°C, 最大功耗 {effectiveW:F0}W"
                   + GetHeatTransferInfo(effectiveDelta);
        }
    }

    // ==========================================
    // 滑块1: 最大功率限制
    // ==========================================
    private class MaxPowerSliderController : ISingleSliderControl, ISliderControl
    {
        private readonly AirConditionerAdjustable owner;
        public MaxPowerSliderController(AirConditionerAdjustable o) { owner = o; }

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.MAXPOWER_TITLE";
        public string SliderUnits => "W";
        public int SliderDecimalPlaces(int index) => 0;
        public float GetSliderMin(int index) => 1f;
        public float GetSliderMax(int index) => owner.GetBuildingMaxPower();
        public float GetSliderValue(int index) => owner.maxPower;

        public void SetSliderValue(float value, int index)
        {
            owner.maxPower = Mathf.Clamp(value, 1f, owner.GetBuildingMaxPower());
            owner.UpdateState();
        }

        public string GetSliderTooltipKey(int index) => "";
        public string GetSliderTooltip(int index)
        {
            float effectiveDelta = owner.GetEffectiveMaxDelta();
            float effectiveW = owner.GetBuildingMaxPower() * effectiveDelta / 20f;
            string status = owner.usePowerLimitMode
                ? STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.ACTIVE_SUFFIX
                : STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.INACTIVE_SUFFIX;

            return string.Format(STRINGS.UI.UISIDESCREENS.SMARTCONDITIONERCONTROL.MAXPOWER_TOOLTIP,
                       owner.maxPower.ToString("F0")) + status
                   + $"\n当前生效: 最大降温 {effectiveDelta:F1}°C, 最大功耗 {effectiveW:F0}W"
                   + GetHeatTransferInfo(effectiveDelta);
        }
    }
}
