using STRINGS;
using TUNING;
using UnityEngine;

public class FishDeliveryPointConfig : IBuildingConfig
{
	public const string ID = "FishDeliveryPoint";

	public const int STRAW_LENGTH = 4;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("FishDeliveryPoint", 1, 3, "fishrelocator_kanim", 10, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, MATERIALS.RAW_METALS, 1600f, BuildLocationRule.Anywhere, TUNING.BUILDINGS.DECOR.PENALTY.TIER2, NOISE_POLLUTION.NOISY.TIER0);
		buildingDef.AudioCategory = "Metal";
		buildingDef.Entombable = true;
		buildingDef.Floodable = false;
		buildingDef.ForegroundLayer = Grid.SceneLayer.TileMain;
		buildingDef.ViewMode = OverlayModes.Rooms.ID;
		buildingDef.AddSearchTerms(SEARCH_TERMS.RANCHING);
		buildingDef.AddSearchTerms(SEARCH_TERMS.CRITTER);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(GameTags.CodexCategories.CreatureRelocator);
		Storage storage = go.AddOrGet<Storage>();
		storage.allowItemRemoval = false;
		storage.showDescriptor = true;
		storage.storageFilters = STORAGEFILTERS.SWIMMING_CREATURES;
		storage.workAnims = new HashedString[1]
		{
			new HashedString("working_pre")
		};
		storage.workAnimPlayMode = KAnim.PlayMode.Once;
		storage.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_fishrelocator_kanim") };
		storage.synchronizeAnims = false;
		storage.useGunForDelivery = false;
		storage.allowSettingOnlyFetchMarkedItems = false;
		storage.faceTargetWhenWorking = false;
		CreatureDeliveryPoint creatureDeliveryPoint = go.AddOrGet<CreatureDeliveryPoint>();
		creatureDeliveryPoint.deliveryOffsets = new CellOffset[1]
		{
			new CellOffset(0, 1)
		};
		creatureDeliveryPoint.spawnOffset = new CellOffset(0, -1);
		creatureDeliveryPoint.largeCritterSpawnOffset = new CellOffset(0, -2);
		creatureDeliveryPoint.playAnimsOnFetch = true;
		BaggableCritterCapacityTracker baggableCritterCapacityTracker = go.AddOrGet<BaggableCritterCapacityTracker>();
		baggableCritterCapacityTracker.maximumCreatures = 20;
		baggableCritterCapacityTracker.cavityOffset = CellOffset.down;
		baggableCritterCapacityTracker.requireLiquidOffset = true;
		go.AddOrGet<TreeFilterable>();
		BuildingPointStraw buildingPointStraw = go.AddOrGet<BuildingPointStraw>();
		buildingPointStraw.canControlAnimStates = false;
		buildingPointStraw.usesSymbols = false;
		buildingPointStraw.maxDepth = 4;
		component.prefabInitFn += OnPrefabInit;
	}

	private void OnPrefabInit(GameObject instance)
	{
		KBatchedAnimController[] componentsInChildrenOnly = instance.GetComponentsInChildrenOnly<KBatchedAnimController>();
		foreach (KBatchedAnimController kBatchedAnimController in componentsInChildrenOnly)
		{
			kBatchedAnimController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.LiquidVisibilityLayer, isActive: false);
			kBatchedAnimController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.WaterProof, isActive: true);
		}
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		MakeBaseSolid.Def def = go.AddOrGetDef<MakeBaseSolid.Def>();
		def.solidOffsets = new CellOffset[1]
		{
			new CellOffset(0, 0)
		};
	}
}
