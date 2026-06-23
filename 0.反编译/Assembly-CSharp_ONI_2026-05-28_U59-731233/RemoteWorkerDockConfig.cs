using STRINGS;
using TUNING;
using UnityEngine;

public class RemoteWorkerDockConfig : IBuildingConfig
{
	public static string ID = "RemoteWorkerDock";

	public const float NEW_WORKER_DELAY_SECONDS = 2f;

	public const int WORK_RANGE = 12;

	public const float LUBRICANT_CAPACITY_KG = 50f;

	public const string ON_EMPTY_ANIM = "on_empty";

	public const string ON_FULL_ANIM = "on_full";

	public const string OFF_EMPTY_ANIM = "off_empty";

	public const string OFF_FULL_ANIM = "off_full";

	public const string NEW_WORKER_ANIM = "new_worker";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "remote_work_dock_kanim", 100, 60f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.PLASTICS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "Plastic";
		buildingDef.InputConduitType = ConduitType.Liquid;
		buildingDef.OutputConduitType = ConduitType.Liquid;
		buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
		buildingDef.UtilityInputOffset = new CellOffset(0, 1);
		buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
		buildingDef.PowerInputOffset = new CellOffset(0, 0);
		buildingDef.RequiresPowerInput = true;
		buildingDef.EnergyConsumptionWhenActive = 120f;
		buildingDef.SelfHeatKilowattsWhenActive = 2f;
		buildingDef.ExhaustKilowattsWhenActive = 0f;
		buildingDef.AddSearchTerms(SEARCH_TERMS.ROBOT);
		return buildingDef;
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		base.DoPostConfigurePreview(def, go);
		AddVisualizer(go);
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		AddVisualizer(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<RemoteWorkerDock>();
		go.AddOrGet<RemoteWorkerDockAnimSM>();
		go.AddOrGet<Operational>();
		go.AddOrGet<UserNameable>();
		Storage storage = go.AddComponent<Storage>();
		storage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
		ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
		conduitConsumer.conduitType = ConduitType.Liquid;
		conduitConsumer.capacityTag = GameTags.LubricatingOil;
		conduitConsumer.capacityKG = 50f;
		conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;
		ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
		conduitDispenser.conduitType = ConduitType.Liquid;
		conduitDispenser.elementFilter = new SimHashes[1] { SimHashes.LiquidGunk };
		AddVisualizer(go);
		RangeVisualizer rangeVisualizer = go.AddOrGet<RangeVisualizer>();
	}

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC3;
	}

	private void AddVisualizer(GameObject prefab)
	{
		RangeVisualizer rangeVisualizer = prefab.AddOrGet<RangeVisualizer>();
		rangeVisualizer.RangeMin.x = -12;
		rangeVisualizer.RangeMin.y = 0;
		rangeVisualizer.RangeMax.x = 12;
		rangeVisualizer.RangeMax.y = 0;
		rangeVisualizer.OriginOffset = default(Vector2I);
		rangeVisualizer.BlockingTileVisible = false;
		prefab.GetComponent<KPrefabID>().instantiateFn += delegate(GameObject go)
		{
			go.GetComponent<RangeVisualizer>().BlockingCb = DockPathBlockingCB;
		};
	}

	public static bool DockPathBlockingCB(int cell)
	{
		int num = Grid.CellAbove(cell);
		int num2 = Grid.CellBelow(cell);
		if (num == Grid.InvalidCell || num2 == Grid.InvalidCell)
		{
			return true;
		}
		if (!Grid.Foundation[num2] && !Grid.Solid[num2])
		{
			return true;
		}
		if (Grid.Solid[cell] || Grid.Solid[num])
		{
			return true;
		}
		return false;
	}
}
