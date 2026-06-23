using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class MissileBasicConfig : IEntityConfig
{
	public const string ID = "MissileBasic";

	public const float MASS_PER_MISSILE = 10f;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateLooseEntity("MissileBasic", ITEMS.MISSILE_BASIC.NAME, ITEMS.MISSILE_BASIC.DESC, 10f, unitMass: true, Assets.GetAnim("missile_kanim"), "object", Grid.SceneLayer.Ore, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true, 0, SimHashes.Iron, new List<Tag> { GameTags.PedestalDisplayable });
		gameObject.AddTag(GameTags.IndustrialProduct);
		gameObject.AddOrGetDef<MissileProjectile.Def>();
		EntitySplitter entitySplitter = gameObject.AddOrGet<EntitySplitter>();
		entitySplitter.maxStackSize = 50f;
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
