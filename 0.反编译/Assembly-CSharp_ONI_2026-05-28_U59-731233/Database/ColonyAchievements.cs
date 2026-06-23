using System.Collections.Generic;
using FMODUnity;
using STRINGS;
using TUNING;

namespace Database;

public class ColonyAchievements : ResourceSet<ColonyAchievement>
{
	public ColonyAchievement Thriving;

	public ColonyAchievement ReachedDistantPlanet;

	public ColonyAchievement CollectedArtifacts;

	public ColonyAchievement Survived100Cycles;

	public ColonyAchievement ReachedSpace;

	public ColonyAchievement CompleteSkillBranch;

	public ColonyAchievement CompleteResearchTree;

	public ColonyAchievement Clothe8Dupes;

	public ColonyAchievement Build4NatureReserves;

	public ColonyAchievement Minimum20LivingDupes;

	public ColonyAchievement TameAGassyMoo;

	public ColonyAchievement CoolBuildingTo6K;

	public ColonyAchievement EatkCalFromMeatByCycle100;

	public ColonyAchievement NoFarmTilesAndKCal;

	public ColonyAchievement Generate240000kJClean;

	public ColonyAchievement BuildOutsideStartBiome;

	public ColonyAchievement Travel10000InTubes;

	public ColonyAchievement VarietyOfRooms;

	public ColonyAchievement TameAllBasicCritters;

	public ColonyAchievement SurviveOneYear;

	public ColonyAchievement ExploreOilBiome;

	public ColonyAchievement EatCookedFood;

	public ColonyAchievement BasicPumping;

	public ColonyAchievement BasicComforts;

	public ColonyAchievement PlumbedWashrooms;

	public ColonyAchievement AutomateABuilding;

	public ColonyAchievement MasterpiecePainting;

	public ColonyAchievement InspectPOI;

	public ColonyAchievement HatchACritter;

	public ColonyAchievement CuredDisease;

	public ColonyAchievement GeneratorTuneup;

	public ColonyAchievement ClearFOW;

	public ColonyAchievement HatchRefinement;

	public ColonyAchievement BunkerDoorDefense;

	public ColonyAchievement IdleDuplicants;

	public ColonyAchievement ExosuitCycles;

	public ColonyAchievement FirstTeleport;

	public ColonyAchievement SoftLaunch;

	public ColonyAchievement GMOOK;

	public ColonyAchievement MineTheGap;

	public ColonyAchievement LandedOnAllWorlds;

	public ColonyAchievement RadicalTrip;

	public ColonyAchievement SweeterThanHoney;

	public ColonyAchievement SurviveInARocket;

	public ColonyAchievement RunAReactor;

	public ColonyAchievement ActivateGeothermalPlant;

	public ColonyAchievement EfficientData;

	public ColonyAchievement AllTheCircuits;

	public ColonyAchievement AsteroidDestroyed;

	public ColonyAchievement AsteroidSurvived;

	public ColonyAchievement MinnowRecruited;

	public ColonyAchievements(ResourceSet parent)
		: base("ColonyAchievements", parent)
	{
		Thriving = Add(new ColonyAchievement("Thriving", "WINCONDITION_STAY", COLONY_ACHIEVEMENTS.THRIVING.NAME, COLONY_ACHIEVEMENTS.THRIVING.DESCRIPTION, isVictoryCondition: true, new List<ColonyAchievementRequirement>
		{
			new CycleNumber(200),
			new MinimumMorale(),
			new NumberOfDupes(12),
			new MonumentBuilt()
		}, COLONY_ACHIEVEMENTS.THRIVING.MESSAGE_TITLE, COLONY_ACHIEVEMENTS.THRIVING.MESSAGE_BODY, "victoryShorts/Stay", "victoryLoops/Stay_loop", ThrivingSequence.Start, AudioMixerSnapshots.Get().VictoryNISGenericSnapshot, "home_sweet_home"));
		ReachedDistantPlanet = (DlcManager.IsExpansion1Active() ? Add(new ColonyAchievement("ReachedDistantPlanet", "WINCONDITION_LEAVE", COLONY_ACHIEVEMENTS.DISTANT_PLANET_REACHED.NAME, COLONY_ACHIEVEMENTS.DISTANT_PLANET_REACHED.DESCRIPTION, isVictoryCondition: true, new List<ColonyAchievementRequirement>
		{
			new EstablishColonies(),
			new OpenTemporalTear(),
			new SentCraftIntoTemporalTear()
		}, COLONY_ACHIEVEMENTS.DISTANT_PLANET_REACHED.MESSAGE_TITLE_DLC1, COLONY_ACHIEVEMENTS.DISTANT_PLANET_REACHED.MESSAGE_BODY_DLC1, "victoryShorts/Leave", "victoryLoops/Leave_loop", EnterTemporalTearSequence.Start, AudioMixerSnapshots.Get().VictoryNISRocketSnapshot, "rocket")) : Add(new ColonyAchievement("ReachedDistantPlanet", "WINCONDITION_LEAVE", COLONY_ACHIEVEMENTS.DISTANT_PLANET_REACHED.NAME, COLONY_ACHIEVEMENTS.DISTANT_PLANET_REACHED.DESCRIPTION, isVictoryCondition: true, new List<ColonyAchievementRequirement>
		{
			new ReachedSpace(Db.Get().SpaceDestinationTypes.Wormhole)
		}, COLONY_ACHIEVEMENTS.DISTANT_PLANET_REACHED.MESSAGE_TITLE, COLONY_ACHIEVEMENTS.DISTANT_PLANET_REACHED.MESSAGE_BODY, "victoryShorts/Leave", "victoryLoops/Leave_loop", ReachedDistantPlanetSequence.Start, AudioMixerSnapshots.Get().VictoryNISRocketSnapshot, "rocket")));
		if (DlcManager.IsExpansion1Active())
		{
			CollectedArtifacts = new ColonyAchievement("CollectedArtifacts", "WINCONDITION_ARTIFACTS", COLONY_ACHIEVEMENTS.STUDY_ARTIFACTS.NAME, COLONY_ACHIEVEMENTS.STUDY_ARTIFACTS.DESCRIPTION, isVictoryCondition: true, new List<ColonyAchievementRequirement>
			{
				new CollectedArtifacts(),
				new CollectedSpaceArtifacts()
			}, COLONY_ACHIEVEMENTS.STUDY_ARTIFACTS.MESSAGE_TITLE, COLONY_ACHIEVEMENTS.STUDY_ARTIFACTS.MESSAGE_BODY, "victoryShorts/Artifact", "victoryLoops/Artifact_loop", ArtifactSequence.Start, AudioMixerSnapshots.Get().VictoryNISGenericSnapshot, "cosmic_archaeology", DlcManager.EXPANSION1, null, "EXPANSION1_ID");
			Add(CollectedArtifacts);
		}
		if (DlcManager.IsContentSubscribed("DLC2_ID"))
		{
			ActivateGeothermalPlant = Add(new ColonyAchievement("ActivatedGeothermalPlant", "WINCONDITION_GEOPLANT", COLONY_ACHIEVEMENTS.ACTIVATEGEOTHERMALPLANT.NAME, COLONY_ACHIEVEMENTS.ACTIVATEGEOTHERMALPLANT.DESCRIPTION, isVictoryCondition: true, new List<ColonyAchievementRequirement>
			{
				new DiscoverGeothermalFacility(),
				new RepairGeothermalController(),
				new UseGeothermalPlant(),
				new ClearBlockedGeothermalVent()
			}, COLONY_ACHIEVEMENTS.ACTIVATEGEOTHERMALPLANT.MESSAGE_TITLE, COLONY_ACHIEVEMENTS.ACTIVATEGEOTHERMALPLANT.MESSAGE_BODY, "victoryShorts/Geothermal", "victoryLoops/Geothermal_loop", GeothermalVictorySequence.Start, AudioMixerSnapshots.Get().VictoryNISGenericSnapshot, "geothermalplant", DlcManager.DLC2, null, "DLC2_ID", "GeothermalImperative"));
		}
		Survived100Cycles = Add(new ColonyAchievement("Survived100Cycles", "SURVIVE_HUNDRED_CYCLES", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.SURVIVE_HUNDRED_CYCLES, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.SURVIVE_HUNDRED_CYCLES_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new CycleNumber()
		}, "", "", "", "", null, default(EventReference), "Turn_of_the_Century"));
		ReachedSpace = (DlcManager.IsExpansion1Active() ? Add(new ColonyAchievement("ReachedSpace", "REACH_SPACE_ANY_DESTINATION", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.REACH_SPACE_ANY_DESTINATION, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.REACH_SPACE_ANY_DESTINATION_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new LaunchedCraft()
		}, "", "", "", "", null, default(EventReference), "space_race")) : Add(new ColonyAchievement("ReachedSpace", "REACH_SPACE_ANY_DESTINATION", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.REACH_SPACE_ANY_DESTINATION, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.REACH_SPACE_ANY_DESTINATION_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new ReachedSpace()
		}, "", "", "", "", null, default(EventReference), "space_race")));
		CompleteSkillBranch = Add(new ColonyAchievement("CompleteSkillBranch", "COMPLETED_SKILL_BRANCH", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.COMPLETED_SKILL_BRANCH, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.COMPLETED_SKILL_BRANCH_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new SkillBranchComplete(Db.Get().Skills.GetTerminalSkills())
		}, "", "", "", "", null, default(EventReference), "CompleteSkillBranch"));
		CompleteResearchTree = Add(new ColonyAchievement("CompleteResearchTree", "COMPLETED_RESEARCH", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.COMPLETED_RESEARCH, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.COMPLETED_RESEARCH_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new ResearchComplete()
		}, "", "", "", "", null, default(EventReference), "honorary_doctorate"));
		Clothe8Dupes = Add(new ColonyAchievement("Clothe8Dupes", "EQUIP_EIGHT_DUPES", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.EQUIP_N_DUPES, string.Format(COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.EQUIP_N_DUPES_DESCRIPTION, 8), isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new EquipNDupes(Db.Get().AssignableSlots.Outfit, 8)
		}, "", "", "", "", null, default(EventReference), "and_nowhere_to_go"));
		TameAllBasicCritters = Add(new ColonyAchievement("TameAllBasicCritters", "TAME_BASIC_CRITTERS", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.TAME_BASIC_CRITTERS, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.TAME_BASIC_CRITTERS_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new CritterTypesWithTraits(new List<Tag> { "Drecko", "Hatch", "LightBug", "Mole", "Oilfloater", "Pacu", "Puft", "Moo", "Crab", "Squirrel" })
		}, "", "", "", "", null, default(EventReference), "Animal_friends"));
		Build4NatureReserves = Add(new ColonyAchievement("Build4NatureReserves", "BUILD_NATURE_RESERVES", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.BUILD_NATURE_RESERVES, string.Format(COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.BUILD_NATURE_RESERVES_DESCRIPTION, Db.Get().RoomTypes.NatureReserve.Name, 4), isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new BuildNRoomTypes(Db.Get().RoomTypes.NatureReserve, 4)
		}, "", "", "", "", null, default(EventReference), "Some_Reservations"));
		Minimum20LivingDupes = Add(new ColonyAchievement("Minimum20LivingDupes", "TWENTY_DUPES", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.TWENTY_DUPES, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.TWENTY_DUPES_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new NumberOfDupes(20)
		}, "", "", "", "", null, default(EventReference), "no_place_like_clone"));
		TameAGassyMoo = Add(new ColonyAchievement("TameAGassyMoo", "TAME_GASSYMOO", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.TAME_GASSYMOO, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.TAME_GASSYMOO_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new CritterTypesWithTraits(new List<Tag> { "Moo", "DieselMoo" }, allRequired: false)
		}, "", "", "", "", null, default(EventReference), "moovin_on_up"));
		CoolBuildingTo6K = Add(new ColonyAchievement("CoolBuildingTo6K", "SIXKELVIN_BUILDING", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.SIXKELVIN_BUILDING, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.SIXKELVIN_BUILDING_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new CoolBuildingToXKelvin(6)
		}, "", "", "", "", null, default(EventReference), "not_0k"));
		EatkCalFromMeatByCycle100 = Add(new ColonyAchievement("EatkCalFromMeatByCycle100", "EAT_MEAT", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.EAT_MEAT, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.EAT_MEAT_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new BeforeCycleNumber(),
			new EatXCaloriesFromY(400000, new List<string>
			{
				FOOD.FOOD_TYPES.MEAT.Id,
				FOOD.FOOD_TYPES.DEEP_FRIED_MEAT.Id,
				FOOD.FOOD_TYPES.PEMMICAN.Id,
				FOOD.FOOD_TYPES.FISH_MEAT.Id,
				FOOD.FOOD_TYPES.COOKED_FISH.Id,
				FOOD.FOOD_TYPES.DEEP_FRIED_FISH.Id,
				FOOD.FOOD_TYPES.SHELLFISH_MEAT.Id,
				FOOD.FOOD_TYPES.DEEP_FRIED_SHELLFISH.Id,
				FOOD.FOOD_TYPES.COOKED_MEAT.Id,
				FOOD.FOOD_TYPES.SURF_AND_TURF.Id,
				FOOD.FOOD_TYPES.BURGER.Id,
				FOOD.FOOD_TYPES.JAWBOFILLET.Id,
				FOOD.FOOD_TYPES.SMOKED_FISH.Id,
				FOOD.FOOD_TYPES.SMOKED_DINOSAURMEAT.Id,
				FOOD.FOOD_TYPES.SQUID_MEAT.Id,
				FOOD.FOOD_TYPES.URCHINMEAT.Id
			})
		}, "", "", "", "", null, default(EventReference), "Carnivore"));
		NoFarmTilesAndKCal = Add(new ColonyAchievement("NoFarmTilesAndKCal", "NO_PLANTERBOX", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.NO_PLANTERBOX, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.NO_PLANTERBOX_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new NoFarmables(),
			new EatXCalories(400000)
		}, "", "", "", "", null, default(EventReference), "Locavore"));
		Generate240000kJClean = Add(new ColonyAchievement("Generate240000kJClean", "CLEAN_ENERGY", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.CLEAN_ENERGY, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.CLEAN_ENERGY_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new ProduceXEngeryWithoutUsingYList(240000f, new List<Tag> { "MethaneGenerator", "PetroleumGenerator", "WoodGasGenerator", "Generator", "PeatGenerator" })
		}, "", "", "", "", null, default(EventReference), "sustainably_sustaining"));
		BuildOutsideStartBiome = Add(new ColonyAchievement("BuildOutsideStartBiome", "BUILD_OUTSIDE_BIOME", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.BUILD_OUTSIDE_BIOME, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.BUILD_OUTSIDE_BIOME_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new BuildOutsideStartBiome()
		}, "", "", "", "", null, default(EventReference), "build_outside"));
		Travel10000InTubes = Add(new ColonyAchievement("Travel10000InTubes", "TUBE_TRAVEL_DISTANCE", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.TUBE_TRAVEL_DISTANCE, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.TUBE_TRAVEL_DISTANCE_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new TravelXUsingTransitTubes(NavType.Tube, 10000)
		}, "", "", "", "", null, default(EventReference), "Totally-Tubular"));
		VarietyOfRooms = Add(new ColonyAchievement("VarietyOfRooms", "VARIETY_OF_ROOMS", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.VARIETY_OF_ROOMS, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.VARIETY_OF_ROOMS_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new BuildRoomType(Db.Get().RoomTypes.NatureReserve),
			new BuildRoomType(Db.Get().RoomTypes.Hospital),
			new BuildRoomType(Db.Get().RoomTypes.RecRoom),
			new BuildRoomType(Db.Get().RoomTypes.GreatHall),
			new BuildRoomType(Db.Get().RoomTypes.Bedroom),
			new BuildRoomType(Db.Get().RoomTypes.PlumbedBathroom),
			new BuildRoomType(Db.Get().RoomTypes.Farm),
			new BuildRoomType(Db.Get().RoomTypes.CreaturePen)
		}, "", "", "", "", null, default(EventReference), "Get-a-Room"));
		SurviveOneYear = Add(new ColonyAchievement("SurviveOneYear", "SURVIVE_ONE_YEAR", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.SURVIVE_ONE_YEAR, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.SURVIVE_ONE_YEAR_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new FractionalCycleNumber(365.25f)
		}, "", "", "", "", null, default(EventReference), "One_year"));
		ExploreOilBiome = Add(new ColonyAchievement("ExploreOilBiome", "EXPLORE_OIL_BIOME", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.EXPLORE_OIL_BIOME, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.EXPLORE_OIL_BIOME_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new ExploreOilFieldSubZone()
		}, "", "", "", "", null, default(EventReference), "enter_oil_biome"));
		EatCookedFood = Add(new ColonyAchievement("EatCookedFood", "COOKED_FOOD", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.COOKED_FOOD, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.COOKED_FOOD_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new EatXKCalProducedByY(1, new List<Tag> { "GourmetCookingStation", "CookingStation", "Deepfryer", "Smoker", "SushiBar" })
		}, "", "", "", "", null, default(EventReference), "its_not_raw"));
		BasicPumping = Add(new ColonyAchievement("BasicPumping", "BASIC_PUMPING", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.BASIC_PUMPING, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.BASIC_PUMPING_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new VentXKG(SimHashes.Oxygen, 1000f)
		}, "", "", "", "", null, default(EventReference), "BasicPumping"));
		BasicComforts = Add(new ColonyAchievement("BasicComforts", "BASIC_COMFORTS", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.BASIC_COMFORTS, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.BASIC_COMFORTS_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new AtLeastOneBuildingForEachDupe(new List<Tag> { "FlushToilet", "Outhouse" }),
			new AtLeastOneBuildingForEachDupe(new List<Tag> { "Bed", "LuxuryBed" })
		}, "", "", "", "", null, default(EventReference), "1bed_1toilet"));
		PlumbedWashrooms = Add(new ColonyAchievement("PlumbedWashrooms", "PLUMBED_WASHROOMS", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.PLUMBED_WASHROOMS, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.PLUMBED_WASHROOMS_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new UpgradeAllBasicBuildings("Outhouse", "FlushToilet"),
			new UpgradeAllBasicBuildings("WashBasin", "WashSink")
		}, "", "", "", "", null, default(EventReference), "royal_flush"));
		AutomateABuilding = Add(new ColonyAchievement("AutomateABuilding", "AUTOMATE_A_BUILDING", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.AUTOMATE_A_BUILDING, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.AUTOMATE_A_BUILDING_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new AutomateABuilding()
		}, "", "", "", "", null, default(EventReference), "red_light_green_light"));
		MasterpiecePainting = Add(new ColonyAchievement("MasterpiecePainting", "MASTERPIECE_PAINTING", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.MASTERPIECE_PAINTING, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.MASTERPIECE_PAINTING_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new CreateMasterPainting()
		}, "", "", "", "", null, default(EventReference), "art_underground"));
		InspectPOI = Add(new ColonyAchievement("InspectPOI", "INSPECT_POI", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.INSPECT_POI, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.INSPECT_POI_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new ActivateLorePOI()
		}, "", "", "", "", null, default(EventReference), "ghosts_of_gravitas"));
		HatchACritter = Add(new ColonyAchievement("HatchACritter", "HATCH_A_CRITTER", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.HATCH_A_CRITTER, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.HATCH_A_CRITTER_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new CritterTypeExists(new List<Tag>
			{
				"DreckoPlasticBaby", "HatchHardBaby", "HatchMetalBaby", "HatchVeggieBaby", "LightBugBlackBaby", "LightBugBlueBaby", "LightBugCrystalBaby", "LightBugOrangeBaby", "LightBugPinkBaby", "LightBugPurpleBaby",
				"OilfloaterDecorBaby", "OilfloaterHighTempBaby", "PacuCleanerBaby", "PacuTropicalBaby", "PuftBleachstoneBaby", "PuftOxyliteBaby", "SquirrelHugBaby", "CrabWoodBaby", "CrabFreshWaterBaby", "MoleDelicacyBaby",
				"GlassDeerBaby", "AlgaeStegoBaby", "SnailIronBaby"
			})
		}, "", "", "", "", null, default(EventReference), "good_egg"));
		CuredDisease = Add(new ColonyAchievement("CuredDisease", "CURED_DISEASE", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.CURED_DISEASE, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.CURED_DISEASE_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new CureDisease()
		}, "", "", "", "", null, default(EventReference), "medic"));
		GeneratorTuneup = Add(new ColonyAchievement("GeneratorTuneup", "GENERATOR_TUNEUP", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.GENERATOR_TUNEUP, string.Format(COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.GENERATOR_TUNEUP_DESCRIPTION, 100), isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new TuneUpGenerator(100f)
		}, "", "", "", "", null, default(EventReference), "tune_up_for_what"));
		ClearFOW = Add(new ColonyAchievement("ClearFOW", "CLEAR_FOW", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.CLEAR_FOW, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.CLEAR_FOW_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new RevealAsteriod(0.8f)
		}, "", "", "", "", null, default(EventReference), "pulling_back_the_veil"));
		HatchRefinement = Add(new ColonyAchievement("HatchRefinement", "HATCH_REFINEMENT", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.HATCH_REFINEMENT, string.Format(COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.HATCH_REFINEMENT_DESCRIPTION, GameUtil.GetFormattedMass(10000f, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.Tonne)), isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new CreaturePoopKGProduction("HatchMetal", 10000f)
		}, "", "", "", "", null, default(EventReference), "down_the_hatch"));
		BunkerDoorDefense = Add(new ColonyAchievement("BunkerDoorDefense", "BUNKER_DOOR_DEFENSE", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.BUNKER_DOOR_DEFENSE, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.BUNKER_DOOR_DEFENSE_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new BlockedCometWithBunkerDoor()
		}, "", "", "", "", null, default(EventReference), "Immovable_Object"));
		IdleDuplicants = Add(new ColonyAchievement("IdleDuplicants", "IDLE_DUPLICANTS", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.IDLE_DUPLICANTS, COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.IDLE_DUPLICANTS_DESCRIPTION, isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new DupesVsSolidTransferArmFetch(1f, 5)
		}, "", "", "", "", null, default(EventReference), "easy_livin"));
		ExosuitCycles = Add(new ColonyAchievement("ExosuitCycles", "EXOSUIT_CYCLES", COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.EXOSUIT_CYCLES, string.Format(COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.EXOSUIT_CYCLES_DESCRIPTION, 10), isVictoryCondition: false, new List<ColonyAchievementRequirement>
		{
			new DupesCompleteChoreInExoSuitForCycles(10)
		}, "", "", "", "", null, default(EventReference), "job_suitability"));
		if (DlcManager.IsExpansion1Active())
		{
			string name = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.FIRST_TELEPORT;
			string description = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.FIRST_TELEPORT_DESCRIPTION;
			List<ColonyAchievementRequirement> requirementChecklist = new List<ColonyAchievementRequirement>
			{
				new TeleportDuplicant(),
				new DefrostDuplicant()
			};
			string[] eXPANSION = DlcManager.EXPANSION1;
			FirstTeleport = Add(new ColonyAchievement("FirstTeleport", "FIRST_TELEPORT", name, description, isVictoryCondition: false, requirementChecklist, "", "", "", "", null, default(EventReference), "first_teleport_of_call", eXPANSION, null, "EXPANSION1_ID"));
			string name2 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.SOFT_LAUNCH;
			string description2 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.SOFT_LAUNCH_DESCRIPTION;
			List<ColonyAchievementRequirement> requirementChecklist2 = new List<ColonyAchievementRequirement>
			{
				new BuildALaunchPad()
			};
			eXPANSION = DlcManager.EXPANSION1;
			SoftLaunch = Add(new ColonyAchievement("SoftLaunch", "SOFT_LAUNCH", name2, description2, isVictoryCondition: false, requirementChecklist2, "", "", "", "", null, default(EventReference), "soft_launch", eXPANSION, null, "EXPANSION1_ID"));
			string name3 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.GMO_OK;
			string description3 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.GMO_OK_DESCRIPTION;
			List<ColonyAchievementRequirement> requirementChecklist3 = new List<ColonyAchievementRequirement>
			{
				new AnalyzeSeed(BasicFabricMaterialPlantConfig.ID),
				new AnalyzeSeed("BasicSingleHarvestPlant"),
				new AnalyzeSeed("GasGrass"),
				new AnalyzeSeed("MushroomPlant"),
				new AnalyzeSeed("PrickleFlower"),
				new AnalyzeSeed("SaltPlant"),
				new AnalyzeSeed(SeaLettuceConfig.ID),
				new AnalyzeSeed("SpiceVine"),
				new AnalyzeSeed("SwampHarvestPlant"),
				new AnalyzeSeed(SwampLilyConfig.ID),
				new AnalyzeSeed("WormPlant"),
				new AnalyzeSeed("ColdWheat"),
				new AnalyzeSeed("BeanPlant")
			};
			eXPANSION = DlcManager.EXPANSION1;
			GMOOK = Add(new ColonyAchievement("GMOOK", "GMO_OK", name3, description3, isVictoryCondition: false, requirementChecklist3, "", "", "", "", null, default(EventReference), "gmo_ok", eXPANSION, null, "EXPANSION1_ID"));
			string name4 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.MINE_THE_GAP;
			string description4 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.MINE_THE_GAP_DESCRIPTION;
			List<ColonyAchievementRequirement> requirementChecklist4 = new List<ColonyAchievementRequirement>
			{
				new HarvestAmountFromSpacePOI(1000000f)
			};
			eXPANSION = DlcManager.EXPANSION1;
			MineTheGap = Add(new ColonyAchievement("MineTheGap", "MINE_THE_GAP", name4, description4, isVictoryCondition: false, requirementChecklist4, "", "", "", "", null, default(EventReference), "mine_the_gap", eXPANSION, null, "EXPANSION1_ID"));
			string name5 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.LAND_ON_ALL_WORLDS;
			string description5 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.LAND_ON_ALL_WORLDS_DESCRIPTION;
			List<ColonyAchievementRequirement> requirementChecklist5 = new List<ColonyAchievementRequirement>
			{
				new LandOnAllWorlds()
			};
			eXPANSION = DlcManager.EXPANSION1;
			LandedOnAllWorlds = Add(new ColonyAchievement("LandedOnAllWorlds", "LANDED_ON_ALL_WORLDS", name5, description5, isVictoryCondition: false, requirementChecklist5, "", "", "", "", null, default(EventReference), "land_on_all_worlds", eXPANSION, null, "EXPANSION1_ID"));
			string name6 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.RADICAL_TRIP;
			string description6 = string.Format(COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.RADICAL_TRIP_DESCRIPTION, 10);
			List<ColonyAchievementRequirement> requirementChecklist6 = new List<ColonyAchievementRequirement>
			{
				new RadBoltTravelDistance(10000)
			};
			eXPANSION = DlcManager.EXPANSION1;
			RadicalTrip = Add(new ColonyAchievement("RadicalTrip", "RADICAL_TRIP", name6, description6, isVictoryCondition: false, requirementChecklist6, "", "", "", "", null, default(EventReference), "radical_trip", eXPANSION, null, "EXPANSION1_ID"));
			string name7 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.SWEETER_THAN_HONEY;
			string description7 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.SWEETER_THAN_HONEY_DESCRIPTION;
			List<ColonyAchievementRequirement> requirementChecklist7 = new List<ColonyAchievementRequirement>
			{
				new HarvestAHiveWithoutBeingStung()
			};
			eXPANSION = DlcManager.EXPANSION1;
			SweeterThanHoney = Add(new ColonyAchievement("SweeterThanHoney", "SWEETER_THAN_HONEY", name7, description7, isVictoryCondition: false, requirementChecklist7, "", "", "", "", null, default(EventReference), "sweeter_than_honey", eXPANSION, null, "EXPANSION1_ID"));
			string name8 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.SURVIVE_IN_A_ROCKET;
			string description8 = string.Format(COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.SURVIVE_IN_A_ROCKET_DESCRIPTION, 10, 25);
			List<ColonyAchievementRequirement> requirementChecklist8 = new List<ColonyAchievementRequirement>
			{
				new SurviveARocketWithMinimumMorale(25f, 10)
			};
			eXPANSION = DlcManager.EXPANSION1;
			SurviveInARocket = Add(new ColonyAchievement("SurviveInARocket", "SURVIVE_IN_A_ROCKET", name8, description8, isVictoryCondition: false, requirementChecklist8, "", "", "", "", null, default(EventReference), "survive_a_rocket", eXPANSION, null, "EXPANSION1_ID"));
			string name9 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.REACTOR_USAGE;
			string description9 = string.Format(COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.REACTOR_USAGE_DESCRIPTION, 5);
			List<ColonyAchievementRequirement> requirementChecklist9 = new List<ColonyAchievementRequirement>
			{
				new RunReactorForXDays(5)
			};
			eXPANSION = DlcManager.EXPANSION1;
			RunAReactor = Add(new ColonyAchievement("RunAReactor", "REACTOR_USAGE", name9, description9, isVictoryCondition: false, requirementChecklist9, "", "", "", "", null, default(EventReference), "thats_rad", eXPANSION, null, "EXPANSION1_ID"));
		}
		if (DlcManager.IsContentSubscribed("DLC3_ID"))
		{
			string name10 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.DATA_DRIVEN;
			string description10 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.DATA_DRIVEN_DESCRIPTION;
			List<ColonyAchievementRequirement> requirementChecklist10 = new List<ColonyAchievementRequirement>
			{
				new EfficientDataMiningCheck()
			};
			string[] eXPANSION = DlcManager.DLC3;
			EfficientData = Add(new ColonyAchievement("EfficientData", "EFFICIENT_DATAMINING", name10, description10, isVictoryCondition: false, requirementChecklist10, "", "", "", "", null, default(EventReference), "efficient_data_mining", eXPANSION, null, "DLC3_ID"));
			string name11 = COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.MVB;
			string description11 = string.Format(COLONY_ACHIEVEMENTS.MISC_REQUIREMENTS.MVB_DESCRIPTION, 8);
			List<ColonyAchievementRequirement> requirementChecklist11 = new List<ColonyAchievementRequirement>
			{
				new AllTheCircuitsCompleteCheck()
			};
			eXPANSION = DlcManager.DLC3;
			AllTheCircuits = Add(new ColonyAchievement("AllTheCircuits", "ALL_THE_CIRCUITS", name11, description11, isVictoryCondition: false, requirementChecklist11, "", "", "", "", null, default(EventReference), "all_the_circuits", eXPANSION, null, "DLC3_ID"));
		}
		if (DlcManager.IsContentSubscribed("DLC4_ID"))
		{
			AsteroidDestroyed = Add(new ColonyAchievement("AsteroidDestroyed", "ASTEROID_DESTROYED", COLONY_ACHIEVEMENTS.ASTEROID_DESTROYED.NAME, COLONY_ACHIEVEMENTS.ASTEROID_DESTROYED.DESCRIPTION, isVictoryCondition: true, new List<ColonyAchievementRequirement>
			{
				new DefeatPrehistoricAsteroid()
			}, COLONY_ACHIEVEMENTS.ASTEROID_DESTROYED.MESSAGE_TITLE, COLONY_ACHIEVEMENTS.ASTEROID_DESTROYED.MESSAGE_BODY, "DLC4/LargeImpactorDefeatedVideo", "DLC4/LargeImpactorSpacePOIVideo", delegate
			{
				LargeImpactorDestroyedSequence.Start();
			}, AudioMixerSnapshots.Get().VictoryNISGenericSnapshot, "blast_line_of_defense", DlcManager.DLC4, null, "DLC4_ID", "DemoliorImperative"));
			string name12 = COLONY_ACHIEVEMENTS.ASTEROID_SURVIVED.NAME;
			string description12 = COLONY_ACHIEVEMENTS.ASTEROID_SURVIVED.DESCRIPTION;
			List<ColonyAchievementRequirement> requirementChecklist12 = new List<ColonyAchievementRequirement>
			{
				new SurvivedPrehistoricAsteroidImpact(100),
				new NoDuplicantsCanDie()
			};
			string[] eXPANSION = DlcManager.DLC4;
			AsteroidSurvived = Add(new ColonyAchievement("AsteroidSurvived", "ASTEROID_SURVIVED", name12, description12, isVictoryCondition: false, requirementChecklist12, "", "", "", "", null, default(EventReference), "life_found_a_way", eXPANSION, null, "DLC4_ID", "DemoliorSurivedAchievement"));
		}
		if (DlcManager.IsContentSubscribed("DLC5_ID"))
		{
			MinnowRecruited = Add(new ColonyAchievement("MinnowRecruited", "MINNOW_RECRUITED", COLONY_ACHIEVEMENTS.FINDING_MINNOW.NAME, COLONY_ACHIEVEMENTS.FINDING_MINNOW.DESCRIPTION, isVictoryCondition: true, new List<ColonyAchievementRequirement>
			{
				new MinnowRecruited(MinnowImperativePOIStates.MinnowPOIIdentity.POI_A),
				new MinnowRecruited(MinnowImperativePOIStates.MinnowPOIIdentity.POI_B),
				new MinnowRecruited(MinnowImperativePOIStates.MinnowPOIIdentity.POI_C)
			}, COLONY_ACHIEVEMENTS.FINDING_MINNOW.MESSAGE_TITLE, COLONY_ACHIEVEMENTS.FINDING_MINNOW.MESSAGE_BODY, "victoryShorts/Stay", "victoryLoops/Stay_loop", FindingMinnowCompleteSequence.Start, AudioMixerSnapshots.Get().VictoryNISGenericSnapshot, "minnow_recruited", DlcManager.DLC5, null, "DLC5_ID", "MinnowRecruitedAchievement"));
		}
	}
}
