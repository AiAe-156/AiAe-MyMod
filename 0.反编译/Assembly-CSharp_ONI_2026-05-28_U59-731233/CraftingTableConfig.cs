using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class CraftingTableConfig : IBuildingConfig
{
	public const string ID = "CraftingTable";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("CraftingTable", 2, 2, "craftingStation_kanim", 100, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.RAW_METALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER3, decor: TUNING.BUILDINGS.DECOR.NONE);
		buildingDef.RequiresPowerInput = true;
		buildingDef.EnergyConsumptionWhenActive = 60f;
		buildingDef.ViewMode = OverlayModes.Power.ID;
		buildingDef.AudioCategory = "Metal";
		buildingDef.PowerInputOffset = new CellOffset(1, 0);
		buildingDef.POIUnlockable = true;
		buildingDef.AddSearchTerms(SEARCH_TERMS.BIONIC);
		return buildingDef;
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
		BuildingComplete buildingComplete = go.AddOrGet<BuildingComplete>();
		buildingComplete.isManuallyOperated = true;
		go.AddOrGet<ComplexFabricatorWorkable>().overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_craftingstation_kanim") };
		Prioritizable.AddRef(go);
		BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);
		ConfigureRecipes();
	}

	private void ConfigureRecipes()
	{
		List<Tag> list = new List<Tag>();
		list.AddRange(GameTags.StartingMetalOres);
		list.Add(SimHashes.IronOre.CreateTag());
		CreateMetalMiniVoltRecipe(list.ToArray());
		if (DlcManager.IsAllContentSubscribed(new string[2] { "EXPANSION1_ID", "DLC3_ID" }))
		{
			ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement(SimHashes.UraniumOre.CreateTag(), 10f, inheritElement: true)
			};
			ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement("DisposableElectrobank_UraniumOre".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
			};
			string id = ComplexRecipeManager.MakeRecipeID("CraftingTable", array, array2);
			new ComplexRecipe(id, array, array2, new string[1] { "DLC3_ID" })
			{
				time = INDUSTRIAL.RECIPES.STANDARD_FABRICATION_TIME * 2f,
				description = string.Format(STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.RECIPE_DESCRIPTION, ElementLoader.FindElementByHash(SimHashes.UraniumOre).name, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.ELECTROBANK_URANIUM_ORE.NAME),
				nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
				fabricators = new List<Tag> { "CraftingTable" },
				requiredTech = Db.Get().TechItems.disposableElectrobankUraniumOre.parentTechId,
				sortOrder = 0
			};
		}
		ComplexRecipe.RecipeElement[] array3 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(GameTags.BasicMetalOres, 50f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated, "", storeElement: true)
		};
		ComplexRecipe.RecipeElement[] array4 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("Oxygen_Mask".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id2 = ComplexRecipeManager.MakeRecipeID("CraftingTable", array3, array4);
		new ComplexRecipe(id2, array3, array4)
		{
			time = TUNING.EQUIPMENT.SUITS.OXYMASK_FABTIME,
			description = STRINGS.EQUIPMENT.PREFABS.OXYGEN_MASK.RECIPE_DESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "CraftingTable" },
			requiredTech = Db.Get().TechItems.oxygenMask.parentTechId,
			sortOrder = 2
		};
		ComplexRecipe.RecipeElement[] array5 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("Worn_Oxygen_Mask".ToTag(), 1f, inheritElement: true)
		};
		ComplexRecipe.RecipeElement[] array6 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("Oxygen_Mask".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id3 = ComplexRecipeManager.MakeRecipeID("CraftingTable", array5, array6);
		ComplexRecipe complexRecipe = new ComplexRecipe(id3, array5, array6)
		{
			time = TUNING.EQUIPMENT.SUITS.OXYMASK_FABTIME,
			description = STRINGS.EQUIPMENT.PREFABS.OXYGEN_MASK.REPAIR_WORN_DESC,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Custom,
			fabricators = new List<Tag> { "CraftingTable" },
			requiredTech = Db.Get().TechItems.oxygenMask.parentTechId,
			sortOrder = 2
		};
		complexRecipe.customName = STRINGS.EQUIPMENT.PREFABS.OXYGEN_MASK.REPAIR_WORN_RECIPE_NAME;
		if (DlcManager.IsContentSubscribed("DLC5_ID"))
		{
			int num = 1;
			ComplexRecipe.RecipeElement[] array7 = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement(new Tag[1] { SimHashes.Rubber.CreateTag() }, (float)num * 50f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated, "", storeElement: false, inheritElement: true)
			};
			ComplexRecipe.RecipeElement[] array8 = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement("RubberGasket".ToTag(), num, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
			};
			string id4 = ComplexRecipeManager.MakeRecipeID("CraftingTable", array7, array8);
			new ComplexRecipe(id4, array7, array8)
			{
				time = INDUSTRIAL.RECIPES.STANDARD_FABRICATION_TIME,
				description = string.Format(STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.RECIPE_DESCRIPTION, ELEMENTS.RUBBER.NAME, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.RUBBER_GASKET.NAME),
				nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
				fabricators = new List<Tag> { "CraftingTable" },
				requiredTech = Db.Get().TechItems.gasket.parentTechId,
				sortOrder = 3
			};
			int num2 = 1;
			ComplexRecipe.RecipeElement[] array9 = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement(new Tag[1] { SimHashes.Polypropylene.CreateTag() }, (float)num2 * 50f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated, "", storeElement: false, inheritElement: true)
			};
			ComplexRecipe.RecipeElement[] array10 = new ComplexRecipe.RecipeElement[1]
			{
				new ComplexRecipe.RecipeElement("PlasticGasket".ToTag(), num2, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
			};
			string id5 = ComplexRecipeManager.MakeRecipeID("CraftingTable", array9, array10);
			new ComplexRecipe(id5, array9, array10)
			{
				time = INDUSTRIAL.RECIPES.STANDARD_FABRICATION_TIME,
				description = string.Format(STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.RECIPE_DESCRIPTION, ELEMENTS.POLYPROPYLENE.NAME, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.PLASTIC_GASKET.NAME),
				nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
				fabricators = new List<Tag> { "CraftingTable" },
				requiredTech = Db.Get().TechItems.gasket.parentTechId,
				sortOrder = 4
			};
		}
	}

	private void CreateMetalMiniVoltRecipe(Tag[] inputMetals)
	{
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(inputMetals, 200f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated, "", storeElement: false, inheritElement: true)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("DisposableElectrobank_RawMetal".ToTag(), 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string id = ComplexRecipeManager.MakeRecipeID("CraftingTable", array, array2);
		new ComplexRecipe(id, array, array2, DlcManager.DLC3)
		{
			time = INDUSTRIAL.RECIPES.STANDARD_FABRICATION_TIME * 2f,
			description = string.Format(STRINGS.BUILDINGS.PREFABS.CRAFTINGTABLE.RECIPE_DESCRIPTION, MISC.TAGS.METAL, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.ELECTROBANK_METAL_ORE.NAME),
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { "CraftingTable" },
			sortOrder = 0
		};
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<KPrefabID>().prefabInitFn += delegate
		{
			Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_Suits);
		};
		go.GetComponent<KPrefabID>().prefabSpawnFn += delegate(GameObject game_object)
		{
			ComplexFabricatorWorkable component = game_object.GetComponent<ComplexFabricatorWorkable>();
			component.WorkerStatusItem = Db.Get().DuplicantStatusItems.Fabricating;
			component.AttributeConverter = Db.Get().AttributeConverters.MachinerySpeed;
			component.AttributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
			component.SkillExperienceSkillGroup = Db.Get().SkillGroups.Technicals.Id;
			component.SkillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
			DiscoveredResources.Instance.Discover("Worn_Oxygen_Mask");
		};
	}
}
