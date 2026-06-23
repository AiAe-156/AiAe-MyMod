using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class IceCavesForagePlantPlantedConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "IceCavesForagePlantPlanted";

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC2;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		string name = STRINGS.CREATURES.SPECIES.ICECAVESFORAGEPLANTPLANTED.NAME;
		string desc = STRINGS.CREATURES.SPECIES.ICECAVESFORAGEPLANTPLANTED.DESC;
		EffectorValues tIER = DECOR.BONUS.TIER1;
		KAnimFile anim = Assets.GetAnim("frozenberries_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.Hanging };
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("IceCavesForagePlantPlanted", name, desc, 100f, anim, "idle", Grid.SceneLayer.BuildingBack, 1, 2, tIER, default(EffectorValues), SimHashes.Creature, additionalTags, 253.15f);
		EntityTemplates.MakeHangingOffsets(gameObject, 1, 2);
		gameObject.AddOrGet<SimTemperatureTransfer>();
		OccupyArea occupyArea = gameObject.AddOrGet<OccupyArea>();
		occupyArea.objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
		gameObject.AddOrGet<EntombVulnerable>();
		gameObject.AddOrGet<DrowningMonitor>();
		gameObject.AddOrGet<Prioritizable>();
		gameObject.AddOrGet<Uprootable>();
		UprootedMonitor uprootedMonitor = gameObject.AddOrGet<UprootedMonitor>();
		uprootedMonitor.monitorCells = new CellOffset[1]
		{
			new CellOffset(0, 1)
		};
		gameObject.AddOrGet<Harvestable>();
		gameObject.AddOrGet<HarvestDesignatable>();
		SeedProducer seedProducer = gameObject.AddOrGet<SeedProducer>();
		seedProducer.Configure("IceCavesForagePlant", SeedProducer.ProductionType.DigOnly, 2);
		gameObject.AddOrGet<BasicForagePlantPlanted>();
		gameObject.AddOrGet<KBatchedAnimController>().randomiseLoopedOffset = true;
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
