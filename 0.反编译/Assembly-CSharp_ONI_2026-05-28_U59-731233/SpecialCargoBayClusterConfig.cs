using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class SpecialCargoBayClusterConfig : IBuildingConfig
{
	public const string ID = "SpecialCargoBayCluster";

	private static readonly List<Storage.StoredItemModifier> StoredCrittersModifiers = new List<Storage.StoredItemModifier>
	{
		Storage.StoredItemModifier.Insulate,
		Storage.StoredItemModifier.Hide
	};

	private static readonly List<Storage.StoredItemModifier> StoredLootModifiers = new List<Storage.StoredItemModifier>
	{
		Storage.StoredItemModifier.Hide,
		Storage.StoredItemModifier.Seal
	};

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("SpecialCargoBayCluster", 3, 1, "rocket_storage_live_small_kanim", 1000, 60f, TUNING.BUILDINGS.ROCKETRY_MASS_KG.HOLLOW_TIER1, MATERIALS.REFINED_METALS, 9999f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NOISY.TIER2, decor: TUNING.BUILDINGS.DECOR.NONE);
		BuildingTemplates.CreateRocketBuildingDef(buildingDef);
		buildingDef.SceneLayer = Grid.SceneLayer.Building;
		buildingDef.OverheatTemperature = 2273.15f;
		buildingDef.Floodable = false;
		buildingDef.AttachmentSlotTag = GameTags.Rocket;
		buildingDef.ObjectLayer = ObjectLayer.Building;
		buildingDef.RequiresPowerInput = false;
		buildingDef.attachablePosition = new CellOffset(0, 0);
		buildingDef.CanMove = true;
		buildingDef.Cancellable = false;
		buildingDef.ShowInBuildMenu = false;
		buildingDef.AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		BuildingAttachPoint buildingAttachPoint = go.AddOrGet<BuildingAttachPoint>();
		buildingAttachPoint.points = new BuildingAttachPoint.HardPoint[1]
		{
			new BuildingAttachPoint.HardPoint(new CellOffset(0, 1), GameTags.Rocket, null)
		};
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		Storage storage = go.AddOrGet<Storage>();
		storage.SetDefaultStoredItemModifiers(StoredCrittersModifiers);
		storage.showCapacityStatusItem = false;
		storage.showInUI = false;
		storage.storageFilters = new List<Tag> { GameTags.BagableCreature };
		storage.allowSettingOnlyFetchMarkedItems = false;
		storage.allowItemRemoval = false;
		Storage storage2 = go.AddComponent<Storage>();
		storage2.SetDefaultStoredItemModifiers(StoredLootModifiers);
		storage2.showCapacityStatusItem = false;
		storage2.showInUI = false;
		storage2.allowSettingOnlyFetchMarkedItems = false;
		storage2.allowItemRemoval = false;
		go.AddOrGet<CopyBuildingSettings>();
		SpecialCargoBayCluster.Def def = go.AddOrGetDef<SpecialCargoBayCluster.Def>();
		SpecialCargoBayClusterReceptacle specialCargoBayClusterReceptacle = go.AddOrGet<SpecialCargoBayClusterReceptacle>();
		specialCargoBayClusterReceptacle.AddDepositTag(GameTags.BagableCreature);
		specialCargoBayClusterReceptacle.AddAdditionalCriteria((GameObject obj) => obj.HasTag(GameTags.Creatures.Deliverable));
		specialCargoBayClusterReceptacle.sideProductStorage = storage2;
		BuildingTemplates.ExtendBuildingToRocketModuleCluster(go, null, ROCKETRY.BURDEN.INSIGNIFICANT);
		SymbolOverrideControllerUtil.AddToPrefab(go);
		Prioritizable prioritizable = go.AddOrGet<Prioritizable>();
		Prioritizable.AddRef(go);
	}
}
