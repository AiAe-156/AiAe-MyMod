using STRINGS;
using TUNING;
using UnityEngine;

public class OilChangerConfig : IBuildingConfig
{
	public const string ID = "OilChanger";

	public float OIL_CAPACITY = 400f;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC3;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("OilChanger", 3, 3, "oilchange_station_kanim", 30, 60f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.RAW_METALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		buildingDef.RequiresPowerInput = true;
		buildingDef.EnergyConsumptionWhenActive = 120f;
		buildingDef.Overheatable = false;
		buildingDef.ExhaustKilowattsWhenActive = 0.25f;
		buildingDef.SelfHeatKilowattsWhenActive = 0f;
		buildingDef.InputConduitType = ConduitType.Liquid;
		buildingDef.PowerInputOffset = new CellOffset(1, 0);
		buildingDef.UtilityInputOffset = new CellOffset(1, 2);
		buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
		buildingDef.AudioCategory = "Metal";
		buildingDef.PermittedRotations = PermittedRotations.Unrotatable;
		buildingDef.AddSearchTerms(SEARCH_TERMS.BIONIC);
		buildingDef.AddSearchTerms(SEARCH_TERMS.MEDICINE);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.BionicUpkeepType);
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(GameTags.CodexCategories.BionicBuilding);
		Storage storage = go.AddComponent<Storage>();
		storage.capacityKg = OIL_CAPACITY;
		storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		OilChangerWorkableUse oilChangerWorkableUse = go.AddOrGet<OilChangerWorkableUse>();
		oilChangerWorkableUse.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_oilchange_kanim") };
		oilChangerWorkableUse.resetProgressOnStop = true;
		ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
		conduitConsumer.forceAlwaysSatisfied = true;
		conduitConsumer.conduitType = ConduitType.Liquid;
		conduitConsumer.capacityTag = GameTags.LubricatingOil;
		conduitConsumer.capacityKG = OIL_CAPACITY;
		conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;
		go.AddOrGetDef<OilChanger.Def>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
