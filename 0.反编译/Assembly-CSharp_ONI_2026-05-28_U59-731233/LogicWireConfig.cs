using STRINGS;
using TUNING;
using UnityEngine;

public class LogicWireConfig : BaseLogicWireConfig
{
	public const string ID = "LogicWire";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = CreateBuildingDef("LogicWire", "logic_wires_kanim", 3f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER_TINY, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER0);
		buildingDef.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		return buildingDef;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		DoPostConfigureComplete(LogicWire.BitDepth.OneBit, go);
	}
}
