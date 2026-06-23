using TUNING;
using UnityEngine;

public class UnderwaterBreathingStationConfig : IBuildingConfig
{
	public const string ID = "UnderwaterBreathingStation";

	private const float OXYGEN_CONSUMPTION_RATE = 5f;

	private const float OXYGEN_STORAGE_CAPACITY = 25f;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("UnderwaterBreathingStation", 2, 2, "underwater_breathing_station_kanim", 30, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.ALL_METALS, 1600f, BuildLocationRule.OnBackWall, noise: NOISE_POLLUTION.NOISY.TIER0, decor: BUILDINGS.DECOR.NONE);
		buildingDef.Overheatable = false;
		buildingDef.Floodable = false;
		buildingDef.ViewMode = OverlayModes.Oxygen.ID;
		buildingDef.AudioCategory = "HollowMetal";
		buildingDef.EnergyConsumptionWhenActive = 60f;
		buildingDef.ExhaustKilowattsWhenActive = 0.125f;
		buildingDef.SelfHeatKilowattsWhenActive = 0.5f;
		buildingDef.InputConduitType = ConduitType.Gas;
		buildingDef.UtilityInputOffset = new CellOffset(1, 1);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		Prioritizable.AddRef(go);
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = 25f;
		storage.showInUI = true;
		storage.showCapacityStatusItem = true;
		storage.showCapacityAsMainStatus = true;
		storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
		conduitConsumer.conduitType = ConduitType.Gas;
		conduitConsumer.consumptionRate = 5f;
		conduitConsumer.capacityKG = 25f;
		conduitConsumer.capacityTag = GameTags.Breathable;
		conduitConsumer.forceAlwaysSatisfied = false;
		conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGetDef<UnderwaterBreathingStation.Def>();
		go.AddOrGet<UnderwaterBreathingLocation>();
		go.AddOrGet<UnderwaterBreathingLocationWorkable>();
	}
}
