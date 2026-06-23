using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class GravitasBathroomMirrorConfig : IEntityConfig
{
	public const string ID = "GravitasBathroomMirror";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("GravitasBathroomMirror", STRINGS.BUILDINGS.PREFABS.PROPGRAVITASBATHROOMMIRROR.NAME, STRINGS.BUILDINGS.PREFABS.PROPGRAVITASBATHROOMMIRROR.DESC, 50f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("gravitas_bathroom_mirror_kanim"), initialAnim: "on", sceneLayer: Grid.SceneLayer.Building, width: 1, height: 1, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.Gravitas });
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Glass);
		component.Temperature = 294.15f;
		LoreBearerUtil.AddLoreTo(gameObject, LoreBearerUtil.UnlockSpecificEntryThenNext("story_trait_hijackheadquarters_mirror", UI.USERMENUACTIONS.READLORE.SEARCH_FLATOBJECT_SUCCESS.SEARCH1, LoreBearerUtil.UnlockNextJournalEntry, focus: true));
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
