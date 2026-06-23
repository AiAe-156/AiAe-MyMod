using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MeterScreen_ValueTrackerDisplayer : KMonoBehaviour
{
	public LocText Label;

	public ToolTip Tooltip;

	public GameObject diagnosticGraph;

	public TextStyleSetting ToolTipStyle_Header;

	public TextStyleSetting ToolTipStyle_Property;

	protected Func<List<MinionIdentity>, List<MinionIdentity>> minionListCustomSortOperation = null;

	private List<MinionIdentity> worldLiveMinionIdentities;

	protected override void OnSpawn()
	{
		Tooltip.OnToolTip = OnTooltip;
		base.OnSpawn();
	}

	public void Refresh()
	{
		RefreshWorldMinionIdentities();
		InternalRefresh();
	}

	protected abstract void InternalRefresh();

	protected abstract string OnTooltip();

	public virtual void OnClick(BaseEventData base_ev_data)
	{
	}

	private void RefreshWorldMinionIdentities()
	{
		worldLiveMinionIdentities = new List<MinionIdentity>(from x in Components.LiveMinionIdentities.GetWorldItems(ClusterManager.Instance.activeWorldId)
			where !x.IsNullOrDestroyed()
			select x);
	}

	protected virtual List<MinionIdentity> GetWorldMinionIdentities()
	{
		if (worldLiveMinionIdentities == null)
		{
			RefreshWorldMinionIdentities();
		}
		if (minionListCustomSortOperation != null)
		{
			worldLiveMinionIdentities = minionListCustomSortOperation(worldLiveMinionIdentities);
		}
		return worldLiveMinionIdentities;
	}

	protected virtual List<MinionIdentity> GetAllMinionsFromAllWorlds()
	{
		List<MinionIdentity> list = new List<MinionIdentity>(Components.LiveMinionIdentities.Items.Where((MinionIdentity x) => !x.IsNullOrDestroyed()));
		if (minionListCustomSortOperation != null)
		{
			worldLiveMinionIdentities = minionListCustomSortOperation(list);
		}
		return list;
	}
}
