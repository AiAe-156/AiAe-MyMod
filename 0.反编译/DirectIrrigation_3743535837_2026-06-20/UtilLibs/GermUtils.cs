using Klei.AI.DiseaseGrowthRules;

namespace UtilLibs;

public static class GermUtils
{
	public static GrowthRule GrowthRule(float f_underPopulationDeathRate, float f_minCountPerKG, float f_populationHalfLife, float f_maxCountPerKG, float f_overPopulationHalfLife, int i_minDiffusionCount, float f_diffusionScale, byte b_minDiffusionInfestationTickCount)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		return new GrowthRule
		{
			underPopulationDeathRate = f_underPopulationDeathRate,
			minCountPerKG = f_minCountPerKG,
			populationHalfLife = f_populationHalfLife,
			maxCountPerKG = f_maxCountPerKG,
			overPopulationHalfLife = f_overPopulationHalfLife,
			minDiffusionCount = i_minDiffusionCount,
			diffusionScale = f_diffusionScale,
			minDiffusionInfestationTickCount = b_minDiffusionInfestationTickCount
		};
	}

	public static GrowthRule GrowthRule_Default()
	{
		return GrowthRule(2.666667f, 0.4f, 12000f, 500f, 1200f, 1000, 0.001f, 1);
	}

	public static StateGrowthRule StateGrowthRule_diffScale_minDiffCount(State state, float minCountPerKG, float populationHalfLife, float overPopulationHalfLife, float diffusionScale, int minDiffusionCount)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		StateGrowthRule val = new StateGrowthRule(state);
		((GrowthRule)val).minCountPerKG = minCountPerKG;
		((GrowthRule)val).populationHalfLife = populationHalfLife;
		((GrowthRule)val).overPopulationHalfLife = overPopulationHalfLife;
		((GrowthRule)val).diffusionScale = diffusionScale;
		((GrowthRule)val).minDiffusionCount = minDiffusionCount;
		return val;
	}

	public static StateGrowthRule StateGrowthRule_maxPerKg_diffScale_minDiffCount(State state, float minCountPerKG, float populationHalfLife, float overPopulationHalfLife, float maxCountPerKG, float diffusionScale, int minDiffusionCount)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		StateGrowthRule val = new StateGrowthRule(state);
		((GrowthRule)val).minCountPerKG = minCountPerKG;
		((GrowthRule)val).populationHalfLife = populationHalfLife;
		((GrowthRule)val).overPopulationHalfLife = overPopulationHalfLife;
		((GrowthRule)val).maxCountPerKG = maxCountPerKG;
		((GrowthRule)val).diffusionScale = diffusionScale;
		((GrowthRule)val).minDiffusionCount = minDiffusionCount;
		return val;
	}

	public static StateGrowthRule StateGrowthRule_maxPerKg_DiffScale(State state, float minCountPerKG, float populationHalfLife, float overPopulationHalfLife, float maxCountPerKG, float diffusionScale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		StateGrowthRule val = new StateGrowthRule(state);
		((GrowthRule)val).minCountPerKG = minCountPerKG;
		((GrowthRule)val).populationHalfLife = populationHalfLife;
		((GrowthRule)val).overPopulationHalfLife = overPopulationHalfLife;
		((GrowthRule)val).maxCountPerKG = maxCountPerKG;
		((GrowthRule)val).diffusionScale = diffusionScale;
		return val;
	}

	public static ElementGrowthRule ElementGrowthRule(SimHashes element, float? updr, float? phl, float? ophl, float? ds, float? min, float? max, int? mdc, byte? mditc)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementGrowthRule val = new ElementGrowthRule(element);
		((GrowthRule)val).underPopulationDeathRate = updr;
		((GrowthRule)val).populationHalfLife = phl;
		((GrowthRule)val).overPopulationHalfLife = ophl;
		((GrowthRule)val).diffusionScale = ds;
		((GrowthRule)val).minCountPerKG = min;
		((GrowthRule)val).maxCountPerKG = max;
		((GrowthRule)val).minDiffusionCount = mdc;
		((GrowthRule)val).minDiffusionInfestationTickCount = mditc;
		return val;
	}

	public static ElementGrowthRule DieInElement(SimHashes element, float scale = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementGrowthRule val = new ElementGrowthRule(element);
		((GrowthRule)val).populationHalfLife = 10f / scale;
		((GrowthRule)val).overPopulationHalfLife = 10f / scale;
		((GrowthRule)val).minDiffusionCount = (int)(100000f * scale);
		((GrowthRule)val).diffusionScale = 0.001f;
		return val;
	}

	public static ElementGrowthRule SurviveInElement(SimHashes element)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementGrowthRule val = new ElementGrowthRule(element);
		((GrowthRule)val).underPopulationDeathRate = 0f;
		((GrowthRule)val).populationHalfLife = float.PositiveInfinity;
		((GrowthRule)val).overPopulationHalfLife = 6000f;
		return val;
	}

	public static ElementGrowthRule SurviveAndSpreadInElement(SimHashes element)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementGrowthRule val = new ElementGrowthRule(element);
		((GrowthRule)val).underPopulationDeathRate = 0f;
		((GrowthRule)val).populationHalfLife = float.PositiveInfinity;
		((GrowthRule)val).overPopulationHalfLife = 3000f;
		((GrowthRule)val).maxCountPerKG = 1000f;
		((GrowthRule)val).diffusionScale = 0.005f;
		return val;
	}

	public static ElementGrowthRule ThriveInElement(SimHashes element, float scale = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementGrowthRule val = new ElementGrowthRule(element);
		((GrowthRule)val).underPopulationDeathRate = 0f;
		((GrowthRule)val).populationHalfLife = -3000f / scale;
		((GrowthRule)val).overPopulationHalfLife = 3000f * scale;
		return val;
	}

	public static ElementGrowthRule ThriveAndSpreadInElement(SimHashes element, float scale = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementGrowthRule val = new ElementGrowthRule(element);
		((GrowthRule)val).underPopulationDeathRate = 0f;
		((GrowthRule)val).populationHalfLife = -3000f / scale;
		((GrowthRule)val).overPopulationHalfLife = 3000f * scale;
		((GrowthRule)val).maxCountPerKG = 5000f * scale;
		((GrowthRule)val).diffusionScale = 0.05f;
		return val;
	}

	public static ElementExposureRule KillingExposure(SimHashes element, float scale = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementExposureRule val = new ElementExposureRule(element);
		((ExposureRule)val).populationHalfLife = 10f / scale;
		return val;
	}

	public static ElementGrowthRule GrowthLike_FoodPoison_PollutedOxygen(SimHashes element)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementGrowthRule val = new ElementGrowthRule(element);
		((GrowthRule)val).populationHalfLife = 12000f;
		((GrowthRule)val).maxCountPerKG = 10000f;
		((GrowthRule)val).overPopulationHalfLife = 3000f;
		((GrowthRule)val).diffusionScale = 0.05f;
		return val;
	}

	public static ElementGrowthRule GrowthLike_FoodPoison_PollutedWater(SimHashes element)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementGrowthRule val = new ElementGrowthRule(element);
		((GrowthRule)val).populationHalfLife = -12000f;
		((GrowthRule)val).overPopulationHalfLife = 12000f;
		return val;
	}

	public static TagGrowthRule GrowthLike_FoodPoison_Edible(Tag tag)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		TagGrowthRule val = new TagGrowthRule(tag);
		((GrowthRule)val).populationHalfLife = -12000f;
		((GrowthRule)val).overPopulationHalfLife = float.PositiveInfinity;
		return val;
	}

	public static ElementGrowthRule GrowthLike_Slimelung_PollutedOxygen(SimHashes element)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementGrowthRule val = new ElementGrowthRule(element);
		((GrowthRule)val).underPopulationDeathRate = 0f;
		((GrowthRule)val).populationHalfLife = -300f;
		((GrowthRule)val).overPopulationHalfLife = 1200f;
		return val;
	}

	public static ElementGrowthRule GrowthLike_Slimelung_Oxygen(SimHashes element)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementGrowthRule val = new ElementGrowthRule(element);
		((GrowthRule)val).populationHalfLife = 1200f;
		((GrowthRule)val).overPopulationHalfLife = 10f;
		return val;
	}

	public static ElementExposureRule ExposureLike_Slimelung_PollutedOxygen(SimHashes element)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementExposureRule val = new ElementExposureRule(element);
		((ExposureRule)val).populationHalfLife = -12000f;
		return val;
	}

	public static ElementExposureRule ExposureLike_Anything_ChlorineGas(SimHashes element)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ElementExposureRule val = new ElementExposureRule(element);
		((ExposureRule)val).populationHalfLife = 10f;
		return val;
	}

	public static StateGrowthRule StateLike_Slimelung_Gas(State state)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		StateGrowthRule val = new StateGrowthRule(state);
		((GrowthRule)val).minCountPerKG = 250f;
		((GrowthRule)val).populationHalfLife = 12000f;
		((GrowthRule)val).overPopulationHalfLife = 1200f;
		((GrowthRule)val).maxCountPerKG = 10000f;
		((GrowthRule)val).minDiffusionCount = 5100;
		((GrowthRule)val).diffusionScale = 0.005f;
		return val;
	}
}
