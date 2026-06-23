using STRINGS;
using TUNING;
using UnityEngine;

public class DiningTableConfig : IBuildingConfig
{
	public const string ID = "DiningTable";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("DiningTable", 1, 1, "diningtable_kanim", 10, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.ALL_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER1);
		buildingDef.WorkTime = 20f;
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.AddSearchTerms(SEARCH_TERMS.DINING);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.DiningTableType);
		go.AddOrGet<MessStation>();
		go.AddOrGet<AnimTileable>();
		go.AddOrGetDef<RocketUsageRestriction.Def>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<KAnimControllerBase>().initialAnim = "off";
		Ownable ownable = go.AddOrGet<Ownable>();
		ownable.slotID = Db.Get().AssignableSlots.MessStation.Id;
		ownable.canBePublic = true;
		ownable.priority = 10;
		Storage storage = BuildingTemplates.CreateDefaultStorage(go);
		storage.showInUI = true;
		storage.capacityKg = TableSaltTuning.SALTSHAKERSTORAGEMASS + CaviarTuning.STORAGEMASS;
		ManualDeliveryKG manualDeliveryKG = go.AddOrGet<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.RequestedItemTag = TableSaltConfig.ID.ToTag();
		manualDeliveryKG.capacity = TableSaltTuning.SALTSHAKERSTORAGEMASS;
		manualDeliveryKG.refillMass = TableSaltTuning.CONSUMABLE_RATE;
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.FoodFetch.IdHash;
		manualDeliveryKG.ShowStatusItem = false;
		ManualDeliveryKG manualDeliveryKG2 = go.AddComponent<ManualDeliveryKG>();
		manualDeliveryKG2.SetStorage(storage);
		manualDeliveryKG2.RequestedItemTag = CaviarConfig.TAG;
		manualDeliveryKG2.capacity = CaviarTuning.STORAGEMASS;
		manualDeliveryKG2.refillMass = CaviarTuning.CONSUMABLE_RATE;
		manualDeliveryKG2.choreTypeIDHash = Db.Get().ChoreTypes.FoodFetch.IdHash;
		manualDeliveryKG2.ShowStatusItem = false;
		go.AddOrGet<Reservable>();
		SymbolOverrideControllerUtil.AddToPrefab(go);
	}
}
