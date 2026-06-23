using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class DewDripConfig : IEntityConfig, IHasDlcRestrictions
{
	public static string ID = "DewDrip";

	private AttributeModifier decorModifier = new AttributeModifier("Decor", 0.1f, ITEMS.INDUSTRIAL_PRODUCTS.DEWDRIP.NAME, is_multiplier: true);

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
		GameObject gameObject = EntityTemplates.CreateLooseEntity(ID, ITEMS.INDUSTRIAL_PRODUCTS.DEWDRIP.NAME, ITEMS.INDUSTRIAL_PRODUCTS.DEWDRIP.DESC, 1f, unitMass: true, Assets.GetAnim("brackorb_kanim"), "object", Grid.SceneLayer.BuildingBack, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.45f, isPickupable: true, 0, SimHashes.Creature, new List<Tag> { GameTags.IndustrialIngredient });
		gameObject.AddOrGet<EntitySplitter>();
		gameObject.AddOrGet<PrefabAttributeModifiers>().AddAttributeDescriptor(decorModifier);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
