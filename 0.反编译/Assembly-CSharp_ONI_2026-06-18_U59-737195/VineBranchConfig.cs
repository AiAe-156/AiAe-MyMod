using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class VineBranchConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "VineBranch";

	public const float GROWING_DURATION_CYCLES = 3f;

	public const float GROWING_DURATION = 1800f;

	public const float FRUIT_GROWING_DURATION = 1800f;

	public const int FRUIT_COUNT_PER_HARVEST = 1;

	public const float PLANT_FIBER_PRODUCED_PER_CYCLE = 6f;

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC4;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		string name = STRINGS.CREATURES.SPECIES.VINEBRANCH.NAME;
		string desc = STRINGS.CREATURES.SPECIES.VINEBRANCH.DESC;
		EffectorValues tIER = DECOR.BONUS.TIER0;
		KAnimFile anim = Assets.GetAnim("vine_kanim");
		List<Tag> additionalTags = new List<Tag>
		{
			GameTags.HideFromSpawnTool,
			GameTags.HideFromCodex,
			GameTags.PlantBranch,
			GameTags.ExcludeFromTemplate
		};
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("VineBranch", name, desc, 1f, anim, "line_idle", Grid.SceneLayer.BuildingFront, 1, 1, tIER, default(EffectorValues), SimHashes.Creature, additionalTags, 308.15f);
		string text = "VineBranchOriginal";
		bool should_grow_old = false;
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 273.15f, 298.15f, 318.15f, 378.15f, null, pressure_sensitive: false, 0f, 0.15f, null, can_drown: true, can_tinker: true, require_solid_tile: false, require_Backwall_Foundation: false, should_grow_old, 2400f, 0f, 2200f, text, STRINGS.CREATURES.SPECIES.VINEBRANCH.NAME);
		gameObject.AddOrGet<HarvestDesignatable>();
		gameObject.AddOrGet<CodexEntryRedirector>().CodexID = "VineMother";
		gameObject.AddOrGet<PlantFiberProducer>().amount = 6f;
		gameObject.AddOrGet<UprootedMonitor>();
		Crop.CropVal cropval = CROPS.CROP_TYPES.Find((Crop.CropVal m) => m.cropId == VineFruitConfig.ID);
		gameObject.AddOrGet<Crop>().Configure(cropval);
		Modifiers component = gameObject.GetComponent<Modifiers>();
		if (gameObject.GetComponent<Traits>() == null)
		{
			gameObject.AddOrGet<Traits>();
			component.initialTraits.Add(text);
		}
		component.initialAmounts.Add(Db.Get().Amounts.Maturity.Id);
		component.initialAmounts.Add(Db.Get().Amounts.Maturity2.Id);
		component.initialAttributes.Add(Db.Get().PlantAttributes.YieldAmount.Id);
		Trait trait = Db.Get().traits.Get(component.initialTraits[0]);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Maturity.maxAttribute.Id, 3f, STRINGS.CREATURES.SPECIES.VINEBRANCH.NAME));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Maturity2.maxAttribute.Id, 3f, STRINGS.CREATURES.SPECIES.VINEBRANCH.NAME));
		trait.Add(new AttributeModifier(Db.Get().PlantAttributes.YieldAmount.Id, cropval.numProduced, STRINGS.CREATURES.SPECIES.VINEBRANCH.NAME));
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.HarvestableIDs, "VineBranch");
		gameObject.AddOrGetDef<VineBranch.Def>().BRANCH_PREFAB_NAME = "VineBranch";
		gameObject.AddOrGet<Harvestable>();
		gameObject.AddOrGet<HarvestDesignatable>();
		WiltCondition wiltCondition = gameObject.AddOrGet<WiltCondition>();
		wiltCondition.WiltDelay = 0f;
		wiltCondition.RecoveryDelay = 0f;
		SeedProducer seedProducer = gameObject.AddOrGet<SeedProducer>();
		seedProducer.Configure("VineMotherSeed", SeedProducer.ProductionType.HarvestOnly);
		seedProducer.seedDropChanceMultiplier = 1f / 6f;
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		inst.AddOrGet<UprootedMonitor>().monitorCells = new CellOffset[0];
		inst.AddOrGet<HarvestDesignatable>().iconOffset = new Vector2(0f, Grid.CellSizeInMeters * 0.75f);
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
