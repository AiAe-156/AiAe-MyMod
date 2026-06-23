using System;
using System.Collections.Generic;
using KSerialization;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/UprootedMonitor")]
public class UprootedMonitor : KMonoBehaviour
{
	private int position;

	[Serialize]
	public bool canBeUprooted = true;

	[Serialize]
	private bool uprooted;

	public CellOffset[] monitorCells = new CellOffset[1]
	{
		new CellOffset(0, -1)
	};

	public Func<ScenePartitionerLayer> customScenePartitionerLayerFn;

	public Func<int, bool> customFoundationCheckFn;

	private List<HandleVector<int>.Handle> partitionerEntries = new List<HandleVector<int>.Handle>();

	private static readonly EventSystem.IntraObjectHandler<UprootedMonitor> OnUprootedDelegate = new EventSystem.IntraObjectHandler<UprootedMonitor>(delegate(UprootedMonitor component, object data)
	{
		if (!component.uprooted)
		{
			component.GetComponent<KPrefabID>().AddTag(GameTags.Uprooted);
			component.uprooted = true;
			component.Trigger(-216549700);
		}
	});

	public bool IsUprooted
	{
		get
		{
			if (!uprooted)
			{
				return GetComponent<KPrefabID>().HasTag(GameTags.Uprooted);
			}
			return true;
		}
	}

	protected override void OnPrefabInit()
	{
		Subscribe(-216549700, OnUprootedDelegate);
		position = Grid.PosToCell(base.gameObject);
		base.OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		RegisterMonitoredCellsPartitionerEntries();
	}

	public void SetNewMonitorCells(CellOffset[] cellsOffsets)
	{
		UnregisterMonitoredCellsPartitionerEntries();
		monitorCells = cellsOffsets;
		RegisterMonitoredCellsPartitionerEntries();
	}

	private void UnregisterMonitoredCellsPartitionerEntries()
	{
		foreach (HandleVector<int>.Handle partitionerEntry in partitionerEntries)
		{
			HandleVector<int>.Handle handle = partitionerEntry;
			GameScenePartitioner.Instance.Free(ref handle);
		}
		partitionerEntries.Clear();
	}

	private void RegisterMonitoredCellsPartitionerEntries()
	{
		CellOffset[] array = monitorCells;
		foreach (CellOffset offset in array)
		{
			int cell = Grid.OffsetCell(position, offset);
			if (Grid.IsValidCell(position) && Grid.IsValidCell(cell))
			{
				if (customScenePartitionerLayerFn == null)
				{
					partitionerEntries.Add(GameScenePartitioner.Instance.Add("UprootedMonitor.OnSpawn", base.gameObject, cell, GameScenePartitioner.Instance.solidChangedLayer, OnGroundChanged));
				}
				else
				{
					partitionerEntries.Add(GameScenePartitioner.Instance.Add("UprootedMonitor.OnSpawn", base.gameObject, cell, customScenePartitionerLayerFn(), OnFoundationChanged));
				}
			}
		}
		OnGroundChanged(null);
	}

	protected override void OnCleanUp()
	{
		UnregisterMonitoredCellsPartitionerEntries();
		base.OnCleanUp();
	}

	public bool CheckTileGrowable()
	{
		if (!canBeUprooted)
		{
			return true;
		}
		if (uprooted)
		{
			return false;
		}
		if (!IsSuitableFoundation(position))
		{
			return false;
		}
		return true;
	}

	public bool IsSuitableFoundation(int cell)
	{
		bool flag = true;
		CellOffset[] array = monitorCells;
		foreach (CellOffset offset in array)
		{
			if (!Grid.IsCellOffsetValid(cell, offset))
			{
				return false;
			}
			int num = Grid.OffsetCell(cell, offset);
			flag = ((customFoundationCheckFn == null) ? Grid.Solid[num] : customFoundationCheckFn(num));
			if (!flag)
			{
				break;
			}
		}
		return flag;
	}

	public void OnGroundChanged(object callbackData)
	{
		OnFoundationChanged(callbackData);
	}

	public void OnFoundationChanged(object callbackData)
	{
		if (!CheckTileGrowable())
		{
			uprooted = true;
		}
		if (uprooted)
		{
			GetComponent<KPrefabID>().AddTag(GameTags.Uprooted);
			Trigger(-216549700);
		}
	}
}
