using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class SeaTreeBranchConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SeaTreeBranch";

	public const float GROWING_DURATION_CYCLES = 3f;

	public const float GROWING_DURATION = 1800f;

	public const float BULB_GROWING_DURATION = 1800f;

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		string name = STRINGS.CREATURES.SPECIES.SEATREEBRANCH.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEATREEBRANCH.DESC;
		EffectorValues tIER = DECOR.BONUS.TIER0;
		KAnimFile anim = Assets.GetAnim("sea_fairy_plant_kanim");
		List<Tag> additionalTags = new List<Tag>
		{
			GameTags.HideFromSpawnTool,
			GameTags.HideFromCodex,
			GameTags.PlantBranch,
			GameTags.ExcludeFromTemplate
		};
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("SeaTreeBranch", name, desc, 1f, anim, "branch_idle", Grid.SceneLayer.BuildingFront, 1, 1, tIER, default(EffectorValues), SimHashes.Creature, additionalTags, 302.65f);
		string text = "SeaTreeBranchOriginal";
		bool flag = false;
		bool should_grow_old = flag;
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 248.15f, 295.15f, 310.15f, 398.15f, PLANTS.SAFE_ELEMENTS.AllWaters, pressure_sensitive: false, 0f, 0.15f, null, can_drown: false, can_tinker: true, require_solid_tile: false, require_Backwall_Foundation: false, should_grow_old, 2400f, 0f, 2200f, text, STRINGS.CREATURES.SPECIES.SEATREEBRANCH.NAME);
		gameObject.AddOrGet<PressureVulnerable>().allCellsMustBeSafe = true;
		gameObject.AddOrGet<HarvestDesignatable>();
		gameObject.AddOrGet<CodexEntryRedirector>().CodexID = "SeaTree";
		gameObject.AddOrGet<UprootedMonitor>();
		Crop.CropVal cropval = CROPS.CROP_TYPES.Find((Crop.CropVal m) => m.cropId == "SeaFairy");
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
		trait.Add(new AttributeModifier(Db.Get().Amounts.Maturity.maxAttribute.Id, 3f, STRINGS.CREATURES.SPECIES.SEATREEBRANCH.NAME));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Maturity2.maxAttribute.Id, 3f, STRINGS.CREATURES.SPECIES.SEATREEBRANCH.NAME));
		trait.Add(new AttributeModifier(Db.Get().PlantAttributes.YieldAmount.Id, cropval.numProduced, STRINGS.CREATURES.SPECIES.SEATREEBRANCH.NAME));
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.HarvestableIDs, "SeaTreeBranch");
		SeaTreeBranch.Def def = gameObject.AddOrGetDef<SeaTreeBranch.Def>();
		def.MAX_BRANCH_COUNT = 8;
		def.BRANCH_PREFAB_NAME = "SeaTreeBranch";
		gameObject.AddOrGet<Harvestable>();
		gameObject.AddOrGet<HarvestDesignatable>();
		WiltCondition wiltCondition = gameObject.AddOrGet<WiltCondition>();
		wiltCondition.WiltDelay = 0f;
		wiltCondition.RecoveryDelay = 0f;
		SeedProducer seedProducer = gameObject.AddOrGet<SeedProducer>();
		seedProducer.Configure("SeaTreeSeed", SeedProducer.ProductionType.HarvestOnly);
		seedProducer.seedDropChanceMultiplier = 0.125f;
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
