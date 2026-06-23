using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Database;
using Klei;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;
using UnityEngine.Pool;

public static class GameUtil
{
	public enum UnitClass
	{
		SimpleFloat,
		SimpleInteger,
		Temperature,
		Mass,
		Calories,
		Percent,
		Distance,
		Disease,
		Radiation,
		Energy,
		Power,
		Lux,
		Time,
		Seconds,
		Cycles
	}

	public enum TemperatureUnit
	{
		Celsius,
		Fahrenheit,
		Kelvin
	}

	public enum MassUnit
	{
		Kilograms,
		Pounds
	}

	public enum MetricMassFormat
	{
		UseThreshold,
		Kilogram,
		Gram,
		Tonne
	}

	public enum TemperatureInterpretation
	{
		Absolute,
		Relative
	}

	public enum TimeSlice
	{
		None,
		ModifyOnly,
		PerSecond,
		PerCycle
	}

	public enum MeasureUnit
	{
		mass,
		kcal,
		quantity
	}

	public enum IdentityDescriptorTense
	{
		Normal,
		Possessive,
		Plural
	}

	public enum WattageFormatterUnit
	{
		Watts,
		Kilowatts,
		Automatic
	}

	public enum HeatEnergyFormatterUnit
	{
		DTU_S,
		KDTU_S,
		Automatic
	}

	public static class Hardness
	{
		public const int VERY_SOFT = 0;

		public const int SOFT = 10;

		public const int FIRM = 25;

		public const int VERY_FIRM = 50;

		public const int NEARLY_IMPENETRABLE = 150;

		public const int SUPER_DUPER_HARD = 200;

		public const int RADIOACTIVE_MATERIALS = 251;

		public const int IMPENETRABLE = 255;

		public static Color ImpenetrableColor = new Color(0.83137256f, 0.28627452f, 24f / 85f);

		public static Color nearlyImpenetrableColor = new Color(63f / 85f, 0.34901962f, 0.49803922f);

		public static Color veryFirmColor = new Color(0.6392157f, 20f / 51f, 0.6039216f);

		public static Color firmColor = new Color(0.5254902f, 0.41960785f, 0.64705884f);

		public static Color softColor = new Color(0.42745098f, 41f / 85f, 0.75686276f);

		public static Color verySoftColor = new Color(0.44313726f, 57f / 85f, 69f / 85f);
	}

	public static class GermResistanceValues
	{
		public const float MEDIUM = 2f;

		public const float LARGE = 5f;

		public static Color NegativeLargeColor = new Color(0.83137256f, 0.28627452f, 24f / 85f);

		public static Color NegativeMediumColor = new Color(63f / 85f, 0.34901962f, 0.49803922f);

		public static Color NegativeSmallColor = new Color(0.6392157f, 20f / 51f, 0.6039216f);

		public static Color PositiveSmallColor = new Color(0.5254902f, 0.41960785f, 0.64705884f);

		public static Color PositiveMediumColor = new Color(0.42745098f, 41f / 85f, 0.75686276f);

		public static Color PositiveLargeColor = new Color(0.44313726f, 57f / 85f, 69f / 85f);
	}

	public static class ThermalConductivityValues
	{
		public const float VERY_HIGH = 50f;

		public const float HIGH = 10f;

		public const float MEDIUM = 2f;

		public const float LOW = 1f;

		public static Color veryLowConductivityColor = new Color(0.83137256f, 0.28627452f, 24f / 85f);

		public static Color lowConductivityColor = new Color(63f / 85f, 0.34901962f, 0.49803922f);

		public static Color mediumConductivityColor = new Color(0.6392157f, 20f / 51f, 0.6039216f);

		public static Color highConductivityColor = new Color(0.5254902f, 0.41960785f, 0.64705884f);

		public static Color veryHighConductivityColor = new Color(0.42745098f, 41f / 85f, 0.75686276f);
	}

	public static class BreathableValues
	{
		public static Color positiveColor = new Color(0.44313726f, 57f / 85f, 69f / 85f);

		public static Color warningColor = new Color(0.6392157f, 20f / 51f, 0.6039216f);

		public static Color negativeColor = new Color(0.83137256f, 0.28627452f, 24f / 85f);
	}

	public static class WireLoadValues
	{
		public static Color warningColor = new Color(0.9843137f, 0.6901961f, 0.23137255f);

		public static Color negativeColor = new Color(1f, 0.19215687f, 0.19215687f);
	}

	public static TemperatureUnit temperatureUnit;

	public static MassUnit massUnit;

	private static string[] adjectives;

	public static TagSet foodTags = new TagSet("BasicPlantFood", "MushBar", "ColdWheatSeed", "ColdWheatSeed", "SpiceNut", "PrickleFruit", "Meat", "Mushroom", "ColdWheat", GameTags.Compostable.Name);

	public static TagSet solidTags = new TagSet("Filter", "Coal", "BasicFabric", "SwampLilyFlower", "RefinedMetal");

	public static CellOffset[] Expand(this CellOffset[] original)
	{
		List<CellOffset> list = new List<CellOffset>(original);
		Vector4 vector = new Vector2(float.MaxValue, float.MinValue);
		Vector4 vector2 = new Vector2(float.MaxValue, float.MinValue);
		for (int i = 0; i < original.Length; i++)
		{
			CellOffset cellOffset = original[i];
			if ((float)cellOffset.x < vector.x)
			{
				vector.x = cellOffset.x;
			}
			if ((float)cellOffset.x > vector.y)
			{
				vector.y = cellOffset.x;
			}
			if ((float)cellOffset.y < vector2.x)
			{
				vector2.x = cellOffset.y;
			}
			if ((float)cellOffset.y > vector2.y)
			{
				vector2.y = cellOffset.y;
			}
		}
		for (int j = 0; j < original.Length; j++)
		{
			CellOffset cellOffset2 = original[j];
			Vector2Int zero = Vector2Int.zero;
			if ((float)cellOffset2.x == vector.x)
			{
				list.Add(new CellOffset(cellOffset2.x - 1, cellOffset2.y));
				zero.x = -1;
			}
			if ((float)cellOffset2.x == vector.y)
			{
				list.Add(new CellOffset(cellOffset2.x + 1, cellOffset2.y));
				zero.x = 1;
			}
			if ((float)cellOffset2.y == vector2.x)
			{
				list.Add(new CellOffset(cellOffset2.x, cellOffset2.y - 1));
				zero.y = -1;
			}
			if ((float)cellOffset2.y == vector2.y)
			{
				list.Add(new CellOffset(cellOffset2.x, cellOffset2.y + 1));
				zero.y = 1;
			}
			if (zero.x != 0 && zero.y != 0)
			{
				list.Add(new CellOffset((int)((zero.x < 0) ? vector.x : vector.y) + zero.x, (int)((zero.y < 0) ? vector2.x : vector2.y) + zero.y));
			}
		}
		return list.ToArray();
	}

	public static void TintLiquidSymbolOnBuilding(string symbolName, KBatchedAnimController controller, Element element)
	{
		Color color = (element.IsMoltenMetal ? WaterCubes.MOLTEN_METAL_COLOR : ((Color)element.substance.colour));
		color.a = 1f;
		string text = symbolName + "_bloom";
		string name = (element.substance.Glows ? text : symbolName);
		controller.SetSymbolVisiblity(new KAnimHashedString(symbolName), !element.substance.Glows);
		controller.SetSymbolVisiblity(new KAnimHashedString(text), element.substance.Glows);
		controller.SetSymbolTint(new KAnimHashedString(name), color);
	}

	public static string GetTemperatureUnitSuffix()
	{
		return temperatureUnit switch
		{
			TemperatureUnit.Celsius => UI.UNITSUFFIXES.TEMPERATURE.CELSIUS, 
			TemperatureUnit.Fahrenheit => UI.UNITSUFFIXES.TEMPERATURE.FAHRENHEIT, 
			_ => UI.UNITSUFFIXES.TEMPERATURE.KELVIN, 
		};
	}

	private static string AddTemperatureUnitSuffix(string text)
	{
		return text + GetTemperatureUnitSuffix();
	}

	public static float GetTemperatureConvertedFromKelvin(float temperature, TemperatureUnit targetUnit)
	{
		return targetUnit switch
		{
			TemperatureUnit.Celsius => temperature - 273.15f, 
			TemperatureUnit.Fahrenheit => temperature * 1.8f - 459.67f, 
			_ => temperature, 
		};
	}

	public static float GetConvertedTemperature(float temperature, bool roundOutput = false)
	{
		float num = 0f;
		switch (temperatureUnit)
		{
		case TemperatureUnit.Celsius:
			num = temperature - 273.15f;
			if (!roundOutput)
			{
				return num;
			}
			return Mathf.Round(num);
		case TemperatureUnit.Fahrenheit:
			num = temperature * 1.8f - 459.67f;
			if (!roundOutput)
			{
				return num;
			}
			return Mathf.Round(num);
		default:
			if (!roundOutput)
			{
				return temperature;
			}
			return Mathf.Round(temperature);
		}
	}

	public static float GetTemperatureConvertedToKelvin(float temperature, TemperatureUnit fromUnit)
	{
		return fromUnit switch
		{
			TemperatureUnit.Celsius => temperature + 273.15f, 
			TemperatureUnit.Fahrenheit => (temperature + 459.67f) * 5f / 9f, 
			_ => temperature, 
		};
	}

	public static float GetTemperatureConvertedToKelvin(float temperature)
	{
		return temperatureUnit switch
		{
			TemperatureUnit.Celsius => temperature + 273.15f, 
			TemperatureUnit.Fahrenheit => (temperature + 459.67f) * 5f / 9f, 
			_ => temperature, 
		};
	}

	private static float GetConvertedTemperatureDelta(float kelvin_delta)
	{
		return temperatureUnit switch
		{
			TemperatureUnit.Celsius => kelvin_delta, 
			TemperatureUnit.Fahrenheit => kelvin_delta * 1.8f, 
			TemperatureUnit.Kelvin => kelvin_delta, 
			_ => kelvin_delta, 
		};
	}

	public static float ApplyTimeSlice(float val, TimeSlice timeSlice)
	{
		if (timeSlice == TimeSlice.PerCycle)
		{
			return val * 600f;
		}
		return val;
	}

	public static float ApplyTimeSlice(int val, TimeSlice timeSlice)
	{
		if (timeSlice == TimeSlice.PerCycle)
		{
			return (float)val * 600f;
		}
		return val;
	}

	public static string AddTimeSliceText(string text, TimeSlice timeSlice)
	{
		return timeSlice switch
		{
			TimeSlice.PerSecond => text + UI.UNITSUFFIXES.PERSECOND, 
			TimeSlice.PerCycle => text + UI.UNITSUFFIXES.PERCYCLE, 
			_ => text, 
		};
	}

	public static void AddTimeSliceText(StringBuilder builder, TimeSlice timeSlice)
	{
		switch (timeSlice)
		{
		case TimeSlice.PerSecond:
			builder.Append(UI.UNITSUFFIXES.PERSECOND);
			break;
		case TimeSlice.PerCycle:
			builder.Append(UI.UNITSUFFIXES.PERCYCLE);
			break;
		case TimeSlice.None:
		case TimeSlice.ModifyOnly:
			break;
		}
	}

	public static string AddPositiveSign(string text, bool positive)
	{
		if (positive)
		{
			return string.Format(UI.POSITIVE_FORMAT, text);
		}
		return text;
	}

	public static float AttributeSkillToAlpha(AttributeInstance attributeInstance)
	{
		return Mathf.Min(attributeInstance.GetTotalValue() / 10f, 1f);
	}

	public static float AttributeSkillToAlpha(float attributeSkill)
	{
		return Mathf.Min(attributeSkill / 10f, 1f);
	}

	public static float AptitudeToAlpha(float aptitude)
	{
		return Mathf.Min(aptitude / 10f, 1f);
	}

	public static float GetThermalEnergy(PrimaryElement pe)
	{
		return pe.Temperature * pe.Mass * pe.Element.specificHeatCapacity;
	}

	public static float CalculateTemperatureChange(float shc, float mass, float kilowatts)
	{
		return kilowatts / (shc * mass);
	}

	public static void DeltaThermalEnergy(PrimaryElement pe, float kilowatts, float targetTemperature)
	{
		float num = CalculateTemperatureChange(pe.Element.specificHeatCapacity, pe.Mass, kilowatts);
		float value = pe.Temperature + num;
		value = ((!(targetTemperature > pe.Temperature)) ? Mathf.Clamp(value, targetTemperature, pe.Temperature) : Mathf.Clamp(value, pe.Temperature, targetTemperature));
		pe.Temperature = value;
	}

	public static BindingEntry ActionToBinding(Action action)
	{
		BindingEntry[] keyBindings = GameInputMapping.KeyBindings;
		for (int i = 0; i < keyBindings.Length; i++)
		{
			BindingEntry result = keyBindings[i];
			if (result.mAction == action)
			{
				return result;
			}
		}
		throw new ArgumentException(action.ToString() + " is not bound in GameInputBindings");
	}

	public static string GetIdentityDescriptor(GameObject go, IdentityDescriptorTense tense = IdentityDescriptorTense.Normal)
	{
		if ((bool)go.GetComponent<MinionIdentity>())
		{
			switch (tense)
			{
			case IdentityDescriptorTense.Normal:
				return DUPLICANTS.STATS.SUBJECTS.DUPLICANT;
			case IdentityDescriptorTense.Possessive:
				return DUPLICANTS.STATS.SUBJECTS.DUPLICANT_POSSESSIVE;
			case IdentityDescriptorTense.Plural:
				return DUPLICANTS.STATS.SUBJECTS.DUPLICANT_PLURAL;
			}
		}
		else if ((bool)go.GetComponent<CreatureBrain>())
		{
			switch (tense)
			{
			case IdentityDescriptorTense.Normal:
				return DUPLICANTS.STATS.SUBJECTS.CREATURE;
			case IdentityDescriptorTense.Possessive:
				return DUPLICANTS.STATS.SUBJECTS.CREATURE_POSSESSIVE;
			case IdentityDescriptorTense.Plural:
				return DUPLICANTS.STATS.SUBJECTS.CREATURE_PLURAL;
			}
		}
		else
		{
			switch (tense)
			{
			case IdentityDescriptorTense.Normal:
				return DUPLICANTS.STATS.SUBJECTS.PLANT;
			case IdentityDescriptorTense.Possessive:
				return DUPLICANTS.STATS.SUBJECTS.PLANT_POSESSIVE;
			case IdentityDescriptorTense.Plural:
				return DUPLICANTS.STATS.SUBJECTS.PLANT_PLURAL;
			}
		}
		return "";
	}

	public static float GetEnergyInPrimaryElement(PrimaryElement element)
	{
		return 0.001f * (element.Temperature * (element.Mass * 1000f * element.Element.specificHeatCapacity));
	}

	public static float EnergyToTemperatureDelta(float kilojoules, PrimaryElement element)
	{
		Debug.Assert(element.Mass > 0f);
		float num = Mathf.Max(GetEnergyInPrimaryElement(element) - kilojoules, 1f);
		float temperature = element.Temperature;
		return num / (0.001f * (element.Mass * (element.Element.specificHeatCapacity * 1000f))) - temperature;
	}

	public static float CalculateEnergyDeltaForElement(PrimaryElement element, float startTemp, float endTemp)
	{
		return CalculateEnergyDeltaForElementChange(element.Mass, element.Element.specificHeatCapacity, startTemp, endTemp);
	}

	public static float CalculateEnergyDeltaForElementChange(float mass, float shc, float startTemp, float endTemp)
	{
		return (endTemp - startTemp) * mass * shc;
	}

	public static float GetFinalTemperature(float t1, float m1, float t2, float m2)
	{
		float num = m1 + m2;
		float value = (t1 * m1 + t2 * m2) / num;
		float num2 = Mathf.Min(t1, t2);
		float num3 = Mathf.Max(t1, t2);
		value = Mathf.Clamp(value, num2, num3);
		if (float.IsNaN(value) || float.IsInfinity(value))
		{
			Debug.LogError($"Calculated an invalid temperature: t1={t1}, m1={m1}, t2={t2}, m2={m2}, min_temp={num2}, max_temp={num3}");
		}
		return value;
	}

	public static void ForceConduction(PrimaryElement a, PrimaryElement b, float dt)
	{
		float num = a.Temperature * a.Element.specificHeatCapacity * a.Mass;
		float num2 = b.Temperature * b.Element.specificHeatCapacity * b.Mass;
		float num3 = Math.Min(a.Element.thermalConductivity, b.Element.thermalConductivity);
		float num4 = Math.Min(a.Mass, b.Mass);
		float val = (b.Temperature - a.Temperature) * (num3 * num4) * dt;
		float num5 = (num + num2) / (a.Element.specificHeatCapacity * a.Mass + b.Element.specificHeatCapacity * b.Mass);
		float val2 = Math.Abs((num5 - a.Temperature) * a.Element.specificHeatCapacity * a.Mass);
		float val3 = Math.Abs((num5 - b.Temperature) * b.Element.specificHeatCapacity * b.Mass);
		float num6 = Math.Min(val2, val3);
		val = Math.Min(val, num6);
		val = Math.Max(val, 0f - num6);
		a.Temperature = (num + val) / a.Element.specificHeatCapacity / a.Mass;
		b.Temperature = (num2 - val) / b.Element.specificHeatCapacity / b.Mass;
	}

	public static string FloatToString(float f, string format = null)
	{
		if (float.IsPositiveInfinity(f))
		{
			return UI.POS_INFINITY;
		}
		if (float.IsNegativeInfinity(f))
		{
			return UI.NEG_INFINITY;
		}
		return f.ToString(format);
	}

	public static void AppendFloatToString(StringBuilder builder, float f, string format = null)
	{
		if (float.IsPositiveInfinity(f))
		{
			builder.Append(UI.POS_INFINITY);
		}
		else if (float.IsNegativeInfinity(f))
		{
			builder.Append(UI.NEG_INFINITY);
		}
		else if (format != null)
		{
			Span<char> destination = stackalloc char[64];
			f.TryFormat(destination, out var charsWritten, format);
			builder.Append(destination.Slice(0, charsWritten));
		}
		else
		{
			builder.Append(f);
		}
	}

	public static string GetFloatWithDecimalPoint(float f)
	{
		string text = "";
		text = ((f == 0f) ? "0" : ((!(Mathf.Abs(f) < 1f)) ? "#,###.#" : "#,##0.#"));
		return FloatToString(f, text);
	}

	public static void AppendFloatWithDecimalPoint(StringBuilder builder, float f)
	{
		if (f == 0f)
		{
			builder.AppendFormat("{0:0}", f);
		}
		else if (Mathf.Abs(f) < 1f)
		{
			builder.AppendFormat("{0:#,##0.#}", f);
		}
		else
		{
			builder.AppendFormat("{0:#,###.#}", f);
		}
	}

	public static string GetStandardFloat(float f)
	{
		string text = "";
		text = ((f == 0f) ? "0" : ((Mathf.Abs(f) < 1f) ? "#,##0.#" : ((!(Mathf.Abs(f) < 10f)) ? "#,###" : "#,###.#")));
		return FloatToString(f, text);
	}

	public static void AppendStandardFloat(StringBuilder builder, float f)
	{
		if (float.IsPositiveInfinity(f))
		{
			builder.Append(UI.POS_INFINITY);
		}
		else if (float.IsNegativeInfinity(f))
		{
			builder.Append(UI.NEG_INFINITY);
		}
		else if (f == 0f)
		{
			builder.AppendFormat("{0:0}", f);
		}
		else if (Math.Abs(f) < 1f)
		{
			builder.AppendFormat("{0:#,##0.##}", f);
		}
		else if (Math.Abs(f) < 10f)
		{
			builder.AppendFormat("{0:#,##0.##}", f);
		}
		else
		{
			builder.AppendFormat("{0:#,###}", f);
		}
	}

	public static string GetStandardPercentageFloat(float f, bool allowHundredths = false)
	{
		string text = "";
		text = ((Mathf.Abs(f) == 0f) ? "0" : ((Mathf.Abs(f) < 0.1f && allowHundredths) ? "##0.##" : ((!(Mathf.Abs(f) < 1f)) ? "##0" : "##0.#")));
		return FloatToString(f, text);
	}

	public static void AppendStandardPercentageFloat(StringBuilder builder, float f, bool allowHundredths = false)
	{
		if (Mathf.Abs(f) == 0f)
		{
			builder.AppendFormat("{0:0}", f);
			return;
		}
		if (Mathf.Abs(f) < 0.1f && allowHundredths)
		{
			builder.AppendFormat("{0:##0.##}", f);
			return;
		}
		if (Mathf.Abs(f) < 1f)
		{
			builder.AppendFormat("{0:##0.#}", f);
			return;
		}
		if (f < 100f && f >= 99.5f)
		{
			f = 99f;
		}
		builder.AppendFormat("{0:##0}", f);
	}

	public static string GetUnitFormattedName(GameObject go, bool upperName = false)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		if (component != null && Assets.IsTagCountable(component.PrefabTag))
		{
			PrimaryElement component2 = go.GetComponent<PrimaryElement>();
			return GetUnitFormattedName(go.GetProperName(), component2.Units, upperName);
		}
		if (!upperName)
		{
			return go.GetProperName();
		}
		return StringFormatter.ToUpper(go.GetProperName());
	}

	public static string GetUnitFormattedName(string name, float count, bool upperName = false)
	{
		if (upperName)
		{
			name = name.ToUpper();
		}
		return StringFormatter.Replace(UI.NAME_WITH_UNITS, "{0}", name).Replace("{1}", $"{count:0.##}");
	}

	public static void AppendFormattedUnits(StringBuilder builder, float units, TimeSlice timeSlice = TimeSlice.None, bool displaySuffix = true, string floatFormatOverride = "")
	{
		units = ApplyTimeSlice(units, timeSlice);
		if (!floatFormatOverride.IsNullOrWhiteSpace())
		{
			builder.AppendFormat(floatFormatOverride, units);
		}
		else
		{
			AppendStandardFloat(builder, units);
		}
		if (displaySuffix)
		{
			builder.Append((units == 1f) ? UI.UNITSUFFIXES.UNIT : UI.UNITSUFFIXES.UNITS);
		}
		AddTimeSliceText(builder, timeSlice);
	}

	public static string GetFormattedUnits(float units, TimeSlice timeSlice = TimeSlice.None, bool displaySuffix = true, string floatFormatOverride = "")
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedUnits(stringBuilder, units, timeSlice, displaySuffix, floatFormatOverride);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedRocketRangePerCycle(StringBuilder builder, float range, bool displaySuffix = true)
	{
		if (displaySuffix)
		{
			builder.AppendFormat("{0:N1} {1}", range, UI.CLUSTERMAP.TILES_PER_CYCLE);
		}
		else
		{
			builder.AppendFormat("{0:N1}", range);
		}
	}

	public static string GetFormattedRocketRangePerCycle(float range, bool displaySuffix = true)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedRocketRangePerCycle(stringBuilder, range, displaySuffix);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedRocketRange(StringBuilder builder, int rangeInTiles, bool displaySuffix = true)
	{
		builder.Append(rangeInTiles);
		if (displaySuffix)
		{
			builder.Append(" ");
			builder.Append(UI.CLUSTERMAP.TILES);
		}
	}

	public static string GetFormattedRocketRange(int rangeInTiles, bool displaySuffix = true)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedRocketRange(stringBuilder, rangeInTiles, displaySuffix);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static string ApplyBoldString(string source)
	{
		return "<b>" + source + "</b>";
	}

	public static void AppendBoldString(StringBuilder builder, string source)
	{
		builder.AppendFormat("<b>{0}</b>", source);
	}

	public static float GetRoundedTemperatureInKelvin(float kelvin)
	{
		float result = 0f;
		switch (temperatureUnit)
		{
		case TemperatureUnit.Celsius:
			result = GetTemperatureConvertedToKelvin(Mathf.Round(GetConvertedTemperature(Mathf.Round(kelvin), roundOutput: true)));
			break;
		case TemperatureUnit.Fahrenheit:
			result = GetTemperatureConvertedToKelvin(Mathf.RoundToInt(GetTemperatureConvertedFromKelvin(kelvin, TemperatureUnit.Fahrenheit)), TemperatureUnit.Fahrenheit);
			break;
		case TemperatureUnit.Kelvin:
			result = Mathf.RoundToInt(kelvin);
			break;
		}
		return result;
	}

	public static void AppendFormattedTemperature(StringBuilder builder, float temp, TimeSlice timeSlice = TimeSlice.None, TemperatureInterpretation interpretation = TemperatureInterpretation.Absolute, bool displayUnits = true, bool roundInDestinationFormat = false)
	{
		temp = interpretation switch
		{
			TemperatureInterpretation.Absolute => GetConvertedTemperature(temp, roundInDestinationFormat), 
			_ => GetConvertedTemperatureDelta(temp), 
		};
		temp = ApplyTimeSlice(temp, timeSlice);
		if (Mathf.Abs(temp) < 0.1f)
		{
			builder.AppendFormat("{0:##0.####}", temp);
		}
		else
		{
			builder.AppendFormat("{0:##0.#}", temp);
		}
		if (displayUnits)
		{
			builder.Append(GetTemperatureUnitSuffix());
		}
		AddTimeSliceText(builder, timeSlice);
	}

	public static string GetFormattedTemperature(float temp, TimeSlice timeSlice = TimeSlice.None, TemperatureInterpretation interpretation = TemperatureInterpretation.Absolute, bool displayUnits = true, bool roundInDestinationFormat = false)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedTemperature(stringBuilder, temp, timeSlice, interpretation, displayUnits, roundInDestinationFormat);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedCaloriesForItem(StringBuilder builder, Tag tag, float amount, TimeSlice timeSlice = TimeSlice.None, bool forceKcal = true)
	{
		EdiblesManager.FoodInfo foodInfo = EdiblesManager.GetFoodInfo(tag.Name);
		AppendFormattedCalories(builder, (foodInfo != null) ? (foodInfo.CaloriesPerUnit * amount) : (-1f), timeSlice, forceKcal);
	}

	public static string GetFormattedCaloriesForItem(Tag tag, float amount, TimeSlice timeSlice = TimeSlice.None, bool forceKcal = true)
	{
		return GetFormattedCaloriesForItem(tag, amount, showSuffix: true, timeSlice, forceKcal);
	}

	public static string GetFormattedCaloriesForItem(Tag tag, float amount, bool showSuffix, TimeSlice timeSlice = TimeSlice.None, bool forceKcal = true)
	{
		EdiblesManager.FoodInfo foodInfo = EdiblesManager.GetFoodInfo(tag.Name);
		return GetFormattedCalories((foodInfo != null) ? (foodInfo.CaloriesPerUnit * amount) : (-1f), showSuffix, timeSlice, forceKcal);
	}

	public static void AppendFormattedCalories(StringBuilder builder, float calories, TimeSlice timeSlice = TimeSlice.None, bool forceKcal = true)
	{
		AppendFormattedCalories(builder, calories, showSuffix: true, timeSlice, forceKcal);
	}

	public static void AppendFormattedCalories(StringBuilder builder, float calories, bool showSuffix, TimeSlice timeSlice = TimeSlice.None, bool forceKcal = true)
	{
		string value = UI.UNITSUFFIXES.CALORIES.CALORIE;
		if (Mathf.Abs(calories) >= 1000f || forceKcal)
		{
			calories /= 1000f;
			value = UI.UNITSUFFIXES.CALORIES.KILOCALORIE;
		}
		calories = ApplyTimeSlice(calories, timeSlice);
		AppendStandardFloat(builder, calories);
		if (showSuffix)
		{
			builder.Append(value);
			AddTimeSliceText(builder, timeSlice);
		}
	}

	public static string GetFormattedCalories(float calories, TimeSlice timeSlice = TimeSlice.None, bool forceKcal = true)
	{
		return GetFormattedCalories(calories, showSuffix: true, timeSlice, forceKcal);
	}

	public static string GetFormattedCalories(float calories, bool showSuffix, TimeSlice timeSlice = TimeSlice.None, bool forceKcal = true)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedCalories(stringBuilder, calories, showSuffix, timeSlice, forceKcal);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static string GetFormattedPreyConsumptionValuePerCycle(Tag preyTag, float crittersPerSecond, bool perCycle = true)
	{
		Assets.GetPrefab(preyTag).GetComponent<PrimaryElement>();
		return GetFormattedUnits(crittersPerSecond, TimeSlice.PerCycle);
	}

	public static string GetFormattedDirectPlantConsumptionValuePerCycle(Tag plantTag, float consumer_caloriesLossPerCaloriesPerKG, bool perCycle = true)
	{
		IPlantConsumptionInstructions[] plantConsumptionInstructions = GetPlantConsumptionInstructions(Assets.GetPrefab(plantTag));
		if (plantConsumptionInstructions == null || plantConsumptionInstructions.Length == 0)
		{
			return "Error";
		}
		IPlantConsumptionInstructions[] array = plantConsumptionInstructions;
		foreach (IPlantConsumptionInstructions plantConsumptionInstructions2 in array)
		{
			if (plantConsumptionInstructions2.GetDietFoodType() == Diet.Info.FoodType.EatPlantDirectly)
			{
				return plantConsumptionInstructions2.GetFormattedConsumptionPerCycle(consumer_caloriesLossPerCaloriesPerKG);
			}
		}
		return "Error";
	}

	public static string GetFormattedBranchGrowerPlantProductionValuePerCycle(Tag productTag, float outputAmountPerBranch, int branchCount, bool perCycle = true)
	{
		return SafeStringFormat(UI.BUILDINGEFFECTS.TOOLTIPS.BRANCH_GROWER_PLANT_POTENTIAL_OUTPUT, GetFormattedByTag(productTag, outputAmountPerBranch, showSuffix: false, TimeSlice.PerCycle), GetFormattedByTag(productTag, outputAmountPerBranch * (float)branchCount, TimeSlice.PerCycle));
	}

	public static string GetFormattedBranchGrowerPlantPlantFiberProductionValuePerCycle(Tag productTag, float outputAmountPerBranch, int branchCount, bool perCycle = true)
	{
		return SafeStringFormat(UI.BUILDINGEFFECTS.TOOLTIPS.BRANCH_GROWER_PLANT_POTENTIAL_OUTPUT, GetFormattedMass(ApplyTimeSlice(outputAmountPerBranch, TimeSlice.PerCycle)), GetFormattedMass(outputAmountPerBranch * (float)branchCount, TimeSlice.PerCycle, MetricMassFormat.Kilogram));
	}

	public static string GetFormattedPlantStorageConsumptionValuePerCycle(Tag plantTag, float consumer_caloriesLossPerCaloriesPerKG, bool perCycle = true)
	{
		IPlantConsumptionInstructions[] plantConsumptionInstructions = GetPlantConsumptionInstructions(Assets.GetPrefab(plantTag));
		if (plantConsumptionInstructions == null || plantConsumptionInstructions.Length == 0)
		{
			return "Error";
		}
		IPlantConsumptionInstructions[] array = plantConsumptionInstructions;
		foreach (IPlantConsumptionInstructions plantConsumptionInstructions2 in array)
		{
			if (plantConsumptionInstructions2.GetDietFoodType() == Diet.Info.FoodType.EatPlantStorage)
			{
				return plantConsumptionInstructions2.GetFormattedConsumptionPerCycle(consumer_caloriesLossPerCaloriesPerKG);
			}
		}
		return "Error";
	}

	public static IPlantConsumptionInstructions[] GetPlantConsumptionInstructions(GameObject prefab)
	{
		IPlantConsumptionInstructions[] components = prefab.GetComponents<IPlantConsumptionInstructions>();
		List<IPlantConsumptionInstructions> allSMI = prefab.GetAllSMI<IPlantConsumptionInstructions>();
		List<IPlantConsumptionInstructions> list = new List<IPlantConsumptionInstructions>();
		if (components != null)
		{
			list.AddRange(components);
		}
		if (allSMI != null)
		{
			list.AddRange(allSMI);
		}
		return list.ToArray();
	}

	public static void AppendFormattedPlantGrowth(StringBuilder builder, float percent, TimeSlice timeSlice = TimeSlice.None)
	{
		percent = ApplyTimeSlice(percent, timeSlice);
		AppendStandardPercentageFloat(builder, percent, allowHundredths: true);
		builder.Append(UI.UNITSUFFIXES.PERCENT);
		builder.Append(" ");
		builder.Append(UI.UNITSUFFIXES.GROWTH);
		AddTimeSliceText(builder, timeSlice);
	}

	public static string GetFormattedPlantGrowth(float percent, TimeSlice timeSlice = TimeSlice.None)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedPlantGrowth(stringBuilder, percent, timeSlice);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedPercent(StringBuilder builder, float percent, TimeSlice timeSlice = TimeSlice.None)
	{
		AppendStandardPercentageFloat(builder, ApplyTimeSlice(percent, timeSlice));
		builder.Append(UI.UNITSUFFIXES.PERCENT);
		AddTimeSliceText(builder, timeSlice);
	}

	public static string GetFormattedPercent(float percent, TimeSlice timeSlice = TimeSlice.None)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedPercent(stringBuilder, percent, timeSlice);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedRoundedJoules(StringBuilder builder, float joules)
	{
		if (Mathf.Abs(joules) > 1000f)
		{
			builder.AppendFormat("{0:F1}", joules / 1000f);
			builder.Append(UI.UNITSUFFIXES.ELECTRICAL.KILOJOULE);
		}
		else
		{
			builder.AppendFormat("{0:F1}", joules);
			builder.Append(UI.UNITSUFFIXES.ELECTRICAL.JOULE);
		}
	}

	public static string GetFormattedRoundedJoules(float joules)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedRoundedJoules(stringBuilder, joules);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static string GetFormattedJoules(float joules, string floatFormat = "F1", TimeSlice timeSlice = TimeSlice.None)
	{
		if (timeSlice == TimeSlice.PerSecond)
		{
			return GetFormattedWattage(joules);
		}
		joules = ApplyTimeSlice(joules, timeSlice);
		string text = ((Math.Abs(joules) > 1000000f) ? (FloatToString(joules / 1000000f, floatFormat) + UI.UNITSUFFIXES.ELECTRICAL.MEGAJOULE) : ((!(Mathf.Abs(joules) > 1000f)) ? (FloatToString(joules, floatFormat) + UI.UNITSUFFIXES.ELECTRICAL.JOULE) : (FloatToString(joules / 1000f, floatFormat) + UI.UNITSUFFIXES.ELECTRICAL.KILOJOULE)));
		return AddTimeSliceText(text, timeSlice);
	}

	public static void AppendFormattedRads(StringBuilder builder, float rads, TimeSlice timeSlice = TimeSlice.None)
	{
		rads = ApplyTimeSlice(rads, timeSlice);
		AppendStandardFloat(builder, rads);
		builder.Append(UI.UNITSUFFIXES.RADIATION.RADS);
		AddTimeSliceText(builder, timeSlice);
	}

	public static string GetFormattedRads(float rads, TimeSlice timeSlice = TimeSlice.None)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedRads(stringBuilder, rads, timeSlice);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedHighEnergyParticles(StringBuilder builder, float units, TimeSlice timeSlice = TimeSlice.None, bool displayUnits = true)
	{
		AppendFloatWithDecimalPoint(builder, units);
		if (displayUnits)
		{
			builder.Append((units == 1f) ? UI.UNITSUFFIXES.HIGHENERGYPARTICLES.PARTRICLE : UI.UNITSUFFIXES.HIGHENERGYPARTICLES.PARTRICLES);
		}
		AddTimeSliceText(builder, timeSlice);
	}

	public static string GetFormattedHighEnergyParticles(float units, TimeSlice timeSlice = TimeSlice.None, bool displayUnits = true)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedHighEnergyParticles(stringBuilder, units, timeSlice, displayUnits);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedWattage(StringBuilder builder, float watts, WattageFormatterUnit unit = WattageFormatterUnit.Automatic, bool displayUnits = true)
	{
		string text = null;
		switch (unit)
		{
		case WattageFormatterUnit.Automatic:
			if (Mathf.Abs(watts) > 1000f)
			{
				watts /= 1000f;
				text = UI.UNITSUFFIXES.ELECTRICAL.KILOWATT;
			}
			else
			{
				text = UI.UNITSUFFIXES.ELECTRICAL.WATT;
			}
			break;
		case WattageFormatterUnit.Kilowatts:
			watts /= 1000f;
			text = UI.UNITSUFFIXES.ELECTRICAL.KILOWATT;
			break;
		case WattageFormatterUnit.Watts:
			text = UI.UNITSUFFIXES.ELECTRICAL.WATT;
			break;
		}
		AppendFloatToString(builder, watts, "###0.##");
		if (displayUnits && text != null)
		{
			builder.Append(text);
		}
	}

	public static string GetFormattedWattage(float watts, WattageFormatterUnit unit = WattageFormatterUnit.Automatic, bool displayUnits = true)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedWattage(stringBuilder, watts, unit, displayUnits);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedHeatEnergy(StringBuilder builder, float dtu, HeatEnergyFormatterUnit unit = HeatEnergyFormatterUnit.Automatic)
	{
		string text = null;
		string format;
		switch (unit)
		{
		default:
			if (Mathf.Abs(dtu) > 1000f)
			{
				dtu /= 1000f;
				text = UI.UNITSUFFIXES.HEAT.KDTU;
				format = "###0.##";
			}
			else
			{
				text = UI.UNITSUFFIXES.HEAT.DTU;
				format = "###0.";
			}
			break;
		case HeatEnergyFormatterUnit.KDTU_S:
			dtu /= 1000f;
			text = UI.UNITSUFFIXES.HEAT.KDTU;
			format = "###0.##";
			break;
		case HeatEnergyFormatterUnit.DTU_S:
			text = UI.UNITSUFFIXES.HEAT.DTU;
			format = "###0.";
			break;
		}
		AppendFloatToString(builder, dtu, format);
		builder.Append(text);
	}

	public static string GetFormattedHeatEnergy(float dtu, HeatEnergyFormatterUnit unit = HeatEnergyFormatterUnit.Automatic)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedHeatEnergy(stringBuilder, dtu, unit);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedHeatEnergyRate(StringBuilder builder, float dtu_s, HeatEnergyFormatterUnit unit = HeatEnergyFormatterUnit.Automatic)
	{
		string text = null;
		switch (unit)
		{
		case HeatEnergyFormatterUnit.Automatic:
			if (Mathf.Abs(dtu_s) > 1000f)
			{
				dtu_s /= 1000f;
				text = UI.UNITSUFFIXES.HEAT.KDTU_S;
			}
			else
			{
				text = UI.UNITSUFFIXES.HEAT.DTU_S;
			}
			break;
		case HeatEnergyFormatterUnit.KDTU_S:
			dtu_s /= 1000f;
			text = UI.UNITSUFFIXES.HEAT.KDTU_S;
			break;
		case HeatEnergyFormatterUnit.DTU_S:
			text = UI.UNITSUFFIXES.HEAT.DTU_S;
			break;
		}
		AppendFloatToString(builder, dtu_s);
		if (text != null)
		{
			builder.Append(text);
		}
	}

	public static string GetFormattedHeatEnergyRate(float dtu_s, HeatEnergyFormatterUnit unit = HeatEnergyFormatterUnit.Automatic)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedHeatEnergyRate(stringBuilder, dtu_s, unit);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static string GetFormattedInt(float num, TimeSlice timeSlice = TimeSlice.None)
	{
		num = ApplyTimeSlice(num, timeSlice);
		return AddTimeSliceText(FloatToString(num, "F0"), timeSlice);
	}

	public static string GetSpeciesNameFromGameObject(GameObject critterGameObject)
	{
		CreatureBrain component = critterGameObject.GetComponent<CreatureBrain>();
		if (component != null)
		{
			return GetNameForSpecies(component.species);
		}
		return "UNKNOWN SPECIES";
	}

	public static string GetNameForSpecies(Tag species)
	{
		Option<string> option = Option.None;
		return ((species == GameTags.Creatures.Species.HatchSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.HATCHSPECIES) : ((species == GameTags.Creatures.Species.LightBugSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.LIGHTBUGSPECIES) : ((species == GameTags.Creatures.Species.OilFloaterSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.OILFLOATERSPECIES) : ((species == GameTags.Creatures.Species.DreckoSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.DRECKOSPECIES) : ((species == GameTags.Creatures.Species.GlomSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.GLOMSPECIES) : ((species == GameTags.Creatures.Species.PuftSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.PUFTSPECIES) : ((species == GameTags.Creatures.Species.PacuSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.PACUSPECIES) : ((species == GameTags.Creatures.Species.MooSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.MOOSPECIES) : ((species == GameTags.Creatures.Species.MoleSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.MOLESPECIES) : ((species == GameTags.Creatures.Species.SquirrelSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.SQUIRRELSPECIES) : ((species == GameTags.Creatures.Species.CrabSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.CRABSPECIES) : ((species == GameTags.Creatures.Species.DivergentSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.DIVERGENTSPECIES) : ((species == GameTags.Creatures.Species.StaterpillarSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.STATERPILLARSPECIES) : ((species == GameTags.Creatures.Species.BeetaSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.BEETASPECIES) : ((species == GameTags.Creatures.Species.BellySpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.BELLYSPECIES) : ((species == GameTags.Creatures.Species.SealSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.SEALSPECIES) : ((species == GameTags.Creatures.Species.DeerSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.DEERSPECIES) : ((species == GameTags.Creatures.Species.RaptorSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.RAPTORSPECIES) : ((species == GameTags.Creatures.Species.ChameleonSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.CHAMELEONSPECIES) : ((species == GameTags.Creatures.Species.PrehistoricPacuSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.PREHISTORICPACUSPECIES) : ((species == GameTags.Creatures.Species.StegoSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.STEGOSPECIES) : ((species == GameTags.Creatures.Species.ButterflySpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.BUTTERFLYSPECIES) : ((species == GameTags.Creatures.Species.ParrotFishSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.PARROTFISHSPECIES) : ((species == GameTags.Creatures.Species.SnailSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.SNAILSPECIES) : ((species == GameTags.Creatures.Species.SquidSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.SQUIDSPECIES) : ((species == GameTags.Creatures.Species.PufferFishSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.PUFFERFISHSPECIES) : ((species == GameTags.Creatures.Species.SeaFairySpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.SEAFAIRYSPECIES) : ((species == GameTags.Creatures.Species.SeaTurtleSpecies) ? Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.SEATURTLESPECIES) : ((!(species == GameTags.Creatures.Species.SeaHorseSpecies)) ? ((Option<string>)Option.None) : Option.Some((string)STRINGS.CREATURES.FAMILY_PLURAL.SEAHORSESPECIES)))))))))))))))))))))))))))))).Value;
	}

	public static void AppendFormattedSimple(StringBuilder builder, float num, TimeSlice timeSlice = TimeSlice.None, string formatString = null)
	{
		num = ApplyTimeSlice(num, timeSlice);
		if (formatString != null)
		{
			AppendFloatToString(builder, num, formatString);
		}
		else if (num == 0f)
		{
			builder.Append("0");
		}
		else if (Mathf.Abs(num) < 1f)
		{
			AppendFloatToString(builder, num, "#,##0.##");
		}
		else if (Mathf.Abs(num) < 10f)
		{
			AppendFloatToString(builder, num, "#,###.##");
		}
		else
		{
			AppendFloatToString(builder, num, "#,###.##");
		}
		AddTimeSliceText(builder, timeSlice);
	}

	public static string GetFormattedSimple(float num, TimeSlice timeSlice = TimeSlice.None, string formatString = null)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedSimple(stringBuilder, num, timeSlice, formatString);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedLux(StringBuilder builder, int lux)
	{
		builder.Append(lux);
		builder.Append(UI.UNITSUFFIXES.LIGHT.LUX);
	}

	public static string GetFormattedLux(int lux)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedLux(stringBuilder, lux);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static string GetLightDescription(int lux)
	{
		if (lux == 0)
		{
			return UI.OVERLAYS.LIGHTING.RANGES.NO_LIGHT;
		}
		if (lux < DUPLICANTSTATS.STANDARD.Light.LOW_LIGHT)
		{
			return UI.OVERLAYS.LIGHTING.RANGES.VERY_LOW_LIGHT;
		}
		if (lux < DUPLICANTSTATS.STANDARD.Light.MEDIUM_LIGHT)
		{
			return UI.OVERLAYS.LIGHTING.RANGES.LOW_LIGHT;
		}
		if (lux < DUPLICANTSTATS.STANDARD.Light.HIGH_LIGHT)
		{
			return UI.OVERLAYS.LIGHTING.RANGES.MEDIUM_LIGHT;
		}
		if (lux < DUPLICANTSTATS.STANDARD.Light.VERY_HIGH_LIGHT)
		{
			return UI.OVERLAYS.LIGHTING.RANGES.HIGH_LIGHT;
		}
		if (lux < DUPLICANTSTATS.STANDARD.Light.MAX_LIGHT)
		{
			return UI.OVERLAYS.LIGHTING.RANGES.VERY_HIGH_LIGHT;
		}
		return UI.OVERLAYS.LIGHTING.RANGES.MAX_LIGHT;
	}

	public static string GetRadiationDescription(float radsPerCycle)
	{
		if (radsPerCycle == 0f)
		{
			return UI.OVERLAYS.RADIATION.RANGES.NONE;
		}
		if (radsPerCycle < 100f)
		{
			return UI.OVERLAYS.RADIATION.RANGES.VERY_LOW;
		}
		if (radsPerCycle < 200f)
		{
			return UI.OVERLAYS.RADIATION.RANGES.LOW;
		}
		if (radsPerCycle < 400f)
		{
			return UI.OVERLAYS.RADIATION.RANGES.MEDIUM;
		}
		if (radsPerCycle < 2000f)
		{
			return UI.OVERLAYS.RADIATION.RANGES.HIGH;
		}
		if (radsPerCycle < 4000f)
		{
			return UI.OVERLAYS.RADIATION.RANGES.VERY_HIGH;
		}
		return UI.OVERLAYS.RADIATION.RANGES.MAX;
	}

	public static void AppendFormattedByTag(StringBuilder builder, Tag tag, float amount, TimeSlice timeSlice = TimeSlice.None)
	{
		if (GameTags.DisplayAsCalories.Contains(tag))
		{
			AppendFormattedCaloriesForItem(builder, tag, amount, timeSlice);
		}
		else if (GameTags.DisplayAsUnits.Contains(tag))
		{
			AppendFormattedUnits(builder, amount, timeSlice);
		}
		else
		{
			AppendFormattedMass(builder, amount, timeSlice);
		}
	}

	public static string GetFormattedByTag(Tag tag, float amount, TimeSlice timeSlice = TimeSlice.None)
	{
		return GetFormattedByTag(tag, amount, showSuffix: true, timeSlice);
	}

	public static string GetFormattedByTag(Tag tag, float amount, bool showSuffix, TimeSlice timeSlice = TimeSlice.None)
	{
		if (GameTags.DisplayAsCalories.Contains(tag))
		{
			return GetFormattedCaloriesForItem(tag, amount, showSuffix, timeSlice);
		}
		if (GameTags.DisplayAsUnits.Contains(tag))
		{
			return GetFormattedUnits(amount, timeSlice, showSuffix);
		}
		return GetFormattedMass(amount, timeSlice, MetricMassFormat.UseThreshold, showSuffix);
	}

	public static string GetFormattedFoodQuality(int quality)
	{
		if (adjectives == null)
		{
			adjectives = LocString.GetStrings(typeof(DUPLICANTS.NEEDS.FOOD_QUALITY.ADJECTIVES));
		}
		LocString obj = ((quality >= 0) ? DUPLICANTS.NEEDS.FOOD_QUALITY.ADJECTIVE_FORMAT_POSITIVE : DUPLICANTS.NEEDS.FOOD_QUALITY.ADJECTIVE_FORMAT_NEGATIVE);
		int value = quality - DUPLICANTS.NEEDS.FOOD_QUALITY.ADJECTIVE_INDEX_OFFSET;
		value = Mathf.Clamp(value, 0, adjectives.Length);
		return string.Format(obj, adjectives[value], AddPositiveSign(quality.ToString(), quality > 0));
	}

	public static string GetFormattedBytes(ulong amount)
	{
		string[] array = new string[5]
		{
			UI.UNITSUFFIXES.INFORMATION.BYTE,
			UI.UNITSUFFIXES.INFORMATION.KILOBYTE,
			UI.UNITSUFFIXES.INFORMATION.MEGABYTE,
			UI.UNITSUFFIXES.INFORMATION.GIGABYTE,
			UI.UNITSUFFIXES.INFORMATION.TERABYTE
		};
		int num = ((amount != 0L) ? ((int)Math.Floor(Math.Floor(Math.Log(amount)) / Math.Log(1024.0))) : 0);
		double num2 = (double)amount / Math.Pow(1024.0, num);
		Debug.Assert(num >= 0 && num < array.Length);
		return $"{num2:F} {array[num]}";
	}

	public static string GetFormattedInfomation(float amount, TimeSlice timeSlice = TimeSlice.None)
	{
		amount = ApplyTimeSlice(amount, timeSlice);
		string text = "";
		if (amount < 1024f)
		{
			text = UI.UNITSUFFIXES.INFORMATION.KILOBYTE;
		}
		else if (amount < 1048576f)
		{
			amount /= 1000f;
			text = UI.UNITSUFFIXES.INFORMATION.MEGABYTE;
		}
		else if (amount < 1.0737418E+09f)
		{
			amount /= 1048576f;
			text = UI.UNITSUFFIXES.INFORMATION.GIGABYTE;
		}
		return AddTimeSliceText(amount + text, timeSlice);
	}

	public static LocString GetCurrentMassUnit(bool useSmallUnit = false)
	{
		LocString result = null;
		switch (massUnit)
		{
		case MassUnit.Kilograms:
			result = ((!useSmallUnit) ? UI.UNITSUFFIXES.MASS.KILOGRAM : UI.UNITSUFFIXES.MASS.GRAM);
			break;
		case MassUnit.Pounds:
			result = UI.UNITSUFFIXES.MASS.POUND;
			break;
		}
		return result;
	}

	public static void AppendFormattedMass(StringBuilder builder, float mass, TimeSlice timeSlice = TimeSlice.None, MetricMassFormat massFormat = MetricMassFormat.UseThreshold, bool includeSuffix = true, string floatFormat = "{0:0.#}")
	{
		if (mass == float.MinValue)
		{
			builder.Append(UI.CALCULATING);
			return;
		}
		if (float.IsPositiveInfinity(mass))
		{
			builder.Append(UI.POS_INFINITY);
			builder.Append(UI.UNITSUFFIXES.MASS.TONNE);
			return;
		}
		if (float.IsNegativeInfinity(mass))
		{
			builder.Append(UI.NEG_INFINITY);
			builder.Append(UI.UNITSUFFIXES.MASS.TONNE);
			return;
		}
		mass = ApplyTimeSlice(mass, timeSlice);
		string value;
		if (massUnit == MassUnit.Kilograms)
		{
			value = UI.UNITSUFFIXES.MASS.TONNE;
			switch (massFormat)
			{
			case MetricMassFormat.UseThreshold:
			{
				float num = Mathf.Abs(mass);
				if (0f < num)
				{
					if (num < 5E-06f)
					{
						value = UI.UNITSUFFIXES.MASS.MICROGRAM;
						mass = Mathf.Floor(mass * 1E+09f);
					}
					else if (num < 0.005f)
					{
						mass *= 1000000f;
						value = UI.UNITSUFFIXES.MASS.MILLIGRAM;
					}
					else if (Mathf.Abs(mass) < 5f)
					{
						mass *= 1000f;
						value = UI.UNITSUFFIXES.MASS.GRAM;
					}
					else if (Mathf.Abs(mass) < 5000f)
					{
						value = UI.UNITSUFFIXES.MASS.KILOGRAM;
					}
					else
					{
						mass /= 1000f;
						value = UI.UNITSUFFIXES.MASS.TONNE;
					}
				}
				else
				{
					value = UI.UNITSUFFIXES.MASS.KILOGRAM;
				}
				break;
			}
			case MetricMassFormat.Kilogram:
				value = UI.UNITSUFFIXES.MASS.KILOGRAM;
				break;
			case MetricMassFormat.Gram:
				mass *= 1000f;
				value = UI.UNITSUFFIXES.MASS.GRAM;
				break;
			case MetricMassFormat.Tonne:
				mass /= 1000f;
				value = UI.UNITSUFFIXES.MASS.TONNE;
				break;
			}
		}
		else
		{
			mass /= 2.2f;
			value = UI.UNITSUFFIXES.MASS.POUND;
			if (massFormat == MetricMassFormat.UseThreshold)
			{
				float num2 = Mathf.Abs(mass);
				if (num2 < 5f && num2 > 0.001f)
				{
					mass *= 256f;
					value = UI.UNITSUFFIXES.MASS.DRACHMA;
				}
				else
				{
					mass *= 7000f;
					value = UI.UNITSUFFIXES.MASS.GRAIN;
				}
			}
		}
		builder.AppendFormat(floatFormat, mass);
		if (includeSuffix)
		{
			builder.Append(value);
			AddTimeSliceText(builder, timeSlice);
		}
	}

	public static string GetFormattedMass(float mass, TimeSlice timeSlice = TimeSlice.None, MetricMassFormat massFormat = MetricMassFormat.UseThreshold, bool includeSuffix = true, string floatFormat = "{0:0.#}")
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedMass(stringBuilder, mass, timeSlice, massFormat, includeSuffix, floatFormat);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedTime(StringBuilder builder, float seconds)
	{
		builder.AppendFormat(UI.FORMATSECONDS, (int)seconds);
	}

	public static string GetFormattedTime(float seconds, string floatFormat = "F0")
	{
		return string.Format(UI.FORMATSECONDS, seconds.ToString(floatFormat));
	}

	public static void AppendFormattedEngineEfficiency(StringBuilder builder, float amount)
	{
		builder.Append(amount);
		builder.Append(" km /");
		builder.Append(UI.UNITSUFFIXES.MASS.KILOGRAM);
	}

	public static string GetFormattedEngineEfficiency(float amount)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedEngineEfficiency(stringBuilder, amount);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedDistance(StringBuilder builder, float meters)
	{
		if (Mathf.Abs(meters) < 1f)
		{
			builder.AppendFormat("{0:0.0} cm", Math.Abs(meters * 100f));
		}
		else if (meters < 1000f)
		{
			builder.Append(meters);
			builder.Append(" m");
		}
		else
		{
			builder.AppendFormat("{0:0.0} km", meters / 1000f);
		}
	}

	public static string GetFormattedDistance(float meters)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedDistance(stringBuilder, meters);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static void AppendFormattedCycles(StringBuilder builder, float seconds, bool forceCycles = false)
	{
		if (forceCycles || Math.Abs(seconds) > 100f)
		{
			builder.AppendFormat(UI.FORMATDAY, seconds / 600f);
		}
		else
		{
			AppendFormattedTime(builder, seconds);
		}
	}

	public static string GetFormattedCycles(float seconds, string formatString = "F1", bool forceCycles = false)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendFormattedCycles(stringBuilder, seconds, forceCycles);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static float GetDisplaySHC(float shc)
	{
		if (temperatureUnit == TemperatureUnit.Fahrenheit)
		{
			shc /= 1.8f;
		}
		return shc;
	}

	public static string GetSHCSuffix()
	{
		return $"(DTU/g)/{GetTemperatureUnitSuffix()}";
	}

	public static string GetFormattedSHC(float shc)
	{
		shc = GetDisplaySHC(shc);
		return string.Format("{0} (DTU/g)/{1}", shc.ToString("0.000"), GetTemperatureUnitSuffix());
	}

	public static float GetDisplayThermalConductivity(float tc)
	{
		if (temperatureUnit == TemperatureUnit.Fahrenheit)
		{
			tc /= 1.8f;
		}
		return tc;
	}

	public static string GetThermalConductivitySuffix()
	{
		return $"(DTU/(m*s))/{GetTemperatureUnitSuffix()}";
	}

	public static string GetFormattedThermalConductivity(float tc)
	{
		tc = GetDisplayThermalConductivity(tc);
		return string.Format("{0} (DTU/(m*s))/{1}", tc.ToString("0.000"), GetTemperatureUnitSuffix());
	}

	public static string GetElementNameByElementHash(SimHashes elementHash)
	{
		return ElementLoader.FindElementByHash(elementHash).tag.ProperName();
	}

	public static string SafeStringFormat(string source, params object[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			string text = "{" + i + "}";
			if (!source.Contains(text))
			{
				KCrashReporter.ReportDevNotification($"Format error in string: \"{source}\". Source is missing the {{{i}}} format marker for argument \"{args[i]}\" insertion.", Environment.StackTrace);
			}
			else
			{
				source = source.Replace(text, args[i].ToString());
			}
		}
		return source;
	}

	public static bool HasTrait(GameObject go, string traitName)
	{
		Traits component = go.GetComponent<Traits>();
		if (!(component == null))
		{
			return component.HasTrait(traitName);
		}
		return false;
	}

	public static float GetRadiationAbsorptionPercentage(int cell)
	{
		if (Grid.IsValidCell(cell))
		{
			return GetRadiationAbsorptionPercentage(Grid.Element[cell], Grid.Mass[cell], Grid.IsSolidCell(cell) && (Grid.Properties[cell] & 0x80) == 128);
		}
		return 0f;
	}

	public static float GetRadiationAbsorptionPercentage(Element elem, float mass, bool isConstructed)
	{
		float num = 0f;
		float num2 = 2000f;
		float num3 = 0.3f;
		float num4 = 0.7f;
		float num5 = 0.8f;
		num = ((!isConstructed) ? (elem.radiationAbsorptionFactor * num3 + mass / num2 * elem.radiationAbsorptionFactor * num4) : (elem.radiationAbsorptionFactor * num5));
		return Mathf.Clamp(num, 0f, 1f);
	}

	public static void AppendHardnessString(StringBuilder builder, Element element, bool addColor = true)
	{
		if (!element.IsSolid)
		{
			builder.Append(ELEMENTS.HARDNESS.NA);
			return;
		}
		Color firmColor = Hardness.firmColor;
		string text = null;
		if (element.hardness >= byte.MaxValue)
		{
			firmColor = Hardness.ImpenetrableColor;
			text = ELEMENTS.HARDNESS.IMPENETRABLE;
		}
		else if (element.hardness >= 150)
		{
			firmColor = Hardness.nearlyImpenetrableColor;
			text = ELEMENTS.HARDNESS.NEARLYIMPENETRABLE;
		}
		else if (element.hardness >= 50)
		{
			firmColor = Hardness.veryFirmColor;
			text = ELEMENTS.HARDNESS.VERYFIRM;
		}
		else if (element.hardness >= 25)
		{
			firmColor = Hardness.firmColor;
			text = ELEMENTS.HARDNESS.FIRM;
		}
		else if (element.hardness >= 10)
		{
			firmColor = Hardness.softColor;
			text = ELEMENTS.HARDNESS.SOFT;
		}
		else
		{
			firmColor = Hardness.verySoftColor;
			text = ELEMENTS.HARDNESS.VERYSOFT;
		}
		if (addColor)
		{
			builder.AppendFormat("<color=#{0}>", firmColor.ToHexString());
		}
		builder.AppendFormat(text, element.hardness);
		if (addColor)
		{
			builder.Append("</color>");
		}
	}

	public static string GetHardnessString(Element element, bool addColor = true)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		AppendHardnessString(stringBuilder, element, addColor);
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public static string GetGermResistanceModifierString(float modifier, bool addColor = true)
	{
		Color c = Color.black;
		string text = "";
		if (modifier > 0f)
		{
			if (modifier >= 5f)
			{
				c = GermResistanceValues.PositiveLargeColor;
				text = string.Format(DUPLICANTS.ATTRIBUTES.GERMRESISTANCE.MODIFIER_DESCRIPTORS.POSITIVE_LARGE, modifier);
			}
			else if (modifier >= 2f)
			{
				c = GermResistanceValues.PositiveMediumColor;
				text = string.Format(DUPLICANTS.ATTRIBUTES.GERMRESISTANCE.MODIFIER_DESCRIPTORS.POSITIVE_MEDIUM, modifier);
			}
			else if (modifier > 0f)
			{
				c = GermResistanceValues.PositiveSmallColor;
				text = string.Format(DUPLICANTS.ATTRIBUTES.GERMRESISTANCE.MODIFIER_DESCRIPTORS.POSITIVE_SMALL, modifier);
			}
		}
		else if (modifier < 0f)
		{
			if (modifier <= -5f)
			{
				c = GermResistanceValues.NegativeLargeColor;
				text = string.Format(DUPLICANTS.ATTRIBUTES.GERMRESISTANCE.MODIFIER_DESCRIPTORS.NEGATIVE_LARGE, modifier);
			}
			else if (modifier <= -2f)
			{
				c = GermResistanceValues.NegativeMediumColor;
				text = string.Format(DUPLICANTS.ATTRIBUTES.GERMRESISTANCE.MODIFIER_DESCRIPTORS.NEGATIVE_MEDIUM, modifier);
			}
			else if (modifier < 0f)
			{
				c = GermResistanceValues.NegativeSmallColor;
				text = string.Format(DUPLICANTS.ATTRIBUTES.GERMRESISTANCE.MODIFIER_DESCRIPTORS.NEGATIVE_SMALL, modifier);
			}
		}
		else
		{
			addColor = false;
			text = string.Format(DUPLICANTS.ATTRIBUTES.GERMRESISTANCE.MODIFIER_DESCRIPTORS.NONE, modifier);
		}
		if (addColor)
		{
			text = $"<color=#{c.ToHexString()}>{text}</color>";
		}
		return text;
	}

	public static string GetThermalConductivityString(Element element, bool addColor = true, bool addValue = true)
	{
		Color mediumConductivityColor = ThermalConductivityValues.mediumConductivityColor;
		string text = "";
		if (element.thermalConductivity >= 50f)
		{
			mediumConductivityColor = ThermalConductivityValues.veryHighConductivityColor;
			text = UI.ELEMENTAL.THERMALCONDUCTIVITY.ADJECTIVES.VERY_HIGH_CONDUCTIVITY;
		}
		else if (element.thermalConductivity >= 10f)
		{
			mediumConductivityColor = ThermalConductivityValues.highConductivityColor;
			text = UI.ELEMENTAL.THERMALCONDUCTIVITY.ADJECTIVES.HIGH_CONDUCTIVITY;
		}
		else if (element.thermalConductivity >= 2f)
		{
			mediumConductivityColor = ThermalConductivityValues.mediumConductivityColor;
			text = UI.ELEMENTAL.THERMALCONDUCTIVITY.ADJECTIVES.MEDIUM_CONDUCTIVITY;
		}
		else if (element.thermalConductivity >= 1f)
		{
			mediumConductivityColor = ThermalConductivityValues.lowConductivityColor;
			text = UI.ELEMENTAL.THERMALCONDUCTIVITY.ADJECTIVES.LOW_CONDUCTIVITY;
		}
		else
		{
			mediumConductivityColor = ThermalConductivityValues.veryLowConductivityColor;
			text = UI.ELEMENTAL.THERMALCONDUCTIVITY.ADJECTIVES.VERY_LOW_CONDUCTIVITY;
		}
		if (addColor)
		{
			text = $"<color=#{mediumConductivityColor.ToHexString()}>{text}</color>";
		}
		if (addValue)
		{
			text = string.Format(UI.ELEMENTAL.THERMALCONDUCTIVITY.ADJECTIVES.VALUE_WITH_ADJECTIVE, element.thermalConductivity.ToString(), text);
		}
		return text;
	}

	public static string GetBreathableString(Element element, float Mass)
	{
		if (!element.IsGas && !element.IsVacuum)
		{
			return "";
		}
		Color positiveColor = BreathableValues.positiveColor;
		LocString arg;
		switch (element.id)
		{
		case SimHashes.Oxygen:
			if (Mass >= SimDebugView.optimallyBreathable)
			{
				positiveColor = BreathableValues.positiveColor;
				arg = UI.OVERLAYS.OXYGEN.LEGEND1;
			}
			else if (Mass >= SimDebugView.minimumBreathable + (SimDebugView.optimallyBreathable - SimDebugView.minimumBreathable) / 2f)
			{
				positiveColor = BreathableValues.positiveColor;
				arg = UI.OVERLAYS.OXYGEN.LEGEND2;
			}
			else if (Mass >= SimDebugView.minimumBreathable)
			{
				positiveColor = BreathableValues.warningColor;
				arg = UI.OVERLAYS.OXYGEN.LEGEND3;
			}
			else
			{
				positiveColor = BreathableValues.negativeColor;
				arg = UI.OVERLAYS.OXYGEN.LEGEND4;
			}
			break;
		case SimHashes.ContaminatedOxygen:
			if (Mass >= SimDebugView.optimallyBreathable)
			{
				positiveColor = BreathableValues.positiveColor;
				arg = UI.OVERLAYS.OXYGEN.LEGEND1;
			}
			else if (Mass >= SimDebugView.minimumBreathable + (SimDebugView.optimallyBreathable - SimDebugView.minimumBreathable) / 2f)
			{
				positiveColor = BreathableValues.positiveColor;
				arg = UI.OVERLAYS.OXYGEN.LEGEND2;
			}
			else if (Mass >= SimDebugView.minimumBreathable)
			{
				positiveColor = BreathableValues.warningColor;
				arg = UI.OVERLAYS.OXYGEN.LEGEND3;
			}
			else
			{
				positiveColor = BreathableValues.negativeColor;
				arg = UI.OVERLAYS.OXYGEN.LEGEND4;
			}
			break;
		default:
			positiveColor = BreathableValues.negativeColor;
			arg = UI.OVERLAYS.OXYGEN.LEGEND4;
			break;
		}
		return string.Format(ELEMENTS.BREATHABLEDESC, positiveColor.ToHexString(), arg);
	}

	public static string GetWireLoadColor(float load, float maxLoad, float potentialLoad)
	{
		Color c = ((load > maxLoad + POWER.FLOAT_FUDGE_FACTOR) ? WireLoadValues.negativeColor : ((!(potentialLoad > maxLoad) || !(load / maxLoad >= 0.75f)) ? Color.white : WireLoadValues.warningColor));
		return c.ToHexString();
	}

	public static string GetHotkeyString(Action action)
	{
		if (KInputManager.currentControllerIsGamepad)
		{
			return UI.FormatAsHotkey(GetActionString(action));
		}
		return UI.FormatAsHotkey("[" + GetActionString(action) + "]");
	}

	public static string ReplaceHotkeyString(string template, Action action)
	{
		return template.Replace("{Hotkey}", GetHotkeyString(action));
	}

	public static string ReplaceHotkeyString(string template, Action action1, Action action2)
	{
		return template.Replace("{Hotkey}", GetHotkeyString(action1) + GetHotkeyString(action2));
	}

	public static string GetKeycodeLocalized(KKeyCode key_code)
	{
		string result = key_code.ToString();
		switch (key_code)
		{
		case KKeyCode.Return:
			result = INPUT.ENTER;
			break;
		case KKeyCode.Escape:
			result = INPUT.ESCAPE;
			break;
		case KKeyCode.Backslash:
			result = "\\";
			break;
		case KKeyCode.Backspace:
			result = INPUT.BACKSPACE;
			break;
		case KKeyCode.Plus:
			result = "+";
			break;
		case KKeyCode.Slash:
			result = "/";
			break;
		case KKeyCode.Space:
			result = INPUT.SPACE;
			break;
		case KKeyCode.Tab:
			result = INPUT.TAB;
			break;
		case KKeyCode.LeftBracket:
			result = "[";
			break;
		case KKeyCode.RightBracket:
			result = "]";
			break;
		case KKeyCode.Semicolon:
			result = ";";
			break;
		case KKeyCode.Colon:
			result = ":";
			break;
		case KKeyCode.Period:
			result = INPUT.PERIOD;
			break;
		case KKeyCode.Comma:
			result = ",";
			break;
		case KKeyCode.BackQuote:
			result = INPUT.BACKQUOTE;
			break;
		case KKeyCode.MouseScrollUp:
			result = INPUT.MOUSE_SCROLL_UP;
			break;
		case KKeyCode.MouseScrollDown:
			result = INPUT.MOUSE_SCROLL_DOWN;
			break;
		case KKeyCode.Minus:
			result = "-";
			break;
		case KKeyCode.Equals:
			result = "=";
			break;
		case KKeyCode.LeftShift:
			result = INPUT.LEFT_SHIFT;
			break;
		case KKeyCode.RightShift:
			result = INPUT.RIGHT_SHIFT;
			break;
		case KKeyCode.LeftAlt:
			result = INPUT.LEFT_ALT;
			break;
		case KKeyCode.RightAlt:
			result = INPUT.RIGHT_ALT;
			break;
		case KKeyCode.LeftControl:
			result = INPUT.LEFT_CTRL;
			break;
		case KKeyCode.RightControl:
			result = INPUT.RIGHT_CTRL;
			break;
		case KKeyCode.Insert:
			result = INPUT.INSERT;
			break;
		case KKeyCode.Mouse0:
			result = string.Concat(INPUT.MOUSE, " 0");
			break;
		case KKeyCode.Mouse1:
			result = string.Concat(INPUT.MOUSE, " 1");
			break;
		case KKeyCode.Mouse2:
			result = string.Concat(INPUT.MOUSE, " 2");
			break;
		case KKeyCode.Mouse3:
			result = string.Concat(INPUT.MOUSE, " 3");
			break;
		case KKeyCode.Mouse4:
			result = string.Concat(INPUT.MOUSE, " 4");
			break;
		case KKeyCode.Mouse5:
			result = string.Concat(INPUT.MOUSE, " 5");
			break;
		case KKeyCode.Mouse6:
			result = string.Concat(INPUT.MOUSE, " 6");
			break;
		case KKeyCode.Keypad0:
			result = string.Concat(INPUT.NUM, " 0");
			break;
		case KKeyCode.Keypad1:
			result = string.Concat(INPUT.NUM, " 1");
			break;
		case KKeyCode.Keypad2:
			result = string.Concat(INPUT.NUM, " 2");
			break;
		case KKeyCode.Keypad3:
			result = string.Concat(INPUT.NUM, " 3");
			break;
		case KKeyCode.Keypad4:
			result = string.Concat(INPUT.NUM, " 4");
			break;
		case KKeyCode.Keypad5:
			result = string.Concat(INPUT.NUM, " 5");
			break;
		case KKeyCode.Keypad6:
			result = string.Concat(INPUT.NUM, " 6");
			break;
		case KKeyCode.Keypad7:
			result = string.Concat(INPUT.NUM, " 7");
			break;
		case KKeyCode.Keypad8:
			result = string.Concat(INPUT.NUM, " 8");
			break;
		case KKeyCode.Keypad9:
			result = string.Concat(INPUT.NUM, " 9");
			break;
		case KKeyCode.KeypadMultiply:
			result = string.Concat(INPUT.NUM, " *");
			break;
		case KKeyCode.KeypadPeriod:
			result = string.Concat(INPUT.NUM, " ", INPUT.PERIOD);
			break;
		case KKeyCode.KeypadPlus:
			result = string.Concat(INPUT.NUM, " +");
			break;
		case KKeyCode.KeypadMinus:
			result = string.Concat(INPUT.NUM, " -");
			break;
		case KKeyCode.KeypadDivide:
			result = string.Concat(INPUT.NUM, " /");
			break;
		case KKeyCode.KeypadEnter:
			result = string.Concat(INPUT.NUM, " ", INPUT.ENTER);
			break;
		default:
			if (KKeyCode.A <= key_code && key_code <= KKeyCode.Z)
			{
				result = ((char)(65 + (key_code - 97))).ToString();
			}
			else if (KKeyCode.Alpha0 <= key_code && key_code <= KKeyCode.Alpha9)
			{
				result = ((char)(48 + (key_code - 48))).ToString();
			}
			else if (KKeyCode.F1 <= key_code && key_code <= KKeyCode.F12)
			{
				result = "F" + (int)(key_code - 282 + 1);
			}
			else
			{
				Debug.LogWarning("Unable to find proper string for KKeyCode: " + key_code.ToString() + " using key_code.ToString()");
			}
			break;
		case KKeyCode.None:
			break;
		}
		return result;
	}

	public static string GetActionString(Action action)
	{
		string result = "";
		if (action == Action.NumActions)
		{
			return result;
		}
		BindingEntry bindingEntry = ActionToBinding(action);
		KKeyCode mKeyCode = bindingEntry.mKeyCode;
		if (KInputManager.currentControllerIsGamepad)
		{
			return KInputManager.steamInputInterpreter.GetActionGlyph(action);
		}
		if (bindingEntry.mModifier == Modifier.None)
		{
			return GetKeycodeLocalized(mKeyCode).ToUpper();
		}
		string text = "";
		switch (bindingEntry.mModifier)
		{
		case Modifier.Shift:
			text = INPUT.SHIFT.ToString();
			break;
		case Modifier.Ctrl:
			text = INPUT.CTRL.ToString();
			break;
		case Modifier.CapsLock:
			text = GetKeycodeLocalized(KKeyCode.CapsLock);
			break;
		case Modifier.Alt:
			text = INPUT.ALT.ToString();
			break;
		case Modifier.Backtick:
			text = GetKeycodeLocalized(KKeyCode.BackQuote);
			break;
		}
		return (text + " + " + GetKeycodeLocalized(mKeyCode)).ToUpper();
	}

	public static void CreateExplosion(Vector3 explosion_pos)
	{
		Vector2 vector = new Vector2(explosion_pos.x, explosion_pos.y);
		float num = 5f * 5f;
		foreach (Health item in Components.Health.Items)
		{
			Vector3 position = item.transform.GetPosition();
			float sqrMagnitude = (new Vector2(position.x, position.y) - vector).sqrMagnitude;
			if (num >= sqrMagnitude && item != null)
			{
				item.Damage(item.maxHitPoints);
			}
		}
	}

	private static void GetNonSolidCells(int x, int y, List<int> cells, int min_x, int min_y, int max_x, int max_y)
	{
		int num = Grid.XYToCell(x, y);
		if (Grid.IsValidCell(num) && !Grid.Solid[num] && !Grid.DupePassable[num] && x >= min_x && x <= max_x && y >= min_y && y <= max_y && !cells.Contains(num))
		{
			cells.Add(num);
			GetNonSolidCells(x + 1, y, cells, min_x, min_y, max_x, max_y);
			GetNonSolidCells(x - 1, y, cells, min_x, min_y, max_x, max_y);
			GetNonSolidCells(x, y + 1, cells, min_x, min_y, max_x, max_y);
			GetNonSolidCells(x, y - 1, cells, min_x, min_y, max_x, max_y);
		}
	}

	public static void GetNonSolidCells(int cell, int radius, List<int> cells)
	{
		int x = 0;
		int y = 0;
		Grid.CellToXY(cell, out x, out y);
		GetNonSolidCells(x, y, cells, x - radius, y - radius, x + radius, y + radius);
	}

	public static float GetMaxStressInActiveWorld()
	{
		if (Components.LiveMinionIdentities.Count <= 0)
		{
			return 0f;
		}
		float num = 0f;
		foreach (MinionIdentity item in Components.LiveMinionIdentities.Items)
		{
			if (!item.IsNullOrDestroyed() && item.GetMyWorldId() == ClusterManager.Instance.activeWorldId)
			{
				AmountInstance amountInstance = Db.Get().Amounts.Stress.Lookup(item);
				if (amountInstance != null)
				{
					num = Mathf.Max(num, amountInstance.value);
				}
			}
		}
		return num;
	}

	public static float GetAverageStressInActiveWorld()
	{
		if (Components.LiveMinionIdentities.Count <= 0)
		{
			return 0f;
		}
		float num = 0f;
		int num2 = 0;
		foreach (MinionIdentity item in Components.LiveMinionIdentities.Items)
		{
			if (!item.IsNullOrDestroyed() && item.GetMyWorldId() == ClusterManager.Instance.activeWorldId)
			{
				num += Db.Get().Amounts.Stress.Lookup(item).value;
				num2++;
			}
		}
		return num / (float)num2;
	}

	public static string MigrateFMOD(FMODAsset asset)
	{
		if (asset == null)
		{
			return null;
		}
		if (asset.path == null)
		{
			return asset.name;
		}
		return asset.path;
	}

	private static void SortGameObjectDescriptors(List<IGameObjectEffectDescriptor> descriptorList)
	{
		descriptorList.Sort(delegate(IGameObjectEffectDescriptor e1, IGameObjectEffectDescriptor e2)
		{
			int num = TUNING.BUILDINGS.COMPONENT_DESCRIPTION_ORDER.IndexOf(e1.GetType());
			int value = TUNING.BUILDINGS.COMPONENT_DESCRIPTION_ORDER.IndexOf(e2.GetType());
			return num.CompareTo(value);
		});
	}

	public static void IndentListOfDescriptors(List<Descriptor> list, int indentCount = 1)
	{
		for (int i = 0; i < list.Count; i++)
		{
			Descriptor value = list[i];
			for (int j = 0; j < indentCount; j++)
			{
				value.IncreaseIndent();
			}
			list[i] = value;
		}
	}

	public static List<Descriptor> GetAllDescriptors(GameObject go, bool simpleInfoScreen = false)
	{
		List<Descriptor> list = new List<Descriptor>();
		List<IGameObjectEffectDescriptor> list2 = new List<IGameObjectEffectDescriptor>(go.GetComponents<IGameObjectEffectDescriptor>());
		StateMachineController component = go.GetComponent<StateMachineController>();
		if (component != null)
		{
			list2.AddRange(component.GetDescriptors());
		}
		SortGameObjectDescriptors(list2);
		foreach (IGameObjectEffectDescriptor item in list2)
		{
			List<Descriptor> descriptors = item.GetDescriptors(go);
			if (descriptors == null)
			{
				continue;
			}
			foreach (Descriptor item2 in descriptors)
			{
				if (!item2.onlyForSimpleInfoScreen || simpleInfoScreen)
				{
					list.Add(item2);
				}
			}
		}
		KPrefabID component2 = go.GetComponent<KPrefabID>();
		if (component2 != null && component2.AdditionalRequirements != null)
		{
			foreach (Descriptor additionalRequirement in component2.AdditionalRequirements)
			{
				if (!additionalRequirement.onlyForSimpleInfoScreen || simpleInfoScreen)
				{
					list.Add(additionalRequirement);
				}
			}
		}
		if (component2 != null && component2.AdditionalEffects != null)
		{
			foreach (Descriptor additionalEffect in component2.AdditionalEffects)
			{
				if (!additionalEffect.onlyForSimpleInfoScreen || simpleInfoScreen)
				{
					list.Add(additionalEffect);
				}
			}
		}
		return list;
	}

	public static List<Descriptor> GetDetailDescriptors(List<Descriptor> descriptors)
	{
		List<Descriptor> list = new List<Descriptor>();
		foreach (Descriptor descriptor in descriptors)
		{
			if (descriptor.type == Descriptor.DescriptorType.Detail)
			{
				list.Add(descriptor);
			}
		}
		IndentListOfDescriptors(list);
		return list;
	}

	public static List<Descriptor> GetRequirementDescriptors(List<Descriptor> descriptors)
	{
		return GetRequirementDescriptors(descriptors, indent: true);
	}

	public static List<Descriptor> GetRequirementDescriptors(List<Descriptor> descriptors, bool indent)
	{
		List<Descriptor> list = new List<Descriptor>();
		foreach (Descriptor descriptor in descriptors)
		{
			if (descriptor.type == Descriptor.DescriptorType.Requirement)
			{
				list.Add(descriptor);
			}
		}
		if (indent)
		{
			IndentListOfDescriptors(list);
		}
		return list;
	}

	public static List<Descriptor> GetEffectDescriptors(List<Descriptor> descriptors)
	{
		List<Descriptor> list = new List<Descriptor>();
		foreach (Descriptor descriptor in descriptors)
		{
			if (descriptor.type == Descriptor.DescriptorType.Effect || descriptor.type == Descriptor.DescriptorType.DiseaseSource)
			{
				list.Add(descriptor);
			}
		}
		IndentListOfDescriptors(list);
		return list;
	}

	public static List<Descriptor> GetInformationDescriptors(List<Descriptor> descriptors)
	{
		List<Descriptor> list = new List<Descriptor>();
		foreach (Descriptor descriptor in descriptors)
		{
			if (descriptor.type == Descriptor.DescriptorType.Lifecycle)
			{
				list.Add(descriptor);
			}
		}
		IndentListOfDescriptors(list);
		return list;
	}

	public static List<Descriptor> GetCropOptimumConditionDescriptors(List<Descriptor> descriptors)
	{
		List<Descriptor> list = new List<Descriptor>();
		foreach (Descriptor descriptor in descriptors)
		{
			if (descriptor.type == Descriptor.DescriptorType.Lifecycle)
			{
				Descriptor item = descriptor;
				item.text = "• " + item.text;
				list.Add(item);
			}
		}
		IndentListOfDescriptors(list);
		return list;
	}

	public static List<Descriptor> GetGameObjectRequirements(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor>();
		List<IGameObjectEffectDescriptor> list2 = new List<IGameObjectEffectDescriptor>(go.GetComponents<IGameObjectEffectDescriptor>());
		StateMachineController component = go.GetComponent<StateMachineController>();
		if (component != null)
		{
			list2.AddRange(component.GetDescriptors());
		}
		SortGameObjectDescriptors(list2);
		foreach (IGameObjectEffectDescriptor item in list2)
		{
			List<Descriptor> descriptors = item.GetDescriptors(go);
			if (descriptors == null)
			{
				continue;
			}
			foreach (Descriptor item2 in descriptors)
			{
				if (item2.type == Descriptor.DescriptorType.Requirement)
				{
					list.Add(item2);
				}
			}
		}
		KPrefabID component2 = go.GetComponent<KPrefabID>();
		if (component2.AdditionalRequirements != null)
		{
			list.AddRange(component2.AdditionalRequirements);
		}
		return list;
	}

	public static List<Descriptor> GetGameObjectEffects(GameObject go, bool simpleInfoScreen = false)
	{
		List<Descriptor> list = new List<Descriptor>();
		List<IGameObjectEffectDescriptor> list2 = new List<IGameObjectEffectDescriptor>(go.GetComponents<IGameObjectEffectDescriptor>());
		StateMachineController component = go.GetComponent<StateMachineController>();
		if (component != null)
		{
			list2.AddRange(component.GetDescriptors());
		}
		SortGameObjectDescriptors(list2);
		foreach (IGameObjectEffectDescriptor item in list2)
		{
			List<Descriptor> descriptors = item.GetDescriptors(go);
			if (descriptors == null)
			{
				continue;
			}
			foreach (Descriptor item2 in descriptors)
			{
				if ((!item2.onlyForSimpleInfoScreen || simpleInfoScreen) && (item2.type == Descriptor.DescriptorType.Effect || item2.type == Descriptor.DescriptorType.DiseaseSource))
				{
					list.Add(item2);
				}
			}
		}
		KPrefabID component2 = go.GetComponent<KPrefabID>();
		if (component2 != null && component2.AdditionalEffects != null)
		{
			foreach (Descriptor additionalEffect in component2.AdditionalEffects)
			{
				if (!additionalEffect.onlyForSimpleInfoScreen || simpleInfoScreen)
				{
					list.Add(additionalEffect);
				}
			}
		}
		return list;
	}

	public static void PartitionBuildingDescriptors(GameObject buildingComplete, bool simpleInfoScreen, out List<Descriptor> allDescs, List<(ElementConverter converter, List<Descriptor> descriptors)> converterDescCache, List<Descriptor> nonConverterReqs, List<Descriptor> nonConverterEffects, out bool hasConverterReqs)
	{
		ElementConverter[] components = buildingComplete.GetComponents<ElementConverter>();
		allDescs = GetAllDescriptors(buildingComplete, simpleInfoScreen);
		hasConverterReqs = false;
		HashSetPool<string, BuildingDef>.PooledHashSet pooledHashSet = HashSetPool<string, BuildingDef>.Allocate();
		ElementConverter[] array = components;
		foreach (ElementConverter elementConverter in array)
		{
			if (elementConverter.consumedElements == null || elementConverter.consumedElements.Length == 0)
			{
				continue;
			}
			List<Descriptor> descriptors = elementConverter.GetDescriptors(buildingComplete);
			converterDescCache.Add((elementConverter, descriptors));
			if (descriptors == null)
			{
				continue;
			}
			foreach (Descriptor item3 in descriptors)
			{
				pooledHashSet.Add(item3.text);
				if (item3.type == Descriptor.DescriptorType.Requirement)
				{
					hasConverterReqs = true;
				}
			}
		}
		IConverterByproduct defImplementingInterface = buildingComplete.GetDefImplementingInterface<IConverterByproduct>();
		if (defImplementingInterface != null && defImplementingInterface.ByproductRate > 0f)
		{
			foreach (var item4 in converterDescCache)
			{
				ElementConverter item = item4.converter;
				List<Descriptor> item2 = item4.descriptors;
				bool flag = false;
				ElementConverter.ConsumedElement[] consumedElements = item.consumedElements;
				for (int i = 0; i < consumedElements.Length; i++)
				{
					if (consumedElements[i].Tag == defImplementingInterface.ByproductAssociatedInputTag)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					defImplementingInterface.GetByproductDescriptors(buildingComplete, item2);
					break;
				}
			}
		}
		foreach (Descriptor allDesc in allDescs)
		{
			if (!pooledHashSet.Contains(allDesc.text))
			{
				switch (allDesc.type)
				{
				case Descriptor.DescriptorType.Requirement:
					nonConverterReqs.Add(allDesc);
					break;
				case Descriptor.DescriptorType.Effect:
				case Descriptor.DescriptorType.DiseaseSource:
					nonConverterEffects.Add(allDesc);
					break;
				}
			}
		}
		pooledHashSet.Recycle();
	}

	public static void BuildPartitionedRequirements(List<Descriptor> result, List<Descriptor> nonConverterReqs, List<(ElementConverter converter, List<Descriptor> descriptors)> converterDescCache, bool hasConverterReqs)
	{
		foreach (Descriptor nonConverterReq in nonConverterReqs)
		{
			nonConverterReq.IncreaseIndent();
			result.Add(nonConverterReq);
		}
		if (hasConverterReqs)
		{
			Descriptor item = default(Descriptor);
			item.SetupDescriptor(UI.BUILDINGEFFECTS.OPERATIONINPUTS, "", Descriptor.DescriptorType.Requirement);
			item.IncreaseIndent();
			result.Add(item);
		}
		foreach (var item3 in converterDescCache)
		{
			foreach (Descriptor item4 in item3.descriptors)
			{
				if (item4.type == Descriptor.DescriptorType.Requirement)
				{
					Descriptor item2 = item4;
					item2.text = "• " + item2.text;
					item2.IncreaseIndent();
					item2.IncreaseIndent();
					result.Add(item2);
				}
			}
		}
	}

	public static void BuildPartitionedEffects(List<Descriptor> result, List<Descriptor> nonConverterEffects, List<(ElementConverter converter, List<Descriptor> descriptors)> converterDescCache)
	{
		foreach (Descriptor nonConverterEffect in nonConverterEffects)
		{
			nonConverterEffect.IncreaseIndent();
			result.Add(nonConverterEffect);
		}
		foreach (var item5 in converterDescCache)
		{
			ElementConverter item = item5.converter;
			List<Descriptor> item2 = item5.descriptors;
			string text = item.consumedElements[0].Name;
			for (int i = 1; i < item.consumedElements.Length; i++)
			{
				text = text + ", " + item.consumedElements[i].Name;
			}
			Descriptor item3 = new Descriptor(text + ":", "");
			item3.IncreaseIndent();
			result.Add(item3);
			foreach (Descriptor item6 in item2)
			{
				if (item6.type != Descriptor.DescriptorType.Requirement)
				{
					Descriptor item4 = item6;
					item4.IncreaseIndent();
					item4.IncreaseIndent();
					result.Add(item4);
				}
			}
		}
	}

	public static List<Descriptor> GetPlantRequirementDescriptors(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor>();
		List<Descriptor> requirementDescriptors = GetRequirementDescriptors(GetAllDescriptors(go));
		if (requirementDescriptors.Count > 0)
		{
			Descriptor item = default(Descriptor);
			item.SetupDescriptor(UI.UISIDESCREENS.PLANTERSIDESCREEN.PLANTREQUIREMENTS, UI.UISIDESCREENS.PLANTERSIDESCREEN.TOOLTIPS.PLANTREQUIREMENTS, Descriptor.DescriptorType.Requirement);
			list.Add(item);
			list.AddRange(requirementDescriptors);
		}
		return list;
	}

	public static List<Descriptor> GetPlantLifeCycleDescriptors(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor>();
		List<Descriptor> informationDescriptors = GetInformationDescriptors(GetAllDescriptors(go));
		if (informationDescriptors.Count > 0)
		{
			Descriptor item = default(Descriptor);
			item.SetupDescriptor(UI.UISIDESCREENS.PLANTERSIDESCREEN.LIFECYCLE, UI.UISIDESCREENS.PLANTERSIDESCREEN.TOOLTIPS.PLANTLIFECYCLE, Descriptor.DescriptorType.Lifecycle);
			list.Add(item);
			list.AddRange(informationDescriptors);
		}
		return list;
	}

	public static List<Descriptor> GetPlantEffectDescriptors(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor>();
		List<Descriptor> allDescriptors = GetAllDescriptors(go);
		List<Descriptor> list2 = new List<Descriptor>();
		list2.AddRange(GetEffectDescriptors(allDescriptors));
		if (list2.Count > 0)
		{
			Descriptor item = default(Descriptor);
			item.SetupDescriptor(UI.UISIDESCREENS.PLANTERSIDESCREEN.PLANTEFFECTS, UI.UISIDESCREENS.PLANTERSIDESCREEN.TOOLTIPS.PLANTEFFECTS);
			list.Add(item);
			list.AddRange(list2);
		}
		return list;
	}

	public static string GetGameObjectEffectsTooltipString(GameObject go)
	{
		string text = "";
		List<Descriptor> gameObjectEffects = GetGameObjectEffects(go);
		if (gameObjectEffects.Count > 0)
		{
			text = string.Concat(text, UI.BUILDINGEFFECTS.OPERATIONEFFECTS, "\n");
		}
		foreach (Descriptor item in gameObjectEffects)
		{
			text = text + item.IndentedText() + "\n";
		}
		return text;
	}

	public static List<Descriptor> GetEquipmentEffects(EquipmentDef def)
	{
		Debug.Assert(def != null);
		List<Descriptor> list = new List<Descriptor>();
		List<AttributeModifier> attributeModifiers = def.AttributeModifiers;
		if (attributeModifiers != null)
		{
			foreach (AttributeModifier item in attributeModifiers)
			{
				string name = Db.Get().Attributes.Get(item.AttributeId).Name;
				string formattedString = item.GetFormattedString();
				string newValue = ((item.Value >= 0f) ? "produced" : "consumed");
				string text = UI.GAMEOBJECTEFFECTS.EQUIPMENT_MODS.text.Replace("{Attribute}", name).Replace("{Style}", newValue).Replace("{Value}", formattedString);
				list.Add(new Descriptor(text, text));
			}
		}
		return list;
	}

	public static string GetRecipeDescription(Recipe recipe)
	{
		string text = null;
		if (recipe != null)
		{
			text = recipe.recipeDescription;
		}
		if (text == null)
		{
			text = RESEARCH.TYPES.MISSINGRECIPEDESC;
			Debug.LogWarning("Missing recipeDescription");
		}
		return text;
	}

	public static int GetCurrentCycle()
	{
		return GameClock.Instance.GetCycle() + 1;
	}

	public static float GetCurrentTimeInCycles()
	{
		return GameClock.Instance.GetTimeInCycles() + 1f;
	}

	public static GameObject GetActiveTelepad()
	{
		GameObject telepad = GetTelepad(ClusterManager.Instance.activeWorldId);
		if (telepad == null)
		{
			telepad = GetTelepad(ClusterManager.Instance.GetStartWorld().id);
		}
		return telepad;
	}

	public static GameObject GetTelepad(int worldId)
	{
		if (Components.Telepads.Count > 0)
		{
			for (int i = 0; i < Components.Telepads.Count; i++)
			{
				if (Components.Telepads[i].GetMyWorldId() == worldId)
				{
					return Components.Telepads[i].gameObject;
				}
			}
		}
		return null;
	}

	public static GameObject KInstantiate(GameObject original, Vector3 position, Grid.SceneLayer sceneLayer, string name = null, int gameLayer = 0)
	{
		return KInstantiate(original, position, sceneLayer, null, name, gameLayer);
	}

	public static GameObject KInstantiate(GameObject original, Vector3 position, Grid.SceneLayer sceneLayer, GameObject parent, string name = null, int gameLayer = 0)
	{
		position.z = Grid.GetLayerZ(sceneLayer);
		return Util.KInstantiate(original, position, Quaternion.identity, parent, name, initialize_id: true, gameLayer);
	}

	public static GameObject KInstantiate(GameObject original, Grid.SceneLayer sceneLayer, string name = null, int gameLayer = 0)
	{
		return KInstantiate(original, Vector3.zero, sceneLayer, name, gameLayer);
	}

	public static GameObject KInstantiate(Component original, Grid.SceneLayer sceneLayer, string name = null, int gameLayer = 0)
	{
		return KInstantiate(original.gameObject, Vector3.zero, sceneLayer, name, gameLayer);
	}

	public unsafe static void IsEmissionBlocked(int cell, out bool all_not_gaseous, out bool all_over_pressure)
	{
		int* ptr = stackalloc int[4];
		*ptr = Grid.CellBelow(cell);
		ptr[1] = Grid.CellLeft(cell);
		ptr[2] = Grid.CellRight(cell);
		ptr[3] = Grid.CellAbove(cell);
		all_not_gaseous = true;
		all_over_pressure = true;
		for (int i = 0; i < 4; i++)
		{
			int num = ptr[i];
			if (Grid.IsValidCell(num))
			{
				Element element = Grid.Element[num];
				all_not_gaseous = all_not_gaseous && !element.IsGas && !element.IsVacuum;
				all_over_pressure = all_over_pressure && ((!element.IsGas && !element.IsVacuum) || Grid.Mass[num] >= 1.8f);
			}
		}
	}

	public static float GetDecorAtCell(int cell)
	{
		return GetDecorAtCell(cell, includeLightDecor: true);
	}

	public static float GetDecorAtCell(int cell, bool includeLightDecor)
	{
		float num = 0f;
		if (!Grid.Solid[cell])
		{
			num = Grid.Decor[cell];
			if (includeLightDecor)
			{
				num += (float)DecorProvider.GetLightDecorBonus(cell);
			}
		}
		return num;
	}

	public static string GetUnitTypeMassOrUnit(GameObject go)
	{
		string result = UI.UNITSUFFIXES.UNITS;
		KPrefabID component = go.GetComponent<KPrefabID>();
		if (component != null)
		{
			result = (component.Tags.Contains(GameTags.Seed) ? UI.UNITSUFFIXES.UNITS : UI.UNITSUFFIXES.MASS.KILOGRAM);
		}
		return result;
	}

	public static string GetKeywordStyle(Tag tag)
	{
		Element element = ElementLoader.GetElement(tag);
		if (element != null)
		{
			return GetKeywordStyle(element);
		}
		if (foodTags.Contains(tag))
		{
			return "food";
		}
		if (solidTags.Contains(tag))
		{
			return "solid";
		}
		return null;
	}

	public static string GetKeywordStyle(SimHashes hash)
	{
		Element element = ElementLoader.FindElementByHash(hash);
		if (element != null)
		{
			return GetKeywordStyle(element);
		}
		return null;
	}

	public static string GetKeywordStyle(Element element)
	{
		if (element.id == SimHashes.Oxygen)
		{
			return "oxygen";
		}
		if (element.IsSolid)
		{
			return "solid";
		}
		if (element.IsLiquid)
		{
			return "liquid";
		}
		if (element.IsGas)
		{
			return "gas";
		}
		if (element.IsVacuum)
		{
			return "vacuum";
		}
		return null;
	}

	public static string GetKeywordStyle(GameObject go)
	{
		string result = "";
		Edible component = go.GetComponent<Edible>();
		Equippable component2 = go.GetComponent<Equippable>();
		MedicinalPill component3 = go.GetComponent<MedicinalPill>();
		ResearchPointObject component4 = go.GetComponent<ResearchPointObject>();
		if (component != null)
		{
			result = "food";
		}
		else if (component2 != null)
		{
			result = "equipment";
		}
		else if (component3 != null)
		{
			result = "medicine";
		}
		else if (component4 != null)
		{
			result = "research";
		}
		return result;
	}

	public static Sprite GetBiomeSprite(string id)
	{
		string text = "biomeIcon" + char.ToUpper(id[0]) + id.Substring(1).ToLower();
		Sprite sprite = Assets.GetSprite(text);
		if (sprite != null)
		{
			return new Tuple<Sprite, Color>(sprite, Color.white).first;
		}
		Debug.LogWarning("Missing codex biome icon: " + text);
		return null;
	}

	public static string GenerateRandomDuplicantName()
	{
		string text = "";
		string text2 = "";
		string text3 = "";
		bool flag = UnityEngine.Random.Range(0f, 1f) >= 0.5f;
		List<string> list = new List<string>(LocString.GetStrings(typeof(NAMEGEN.DUPLICANT.NAME.NB)));
		list.AddRange(flag ? LocString.GetStrings(typeof(NAMEGEN.DUPLICANT.NAME.MALE)) : LocString.GetStrings(typeof(NAMEGEN.DUPLICANT.NAME.FEMALE)));
		text3 = list.GetRandom();
		if (UnityEngine.Random.Range(0f, 1f) > 0.7f)
		{
			List<string> list2 = new List<string>(LocString.GetStrings(typeof(NAMEGEN.DUPLICANT.PREFIX.NB)));
			list2.AddRange(flag ? LocString.GetStrings(typeof(NAMEGEN.DUPLICANT.PREFIX.MALE)) : LocString.GetStrings(typeof(NAMEGEN.DUPLICANT.PREFIX.FEMALE)));
			text = list2.GetRandom();
		}
		if (!string.IsNullOrEmpty(text))
		{
			text += " ";
		}
		if (UnityEngine.Random.Range(0f, 1f) >= 0.9f)
		{
			List<string> list3 = new List<string>(LocString.GetStrings(typeof(NAMEGEN.DUPLICANT.SUFFIX.NB)));
			list3.AddRange(flag ? LocString.GetStrings(typeof(NAMEGEN.DUPLICANT.SUFFIX.MALE)) : LocString.GetStrings(typeof(NAMEGEN.DUPLICANT.SUFFIX.FEMALE)));
			text2 = list3.GetRandom();
		}
		if (!string.IsNullOrEmpty(text2))
		{
			text2 = " " + text2;
		}
		return text + text3 + text2;
	}

	public static string GenerateRandomLaunchPadName()
	{
		return NAMEGEN.LAUNCHPAD.FORMAT.Replace("{Name}", UnityEngine.Random.Range(1, 1000).ToString());
	}

	public static string GenerateRandomRocketName()
	{
		string text = "";
		string newValue = "";
		string newValue2 = "";
		string newValue3 = "";
		int num = 1;
		int num2 = 2;
		int num3 = 4;
		text = new List<string>(LocString.GetStrings(typeof(NAMEGEN.ROCKET.NOUN))).GetRandom();
		int num4 = 0;
		if (UnityEngine.Random.value > 0.7f)
		{
			newValue = new List<string>(LocString.GetStrings(typeof(NAMEGEN.ROCKET.PREFIX))).GetRandom();
			num4 |= num;
		}
		if (UnityEngine.Random.value > 0.5f)
		{
			newValue2 = new List<string>(LocString.GetStrings(typeof(NAMEGEN.ROCKET.ADJECTIVE))).GetRandom();
			num4 |= num2;
		}
		if (UnityEngine.Random.value > 0.1f)
		{
			newValue3 = new List<string>(LocString.GetStrings(typeof(NAMEGEN.ROCKET.SUFFIX))).GetRandom();
			num4 |= num3;
		}
		string text2 = ((num4 == (num | num2 | num3)) ? ((string)NAMEGEN.ROCKET.FMT_PREFIX_ADJECTIVE_NOUN_SUFFIX) : ((num4 == (num2 | num3)) ? ((string)NAMEGEN.ROCKET.FMT_ADJECTIVE_NOUN_SUFFIX) : ((num4 == (num | num3)) ? ((string)NAMEGEN.ROCKET.FMT_PREFIX_NOUN_SUFFIX) : ((num4 == num3) ? ((string)NAMEGEN.ROCKET.FMT_NOUN_SUFFIX) : ((num4 == (num | num2)) ? ((string)NAMEGEN.ROCKET.FMT_PREFIX_ADJECTIVE_NOUN) : ((num4 == num) ? ((string)NAMEGEN.ROCKET.FMT_PREFIX_NOUN) : ((num4 != num2) ? ((string)NAMEGEN.ROCKET.FMT_NOUN) : ((string)NAMEGEN.ROCKET.FMT_ADJECTIVE_NOUN))))))));
		DebugUtil.LogArgs("Rocket name bits:", Convert.ToString(num4, 2));
		return text2.Replace("{Prefix}", newValue).Replace("{Adjective}", newValue2).Replace("{Noun}", text)
			.Replace("{Suffix}", newValue3);
	}

	public static string GenerateRandomWorldName(string[] nameTables)
	{
		if (nameTables == null)
		{
			Debug.LogWarning("No name tables provided to generate world name. Using GENERIC");
			nameTables = new string[1] { "GENERIC" };
		}
		string text = "";
		string[] array = nameTables;
		foreach (string text2 in array)
		{
			text += Strings.Get("STRINGS.NAMEGEN.WORLD.ROOTS." + text2.ToUpper());
		}
		string text3 = RandomValueFromSeparatedString(text);
		if (string.IsNullOrEmpty(text3))
		{
			text3 = RandomValueFromSeparatedString(Strings.Get(NAMEGEN.WORLD.ROOTS.GENERIC));
		}
		string text4 = RandomValueFromSeparatedString(NAMEGEN.WORLD.SUFFIXES.GENERICLIST);
		return text3 + text4;
	}

	public static float GetThermalComfort(Tag duplicantType, int cell, float tolerance)
	{
		DUPLICANTSTATS statsFor = DUPLICANTSTATS.GetStatsFor(duplicantType);
		float num = 0f;
		Element element = ElementLoader.FindElementByHash(SimHashes.Creature);
		if (Grid.Element[cell].thermalConductivity != 0f)
		{
			num = SimUtil.CalculateEnergyFlowCreatures(cell, statsFor.Temperature.Internal.IDEAL, element.specificHeatCapacity, element.thermalConductivity, statsFor.Temperature.SURFACE_AREA, statsFor.Temperature.SKIN_THICKNESS + 0.0025f);
		}
		num -= tolerance;
		return num * 1000f;
	}

	public static void FocusCamera(Transform target, bool select = true, bool show_back_button = true)
	{
		FocusCamera(target.GetPosition(), 2f, playSound: true, show_back_button);
		if (select)
		{
			KSelectable component = target.GetComponent<KSelectable>();
			SelectTool.Instance.Select(component);
		}
	}

	public static void FocusCameraOnWorld(int worldID, Vector3 pos, float forceOrthgraphicSize = 10f, System.Action callback = null, bool show_back_button = true)
	{
		CameraController.Instance.ActiveWorldStarWipe(worldID, pos, forceOrthgraphicSize, callback);
		if (show_back_button && NotificationScreen_TemporaryActions.Instance != null)
		{
			NotificationScreen_TemporaryActions.Instance.CreateCameraReturnActionButton(CameraController.Instance.transform.position);
		}
	}

	public static void FocusCamera(int cell, bool show_back_button = true)
	{
		FocusCamera(Grid.CellToPos(cell), 2f, playSound: true, show_back_button);
	}

	public static void FocusCamera(Vector3 position, float speed = 2f, bool playSound = true, bool show_back_button = true)
	{
		CameraController.Instance.CameraGoTo(position, speed, playSound);
		if (show_back_button && NotificationScreen_TemporaryActions.Instance != null)
		{
			NotificationScreen_TemporaryActions.Instance.CreateCameraReturnActionButton(CameraController.Instance.transform.position);
		}
	}

	public static string RandomValueFromSeparatedString(string source, string separator = "\n")
	{
		int startIndex = 0;
		int num = 0;
		while (true)
		{
			startIndex = source.IndexOf(separator, startIndex);
			if (startIndex == -1)
			{
				break;
			}
			startIndex += separator.Length;
			num++;
		}
		if (num == 0)
		{
			return "";
		}
		int num2 = UnityEngine.Random.Range(0, num);
		startIndex = 0;
		for (int i = 0; i < num2; i++)
		{
			startIndex = source.IndexOf(separator, startIndex) + separator.Length;
		}
		int num3 = source.IndexOf(separator, startIndex);
		return source.Substring(startIndex, (num3 == -1) ? (source.Length - startIndex) : (num3 - startIndex));
	}

	public static string GetFormattedDiseaseName(byte idx, bool color = false)
	{
		Disease disease = Db.Get().Diseases[idx];
		if (color)
		{
			return string.Format(UI.OVERLAYS.DISEASE.DISEASE_NAME_FORMAT, disease.Name, ColourToHex(GlobalAssets.Instance.colorSet.GetColorByName(disease.overlayColourName)));
		}
		return string.Format(UI.OVERLAYS.DISEASE.DISEASE_NAME_FORMAT_NO_COLOR, disease.Name);
	}

	public static string GetFormattedDisease(byte idx, int units, bool color = false)
	{
		if (idx != byte.MaxValue && units > 0)
		{
			Disease disease = Db.Get().Diseases[idx];
			if (color)
			{
				return string.Format(UI.OVERLAYS.DISEASE.DISEASE_FORMAT, disease.Name, GetFormattedDiseaseAmount(units), ColourToHex(GlobalAssets.Instance.colorSet.GetColorByName(disease.overlayColourName)));
			}
			return string.Format(UI.OVERLAYS.DISEASE.DISEASE_FORMAT_NO_COLOR, disease.Name, GetFormattedDiseaseAmount(units));
		}
		return UI.OVERLAYS.DISEASE.NO_DISEASE;
	}

	public static string GetFormattedDiseaseAmount(int units, TimeSlice timeSlice = TimeSlice.None)
	{
		ApplyTimeSlice(units, timeSlice);
		return AddTimeSliceText(units.ToString("#,##0") + UI.UNITSUFFIXES.DISEASE.UNITS, timeSlice);
	}

	public static string GetFormattedDiseaseAmount(long units, TimeSlice timeSlice = TimeSlice.None)
	{
		ApplyTimeSlice(units, timeSlice);
		return AddTimeSliceText(units.ToString("#,##0") + UI.UNITSUFFIXES.DISEASE.UNITS, timeSlice);
	}

	public static string ColourizeString(Color32 colour, string str)
	{
		return $"<color=#{ColourToHex(colour)}>{str}</color>";
	}

	public static string ColourToHex(Color32 colour)
	{
		return $"{colour.r:X2}{colour.g:X2}{colour.b:X2}{colour.a:X2}";
	}

	public static string GetFormattedDecor(float value, bool enforce_max = false)
	{
		string arg = "";
		LocString locString = ((value > DecorMonitor.MAXIMUM_DECOR_VALUE && enforce_max) ? UI.OVERLAYS.DECOR.MAXIMUM_DECOR : UI.OVERLAYS.DECOR.VALUE);
		if (enforce_max)
		{
			value = Math.Min(value, DecorMonitor.MAXIMUM_DECOR_VALUE);
		}
		if (value > 0f)
		{
			arg = "+";
		}
		else if (!(value < 0f))
		{
			locString = UI.OVERLAYS.DECOR.VALUE_ZERO;
		}
		return string.Format(locString, arg, value);
	}

	public static Color GetDecorColourFromValue(int decor)
	{
		Color black = Color.black;
		float num = (float)decor / 100f;
		if (num > 0f)
		{
			return Color.Lerp(new Color(0.15f, 0f, 0f), new Color(0f, 1f, 0f), Mathf.Abs(num));
		}
		return Color.Lerp(new Color(0.15f, 0f, 0f), new Color(1f, 0f, 0f), Mathf.Abs(num));
	}

	public static List<Descriptor> GetMaterialDescriptors(Element element)
	{
		List<Descriptor> list = new List<Descriptor>();
		if (element.attributeModifiers.Count > 0)
		{
			foreach (AttributeModifier attributeModifier in element.attributeModifiers)
			{
				string txt = string.Format(Strings.Get(new StringKey("STRINGS.ELEMENTS.MATERIAL_MODIFIERS." + attributeModifier.AttributeId.ToUpper())), attributeModifier.GetFormattedString());
				string tooltip = string.Format(Strings.Get(new StringKey("STRINGS.ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP." + attributeModifier.AttributeId.ToUpper())), attributeModifier.GetFormattedString());
				Descriptor item = default(Descriptor);
				item.SetupDescriptor(txt, tooltip);
				item.IncreaseIndent();
				list.Add(item);
			}
		}
		list.AddRange(GetSignificantMaterialPropertyDescriptors(element));
		return list;
	}

	public static string GetMaterialTooltips(Element element)
	{
		string text = element.tag.ProperName();
		foreach (AttributeModifier attributeModifier in element.attributeModifiers)
		{
			string name = Db.Get().BuildingAttributes.Get(attributeModifier.AttributeId).Name;
			string formattedString = attributeModifier.GetFormattedString();
			text = text + "\n    • " + string.Format(DUPLICANTS.MODIFIERS.MODIFIER_FORMAT, name, formattedString);
		}
		return text + GetSignificantMaterialPropertyTooltips(element);
	}

	public static string GetSignificantMaterialPropertyTooltips(Element element)
	{
		string text = "";
		List<Descriptor> significantMaterialPropertyDescriptors = GetSignificantMaterialPropertyDescriptors(element);
		if (significantMaterialPropertyDescriptors.Count > 0)
		{
			text += "\n";
			for (int i = 0; i < significantMaterialPropertyDescriptors.Count; i++)
			{
				text = text + "    • " + Util.StripTextFormatting(significantMaterialPropertyDescriptors[i].text) + "\n";
			}
		}
		return text;
	}

	public static List<Descriptor> GetSignificantMaterialPropertyDescriptors(Element element)
	{
		List<Descriptor> list = new List<Descriptor>();
		if (element.thermalConductivity > 10f)
		{
			Descriptor item = default(Descriptor);
			item.SetupDescriptor(string.Format(ELEMENTS.MATERIAL_MODIFIERS.HIGH_THERMAL_CONDUCTIVITY, GetThermalConductivityString(element, addColor: false, addValue: false)), string.Format(ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.HIGH_THERMAL_CONDUCTIVITY, element.name, element.thermalConductivity.ToString("0.#####")));
			item.IncreaseIndent();
			list.Add(item);
		}
		if (element.thermalConductivity < 1f)
		{
			Descriptor item2 = default(Descriptor);
			item2.SetupDescriptor(string.Format(ELEMENTS.MATERIAL_MODIFIERS.LOW_THERMAL_CONDUCTIVITY, GetThermalConductivityString(element, addColor: false, addValue: false)), string.Format(ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.LOW_THERMAL_CONDUCTIVITY, element.name, element.thermalConductivity.ToString("0.#####")));
			item2.IncreaseIndent();
			list.Add(item2);
		}
		if (element.specificHeatCapacity <= 0.2f)
		{
			Descriptor item3 = default(Descriptor);
			item3.SetupDescriptor(ELEMENTS.MATERIAL_MODIFIERS.LOW_SPECIFIC_HEAT_CAPACITY, string.Format(ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.LOW_SPECIFIC_HEAT_CAPACITY, element.name, element.specificHeatCapacity * 1f));
			item3.IncreaseIndent();
			list.Add(item3);
		}
		if (element.specificHeatCapacity >= 1f)
		{
			Descriptor item4 = default(Descriptor);
			item4.SetupDescriptor(ELEMENTS.MATERIAL_MODIFIERS.HIGH_SPECIFIC_HEAT_CAPACITY, string.Format(ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.HIGH_SPECIFIC_HEAT_CAPACITY, element.name, element.specificHeatCapacity * 1f));
			item4.IncreaseIndent();
			list.Add(item4);
		}
		if (Sim.IsRadiationEnabled() && element.radiationAbsorptionFactor >= 0.8f)
		{
			Descriptor item5 = default(Descriptor);
			item5.SetupDescriptor(ELEMENTS.MATERIAL_MODIFIERS.EXCELLENT_RADIATION_SHIELD, string.Format(ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.EXCELLENT_RADIATION_SHIELD, element.name, element.radiationAbsorptionFactor));
			item5.IncreaseIndent();
			list.Add(item5);
		}
		return list;
	}

	public static int NaturalBuildingCell(this KMonoBehaviour cmp)
	{
		return Grid.PosToCell(cmp.transform.GetPosition());
	}

	public static List<Descriptor> GetMaterialDescriptors(Tag tag)
	{
		List<Descriptor> list = new List<Descriptor>();
		Element element = ElementLoader.GetElement(tag);
		if (element != null)
		{
			if (element.attributeModifiers.Count > 0)
			{
				foreach (AttributeModifier attributeModifier in element.attributeModifiers)
				{
					string txt = string.Format(Strings.Get(new StringKey("STRINGS.ELEMENTS.MATERIAL_MODIFIERS." + attributeModifier.AttributeId.ToUpper())), attributeModifier.GetFormattedString());
					string tooltip = string.Format(Strings.Get(new StringKey("STRINGS.ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP." + attributeModifier.AttributeId.ToUpper())), attributeModifier.GetFormattedString());
					Descriptor item = default(Descriptor);
					item.SetupDescriptor(txt, tooltip);
					item.IncreaseIndent();
					list.Add(item);
				}
			}
			list.AddRange(GetSignificantMaterialPropertyDescriptors(element));
		}
		else
		{
			GameObject gameObject = Assets.TryGetPrefab(tag);
			if (gameObject != null)
			{
				PrefabAttributeModifiers component = gameObject.GetComponent<PrefabAttributeModifiers>();
				if (component != null)
				{
					foreach (AttributeModifier descriptor in component.descriptors)
					{
						string txt2 = string.Format(Strings.Get(new StringKey("STRINGS.ELEMENTS.MATERIAL_MODIFIERS." + descriptor.AttributeId.ToUpper())), descriptor.GetFormattedString());
						string tooltip2 = string.Format(Strings.Get(new StringKey("STRINGS.ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP." + descriptor.AttributeId.ToUpper())), descriptor.GetFormattedString());
						Descriptor item2 = default(Descriptor);
						item2.SetupDescriptor(txt2, tooltip2);
						item2.IncreaseIndent();
						list.Add(item2);
					}
				}
			}
		}
		return list;
	}

	public static string GetMaterialTooltips(Tag tag)
	{
		string text = tag.ProperName();
		Element element = ElementLoader.GetElement(tag);
		if (element != null)
		{
			foreach (AttributeModifier attributeModifier in element.attributeModifiers)
			{
				string name = Db.Get().BuildingAttributes.Get(attributeModifier.AttributeId).Name;
				string formattedString = attributeModifier.GetFormattedString();
				text = text + "\n    • " + string.Format(DUPLICANTS.MODIFIERS.MODIFIER_FORMAT, name, formattedString);
			}
			text += GetSignificantMaterialPropertyTooltips(element);
		}
		else
		{
			GameObject gameObject = Assets.TryGetPrefab(tag);
			if (gameObject != null)
			{
				PrefabAttributeModifiers component = gameObject.GetComponent<PrefabAttributeModifiers>();
				if (component != null)
				{
					foreach (AttributeModifier descriptor in component.descriptors)
					{
						string name2 = Db.Get().BuildingAttributes.Get(descriptor.AttributeId).Name;
						string formattedString2 = descriptor.GetFormattedString();
						text = text + "\n    • " + string.Format(DUPLICANTS.MODIFIERS.MODIFIER_FORMAT, name2, formattedString2);
					}
				}
			}
		}
		return text;
	}

	public static bool AreChoresUIMergeable(Chore.Precondition.Context choreA, Chore.Precondition.Context choreB)
	{
		if (choreA.chore.target.isNull || choreB.chore.target.isNull)
		{
			return false;
		}
		ChoreType choreType = choreB.chore.choreType;
		ChoreType choreType2 = choreA.chore.choreType;
		if (choreA.chore.choreType == choreB.chore.choreType && choreA.chore.target.GetComponent<KPrefabID>().PrefabTag == choreB.chore.target.GetComponent<KPrefabID>().PrefabTag)
		{
			return true;
		}
		if (choreA.chore.choreType == Db.Get().ChoreTypes.Dig && choreB.chore.choreType == Db.Get().ChoreTypes.Dig)
		{
			return true;
		}
		if (choreA.chore.choreType == Db.Get().ChoreTypes.Relax && choreB.chore.choreType == Db.Get().ChoreTypes.Relax)
		{
			return true;
		}
		if ((choreType2 == Db.Get().ChoreTypes.ReturnSuitIdle || choreType2 == Db.Get().ChoreTypes.ReturnSuitUrgent) && (choreType == Db.Get().ChoreTypes.ReturnSuitIdle || choreType == Db.Get().ChoreTypes.ReturnSuitUrgent))
		{
			return true;
		}
		if (choreA.chore.target.gameObject == choreB.chore.target.gameObject && choreA.chore.choreType == choreB.chore.choreType)
		{
			return true;
		}
		return false;
	}

	public static string GetChoreName(Chore chore, object choreData)
	{
		string result = "";
		if (chore.choreType == Db.Get().ChoreTypes.Fetch || chore.choreType == Db.Get().ChoreTypes.MachineFetch || chore.choreType == Db.Get().ChoreTypes.FabricateFetch || chore.choreType == Db.Get().ChoreTypes.FetchCritical || chore.choreType == Db.Get().ChoreTypes.PowerFetch)
		{
			result = chore.GetReportName(chore.gameObject.GetProperName());
		}
		else if (chore.choreType == Db.Get().ChoreTypes.StorageFetch || chore.choreType == Db.Get().ChoreTypes.FoodFetch)
		{
			FetchChore fetchChore = chore as FetchChore;
			if (chore is FetchAreaChore { GetFetchTarget: var getFetchTarget })
			{
				KMonoBehaviour kMonoBehaviour = choreData as KMonoBehaviour;
				result = ((getFetchTarget != null) ? chore.GetReportName(getFetchTarget.GetProperName()) : ((!(kMonoBehaviour != null)) ? chore.GetReportName() : chore.GetReportName(kMonoBehaviour.GetProperName())));
			}
			else if (fetchChore != null)
			{
				Pickupable fetchTarget = fetchChore.fetchTarget;
				KMonoBehaviour kMonoBehaviour2 = choreData as KMonoBehaviour;
				result = ((fetchTarget != null) ? chore.GetReportName(fetchTarget.GetProperName()) : ((!(kMonoBehaviour2 != null)) ? chore.GetReportName() : chore.GetReportName(kMonoBehaviour2.GetProperName())));
			}
		}
		else
		{
			result = chore.GetReportName();
		}
		return result;
	}

	public static string ChoreGroupsForChoreType(ChoreType choreType)
	{
		if (choreType.groups == null || choreType.groups.Length == 0)
		{
			return null;
		}
		string text = "";
		for (int i = 0; i < choreType.groups.Length; i++)
		{
			if (i != 0)
			{
				text += UI.UISIDESCREENS.MINIONTODOSIDESCREEN.CHORE_GROUP_SEPARATOR;
			}
			text += choreType.groups[i].Name;
		}
		return text;
	}

	public static List<BuildingDef> GetBuildingsRequiringSkillPerk(string perkID)
	{
		return Assets.BuildingDefs.Where((BuildingDef building) => building.RequiredSkillPerkID == perkID).ToList();
	}

	public static string NamesOfBuildingsRequiringSkillPerk(string perkID)
	{
		List<string> list = (from building in GetBuildingsRequiringSkillPerk(perkID)
			select SafeStringFormat(UI.ROLES_SCREEN.PERKS.CAN_USE_BUILDING.DESCRIPTION, building.Name)).ToList();
		if (list == null || list.Count == 0)
		{
			return null;
		}
		return string.Join("\n", list);
	}

	public static string NamesOfBoostersWithSkillPerk(string perkID)
	{
		List<string> values = (from tag in BionicUpgradeComponentConfig.GetBoostersWithSkillPerk(perkID)
			select Strings.Get($"STRINGS.ITEMS.BIONIC_BOOSTERS.{tag.ToString().ToUpper()}.NAME").String).ToList();
		return string.Join("\n", values);
	}

	public static string NamesOfSkillsWithSkillPerk(string perkID)
	{
		List<string> list = (from match in Db.Get().Skills.resources
			where !match.deprecated && match.GivesPerk(perkID)
			select match.Name).ToList();
		return string.Join("\n", list.ToArray());
	}

	public static bool IsCapturingTimeLapse()
	{
		if (Game.Instance != null && Game.Instance.timelapser != null)
		{
			return Game.Instance.timelapser.CapturingTimelapseScreenshot;
		}
		return false;
	}

	public static ExposureType GetExposureTypeForDisease(Disease disease)
	{
		for (int i = 0; i < GERM_EXPOSURE.TYPES.Length; i++)
		{
			if (disease.id == GERM_EXPOSURE.TYPES[i].germ_id)
			{
				return GERM_EXPOSURE.TYPES[i];
			}
		}
		return null;
	}

	public static Sickness GetSicknessForDisease(Disease disease)
	{
		for (int i = 0; i < GERM_EXPOSURE.TYPES.Length; i++)
		{
			if (disease.id == GERM_EXPOSURE.TYPES[i].germ_id)
			{
				if (GERM_EXPOSURE.TYPES[i].sickness_id == null)
				{
					return null;
				}
				return Db.Get().Sicknesses.Get(GERM_EXPOSURE.TYPES[i].sickness_id);
			}
		}
		return null;
	}

	public static void SubscribeToTags<T>(T target, EventSystem.IntraObjectHandler<T> handler, bool triggerImmediately) where T : KMonoBehaviour
	{
		if (triggerImmediately)
		{
			Boxed<TagChangedEventData> boxed = Boxed<TagChangedEventData>.Get(new TagChangedEventData(Tag.Invalid, added: false));
			handler.Trigger(target.gameObject, boxed);
			Boxed<TagChangedEventData>.Release(boxed);
		}
		target.Subscribe(-1582839653, handler);
	}

	public static void UnsubscribeToTags<T>(T target, EventSystem.IntraObjectHandler<T> handler) where T : KMonoBehaviour
	{
		target.Unsubscribe(-1582839653, handler);
	}

	public static EventSystem.IntraObjectHandler<T> CreateHasTagHandler<T>(Tag tag, Action<T, object> callback) where T : KMonoBehaviour
	{
		return new EventSystem.IntraObjectHandler<T>(delegate(T component, object data)
		{
			TagChangedEventData tagChangedEventData = ((Boxed<TagChangedEventData>)data).value;
			if (tagChangedEventData.tag == Tag.Invalid)
			{
				KPrefabID component2 = component.GetComponent<KPrefabID>();
				tagChangedEventData = new TagChangedEventData(tag, component2.HasTag(tag));
			}
			if (tagChangedEventData.tag == tag && tagChangedEventData.added)
			{
				callback(component, data);
			}
		});
	}

	public static void DestroyCell(int cell, CellElementEvent eventSource, bool deleteMinions = true)
	{
		List<GameObject> value;
		using (CollectionPool<List<GameObject>, GameObject>.Get(out value))
		{
			value.Add(Grid.Objects[cell, 2]);
			value.Add(Grid.Objects[cell, 1]);
			value.Add(Grid.Objects[cell, 9]);
			value.Add(Grid.Objects[cell, 5]);
			value.Add(Grid.Objects[cell, 12]);
			value.Add(Grid.Objects[cell, 15]);
			value.Add(Grid.Objects[cell, 16]);
			value.Add(Grid.Objects[cell, 19]);
			value.Add(Grid.Objects[cell, 20]);
			value.Add(Grid.Objects[cell, 23]);
			value.Add(Grid.Objects[cell, 26]);
			value.Add(Grid.Objects[cell, 29]);
			value.Add(Grid.Objects[cell, 27]);
			value.Add(Grid.Objects[cell, 31]);
			value.Add(Grid.Objects[cell, 30]);
			foreach (Comet item in Components.Meteors.GetItems(Grid.WorldIdx[cell]))
			{
				if (!item.IsNullOrDestroyed() && Grid.PosToCell(item) == cell)
				{
					value.Add(item.gameObject);
				}
			}
			foreach (GameObject item2 in value)
			{
				if (item2 != null)
				{
					Util.KDestroyGameObject(item2);
				}
			}
			ClearCell(cell, deleteMinions);
			FallingWater.instance.ClearParticles(cell);
			if (ElementLoader.elements[Grid.ElementIdx[cell]].id == SimHashes.Void)
			{
				SimMessages.ReplaceElement(cell, SimHashes.Void, eventSource, 0f, 0f);
			}
			else
			{
				SimMessages.ReplaceElement(cell, SimHashes.Vacuum, eventSource, 0f, 0f);
			}
			if (BackwallManager.HasBackwall(cell))
			{
				SimMessages.Dig(cell, -1, skipEvent: true, backwall: true);
			}
		}
	}

	public static void ClearCell(int cell, bool deleteMinions = true)
	{
		Vector2I vector2I = Grid.CellToXY(cell);
		List<ScenePartitionerEntry> value;
		using (CollectionPool<List<ScenePartitionerEntry>, ScenePartitionerEntry>.Get(out value))
		{
			GameScenePartitioner.Instance.GatherEntries(vector2I.x, vector2I.y, 1, 1, GameScenePartitioner.Instance.pickupablesLayer, value);
			for (int i = 0; i < value.Count; i++)
			{
				Pickupable pickupable = value[i].obj as Pickupable;
				if (!(pickupable == null))
				{
					bool flag = pickupable.KPrefabID.HasTag(GameTags.BaseMinion);
					if (deleteMinions || !flag)
					{
						Util.KDestroyGameObject(pickupable.gameObject);
					}
				}
			}
		}
	}
}
