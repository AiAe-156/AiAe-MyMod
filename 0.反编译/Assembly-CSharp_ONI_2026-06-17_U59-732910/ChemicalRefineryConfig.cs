using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class ChemicalRefineryConfig : IBuildingConfig
{
	public const string ID = "ChemicalRefinery";

	private HashedString[] dupeInteractAnims;

	private const float INPUT_KG = 100f;

	private const float OUTPUT_KG = 100f;

	private static readonly List<Storage.StoredItemModifier> RefineryStoredItemModifiers = new List<Storage.StoredItemModifier>
	{
		Storage.StoredItemModifier.Hide,
		Storage.StoredItemModifier.Preserve,
		Storage.StoredItemModifier.Insulate,
		Storage.StoredItemModifier.Seal
	};

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("ChemicalRefinery", 4, 3, "chemistry_lab_kanim", 30, 60f, new float[2]
		{
			TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3[0],
			TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2[0]
		}, new string[2] { "RefinedMetal", "Glasses" }, 2400f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER1, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
		obj.RequiresPowerInput = true;
		obj.EnergyConsumptionWhenActive = 480f;
		obj.SelfHeatKilowattsWhenActive = 1f;
		obj.ViewMode = OverlayModes.LiquidConduits.ID;
		obj.AudioCategory = "HollowMetal";
		obj.AudioSize = "large";
		obj.InputConduitType = ConduitType.Liquid;
		obj.OutputConduitType = ConduitType.Liquid;
		obj.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		obj.PowerInputOffset = new CellOffset(1, 0);
		obj.UtilityInputOffset = new CellOffset(0, 0);
		obj.UtilityOutputOffset = new CellOffset(2, 1);
		obj.RequiredSkillPerkID = Db.Get().SkillPerks.AllowChemistry.Id;
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<DropAllWorkable>();
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
		complexFabricator.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		complexFabricator.duplicantOperated = true;
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		ComplexFabricatorWorkable complexFabricatorWorkable = go.AddOrGet<ComplexFabricatorWorkable>();
		complexFabricatorWorkable.requiredSkillPerk = Db.Get().SkillPerks.AllowChemistry.Id;
		BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);
		complexFabricatorWorkable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_chemistrylab_kanim") };
		complexFabricatorWorkable.workingPstComplete = new HashedString[1] { "working_pst_complete" };
		complexFabricator.heatedTemperature = SupermaterialRefineryConfig.OUTPUT_TEMPERATURE;
		complexFabricator.storeProduced = true;
		complexFabricator.inStorage.SetDefaultStoredItemModifiers(RefineryStoredItemModifiers);
		complexFabricator.buildStorage.SetDefaultStoredItemModifiers(RefineryStoredItemModifiers);
		complexFabricator.outStorage.SetDefaultStoredItemModifiers(RefineryStoredItemModifiers);
		complexFabricator.storeProduced = false;
		complexFabricator.keepExcessLiquids = true;
		complexFabricator.inStorage.capacityKg = 1000f;
		ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
		conduitDispenser.conduitType = ConduitType.Liquid;
		conduitDispenser.alwaysDispense = true;
		conduitDispenser.elementFilter = null;
		conduitDispenser.storage = go.GetComponent<ComplexFabricator>().outStorage;
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		Prioritizable.AddRef(go);
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement(SimHashes.Water.CreateTag(), 93f),
			new ComplexRecipe.RecipeElement(SimHashes.Salt.CreateTag(), 7f)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(SimHashes.SaltWater.CreateTag(), 100f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature, storeElement: true)
		};
		new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("ChemicalRefinery", array, array2), array, array2)
		{
			time = 40f,
			description = STRINGS.BUILDINGS.PREFABS.CHEMICALREFINERY.SALTWATER_RECIPE_DESCRIPTION,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { TagManager.Create("ChemicalRefinery") }
		};
		ComplexRecipe.RecipeElement[] array3 = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement(SimHashes.PhytoOil.CreateTag(), 160f),
			new ComplexRecipe.RecipeElement(SimHashes.BleachStone.CreateTag(), 40f)
		};
		ComplexRecipe.RecipeElement[] array4 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(SimHashes.RefinedLipid.CreateTag(), 200f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated, storeElement: true)
		};
		new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("ChemicalRefinery", array3, array4), array3, array4)
		{
			time = 40f,
			description = STRINGS.BUILDINGS.PREFABS.CHEMICALREFINERY.REFINEDLIPID_RECIPE_DESCRIPTION,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { TagManager.Create("ChemicalRefinery") }
		};
		float num = 0.01f;
		float num2 = (1f - num) * 0.5f;
		ComplexRecipe.RecipeElement[] array5 = new ComplexRecipe.RecipeElement[3]
		{
			new ComplexRecipe.RecipeElement(SimHashes.Fullerene.CreateTag(), 100f * num),
			new ComplexRecipe.RecipeElement(SimHashes.Gold.CreateTag(), 100f * num2),
			new ComplexRecipe.RecipeElement(SimHashes.Petroleum.CreateTag(), 100f * num2)
		};
		ComplexRecipe.RecipeElement[] array6 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(SimHashes.SuperCoolant.CreateTag(), 100f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated, storeElement: true)
		};
		new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("ChemicalRefinery", array5, array6), array5, array6)
		{
			time = 80f,
			description = STRINGS.BUILDINGS.PREFABS.SUPERMATERIALREFINERY.SUPERCOOLANT_RECIPE_DESCRIPTION,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { TagManager.Create("ChemicalRefinery") },
			requiredTech = Db.Get().TechItems.superLiquids.parentTechId
		};
		float num3 = 0.35f;
		ComplexRecipe.RecipeElement[] array7 = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement(SimHashes.Isoresin.CreateTag(), 100f * num3),
			new ComplexRecipe.RecipeElement(SimHashes.Petroleum.CreateTag(), 100f * (1f - num3))
		};
		ComplexRecipe.RecipeElement[] array8 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement(SimHashes.ViscoGel.CreateTag(), 100f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated, storeElement: true)
		};
		new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("ChemicalRefinery", array7, array8), array7, array8)
		{
			time = 80f,
			description = STRINGS.BUILDINGS.PREFABS.SUPERMATERIALREFINERY.VISCOGEL_RECIPE_DESCRIPTION,
			nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
			fabricators = new List<Tag> { TagManager.Create("ChemicalRefinery") },
			requiredTech = Db.Get().TechItems.superLiquids.parentTechId
		};
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<RequireInputs>().requireConduitHasMass = false;
		go.GetComponent<KPrefabID>().prefabSpawnFn += delegate(GameObject game_object)
		{
			ComplexFabricatorWorkable component = game_object.GetComponent<ComplexFabricatorWorkable>();
			component.WorkerStatusItem = Db.Get().DuplicantStatusItems.Processing;
			component.AttributeConverter = Db.Get().AttributeConverters.ResearchSpeed;
			component.AttributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
			component.SkillExperienceSkillGroup = Db.Get().SkillGroups.Research.Id;
			component.SkillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
			KAnimFile anim = Assets.GetAnim("anim_interacts_chemistrylab_kanim");
			KAnimFile[] overrideAnims = new KAnimFile[1] { anim };
			component.overrideAnims = overrideAnims;
			component.workAnims = new HashedString[2] { "working_pre", "working_loop" };
			component.synchronizeAnims = false;
		};
	}
}
