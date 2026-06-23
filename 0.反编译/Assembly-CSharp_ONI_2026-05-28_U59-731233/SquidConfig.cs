using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(1)]
public class SquidConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Squid";

	public const string BASE_TRAIT_ID = "SquidBaseTrait";

	public const string EGG_ID = "SquidEgg";

	public const int EGG_SORT_ORDER = 500;

	public static GameObject CreateSquid(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BaseSquidConfig.CreatePrefab(id, "SquidBaseTrait", name, desc, anim_file, is_baby, null, 313.15f, 373.15f, 293.15f, 393.15f);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, SquidTuning.PEN_SIZE_PER_CREATURE, add_fixed_capturable_monitor: true);
		EntityTemplates.CreateAndRegisterBaggedCreature(prefab, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		Trait trait = Db.Get().CreateTrait("SquidBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, SquidTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - SquidTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 50f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 100f, name));
		return prefab;
	}

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject prefab = CreateSquid("Squid", CREATURES.SPECIES.SQUID.NAME, CREATURES.SPECIES.SQUID.DESC, "squid_kanim", is_baby: false);
		prefab = EntityTemplates.ExtendEntityToFertileCreature(prefab, this, "SquidEgg", CREATURES.SPECIES.SQUID.EGG_NAME, CREATURES.SPECIES.SQUID.DESC, "egg_squid_kanim", SquidTuning.EGG_MASS, 0f, "SquidBaby", 60.000004f, 20f, SquidTuning.EGG_CHANCES_BASE, 500, is_ranchable: true, add_fish_overcrowding_monitor: true, 1f, deprecated: false, preventEggFromDroppingProducts: false, SquidTuning.EGG_MASS);
		prefab.AddTag(GameTags.OriginalCreature);
		EggProtectionMonitor.Def def = prefab.AddOrGetDef<EggProtectionMonitor.Def>();
		def.defaultFaction = FactionManager.FactionID.Prey;
		def.allyTags = new Tag[1] { GameTags.Creatures.SquidFriend };
		def.eggTags = new List<Tag> { "SquidEgg".ToTag() };
		return prefab;
	}

	public void OnPrefabInit(GameObject prefab)
	{
		prefab.AddOrGet<LoopingSounds>();
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
