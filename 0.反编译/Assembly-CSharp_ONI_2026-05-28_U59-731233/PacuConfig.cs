using STRINGS;
using UnityEngine;

[EntityConfigOrder(1)]
public class PacuConfig : IEntityConfig
{
	public const string ID = "Pacu";

	public const string BASE_TRAIT_ID = "PacuBaseTrait";

	public const string EGG_ID = "PacuEgg";

	public const int EGG_SORT_ORDER = 500;

	public static GameObject CreatePacu(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BasePacuConfig.CreatePrefab(id, "PacuBaseTrait", name, desc, anim_file, is_baby, null, 273.15f, 333.15f, 253.15f, 373.15f);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, PacuTuning.PEN_SIZE_PER_CREATURE, add_fixed_capturable_monitor: true);
		prefab.AddTag(GameTags.OriginalCreature);
		return prefab;
	}

	public GameObject CreatePrefab()
	{
		GameObject prefab = CreatePacu("Pacu", CREATURES.SPECIES.PACU.NAME, CREATURES.SPECIES.PACU.DESC, "pacu_kanim", is_baby: false);
		prefab = EntityTemplates.ExtendEntityToFertileCreature(prefab, this as IHasDlcRestrictions, "PacuEgg", CREATURES.SPECIES.PACU.EGG_NAME, CREATURES.SPECIES.PACU.DESC, "egg_pacu_kanim", PacuTuning.EGG_MASS, PacuTuning.EGG_SHELL_RATIO, "PacuBaby", 15.000001f, 5f, PacuTuning.EGG_CHANCES_BASE, 500, is_ranchable: true, add_fish_overcrowding_monitor: true, 0.75f, deprecated: false, preventEggFromDroppingProducts: false, PacuTuning.EGG_MASS);
		prefab.AddTag(GameTags.OriginalCreature);
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
