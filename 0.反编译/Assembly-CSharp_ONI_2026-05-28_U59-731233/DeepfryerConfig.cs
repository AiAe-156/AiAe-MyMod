using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class DeepfryerConfig : IBuildingConfig
{
	public const string ID = "Deepfryer";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC2;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("Deepfryer", 2, 2, "deepfryer_kanim", 30, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER3, decor: TUNING.BUILDINGS.DECOR.NONE);
		buildingDef.ViewMode = OverlayModes.Rooms.ID;
		BuildingTemplates.CreateElectricalBuildingDef(buildingDef);
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "large";
		buildingDef.EnergyConsumptionWhenActive = 480f;
		buildingDef.ExhaustKilowattsWhenActive = 2f;
		buildingDef.SelfHeatKilowattsWhenActive = 8f;
		buildingDef.RequiredSkillPerkID = Db.Get().SkillPerks.CanDeepFry.Id;
		buildingDef.AddSearchTerms(SEARCH_TERMS.FOOD);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		Deepfryer deepfryer = go.AddOrGet<Deepfryer>();
		deepfryer.heatedTemperature = 368.15f;
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		RoomTracker roomTracker = go.AddOrGet<RoomTracker>();
		roomTracker.requiredRoomType = Db.Get().RoomTypes.Kitchen.Id;
		roomTracker.requirement = RoomTracker.Requirement.Required;
		go.AddOrGet<ComplexFabricatorWorkable>().overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_deepfryer_kanim") };
		deepfryer.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		Prioritizable.AddRef(go);
		go.AddOrGet<DropAllWorkable>();
		ConfigureRecipes();
		go.AddOrGetDef<PoweredController.Def>();
		BuildingTemplates.CreateComplexFabricatorStorage(go, deepfryer);
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(RoomConstraints.ConstraintTags.CookTop);
	}

	private void ConfigureRecipes()
	{
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement(CarrotConfig.ID, 1f),
			new ComplexRecipe.RecipeElement("Tallow", 1f)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("FriesCarrot", 1f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated)
		};
		string id = ComplexRecipeManager.MakeRecipeID("Deepfryer", array, array2);
		FriesCarrotConfig.recipe = new ComplexRecipe(id, array, array2, GetRequiredDlcIds())
		{
			time = FOOD.RECIPES.STANDARD_COOK_TIME,
			description = STRINGS.ITEMS.FOOD.FRIESCARROT.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "Deepfryer" },
			sortOrder = 100
		};
		ComplexRecipe.RecipeElement[] array3 = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement("BeanPlantSeed", 6f),
			new ComplexRecipe.RecipeElement("Tallow", 1f)
		};
		ComplexRecipe.RecipeElement[] array4 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("DeepFriedNosh", 1f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated)
		};
		string id2 = ComplexRecipeManager.MakeRecipeID("Deepfryer", array3, array4);
		new ComplexRecipe(id2, array3, array4, GetRequiredDlcIds())
		{
			time = FOOD.RECIPES.STANDARD_COOK_TIME,
			description = STRINGS.ITEMS.FOOD.DEEPFRIEDNOSH.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "Deepfryer" },
			sortOrder = 200
		};
		ComplexRecipe.RecipeElement[] array5 = new ComplexRecipe.RecipeElement[3]
		{
			new ComplexRecipe.RecipeElement("FishMeat", 1f),
			new ComplexRecipe.RecipeElement("Tallow", 2.4f),
			new ComplexRecipe.RecipeElement(new Tag[2]
			{
				"ColdWheatSeed",
				FernFoodConfig.ID
			}, 2f)
		};
		ComplexRecipe.RecipeElement[] array6 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("DeepFriedFish", 1f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated)
		};
		string id3 = ComplexRecipeManager.MakeRecipeID("Deepfryer", array5, array6);
		DeepFriedFishConfig.recipe = new ComplexRecipe(id3, array5, array6, GetRequiredDlcIds())
		{
			time = FOOD.RECIPES.STANDARD_COOK_TIME,
			description = STRINGS.ITEMS.FOOD.DEEPFRIEDFISH.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "Deepfryer" },
			sortOrder = 300
		};
		ComplexRecipe.RecipeElement[] array7 = new ComplexRecipe.RecipeElement[3]
		{
			new ComplexRecipe.RecipeElement("ShellfishMeat", 1f),
			new ComplexRecipe.RecipeElement("Tallow", 2.4f),
			new ComplexRecipe.RecipeElement(new Tag[2]
			{
				"ColdWheatSeed",
				FernFoodConfig.ID
			}, 2f)
		};
		ComplexRecipe.RecipeElement[] array8 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("DeepFriedShellfish", 1f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated)
		};
		string id4 = ComplexRecipeManager.MakeRecipeID("Deepfryer", array7, array8);
		DeepFriedShellfishConfig.recipe = new ComplexRecipe(id4, array7, array8, GetRequiredDlcIds())
		{
			time = FOOD.RECIPES.STANDARD_COOK_TIME,
			description = STRINGS.ITEMS.FOOD.DEEPFRIEDSHELLFISH.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "Deepfryer" },
			sortOrder = 300
		};
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
