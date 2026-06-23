using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class FishFeederConfig : IBuildingConfig
{
	public const string ID = "FishFeeder";

	private static HashSet<Tag> forbiddenTags = new HashSet<Tag>();

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("FishFeeder", 1, 3, "fishfeeder_kanim", 100, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.RAW_METALS, 1600f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		buildingDef.AudioCategory = "Metal";
		buildingDef.Entombable = true;
		buildingDef.Floodable = false;
		buildingDef.ForegroundLayer = Grid.SceneLayer.TileMain;
		buildingDef.AddSearchTerms(SEARCH_TERMS.RANCHING);
		buildingDef.AddSearchTerms(SEARCH_TERMS.CRITTER);
		return buildingDef;
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		Prioritizable.AddRef(go);
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = 200f;
		storage.showInUI = true;
		storage.showDescriptor = true;
		storage.allowItemRemoval = false;
		storage.allowSettingOnlyFetchMarkedItems = false;
		storage.showCapacityStatusItem = true;
		storage.showCapacityAsMainStatus = true;
		storage.dropOffset = Vector2.up * 1f;
		storage.storageID = new Tag("FishFeederTop");
		Storage storage2 = go.AddComponent<Storage>();
		storage2.capacityKg = 200f;
		storage2.showInUI = true;
		storage2.showDescriptor = true;
		storage2.allowItemRemoval = false;
		storage2.dropOffset = Vector2.up * 3.5f;
		storage2.storageID = new Tag("FishFeederBot");
		StorageLocker storageLocker = go.AddOrGet<StorageLocker>();
		storageLocker.choreTypeID = Db.Get().ChoreTypes.RanchingFetch.Id;
		go.AddOrGet<UserNameable>();
		go.AddOrGet<TreeFilterable>().filterAllStoragesOnBuilding = true;
		CreatureFeeder creatureFeeder = go.AddOrGet<CreatureFeeder>();
		creatureFeeder.effectId = "AteFromFeeder";
		creatureFeeder.feederOffset = new CellOffset(0, -2);
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.prefabInitFn += OnPrefabInit;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGetDef<StorageController.Def>();
		go.AddOrGetDef<FishFeeder.Def>();
		MakeBaseSolid.Def def = go.AddOrGetDef<MakeBaseSolid.Def>();
		def.solidOffsets = new CellOffset[1]
		{
			new CellOffset(0, 0)
		};
		SymbolOverrideControllerUtil.AddToPrefab(go);
	}

	public override void ConfigurePost(BuildingDef def)
	{
		List<Tag> list = new List<Tag>();
		Tag[] target_species = new Tag[6]
		{
			GameTags.Creatures.Species.PacuSpecies,
			GameTags.Creatures.Species.PrehistoricPacuSpecies,
			GameTags.Creatures.Species.ParrotFishSpecies,
			GameTags.Creatures.Species.PufferFishSpecies,
			GameTags.Creatures.Species.SeaHorseSpecies,
			GameTags.Creatures.Species.SeaTurtleSpecies
		};
		Dictionary<Tag, Diet> dictionary = DietManager.CollectDiets(target_species);
		foreach (KeyValuePair<Tag, Diet> item in dictionary)
		{
			Diet value = item.Value;
			if (value.CanEatPreyCritter)
			{
				Diet.Info[] preyInfos = value.preyInfos;
				foreach (Diet.Info info in preyInfos)
				{
					foreach (Tag consumedTag in info.consumedTags)
					{
						forbiddenTags.Add(consumedTag);
					}
				}
			}
			list.Add(item.Key);
		}
		def.BuildingComplete.GetComponent<Storage>().storageFilters = list;
	}

	private void OnPrefabInit(GameObject instance)
	{
		TreeFilterable component = instance.GetComponent<TreeFilterable>();
		foreach (Tag forbiddenTag in forbiddenTags)
		{
			component.ForbiddenTags.Add(forbiddenTag);
		}
		KBatchedAnimController[] componentsInChildrenOnly = instance.GetComponentsInChildrenOnly<KBatchedAnimController>();
		foreach (KBatchedAnimController kBatchedAnimController in componentsInChildrenOnly)
		{
			if (kBatchedAnimController.name.Contains("_fg"))
			{
				kBatchedAnimController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.LiquidVisibilityLayer, isActive: false);
				kBatchedAnimController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.WaterProof, isActive: true);
			}
		}
	}
}
