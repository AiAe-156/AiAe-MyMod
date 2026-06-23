using System;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class OwnablesSecondSideScreenRow : KMonoBehaviour
{
	public static string NO_DATA_MESSAGE = UI.UISIDESCREENS.OWNABLESSIDESCREEN.NO_ITEM_FOUND;

	public static string NOT_ASSIGNED = UI.UISIDESCREENS.OWNABLESSECONDSIDESCREEN.NOT_ASSIGNED;

	public static string ASSIGNED_TO_SELF = UI.UISIDESCREENS.OWNABLESSECONDSIDESCREEN.ASSIGNED_TO_SELF_STATUS;

	public static string ASSIGNED_TO_OTHER = UI.UISIDESCREENS.OWNABLESSECONDSIDESCREEN.ASSIGNED_TO_OTHER_STATUS;

	public KImage icon;

	public KImage emptyIcon;

	public LocText nameLabel;

	public LocText statusLabel;

	public Button eyeButton;

	public ToolTip tooltip;

	public Action<OwnablesSecondSideScreenRow> OnRowItemAssigneeChanged;

	public Action<OwnablesSecondSideScreenRow> OnRowItemDestroyed;

	public Action<OwnablesSecondSideScreenRow> OnRowClicked;

	public Func<Assignables, string> customTooltipFunc = null;

	private MultiToggle toggle;

	private int changeAssignmentListenerIDX = -1;

	private int destroyListenerIDX = -1;

	public AssignableSlotInstance minionSlotInstance { get; private set; }

	public Assignable item { get; private set; }

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		toggle = GetComponent<MultiToggle>();
		MultiToggle multiToggle = toggle;
		multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, new System.Action(OnMultitoggleClicked));
		eyeButton.onClick.AddListener(FocusCameraOnAssignedItem);
	}

	public void SetData(AssignableSlotInstance minion, Assignable item_assignable)
	{
		minionSlotInstance = minion;
		item = item_assignable;
		changeAssignmentListenerIDX = item.Subscribe(684616645, _OnItemAssignationChanged);
		destroyListenerIDX = item.Subscribe(1969584890, _OnRowItemDestroyed);
		customTooltipFunc = item.customAssignmentUITooltipFunc;
		Refresh();
	}

	public void Refresh()
	{
		if (item != null)
		{
			Tag tag = item.PrefabID();
			string properName = item.GetProperName();
			nameLabel.text = properName;
			icon.sprite = Def.GetUISprite(item.gameObject).first;
			bool flag = item.IsAssigned() && !minionSlotInstance.IsUnassigning() && minionSlotInstance.assignable != item;
			if (item.IsAssigned())
			{
				statusLabel.SetText(string.Format(flag ? ASSIGNED_TO_OTHER : ASSIGNED_TO_SELF, item.assignee.GetProperName()));
			}
			else
			{
				statusLabel.SetText(NOT_ASSIGNED);
			}
			if (customTooltipFunc == null)
			{
				InfoDescription component = item.gameObject.GetComponent<InfoDescription>();
				bool flag2 = component != null && !string.IsNullOrEmpty(component.description);
				string simpleTooltip = (flag2 ? component.description : properName);
				tooltip.SizingSetting = ((!flag2) ? ToolTip.ToolTipSizeSetting.DynamicWidthNoWrap : ToolTip.ToolTipSizeSetting.MaxWidthWrapContent);
				tooltip.SetSimpleTooltip(simpleTooltip);
			}
			else
			{
				tooltip.SizingSetting = ToolTip.ToolTipSizeSetting.MaxWidthWrapContent;
				tooltip.SetSimpleTooltip(customTooltipFunc(minionSlotInstance.assignables));
			}
		}
		else
		{
			nameLabel.text = NO_DATA_MESSAGE;
			tooltip.SetSimpleTooltip(null);
		}
		bool flag3 = item != null && minionSlotInstance != null && !minionSlotInstance.IsUnassigning() && minionSlotInstance.assignable == item;
		toggle.ChangeState(flag3 ? 1 : 0);
		emptyIcon.gameObject.SetActive(item == null);
		icon.gameObject.SetActive(item != null);
		eyeButton.gameObject.SetActive(item != null);
		statusLabel.gameObject.SetActive(item != null);
	}

	public void ClearData()
	{
		if (item != null)
		{
			if (destroyListenerIDX != -1)
			{
				item.Unsubscribe(destroyListenerIDX);
			}
			if (changeAssignmentListenerIDX != -1)
			{
				item.Unsubscribe(changeAssignmentListenerIDX);
			}
		}
		minionSlotInstance = null;
		item = null;
		destroyListenerIDX = -1;
		changeAssignmentListenerIDX = -1;
		Refresh();
	}

	private void _OnItemAssignationChanged(object o)
	{
		OnRowItemAssigneeChanged?.Invoke(this);
	}

	private void _OnRowItemDestroyed(object o)
	{
		OnRowItemDestroyed?.Invoke(this);
	}

	private void OnMultitoggleClicked()
	{
		OnRowClicked?.Invoke(this);
	}

	private void FocusCameraOnAssignedItem()
	{
		if (item != null)
		{
			GameObject targetGameObject = item.gameObject;
			if (item.HasTag(GameTags.Equipped))
			{
				targetGameObject = item.assignee.GetOwners()[0].GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
			}
			GameUtil.FocusCamera(targetGameObject.transform, select: false);
		}
	}
}
