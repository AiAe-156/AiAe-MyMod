using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class SmokerConfig : IBuildingConfig
{
	public const string ID = "Smoker";

	private const float FUEL_CONSUME_RATE = 0.2f;

	private const float CO2_EMIT_RATE = 0.02f;

	public const float EMPTYING_WORK_TIME = 50f;

	private static readonly List<Storage.StoredItemModifier> GourmetCookingStationStoredItemModifiers = new List<Storage.StoredItemModifier>
	{
		Storage.StoredItemModifier.Hide,
		Storage.StoredItemModifier.Preserve,
		Storage.StoredItemModifier.Insulate,
		Storage.StoredItemModifier.Seal
	};

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC4;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("Smoker", 4, 3, "smoker_kanim", 30, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER3, decor: TUNING.BUILDINGS.DECOR.NONE);
		obj.AudioCategory = "Metal";
		obj.AudioSize = "large";
		obj.ExhaustKilowattsWhenActive = 1f;
		obj.SelfHeatKilowattsWhenActive = 8f;
		obj.OutputConduitType = ConduitType.Gas;
		obj.UtilityOutputOffset = new CellOffset(1, 1);
		obj.RequiredSkillPerkID = Db.Get().SkillPerks.CanGasRange.Id;
		obj.AddSearchTerms(SEARCH_TERMS.FOOD);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<DropAllWorkable>();
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
		complexFabricator.heatedTemperature = 368.15f;
		complexFabricator.duplicantOperated = false;
		complexFabricator.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		complexFabricator.fetchChoreTypeIdHash = Db.Get().ChoreTypes.CookFetch.IdHash;
		complexFabricator.showProgressBar = true;
		complexFabricator.storeProduced = true;
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		Storage storage = go.AddComponent<Storage>();
		ManualDeliveryKG manualDeliveryKG = go.AddComponent<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.CookFetch.IdHash;
		manualDeliveryKG.RequestedItemTag = SimHashes.Peat.CreateTag();
		manualDeliveryKG.capacity = 240f;
		manualDeliveryKG.refillMass = 120f;
		ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
		elementConverter.SetStorage(storage);
		elementConverter.outputElements = new ElementConverter.OutputElement[1]
		{
			new ElementConverter.OutputElement(0.02f, SimHashes.CarbonDioxide, 348.15f, useEntityTemperature: false, storeOutput: true, 0f, 2f)
		};
		elementConverter.OperationalRequirement = Operational.State.Active;
		ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
		conduitDispenser.conduitType = ConduitType.Gas;
		conduitDispenser.alwaysDispense = true;
		conduitDispenser.elementFilter = null;
		conduitDispenser.storage = storage;
		BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);
		complexFabricator.inStorage.SetDefaultStoredItemModifiers(GourmetCookingStationStoredItemModifiers);
		complexFabricator.buildStorage.SetDefaultStoredItemModifiers(GourmetCookingStationStoredItemModifiers);
		complexFabricator.outStorage.SetDefaultStoredItemModifiers(GourmetCookingStationStoredItemModifiers);
		ConfigureRecipes();
		Prioritizable.AddRef(go);
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.CookTop);
		go.AddOrGetDef<FoodSmoker.Def>();
		KAnimFile[] overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_smoker_kanim") };
		FoodSmokerWorkableEmpty foodSmokerWorkableEmpty = go.AddOrGet<FoodSmokerWorkableEmpty>();
		foodSmokerWorkableEmpty.workTime = 50f;
		foodSmokerWorkableEmpty.overrideAnims = overrideAnims;
		foodSmokerWorkableEmpty.workLayer = Grid.SceneLayer.Front;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}

	private void ConfigureRecipes()
	{
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement("DinosaurMeat", 6f),
			new ComplexRecipe.RecipeElement(GameTags.BasicWoods.Append(SimHashes.Peat.CreateTag()), 100f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature, "")
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("SmokedDinosaurMeat", 3.2f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated)
		};
		new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("Smoker", array, array2), array, array2)
		{
			time = 600f,
			description = STRINGS.ITEMS.FOOD.SMOKEDDINOSAURMEAT.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "Smoker" },
			sortOrder = 600
		};
		ComplexRecipe.RecipeElement[] array3 = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement(new Tag[2] { "FishMeat", "PrehistoricPacuFillet" }, 6f),
			new ComplexRecipe.RecipeElement(GameTags.BasicWoods.Append(SimHashes.Peat.CreateTag()), 100f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature, "")
		};
		ComplexRecipe.RecipeElement[] array4 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("SmokedFish", 4f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated)
		};
		new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("Smoker", array3, array4), array3, array4)
		{
			time = 600f,
			description = STRINGS.ITEMS.FOOD.SMOKEDFISH.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "Smoker" },
			sortOrder = 600
		};
		ComplexRecipe.RecipeElement[] array5 = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement(new Tag[3] { "GardenFoodPlantFood", "HardSkinBerry", "WormBasicFruit" }, 7f),
			new ComplexRecipe.RecipeElement(GameTags.BasicWoods.Append(SimHashes.Peat.CreateTag()), 100f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature, "")
		};
		ComplexRecipe.RecipeElement[] array6 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("SmokedVegetables", 4f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated)
		};
		new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("Smoker", array5, array6), array5, array6)
		{
			time = 600f,
			description = STRINGS.ITEMS.FOOD.SMOKEDVEGETABLES.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "Smoker" },
			sortOrder = 600
		};
	}
}
