using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class FossilSiteConfig_Rock : IEntityConfig
{
	public static readonly HashedString FossilQuestCriteriaID = "LostRockFossil";

	public const string ID = "FossilRock";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("FossilRock", CODEX.STORY_TRAITS.FOSSILHUNT.ENTITIES.FOSSIL_ROCK.NAME, CODEX.STORY_TRAITS.FOSSILHUNT.ENTITIES.FOSSIL_ROCK.DESC, 4000f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER4, noise: NOISE_POLLUTION.NOISY.TIER3, anim: Assets.GetAnim("fossil_rock_kanim"), initialAnim: "object", sceneLayer: Grid.SceneLayer.BuildingBack, width: 2, height: 2, element: SimHashes.Creature, additionalTags: new List<Tag> { GameTags.Gravitas });
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Fossil);
		component.Temperature = 315f;
		gameObject.AddOrGet<Operational>();
		EntombVulnerable entombVulnerable = gameObject.AddOrGet<EntombVulnerable>();
		gameObject.AddOrGet<Demolishable>().allowDemolition = false;
		MinorFossilDigSite.Def def = gameObject.AddOrGetDef<MinorFossilDigSite.Def>();
		def.fossilQuestCriteriaID = FossilQuestCriteriaID;
		FossilHuntInitializer.Def def2 = gameObject.AddOrGetDef<FossilHuntInitializer.Def>();
		MinorDigSiteWorkable minorDigSiteWorkable = gameObject.AddOrGet<MinorDigSiteWorkable>();
		Prioritizable prioritizable = gameObject.AddOrGet<Prioritizable>();
		Prioritizable.AddRef(gameObject);
		gameObject.AddOrGet<LoopingSounds>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		EntombVulnerable component = inst.GetComponent<EntombVulnerable>();
		component.SetStatusItem(Db.Get().BuildingStatusItems.FossilEntombed);
		OccupyArea component2 = inst.GetComponent<OccupyArea>();
		component2.objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
