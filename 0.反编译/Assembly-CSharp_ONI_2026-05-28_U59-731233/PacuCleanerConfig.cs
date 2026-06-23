using STRINGS;
using TUNING;
using UnityEngine;

[EntityConfigOrder(2)]
public class PacuCleanerConfig : IEntityConfig
{
	public const string ID = "PacuCleaner";

	public const string BASE_TRAIT_ID = "PacuCleanerBaseTrait";

	public const string EGG_ID = "PacuCleanerEgg";

	public const float POLLUTED_WATER_CONVERTED_PER_CYCLE = 120f;

	public const SimHashes INPUT_ELEMENT = SimHashes.DirtyWater;

	public static SimHashes OUTPUT_ELEMENT = SimHashes.Water;

	public static readonly EffectorValues DECOR = TUNING.BUILDINGS.DECOR.BONUS.TIER4;

	public const int EGG_SORT_ORDER = 501;

	public static GameObject CreatePacu(string id, string name, string desc, string anim_file, bool is_baby)
	{
		GameObject prefab = BasePacuConfig.CreatePrefab(id, "PacuCleanerBaseTrait", name, desc, anim_file, is_baby, "glp_", 243.15f, 278.15f, 223.15f, 298.15f);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, PacuTuning.PEN_SIZE_PER_CREATURE, add_fixed_capturable_monitor: true);
		if (!is_baby)
		{
			Storage storage = prefab.AddComponent<Storage>();
			storage.capacityKg = 10f;
			storage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
			ElementConsumer elementConsumer = prefab.AddOrGet<PassiveElementConsumer>();
			elementConsumer.elementToConsume = SimHashes.DirtyWater;
			elementConsumer.consumptionRate = 0.2f;
			elementConsumer.capacityKG = 10f;
			elementConsumer.consumptionRadius = 3;
			elementConsumer.showInStatusPanel = true;
			elementConsumer.sampleCellOffset = new Vector3(0f, 0f, 0f);
			elementConsumer.isRequired = false;
			elementConsumer.storeOnConsume = true;
			elementConsumer.showDescriptor = false;
			prefab.AddOrGet<UpdateElementConsumerPosition>();
			BubbleSpawner bubbleSpawner = prefab.AddComponent<BubbleSpawner>();
			bubbleSpawner.emitMass = 2f;
			bubbleSpawner.emitVariance = 0.5f;
			bubbleSpawner.element = OUTPUT_ELEMENT;
			ElementConverter elementConverter = prefab.AddOrGet<ElementConverter>();
			elementConverter.consumedElements = new ElementConverter.ConsumedElement[1]
			{
				new ElementConverter.ConsumedElement(SimHashes.DirtyWater.CreateTag(), 0.2f)
			};
			elementConverter.outputElements = new ElementConverter.OutputElement[1]
			{
				new ElementConverter.OutputElement(0.2f, OUTPUT_ELEMENT, 0f, useEntityTemperature: true, storeOutput: true)
			};
		}
		return prefab;
	}

	public GameObject CreatePrefab()
	{
		GameObject prefab = CreatePacu("PacuCleaner", STRINGS.CREATURES.SPECIES.PACU.VARIANT_CLEANER.NAME, STRINGS.CREATURES.SPECIES.PACU.VARIANT_CLEANER.DESC, "pacu_kanim", is_baby: false);
		prefab = EntityTemplates.ExtendEntityToWildCreature(prefab, PacuTuning.PEN_SIZE_PER_CREATURE, add_fixed_capturable_monitor: true);
		return EntityTemplates.ExtendEntityToFertileCreature(prefab, this as IHasDlcRestrictions, "PacuCleanerEgg", STRINGS.CREATURES.SPECIES.PACU.VARIANT_CLEANER.EGG_NAME, STRINGS.CREATURES.SPECIES.PACU.VARIANT_CLEANER.DESC, "egg_pacu_kanim", PacuTuning.EGG_MASS, PacuTuning.EGG_SHELL_RATIO, "PacuCleanerBaby", 15.000001f, 5f, PacuTuning.EGG_CHANCES_CLEANER, 501, is_ranchable: true, add_fish_overcrowding_monitor: true, 0.75f, deprecated: false, preventEggFromDroppingProducts: false, PacuTuning.EGG_MASS);
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
		if (inst.TryGetComponent<ElementConsumer>(out var component))
		{
			component.EnableConsumption(enabled: true);
		}
	}
}
