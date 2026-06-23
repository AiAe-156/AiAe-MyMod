using System.Collections.Generic;
using System.Linq;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class RemoteWorkTerminalSidescreen : SideScreenContent
{
	private RemoteWorkTerminal targetTerminal;

	public GameObject rowPrefab;

	public RectTransform rowContainer;

	public Dictionary<object, GameObject> rows = new Dictionary<object, GameObject>();

	private int uiRefreshSubHandle = -1;

	public override string GetTitle()
	{
		return UI.UISIDESCREENS.REMOTE_WORK_TERMINAL_SIDE_SCREEN.TITLE;
	}

	protected override void OnShow(bool show)
	{
		base.OnShow(show);
		rowPrefab.SetActive(value: false);
		if (show)
		{
			RefreshOptions();
		}
	}

	public override bool IsValidForTarget(GameObject target)
	{
		return target.GetComponent<RemoteWorkTerminal>() != null;
	}

	public override void SetTarget(GameObject target)
	{
		targetTerminal = target.GetComponent<RemoteWorkTerminal>();
		RefreshOptions();
		uiRefreshSubHandle = target.Subscribe(1980521255, RefreshOptions);
	}

	public override void ClearTarget()
	{
		if (uiRefreshSubHandle != -1 && targetTerminal != null)
		{
			targetTerminal.gameObject.Unsubscribe(uiRefreshSubHandle);
			uiRefreshSubHandle = -1;
		}
	}

	private void RefreshOptions(object data = null)
	{
		int num = 0;
		SetRow(num++, UI.UISIDESCREENS.REMOTE_WORK_TERMINAL_SIDE_SCREEN.NOTHING_SELECTED, Assets.GetSprite("action_building_disabled"), null);
		foreach (RemoteWorkerDock item in Components.RemoteWorkerDocks.GetItems(targetTerminal.GetMyWorldId()))
		{
			item.GetProperName();
			_ = Def.GetUISprite(item.gameObject).first;
			SetRow(num++, UI.StripLinkFormatting(item.GetProperName()), Def.GetUISprite(item.gameObject)?.first, item);
		}
		for (int i = num; i < rowContainer.childCount; i++)
		{
			rowContainer.GetChild(i).gameObject.SetActive(value: false);
		}
	}

	private void ClearRows()
	{
		for (int num = rowContainer.childCount - 1; num >= 0; num--)
		{
			Util.KDestroyGameObject(rowContainer.GetChild(num));
		}
		rows.Clear();
	}

	private void SetRow(int idx, string name, Sprite icon, RemoteWorkerDock dock)
	{
		_ = dock == null;
		GameObject gameObject = ((idx >= rowContainer.childCount) ? Util.KInstantiateUI(rowPrefab, rowContainer.gameObject, force_active: true) : rowContainer.GetChild(idx).gameObject);
		HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
		LocText reference = component.GetReference<LocText>("label");
		reference.text = name;
		reference.ApplySettings();
		Image reference2 = component.GetReference<Image>("icon");
		reference2.sprite = icon;
		reference2.color = Color.white;
		ToolTip toolTip = gameObject.GetComponentsInChildren<ToolTip>().First();
		toolTip.SetSimpleTooltip(UI.UISIDESCREENS.REMOTE_WORK_TERMINAL_SIDE_SCREEN.DOCK_TOOLTIP);
		toolTip.enabled = dock != null;
		MultiToggle component2 = gameObject.GetComponent<MultiToggle>();
		component2.ChangeState((targetTerminal.FutureDock == dock) ? 1 : 0);
		component2.onClick = delegate
		{
			targetTerminal.FutureDock = dock;
			RefreshOptions();
		};
		component2.onDoubleClick = delegate
		{
			GameUtil.FocusCamera((dock == null) ? targetTerminal.transform.GetPosition() : dock.transform.GetPosition());
			return true;
		};
		if (!gameObject.activeSelf)
		{
			gameObject.SetActive(value: true);
		}
	}
}
