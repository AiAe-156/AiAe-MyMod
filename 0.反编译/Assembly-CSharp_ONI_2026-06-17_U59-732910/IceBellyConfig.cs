using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(1)]
public class IceBellyConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "IceBelly";

	public const string BASE_TRAIT_ID = "IceBellyBaseTrait";

	public const string EGG_ID = "IceBellyEgg";

	public static Tag SCALE_GROWTH_EMIT_ELEMENT = BasicFabricConfig.ID;

	public static float SCALE_INITIAL_GROWTH_PCT = 0.25f;

	public static float SCALE_GROWTH_TIME_IN_CYCLES = 10f;

	public static float FIBER_PER_CYCLE = 0.5f;

	public static int EGG_SORT_ORDER = 0;

	public static GameObject CreateIceBelly(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject gameObject = EntityTemplates.ExtendEntityToWildCreature(BaseBellyConfig.BaseBelly(id, name, desc, anim_file, "IceBellyBaseTrait", is_baby), MooTuning.PEN_SIZE_PER_CREATURE);
		gameObject.AddOrGet<WarmBlooded>().BaseGenerationKW = 1.3f;
		Trait trait = Db.Get().CreateTrait("IceBellyBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, BellyTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - BellyTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 25f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 200f, name));
		gameObject.AddOrGet<DiseaseSourceVisualizer>().alwaysShowDisease = BellyTuning.GERM_ID_EMMITED_ON_POOP;
		WellFedShearable.Def def = gameObject.AddOrGetDef<WellFedShearable.Def>();
		def.effectId = "IceBellyWellFed";
		def.caloriesPerCycle = BellyTuning.STANDARD_CALORIES_PER_CYCLE;
		def.growthDurationCycles = SCALE_GROWTH_TIME_IN_CYCLES;
		def.dropMass = FIBER_PER_CYCLE * SCALE_GROWTH_TIME_IN_CYCLES;
		def.itemDroppedOnShear = SCALE_GROWTH_EMIT_ELEMENT;
		def.levelCount = 6;
		def.hideSymbols = GoldBellyConfig.SCALE_SYMBOLS;
		GameObject gameObject2 = BaseBellyConfig.SetupDiet(gameObject, BaseBellyConfig.StandardDiets(), BellyTuning.CALORIES_PER_UNIT_EATEN, 1f);
		gameObject2.AddTag(GameTags.OriginalCreature);
		return gameObject2;
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
		GameObject gameObject = EntityTemplates.ExtendEntityToFertileCreature(CreateIceBelly("IceBelly", CREATURES.SPECIES.ICEBELLY.NAME, CREATURES.SPECIES.ICEBELLY.DESC, "ice_belly_kanim", is_baby: false), this, "IceBellyEgg", CREATURES.SPECIES.ICEBELLY.EGG_NAME, CREATURES.SPECIES.ICEBELLY.DESC, "egg_icebelly_kanim", 8f, "IceBellyBaby", 120.00001f, 40f, BellyTuning.EGG_CHANCES_BASE, EGG_SORT_ORDER);
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
