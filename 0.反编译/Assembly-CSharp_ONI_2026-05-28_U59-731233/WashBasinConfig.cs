using STRINGS;
using TUNING;
using UnityEngine;

public class WashBasinConfig : IBuildingConfig
{
	public const string ID = "WashBasin";

	public static readonly int DISEASE_REMOVAL_COUNT = DUPLICANTSTATS.STANDARD.Secretions.DISEASE_PER_PEE + 20000;

	public const float WATER_PER_USE = 5f;

	public const int USES_PER_FLUSH = 40;

	public const float WORK_TIME = 5f;

	public override BuildingDef CreateBuildingDef()
	{
		string[] rAW_MINERALS_OR_METALS = MATERIALS.RAW_MINERALS_OR_METALS;
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(construction_mass: TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, construction_materials: rAW_MINERALS_OR_METALS, melting_point: 1600f, build_location_rule: BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER0, id: "WashBasin", width: 2, height: 3, anim: "wash_basin_kanim", hitpoints: 30, construction_time: 30f, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER1);
		buildingDef.AddSearchTerms(SEARCH_TERMS.SINK);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.WashStation);
		HandSanitizer handSanitizer = go.AddOrGet<HandSanitizer>();
		handSanitizer.massConsumedPerUse = 5f;
		handSanitizer.consumedElement = SimHashes.Water;
		handSanitizer.outputElement = SimHashes.DirtyWater;
		handSanitizer.diseaseRemovalCount = DISEASE_REMOVAL_COUNT;
		handSanitizer.maxUses = 40;
		handSanitizer.dumpWhenFull = true;
		go.AddOrGet<DirectionControl>();
		HandSanitizer.Work work = go.AddOrGet<HandSanitizer.Work>();
		KAnimFile[] overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_washbasin_kanim") };
		work.overrideAnims = overrideAnims;
		work.workTime = 5f;
		work.trackUses = true;
		Storage storage = go.AddOrGet<Storage>();
		storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		ManualDeliveryKG manualDeliveryKG = go.AddOrGet<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.RequestedItemTag = GameTagExtensions.Create(SimHashes.Water);
		manualDeliveryKG.MinimumMass = 5f;
		manualDeliveryKG.capacity = 200f;
		manualDeliveryKG.refillMass = 40f;
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.FetchCritical.IdHash;
		go.AddOrGet<LoopingSounds>();
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.prefabInitFn += OnInit;
	}

	private void OnInit(GameObject go)
	{
		HandSanitizer.Work component = go.GetComponent<HandSanitizer.Work>();
		KAnimFile[] value = new KAnimFile[1] { Assets.GetAnim("anim_interacts_washbasin_kanim") };
		component.workerTypeOverrideAnims.Add(MinionConfig.ID, value);
		component.workerTypeOverrideAnims.Add(BionicMinionConfig.ID, new KAnimFile[1] { Assets.GetAnim("anim_bionic_interacts_washbasin_kanim") });
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
