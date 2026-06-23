using Database;
using STRINGS;
using TUNING;
using UnityEngine;

public class MonumentBottomConfig : IBuildingConfig
{
	public const string ID = "MonumentBottom";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("MonumentBottom", 5, 5, "monument_base_a_kanim", 1000, 60f, new float[2] { 7500f, 2500f }, new string[2]
		{
			SimHashes.Steel.ToString(),
			SimHashes.Obsidian.ToString()
		}, 9999f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER2, decor: TUNING.BUILDINGS.DECOR.BONUS.MONUMENT.INCOMPLETE);
		BuildingTemplates.CreateMonumentBuildingDef(buildingDef);
		buildingDef.SceneLayer = Grid.SceneLayer.BuildingFront;
		buildingDef.OverheatTemperature = 2273.15f;
		buildingDef.Floodable = false;
		buildingDef.AttachmentSlotTag = "MonumentBottom";
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
			new BuildingAttachPoint.HardPoint(new CellOffset(0, 5), "MonumentMiddle", null)
		};
		MonumentPart monumentPart = go.AddOrGet<MonumentPart>();
		monumentPart.part = MonumentPartResource.Part.Bottom;
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
			monumentPart.part = MonumentPartResource.Part.Bottom;
			monumentPart.stateUISymbol = "base";
		};
	}
}
