using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(1)]
public class StegoConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Stego";

	public const string BASE_TRAIT_ID = "StegoBaseTrait";

	public const string EGG_ID = "StegoEgg";

	public static int EGG_SORT_ORDER;

	public List<Emote> StegoEmotes = new List<Emote> { Db.Get().Emotes.Critter.Roar };

	public static GameObject CreateStego(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BaseStegoConfig.BaseStego(id, name, desc, anim_file, "StegoBaseTrait", is_baby);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, StegoTuning.PEN_SIZE_PER_CREATURE);
		Trait trait = Db.Get().CreateTrait("StegoBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, StegoTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - StegoTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 50f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 200f, name));
		prefab = BaseStegoConfig.SetupDiet(prefab, BaseStegoConfig.StandardDiets(), StegoTuning.CALORIES_PER_UNIT_EATEN, StegoTuning.MIN_POOP_SIZE_IN_KG);
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
		GameObject prefab = CreateStego("Stego", CREATURES.SPECIES.STEGO.NAME, CREATURES.SPECIES.STEGO.DESC, "stego_kanim", is_baby: false);
		prefab = EntityTemplates.ExtendEntityToFertileCreature(prefab, this, "StegoEgg", CREATURES.SPECIES.STEGO.EGG_NAME, CREATURES.SPECIES.STEGO.DESC, "egg_stego_kanim", 8f, "StegoBaby", 120.00001f, 40f, StegoTuning.EGG_CHANCES_BASE, EGG_SORT_ORDER);
		prefab.AddTag(GameTags.LargeCreature);
		return prefab;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
