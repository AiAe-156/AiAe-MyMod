using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

[EntityConfigOrder(1)]
public class CrabConfig : IEntityConfig
{
	public const string ID = "Crab";

	public const string BASE_TRAIT_ID = "CrabBaseTrait";

	public const string EGG_ID = "CrabEgg";

	private const SimHashes EMIT_ELEMENT = SimHashes.Sand;

	private static float KG_ORE_EATEN_PER_CYCLE = 70f;

	private static float CALORIES_PER_KG_OF_ORE = CrabTuning.STANDARD_CALORIES_PER_CYCLE / KG_ORE_EATEN_PER_CYCLE;

	private static float MIN_POOP_SIZE_IN_KG = 25f;

	public static int EGG_SORT_ORDER = 0;

	public const float SHELL_DROP_COUNT = 60f;

	public const float SHELLFISH_MEAT_DROP_COUNT = 1.2f;

	public static GameObject CreateCrab(string id, string name, string desc, string anim_file, bool is_baby, string[] deathDropIDs, float[] deathDropCounts)
	{
		GameObject prefab = BaseCrabConfig.BaseCrab(id, name, desc, anim_file, "CrabBaseTrait", is_baby, null, deathDropIDs, deathDropCounts);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, CrabTuning.PEN_SIZE_PER_CREATURE);
		Trait trait = Db.Get().CreateTrait("CrabBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, CrabTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - CrabTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 25f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 100f, name));
		List<Diet.Info> diet_infos = BaseCrabConfig.BasicDiet(SimHashes.Sand.CreateTag(), CALORIES_PER_KG_OF_ORE, TUNING.CREATURES.CONVERSION_EFFICIENCY.NORMAL, null, 0f);
		prefab = BaseCrabConfig.SetupDiet(prefab, diet_infos, CALORIES_PER_KG_OF_ORE, MIN_POOP_SIZE_IN_KG);
		prefab.AddTag(GameTags.OriginalCreature);
		return prefab;
	}

	public GameObject CreatePrefab()
	{
		GameObject prefab = CreateCrab("Crab", STRINGS.CREATURES.SPECIES.CRAB.NAME, STRINGS.CREATURES.SPECIES.CRAB.DESC, "pincher_kanim", is_baby: false, new string[2] { "CrabShell", "ShellfishMeat" }, new float[2] { 60f, 1.2f });
		prefab = EntityTemplates.ExtendEntityToFertileCreature(prefab, this as IHasDlcRestrictions, "CrabEgg", STRINGS.CREATURES.SPECIES.CRAB.EGG_NAME, STRINGS.CREATURES.SPECIES.CRAB.DESC, "egg_pincher_kanim", CrabTuning.EGG_MASS, "CrabBaby", 60.000004f, 20f, CrabTuning.EGG_CHANCES_BASE, EGG_SORT_ORDER);
		EggProtectionMonitor.Def def = prefab.AddOrGetDef<EggProtectionMonitor.Def>();
		def.allyTags = new Tag[1] { GameTags.Creatures.CrabFriend };
		return prefab;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
