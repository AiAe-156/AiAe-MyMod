using STRINGS;
using TUNING;
using UnityEngine;

public class ReefGeneratorConfig : IBuildingConfig
{
	public const string ID = "ReefGenerator";

	private const float WATTS_PRODUCED = 300f;

	private const int GASKET_UNITS = 1;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("ReefGenerator", 3, 3, "turbine_reef_kanim", 100, 60f, new float[2]
		{
			TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER5[0],
			1f
		}, new string[2] { "Metal", "BuildingGasket" }, 2400f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER5, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		buildingDef.SceneLayer = Grid.SceneLayer.BuildingFront;
		buildingDef.ForegroundLayer = Grid.SceneLayer.BuildingBack;
		buildingDef.GeneratorWattageRating = 300f;
		buildingDef.GeneratorBaseCapacity = buildingDef.GeneratorWattageRating;
		buildingDef.ExhaustKilowattsWhenActive = 0f;
		buildingDef.SelfHeatKilowattsWhenActive = 0f;
		buildingDef.RequiresPowerOutput = true;
		buildingDef.PowerOutputOffset = new CellOffset(0, 0);
		buildingDef.ViewMode = OverlayModes.Power.ID;
		buildingDef.AudioCategory = "HollowMetal";
		buildingDef.AudioSize = "large";
		buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(-1, 2));
		buildingDef.AddSearchTerms(SEARCH_TERMS.POWER);
		buildingDef.AddSearchTerms(SEARCH_TERMS.GENERATOR);
		buildingDef.AttachmentSlotTag = GameTags.ReefGenerator;
		buildingDef.BuildLocationRule = BuildLocationRule.BuildingAttachPoint;
		buildingDef.ObjectLayer = ObjectLayer.AttachableBuilding;
		buildingDef.PowerOutputOffset = new CellOffset(1, 2);
		buildingDef.Floodable = false;
		buildingDef.POIUnlockable = true;
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		component.AddTag(RoomConstraints.ConstraintTags.PowerBuilding);
		component.AddTag(RoomConstraints.ConstraintTags.GeneratorType);
		component.AddTag(RoomConstraints.ConstraintTags.HeavyDutyGeneratorType);
		ReefGeneratorPower reefGeneratorPower = go.AddOrGet<ReefGeneratorPower>();
		reefGeneratorPower.powerDistributionOrder = 9;
		go.AddOrGet<LoopingSounds>();
		Prioritizable.AddRef(go);
		go.AddOrGetDef<ReefGenerator.Def>();
		Tinkerable.MakePowerTinkerable(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LogicOperationalController>();
	}
}
