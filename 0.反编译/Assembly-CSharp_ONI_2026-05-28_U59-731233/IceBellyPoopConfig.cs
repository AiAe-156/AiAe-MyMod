using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class IceBellyPoopConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "IceBellyPoop";

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC2;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateLooseEntity("IceBellyPoop", STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.ICE_BELLY_POOP.NAME, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.ICE_BELLY_POOP.DESC, 100f, unitMass: false, Assets.GetAnim("bammoth_poop_kanim"), "idle3", Grid.SceneLayer.BuildingBack, EntityTemplates.CollisionShape.CIRCLE, 0.4f, 0.4f, isPickupable: true, 0, SimHashes.Creature, new List<Tag> { GameTags.PedestalDisplayable });
		KCollider2D component = gameObject.GetComponent<KCollider2D>();
		component.offset = new Vector2(0f, 0.05f);
		gameObject.AddTag(GameTags.IndustrialProduct);
		OccupyArea occupyArea = gameObject.AddOrGet<OccupyArea>();
		occupyArea.SetCellOffsets(EntityTemplates.GenerateOffsets(1, 1));
		DecorProvider decorProvider = gameObject.AddOrGet<DecorProvider>();
		decorProvider.SetValues(DECOR.PENALTY.TIER3);
		decorProvider.overrideName = STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.ICE_BELLY_POOP.NAME;
		EntitySplitter entitySplitter = gameObject.AddOrGet<EntitySplitter>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
