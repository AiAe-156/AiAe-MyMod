using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class POIDlc5TechUnlockConfig : IEntityConfig, IHasDlcRestrictions
{
	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("POIDlc5TechUnlock", STRINGS.BUILDINGS.PREFABS.DLC5POITECHUNLOCKS.NAME, STRINGS.BUILDINGS.PREFABS.DLC5POITECHUNLOCKS.DESC, 100f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("research_unlock_aquatic_kanim"), initialAnim: "on", sceneLayer: Grid.SceneLayer.Building, width: 3, height: 3, element: SimHashes.Creature, additionalTags: new List<Tag>
		{
			GameTags.Gravitas,
			GameTags.RoomProberBuilding,
			GameTags.LightSource
		});
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Unobtanium);
		component.Temperature = 294.15f;
		gameObject.AddOrGet<OccupyArea>().objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
		gameObject.AddOrGet<Demolishable>();
		POITechItemUnlockWorkable pOITechItemUnlockWorkable = gameObject.AddOrGet<POITechItemUnlockWorkable>();
		pOITechItemUnlockWorkable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_research_unlock_kanim") };
		pOITechItemUnlockWorkable.workTime = 5f;
		POITechItemUnlocks.Def def = gameObject.AddOrGetDef<POITechItemUnlocks.Def>();
		def.POITechUnlockIDs = new List<string> { "ReefGenerator", "PlasticGasket", "MeshTile" };
		def.PopUpName = STRINGS.BUILDINGS.PREFABS.DLC5POITECHUNLOCKS.NAME;
		def.animName = "tidal_turbine_kanim";
		def.loreUnlockId = "notes_marinea";
		Light2D light2D = gameObject.AddComponent<Light2D>();
		light2D.Color = LIGHT2D.POI_TECH_UNLOCK_COLOR;
		light2D.Range = 5f;
		light2D.Angle = 2.6f;
		light2D.Direction = LIGHT2D.POI_TECH_DIRECTION;
		light2D.Offset = LIGHT2D.POI_TECH_UNLOCK_OFFSET;
		light2D.overlayColour = LIGHT2D.POI_TECH_UNLOCK_OVERLAYCOLOR;
		light2D.shape = LightShape.Cone;
		light2D.drawOverlay = true;
		light2D.Lux = 1800;
		gameObject.AddOrGet<Prioritizable>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
