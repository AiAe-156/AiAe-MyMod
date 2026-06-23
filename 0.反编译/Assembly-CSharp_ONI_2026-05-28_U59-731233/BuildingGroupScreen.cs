using System;
using STRINGS;
using TMPro;
using UnityEngine;

public class BuildingGroupScreen : KScreen
{
	public static BuildingGroupScreen Instance;

	public KInputTextField inputField;

	[SerializeField]
	public KButton clearButton;

	public static bool SearchIsEmpty
	{
		get
		{
			if (Instance == null)
			{
				return true;
			}
			return Instance.inputField.text.IsNullOrWhiteSpace();
		}
	}

	public static bool IsEditing
	{
		get
		{
			if (Instance == null)
			{
				return false;
			}
			return Instance.isEditing;
		}
	}

	protected override void OnPrefabInit()
	{
		Instance = this;
		base.OnPrefabInit();
		base.ConsumeMouseScroll = true;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		KInputTextField kInputTextField = inputField;
		kInputTextField.onFocus = (System.Action)Delegate.Combine(kInputTextField.onFocus, (System.Action)delegate
		{
			base.isEditing = true;
			UISounds.PlaySound(UISounds.Sound.Find);
			ConfigurePlanScreenForSearch();
		});
		inputField.onEndEdit.AddListener(delegate
		{
			base.isEditing = false;
		});
		inputField.OnValueChangesPaused = delegate
		{
			PlanScreen.Instance.RefreshCategoryPanelTitle();
			PlanScreen.Instance.RefreshSearch();
		};
		inputField.placeholder.GetComponent<TextMeshProUGUI>().text = UI.BUILDMENU.SEARCH_TEXT_PLACEHOLDER;
		clearButton.onClick += ClearSearch;
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		base.ConsumeMouseScroll = true;
		BindTooltip();
		KInputManager.InputChange.AddListener(BindTooltip);
	}

	protected override void OnDeactivate()
	{
		KInputManager.InputChange.RemoveListener(BindTooltip);
		base.OnDeactivate();
	}

	private void BindTooltip()
	{
		inputField.GetComponent<ToolTip>().toolTip = GameUtil.ReplaceHotkeyString(UI.BUILDMENU.SEARCH_TOOLTIP, Action.Find);
	}

	public void ClearSearch()
	{
		inputField.text = "";
		inputField.ForceChangeValueRefresh();
	}

	private void ConfigurePlanScreenForSearch()
	{
		PlanScreen.Instance.SoftCloseRecipe();
		PlanScreen.Instance.ClearSelection();
		PlanScreen.Instance.ForceRefreshAllBuildingToggles();
		PlanScreen.Instance.ConfigurePanelSize();
	}
}
