using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class ResearchDatabankConfig : IEntityConfig
{
	public const string ID = "ResearchDatabank";

	public static readonly Tag TAG = TagManager.Create("ResearchDatabank");

	public const float MASS = 1f;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateLooseEntity("ResearchDatabank", STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.RESEARCH_DATABANK.NAME, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.RESEARCH_DATABANK.DESC, 1f, unitMass: true, Assets.GetAnim("floppy_disc_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.CIRCLE, 0.35f, 0.35f, isPickupable: true, 0, SimHashes.Creature, new List<Tag>
		{
			GameTags.TechComponents,
			GameTags.Experimental,
			GameTags.PedestalDisplayable
		});
		if (DlcManager.FeatureClusterSpaceEnabled())
		{
			gameObject.AddTag(GameTags.HideFromSpawnTool);
		}
		gameObject.AddOrGet<EntitySplitter>().maxStackSize = ROCKETRY.DESTINATION_RESEARCH.BASIC;
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
		if (Game.IsDlcActiveForCurrentSave("DLC2_ID") && SaveLoader.Instance.ClusterLayout != null && SaveLoader.Instance.ClusterLayout.clusterTags.Contains("CeresCluster"))
		{
			inst.AddOrGet<KBatchedAnimController>().SwapAnims(new KAnimFile[1] { Assets.GetAnim("floppy_disc_ceres_kanim") });
		}
	}
}
