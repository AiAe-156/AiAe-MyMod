using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class CrabShellConfig : IEntityConfig
{
	public const string ID = "CrabShell";

	public static readonly Tag TAG = TagManager.Create("CrabShell");

	public const float ADULT_MASS = 60f;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateLooseEntity("CrabShell", ITEMS.INDUSTRIAL_PRODUCTS.CRAB_SHELL.NAME, ITEMS.INDUSTRIAL_PRODUCTS.CRAB_SHELL.DESC, 1f, unitMass: false, Assets.GetAnim("crabshell_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.9f, 0.6f, isPickupable: true, 0, SimHashes.Creature, new List<Tag>
		{
			GameTags.Organics,
			GameTags.MoltShell
		});
		gameObject.AddOrGet<EntitySplitter>();
		gameObject.AddOrGet<SimpleMassStatusItem>();
		EntitySizeVisualizer entitySizeVisualizer = gameObject.AddComponent<EntitySizeVisualizer>();
		entitySizeVisualizer.TierSetType = OreSizeVisualizerComponents.TiersSetType.PokeShells;
		EntityTemplates.CreateAndRegisterCompostableFromPrefab(gameObject);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		Compostable component = inst.GetComponent<Compostable>();
		component.OnDeserializeCb = delegate(KMonoBehaviour kMonoBehaviour)
		{
			if (SaveLoader.Instance.GameInfo.IsVersionOlderThan(7, 36))
			{
				PrimaryElement component2 = kMonoBehaviour.GetComponent<PrimaryElement>();
				PrimaryElement component3 = kMonoBehaviour.GetComponent<PrimaryElement>();
				if (component3 != null)
				{
					component3.MassPerUnit = 1f;
					component3.Mass = component3.Units * 60f;
				}
				KPrefabID component4 = kMonoBehaviour.GetComponent<KPrefabID>();
				if (component4 != null)
				{
					component4.RemoveTag(GameTags.IndustrialIngredient);
				}
			}
		};
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
