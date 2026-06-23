using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class OwnablesSecondSideScreen : KScreen
{
	public MultiToggle noneRow;

	public OwnablesSecondSideScreenRow originalRow;

	public System.Action OnScreenDeactivated;

	private List<OwnablesSecondSideScreenRow> itemRows = new List<OwnablesSecondSideScreenRow>();

	public AssignableSlotInstance Slot { get; private set; }

	public IAssignableIdentity OwnerIdentity { get; private set; }

	public AssignableSlot SlotType => (Slot == null) ? null : Slot.slot;

	public Assignable CurrentSlotItem => HasItem ? Slot.assignable : null;

	public bool HasItem => Slot != null && Slot.IsAssigned();

	protected override void OnSpawn()
	{
		base.OnSpawn();
		originalRow.gameObject.SetActive(value: false);
		MultiToggle multiToggle = noneRow;
		multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, new System.Action(OnNoneRowClicked));
	}

	private void OnNoneRowClicked()
	{
		UnassignCurrentItem();
		RefreshNoneRow();
	}

	protected override void OnCmpDisable()
	{
		SetSlot(null);
		base.OnCmpDisable();
	}

	public void SetSlot(AssignableSlotInstance slot)
	{
		Components.AssignableItems.Unregister(OnNewItemAvailable, OnItemUnregistered);
		Slot = slot;
		OwnerIdentity = slot?.assignables.GetComponent<IAssignableIdentity>();
		if (Slot != null)
		{
			Components.AssignableItems.Register(OnNewItemAvailable, OnItemUnregistered);
		}
		RefreshItemListOptions(sortRows: true);
	}

	public void SortRows()
	{
		if (itemRows != null)
		{
			itemRows.Sort((OwnablesSecondSideScreenRow a, OwnablesSecondSideScreenRow b) => string.Compare(UI.StripLinkFormatting(a.nameLabel.text), UI.StripLinkFormatting(b.nameLabel.text)) * -1);
			OwnablesSecondSideScreenRow ownablesSecondSideScreenRow = null;
			for (int num = 0; num < itemRows.Count; num++)
			{
				OwnablesSecondSideScreenRow ownablesSecondSideScreenRow2 = itemRows[num];
				if (ownablesSecondSideScreenRow2.item == null || ownablesSecondSideScreenRow2.item.IsAssigned())
				{
					if (ownablesSecondSideScreenRow == null && ownablesSecondSideScreenRow2 != null && ownablesSecondSideScreenRow2.item != null && ownablesSecondSideScreenRow2.item.IsAssigned() && ownablesSecondSideScreenRow2.item == CurrentSlotItem)
					{
						ownablesSecondSideScreenRow = ownablesSecondSideScreenRow2;
					}
					else
					{
						ownablesSecondSideScreenRow2.transform.SetAsLastSibling();
					}
				}
				else
				{
					ownablesSecondSideScreenRow2.transform.SetAsFirstSibling();
				}
			}
			if (ownablesSecondSideScreenRow != null)
			{
				ownablesSecondSideScreenRow.transform.SetAsFirstSibling();
			}
		}
		noneRow.transform.SetAsFirstSibling();
	}

	public void RefreshItemListOptions(bool sortRows = false)
	{
		GameObject gameObject = ((OwnerIdentity == null) ? null : OwnerIdentity.GetOwners()[0].GetComponent<MinionAssignablesProxy>().GetTargetGameObject());
		int worldID = ((OwnerIdentity == null) ? 255 : gameObject.GetMyWorldId());
		List<Assignable> list = null;
		int b = 0;
		bool showItemsAssignedToOthers = true;
		if (Slot != null && (Slot is EquipmentSlotInstance || Slot.ID.Contains("BionicUpgrade")))
		{
			showItemsAssignedToOthers = false;
		}
		if (worldID != 255)
		{
			list = Components.AssignableItems.Items.FindAll(delegate(Assignable i)
			{
				bool flag = i.slotID == SlotType.Id && i.CanAssignTo(OwnerIdentity);
				if (flag && i is Equippable)
				{
					Equippable equippable = i as Equippable;
					GameObject targetGameObject = equippable.gameObject;
					if (equippable.isEquipped)
					{
						targetGameObject = equippable.assignee.GetOwners()[0].GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
					}
					flag = flag && targetGameObject.GetMyWorldId() == worldID;
				}
				bool flag2 = i.assignee != null && i.assignee.GetSoleOwner() == OwnerIdentity.GetSoleOwner();
				bool flag3 = flag2 && Slot.assignable == i;
				if (!showItemsAssignedToOthers)
				{
					if (i.assignee != null && !flag2)
					{
						flag = false;
					}
					if (flag2 && !flag3)
					{
						flag = false;
					}
				}
				return flag;
			});
			b = list.Count;
		}
		for (int num = 0; num < Mathf.Max(itemRows.Count, b); num++)
		{
			if (list != null && num < list.Count)
			{
				Assignable assignable = list[num];
				if (num >= itemRows.Count)
				{
					OwnablesSecondSideScreenRow item = CreateItemRow(assignable);
					itemRows.Add(item);
				}
				OwnablesSecondSideScreenRow ownablesSecondSideScreenRow = itemRows[num];
				ownablesSecondSideScreenRow.gameObject.SetActive(value: true);
				ownablesSecondSideScreenRow.SetData(Slot, assignable);
			}
			else
			{
				OwnablesSecondSideScreenRow ownablesSecondSideScreenRow2 = itemRows[num];
				ownablesSecondSideScreenRow2.ClearData();
				ownablesSecondSideScreenRow2.gameObject.SetActive(value: false);
			}
		}
		if (sortRows)
		{
			SortRows();
		}
		RefreshNoneRow();
	}

	private void RefreshNoneRow()
	{
		noneRow.ChangeState((!HasItem) ? 1 : 0);
	}

	private OwnablesSecondSideScreenRow CreateItemRow(Assignable item)
	{
		OwnablesSecondSideScreenRow component = Util.KInstantiateUI(originalRow.gameObject, originalRow.transform.parent.gameObject).GetComponent<OwnablesSecondSideScreenRow>();
		component.OnRowClicked = (Action<OwnablesSecondSideScreenRow>)Delegate.Combine(component.OnRowClicked, new Action<OwnablesSecondSideScreenRow>(OnItemRowClicked));
		component.OnRowItemAssigneeChanged = (Action<OwnablesSecondSideScreenRow>)Delegate.Combine(component.OnRowItemAssigneeChanged, new Action<OwnablesSecondSideScreenRow>(OnItemRowAsigneeChanged));
		component.OnRowItemDestroyed = (Action<OwnablesSecondSideScreenRow>)Delegate.Combine(component.OnRowItemDestroyed, new Action<OwnablesSecondSideScreenRow>(OnItemDestroyed));
		return component;
	}

	private void OnItemDestroyed(OwnablesSecondSideScreenRow correspondingItemRow)
	{
		correspondingItemRow.ClearData();
		correspondingItemRow.gameObject.SetActive(value: false);
	}

	private void OnItemRowAsigneeChanged(OwnablesSecondSideScreenRow correspondingItemRow)
	{
		correspondingItemRow.Refresh();
		RefreshNoneRow();
	}

	private void OnItemRowClicked(OwnablesSecondSideScreenRow rowClicked)
	{
		Assignable item = rowClicked.item;
		bool flag = item.IsAssigned() && item.assignee is AssignmentGroup;
		bool flag2 = item.IsAssigned() && item.IsAssignedTo(OwnerIdentity) && !flag && Slot.IsAssigned() && Slot.assignable == item;
		if (item.IsAssigned())
		{
			item.Unassign();
		}
		if (!flag2)
		{
			item.Assign(OwnerIdentity, Slot);
		}
		rowClicked.Refresh();
		RefreshNoneRow();
	}

	private void UnassignCurrentItem()
	{
		if (Slot != null)
		{
			Slot.Unassign();
			RefreshItemListOptions();
		}
	}

	private void OnNewItemAvailable(Assignable item)
	{
		if (Slot != null && item.slotID == SlotType.Id)
		{
			RefreshItemListOptions();
		}
	}

	private void OnItemUnregistered(Assignable item)
	{
		if (Slot != null && item.slotID == SlotType.Id)
		{
			RefreshItemListOptions();
		}
	}
}
