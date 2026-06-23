using STRINGS;
using TUNING;
using UnityEngine;

public class MilkFeederConfig : IBuildingConfig
{
	public const string ID = "MilkFeeder";

	public const string HAD_CONSUMED_MILK_RECENTLY_EFFECT_ID = "HadMilk";

	public const string HAD_CONSUMED_INK_RECENTLY_EFFECT_ID = "HadInk";

	public static readonly Tuple<Tag, string>[] EffectsPerDrinkableLiquid = new Tuple<Tag, string>[2]
	{
		new Tuple<Tag, string>(SimHashes.Milk.CreateTag(), "HadMilk"),
		new Tuple<Tag, string>(SimHashes.Ink.CreateTag(), "HadInk")
	};

	public const float EFFECT_DURATION_IN_SECONDS = 600f;

	public const float UNITS_OF_MILK_CONSUMED_PER_FEEDING = 5f;

	private static readonly CellOffset DRINK_FROM_OFFSET = new CellOffset(1, 0);

	private static readonly Tag MILK_TAG = SimHashes.Milk.CreateTag();

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("MilkFeeder", 3, 3, "critter_milk_feeder_kanim", 100, 120f, new float[2]
		{
			TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4[0],
			TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2[0]
		}, new string[2] { "RefinedMetal", "Glasses" }, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		obj.AudioCategory = "Metal";
		obj.PermittedRotations = PermittedRotations.FlipH;
		obj.InputConduitType = ConduitType.Liquid;
		obj.UtilityInputOffset = new CellOffset(0, 0);
		obj.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		obj.AddSearchTerms(SEARCH_TERMS.RANCHING);
		obj.AddSearchTerms(SEARCH_TERMS.CRITTER);
		return obj;
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		Prioritizable.AddRef(go);
		go.AddOrGet<LogicOperationalController>();
		RoomTracker roomTracker = go.AddOrGet<RoomTracker>();
		roomTracker.requiredRoomType = Db.Get().RoomTypes.CreaturePen.Id;
		roomTracker.requirement = RoomTracker.Requirement.Required;
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = 80f;
		storage.showInUI = true;
		storage.showDescriptor = true;
		storage.allowItemRemoval = false;
		storage.allowSettingOnlyFetchMarkedItems = false;
		storage.showCapacityStatusItem = true;
		storage.showCapacityAsMainStatus = true;
		storage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
		ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
		conduitConsumer.conduitType = ConduitType.Liquid;
		conduitConsumer.consumptionRate = 10f;
		conduitConsumer.capacityTag = GameTags.Creatures.CritterDrinkable;
		conduitConsumer.forceAlwaysSatisfied = true;
		conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;
		conduitConsumer.storage = storage;
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.RanchStationType);
		MilkFeeder.Def def = go.AddOrGetDef<MilkFeeder.Def>();
		def.elementProducedTag = GameTags.Creatures.CritterDrinkable;
		def.unitsProducedPerFeeding = 5f;
		def.drinkCellOffset = DRINK_FROM_OFFSET;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}

	public override void ConfigurePost(BuildingDef def)
	{
	}
}
