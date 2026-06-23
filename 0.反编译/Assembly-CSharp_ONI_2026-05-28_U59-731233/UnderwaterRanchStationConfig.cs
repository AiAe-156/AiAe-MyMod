using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class UnderwaterRanchStationConfig : IBuildingConfig
{
	public const string ID = "UnderwaterRanchStation";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("UnderwaterRanchStation", 2, 3, "rancherstation_aquatic_kanim", 30, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 1600f, BuildLocationRule.OnBackWall, noise: NOISE_POLLUTION.NOISY.TIER1, decor: TUNING.BUILDINGS.DECOR.NONE);
		buildingDef.Floodable = false;
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "large";
		buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		buildingDef.RequiredSkillPerkID = Db.Get().SkillPerks.CanUseRanchStation.Id;
		buildingDef.AddSearchTerms(SEARCH_TERMS.RANCHING);
		buildingDef.AddSearchTerms(SEARCH_TERMS.CRITTER);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<LoopingSounds>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<BuildingSubmergable>();
		go.AddOrGet<LogicOperationalController>();
		RanchStation.Def def = go.AddOrGetDef<RanchStation.Def>();
		def.RequiresRoom = false;
		def.IsCritterEligibleToBeRanchedCb = (GameObject creature_go, RanchStation.Instance ranch_station_smi) => creature_go.HasTag(GameTags.Creatures.Swimmer) && !creature_go.GetComponent<Effects>().HasEffect("Ranched");
		def.RancherWipesBrowAnim = true;
		def.RancherCallingAndWipeBrowAnim = "anim_interacts_rancherstation_aquatic_kanim";
		def.RancherInteractAnim = "anim_interacts_rancherstation_aquatic_kanim";
		def.OnRanchCompleteCb = delegate(GameObject creature_go, WorkerBase rancher_wb)
		{
			RanchableMonitor.Instance sMI = creature_go.GetSMI<RanchableMonitor.Instance>();
			RanchStation.Instance targetRanchStation = sMI.TargetRanchStation;
			RancherChore.RancherChoreStates.Instance sMI2 = targetRanchStation.GetSMI<RancherChore.RancherChoreStates.Instance>();
			float num = rancher_wb.GetAttributes()?.Get(Db.Get().Attributes.Ranching.Id).GetTotalValue() ?? 0f;
			float num2 = 1f + num * 0.1f;
			creature_go.GetComponent<Effects>().Add("Ranched", should_save: true).timeRemaining *= num2;
			AmountInstance amountInstance = Db.Get().Amounts.HitPoints.Lookup(creature_go);
			amountInstance?.ApplyDelta(amountInstance.GetMax() - amountInstance.value + 1f);
		};
		def.RanchedPreAnim = "grooming_pre";
		def.RanchedLoopAnim = "grooming_loop";
		def.RanchedPstAnim = "grooming_pst";
		def.WorkTime = 12f;
		def.GetTargetRanchCell = delegate(RanchStation.Instance smi)
		{
			int result = Grid.InvalidCell;
			if (!smi.IsNullOrStopped())
			{
				result = Grid.CellRight(Grid.PosToCell(smi.transform.GetPosition()));
			}
			return result;
		};
		RoomTracker roomTracker = go.AddOrGet<RoomTracker>();
		roomTracker.requiredRoomType = Db.Get().RoomTypes.CreaturePen.Id;
		roomTracker.requirement = RoomTracker.Requirement.TrackingOnly;
		MultiSkillPerkMissingComplainer multiSkillPerkMissingComplainer = go.AddOrGet<MultiSkillPerkMissingComplainer>();
		multiSkillPerkMissingComplainer.requiredSkillPerks = new string[2]
		{
			Db.Get().SkillPerks.CanUseRanchStation.Id,
			Db.Get().SkillPerks.CanSwim.Id
		};
		Prioritizable.AddRef(go);
	}
}
