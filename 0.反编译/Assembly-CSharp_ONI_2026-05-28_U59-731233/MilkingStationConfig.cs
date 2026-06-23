using TUNING;
using UnityEngine;

public class MilkingStationConfig : IBuildingConfig
{
	public const string ID = "MilkingStation";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("MilkingStation", 2, 4, "milking_station_kanim", 30, 60f, new float[2]
		{
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER4[0],
			4f
		}, new string[2] { "RefinedMetal", "BuildingGasket" }, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER1, decor: BUILDINGS.DECOR.PENALTY.TIER2);
		buildingDef.ViewMode = OverlayModes.Rooms.ID;
		buildingDef.OutputConduitType = ConduitType.Liquid;
		buildingDef.UtilityOutputOffset = new CellOffset(1, 0);
		buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "large";
		buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		buildingDef.OutputConduitType = ConduitType.Liquid;
		buildingDef.UtilityOutputOffset = new CellOffset(1, 1);
		buildingDef.RequiredSkillPerkID = Db.Get().SkillPerks.CanUseMilkingStation.Id;
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.RanchStationType);
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = Mathf.Max(MooTuning.MILK_AMOUNT_AT_MILKING, MooTuning.DIESEL_PER_CYCLE) * 2f;
		storage.showInUI = true;
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		Prioritizable.AddRef(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		RoomTracker roomTracker = go.AddOrGet<RoomTracker>();
		roomTracker.requiredRoomType = Db.Get().RoomTypes.CreaturePen.Id;
		roomTracker.requirement = RoomTracker.Requirement.Required;
		SkillPerkMissingComplainer skillPerkMissingComplainer = go.AddOrGet<SkillPerkMissingComplainer>();
		skillPerkMissingComplainer.requiredSkillPerk = Db.Get().SkillPerks.CanUseMilkingStation.Id;
		RanchStation.Def def = go.AddOrGetDef<RanchStation.Def>();
		def.IsCritterEligibleToBeRanchedCb = (GameObject creature_go, RanchStation.Instance ranch_station_smi) => creature_go.GetSMI<IMilkable>()?.IsReadyToBeMilked() ?? false;
		def.RancherInteractAnim = "anim_interacts_milking_station_kanim";
		def.RanchedPreAnim = "mooshake_pre";
		def.RanchedLoopAnim = "mooshake_loop";
		def.RanchedPstAnim = "mooshake_pst";
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
			RanchableMonitor.Instance sMI = creature_go.GetSMI<RanchableMonitor.Instance>();
			RanchStation.Instance targetRanchStation = sMI.TargetRanchStation;
			IMilkable sMI2 = creature_go.GetSMI<IMilkable>();
			sMI2.MilkingComplete(targetRanchStation.GetComponent<Storage>());
		};
		def.OnRanchWorkBegins = delegate(RanchedStates.Instance creature, Workable workable)
		{
			IMilkable sMI = creature.gameObject.GetSMI<IMilkable>();
			if (sMI != null)
			{
				Element element = ElementLoader.FindElementByHash(sMI.GetMilkElement());
				Color color = element.substance.colour;
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
