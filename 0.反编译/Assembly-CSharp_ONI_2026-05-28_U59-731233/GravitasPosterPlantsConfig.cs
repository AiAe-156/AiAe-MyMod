using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class GravitasPosterPlantsConfig : IEntityConfig
{
	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("GravitasPosterPlants", STRINGS.BUILDINGS.PREFABS.PROPGRAVITASPOSTERPLANTS.NAME, STRINGS.BUILDINGS.PREFABS.PROPGRAVITASPOSTERPLANTS.DESC, 25f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("gravitas_poster_plant_growth_kanim"), initialAnim: "off", sceneLayer: Grid.SceneLayer.Building, width: 2, height: 2, permittedRotation: PermittedRotations.Unrotatable, orientation: Orientation.Neutral, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.Gravitas });
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Granite);
		component.Temperature = 294.15f;
		LoreBearerUtil.AddLoreTo(gameObject, LoreBearerUtil.UnlockNextJournalEntry);
		gameObject.AddOrGet<Demolishable>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		OccupyArea component = inst.GetComponent<OccupyArea>();
		component.objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
