using System;
using System.Reflection;
using ConstantTemperatureCooler.STRINGS;
using KSerialization;
using STRINGS;
using UnityEngine;

namespace ConstantTemperatureCooler;

[SerializationConfig(/*Could not decode attribute arguments.*/)]
public class AirConditionerAdjustable : KMonoBehaviour, IUserControlledCapacity
{
	private static readonly IntraObjectHandler<AirConditionerAdjustable> OnCopySettingsDelegate = new IntraObjectHandler<AirConditionerAdjustable>((Action<AirConditionerAdjustable, object>)OnCopySettings);

	public const string KEY = "STRINGS.UI.UISIDESCREENS.AIRCONDITIONERTEMPERATURESIDESCREEN";

	[MyCmpReq]
	public AirConditioner airConditioner;

	[MyCmpReq]
	public EnergyConsumer energyConsumer;

	[MyCmpAdd]
	public CopyBuildingSettings copyBuildingSettings;

	[Serialize]
	private float targetTemperature = 0f;

	public float UserMaxCapacity
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return GameUtil.GetTemperatureConvertedFromKelvin(targetTemperature, GameUtil.temperatureUnit);
		}
		set
		{
			targetTemperature = GameUtil.GetTemperatureConvertedToKelvin(value);
		}
	}

	public float AmountStored => UserMaxCapacity;

	public float MinCapacity => GameUtil.GetTemperatureConvertedFromKelvin(0f, GameUtil.temperatureUnit);

	public float MaxCapacity => GameUtil.GetTemperatureConvertedFromKelvin(573.15f, GameUtil.temperatureUnit);

	public bool WholeValues => false;

	public LocString CapacityUnits => LocString.op_Implicit(GameUtil.GetTemperatureUnitSuffix());

	public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.AIRCONDITIONERTEMPERATURESIDESCREEN.TITLE";

	public string SliderUnits => GameUtil.GetTemperatureUnitSuffix();

	private static void SetTargetTemperatureDirect(AirConditioner instance, float value)
	{
		FieldInfo field = typeof(AirConditioner).GetField("targetTemperature", BindingFlags.Instance | BindingFlags.NonPublic);
		field.SetValue(instance, value);
	}

	private static void OnCopySettings(AirConditionerAdjustable comp, object data)
	{
		comp.OnCopySettings(data);
	}

	public int SliderDecimalPlaces(int i)
	{
		return 8;
	}

	public float GetSliderValue(int i)
	{
		return targetTemperature;
	}

	public string GetSliderTooltipKey(int i)
	{
		return "STRINGS.UI.UISIDESCREENS.AIRCONDITIONERTEMPERATURESIDESCREEN.TOOLTIP";
	}

	public string GetSliderTooltip()
	{
		return string.Format(LocString.op_Implicit(UI.UISIDESCREENS.AIRCONDITIONERTEMPERATURESIDESCREEN.TOOLTIP), targetTemperature, SliderUnits, GetWattsConsumed(), ELECTRICAL.WATT);
	}

	public void SetSliderValue(float val, int i)
	{
		targetTemperature = val;
		Update();
	}

	public float GetWattsConsumed()
	{
		return Mathf.Abs(energyConsumer.WattsNeededWhenActive * (airConditioner.temperatureDelta / 20f));
	}

	protected override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		((KMonoBehaviour)this).Subscribe<AirConditionerAdjustable>(-905833192, OnCopySettingsDelegate);
	}

	protected override void OnSpawn()
	{
		Update();
	}

	internal void OnCopySettings(object data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		AirConditionerAdjustable component = ((GameObject)data).GetComponent<AirConditionerAdjustable>();
		if ((Object)(object)component != (Object)null)
		{
			targetTemperature = component.targetTemperature;
		}
	}

	internal void Update()
	{
		if (targetTemperature != airConditioner.TargetTemperature)
		{
			SetTargetTemperatureDirect(airConditioner, targetTemperature);
		}
		energyConsumer.BaseWattageRating = GetWattsConsumed();
	}
}
