using UnityEngine;

namespace FUtility;

public class CONSTS
{
	public static class UI_SOUNDS_EVENTS
	{
		public const string CLICK_OPEN = "event:/UI/Mouse/HUD_Click_Open";

		public const string CLICK = "event:/UI/Mouse/HUD_Click";

		public const string MOUSE_OVER = "event:/UI/Mouse/HUD_Mouseover";

		public const string SLIDER_START = "event:/UI/Mouse/Slider/Slider_Start";

		public const string SLIDER_MOVE = "event:/UI/Mouse/Slider/Slider_Move";

		public const string SLIDER_END = "event:/UI/Mouse/Slider/Slider_End";

		public const string SLIDER_BOUNDARY_LOW = "event:/UI/Mouse/Slider/Slider_Boundary_Low";

		public const string SLIDER_BOUNDARY_HIGH = "event:/UI/Mouse/Slider/Slider_Boundary_High";
	}

	public class BATCH_TAGS
	{
		public const int SWAPS = -77805842;

		public const int INTERACTS = -1371425853;
	}

	public static class PERSONALITY_TYPE
	{
		public const string GRUMPY = "Grumpy";

		public const string COOL = "Cool";

		public const string DOOFY = "Doofy";

		public const string SWEET = "Sweet";
	}

	public class COLORS
	{
		public static Color KLEI_PINK = Color32.op_Implicit(new Color32((byte)127, (byte)61, (byte)94, byte.MaxValue));

		public static Color KLEI_BLUE = Color32.op_Implicit(new Color32((byte)62, (byte)67, (byte)87, byte.MaxValue));
	}

	public static class AUDIO_CATEGORY
	{
		public const string METAL = "Metal";

		public const string GLASS = "Glass";

		public const string HOLLOWMETAL = "HollowMetal";

		public const string PLASTIC = "Plastic";

		public const string SOLIDMETAL = "SolidMetal";
	}

	public static class NAV_GRID
	{
		public const string WALKER_BABY = "WalkerBabyNavGrid";

		public const string WALKER_1X1 = "WalkerNavGrid1x1";

		public const string WALKER_1X2 = "WalkerNavGrid1x2";

		public const string MINION = "MinionNavGrid";

		public const string ROBOT = "RobotNavGrid";

		public const string DIGGER = "DiggerNavGrid";

		public const string DRECKO = "DreckoNavGrid";

		public const string DRECKO_BABY = "DreckoBabyNavGrid";

		public const string FLYER_1X1 = "FlyerNavGrid1x1";

		public const string FLYER_1X2 = "FlyerNavGrid1x2";

		public const string FLYER_2X2 = "FlyerNavGrid2x2";

		public const string SLICKSTER = "FloaterNavGrid";

		public const string SWIMMER = "SwimmerNavGrid";

		public const string PIP = "SquirrelNavGrid";
	}

	public static class SUB_BUILD_CATEGORY
	{
		public static class Base
		{
			public const string LADDERS = "ladders";

			public const string TILES = "tiles";

			public const string PRINTING_PODS = "printing pods";

			public const string DOORS = "doors";

			public const string STORAGE = "storage";

			public const string TUBES = "tubes";
		}

		public static class Oxygen
		{
			public const string PRODUCERS = "producers";

			public const string SCRUBBERS = "scrubbers";
		}

		public static class Power
		{
			public const string GENERATORS = "generators";

			public const string WIRES = "wires";

			public const string BATTERIES = "batteries";

			public const string TRANSFORMERS = "transformers";

			public const string SWITCHES = "switches";
		}

		public static class Food
		{
			public const string COOKING = "cooking";

			public const string FARMING = "farming";

			public const string STORAGE = "storage";

			public const string RANCHING = "ranching";
		}

		public static class Plumbing
		{
			public const string BATHROOM = "bathroom";

			public const string PIPES = "pipes";

			public const string PUMPS = "pumps";

			public const string VALVES = "valves";

			public const string SENSORS = "sensors";

			public const string LAUNCH_PAD = "launch pad";
		}

		public static class HVAC
		{
			public const string PIPES = "pipes";

			public const string PUMPS = "pumps";

			public const string VALVES = "valves";

			public const string SENSORS = "sensors";

			public const string LAUNCH_PAD = "launch pad";
		}

		public static class Refining
		{
			public const string MATERIALS = "materials";

			public const string OIL = "oil";

			public const string ADVANCED = "advanced";
		}

		public static class Medical
		{
			public const string CLEANING = "cleaning";

			public const string DEFCLEANING = "defcleaning";

			public const string HOSPITAL = "hospital";

			public const string WELLNESS = "wellness";
		}

		public static class Furniture
		{
			public const string BEDS = "beds";

			public const string LIGHTS = "lights";

			public const string DINING = "dining";

			public const string RECREATION = "recreation";

			public const string DEFARECREATIONULT = "defarecreationult";

			public const string POTS = "pots";

			public const string ELECTRONIC_DECOR = "electronic decor";

			public const string DECOR = "decor";

			public const string MOULDING = "moulding";

			public const string CANVAS = "canvas";

			public const string DISPALY = "dispaly";

			public const string SIGNS = "signs";

			public const string MONUMENT = "monument";
		}

		public static class Equipment
		{
			public const string RESEARCH = "research";

			public const string EXPLORATION = "exploration";

			public const string WORK_STATIONS = "work stations";

			public const string SUITS_GENERAL = "suits general";

			public const string OXYGEN_MASKS = "oxygen masks";

			public const string ATMO_SUITS = "atmo suits";

			public const string JET_SUITS = "jet suits";

			public const string LEAD_SUITS = "lead suits";
		}

		public static class Utilities
		{
			public const string TEMPERATURE = "temperature";

			public const string OTHER_UTILITIES = "other utilities";

			public const string SPECIAL = "special";
		}

		public static class Automation
		{
			public const string WIRES = "wires";

			public const string SENSORS = "sensors";

			public const string SWITCHES = "switches";

			public const string DEFAULT = "default";

			public const string LOGIC_GATES = "logic gates";

			public const string UTILITIES = "utilities";
		}

		public static class Conveyance
		{
			public const string CONDUIT = "conduit";

			public const string VALVES = "valves";

			public const string UTILITIES = "utilities";

			public const string LAUNCH_PAD = "launch pad";
		}

		public static class Rocketry
		{
			public const string TELESCOPES = "telescopes";

			public const string LAUNCH_PAD = "launch pad";

			public const string RAILGUNS = "railguns";

			public const string ENGINES = "engines";

			public const string FUEL_AND_OXIDIZER = "fuel and oxidizer";

			public const string CARGO = "cargo";

			public const string UTILITY = "utility";

			public const string COMMAND = "command";

			public const string FITTINGS = "fittings";
		}

		public static class HEP_CATEGORY
		{
			public const string HEP = "HEP";

			public const string URANIUM = "uranium";

			public const string RADIATION = "radiation";
		}

		public const string UNCATEGORIZED = "uncategorized";
	}

	public static class BUILD_CATEGORY
	{
		public const string BASE = "Base";

		public const string OXYGEN = "Oxygen";

		public const string POWER = "Power";

		public const string FOOD = "Food";

		public const string PLUMBING = "Plumbing";

		public const string HVAC = "HVAC";

		public const string REFINING = "Refining";

		public const string MEDICAL = "Medical";

		public const string FURNITURE = "Furniture";

		public const string EQUIPMENT = "Equipment";

		public const string UTILITIES = "Utilities";

		public const string AUTOMATION = "Automation";

		public const string CONVEYANCE = "Conveyance";

		public const string ROCKETRY = "Rocketry";

		public const string HEP = "HEP";
	}

	public static class TECH
	{
		public static class FOOD
		{
			public const string FARMING_TECH = "FarmingTech";

			public const string FINE_DINING = "FineDining";

			public const string FINER_DINING = "FinerDining";

			public const string FOOD_REPURPOSING = "FoodRepurposing";

			public const string AGRICULTURE = "Agriculture";

			public const string RANCHING = "Ranching";

			public const string ANIMAL_CONTROL = "AnimalControl";

			public const string BIOENGINEERING = "Bioengineering";
		}

		public static class POWER
		{
			public const string POWER_REGULATION = "PowerRegulation";

			public const string COMBUSTION = "Combustion";

			public const string IMPROVED_COMBUSTION = "ImprovedCombustion";

			public const string ACOUSTICS = "Acoustics";

			public const string ADVANCED_POWER_REGULATION = "AdvancedPowerRegulation";

			public const string PLASTICS = "Plastics";

			public const string PRETTY_GOOD_CONDUCTORS = "PrettyGoodConductors";

			public const string VALVE_MINIATURIZATION = "ValveMiniaturization";

			public const string RENEWABLE_ENERGY = "RenewableEnergy";

			public const string SPACE_POWER = "SpacePower";

			public const string HYDRO_CARBON_PROPULSION = "HydrocarbonPropulsion";

			public const string BETTER_HYDRO_CARBON_PROPULSION = "BetterHydroCarbonPropulsion";

			public const string SPACE_COMBUSTION = "SpaceCombustion";
		}

		public static class SOLIDS
		{
			public const string BASIC_REFINEMENT = "BasicRefinement";

			public const string REFINED_OBJECTS = "RefinedObjects";

			public const string SMART_STORAGE = "SmartStorage";

			public const string SMELTING = "Smelting";

			public const string SOLID_TRANSPORT = "SolidTransport";

			public const string HIGH_TEMP_FORGING = "HighTempForging";

			public const string HIGH_PRESSURE_FORGING = "HighPressureForging";

			public const string SOLID_SPACE = "SolidSpace";

			public const string SOLID_MANAGEMENT = "SolidManagement";

			public const string HIGH_VELOCITY_TRANSPORT = "HighVelocityTransport";

			public const string HIGH_VELOCITY_DESTRUCTION = "HighVelocityDestruction";
		}

		public static class COLONY_DEVELOPMENT
		{
			public const string JOBS = "Jobs";

			public const string ADVANCED_RESEARCH = "AdvancedResearch";

			public const string NUCLEAR_REFINEMENT = "NuclearRefinement";

			public const string CRYO_FUEL_PROPULSION = "CryoFuelPropulsion";

			public const string SPACE_PROGRAM = "SpaceProgram";

			public const string CRASH_PLAN = "CrashPlan";

			public const string DURABLE_LIFE_SUPPORT = "DurableLifeSupport";

			public const string NUCLEAR_RESEARCH = "NuclearResearch";

			public const string ADVANCED_NUCLEAR_RESEARCH = "AdvancedNuclearResearch";

			public const string NUCLEAR_PROPULSION = "NuclearPropulsion";

			public const string NOTIFICATION_SYSTEMS = "NotificationSystems";

			public const string ARTIFICIAL_FRIENDS = "ArtificialFriends";

			public const string ROBOTIC_TOOLS = "RoboticTools";
		}

		public static class MEDICINE
		{
			public const string MEDICINEI = "MedicineI";

			public const string MEDICINEII = "MedicineII";

			public const string MEDICINEIII = "MedicineIII";

			public const string MEDICINEIV = "MedicineIV";

			public const string RADIATION_PROTECTION = "RadiationProtection";
		}

		public static class LIQUIDS
		{
			public const string LIQUID_PIPING = "LiquidPiping";

			public const string IMPROVED_OXYGEN = "ImprovedOxygen";

			public const string SANITATION_SCIENCES = "SanitationSciences";

			public const string ADVANCED_SANITATION = "AdvancedSanitation";

			public const string ADVANCED_FILTRATION = "AdvancedFiltration";

			public const string LIQUID_FILTERING = "LiquidFiltering";

			public const string DISTILLATION = "Distillation";

			public const string IMPROVED_LIQUID_PIPING = "ImprovedLiquidPiping";

			public const string LIQUID_TEMPERATURE = "LiquidTemperature";

			public const string PRECISION_PLUMBING = "PrecisionPlumbing";

			public const string FLOW_REDIRECTION = "FlowRedirection";

			public const string LIQUID_DISTRIBUTION = "LiquidDistribution";

			public const string JETPACKS = "Jetpacks";
		}

		public static class GASES
		{
			public const string GAS_PIPING = "GasPiping";

			public const string PRESSURE_MANAGEMENT = "PressureManagement";

			public const string TEMPERATURE_MODULATION = "TemperatureModulation";

			public const string DIRECTED_AIR_STREAMS = "DirectedAirStreams";

			public const string IMPROVED_GAS_PIPING = "ImprovedGasPiping";

			public const string HVAC = "HVAC";

			public const string CATALYTICS = "Catalytics";

			public const string PORTABLE_GASSES = "PortableGasses";

			public const string SPACEGAS = "SpaceGas";

			public const string GAS_DISTRIBUTION = "GasDistribution";
		}

		public static class EXOSUITS
		{
			public const string SUITS = "Suits";

			public const string TRAVEL_TUBES = "TravelTubes";
		}

		public static class DECOR
		{
			public const string INTERIOR_DECOR = "InteriorDecor";

			public const string ARTISTRY = "Artistry";

			public const string CLOTHING = "Clothing";

			public const string FINEART = "FineArt";

			public const string LUXURY = "Luxury";

			public const string REFRACTIVE_DECOR = "RefractiveDecor";

			public const string GLASS_FURNISHINGS = "GlassFurnishings";

			public const string RENAISSANCE_ART = "RenaissanceArt";

			public const string ENVIRONMENTAL_APPRECIATION = "EnvironmentalAppreciation";

			public const string SCREENS = "Screens";

			public const string MONUMENTS = "Monuments";
		}

		public static class COMPUTERS
		{
			public const string LOGIC_CONTROL = "LogicControl";

			public const string GENERIC_SENSORS = "GenericSensors";

			public const string LOGIC_CIRCUITS = "LogicCircuits";

			public const string DUPE_TRAFFIC_CONTROL = "DupeTrafficControl";

			public const string PARALLEL_AUTOMATION = "ParallelAutomation";

			public const string MULTIPLEXING = "Multiplexing";

			public const string ADVANCED_SCANNERS = "AdvancedScanners";
		}

		public static class ROCKETS
		{
			public const string SKY_DETECTORS = "SkyDetectors";

			public const string BASIC_ROCKETRY = "BasicRocketry";

			public const string ENGINESI = "EnginesI";

			public const string CARGOI = "CargoI";

			public const string ENGINESII = "EnginesII";

			public const string CARGOII = "CargoII";

			public const string ENGINESIII = "EnginesIII";

			public const string CARGOIII = "CargoIII";

			public const string ADVANCED_RESOURCE_EXTRACTION = "AdvancedResourceExtraction";
		}
	}

	public const float CYCLE_LENGTH = 600f;
}
