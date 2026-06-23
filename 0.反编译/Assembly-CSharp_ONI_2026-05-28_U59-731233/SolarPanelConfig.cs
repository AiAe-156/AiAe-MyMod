using STRINGS;
using TUNING;
using UnityEngine;

public class SolarPanelConfig : IBuildingConfig
{
	public const string ID = "SolarPanel";

	public const float WATTS_PER_LUX = 0.00053f;

	public const float MAX_WATTS = 380f;

	private const int WIDTH = 7;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("SolarPanel", 7, 3, "solar_panel_kanim", 100, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.GLASSES, 2400f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NOISY.TIER5, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		buildingDef.GeneratorWattageRating = 380f;
		buildingDef.GeneratorBaseCapacity = buildingDef.GeneratorWattageRating;
		buildingDef.ExhaustKilowattsWhenActive = 0f;
		buildingDef.SelfHeatKilowattsWhenActive = 0f;
		buildingDef.BuildLocationRule = BuildLocationRule.Anywhere;
		buildingDef.HitPoints = 10;
		buildingDef.RequiresPowerOutput = true;
		buildingDef.PowerOutputOffset = new CellOffset(0, 0);
		buildingDef.ViewMode = OverlayModes.Power.ID;
		buildingDef.AudioCategory = "HollowMetal";
		buildingDef.AudioSize = "large";
		buildingDef.AddSearchTerms(SEARCH_TERMS.POWER);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		component.AddTag(RoomConstraints.ConstraintTags.PowerBuilding);
		component.AddTag(RoomConstraints.ConstraintTags.GeneratorType);
		component.AddTag(RoomConstraints.ConstraintTags.HeavyDutyGeneratorType);
		go.AddOrGet<LoopingSounds>();
		Prioritizable.AddRef(go);
		component.prefabSpawnFn += OnSpawn;
	}

	private void OnSpawn(GameObject instance)
	{
		KBatchedAnimController[] componentsInChildrenOnly = instance.GetComponentsInChildrenOnly<KBatchedAnimController>();
		foreach (KBatchedAnimController kBatchedAnimController in componentsInChildrenOnly)
		{
			if (kBatchedAnimController.name.Contains("_fg"))
			{
				kBatchedAnimController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.LiquidVisibilityLayer, isActive: false);
				kBatchedAnimController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.WaterProof, isActive: true);
			}
		}
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		Repairable repairable = go.AddOrGet<Repairable>();
		repairable.expectedRepairTime = 52.5f;
		SolarPanel solarPanel = go.AddOrGet<SolarPanel>();
		solarPanel.powerDistributionOrder = 9;
		go.AddOrGetDef<PoweredActiveController.Def>();
		MakeBaseSolid.Def def = go.AddOrGetDef<MakeBaseSolid.Def>();
		def.occupyFoundationLayer = false;
		def.solidOffsets = new CellOffset[7];
		for (int i = 0; i < 7; i++)
		{
			def.solidOffsets[i] = new CellOffset(i - 3, 0);
		}
	}
}
