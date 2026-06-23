using System;
using STRINGS;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/AssignableSideScreenRow")]
public class AssignableSideScreenRow : KMonoBehaviour
{
	public enum AssignableState
	{
		Selected,
		AssignedToOther,
		Unassigned,
		Disabled
	}

	[SerializeField]
	private CrewPortrait crewPortraitPrefab;

	[SerializeField]
	private LocText assignmentText;

	public AssignableSideScreen sideScreen;

	private CrewPortrait portraitInstance;

	[MyCmpReq]
	private MultiToggle toggle;

	public IAssignableIdentity targetIdentity;

	public AssignableState currentState;

	private int refreshHandle = -1;

	public void Refresh(object data = null)
	{
		if (!sideScreen.targetAssignable.CanAssignTo(targetIdentity))
		{
			currentState = AssignableState.Disabled;
			assignmentText.text = UI.UISIDESCREENS.ASSIGNABLESIDESCREEN.DISABLED;
		}
		else if (sideScreen.targetAssignable.assignee == targetIdentity)
		{
			currentState = AssignableState.Selected;
			assignmentText.text = UI.UISIDESCREENS.ASSIGNABLESIDESCREEN.ASSIGNED;
		}
		else
		{
			bool flag = false;
			KMonoBehaviour kMonoBehaviour = targetIdentity as KMonoBehaviour;
			if (kMonoBehaviour != null)
			{
				Ownables component = kMonoBehaviour.GetComponent<Ownables>();
				if (component != null)
				{
					AssignableSlotInstance[] slots = component.GetSlots(sideScreen.targetAssignable.slot);
					if (slots != null && slots.Length != 0)
					{
						AssignableSlotInstance assignableSlotInstance = slots.FindFirst((AssignableSlotInstance s) => !s.IsAssigned());
						if (assignableSlotInstance == null)
						{
							assignableSlotInstance = slots[0];
						}
						if (assignableSlotInstance != null && assignableSlotInstance.IsAssigned())
						{
							currentState = AssignableState.AssignedToOther;
							assignmentText.text = assignableSlotInstance.assignable.GetProperName();
							flag = true;
						}
					}
				}
				Equipment component2 = kMonoBehaviour.GetComponent<Equipment>();
				if (component2 != null)
				{
					AssignableSlotInstance[] slots2 = component2.GetSlots(sideScreen.targetAssignable.slot);
					if (slots2 != null && slots2.Length != 0)
					{
						AssignableSlotInstance assignableSlotInstance2 = slots2.FindFirst((AssignableSlotInstance s) => !s.IsAssigned());
						if (assignableSlotInstance2 == null)
						{
							assignableSlotInstance2 = slots2[0];
						}
						if (assignableSlotInstance2 != null && assignableSlotInstance2.IsAssigned())
						{
							currentState = AssignableState.AssignedToOther;
							assignmentText.text = assignableSlotInstance2.assignable.GetProperName();
							flag = true;
						}
					}
				}
			}
			if (!flag)
			{
				currentState = AssignableState.Unassigned;
				assignmentText.text = UI.UISIDESCREENS.ASSIGNABLESIDESCREEN.UNASSIGNED;
			}
		}
		toggle.ChangeState((int)currentState);
	}

	protected override void OnCleanUp()
	{
		Game.Instance.Unsubscribe(ref refreshHandle);
		base.OnCleanUp();
	}

	public void SetContent(IAssignableIdentity identity_object, Action<IAssignableIdentity> selectionCallback, AssignableSideScreen assignableSideScreen)
	{
		Game.Instance.Unsubscribe(ref refreshHandle);
		refreshHandle = Game.Instance.Subscribe(-2146166042, delegate
		{
			if (this != null && base.gameObject != null && base.gameObject.activeInHierarchy)
			{
				Refresh();
			}
		});
		toggle = GetComponent<MultiToggle>();
		sideScreen = assignableSideScreen;
		targetIdentity = identity_object;
		if (portraitInstance == null)
		{
			portraitInstance = Util.KInstantiateUI<CrewPortrait>(crewPortraitPrefab.gameObject, base.gameObject);
			portraitInstance.transform.SetSiblingIndex(1);
			portraitInstance.SetAlpha(1f);
		}
		toggle.onClick = delegate
		{
			selectionCallback(targetIdentity);
		};
		portraitInstance.SetIdentityObject(identity_object, jobEnabled: false);
		GetComponent<ToolTip>().OnToolTip = GetTooltip;
		Refresh();
	}

	private string GetTooltip()
	{
		ToolTip component = GetComponent<ToolTip>();
		component.ClearMultiStringTooltip();
		if (sideScreen.targetAssignable.customAssignablesUITooltipFunc != null)
		{
			return sideScreen.targetAssignable.customAssignablesUITooltipFunc(targetIdentity.GetSoleOwner());
		}
		if (targetIdentity != null && !targetIdentity.IsNull())
		{
			switch (currentState)
			{
			case AssignableState.Selected:
				component.AddMultiStringTooltip(string.Format(UI.UISIDESCREENS.ASSIGNABLESIDESCREEN.UNASSIGN_TOOLTIP, targetIdentity.GetProperName()), null);
				break;
			case AssignableState.Disabled:
				component.AddMultiStringTooltip(string.Format(UI.UISIDESCREENS.ASSIGNABLESIDESCREEN.DISABLED_TOOLTIP, targetIdentity.GetProperName()), null);
				break;
			default:
				component.AddMultiStringTooltip(string.Format(UI.UISIDESCREENS.ASSIGNABLESIDESCREEN.ASSIGN_TO_TOOLTIP, targetIdentity.GetProperName()), null);
				break;
			}
		}
		return "";
	}
}
