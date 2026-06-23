using TUNING;
using UnityEngine;

public class RocketInteriorLiquidOutputPortConfig : IBuildingConfig
{
	public const string ID = "RocketInteriorLiquidOutputPort";

	private ConduitPortInfo liquidOutputPort = new ConduitPortInfo(ConduitType.Liquid, new CellOffset(0, 0));

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("RocketInteriorLiquidOutputPort", 1, 1, "rocket_interior_port_liquid_out_kanim", 100, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.REFINED_METALS, 9999f, BuildLocationRule.Tile, noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.BONUS.TIER0);
		buildingDef.DefaultAnimState = "liquid_out";
		buildingDef.Floodable = false;
		buildingDef.Entombable = false;
		buildingDef.Overheatable = false;
		buildingDef.UseStructureTemperature = false;
		buildingDef.Replaceable = false;
		buildingDef.Invincible = true;
		buildingDef.BaseTimeUntilRepair = -1f;
		buildingDef.AudioCategory = "Metal";
		buildingDef.SceneLayer = Grid.SceneLayer.TileMain;
		buildingDef.ShowInBuildMenu = false;
		return buildingDef;
	}

	private void AttachPort(GameObject go)
	{
		ConduitSecondaryOutput conduitSecondaryOutput = go.AddComponent<ConduitSecondaryOutput>();
		conduitSecondaryOutput.portInfo = liquidOutputPort;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		SimCellOccupier simCellOccupier = go.AddOrGet<SimCellOccupier>();
		simCellOccupier.notifyOnMelt = true;
		BuildingHP buildingHP = go.AddOrGet<BuildingHP>();
		buildingHP.destroyOnDamaged = true;
		RocketConduitReceiver rocketConduitReceiver = go.AddComponent<RocketConduitReceiver>();
		rocketConduitReceiver.conduitPortInfo = liquidOutputPort;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		GeneratedBuildings.RemoveLoopingSounds(go);
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(GameTags.Bunker);
		component.AddTag(GameTags.FloorTiles);
		component.AddTag(GameTags.NoRocketRefund);
		MakeBaseSolid.Def def = go.AddOrGetDef<MakeBaseSolid.Def>();
		def.solidOffsets = new CellOffset[1]
		{
			new CellOffset(0, 0)
		};
		go.AddOrGet<BuildingCellVisualizer>();
		go.GetComponent<Deconstructable>().allowDeconstruction = false;
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		base.DoPostConfigurePreview(def, go);
		go.AddOrGet<BuildingCellVisualizer>();
		AttachPort(go);
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		base.DoPostConfigureUnderConstruction(go);
		go.AddOrGet<BuildingCellVisualizer>();
		AttachPort(go);
	}
}
