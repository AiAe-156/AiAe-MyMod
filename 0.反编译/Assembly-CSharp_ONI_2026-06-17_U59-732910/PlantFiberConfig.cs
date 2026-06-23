using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class PlantFiberConfig : IEntityConfig
{
	public const string ID = "PlantFiber";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateLooseEntity("PlantFiber", ITEMS.INDUSTRIAL_PRODUCTS.PLANT_FIBER.NAME, ITEMS.INDUSTRIAL_PRODUCTS.PLANT_FIBER.DESC, 1f, unitMass: false, Assets.GetAnim("plant_matter_kanim"), "idle1", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.CIRCLE, 0.25f, 0.25f, isPickupable: true, 0, SimHashes.Creature, new List<Tag>
		{
			GameTags.IndustrialProduct,
			GameTags.PedestalDisplayable
		});
		gameObject.AddOrGet<EntitySplitter>();
		gameObject.AddComponent<EntitySizeVisualizer>().TierSetType = OreSizeVisualizerComponents.TiersSetType.PlantFiber;
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
