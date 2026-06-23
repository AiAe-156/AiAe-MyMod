using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(1)]
public class SeaFairyConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SeaFairy";

	public const string BASE_TRAIT_ID = "SeaFairyBaseTrait";

	public static GameObject CreateSeaFairy(string id, string name, string desc, string anim_file)
	{
		GameObject gameObject = BaseSeaFairyConfig.BaseSeaFairy(id, name, desc, anim_file, "SeaFairyBaseTrait");
		gameObject.AddOrGetDef<AgeMonitor.Def>();
		gameObject.AddOrGetDef<FixedCapturableMonitor.Def>();
		Trait trait = Db.Get().CreateTrait("SeaFairyBaseTrait", name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 5f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 5f, name));
		gameObject.AddTag(GameTags.OriginalCreature);
		return gameObject;
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
		return CreateSeaFairy("SeaFairy", CREATURES.SPECIES.SEAFAIRY.NAME, CREATURES.SPECIES.SEAFAIRY.DESC, "sea_fairy_kanim");
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
