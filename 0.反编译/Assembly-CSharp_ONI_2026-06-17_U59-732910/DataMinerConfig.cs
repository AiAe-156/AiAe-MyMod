using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class DataMinerConfig : IBuildingConfig
{
	public const string ID = "DataMiner";

	public const float POWER_USAGE_W = 1000f;

	public const float BASE_UNITS_PRODUCED_PER_CYCLE = 3f;

	public const float BASE_DTU_PRODUCTION = 3f;

	public const float STORAGE_CAPACITY_KG = 1000f;

	public const float MASS_CONSUMED_PER_BANK_KG = 5f;

	public const float BASE_DURATION_SECONDS = 200f;

	public static MathUtil.MinMax PRODUCTION_RATE_SCALE = new MathUtil.MinMax(0.6f, 5.3333335f);

	public static MathUtil.MinMax TEMPERATURE_SCALING_RANGE = new MathUtil.MinMax(10f, 325f);

	public SimHashes INPUT_MATERIAL = SimHashes.Polypropylene;

	public Tag INPUT_MATERIAL_TAG = SimHashes.Polypropylene.CreateTag();

	public Tag OUTPUT_MATERIAL_TAG = DatabankHelper.TAG;

	public string OUTPUT_MATERIAL_NAME = DatabankHelper.NAME;

	public const float BASE_PRODUCTION_PROGRESS_PER_TICK = 0.001f;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC3;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("DataMiner", 3, 2, "data_miner_kanim", 30, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER1, decor: TUNING.BUILDINGS.DECOR.NONE);
		obj.RequiresPowerInput = true;
		obj.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		obj.EnergyConsumptionWhenActive = 1000f;
		obj.ExhaustKilowattsWhenActive = 0.5f;
		obj.SelfHeatKilowattsWhenActive = 3f;
		obj.ViewMode = OverlayModes.Power.ID;
		obj.AudioCategory = "Metal";
		obj.AudioSize = "large";
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.AddOrGet<DropAllWorkable>();
		go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
		go.AddOrGet<LogicOperationalController>();
		go.AddOrGet<CopyBuildingSettings>();
		DataMiner dataMiner = go.AddOrGet<DataMiner>();
		dataMiner.duplicantOperated = false;
		dataMiner.showProgressBar = true;
		dataMiner.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		BuildingTemplates.CreateComplexFabricatorStorage(go, dataMiner);
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(INPUT_MATERIAL_TAG, 5f)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(OUTPUT_MATERIAL_TAG, 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		string obsolete_id = ComplexRecipeManager.MakeObsoleteRecipeID("DataMiner", OUTPUT_MATERIAL_TAG);
		string text = ComplexRecipeManager.MakeRecipeID("DataMiner", array, array2);
		new ComplexRecipe(text, array, array2)
		{
			time = 200f,
			description = string.Format(STRINGS.BUILDINGS.PREFABS.EGGCRACKER.RECIPE_DESCRIPTION, ElementLoader.FindElementByHash(INPUT_MATERIAL).name, OUTPUT_MATERIAL_NAME),
			fabricators = new List<Tag> { TagManager.Create("DataMiner") },
			nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult,
			sortOrder = 300
		};
		ComplexRecipeManager.Get().AddObsoleteIDMapping(obsolete_id, text);
		Prioritizable.AddRef(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
