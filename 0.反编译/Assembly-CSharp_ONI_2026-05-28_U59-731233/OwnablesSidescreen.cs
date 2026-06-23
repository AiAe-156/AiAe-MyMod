using System;
using Klei.AI;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class OwnablesSidescreen : SideScreenContent
{
	public struct Category
	{
		public Func<IAssignableIdentity, Assignables> getAssignablesFn;

		public OwnablesSidescreenCategoryRow.Data data;

		public Category(Func<IAssignableIdentity, Assignables> getAssignablesFn, OwnablesSidescreenCategoryRow.Data categoryData)
		{
			this.getAssignablesFn = getAssignablesFn;
			data = categoryData;
		}
	}

	public OwnablesSecondSideScreen selectedSlotScreenPrefab;

	public OwnablesSidescreenCategoryRow originalCategoryRow;

	[Header("Editor Settings")]
	public bool usingSlider = true;

	public GameObject titleSection;

	public GameObject scrollbarSection;

	public VerticalLayoutGroup mainLayoutGroup;

	public KScrollRect scrollRect;

	private OwnablesSidescreenCategoryRow[] categoryRows;

	private AssignableSlotInstance lastSelectedSlot;

	private Category[] categories = null;

	public Action<AssignableSlotInstance> OnSlotInstanceSelected;

	private MinionIdentity lastTarget;

	private int minionDestroyedCallbackIDX = -1;

	private void DefineCategories()
	{
		if (categories == null)
		{
			categories = new Category[2]
			{
				new Category((IAssignableIdentity assignableIdentity) => (assignableIdentity as MinionIdentity).GetEquipment(), new OwnablesSidescreenCategoryRow.Data(UI.UISIDESCREENS.OWNABLESSIDESCREEN.CATEGORIES.SUITS, new OwnablesSidescreenCategoryRow.AssignableSlotData[3]
				{
					new OwnablesSidescreenCategoryRow.AssignableSlotData(Db.Get().AssignableSlots.Suit, Always),
					new OwnablesSidescreenCategoryRow.AssignableSlotData(Db.Get().AssignableSlots.Outfit, Always),
					new OwnablesSidescreenCategoryRow.AssignableSlotData(Db.Get().AssignableSlots.Shoes, Always)
				})),
				new Category((IAssignableIdentity assignableIdentity) => assignableIdentity.GetSoleOwner(), new OwnablesSidescreenCategoryRow.Data(UI.UISIDESCREENS.OWNABLESSIDESCREEN.CATEGORIES.AMENITIES, new OwnablesSidescreenCategoryRow.AssignableSlotData[3]
				{
					new OwnablesSidescreenCategoryRow.AssignableSlotData(Db.Get().AssignableSlots.Bed, Always),
					new OwnablesSidescreenCategoryRow.AssignableSlotData(Db.Get().AssignableSlots.Toilet, Always),
					new OwnablesSidescreenCategoryRow.AssignableSlotData(Db.Get().AssignableSlots.MessStation, MessStation.CanBeAssignedTo)
				}))
			};
		}
	}

	private bool Always(IAssignableIdentity identity)
	{
		return true;
	}

	private Func<IAssignableIdentity, bool> HasAmount(string amountID)
	{
		return delegate(IAssignableIdentity identity)
		{
			if (identity == null)
			{
				return false;
			}
			GameObject targetGameObject = identity.GetOwners()[0].GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
			AmountInstance amountInstance = Db.Get().Amounts.Get(amountID).Lookup(targetGameObject);
			return amountInstance != null;
		};
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
	}

	private void ActivateSecondSidescreen(AssignableSlotInstance slot)
	{
		OwnablesSecondSideScreen ownablesSecondSideScreen = (OwnablesSecondSideScreen)DetailsScreen.Instance.SetSecondarySideScreen(selectedSlotScreenPrefab, slot.slot.Name);
		ownablesSecondSideScreen.SetSlot(slot);
		if (slot != null && OnSlotInstanceSelected != null)
		{
			OnSlotInstanceSelected(slot);
		}
	}

	private void DeactivateSecondScreen()
	{
		DetailsScreen.Instance.ClearSecondarySideScreen();
	}

	public override void SetTarget(GameObject target)
	{
		base.SetTarget(target);
		UnsubscribeFromLastTarget();
		lastSelectedSlot = null;
		DefineCategories();
		CreateCategoryRows();
		DeactivateSecondScreen();
		RefreshSelectedStatusOnRows();
		IAssignableIdentity component = target.GetComponent<IAssignableIdentity>();
		for (int i = 0; i < categoryRows.Length; i++)
		{
			Category category = categories[i];
			Assignables owner = category.getAssignablesFn(component);
			categoryRows[i].SetOwner(owner);
		}
		titleSection.SetActive(target.GetComponent<MinionIdentity>().model == BionicMinionConfig.MODEL);
		MinionIdentity minionIdentity = component as MinionIdentity;
		if (minionIdentity != null)
		{
			lastTarget = minionIdentity;
			minionDestroyedCallbackIDX = minionIdentity.gameObject.Subscribe(1502190696, OnTargetDestroyed);
		}
	}

	private void OnTargetDestroyed(object o)
	{
		ClearTarget();
	}

	public override void ClearTarget()
	{
		base.ClearTarget();
		lastSelectedSlot = null;
		RefreshSelectedStatusOnRows();
		for (int i = 0; i < categoryRows.Length; i++)
		{
			categoryRows[i].SetOwner(null);
		}
		DeactivateSecondScreen();
		UnsubscribeFromLastTarget();
	}

	private void CreateCategoryRows()
	{
		if (categoryRows == null)
		{
			originalCategoryRow.gameObject.SetActive(value: false);
			categoryRows = new OwnablesSidescreenCategoryRow[categories.Length];
			for (int i = 0; i < categories.Length; i++)
			{
				OwnablesSidescreenCategoryRow.Data data = categories[i].data;
				OwnablesSidescreenCategoryRow component = Util.KInstantiateUI(originalCategoryRow.gameObject, originalCategoryRow.transform.parent.gameObject).GetComponent<OwnablesSidescreenCategoryRow>();
				component.OnSlotRowClicked = (Action<OwnablesSidescreenItemRow>)Delegate.Combine(component.OnSlotRowClicked, new Action<OwnablesSidescreenItemRow>(OnSlotRowClicked));
				component.gameObject.SetActive(value: true);
				component.SetCategoryData(data);
				categoryRows[i] = component;
			}
			RefreshSelectedStatusOnRows();
		}
	}

	private void OnSlotRowClicked(OwnablesSidescreenItemRow slotRow)
	{
		if (slotRow.IsLocked || slotRow.SlotInstance == lastSelectedSlot)
		{
			SetSelectedSlot(null);
		}
		else
		{
			SetSelectedSlot(slotRow.SlotInstance);
		}
	}

	public void RefreshSelectedStatusOnRows()
	{
		if (categoryRows != null)
		{
			for (int i = 0; i < categoryRows.Length; i++)
			{
				OwnablesSidescreenCategoryRow ownablesSidescreenCategoryRow = categoryRows[i];
				ownablesSidescreenCategoryRow.SetSelectedRow_VisualsOnly(lastSelectedSlot);
			}
		}
	}

	public void SetSelectedSlot(AssignableSlotInstance slot)
	{
		lastSelectedSlot = slot;
		if (slot != null)
		{
			ActivateSecondSidescreen(slot);
		}
		else
		{
			DeactivateSecondScreen();
		}
		RefreshSelectedStatusOnRows();
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		if (categoryRows != null)
		{
			for (int i = 0; i < categoryRows.Length; i++)
			{
				if (categoryRows[i] != null)
				{
					categoryRows[i].SetOwner(null);
				}
			}
		}
		UnsubscribeFromLastTarget();
	}

	private void UnsubscribeFromLastTarget()
	{
		if (lastTarget != null && minionDestroyedCallbackIDX != -1)
		{
			lastTarget.Unsubscribe(minionDestroyedCallbackIDX);
		}
		minionDestroyedCallbackIDX = -1;
		lastTarget = null;
	}

	public override bool IsValidForTarget(GameObject target)
	{
		return target.GetComponent<IAssignableIdentity>() != null;
	}

	public void OnValidate()
	{
	}

	private void SetScrollBarVisibility(bool isVisible)
	{
		scrollbarSection.gameObject.SetActive(isVisible);
		mainLayoutGroup.padding.right = (isVisible ? 20 : 0);
		scrollRect.enabled = isVisible;
	}
}
