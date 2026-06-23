using KSerialization;
using STRINGS;
using TUNING;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/Uprootable")]
public class Uprootable : Workable, IDigActionEntity
{
	[Serialize]
	protected bool isMarkedForUproot;

	protected bool uprootComplete = false;

	[MyCmpReq]
	private Prioritizable prioritizable;

	[SerializeField]
	public HashedString choreTypeIdHash;

	[Serialize]
	protected bool canBeUprooted = true;

	public bool deselectOnUproot = true;

	protected Chore chore;

	private string buttonLabel;

	private string buttonTooltip;

	private string cancelButtonLabel;

	private string cancelButtonTooltip;

	private StatusItem pendingStatusItem;

	public OccupyArea area;

	private Storage planterStorage;

	public bool showUserMenuButtons = true;

	public HandleVector<int>.Handle partitionerEntry;

	private static readonly EventSystem.IntraObjectHandler<Uprootable> OnPlanterStorageDelegate = new EventSystem.IntraObjectHandler<Uprootable>(delegate(Uprootable component, object data)
	{
		component.OnPlanterStorage(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Uprootable> ForceCancelUprootDelegate = new EventSystem.IntraObjectHandler<Uprootable>(delegate(Uprootable component, object data)
	{
		component.ForceCancelUproot(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Uprootable> OnCancelDelegate = new EventSystem.IntraObjectHandler<Uprootable>(delegate(Uprootable component, object data)
	{
		component.OnCancel(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Uprootable> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<Uprootable>(delegate(Uprootable component, object data)
	{
		component.OnRefreshUserMenu(data);
	});

	public bool IsMarkedForUproot => isMarkedForUproot;

	public Storage GetPlanterStorage => planterStorage;

	public bool CanUproot()
	{
		return canBeUprooted && !uprootComplete;
	}

	public static bool CanUproot(GameObject plant, out Uprootable uprootable)
	{
		if (plant == null)
		{
			uprootable = null;
			return false;
		}
		uprootable = plant.GetComponent<Uprootable>();
		return uprootable != null && uprootable.CanUproot();
	}

	public static bool CanUproot(GameObject plant)
	{
		Uprootable uprootable;
		return CanUproot(plant, out uprootable);
	}

	protected Uprootable()
	{
		SetOffsetTable(OffsetGroups.InvertedStandardTable);
		buttonLabel = UI.USERMENUACTIONS.UPROOT.NAME;
		buttonTooltip = UI.USERMENUACTIONS.UPROOT.TOOLTIP;
		cancelButtonLabel = UI.USERMENUACTIONS.CANCELUPROOT.NAME;
		cancelButtonTooltip = UI.USERMENUACTIONS.CANCELUPROOT.TOOLTIP;
		pendingStatusItem = Db.Get().MiscStatusItems.PendingUproot;
		workerStatusItem = Db.Get().DuplicantStatusItems.Uprooting;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		pendingStatusItem = Db.Get().MiscStatusItems.PendingUproot;
		workerStatusItem = Db.Get().DuplicantStatusItems.Uprooting;
		attributeConverter = Db.Get().AttributeConverters.HarvestSpeed;
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
		skillExperienceSkillGroup = Db.Get().SkillGroups.Farming.Id;
		skillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
		multitoolContext = "harvest";
		multitoolHitEffectTag = "fx_harvest_splash";
		Subscribe(1309017699, OnPlanterStorageDelegate);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Subscribe(2127324410, ForceCancelUprootDelegate);
		SetWorkTime(12.5f);
		Subscribe(2127324410, OnCancelDelegate);
		Subscribe(493375141, OnRefreshUserMenuDelegate);
		faceTargetWhenWorking = true;
		Components.Uprootables.Add(this);
		area = GetComponent<OccupyArea>();
		Prioritizable.AddRef(base.gameObject);
		base.gameObject.AddTag(GameTags.Plant);
		Extents extents = new Extents(Grid.PosToCell(base.gameObject), base.gameObject.GetComponent<OccupyArea>().OccupiedCellsOffsets);
		partitionerEntry = GameScenePartitioner.Instance.Add(base.gameObject.name, base.gameObject.GetComponent<KPrefabID>(), extents, GameScenePartitioner.Instance.plants, null);
		GameScenePartitioner.Instance.TriggerEvent(extents, GameScenePartitioner.Instance.plantsChangedLayer, this);
		if (isMarkedForUproot)
		{
			MarkForUproot();
		}
	}

	private void OnPlanterStorage(object data)
	{
		planterStorage = (Storage)data;
		Prioritizable component = GetComponent<Prioritizable>();
		if (component != null)
		{
			component.showIcon = planterStorage == null;
		}
	}

	public bool IsInPlanterBox()
	{
		return planterStorage != null;
	}

	public virtual void Uproot()
	{
		isMarkedForUproot = false;
		chore = null;
		uprootComplete = true;
		Trigger(-216549700, (object)this);
		GetComponent<KSelectable>().RemoveStatusItem(Db.Get().MiscStatusItems.PendingUproot);
		GetComponent<KSelectable>().RemoveStatusItem(Db.Get().MiscStatusItems.Operating);
		Game.Instance.userMenu.Refresh(base.gameObject);
	}

	public void SetCanBeUprooted(bool state)
	{
		canBeUprooted = state;
		if (canBeUprooted)
		{
			SetUprootedComplete(state: false);
		}
		Game.Instance.userMenu.Refresh(base.gameObject);
	}

	public void SetUprootedComplete(bool state)
	{
		uprootComplete = state;
	}

	public void MarkForUproot(bool instantOnDebug = true)
	{
		if (canBeUprooted)
		{
			if (DebugHandler.InstantBuildMode && instantOnDebug)
			{
				Uproot();
			}
			else if (chore == null)
			{
				ChoreType chore_type = (choreTypeIdHash.IsValid ? Db.Get().ChoreTypes.GetByHash(choreTypeIdHash) : Db.Get().ChoreTypes.Uproot);
				chore = new WorkChore<Uprootable>(chore_type, this);
				GetComponent<KSelectable>().AddStatusItem(pendingStatusItem, this);
			}
			isMarkedForUproot = true;
		}
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		Uproot();
	}

	private void OnCancel(object _)
	{
		if (chore != null)
		{
			chore.Cancel("Cancel uproot");
			chore = null;
			GetComponent<KSelectable>().RemoveStatusItem(Db.Get().MiscStatusItems.PendingUproot);
		}
		isMarkedForUproot = false;
		choreTypeIdHash = HashedString.Invalid;
		Game.Instance.userMenu.Refresh(base.gameObject);
		Trigger(1198393204);
	}

	public bool HasChore()
	{
		if (chore == null)
		{
			return false;
		}
		return true;
	}

	private void OnClickUproot()
	{
		MarkForUproot();
	}

	protected void OnClickCancelUproot()
	{
		OnCancel(null);
	}

	public virtual void ForceCancelUproot(object _ = null)
	{
		OnCancel(null);
	}

	private void OnRefreshUserMenu(object data)
	{
		if (!showUserMenuButtons)
		{
			return;
		}
		if (uprootComplete)
		{
			if (deselectOnUproot)
			{
				KSelectable component = GetComponent<KSelectable>();
				if (component != null && SelectTool.Instance.selected == component)
				{
					SelectTool.Instance.Select(null);
				}
			}
		}
		else if (canBeUprooted)
		{
			KIconButtonMenu.ButtonInfo button = ((chore != null) ? new KIconButtonMenu.ButtonInfo("action_uproot", cancelButtonLabel, OnClickCancelUproot, Action.NumActions, null, null, null, cancelButtonTooltip) : new KIconButtonMenu.ButtonInfo("action_uproot", buttonLabel, OnClickUproot, Action.NumActions, null, null, null, buttonTooltip));
			Game.Instance.userMenu.AddButton(base.gameObject, button);
		}
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		Extents extents = new Extents(Grid.PosToCell(base.gameObject), base.gameObject.GetComponent<OccupyArea>().OccupiedCellsOffsets);
		GameScenePartitioner.Instance.Free(ref partitionerEntry);
		GameScenePartitioner.Instance.TriggerEvent(extents, GameScenePartitioner.Instance.plantsChangedLayer, this);
		Components.Uprootables.Remove(this);
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		base.OnStartWork(worker);
		GetComponent<KSelectable>().RemoveStatusItem(Db.Get().MiscStatusItems.PendingUproot);
	}

	public void Dig()
	{
		Uproot();
	}

	public void MarkForDig(bool instantOnDebug = true)
	{
		MarkForUproot(instantOnDebug);
	}
}
