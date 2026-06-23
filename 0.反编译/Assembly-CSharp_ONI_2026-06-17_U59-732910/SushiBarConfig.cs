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
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("SushiBar", 4, 2, "sushi_station_kanim", 30, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.WOODS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER3, decor: TUNING.BUILDINGS.DECOR.NONE);
		BuildingTemplates.CreateElectricalBuildingDef(obj);
		obj.AudioCategory = "Metal";
		obj.AudioSize = "large";
		obj.RequiresPowerInput = false;
		obj.EnergyConsumptionWhenActive = 0f;
		obj.ExhaustKilowattsWhenActive = 0f;
		obj.SelfHeatKilowattsWhenActive = 0.5f;
		obj.RequiredSkillPerkID = Db.Get().SkillPerks.CanSushiBar.Id;
		obj.AddSearchTerms(SEARCH_TERMS.FOOD);
		return obj;
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
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.CookTop);
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
		EdamameConfig.recipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("SushiBar", array, array2), array, array2)
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
		MakiConfig.recipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("SushiBar", array3, array4), array3, array4)
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
		UrchinMeatConfig.recipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("SushiBar", array5, array6), array5, array6)
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
		NigiriConfig.recipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("SushiBar", array7, array8), array7, array8)
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
