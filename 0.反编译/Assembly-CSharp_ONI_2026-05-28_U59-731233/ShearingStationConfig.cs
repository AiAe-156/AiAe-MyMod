using TUNING;
using UnityEngine;

public class ShearingStationConfig : IBuildingConfig
{
	public const string ID = "ShearingStation";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("ShearingStation", 3, 3, "shearing_station_kanim", 100, 10f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.RAW_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.NONE);
		buildingDef.RequiresPowerInput = true;
		buildingDef.EnergyConsumptionWhenActive = 60f;
		buildingDef.ExhaustKilowattsWhenActive = 0.125f;
		buildingDef.SelfHeatKilowattsWhenActive = 0.5f;
		buildingDef.Floodable = true;
		buildingDef.Entombable = true;
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "large";
		buildingDef.UtilityInputOffset = new CellOffset(0, 0);
		buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
		buildingDef.DefaultAnimState = "on";
		buildingDef.ShowInBuildMenu = true;
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<LoopingSounds>();
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.RanchStationType);
		RoomTracker roomTracker = go.AddOrGet<RoomTracker>();
		roomTracker.requiredRoomType = Db.Get().RoomTypes.CreaturePen.Id;
		roomTracker.requirement = RoomTracker.Requirement.Required;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		RanchStation.Def def = go.AddOrGetDef<RanchStation.Def>();
		def.IsCritterEligibleToBeRanchedCb = (GameObject creature_go, RanchStation.Instance ranch_station_smi) => creature_go.GetSMI<IShearable>()?.IsFullyGrown() ?? false;
		def.OnRanchCompleteCb = delegate(GameObject creature_go, WorkerBase rancher_wb)
		{
			IShearable sMI = creature_go.GetSMI<IShearable>();
			Tuple<Tag, float> itemDroppedOnShear = sMI.GetItemDroppedOnShear();
			DropShearable(rancher_wb.gameObject, creature_go, itemDroppedOnShear.first, itemDroppedOnShear.second);
			sMI.Shear();
		};
		def.RancherInteractAnim = "anim_interacts_shearingstation_kanim";
		def.WorkTime = 12f;
		def.RanchedPreAnim = "shearing_pre";
		def.RanchedLoopAnim = "shearing_loop";
		def.RanchedPstAnim = "shearing_pst";
		def.RancherWipesBrowAnim = false;
		SkillPerkMissingComplainer skillPerkMissingComplainer = go.AddOrGet<SkillPerkMissingComplainer>();
		skillPerkMissingComplainer.requiredSkillPerk = Db.Get().SkillPerks.CanUseRanchStation.Id;
		Prioritizable.AddRef(go);
	}

	private void DropShearable(GameObject go, GameObject critter, Tag item_dropped, float mass)
	{
		PrimaryElement component = critter.GetComponent<PrimaryElement>();
		GameObject gameObject = Util.KInstantiate(Assets.GetPrefab(item_dropped));
		int cell = Grid.CellLeft(Grid.PosToCell(go));
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
