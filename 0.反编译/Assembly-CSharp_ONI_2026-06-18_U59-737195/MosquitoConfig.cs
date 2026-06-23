using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(1)]
public class MosquitoConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string BASE_TRAIT_ID = "MosquitoBaseTrait";

	public const string ID = "Mosquito";

	public const string EGG_ID = "MosquitoEgg";

	public static int EGG_SORT_ORDER = 300;

	public const int ADULT_LIFESPAN = 5;

	public const int BABY_LIFESPAN = 5;

	public const int LIFE_SPAN = 10;

	public static GameObject CreateMosquito(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject gameObject = BaseMosquitoConfig.BaseMosquito(id, name, desc, anim_file, "MosquitoBaseTrait", null, is_baby, 278.15f, 338.15f, 273.15f, 348.15f, "attack_pre", "attack_loop", "attack_pst", "STRINGS.CREATURES.STATUSITEMS.MOSQUITO_GOING_FOR_FOOD", "STRINGS.CREATURES.STATUSITEMS.EATING");
		gameObject.AddOrGetDef<AgeMonitor.Def>();
		Trait trait = Db.Get().CreateTrait("MosquitoBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 5f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 10f, name));
		gameObject.AddTag(GameTags.OriginalCreature);
		return gameObject;
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
		_ = (string)CREATURES.SPECIES.MOSQUITO.NAME;
		GameObject gameObject = EntityTemplates.ExtendEntityToFertileCreature(CreateMosquito("Mosquito", CREATURES.SPECIES.MOSQUITO.NAME, CREATURES.SPECIES.MOSQUITO.DESC, "mosquito_kanim", is_baby: false), this, "MosquitoEgg", CREATURES.SPECIES.MOSQUITO.EGG_NAME, CREATURES.SPECIES.MOSQUITO.DESC, "egg_mosquito_kanim", 1f, 0.5f, "MosquitoBaby", 4.5f, 2f, MosquitoTuning.EGG_CHANCES_BASE, EGG_SORT_ORDER, is_ranchable: false, add_fish_overcrowding_monitor: false, 0.75f, deprecated: false, preventEggFromDroppingProducts: true, 1f, allowEggCrackerRecipeCreation: false);
		gameObject.AddTag(GameTags.OriginalCreature);
		MosquitoHungerMonitor mosquitoHungerMonitor = gameObject.AddOrGet<MosquitoHungerMonitor>();
		mosquitoHungerMonitor.AllowedTargetTags = new List<Tag>
		{
			GameTags.BaseMinion,
			GameTags.Creature
		};
		mosquitoHungerMonitor.ForbiddenTargetTags = new List<Tag>
		{
			"Mosquito",
			GameTags.SwimmingCreature,
			GameTags.Dead,
			GameTags.HasAirtightSuit
		};
		gameObject.AddOrGetDef<AgeMonitor.Def>().minAgePercentOnSpawn = 0.5f;
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
