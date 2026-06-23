using System;
using System.Collections.Generic;
using System.Linq;
using STRINGS;
using UnityEngine;

public class MeterScreen : KScreen, IRender1000ms
{
	private struct DisplayInfo
	{
		public int selectedIndex;
	}

	[SerializeField]
	private LocText currentMinions;

	public ToolTip MinionsTooltip;

	public MeterScreen_ValueTrackerDisplayer[] valueDisplayers;

	public TextStyleSetting ToolTipStyle_Header;

	public TextStyleSetting ToolTipStyle_Property;

	private bool startValuesSet = false;

	public MultiToggle RedAlertButton;

	public ToolTip RedAlertTooltip;

	private DisplayInfo immunityDisplayInfo = new DisplayInfo
	{
		selectedIndex = -1
	};

	private List<MinionIdentity> worldLiveMinionIdentities;

	private int cachedMinionCount = -1;

	public static MeterScreen Instance { get; private set; }

	public bool StartValuesSet => startValuesSet;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		Instance = this;
	}

	protected override void OnSpawn()
	{
		RedAlertTooltip.OnToolTip = OnRedAlertTooltip;
		MultiToggle redAlertButton = RedAlertButton;
		redAlertButton.onClick = (System.Action)Delegate.Combine(redAlertButton.onClick, (System.Action)delegate
		{
			OnRedAlertClick();
		});
		Game.Instance.Subscribe(1983128072, delegate
		{
			Refresh();
		});
		Game.Instance.Subscribe(1585324898, delegate
		{
			RefreshRedAlertButtonState();
		});
		Game.Instance.Subscribe(-1393151672, delegate
		{
			RefreshRedAlertButtonState();
		});
	}

	private void OnRedAlertClick()
	{
		bool flag = !ClusterManager.Instance.activeWorld.AlertManager.IsRedAlertToggledOn();
		ClusterManager.Instance.activeWorld.AlertManager.ToggleRedAlert(flag);
		if (flag)
		{
			KMonoBehaviour.PlaySound(GlobalAssets.GetSound("HUD_Click_Open"));
		}
		else
		{
			KMonoBehaviour.PlaySound(GlobalAssets.GetSound("HUD_Click_Close"));
		}
	}

	private void RefreshRedAlertButtonState()
	{
		RedAlertButton.ChangeState(ClusterManager.Instance.activeWorld.IsRedAlert() ? 1 : 0);
	}

	public void Render1000ms(float dt)
	{
		Refresh();
	}

	public void InitializeValues()
	{
		if (!startValuesSet)
		{
			startValuesSet = true;
			Refresh();
		}
	}

	private void Refresh()
	{
		RefreshWorldMinionIdentities();
		RefreshMinions();
		for (int i = 0; i < valueDisplayers.Length; i++)
		{
			MeterScreen_ValueTrackerDisplayer meterScreen_ValueTrackerDisplayer = valueDisplayers[i];
			meterScreen_ValueTrackerDisplayer.Refresh();
		}
		RefreshRedAlertButtonState();
	}

	private void RefreshWorldMinionIdentities()
	{
		worldLiveMinionIdentities = new List<MinionIdentity>(from x in Components.LiveMinionIdentities.GetWorldItems(ClusterManager.Instance.activeWorldId)
			where !x.IsNullOrDestroyed()
			select x);
	}

	private List<MinionIdentity> GetWorldMinionIdentities()
	{
		if (worldLiveMinionIdentities == null)
		{
			RefreshWorldMinionIdentities();
		}
		return worldLiveMinionIdentities;
	}

	private void RefreshMinions()
	{
		int count = Components.LiveMinionIdentities.Count;
		int count2 = GetWorldMinionIdentities().Count;
		if (count2 != cachedMinionCount)
		{
			cachedMinionCount = count2;
			string text = "";
			if (DlcManager.FeatureClusterSpaceEnabled())
			{
				ClusterGridEntity component = ClusterManager.Instance.activeWorld.GetComponent<ClusterGridEntity>();
				text = string.Format(UI.TOOLTIPS.METERSCREEN_POPULATION_CLUSTER, component.Name, count2, count);
				currentMinions.text = $"{count2}/{count}";
			}
			else
			{
				currentMinions.text = $"{count}";
				text = string.Format(UI.TOOLTIPS.METERSCREEN_POPULATION, count.ToString("0"));
			}
			MinionsTooltip.ClearMultiStringTooltip();
			MinionsTooltip.AddMultiStringTooltip(text, ToolTipStyle_Header);
		}
	}

	private string OnRedAlertTooltip()
	{
		RedAlertTooltip.ClearMultiStringTooltip();
		RedAlertTooltip.AddMultiStringTooltip(UI.TOOLTIPS.RED_ALERT_TITLE, ToolTipStyle_Header);
		RedAlertTooltip.AddMultiStringTooltip(UI.TOOLTIPS.RED_ALERT_CONTENT, ToolTipStyle_Property);
		return "";
	}
}
