using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using KSerialization;
using UnityEngine;

public abstract class Assignable : KMonoBehaviour, ISaveLoadable
{
	public string slotID;

	private AssignableSlot _slot;

	public IAssignableIdentity assignee;

	[Serialize]
	protected Ref<KMonoBehaviour> assignee_identityRef = new Ref<KMonoBehaviour>();

	[Serialize]
	protected string assignee_slotInstanceID;

	[Serialize]
	private string assignee_groupID = "";

	public AssignableSlot[] subSlots;

	public bool canBePublic;

	[Serialize]
	private bool canBeAssigned = true;

	public int priority;

	private List<Func<MinionAssignablesProxy, bool>> autoassignmentPreconditions = new List<Func<MinionAssignablesProxy, bool>>();

	private List<Func<MinionAssignablesProxy, bool>> assignmentPreconditions = new List<Func<MinionAssignablesProxy, bool>>();

	public Func<Assignables, string> customAssignmentUITooltipFunc;

	public Func<Assignables, string> customAssignablesUITooltipFunc;

	public AssignableSlot slot
	{
		get
		{
			if (_slot == null)
			{
				_slot = Db.Get().AssignableSlots.Get(slotID);
			}
			return _slot;
		}
	}

	public bool CanBeAssigned => canBeAssigned;

	public event Action<IAssignableIdentity> OnAssign;

	[OnDeserialized]
	internal void OnDeserialized()
	{
	}

	private void RestoreAssignee()
	{
		IAssignableIdentity savedAssignee = GetSavedAssignee();
		if (savedAssignee != null)
		{
			AssignableSlotInstance savedSlotInstance = GetSavedSlotInstance(savedAssignee);
			Assign(savedAssignee, savedSlotInstance);
		}
	}

	private AssignableSlotInstance GetSavedSlotInstance(IAssignableIdentity savedAsignee)
	{
		if ((savedAsignee != null && savedAsignee is MinionIdentity) || savedAsignee is StoredMinionIdentity || savedAsignee is MinionAssignablesProxy)
		{
			Ownables soleOwner = savedAsignee.GetSoleOwner();
			if (soleOwner != null)
			{
				AssignableSlotInstance[] slots = soleOwner.GetSlots(slot);
				if (slots != null)
				{
					AssignableSlotInstance assignableSlotInstance = slots.FindFirst((AssignableSlotInstance i) => i.ID == assignee_slotInstanceID);
					if (assignableSlotInstance != null)
					{
						return assignableSlotInstance;
					}
				}
			}
			Equipment component = soleOwner.GetComponent<Equipment>();
			if (component != null)
			{
				AssignableSlotInstance[] slots2 = component.GetSlots(slot);
				if (slots2 != null)
				{
					AssignableSlotInstance assignableSlotInstance2 = slots2.FindFirst((AssignableSlotInstance i) => i.ID == assignee_slotInstanceID);
					if (assignableSlotInstance2 != null)
					{
						return assignableSlotInstance2;
					}
				}
			}
		}
		return null;
	}

	private IAssignableIdentity GetSavedAssignee()
	{
		if (assignee_identityRef.Get() != null)
		{
			return assignee_identityRef.Get().GetComponent<IAssignableIdentity>();
		}
		if (!string.IsNullOrEmpty(assignee_groupID))
		{
			return Game.Instance.assignmentManager.assignment_groups[assignee_groupID];
		}
		return null;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		RestoreAssignee();
		Components.AssignableItems.Add(this);
		Game.Instance.assignmentManager.Add(this);
		if (assignee == null && canBePublic)
		{
			Assign(Game.Instance.assignmentManager.assignment_groups["public"]);
		}
		assignmentPreconditions.Add(delegate(MinionAssignablesProxy proxy)
		{
			GameObject targetGameObject = proxy.GetTargetGameObject();
			return (targetGameObject.GetComponent<KMonoBehaviour>().GetMyWorldId() == this.GetMyWorldId() || targetGameObject.IsMyParentWorld(base.gameObject)) ? true : false;
		});
		autoassignmentPreconditions.Add(delegate
		{
			Operational component = GetComponent<Operational>();
			return !(component != null) || component.IsOperational;
		});
	}

	protected override void OnCleanUp()
	{
		Unassign();
		Components.AssignableItems.Remove(this);
		Game.Instance.assignmentManager.Remove(this);
		base.OnCleanUp();
	}

	public bool CanAutoAssignTo(IAssignableIdentity identity)
	{
		MinionAssignablesProxy minionAssignablesProxy = identity as MinionAssignablesProxy;
		if (minionAssignablesProxy == null)
		{
			return true;
		}
		if (!CanAssignTo(minionAssignablesProxy))
		{
			return false;
		}
		foreach (Func<MinionAssignablesProxy, bool> autoassignmentPrecondition in autoassignmentPreconditions)
		{
			if (!autoassignmentPrecondition(minionAssignablesProxy))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanAssignTo(IAssignableIdentity identity)
	{
		MinionAssignablesProxy minionAssignablesProxy = identity as MinionAssignablesProxy;
		if (minionAssignablesProxy == null)
		{
			return true;
		}
		foreach (Func<MinionAssignablesProxy, bool> assignmentPrecondition in assignmentPreconditions)
		{
			if (!assignmentPrecondition(minionAssignablesProxy))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsAssigned()
	{
		return assignee != null;
	}

	public bool IsAssignedTo(IAssignableIdentity identity)
	{
		Debug.Assert(identity != null, "IsAssignedTo identity is null");
		Ownables soleOwner = identity.GetSoleOwner();
		Debug.Assert(soleOwner != null, "IsAssignedTo identity sole owner is null");
		if (assignee != null)
		{
			foreach (Ownables owner in assignee.GetOwners())
			{
				Debug.Assert(owner, "Assignable owners list contained null");
				if (owner.gameObject == soleOwner.gameObject)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual void Assign(IAssignableIdentity new_assignee)
	{
		Assign(new_assignee, null);
	}

	public virtual void Assign(IAssignableIdentity new_assignee, AssignableSlotInstance specificSlotInstance)
	{
		if (new_assignee == assignee)
		{
			return;
		}
		if (new_assignee is KMonoBehaviour)
		{
			if (!CanAssignTo(new_assignee))
			{
				return;
			}
			assignee_identityRef.Set((KMonoBehaviour)new_assignee);
			assignee_groupID = "";
		}
		else if (new_assignee is AssignmentGroup)
		{
			assignee_identityRef.Set(null);
			assignee_groupID = ((AssignmentGroup)new_assignee).id;
		}
		GetComponent<KPrefabID>().AddTag(GameTags.Assigned);
		assignee = new_assignee;
		assignee_slotInstanceID = null;
		if (slot != null && (new_assignee is MinionIdentity || new_assignee is StoredMinionIdentity || new_assignee is MinionAssignablesProxy))
		{
			if (specificSlotInstance == null)
			{
				Ownables soleOwner = new_assignee.GetSoleOwner();
				if (soleOwner != null)
				{
					AssignableSlotInstance assignableSlotInstance = soleOwner.GetSlot(slot);
					if (assignableSlotInstance != null)
					{
						assignee_slotInstanceID = assignableSlotInstance.ID;
						assignableSlotInstance.Assign(this);
					}
				}
				Equipment component = soleOwner.GetComponent<Equipment>();
				if (component != null)
				{
					AssignableSlotInstance assignableSlotInstance2 = component.GetSlot(slot);
					if (assignableSlotInstance2 != null)
					{
						assignee_slotInstanceID = assignableSlotInstance2.ID;
						assignableSlotInstance2.Assign(this);
					}
				}
			}
			else
			{
				assignee_slotInstanceID = specificSlotInstance.ID;
				specificSlotInstance.Assign(this);
			}
		}
		if (this.OnAssign != null)
		{
			this.OnAssign(new_assignee);
		}
		Trigger(684616645, (object)new_assignee);
	}

	public virtual void Unassign()
	{
		if (assignee == null)
		{
			return;
		}
		GetComponent<KPrefabID>().RemoveTag(GameTags.Assigned);
		if (slot != null)
		{
			Ownables soleOwner = assignee.GetSoleOwner();
			if ((bool)soleOwner)
			{
				(soleOwner.GetSlots(slot)?.FindFirst((AssignableSlotInstance s) => s.assignable == this))?.Unassign();
				Equipment component = soleOwner.GetComponent<Equipment>();
				if (component != null)
				{
					(component.GetSlots(slot)?.FindFirst((AssignableSlotInstance s) => s.assignable == this))?.Unassign();
				}
			}
		}
		assignee = null;
		if (canBePublic)
		{
			Assign(Game.Instance.assignmentManager.assignment_groups["public"]);
		}
		assignee_slotInstanceID = null;
		assignee_identityRef.Set(null);
		assignee_groupID = "";
		if (this.OnAssign != null)
		{
			this.OnAssign(null);
		}
		Trigger(684616645);
	}

	public void SetCanBeAssigned(bool state)
	{
		canBeAssigned = state;
	}

	public void AddAssignPrecondition(Func<MinionAssignablesProxy, bool> precondition)
	{
		assignmentPreconditions.Add(precondition);
	}

	public void AddAutoassignPrecondition(Func<MinionAssignablesProxy, bool> precondition)
	{
		autoassignmentPreconditions.Add(precondition);
	}

	public int GetNavigationCost(Navigator navigator)
	{
		int num = -1;
		int cell = Grid.PosToCell(this);
		IApproachable component = GetComponent<IApproachable>();
		CellOffset[] array = ((component != null) ? component.GetOffsets() : new CellOffset[1]);
		DebugUtil.DevAssert(navigator != null, "Navigator is mysteriously null");
		if (navigator == null)
		{
			return -1;
		}
		CellOffset[] array2 = array;
		foreach (CellOffset offset in array2)
		{
			int cell2 = Grid.OffsetCell(cell, offset);
			int navigationCost = navigator.GetNavigationCost(cell2);
			if (navigationCost != -1 && (num == -1 || navigationCost < num))
			{
				num = navigationCost;
			}
		}
		return num;
	}
}
