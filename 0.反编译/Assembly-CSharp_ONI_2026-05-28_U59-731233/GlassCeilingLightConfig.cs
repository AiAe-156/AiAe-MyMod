using TUNING;
using UnityEngine;

public class GlassCeilingLightConfig : IBuildingConfig
{
	public const string ID = "GlassCeilingLight";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("GlassCeilingLight", 1, 1, "glassceilinglight_jelly_green_kanim", 10, 10f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, MATERIALS.GLASSES, 800f, BuildLocationRule.OnCeiling, noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.BONUS.TIER1);
		buildingDef.Floodable = false;
		buildingDef.RequiresPowerInput = true;
		buildingDef.EnergyConsumptionWhenActive = 10f;
		buildingDef.SelfHeatKilowattsWhenActive = 0.5f;
		buildingDef.ViewMode = OverlayModes.Light.ID;
		buildingDef.AudioCategory = "Glass";
		return buildingDef;
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		LightShapePreview lightShapePreview = go.AddComponent<LightShapePreview>();
		lightShapePreview.lux = 1800;
		lightShapePreview.radius = 8f;
		lightShapePreview.shape = LightShape.Cone;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.GetComponent<KPrefabID>().AddTag(GameTags.LightSource);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LoopingSounds>();
		Light2D light2D = go.AddOrGet<Light2D>();
		light2D.overlayColour = LIGHT2D.GLASSCEILINGLIGHT_GREEN_OVERLAY;
		light2D.Color = LIGHT2D.GLASSCEILINGLIGHT_GREEN;
		light2D.Range = 8f;
		light2D.Angle = 2.6f;
		light2D.Direction = LIGHT2D.CEILINGLIGHT_DIRECTION;
		light2D.Offset = LIGHT2D.CEILINGLIGHT_OFFSET;
		light2D.shape = LightShape.Cone;
		light2D.drawOverlay = true;
		light2D.Lux = 1800;
		go.AddOrGetDef<LightController.Def>();
	}
}
