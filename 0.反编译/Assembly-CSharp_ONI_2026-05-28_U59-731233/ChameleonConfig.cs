using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class ChameleonConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Chameleon";

	public const string BASE_TRAIT_ID = "ChameleonBaseTrait";

	public const string EGG_ID = "ChameleonEgg";

	public static Tag POOP_ELEMENT = SimHashes.BleachStone.CreateTag();

	private static float DRIPS_EATEN_PER_CYCLE = 1f;

	private static float CALORIES_PER_DRIP_EATEN = ChameleonTuning.STANDARD_CALORIES_PER_CYCLE / DRIPS_EATEN_PER_CYCLE;

	private static float KG_POOP_PER_DRIP = 10f;

	private static float MIN_POOP_SIZE_IN_KG = 10f;

	private static float MIN_POOP_SIZE_IN_CALORIES = CALORIES_PER_DRIP_EATEN * MIN_POOP_SIZE_IN_KG / KG_POOP_PER_DRIP;

	private static int LIFESPAN = 50;

	public static int EGG_SORT_ORDER = 800;

	public static GameObject CreateChameleon(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BaseChameleonConfig.BaseChameleon(id, name, desc, anim_file, "ChameleonBaseTrait", is_baby, null, 233.15f, 293.15f, 173.15f, 373.15f);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, ChameleonTuning.PEN_SIZE_PER_CREATURE);
		Trait trait = Db.Get().CreateTrait("ChameleonBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, ChameleonTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - ChameleonTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 25f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, LIFESPAN, name));
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add(DewDripConfig.ID.ToTag());
		Diet.Info[] infos = new Diet.Info[1]
		{
			new Diet.Info(hashSet, POOP_ELEMENT, CALORIES_PER_DRIP_EATEN, KG_POOP_PER_DRIP)
		};
		Diet diet = new Diet(infos);
		CreatureCalorieMonitor.Def def = prefab.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def.diet = diet;
		def.minConsumedCaloriesBeforePooping = MIN_POOP_SIZE_IN_CALORIES;
		SolidConsumerMonitor.Def def2 = prefab.AddOrGetDef<SolidConsumerMonitor.Def>();
		def2.diet = diet;
		prefab.AddOrGetDef<SetNavOrientationOnSpawnMonitor.Def>();
		prefab.AddTag(GameTags.OriginalCreature);
		EntityTemplates.AddSecondaryExcretion(prefab, SimHashes.ChlorineGas, 0.005f);
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

	public virtual GameObject CreatePrefab()
	{
		GameObject prefab = CreateChameleon("Chameleon", CREATURES.SPECIES.CHAMELEON.NAME, CREATURES.SPECIES.CHAMELEON.DESC, "chameleo_kanim", is_baby: false);
		return EntityTemplates.ExtendEntityToFertileCreature(prefab, this, "ChameleonEgg", CREATURES.SPECIES.CHAMELEON.EGG_NAME, CREATURES.SPECIES.CHAMELEON.DESC, "egg_chameleo_kanim", ChameleonTuning.EGG_MASS, "ChameleonBaby", 0.6f * (float)LIFESPAN, 0.2f * (float)LIFESPAN, ChameleonTuning.EGG_CHANCES_BASE, EGG_SORT_ORDER);
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
