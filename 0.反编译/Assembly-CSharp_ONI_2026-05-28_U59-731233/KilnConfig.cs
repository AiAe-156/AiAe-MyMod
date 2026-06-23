using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class KilnConfig : IBuildingConfig
{
	public const string ID = "Kiln";

	public const float INPUT_CLAY_PER_SECOND = 1f;

	public const float CERAMIC_PER_SECOND = 1f;

	public const float CO2_RATIO = 0.1f;

	public const float OUTPUT_TEMP = 353.15f;

	public const float REFILL_RATE = 2400f;

	public const float CERAMIC_STORAGE_AMOUNT = 2400f;

	public const float COAL_RATE = 0.1f;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("Kiln", 2, 2, "kiln_kanim", 100, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.ALL_METALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER5, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
		buildingDef.Overheatable = false;
		buildingDef.RequiresPowerInput = false;
		buildingDef.ExhaustKilowattsWhenActive = 16f;
		buildingDef.SelfHeatKilowattsWhenActive = 4f;
		buildingDef.AudioCategory = "HollowMetal";
		buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 1));
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.AddOrGet<DropAllWorkable>();
		go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
		ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
		complexFabricator.heatedTemperature = 353.15f;
		complexFabricator.duplicantOperated = false;
		complexFabricator.showProgressBar = true;
		complexFabricator.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);
		ConfigureRecipes();
		Prioritizable.AddRef(go);
	}

	private void ConfigureRecipes()
	{
		Tag tag = SimHashes.Ceramic.CreateTag();
		Tag material = SimHashes.Clay.CreateTag();
		Tag tag2 = SimHashes.Carbon.CreateTag();
		Tag tag3 = SimHashes.Peat.CreateTag();
		float amount = 100f;
		float amount2 = 25f;
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement(material, amount),
			new ComplexRecipe.RecipeElement(GameTags.BasicWoods.Append(new Tag[2]
			{
				SimHashes.Carbon.CreateTag(),
				SimHashes.Peat.CreateTag()
			}), amount2)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(tag, amount, ComplexRecipe.RecipeElement.TemperatureOperation.Heated)
		};
		string obsolete_id = ComplexRecipeManager.MakeObsoleteRecipeID("Kiln", tag);
		string text = ComplexRecipeManager.MakeRecipeID("Kiln", array, array2);
		new ComplexRecipe(text, array, array2)
		{
			time = 40f,
			description = string.Format(STRINGS.BUILDINGS.PREFABS.EGGCRACKER.RECIPE_DESCRIPTION, ElementLoader.FindElementByHash(SimHashes.Clay).name, ElementLoader.FindElementByHash(SimHashes.Ceramic).name),
			fabricators = new List<Tag> { TagManager.Create("Kiln") },
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			sortOrder = 100
		};
		ComplexRecipeManager.Get().AddObsoleteIDMapping(obsolete_id, text);
		Tag tag4 = SimHashes.RefinedCarbon.CreateTag();
		ComplexRecipe.RecipeElement[] array3 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(GameTags.BasicWoods.Append(new Tag[2] { tag2, tag3 }), new float[5] { 200f, 200f, 200f, 125f, 300f })
		};
		ComplexRecipe.RecipeElement[] array4 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(tag4, 100f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated)
		};
		string obsolete_id2 = ComplexRecipeManager.MakeObsoleteRecipeID("Kiln", tag4);
		string text2 = ComplexRecipeManager.MakeRecipeID("Kiln", array3, array4);
		new ComplexRecipe(text2, array3, array4)
		{
			time = 40f,
			description = string.Format(STRINGS.BUILDINGS.PREFABS.EGGCRACKER.RECIPE_DESCRIPTION, ElementLoader.FindElementByHash(SimHashes.Carbon).name, ElementLoader.FindElementByHash(SimHashes.RefinedCarbon).name),
			fabricators = new List<Tag> { TagManager.Create("Kiln") },
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			sortOrder = 200
		};
		ComplexRecipeManager.Get().AddObsoleteIDMapping(obsolete_id2, text2);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LogicOperationalController>();
		go.AddOrGetDef<PoweredActiveController.Def>();
		SymbolOverrideControllerUtil.AddToPrefab(go);
	}
}
