using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/ManualDeliveryKG")]
public class ManualDeliveryKG : KMonoBehaviour, ISim1000ms
{
	[MyCmpGet]
	private Operational operational;

	[SerializeField]
	private Storage storage;

	[SerializeField]
	private Tag requestedItemTag;

	private Tag[] forbiddenTags;

	[SerializeField]
	public float capacity = 100f;

	[SerializeField]
	public float refillMass = 10f;

	[SerializeField]
	public float MinimumMass = 10f;

	[SerializeField]
	public bool RoundFetchAmountToInt = false;

	[SerializeField]
	public bool FillToCapacity = false;

	[SerializeField]
	public Operational.State operationalRequirement = Operational.State.Operational;

	[SerializeField]
	public bool allowPause = false;

	[SerializeField]
	private bool paused = false;

	[SerializeField]
	public HashedString choreTypeIDHash;

	[Serialize]
	private bool userPaused;

	public bool handlePrioritizable = true;

	public bool ShowStatusItem = true;

	public bool FillToMinimumMass = false;

	private FetchList2 fetchList;

	private int onStorageChangeSubscription = -1;

	private static readonly EventSystem.IntraObjectHandler<ManualDeliveryKG> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<ManualDeliveryKG>(delegate(ManualDeliveryKG component, object data)
	{
		component.OnRefreshUserMenu(data);
	});

	private static readonly EventSystem.IntraObjectHandler<ManualDeliveryKG> OnOperationalChangedDelegate = new EventSystem.IntraObjectHandler<ManualDeliveryKG>(delegate(ManualDeliveryKG component, object data)
	{
		component.OnOperationalChanged(data);
	});

	private static readonly EventSystem.IntraObjectHandler<ManualDeliveryKG> OnStorageChangedDelegate = new EventSystem.IntraObjectHandler<ManualDeliveryKG>(delegate(ManualDeliveryKG component, object data)
	{
		component.OnStorageChanged(data);
	});

	public bool IsPaused => paused;

	public float Capacity => capacity;

	public Tag RequestedItemTag
	{
		get
		{
			return requestedItemTag;
		}
		set
		{
			requestedItemTag = value;
			AbortDelivery("Requested Item Tag Changed");
		}
	}

	public Tag[] ForbiddenTags
	{
		get
		{
			return forbiddenTags;
		}
		set
		{
			forbiddenTags = value;
			AbortDelivery("Forbidden Tags Changed");
		}
	}

	public Storage DebugStorage => storage;

	public FetchList2 DebugFetchList => fetchList;

	private float MassStored => storage.GetMassAvailable(requestedItemTag);

	protected override void OnSpawn()
	{
		base.OnSpawn();
		DebugUtil.Assert(choreTypeIDHash.IsValid, "ManualDeliveryKG Must have a valid chore type specified!", base.name);
		if (allowPause)
		{
			Subscribe(493375141, OnRefreshUserMenuDelegate);
			Subscribe(-111137758, OnRefreshUserMenuDelegate);
		}
		Subscribe(-592767678, OnOperationalChangedDelegate);
		if (storage != null)
		{
			SetStorage(storage);
		}
		if (handlePrioritizable)
		{
			Prioritizable.AddRef(base.gameObject);
		}
		if (userPaused && allowPause)
		{
			OnPause();
		}
	}

	protected override void OnCleanUp()
	{
		AbortDelivery("ManualDeliverKG destroyed");
		if (handlePrioritizable && base.isSpawned)
		{
			Prioritizable.RemoveRef(base.gameObject);
		}
		base.OnCleanUp();
	}

	public void SetStorage(Storage storage)
	{
		if (this.storage != null)
		{
			this.storage.Unsubscribe(onStorageChangeSubscription);
			onStorageChangeSubscription = -1;
		}
		AbortDelivery("storage pointer changed");
		this.storage = storage;
		if (this.storage != null && base.isSpawned)
		{
			Debug.Assert(onStorageChangeSubscription == -1);
			onStorageChangeSubscription = this.storage.Subscribe(-1697596308, OnStorageChangedDelegate);
		}
	}

	public void Pause(bool pause, string reason)
	{
		if (paused != pause)
		{
			paused = pause;
			if (pause)
			{
				AbortDelivery(reason);
			}
		}
	}

	public void Sim1000ms(float dt)
	{
		UpdateDeliveryState();
	}

	[ContextMenu("UpdateDeliveryState")]
	public void UpdateDeliveryState()
	{
		if (requestedItemTag.IsValid && !(storage == null))
		{
			UpdateFetchList();
		}
	}

	public void RequestDelivery()
	{
		if (fetchList == null)
		{
			float massStored = MassStored;
			if (massStored < capacity)
			{
				CreateFetchChore(massStored);
			}
		}
	}

	private void CreateFetchChore(float stored_mass)
	{
		float b = capacity - stored_mass;
		b = Mathf.Max(PICKUPABLETUNING.MINIMUM_PICKABLE_AMOUNT, b);
		if (RoundFetchAmountToInt)
		{
			b = (int)b;
			if (b < 0.1f)
			{
				return;
			}
		}
		ChoreType byHash = Db.Get().ChoreTypes.GetByHash(choreTypeIDHash);
		this.fetchList = new FetchList2(storage, byHash);
		this.fetchList.ShowStatusItem = ShowStatusItem;
		this.fetchList.MinimumAmount[requestedItemTag] = Mathf.Max(PICKUPABLETUNING.MINIMUM_PICKABLE_AMOUNT, MinimumMass);
		FetchList2 fetchList = this.fetchList;
		Tag obj = requestedItemTag;
		float amount = b;
		fetchList.Add(obj, forbiddenTags, amount);
		this.fetchList.Submit(OnFetchComplete, check_storage_contents: false);
	}

	private void OnFetchComplete()
	{
		if (FillToCapacity && storage != null)
		{
			float amountAvailable = storage.GetAmountAvailable(requestedItemTag);
			if (amountAvailable < capacity)
			{
				CreateFetchChore(amountAvailable);
			}
		}
	}

	private void UpdateFetchList()
	{
		if (paused)
		{
			return;
		}
		if (fetchList != null && fetchList.IsComplete)
		{
			fetchList = null;
		}
		if (!(operational == null) && !operational.MeetsRequirements(operationalRequirement))
		{
			if (fetchList != null)
			{
				fetchList.Cancel("Operational requirements");
				fetchList = null;
			}
		}
		else if (fetchList == null)
		{
			float massStored = MassStored;
			if (massStored < refillMass)
			{
				RequestDelivery();
			}
		}
		else if (FillToMinimumMass)
		{
			Dictionary<Tag, float> remaining = fetchList.GetRemaining();
			if (remaining.ContainsKey(requestedItemTag) && remaining[requestedItemTag] < MinimumMass)
			{
				AbortDelivery("Invalid Mass");
			}
		}
	}

	public void AbortDelivery(string reason)
	{
		if (this.fetchList != null)
		{
			FetchList2 fetchList = this.fetchList;
			this.fetchList = null;
			fetchList.Cancel(reason);
		}
	}

	protected void OnStorageChanged(object data)
	{
		UpdateDeliveryState();
	}

	private void OnPause()
	{
		userPaused = true;
		Pause(pause: true, "Forbid manual delivery");
	}

	private void OnResume()
	{
		userPaused = false;
		Pause(pause: false, "Allow manual delivery");
	}

	private void OnRefreshUserMenu(object data)
	{
		if (allowPause)
		{
			KIconButtonMenu.ButtonInfo button = ((!paused) ? new KIconButtonMenu.ButtonInfo("action_move_to_storage", UI.USERMENUACTIONS.MANUAL_DELIVERY.NAME, OnPause, Action.NumActions, null, null, null, UI.USERMENUACTIONS.MANUAL_DELIVERY.TOOLTIP) : new KIconButtonMenu.ButtonInfo("action_move_to_storage", UI.USERMENUACTIONS.MANUAL_DELIVERY.NAME_OFF, OnResume, Action.NumActions, null, null, null, UI.USERMENUACTIONS.MANUAL_DELIVERY.TOOLTIP_OFF));
			Game.Instance.userMenu.AddButton(base.gameObject, button);
		}
	}

	private void OnOperationalChanged(object _)
	{
		UpdateDeliveryState();
	}
}
