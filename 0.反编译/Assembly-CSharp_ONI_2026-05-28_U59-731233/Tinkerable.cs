using System.Collections.Generic;
using FMOD.Studio;
using KSerialization;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/Tinkerable")]
public class Tinkerable : Workable
{
	private Chore chore;

	[MyCmpGet]
	private Storage storage;

	[MyCmpGet]
	private Effects effects;

	[MyCmpGet]
	private RoomTracker roomTracker;

	public Tag tinkerMaterialTag;

	public float tinkerMaterialAmount;

	public float tinkerMass;

	public string addedEffect;

	public string effectAttributeId;

	public float effectMultiplier;

	public string[] boostSymbolNames;

	public string onCompleteSFX;

	public HashedString choreTypeTinker = Db.Get().ChoreTypes.PowerTinker.IdHash;

	public HashedString choreTypeFetch = Db.Get().ChoreTypes.PowerFetch.IdHash;

	[Serialize]
	private bool userMenuAllowed = true;

	private static readonly EventSystem.IntraObjectHandler<Tinkerable> OnEffectRemovedDelegate = new EventSystem.IntraObjectHandler<Tinkerable>(delegate(Tinkerable component, object data)
	{
		component.OnEffectRemoved(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Tinkerable> OnStorageChangeDelegate = new EventSystem.IntraObjectHandler<Tinkerable>(delegate(Tinkerable component, object data)
	{
		component.OnStorageChange(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Tinkerable> OnUpdateRoomDelegate = new EventSystem.IntraObjectHandler<Tinkerable>(delegate(Tinkerable component, object data)
	{
		component.OnUpdateRoom(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Tinkerable> OnOperationalChangedDelegate = new EventSystem.IntraObjectHandler<Tinkerable>(delegate(Tinkerable component, object data)
	{
		component.OnOperationalChanged(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Tinkerable> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<Tinkerable>(delegate(Tinkerable component, object data)
	{
		component.OnRefreshUserMenu(data);
	});

	private bool prioritizableAdded = false;

	private SchedulerHandle updateHandle;

	private bool hasReservedMaterial = false;

	public static Tinkerable MakePowerTinkerable(GameObject prefab)
	{
		RoomTracker roomTracker = prefab.AddOrGet<RoomTracker>();
		roomTracker.requiredRoomType = Db.Get().RoomTypes.PowerPlant.Id;
		roomTracker.requirement = RoomTracker.Requirement.TrackingOnly;
		Tinkerable tinkerable = prefab.AddOrGet<Tinkerable>();
		tinkerable.tinkerMaterialTag = PowerControlStationConfig.TINKER_TOOLS;
		tinkerable.tinkerMaterialAmount = 1f;
		tinkerable.tinkerMass = 5f;
		tinkerable.requiredSkillPerk = PowerControlStationConfig.ROLE_PERK;
		tinkerable.onCompleteSFX = "Generator_Microchip_installed";
		tinkerable.boostSymbolNames = new string[2] { "booster", "blue_light_bloom" };
		tinkerable.SetWorkTime(30f);
		tinkerable.workerStatusItem = Db.Get().DuplicantStatusItems.Tinkering;
		tinkerable.attributeConverter = Db.Get().AttributeConverters.MachinerySpeed;
		tinkerable.attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
		tinkerable.choreTypeTinker = Db.Get().ChoreTypes.PowerTinker.IdHash;
		tinkerable.choreTypeFetch = Db.Get().ChoreTypes.PowerFetch.IdHash;
		tinkerable.addedEffect = "PowerTinker";
		tinkerable.effectAttributeId = Db.Get().Attributes.Machinery.Id;
		tinkerable.effectMultiplier = 0.025f;
		tinkerable.multitoolContext = "powertinker";
		tinkerable.multitoolHitEffectTag = "fx_powertinker_splash";
		tinkerable.shouldShowSkillPerkStatusItem = false;
		prefab.AddOrGet<Storage>();
		prefab.AddOrGet<Effects>();
		KPrefabID component = prefab.GetComponent<KPrefabID>();
		component.prefabInitFn += delegate(GameObject inst)
		{
			inst.GetComponent<Tinkerable>().SetOffsetTable(OffsetGroups.InvertedStandardTable);
		};
		return tinkerable;
	}

	public static Tinkerable MakeFarmTinkerable(GameObject prefab)
	{
		RoomTracker roomTracker = prefab.AddOrGet<RoomTracker>();
		roomTracker.requiredRoomType = Db.Get().RoomTypes.Farm.Id;
		roomTracker.requirement = RoomTracker.Requirement.TrackingOnly;
		Tinkerable tinkerable = prefab.AddOrGet<Tinkerable>();
		tinkerable.tinkerMaterialTag = FarmStationConfig.TINKER_TOOLS;
		tinkerable.tinkerMaterialAmount = 1f;
		tinkerable.tinkerMass = 5f;
		tinkerable.requiredSkillPerk = Db.Get().SkillPerks.CanFarmTinker.Id;
		tinkerable.workerStatusItem = Db.Get().DuplicantStatusItems.Tinkering;
		tinkerable.addedEffect = "FarmTinker";
		tinkerable.effectAttributeId = Db.Get().Attributes.Botanist.Id;
		tinkerable.effectMultiplier = 0.1f;
		tinkerable.SetWorkTime(15f);
		tinkerable.attributeConverter = Db.Get().AttributeConverters.PlantTendSpeed;
		tinkerable.attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
		tinkerable.choreTypeTinker = Db.Get().ChoreTypes.CropTend.IdHash;
		tinkerable.choreTypeFetch = Db.Get().ChoreTypes.FarmFetch.IdHash;
		tinkerable.multitoolContext = "tend";
		tinkerable.multitoolHitEffectTag = "fx_tend_splash";
		tinkerable.shouldShowSkillPerkStatusItem = false;
		prefab.AddOrGet<Storage>();
		prefab.AddOrGet<Effects>();
		KPrefabID component = prefab.GetComponent<KPrefabID>();
		component.prefabInitFn += delegate(GameObject inst)
		{
			inst.GetComponent<Tinkerable>().SetOffsetTable(OffsetGroups.InvertedStandardTable);
		};
		return tinkerable;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_use_machine_kanim") };
		workerStatusItem = Db.Get().DuplicantStatusItems.Tinkering;
		attributeConverter = Db.Get().AttributeConverters.MachinerySpeed;
		faceTargetWhenWorking = true;
		synchronizeAnims = false;
		Subscribe(-1157678353, OnEffectRemovedDelegate);
		Subscribe(-1697596308, OnStorageChangeDelegate);
		Subscribe(144050788, OnUpdateRoomDelegate);
		Subscribe(-592767678, OnOperationalChangedDelegate);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Prioritizable.AddRef(base.gameObject);
		prioritizableAdded = true;
		Subscribe(493375141, OnRefreshUserMenuDelegate);
		UpdateVisual();
	}

	protected override void OnCleanUp()
	{
		UpdateMaterialReservation(shouldReserve: false);
		if (updateHandle.IsValid)
		{
			updateHandle.ClearScheduler();
		}
		if (prioritizableAdded)
		{
			Prioritizable.RemoveRef(base.gameObject);
		}
		base.OnCleanUp();
	}

	private void OnOperationalChanged(object _)
	{
		QueueUpdateChore();
	}

	private void OnEffectRemoved(object _)
	{
		QueueUpdateChore();
	}

	private void OnUpdateRoom(object _)
	{
		QueueUpdateChore();
	}

	private void OnStorageChange(object data)
	{
		GameObject go = (GameObject)data;
		if (go.IsPrefabID(tinkerMaterialTag))
		{
			QueueUpdateChore();
		}
	}

	private void QueueUpdateChore()
	{
		if (updateHandle.IsValid)
		{
			updateHandle.ClearScheduler();
		}
		updateHandle = GameScheduler.Instance.Schedule("UpdateTinkerChore", 1.2f, UpdateChoreCallback);
	}

	private void UpdateChoreCallback(object obj)
	{
		UpdateChore();
	}

	private void UpdateChore()
	{
		Operational component = GetComponent<Operational>();
		bool flag = component == null || component.IsFunctional;
		bool flag2 = HasEffect();
		bool flag3 = HasCorrectRoom();
		bool flag4 = !flag2 && flag && flag3 && userMenuAllowed;
		bool flag5 = flag2 || !flag3 || !userMenuAllowed;
		if (chore == null && flag4)
		{
			UpdateMaterialReservation(shouldReserve: true);
			if (HasMaterial())
			{
				chore = new WorkChore<Tinkerable>(Db.Get().ChoreTypes.GetByHash(choreTypeTinker), this, null, run_until_complete: true, null, null, null, allow_in_red_alert: true, null, ignore_schedule_block: false, only_when_operational: false);
				if (component != null)
				{
					chore.AddPrecondition(ChorePreconditions.instance.IsFunctional, component);
				}
			}
			else
			{
				chore = new FetchChore(Db.Get().ChoreTypes.GetByHash(choreTypeFetch), storage, tinkerMaterialAmount * tinkerMass, new HashSet<Tag> { tinkerMaterialTag }, FetchChore.MatchCriteria.MatchID, Tag.Invalid, null, null, run_until_complete: true, OnFetchComplete, null, null, Operational.State.Functional);
			}
			chore.AddPrecondition(ChorePreconditions.instance.HasSkillPerk, requiredSkillPerk);
			RoomTracker component2 = GetComponent<RoomTracker>();
			if (!string.IsNullOrEmpty(component2.requiredRoomType))
			{
				chore.AddPrecondition(ChorePreconditions.instance.IsInMyRoom, Grid.PosToCell(base.transform.GetPosition()));
			}
		}
		else if (chore != null && flag5)
		{
			UpdateMaterialReservation(shouldReserve: false);
			chore.Cancel("No longer needed");
			chore = null;
		}
	}

	private bool HasCorrectRoom()
	{
		return roomTracker.IsInCorrectRoom();
	}

	private bool RoomHasTinkerstation()
	{
		if (!roomTracker.IsInCorrectRoom())
		{
			return false;
		}
		if (roomTracker.room == null)
		{
			return false;
		}
		foreach (KPrefabID building in roomTracker.room.buildings)
		{
			if (!(building == null))
			{
				TinkerStation component = building.GetComponent<TinkerStation>();
				if (component != null && component.outputPrefab == tinkerMaterialTag)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void UpdateMaterialReservation(bool shouldReserve)
	{
		if (shouldReserve && !hasReservedMaterial)
		{
			MaterialNeeds.UpdateNeed(tinkerMaterialTag, tinkerMaterialAmount, base.gameObject.GetMyWorldId());
			hasReservedMaterial = shouldReserve;
		}
		else if (!shouldReserve && hasReservedMaterial)
		{
			MaterialNeeds.UpdateNeed(tinkerMaterialTag, 0f - tinkerMaterialAmount, base.gameObject.GetMyWorldId());
			hasReservedMaterial = shouldReserve;
		}
	}

	private void OnFetchComplete(Chore data)
	{
		UpdateMaterialReservation(shouldReserve: false);
		chore = null;
		UpdateChore();
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		base.OnCompleteWork(worker);
		storage.ConsumeIgnoringDisease(tinkerMaterialTag, tinkerMaterialAmount);
		float totalValue = worker.GetAttributes().Get(Db.Get().Attributes.Get(effectAttributeId)).GetTotalValue();
		effects.Add(addedEffect, should_save: true).timeRemaining *= 1f + totalValue * effectMultiplier;
		UpdateVisual();
		UpdateMaterialReservation(shouldReserve: false);
		chore = null;
		UpdateChore();
		string sound = GlobalAssets.GetSound(onCompleteSFX);
		if (sound != null)
		{
			EventInstance instance = SoundEvent.BeginOneShot(sound, base.transform.position);
			SoundEvent.EndOneShot(instance);
		}
	}

	private void UpdateVisual()
	{
		if (boostSymbolNames != null)
		{
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			bool is_visible = effects.HasEffect(addedEffect);
			string[] array = boostSymbolNames;
			foreach (string text in array)
			{
				component.SetSymbolVisiblity(text, is_visible);
			}
		}
	}

	private bool HasMaterial()
	{
		return storage.GetAmountAvailable(tinkerMaterialTag) >= tinkerMaterialAmount;
	}

	private bool HasEffect()
	{
		return effects.HasEffect(addedEffect);
	}

	private void OnRefreshUserMenu(object data)
	{
		if (roomTracker.IsInCorrectRoom())
		{
			string arg = Db.Get().effects.Get(addedEffect).Name;
			string properName = this.GetProperName();
			KIconButtonMenu.ButtonInfo button = (userMenuAllowed ? new KIconButtonMenu.ButtonInfo("action_switch_toggle", UI.USERMENUACTIONS.TINKER.DISALLOW, OnClickToggleTinker, Action.NumActions, null, null, null, string.Format(UI.USERMENUACTIONS.TINKER.TOOLTIP_DISALLOW, arg, properName)) : new KIconButtonMenu.ButtonInfo("action_switch_toggle", UI.USERMENUACTIONS.TINKER.ALLOW, OnClickToggleTinker, Action.NumActions, null, null, null, string.Format(UI.USERMENUACTIONS.TINKER.TOOLTIP_ALLOW, arg, properName)));
			Game.Instance.userMenu.AddButton(base.gameObject, button);
		}
	}

	private void OnClickToggleTinker()
	{
		userMenuAllowed = !userMenuAllowed;
		UpdateChore();
	}
}
