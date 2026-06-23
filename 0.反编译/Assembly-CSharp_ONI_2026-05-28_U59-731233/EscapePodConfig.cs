using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class EscapePodConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "EscapePod";

	public const float MASS = 100f;

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("EscapePod", STRINGS.BUILDINGS.PREFABS.ESCAPEPOD.NAME, STRINGS.BUILDINGS.PREFABS.ESCAPEPOD.DESC, 100f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("escape_pod_kanim"), initialAnim: "grounded", sceneLayer: Grid.SceneLayer.Building, width: 1, height: 2, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.RoomProberBuilding });
		KBatchedAnimController kBatchedAnimController = gameObject.AddOrGet<KBatchedAnimController>();
		kBatchedAnimController.fgLayer = Grid.SceneLayer.BuildingFront;
		TravellingCargoLander.Def def = gameObject.AddOrGetDef<TravellingCargoLander.Def>();
		def.landerWidth = 1;
		def.landingSpeed = 15f;
		def.deployOnLanding = true;
		CargoDropperMinion.Def def2 = gameObject.AddOrGetDef<CargoDropperMinion.Def>();
		def2.kAnimName = "anim_interacts_escape_pod_kanim";
		def2.animName = "deploying";
		def2.animLayer = Grid.SceneLayer.BuildingUse;
		def2.notifyOnJettison = true;
		BallisticClusterGridEntity ballisticClusterGridEntity = gameObject.AddOrGet<BallisticClusterGridEntity>();
		ballisticClusterGridEntity.clusterAnimName = "escape_pod01_kanim";
		ballisticClusterGridEntity.isWorldEntity = true;
		ballisticClusterGridEntity.nameKey = new StringKey("STRINGS.BUILDINGS.PREFABS.ESCAPEPOD.NAME");
		ClusterDestinationSelector clusterDestinationSelector = gameObject.AddOrGet<ClusterDestinationSelector>();
		clusterDestinationSelector.assignable = false;
		clusterDestinationSelector.shouldPointTowardsPath = true;
		clusterDestinationSelector.requireAsteroidDestination = true;
		clusterDestinationSelector.canNavigateFogOfWar = true;
		gameObject.AddOrGet<ClusterTraveler>();
		gameObject.AddOrGet<MinionStorage>();
		gameObject.AddOrGet<Prioritizable>();
		Prioritizable.AddRef(gameObject);
		gameObject.AddOrGet<Operational>();
		Deconstructable deconstructable = gameObject.AddOrGet<Deconstructable>();
		deconstructable.audioSize = "large";
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		OccupyArea component = inst.GetComponent<OccupyArea>();
		component.ApplyToCells = false;
		component.objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
