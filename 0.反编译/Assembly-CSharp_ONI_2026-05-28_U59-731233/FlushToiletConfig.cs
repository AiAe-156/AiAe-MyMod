using STRINGS;
using TUNING;
using UnityEngine;

public class FlushToiletConfig : IBuildingConfig
{
	private const float WATER_USAGE = 5f;

	public const string ID = "FlushToilet";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("FlushToilet", 2, 3, "toiletflush_kanim", 30, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.RAW_METALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
		buildingDef.Overheatable = false;
		buildingDef.ExhaustKilowattsWhenActive = 0.25f;
		buildingDef.SelfHeatKilowattsWhenActive = 0f;
		buildingDef.InputConduitType = ConduitType.Liquid;
		buildingDef.OutputConduitType = ConduitType.Liquid;
		buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
		buildingDef.DiseaseCellVisName = DUPLICANTSTATS.STANDARD.Secretions.PEE_DISEASE;
		buildingDef.AudioCategory = "Metal";
		buildingDef.UtilityInputOffset = new CellOffset(0, 0);
		buildingDef.UtilityOutputOffset = new CellOffset(1, 1);
		buildingDef.PermittedRotations = PermittedRotations.FlipH;
		buildingDef.AddSearchTerms(SEARCH_TERMS.TOILET);
		SoundEventVolumeCache.instance.AddVolume("toiletflush_kanim", "Lavatory_flush", NOISE_POLLUTION.NOISY.TIER3);
		SoundEventVolumeCache.instance.AddVolume("toiletflush_kanim", "Lavatory_door_close", NOISE_POLLUTION.NOISY.TIER1);
		SoundEventVolumeCache.instance.AddVolume("toiletflush_kanim", "Lavatory_door_open", NOISE_POLLUTION.NOISY.TIER1);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		go.AddOrGet<LoopingSounds>();
		component.AddTag(RoomConstraints.ConstraintTags.ToiletType);
		component.AddTag(RoomConstraints.ConstraintTags.FlushToiletType);
		FlushToilet flushToilet = go.AddOrGet<FlushToilet>();
		flushToilet.massConsumedPerUse = 5f;
		flushToilet.massEmittedPerUse = 5f + DUPLICANTSTATS.STANDARD.Secretions.PEE_PER_TOILET_PEE;
		flushToilet.newPeeTemperature = DUPLICANTSTATS.STANDARD.Temperature.Internal.IDEAL;
		flushToilet.diseaseId = DUPLICANTSTATS.STANDARD.Secretions.PEE_DISEASE;
		flushToilet.diseasePerFlush = DUPLICANTSTATS.STANDARD.Secretions.DISEASE_PER_PEE;
		flushToilet.diseaseOnDupePerFlush = DUPLICANTSTATS.STANDARD.Secretions.DISEASE_PER_PEE / 20;
		flushToilet.requireOutput = true;
		ToiletWorkableUse toiletWorkableUse = go.AddOrGet<ToiletWorkableUse>();
		toiletWorkableUse.workLayer = Grid.SceneLayer.BuildingFront;
		toiletWorkableUse.resetProgressOnStop = true;
		ToiletWorkableClean toiletWorkableClean = go.AddOrGet<ToiletWorkableClean>();
		toiletWorkableClean.workTime = 90f;
		toiletWorkableClean.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_toiletflush_kanim") };
		toiletWorkableClean.workLayer = Grid.SceneLayer.BuildingFront;
		toiletWorkableClean.SetIsCloggedByGunk(isIt: true);
		ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
		conduitConsumer.conduitType = ConduitType.Liquid;
		conduitConsumer.capacityTag = ElementLoader.FindElementByHash(SimHashes.Water).tag;
		conduitConsumer.capacityKG = 5f;
		conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Store;
		ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
		conduitDispenser.conduitType = ConduitType.Liquid;
		conduitDispenser.invertElementFilter = true;
		conduitDispenser.elementFilter = new SimHashes[1] { SimHashes.Water };
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = 25f;
		storage.doDiseaseTransfer = false;
		storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		Ownable ownable = go.AddOrGet<Ownable>();
		ownable.slotID = Db.Get().AssignableSlots.Toilet.Id;
		ownable.canBePublic = true;
		RequireOutputs requireOutputs = go.AddOrGet<RequireOutputs>();
		requireOutputs.ignoreFullPipe = true;
		Prioritizable.AddRef(go);
		go.AddOrGetDef<RocketUsageRestriction.Def>();
		component.prefabInitFn += OnInit;
	}

	private void OnInit(GameObject go)
	{
		ToiletWorkableUse component = go.GetComponent<ToiletWorkableUse>();
		HashedString[] value = new HashedString[1] { "working_pst" };
		KAnimFile[] value2 = new KAnimFile[1] { Assets.GetAnim("anim_interacts_toiletflush_kanim") };
		component.workerTypeOverrideAnims.Add(MinionConfig.ID, value2);
		component.workerTypePstAnims.Add(MinionConfig.ID, value);
		component.workerTypeOverrideAnims.Add(BionicMinionConfig.ID, new KAnimFile[1] { Assets.GetAnim("anim_bionic_interacts_toiletflush_kanim") });
		component.workerTypePstAnims.Add(BionicMinionConfig.ID, new HashedString[1] { "working_gunky_pst" });
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
