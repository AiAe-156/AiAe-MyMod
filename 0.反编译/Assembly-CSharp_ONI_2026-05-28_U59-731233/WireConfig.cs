using STRINGS;
using TUNING;
using UnityEngine;

public class WireConfig : BaseWireConfig
{
	public const string ID = "Wire";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = CreateBuildingDef("Wire", "utilities_electric_kanim", 3f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0, 0.05f, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER0);
		buildingDef.AddSearchTerms(SEARCH_TERMS.POWER);
		buildingDef.AddSearchTerms(SEARCH_TERMS.WIRE);
		return buildingDef;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		DoPostConfigureComplete(Wire.WattageRating.Max1000, go);
	}
}
