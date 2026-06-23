using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class MissileFabricatorConfig : IBuildingConfig
{
	public const string ID = "MissileFabricator";

	public const float MISSILE_FABRICATION_TIME = 80f;

	public const float CO2_PRODUCTION_RATE = 0.0125f;

	public const float LONG_RANGE_MISSILE_REFINED_METAL = 50f;

	public const float LONG_RANGE_MISSILE_LIQUID_INPUT = 200f;

	public const float LONG_RANGE_MISSILE_SOLID_INPUT = 100f;

	private static readonly List<Storage.StoredItemModifier> RefineryStoredItemModifiers = new List<Storage.StoredItemModifier>
	{
		Storage.StoredItemModifier.Hide,
		Storage.StoredItemModifier.Preserve,
		Storage.StoredItemModifier.Seal
	};

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("MissileFabricator", 5, 4, "missile_fabricator_kanim", 250, 60f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER5, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER6, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		obj.RequiresPowerInput = true;
		obj.EnergyConsumptionWhenActive = 960f;
		obj.SelfHeatKilowattsWhenActive = 8f;
		obj.ExhaustKilowattsWhenActive = 0f;
		obj.ViewMode = OverlayModes.Power.ID;
		obj.AudioCategory = "Metal";
		obj.PowerInputOffset = new CellOffset(1, 0);
		obj.InputConduitType = ConduitType.Liquid;
		obj.UtilityInputOffset = new CellOffset(-1, 1);
		obj.RequiredSkillPerkID = Db.Get().SkillPerks.CanMakeMissiles.Id;
		obj.AddSearchTerms(SEARCH_TERMS.MISSILE);
		obj.POIUnlockable = true;
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.AddOrGet<DropAllWorkable>();
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
		complexFabricator.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
		complexFabricator.keepExcessLiquids = true;
		complexFabricator.allowManualFluidDelivery = false;
		ComplexFabricatorWorkable complexFabricatorWorkable = go.AddOrGet<ComplexFabricatorWorkable>();
		complexFabricator.duplicantOperated = true;
		BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);
		complexFabricator.storeProduced = false;
		complexFabricator.inStorage.SetDefaultStoredItemModifiers(RefineryStoredItemModifiers);
		complexFabricator.buildStorage.SetDefaultStoredItemModifiers(RefineryStoredItemModifiers);
		complexFabricator.outputOffset = new Vector3(1f, 0.5f);
		complexFabricatorWorkable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_missile_fabricator_kanim") };
		BuildingElementEmitter buildingElementEmitter = go.AddOrGet<BuildingElementEmitter>();
		buildingElementEmitter.emitRate = 0.0125f;
		buildingElementEmitter.temperature = 313.15f;
		buildingElementEmitter.element = SimHashes.CarbonDioxide;
		buildingElementEmitter.modifierOffset = new Vector2(2f, 2f);
		ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
		conduitConsumer.capacityTag = GameTags.Liquid;
		conduitConsumer.capacityKG = 400f;
		conduitConsumer.storage = complexFabricator.inStorage;
		conduitConsumer.alwaysConsume = false;
		conduitConsumer.forceAlwaysSatisfied = true;
		conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Store;
		ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[2]
		{
			new ComplexRecipe.RecipeElement(GameTags.BasicRefinedMetals, 25f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated, "", storeElement: false, inheritElement: true),
			new ComplexRecipe.RecipeElement(new Tag[2]
			{
				SimHashes.Petroleum.CreateTag(),
				SimHashes.RefinedLipid.CreateTag()
			}, 50f)
		};
		ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("MissileBasic", 5f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		ComplexRecipe complexRecipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("MissileFabricator", array, array2), array, array2);
		complexRecipe.time = 80f;
		complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.Result;
		complexRecipe.description = GameUtil.SafeStringFormat(STRINGS.BUILDINGS.PREFABS.MISSILEFABRICATOR.RECIPE_DESCRIPTION, STRINGS.ITEMS.MISSILE_BASIC.NAME, MISC.TAGS.REFINEDMETAL, ElementLoader.FindElementByHash(SimHashes.Petroleum).name);
		complexRecipe.fabricators = new List<Tag> { TagManager.Create("MissileFabricator") };
		ComplexRecipe.RecipeElement[] array3 = new ComplexRecipe.RecipeElement[3]
		{
			new ComplexRecipe.RecipeElement(GameTags.BasicRefinedMetals, 50f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated, "", storeElement: false, inheritElement: true),
			new ComplexRecipe.RecipeElement(new Tag[2]
			{
				SimHashes.Fertilizer.CreateTag(),
				SimHashes.Peat.CreateTag()
			}, 100f),
			new ComplexRecipe.RecipeElement(new Tag[2]
			{
				SimHashes.Petroleum.CreateTag(),
				SimHashes.RefinedLipid.CreateTag()
			}, 200f)
		};
		ComplexRecipe.RecipeElement[] array4 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("MissileLongRange", 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		complexRecipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("MissileFabricator", array3, array4), array3, array4, DlcManager.DLC4, DlcManager.EXPANSION1);
		complexRecipe.time = 80f;
		complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.Result;
		complexRecipe.description = GameUtil.SafeStringFormat(STRINGS.BUILDINGS.PREFABS.MISSILEFABRICATOR.RECIPE_DESCRIPTION_LONGRANGE, STRINGS.ITEMS.MISSILE_LONGRANGE.NAME, MISC.TAGS.REFINEDMETAL, ElementLoader.FindElementByHash(SimHashes.Fertilizer).name, ElementLoader.FindElementByHash(SimHashes.Petroleum).name);
		complexRecipe.fabricators = new List<Tag> { TagManager.Create("MissileFabricator") };
		ComplexRecipe.RecipeElement[] array5 = new ComplexRecipe.RecipeElement[3]
		{
			new ComplexRecipe.RecipeElement(GameTags.BasicRefinedMetals, 50f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated, "", storeElement: false, inheritElement: true),
			new ComplexRecipe.RecipeElement(new Tag[2]
			{
				SimHashes.Fertilizer.CreateTag(),
				SimHashes.Peat.CreateTag()
			}, 100f),
			new ComplexRecipe.RecipeElement(new Tag[2]
			{
				SimHashes.Petroleum.CreateTag(),
				SimHashes.RefinedLipid.CreateTag()
			}, 200f)
		};
		ComplexRecipe.RecipeElement[] array6 = new ComplexRecipe.RecipeElement[1]
		{
			new ComplexRecipe.RecipeElement("MissileLongRange", 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
		};
		complexRecipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("MissileFabricator", array5, array6), array5, array6, DlcManager.EXPANSION1, new string[0]);
		complexRecipe.time = 80f;
		complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.Result;
		complexRecipe.description = GameUtil.SafeStringFormat(STRINGS.BUILDINGS.PREFABS.MISSILEFABRICATOR.RECIPE_DESCRIPTION_LONGRANGE, STRINGS.ITEMS.MISSILE_LONGRANGE.NAME, MISC.TAGS.REFINEDMETAL, ElementLoader.FindElementByHash(SimHashes.Fertilizer).name, ElementLoader.FindElementByHash(SimHashes.Petroleum).name);
		complexRecipe.fabricators = new List<Tag> { TagManager.Create("MissileFabricator") };
		Prioritizable.AddRef(go);
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
			component.requiredSkillPerk = Db.Get().SkillPerks.CanMakeMissiles.Id;
		};
	}
}
