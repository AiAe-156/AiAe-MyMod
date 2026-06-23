using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class StaterpillarConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Staterpillar";

	public const string BASE_TRAIT_ID = "StaterpillarBaseTrait";

	public const string EGG_ID = "StaterpillarEgg";

	public const int EGG_SORT_ORDER = 0;

	private static float KG_ORE_EATEN_PER_CYCLE = 60f;

	private static float CALORIES_PER_KG_OF_ORE = StaterpillarTuning.STANDARD_CALORIES_PER_CYCLE / KG_ORE_EATEN_PER_CYCLE;

	public static GameObject CreateStaterpillar(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BaseStaterpillarConfig.BaseStaterpillar(id, name, desc, anim_file, "StaterpillarBaseTrait", is_baby, ObjectLayer.Wire, StaterpillarGeneratorConfig.ID, Tag.Invalid, null, 283.15f, 313.15f, 173.15f, 373.15f);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, TUNING.CREATURES.SPACE_REQUIREMENTS.TIER3);
		Trait trait = Db.Get().CreateTrait("StaterpillarBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, StaterpillarTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - StaterpillarTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 25f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 100f, name));
		List<Diet.Info> list = new List<Diet.Info>();
		list.AddRange(BaseStaterpillarConfig.RawMetalDiet(SimHashes.Hydrogen.CreateTag(), CALORIES_PER_KG_OF_ORE, StaterpillarTuning.POOP_CONVERSTION_RATE, null, 0f));
		list.AddRange(BaseStaterpillarConfig.RefinedMetalDiet(SimHashes.Hydrogen.CreateTag(), CALORIES_PER_KG_OF_ORE, StaterpillarTuning.POOP_CONVERSTION_RATE, null, 0f));
		prefab = BaseStaterpillarConfig.SetupDiet(prefab, list);
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

	public virtual GameObject CreatePrefab()
	{
		GameObject prefab = CreateStaterpillar("Staterpillar", STRINGS.CREATURES.SPECIES.STATERPILLAR.NAME, STRINGS.CREATURES.SPECIES.STATERPILLAR.DESC, "caterpillar_kanim", is_baby: false);
		return EntityTemplates.ExtendEntityToFertileCreature(prefab, this, "StaterpillarEgg", STRINGS.CREATURES.SPECIES.STATERPILLAR.EGG_NAME, STRINGS.CREATURES.SPECIES.STATERPILLAR.DESC, "egg_caterpillar_kanim", StaterpillarTuning.EGG_MASS, "StaterpillarBaby", 60.000004f, 20f, StaterpillarTuning.EGG_CHANCES_BASE, 0);
	}

	public void OnPrefabInit(GameObject prefab)
	{
		KBatchedAnimController component = prefab.GetComponent<KBatchedAnimController>();
		component.SetSymbolVisiblity("gulp", is_visible: false);
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
