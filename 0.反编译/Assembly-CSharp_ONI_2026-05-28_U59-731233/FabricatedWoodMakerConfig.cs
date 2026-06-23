using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class FabricatedWoodMakerConfig : IBuildingConfig
{
	public const string ID = "FabricatedWoodMaker";

	public const float OUTPUT_TEMP = 333.15f;

	public const SimHashes BINDING_LIQUID = SimHashes.NaturalResin;

	private static readonly List<Storage.StoredItemModifier> BindingLiquidStoredItemModifiers = new List<Storage.StoredItemModifier>
	{
		Storage.StoredItemModifier.Hide,
		Storage.StoredItemModifier.Preserve,
		Storage.StoredItemModifier.Insulate,
		Storage.StoredItemModifier.Seal
	};

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("FabricatedWoodMaker", 4, 3, "plantmatter_compressor_kanim", 100, 60f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.ALL_METALS, 1600f, BuildLocationRule.OnFloor, DECOR.PENALTY.TIER1, NOISE_POLLUTION.NOISY.TIER4);
		buildingDef.InputConduitType = ConduitType.Liquid;
		buildingDef.OutputConduitType = ConduitType.None;
		buildingDef.UtilityInputOffset = new CellOffset(0, 0);
		buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
		buildingDef.RequiresPowerInput = true;
		buildingDef.RequiresPowerOutput = false;
		buildingDef.PowerInputOffset = new CellOffset(0, 0);
		buildingDef.PowerOutputOffset = new CellOffset(0, 0);
		buildingDef.EnergyConsumptionWhenActive = 480f;
		buildingDef.UseHighEnergyParticleInputPort = false;
		buildingDef.UseHighEnergyParticleOutputPort = false;
		buildingDef.HighEnergyParticleInputOffset = new CellOffset(0, 0);
		buildingDef.HighEnergyParticleOutputOffset = new CellOffset(0, 0);
		buildingDef.PermittedRotations = PermittedRotations.Unrotatable;
		buildingDef.DragBuild = false;
		buildingDef.Replaceable = true;
		buildingDef.ExhaustKilowattsWhenActive = 0.25f;
		buildingDef.SelfHeatKilowattsWhenActive = 1f;
		buildingDef.UseStructureTemperature = true;
		buildingDef.Overheatable = true;
		buildingDef.Floodable = true;
		buildingDef.Disinfectable = true;
		buildingDef.Entombable = true;
		buildingDef.Invincible = false;
		buildingDef.Repairable = true;
		buildingDef.IsFoundation = false;
		buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(1, 0));
		buildingDef.AudioCategory = "Metal";
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
		complexFabricator.heatedTemperature = 333.15f;
		complexFabricator.duplicantOperated = true;
		complexFabricator.showProgressBar = true;
		complexFabricator.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		ComplexFabricatorWorkable complexFabricatorWorkable = go.AddOrGet<ComplexFabricatorWorkable>();
		complexFabricatorWorkable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_plywoodPress_kanim") };
		complexFabricatorWorkable.workingPstComplete = new HashedString[1] { "working_pst_complete" };
		BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		go.AddOrGet<LoopingSounds>();
		ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
		conduitConsumer.capacityTag = SimHashes.NaturalResin.CreateTag();
		conduitConsumer.capacityKG = 1000f;
		conduitConsumer.alwaysConsume = true;
		conduitConsumer.storage = complexFabricator.inStorage;
		conduitConsumer.forceAlwaysSatisfied = true;
		complexFabricator.inStorage.SetDefaultStoredItemModifiers(BindingLiquidStoredItemModifiers);
		complexFabricator.buildStorage.SetDefaultStoredItemModifiers(BindingLiquidStoredItemModifiers);
		complexFabricator.outStorage.SetDefaultStoredItemModifiers(BindingLiquidStoredItemModifiers);
		complexFabricator.storeProduced = false;
		complexFabricator.keepExcessLiquids = true;
		ConfigureRecipes();
		Prioritizable.AddRef(go);
	}

	private void ConfigureRecipes()
	{
		float amount = 10f;
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement("PlantFiber", 90f),
			new ComplexRecipe.RecipeElement(SimHashes.NaturalResin.CreateTag(), amount)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("FabricatedWood", 100f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated)
		};
		string id = ComplexRecipeManager.MakeRecipeID("FabricatedWoodMaker", array, array2);
		ComplexRecipe complexRecipe = new ComplexRecipe(id, array, array2);
		complexRecipe.time = 40f;
		complexRecipe.description = GameUtil.SafeStringFormat(STRINGS.BUILDINGS.PREFABS.FABRICATEDWOODMAKER.RECIPE_DESC, STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.PLANT_FIBER.NAME, ElementLoader.FindElementByHash(SimHashes.NaturalResin).name, Assets.GetPrefab("FabricatedWood").GetProperName());
		complexRecipe.fabricators = new List<Tag> { TagManager.Create("FabricatedWoodMaker") };
		complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.Result;
		complexRecipe.sortOrder = 100;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<RequireInputs>().SetRequirements(power: true, conduit: false);
		go.GetComponent<KPrefabID>().prefabSpawnFn += delegate(GameObject game_object)
		{
			ComplexFabricatorWorkable component = game_object.GetComponent<ComplexFabricatorWorkable>();
			component.WorkerStatusItem = Db.Get().DuplicantStatusItems.Fabricating;
			component.AttributeConverter = Db.Get().AttributeConverters.MachinerySpeed;
			component.AttributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
			component.SkillExperienceSkillGroup = Db.Get().SkillGroups.Technicals.Id;
			component.SkillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
			KAnimFile anim = Assets.GetAnim("anim_interacts_plywoodPress_kanim");
			KAnimFile[] overrideAnims = new KAnimFile[1] { anim };
			component.overrideAnims = overrideAnims;
			component.workAnims = new HashedString[2] { "working_pre", "working_loop" };
			component.synchronizeAnims = false;
		};
	}
}
