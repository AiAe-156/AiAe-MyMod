using STRINGS;
using TUNING;
using UnityEngine;

public class LogicWireConfig : BaseLogicWireConfig
{
	public const string ID = "LogicWire";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = CreateBuildingDef("LogicWire", "logic_wires_kanim", 3f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER_TINY, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER0);
		obj.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		return obj;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		DoPostConfigureComplete(LogicWire.BitDepth.OneBit, go);
	}
}
