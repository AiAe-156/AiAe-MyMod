using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class FetchDroneConfig : IEntityConfig, IHasDlcRestrictions
{
	public struct LaserEffect
	{
		public string id;

		public string animFile;

		public string anim;

		public HashedString context;
	}

	public const string ID = "FetchDrone";

	public const SimHashes MATERIAL = SimHashes.Steel;

	public const float MASS = 200f;

	private const float WIDTH = 1f;

	private const float HEIGHT = 1f;

	private const float CARRY_AMOUNT = 200f;

	private const float HIT_POINTS = 50f;

	private string name = STRINGS.ROBOTS.MODELS.FLYDO.NAME;

	private string desc = STRINGS.ROBOTS.MODELS.FLYDO.DESC;

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC3;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateBasicEntity("FetchDrone", name, desc, 200f, unitMass: true, Assets.GetAnim("swoopy_bot_kanim"), "idle_loop", Grid.SceneLayer.Move, SimHashes.Creature, new List<Tag>
		{
			GameTags.Robots.Behaviours.HasDoorPermissions,
			GameTags.Experimental
		});
		KBatchedAnimController component = gameObject.GetComponent<KBatchedAnimController>();
		component.isMovable = true;
		gameObject.AddOrGet<LoopingSounds>();
		KBoxCollider2D kBoxCollider2D = gameObject.AddOrGet<KBoxCollider2D>();
		kBoxCollider2D.size = new Vector2(1f, 1f);
		kBoxCollider2D.offset = new Vector2f(0f, 0.5f);
		Modifiers modifiers = gameObject.AddOrGet<Modifiers>();
		modifiers.initialAmounts.Add(Db.Get().Amounts.HitPoints.Id);
		modifiers.initialAttributes.Add(Db.Get().Attributes.CarryAmount.Id);
		modifiers.initialAmounts.Add(Db.Get().Amounts.InternalElectroBank.Id);
		string text = "FetchDroneBaseTrait";
		Traits traits = gameObject.AddOrGet<Traits>();
		Trait trait = Db.Get().CreateTrait(text, name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Attributes.CarryAmount.Id, TUNING.ROBOTS.FETCHDRONE.CARRY_CAPACITY, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.InternalElectroBank.maxAttribute.Id, 120000f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.InternalElectroBank.deltaAttribute.Id, -50f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, TUNING.ROBOTS.FETCHDRONE.HIT_POINTS, name));
		modifiers.initialTraits.Add(text);
		gameObject.AddOrGet<AttributeConverters>();
		GridVisibility gridVisibility = gameObject.AddOrGet<GridVisibility>();
		gridVisibility.radius = 30;
		gridVisibility.innerRadius = 20f;
		StandardWorker standardWorker = gameObject.AddOrGet<StandardWorker>();
		standardWorker.isFetchDrone = true;
		gameObject.AddOrGet<Effects>();
		gameObject.AddOrGet<Traits>();
		gameObject.AddOrGet<AnimEventHandler>();
		MoverLayerOccupier moverLayerOccupier = gameObject.AddOrGet<MoverLayerOccupier>();
		moverLayerOccupier.objectLayers = new ObjectLayer[2]
		{
			ObjectLayer.Rover,
			ObjectLayer.Mover
		};
		moverLayerOccupier.cellOffsets = new CellOffset[2]
		{
			CellOffset.none,
			new CellOffset(0, 1)
		};
		gameObject.AddOrGet<FetchDrone>();
		Storage storage = gameObject.AddComponent<Storage>();
		storage.fxPrefix = Storage.FXPrefix.PickedUp;
		storage.dropOnLoad = true;
		storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		gameObject.AddOrGetDef<DebugGoToMonitor.Def>();
		Deconstructable deconstructable = gameObject.AddOrGet<Deconstructable>();
		deconstructable.enabled = false;
		deconstructable.audioSize = "medium";
		deconstructable.looseEntityDeconstructable = true;
		Storage storage2 = gameObject.AddComponent<Storage>();
		storage2.storageID = GameTags.ChargedPortableBattery;
		storage2.showInUI = true;
		storage2.storageFilters = new List<Tag> { GameTags.ChargedPortableBattery };
		storage2.SetDefaultStoredItemModifiers(new List<Storage.StoredItemModifier>
		{
			Storage.StoredItemModifier.Hide,
			Storage.StoredItemModifier.Insulate
		});
		TreeFilterable treeFilterable = gameObject.AddOrGet<TreeFilterable>();
		treeFilterable.storageToFilterTag = storage2.storageID;
		treeFilterable.dropIncorrectOnFilterChange = false;
		treeFilterable.tintOnNoFiltersSet = false;
		ManualDeliveryKG manualDeliveryKG = gameObject.AddOrGet<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage2);
		manualDeliveryKG.RequestedItemTag = GameTags.ChargedPortableBattery;
		manualDeliveryKG.capacity = 21f;
		manualDeliveryKG.refillMass = 21f;
		manualDeliveryKG.MinimumMass = 1f;
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.RepairFetch.IdHash;
		RobotElectroBankMonitor.Def def = gameObject.AddOrGetDef<RobotElectroBankMonitor.Def>();
		def.lowBatteryWarningPercent = 0.2f;
		RobotAi.Def def2 = gameObject.AddOrGetDef<RobotAi.Def>();
		def2.DeleteOnDead = true;
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new RobotDeathStates.Def
		{
			deathAnim = "idle_dead"
		}, condition: true, Db.Get().ChoreTypes.Die.priority).Add(new RobotElectroBankDeadStates.Def(), condition: true, Db.Get().ChoreTypes.Die.priority).Add(new DebugGoToStates.Def())
			.Add(new DrowningStates.Def())
			.Add(new IdleStates.Def
			{
				priorityClass = PriorityScreen.PriorityClass.idle
			}, condition: true, Db.Get().ChoreTypes.Idle.priority);
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Robots.Models.FetchDrone, null);
		KPrefabID kPrefabID = gameObject.AddOrGet<KPrefabID>();
		kPrefabID.RemoveTag(GameTags.CreatureBrain);
		kPrefabID.AddTag(GameTags.DupeBrain);
		kPrefabID.AddTag(GameTags.Robot);
		Navigator navigator = gameObject.AddOrGet<Navigator>();
		navigator.NavGridName = "RobotFlyerGrid1x1";
		navigator.CurrentNavType = NavType.Hover;
		navigator.defaultSpeed = 2f;
		navigator.updateProber = true;
		navigator.executePathProbeTaskAsync = true;
		navigator.sceneLayer = Grid.SceneLayer.Creatures;
		gameObject.AddOrGet<Sensors>();
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		pickupable.handleFallerComponents = false;
		pickupable.SetWorkTime(5f);
		Clearable clearable = gameObject.AddOrGet<Clearable>();
		clearable.isClearable = false;
		gameObject.AddOrGet<SnapOn>();
		gameObject.AddOrGet<Movable>();
		SetupLaserEffects(gameObject);
		component.SetSymbolVisiblity("snapto_pivot", is_visible: false);
		component.SetSymbolVisiblity("snapto_thing", is_visible: false);
		component.SetSymbolVisiblity("snapTo_chest", is_visible: false);
		gameObject.AddOrGet<EntombVulnerable>();
		OccupyArea occupyArea = gameObject.AddComponent<OccupyArea>();
		occupyArea.SetCellOffsets(new CellOffset[1] { CellOffset.none });
		gameObject.AddOrGet<DrowningMonitor>();
		gameObject.AddOrGetDef<SubmergedMonitor.Def>();
		gameObject.AddOrGet<Health>();
		MoveToLocationMonitor.Def def3 = gameObject.AddOrGetDef<MoveToLocationMonitor.Def>();
		def3.invalidTagsForMoveTo = new Tag[1] { GameTags.Robots.Behaviours.NoElectroBank };
		SymbolOverrideControllerUtil.AddToPrefab(gameObject);
		gameObject.AddOrGet<CopyBuildingSettings>();
		return gameObject;
	}

	private static void SetupLaserEffects(GameObject prefab)
	{
		GameObject gameObject = new GameObject("LaserEffect");
		gameObject.transform.parent = prefab.transform;
		KBatchedAnimEventToggler kBatchedAnimEventToggler = gameObject.AddComponent<KBatchedAnimEventToggler>();
		kBatchedAnimEventToggler.eventSource = prefab;
		kBatchedAnimEventToggler.enableEvent = "LaserOn";
		kBatchedAnimEventToggler.disableEvent = "LaserOff";
		kBatchedAnimEventToggler.entries = new List<KBatchedAnimEventToggler.Entry>();
		LaserEffect[] array = new LaserEffect[14]
		{
			new LaserEffect
			{
				id = "DigEffect",
				animFile = "laser_kanim",
				anim = "idle",
				context = "dig"
			},
			new LaserEffect
			{
				id = "BuildEffect",
				animFile = "construct_beam_kanim",
				anim = "loop",
				context = "build"
			},
			new LaserEffect
			{
				id = "FetchLiquidEffect",
				animFile = "hose_fx_kanim",
				anim = "loop",
				context = "fetchliquid"
			},
			new LaserEffect
			{
				id = "PaintEffect",
				animFile = "paint_beam_kanim",
				anim = "loop",
				context = "paint"
			},
			new LaserEffect
			{
				id = "HarvestEffect",
				animFile = "plant_harvest_beam_kanim",
				anim = "loop",
				context = "harvest"
			},
			new LaserEffect
			{
				id = "CaptureEffect",
				animFile = "net_gun_fx_kanim",
				anim = "loop",
				context = "capture"
			},
			new LaserEffect
			{
				id = "AttackEffect",
				animFile = "attack_beam_fx_kanim",
				anim = "loop",
				context = "attack"
			},
			new LaserEffect
			{
				id = "PickupEffect",
				animFile = "vacuum_fx_kanim",
				anim = "loop",
				context = "pickup"
			},
			new LaserEffect
			{
				id = "StoreEffect",
				animFile = "vacuum_reverse_fx_kanim",
				anim = "loop",
				context = "store"
			},
			new LaserEffect
			{
				id = "DisinfectEffect",
				animFile = "plant_spray_beam_kanim",
				anim = "loop",
				context = "disinfect"
			},
			new LaserEffect
			{
				id = "TendEffect",
				animFile = "plant_tending_beam_fx_kanim",
				anim = "loop",
				context = "tend"
			},
			new LaserEffect
			{
				id = "PowerTinkerEffect",
				animFile = "electrician_beam_fx_kanim",
				anim = "idle",
				context = "powertinker"
			},
			new LaserEffect
			{
				id = "SpecialistDigEffect",
				animFile = "senior_miner_beam_fx_kanim",
				anim = "idle",
				context = "specialistdig"
			},
			new LaserEffect
			{
				id = "DemolishEffect",
				animFile = "poi_demolish_fx_kanim",
				anim = "idle",
				context = "demolish"
			}
		};
		KBatchedAnimController component = prefab.GetComponent<KBatchedAnimController>();
		LaserEffect[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			LaserEffect laserEffect = array2[i];
			GameObject gameObject2 = new GameObject(laserEffect.id);
			gameObject2.transform.parent = gameObject.transform;
			KPrefabID kPrefabID = gameObject2.AddOrGet<KPrefabID>();
			kPrefabID.PrefabTag = new Tag(laserEffect.id);
			KBatchedAnimTracker kBatchedAnimTracker = gameObject2.AddOrGet<KBatchedAnimTracker>();
			kBatchedAnimTracker.controller = component;
			kBatchedAnimTracker.symbol = new HashedString("snapto_thing");
			kBatchedAnimTracker.offset = new Vector3(40f, 0f, 0f);
			kBatchedAnimTracker.useTargetPoint = true;
			KBatchedAnimController kBatchedAnimController = gameObject2.AddOrGet<KBatchedAnimController>();
			kBatchedAnimController.AnimFiles = new KAnimFile[1] { Assets.GetAnim(laserEffect.animFile) };
			KBatchedAnimEventToggler.Entry item = new KBatchedAnimEventToggler.Entry
			{
				anim = laserEffect.anim,
				context = laserEffect.context,
				controller = kBatchedAnimController
			};
			kBatchedAnimEventToggler.entries.Add(item);
			gameObject2.AddOrGet<LoopingSounds>();
		}
	}

	public void OnPrefabInit(GameObject inst)
	{
		ChoreConsumer component = inst.GetComponent<ChoreConsumer>();
		if (component != null)
		{
			component.AddProvider(GlobalChoreProvider.Instance);
		}
	}

	public void OnSpawn(GameObject inst)
	{
		Sensors component = inst.GetComponent<Sensors>();
		component.Add(new PathProberSensor(component));
		component.Add(new PickupableSensor(component));
		LoopingSounds component2 = inst.GetComponent<LoopingSounds>();
		component2.StartSound(GlobalAssets.GetSound("Flydo_flying_LP"));
		Movable component3 = inst.GetComponent<Movable>();
		component3.tagRequiredForMove = GameTags.Robots.Behaviours.NoElectroBank;
		component3.onDeliveryComplete = delegate(GameObject go)
		{
			go.GetComponent<KBatchedAnimController>().Play("dead_battery");
		};
		component3.onPickupComplete = delegate(GameObject go)
		{
			go.GetComponent<KBatchedAnimController>().Play("in_storage");
		};
		Navigator navigator = inst.AddOrGet<Navigator>();
		navigator.transitionDriver.overrideLayers.Add(new DoorTransitionLayer(navigator));
		navigator.reportOccupation = true;
	}
}
