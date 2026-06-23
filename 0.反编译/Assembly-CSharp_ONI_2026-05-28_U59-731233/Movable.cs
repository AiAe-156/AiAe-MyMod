using System;
using System.Runtime.CompilerServices;
using KSerialization;
using STRINGS;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/Workable/Movable")]
public class Movable : Workable
{
	[MyCmpReq]
	private Pickupable pickupable;

	public Tag tagRequiredForMove = Tag.Invalid;

	[Serialize]
	private bool isMarkedForMove;

	[Serialize]
	private Ref<Storage> storageProxy = null;

	private int storageReachableChangedHandle = -1;

	private int reachableChangedHandle = -1;

	private int cancelHandle = -1;

	private int tagsChangedHandle = -1;

	private Guid pendingMoveGuid;

	private Guid storageUnreachableGuid;

	public Action<GameObject> onDeliveryComplete = null;

	public Action<GameObject> onPickupComplete = null;

	private static Action<object, object> OnReachableChangedDispatcher = delegate(object context, object data)
	{
		Unsafe.As<Movable>(context).OnReachableChanged(data);
	};

	private static Action<object, object> OnSplitFromChunkDispatcher = delegate(object context, object data)
	{
		Unsafe.As<Movable>(context).OnSplitFromChunk(data);
	};

	private static Action<object, object> CleanupMoveDispatcher = delegate(object context, object data)
	{
		Unsafe.As<Movable>(context).CleanupMove(data);
	};

	private static Action<object, object> OnTagsChangedDispatcher = delegate(object context, object data)
	{
		Unsafe.As<Movable>(context).OnTagsChanged(data);
	};

	private static Action<object, object> OnRefreshUserMenuDispatcher = delegate(object context, object data)
	{
		Unsafe.As<Movable>(context).OnRefreshUserMenu(data);
	};

	public bool IsMarkedForMove => isMarkedForMove;

	public Storage StorageProxy => (storageProxy != null) ? storageProxy.Get() : null;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Subscribe(493375141, OnRefreshUserMenuDispatcher, this);
		Subscribe(1335436905, OnSplitFromChunkDispatcher, this);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (isMarkedForMove)
		{
			if (StorageProxy != null)
			{
				if (reachableChangedHandle < 0)
				{
					reachableChangedHandle = Subscribe(-1432940121, OnReachableChangedDispatcher, this);
				}
				if (storageReachableChangedHandle < 0)
				{
					storageReachableChangedHandle = StorageProxy.Subscribe(-1432940121, OnReachableChangedDispatcher, this);
				}
				if (cancelHandle < 0)
				{
					cancelHandle = Subscribe(2127324410, CleanupMoveDispatcher, this);
				}
				if (tagsChangedHandle < 0)
				{
					tagsChangedHandle = Subscribe(-1582839653, OnTagsChangedDispatcher, this);
				}
				base.gameObject.AddTag(GameTags.MarkedForMove);
			}
			else
			{
				isMarkedForMove = false;
			}
		}
		if (IsCritterPickupable(base.gameObject))
		{
			skillsUpdateHandle = Game.Instance.Subscribe(-1523247426, Workable.UpdateStatusItemDispatcher, this);
			shouldShowSkillPerkStatusItem = isMarkedForMove;
			requiredSkillPerk = Db.Get().SkillPerks.CanWrangleCreatures.Id;
			UpdateStatusItem();
		}
	}

	private void OnReachableChanged(object _)
	{
		if (!isMarkedForMove)
		{
			return;
		}
		if (StorageProxy != null)
		{
			int num = Grid.PosToCell(pickupable);
			int num2 = Grid.PosToCell(StorageProxy);
			if (num != num2)
			{
				bool flag = MinionGroupProber.Get().IsReachable(num, OffsetGroups.Standard) && MinionGroupProber.Get().IsReachable(num2, OffsetGroups.Standard);
				if (pickupable.KPrefabID.HasTag(GameTags.Creatures.Confined))
				{
					flag = false;
				}
				KSelectable component = GetComponent<KSelectable>();
				pendingMoveGuid = component.ToggleStatusItem(Db.Get().MiscStatusItems.MarkedForMove, pendingMoveGuid, flag, this);
				storageUnreachableGuid = component.ToggleStatusItem(Db.Get().MiscStatusItems.MoveStorageUnreachable, storageUnreachableGuid, !flag, this);
			}
		}
		else
		{
			ClearMove();
		}
	}

	private void OnSplitFromChunk(object data)
	{
		Pickupable pickupable = data as Pickupable;
		if (pickupable != null)
		{
			Movable component = pickupable.GetComponent<Movable>();
			if (component.isMarkedForMove)
			{
				storageProxy = new Ref<Storage>(component.StorageProxy);
				MarkForMove();
			}
		}
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		if (isMarkedForMove && StorageProxy != null)
		{
			StorageProxy.GetComponent<CancellableMove>().RemoveMovable(this);
			ClearStorageProxy();
		}
	}

	private void CleanupMove(object _)
	{
		if (StorageProxy != null)
		{
			StorageProxy.GetComponent<CancellableMove>().OnCancel(this);
		}
	}

	private void OnTagsChanged(object data)
	{
		if (isMarkedForMove && !HasTagRequiredToMove() && StorageProxy != null)
		{
			StorageProxy.GetComponent<CancellableMove>().OnCancel(this);
		}
	}

	public void ClearMove()
	{
		if (isMarkedForMove)
		{
			isMarkedForMove = false;
			KSelectable component = GetComponent<KSelectable>();
			pendingMoveGuid = component.RemoveStatusItem(pendingMoveGuid);
			storageUnreachableGuid = component.RemoveStatusItem(storageUnreachableGuid);
			ClearStorageProxy();
			base.gameObject.RemoveTag(GameTags.MarkedForMove);
			Unsubscribe(ref reachableChangedHandle);
			Unsubscribe(ref cancelHandle);
			Unsubscribe(ref tagsChangedHandle);
		}
		UpdateStatusItem();
	}

	private void ClearStorageProxy()
	{
		StorageProxy.Unsubscribe(ref storageReachableChangedHandle);
		storageProxy = null;
	}

	private void OnClickMove()
	{
		MoveToLocationTool.Instance.Activate(this);
	}

	private void OnClickCancel()
	{
		if (StorageProxy != null)
		{
			StorageProxy.GetComponent<CancellableMove>().OnCancel(this);
		}
	}

	private void OnRefreshUserMenu(object data)
	{
		if (!pickupable.KPrefabID.HasTag(GameTags.Stored) && HasTagRequiredToMove())
		{
			KIconButtonMenu.ButtonInfo button = (isMarkedForMove ? new KIconButtonMenu.ButtonInfo("action_control", UI.USERMENUACTIONS.PICKUPABLEMOVE.NAME_OFF, OnClickCancel, Action.NumActions, null, null, null, UI.USERMENUACTIONS.PICKUPABLEMOVE.TOOLTIP_OFF) : new KIconButtonMenu.ButtonInfo("action_control", UI.USERMENUACTIONS.PICKUPABLEMOVE.NAME, OnClickMove, Action.NumActions, null, null, null, UI.USERMENUACTIONS.PICKUPABLEMOVE.TOOLTIP));
			Game.Instance.userMenu.AddButton(base.gameObject, button);
		}
	}

	private bool HasTagRequiredToMove()
	{
		return tagRequiredForMove == Tag.Invalid || pickupable.KPrefabID.HasTag(tagRequiredForMove);
	}

	public void MoveToLocation(int cell)
	{
		CreateStorageProxy(cell);
		MarkForMove();
		base.gameObject.Trigger(1122777325, (object)base.gameObject);
	}

	private void MarkForMove()
	{
		Trigger(2127324410);
		isMarkedForMove = true;
		OnReachableChanged(null);
		storageReachableChangedHandle = StorageProxy.Subscribe(-1432940121, OnReachableChangedDispatcher, this);
		reachableChangedHandle = Subscribe(-1432940121, OnReachableChangedDispatcher, this);
		StorageProxy.GetComponent<CancellableMove>().SetMovable(this);
		base.gameObject.AddTag(GameTags.MarkedForMove);
		cancelHandle = Subscribe(2127324410, CleanupMoveDispatcher, this);
		tagsChangedHandle = Subscribe(-1582839653, OnTagsChangedDispatcher, this);
		UpdateStatusItem();
	}

	private void UpdateStatusItem()
	{
		if (IsCritterPickupable(base.gameObject))
		{
			shouldShowSkillPerkStatusItem = isMarkedForMove;
			base.UpdateStatusItem();
		}
	}

	public bool CanMoveTo(int cell)
	{
		return !Grid.IsSolidCell(cell) && Grid.IsWorldValidCell(cell) && base.gameObject.IsMyParentWorld(cell);
	}

	private void CreateStorageProxy(int cell)
	{
		if (storageProxy == null || storageProxy.Get() == null)
		{
			if (Grid.Objects[cell, 44] != null)
			{
				GameObject gameObject = Grid.Objects[cell, 44];
				Storage component = gameObject.GetComponent<Storage>();
				storageProxy = new Ref<Storage>(component);
			}
			else
			{
				Vector3 position = Grid.CellToPosCBC(cell, MoveToLocationTool.Instance.visualizerLayer);
				GameObject gameObject2 = Util.KInstantiate(Assets.GetPrefab(MovePickupablePlacerConfig.ID), position);
				Storage component2 = gameObject2.GetComponent<Storage>();
				gameObject2.SetActive(value: true);
				storageProxy = new Ref<Storage>(component2);
			}
		}
	}

	public static bool IsCritterPickupable(GameObject pickupable_go)
	{
		return pickupable_go.GetComponent<Capturable>();
	}
}
