using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class OrbitalResearchDatabankConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "OrbitalResearchDatabank";

	public static readonly Tag TAG = TagManager.Create("OrbitalResearchDatabank");

	public const float MASS = 1f;

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateLooseEntity("OrbitalResearchDatabank", STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.ORBITAL_RESEARCH_DATABANK.NAME, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.ORBITAL_RESEARCH_DATABANK.DESC, 1f, unitMass: true, Assets.GetAnim("floppy_disc_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.CIRCLE, 0.35f, 0.35f, isPickupable: true, 0, SimHashes.Creature, new List<Tag>
		{
			GameTags.TechComponents,
			GameTags.Experimental,
			GameTags.PedestalDisplayable
		});
		EntitySplitter entitySplitter = gameObject.AddOrGet<EntitySplitter>();
		entitySplitter.maxStackSize = ROCKETRY.DESTINATION_RESEARCH.BASIC;
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
		if (Game.IsDlcActiveForCurrentSave("DLC2_ID") && SaveLoader.Instance.ClusterLayout != null && SaveLoader.Instance.ClusterLayout.clusterTags.Contains("CeresCluster"))
		{
			KBatchedAnimController kBatchedAnimController = inst.AddOrGet<KBatchedAnimController>();
			kBatchedAnimController.SwapAnims(new KAnimFile[1] { Assets.GetAnim("floppy_disc_ceres_kanim") });
		}
	}
}
