using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class GeneShufflerConfig : IEntityConfig
{
	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("GeneShuffler", STRINGS.BUILDINGS.PREFABS.GENESHUFFLER.NAME, STRINGS.BUILDINGS.PREFABS.GENESHUFFLER.DESC, 2000f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("geneshuffler_kanim"), initialAnim: "on", sceneLayer: Grid.SceneLayer.Building, width: 4, height: 3, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.Gravitas });
		gameObject.AddTag(GameTags.NotRoomAssignable);
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Unobtanium);
		component.Temperature = 294.15f;
		gameObject.AddOrGet<Operational>();
		gameObject.AddOrGet<Notifier>();
		gameObject.AddOrGet<GeneShuffler>();
		LoreBearerUtil.AddLoreTo(gameObject, LoreBearerUtil.NerualVacillator);
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGet<Ownable>();
		gameObject.AddOrGet<Prioritizable>();
		gameObject.AddOrGet<Demolishable>();
		Storage storage = gameObject.AddOrGet<Storage>();
		storage.dropOnLoad = true;
		ManualDeliveryKG manualDeliveryKG = gameObject.AddOrGet<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.MachineFetch.IdHash;
		manualDeliveryKG.RequestedItemTag = new Tag("GeneShufflerRecharge");
		manualDeliveryKG.refillMass = 1f;
		manualDeliveryKG.MinimumMass = 1f;
		manualDeliveryKG.capacity = 1f;
		KBatchedAnimController kBatchedAnimController = gameObject.AddOrGet<KBatchedAnimController>();
		kBatchedAnimController.sceneLayer = Grid.SceneLayer.BuildingBack;
		kBatchedAnimController.fgLayer = Grid.SceneLayer.BuildingFront;
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		GeneShuffler component = inst.GetComponent<GeneShuffler>();
		component.workLayer = Grid.SceneLayer.Building;
		Ownable component2 = inst.GetComponent<Ownable>();
		component2.slotID = Db.Get().AssignableSlots.GeneShuffler.Id;
		OccupyArea component3 = inst.GetComponent<OccupyArea>();
		component3.objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
		inst.GetComponent<Deconstructable>();
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
