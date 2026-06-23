using TUNING;
using UnityEngine;

public class HEPBridgeTileConfig : IBuildingConfig
{
	public const string ID = "HEPBridgeTile";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("HEPBridgeTile", 2, 1, "radbolt_joint_plate_kanim", 100, 3f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.PLASTICS, 1600f, BuildLocationRule.Tile, noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.PENALTY.TIER5);
		buildingDef.Overheatable = false;
		buildingDef.UseStructureTemperature = false;
		buildingDef.Floodable = false;
		buildingDef.Entombable = false;
		buildingDef.AudioCategory = "Plastic";
		buildingDef.AudioSize = "small";
		buildingDef.BaseTimeUntilRepair = -1f;
		buildingDef.InitialOrientation = Orientation.R180;
		buildingDef.ForegroundLayer = Grid.SceneLayer.TileMain;
		buildingDef.PermittedRotations = PermittedRotations.R360;
		buildingDef.ViewMode = OverlayModes.Radiation.ID;
		buildingDef.UseHighEnergyParticleInputPort = true;
		buildingDef.HighEnergyParticleInputOffset = new CellOffset(1, 0);
		buildingDef.UseHighEnergyParticleOutputPort = true;
		buildingDef.HighEnergyParticleOutputOffset = new CellOffset(0, 0);
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.RadiationIDs, "HEPBridgeTile");
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		BuildingHP buildingHP = go.AddOrGet<BuildingHP>();
		buildingHP.destroyOnDamaged = true;
		go.AddOrGet<TileTemperature>();
		HighEnergyParticleStorage highEnergyParticleStorage = go.AddOrGet<HighEnergyParticleStorage>();
		highEnergyParticleStorage.autoStore = true;
		highEnergyParticleStorage.showInUI = false;
		highEnergyParticleStorage.capacity = 501f;
		HighEnergyParticleRedirector highEnergyParticleRedirector = go.AddOrGet<HighEnergyParticleRedirector>();
		highEnergyParticleRedirector.directorDelay = 0.5f;
		highEnergyParticleRedirector.directionControllable = false;
		highEnergyParticleRedirector.Direction = EightDirection.Right;
		go.GetComponent<KPrefabID>().prefabInitFn += OnPrefabInit;
	}

	private void OnPrefabInit(GameObject instance)
	{
		KBatchedAnimController[] componentsInChildrenOnly = instance.GetComponentsInChildrenOnly<KBatchedAnimController>();
		foreach (KBatchedAnimController kBatchedAnimController in componentsInChildrenOnly)
		{
			kBatchedAnimController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.LiquidVisibilityLayer, isActive: false);
			kBatchedAnimController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.WaterProof, isActive: true);
		}
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		base.DoPostConfigurePreview(def, go);
		go.AddOrGet<HEPBridgeTileVisualizer>();
		go.AddOrGet<BuildingCellVisualizer>();
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		base.DoPostConfigureUnderConstruction(go);
		go.AddOrGet<BuildingCellVisualizer>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<KPrefabID>().AddTag(GameTags.HEPPassThrough);
		go.AddOrGet<BuildingCellVisualizer>();
		MakeBaseSolid.Def def = go.AddOrGetDef<MakeBaseSolid.Def>();
		def.solidOffsets = new CellOffset[1]
		{
			new CellOffset(0, 0)
		};
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.prefabSpawnFn += delegate(GameObject inst)
		{
			Rotatable component2 = inst.GetComponent<Rotatable>();
			HighEnergyParticleRedirector component3 = inst.GetComponent<HighEnergyParticleRedirector>();
			switch (component2.Orientation)
			{
			case Orientation.Neutral:
				component3.Direction = EightDirection.Left;
				break;
			case Orientation.R90:
				component3.Direction = EightDirection.Up;
				break;
			case Orientation.R180:
				component3.Direction = EightDirection.Right;
				break;
			case Orientation.R270:
				component3.Direction = EightDirection.Down;
				break;
			}
		};
	}
}
