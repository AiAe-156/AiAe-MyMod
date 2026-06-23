using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(2)]
public class DieselMooConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "DieselMoo";

	public const string BASE_TRAIT_ID = "DieselMooBaseTrait";

	public static Tag POOP_ELEMENT = SimHashes.CarbonDioxide.CreateTag();

	public static SimHashes MILK_ELEMENT = SimHashes.RefinedLipid;

	public string[] GetRequiredDlcIds()
	{
		return null;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public static GameObject CreateMoo(string id, string name, string desc, string anim_file, List<BeckoningMonitor.SongChance> initialSongChances, bool is_baby)
	{
		GameObject gameObject = BaseMooConfig.BaseMoo(id, name, CREATURES.SPECIES.DIESELMOO.DESC, "DieselMooBaseTrait", anim_file, initialSongChances, is_baby, "die_");
		EntityTemplates.ExtendEntityToWildCreature(gameObject, MooTuning.PEN_SIZE_PER_CREATURE);
		Trait trait = Db.Get().CreateTrait("DieselMooBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, MooTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - MooTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 50f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, MooTuning.STANDARD_LIFESPAN, name));
		BaseMooConfig.SetupBaseDiet(gameObject, POOP_ELEMENT);
		BeckoningMonitor.Def def = gameObject.AddOrGetDef<BeckoningMonitor.Def>();
		def.effectId = "HuskyMooFed";
		MilkProductionMonitor.Def def2 = gameObject.AddOrGetDef<MilkProductionMonitor.Def>();
		def2.effectId = "HuskyMooWellFed";
		def2.element = MILK_ELEMENT;
		def2.Capacity = 800f;
		gameObject.AddTag(GameTags.OriginalCreature);
		return gameObject;
	}

	public GameObject CreatePrefab()
	{
		return CreateMoo("DieselMoo", CREATURES.SPECIES.DIESELMOO.NAME, CREATURES.SPECIES.DIESELMOO.DESC, "gassy_moo_kanim", MooTuning.DieselSongChances, is_baby: false);
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
		BaseMooConfig.OnSpawn(inst);
	}
}
