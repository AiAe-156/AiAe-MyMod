using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

[EntityConfigOrder(2)]
public class AlgaeStegoConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "AlgaeStego";

	public const string BASE_TRAIT_ID = "AlgaeStegoBaseTrait";

	public const string EGG_ID = "AlgaeStegoEgg";

	public const int EGG_SORT_ORDER = 0;

	public const float VINE_FOOD_PER_CYCLE = 4f;

	public const float PRODUCT_PRODUCED_PER_CYCLE = 132f;

	public const SimHashes POOP_ELEMENT = SimHashes.Algae;

	public const float MIN_POOP_SIZE_IN_KG = 4f;

	public List<Emote> StegoEmotes = new List<Emote> { Db.Get().Emotes.Critter.Roar };

	public static GameObject CreateStego(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BaseStegoConfig.BaseStego(id, name, desc, anim_file, "AlgaeStegoBaseTrait", is_baby, "alg_");
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, StegoTuning.PEN_SIZE_PER_CREATURE);
		Trait trait = Db.Get().CreateTrait("AlgaeStegoBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, StegoTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - StegoTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 50f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 200f, name));
		return BaseStegoConfig.SetupDiet(prefab, Diets(), StegoTuning.CALORIES_PER_UNIT_EATEN, 4f);
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
		GameObject prefab = CreateStego("AlgaeStego", STRINGS.CREATURES.SPECIES.ALGAE_STEGO.NAME, STRINGS.CREATURES.SPECIES.ALGAE_STEGO.DESC, "stego_kanim", is_baby: false);
		prefab = EntityTemplates.ExtendEntityToFertileCreature(prefab, this, "AlgaeStegoEgg", STRINGS.CREATURES.SPECIES.ALGAE_STEGO.EGG_NAME, STRINGS.CREATURES.SPECIES.ALGAE_STEGO.DESC, "egg_stego_kanim", 8f, "AlgaeStegoBaby", 120.00001f, 40f, StegoTuning.EGG_CHANCES_ALGAE, 0);
		KBatchedAnimController component = prefab.GetComponent<KBatchedAnimController>();
		component.SetSymbolVisiblity("stego_eye_yellow", is_visible: false);
		component.SetSymbolVisiblity("stego_scale", is_visible: false);
		component.SetSymbolVisiblity("stego_pupil", is_visible: false);
		prefab.AddTag(GameTags.LargeCreature);
		return prefab;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}

	public static List<Diet.Info> Diets()
	{
		List<Diet.Info> list = new List<Diet.Info>();
		list.Add(new Diet.Info(new HashSet<Tag> { VineFruitConfig.ID }, SimHashes.Algae.CreateTag(), StegoTuning.CALORIES_PER_KG_OF_ORE, 33f));
		float num = FOOD.FOOD_TYPES.PRICKLEFRUIT.CaloriesPerUnit / FOOD.FOOD_TYPES.VINEFRUIT.CaloriesPerUnit;
		list.Add(new Diet.Info(new HashSet<Tag> { PrickleFruitConfig.ID }, SimHashes.Algae.CreateTag(), StegoTuning.CALORIES_PER_KG_OF_ORE * num, 132f / (4f / num)));
		if (DlcManager.IsExpansion1Active())
		{
			float num2 = FOOD.FOOD_TYPES.SWAMPFRUIT.CaloriesPerUnit / FOOD.FOOD_TYPES.VINEFRUIT.CaloriesPerUnit;
			float num3 = 1.5f;
			list.Add(new Diet.Info(new HashSet<Tag> { SwampFruitConfig.ID }, SimHashes.Algae.CreateTag(), StegoTuning.CALORIES_PER_KG_OF_ORE * num2, 132f / (4f / num2) * num3));
		}
		return list;
	}
}
