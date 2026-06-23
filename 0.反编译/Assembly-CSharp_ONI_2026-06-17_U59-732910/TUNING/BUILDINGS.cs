using System;
using System.Collections.Generic;

namespace TUNING;

public class BUILDINGS
{
	public class PHARMACY
	{
		public class FABRICATIONTIME
		{
			public const float TIER0 = 50f;

			public const float TIER1 = 100f;

			public const float TIER2 = 200f;
		}
	}

	public class NUCLEAR_REACTOR
	{
		public class REACTOR_MASSES
		{
			public const float MIN = 1f;

			public const float MAX = 10f;
		}
	}

	public class OVERPRESSURE
	{
		public const float TIER0 = 1.8f;
	}

	public class OVERHEAT_TEMPERATURES
	{
		public const float LOW_3 = 10f;

		public const float LOW_2 = 328.15f;

		public const float LOW_1 = 338.15f;

		public const float NORMAL = 348.15f;

		public const float HIGH_1 = 363.15f;

		public const float HIGH_2 = 398.15f;

		public const float HIGH_3 = 1273.15f;

		public const float HIGH_4 = 2273.15f;
	}

	public class OVERHEAT_MATERIAL_MOD
	{
		public const float LOW_3 = -200f;

		public const float LOW_2 = -20f;

		public const float LOW_1 = -10f;

		public const float NORMAL = 0f;

		public const float HIGH_1 = 15f;

		public const float HIGH_2 = 50f;

		public const float HIGH_3 = 200f;

		public const float HIGH_4 = 500f;

		public const float HIGH_5 = 900f;
	}

	public class DECOR_MATERIAL_MOD
	{
		public const float NORMAL = 0f;

		public const float HIGH_1 = 0.1f;

		public const float HIGH_2 = 0.2f;

		public const float HIGH_3 = 0.5f;

		public const float HIGH_4 = 1f;
	}

	public class CONSTRUCTION_MASS_KG
	{
		public static readonly float[] TIER_TINY = new float[1] { 5f };

		public static readonly float[] TIER_SMALL = new float[1] { 10f };

		public static readonly float[] TIER0 = new float[1] { 25f };

		public static readonly float[] TIER1 = new float[1] { 50f };

		public static readonly float[] TIER2 = new float[1] { 100f };

		public static readonly float[] TIER3 = new float[1] { 200f };

		public static readonly float[] TIER4 = new float[1] { 400f };

		public static readonly float[] TIER5 = new float[1] { 800f };

		public static readonly float[] TIER6 = new float[1] { 1200f };

		public static readonly float[] TIER7 = new float[1] { 2000f };
	}

	public class ROCKETRY_MASS_KG
	{
		public static float[] COMMAND_MODULE_MASS = new float[1] { 200f };

		public static float[] CARGO_MASS = new float[1] { 1000f };

		public static float[] CARGO_MASS_SMALL = new float[1] { 400f };

		public static float[] FUEL_TANK_DRY_MASS = new float[1] { 100f };

		public static float[] FUEL_TANK_WET_MASS = new float[1] { 900f };

		public static float[] FUEL_TANK_WET_MASS_SMALL = new float[1] { 300f };

		public static float[] FUEL_TANK_WET_MASS_GAS = new float[1] { 100f };

		public static float[] FUEL_TANK_WET_MASS_GAS_LARGE = new float[1] { 150f };

		public static float[] OXIDIZER_TANK_OXIDIZER_MASS = new float[1] { 900f };

		public static float[] ENGINE_MASS_SMALL = new float[1] { 200f };

		public static float[] ENGINE_MASS_LARGE = new float[1] { 500f };

		public static float[] NOSE_CONE_TIER1 = new float[2] { 200f, 100f };

		public static float[] NOSE_CONE_TIER2 = new float[2] { 400f, 200f };

		public static float[] HOLLOW_TIER1 = new float[1] { 200f };

		public static float[] HOLLOW_TIER2 = new float[1] { 400f };

		public static float[] HOLLOW_TIER3 = new float[1] { 800f };

		public static float[] DENSE_TIER0 = new float[1] { 200f };

		public static float[] DENSE_TIER1 = new float[1] { 500f };

		public static float[] DENSE_TIER2 = new float[1] { 1000f };

		public static float[] DENSE_TIER3 = new float[1] { 2000f };
	}

	public class ENERGY_CONSUMPTION_WHEN_ACTIVE
	{
		public const float TIER0 = 0f;

		public const float TIER1 = 5f;

		public const float TIER2 = 60f;

		public const float TIER3 = 120f;

		public const float TIER4 = 240f;

		public const float TIER5 = 480f;

		public const float TIER6 = 960f;

		public const float TIER7 = 1200f;

		public const float TIER8 = 1600f;
	}

	public class EXHAUST_ENERGY_ACTIVE
	{
		public const float TIER0 = 0f;

		public const float TIER1 = 0.125f;

		public const float TIER2 = 0.25f;

		public const float TIER3 = 0.5f;

		public const float TIER4 = 1f;

		public const float TIER5 = 2f;

		public const float TIER6 = 4f;

		public const float TIER7 = 8f;

		public const float TIER8 = 16f;
	}

	public class JOULES_LEAK_PER_CYCLE
	{
		public const float TIER0 = 400f;

		public const float TIER1 = 1000f;

		public const float TIER2 = 2000f;
	}

	public class SELF_HEAT_KILOWATTS
	{
		public const float TIER0 = 0f;

		public const float TIER1 = 0.5f;

		public const float TIER2 = 1f;

		public const float TIER3 = 2f;

		public const float TIER4 = 4f;

		public const float TIER5 = 8f;

		public const float TIER6 = 16f;

		public const float TIER7 = 32f;

		public const float TIER8 = 64f;

		public const float TIER_NUCLEAR = 16384f;
	}

	public class MELTING_POINT_KELVIN
	{
		public const float TIER0 = 800f;

		public const float TIER1 = 1600f;

		public const float TIER2 = 2400f;

		public const float TIER3 = 3200f;

		public const float TIER4 = 9999f;
	}

	public class CONSTRUCTION_TIME_SECONDS
	{
		public const float TIER0 = 3f;

		public const float TIER1 = 10f;

		public const float TIER2 = 30f;

		public const float TIER3 = 60f;

		public const float TIER4 = 120f;

		public const float TIER5 = 240f;

		public const float TIER6 = 480f;
	}

	public class HITPOINTS
	{
		public const int TIER0 = 10;

		public const int TIER1 = 30;

		public const int TIER2 = 100;

		public const int TIER3 = 250;

		public const int TIER4 = 1000;
	}

	public class DAMAGE_SOURCES
	{
		public const int CONDUIT_CONTENTS_BOILED = 1;

		public const int CONDUIT_CONTENTS_FROZE = 1;

		public const int BAD_INPUT_ELEMENT = 1;

		public const int BUILDING_OVERHEATED = 1;

		public const int HIGH_LIQUID_PRESSURE = 10;

		public const int MICROMETEORITE = 1;

		public const int CORROSIVE_ELEMENT = 1;
	}

	public class RELOCATION_TIME_SECONDS
	{
		public const float DECONSTRUCT = 4f;

		public const float CONSTRUCT = 4f;
	}

	public class WORK_TIME_SECONDS
	{
		public const float VERYSHORT_WORK_TIME = 5f;

		public const float SHORT_WORK_TIME = 15f;

		public const float MEDIUM_WORK_TIME = 30f;

		public const float LONG_WORK_TIME = 90f;

		public const float VERY_LONG_WORK_TIME = 150f;

		public const float EXTENSIVE_WORK_TIME = 180f;
	}

	public class FABRICATION_TIME_SECONDS
	{
		public const float VERY_SHORT = 20f;

		public const float SHORT = 40f;

		public const float MODERATE = 80f;

		public const float LONG = 250f;
	}

	public class DECOR
	{
		public class BONUS
		{
			public class MONUMENT
			{
				public static readonly EffectorValues COMPLETE = new EffectorValues
				{
					amount = 40,
					radius = 10
				};

				public static readonly EffectorValues INCOMPLETE = new EffectorValues
				{
					amount = 10,
					radius = 5
				};
			}

			public static readonly EffectorValues TIER0 = new EffectorValues
			{
				amount = 5,
				radius = 1
			};

			public static readonly EffectorValues TIER1 = new EffectorValues
			{
				amount = 10,
				radius = 2
			};

			public static readonly EffectorValues TIER2 = new EffectorValues
			{
				amount = 15,
				radius = 3
			};

			public static readonly EffectorValues TIER3 = new EffectorValues
			{
				amount = 20,
				radius = 4
			};

			public static readonly EffectorValues TIER4 = new EffectorValues
			{
				amount = 25,
				radius = 5
			};

			public static readonly EffectorValues TIER5 = new EffectorValues
			{
				amount = 30,
				radius = 6
			};
		}

		public class PENALTY
		{
			public static readonly EffectorValues TIER0 = new EffectorValues
			{
				amount = -5,
				radius = 1
			};

			public static readonly EffectorValues TIER1 = new EffectorValues
			{
				amount = -10,
				radius = 2
			};

			public static readonly EffectorValues TIER2 = new EffectorValues
			{
				amount = -15,
				radius = 3
			};

			public static readonly EffectorValues TIER3 = new EffectorValues
			{
				amount = -20,
				radius = 4
			};

			public static readonly EffectorValues TIER4 = new EffectorValues
			{
				amount = -20,
				radius = 5
			};

			public static readonly EffectorValues TIER5 = new EffectorValues
			{
				amount = -25,
				radius = 6
			};
		}

		public static readonly EffectorValues NONE = new EffectorValues
		{
			amount = 0,
			radius = 1
		};
	}

	public class MASS_KG
	{
		public const float TIER0 = 25f;

		public const float TIER1 = 50f;

		public const float TIER2 = 100f;

		public const float TIER3 = 200f;

		public const float TIER4 = 400f;

		public const float TIER5 = 800f;

		public const float TIER6 = 1200f;

		public const float TIER7 = 2000f;
	}

	public class UPGRADES
	{
		public class MATERIALTAGS
		{
			public const string METAL = "Metal";

			public const string REFINEDMETAL = "RefinedMetal";

			public const string CARBON = "Carbon";
		}

		public class MATERIALMASS
		{
			public const int TIER0 = 100;

			public const int TIER1 = 200;

			public const int TIER2 = 400;

			public const int TIER3 = 500;
		}

		public class MODIFIERAMOUNTS
		{
			public const float MANUALGENERATOR_ENERGYGENERATION = 1.2f;

			public const float MANUALGENERATOR_CAPACITY = 2f;

			public const float PROPANEGENERATOR_ENERGYGENERATION = 1.6f;

			public const float PROPANEGENERATOR_HEATGENERATION = 1.6f;

			public const float GENERATOR_HEATGENERATION = 0.8f;

			public const float GENERATOR_ENERGYGENERATION = 1.3f;

			public const float TURBINE_ENERGYGENERATION = 1.2f;

			public const float TURBINE_CAPACITY = 1.2f;

			public const float SUITRECHARGER_EXECUTIONTIME = 1.2f;

			public const float SUITRECHARGER_HEATGENERATION = 1.2f;

			public const float STORAGELOCKER_CAPACITY = 2f;

			public const float SOLARPANEL_ENERGYGENERATION = 1.2f;

			public const float SMELTER_HEATGENERATION = 0.7f;
		}

		public const float BUILDTIME_TIER0 = 120f;
	}

	public enum PlanSubcategoryName
	{
		ladders,
		tiles,
		printingpods,
		doors,
		storage,
		transport,
		operations,
		producers,
		scrubbers,
		distributors,
		generators,
		wires,
		batteries,
		electrobankbuildings,
		powercontrol,
		switches,
		cooking,
		farming,
		ranching,
		washroom,
		pipes,
		pumps,
		valves,
		sensors,
		buildmenuports,
		organic,
		materials,
		oil,
		advanced,
		hygiene,
		medical,
		wellness,
		beds,
		lights,
		dining,
		recreation,
		decor,
		research,
		archaeology,
		meteordefense,
		exploration,
		industrialstation,
		workstations,
		manufacturing,
		equipment,
		missiles,
		temperature,
		sanitation,
		logicmanager,
		logicaudio,
		logicgates,
		transmissions,
		conveyancestructures,
		automated,
		telescopes,
		rocketstructures,
		fittings,
		rocketnav,
		module,
		cargo,
		engines,
		tanks
	}

	public const float DEFAULT_STORAGE_CAPACITY = 2000f;

	public const float STANDARD_MANUAL_REFILL_LEVEL = 0.2f;

	public const float MASS_TEMPERATURE_SCALE = 0.2f;

	public const float AIRCONDITIONER_TEMPDELTA = -14f;

	public const float MAX_ENVIRONMENT_DELTA = -50f;

	public const float COMPOST_FLIP_TIME = 20f;

	public const int TUBE_LAUNCHER_MAX_CHARGES = 3;

	public const float TUBE_LAUNCHER_RECHARGE_TIME = 10f;

	public const float TUBE_LAUNCHER_WORK_TIME = 1f;

	public const float SMELTER_INGOT_INPUTKG = 500f;

	public const float SMELTER_INGOT_OUTPUTKG = 100f;

	public const float SMELTER_FABRICATIONTIME = 120f;

	public const float GEOREFINERY_SLAB_INPUTKG = 1000f;

	public const float GEOREFINERY_SLAB_OUTPUTKG = 200f;

	public const float GEOREFINERY_FABRICATIONTIME = 120f;

	public const float MASS_BURN_RATE_HYDROGENGENERATOR = 0.1f;

	public const float COOKER_FOOD_TEMPERATURE = 368.15f;

	public const float OVERHEAT_DAMAGE_INTERVAL = 7.5f;

	public const float MIN_BUILD_TEMPERATURE = 0f;

	public const float MAX_BUILD_TEMPERATURE = 318.15f;

	public const float MELTDOWN_TEMPERATURE = 533.15f;

	public const float REPAIR_FORCE_TEMPERATURE = 293.15f;

	public const int REPAIR_EFFECTIVENESS_BASE = 10;

	public static Dictionary<string, string> PLANSUBCATEGORYSORTING = new Dictionary<string, string>
	{
		{
			"Ladder",
			PlanSubcategoryName.ladders.ToString()
		},
		{
			"FirePole",
			PlanSubcategoryName.ladders.ToString()
		},
		{
			"LadderFast",
			PlanSubcategoryName.ladders.ToString()
		},
		{
			"Tile",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"SnowTile",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"WoodTile",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"GasPermeableMembrane",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"MeshTile",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"RubberTile",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"InsulationTile",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"PlasticTile",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"MetalTile",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"GlassTile",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"StorageTile",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"BunkerTile",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"ExteriorWall",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"GlassExteriorWall",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"CarpetTile",
			PlanSubcategoryName.tiles.ToString()
		},
		{
			"ExobaseHeadquarters",
			PlanSubcategoryName.printingpods.ToString()
		},
		{
			"Door",
			PlanSubcategoryName.doors.ToString()
		},
		{
			"WoodenDoor",
			PlanSubcategoryName.doors.ToString()
		},
		{
			"ManualPressureDoor",
			PlanSubcategoryName.doors.ToString()
		},
		{
			"InsulatedDoor",
			PlanSubcategoryName.doors.ToString()
		},
		{
			"PressureDoor",
			PlanSubcategoryName.doors.ToString()
		},
		{
			"BunkerDoor",
			PlanSubcategoryName.doors.ToString()
		},
		{
			"StorageLocker",
			PlanSubcategoryName.storage.ToString()
		},
		{
			"StorageLockerSmart",
			PlanSubcategoryName.storage.ToString()
		},
		{
			"LiquidReservoir",
			PlanSubcategoryName.storage.ToString()
		},
		{
			"GasReservoir",
			PlanSubcategoryName.storage.ToString()
		},
		{
			"ObjectDispenser",
			PlanSubcategoryName.storage.ToString()
		},
		{
			"TravelTube",
			PlanSubcategoryName.transport.ToString()
		},
		{
			"TravelTubeEntrance",
			PlanSubcategoryName.transport.ToString()
		},
		{
			"TravelTubeWallBridge",
			PlanSubcategoryName.transport.ToString()
		},
		{
			RemoteWorkerDockConfig.ID,
			PlanSubcategoryName.operations.ToString()
		},
		{
			RemoteWorkTerminalConfig.ID,
			PlanSubcategoryName.operations.ToString()
		},
		{
			"MineralDeoxidizer",
			PlanSubcategoryName.producers.ToString()
		},
		{
			"SublimationStation",
			PlanSubcategoryName.producers.ToString()
		},
		{
			"Oxysconce",
			PlanSubcategoryName.producers.ToString()
		},
		{
			"Electrolyzer",
			PlanSubcategoryName.producers.ToString()
		},
		{
			"RustDeoxidizer",
			PlanSubcategoryName.producers.ToString()
		},
		{
			"AirFilter",
			PlanSubcategoryName.scrubbers.ToString()
		},
		{
			"CO2Scrubber",
			PlanSubcategoryName.scrubbers.ToString()
		},
		{
			"AlgaeHabitat",
			PlanSubcategoryName.scrubbers.ToString()
		},
		{
			"UnderwaterBreathingStation",
			PlanSubcategoryName.distributors.ToString()
		},
		{
			"DevGenerator",
			PlanSubcategoryName.generators.ToString()
		},
		{
			"ManualGenerator",
			PlanSubcategoryName.generators.ToString()
		},
		{
			"Generator",
			PlanSubcategoryName.generators.ToString()
		},
		{
			"WoodGasGenerator",
			PlanSubcategoryName.generators.ToString()
		},
		{
			"PeatGenerator",
			PlanSubcategoryName.generators.ToString()
		},
		{
			"ReefGenerator",
			PlanSubcategoryName.generators.ToString()
		},
		{
			"HydrogenGenerator",
			PlanSubcategoryName.generators.ToString()
		},
		{
			"MethaneGenerator",
			PlanSubcategoryName.generators.ToString()
		},
		{
			"PetroleumGenerator",
			PlanSubcategoryName.generators.ToString()
		},
		{
			"SteamTurbine",
			PlanSubcategoryName.generators.ToString()
		},
		{
			"SteamTurbine2",
			PlanSubcategoryName.generators.ToString()
		},
		{
			"SolarPanel",
			PlanSubcategoryName.generators.ToString()
		},
		{
			"Wire",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"WireBridge",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"HighWattageWire",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"WireBridgeHighWattage",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"WireRefined",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"WireRefinedBridge",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"WireRefinedHighWattage",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"WireRefinedBridgeHighWattage",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"WireRubber",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"WireRubberBridge",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"Battery",
			PlanSubcategoryName.batteries.ToString()
		},
		{
			"BatteryMedium",
			PlanSubcategoryName.batteries.ToString()
		},
		{
			"BatterySmart",
			PlanSubcategoryName.batteries.ToString()
		},
		{
			"ElectrobankCharger",
			PlanSubcategoryName.electrobankbuildings.ToString()
		},
		{
			"SmallElectrobankDischarger",
			PlanSubcategoryName.electrobankbuildings.ToString()
		},
		{
			"LargeElectrobankDischarger",
			PlanSubcategoryName.electrobankbuildings.ToString()
		},
		{
			"PowerTransformerSmall",
			PlanSubcategoryName.powercontrol.ToString()
		},
		{
			"PowerTransformer",
			PlanSubcategoryName.powercontrol.ToString()
		},
		{
			SwitchConfig.ID,
			PlanSubcategoryName.switches.ToString()
		},
		{
			LogicPowerRelayConfig.ID,
			PlanSubcategoryName.switches.ToString()
		},
		{
			TemperatureControlledSwitchConfig.ID,
			PlanSubcategoryName.switches.ToString()
		},
		{
			PressureSwitchLiquidConfig.ID,
			PlanSubcategoryName.switches.ToString()
		},
		{
			PressureSwitchGasConfig.ID,
			PlanSubcategoryName.switches.ToString()
		},
		{
			"MicrobeMusher",
			PlanSubcategoryName.cooking.ToString()
		},
		{
			"CookingStation",
			PlanSubcategoryName.cooking.ToString()
		},
		{
			"Deepfryer",
			PlanSubcategoryName.cooking.ToString()
		},
		{
			"GourmetCookingStation",
			PlanSubcategoryName.cooking.ToString()
		},
		{
			"SpiceGrinder",
			PlanSubcategoryName.cooking.ToString()
		},
		{
			"FoodDehydrator",
			PlanSubcategoryName.cooking.ToString()
		},
		{
			"FoodRehydrator",
			PlanSubcategoryName.cooking.ToString()
		},
		{
			"Smoker",
			PlanSubcategoryName.cooking.ToString()
		},
		{
			"SushiBar",
			PlanSubcategoryName.cooking.ToString()
		},
		{
			"PlanterBox",
			PlanSubcategoryName.farming.ToString()
		},
		{
			"FarmTile",
			PlanSubcategoryName.farming.ToString()
		},
		{
			"HydroponicFarm",
			PlanSubcategoryName.farming.ToString()
		},
		{
			"WideFarmTile",
			PlanSubcategoryName.farming.ToString()
		},
		{
			"LargeBackwallFarm",
			PlanSubcategoryName.farming.ToString()
		},
		{
			"RationBox",
			PlanSubcategoryName.storage.ToString()
		},
		{
			"Refrigerator",
			PlanSubcategoryName.storage.ToString()
		},
		{
			"MiniFridge",
			PlanSubcategoryName.storage.ToString()
		},
		{
			"CreatureDeliveryPoint",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"UnderwaterMilkFeeder",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"CritterDropOff",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"CritterPickUp",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"FishDeliveryPoint",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"FishPickUp",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"CreatureFeeder",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"FishFeeder",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"MilkFeeder",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"EggIncubator",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"EggCracker",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"CreatureGroundTrap",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"CreatureAirTrap",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"WaterTrap",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"CritterCondo",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"UnderwaterCritterCondo",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"AirBorneCritterCondo",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"Outhouse",
			PlanSubcategoryName.washroom.ToString()
		},
		{
			"FlushToilet",
			PlanSubcategoryName.washroom.ToString()
		},
		{
			"WallToilet",
			PlanSubcategoryName.washroom.ToString()
		},
		{
			ShowerConfig.ID,
			PlanSubcategoryName.washroom.ToString()
		},
		{
			"GunkEmptier",
			PlanSubcategoryName.washroom.ToString()
		},
		{
			"LiquidConduit",
			PlanSubcategoryName.pipes.ToString()
		},
		{
			"InsulatedLiquidConduit",
			PlanSubcategoryName.pipes.ToString()
		},
		{
			"LiquidConduitRadiant",
			PlanSubcategoryName.pipes.ToString()
		},
		{
			"LiquidConduitBridge",
			PlanSubcategoryName.pipes.ToString()
		},
		{
			"ContactConductivePipeBridge",
			PlanSubcategoryName.pipes.ToString()
		},
		{
			"LiquidVent",
			PlanSubcategoryName.pipes.ToString()
		},
		{
			"LiquidPump",
			PlanSubcategoryName.pumps.ToString()
		},
		{
			"LiquidMiniPump",
			PlanSubcategoryName.pumps.ToString()
		},
		{
			"LiquidPumpingStation",
			PlanSubcategoryName.pumps.ToString()
		},
		{
			"DevPumpLiquid",
			PlanSubcategoryName.pumps.ToString()
		},
		{
			"BottleEmptier",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"LiquidFilter",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"LiquidConduitPreferentialFlow",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"LiquidConduitOverflow",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"LiquidValve",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"LiquidLogicValve",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"LiquidLimitValve",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"LiquidBottler",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"BottleEmptierConduitLiquid",
			PlanSubcategoryName.valves.ToString()
		},
		{
			LiquidConduitElementSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LiquidConduitDiseaseSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LiquidConduitTemperatureSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			"ModularLaunchpadPortLiquid",
			PlanSubcategoryName.buildmenuports.ToString()
		},
		{
			"ModularLaunchpadPortLiquidUnloader",
			PlanSubcategoryName.buildmenuports.ToString()
		},
		{
			"GasConduit",
			PlanSubcategoryName.pipes.ToString()
		},
		{
			"InsulatedGasConduit",
			PlanSubcategoryName.pipes.ToString()
		},
		{
			"GasConduitRadiant",
			PlanSubcategoryName.pipes.ToString()
		},
		{
			"GasConduitBridge",
			PlanSubcategoryName.pipes.ToString()
		},
		{
			"GasVent",
			PlanSubcategoryName.pipes.ToString()
		},
		{
			"GasVentHighPressure",
			PlanSubcategoryName.pipes.ToString()
		},
		{
			"GasPump",
			PlanSubcategoryName.pumps.ToString()
		},
		{
			"GasMiniPump",
			PlanSubcategoryName.pumps.ToString()
		},
		{
			"DevPumpGas",
			PlanSubcategoryName.pumps.ToString()
		},
		{
			"GasBottler",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"BottleEmptierGas",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"BottleEmptierConduitGas",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"GasFilter",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"GasConduitPreferentialFlow",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"GasConduitOverflow",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"GasValve",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"GasLogicValve",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"GasLimitValve",
			PlanSubcategoryName.valves.ToString()
		},
		{
			GasConduitElementSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			GasConduitDiseaseSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			GasConduitTemperatureSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			"ModularLaunchpadPortGas",
			PlanSubcategoryName.buildmenuports.ToString()
		},
		{
			"ModularLaunchpadPortGasUnloader",
			PlanSubcategoryName.buildmenuports.ToString()
		},
		{
			"Compost",
			PlanSubcategoryName.organic.ToString()
		},
		{
			"FertilizerMaker",
			PlanSubcategoryName.organic.ToString()
		},
		{
			"AlgaeDistillery",
			PlanSubcategoryName.organic.ToString()
		},
		{
			"EthanolDistillery",
			PlanSubcategoryName.organic.ToString()
		},
		{
			"SludgePress",
			PlanSubcategoryName.organic.ToString()
		},
		{
			"MilkFatSeparator",
			PlanSubcategoryName.organic.ToString()
		},
		{
			"MilkPress",
			PlanSubcategoryName.organic.ToString()
		},
		{
			"IceKettle",
			PlanSubcategoryName.materials.ToString()
		},
		{
			"WaterPurifier",
			PlanSubcategoryName.materials.ToString()
		},
		{
			"Desalinator",
			PlanSubcategoryName.materials.ToString()
		},
		{
			"RockCrusher",
			PlanSubcategoryName.materials.ToString()
		},
		{
			"Kiln",
			PlanSubcategoryName.materials.ToString()
		},
		{
			"FabricatedWoodMaker",
			PlanSubcategoryName.materials.ToString()
		},
		{
			"MetalRefinery",
			PlanSubcategoryName.materials.ToString()
		},
		{
			"GlassForge",
			PlanSubcategoryName.materials.ToString()
		},
		{
			"RubberMaker",
			PlanSubcategoryName.materials.ToString()
		},
		{
			"OilRefinery",
			PlanSubcategoryName.oil.ToString()
		},
		{
			"Polymerizer",
			PlanSubcategoryName.oil.ToString()
		},
		{
			"OxyliteRefinery",
			PlanSubcategoryName.advanced.ToString()
		},
		{
			"ChemicalRefinery",
			PlanSubcategoryName.advanced.ToString()
		},
		{
			"SupermaterialRefinery",
			PlanSubcategoryName.advanced.ToString()
		},
		{
			"DiamondPress",
			PlanSubcategoryName.advanced.ToString()
		},
		{
			"Chlorinator",
			PlanSubcategoryName.advanced.ToString()
		},
		{
			"WashBasin",
			PlanSubcategoryName.hygiene.ToString()
		},
		{
			"WashSink",
			PlanSubcategoryName.hygiene.ToString()
		},
		{
			"HandSanitizer",
			PlanSubcategoryName.hygiene.ToString()
		},
		{
			"DecontaminationShower",
			PlanSubcategoryName.hygiene.ToString()
		},
		{
			"Apothecary",
			PlanSubcategoryName.medical.ToString()
		},
		{
			"DoctorStation",
			PlanSubcategoryName.medical.ToString()
		},
		{
			"AdvancedDoctorStation",
			PlanSubcategoryName.medical.ToString()
		},
		{
			"MedicalCot",
			PlanSubcategoryName.medical.ToString()
		},
		{
			"DevLifeSupport",
			PlanSubcategoryName.medical.ToString()
		},
		{
			"MassageTable",
			PlanSubcategoryName.wellness.ToString()
		},
		{
			"Grave",
			PlanSubcategoryName.wellness.ToString()
		},
		{
			"OilChanger",
			PlanSubcategoryName.wellness.ToString()
		},
		{
			"Bed",
			PlanSubcategoryName.beds.ToString()
		},
		{
			"LuxuryBed",
			PlanSubcategoryName.beds.ToString()
		},
		{
			LadderBedConfig.ID,
			PlanSubcategoryName.beds.ToString()
		},
		{
			"FloorLamp",
			PlanSubcategoryName.lights.ToString()
		},
		{
			"CeilingLight",
			PlanSubcategoryName.lights.ToString()
		},
		{
			"GlassCeilingLight",
			PlanSubcategoryName.lights.ToString()
		},
		{
			"SunLamp",
			PlanSubcategoryName.lights.ToString()
		},
		{
			"DevLightGenerator",
			PlanSubcategoryName.lights.ToString()
		},
		{
			"MercuryCeilingLight",
			PlanSubcategoryName.lights.ToString()
		},
		{
			"DiningTable",
			PlanSubcategoryName.dining.ToString()
		},
		{
			"MultiMinionDiningTable",
			PlanSubcategoryName.dining.ToString()
		},
		{
			"WaterCooler",
			PlanSubcategoryName.recreation.ToString()
		},
		{
			"Phonobox",
			PlanSubcategoryName.recreation.ToString()
		},
		{
			"ArcadeMachine",
			PlanSubcategoryName.recreation.ToString()
		},
		{
			"EspressoMachine",
			PlanSubcategoryName.recreation.ToString()
		},
		{
			"HotTub",
			PlanSubcategoryName.recreation.ToString()
		},
		{
			"MechanicalSurfboard",
			PlanSubcategoryName.recreation.ToString()
		},
		{
			"Sauna",
			PlanSubcategoryName.recreation.ToString()
		},
		{
			"Juicer",
			PlanSubcategoryName.recreation.ToString()
		},
		{
			"SodaFountain",
			PlanSubcategoryName.recreation.ToString()
		},
		{
			"BeachChair",
			PlanSubcategoryName.recreation.ToString()
		},
		{
			"VerticalWindTunnel",
			PlanSubcategoryName.recreation.ToString()
		},
		{
			"Telephone",
			PlanSubcategoryName.recreation.ToString()
		},
		{
			"FlowerVase",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"FlowerVaseWall",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"FlowerVaseHanging",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"FlowerVaseHangingFancy",
			PlanSubcategoryName.decor.ToString()
		},
		{
			PixelPackConfig.ID,
			PlanSubcategoryName.decor.ToString()
		},
		{
			"SmallSculpture",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"Sculpture",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"IceSculpture",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"MarbleSculpture",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"MetalSculpture",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"WoodSculpture",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"FossilSculpture",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"CeilingFossilSculpture",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"CrownMoulding",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"CornerMoulding",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"Canvas",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"CanvasWide",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"CanvasTall",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"ItemPedestal",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"Shelf",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"ParkSign",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"MonumentBottom",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"MonumentMiddle",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"MonumentTop",
			PlanSubcategoryName.decor.ToString()
		},
		{
			"ResearchCenter",
			PlanSubcategoryName.research.ToString()
		},
		{
			"AdvancedResearchCenter",
			PlanSubcategoryName.research.ToString()
		},
		{
			"GeoTuner",
			PlanSubcategoryName.research.ToString()
		},
		{
			"NuclearResearchCenter",
			PlanSubcategoryName.research.ToString()
		},
		{
			"OrbitalResearchCenter",
			PlanSubcategoryName.research.ToString()
		},
		{
			"CosmicResearchCenter",
			PlanSubcategoryName.research.ToString()
		},
		{
			"DLC1CosmicResearchCenter",
			PlanSubcategoryName.research.ToString()
		},
		{
			"DataMiner",
			PlanSubcategoryName.research.ToString()
		},
		{
			"ArtifactAnalysisStation",
			PlanSubcategoryName.archaeology.ToString()
		},
		{
			"MissileFabricator",
			PlanSubcategoryName.meteordefense.ToString()
		},
		{
			"AstronautTrainingCenter",
			PlanSubcategoryName.exploration.ToString()
		},
		{
			"PowerControlStation",
			PlanSubcategoryName.industrialstation.ToString()
		},
		{
			"ResetSkillsStation",
			PlanSubcategoryName.industrialstation.ToString()
		},
		{
			"RoleStation",
			PlanSubcategoryName.workstations.ToString()
		},
		{
			"RanchStation",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"ShearingStation",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"MilkingStation",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"UnderwaterRanchStation",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"UnderwaterShearingStation",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"UnderwaterMilkingStation",
			PlanSubcategoryName.ranching.ToString()
		},
		{
			"FarmStation",
			PlanSubcategoryName.farming.ToString()
		},
		{
			"GeneticAnalysisStation",
			PlanSubcategoryName.farming.ToString()
		},
		{
			"CraftingTable",
			PlanSubcategoryName.manufacturing.ToString()
		},
		{
			"AdvancedCraftingTable",
			PlanSubcategoryName.manufacturing.ToString()
		},
		{
			"ClothingFabricator",
			PlanSubcategoryName.manufacturing.ToString()
		},
		{
			"ClothingAlterationStation",
			PlanSubcategoryName.manufacturing.ToString()
		},
		{
			"SuitFabricator",
			PlanSubcategoryName.manufacturing.ToString()
		},
		{
			"OxygenMaskMarker",
			PlanSubcategoryName.equipment.ToString()
		},
		{
			"OxygenMaskLocker",
			PlanSubcategoryName.equipment.ToString()
		},
		{
			"SuitMarker",
			PlanSubcategoryName.equipment.ToString()
		},
		{
			"SuitLocker",
			PlanSubcategoryName.equipment.ToString()
		},
		{
			"JetSuitMarker",
			PlanSubcategoryName.equipment.ToString()
		},
		{
			"JetSuitLocker",
			PlanSubcategoryName.equipment.ToString()
		},
		{
			"MissileLauncher",
			PlanSubcategoryName.missiles.ToString()
		},
		{
			"LeadSuitMarker",
			PlanSubcategoryName.equipment.ToString()
		},
		{
			"LeadSuitLocker",
			PlanSubcategoryName.equipment.ToString()
		},
		{
			"Campfire",
			PlanSubcategoryName.temperature.ToString()
		},
		{
			"DevHeater",
			PlanSubcategoryName.temperature.ToString()
		},
		{
			"SpaceHeater",
			PlanSubcategoryName.temperature.ToString()
		},
		{
			"LiquidHeater",
			PlanSubcategoryName.temperature.ToString()
		},
		{
			"LiquidConditioner",
			PlanSubcategoryName.temperature.ToString()
		},
		{
			"LiquidCooledFan",
			PlanSubcategoryName.temperature.ToString()
		},
		{
			"IceCooledFan",
			PlanSubcategoryName.temperature.ToString()
		},
		{
			"IceMachine",
			PlanSubcategoryName.temperature.ToString()
		},
		{
			"AirConditioner",
			PlanSubcategoryName.temperature.ToString()
		},
		{
			"ThermalBlock",
			PlanSubcategoryName.temperature.ToString()
		},
		{
			"OreScrubber",
			PlanSubcategoryName.sanitation.ToString()
		},
		{
			"OilWellCap",
			PlanSubcategoryName.oil.ToString()
		},
		{
			"SweepBotStation",
			PlanSubcategoryName.sanitation.ToString()
		},
		{
			"LogicWire",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"LogicWireBridge",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"LogicRibbon",
			PlanSubcategoryName.wires.ToString()
		},
		{
			"LogicRibbonBridge",
			PlanSubcategoryName.wires.ToString()
		},
		{
			LogicRibbonReaderConfig.ID,
			PlanSubcategoryName.wires.ToString()
		},
		{
			LogicRibbonWriterConfig.ID,
			PlanSubcategoryName.wires.ToString()
		},
		{
			"LogicDuplicantSensor",
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicPressureSensorGasConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicPressureSensorLiquidConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicTemperatureSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicLightSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicWattageSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicTimeOfDaySensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicTimerSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicDiseaseSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicElementSensorGasConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicElementSensorLiquidConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicCritterCountSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicRadiationSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicHEPSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			CometDetectorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			LogicCounterConfig.ID,
			PlanSubcategoryName.logicmanager.ToString()
		},
		{
			"Checkpoint",
			PlanSubcategoryName.logicmanager.ToString()
		},
		{
			LogicAlarmConfig.ID,
			PlanSubcategoryName.logicmanager.ToString()
		},
		{
			LogicHammerConfig.ID,
			PlanSubcategoryName.logicaudio.ToString()
		},
		{
			LogicSwitchConfig.ID,
			PlanSubcategoryName.switches.ToString()
		},
		{
			"FloorSwitch",
			PlanSubcategoryName.switches.ToString()
		},
		{
			"LogicGateNOT",
			PlanSubcategoryName.logicgates.ToString()
		},
		{
			"LogicGateAND",
			PlanSubcategoryName.logicgates.ToString()
		},
		{
			"LogicGateOR",
			PlanSubcategoryName.logicgates.ToString()
		},
		{
			"LogicGateBUFFER",
			PlanSubcategoryName.logicgates.ToString()
		},
		{
			"LogicGateFILTER",
			PlanSubcategoryName.logicgates.ToString()
		},
		{
			"LogicGateXOR",
			PlanSubcategoryName.logicgates.ToString()
		},
		{
			LogicMemoryConfig.ID,
			PlanSubcategoryName.logicgates.ToString()
		},
		{
			"LogicGateMultiplexer",
			PlanSubcategoryName.logicgates.ToString()
		},
		{
			"LogicGateDemultiplexer",
			PlanSubcategoryName.logicgates.ToString()
		},
		{
			"LogicInterasteroidSender",
			PlanSubcategoryName.transmissions.ToString()
		},
		{
			"LogicInterasteroidReceiver",
			PlanSubcategoryName.transmissions.ToString()
		},
		{
			"SolidConduit",
			PlanSubcategoryName.conveyancestructures.ToString()
		},
		{
			"UnderwaterVentDrill",
			PlanSubcategoryName.conveyancestructures.ToString()
		},
		{
			"SolidConduitBridge",
			PlanSubcategoryName.conveyancestructures.ToString()
		},
		{
			"SolidConduitInbox",
			PlanSubcategoryName.conveyancestructures.ToString()
		},
		{
			"SolidConduitOutbox",
			PlanSubcategoryName.conveyancestructures.ToString()
		},
		{
			"SolidFilter",
			PlanSubcategoryName.conveyancestructures.ToString()
		},
		{
			"SolidVent",
			PlanSubcategoryName.conveyancestructures.ToString()
		},
		{
			"DevPumpSolid",
			PlanSubcategoryName.pumps.ToString()
		},
		{
			"SolidLogicValve",
			PlanSubcategoryName.valves.ToString()
		},
		{
			"SolidLimitValve",
			PlanSubcategoryName.valves.ToString()
		},
		{
			SolidConduitDiseaseSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			SolidConduitElementSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			SolidConduitTemperatureSensorConfig.ID,
			PlanSubcategoryName.sensors.ToString()
		},
		{
			"AutoMiner",
			PlanSubcategoryName.automated.ToString()
		},
		{
			"SolidTransferArm",
			PlanSubcategoryName.automated.ToString()
		},
		{
			"ModularLaunchpadPortSolid",
			PlanSubcategoryName.buildmenuports.ToString()
		},
		{
			"ModularLaunchpadPortSolidUnloader",
			PlanSubcategoryName.buildmenuports.ToString()
		},
		{
			"Telescope",
			PlanSubcategoryName.telescopes.ToString()
		},
		{
			"ClusterTelescope",
			PlanSubcategoryName.telescopes.ToString()
		},
		{
			"ClusterTelescopeEnclosed",
			PlanSubcategoryName.telescopes.ToString()
		},
		{
			"LaunchPad",
			PlanSubcategoryName.rocketstructures.ToString()
		},
		{
			"Gantry",
			PlanSubcategoryName.rocketstructures.ToString()
		},
		{
			"ModularLaunchpadPortBridge",
			PlanSubcategoryName.rocketstructures.ToString()
		},
		{
			"RailGun",
			PlanSubcategoryName.fittings.ToString()
		},
		{
			"RailGunPayloadOpener",
			PlanSubcategoryName.fittings.ToString()
		},
		{
			"LandingBeacon",
			PlanSubcategoryName.rocketnav.ToString()
		},
		{
			"SteamEngine",
			PlanSubcategoryName.engines.ToString()
		},
		{
			"KeroseneEngine",
			PlanSubcategoryName.engines.ToString()
		},
		{
			"BiodieselEngine",
			PlanSubcategoryName.engines.ToString()
		},
		{
			"HydrogenEngine",
			PlanSubcategoryName.engines.ToString()
		},
		{
			"SolidBooster",
			PlanSubcategoryName.engines.ToString()
		},
		{
			"LiquidFuelTank",
			PlanSubcategoryName.tanks.ToString()
		},
		{
			"OxidizerTank",
			PlanSubcategoryName.tanks.ToString()
		},
		{
			"OxidizerTankLiquid",
			PlanSubcategoryName.tanks.ToString()
		},
		{
			"CargoBay",
			PlanSubcategoryName.cargo.ToString()
		},
		{
			"GasCargoBay",
			PlanSubcategoryName.cargo.ToString()
		},
		{
			"LiquidCargoBay",
			PlanSubcategoryName.cargo.ToString()
		},
		{
			"SpecialCargoBay",
			PlanSubcategoryName.cargo.ToString()
		},
		{
			"CommandModule",
			PlanSubcategoryName.rocketnav.ToString()
		},
		{
			RocketControlStationConfig.ID,
			PlanSubcategoryName.rocketnav.ToString()
		},
		{
			LogicClusterLocationSensorConfig.ID,
			PlanSubcategoryName.rocketnav.ToString()
		},
		{
			"MissionControl",
			PlanSubcategoryName.rocketnav.ToString()
		},
		{
			"MissionControlCluster",
			PlanSubcategoryName.rocketnav.ToString()
		},
		{
			"RoboPilotCommandModule",
			PlanSubcategoryName.rocketnav.ToString()
		},
		{
			"TouristModule",
			PlanSubcategoryName.module.ToString()
		},
		{
			"ResearchModule",
			PlanSubcategoryName.module.ToString()
		},
		{
			"RocketInteriorPowerPlug",
			PlanSubcategoryName.fittings.ToString()
		},
		{
			"RocketInteriorLiquidInput",
			PlanSubcategoryName.fittings.ToString()
		},
		{
			"RocketInteriorLiquidOutput",
			PlanSubcategoryName.fittings.ToString()
		},
		{
			"RocketInteriorGasInput",
			PlanSubcategoryName.fittings.ToString()
		},
		{
			"RocketInteriorGasOutput",
			PlanSubcategoryName.fittings.ToString()
		},
		{
			"RocketInteriorSolidInput",
			PlanSubcategoryName.fittings.ToString()
		},
		{
			"RocketInteriorSolidOutput",
			PlanSubcategoryName.fittings.ToString()
		},
		{
			"ManualHighEnergyParticleSpawner",
			PlanSubcategoryName.producers.ToString()
		},
		{
			"HighEnergyParticleSpawner",
			PlanSubcategoryName.producers.ToString()
		},
		{
			"DevHEPSpawner",
			PlanSubcategoryName.producers.ToString()
		},
		{
			"HighEnergyParticleRedirector",
			PlanSubcategoryName.transmissions.ToString()
		},
		{
			"HEPBattery",
			PlanSubcategoryName.batteries.ToString()
		},
		{
			"HEPBridgeTile",
			PlanSubcategoryName.transmissions.ToString()
		},
		{
			"NuclearReactor",
			PlanSubcategoryName.producers.ToString()
		},
		{
			"UraniumCentrifuge",
			PlanSubcategoryName.producers.ToString()
		},
		{
			"RadiationLight",
			PlanSubcategoryName.producers.ToString()
		},
		{
			"DevRadiationGenerator",
			PlanSubcategoryName.producers.ToString()
		}
	};

	public static List<PlanScreen.PlanInfo> PLANORDER = new List<PlanScreen.PlanInfo>
	{
		new PlanScreen.PlanInfo(new HashedString("Base"), hideIfNotResearched: false, new List<string>
		{
			"Ladder", "FirePole", "LadderFast", "Tile", "SnowTile", "WoodTile", "GasPermeableMembrane", "MeshTile", "RubberTile", "InsulationTile",
			"PlasticTile", "MetalTile", "GlassTile", "StorageTile", "BunkerTile", "CarpetTile", "ExteriorWall", "GlassExteriorWall", "ExobaseHeadquarters", "Door",
			"WoodenDoor", "ManualPressureDoor", "InsulatedDoor", "PressureDoor", "BunkerDoor", "StorageLocker", "StorageLockerSmart", "LiquidReservoir", "GasReservoir", "ObjectDispenser",
			"TravelTube", "TravelTubeEntrance", "TravelTubeWallBridge"
		}),
		new PlanScreen.PlanInfo(new HashedString("Oxygen"), hideIfNotResearched: false, new List<string> { "MineralDeoxidizer", "SublimationStation", "Oxysconce", "AlgaeHabitat", "AirFilter", "CO2Scrubber", "Electrolyzer", "RustDeoxidizer", "UnderwaterBreathingStation" }),
		new PlanScreen.PlanInfo(new HashedString("Power"), hideIfNotResearched: false, new List<string>
		{
			"DevGenerator",
			"ManualGenerator",
			"Generator",
			"WoodGasGenerator",
			"PeatGenerator",
			"ReefGenerator",
			"HydrogenGenerator",
			"MethaneGenerator",
			"PetroleumGenerator",
			"SteamTurbine",
			"SteamTurbine2",
			"SolarPanel",
			"Wire",
			"WireBridge",
			"HighWattageWire",
			"WireBridgeHighWattage",
			"WireRefined",
			"WireRefinedBridge",
			"WireRefinedHighWattage",
			"WireRefinedBridgeHighWattage",
			"WireRubber",
			"WireRubberBridge",
			"Battery",
			"BatteryMedium",
			"BatterySmart",
			"ElectrobankCharger",
			"SmallElectrobankDischarger",
			"LargeElectrobankDischarger",
			"PowerTransformerSmall",
			"PowerTransformer",
			SwitchConfig.ID,
			LogicPowerRelayConfig.ID,
			TemperatureControlledSwitchConfig.ID,
			PressureSwitchLiquidConfig.ID,
			PressureSwitchGasConfig.ID
		}),
		new PlanScreen.PlanInfo(new HashedString("Food"), hideIfNotResearched: false, new List<string>
		{
			"UnderwaterMilkFeeder", "MicrobeMusher", "CookingStation", "Deepfryer", "GourmetCookingStation", "SpiceGrinder", "FoodDehydrator", "FoodRehydrator", "Smoker", "SushiBar",
			"PlanterBox", "FarmTile", "HydroponicFarm", "WideFarmTile", "LargeBackwallFarm", "RationBox", "Refrigerator", "MiniFridge", "CreatureDeliveryPoint", "CritterPickUp",
			"CritterDropOff", "FishPickUp", "FishDeliveryPoint", "CreatureFeeder", "FishFeeder", "MilkFeeder", "EggIncubator", "EggCracker", "CreatureGroundTrap", "WaterTrap",
			"CreatureAirTrap", "CritterCondo", "UnderwaterCritterCondo", "AirBorneCritterCondo"
		}),
		new PlanScreen.PlanInfo(new HashedString("Plumbing"), hideIfNotResearched: false, new List<string>
		{
			"DevPumpLiquid",
			"Outhouse",
			"FlushToilet",
			"WallToilet",
			ShowerConfig.ID,
			"GunkEmptier",
			"LiquidPumpingStation",
			"BottleEmptier",
			"BottleEmptierConduitLiquid",
			"LiquidBottler",
			"LiquidConduit",
			"InsulatedLiquidConduit",
			"LiquidConduitRadiant",
			"LiquidConduitBridge",
			"LiquidConduitPreferentialFlow",
			"LiquidConduitOverflow",
			"LiquidPump",
			"LiquidMiniPump",
			"LiquidVent",
			"LiquidFilter",
			"LiquidValve",
			"LiquidLogicValve",
			"LiquidLimitValve",
			LiquidConduitElementSensorConfig.ID,
			LiquidConduitDiseaseSensorConfig.ID,
			LiquidConduitTemperatureSensorConfig.ID,
			"ModularLaunchpadPortLiquid",
			"ModularLaunchpadPortLiquidUnloader",
			"ContactConductivePipeBridge"
		}),
		new PlanScreen.PlanInfo(new HashedString("HVAC"), hideIfNotResearched: false, new List<string>
		{
			"DevPumpGas",
			"GasConduit",
			"InsulatedGasConduit",
			"GasConduitRadiant",
			"GasConduitBridge",
			"GasConduitPreferentialFlow",
			"GasConduitOverflow",
			"GasPump",
			"GasMiniPump",
			"GasVent",
			"GasVentHighPressure",
			"GasFilter",
			"GasValve",
			"GasLogicValve",
			"GasLimitValve",
			"GasBottler",
			"BottleEmptierGas",
			"BottleEmptierConduitGas",
			"ModularLaunchpadPortGas",
			"ModularLaunchpadPortGasUnloader",
			GasConduitElementSensorConfig.ID,
			GasConduitDiseaseSensorConfig.ID,
			GasConduitTemperatureSensorConfig.ID
		}),
		new PlanScreen.PlanInfo(new HashedString("Refining"), hideIfNotResearched: false, new List<string>
		{
			"FabricatedWoodMaker", "Compost", "WaterPurifier", "Desalinator", "FertilizerMaker", "AlgaeDistillery", "EthanolDistillery", "RockCrusher", "Kiln", "SludgePress",
			"MetalRefinery", "GlassForge", "OilRefinery", "Polymerizer", "RubberMaker", "OxyliteRefinery", "Chlorinator", "ChemicalRefinery", "SupermaterialRefinery", "DiamondPress",
			"MilkFatSeparator", "MilkPress"
		}),
		new PlanScreen.PlanInfo(new HashedString(PlanSubcategoryName.medical.ToString()), hideIfNotResearched: false, new List<string>
		{
			"DevLifeSupport", "WashBasin", "WashSink", "HandSanitizer", "DecontaminationShower", "OilChanger", "Apothecary", "DoctorStation", "AdvancedDoctorStation", "MedicalCot",
			"MassageTable", "Grave"
		}),
		new PlanScreen.PlanInfo(new HashedString("Furniture"), hideIfNotResearched: false, new List<string>
		{
			"Shelf",
			"Bed",
			"LuxuryBed",
			LadderBedConfig.ID,
			"FloorLamp",
			"CeilingLight",
			"GlassCeilingLight",
			"SunLamp",
			"DevLightGenerator",
			"MercuryCeilingLight",
			"DiningTable",
			"MultiMinionDiningTable",
			"WaterCooler",
			"Phonobox",
			"ArcadeMachine",
			"EspressoMachine",
			"HotTub",
			"MechanicalSurfboard",
			"Sauna",
			"Juicer",
			"SodaFountain",
			"BeachChair",
			"VerticalWindTunnel",
			PixelPackConfig.ID,
			"Telephone",
			"FlowerVase",
			"FlowerVaseWall",
			"FlowerVaseHanging",
			"FlowerVaseHangingFancy",
			"SmallSculpture",
			"Sculpture",
			"IceSculpture",
			"WoodSculpture",
			"MarbleSculpture",
			"MetalSculpture",
			"FossilSculpture",
			"CeilingFossilSculpture",
			"CrownMoulding",
			"CornerMoulding",
			"Canvas",
			"CanvasWide",
			"CanvasTall",
			"ItemPedestal",
			"MonumentBottom",
			"MonumentMiddle",
			"MonumentTop",
			"ParkSign"
		}),
		new PlanScreen.PlanInfo(new HashedString("Equipment"), hideIfNotResearched: false, new List<string>
		{
			"ResearchCenter",
			"AdvancedResearchCenter",
			"NuclearResearchCenter",
			"OrbitalResearchCenter",
			"CosmicResearchCenter",
			"DLC1CosmicResearchCenter",
			"Telescope",
			"GeoTuner",
			"DataMiner",
			"PowerControlStation",
			"FarmStation",
			"GeneticAnalysisStation",
			"RanchStation",
			"ShearingStation",
			"MilkingStation",
			"UnderwaterRanchStation",
			"UnderwaterShearingStation",
			"UnderwaterMilkingStation",
			"RoleStation",
			"ResetSkillsStation",
			"ArtifactAnalysisStation",
			RemoteWorkerDockConfig.ID,
			RemoteWorkTerminalConfig.ID,
			"MissileFabricator",
			"CraftingTable",
			"AdvancedCraftingTable",
			"ClothingFabricator",
			"ClothingAlterationStation",
			"SuitFabricator",
			"OxygenMaskMarker",
			"OxygenMaskLocker",
			"SuitMarker",
			"SuitLocker",
			"JetSuitMarker",
			"JetSuitLocker",
			"LeadSuitMarker",
			"LeadSuitLocker",
			"AstronautTrainingCenter"
		}),
		new PlanScreen.PlanInfo(new HashedString("Utilities"), hideIfNotResearched: true, new List<string>
		{
			"UnderwaterVentDrill", "Campfire", "DevHeater", "IceKettle", "SpaceHeater", "LiquidHeater", "LiquidCooledFan", "IceCooledFan", "IceMachine", "AirConditioner",
			"LiquidConditioner", "OreScrubber", "OilWellCap", "ThermalBlock", "SweepBotStation"
		}),
		new PlanScreen.PlanInfo(new HashedString("Automation"), hideIfNotResearched: true, new List<string>
		{
			"LogicWire",
			"LogicWireBridge",
			"LogicRibbon",
			"LogicRibbonBridge",
			LogicSwitchConfig.ID,
			"LogicDuplicantSensor",
			LogicPressureSensorGasConfig.ID,
			LogicPressureSensorLiquidConfig.ID,
			LogicTemperatureSensorConfig.ID,
			LogicLightSensorConfig.ID,
			LogicWattageSensorConfig.ID,
			LogicTimeOfDaySensorConfig.ID,
			LogicTimerSensorConfig.ID,
			LogicDiseaseSensorConfig.ID,
			LogicElementSensorGasConfig.ID,
			LogicElementSensorLiquidConfig.ID,
			LogicCritterCountSensorConfig.ID,
			LogicRadiationSensorConfig.ID,
			LogicHEPSensorConfig.ID,
			LogicCounterConfig.ID,
			LogicAlarmConfig.ID,
			LogicHammerConfig.ID,
			"LogicInterasteroidSender",
			"LogicInterasteroidReceiver",
			LogicRibbonReaderConfig.ID,
			LogicRibbonWriterConfig.ID,
			"FloorSwitch",
			"Checkpoint",
			CometDetectorConfig.ID,
			"LogicGateNOT",
			"LogicGateAND",
			"LogicGateOR",
			"LogicGateBUFFER",
			"LogicGateFILTER",
			"LogicGateXOR",
			LogicMemoryConfig.ID,
			"LogicGateMultiplexer",
			"LogicGateDemultiplexer"
		}),
		new PlanScreen.PlanInfo(new HashedString("Conveyance"), hideIfNotResearched: true, new List<string>
		{
			"DevPumpSolid",
			"SolidTransferArm",
			"SolidConduit",
			"SolidConduitBridge",
			"SolidConduitInbox",
			"SolidConduitOutbox",
			"SolidFilter",
			"SolidVent",
			"SolidLogicValve",
			"SolidLimitValve",
			SolidConduitDiseaseSensorConfig.ID,
			SolidConduitElementSensorConfig.ID,
			SolidConduitTemperatureSensorConfig.ID,
			"AutoMiner",
			"ModularLaunchpadPortSolid",
			"ModularLaunchpadPortSolidUnloader"
		}),
		new PlanScreen.PlanInfo(new HashedString("Rocketry"), hideIfNotResearched: true, new List<string>
		{
			"ClusterTelescope",
			"ClusterTelescopeEnclosed",
			"MissionControl",
			"MissionControlCluster",
			"LaunchPad",
			"Gantry",
			"SteamEngine",
			"KeroseneEngine",
			"BiodieselEngine",
			"SolidBooster",
			"LiquidFuelTank",
			"OxidizerTank",
			"OxidizerTankLiquid",
			"CargoBay",
			"GasCargoBay",
			"LiquidCargoBay",
			"CommandModule",
			"RoboPilotCommandModule",
			"TouristModule",
			"ResearchModule",
			"SpecialCargoBay",
			"HydrogenEngine",
			RocketControlStationConfig.ID,
			"RocketInteriorPowerPlug",
			"RocketInteriorLiquidInput",
			"RocketInteriorLiquidOutput",
			"RocketInteriorGasInput",
			"RocketInteriorGasOutput",
			"RocketInteriorSolidInput",
			"RocketInteriorSolidOutput",
			LogicClusterLocationSensorConfig.ID,
			"RailGun",
			"RailGunPayloadOpener",
			"LandingBeacon",
			"MissileLauncher",
			"ModularLaunchpadPortBridge"
		}),
		new PlanScreen.PlanInfo(new HashedString("HEP"), hideIfNotResearched: true, new List<string> { "RadiationLight", "ManualHighEnergyParticleSpawner", "NuclearReactor", "UraniumCentrifuge", "HighEnergyParticleSpawner", "DevHEPSpawner", "HighEnergyParticleRedirector", "HEPBattery", "HEPBridgeTile", "DevRadiationGenerator" }, DlcManager.EXPANSION1)
	};

	public static List<Type> COMPONENT_DESCRIPTION_ORDER = new List<Type>
	{
		typeof(BottleEmptier),
		typeof(CookingStation),
		typeof(GourmetCookingStation),
		typeof(RoleStation),
		typeof(ResearchCenter),
		typeof(NuclearResearchCenter),
		typeof(LiquidCooledFan),
		typeof(HandSanitizer),
		typeof(HandSanitizer.Work),
		typeof(PlantAirConditioner),
		typeof(Clinic),
		typeof(BuildingElementEmitter),
		typeof(ElementConverter),
		typeof(ElementConsumer),
		typeof(PassiveElementConsumer),
		typeof(TinkerStation),
		typeof(EnergyConsumer),
		typeof(AirConditioner),
		typeof(Storage),
		typeof(Battery),
		typeof(AirFilter),
		typeof(FlushToilet),
		typeof(Toilet),
		typeof(EnergyGenerator),
		typeof(MassageTable),
		typeof(Shower),
		typeof(Ownable),
		typeof(PlantablePlot),
		typeof(RelaxationPoint),
		typeof(BuildingComplete),
		typeof(Building),
		typeof(BuildingPreview),
		typeof(BuildingUnderConstruction),
		typeof(Crop),
		typeof(Growing),
		typeof(Equippable),
		typeof(ColdBreather),
		typeof(ResearchPointObject),
		typeof(SuitTank),
		typeof(IlluminationVulnerable),
		typeof(TemperatureVulnerable),
		typeof(ExternalTemperatureMonitor),
		typeof(CritterTemperatureMonitor),
		typeof(PressureVulnerable),
		typeof(SubmersionMonitor),
		typeof(BatterySmart),
		typeof(Compost),
		typeof(Refrigerator),
		typeof(Bed),
		typeof(OreScrubber),
		typeof(OreScrubber.Work),
		typeof(MinimumOperatingTemperature),
		typeof(RoomTracker),
		typeof(EnergyConsumerSelfSustaining),
		typeof(ArcadeMachine),
		typeof(Telescope),
		typeof(EspressoMachine),
		typeof(JetSuitTank),
		typeof(Phonobox),
		typeof(ArcadeMachine),
		typeof(BeachChair),
		typeof(Sauna),
		typeof(VerticalWindTunnel),
		typeof(HotTub),
		typeof(Juicer),
		typeof(SodaFountain),
		typeof(MechanicalSurfboard),
		typeof(BottleEmptier),
		typeof(AccessControl),
		typeof(GammaRayOven),
		typeof(Reactor),
		typeof(HighEnergyParticlePort),
		typeof(LeadSuitTank),
		typeof(ActiveParticleConsumer.Def),
		typeof(WaterCooler),
		typeof(Edible),
		typeof(PlantableSeed),
		typeof(SicknessTrigger),
		typeof(MedicinalPill),
		typeof(SeedProducer),
		typeof(Geyser),
		typeof(SpaceHeater),
		typeof(Overheatable),
		typeof(CreatureCalorieMonitor.Def),
		typeof(LureableMonitor.Def),
		typeof(FertilizationMonitor.Def),
		typeof(IrrigationMonitor.Def),
		typeof(ScaleGrowthMonitor.Def),
		typeof(TravelTubeEntrance.Work),
		typeof(ToiletWorkableUse),
		typeof(ReceptacleMonitor),
		typeof(Light2D),
		typeof(Ladder),
		typeof(SimCellOccupier),
		typeof(Vent),
		typeof(LogicPorts),
		typeof(Capturable),
		typeof(Trappable),
		typeof(SpaceArtifact),
		typeof(MessStation),
		typeof(PlantElementEmitter),
		typeof(Radiator),
		typeof(DecorProvider)
	};
}
