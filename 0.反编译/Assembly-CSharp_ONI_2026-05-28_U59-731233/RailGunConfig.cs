using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class RailGunConfig : IBuildingConfig
{
	public const string ID = "RailGun";

	public const string PORT_ID = "HEP_STORAGE";

	public const int RANGE = 20;

	public const float BASE_PARTICLE_COST = 0f;

	public const float HEX_PARTICLE_COST = 10f;

	public const float HEP_CAPACITY = 200f;

	public const float TAKEOFF_VELOCITY = 35f;

	public const int MAINTENANCE_AFTER_NUM_PAYLOADS = 6;

	public const int MAINTENANCE_COOLDOWN = 30;

	public const float CAPACITY = 1200f;

	private ConduitPortInfo solidInputPort = new ConduitPortInfo(ConduitType.Solid, new CellOffset(-1, 0));

	private ConduitPortInfo liquidInputPort = new ConduitPortInfo(ConduitType.Liquid, new CellOffset(0, 0));

	private ConduitPortInfo gasInputPort = new ConduitPortInfo(ConduitType.Gas, new CellOffset(1, 0));

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("RailGun", 5, 6, "rail_gun_kanim", 250, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER5, decor: TUNING.BUILDINGS.DECOR.NONE);
		buildingDef.Floodable = false;
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.BaseTimeUntilRepair = 400f;
		buildingDef.DefaultAnimState = "off";
		buildingDef.RequiresPowerInput = true;
		buildingDef.PowerInputOffset = new CellOffset(-2, 0);
		buildingDef.EnergyConsumptionWhenActive = 240f;
		buildingDef.ViewMode = OverlayModes.Power.ID;
		buildingDef.ExhaustKilowattsWhenActive = 0.5f;
		buildingDef.SelfHeatKilowattsWhenActive = 2f;
		buildingDef.UseHighEnergyParticleInputPort = true;
		buildingDef.HighEnergyParticleInputOffset = new CellOffset(-2, 1);
		buildingDef.LogicInputPorts = new List<LogicPorts.Port> { LogicPorts.Port.InputPort(RailGun.PORT_ID, new CellOffset(-2, 2), STRINGS.BUILDINGS.PREFABS.RAILGUN.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.RAILGUN.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.RAILGUN.LOGIC_PORT_INACTIVE) };
		buildingDef.LogicOutputPorts = new List<LogicPorts.Port> { LogicPorts.Port.OutputPort("HEP_STORAGE", new CellOffset(2, 0), STRINGS.BUILDINGS.PREFABS.HEPENGINE.LOGIC_PORT_STORAGE, STRINGS.BUILDINGS.PREFABS.HEPENGINE.LOGIC_PORT_STORAGE_ACTIVE, STRINGS.BUILDINGS.PREFABS.HEPENGINE.LOGIC_PORT_STORAGE_INACTIVE) };
		buildingDef.AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		return buildingDef;
	}

	private void AttachPorts(GameObject go)
	{
		ConduitSecondaryInput conduitSecondaryInput = go.AddComponent<ConduitSecondaryInput>();
		conduitSecondaryInput.portInfo = liquidInputPort;
		ConduitSecondaryInput conduitSecondaryInput2 = go.AddComponent<ConduitSecondaryInput>();
		conduitSecondaryInput2.portInfo = gasInputPort;
		ConduitSecondaryInput conduitSecondaryInput3 = go.AddComponent<ConduitSecondaryInput>();
		conduitSecondaryInput3.portInfo = solidInputPort;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		RailGun railGun = go.AddOrGet<RailGun>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.AddOrGet<LoopingSounds>();
		ClusterDestinationSelector clusterDestinationSelector = go.AddOrGet<ClusterDestinationSelector>();
		clusterDestinationSelector.changeTargetButtonTooltipString = UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.CHANGE_DESTINATION_BUTTON_TOOLTIP_RAILGUN;
		clusterDestinationSelector.clearTargetButtonTooltipString = UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.CLEAR_DESTINATION_BUTTON_TOOLTIP_RAILGUN;
		clusterDestinationSelector.assignable = true;
		clusterDestinationSelector.requireAsteroidDestination = true;
		railGun.liquidPortInfo = liquidInputPort;
		railGun.gasPortInfo = gasInputPort;
		railGun.solidPortInfo = solidInputPort;
		HighEnergyParticleStorage highEnergyParticleStorage = go.AddOrGet<HighEnergyParticleStorage>();
		highEnergyParticleStorage.capacity = 200f;
		highEnergyParticleStorage.autoStore = true;
		highEnergyParticleStorage.showInUI = false;
		highEnergyParticleStorage.PORT_ID = "HEP_STORAGE";
		highEnergyParticleStorage.showCapacityStatusItem = true;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		List<Tag> list = new List<Tag>();
		list.AddRange(STORAGEFILTERS.STORAGE_LOCKERS_STANDARD);
		list.AddRange(STORAGEFILTERS.GASES);
		list.AddRange(STORAGEFILTERS.FOOD);
		Storage storage = BuildingTemplates.CreateDefaultStorage(go);
		storage.showInUI = true;
		storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		storage.storageFilters = list;
		storage.allowSettingOnlyFetchMarkedItems = false;
		storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
		storage.capacityKg = 1200f;
		HighEnergyParticlePort component = go.GetComponent<HighEnergyParticlePort>();
		component.requireOperational = false;
		AddVisualizer(go);
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		AttachPorts(go);
		AddVisualizer(go);
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		AttachPorts(go);
		AddVisualizer(go);
	}

	private static void AddVisualizer(GameObject prefab)
	{
		SkyVisibilityVisualizer skyVisibilityVisualizer = prefab.AddOrGet<SkyVisibilityVisualizer>();
		skyVisibilityVisualizer.RangeMin = -2;
		skyVisibilityVisualizer.RangeMax = 1;
		skyVisibilityVisualizer.AllOrNothingVisibility = true;
		KPrefabID component = prefab.GetComponent<KPrefabID>();
		component.instantiateFn += delegate(GameObject go)
		{
			go.GetComponent<SkyVisibilityVisualizer>().SkyVisibilityCb = RailGunSkyVisibility;
		};
	}

	private static bool RailGunSkyVisibility(int cell)
	{
		DebugUtil.DevAssert(ClusterManager.Instance != null, "RailGun assumes DLC");
		if (Grid.IsValidCell(cell) && Grid.WorldIdx[cell] != byte.MaxValue)
		{
			int num = (int)ClusterManager.Instance.GetWorld(Grid.WorldIdx[cell]).maximumBounds.y;
			int num2 = cell;
			while (Grid.CellRow(num2) <= num)
			{
				if (!Grid.IsValidCell(num2) || Grid.Solid[num2])
				{
					return false;
				}
				num2 = Grid.CellAbove(num2);
			}
			return true;
		}
		return false;
	}
}
