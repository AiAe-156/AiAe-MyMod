using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ElementData;
using Klei.AI;
using STRINGS;
using UnityEngine;

[Serializable]
[DebuggerDisplay("{name}")]
public class Element : IComparable<Element>
{
	[Serializable]
	public enum State : byte
	{
		Vacuum = 0,
		Gas = 1,
		Liquid = 2,
		Solid = 3,
		Unbreakable = 4,
		Unstable = 8,
		TemperatureInsulated = 16
	}

	public const int INVALID_ID = 0;

	public SimHashes id;

	public Tag tag;

	public ushort idx;

	public float specificHeatCapacity;

	public float thermalConductivity = 1f;

	public float molarMass = 1f;

	public float strength;

	public float flow = 0f;

	public float maxCompression = 0f;

	public float viscosity = 0f;

	public float minHorizontalFlow = float.PositiveInfinity;

	public float minVerticalFlow = float.PositiveInfinity;

	public float maxMass = 10000f;

	public float solidSurfaceAreaMultiplier;

	public float liquidSurfaceAreaMultiplier;

	public float gasSurfaceAreaMultiplier;

	public State state;

	public byte hardness = 0;

	public float lowTemp;

	public SimHashes lowTempTransitionTarget;

	public Element lowTempTransition;

	public float highTemp;

	public SimHashes highTempTransitionTarget;

	public Element highTempTransition;

	public SimHashes highTempTransitionOreID = SimHashes.Vacuum;

	public float highTempTransitionOreMassConversion = 0f;

	public SimHashes lowTempTransitionOreID = SimHashes.Vacuum;

	public float lowTempTransitionOreMassConversion = 0f;

	public SimHashes sublimateId;

	public SimHashes convertId;

	public SpawnFXHashes sublimateFX;

	public float sublimateRate;

	public float sublimateEfficiency;

	public float sublimateProbability;

	public float offGasPercentage;

	public float lightAbsorptionFactor;

	public float radiationAbsorptionFactor;

	public float radiationPer1000Mass;

	public Sim.PhysicsData defaultValues;

	public SimHashes refinedMetalTarget;

	public float toxicity;

	public Substance substance;

	public Tag materialCategory;

	public int buildMenuSort;

	public ElementComposition[] elementComposition;

	public Tag[] oreTags = new Tag[0];

	public List<AttributeModifier> attributeModifiers = new List<AttributeModifier>();

	public bool disabled;

	public string dlcId;

	private const float MOLTEN_LIQUID_MIN_TEMP = 373.15f;

	public const byte StateMask = 3;

	public bool IsMoltenMetal => IsLiquid && substance.Metalic && lowTemp > 373.15f && HasTag(GameTags.Metal);

	public bool IsSlippery => HasTag(GameTags.Slippery);

	public bool IsUnstable => HasTag(GameTags.Unstable);

	public bool IsLiquid => (state & State.Solid) == State.Liquid;

	public bool IsGas => (state & State.Solid) == State.Gas;

	public bool IsSolid => (state & State.Solid) == State.Solid;

	public bool IsVacuum => (state & State.Solid) == 0;

	public bool IsTemperatureInsulated => (state & State.TemperatureInsulated) != 0;

	public bool HasTransitionUp => highTempTransitionTarget != 0 && highTempTransitionTarget != SimHashes.Unobtanium && highTempTransition != null && highTempTransition != this;

	public string name { get; set; }

	public string nameUpperCase { get; set; }

	public string description { get; set; }

	public float GetRelativeHeatLevel(float currentTemperature)
	{
		float num = lowTemp - 3f;
		float num2 = highTemp + 3f;
		return Mathf.Clamp01((currentTemperature - num) / (num2 - num));
	}

	public float PressureToMass(float pressure)
	{
		return pressure / defaultValues.pressure;
	}

	public bool IsState(State expected_state)
	{
		return (state & State.Solid) == expected_state;
	}

	public string GetStateString()
	{
		return GetStateString(state);
	}

	public static string GetStateString(State state)
	{
		if ((state & State.Solid) == State.Solid)
		{
			return ELEMENTS.STATE.SOLID;
		}
		if ((state & State.Solid) == State.Liquid)
		{
			return ELEMENTS.STATE.LIQUID;
		}
		if ((state & State.Solid) == State.Gas)
		{
			return ELEMENTS.STATE.GAS;
		}
		return ELEMENTS.STATE.VACUUM;
	}

	public string FullDescription(bool addHardnessColor = true)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		stringBuilder.Clear();
		stringBuilder.Append(Description());
		if (IsSolid)
		{
			stringBuilder.Append("\n\n");
			stringBuilder.AppendFormat(ELEMENTS.ELEMENTDESCSOLID, GetMaterialCategoryTag().ProperName(), GameUtil.GetFormattedTemperature(highTemp), GameUtil.GetHardnessString(this, addHardnessColor));
		}
		else if (IsLiquid)
		{
			stringBuilder.Append("\n\n");
			stringBuilder.AppendFormat(ELEMENTS.ELEMENTDESCLIQUID, GetMaterialCategoryTag().ProperName(), GameUtil.GetFormattedTemperature(lowTemp), GameUtil.GetFormattedTemperature(highTemp));
		}
		else if (!IsVacuum)
		{
			stringBuilder.Append("\n\n");
			stringBuilder.AppendFormat(ELEMENTS.ELEMENTDESCGAS, GetMaterialCategoryTag().ProperName(), GameUtil.GetFormattedTemperature(lowTemp));
		}
		StringBuilder stringBuilder2 = GlobalStringBuilderPool.Alloc();
		stringBuilder2.Append(ELEMENTS.THERMALPROPERTIES);
		stringBuilder2.Replace("{SPECIFIC_HEAT_CAPACITY}", GameUtil.GetFormattedSHC(specificHeatCapacity));
		stringBuilder2.Replace("{THERMAL_CONDUCTIVITY}", GameUtil.GetFormattedThermalConductivity(thermalConductivity));
		stringBuilder.Append("\n");
		stringBuilder.Append(stringBuilder2.ToString());
		GlobalStringBuilderPool.Free(stringBuilder2);
		if (DlcManager.FeatureRadiationEnabled())
		{
			stringBuilder.Append("\n");
			stringBuilder.AppendFormat(ELEMENTS.RADIATIONPROPERTIES, radiationAbsorptionFactor, GameUtil.GetFormattedRads(radiationPer1000Mass * 1.1f / 600f, GameUtil.TimeSlice.PerCycle));
		}
		if (oreTags.Length != 0 && !IsVacuum)
		{
			stringBuilder.Append("\n\n");
			StringBuilder stringBuilder3 = GlobalStringBuilderPool.Alloc();
			for (int i = 0; i < oreTags.Length; i++)
			{
				Tag item = new Tag(oreTags[i]);
				if (!GameTags.HiddenElementTags.Contains(item))
				{
					stringBuilder3.Append(item.ProperName());
					if (i < oreTags.Length - 1)
					{
						stringBuilder3.Append(", ");
					}
				}
			}
			stringBuilder.AppendFormat(ELEMENTS.ELEMENTPROPERTIES, GlobalStringBuilderPool.ReturnAndFree(stringBuilder3));
		}
		if (attributeModifiers.Count > 0)
		{
			foreach (AttributeModifier attributeModifier in attributeModifiers)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat(DUPLICANTS.MODIFIERS.MODIFIER_FORMAT, Db.Get().BuildingAttributes.Get(attributeModifier.AttributeId).Name, attributeModifier.GetFormattedString());
			}
		}
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public string Description()
	{
		return description;
	}

	public bool HasTag(Tag search_tag)
	{
		if (tag == search_tag)
		{
			return true;
		}
		return Array.IndexOf(oreTags, search_tag) != -1;
	}

	public Tag GetMaterialCategoryTag()
	{
		return materialCategory;
	}

	public int CompareTo(Element other)
	{
		return id - other.id;
	}
}
