using TUNING;
using UnityEngine;

public class UnderwaterMilkingStationConfig : IBuildingConfig
{
	public const string ID = "UnderwaterMilkingStation";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("UnderwaterMilkingStation", 3, 3, "milking_station_aquatic_kanim", 30, 60f, new float[2]
		{
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER4[0],
			4f
		}, new string[2] { "RefinedMetal", "BuildingGasket" }, 1600f, BuildLocationRule.OnBackWall, noise: NOISE_POLLUTION.NOISY.TIER1, decor: BUILDINGS.DECOR.PENALTY.TIER2);
		obj.ViewMode = OverlayModes.Rooms.ID;
		obj.OutputConduitType = ConduitType.Liquid;
		obj.UtilityOutputOffset = new CellOffset(1, 0);
		obj.ViewMode = OverlayModes.LiquidConduits.ID;
		obj.Overheatable = false;
		obj.Floodable = false;
		obj.AudioCategory = "Metal";
		obj.AudioSize = "large";
		obj.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		obj.OutputConduitType = ConduitType.Liquid;
		obj.UtilityOutputOffset = new CellOffset(1, 1);
		obj.RequiredSkillPerkID = Db.Get().SkillPerks.CanUseMilkingStation.Id;
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.RanchStationType);
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = Mathf.Max(SquidTuning.INK_AMOUNT_AT_MILKING, MooTuning.MILK_PER_CYCLE) * 2f;
		storage.showInUI = true;
		storage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		Prioritizable.AddRef(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LogicOperationalController>();
		go.AddOrGet<BuildingSubmergable>();
		RoomTracker roomTracker = go.AddOrGet<RoomTracker>();
		roomTracker.requiredRoomType = Db.Get().RoomTypes.CreaturePen.Id;
		roomTracker.requirement = RoomTracker.Requirement.Required;
		go.AddOrGet<MultiSkillPerkMissingComplainer>().requiredSkillPerks = new string[2]
		{
			Db.Get().SkillPerks.CanUseMilkingStation.Id,
			Db.Get().SkillPerks.CanSwim.Id
		};
		RanchStation.Def def = go.AddOrGetDef<RanchStation.Def>();
		def.IsCritterEligibleToBeRanchedCb = (GameObject creature_go, RanchStation.Instance ranch_station_smi) => creature_go.GetSMI<IMilkable>()?.IsReadyToBeMilked() ?? false;
		def.RancherCallingAndWipeBrowAnim = "anim_interacts_rancherstation_aquatic_kanim";
		def.RancherInteractAnim = "anim_interacts_milking_station_aquatic_kanim";
		def.RanchedPreAnim = "milking_pre";
		def.RanchedLoopAnim = "milking_loop";
		def.RanchedPstAnim = "milking_pst";
		def.WorkTime = 20f;
		def.CreatureRanchingStatusItem = Db.Get().CreatureStatusItems.GettingMilked;
		def.RancherWipesBrowAnim = false;
		def.GetTargetRanchCell = delegate(RanchStation.Instance smi)
		{
			int result = Grid.InvalidCell;
			if (!smi.IsNullOrStopped())
			{
				result = Grid.PosToCell(smi.transform.GetPosition());
			}
			return result;
		};
		def.OnRanchCompleteCb = delegate(GameObject creature_go, WorkerBase rancher_wb)
		{
			RanchStation.Instance targetRanchStation = creature_go.GetSMI<RanchableMonitor.Instance>().TargetRanchStation;
			creature_go.GetSMI<IMilkable>().MilkingComplete(targetRanchStation.GetComponent<Storage>());
		};
		def.OnRanchWorkBegins = delegate(RanchedStates.Instance creature, Workable workable)
		{
			IMilkable sMI = creature.gameObject.GetSMI<IMilkable>();
			if (sMI != null)
			{
				Color color = ElementLoader.FindElementByHash(sMI.GetMilkElement()).substance.colour;
				color.a = 1f;
				workable.GetComponent<KBatchedAnimController>().SetSymbolTint(new KAnimHashedString("gushfx"), color);
			}
		};
		ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
		conduitDispenser.conduitType = ConduitType.Liquid;
		conduitDispenser.alwaysDispense = true;
		conduitDispenser.elementFilter = null;
	}
}
