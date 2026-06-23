using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class CreatureFeederConfig : IBuildingConfig
{
	public const string ID = "CreatureFeeder";

	private static HashSet<Tag> forbiddenTags = new HashSet<Tag>();

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("CreatureFeeder", 1, 2, "feeder_kanim", 100, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.RAW_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		buildingDef.AudioCategory = "Metal";
		return buildingDef;
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		Prioritizable.AddRef(go);
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = 2000f;
		storage.showInUI = true;
		storage.showDescriptor = true;
		storage.allowItemRemoval = false;
		storage.allowSettingOnlyFetchMarkedItems = false;
		storage.showCapacityStatusItem = true;
		storage.showCapacityAsMainStatus = true;
		StorageLocker storageLocker = go.AddOrGet<StorageLocker>();
		storageLocker.choreTypeID = Db.Get().ChoreTypes.RanchingFetch.Id;
		go.AddOrGet<UserNameable>();
		go.AddOrGet<TreeFilterable>();
		Effect effect = new Effect("AteFromFeeder", STRINGS.CREATURES.MODIFIERS.ATE_FROM_FEEDER.NAME, STRINGS.CREATURES.MODIFIERS.ATE_FROM_FEEDER.TOOLTIP, 1200f, show_in_ui: true, trigger_floating_text: false, is_bad: false);
		effect.Add(new AttributeModifier(Db.Get().Amounts.Wildness.deltaAttribute.Id, -1f / 60f, STRINGS.CREATURES.MODIFIERS.ATE_FROM_FEEDER.NAME));
		Db.Get().effects.Add(effect);
		CreatureFeeder creatureFeeder = go.AddOrGet<CreatureFeeder>();
		creatureFeeder.effectId = effect.Id;
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.prefabInitFn += OnPrefabInit;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGetDef<StorageController.Def>();
	}

	public override void ConfigurePost(BuildingDef def)
	{
		List<Tag> list = new List<Tag>();
		Tag[] target_species = new Tag[14]
		{
			GameTags.Creatures.Species.LightBugSpecies,
			GameTags.Creatures.Species.HatchSpecies,
			GameTags.Creatures.Species.MoleSpecies,
			GameTags.Creatures.Species.CrabSpecies,
			GameTags.Creatures.Species.StaterpillarSpecies,
			GameTags.Creatures.Species.DivergentSpecies,
			GameTags.Creatures.Species.DeerSpecies,
			GameTags.Creatures.Species.BellySpecies,
			GameTags.Creatures.Species.SealSpecies,
			GameTags.Creatures.Species.StegoSpecies,
			GameTags.Creatures.Species.RaptorSpecies,
			GameTags.Creatures.Species.ChameleonSpecies,
			GameTags.Creatures.Species.MooSpecies,
			GameTags.Creatures.Species.SnailSpecies
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
	}
}
