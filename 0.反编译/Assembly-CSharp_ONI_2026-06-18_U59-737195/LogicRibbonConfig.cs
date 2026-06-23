using STRINGS;
using TUNING;
using UnityEngine;

public class LogicRibbonConfig : BaseLogicWireConfig
{
	public const string ID = "LogicRibbon";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = CreateBuildingDef("LogicRibbon", "logic_ribbon_kanim", 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER0);
		obj.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		return obj;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		DoPostConfigureComplete(LogicWire.BitDepth.FourBit, go);
	}
}
