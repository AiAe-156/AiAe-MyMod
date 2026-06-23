using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class PropDrySuitHanger : IEntityConfig, IHasDlcRestrictions
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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("PropDrySuitHanger", STRINGS.BUILDINGS.PREFABS.PROPDRYSUITHANGER.NAME, STRINGS.BUILDINGS.PREFABS.PROPDRYSUITHANGER.DESC, 50f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("unlock_clothing_aquatic_kanim"), initialAnim: "on", sceneLayer: Grid.SceneLayer.Building, width: 1, height: 2, element: SimHashes.Creature, additionalTags: new List<Tag>
		{
			GameTags.Gravitas,
			GameTags.RoomProberBuilding
		});
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.ZincOre);
		component.Temperature = 294.15f;
		OccupyArea component2 = gameObject.GetComponent<OccupyArea>();
		component2.objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
		Workable workable = gameObject.AddOrGet<Workable>();
		workable.synchronizeAnims = false;
		workable.resetProgressOnStop = true;
		SetLocker setLocker = gameObject.AddOrGet<SetLocker>();
		setLocker.overrideAnim = "anim_interacts_clothingfactory_kanim";
		setLocker.dropOffset = new Vector2I(0, 1);
		setLocker.dropOnDeconstruct = true;
		Deconstructable deconstructable = gameObject.AddOrGet<Deconstructable>();
		deconstructable.audioSize = "small";
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		SetLocker component = inst.GetComponent<SetLocker>();
		component.possible_contents_ids = new string[1][] { new string[1] { "DrySuit" } };
		component.ChooseContents();
	}

	public void OnSpawn(GameObject inst)
	{
		Deconstructable component = inst.GetComponent<Deconstructable>();
		component.SetWorkTime(5f);
	}
}
