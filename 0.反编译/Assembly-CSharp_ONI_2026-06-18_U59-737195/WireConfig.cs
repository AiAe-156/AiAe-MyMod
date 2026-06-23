using STRINGS;
using TUNING;
using UnityEngine;

public class WireConfig : BaseWireConfig
{
	public const string ID = "Wire";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = CreateBuildingDef("Wire", "utilities_electric_kanim", 3f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0, 0.05f, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER0);
		obj.AddSearchTerms(SEARCH_TERMS.POWER);
		obj.AddSearchTerms(SEARCH_TERMS.WIRE);
		return obj;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		DoPostConfigureComplete(Wire.WattageRating.Max1000, go);
	}
}
