using STRINGS;
using UnityEngine;

[EntityConfigOrder(1)]
public class SnailConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Snail";

	public const string BASE_TRAIT_ID = "SnailBaseTrait";

	public const string EGG_ID = "SnailEgg";

	public const int EGG_SORT_ORDER = 700;

	public static GameObject CreateSnail(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BaseSnailConfig.CreatePrefab(id, "SnailBaseTrait", name, desc, anim_file, is_baby, null, 278.15f, 348.15f, 253.15f, 373.15f, "SnailShell");
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, SnailTuning.PEN_SIZE_PER_CREATURE, add_fixed_capturable_monitor: true);
		prefab.AddTag(GameTags.OriginalCreature);
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
		GameObject prefab = CreateSnail("Snail", CREATURES.SPECIES.SNAIL.NAME, CREATURES.SPECIES.SNAIL.DESC, "snail_kanim", is_baby: false);
		prefab = EntityTemplates.ExtendEntityToFertileCreature(prefab, this, "SnailEgg", CREATURES.SPECIES.SNAIL.EGG_NAME, CREATURES.SPECIES.SNAIL.DESC, "egg_snail_kanim", SnailTuning.EGG_MASS, 0f, "SnailBaby", 15.000001f, 5f, SnailTuning.EGG_CHANCES_BASE, 700, is_ranchable: true, add_fish_overcrowding_monitor: false, 1f, deprecated: false, preventEggFromDroppingProducts: false, SnailTuning.EGG_MASS);
		Diet diet = new Diet(BaseSnailConfig.SaltToDirtDiet());
		CreatureCalorieMonitor.Def def = prefab.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def.diet = diet;
		def.minConsumedCaloriesBeforePooping = SnailTuning.CALORIES_PER_KG_OF_ORE * SnailTuning.MIN_POOP_SIZE_IN_KG;
		SolidConsumerMonitor.Def def2 = prefab.AddOrGetDef<SolidConsumerMonitor.Def>();
		def2.diet = diet;
		return prefab;
	}

	public void OnPrefabInit(GameObject prefab)
	{
		prefab.AddOrGet<LoopingSounds>();
	}

	public void OnSpawn(GameObject inst)
	{
		KBatchedAnimController component = inst.GetComponent<KBatchedAnimController>();
		component.SetSymbolVisiblity((HashedString)"beached_limpetgrowth", is_visible: false);
		BaseSnailConfig.OnSpawn(inst);
	}
}
