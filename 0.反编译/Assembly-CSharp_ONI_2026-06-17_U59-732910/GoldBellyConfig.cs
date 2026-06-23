using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(2)]
public class GoldBellyConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "GoldBelly";

	public const string BASE_TRAIT_ID = "GoldBellyBaseTrait";

	public const string EGG_ID = "GoldBellyEgg";

	public const int GERMS_EMMITED_PER_KG_POOPED = 1000;

	public static Tag SCALE_GROWTH_EMIT_ELEMENT = "GoldBellyCrown";

	public static float SCALE_INITIAL_GROWTH_PCT = 0.25f;

	public const float SCALE_GROWTH_TIME_IN_CYCLES = 10f;

	public const float GOLD_PER_CYCLE = 25f;

	public static int EGG_SORT_ORDER = 0;

	public static KAnimHashedString[] SCALE_SYMBOLS = new KAnimHashedString[5] { "antler_0", "antler_1", "antler_2", "antler_3", "antler_4" };

	public static GameObject CreateGoldBelly(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject gameObject = EntityTemplates.ExtendEntityToWildCreature(BaseBellyConfig.BaseBelly(id, name, desc, anim_file, "GoldBellyBaseTrait", is_baby, "king_"), MooTuning.PEN_SIZE_PER_CREATURE);
		gameObject.AddOrGet<WarmBlooded>().BaseGenerationKW = 1.3f;
		Trait trait = Db.Get().CreateTrait("GoldBellyBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, BellyTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - BellyTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 50f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 200f, name));
		string alwaysShowDisease = "PollenGerms";
		gameObject.AddOrGet<DiseaseSourceVisualizer>().alwaysShowDisease = alwaysShowDisease;
		WellFedShearable.Def def = gameObject.AddOrGetDef<WellFedShearable.Def>();
		def.effectId = "GoldBellyWellFed";
		def.caloriesPerCycle = BellyTuning.STANDARD_CALORIES_PER_CYCLE;
		def.growthDurationCycles = 10f;
		def.dropMass = 250f;
		def.itemDroppedOnShear = SCALE_GROWTH_EMIT_ELEMENT;
		def.requiredDiet = "FriesCarrot";
		def.levelCount = 6;
		def.scaleGrowthSymbols = SCALE_SYMBOLS;
		return BaseBellyConfig.SetupDiet(gameObject, BaseBellyConfig.StandardDiets(), BellyTuning.CALORIES_PER_UNIT_EATEN, 1f);
	}

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC2;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.ExtendEntityToFertileCreature(CreateGoldBelly("GoldBelly", CREATURES.SPECIES.ICEBELLY.VARIANT_GOLD.NAME, CREATURES.SPECIES.ICEBELLY.VARIANT_GOLD.DESC, "ice_belly_kanim", is_baby: false), this, "GoldBellyEgg", CREATURES.SPECIES.ICEBELLY.VARIANT_GOLD.EGG_NAME, CREATURES.SPECIES.ICEBELLY.VARIANT_GOLD.DESC, "egg_icebelly_kanim", 8f, "GoldBellyBaby", 120.00001f, 40f, BellyTuning.EGG_CHANCES_GOLD, EGG_SORT_ORDER);
		gameObject.AddOrGetDef<OvercrowdingMonitor.Def>();
		gameObject.AddTag(GameTags.LargeCreature);
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
