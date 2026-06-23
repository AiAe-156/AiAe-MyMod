using System;
using System.Collections.Generic;
using System.Linq;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public static class BaseMinionConfig
{
	public struct LaserEffect
	{
		public string id;

		public string animFile;

		public string anim;

		public HashedString context;
	}

	public struct Dream
	{
		public string id;

		public string animFile;

		public string anim;

		public HashedString context;
	}

	public const int MINION_BASE_SYMBOL_LAYER = 0;

	public const int MINION_HAIR_ALWAYS_HACK_LAYER = 1;

	public const int MINION_EXPRESSION_SYMBOL_LAYER = 2;

	public const int MINION_MOUTH_FLAP_LAYER = 3;

	public const int MINION_CLOTHING_SYMBOL_LAYER = 4;

	public const int MINION_PICKUP_SYMBOL_LAYER = 5;

	public const int MINION_SUIT_SYMBOL_LAYER = 6;

	public static CellOffset[] ATTACK_OFFSETS = CreateAttackCellOffsets(OffsetGroups.InvertedStandardTable);

	public static string GetMinionIDForModel(Tag model)
	{
		return model.ToString();
	}

	public static string GetMinionNameForModel(Tag model)
	{
		return model.ProperName();
	}

	public static string GetMinionBaseTraitIDForModel(Tag model)
	{
		return GetMinionIDForModel(model) + "BaseTrait";
	}

	public static Sprite GetSpriteForMinionModel(Tag model)
	{
		return Assets.GetSprite($"ui_duplicant_{model.ToString().ToLower()}_selection");
	}

	public static GameObject BaseMinion(Tag model, string[] minionAttributes, string[] minionAmounts, AttributeModifier[] minionTraits)
	{
		string minionIDForModel = GetMinionIDForModel(model);
		string minionNameForModel = GetMinionNameForModel(model);
		string minionBaseTraitIDForModel = GetMinionBaseTraitIDForModel(model);
		DUPLICANTSTATS statsFor = DUPLICANTSTATS.GetStatsFor(model);
		GameObject gameObject = EntityTemplates.CreateEntity(minionIDForModel, minionNameForModel);
		gameObject.AddOrGet<StateMachineController>();
		MinionModifiers modifiers = gameObject.AddOrGet<MinionModifiers>();
		gameObject.AddOrGet<Traits>();
		gameObject.AddOrGet<Effects>();
		gameObject.AddOrGet<AttributeLevels>();
		gameObject.AddOrGet<AttributeConverters>();
		AddMinionAttributes(modifiers, minionAttributes);
		AddMinionAmounts(modifiers, minionAmounts);
		AddMinionTraits(minionNameForModel, minionBaseTraitIDForModel, modifiers, minionTraits);
		gameObject.AddOrGet<MinionBrain>();
		KPrefabID kPrefabID = gameObject.AddOrGet<KPrefabID>();
		kPrefabID.AddTag(GameTags.DupeBrain);
		kPrefabID.AddTag(GameTags.BaseMinion);
		gameObject.AddOrGet<StandardWorker>();
		gameObject.AddOrGet<ChoreConsumer>();
		Storage storage = gameObject.AddOrGet<Storage>();
		storage.fxPrefix = Storage.FXPrefix.PickedUp;
		storage.dropOnLoad = true;
		storage.SetDefaultStoredItemModifiers(new List<Storage.StoredItemModifier>
		{
			Storage.StoredItemModifier.Preserve,
			Storage.StoredItemModifier.Seal
		});
		gameObject.AddTag(GameTags.CorrosionProof);
		gameObject.AddOrGet<Health>();
		gameObject.AddOrGet<MinionIdentity>();
		OxygenBreather oxygenBreather = gameObject.AddOrGet<OxygenBreather>();
		oxygenBreather.lowOxygenThreshold = statsFor.BaseStats.LOW_OXYGEN_THRESHOLD;
		oxygenBreather.noOxygenThreshold = statsFor.BaseStats.NO_OXYGEN_THRESHOLD;
		oxygenBreather.O2toCO2conversion = statsFor.BaseStats.OXYGEN_TO_CO2_CONVERSION;
		oxygenBreather.mouthOffset = new Vector2f(0.25f, 0.97f);
		oxygenBreather.minCO2ToEmit = statsFor.BaseStats.MIN_CO2_TO_EMIT;
		WarmBlooded warmBlooded = gameObject.AddOrGet<WarmBlooded>();
		warmBlooded.complexity = WarmBlooded.ComplexityType.FullHomeostasis;
		warmBlooded.IdealTemperature = statsFor.Temperature.Internal.IDEAL;
		warmBlooded.BaseGenerationKW = statsFor.BaseStats.DUPLICANT_BASE_GENERATION_KILOWATTS;
		warmBlooded.WarmingKW = statsFor.BaseStats.DUPLICANT_WARMING_KILOWATTS;
		warmBlooded.KCal2Joules = statsFor.BaseStats.KCAL2JOULES;
		warmBlooded.CoolingKW = statsFor.BaseStats.DUPLICANT_COOLING_KILOWATTS;
		warmBlooded.CaloriesModifierDescription = DUPLICANTS.MODIFIERS.BURNINGCALORIES.NAME;
		warmBlooded.BodyRegulatorModifierDescription = DUPLICANTS.MODIFIERS.HOMEOSTASIS.NAME;
		warmBlooded.BaseTemperatureModifierDescription = minionNameForModel;
		GridVisibility gridVisibility = gameObject.AddOrGet<GridVisibility>();
		gridVisibility.radius = 30;
		gridVisibility.innerRadius = 20f;
		gameObject.AddOrGet<MiningSounds>();
		gameObject.AddOrGet<LoopingSounds>().updatePosition = true;
		gameObject.AddOrGet<SaveLoadRoot>().associatedTag = MinionConfig.ID;
		MoverLayerOccupier moverLayerOccupier = gameObject.AddOrGet<MoverLayerOccupier>();
		moverLayerOccupier.objectLayers = new ObjectLayer[2]
		{
			ObjectLayer.Minion,
			ObjectLayer.Mover
		};
		moverLayerOccupier.cellOffsets = new CellOffset[2]
		{
			CellOffset.none,
			new CellOffset(0, 1)
		};
		Navigator navigator = gameObject.AddOrGet<Navigator>();
		navigator.NavGridName = statsFor.BaseStats.NAV_GRID_NAME;
		navigator.CurrentNavType = NavType.Floor;
		navigator.executePathProbeTaskAsync = true;
		KBatchedAnimController kBatchedAnimController = gameObject.AddOrGet<KBatchedAnimController>();
		kBatchedAnimController.isMovable = true;
		kBatchedAnimController.sceneLayer = Grid.SceneLayer.Move;
		kBatchedAnimController.AnimFiles = new KAnimFile[9]
		{
			Assets.GetAnim("body_comp_default_kanim"),
			Assets.GetAnim("anim_construction_default_kanim"),
			Assets.GetAnim("anim_idles_default_kanim"),
			Assets.GetAnim("anim_loco_firepole_kanim"),
			Assets.GetAnim("anim_loco_new_kanim"),
			Assets.GetAnim("anim_loco_tube_kanim"),
			Assets.GetAnim("anim_construction_firepole_kanim"),
			Assets.GetAnim("anim_construction_jetsuit_kanim"),
			Assets.GetAnim("anim_construction_swim_kanim")
		};
		KBoxCollider2D kBoxCollider2D = gameObject.AddOrGet<KBoxCollider2D>();
		kBoxCollider2D.offset = new Vector2(0f, 0.75f);
		kBoxCollider2D.size = new Vector2(1f, 1.5f);
		gameObject.AddOrGet<SnapOn>().snapPoints = new List<SnapOn.SnapPoint>(new SnapOn.SnapPoint[20]
		{
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "dig",
				buildFile = Assets.GetAnim("excavator_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "build",
				buildFile = Assets.GetAnim("constructor_gun_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "fetchliquid",
				buildFile = Assets.GetAnim("water_gun_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "paint",
				buildFile = Assets.GetAnim("painting_gun_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "harvest",
				buildFile = Assets.GetAnim("plant_harvester_gun_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "capture",
				buildFile = Assets.GetAnim("net_gun_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "attack",
				buildFile = Assets.GetAnim("attack_gun_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "pickup",
				buildFile = Assets.GetAnim("pickupdrop_gun_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "store",
				buildFile = Assets.GetAnim("pickupdrop_gun_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "disinfect",
				buildFile = Assets.GetAnim("plant_spray_gun_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "tend",
				buildFile = Assets.GetAnim("plant_harvester_gun_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "carry",
				automatic = false,
				context = "",
				buildFile = null,
				overrideSymbol = "snapTo_chest"
			},
			new SnapOn.SnapPoint
			{
				pointName = "build",
				automatic = false,
				context = "",
				buildFile = null,
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "remote",
				automatic = false,
				context = "",
				buildFile = null,
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "snapTo_neck",
				automatic = false,
				context = "",
				buildFile = Assets.GetAnim("body_oxygen_kanim"),
				overrideSymbol = "snapTo_neck"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "powertinker",
				buildFile = Assets.GetAnim("electrician_gun_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "specialistdig",
				buildFile = Assets.GetAnim("excavator_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = "mask_oxygen",
				automatic = false,
				context = "",
				buildFile = Assets.GetAnim("mask_oxygen_kanim"),
				overrideSymbol = "snapTo_goggles"
			},
			new SnapOn.SnapPoint
			{
				pointName = "dig",
				automatic = false,
				context = "demolish",
				buildFile = Assets.GetAnim("poi_demolish_gun_kanim"),
				overrideSymbol = "snapTo_rgtHand"
			},
			new SnapOn.SnapPoint
			{
				pointName = TUNING.EQUIPMENT.SHOES.SNAPON0,
				automatic = false,
				context = "",
				buildFile = Assets.GetAnim("rubber_boots_kanim"),
				overrideSymbol = "foot"
			}
		});
		PrimaryElement primaryElement = gameObject.AddOrGet<PrimaryElement>();
		primaryElement.InternalTemperature = statsFor.Temperature.Internal.IDEAL;
		primaryElement.MassPerUnit = statsFor.BaseStats.DEFAULT_MASS;
		primaryElement.ElementID = SimHashes.Creature;
		gameObject.AddOrGet<InfraredTemperatureAmount>();
		gameObject.AddOrGet<ChoreProvider>();
		gameObject.AddOrGetDef<DebugGoToMonitor.Def>();
		gameObject.AddOrGet<Sensors>();
		gameObject.AddOrGet<Chattable>();
		gameObject.AddOrGet<FaceGraph>();
		gameObject.AddOrGet<Accessorizer>();
		gameObject.AddOrGet<WearableAccessorizer>();
		gameObject.AddOrGet<Schedulable>();
		EntityLuminescence.Def def = gameObject.AddOrGetDef<EntityLuminescence.Def>();
		def.lightColor = Color.green;
		def.lightRange = 2f;
		def.lightAngle = 0f;
		def.lightDirection = LIGHT2D.DEFAULT_DIRECTION;
		def.lightOffset = new Vector2(0.05f, 0.5f);
		def.lightShape = LightShape.Circle;
		gameObject.AddOrGet<AnimEventHandler>();
		gameObject.AddOrGet<FactionAlignment>().Alignment = FactionManager.FactionID.Duplicant;
		gameObject.AddOrGet<Weapon>();
		gameObject.AddOrGet<RangedAttackable>();
		gameObject.AddOrGet<CharacterOverlay>().shouldShowName = true;
		OccupyArea occupyArea = gameObject.AddOrGet<OccupyArea>();
		occupyArea.objectLayers = new ObjectLayer[1];
		occupyArea.ApplyToCells = false;
		occupyArea.SetCellOffsets(new CellOffset[2]
		{
			new CellOffset(0, 0),
			new CellOffset(0, 1)
		});
		gameObject.AddOrGet<Pickupable>();
		CreatureSimTemperatureTransfer creatureSimTemperatureTransfer = gameObject.AddOrGet<CreatureSimTemperatureTransfer>();
		creatureSimTemperatureTransfer.SurfaceArea = statsFor.Temperature.SURFACE_AREA;
		creatureSimTemperatureTransfer.Thickness = statsFor.Temperature.SKIN_THICKNESS;
		creatureSimTemperatureTransfer.GroundTransferScale = statsFor.Temperature.GROUND_TRANSFER_SCALE;
		gameObject.AddOrGet<SicknessTrigger>();
		gameObject.AddOrGet<ClothingWearer>();
		gameObject.AddOrGet<SuitEquipper>();
		gameObject.AddOrGet<DecorProvider>().baseRadius = 3f;
		gameObject.AddOrGet<ConsumableConsumer>();
		gameObject.AddOrGet<NoiseListener>();
		gameObject.AddOrGet<MinionResume>();
		DuplicantNoiseLevels.SetupNoiseLevels();
		SetupLaserEffects(gameObject);
		SetupDreams(gameObject);
		SymbolOverrideControllerUtil.AddToPrefab(gameObject).applySymbolOverridesEveryFrame = true;
		ConfigureSymbols(gameObject);
		return gameObject;
	}

	public static void BasePrefabInit(GameObject go, Tag duplicantModel)
	{
		DUPLICANTSTATS statsFor = DUPLICANTSTATS.GetStatsFor(duplicantModel);
		AmountInstance amountInstance = Db.Get().Amounts.ImmuneLevel.Lookup(go);
		amountInstance.value = amountInstance.GetMax();
		Db.Get().Amounts.Stress.Lookup(go).value = 5f;
		Db.Get().Amounts.Temperature.Lookup(go).value = statsFor.Temperature.Internal.IDEAL;
		AmountInstance amountInstance2 = Db.Get().Amounts.Breath.Lookup(go);
		amountInstance2.value = amountInstance2.GetMax();
	}

	public static void BaseOnSpawn(GameObject go, Tag duplicantModel, Func<RationalAi.Instance, StateMachine.Instance>[] rationalAiSM)
	{
		Sensors component = go.GetComponent<Sensors>();
		component.Add(new PathProberSensor(component));
		component.Add(new SafeCellSensor(component));
		component.Add(new IdleCellSensor(component));
		component.Add(new PickupableSensor(component));
		component.Add(new ClosestEdibleSensor(component));
		component.Add(new AssignableReachabilitySensor(component));
		component.Add(new MingleCellSensor(component));
		if (go.TryGetComponent<Traits>(out var component2) && component2.HasTrait("BalloonArtist"))
		{
			component.Add(new BalloonStandCellSensor(component));
		}
		RationalAi.Instance instance = new RationalAi.Instance(go.GetComponent<StateMachineController>(), duplicantModel);
		instance.stateMachinesToRunWhenAlive = rationalAiSM;
		instance.StartSM();
		Navigator component3 = go.GetComponent<Navigator>();
		component3.transitionDriver.overrideLayers.Add(new BipedTransitionLayer(component3, 3.325f, 2.5f));
		component3.transitionDriver.overrideLayers.Add(new DoorTransitionLayer(component3));
		component3.transitionDriver.overrideLayers.Add(new TubeTransitionLayer(component3));
		component3.transitionDriver.overrideLayers.Add(new LadderDiseaseTransitionLayer(component3));
		component3.transitionDriver.overrideLayers.Add(new ReactableTransitionLayer(component3));
		component3.transitionDriver.overrideLayers.Add(new NavTeleportTransitionLayer(component3));
		component3.transitionDriver.overrideLayers.Add(new SplashTransitionLayer(component3));
		component3.transitionDriver.overrideLayers.Add(new BipedSwimTransitionLayer(component3));
	}

	public static AttributeModifier[] BaseMinionTraits(Tag minionModel)
	{
		string minionNameForModel = GetMinionNameForModel(minionModel);
		DUPLICANTSTATS statsFor = DUPLICANTSTATS.GetStatsFor(minionModel);
		AttributeModifier[] array = new AttributeModifier[12]
		{
			new AttributeModifier(Db.Get().Attributes.TransitTubeTravelSpeed.Id, statsFor.BaseStats.TRANSIT_TUBE_TRAVEL_SPEED, minionNameForModel),
			new AttributeModifier(Db.Get().Attributes.AirConsumptionRate.Id, statsFor.BaseStats.OXYGEN_USED_PER_SECOND, minionNameForModel),
			new AttributeModifier(Db.Get().Attributes.MaxUnderwaterTravelCost.Id, statsFor.BaseStats.MAX_UNDERWATER_TRAVEL_COST, minionNameForModel),
			new AttributeModifier(Db.Get().Attributes.DecorExpectation.Id, statsFor.BaseStats.DECOR_EXPECTATION, minionNameForModel),
			new AttributeModifier(Db.Get().Attributes.RoomTemperaturePreference.Id, statsFor.BaseStats.ROOM_TEMPERATURE_PREFERENCE, minionNameForModel),
			new AttributeModifier(Db.Get().Attributes.CarryAmount.Id, statsFor.BaseStats.CARRY_CAPACITY, minionNameForModel),
			new AttributeModifier(Db.Get().Attributes.QualityOfLife.Id, 1f, minionNameForModel),
			new AttributeModifier(Db.Get().Attributes.Sneezyness.Id, 0f, minionNameForModel),
			new AttributeModifier(Db.Get().Attributes.RadiationResistance.Id, 0f, minionNameForModel),
			new AttributeModifier(Db.Get().Amounts.Toxicity.deltaAttribute.Id, 0f, minionNameForModel),
			new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, statsFor.BaseStats.HIT_POINTS, minionNameForModel),
			new AttributeModifier(Db.Get().Amounts.ImmuneLevel.deltaAttribute.Id, statsFor.BaseStats.IMMUNE_LEVEL_RECOVERY, minionNameForModel)
		};
		if (!DlcManager.IsExpansion1Active())
		{
			array = array.Append(new AttributeModifier(Db.Get().Attributes.SpaceNavigation.Id, 1f, minionNameForModel));
		}
		return array;
	}

	public static string[] BaseMinionAttributes()
	{
		return new string[12]
		{
			Db.Get().Attributes.AirConsumptionRate.Id,
			Db.Get().Attributes.MaxUnderwaterTravelCost.Id,
			Db.Get().Attributes.DecorExpectation.Id,
			Db.Get().Attributes.RoomTemperaturePreference.Id,
			Db.Get().Attributes.CarryAmount.Id,
			Db.Get().Attributes.QualityOfLife.Id,
			Db.Get().Attributes.SpaceNavigation.Id,
			Db.Get().Attributes.Sneezyness.Id,
			Db.Get().Attributes.RadiationResistance.Id,
			Db.Get().Attributes.RadiationRecovery.Id,
			Db.Get().Attributes.TransitTubeTravelSpeed.Id,
			Db.Get().Attributes.Luminescence.Id
		};
	}

	public static string[] BaseMinionAmounts()
	{
		return new string[8]
		{
			Db.Get().Amounts.HitPoints.Id,
			Db.Get().Amounts.ImmuneLevel.Id,
			Db.Get().Amounts.Breath.Id,
			Db.Get().Amounts.Stress.Id,
			Db.Get().Amounts.Toxicity.Id,
			Db.Get().Amounts.Temperature.Id,
			Db.Get().Amounts.Decor.Id,
			Db.Get().Amounts.RadiationBalance.Id
		};
	}

	public static Func<RationalAi.Instance, StateMachine.Instance>[] BaseRationalAiStateMachines()
	{
		return new Func<RationalAi.Instance, StateMachine.Instance>[44]
		{
			(RationalAi.Instance smi) => new RadiationMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new InSpaceMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new ThoughtGraph.Instance(smi.master),
			(RationalAi.Instance smi) => new StressMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new EmoteMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new SneezeMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new DecorMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new IncapacitationMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new IdleMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new DoctorMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new SicknessMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new GermExposureMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new RoomMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new TemperatureMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new ExternalTemperatureMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new ScaldingMonitor.Instance(smi.master, new ScaldingMonitor.Def
			{
				defaultScaldingTreshold = 345f
			}),
			(RationalAi.Instance smi) => new ColdImmunityMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new HeatImmunityMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new LightMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new RedAlertMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new CringeMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new FallMonitor.Instance(smi.master, shouldPlayEmotes: true, "anim_emotes_default_kanim"),
			(RationalAi.Instance smi) => new WoundMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new SafeCellMonitor.Instance(smi.master, new SafeCellMonitor.Def()),
			(RationalAi.Instance smi) => new SuffocationMonitor.Instance(smi.master, new SuffocationMonitor.Def()),
			(RationalAi.Instance smi) => new MoveToLocationMonitor.Instance(smi.master, new MoveToLocationMonitor.Def()),
			(RationalAi.Instance smi) => new RocketPassengerMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new ReactionMonitor.Instance(smi.master, new ReactionMonitor.Def()),
			(RationalAi.Instance smi) => new SuitWearer.Instance(smi.master),
			(RationalAi.Instance smi) => new TubeTraveller.Instance(smi.master),
			(RationalAi.Instance smi) => new MingleMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new MournMonitor.Instance(smi.master),
			(RationalAi.Instance smi) => new SpeechMonitor.Instance(smi.master, new SpeechMonitor.Def()),
			(RationalAi.Instance smi) => new BlinkMonitor.Instance(smi.master, new BlinkMonitor.Def()),
			(RationalAi.Instance smi) => new ConversationMonitor.Instance(smi.master, new ConversationMonitor.Def()),
			(RationalAi.Instance smi) => new CoughMonitor.Instance(smi.master, new CoughMonitor.Def()),
			(RationalAi.Instance smi) => new GameplayEventMonitor.Instance(smi.master, new GameplayEventMonitor.Def()),
			(RationalAi.Instance smi) => new GasLiquidExposureMonitor.Instance(smi.master, new GasLiquidExposureMonitor.Def()),
			(RationalAi.Instance smi) => new InspirationEffectMonitor.Instance(smi.master, new InspirationEffectMonitor.Def()),
			(RationalAi.Instance smi) => new SlipperyMonitor.Instance(smi.master, new SlipperyMonitor.Def()),
			(RationalAi.Instance smi) => new PressureMonitor.Instance(smi.master, new PressureMonitor.Def()),
			(RationalAi.Instance smi) => new ThreatMonitor.Instance(smi.master, new ThreatMonitor.Def
			{
				fleethresholdState = DUPLICANTSTATS.GetStatsFor(smi.MinionModel).Combat.FLEE_THRESHOLD,
				offsets = ATTACK_OFFSETS
			}),
			(RationalAi.Instance smi) => new RecreationTimeMonitor.Instance(smi.master, new RecreationTimeMonitor.Def()),
			(RationalAi.Instance smi) => new SwimMonitor.Instance(smi.master)
		};
	}

	private static CellOffset[] CreateAttackCellOffsets(CellOffset[][] table)
	{
		CellOffset[] array = new CellOffset[table.Sum((CellOffset[] row) => row.Length)];
		int num = 0;
		foreach (CellOffset[] array2 in table)
		{
			foreach (CellOffset cellOffset in array2)
			{
				array[num] = cellOffset;
				num++;
			}
		}
		return array;
	}

	public static void SetupDreams(GameObject prefab)
	{
		GameObject gameObject = new GameObject("Dreams");
		gameObject.transform.SetParent(prefab.transform, worldPositionStays: false);
		KBatchedAnimEventToggler kBatchedAnimEventToggler = gameObject.AddComponent<KBatchedAnimEventToggler>();
		kBatchedAnimEventToggler.eventSource = prefab;
		kBatchedAnimEventToggler.enableEvent = "DreamsOn";
		kBatchedAnimEventToggler.disableEvent = "DreamsOff";
		kBatchedAnimEventToggler.entries = new List<KBatchedAnimEventToggler.Entry>();
		Dream[] array = new Dream[1]
		{
			new Dream
			{
				id = "Common Dream",
				animFile = "dream_tear_swirly_kanim",
				anim = "dream_loop",
				context = "sleep"
			}
		};
		KBatchedAnimController component = prefab.GetComponent<KBatchedAnimController>();
		for (int i = 0; i < array.Length; i++)
		{
			Dream dream = array[i];
			GameObject gameObject2 = new GameObject(dream.id);
			gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
			gameObject2.AddOrGet<KPrefabID>().PrefabTag = new Tag(dream.id);
			KBatchedAnimTracker kBatchedAnimTracker = gameObject2.AddOrGet<KBatchedAnimTracker>();
			kBatchedAnimTracker.controller = component;
			kBatchedAnimTracker.symbol = new HashedString("snapto_pivot");
			kBatchedAnimTracker.offset = new Vector3(180f, -300f, 0f);
			kBatchedAnimTracker.useTargetPoint = true;
			KBatchedAnimController kBatchedAnimController = gameObject2.AddOrGet<KBatchedAnimController>();
			kBatchedAnimController.AnimFiles = new KAnimFile[1] { Assets.GetAnim(dream.animFile) };
			KBatchedAnimEventToggler.Entry item = new KBatchedAnimEventToggler.Entry
			{
				anim = dream.anim,
				context = dream.context,
				controller = kBatchedAnimController
			};
			kBatchedAnimEventToggler.entries.Add(item);
			gameObject2.AddOrGet<LoopingSounds>();
		}
	}

	public static void SetupLaserEffects(GameObject prefab)
	{
		GameObject gameObject = new GameObject("LaserEffect");
		gameObject.transform.parent = prefab.transform;
		KBatchedAnimEventToggler kBatchedAnimEventToggler = gameObject.AddComponent<KBatchedAnimEventToggler>();
		kBatchedAnimEventToggler.eventSource = prefab;
		kBatchedAnimEventToggler.enableEvent = "LaserOn";
		kBatchedAnimEventToggler.disableEvent = "LaserOff";
		kBatchedAnimEventToggler.entries = new List<KBatchedAnimEventToggler.Entry>();
		LaserEffect[] obj = new LaserEffect[14]
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
		LaserEffect[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			LaserEffect laserEffect = array[i];
			GameObject gameObject2 = new GameObject(laserEffect.id);
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.AddOrGet<KPrefabID>().PrefabTag = new Tag(laserEffect.id);
			KBatchedAnimTracker kBatchedAnimTracker = gameObject2.AddOrGet<KBatchedAnimTracker>();
			kBatchedAnimTracker.controller = component;
			kBatchedAnimTracker.symbol = new HashedString("snapTo_rgtHand");
			kBatchedAnimTracker.offset = new Vector3(195f, -35f, 0f);
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

	public static void ConfigureSymbols(GameObject go, bool show_defaults = true)
	{
		KBatchedAnimController component = go.GetComponent<KBatchedAnimController>();
		component.SetSymbolVisiblity("snapto_hat", is_visible: false);
		component.SetSymbolVisiblity("snapTo_hat_hair", is_visible: false);
		component.SetSymbolVisiblity("snapTo_headfx", is_visible: false);
		component.SetSymbolVisiblity("snapto_chest", is_visible: false);
		component.SetSymbolVisiblity("snapto_neck", is_visible: false);
		component.SetSymbolVisiblity("snapto_goggles", is_visible: false);
		component.SetSymbolVisiblity("snapto_pivot", is_visible: false);
		component.SetSymbolVisiblity("snapTo_rgtHand", is_visible: false);
		component.SetSymbolVisiblity("neck", show_defaults);
		component.SetSymbolVisiblity("belt", show_defaults);
		component.SetSymbolVisiblity("pelvis", show_defaults);
		component.SetSymbolVisiblity("foot", show_defaults);
		component.SetSymbolVisiblity("leg", show_defaults);
		component.SetSymbolVisiblity("cuff", show_defaults);
		component.SetSymbolVisiblity("arm_sleeve", show_defaults);
		component.SetSymbolVisiblity("arm_lower_sleeve", show_defaults);
		component.SetSymbolVisiblity("torso", show_defaults);
		component.SetSymbolVisiblity("hand_paint", show_defaults);
		component.SetSymbolVisiblity("necklace", is_visible: false);
		component.SetSymbolVisiblity("skirt", is_visible: false);
	}

	public static void CopyVisibleSymbols(GameObject go, GameObject copy)
	{
		KBatchedAnimController component = go.GetComponent<KBatchedAnimController>();
		KBatchedAnimController component2 = copy.GetComponent<KBatchedAnimController>();
		component.SetSymbolVisiblity("snapto_hat", component2.GetSymbolVisiblity("snapto_hat"));
		component.SetSymbolVisiblity("snapTo_hat_hair", component2.GetSymbolVisiblity("snapTo_hat_hair"));
		component.SetSymbolVisiblity("snapTo_hair", component2.GetSymbolVisiblity("snapTo_hair"));
		component.SetSymbolVisiblity("snapTo_headfx", component2.GetSymbolVisiblity("snapTo_headfx"));
		component.SetSymbolVisiblity("snapto_chest", component2.GetSymbolVisiblity("snapto_chest"));
		component.SetSymbolVisiblity("snapto_neck", component2.GetSymbolVisiblity("snapto_neck"));
		component.SetSymbolVisiblity("snapto_goggles", component2.GetSymbolVisiblity("snapto_goggles"));
		component.SetSymbolVisiblity("snapto_pivot", component2.GetSymbolVisiblity("snapto_pivot"));
		component.SetSymbolVisiblity("snapTo_rgtHand", component2.GetSymbolVisiblity("snapTo_rgtHand"));
		component.SetSymbolVisiblity("neck", component2.GetSymbolVisiblity("neck"));
		component.SetSymbolVisiblity("belt", component2.GetSymbolVisiblity("belt"));
		component.SetSymbolVisiblity("pelvis", component2.GetSymbolVisiblity("pelvis"));
		component.SetSymbolVisiblity("foot", component2.GetSymbolVisiblity("foot"));
		component.SetSymbolVisiblity("leg", component2.GetSymbolVisiblity("leg"));
		component.SetSymbolVisiblity("cuff", component2.GetSymbolVisiblity("cuff"));
		component.SetSymbolVisiblity("arm_sleeve", component2.GetSymbolVisiblity("arm_sleeve"));
		component.SetSymbolVisiblity("arm_lower_sleeve", component2.GetSymbolVisiblity("arm_lower_sleeve"));
		component.SetSymbolVisiblity("torso", component2.GetSymbolVisiblity("torso"));
		component.SetSymbolVisiblity("hand_paint", component2.GetSymbolVisiblity("hand_paint"));
		component.SetSymbolVisiblity("necklace", component2.GetSymbolVisiblity("necklace"));
		component.SetSymbolVisiblity("skirt", component2.GetSymbolVisiblity("skirt"));
	}

	public static void AddMinionTraits(string name, string baseTraitID, Modifiers modifiers, AttributeModifier[] traits)
	{
		Trait trait = Db.Get().CreateTrait(baseTraitID, name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		for (int i = 0; i < traits.Length; i++)
		{
			trait.Add(traits[i]);
		}
		modifiers.initialTraits.Add(baseTraitID);
	}

	public static void AddMinionAttributes(Modifiers modifiers, string[] attributes)
	{
		for (int i = 0; i < attributes.Length; i++)
		{
			modifiers.initialAttributes.Add(attributes[i]);
		}
	}

	public static void AddMinionAmounts(Modifiers modifiers, string[] amounts)
	{
		for (int i = 0; i < amounts.Length; i++)
		{
			modifiers.initialAmounts.Add(amounts[i]);
		}
	}
}
