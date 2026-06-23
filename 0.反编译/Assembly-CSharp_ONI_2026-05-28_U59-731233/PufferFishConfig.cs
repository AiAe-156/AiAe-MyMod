using STRINGS;
using UnityEngine;

[EntityConfigOrder(1)]
public class PufferFishConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "PufferFish";

	public const string BASE_TRAIT_ID = "PufferFishBaseTrait";

	public const string EGG_ID = "PufferFishEgg";

	public const int EGG_SORT_ORDER = 500;

	public static GameObject CreatePufferFish(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BasePufferFish.CreatePrefab(id, "PufferFishBaseTrait", name, desc, anim_file, is_baby, null, 273.15f, 333.15f, 253.15f, 373.15f);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, PufferFishTuning.PEN_SIZE_PER_CREATURE, add_fixed_capturable_monitor: true);
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
		GameObject prefab = CreatePufferFish("PufferFish", CREATURES.SPECIES.PUFFERFISH.NAME, CREATURES.SPECIES.PUFFERFISH.DESC, "blowfish_kanim", is_baby: false);
		prefab = EntityTemplates.ExtendEntityToFertileCreature(prefab, this, "PufferFishEgg", CREATURES.SPECIES.PUFFERFISH.EGG_NAME, CREATURES.SPECIES.PUFFERFISH.DESC, "egg_blowfish_kanim", PufferFishTuning.EGG_MASS, "PufferFishBaby", 15.000001f, 5f, PufferFishTuning.EGG_CHANCES_BASE, 500, is_ranchable: true, add_fish_overcrowding_monitor: true);
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
