using STRINGS;
using UnityEngine;

[EntityConfigOrder(2)]
public class SnailIronConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SnailIron";

	public const string BASE_TRAIT_ID = "SnailIronBaseTrait";

	public const string EGG_ID = "SnailIronEgg";

	public const int EGG_SORT_ORDER = 700;

	public static GameObject CreateSnail(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BaseSnailConfig.CreatePrefab(id, "SnailIronBaseTrait", name, desc, anim_file, is_baby, "iron_", 333.15f, 388.15f, 308.15f, 413.15f, "SnailIronShell");
		return EntityTemplates.ExtendEntityToWildCreature(prefab, SnailTuning.PEN_SIZE_PER_CREATURE, add_fixed_capturable_monitor: true);
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
		GameObject prefab = CreateSnail("SnailIron", CREATURES.SPECIES.SNAIL.VARIANT_IRON.NAME, CREATURES.SPECIES.SNAIL.VARIANT_IRON.DESC, "snail_kanim", is_baby: false);
		prefab = EntityTemplates.ExtendEntityToFertileCreature(prefab, this, "SnailIronEgg", CREATURES.SPECIES.SNAIL.VARIANT_IRON.EGG_NAME, CREATURES.SPECIES.SNAIL.VARIANT_IRON.DESC, "egg_snail_iron_kanim", SnailTuning.EGG_MASS, "SnailIronBaby", 15.000001f, 5f, SnailTuning.EGG_CHANCES_BASE, 700);
		Diet diet = new Diet(BaseSnailConfig.SulfurToObsidianDiet());
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
