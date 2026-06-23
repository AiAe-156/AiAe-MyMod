using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class FeatherFabricConfig : IEntityConfig, IHasDlcRestrictions
{
	public static string ID = "FeatherFabric";

	private AttributeModifier decorModifier = new AttributeModifier("Decor", 0.1f, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.BASIC_FABRIC.NAME, is_multiplier: true);

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
		GameObject gameObject = EntityTemplates.CreateLooseEntity(ID, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.FEATHER_FABRIC.NAME, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.FEATHER_FABRIC.DESC, 1f, unitMass: true, Assets.GetAnim("feather_kanim"), "object", Grid.SceneLayer.BuildingBack, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.45f, isPickupable: true, SORTORDER.BUILDINGELEMENTS + BasicFabricTuning.SORTORDER, SimHashes.Creature, new List<Tag>
		{
			GameTags.IndustrialIngredient,
			GameTags.BuildingFiber,
			GameTags.PedestalDisplayable
		});
		gameObject.AddOrGet<EntitySplitter>();
		KBoxCollider2D kBoxCollider2D = gameObject.AddOrGet<KBoxCollider2D>();
		kBoxCollider2D.offset = new Vector2f(0f, 0.3f);
		kBoxCollider2D.size = new Vector2f(0.8f, 0.8f);
		PrefabAttributeModifiers prefabAttributeModifiers = gameObject.AddOrGet<PrefabAttributeModifiers>();
		prefabAttributeModifiers.AddAttributeDescriptor(decorModifier);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
