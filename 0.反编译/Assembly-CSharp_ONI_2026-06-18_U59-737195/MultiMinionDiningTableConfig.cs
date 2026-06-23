using STRINGS;
using TUNING;
using UnityEngine;

public class MultiMinionDiningTableConfig : IBuildingConfig
{
	public struct Seat
	{
		private readonly HashedString eatAnim;

		private readonly HashedString reloadElectrobankAnim;

		private readonly HashedString saltSymbol;

		private CellOffset tableRelativeLocation;

		public readonly HashedString EatAnim => eatAnim;

		public readonly HashedString ReloadElectrobankAnim => reloadElectrobankAnim;

		public readonly HashedString SaltSymbol => saltSymbol;

		public readonly CellOffset TableRelativeLocation => tableRelativeLocation;

		public Seat(HashedString eatAnim, HashedString reloadElectrobankAnim, HashedString saltSymbol, CellOffset tableRelativeLocation)
		{
			this.eatAnim = eatAnim;
			this.reloadElectrobankAnim = reloadElectrobankAnim;
			this.saltSymbol = saltSymbol;
			this.tableRelativeLocation = tableRelativeLocation;
		}
	}

	public const string ID = "MultiMinionDiningTable";

	public static readonly Seat[] seats = new Seat[3]
	{
		new Seat("anim_eat_table_kanim", "anim_bionic_eat_table_kanim", "saltshaker", new CellOffset(0, 0)),
		new Seat("anim_eat_table_L_kanim", "anim_bionic_eat_table_L_kanim", "saltshaker_L", new CellOffset(-1, 0)),
		new Seat("anim_eat_table_R_kanim", "anim_bionic_eat_table_R_kanim", "saltshaker_R", new CellOffset(1, 0))
	};

	public static int SeatCount => seats.Length;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("MultiMinionDiningTable", 5, 1, "multi_dupe_table_kanim", 10, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.WOODS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER2);
		obj.WorkTime = 20f;
		obj.Overheatable = false;
		obj.AudioCategory = "Metal";
		obj.AddSearchTerms(SEARCH_TERMS.DINING);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.DiningTableType);
		go.AddOrGetDef<RocketUsageRestriction.Def>();
		go.AddOrGet<MultiMinionDiningTable>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<KAnimControllerBase>().initialAnim = "off";
		Storage storage = BuildingTemplates.CreateDefaultStorage(go);
		storage.showInUI = true;
		storage.capacityKg = (TableSaltTuning.SALTSHAKERSTORAGEMASS + CaviarTuning.STORAGEMASS) * (float)SeatCount;
		ManualDeliveryKG manualDeliveryKG = go.AddOrGet<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.RequestedItemTag = TableSaltConfig.ID.ToTag();
		manualDeliveryKG.capacity = TableSaltTuning.SALTSHAKERSTORAGEMASS * (float)SeatCount;
		manualDeliveryKG.refillMass = TableSaltTuning.CONSUMABLE_RATE * (float)SeatCount;
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.FoodFetch.IdHash;
		manualDeliveryKG.ShowStatusItem = false;
		ManualDeliveryKG manualDeliveryKG2 = go.AddComponent<ManualDeliveryKG>();
		manualDeliveryKG2.SetStorage(storage);
		manualDeliveryKG2.RequestedItemTag = CaviarConfig.TAG;
		manualDeliveryKG2.capacity = CaviarTuning.STORAGEMASS * (float)SeatCount;
		manualDeliveryKG2.refillMass = CaviarTuning.CONSUMABLE_RATE * (float)SeatCount;
		manualDeliveryKG2.choreTypeIDHash = Db.Get().ChoreTypes.FoodFetch.IdHash;
		manualDeliveryKG2.ShowStatusItem = false;
		SymbolOverrideControllerUtil.AddToPrefab(go);
	}
}
