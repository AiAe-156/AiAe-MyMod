using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class MicrobeMusherConfig : IBuildingConfig
{
	public const string ID = "MicrobeMusher";

	public static EffectorValues DECOR = TUNING.BUILDINGS.DECOR.PENALTY.TIER2;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("MicrobeMusher", 2, 3, "microbemusher_kanim", 30, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER3, decor: DECOR);
		buildingDef.RequiresPowerInput = true;
		buildingDef.EnergyConsumptionWhenActive = 240f;
		buildingDef.ExhaustKilowattsWhenActive = 0.5f;
		buildingDef.SelfHeatKilowattsWhenActive = 2f;
		buildingDef.ViewMode = OverlayModes.Power.ID;
		buildingDef.AudioCategory = "Glass";
		buildingDef.AudioSize = "large";
		buildingDef.AddSearchTerms(SEARCH_TERMS.FOOD);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<DropAllWorkable>();
		Prioritizable.AddRef(go);
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
		conduitConsumer.conduitType = ConduitType.Liquid;
		MicrobeMusher microbeMusher = go.AddOrGet<MicrobeMusher>();
		microbeMusher.mushbarSpawnOffset = new Vector3(1f, 0f, 0f);
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		go.AddOrGet<ComplexFabricatorWorkable>().overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_musher_kanim") };
		microbeMusher.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		BuildingTemplates.CreateComplexFabricatorStorage(go, microbeMusher);
		ConfigureRecipes();
		go.AddOrGetDef<PoweredController.Def>();
	}

	private void ConfigureRecipes()
	{
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement("Dirt".ToTag(), 75f),
			new ComplexRecipe.RecipeElement(new Tag[2]
			{
				SimHashes.Water.CreateTag(),
				SimHashes.Mucus.CreateTag()
			}, 75f)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("MushBar".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id = ComplexRecipeManager.MakeRecipeID("MicrobeMusher", array, array2);
		MushBarConfig.recipe = new ComplexRecipe(id, array, array2)
		{
			time = 40f,
			description = STRINGS.ITEMS.FOOD.MUSHBAR.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "MicrobeMusher" },
			sortOrder = 1
		};
		MushBarConfig.recipe.SetFabricationAnim("mushbar_kanim");
		ComplexRecipe.RecipeElement[] array3 = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement("BasicPlantFood", 2f),
			new ComplexRecipe.RecipeElement(new Tag[2]
			{
				SimHashes.Water.CreateTag(),
				SimHashes.Mucus.CreateTag()
			}, 50f)
		};
		ComplexRecipe.RecipeElement[] array4 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("BasicPlantBar".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id2 = ComplexRecipeManager.MakeRecipeID("MicrobeMusher", array3, array4);
		BasicPlantBarConfig.recipe = new ComplexRecipe(id2, array3, array4)
		{
			time = FOOD.RECIPES.STANDARD_COOK_TIME,
			description = STRINGS.ITEMS.FOOD.BASICPLANTBAR.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "MicrobeMusher" },
			sortOrder = 2
		};
		BasicPlantBarConfig.recipe.SetFabricationAnim("liceloaf_kanim");
		ComplexRecipe.RecipeElement[] array5 = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement("BeanPlantSeed", 6f),
			new ComplexRecipe.RecipeElement("Water".ToTag(), 50f)
		};
		ComplexRecipe.RecipeElement[] array6 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("Tofu".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id3 = ComplexRecipeManager.MakeRecipeID("MicrobeMusher", array5, array6);
		TofuConfig.recipe = new ComplexRecipe(id3, array5, array6)
		{
			time = FOOD.RECIPES.STANDARD_COOK_TIME,
			description = STRINGS.ITEMS.FOOD.TOFU.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "MicrobeMusher" },
			sortOrder = 3
		};
		TofuConfig.recipe.SetFabricationAnim("loafu_kanim");
		ComplexRecipe.RecipeElement[] array7 = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement(new Tag[2]
			{
				"ColdWheatSeed",
				FernFoodConfig.ID
			}, 5f),
			new ComplexRecipe.RecipeElement(new Tag[2]
			{
				PrickleFruitConfig.ID,
				"HardSkinBerry"
			}, new float[2] { 1f, 2f })
		};
		ComplexRecipe.RecipeElement[] array8 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("FruitCake".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id4 = ComplexRecipeManager.MakeRecipeID("MicrobeMusher", array7, array8);
		FruitCakeConfig.recipe = new ComplexRecipe(id4, array7, array8)
		{
			time = FOOD.RECIPES.STANDARD_COOK_TIME,
			description = STRINGS.ITEMS.FOOD.FRUITCAKE.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "MicrobeMusher" },
			sortOrder = 3
		};
		FruitCakeConfig.recipe.SetFabricationAnim("fruitcake_kanim");
		if (DlcManager.IsContentSubscribed("DLC2_ID"))
		{
			ComplexRecipe.RecipeElement[] array9 = new ComplexRecipe.RecipeElement[2]
			{
				new ComplexRecipe.RecipeElement("Meat", 1f),
				new ComplexRecipe.RecipeElement("Tallow", 1f)
			};
			ComplexRecipe.RecipeElement[] array10 = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement("Pemmican".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
			};
			string id5 = ComplexRecipeManager.MakeRecipeID("MicrobeMusher", array9, array10);
			PemmicanConfig.recipe = new ComplexRecipe(id5, array9, array10, DlcManager.DLC2)
			{
				time = FOOD.RECIPES.STANDARD_COOK_TIME,
				description = STRINGS.ITEMS.FOOD.PEMMICAN.RECIPEDESC,
				nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
				fabricators = new List<Tag> { "MicrobeMusher" },
				sortOrder = 4
			};
			PemmicanConfig.recipe.SetFabricationAnim("pemmican_kanim");
		}
		ComplexRecipe.RecipeElement[] array11 = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement(new Tag[6] { "BasicSingleHarvestPlantSeed", "PrickleFlowerSeed", "MushroomSeed", "SeaLettuceSeed", "BeanPlantSeed", "ColdWheatSeed" }, 1f),
			new ComplexRecipe.RecipeElement("Water".ToTag(), 5f)
		};
		ComplexRecipe.RecipeElement[] array12 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("FishFood".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id6 = ComplexRecipeManager.MakeRecipeID("MicrobeMusher", array11, array12);
		FishFoodConfig.recipe = new ComplexRecipe(id6, array11, array12)
		{
			time = FOOD.RECIPES.STANDARD_COOK_TIME,
			description = STRINGS.ITEMS.FOOD.FISHFOOD.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "MicrobeMusher" },
			sortOrder = 5
		};
		FishFoodConfig.recipe.SetFabricationAnim("fishfood_kanim");
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
