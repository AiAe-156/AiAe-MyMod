using TUNING;
using UnityEngine;

public class StaterpillarGasConnectorConfig : IBuildingConfig
{
	public static readonly string ID = "StaterpillarGasConnector";

	private const int WIDTH = 1;

	private const int HEIGHT = 2;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		string iD = ID;
		string[] aLL_METALS = MATERIALS.ALL_METALS;
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(construction_mass: BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, construction_materials: aLL_METALS, melting_point: 9999f, build_location_rule: BuildLocationRule.OnFoundationRotatable, noise: NOISE_POLLUTION.NOISY.TIER0, id: iD, width: 1, height: 2, anim: "egg_caterpillar_kanim", hitpoints: 1000, construction_time: 10f, decor: BUILDINGS.DECOR.NONE);
		buildingDef.Overheatable = false;
		buildingDef.Floodable = false;
		buildingDef.OverheatTemperature = 423.15f;
		buildingDef.PermittedRotations = PermittedRotations.FlipV;
		buildingDef.ViewMode = OverlayModes.GasConduits.ID;
		buildingDef.AudioCategory = "Plastic";
		buildingDef.OutputConduitType = ConduitType.Gas;
		buildingDef.UtilityOutputOffset = new CellOffset(0, 1);
		buildingDef.PlayConstructionSounds = false;
		buildingDef.ShowInBuildMenu = false;
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<Storage>();
		ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
		conduitDispenser.conduitType = ConduitType.Gas;
		conduitDispenser.alwaysDispense = true;
		conduitDispenser.elementFilter = null;
		conduitDispenser.isOn = false;
		Deconstructable component = go.GetComponent<Deconstructable>();
		component.SetAllowDeconstruction(allow: false);
		go.GetComponent<KSelectable>().IsSelectable = false;
	}
}
