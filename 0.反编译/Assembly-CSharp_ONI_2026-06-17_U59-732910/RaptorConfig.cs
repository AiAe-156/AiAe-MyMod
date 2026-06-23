using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

[EntityConfigOrder(1)]
public class RaptorConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Raptor";

	public const string BASE_TRAIT_ID = "RaptorBaseTrait";

	public const string EGG_ID = "RaptorEgg";

	public static int EGG_SORT_ORDER = 0;

	public static float SCALE_GROWTH_TIME_IN_CYCLES = 4f;

	public static float SCALE_INITIAL_GROWTH_PCT = 0.9f;

	public static float FIBER_PER_CYCLE = 1f;

	public static Tag SCALE_GROWTH_EMIT_ELEMENT = FeatherFabricConfig.ID;

	public static KAnimHashedString[] SCALE_SYMBOLS = new KAnimHashedString[3] { "scale_0", "scale_1", "scale_2" };

	public List<Emote> RaptorEmotes = new List<Emote>
	{
		Db.Get().Emotes.Critter.Roar,
		Db.Get().Emotes.Critter.RaptorSignal
	};

	public static GameObject CreateRaptor(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BaseRaptorConfig.BaseRaptor(id, name, desc, anim_file, "RaptorBaseTrait", is_baby);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, TUNING.CREATURES.SPACE_REQUIREMENTS.TIER4);
		Trait trait = Db.Get().CreateTrait("RaptorBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, RaptorTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - RaptorTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 50f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 200f, name));
		prefab = BaseRaptorConfig.SetupDiet(prefab, BaseRaptorConfig.StandardDiets());
		WellFedShearable.Def def = prefab.AddOrGetDef<WellFedShearable.Def>();
		def.effectId = "RaptorWellFed";
		def.scaleGrowthSymbols = new KAnimHashedString[2] { "body_feathers", "tail_feather" };
		def.caloriesPerCycle = RaptorTuning.STANDARD_CALORIES_PER_CYCLE;
		def.growthDurationCycles = SCALE_GROWTH_TIME_IN_CYCLES;
		def.dropMass = FIBER_PER_CYCLE * SCALE_GROWTH_TIME_IN_CYCLES;
		def.itemDroppedOnShear = SCALE_GROWTH_EMIT_ELEMENT;
		def.levelCount = 2;
		prefab.AddTag(GameTags.OriginalCreature);
		return prefab;
	}

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC4;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.ExtendEntityToFertileCreature(CreateRaptor("Raptor", STRINGS.CREATURES.SPECIES.RAPTOR.NAME, STRINGS.CREATURES.SPECIES.RAPTOR.DESC, "raptor_kanim", is_baby: false), this, "RaptorEgg", STRINGS.CREATURES.SPECIES.RAPTOR.EGG_NAME, STRINGS.CREATURES.SPECIES.RAPTOR.DESC, "egg_raptor_kanim", 8f, "RaptorBaby", 120.00001f, 40f, RaptorTuning.EGG_CHANCES_BASE, EGG_SORT_ORDER);
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
