using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(1)]
public class SeaTurtleConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SeaTurtle";

	public const string BASE_TRAIT_ID = "SeaTurtleBaseTrait";

	public const string EGG_ID = "SeaTurtleEgg";

	public const int EGG_SORT_ORDER = 500;

	private static readonly KAnimHashedString[] SCALE_GROWTH_SYMBOL_NAMES = new KAnimHashedString[5] { "shell_0", "shell_1", "shell_2", "shell_3", "shell_4" };

	public static GameObject CreateSeaTurtle(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BaseSeaTurtleConfig.CreatePrefab(id, "SeaTurtleBaseTrait", name, desc, anim_file, is_baby, null, 273.15f, 333.15f, 253.15f, 373.15f);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, SeaTurtleTuning.PEN_SIZE_PER_CREATURE, add_fixed_capturable_monitor: true);
		EntityTemplates.CreateAndRegisterBaggedCreature(prefab, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		Trait trait = Db.Get().CreateTrait("SeaTurtleBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, SeaTurtleTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - SeaTurtleTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 50f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 100f, name));
		WellFedShearable.Def def = prefab.AddOrGetDef<WellFedShearable.Def>();
		def.effectId = "SeaTurtleWellFed";
		def.caloriesPerCycle = SeaTurtleTuning.STANDARD_CALORIES_PER_CYCLE;
		def.growthDurationCycles = SeaTurtleTuning.SCALE_GROWTH_TIME_IN_CYCLES;
		def.dropMass = SeaTurtleTuning.ORE_PER_CYCLE * SeaTurtleTuning.SCALE_GROWTH_TIME_IN_CYCLES;
		def.itemDroppedOnShear = ElementLoader.FindElementByHash(SimHashes.IronOre).tag;
		def.levelCount = 6;
		def.scaleGrowthSymbols = SCALE_GROWTH_SYMBOL_NAMES;
		prefab.AddTag(GameTags.OriginalCreature);
		return prefab;
	}

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject prefab = CreateSeaTurtle("SeaTurtle", CREATURES.SPECIES.SEATURTLE.NAME, CREATURES.SPECIES.SEATURTLE.DESC, "turtle_kanim", is_baby: false);
		prefab = EntityTemplates.ExtendEntityToFertileCreature(prefab, this, "SeaTurtleEgg", CREATURES.SPECIES.SEATURTLE.EGG_NAME, CREATURES.SPECIES.SEATURTLE.DESC, "egg_turtle_kanim", SeaTurtleTuning.EGG_MASS, SeaTurtleTuning.EGG_SHELL_RATIO, "SeaTurtleBaby", 60.000004f, 20f, SeaTurtleTuning.EGG_CHANCES_BASE, 500, is_ranchable: true, add_fish_overcrowding_monitor: true, 1f, deprecated: false, preventEggFromDroppingProducts: false, SeaTurtleTuning.EGG_MASS);
		prefab.AddTag(GameTags.LargeCreature);
		prefab.AddTag(GameTags.OriginalCreature);
		return prefab;
	}

	public void OnPrefabInit(GameObject prefab)
	{
		prefab.AddOrGet<LoopingSounds>();
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
