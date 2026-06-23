using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class NoriConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Nori";

	public const float KCAL_PER_UNIT = 400000f;

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
		GameObject gameObject = EntityTemplates.CreateLooseEntity("Nori", STRINGS.ITEMS.FOOD.NORI.NAME, STRINGS.ITEMS.FOOD.NORI.DESC, 1f, unitMass: false, Assets.GetAnim("nori_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.CIRCLE, 0.25f, 0.25f, isPickupable: true, 0, SimHashes.Creature, new List<Tag> { GameTags.PedestalDisplayable });
		gameObject.AddOrGet<EntitySplitter>();
		EntityTemplates.ExtendEntityToFood(gameObject, FOOD.FOOD_TYPES.NORI);
		EntityTemplates.CreateAndRegisterCompostableFromPrefab(gameObject);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
