using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class MooConfig : IEntityConfig
{
	public const string ID = "Moo";

	public const string BASE_TRAIT_ID = "MooBaseTrait";

	public const SimHashes CONSUME_ELEMENT = SimHashes.Carbon;

	public static Tag POOP_ELEMENT = SimHashes.Methane.CreateTag();

	public static GameObject CreateMoo(string id, string name, string desc, string anim_file, List<BeckoningMonitor.SongChance> initialSongChances, bool is_baby)
	{
		GameObject gameObject = BaseMooConfig.BaseMoo(id, name, CREATURES.SPECIES.MOO.DESC, "MooBaseTrait", anim_file, initialSongChances, is_baby, null);
		EntityTemplates.ExtendEntityToWildCreature(gameObject, MooTuning.PEN_SIZE_PER_CREATURE);
		Trait trait = Db.Get().CreateTrait("MooBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, MooTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - MooTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 50f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, MooTuning.STANDARD_LIFESPAN, name));
		BaseMooConfig.SetupBaseDiet(gameObject, POOP_ELEMENT);
		gameObject.AddTag(GameTags.OriginalCreature);
		return gameObject;
	}

	public GameObject CreatePrefab()
	{
		return CreateMoo("Moo", CREATURES.SPECIES.MOO.NAME, CREATURES.SPECIES.MOO.DESC, "gassy_moo_kanim", MooTuning.BaseSongChances, is_baby: false);
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
		BaseMooConfig.OnSpawn(inst);
	}
}
