using System;

public class OwnablesSidescreenCategoryRow : KMonoBehaviour
{
	public struct AssignableSlotData
	{
		public AssignableSlot slot;

		public Func<IAssignableIdentity, bool> IsApplicableCallback;

		public AssignableSlotData(AssignableSlot slot, Func<IAssignableIdentity, bool> isApplicableCallback)
		{
			this.slot = slot;
			IsApplicableCallback = isApplicableCallback;
		}
	}

	public struct Data
	{
		public string name;

		public AssignableSlot[] slots;

		private AssignableSlotData[] slotsData;

		public Data(string name, AssignableSlotData[] slotsData)
		{
			this.name = name;
			this.slotsData = slotsData;
			slots = new AssignableSlot[slotsData.Length];
			for (int i = 0; i < slotsData.Length; i++)
			{
				slots[i] = slotsData[i].slot;
			}
		}

		public bool IsSlotApplicable(IAssignableIdentity identity, AssignableSlot slot)
		{
			for (int i = 0; i < slotsData.Length; i++)
			{
				AssignableSlotData assignableSlotData = slotsData[i];
				if (assignableSlotData.slot == slot)
				{
					return assignableSlotData.IsApplicableCallback(identity);
				}
			}
			return false;
		}
	}

	public Action<OwnablesSidescreenItemRow> OnSlotRowClicked;

	public LocText titleLabel;

	public OwnablesSidescreenItemRow originalItemRow;

	private Assignables owner;

	private Data data;

	private OwnablesSidescreenItemRow[] itemRows;

	private AssignableSlot[] slots => data.slots;

	public void SetCategoryData(Data categoryData)
	{
		DeleteAllRows();
		data = categoryData;
		titleLabel.text = categoryData.name;
	}

	public void SetOwner(Assignables owner)
	{
		this.owner = owner;
		if (owner != null)
		{
			RecreateAllItemRows();
		}
		else
		{
			DeleteAllRows();
		}
	}

	private void RecreateAllItemRows()
	{
		DeleteAllRows();
		itemRows = new OwnablesSidescreenItemRow[slots.Length];
		IAssignableIdentity component = owner.gameObject.GetComponent<IAssignableIdentity>();
		for (int i = 0; i < slots.Length; i++)
		{
			AssignableSlot slot = slots[i];
			itemRows[i] = CreateRow(slot, component);
		}
	}

	private OwnablesSidescreenItemRow CreateRow(AssignableSlot slot, IAssignableIdentity ownerIdentity)
	{
		originalItemRow.gameObject.SetActive(value: false);
		OwnablesSidescreenItemRow component = Util.KInstantiateUI(originalItemRow.gameObject, originalItemRow.transform.parent.gameObject).GetComponent<OwnablesSidescreenItemRow>();
		component.OnSlotRowClicked = (Action<OwnablesSidescreenItemRow>)Delegate.Combine(component.OnSlotRowClicked, new Action<OwnablesSidescreenItemRow>(OnRowClicked));
		component.gameObject.SetActive(value: true);
		component.SetData(owner, slot, !data.IsSlotApplicable(ownerIdentity, slot));
		return component;
	}

	private void OnRowClicked(OwnablesSidescreenItemRow row)
	{
		OnSlotRowClicked?.Invoke(row);
	}

	private void DeleteAllRows()
	{
		originalItemRow.gameObject.SetActive(value: false);
		if (itemRows != null)
		{
			for (int i = 0; i < itemRows.Length; i++)
			{
				itemRows[i].ClearData();
				itemRows[i].DeleteObject();
			}
			itemRows = null;
		}
	}

	public void SetSelectedRow_VisualsOnly(AssignableSlotInstance slotInstance)
	{
		if (itemRows != null)
		{
			for (int i = 0; i < itemRows.Length; i++)
			{
				OwnablesSidescreenItemRow ownablesSidescreenItemRow = itemRows[i];
				ownablesSidescreenItemRow.SetSelectedVisualState(ownablesSidescreenItemRow.SlotInstance == slotInstance);
			}
		}
	}
}
