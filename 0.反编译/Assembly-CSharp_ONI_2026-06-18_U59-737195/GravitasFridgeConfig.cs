using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class GravitasFridgeConfig : IEntityConfig
{
	public GameObject CreatePrefab()
	{
		GameObject obj = EntityTemplates.CreatePlacedEntity("GravitasFridge", STRINGS.BUILDINGS.PREFABS.PROPGRAVITASFRIDGE.NAME, STRINGS.BUILDINGS.PREFABS.PROPGRAVITASFRIDGE.DESC, 50f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("gravitas_fridge_kanim"), initialAnim: "off", sceneLayer: Grid.SceneLayer.Building, width: 2, height: 2, permittedRotation: PermittedRotations.Unrotatable, orientation: Orientation.Neutral, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.Gravitas });
		PrimaryElement component = obj.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Steel);
		component.Temperature = 294.15f;
		LoreBearerUtil.AddLoreTo(obj, LoreBearerUtil.UnlockSpecificEntryThenNext("story_trait_hijackheadquarters_initial", UI.USERMENUACTIONS.READLORE.SEARCH_OBJECT_SUCCESS.SEARCH3, LoreBearerUtil.UnlockNextResearchNote, focus: true));
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
