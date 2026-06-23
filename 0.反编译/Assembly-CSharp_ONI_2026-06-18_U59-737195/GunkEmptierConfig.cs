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
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("GunkEmptier", 3, 3, "gunkdump_station_kanim", 30, 60f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.RAW_METALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		obj.Overheatable = false;
		obj.ExhaustKilowattsWhenActive = 0.125f;
		obj.SelfHeatKilowattsWhenActive = 0f;
		obj.OutputConduitType = ConduitType.Liquid;
		obj.UtilityOutputOffset = new CellOffset(-1, 0);
		obj.ViewMode = OverlayModes.LiquidConduits.ID;
		obj.AudioCategory = "Metal";
		obj.PermittedRotations = PermittedRotations.Unrotatable;
		obj.AddSearchTerms(SEARCH_TERMS.TOILET);
		obj.AddSearchTerms(SEARCH_TERMS.BIONIC);
		return obj;
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
		go.AddOrGetDef<RocketUsageRestriction.Def>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
