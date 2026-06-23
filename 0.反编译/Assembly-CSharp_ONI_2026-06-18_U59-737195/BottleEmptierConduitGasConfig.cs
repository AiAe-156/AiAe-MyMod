using TUNING;
using UnityEngine;

public class BottleEmptierConduitGasConfig : IBuildingConfig
{
	public const string ID = "BottleEmptierConduitGas";

	private const int WIDTH = 1;

	private const int HEIGHT = 2;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("BottleEmptierConduitGas", 1, 2, "bottle_emptier_gas_conduit_kanim", 30, 60f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER1, decor: BUILDINGS.DECOR.PENALTY.TIER2);
		obj.Floodable = false;
		obj.AudioCategory = "Metal";
		obj.Overheatable = false;
		obj.PermittedRotations = PermittedRotations.FlipH;
		obj.OutputConduitType = ConduitType.Gas;
		obj.UtilityOutputOffset = new CellOffset(0, 0);
		obj.ViewMode = OverlayModes.GasConduits.ID;
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, "BottleEmptierConduitGas");
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		Prioritizable.AddRef(go);
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		component.AddTag(GameTags.OverlayBehindConduits);
		Storage storage = go.AddOrGet<Storage>();
		storage.storageFilters = STORAGEFILTERS.GASES;
		storage.showInUI = true;
		storage.showDescriptor = true;
		storage.capacityKg = 200f;
		storage.gunTargetOffset = new Vector2(0f, 1f);
		go.AddOrGet<TreeFilterable>();
		BottleEmptier bottleEmptier = go.AddOrGet<BottleEmptier>();
		bottleEmptier.isGasEmptier = true;
		bottleEmptier.emptyRate = 0.25f;
		bottleEmptier.emit = false;
		ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
		conduitDispenser.conduitType = ConduitType.Gas;
		conduitDispenser.elementFilter = null;
		conduitDispenser.storage = storage;
		go.AddOrGet<RequireOutputs>().ignoreFullPipe = true;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
