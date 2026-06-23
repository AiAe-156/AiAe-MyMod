using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(1)]
public class PrehistoricPacuConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "PrehistoricPacu";

	public const string BASE_TRAIT_ID = "PrehistoricPacuBaseTrait";

	public const string EGG_ID = "PrehistoricPacuEgg";

	public const int EGG_SORT_ORDER = 500;

	public static GameObject CreatePrehistoricPacu(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject gameObject = EntityTemplates.ExtendEntityToWildCreature(BasePrehistoricPacuConfig.CreatePrefab(id, "PrehistoricPacuBaseTrait", name, desc, anim_file, is_baby, null, 273.15f, 333.15f, 253.15f, 373.15f), PrehistoricPacuTuning.PEN_SIZE_PER_CREATURE, add_fixed_capturable_monitor: true);
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		Trait trait = Db.Get().CreateTrait("PrehistoricPacuBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, PrehistoricPacuTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - PrehistoricPacuTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 50f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 100f, name));
		gameObject.AddTag(GameTags.OriginalCreature);
		return gameObject;
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
		GameObject gameObject = EntityTemplates.ExtendEntityToFertileCreature(CreatePrehistoricPacu("PrehistoricPacu", CREATURES.SPECIES.PREHISTORICPACU.NAME, CREATURES.SPECIES.PREHISTORICPACU.DESC, "paculacanth_kanim", is_baby: false), this, "PrehistoricPacuEgg", CREATURES.SPECIES.PREHISTORICPACU.EGG_NAME, CREATURES.SPECIES.PREHISTORICPACU.DESC, "egg_paculacanth_kanim", PrehistoricPacuTuning.EGG_MASS, PrehistoricPacuTuning.EGG_SHELL_RATIO, "PrehistoricPacuBaby", 60.000004f, 20f, PrehistoricPacuTuning.EGG_CHANCES_BASE, 500, is_ranchable: true, add_fish_overcrowding_monitor: true, 0.75f, deprecated: false, preventEggFromDroppingProducts: false, PrehistoricPacuTuning.EGG_MASS);
		gameObject.AddTag(GameTags.LargeCreature);
		gameObject.AddTag(GameTags.OriginalCreature);
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
		prefab.AddOrGet<LoopingSounds>();
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
