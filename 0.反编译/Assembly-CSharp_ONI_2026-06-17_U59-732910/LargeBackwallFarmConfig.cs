using System.Collections.Generic;
using TUNING;
using UnityEngine;

public class LargeBackwallFarmConfig : IBuildingConfig
{
	public const string ID = "LargeBackwallFarm";

	public override string[] GetRequiredDlcIds()
	{
		return new string[1] { "DLC5_ID" };
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("LargeBackwallFarm", 2, 2, "backwall_planter_box_kanim", 250, 60f, new float[2] { 100f, 50f }, new string[2] { "Metal", "Glasses" }, 800f, BuildLocationRule.Anywhere, DECOR.PENALTY.TIER0, NOISE_POLLUTION.NOISY.TIER0);
		buildingDef.ObjectLayer = ObjectLayer.Backwall;
		buildingDef.SceneLayer = Grid.SceneLayer.Backwall;
		buildingDef.ForegroundLayer = Grid.SceneLayer.BuildingBack;
		buildingDef.PermittedRotations = PermittedRotations.Unrotatable;
		buildingDef.DragBuild = true;
		buildingDef.Replaceable = false;
		buildingDef.Overheatable = false;
		buildingDef.Floodable = false;
		buildingDef.Repairable = false;
		buildingDef.ReplacementTags = new List<Tag> { GameTags.Backwall };
		buildingDef.AudioCategory = "Glass";
		buildingDef.AudioSize = "medium";
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		Prioritizable.AddRef(go);
		KPrefabID component = go.GetComponent<KPrefabID>();
		BuildingTemplates.CreateDefaultStorage(go).SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		PlantablePlot plantablePlot = go.AddOrGet<PlantablePlot>();
		plantablePlot.occupyingObjectRelativePosition = new Vector3(0.49f, 0f, -0.5f);
		plantablePlot.AddDepositTag(GameTags.BackwallSeed);
		plantablePlot.AddAdditionalCriteria(ForbiddenTags);
		plantablePlot.SetFertilizationFlags(fertilizer: true, liquid_piping: false);
		plantablePlot.SetReceptacleDirection(SingleEntityReceptacle.ReceptacleDirection.Top);
		go.AddOrGet<CopyBuildingSettings>().copyGroupTag = GameTags.Farm;
		go.AddOrGet<LoopingSounds>();
		component.prefabInitFn += OnPrefabInit;
		go.AddComponent<ZoneTile>();
	}

	private void OnPrefabInit(GameObject instance)
	{
		instance.AddOrGet<PlantablePlot>().AddAdditionalCriteria(ForbiddenTags);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<PlantablePlot>().AddAdditionalCriteria(ForbiddenTags);
	}

	private static bool ForbiddenTags(GameObject objInQuestion)
	{
		return !objInQuestion.GetComponent<KPrefabID>().HasTag(GameTags.DecorSeed);
	}
}
