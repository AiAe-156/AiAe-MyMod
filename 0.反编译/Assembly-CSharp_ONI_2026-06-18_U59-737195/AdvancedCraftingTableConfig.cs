using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class AdvancedCraftingTableConfig : IBuildingConfig
{
	public const string ID = "AdvancedCraftingTable";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC3;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("AdvancedCraftingTable", 3, 3, "advanced_crafting_table_kanim", 100, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.RAW_METALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER3, decor: TUNING.BUILDINGS.DECOR.NONE);
		obj.RequiresPowerInput = true;
		obj.EnergyConsumptionWhenActive = 480f;
		obj.ViewMode = OverlayModes.Power.ID;
		obj.AudioCategory = "Metal";
		obj.PowerInputOffset = new CellOffset(0, 0);
		obj.RequiredSkillPerkID = Db.Get().SkillPerks.CanCraftElectronics.Id;
		obj.AddSearchTerms(SEARCH_TERMS.ROBOT);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<DropAllWorkable>();
		go.AddOrGet<Prioritizable>();
		ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
		complexFabricator.heatedTemperature = 318.15f;
		complexFabricator.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		go.AddOrGet<ComplexFabricatorWorkable>().overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_advanced_crafting_table_kanim") };
		Prioritizable.AddRef(go);
		BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);
		ConfigureRecipes();
	}

	private void ConfigureRecipes()
	{
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(SimHashes.Katairite.CreateTag(), 200f, inheritElement: true)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("EmptyElectrobank".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		ElectrobankConfig.recipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("AdvancedCraftingTable", array, array2), array, array2)
		{
			time = INDUSTRIAL.RECIPES.STANDARD_FABRICATION_TIME * 2f,
			description = string.Format(STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.GENERIC_RECIPE_DESCRIPTION, ElementLoader.FindElementByHash(SimHashes.Katairite).name, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.ELECTROBANK.NAME),
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Custom,
			customName = STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.ELECTROBANK.NAME,
			customSpritePrefabID = "Electrobank",
			fabricators = new List<Tag> { "AdvancedCraftingTable" },
			requiredTech = Db.Get().TechItems.electrobank.parentTechId,
			sortOrder = 0
		};
		ComplexRecipe.RecipeElement[] array3 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(SimHashes.Polypropylene.CreateTag(), 200f, inheritElement: true)
		};
		ComplexRecipe.RecipeElement[] array4 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("FetchDrone".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("AdvancedCraftingTable", array3, array4), array3, array4)
		{
			time = INDUSTRIAL.RECIPES.STANDARD_FABRICATION_TIME * 4f,
			description = string.Format(STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.GENERIC_RECIPE_DESCRIPTION, ElementLoader.FindElementByHash(SimHashes.Polypropylene).name, STRINGS.ROBOTS.MODELS.FLYDO.NAME),
			nameDisplay = ComplexRecipe.RecipeNameDisplay.ResultWithIngredient,
			fabricators = new List<Tag> { "AdvancedCraftingTable" },
			requiredTech = Db.Get().TechItems.fetchDrone.parentTechId,
			sortOrder = 1
		};
		ComplexRecipe.RecipeElement[] array5 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(SimHashes.HardPolypropylene.CreateTag(), 200f, inheritElement: true)
		};
		ComplexRecipe.RecipeElement[] array6 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("FetchDrone".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("AdvancedCraftingTable", array5, array6), array5, array6)
		{
			time = INDUSTRIAL.RECIPES.STANDARD_FABRICATION_TIME * 4f,
			description = string.Format(STRINGS.BUILDINGS.PREFABS.ADVANCEDCRAFTINGTABLE.GENERIC_RECIPE_DESCRIPTION, ElementLoader.FindElementByHash(SimHashes.HardPolypropylene).name, STRINGS.ROBOTS.MODELS.FLYDO.NAME),
			nameDisplay = ComplexRecipe.RecipeNameDisplay.ResultWithIngredient,
			fabricators = new List<Tag> { "AdvancedCraftingTable" },
			requiredTech = Db.Get().TechItems.fetchDrone.parentTechId,
			sortOrder = 2
		};
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<KPrefabID>().prefabSpawnFn += delegate(GameObject game_object)
		{
			ComplexFabricatorWorkable component = game_object.GetComponent<ComplexFabricatorWorkable>();
			component.WorkerStatusItem = Db.Get().DuplicantStatusItems.Fabricating;
			component.AttributeConverter = Db.Get().AttributeConverters.MachinerySpeed;
			component.AttributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
			component.SkillExperienceSkillGroup = Db.Get().SkillGroups.Technicals.Id;
			component.SkillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
			component.requiredSkillPerk = Db.Get().SkillPerks.CanCraftElectronics.Id;
		};
	}
}
