using TUNING;
using UnityEngine;

public class WireRubberConfig : BaseWireConfig
{
	public const string ID = "WireRubber";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = CreateBuildingDef("WireRubber", "utilities_electric_rubber_kanim", 3f, new float[2]
		{
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER0[0],
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER_SMALL[0]
		}, 0.05f, noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.NONE);
		buildingDef.MaterialCategory = new string[2] { "RefinedMetal", "Rubber&Plastic" };
		return buildingDef;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		DoPostConfigureComplete(Wire.WattageRating.Max4000, go);
	}
}
