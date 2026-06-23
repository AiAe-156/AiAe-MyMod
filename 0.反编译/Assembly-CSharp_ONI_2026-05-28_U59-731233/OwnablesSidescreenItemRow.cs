using System;
using STRINGS;
using UnityEngine;

public class OwnablesSidescreenItemRow : KMonoBehaviour
{
	private static string EMPTY_TEXT = UI.UISIDESCREENS.OWNABLESSIDESCREEN.NO_ITEM_ASSIGNED;

	public KImage lockedIcon;

	public KImage itemIcon;

	public LocText textLabel;

	public ToolTip tooltip;

	[Header("Icon settings")]
	public KImage frameOuterBorder;

	public Action<OwnablesSidescreenItemRow> OnSlotRowClicked;

	public MultiToggle toggle;

	private int subscribe_IDX = -1;

	public bool IsLocked { get; private set; }

	public bool SlotIsAssigned => Slot != null && SlotInstance != null && !SlotInstance.IsUnassigning() && SlotInstance.IsAssigned();

	public AssignableSlotInstance SlotInstance { get; private set; }

	public AssignableSlot Slot { get; private set; }

	public Assignables Owner { get; private set; }

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		MultiToggle multiToggle = toggle;
		multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, new System.Action(OnRowClicked));
		SetSelectedVisualState(shouldDisplayAsSelected: false);
	}

	private void OnRowClicked()
	{
		OnSlotRowClicked?.Invoke(this);
	}

	public void SetLockState(bool locked)
	{
		IsLocked = locked;
		Refresh();
	}

	public void SetData(Assignables owner, AssignableSlot slot, bool IsLocked)
	{
		if (Owner != null)
		{
			ClearData();
		}
		Owner = owner;
		Slot = slot;
		SlotInstance = owner.GetSlot(slot);
		subscribe_IDX = Owner.Subscribe(-1585839766, delegate
		{
			Refresh();
		});
		SetLockState(IsLocked);
		if (!IsLocked)
		{
			Refresh();
		}
	}

	public void ClearData()
	{
		if (Owner != null && subscribe_IDX != -1)
		{
			Owner.Unsubscribe(subscribe_IDX);
		}
		Owner = null;
		Slot = null;
		SlotInstance = null;
		IsLocked = false;
		subscribe_IDX = -1;
		DisplayAsEmpty();
	}

	private void Refresh()
	{
		if (!this.IsNullOrDestroyed())
		{
			if (IsLocked)
			{
				DisplayAsLocked();
			}
			else if (!SlotIsAssigned)
			{
				DisplayAsEmpty();
			}
			else
			{
				DisplayAsOccupied();
			}
		}
	}

	public void SetSelectedVisualState(bool shouldDisplayAsSelected)
	{
		int new_state_index = (shouldDisplayAsSelected ? 1 : 0);
		toggle.ChangeState(new_state_index);
	}

	private void DisplayAsOccupied()
	{
		Assignable assignable = SlotInstance.assignable;
		string properName = assignable.GetProperName();
		string text = Slot.Name + ": " + properName;
		textLabel.SetText(text);
		itemIcon.sprite = Def.GetUISprite(assignable.gameObject).first;
		itemIcon.gameObject.SetActive(value: true);
		lockedIcon.gameObject.SetActive(value: false);
		InfoDescription component = assignable.gameObject.GetComponent<InfoDescription>();
		string simpleTooltip = string.Format(UI.UISIDESCREENS.OWNABLESSIDESCREEN.TOOLTIPS.ITEM_ASSIGNED_GENERIC, properName);
		if (component != null && !string.IsNullOrEmpty(component.description))
		{
			simpleTooltip = string.Format(UI.UISIDESCREENS.OWNABLESSIDESCREEN.TOOLTIPS.ITEM_ASSIGNED, properName, component.description);
		}
		tooltip.SetSimpleTooltip(simpleTooltip);
	}

	private void DisplayAsEmpty()
	{
		textLabel.SetText(((Slot != null) ? (Slot.Name + ": ") : "") + EMPTY_TEXT);
		lockedIcon.gameObject.SetActive(value: false);
		itemIcon.sprite = null;
		itemIcon.gameObject.SetActive(value: false);
		tooltip.SetSimpleTooltip((Slot != null) ? string.Format(UI.UISIDESCREENS.OWNABLESSIDESCREEN.TOOLTIPS.NO_ITEM_ASSIGNED, Slot.Name) : null);
	}

	private void DisplayAsLocked()
	{
		lockedIcon.gameObject.SetActive(value: true);
		itemIcon.sprite = null;
		itemIcon.gameObject.SetActive(value: false);
		textLabel.SetText(string.Format(UI.UISIDESCREENS.OWNABLESSIDESCREEN.NO_APPLICABLE, Slot.Name));
		tooltip.SetSimpleTooltip(string.Format(UI.UISIDESCREENS.OWNABLESSIDESCREEN.TOOLTIPS.NO_APPLICABLE, Slot.Name));
	}

	protected override void OnCleanUp()
	{
		ClearData();
	}
}
