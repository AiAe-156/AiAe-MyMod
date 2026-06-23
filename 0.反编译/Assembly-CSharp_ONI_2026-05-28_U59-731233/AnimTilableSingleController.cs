using System;
using UnityEngine;

public class AnimTilableSingleController : KMonoBehaviour
{
	private HandleVector<int>.Handle partitionerEntry;

	public ObjectLayer objectLayer = ObjectLayer.Building;

	public Tag[] tagsOfNeightboursThatICanTileWith;

	private Extents extents;

	public Action<KBatchedAnimController, bool, bool, bool, bool> RefreshAnimCallback;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		if (tagsOfNeightboursThatICanTileWith == null || tagsOfNeightboursThatICanTileWith.Length == 0)
		{
			tagsOfNeightboursThatICanTileWith = new Tag[1] { GetComponent<KPrefabID>().PrefabTag };
		}
	}

	protected override void OnSpawn()
	{
		OccupyArea component = GetComponent<OccupyArea>();
		if (component != null)
		{
			this.extents = component.GetExtents();
		}
		else
		{
			Building component2 = GetComponent<Building>();
			this.extents = component2.GetExtents();
		}
		Extents extents = new Extents(this.extents.x - 1, this.extents.y - 1, this.extents.width + 2, this.extents.height + 2);
		partitionerEntry = GameScenePartitioner.Instance.Add("AnimTileableSingleController.OnSpawn", base.gameObject, extents, GameScenePartitioner.Instance.objectLayers[(int)objectLayer], OnNeighbourCellsUpdated);
		KBatchedAnimController component3 = GetComponent<KBatchedAnimController>();
		RefreshAnim();
	}

	protected override void OnCleanUp()
	{
		GameScenePartitioner.Instance.Free(ref partitionerEntry);
		base.OnCleanUp();
	}

	private void RefreshAnim()
	{
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		if (RefreshAnimCallback != null)
		{
			int cell = Grid.PosToCell(this);
			bool arg = true;
			bool arg2 = true;
			bool arg3 = true;
			bool arg4 = true;
			Grid.CellToXY(cell, out var x, out var y);
			CellOffset offset = new CellOffset(extents.x - x - 1, 0);
			CellOffset offset2 = new CellOffset(extents.x - x + extents.width, 0);
			CellOffset offset3 = new CellOffset(0, extents.y - y + extents.height);
			CellOffset offset4 = new CellOffset(0, extents.y - y - 1);
			Rotatable component2 = GetComponent<Rotatable>();
			if ((bool)component2)
			{
				offset = component2.GetRotatedCellOffset(offset);
				offset2 = component2.GetRotatedCellOffset(offset2);
				offset3 = component2.GetRotatedCellOffset(offset3);
				offset4 = component2.GetRotatedCellOffset(offset4);
			}
			int num = Grid.OffsetCell(cell, offset);
			int num2 = Grid.OffsetCell(cell, offset2);
			int num3 = Grid.OffsetCell(cell, offset3);
			int num4 = Grid.OffsetCell(cell, offset4);
			if (Grid.IsValidCell(num))
			{
				arg = HasTileableNeighbour(num);
			}
			if (Grid.IsValidCell(num2))
			{
				arg2 = HasTileableNeighbour(num2);
			}
			if (Grid.IsValidCell(num3))
			{
				arg3 = HasTileableNeighbour(num3);
			}
			if (Grid.IsValidCell(num4))
			{
				arg4 = HasTileableNeighbour(num4);
			}
			RefreshAnimCallback(component, arg3, arg2, arg4, arg);
		}
	}

	private bool HasTileableNeighbour(int neighbour_cell)
	{
		bool result = false;
		GameObject gameObject = Grid.Objects[neighbour_cell, (int)objectLayer];
		if (gameObject != null)
		{
			KPrefabID component = gameObject.GetComponent<KPrefabID>();
			if (component != null && component.HasAnyTags(tagsOfNeightboursThatICanTileWith))
			{
				result = true;
			}
		}
		return result;
	}

	private void OnNeighbourCellsUpdated(object data)
	{
		if (partitionerEntry.IsValid())
		{
			RefreshAnim();
		}
	}
}
