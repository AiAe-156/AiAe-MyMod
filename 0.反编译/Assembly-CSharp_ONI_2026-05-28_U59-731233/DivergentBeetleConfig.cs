using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

[EntityConfigOrder(1)]
public class DivergentBeetleConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "DivergentBeetle";

	public const string BASE_TRAIT_ID = "DivergentBeetleBaseTrait";

	public const string EGG_ID = "DivergentBeetleEgg";

	private const float LIFESPAN = 75f;

	private const SimHashes EMIT_ELEMENT = SimHashes.Sucrose;

	private static float KG_ORE_EATEN_PER_CYCLE = 20f;

	private static float CALORIES_PER_KG_OF_ORE = DivergentTuning.STANDARD_CALORIES_PER_CYCLE / KG_ORE_EATEN_PER_CYCLE;

	private static float MIN_POOP_SIZE_IN_KG = 4f;

	public static int EGG_SORT_ORDER = 0;

	public static GameObject CreateDivergentBeetle(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BaseDivergentConfig.BaseDivergent(id, name, desc, 50f, anim_file, is_baby ? null : "critter_build_kanim", "DivergentBeetleBaseTrait", is_baby);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, DivergentTuning.PEN_SIZE_PER_CREATURE);
		Trait trait = Db.Get().CreateTrait("DivergentBeetleBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, DivergentTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - DivergentTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 25f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 75f, name));
		List<Diet.Info> diet_infos = BaseDivergentConfig.BasicSulfurDiet(SimHashes.Sucrose.CreateTag(), CALORIES_PER_KG_OF_ORE, TUNING.CREATURES.CONVERSION_EFFICIENCY.NORMAL, null, 0f);
		prefab = BaseDivergentConfig.SetupDiet(prefab, diet_infos, CALORIES_PER_KG_OF_ORE, MIN_POOP_SIZE_IN_KG);
		prefab.AddTag(GameTags.OriginalCreature);
		return prefab;
	}

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject prefab = CreateDivergentBeetle("DivergentBeetle", STRINGS.CREATURES.SPECIES.DIVERGENT.VARIANT_BEETLE.NAME, STRINGS.CREATURES.SPECIES.DIVERGENT.VARIANT_BEETLE.DESC, "critter_kanim", is_baby: false);
		prefab = EntityTemplates.ExtendEntityToFertileCreature(prefab, this, "DivergentBeetleEgg", STRINGS.CREATURES.SPECIES.DIVERGENT.VARIANT_BEETLE.EGG_NAME, STRINGS.CREATURES.SPECIES.DIVERGENT.VARIANT_BEETLE.DESC, "egg_critter_kanim", DivergentTuning.EGG_MASS, "DivergentBeetleBaby", 45f, 15f, DivergentTuning.EGG_CHANCES_BEETLE, EGG_SORT_ORDER);
		prefab.AddTag(GameTags.Creatures.Pollinator);
		return prefab;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
