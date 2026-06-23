using System;
using UnityEngine;

[SkipSaveFileSerialization]
[AddComponentMenu("KMonoBehaviour/scripts/Submergable")]
public class Submergable : KMonoBehaviour
{
	[MyCmpGet]
	private OccupyArea occupyArea;

	public Func<StatusItem> GetStatusItem = null;

	protected bool isSubmerged;

	private HandleVector<int>.Handle partitionerEntry;

	public bool IsSubmerged => isSubmerged;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		partitionerEntry = GameScenePartitioner.Instance.Add("Submergable.OnSpawn", base.gameObject, occupyArea.GetExtents(), GameScenePartitioner.Instance.liquidChangedLayer, OnElementChanged);
		OnElementChanged(null);
		RefreshStatusItem();
	}

	protected virtual void OnElementChanged(object data)
	{
		bool flag = true;
		int cell = Grid.PosToCell(base.gameObject);
		for (int i = 0; i < occupyArea.OccupiedCellsOffsets.Length; i++)
		{
			CellOffset offset = occupyArea.OccupiedCellsOffsets[i];
			int cell2 = Grid.OffsetCell(cell, offset);
			if (!Grid.IsLiquid(cell2))
			{
				flag = false;
				break;
			}
		}
		if (flag != isSubmerged)
		{
			isSubmerged = flag;
			OnSubmergedStateChanged();
			base.gameObject.Trigger(1983811727);
		}
	}

	protected virtual void OnSubmergedStateChanged()
	{
		RefreshStatusItem();
	}

	protected virtual void RefreshStatusItem()
	{
		if (GetStatusItem != null)
		{
			GetComponent<KSelectable>().ToggleStatusItem(GetStatusItem(), !isSubmerged, this);
		}
	}

	protected override void OnCleanUp()
	{
		GameScenePartitioner.Instance.Free(ref partitionerEntry);
	}
}
