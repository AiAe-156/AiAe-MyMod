using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class RubberMakerConfig : IBuildingConfig
{
	public const string ID = "RubberMaker";

	private const float LATEX_INPUT_KG = 100f;

	public const float OUTPUT_KG = 100f;

	private const float CO2_PER_SECOND = 0.05f;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("RubberMaker", 3, 3, "vulcanizer_kanim", 100, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER3, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
		obj.RequiresPowerInput = true;
		obj.PowerInputOffset = new CellOffset(0, 0);
		obj.EnergyConsumptionWhenActive = 480f;
		obj.ExhaustKilowattsWhenActive = 1f;
		obj.SelfHeatKilowattsWhenActive = 16f;
		obj.InputConduitType = ConduitType.Liquid;
		obj.UtilityInputOffset = new CellOffset(1, 0);
		obj.AudioCategory = "HollowMetal";
		obj.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 1));
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.AddOrGet<DropAllWorkable>();
		go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
		complexFabricator.duplicantOperated = false;
		complexFabricator.showProgressBar = true;
		complexFabricator.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		complexFabricator.storeProduced = false;
		complexFabricator.keepExcessLiquids = true;
		BuildingElementEmitter buildingElementEmitter = go.AddOrGet<BuildingElementEmitter>();
		buildingElementEmitter.emitRate = 0.05f;
		buildingElementEmitter.temperature = 349.15f;
		buildingElementEmitter.element = SimHashes.CarbonDioxide;
		buildingElementEmitter.modifierOffset = new Vector2(0f, 0f);
		BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);
		complexFabricator.inStorage.capacityKg = 200f;
		ConfigureRecipes();
		Prioritizable.AddRef(go);
	}

	private void ConfigureRecipes()
	{
		Tag material = SimHashes.Latex.CreateTag();
		Tag material2 = SimHashes.Rubber.CreateTag();
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(material, 100f)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(material2, 100f)
		};
		new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("RubberMaker", array, array2), array, array2)
		{
			time = 40f,
			description = string.Format(STRINGS.BUILDINGS.PREFABS.EGGCRACKER.RECIPE_DESCRIPTION, ElementLoader.FindElementByHash(SimHashes.Latex).name, ElementLoader.FindElementByHash(SimHashes.Rubber).name),
			fabricators = new List<Tag> { TagManager.Create("RubberMaker") },
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result
		};
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<RequireInputs>().requireConduitHasMass = false;
		go.AddOrGet<LogicOperationalController>();
		go.AddOrGetDef<PoweredActiveController.Def>();
	}
}
