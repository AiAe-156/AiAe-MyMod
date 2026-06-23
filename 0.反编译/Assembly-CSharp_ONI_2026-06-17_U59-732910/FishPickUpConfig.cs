using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class FishPickUpConfig : IBuildingConfig
{
	public const string ID = "FishPickUp";

	public const string INPUT_PORT = "FishPickUpInput";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("FishPickUp", 1, 3, "fishrelocator2_kanim", 10, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, MATERIALS.RAW_METALS, 1600f, BuildLocationRule.Anywhere, TUNING.BUILDINGS.DECOR.PENALTY.TIER2, NOISE_POLLUTION.NOISY.TIER0);
		buildingDef.AudioCategory = "Metal";
		buildingDef.Entombable = true;
		buildingDef.Floodable = false;
		buildingDef.ForegroundLayer = Grid.SceneLayer.TileMain;
		buildingDef.ViewMode = OverlayModes.Rooms.ID;
		buildingDef.LogicInputPorts = new List<LogicPorts.Port> { LogicPorts.Port.InputPort("FishPickUpInput", new CellOffset(0, 0), STRINGS.BUILDINGS.PREFABS.FISHPICKUP.LOGIC_INPUT.DESC, STRINGS.BUILDINGS.PREFABS.FISHPICKUP.LOGIC_INPUT.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.FISHPICKUP.LOGIC_INPUT.LOGIC_PORT_INACTIVE) };
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
		go.AddOrGet<TreeFilterable>();
		BaggableCritterCapacityTracker baggableCritterCapacityTracker = go.AddOrGet<BaggableCritterCapacityTracker>();
		baggableCritterCapacityTracker.maximumCreatures = 20;
		baggableCritterCapacityTracker.cavityOffset = CellOffset.down;
		baggableCritterCapacityTracker.requireLiquidOffset = true;
		BuildingPointStraw buildingPointStraw = go.AddOrGet<BuildingPointStraw>();
		buildingPointStraw.canControlAnimStates = false;
		buildingPointStraw.usesSymbols = false;
		Prioritizable.AddRef(go);
		component.prefabInitFn += OnPrefabInit;
	}

	private void OnPrefabInit(GameObject instance)
	{
		KBatchedAnimController[] componentsInChildrenOnly = instance.GetComponentsInChildrenOnly<KBatchedAnimController>();
		foreach (KBatchedAnimController obj in componentsInChildrenOnly)
		{
			obj.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.LiquidVisibilityLayer, isActive: false);
			obj.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.WaterProof, isActive: true);
		}
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGetDef<MakeBaseSolid.Def>().solidOffsets = new CellOffset[1]
		{
			new CellOffset(0, 0)
		};
		FixedCapturePoint.Def def = go.AddOrGetDef<FixedCapturePoint.Def>();
		def.onAnimName = "on";
		def.offAnimName = "off";
		def.isAmountStoredOverCapacity = delegate(FixedCapturePoint.Instance smi, FixedCapturableMonitor.Instance capturable)
		{
			TreeFilterable component = smi.GetComponent<TreeFilterable>();
			IUserControlledCapacity component2 = smi.GetComponent<IUserControlledCapacity>();
			float amountStored = component2.AmountStored;
			float userMaxCapacity = component2.UserMaxCapacity;
			return amountStored > userMaxCapacity && component.ContainsTag(capturable.PrefabTag);
		};
		def.allowBabies = true;
		def.captureCellOffset = new CellOffset(0, -1);
		def.rancherInteractOffset = new CellOffset(0, 1);
		def.postCaptureOffset = new CellOffset(0, 1);
		def.logicPortId = "FishPickUpInput";
		def.preCaptureAnimName = "working_pst";
		def.getPreCaptureAnimSuffix = delegate(FixedCapturePoint.Instance smi)
		{
			BuildingPointStraw component = smi.GetComponent<BuildingPointStraw>();
			return (!(component != null)) ? "_1" : component.GetAnimSuffix();
		};
		def.getTargetCapturePoint = delegate(FixedCapturePoint.Instance smi)
		{
			int num = Grid.PosToCell(smi);
			BuildingPointStraw component = smi.GetComponent<BuildingPointStraw>();
			int y = ((component != null) ? component.GetDepthOffset() : (-1));
			int num2 = Grid.OffsetCell(num, 0, y);
			return (Grid.IsValidCell(num2) && smi.targetCapturable.Navigator.CanReach(num2)) ? num2 : num;
		};
	}
}
