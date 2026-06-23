using TUNING;
using UnityEngine;

public class BottleEmptierConduitLiquidConfig : IBuildingConfig
{
	public const string ID = "BottleEmptierConduitLiquid";

	private const int WIDTH = 1;

	private const int HEIGHT = 2;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("BottleEmptierConduitLiquid", 1, 2, "bottle_emptier_liquid_conduit_kanim", 30, 60f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER1, decor: BUILDINGS.DECOR.PENALTY.TIER2);
		obj.Floodable = false;
		obj.AudioCategory = "Metal";
		obj.Overheatable = false;
		obj.PermittedRotations = PermittedRotations.FlipH;
		obj.OutputConduitType = ConduitType.Liquid;
		obj.UtilityOutputOffset = new CellOffset(0, 0);
		obj.ViewMode = OverlayModes.LiquidConduits.ID;
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, "BottleEmptierConduitLiquid");
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		Prioritizable.AddRef(go);
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		component.AddTag(GameTags.OverlayBehindConduits);
		Storage storage = go.AddOrGet<Storage>();
		storage.storageFilters = STORAGEFILTERS.LIQUIDS;
		storage.showInUI = true;
		storage.showDescriptor = true;
		storage.capacityKg = 200f;
		storage.gunTargetOffset = new Vector2(0f, 1f);
		go.AddOrGet<TreeFilterable>();
		BottleEmptier bottleEmptier = go.AddOrGet<BottleEmptier>();
		bottleEmptier.emit = false;
		bottleEmptier.emptyRate = 5f;
		ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
		conduitDispenser.conduitType = ConduitType.Liquid;
		conduitDispenser.elementFilter = null;
		conduitDispenser.storage = storage;
		go.AddOrGet<RequireOutputs>().ignoreFullPipe = true;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
