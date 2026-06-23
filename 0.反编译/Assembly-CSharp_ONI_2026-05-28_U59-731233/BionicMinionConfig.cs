using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class BionicMinionConfig : IEntityConfig, IHasDlcRestrictions
{
	public static Tag MODEL = GameTags.Minions.Models.Bionic;

	public static string NAME = DUPLICANTS.MODEL.BIONIC.NAME;

	public static string ID = MODEL.ToString();

	public static string[] DEFAULT_BIONIC_TRAITS = new string[1] { "BionicBaseline" };

	public Func<RationalAi.Instance, StateMachine.Instance>[] RATIONAL_AI_STATE_MACHINES = BaseMinionConfig.BaseRationalAiStateMachines().Append(new Func<RationalAi.Instance, StateMachine.Instance>[10]
	{
		(RationalAi.Instance smi) => new BreathMonitor.Instance(smi.master)
		{
			canRecoverBreath = false
		},
		(RationalAi.Instance smi) => new SteppedInMonitor.Instance(smi.master, new string[1] { "CarpetFeet" }),
		(RationalAi.Instance smi) => new BionicBatteryMonitor.Instance(smi.master, new BionicBatteryMonitor.Def()),
		(RationalAi.Instance smi) => new BionicBedTimeMonitor.Instance(smi.master, new BionicBedTimeMonitor.Def()),
		(RationalAi.Instance smi) => new BionicMicrochipMonitor.Instance(smi.master, new BionicMicrochipMonitor.Def()),
		(RationalAi.Instance smi) => new BionicOilMonitor.Instance(smi.master, new BionicOilMonitor.Def()),
		(RationalAi.Instance smi) => new GunkMonitor.Instance(smi.master, new GunkMonitor.Def()),
		(RationalAi.Instance smi) => new BionicWaterDamageMonitor.Instance(smi.master, new BionicWaterDamageMonitor.Def()),
		(RationalAi.Instance smi) => new BionicUpgradesMonitor.Instance(smi.master, new BionicUpgradesMonitor.Def()),
		(RationalAi.Instance smi) => new BionicOxygenTankMonitor.Instance(smi.master, new BionicOxygenTankMonitor.Def())
	});

	public static string[] GetAttributes()
	{
		return BaseMinionConfig.BaseMinionAttributes().Append(new string[2]
		{
			Db.Get().Attributes.BionicBoosterSlots.Id,
			Db.Get().Attributes.BionicBatteryCountCapacity.Id
		});
	}

	public static string[] GetAmounts()
	{
		return BaseMinionConfig.BaseMinionAmounts().Append(new string[4]
		{
			Db.Get().Amounts.BionicOil.Id,
			Db.Get().Amounts.BionicGunk.Id,
			Db.Get().Amounts.BionicInternalBattery.Id,
			Db.Get().Amounts.BionicOxygenTank.Id
		});
	}

	public static AttributeModifier[] GetTraits()
	{
		return BaseMinionConfig.BaseMinionTraits(MODEL);
	}

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC3;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = BaseMinionConfig.BaseMinion(MODEL, GetAttributes(), GetAmounts(), GetTraits());
		CodexEntryRedirector codexEntryRedirector = gameObject.AddOrGet<CodexEntryRedirector>();
		codexEntryRedirector.CodexID = "DUPLICANTS";
		AttributeLevels attributeLevels = gameObject.AddOrGet<AttributeLevels>();
		attributeLevels.maxAttributeLevel = 0;
		Storage storage = gameObject.AddComponent<Storage>();
		storage.storageID = GameTags.StoragesIds.BionicBatteryStorage;
		storage.SetDefaultStoredItemModifiers(new List<Storage.StoredItemModifier>
		{
			Storage.StoredItemModifier.Hide,
			Storage.StoredItemModifier.Preserve,
			Storage.StoredItemModifier.Seal,
			Storage.StoredItemModifier.Insulate
		});
		storage.storageFilters = new List<Tag>(GameTags.BionicCompatibleBatteries);
		storage.allowItemRemoval = false;
		storage.showInUI = false;
		Storage storage2 = gameObject.AddComponent<Storage>();
		storage2.storageID = GameTags.StoragesIds.BionicUpgradeStorage;
		storage2.SetDefaultStoredItemModifiers(new List<Storage.StoredItemModifier>
		{
			Storage.StoredItemModifier.Hide,
			Storage.StoredItemModifier.Preserve,
			Storage.StoredItemModifier.Seal,
			Storage.StoredItemModifier.Insulate
		});
		storage2.storageFilters = new List<Tag> { GameTags.BionicUpgrade };
		storage2.allowItemRemoval = false;
		storage2.showInUI = false;
		Storage storage3 = gameObject.AddComponent<Storage>();
		storage3.capacityKg = BionicOxygenTankMonitor.OXYGEN_TANK_CAPACITY_KG;
		storage3.storageID = GameTags.StoragesIds.BionicOxygenTankStorage;
		storage3.SetDefaultStoredItemModifiers(new List<Storage.StoredItemModifier>
		{
			Storage.StoredItemModifier.Hide,
			Storage.StoredItemModifier.Preserve,
			Storage.StoredItemModifier.Seal,
			Storage.StoredItemModifier.Insulate
		});
		storage3.allowItemRemoval = false;
		storage3.showInUI = false;
		ManualDeliveryKG manualDeliveryKG = gameObject.AddComponent<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.FetchCritical.IdHash;
		manualDeliveryKG.capacity = 0f;
		manualDeliveryKG.refillMass = 0f;
		manualDeliveryKG.handlePrioritizable = false;
		gameObject.AddOrGet<ReanimateBionicWorkable>();
		WarmBlooded warmBlooded = gameObject.AddOrGet<WarmBlooded>();
		warmBlooded.complexity = WarmBlooded.ComplexityType.HomeostasisWithoutCaloriesImpact;
		gameObject.AddOrGet<BionicMinionStorageExtension>();
		gameObject.AddOrGet<MinionStorageDataHolder>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject go)
	{
		BaseMinionConfig.BasePrefabInit(go, MODEL);
		AmountInstance amountInstance = Db.Get().Amounts.BionicOil.Lookup(go);
		amountInstance.value = amountInstance.GetMax();
		AmountInstance amountInstance2 = Db.Get().Amounts.BionicGunk.Lookup(go);
		amountInstance2.value = amountInstance2.GetMin();
	}

	public void OnSpawn(GameObject go)
	{
		Sensors component = go.GetComponent<Sensors>();
		component.Add(new ClosestElectrobankSensor(component, shouldStartActive: true));
		component.Add(new ClosestOxygenCanisterSensor(component, shouldStartActive: false));
		component.Add(new ClosestLubricantSensor(component, shouldStartActive: false));
		BaseMinionConfig.BaseOnSpawn(go, MODEL, RATIONAL_AI_STATE_MACHINES);
		SafeCellSensor sensor = component.GetSensor<SafeCellSensor>();
		sensor.AddIgnoredFlagsSet(ID, SafeCellQuery.SafeFlags.IsBreathable);
		BionicOxygenTankMonitor.Instance sMI = go.GetSMI<BionicOxygenTankMonitor.Instance>();
		if (sMI != null)
		{
			go.GetComponent<OxygenBreather>().AddGasProvider(sMI);
		}
		BionicFreeDiscoveries(go);
		go.Trigger(1589886948, (object)go);
	}

	private void BionicFreeDiscoveries(GameObject instance)
	{
		GameScheduler.Instance.Schedule("BionicUnlockCraftingTable", 8f, delegate
		{
			TechItem techItem = Db.Get().TechItems.Get("CraftingTable");
			if (!techItem.IsComplete())
			{
				Notifier component = Game.Instance.GetComponent<Notifier>();
				Notification notification = new Notification(MISC.NOTIFICATIONS.BIONICRESEARCHUNLOCK.NAME, NotificationType.MessageImportant, (List<Notification> notificationList, object obj) => MISC.NOTIFICATIONS.BIONICRESEARCHUNLOCK.MESSAGEBODY.Replace("{0}", Assets.GetPrefab("CraftingTable").GetProperName()), Assets.GetPrefab("CraftingTable").GetProperName(), expires: true, 0f, null, null, null, volume_attenuation: true, clear_on_click: true);
				component.Add(notification);
				techItem.POIUnlocked();
			}
			DiscoveredResources.Instance.Discover(PowerControlStationConfig.TINKER_TOOLS);
		});
	}
}
