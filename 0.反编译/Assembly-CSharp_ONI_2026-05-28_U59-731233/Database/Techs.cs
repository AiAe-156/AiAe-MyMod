using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;

namespace Database;

public class Techs : ResourceSet<Tech>
{
	private readonly List<List<Tuple<string, float>>> TECH_TIERS;

	public Techs(ResourceSet parent)
		: base("Techs", parent)
	{
		if (!DlcManager.IsExpansion1Active())
		{
			TECH_TIERS = new List<List<Tuple<string, float>>>
			{
				new List<Tuple<string, float>>(),
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 15f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 20f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 30f),
					new Tuple<string, float>("advanced", 20f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 35f),
					new Tuple<string, float>("advanced", 30f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 40f),
					new Tuple<string, float>("advanced", 50f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 50f),
					new Tuple<string, float>("advanced", 70f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 70f),
					new Tuple<string, float>("advanced", 100f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 70f),
					new Tuple<string, float>("advanced", 100f),
					new Tuple<string, float>("space", 200f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 70f),
					new Tuple<string, float>("advanced", 100f),
					new Tuple<string, float>("space", 400f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 70f),
					new Tuple<string, float>("advanced", 100f),
					new Tuple<string, float>("space", 800f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 70f),
					new Tuple<string, float>("advanced", 100f),
					new Tuple<string, float>("space", 1600f)
				}
			};
		}
		else
		{
			TECH_TIERS = new List<List<Tuple<string, float>>>
			{
				new List<Tuple<string, float>>(),
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 15f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 20f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 30f),
					new Tuple<string, float>("advanced", 20f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 35f),
					new Tuple<string, float>("advanced", 30f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 40f),
					new Tuple<string, float>("advanced", 50f),
					new Tuple<string, float>("orbital", 0f),
					new Tuple<string, float>("nuclear", 20f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 50f),
					new Tuple<string, float>("advanced", 70f),
					new Tuple<string, float>("orbital", 30f),
					new Tuple<string, float>("nuclear", 40f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 70f),
					new Tuple<string, float>("advanced", 100f),
					new Tuple<string, float>("orbital", 250f),
					new Tuple<string, float>("nuclear", 370f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 100f),
					new Tuple<string, float>("advanced", 130f),
					new Tuple<string, float>("orbital", 400f),
					new Tuple<string, float>("nuclear", 435f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 100f),
					new Tuple<string, float>("advanced", 130f),
					new Tuple<string, float>("orbital", 600f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 100f),
					new Tuple<string, float>("advanced", 130f),
					new Tuple<string, float>("orbital", 800f)
				},
				new List<Tuple<string, float>>
				{
					new Tuple<string, float>("basic", 100f),
					new Tuple<string, float>("advanced", 130f),
					new Tuple<string, float>("orbital", 1600f)
				}
			};
		}
	}

	public void Init()
	{
		Tech tech = new Tech("FarmingTech", new List<string> { "AlgaeHabitat", "PlanterBox", "RationBox", "Compost" }, this);
		tech.AddSearchTerms(SEARCH_TERMS.FARM);
		Tech tech2 = new Tech("FineDining", new List<string> { "CookingStation", "EggCracker", "DiningTable", "FarmTile" }, this);
		tech2.AddSearchTerms(SEARCH_TERMS.FOOD);
		Tech tech3 = new Tech("FoodRepurposing", new List<string> { "Juicer", "SpiceGrinder", "MilkPress", "Smoker" }, this);
		tech3.AddSearchTerms(SEARCH_TERMS.FOOD);
		Tech tech4 = new Tech("FinerDining", new List<string> { "GourmetCookingStation", "FoodDehydrator", "FoodRehydrator", "Deepfryer", "SushiBar" }, this);
		tech4.AddSearchTerms(SEARCH_TERMS.FOOD);
		Tech tech5 = new Tech("Agriculture", new List<string> { "FarmStation", "FertilizerMaker", "Refrigerator", "HydroponicFarm", "ParkSign", "RadiationLight", "LargeBackwallFarm", "WideFarmTile" }, this);
		tech5.AddSearchTerms(SEARCH_TERMS.FARM);
		tech5.AddSearchTerms(SEARCH_TERMS.FRIDGE);
		Tech tech6 = new Tech("Ranching", new List<string> { "RanchStation", "CreatureDeliveryPoint", "ShearingStation", "CreatureFeeder", "FishFeeder", "FishDeliveryPoint", "FishPickUp", "CritterPickUp", "CritterDropOff" }, this);
		tech6.AddSearchTerms(SEARCH_TERMS.CRITTER);
		tech6.AddSearchTerms(SEARCH_TERMS.FOOD);
		tech6.AddSearchTerms(SEARCH_TERMS.RANCHING);
		Tech tech7 = new Tech("AnimalControl", new List<string>
		{
			"CreatureAirTrap",
			"CreatureGroundTrap",
			"WaterTrap",
			"EggIncubator",
			LogicCritterCountSensorConfig.ID,
			"UnderwaterRanchStation",
			"UnderwaterShearingStation"
		}, this);
		tech7.AddSearchTerms(SEARCH_TERMS.CRITTER);
		tech7.AddSearchTerms(SEARCH_TERMS.FOOD);
		tech7.AddSearchTerms(SEARCH_TERMS.RANCHING);
		Tech tech8 = new Tech("AnimalComfort", new List<string> { "CritterCondo", "UnderwaterCritterCondo", "AirBorneCritterCondo" }, this);
		tech8.AddSearchTerms(SEARCH_TERMS.CRITTER);
		tech8.AddSearchTerms(SEARCH_TERMS.RANCHING);
		Tech tech9 = new Tech("DairyOperation", new List<string> { "UnderwaterMilkFeeder", "MilkFeeder", "MilkFatSeparator", "MilkingStation", "UnderwaterMilkingStation" }, this);
		tech9.AddSearchTerms(SEARCH_TERMS.CRITTER);
		tech9.AddSearchTerms(SEARCH_TERMS.RANCHING);
		Tech tech10 = new Tech("ImprovedOxygen", new List<string> { "Electrolyzer", "RustDeoxidizer" }, this);
		tech10.AddSearchTerms(SEARCH_TERMS.OXYGEN);
		new Tech("GasPiping", new List<string> { "GasConduit", "GasConduitBridge", "GasPump", "GasVent" }, this);
		new Tech("ImprovedGasPiping", new List<string>
		{
			"InsulatedGasConduit",
			LogicPressureSensorGasConfig.ID,
			"GasLogicValve",
			"GasVentHighPressure"
		}, this);
		Tech tech11 = new Tech("SpaceGas", new List<string> { "CO2Engine", "ModularLaunchpadPortGas", "ModularLaunchpadPortGasUnloader", "GasCargoBaySmall" }, this);
		tech11.AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("PressureManagement", new List<string> { "LiquidValve", "GasValve", "GasPermeableMembrane", "ManualPressureDoor" }, this);
		Tech tech12 = new Tech("DirectedAirStreams", new List<string> { "AirFilter", "CO2Scrubber", "PressureDoor", "UnderwaterBreathingStation" }, this);
		tech12.AddSearchTerms(SEARCH_TERMS.FILTER);
		Tech tech13 = new Tech("LiquidFiltering", new List<string> { "OreScrubber", "Desalinator" }, this);
		tech13.AddSearchTerms(SEARCH_TERMS.FILTER);
		Tech tech14 = new Tech("MedicineI", new List<string> { "Apothecary", "LubricationStick" }, this);
		tech14.AddSearchTerms(SEARCH_TERMS.MEDICINE);
		Tech tech15 = new Tech("MedicineII", new List<string> { "DoctorStation", "HandSanitizer" }, this);
		tech15.AddSearchTerms(SEARCH_TERMS.MEDICINE);
		Tech tech16 = new Tech("MedicineIII", new List<string>
		{
			GasConduitDiseaseSensorConfig.ID,
			LiquidConduitDiseaseSensorConfig.ID,
			LogicDiseaseSensorConfig.ID
		}, this);
		tech16.AddSearchTerms(SEARCH_TERMS.MEDICINE);
		Tech tech17 = new Tech("MedicineIV", new List<string>
		{
			"AdvancedDoctorStation",
			"AdvancedApothecary",
			"HotTub",
			LogicRadiationSensorConfig.ID
		}, this);
		tech17.AddSearchTerms(SEARCH_TERMS.MEDICINE);
		new Tech("LiquidPiping", new List<string> { "LiquidConduit", "LiquidConduitBridge", "LiquidPump", "LiquidVent" }, this);
		new Tech("ImprovedLiquidPiping", new List<string>
		{
			"InsulatedLiquidConduit",
			LogicPressureSensorLiquidConfig.ID,
			"LiquidLogicValve",
			"LiquidConduitPreferentialFlow",
			"LiquidConduitOverflow",
			"LiquidReservoir",
			"PlasticGasket"
		}, this);
		new Tech("PrecisionPlumbing", new List<string> { "EspressoMachine", "LiquidFuelTankCluster", "MercuryCeilingLight", "DrySuit" }, this);
		Tech tech18 = new Tech("SanitationSciences", new List<string>
		{
			"FlushToilet",
			"WashSink",
			ShowerConfig.ID,
			"MeshTile",
			"GunkEmptier"
		}, this);
		tech18.AddSearchTerms(SEARCH_TERMS.TOILET);
		new Tech("FlowRedirection", new List<string> { "MechanicalSurfboard", "LiquidBottler", "ModularLaunchpadPortLiquid", "ModularLaunchpadPortLiquidUnloader", "LiquidCargoBaySmall" }, this);
		new Tech("LiquidDistribution", new List<string> { "BottleEmptierConduitLiquid", "RocketInteriorLiquidInput", "RocketInteriorLiquidOutput", "WallToilet" }, this);
		new Tech("AdvancedSanitation", new List<string> { "DecontaminationShower" }, this);
		Tech tech19 = new Tech("AdvancedFiltration", new List<string> { "GasFilter", "LiquidFilter", "SludgePress", "OilChanger" }, this);
		tech19.AddSearchTerms(SEARCH_TERMS.FILTER);
		Tech tech20 = new Tech("Distillation", new List<string> { "AlgaeDistillery", "EthanolDistillery", "WaterPurifier" }, this);
		tech20.AddSearchTerms(SEARCH_TERMS.WATER);
		Tech tech21 = new Tech("AdvancedDistillation", new List<string> { "ChemicalRefinery" }, this);
		tech20.AddSearchTerms(SEARCH_TERMS.POWER);
		Tech tech22 = new Tech("Catalytics", new List<string> { "OxyliteRefinery", "Chlorinator", "SupermaterialRefinery", "SUPER_LIQUIDS", "SodaFountain", "GasCargoBayCluster" }, this);
		tech22.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech23 = new Tech("AdvancedResourceExtraction", new List<string> { "NoseconeHarvest" }, this);
		tech23.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech24 = new Tech("PowerRegulation", new List<string>
		{
			"BatteryMedium",
			SwitchConfig.ID,
			"WireBridge",
			"SmallElectrobankDischarger"
		}, this);
		tech24.AddSearchTerms(SEARCH_TERMS.POWER);
		tech24.AddSearchTerms(SEARCH_TERMS.BATTERY);
		tech24.AddSearchTerms(SEARCH_TERMS.WIRE);
		Tech tech25 = new Tech("AdvancedPowerRegulation", new List<string>
		{
			"HighWattageWire",
			"WireBridgeHighWattage",
			"HydrogenGenerator",
			LogicPowerRelayConfig.ID,
			"PowerTransformerSmall",
			LogicWattageSensorConfig.ID
		}, this);
		tech25.AddSearchTerms(SEARCH_TERMS.POWER);
		tech25.AddSearchTerms(SEARCH_TERMS.WIRE);
		tech25.AddSearchTerms(SEARCH_TERMS.GENERATOR);
		Tech tech26 = new Tech("PrettyGoodConductors", new List<string> { "WireRefined", "WireRefinedBridge", "WireRefinedHighWattage", "WireRefinedBridgeHighWattage", "PowerTransformer", "LargeElectrobankDischarger" }, this);
		tech26.AddSearchTerms(SEARCH_TERMS.WIRE);
		tech26.AddSearchTerms(SEARCH_TERMS.POWER);
		Tech tech27 = new Tech("RenewableEnergy", new List<string> { "SteamTurbine2", "SolarPanel", "Sauna", "SteamEngineCluster", "WireRubber", "WireRubberBridge" }, this);
		tech27.AddSearchTerms(SEARCH_TERMS.POWER);
		tech27.AddSearchTerms(SEARCH_TERMS.STEAM);
		Tech tech28 = new Tech("Combustion", new List<string> { "Generator", "WoodGasGenerator", "PeatGenerator", "ReefGenerator" }, this);
		tech28.AddSearchTerms(SEARCH_TERMS.POWER);
		tech28.AddSearchTerms(SEARCH_TERMS.GENERATOR);
		Tech tech29 = new Tech("ImprovedCombustion", new List<string> { "MethaneGenerator", "OilRefinery", "PetroleumGenerator" }, this);
		tech29.AddSearchTerms(SEARCH_TERMS.POWER);
		tech29.AddSearchTerms(SEARCH_TERMS.GENERATOR);
		Tech tech30 = new Tech("InteriorDecor", new List<string> { "FlowerVase", "FloorLamp", "CeilingLight" }, this);
		tech30.AddSearchTerms(SEARCH_TERMS.MORALE);
		tech30.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		Tech tech31 = new Tech("Artistry", new List<string> { "WoodenDoor", "FlowerVaseWall", "FlowerVaseHanging", "CornerMoulding", "CrownMoulding", "ItemPedestal", "SmallSculpture", "IceSculpture" }, this);
		tech31.AddSearchTerms(SEARCH_TERMS.MORALE);
		tech31.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		Tech tech32 = new Tech("Clothing", new List<string> { "ClothingFabricator", "CarpetTile", "ExteriorWall" }, this);
		tech32.AddSearchTerms(SEARCH_TERMS.TILE);
		Tech tech33 = new Tech("Acoustics", new List<string> { "BatterySmart", "Phonobox", "PowerControlStation", "ElectrobankCharger", "Electrobank" }, this);
		tech33.AddSearchTerms(SEARCH_TERMS.POWER);
		tech33.AddSearchTerms(SEARCH_TERMS.BATTERY);
		Tech tech34 = new Tech("SpacePower", new List<string> { "BatteryModule", "SolarPanelModule", "RocketInteriorPowerPlug" }, this);
		tech34.AddSearchTerms(SEARCH_TERMS.POWER);
		tech34.AddSearchTerms(SEARCH_TERMS.BATTERY);
		tech34.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech35 = new Tech("NuclearRefinement", new List<string> { "NuclearReactor", "UraniumCentrifuge", "SelfChargingElectrobank" }, this);
		tech35.AddSearchTerms(SEARCH_TERMS.POWER);
		tech35.AddSearchTerms(SEARCH_TERMS.BATTERY);
		Tech tech36 = new Tech("FineArt", new List<string> { "Canvas", "Sculpture", "Shelf" }, this);
		tech36.AddSearchTerms(SEARCH_TERMS.MORALE);
		tech36.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		Tech tech37 = new Tech("EnvironmentalAppreciation", new List<string> { "BeachChair" }, this);
		tech37.AddSearchTerms(SEARCH_TERMS.MORALE);
		if (DlcManager.IsContentSubscribed("DLC4_ID"))
		{
			tech37.AddSearchTerms(SEARCH_TERMS.ARTWORK);
			tech37.AddSearchTerms(SEARCH_TERMS.DINOSAUR);
		}
		Tech tech38 = new Tech("Luxury", new List<string> { "LuxuryBed", "LadderFast", "PlasticTile", "ClothingAlterationStation", "WoodTile", "MultiMinionDiningTable" }, this);
		tech38.AddSearchTerms(SEARCH_TERMS.TILE);
		tech38.AddSearchTerms(SEARCH_TERMS.MORALE);
		Tech tech39 = new Tech("RefractiveDecor", new List<string> { "CanvasWide", "MetalSculpture", "WoodSculpture" }, this);
		tech39.AddSearchTerms(SEARCH_TERMS.MORALE);
		tech39.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		new Tech("GlassFurnishings", new List<string> { "GlassTile", "FlowerVaseHangingFancy", "SunLamp", "GlassExteriorWall", "GlassCeilingLight" }, this);
		new Tech("Screens", new List<string> { PixelPackConfig.ID }, this);
		Tech tech40 = new Tech("RenaissanceArt", new List<string> { "CanvasTall", "MarbleSculpture", "FossilSculpture", "CeilingFossilSculpture" }, this);
		tech40.AddSearchTerms(SEARCH_TERMS.MORALE);
		tech40.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		new Tech("Plastics", new List<string> { "Polymerizer", "OilWellCap" }, this);
		new Tech("ValveMiniaturization", new List<string> { "LiquidMiniPump", "GasMiniPump", "MiniFridge" }, this);
		Tech tech41 = new Tech("HydrocarbonPropulsion", new List<string> { "KeroseneEngineClusterSmall", "MissionControlCluster" }, this);
		tech41.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech42 = new Tech("BetterHydroCarbonPropulsion", new List<string> { "KeroseneEngineCluster", "BiodieselEngineCluster" }, this);
		tech42.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech43 = new Tech("CryoFuelPropulsion", new List<string> { "HydrogenEngineCluster", "OxidizerTankLiquidCluster" }, this);
		tech43.AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("Suits", new List<string> { "SuitsOverlay", "AtmoSuit", "SuitFabricator", "SuitMarker", "SuitLocker" }, this);
		new Tech("Jobs", new List<string> { "WaterCooler", "CraftingTable", "DisposableElectrobank_RawMetal", "Campfire" }, this);
		new Tech("AdvancedResearch", new List<string> { "BetaResearchPoint", "AdvancedResearchCenter", "ResetSkillsStation", "ClusterTelescope", "ExobaseHeadquarters", "AdvancedCraftingTable" }, this);
		Tech tech44 = new Tech("SpaceProgram", new List<string>
		{
			"LaunchPad",
			"HabitatModuleSmall",
			"OrbitalCargoModule",
			RocketControlStationConfig.ID
		}, this);
		tech44.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech45 = new Tech("CrashPlan", new List<string> { "OrbitalResearchPoint", "PioneerModule", "OrbitalResearchCenter", "DLC1CosmicResearchCenter" }, this);
		tech45.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech46 = new Tech("DurableLifeSupport", new List<string> { "NoseconeBasic", "HabitatModuleMedium", "ArtifactAnalysisStation", "ArtifactCargoBay", "SpecialCargoBayCluster" }, this);
		tech46.AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("NuclearResearch", new List<string> { "DeltaResearchPoint", "NuclearResearchCenter", "ManualHighEnergyParticleSpawner", "DisposableElectrobank_UraniumOre" }, this);
		new Tech("AdvancedNuclearResearch", new List<string> { "HighEnergyParticleSpawner", "HighEnergyParticleRedirector", "HEPBridgeTile" }, this);
		new Tech("NuclearStorage", new List<string> { "HEPBattery" }, this);
		Tech tech47 = new Tech("NuclearPropulsion", new List<string> { "HEPEngine" }, this);
		tech47.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech48 = new Tech("NotificationSystems", new List<string>
		{
			LogicHammerConfig.ID,
			LogicAlarmConfig.ID,
			"Telephone"
		}, this);
		tech48.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		Tech tech49 = new Tech("ArtificialFriends", new List<string> { "SweepBotStation", "ScoutModule", "FetchDrone" }, this);
		tech49.AddSearchTerms(SEARCH_TERMS.ROBOT);
		new Tech("BasicRefinement", new List<string> { "RockCrusher", "Kiln" }, this);
		new Tech("RefinedObjects", new List<string>
		{
			"FabricatedWoodMaker",
			"FirePole",
			"ThermalBlock",
			LadderBedConfig.ID
		}, this);
		new Tech("Smelting", new List<string>
		{
			"MetalRefinery",
			"MetalTile",
			"RubberMaker",
			RubberBootsConfig.ID
		}, this);
		Tech tech50 = new Tech("HighTempForging", new List<string> { "GlassForge", "BunkerTile", "BunkerDoor", "GeoTuner" }, this);
		tech50.AddSearchTerms(SEARCH_TERMS.GLASS);
		new Tech("HighPressureForging", new List<string> { "DiamondPress" }, this);
		new Tech("RadiationProtection", new List<string>
		{
			"LeadSuit",
			"LeadSuitMarker",
			"LeadSuitLocker",
			LogicHEPSensorConfig.ID
		}, this);
		new Tech("TemperatureModulation", new List<string> { "InsulatedDoor", "LiquidCooledFan", "IceCooledFan", "IceMachine", "IceKettle", "InsulationTile", "SpaceHeater" }, this);
		new Tech("HVAC", new List<string>
		{
			"AirConditioner",
			LogicTemperatureSensorConfig.ID,
			GasConduitTemperatureSensorConfig.ID,
			GasConduitElementSensorConfig.ID,
			"GasConduitRadiant",
			"GasReservoir",
			"GasLimitValve"
		}, this);
		new Tech("LiquidTemperature", new List<string>
		{
			"LiquidConduitRadiant",
			"LiquidConditioner",
			LiquidConduitTemperatureSensorConfig.ID,
			LiquidConduitElementSensorConfig.ID,
			"LiquidHeater",
			"LiquidLimitValve",
			"ContactConductivePipeBridge"
		}, this);
		Tech tech51 = new Tech("LogicControl", new List<string>
		{
			"AutomationOverlay",
			LogicSwitchConfig.ID,
			"LogicWire",
			"LogicWireBridge",
			"LogicDuplicantSensor"
		}, this);
		tech51.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		Tech tech52 = new Tech("GenericSensors", new List<string>
		{
			"FloorSwitch",
			LogicElementSensorGasConfig.ID,
			LogicElementSensorLiquidConfig.ID,
			"LogicGateNOT",
			LogicTimeOfDaySensorConfig.ID,
			LogicTimerSensorConfig.ID,
			LogicLightSensorConfig.ID,
			LogicClusterLocationSensorConfig.ID
		}, this);
		tech52.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		Tech tech53 = new Tech("LogicCircuits", new List<string> { "LogicGateAND", "LogicGateOR", "LogicGateBUFFER", "LogicGateFILTER" }, this);
		tech53.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		Tech tech54 = new Tech("ParallelAutomation", new List<string>
		{
			"LogicRibbon",
			"LogicRibbonBridge",
			LogicRibbonWriterConfig.ID,
			LogicRibbonReaderConfig.ID
		}, this);
		tech54.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		Tech tech55 = new Tech("DupeTrafficControl", new List<string>
		{
			LogicCounterConfig.ID,
			LogicMemoryConfig.ID,
			"LogicGateXOR",
			"ArcadeMachine",
			"Checkpoint",
			"CosmicResearchCenter"
		}, this);
		tech55.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		tech55.AddSearchTerms(SEARCH_TERMS.RESEARCH);
		tech55.AddSearchTerms(SEARCH_TERMS.MORALE);
		Tech tech56 = new Tech("Multiplexing", new List<string> { "LogicGateMultiplexer", "LogicGateDemultiplexer" }, this);
		tech56.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		Tech tech57 = new Tech("SkyDetectors", new List<string>
		{
			CometDetectorConfig.ID,
			"Telescope",
			"ResearchClusterModule",
			"ClusterTelescopeEnclosed",
			"AstronautTrainingCenter"
		}, this);
		tech57.AddSearchTerms(SEARCH_TERMS.RESEARCH);
		Tech tech58 = new Tech("Missiles", new List<string> { "MissileFabricator", "MissileLauncher" }, this);
		Tech tech59 = new Tech("TravelTubes", new List<string> { "TravelTubeEntrance", "TravelTube", "TravelTubeWallBridge", "VerticalWindTunnel" }, this);
		tech59.AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		Tech tech60 = new Tech("SmartStorage", new List<string> { "ConveyorOverlay", "SolidTransferArm", "StorageLockerSmart", "ObjectDispenser" }, this);
		tech60.AddSearchTerms(SEARCH_TERMS.STORAGE);
		Tech tech61 = new Tech("SolidManagement", new List<string>
		{
			"SolidFilter",
			SolidConduitTemperatureSensorConfig.ID,
			SolidConduitElementSensorConfig.ID,
			SolidConduitDiseaseSensorConfig.ID,
			"StorageTile",
			"CargoBayCluster"
		}, this);
		tech61.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		tech61.AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		tech61.AddSearchTerms(SEARCH_TERMS.STORAGE);
		Tech tech62 = new Tech("HighVelocityTransport", new List<string> { "RailGun", "LandingBeacon" }, this);
		tech62.AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		Tech tech63 = new Tech("BasicRocketry", new List<string> { "CommandModule", "SteamEngine", "ResearchModule", "Gantry" }, this);
		tech63.AddSearchTerms(SEARCH_TERMS.ROCKET);
		tech63.AddSearchTerms(SEARCH_TERMS.RESEARCH);
		tech63.AddSearchTerms(SEARCH_TERMS.STEAM);
		Tech tech64 = new Tech("CargoI", new List<string> { "CargoBay" }, this);
		tech64.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech65 = new Tech("CargoII", new List<string> { "LiquidCargoBay", "GasCargoBay" }, this);
		tech65.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech66 = new Tech("CargoIII", new List<string> { "TouristModule", "SpecialCargoBay" }, this);
		tech66.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech67 = new Tech("EnginesI", new List<string> { "SolidBooster", "MissionControl" }, this);
		tech67.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech68 = new Tech("EnginesII", new List<string> { "KeroseneEngine", "BiodieselEngine", "LiquidFuelTank", "OxidizerTank" }, this);
		tech68.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech69 = new Tech("EnginesIII", new List<string> { "OxidizerTankLiquid", "OxidizerTankCluster", "HydrogenEngine" }, this);
		tech69.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech70 = new Tech("Jetpacks", new List<string> { "JetSuit", "JetSuitMarker", "JetSuitLocker", "LiquidCargoBayCluster" }, this);
		tech70.AddSearchTerms(SEARCH_TERMS.ROCKET);
		tech70.AddSearchTerms(SEARCH_TERMS.MISSILE);
		Tech tech71 = new Tech("SolidTransport", new List<string> { "SolidConduitInbox", "SolidConduit", "SolidConduitBridge", "SolidVent", "SolidCargoBaySmall", "ModularLaunchpadPortSolid", "ModularLaunchpadPortSolidUnloader", "ModularLaunchpadPortBridge" }, this);
		tech71.AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		Tech tech72 = new Tech("Monuments", new List<string> { "MonumentBottom", "MonumentMiddle", "MonumentTop" }, this);
		tech72.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		tech72.AddSearchTerms(SEARCH_TERMS.MORALE);
		Tech tech73 = new Tech("SolidSpace", new List<string> { "SolidLogicValve", "SolidConduitOutbox", "SolidLimitValve", "RocketInteriorSolidInput", "RocketInteriorSolidOutput" }, this);
		tech73.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		tech73.AddSearchTerms(SEARCH_TERMS.ROCKET);
		tech73.AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		Tech tech74 = new Tech("RoboticTools", new List<string> { "AutoMiner", "RailGunPayloadOpener", "RoboPilotModule", "UnderwaterVentDrill" }, this);
		tech74.AddSearchTerms(SEARCH_TERMS.ROBOT);
		Tech tech75 = new Tech("PortableGasses", new List<string> { "GasBottler", "BottleEmptierGas", "OxygenMask", "OxygenMaskLocker", "OxygenMaskMarker", "Oxysconce" }, this);
		tech75.AddSearchTerms(SEARCH_TERMS.OXYGEN);
		Tech tech76 = new Tech("GasDistribution", new List<string> { "BottleEmptierConduitGas", "RocketInteriorGasInput", "RocketInteriorGasOutput", "OxidizerTankCluster" }, this);
		tech76.AddSearchTerms(SEARCH_TERMS.ROCKET);
		InitBaseGameOnly();
		InitExpansion1();
	}

	private void InitBaseGameOnly()
	{
		if (!DlcManager.IsExpansion1Active() && DlcManager.IsContentSubscribed("DLC3_ID"))
		{
			Tech tech = new Tech("DataScienceBaseGame", new List<string>
			{
				"DataMiner",
				RemoteWorkerDockConfig.ID,
				RemoteWorkTerminalConfig.ID,
				"RoboPilotCommandModule"
			}, this);
			tech.AddSearchTerms(SEARCH_TERMS.ROBOT);
		}
	}

	private void InitExpansion1()
	{
		if (DlcManager.IsExpansion1Active())
		{
			Get("HighTempForging").AddUnlockedItemIDs("Gantry");
			Tech tech = new Tech("Bioengineering", new List<string> { "GeneticAnalysisStation" }, this);
			tech.AddSearchTerms(SEARCH_TERMS.RESEARCH);
			Tech tech2 = new Tech("SpaceCombustion", new List<string> { "SugarEngine", "SmallOxidizerTank" }, this);
			tech2.AddSearchTerms(SEARCH_TERMS.ROCKET);
			Tech tech3 = new Tech("HighVelocityDestruction", new List<string> { "NoseconeHarvest" }, this);
			tech3.AddSearchTerms(SEARCH_TERMS.ROCKET);
			Tech tech4 = new Tech("AdvancedScanners", new List<string> { "ScannerModule", "LogicInterasteroidSender", "LogicInterasteroidReceiver" }, this);
			tech4.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
			if (DlcManager.IsContentSubscribed("DLC3_ID"))
			{
				new Tech("DataScience", new List<string>
				{
					"DataMiner",
					RemoteWorkerDockConfig.ID,
					RemoteWorkTerminalConfig.ID
				}, this);
			}
		}
	}

	public void PostProcess()
	{
		foreach (Tech resource in resources)
		{
			List<TechItem> list = new List<TechItem>();
			foreach (string unlockedItemID in resource.unlockedItemIDs)
			{
				TechItem techItem = Db.Get().TechItems.TryGet(unlockedItemID);
				if (techItem != null)
				{
					list.Add(techItem);
				}
			}
			resource.unlockedItems = list;
		}
	}

	public void Load(TextAsset tree_file)
	{
		ResourceTreeLoader<ResourceTreeNode> resourceTreeLoader = new ResourceTreeLoader<ResourceTreeNode>(tree_file);
		List<TechTreeTitle> list = new List<TechTreeTitle>();
		for (int i = 0; i < Db.Get().TechTreeTitles.Count; i++)
		{
			list.Add(Db.Get().TechTreeTitles[i]);
		}
		list.Sort((TechTreeTitle techTreeTitle, TechTreeTitle b) => techTreeTitle.center.y.CompareTo(b.center.y));
		foreach (ResourceTreeNode item in resourceTreeLoader)
		{
			string a = item.Id.Substring(0, 1);
			if (string.Equals(a, "_"))
			{
				continue;
			}
			Tech tech = TryGet(item.Id);
			if (tech == null)
			{
				continue;
			}
			string categoryID = "";
			for (int num = 0; num < list.Count; num++)
			{
				if (list[num].center.y >= item.center.y)
				{
					categoryID = list[num].Id;
					break;
				}
			}
			tech.SetNode(item, categoryID);
			foreach (ResourceTreeNode reference in item.references)
			{
				Tech tech2 = TryGet(reference.Id);
				if (tech2 == null)
				{
					continue;
				}
				categoryID = "";
				for (int num2 = 0; num2 < list.Count; num2++)
				{
					if (list[num2].center.y >= item.center.y)
					{
						categoryID = list[num2].Id;
						break;
					}
				}
				tech2.SetNode(reference, categoryID);
				tech2.requiredTech.Add(tech);
				tech.unlockedTech.Add(tech2);
			}
		}
		foreach (Tech resource in resources)
		{
			resource.tier = GetTier(resource);
			List<Tuple<string, float>> list2 = TECH_TIERS[resource.tier];
			foreach (Tuple<string, float> item2 in list2)
			{
				if (!resource.costsByResearchTypeID.ContainsKey(item2.first))
				{
					resource.costsByResearchTypeID.Add(item2.first, item2.second);
				}
			}
		}
		for (int num3 = Count - 1; num3 >= 0; num3--)
		{
			if (!((Tech)GetResource(num3)).FoundNode)
			{
				Remove((Tech)GetResource(num3));
			}
		}
	}

	public static int GetTier(Tech tech)
	{
		if (tech == null)
		{
			return 0;
		}
		int num = 0;
		foreach (Tech item in tech.requiredTech)
		{
			num = Math.Max(num, GetTier(item));
		}
		return num + 1;
	}

	private void AddPrerequisite(Tech tech, string prerequisite_name)
	{
		Tech tech2 = TryGet(prerequisite_name);
		if (tech2 != null)
		{
			tech.requiredTech.Add(tech2);
			tech2.unlockedTech.Add(tech);
		}
	}

	public Tech TryGetTechForTechItem(string itemId)
	{
		for (int i = 0; i < Count; i++)
		{
			Tech tech = (Tech)GetResource(i);
			if (tech.unlockedItemIDs.Find((string candidateItemId) => candidateItemId == itemId) != null)
			{
				return tech;
			}
		}
		return null;
	}

	public bool IsTechItemComplete(string id)
	{
		foreach (Tech resource in resources)
		{
			foreach (TechItem unlockedItem in resource.unlockedItems)
			{
				if (unlockedItem.Id == id)
				{
					return resource.IsComplete();
				}
			}
		}
		return true;
	}
}
