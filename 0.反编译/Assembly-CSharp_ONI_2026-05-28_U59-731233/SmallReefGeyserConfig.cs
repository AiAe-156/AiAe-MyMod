using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class SmallReefGeyserConfig : IEntityConfig, IHasDlcRestrictions
{
	public static string ID = "SmallReefGeyser";

	private const float INHALE_RATE = 500f;

	private const float INHALE_TIME = 30f;

	private const float LIQUID_CAPACITY = 15000f;

	private const float EXHALE_TIME = 90f;

	private const float EXHALE_RATE = 166.66667f;

	public const float APPROXIMATE_EXHALE_TIME_PER_CYCLE = 450f;

	public GameObject CreatePrefab()
	{
		string iD = ID;
		string name = STRINGS.CREATURES.SPECIES.GEYSER.SMALLREEFGEYSER.NAME;
		string desc = STRINGS.CREATURES.SPECIES.GEYSER.SMALLREEFGEYSER.NAME;
		EffectorValues tIER = TUNING.BUILDINGS.DECOR.BONUS.TIER0;
		KAnimFile anim = Assets.GetAnim("geyser_reef_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.GeyserFeature };
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(iD, name, desc, 2000f, anim, "inactive", Grid.SceneLayer.Building, 3, 2, tIER, default(EffectorValues), SimHashes.Creature, additionalTags);
		gameObject.AddOrGet<OccupyArea>().objectLayers = new ObjectLayer[1] { ObjectLayer.Building };
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		component.SetElement(SimHashes.Katairite);
		component.Temperature = 305.15f;
		gameObject.AddOrGet<LoopingSounds>();
		Storage storage = BuildingTemplates.CreateDefaultStorage(gameObject);
		storage.capacityKg = 15000f;
		storage.showInUI = true;
		ElementConsumer elementConsumer = gameObject.AddOrGet<ElementConsumer>();
		elementConsumer.storeOnConsume = true;
		elementConsumer.configuration = ElementConsumer.Configuration.AllLiquid;
		elementConsumer.capacityKG = 15000f;
		elementConsumer.consumptionRate = 500f;
		elementConsumer.consumptionRadius = 1;
		elementConsumer.sampleCellOffset = new Vector3(0f, 1f);
		elementConsumer.overrideStatusItemString = STRINGS.CREATURES.SPECIES.GEYSER.SMALLREEFGEYSER.LIQUID_CONSUMPTION;
		BreathingGeyser.Def def = gameObject.AddOrGetDef<BreathingGeyser.Def>();
		def.inhaleRate = 500f;
		def.exhaleRate = 166.66667f;
		Submergable submergable = gameObject.AddOrGet<Submergable>();
		submergable.GetStatusItem = GetSubmergableStatusItem;
		BuildingAttachPoint buildingAttachPoint = gameObject.AddOrGet<BuildingAttachPoint>();
		buildingAttachPoint.points = new BuildingAttachPoint.HardPoint[1]
		{
			new BuildingAttachPoint.HardPoint(new CellOffset(0, 0), GameTags.ReefGenerator, null)
		};
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		Submergable submergable = inst.AddOrGet<Submergable>();
		submergable.GetStatusItem = GetSubmergableStatusItem;
	}

	public void OnSpawn(GameObject inst)
	{
		inst.GetComponent<KBatchedAnimController>().SetSymbolVisiblity("geotracker_target", is_visible: false);
	}

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	private static StatusItem GetSubmergableStatusItem()
	{
		return Db.Get().CreatureStatusItems.NotSubmerged;
	}
}
