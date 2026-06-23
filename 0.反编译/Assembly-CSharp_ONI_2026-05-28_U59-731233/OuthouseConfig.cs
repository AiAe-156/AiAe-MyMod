using STRINGS;
using TUNING;
using UnityEngine;

public class OuthouseConfig : IBuildingConfig
{
	public const string ID = "Outhouse";

	private const int USES_PER_REFILL = 15;

	private const float DIRT_PER_REFILL = 200f;

	private const float DIRT_PER_USE = 13f;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("Outhouse", 2, 3, "outhouse_kanim", 30, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.RAW_MINERALS_OR_WOOD, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER4);
		buildingDef.Overheatable = false;
		buildingDef.ExhaustKilowattsWhenActive = 0.25f;
		buildingDef.DiseaseCellVisName = DUPLICANTSTATS.STANDARD.Secretions.PEE_DISEASE;
		buildingDef.AudioCategory = "Metal";
		buildingDef.AddSearchTerms(SEARCH_TERMS.TOILET);
		SoundEventVolumeCache.instance.AddVolume("outhouse_kanim", "Latrine_door_open", NOISE_POLLUTION.NOISY.TIER1);
		SoundEventVolumeCache.instance.AddVolume("outhouse_kanim", "Latrine_door_close", NOISE_POLLUTION.NOISY.TIER1);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		go.AddOrGet<LoopingSounds>();
		component.AddTag(RoomConstraints.ConstraintTags.ToiletType);
		Toilet toilet = go.AddOrGet<Toilet>();
		toilet.maxFlushes = 15;
		toilet.dirtUsedPerFlush = 13f;
		toilet.solidWastePerUse = new Toilet.SpawnInfo(SimHashes.ToxicSand, DUPLICANTSTATS.STANDARD.Secretions.PEE_PER_TOILET_PEE, 0f);
		toilet.solidWasteTemperature = DUPLICANTSTATS.STANDARD.Temperature.Internal.IDEAL;
		toilet.diseaseId = DUPLICANTSTATS.STANDARD.Secretions.PEE_DISEASE;
		toilet.diseasePerFlush = DUPLICANTSTATS.STANDARD.Secretions.DISEASE_PER_PEE;
		toilet.diseaseOnDupePerFlush = DUPLICANTSTATS.STANDARD.Secretions.DISEASE_PER_PEE;
		ToiletWorkableUse toiletWorkableUse = go.AddOrGet<ToiletWorkableUse>();
		toiletWorkableUse.workLayer = Grid.SceneLayer.BuildingFront;
		ToiletWorkableClean toiletWorkableClean = go.AddOrGet<ToiletWorkableClean>();
		toiletWorkableClean.workTime = 90f;
		toiletWorkableClean.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_outhouse_kanim") };
		toiletWorkableClean.workLayer = Grid.SceneLayer.BuildingFront;
		Prioritizable.AddRef(go);
		toiletWorkableClean.SetIsCloggedByGunk(isIt: false);
		Storage storage = go.AddOrGet<Storage>();
		storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		storage.showInUI = true;
		ManualDeliveryKG manualDeliveryKG = go.AddOrGet<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.RequestedItemTag = new Tag("Dirt");
		manualDeliveryKG.capacity = 200f;
		manualDeliveryKG.refillMass = 0.01f;
		manualDeliveryKG.MinimumMass = 200f;
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.FetchCritical.IdHash;
		manualDeliveryKG.operationalRequirement = Operational.State.Functional;
		manualDeliveryKG.FillToCapacity = true;
		Ownable ownable = go.AddOrGet<Ownable>();
		ownable.slotID = Db.Get().AssignableSlots.Toilet.Id;
		ownable.canBePublic = true;
		go.AddOrGetDef<RocketUsageRestriction.Def>();
		component.prefabInitFn += OnInit;
	}

	private void OnInit(GameObject go)
	{
		ToiletWorkableUse component = go.GetComponent<ToiletWorkableUse>();
		KAnimFile[] value = new KAnimFile[1] { Assets.GetAnim("anim_interacts_outhouse_kanim") };
		component.workerTypeOverrideAnims.Add(MinionConfig.ID, value);
		component.workerTypeOverrideAnims.Add(BionicMinionConfig.ID, new KAnimFile[1] { Assets.GetAnim("anim_bionic_interacts_outhouse_kanim") });
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
