using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class MissileSetLockerConfig : IEntityConfig
{
	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("MissileSetLocker", STRINGS.BUILDINGS.PREFABS.MISSILESETLOCKER.NAME, STRINGS.BUILDINGS.PREFABS.MISSILESETLOCKER.DESC, 100f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("armoury_locker_kanim"), initialAnim: "on", sceneLayer: Grid.SceneLayer.Building, width: 1, height: 2, element: SimHashes.Creature, additionalTags: new List<Tag>
		{
			GameTags.Gravitas,
			GameTags.TemplateBuilding
		});
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Unobtanium);
		component.Temperature = 294.15f;
		Workable workable = gameObject.AddOrGet<Workable>();
		workable.synchronizeAnims = true;
		workable.resetProgressOnStop = true;
		SetLocker setLocker = gameObject.AddOrGet<SetLocker>();
		setLocker.overrideAnim = "anim_interacts_locker_kanim";
		setLocker.skipAnim = true;
		setLocker.dropOffset = new Vector2I(0, 1);
		setLocker.numDataBanks = new int[2] { 1, 4 };
		LoreBearerUtil.AddLoreTo(gameObject);
		gameObject.AddOrGet<OccupyArea>().objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
		gameObject.AddOrGet<Demolishable>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		SetLocker component = inst.GetComponent<SetLocker>();
		component.possible_contents_ids = new string[1][] { new string[1] { "MissileLongRange" } };
		component.ChooseContents();
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
