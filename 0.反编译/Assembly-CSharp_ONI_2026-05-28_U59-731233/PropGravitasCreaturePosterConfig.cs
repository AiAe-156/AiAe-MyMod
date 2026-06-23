using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class PropGravitasCreaturePosterConfig : IEntityConfig
{
	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("PropGravitasCreaturePoster", STRINGS.BUILDINGS.PREFABS.PROPGRAVITASCREATUREPOSTER.NAME, STRINGS.BUILDINGS.PREFABS.PROPGRAVITASCREATUREPOSTER.DESC, 50f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("gravitas_poster_kanim"), initialAnim: "off", sceneLayer: Grid.SceneLayer.Building, width: 2, height: 2, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.Gravitas });
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Granite);
		component.Temperature = 294.15f;
		LoreBearerUtil.AddLoreTo(gameObject, LoreBearerUtil.UnlockSpecificEntry("storytrait_crittermanipulator_workiversary", UI.USERMENUACTIONS.READLORE.SEARCH_PROPGRAVITASCREATUREPOSTER));
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
