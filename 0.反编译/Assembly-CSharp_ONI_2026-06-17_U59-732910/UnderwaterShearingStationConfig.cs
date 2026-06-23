using TUNING;
using UnityEngine;

public class UnderwaterShearingStationConfig : IBuildingConfig
{
	public const string ID = "UnderwaterShearingStation";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("UnderwaterShearingStation", 3, 3, "shearing_station_aquatic_kanim", 30, 60f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.OnBackWall, noise: NOISE_POLLUTION.NOISY.TIER1, decor: BUILDINGS.DECOR.PENALTY.TIER2);
		obj.RequiresPowerInput = true;
		obj.EnergyConsumptionWhenActive = 60f;
		obj.ExhaustKilowattsWhenActive = 0.125f;
		obj.SelfHeatKilowattsWhenActive = 0.5f;
		obj.ViewMode = OverlayModes.Power.ID;
		obj.Floodable = false;
		obj.AudioCategory = "Metal";
		obj.AudioSize = "large";
		obj.RequiredSkillPerkID = Db.Get().SkillPerks.CanUseRanchStation.Id;
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.RanchStationType);
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		Prioritizable.AddRef(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		RoomTracker roomTracker = go.AddOrGet<RoomTracker>();
		roomTracker.requiredRoomType = Db.Get().RoomTypes.CreaturePen.Id;
		roomTracker.requirement = RoomTracker.Requirement.Required;
		go.AddOrGet<BuildingSubmergable>();
		go.AddComponent<UnderwaterShearingStaion>();
		go.AddOrGet<MultiSkillPerkMissingComplainer>().requiredSkillPerks = new string[2]
		{
			Db.Get().SkillPerks.CanUseRanchStation.Id,
			Db.Get().SkillPerks.CanSwim.Id
		};
		RanchStation.Def def = go.AddOrGetDef<RanchStation.Def>();
		def.IsCritterEligibleToBeRanchedCb = (GameObject creature_go, RanchStation.Instance ranch_station_smi) => creature_go.GetSMI<FlopMonitor.Instance>() != null && (creature_go.GetSMI<IShearable>()?.IsFullyGrown() ?? false);
		def.RancherInteractAnim = "anim_interacts_shearingstation_aquatic_kanim";
		def.RancherCallingAndWipeBrowAnim = "anim_interacts_rancherstation_aquatic_kanim";
		def.RanchedPreAnim = "shearing_pre";
		def.RanchedLoopAnim = "shearing_loop";
		def.RanchedPstAnim = "shearing_pst";
		def.CreatureRanchingStatusItem = Db.Get().CreatureStatusItems.GettingRanched;
		def.RancherWipesBrowAnim = false;
		def.GetTargetRanchCell = delegate(RanchStation.Instance smi)
		{
			int result = Grid.InvalidCell;
			if (!smi.IsNullOrStopped())
			{
				result = Grid.CellRight(Grid.PosToCell(smi.transform.GetPosition()));
			}
			return result;
		};
		def.OnRanchCompleteCb = delegate(GameObject creature_go, WorkerBase rancher_wb)
		{
			float num = rancher_wb.GetAttributes()?.Get(Db.Get().Attributes.Ranching.Id).GetTotalValue() ?? 0f;
			RanchableMonitor.Instance sMI = creature_go.GetSMI<RanchableMonitor.Instance>();
			IShearable sMI2 = creature_go.GetSMI<IShearable>();
			if (sMI2 != null)
			{
				Tuple<Tag, float> itemDroppedOnShear = sMI2.GetItemDroppedOnShear();
				StoreShearable(sMI.TargetRanchStation.gameObject, creature_go, itemDroppedOnShear.first, itemDroppedOnShear.second);
				sMI2.Shear();
			}
			UnderwaterShearingStaion component = sMI.TargetRanchStation.GetComponent<UnderwaterShearingStaion>();
			if (component != null)
			{
				component.HideShearableSymbol();
			}
		};
		def.OnRanchWorkBegins = delegate(RanchedStates.Instance creature, Workable workable)
		{
			UnderwaterShearingStaion component = workable.GetComponent<UnderwaterShearingStaion>();
			if (component != null)
			{
				Tuple<Tag, float> itemDroppedOnShear = creature.gameObject.GetSMI<IShearable>().GetItemDroppedOnShear();
				component.UpdateShearableSymbol(itemDroppedOnShear.first);
			}
		};
	}

	private void StoreShearable(GameObject station, GameObject critter, Tag item_dropped, float mass)
	{
		PrimaryElement component = critter.GetComponent<PrimaryElement>();
		GameObject gameObject = Util.KInstantiate(Assets.GetPrefab(item_dropped));
		int cell = Grid.CellRight(Grid.PosToCell(critter));
		gameObject.transform.SetPosition(Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore));
		PrimaryElement component2 = gameObject.GetComponent<PrimaryElement>();
		component2.Temperature = component.Temperature;
		component2.Mass = mass;
		component2.AddDisease(component.DiseaseIdx, component.DiseaseCount, "Shearing");
		gameObject.SetActive(value: true);
		Vector2 initial_velocity = new Vector2(Random.Range(-1f, 1f) * 1f, Random.value * 2f + 2f);
		if (GameComps.Fallers.Has(gameObject))
		{
			GameComps.Fallers.Remove(gameObject);
		}
		GameComps.Fallers.Add(gameObject, initial_velocity);
	}
}
