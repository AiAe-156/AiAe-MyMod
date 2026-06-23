using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class SushiBarConfig : IBuildingConfig
{
	public const string ID = "SushiBar";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("SushiBar", 4, 2, "sushi_station_kanim", 30, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.WOODS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER3, decor: TUNING.BUILDINGS.DECOR.NONE);
		BuildingTemplates.CreateElectricalBuildingDef(buildingDef);
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "large";
		buildingDef.RequiresPowerInput = false;
		buildingDef.EnergyConsumptionWhenActive = 0f;
		buildingDef.ExhaustKilowattsWhenActive = 0f;
		buildingDef.SelfHeatKilowattsWhenActive = 0.5f;
		buildingDef.RequiredSkillPerkID = Db.Get().SkillPerks.CanSushiBar.Id;
		buildingDef.AddSearchTerms(SEARCH_TERMS.FOOD);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		SushiBar sushiBar = go.AddOrGet<SushiBar>();
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		ComplexFabricatorLayeredWorkable complexFabricatorLayeredWorkable = go.AddOrGet<ComplexFabricatorLayeredWorkable>();
		complexFabricatorLayeredWorkable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_cookstation_kanim") };
		complexFabricatorLayeredWorkable.foregroundLayer = Grid.SceneLayer.TransferArm;
		complexFabricatorLayeredWorkable.synchronizeAnims = true;
		sushiBar.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		Prioritizable.AddRef(go);
		go.AddOrGet<DropAllWorkable>();
		ConfigureRecipes();
		BuildingTemplates.CreateComplexFabricatorStorage(go, sushiBar);
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(RoomConstraints.ConstraintTags.CookTop);
	}

	private void ConfigureRecipes()
	{
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement("BeanPlantSeed", 1f),
			new ComplexRecipe.RecipeElement("SaltySticksFood", 1f)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("Edamame", 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id = ComplexRecipeManager.MakeRecipeID("SushiBar", array, array2);
		EdamameConfig.recipe = new ComplexRecipe(id, array, array2)
		{
			time = FOOD.RECIPES.STANDARD_COOK_TIME,
			description = STRINGS.ITEMS.FOOD.EDAMAME.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "SushiBar" },
			sortOrder = 1
		};
		ComplexRecipe.RecipeElement[] array3 = new ComplexRecipe.RecipeElement[3]
		{
			new ComplexRecipe.RecipeElement("BasicPlantBar", 1f),
			new ComplexRecipe.RecipeElement("Nori", 1f),
			new ComplexRecipe.RecipeElement("FishMeat", 1f)
		};
		ComplexRecipe.RecipeElement[] array4 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("Maki", 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id2 = ComplexRecipeManager.MakeRecipeID("SushiBar", array3, array4);
		MakiConfig.recipe = new ComplexRecipe(id2, array3, array4)
		{
			time = FOOD.RECIPES.STANDARD_COOK_TIME,
			description = STRINGS.ITEMS.FOOD.MAKI.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "SushiBar" },
			sortOrder = 2
		};
		ComplexRecipe.RecipeElement[] array5 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("Urchin", 1f)
		};
		ComplexRecipe.RecipeElement[] array6 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(UrchinMeatConfig.ID, 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id3 = ComplexRecipeManager.MakeRecipeID("SushiBar", array5, array6);
		UrchinMeatConfig.recipe = new ComplexRecipe(id3, array5, array6)
		{
			time = FOOD.RECIPES.STANDARD_COOK_TIME,
			description = STRINGS.ITEMS.FOOD.URCHINMEAT.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "SushiBar" },
			sortOrder = 5
		};
		ComplexRecipe.RecipeElement[] array7 = new ComplexRecipe.RecipeElement[3]
		{
			new ComplexRecipe.RecipeElement("BasicPlantBar", 1f),
			new ComplexRecipe.RecipeElement("Nori", 1f),
			new ComplexRecipe.RecipeElement("SquidMeat", 1f)
		};
		ComplexRecipe.RecipeElement[] array8 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("Nigiri", 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id4 = ComplexRecipeManager.MakeRecipeID("SushiBar", array7, array8);
		NigiriConfig.recipe = new ComplexRecipe(id4, array7, array8)
		{
			time = FOOD.RECIPES.STANDARD_COOK_TIME,
			description = STRINGS.ITEMS.FOOD.NIGIRI.RECIPEDESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "SushiBar" },
			sortOrder = 3
		};
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
