using STRINGS;
using TUNING;
using UnityEngine;

public class GunkEmptierConfig : IBuildingConfig
{
	public const string ID = "GunkEmptier";

	private static float STORAGE_CAPACITY = GunkMonitor.GUNK_CAPACITY * 1.5f;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC3;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("GunkEmptier", 3, 3, "gunkdump_station_kanim", 30, 60f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.RAW_METALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		buildingDef.Overheatable = false;
		buildingDef.ExhaustKilowattsWhenActive = 0.125f;
		buildingDef.SelfHeatKilowattsWhenActive = 0f;
		buildingDef.OutputConduitType = ConduitType.Liquid;
		buildingDef.UtilityOutputOffset = new CellOffset(-1, 0);
		buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
		buildingDef.AudioCategory = "Metal";
		buildingDef.PermittedRotations = PermittedRotations.Unrotatable;
		buildingDef.AddSearchTerms(SEARCH_TERMS.TOILET);
		buildingDef.AddSearchTerms(SEARCH_TERMS.BIONIC);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(GameTags.CodexCategories.BionicBuilding);
		component.AddTag(RoomConstraints.ConstraintTags.ToiletType);
		component.AddTag(RoomConstraints.ConstraintTags.FlushToiletType);
		Prioritizable.AddRef(go);
		Storage storage = go.AddComponent<Storage>();
		storage.capacityKg = STORAGE_CAPACITY;
		storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		go.AddOrGet<GunkEmptierWorkable>();
		go.AddOrGetDef<GunkEmptier.Def>();
		ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
		conduitDispenser.conduitType = ConduitType.Liquid;
		conduitDispenser.elementFilter = new SimHashes[1] { SimHashes.LiquidGunk };
		Ownable ownable = go.AddOrGet<Ownable>();
		ownable.slotID = Db.Get().AssignableSlots.Toilet.Id;
		ownable.canBePublic = true;
		RocketUsageRestriction.Def def = go.AddOrGetDef<RocketUsageRestriction.Def>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
