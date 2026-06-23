using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class PropGravitasFirstAidKitConfig : IEntityConfig
{
	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("PropGravitasFirstAidKit", STRINGS.BUILDINGS.PREFABS.PROPGRAVITASFIRSTAIDKIT.NAME, STRINGS.BUILDINGS.PREFABS.PROPGRAVITASFIRSTAIDKIT.DESC, 50f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER0, anim: Assets.GetAnim("gravitas_first_aid_kit_kanim"), initialAnim: "off", sceneLayer: Grid.SceneLayer.Building, width: 1, height: 1, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.Gravitas });
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Granite);
		component.Temperature = 294.15f;
		Workable workable = gameObject.AddOrGet<Workable>();
		workable.synchronizeAnims = false;
		workable.resetProgressOnStop = true;
		SetLocker setLocker = gameObject.AddOrGet<SetLocker>();
		setLocker.overrideAnim = "anim_interacts_clothingfactory_kanim";
		setLocker.dropOffset = new Vector2I(0, 1);
		gameObject.AddOrGet<Demolishable>();
		return gameObject;
	}

	public static string[][] GetLockerBaseContents()
	{
		string text = (DlcManager.FeatureRadiationEnabled() ? "BasicRadPill" : "IntermediateCure");
		return new string[2][]
		{
			new string[3] { "BasicCure", "BasicCure", "BasicCure" },
			new string[2] { text, text }
		};
	}

	public void OnPrefabInit(GameObject inst)
	{
		OccupyArea component = inst.GetComponent<OccupyArea>();
		component.objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
		SetLocker component2 = inst.GetComponent<SetLocker>();
		component2.possible_contents_ids = GetLockerBaseContents();
		component2.ChooseContents();
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
