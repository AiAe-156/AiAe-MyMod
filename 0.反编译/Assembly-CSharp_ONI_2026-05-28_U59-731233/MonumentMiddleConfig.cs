using Database;
using STRINGS;
using TUNING;
using UnityEngine;

public class MonumentMiddleConfig : IBuildingConfig
{
	public const string ID = "MonumentMiddle";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("MonumentMiddle", 5, 5, "monument_mid_a_kanim", 1000, 60f, new float[3] { 2500f, 2500f, 5000f }, new string[3]
		{
			SimHashes.Ceramic.ToString(),
			SimHashes.Polypropylene.ToString(),
			SimHashes.Steel.ToString()
		}, 9999f, BuildLocationRule.BuildingAttachPoint, noise: NOISE_POLLUTION.NOISY.TIER2, decor: TUNING.BUILDINGS.DECOR.BONUS.MONUMENT.INCOMPLETE);
		BuildingTemplates.CreateMonumentBuildingDef(buildingDef);
		buildingDef.SceneLayer = Grid.SceneLayer.Building;
		buildingDef.OverheatTemperature = 2273.15f;
		buildingDef.Floodable = false;
		buildingDef.AttachmentSlotTag = "MonumentMiddle";
		buildingDef.ObjectLayer = ObjectLayer.Building;
		buildingDef.PermittedRotations = PermittedRotations.FlipH;
		buildingDef.attachablePosition = new CellOffset(0, 0);
		buildingDef.RequiresPowerInput = false;
		buildingDef.CanMove = false;
		buildingDef.AddSearchTerms(SEARCH_TERMS.STATUE);
		buildingDef.AddSearchTerms(SEARCH_TERMS.MORALE);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		go.AddOrGet<LoopingSounds>();
		BuildingAttachPoint buildingAttachPoint = go.AddOrGet<BuildingAttachPoint>();
		buildingAttachPoint.points = new BuildingAttachPoint.HardPoint[1]
		{
			new BuildingAttachPoint.HardPoint(new CellOffset(0, 5), "MonumentTop", null)
		};
		MonumentPart monumentPart = go.AddOrGet<MonumentPart>();
		monumentPart.part = MonumentPartResource.Part.Middle;
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<KBatchedAnimController>().initialAnim = "option_a";
		go.GetComponent<KPrefabID>().prefabSpawnFn += delegate(GameObject game_object)
		{
			MonumentPart monumentPart = game_object.AddOrGet<MonumentPart>();
			monumentPart.part = MonumentPartResource.Part.Middle;
			monumentPart.stateUISymbol = "mid";
		};
	}
}
