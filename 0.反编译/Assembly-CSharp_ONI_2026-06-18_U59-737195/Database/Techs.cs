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
		new Tech("FarmingTech", new List<string> { "AlgaeHabitat", "PlanterBox", "RationBox", "Compost" }, this).AddSearchTerms(SEARCH_TERMS.FARM);
		new Tech("FineDining", new List<string> { "CookingStation", "EggCracker", "DiningTable", "FarmTile" }, this).AddSearchTerms(SEARCH_TERMS.FOOD);
		new Tech("FoodRepurposing", new List<string> { "Juicer", "SpiceGrinder", "MilkPress", "Smoker" }, this).AddSearchTerms(SEARCH_TERMS.FOOD);
		new Tech("FinerDining", new List<string> { "GourmetCookingStation", "FoodDehydrator", "FoodRehydrator", "Deepfryer", "SushiBar" }, this).AddSearchTerms(SEARCH_TERMS.FOOD);
		Tech tech = new Tech("Agriculture", new List<string> { "FarmStation", "FertilizerMaker", "Refrigerator", "HydroponicFarm", "ParkSign", "RadiationLight", "LargeBackwallFarm", "WideFarmTile" }, this);
		tech.AddSearchTerms(SEARCH_TERMS.FARM);
		tech.AddSearchTerms(SEARCH_TERMS.FRIDGE);
		Tech tech2 = new Tech("Ranching", new List<string> { "RanchStation", "CreatureDeliveryPoint", "ShearingStation", "CreatureFeeder", "FishFeeder", "FishDeliveryPoint", "FishPickUp", "CritterPickUp", "CritterDropOff" }, this);
		tech2.AddSearchTerms(SEARCH_TERMS.CRITTER);
		tech2.AddSearchTerms(SEARCH_TERMS.FOOD);
		tech2.AddSearchTerms(SEARCH_TERMS.RANCHING);
		Tech tech3 = new Tech("AnimalControl", new List<string>
		{
			"CreatureAirTrap",
			"CreatureGroundTrap",
			"WaterTrap",
			"EggIncubator",
			LogicCritterCountSensorConfig.ID,
			"UnderwaterRanchStation",
			"UnderwaterShearingStation"
		}, this);
		tech3.AddSearchTerms(SEARCH_TERMS.CRITTER);
		tech3.AddSearchTerms(SEARCH_TERMS.FOOD);
		tech3.AddSearchTerms(SEARCH_TERMS.RANCHING);
		Tech tech4 = new Tech("AnimalComfort", new List<string> { "CritterCondo", "UnderwaterCritterCondo", "AirBorneCritterCondo" }, this);
		tech4.AddSearchTerms(SEARCH_TERMS.CRITTER);
		tech4.AddSearchTerms(SEARCH_TERMS.RANCHING);
		Tech tech5 = new Tech("DairyOperation", new List<string> { "UnderwaterMilkFeeder", "MilkFeeder", "MilkFatSeparator", "MilkingStation", "UnderwaterMilkingStation" }, this);
		tech5.AddSearchTerms(SEARCH_TERMS.CRITTER);
		tech5.AddSearchTerms(SEARCH_TERMS.RANCHING);
		new Tech("ImprovedOxygen", new List<string> { "Electrolyzer", "RustDeoxidizer" }, this).AddSearchTerms(SEARCH_TERMS.OXYGEN);
		new Tech("GasPiping", new List<string> { "GasConduit", "GasConduitBridge", "GasPump", "GasVent" }, this);
		new Tech("ImprovedGasPiping", new List<string>
		{
			"InsulatedGasConduit",
			LogicPressureSensorGasConfig.ID,
			"GasLogicValve",
			"GasVentHighPressure"
		}, this);
		new Tech("SpaceGas", new List<string> { "CO2Engine", "ModularLaunchpadPortGas", "ModularLaunchpadPortGasUnloader", "GasCargoBaySmall" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("PressureManagement", new List<string> { "LiquidValve", "GasValve", "GasPermeableMembrane", "ManualPressureDoor" }, this);
		new Tech("DirectedAirStreams", new List<string> { "AirFilter", "CO2Scrubber", "PressureDoor", "UnderwaterBreathingStation" }, this).AddSearchTerms(SEARCH_TERMS.FILTER);
		new Tech("LiquidFiltering", new List<string> { "OreScrubber", "Desalinator" }, this).AddSearchTerms(SEARCH_TERMS.FILTER);
		new Tech("MedicineI", new List<string> { "Apothecary", "LubricationStick" }, this).AddSearchTerms(SEARCH_TERMS.MEDICINE);
		new Tech("MedicineII", new List<string> { "DoctorStation", "HandSanitizer" }, this).AddSearchTerms(SEARCH_TERMS.MEDICINE);
		new Tech("MedicineIII", new List<string>
		{
			GasConduitDiseaseSensorConfig.ID,
			LiquidConduitDiseaseSensorConfig.ID,
			LogicDiseaseSensorConfig.ID
		}, this).AddSearchTerms(SEARCH_TERMS.MEDICINE);
		new Tech("MedicineIV", new List<string>
		{
			"AdvancedDoctorStation",
			"AdvancedApothecary",
			"HotTub",
			LogicRadiationSensorConfig.ID
		}, this).AddSearchTerms(SEARCH_TERMS.MEDICINE);
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
		new Tech("PrecisionPlumbing", new List<string> { "EspressoMachine", "LiquidFuelTankCluster", "MercuryCeilingLight" }, this);
		new Tech("SanitationSciences", new List<string>
		{
			"FlushToilet",
			"WashSink",
			ShowerConfig.ID,
			"MeshTile",
			"GunkEmptier"
		}, this).AddSearchTerms(SEARCH_TERMS.TOILET);
		new Tech("FlowRedirection", new List<string> { "MechanicalSurfboard", "LiquidBottler", "ModularLaunchpadPortLiquid", "ModularLaunchpadPortLiquidUnloader", "LiquidCargoBaySmall" }, this);
		new Tech("LiquidDistribution", new List<string> { "BottleEmptierConduitLiquid", "RocketInteriorLiquidInput", "RocketInteriorLiquidOutput", "WallToilet" }, this);
		new Tech("AdvancedSanitation", new List<string> { "DecontaminationShower" }, this);
		new Tech("AdvancedFiltration", new List<string> { "GasFilter", "LiquidFilter", "SludgePress", "OilChanger" }, this).AddSearchTerms(SEARCH_TERMS.FILTER);
		Tech tech6 = new Tech("Distillation", new List<string> { "AlgaeDistillery", "EthanolDistillery", "WaterPurifier" }, this);
		tech6.AddSearchTerms(SEARCH_TERMS.WATER);
		new Tech("AdvancedDistillation", new List<string> { "ChemicalRefinery" }, this);
		tech6.AddSearchTerms(SEARCH_TERMS.POWER);
		new Tech("Catalytics", new List<string> { "OxyliteRefinery", "Chlorinator", "SupermaterialRefinery", "SUPER_LIQUIDS", "SodaFountain", "GasCargoBayCluster" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("AdvancedResourceExtraction", new List<string> { "NoseconeHarvest" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech7 = new Tech("PowerRegulation", new List<string>
		{
			"BatteryMedium",
			SwitchConfig.ID,
			"WireBridge",
			"SmallElectrobankDischarger"
		}, this);
		tech7.AddSearchTerms(SEARCH_TERMS.POWER);
		tech7.AddSearchTerms(SEARCH_TERMS.BATTERY);
		tech7.AddSearchTerms(SEARCH_TERMS.WIRE);
		Tech tech8 = new Tech("AdvancedPowerRegulation", new List<string>
		{
			"HighWattageWire",
			"WireBridgeHighWattage",
			"HydrogenGenerator",
			LogicPowerRelayConfig.ID,
			"PowerTransformerSmall",
			LogicWattageSensorConfig.ID
		}, this);
		tech8.AddSearchTerms(SEARCH_TERMS.POWER);
		tech8.AddSearchTerms(SEARCH_TERMS.WIRE);
		tech8.AddSearchTerms(SEARCH_TERMS.GENERATOR);
		Tech tech9 = new Tech("PrettyGoodConductors", new List<string> { "WireRefined", "WireRefinedBridge", "WireRefinedHighWattage", "WireRefinedBridgeHighWattage", "PowerTransformer", "LargeElectrobankDischarger" }, this);
		tech9.AddSearchTerms(SEARCH_TERMS.WIRE);
		tech9.AddSearchTerms(SEARCH_TERMS.POWER);
		Tech tech10 = new Tech("RenewableEnergy", new List<string> { "SteamTurbine2", "SolarPanel", "Sauna", "SteamEngineCluster", "WireRubber", "WireRubberBridge" }, this);
		tech10.AddSearchTerms(SEARCH_TERMS.POWER);
		tech10.AddSearchTerms(SEARCH_TERMS.STEAM);
		Tech tech11 = new Tech("Combustion", new List<string> { "Generator", "WoodGasGenerator", "PeatGenerator", "ReefGenerator" }, this);
		tech11.AddSearchTerms(SEARCH_TERMS.POWER);
		tech11.AddSearchTerms(SEARCH_TERMS.GENERATOR);
		Tech tech12 = new Tech("ImprovedCombustion", new List<string> { "MethaneGenerator", "OilRefinery", "PetroleumGenerator" }, this);
		tech12.AddSearchTerms(SEARCH_TERMS.POWER);
		tech12.AddSearchTerms(SEARCH_TERMS.GENERATOR);
		Tech tech13 = new Tech("InteriorDecor", new List<string> { "FlowerVase", "FloorLamp", "CeilingLight" }, this);
		tech13.AddSearchTerms(SEARCH_TERMS.MORALE);
		tech13.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		Tech tech14 = new Tech("Artistry", new List<string> { "WoodenDoor", "FlowerVaseWall", "FlowerVaseHanging", "CornerMoulding", "CrownMoulding", "ItemPedestal", "SmallSculpture", "IceSculpture" }, this);
		tech14.AddSearchTerms(SEARCH_TERMS.MORALE);
		tech14.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		new Tech("Clothing", new List<string> { "ClothingFabricator", "CarpetTile", "ExteriorWall" }, this).AddSearchTerms(SEARCH_TERMS.TILE);
		Tech tech15 = new Tech("Acoustics", new List<string> { "BatterySmart", "Phonobox", "PowerControlStation", "ElectrobankCharger", "Electrobank" }, this);
		tech15.AddSearchTerms(SEARCH_TERMS.POWER);
		tech15.AddSearchTerms(SEARCH_TERMS.BATTERY);
		Tech tech16 = new Tech("SpacePower", new List<string> { "BatteryModule", "SolarPanelModule", "RocketInteriorPowerPlug" }, this);
		tech16.AddSearchTerms(SEARCH_TERMS.POWER);
		tech16.AddSearchTerms(SEARCH_TERMS.BATTERY);
		tech16.AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech17 = new Tech("NuclearRefinement", new List<string> { "NuclearReactor", "UraniumCentrifuge", "SelfChargingElectrobank" }, this);
		tech17.AddSearchTerms(SEARCH_TERMS.POWER);
		tech17.AddSearchTerms(SEARCH_TERMS.BATTERY);
		Tech tech18 = new Tech("FineArt", new List<string> { "Canvas", "Sculpture", "Shelf" }, this);
		tech18.AddSearchTerms(SEARCH_TERMS.MORALE);
		tech18.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		Tech tech19 = new Tech("EnvironmentalAppreciation", new List<string> { "BeachChair" }, this);
		tech19.AddSearchTerms(SEARCH_TERMS.MORALE);
		if (DlcManager.IsContentSubscribed("DLC4_ID"))
		{
			tech19.AddSearchTerms(SEARCH_TERMS.ARTWORK);
			tech19.AddSearchTerms(SEARCH_TERMS.DINOSAUR);
		}
		Tech tech20 = new Tech("Luxury", new List<string> { "LuxuryBed", "LadderFast", "PlasticTile", "ClothingAlterationStation", "WoodTile", "MultiMinionDiningTable" }, this);
		tech20.AddSearchTerms(SEARCH_TERMS.TILE);
		tech20.AddSearchTerms(SEARCH_TERMS.MORALE);
		Tech tech21 = new Tech("RefractiveDecor", new List<string> { "CanvasWide", "MetalSculpture", "WoodSculpture" }, this);
		tech21.AddSearchTerms(SEARCH_TERMS.MORALE);
		tech21.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		new Tech("GlassFurnishings", new List<string> { "GlassTile", "FlowerVaseHangingFancy", "SunLamp", "GlassExteriorWall", "GlassCeilingLight" }, this);
		new Tech("Screens", new List<string> { PixelPackConfig.ID }, this);
		Tech tech22 = new Tech("RenaissanceArt", new List<string> { "CanvasTall", "MarbleSculpture", "FossilSculpture", "CeilingFossilSculpture" }, this);
		tech22.AddSearchTerms(SEARCH_TERMS.MORALE);
		tech22.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		new Tech("Plastics", new List<string> { "Polymerizer", "OilWellCap" }, this);
		new Tech("ValveMiniaturization", new List<string> { "LiquidMiniPump", "GasMiniPump", "MiniFridge" }, this);
		new Tech("HydrocarbonPropulsion", new List<string> { "KeroseneEngineClusterSmall", "MissionControlCluster" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("BetterHydroCarbonPropulsion", new List<string> { "KeroseneEngineCluster", "BiodieselEngineCluster" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("CryoFuelPropulsion", new List<string> { "HydrogenEngineCluster", "OxidizerTankLiquidCluster" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("Suits", new List<string> { "SuitsOverlay", "AtmoSuit", "SuitFabricator", "SuitMarker", "SuitLocker" }, this);
		new Tech("Jobs", new List<string> { "WaterCooler", "CraftingTable", "DisposableElectrobank_RawMetal", "Campfire" }, this);
		new Tech("AdvancedResearch", new List<string> { "BetaResearchPoint", "AdvancedResearchCenter", "ResetSkillsStation", "ClusterTelescope", "ExobaseHeadquarters", "AdvancedCraftingTable" }, this);
		new Tech("SpaceProgram", new List<string>
		{
			"LaunchPad",
			"HabitatModuleSmall",
			"OrbitalCargoModule",
			RocketControlStationConfig.ID
		}, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("CrashPlan", new List<string> { "OrbitalResearchPoint", "PioneerModule", "OrbitalResearchCenter", "DLC1CosmicResearchCenter" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("DurableLifeSupport", new List<string> { "NoseconeBasic", "HabitatModuleMedium", "ArtifactAnalysisStation", "ArtifactCargoBay", "SpecialCargoBayCluster" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("NuclearResearch", new List<string> { "DeltaResearchPoint", "NuclearResearchCenter", "ManualHighEnergyParticleSpawner", "DisposableElectrobank_UraniumOre" }, this);
		new Tech("AdvancedNuclearResearch", new List<string> { "HighEnergyParticleSpawner", "HighEnergyParticleRedirector", "HEPBridgeTile" }, this);
		new Tech("NuclearStorage", new List<string> { "HEPBattery" }, this);
		new Tech("NuclearPropulsion", new List<string> { "HEPEngine" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("NotificationSystems", new List<string>
		{
			LogicHammerConfig.ID,
			LogicAlarmConfig.ID,
			"Telephone"
		}, this).AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		new Tech("ArtificialFriends", new List<string> { "SweepBotStation", "ScoutModule", "FetchDrone", "OverclockedBoosters" }, this).AddSearchTerms(SEARCH_TERMS.ROBOT);
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
			RubberBootsConfig.ID,
			"DrySuit"
		}, this);
		new Tech("HighTempForging", new List<string> { "GlassForge", "BunkerTile", "BunkerDoor", "GeoTuner" }, this).AddSearchTerms(SEARCH_TERMS.GLASS);
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
			"ContactConductivePipeBridge",
			"RubberTile"
		}, this);
		new Tech("LogicControl", new List<string>
		{
			"AutomationOverlay",
			LogicSwitchConfig.ID,
			"LogicWire",
			"LogicWireBridge",
			"LogicDuplicantSensor"
		}, this).AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		new Tech("GenericSensors", new List<string>
		{
			"FloorSwitch",
			LogicElementSensorGasConfig.ID,
			LogicElementSensorLiquidConfig.ID,
			"LogicGateNOT",
			LogicTimeOfDaySensorConfig.ID,
			LogicTimerSensorConfig.ID,
			LogicLightSensorConfig.ID,
			LogicClusterLocationSensorConfig.ID
		}, this).AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		new Tech("LogicCircuits", new List<string> { "LogicGateAND", "LogicGateOR", "LogicGateBUFFER", "LogicGateFILTER" }, this).AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		new Tech("ParallelAutomation", new List<string>
		{
			"LogicRibbon",
			"LogicRibbonBridge",
			LogicRibbonWriterConfig.ID,
			LogicRibbonReaderConfig.ID
		}, this).AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		Tech tech23 = new Tech("DupeTrafficControl", new List<string>
		{
			LogicCounterConfig.ID,
			LogicMemoryConfig.ID,
			"LogicGateXOR",
			"ArcadeMachine",
			"Checkpoint",
			"CosmicResearchCenter"
		}, this);
		tech23.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		tech23.AddSearchTerms(SEARCH_TERMS.RESEARCH);
		tech23.AddSearchTerms(SEARCH_TERMS.MORALE);
		new Tech("Multiplexing", new List<string> { "LogicGateMultiplexer", "LogicGateDemultiplexer" }, this).AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		new Tech("SkyDetectors", new List<string>
		{
			CometDetectorConfig.ID,
			"Telescope",
			"ResearchClusterModule",
			"ClusterTelescopeEnclosed",
			"AstronautTrainingCenter"
		}, this).AddSearchTerms(SEARCH_TERMS.RESEARCH);
		new Tech("Missiles", new List<string> { "MissileFabricator", "MissileLauncher" }, this);
		new Tech("TravelTubes", new List<string> { "TravelTubeEntrance", "TravelTube", "TravelTubeWallBridge", "VerticalWindTunnel" }, this).AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		new Tech("SmartStorage", new List<string> { "ConveyorOverlay", "SolidTransferArm", "StorageLockerSmart", "ObjectDispenser" }, this).AddSearchTerms(SEARCH_TERMS.STORAGE);
		Tech tech24 = new Tech("SolidManagement", new List<string>
		{
			"SolidFilter",
			SolidConduitTemperatureSensorConfig.ID,
			SolidConduitElementSensorConfig.ID,
			SolidConduitDiseaseSensorConfig.ID,
			"StorageTile",
			"CargoBayCluster"
		}, this);
		tech24.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		tech24.AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		tech24.AddSearchTerms(SEARCH_TERMS.STORAGE);
		new Tech("HighVelocityTransport", new List<string> { "RailGun", "LandingBeacon" }, this).AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		Tech tech25 = new Tech("BasicRocketry", new List<string> { "CommandModule", "SteamEngine", "ResearchModule", "Gantry" }, this);
		tech25.AddSearchTerms(SEARCH_TERMS.ROCKET);
		tech25.AddSearchTerms(SEARCH_TERMS.RESEARCH);
		tech25.AddSearchTerms(SEARCH_TERMS.STEAM);
		new Tech("CargoI", new List<string> { "CargoBay" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("CargoII", new List<string> { "LiquidCargoBay", "GasCargoBay" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("CargoIII", new List<string> { "TouristModule", "SpecialCargoBay" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("EnginesI", new List<string> { "SolidBooster", "MissionControl" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("EnginesII", new List<string> { "KeroseneEngine", "BiodieselEngine", "LiquidFuelTank", "OxidizerTank" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		new Tech("EnginesIII", new List<string> { "OxidizerTankLiquid", "OxidizerTankCluster", "HydrogenEngine" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		Tech tech26 = new Tech("Jetpacks", new List<string> { "JetSuit", "JetSuitMarker", "JetSuitLocker", "LiquidCargoBayCluster" }, this);
		tech26.AddSearchTerms(SEARCH_TERMS.ROCKET);
		tech26.AddSearchTerms(SEARCH_TERMS.MISSILE);
		new Tech("SolidTransport", new List<string> { "SolidConduitInbox", "SolidConduit", "SolidConduitBridge", "SolidVent", "SolidCargoBaySmall", "ModularLaunchpadPortSolid", "ModularLaunchpadPortSolidUnloader", "ModularLaunchpadPortBridge" }, this).AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		Tech tech27 = new Tech("Monuments", new List<string> { "MonumentBottom", "MonumentMiddle", "MonumentTop" }, this);
		tech27.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		tech27.AddSearchTerms(SEARCH_TERMS.MORALE);
		Tech tech28 = new Tech("SolidSpace", new List<string> { "SolidLogicValve", "SolidConduitOutbox", "SolidLimitValve", "RocketInteriorSolidInput", "RocketInteriorSolidOutput" }, this);
		tech28.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		tech28.AddSearchTerms(SEARCH_TERMS.ROCKET);
		tech28.AddSearchTerms(SEARCH_TERMS.TRANSPORT);
		new Tech("RoboticTools", new List<string> { "AutoMiner", "RailGunPayloadOpener", "RoboPilotModule", "UnderwaterVentDrill" }, this).AddSearchTerms(SEARCH_TERMS.ROBOT);
		new Tech("PortableGasses", new List<string> { "GasBottler", "BottleEmptierGas", "OxygenMask", "OxygenMaskLocker", "OxygenMaskMarker", "Oxysconce" }, this).AddSearchTerms(SEARCH_TERMS.OXYGEN);
		new Tech("GasDistribution", new List<string> { "BottleEmptierConduitGas", "RocketInteriorGasInput", "RocketInteriorGasOutput", "OxidizerTankCluster" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
		InitBaseGameOnly();
		InitExpansion1();
	}

	private void InitBaseGameOnly()
	{
		if (!DlcManager.IsExpansion1Active() && DlcManager.IsContentSubscribed("DLC3_ID"))
		{
			new Tech("DataScienceBaseGame", new List<string>
			{
				"DataMiner",
				RemoteWorkerDockConfig.ID,
				RemoteWorkTerminalConfig.ID,
				"RoboPilotCommandModule"
			}, this).AddSearchTerms(SEARCH_TERMS.ROBOT);
		}
	}

	private void InitExpansion1()
	{
		if (DlcManager.IsExpansion1Active())
		{
			Get("HighTempForging").AddUnlockedItemIDs("Gantry");
			new Tech("Bioengineering", new List<string> { "GeneticAnalysisStation" }, this).AddSearchTerms(SEARCH_TERMS.RESEARCH);
			new Tech("SpaceCombustion", new List<string> { "SugarEngine", "SmallOxidizerTank" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
			new Tech("HighVelocityDestruction", new List<string> { "NoseconeHarvest" }, this).AddSearchTerms(SEARCH_TERMS.ROCKET);
			new Tech("AdvancedScanners", new List<string> { "ScannerModule", "LogicInterasteroidSender", "LogicInterasteroidReceiver" }, this).AddSearchTerms(SEARCH_TERMS.AUTOMATION);
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
		list.Sort((TechTreeTitle a, TechTreeTitle b) => a.center.y.CompareTo(b.center.y));
		foreach (ResourceTreeNode item in resourceTreeLoader)
		{
			if (string.Equals(item.Id.Substring(0, 1), "_"))
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
			foreach (Tuple<string, float> item2 in TECH_TIERS[resource.tier])
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
