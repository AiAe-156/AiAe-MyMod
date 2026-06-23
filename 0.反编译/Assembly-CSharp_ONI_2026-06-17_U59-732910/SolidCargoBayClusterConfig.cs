using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class SolidCargoBayClusterConfig : IBuildingConfig
{
	public const string ID = "CargoBayCluster";

	public float CAPACITY = ROCKETRY.SOLID_CARGO_BAY_CLUSTER_CAPACITY * ROCKETRY.CARGO_CAPACITY_SCALE;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("CargoBayCluster", 5, 5, "rocket_cluster_storage_solid_kanim", 1000, 60f, TUNING.BUILDINGS.ROCKETRY_MASS_KG.DENSE_TIER2, new string[1] { SimHashes.Steel.ToString() }, 9999f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NOISY.TIER2, decor: TUNING.BUILDINGS.DECOR.NONE);
		BuildingTemplates.CreateRocketBuildingDef(obj);
		obj.SceneLayer = Grid.SceneLayer.Building;
		obj.Invincible = true;
		obj.OverheatTemperature = 2273.15f;
		obj.Floodable = false;
		obj.AttachmentSlotTag = GameTags.Rocket;
		obj.ObjectLayer = ObjectLayer.Building;
		obj.RequiresPowerInput = false;
		obj.attachablePosition = new CellOffset(0, 0);
		obj.CanMove = true;
		obj.Cancellable = false;
		obj.ShowInBuildMenu = false;
		obj.AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.AddOrGet<BuildingAttachPoint>().points = new BuildingAttachPoint.HardPoint[1]
		{
			new BuildingAttachPoint.HardPoint(new CellOffset(0, 5), GameTags.Rocket, null)
		};
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go = BuildingTemplates.ExtendBuildingToClusterCargoBay(go, CAPACITY, STORAGEFILTERS.STORAGE_SOLID_CARGO_BAY, new List<Tag>(2)
		{
			GameTags.Liquid,
			GameTags.Gas
		}, CargoBay.CargoType.Solids);
		BuildingTemplates.ExtendBuildingToRocketModuleCluster(go, null, ROCKETRY.BURDEN.MAJOR);
	}
}
