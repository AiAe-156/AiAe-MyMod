using STRINGS;
using TUNING;
using UnityEngine;

public class SuitMarkerConfig : IBuildingConfig
{
	public const string ID = "SuitMarker";

	public override BuildingDef CreateBuildingDef()
	{
		string[] rEFINED_METALS = MATERIALS.REFINED_METALS;
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(construction_mass: TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, construction_materials: rEFINED_METALS, melting_point: 1600f, build_location_rule: BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, id: "SuitMarker", width: 1, height: 3, anim: "changingarea_arrow_kanim", hitpoints: 30, construction_time: 30f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER1);
		buildingDef.PermittedRotations = PermittedRotations.FlipH;
		buildingDef.PreventIdleTraversalPastBuilding = true;
		buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.SuitIDs, "SuitMarker");
		buildingDef.AddSearchTerms(SEARCH_TERMS.ATMOSUIT);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		SuitMarker suitMarker = go.AddOrGet<SuitMarker>();
		suitMarker.LockerTags = new Tag[1]
		{
			new Tag("SuitLocker")
		};
		suitMarker.PathFlag = PathFinder.PotentialPath.Flags.HasAtmoSuit;
		AnimTileable animTileable = go.AddOrGet<AnimTileable>();
		animTileable.tags = new Tag[2]
		{
			new Tag("SuitMarker"),
			new Tag("SuitLocker")
		};
		go.AddTag(GameTags.JetSuitBlocker);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LogicOperationalController>();
	}
}
