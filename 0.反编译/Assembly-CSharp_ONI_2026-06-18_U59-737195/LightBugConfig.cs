using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class LightBugConfig : IEntityConfig
{
	public const string ID = "LightBug";

	public const string BASE_TRAIT_ID = "LightBugBaseTrait";

	public const string EGG_ID = "LightBugEgg";

	private static float KG_ORE_EATEN_PER_CYCLE = 0.166f;

	private static float CALORIES_PER_KG_OF_ORE = LightBugTuning.STANDARD_CALORIES_PER_CYCLE / KG_ORE_EATEN_PER_CYCLE;

	public static int EGG_SORT_ORDER = 100;

	public static GameObject CreateLightBug(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BaseLightBugConfig.BaseLightBug(id, name, desc, anim_file, "LightBugBaseTrait", LIGHT2D.LIGHTBUG_COLOR, DECOR.BONUS.TIER4, is_baby);
		EntityTemplates.ExtendEntityToWildCreature(prefab, LightBugTuning.PEN_SIZE_PER_CREATURE);
		Trait trait = Db.Get().CreateTrait("LightBugBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, LightBugTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - LightBugTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 5f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 25f, name));
		HashSet<Tag> hashSet = new HashSet<Tag>
		{
			TagManager.Create(PrickleFruitConfig.ID),
			TagManager.Create("GrilledPrickleFruit")
		};
		if (DlcManager.IsContentSubscribed("DLC2_ID"))
		{
			hashSet.Add(TagManager.Create("HardSkinBerry"));
			hashSet.Add(TagManager.Create("CookedPikeapple"));
		}
		hashSet.Add(SimHashes.Phosphorite.CreateTag());
		GameObject gameObject = BaseLightBugConfig.SetupDiet(prefab, hashSet, Tag.Invalid, CALORIES_PER_KG_OF_ORE);
		gameObject.AddTag(GameTags.OriginalCreature);
		gameObject.GetComponent<KPrefabID>().AddTag(GameTags.LightSource);
		return gameObject;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = CreateLightBug("LightBug", STRINGS.CREATURES.SPECIES.LIGHTBUG.NAME, STRINGS.CREATURES.SPECIES.LIGHTBUG.DESC, "lightbug_kanim", is_baby: false);
		EntityTemplates.ExtendEntityToFertileCreature(gameObject, this as IHasDlcRestrictions, "LightBugEgg", STRINGS.CREATURES.SPECIES.LIGHTBUG.EGG_NAME, STRINGS.CREATURES.SPECIES.LIGHTBUG.DESC, "egg_lightbug_kanim", LightBugTuning.EGG_MASS, "LightBugBaby", 15.000001f, 5f, LightBugTuning.EGG_CHANCES_BASE, EGG_SORT_ORDER, is_ranchable: true, add_fish_overcrowding_monitor: false, 1f, deprecated: false, preventEggFromDroppingProducts: false, LightBugTuning.EGG_MASS * 0.2f);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
		BaseLightBugConfig.SetupLoopingSounds(inst);
	}
}
