using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class GravitasDeskConfig : IEntityConfig
{
	public GameObject CreatePrefab()
	{
		GameObject obj = EntityTemplates.CreatePlacedEntity("GravitasDesk", STRINGS.BUILDINGS.PREFABS.PROPGRAVITASDESK.NAME, STRINGS.BUILDINGS.PREFABS.PROPGRAVITASDESK.DESC, 50f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("gravitas_desk2_kanim"), initialAnim: "off", sceneLayer: Grid.SceneLayer.Building, width: 4, height: 3, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.Gravitas });
		PrimaryElement component = obj.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Steel);
		component.Temperature = 294.15f;
		LoreBearerUtil.AddLoreTo(obj, LoreBearerUtil.UnlockSpecificEntryThenNext("story_trait_hijackheadquarters_complete", UI.USERMENUACTIONS.READLORE.SEARCH_OBJECT_SUCCESS.SEARCH6, LoreBearerUtil.UnlockNextEmail, focus: true));
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
