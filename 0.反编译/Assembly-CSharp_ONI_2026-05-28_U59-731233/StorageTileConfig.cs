using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class StorageTileConfig : IBuildingConfig
{
	public const string ANIM_NAME = "storagetile_kanim";

	public const string ID = "StorageTile";

	public static float CAPACITY = 1000f;

	private static readonly List<Storage.StoredItemModifier> StoredItemModifiers = new List<Storage.StoredItemModifier>
	{
		Storage.StoredItemModifier.Insulate,
		Storage.StoredItemModifier.Seal,
		Storage.StoredItemModifier.Hide
	};

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("StorageTile", 1, 1, "storagetile_kanim", 30, 30f, new float[2] { 100f, 100f }, new string[2] { "RefinedMetal", "Glasses" }, 800f, BuildLocationRule.Tile, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
		BuildingTemplates.CreateFoundationTileDef(buildingDef);
		buildingDef.Floodable = false;
		buildingDef.Entombable = false;
		buildingDef.Overheatable = false;
		buildingDef.UseStructureTemperature = false;
		buildingDef.AudioCategory = "Glass";
		buildingDef.AudioSize = "small";
		buildingDef.BaseTimeUntilRepair = -1f;
		buildingDef.SceneLayer = Grid.SceneLayer.TileMain;
		buildingDef.AddSearchTerms(SEARCH_TERMS.TILE);
		buildingDef.AddSearchTerms(SEARCH_TERMS.STORAGE);
		buildingDef.ConstructionOffsetFilter = BuildingDef.ConstructionOffsetFilter_OneDown;
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		SimCellOccupier simCellOccupier = go.AddOrGet<SimCellOccupier>();
		simCellOccupier.movementSpeedMultiplier = DUPLICANTSTATS.MOVEMENT_MODIFIERS.PENALTY_2;
		simCellOccupier.notifyOnMelt = true;
		Storage storage = go.AddOrGet<Storage>();
		storage.SetDefaultStoredItemModifiers(StoredItemModifiers);
		storage.capacityKg = CAPACITY;
		storage.showInUI = true;
		storage.allowItemRemoval = true;
		storage.showDescriptor = true;
		storage.storageFilters = STORAGEFILTERS.STORAGE_LOCKERS_STANDARD;
		storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
		storage.showCapacityStatusItem = true;
		storage.showCapacityAsMainStatus = true;
		StorageTileSwitchItemWorkable storageTileSwitchItemWorkable = go.AddOrGet<StorageTileSwitchItemWorkable>();
		TreeFilterable treeFilterable = go.AddOrGet<TreeFilterable>();
		treeFilterable.copySettingsEnabled = false;
		treeFilterable.dropIncorrectOnFilterChange = false;
		treeFilterable.preventAutoAddOnDiscovery = true;
		StorageTile.Def def = go.AddOrGetDef<StorageTile.Def>();
		def.MaxCapacity = CAPACITY;
		def.specialItemCases = new StorageTile.SpecificItemTagSizeInstruction[3]
		{
			new StorageTile.SpecificItemTagSizeInstruction(GameTags.AirtightSuit, 0.5f),
			new StorageTile.SpecificItemTagSizeInstruction(GameTags.Dehydrated, 0.6f),
			new StorageTile.SpecificItemTagSizeInstruction(GameTags.MoltShell, 0.5f)
		};
		go.AddOrGet<TileTemperature>();
		BuildingHP buildingHP = go.AddOrGet<BuildingHP>();
		buildingHP.destroyOnDamaged = true;
		Prioritizable.AddRef(go);
		RocketUsageRestriction.Def def2 = go.AddOrGetDef<RocketUsageRestriction.Def>();
		def2.restrictOperational = false;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<KBatchedAnimController>().initialBlendParameters = 4;
		GeneratedBuildings.RemoveLoopingSounds(go);
		go.GetComponent<KPrefabID>().AddTag(GameTags.FloorTiles);
	}
}
