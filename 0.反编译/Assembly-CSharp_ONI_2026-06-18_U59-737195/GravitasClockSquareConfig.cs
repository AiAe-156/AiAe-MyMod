using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class GravitasClockSquareConfig : IEntityConfig
{
	public GameObject CreatePrefab()
	{
		GameObject obj = EntityTemplates.CreatePlacedEntity("GravitasClockSquare", STRINGS.BUILDINGS.PREFABS.PROPGRAVITASCLOCKSQUARE.NAME, STRINGS.BUILDINGS.PREFABS.PROPGRAVITASCLOCKSQUARE.DESC, 50f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("gravitas_clock_square_kanim"), initialAnim: "off", sceneLayer: Grid.SceneLayer.Building, width: 1, height: 1, permittedRotation: PermittedRotations.Unrotatable, orientation: Orientation.Neutral, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.Gravitas });
		PrimaryElement component = obj.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Glass);
		component.Temperature = 294.15f;
		obj.AddOrGet<Demolishable>();
		return obj;
	}

	public void OnPrefabInit(GameObject inst)
	{
		inst.GetComponent<OccupyArea>().objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
