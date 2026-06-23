using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class ClothingFabricatorConfig : IBuildingConfig
{
	public const string ID = "ClothingFabricator";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("ClothingFabricator", 4, 3, "clothingfactory_kanim", 100, 240f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER5, decor: TUNING.BUILDINGS.DECOR.NONE);
		obj.RequiresPowerInput = true;
		obj.EnergyConsumptionWhenActive = 240f;
		obj.ViewMode = OverlayModes.Power.ID;
		obj.AudioCategory = "Metal";
		obj.PowerInputOffset = new CellOffset(2, 0);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		go.AddOrGet<DropAllWorkable>();
		Prioritizable.AddRef(go);
		ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
		go.AddOrGet<ComplexFabricatorWorkable>().overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_clothingfactory_kanim") };
		go.AddOrGet<ComplexFabricatorWorkable>();
		complexFabricator.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		ConfigureRecipes();
		BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);
	}

	private void ConfigureRecipes()
	{
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(GameTags.Fabrics, TUNING.EQUIPMENT.VESTS.WARM_VEST_MASS, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature, "")
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("Warm_Vest".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		WarmVestConfig.recipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("ClothingFabricator", array, array2), array, array2)
		{
			time = TUNING.EQUIPMENT.VESTS.WARM_VEST_FABTIME,
			description = STRINGS.EQUIPMENT.PREFABS.WARM_VEST.RECIPE_DESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "ClothingFabricator" },
			sortOrder = 1
		};
		ComplexRecipe.RecipeElement[] array3 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(GameTags.Fabrics, TUNING.EQUIPMENT.VESTS.FUNKY_VEST_MASS, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature, "")
		};
		ComplexRecipe.RecipeElement[] array4 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("Funky_Vest".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		FunkyVestConfig.recipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("ClothingFabricator", array3, array4), array3, array4)
		{
			time = TUNING.EQUIPMENT.VESTS.FUNKY_VEST_FABTIME,
			description = STRINGS.EQUIPMENT.PREFABS.FUNKY_VEST.RECIPE_DESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "ClothingFabricator" },
			sortOrder = 1
		};
		if (DlcManager.IsContentSubscribed("DLC5_ID"))
		{
			ComplexRecipe.RecipeElement[] array5 = new ComplexRecipe.RecipeElement[2]
			{
				new ComplexRecipe.RecipeElement(new Tag[1] { SimHashes.Rubber.CreateTag() }, TUNING.EQUIPMENT.SUITS.DRY_SUIT_MASS, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature, ""),
				new ComplexRecipe.RecipeElement(GameTags.Fabrics, 2f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature, "")
			};
			ComplexRecipe.RecipeElement[] array6 = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement("DrySuit".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
			};
			DrySuitConfig.recipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("ClothingFabricator", array5, array6), array5, array6)
			{
				time = TUNING.EQUIPMENT.VESTS.WARM_VEST_FABTIME,
				description = STRINGS.EQUIPMENT.PREFABS.DRYSUIT.RECIPE_DESC,
				nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
				fabricators = new List<Tag> { "ClothingFabricator" },
				sortOrder = 1,
				requiredTech = Db.Get().TechItems.drySuit.parentTechId
			};
		}
		if (DlcManager.IsContentSubscribed("DLC5_ID"))
		{
			ComplexRecipe.RecipeElement[] array7 = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement(new Tag[1] { SimHashes.Rubber.CreateTag() }, TUNING.EQUIPMENT.SHOES.BOOTS_RUBBER_FABRICATION_MASS, ComplexRecipe.RecipeElement.TemperatureOperation.Heated, "", storeElement: false, inheritElement: true)
			};
			ComplexRecipe.RecipeElement[] array8 = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement(RubberBootsConfig.ID, 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
			};
			new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("ClothingFabricator", array7, array8), array7, array8)
			{
				time = TUNING.EQUIPMENT.SHOES.BOOTS_FABRICATIONTIME,
				description = string.Format(STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.RECIPE_DESCRIPTION, ELEMENTS.RUBBER.NAME, STRINGS.EQUIPMENT.PREFABS.RUBBERBOOTS.NAME),
				nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
				fabricators = new List<Tag> { "ClothingFabricator" },
				requiredTech = Db.Get().TechItems.rubberBoots.parentTechId,
				sortOrder = 2
			};
		}
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
		};
	}
}
