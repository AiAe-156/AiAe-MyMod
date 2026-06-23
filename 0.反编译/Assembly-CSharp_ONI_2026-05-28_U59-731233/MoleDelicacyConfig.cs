using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

[EntityConfigOrder(2)]
public class MoleDelicacyConfig : IEntityConfig
{
	public const string ID = "MoleDelicacy";

	public const string BASE_TRAIT_ID = "MoleDelicacyBaseTrait";

	public const string EGG_ID = "MoleDelicacyEgg";

	private static float MIN_POOP_SIZE_IN_CALORIES = 2400000f;

	private static float CALORIES_PER_KG_OF_DIRT = 1000f;

	public static int EGG_SORT_ORDER = 800;

	public static float GINGER_GROWTH_TIME_IN_CYCLES = 8f;

	public static float GINGER_PER_CYCLE = 1f;

	public static Tag SHEAR_DROP_ELEMENT = GingerConfig.ID;

	public static float MIN_GROWTH_TEMPERATURE = 343.15f;

	public static float MAX_GROWTH_TEMPERATURE = 353.15f;

	public static float EGG_CHANCES_TEMPERATURE_MIN = 333.15f;

	public static float EGG_CHANCES_TEMPERATURE_MAX = 373.15f;

	public static GameObject CreateMole(string id, string name, string desc, string anim_file, bool is_baby = false)
	{
		GameObject gameObject = BaseMoleConfig.BaseMole(id, name, desc, "MoleDelicacyBaseTrait", anim_file, is_baby, 173.15f, 373.15f, 73.149994f, 773.15f, "del_", 5);
		gameObject.AddTag(GameTags.Creatures.Digger);
		EntityTemplates.ExtendEntityToWildCreature(gameObject, MoleTuning.PEN_SIZE_PER_CREATURE);
		Trait trait = Db.Get().CreateTrait("MoleDelicacyBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, MoleTuning.DELICACY_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - MoleTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 25f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 100f, name));
		List<Diet.Info> list = BaseMoleConfig.SimpleOreDiet(new List<Tag>
		{
			SimHashes.Regolith.CreateTag(),
			SimHashes.Dirt.CreateTag(),
			SimHashes.IronOre.CreateTag()
		}, CALORIES_PER_KG_OF_DIRT, TUNING.CREATURES.CONVERSION_EFFICIENCY.NORMAL);
		Diet diet = new Diet(list.ToArray());
		CreatureCalorieMonitor.Def def = gameObject.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def.diet = diet;
		def.minConsumedCaloriesBeforePooping = MIN_POOP_SIZE_IN_CALORIES;
		SolidConsumerMonitor.Def def2 = gameObject.AddOrGetDef<SolidConsumerMonitor.Def>();
		def2.diet = diet;
		OvercrowdingMonitor.Def def3 = gameObject.AddOrGetDef<OvercrowdingMonitor.Def>();
		def3.spaceRequiredPerCreature = 0;
		gameObject.AddOrGet<LoopingSounds>();
		if (!is_baby)
		{
			ElementGrowthMonitor.Def def4 = gameObject.AddOrGetDef<ElementGrowthMonitor.Def>();
			def4.defaultGrowthRate = 1f / GINGER_GROWTH_TIME_IN_CYCLES / 600f;
			def4.dropMass = GINGER_PER_CYCLE * GINGER_GROWTH_TIME_IN_CYCLES;
			def4.itemDroppedOnShear = SHEAR_DROP_ELEMENT;
			def4.levelCount = 5;
			def4.minTemperature = MIN_GROWTH_TEMPERATURE;
			def4.maxTemperature = MAX_GROWTH_TEMPERATURE;
		}
		else
		{
			gameObject.GetComponent<Modifiers>().initialAmounts.Add(Db.Get().Amounts.ElementGrowth.Id);
		}
		return gameObject;
	}

	public GameObject CreatePrefab()
	{
		GameObject prefab = CreateMole("MoleDelicacy", STRINGS.CREATURES.SPECIES.MOLE.VARIANT_DELICACY.NAME, STRINGS.CREATURES.SPECIES.MOLE.VARIANT_DELICACY.DESC, "driller_kanim");
		return EntityTemplates.ExtendEntityToFertileCreature(prefab, this as IHasDlcRestrictions, "MoleDelicacyEgg", STRINGS.CREATURES.SPECIES.MOLE.VARIANT_DELICACY.EGG_NAME, STRINGS.CREATURES.SPECIES.MOLE.VARIANT_DELICACY.DESC, "egg_driller_kanim", MoleTuning.EGG_MASS, "MoleDelicacyBaby", 60.000004f, 20f, MoleTuning.EGG_CHANCES_DELICACY, EGG_SORT_ORDER);
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
		SetSpawnNavType(inst);
	}

	public static void SetSpawnNavType(GameObject inst)
	{
		int cell = Grid.PosToCell(inst);
		Navigator component = inst.GetComponent<Navigator>();
		if (component != null)
		{
			if (Grid.IsSolidCell(cell))
			{
				component.SetCurrentNavType(NavType.Solid);
				inst.transform.SetPosition(Grid.CellToPosCBC(cell, Grid.SceneLayer.FXFront));
				KBatchedAnimController component2 = inst.GetComponent<KBatchedAnimController>();
				component2.SetSceneLayer(Grid.SceneLayer.FXFront);
			}
			else
			{
				KBatchedAnimController component3 = inst.GetComponent<KBatchedAnimController>();
				component3.SetSceneLayer(Grid.SceneLayer.Creatures);
			}
		}
	}
}
