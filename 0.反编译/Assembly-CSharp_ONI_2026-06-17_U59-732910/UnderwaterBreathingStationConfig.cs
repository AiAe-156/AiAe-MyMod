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
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("UnderwaterBreathingStation", 2, 2, "underwater_breathing_station_kanim", 30, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.ALL_METALS, 1600f, BuildLocationRule.OnBackWall, noise: NOISE_POLLUTION.NOISY.TIER0, decor: BUILDINGS.DECOR.NONE);
		obj.Overheatable = false;
		obj.Floodable = false;
		obj.ViewMode = OverlayModes.Oxygen.ID;
		obj.AudioCategory = "HollowMetal";
		obj.EnergyConsumptionWhenActive = 60f;
		obj.ExhaustKilowattsWhenActive = 0.125f;
		obj.SelfHeatKilowattsWhenActive = 0.5f;
		obj.InputConduitType = ConduitType.Gas;
		obj.UtilityInputOffset = new CellOffset(1, 1);
		return obj;
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
