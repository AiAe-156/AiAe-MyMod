using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class MissileLongRangeConfig : IEntityConfig
{
	public class DamageEventPayload
	{
		public int damage;

		public static DamageEventPayload sharedInstance = new DamageEventPayload();

		public DamageEventPayload(int damage = 10)
		{
			this.damage = damage;
		}
	}

	public const string ID = "MissileLongRange";

	public const float MASS_PER_MISSILE = 200f;

	public const int DAMAGE_PER_MISSILE = 10;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateLooseEntity("MissileLongRange", ITEMS.MISSILE_LONGRANGE.NAME, ITEMS.MISSILE_LONGRANGE.DESC, 200f, unitMass: true, Assets.GetAnim("longrange_missile_kanim"), "object", Grid.SceneLayer.Ore, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 1f, isPickupable: true, 0, SimHashes.Iron, new List<Tag>());
		gameObject.AddTag(GameTags.LongRangeMissile);
		gameObject.AddTag(GameTags.IndustrialProduct);
		gameObject.AddTag(GameTags.PedestalDisplayable);
		MissileLongRangeProjectile.Def def = gameObject.AddOrGetDef<MissileLongRangeProjectile.Def>();
		def.starmapOverrideSymbol = "payload";
		def.missileName = "STRINGS.ITEMS.MISSILE_LONGRANGE.NAME";
		def.missileDesc = "STRINGS.ITEMS.MISSILE_LONGRANGE.DESC";
		gameObject.AddOrGet<EntitySplitter>().maxStackSize = 200f;
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
