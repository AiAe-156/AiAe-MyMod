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
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("ReefGenerator", 3, 3, "turbine_reef_kanim", 100, 60f, new float[2]
		{
			TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER5[0],
			1f
		}, new string[2] { "Metal", "BuildingGasket" }, 2400f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER5, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		obj.SceneLayer = Grid.SceneLayer.BuildingFront;
		obj.ForegroundLayer = Grid.SceneLayer.BuildingBack;
		obj.GeneratorWattageRating = 300f;
		obj.GeneratorBaseCapacity = obj.GeneratorWattageRating;
		obj.ExhaustKilowattsWhenActive = 0.125f;
		obj.SelfHeatKilowattsWhenActive = 0.5f;
		obj.RequiresPowerOutput = true;
		obj.PowerOutputOffset = new CellOffset(0, 0);
		obj.ViewMode = OverlayModes.Power.ID;
		obj.AudioCategory = "HollowMetal";
		obj.AudioSize = "large";
		obj.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(-1, 2));
		obj.AddSearchTerms(SEARCH_TERMS.POWER);
		obj.AddSearchTerms(SEARCH_TERMS.GENERATOR);
		obj.AttachmentSlotTag = GameTags.ReefGenerator;
		obj.BuildLocationRule = BuildLocationRule.BuildingAttachPoint;
		obj.ObjectLayer = ObjectLayer.AttachableBuilding;
		obj.PowerOutputOffset = new CellOffset(1, 2);
		obj.Floodable = false;
		obj.POIUnlockable = true;
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		component.AddTag(RoomConstraints.ConstraintTags.PowerBuilding);
		component.AddTag(RoomConstraints.ConstraintTags.GeneratorType);
		component.AddTag(RoomConstraints.ConstraintTags.HeavyDutyGeneratorType);
		go.AddOrGet<ReefGeneratorPower>().powerDistributionOrder = 9;
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
