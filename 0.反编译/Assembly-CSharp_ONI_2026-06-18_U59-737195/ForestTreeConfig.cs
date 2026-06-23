using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class ForestTreeConfig : IEntityConfig
{
	public const string ID = "ForestTree";

	public const string SEED_ID = "ForestTreeSeed";

	public const float FERTILIZATION_RATE = 1f / 60f;

	public const float WATER_RATE = 7f / 60f;

	public const float BRANCH_GROWTH_TIME = 2100f;

	public const int NUM_BRANCHES = 7;

	public GameObject CreatePrefab()
	{
		string name = STRINGS.CREATURES.SPECIES.WOOD_TREE.NAME;
		string desc = STRINGS.CREATURES.SPECIES.WOOD_TREE.DESC;
		EffectorValues tIER = DECOR.BONUS.TIER1;
		KAnimFile anim = Assets.GetAnim("tree_kanim");
		List<Tag> additionalTags = new List<Tag>();
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("ForestTree", name, desc, 2f, anim, "idle_empty", Grid.SceneLayer.Building, 1, 2, tIER, default(EffectorValues), SimHashes.Creature, additionalTags, 298.15f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 258.15f, 288.15f, 313.15f, 448.15f, null, pressure_sensitive: true, 0f, 0.15f, "WoodLog", can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: false, 2400f, 0f, 9800f, "ForestTreeOriginal", STRINGS.CREATURES.SPECIES.WOOD_TREE.NAME);
		PlantBranchGrower.Def def = gameObject.AddOrGetDef<PlantBranchGrower.Def>();
		def.preventStartSMIOnSpawn = true;
		def.onBranchSpawned = RollChancesForSeed;
		def.onBranchHarvested = RollChancesForSeed;
		def.onEarlySpawn = TranslateOldBranchesToNewSystem;
		def.BRANCH_PREFAB_NAME = "ForestTreeBranch";
		def.harvestOnDrown = true;
		def.MAX_BRANCH_COUNT = 5;
		def.BRANCH_OFFSETS = new CellOffset[7]
		{
			new CellOffset(-1, 0),
			new CellOffset(-1, 1),
			new CellOffset(-1, 2),
			new CellOffset(0, 2),
			new CellOffset(1, 2),
			new CellOffset(1, 1),
			new CellOffset(1, 0)
		};
		gameObject.AddOrGet<BuddingTrunk>();
		gameObject.AddOrGet<DirectlyEdiblePlant_TreeBranches>();
		gameObject.UpdateComponentRequirement<Harvestable>(required: false);
		Tag tag = ElementLoader.FindElementByHash(SimHashes.DirtyWater).tag;
		EntityTemplates.ExtendPlantToIrrigated(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = tag,
				massConsumptionRate = 7f / 60f
			}
		});
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = GameTags.Dirt,
				massConsumptionRate = 1f / 60f
			}
		});
		gameObject.AddComponent<StandardCropPlant>().wiltsOnReadyToHarvest = true;
		gameObject.AddComponent<ForestTreeSeedMonitor>();
		IHasDlcRestrictions dlcRestrictions = this as IHasDlcRestrictions;
		string name2 = STRINGS.CREATURES.SPECIES.SEEDS.WOOD_TREE.NAME;
		string desc2 = STRINGS.CREATURES.SPECIES.SEEDS.WOOD_TREE.DESC;
		KAnimFile anim2 = Assets.GetAnim("seed_tree_kanim");
		List<Tag> additionalTags2 = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.WOOD_TREE.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, dlcRestrictions, SeedProducer.ProductionType.Hidden, "ForestTreeSeed", name2, desc2, anim2, "object", 1, additionalTags2, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 4, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f), "ForestTree_preview", Assets.GetAnim("tree_kanim"), "place", 3, 3);
		return gameObject;
	}

	public void RollChancesForSeed(PlantBranch.Instance branch_smi, PlantBranchGrower.Instance trunk_smi)
	{
		trunk_smi.GetComponent<ForestTreeSeedMonitor>().TryRollNewSeed();
	}

	public void TranslateOldBranchesToNewSystem(PlantBranchGrower.Instance smi)
	{
		KPrefabID[] andForgetOldSerializedBranches = smi.GetComponent<BuddingTrunk>().GetAndForgetOldSerializedBranches();
		if (andForgetOldSerializedBranches != null)
		{
			smi.ManuallyDefineBranchArray(andForgetOldSerializedBranches);
		}
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
