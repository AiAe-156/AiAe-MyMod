using STRINGS;
using TUNING;
using UnityEngine;

public class WireRubberBridgeConfig : WireBridgeConfig
{
	public new const string ID = "WireRubberBridge";

	protected override string GetID()
	{
		return "WireRubberBridge";
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = base.CreateBuildingDef();
		buildingDef.AnimFiles = new KAnimFile[1] { Assets.GetAnim("utilityelectricbridgerubber_kanim") };
		buildingDef.Mass = new float[2]
		{
			TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0[0],
			TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER_SMALL[0]
		};
		buildingDef.MaterialCategory = new string[2] { "RefinedMetal", "Rubber&Plastic" };
		buildingDef.AddSearchTerms(SEARCH_TERMS.POWER);
		buildingDef.AddSearchTerms(SEARCH_TERMS.WIRE);
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.WireIDs, "WireRubberBridge");
		return buildingDef;
	}

	protected override WireUtilityNetworkLink AddNetworkLink(GameObject go)
	{
		WireUtilityNetworkLink wireUtilityNetworkLink = base.AddNetworkLink(go);
		wireUtilityNetworkLink.maxWattageRating = Wire.WattageRating.Max4000;
		return wireUtilityNetworkLink;
	}
}
