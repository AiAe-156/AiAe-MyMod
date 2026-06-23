using STRINGS;
using TUNING;
using UnityEngine;

[EntityConfigOrder(2)]
public class PacuTropicalConfig : IEntityConfig
{
	public const string ID = "PacuTropical";

	public const string BASE_TRAIT_ID = "PacuTropicalBaseTrait";

	public const string EGG_ID = "PacuTropicalEgg";

	public static readonly EffectorValues DECOR = TUNING.BUILDINGS.DECOR.BONUS.TIER4;

	public const int EGG_SORT_ORDER = 502;

	public static GameObject CreatePacu(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BasePacuConfig.CreatePrefab(id, "PacuTropicalBaseTrait", name, desc, anim_file, is_baby, "trp_", 303.15f, 353.15f, 283.15f, 373.15f);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, PacuTuning.PEN_SIZE_PER_CREATURE, add_fixed_capturable_monitor: true);
		DecorProvider decorProvider = prefab.AddOrGet<DecorProvider>();
		decorProvider.SetValues(DECOR);
		return prefab;
	}

	public GameObject CreatePrefab()
	{
		GameObject prefab = CreatePacu("PacuTropical", STRINGS.CREATURES.SPECIES.PACU.VARIANT_TROPICAL.NAME, STRINGS.CREATURES.SPECIES.PACU.VARIANT_TROPICAL.DESC, "pacu_kanim", is_baby: false);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, PacuTuning.PEN_SIZE_PER_CREATURE, add_fixed_capturable_monitor: true);
		return EntityTemplates.ExtendEntityToFertileCreature(prefab, this as IHasDlcRestrictions, "PacuTropicalEgg", STRINGS.CREATURES.SPECIES.PACU.VARIANT_TROPICAL.EGG_NAME, STRINGS.CREATURES.SPECIES.PACU.VARIANT_TROPICAL.DESC, "egg_pacu_kanim", PacuTuning.EGG_MASS, PacuTuning.EGG_SHELL_RATIO, "PacuTropicalBaby", 15.000001f, 5f, PacuTuning.EGG_CHANCES_TROPICAL, 502, is_ranchable: true, add_fish_overcrowding_monitor: true, 0.75f, deprecated: false, preventEggFromDroppingProducts: false, PacuTuning.EGG_MASS);
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
