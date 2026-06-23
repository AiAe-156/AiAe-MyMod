using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class GravitasToiletPaperHolderConfig : IEntityConfig
{
	public const string ID = "GravitasToiletPaperHolder";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("GravitasToiletPaperHolder", STRINGS.BUILDINGS.PREFABS.PROPGRAVITASBATHROOMTOILETPAPERHOLDER.NAME, STRINGS.BUILDINGS.PREFABS.PROPGRAVITASBATHROOMTOILETPAPERHOLDER.DESC, 50f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("gravitas_toilet_paper_holder_kanim"), initialAnim: "on", sceneLayer: Grid.SceneLayer.Building, width: 1, height: 1, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.Gravitas });
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Granite);
		component.Temperature = 294.15f;
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
