using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class FossilBitsSmallConfig : IEntityConfig
{
	public const string ID = "FossilBitsSmall";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("FossilBitsSmall", CODEX.STORY_TRAITS.FOSSILHUNT.ENTITIES.FOSSIL_BITS.NAME, CODEX.STORY_TRAITS.FOSSILHUNT.ENTITIES.FOSSIL_BITS.DESC, 1500f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0, noise: NOISE_POLLUTION.NOISY.TIER1, anim: Assets.GetAnim("fossil_bits1x2_kanim"), initialAnim: "object", sceneLayer: Grid.SceneLayer.BuildingBack, width: 1, height: 2, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.Gravitas });
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Fossil);
		component.Temperature = 315f;
		gameObject.AddOrGet<Operational>();
		gameObject.AddOrGet<EntombVulnerable>();
		FossilBits fossilBits = gameObject.AddOrGet<FossilBits>();
		Prioritizable prioritizable = gameObject.AddOrGet<Prioritizable>();
		Prioritizable.AddRef(gameObject);
		gameObject.AddOrGet<LoopingSounds>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		OccupyArea component = inst.GetComponent<OccupyArea>();
		component.objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
		EntombVulnerable component2 = inst.GetComponent<EntombVulnerable>();
		component2.SetStatusItem(Db.Get().BuildingStatusItems.FossilEntombed);
		component2.SetShowStatusItemOnEntombed(val: false);
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
