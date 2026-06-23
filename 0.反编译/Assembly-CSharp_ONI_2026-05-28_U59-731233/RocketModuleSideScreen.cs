using System.Collections;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class RocketModuleSideScreen : SideScreenContent
{
	public static RocketModuleSideScreen instance;

	private ReorderableBuilding reorderable;

	public KScreen changeModuleSideScreen;

	public Image moduleIcon;

	[Header("Buttons")]
	public KButton addNewModuleButton;

	public KButton removeModuleButton;

	public KButton changeModuleButton;

	public KButton moveModuleUpButton;

	public KButton moveModuleDownButton;

	[Header("Labels")]
	public LocText removeButtonLabel;

	public LocText moduleNameLabel;

	public LocText moduleDescriptionLabel;

	public TextStyleSetting nameSetting;

	public TextStyleSetting descriptionSetting;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		instance = this;
	}

	protected override void OnForcedCleanUp()
	{
		instance = null;
		base.OnForcedCleanUp();
	}

	public override int GetSideScreenSortOrder()
	{
		return 104;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		addNewModuleButton.onClick += delegate
		{
			Vector2 vector = Vector2.zero;
			if (SelectModuleSideScreen.Instance != null)
			{
				KScrollRect component = SelectModuleSideScreen.Instance.mainContents.GetComponent<KScrollRect>();
				vector = component.content.rectTransform().anchoredPosition;
			}
			ClickAddNew(vector.y);
		};
		removeModuleButton.onClick += ClickRemove;
		moveModuleUpButton.onClick += ClickSwapUp;
		moveModuleDownButton.onClick += ClickSwapDown;
		changeModuleButton.onClick += delegate
		{
			Vector2 vector = Vector2.zero;
			if (SelectModuleSideScreen.Instance != null)
			{
				KScrollRect component = SelectModuleSideScreen.Instance.mainContents.GetComponent<KScrollRect>();
				vector = component.content.rectTransform().anchoredPosition;
			}
			ClickChangeModule(vector.y);
		};
		moduleNameLabel.textStyleSetting = nameSetting;
		moduleDescriptionLabel.textStyleSetting = descriptionSetting;
		moduleNameLabel.ApplySettings();
		moduleDescriptionLabel.ApplySettings();
	}

	protected override void OnCmpDisable()
	{
		base.OnCmpDisable();
		DetailsScreen.Instance.ClearSecondarySideScreen();
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
	}

	public override bool IsValidForTarget(GameObject target)
	{
		return target.GetComponent<ReorderableBuilding>() != null;
	}

	public override void SetTarget(GameObject new_target)
	{
		if (new_target == null)
		{
			Debug.LogError("Invalid gameObject received");
			return;
		}
		reorderable = new_target.GetComponent<ReorderableBuilding>();
		moduleIcon.sprite = Def.GetUISprite(reorderable.gameObject).first;
		moduleNameLabel.SetText(reorderable.GetProperName());
		moduleDescriptionLabel.SetText(reorderable.GetComponent<Building>().Desc);
		UpdateButtonStates();
	}

	public void UpdateButtonStates()
	{
		changeModuleButton.isInteractable = reorderable.CanChangeModule();
		changeModuleButton.GetComponent<ToolTip>().SetSimpleTooltip(changeModuleButton.isInteractable ? UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.BUTTONCHANGEMODULE.DESC.text : UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.BUTTONCHANGEMODULE.INVALID.text);
		addNewModuleButton.isInteractable = true;
		addNewModuleButton.GetComponent<ToolTip>().SetSimpleTooltip(UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.ADDMODULE.DESC.text);
		Deconstructable component = reorderable.GetComponent<Deconstructable>();
		bool flag = component != null && component.IsMarkedForDeconstruction();
		removeModuleButton.isInteractable = component != null && reorderable.CanRemoveModule();
		removeModuleButton.GetComponent<ToolTip>().SetSimpleTooltip((!removeModuleButton.isInteractable) ? UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.BUTTONREMOVEMODULE.INVALID.text : (flag ? UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.BUTTONREMOVEMODULE.DESC_CANCEL.text : UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.BUTTONREMOVEMODULE.DESC.text));
		removeButtonLabel.SetText(flag ? UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.BUTTONREMOVEMODULE.LABEL_CANCEL : UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.BUTTONREMOVEMODULE.LABEL);
		moveModuleDownButton.isInteractable = reorderable.CanSwapDown();
		moveModuleDownButton.GetComponent<ToolTip>().SetSimpleTooltip(moveModuleDownButton.isInteractable ? UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.BUTTONSWAPMODULEDOWN.DESC.text : UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.BUTTONSWAPMODULEDOWN.INVALID.text);
		moveModuleUpButton.isInteractable = reorderable.CanSwapUp();
		moveModuleUpButton.GetComponent<ToolTip>().SetSimpleTooltip(moveModuleUpButton.isInteractable ? UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.BUTTONSWAPMODULEUP.DESC.text : UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.BUTTONSWAPMODULEUP.INVALID.text);
	}

	public void ClickAddNew(float scrollViewPosition, BuildingDef autoSelectDef = null)
	{
		SelectModuleSideScreen selectModuleSideScreen = (SelectModuleSideScreen)DetailsScreen.Instance.SetSecondarySideScreen(changeModuleSideScreen, UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.CHANGEMODULEPANEL);
		selectModuleSideScreen.addingNewModule = true;
		selectModuleSideScreen.SetTarget(reorderable.gameObject);
		if (autoSelectDef != null)
		{
			selectModuleSideScreen.SelectModule(autoSelectDef);
		}
		ScrollToTargetPoint(scrollViewPosition);
	}

	private void ScrollToTargetPoint(float scrollViewPosition)
	{
		if (SelectModuleSideScreen.Instance != null)
		{
			SelectModuleSideScreen.Instance.mainContents.GetComponent<KScrollRect>().content.anchoredPosition = new Vector2(0f, scrollViewPosition);
			if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(DelayedScrollToTargetPoint(scrollViewPosition));
			}
		}
	}

	private IEnumerator DelayedScrollToTargetPoint(float scrollViewPosition)
	{
		if (SelectModuleSideScreen.Instance != null)
		{
			yield return SequenceUtil.WaitForEndOfFrame;
			SelectModuleSideScreen.Instance.mainContents.GetComponent<KScrollRect>().content.anchoredPosition = new Vector2(0f, scrollViewPosition);
		}
	}

	private void ClickRemove()
	{
		Deconstructable component = reorderable.GetComponent<Deconstructable>();
		if (!(component == null))
		{
			if (component.IsMarkedForDeconstruction())
			{
				component.CancelDeconstruction();
			}
			else
			{
				reorderable.Trigger(-790448070);
			}
			UpdateButtonStates();
			component.Trigger(1980521255);
		}
	}

	private void ClickSwapUp()
	{
		reorderable.SwapWithAbove();
		UpdateButtonStates();
	}

	private void ClickSwapDown()
	{
		reorderable.SwapWithBelow();
		UpdateButtonStates();
	}

	private void ClickChangeModule(float scrollViewPosition)
	{
		SelectModuleSideScreen selectModuleSideScreen = (SelectModuleSideScreen)DetailsScreen.Instance.SetSecondarySideScreen(changeModuleSideScreen, UI.UISIDESCREENS.ROCKETMODULESIDESCREEN.CHANGEMODULEPANEL);
		selectModuleSideScreen.addingNewModule = false;
		selectModuleSideScreen.SetTarget(reorderable.gameObject);
		ScrollToTargetPoint(scrollViewPosition);
	}
}
