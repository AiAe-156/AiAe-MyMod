using TUNING;
using UnityEngine;

public class ModularLaunchpadPortBridgeConfig : IBuildingConfig
{
	public const string ID = "ModularLaunchpadPortBridge";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("ModularLaunchpadPortBridge", 1, 2, "rocket_loader_extension_kanim", 1000, 60f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.REFINED_METALS, 9999f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER0, decor: BUILDINGS.DECOR.NONE);
		buildingDef.SceneLayer = Grid.SceneLayer.BuildingBack;
		buildingDef.OverheatTemperature = 2273.15f;
		buildingDef.Floodable = false;
		buildingDef.Entombable = false;
		buildingDef.DefaultAnimState = "idle";
		buildingDef.UseStructureTemperature = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "medium";
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(GameTags.ModularConduitPort);
		component.AddTag(GameTags.NotRocketInteriorBuilding);
		component.AddTag(BaseModularLaunchpadPortConfig.LinkTag);
		ChainedBuilding.Def def = go.AddOrGetDef<ChainedBuilding.Def>();
		def.headBuildingTag = "LaunchPad".ToTag();
		def.linkBuildingTag = BaseModularLaunchpadPortConfig.LinkTag;
		def.objectLayer = ObjectLayer.Building;
		FakeFloorAdder fakeFloorAdder = go.AddOrGet<FakeFloorAdder>();
		fakeFloorAdder.floorOffsets = new CellOffset[1]
		{
			new CellOffset(0, 1)
		};
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
