using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class GoldBellyCrownConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "GoldBellyCrown";

	public const float MASS_PER_UNIT = 250f;

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
		GameObject gameObject = EntityTemplates.CreateLooseEntity("GoldBellyCrown", STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.GOLD_BELLY_CROWN.NAME, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.GOLD_BELLY_CROWN.DESC, 250f, unitMass: true, Assets.GetAnim("bammoth_crown_kanim"), "idle1", Grid.SceneLayer.Ore, EntityTemplates.CollisionShape.RECTANGLE, 0.6f, 0.5f, isPickupable: true, 0, SimHashes.GoldAmalgam, new List<Tag> { GameTags.PedestalDisplayable });
		KCollider2D component = gameObject.GetComponent<KCollider2D>();
		gameObject.AddTag(GameTags.IndustrialProduct);
		OccupyArea occupyArea = gameObject.AddOrGet<OccupyArea>();
		occupyArea.SetCellOffsets(EntityTemplates.GenerateOffsets(1, 1));
		DecorProvider decorProvider = gameObject.AddOrGet<DecorProvider>();
		decorProvider.SetValues(DECOR.BONUS.TIER2);
		decorProvider.overrideName = STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.GOLD_BELLY_CROWN.NAME;
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
