using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RequiresFoundation : KGameObjectComponentManager<RequiresFoundation.Data>, IKComponentManager
{
	public struct Data
	{
		public int cell;

		public int width;

		public int height;

		public BuildLocationRule buildRule;

		public HandleVector<int>.Handle partitionerEntry1;

		public HandleVector<int>.Handle partitionerEntry2;

		public bool validFoundation;

		public Operational.Flag operationalFlag;

		public GameObject go;

		public StatusItem noFoundationStatusItem;

		public Action<object> changeCallback;
	}

	public static readonly Operational.Flag solidFoundation = new Operational.Flag("solid_foundation", Operational.Flag.Type.Functional);

	public static readonly Operational.Flag backwallFoundation = new Operational.Flag("backwall_foundation", Operational.Flag.Type.Functional);

	public HandleVector<int>.Handle Add(GameObject go)
	{
		BuildingDef def = go.GetComponent<Building>().Def;
		int cell = Grid.PosToCell(go.transform.GetPosition());
		Data data = new Data
		{
			cell = cell,
			width = def.WidthInCells,
			height = def.HeightInCells,
			buildRule = def.BuildLocationRule,
			validFoundation = true,
			operationalFlag = solidFoundation,
			go = go
		};
		if (data.buildRule == BuildLocationRule.OnBackWall)
		{
			data.operationalFlag = backwallFoundation;
			data.noFoundationStatusItem = Db.Get().BuildingStatusItems.MissingFoundationBackwall;
		}
		else
		{
			data.operationalFlag = solidFoundation;
			data.noFoundationStatusItem = Db.Get().BuildingStatusItems.MissingFoundation;
		}
		HandleVector<int>.Handle h = Add(go, data);
		if (def.ContinuouslyCheckFoundation)
		{
			Rotatable component = data.go.GetComponent<Rotatable>();
			Orientation orientation = ((component != null) ? component.GetOrientation() : Orientation.Neutral);
			int num = -(def.WidthInCells - 1) / 2;
			int x = def.WidthInCells / 2;
			CellOffset offset = new CellOffset(num, -1);
			CellOffset offset2 = new CellOffset(x, -1);
			switch (data.buildRule)
			{
			case BuildLocationRule.OnCeiling:
			case BuildLocationRule.InCorner:
				offset.y = def.HeightInCells;
				offset2.y = def.HeightInCells;
				break;
			case BuildLocationRule.OnWall:
				offset = new CellOffset(num - 1, 0);
				offset2 = new CellOffset(num - 1, def.HeightInCells);
				break;
			case BuildLocationRule.WallFloor:
				offset = new CellOffset(num - 1, -1);
				offset2 = new CellOffset(x, def.HeightInCells - 1);
				break;
			case BuildLocationRule.OnBackWall:
				offset = new CellOffset(num, 0);
				offset2 = new CellOffset(x, def.HeightInCells - 1);
				break;
			}
			CellOffset rotatedCellOffset = Rotatable.GetRotatedCellOffset(offset, orientation);
			CellOffset rotatedCellOffset2 = Rotatable.GetRotatedCellOffset(offset2, orientation);
			int cell2 = Grid.OffsetCell(cell, rotatedCellOffset);
			int cell3 = Grid.OffsetCell(cell, rotatedCellOffset2);
			Vector2I vector2I = Grid.CellToXY(cell2);
			Vector2I vector2I2 = Grid.CellToXY(cell3);
			float xmin = Mathf.Min(vector2I.x, vector2I2.x);
			float xmax = Mathf.Max(vector2I.x, vector2I2.x);
			float ymin = Mathf.Min(vector2I.y, vector2I2.y);
			float ymax = Mathf.Max(vector2I.y, vector2I2.y);
			Rect rect = Rect.MinMaxRect(xmin, ymin, xmax, ymax);
			if (data.buildRule == BuildLocationRule.OnBackWall)
			{
				data.changeCallback = delegate
				{
					OnFoundationChanged(h);
				};
				data.partitionerEntry1 = GameScenePartitioner.Instance.Add("RequiresFoundation.Add", go, (int)rect.x, (int)rect.y, (int)rect.width + 1, (int)rect.height + 1, GameScenePartitioner.Instance.objectLayers[2], data.changeCallback);
			}
			else
			{
				data.changeCallback = delegate
				{
					OnFoundationChanged(h);
				};
				data.partitionerEntry1 = GameScenePartitioner.Instance.Add("RequiresFoundation.Add", go, (int)rect.x, (int)rect.y, (int)rect.width + 1, (int)rect.height + 1, GameScenePartitioner.Instance.solidChangedLayer, data.changeCallback);
				data.partitionerEntry2 = GameScenePartitioner.Instance.Add("RequiresFoundation.Add", go, (int)rect.x, (int)rect.y, (int)rect.width + 1, (int)rect.height + 1, GameScenePartitioner.Instance.objectLayers[1], data.changeCallback);
			}
			if (def.BuildLocationRule == BuildLocationRule.BuildingAttachPoint || def.BuildLocationRule == BuildLocationRule.OnFloorOrBuildingAttachPoint)
			{
				AttachableBuilding component2 = data.go.GetComponent<AttachableBuilding>();
				component2.onAttachmentNetworkChanged = (Action<object>)Delegate.Combine(component2.onAttachmentNetworkChanged, data.changeCallback);
			}
			SetData(h, data);
			data.changeCallback(h);
			data = GetData(h);
			UpdateValidFoundationState(data.validFoundation, ref data, forceUpdate: true);
		}
		return h;
	}

	protected override void OnCleanUp(HandleVector<int>.Handle h)
	{
		Data new_data = GetData(h);
		GameScenePartitioner.Instance.Free(ref new_data.partitionerEntry1);
		GameScenePartitioner.Instance.Free(ref new_data.partitionerEntry2);
		AttachableBuilding component = new_data.go.GetComponent<AttachableBuilding>();
		if (!component.IsNullOrDestroyed())
		{
			component.onAttachmentNetworkChanged = (Action<object>)Delegate.Remove(component.onAttachmentNetworkChanged, new_data.changeCallback);
		}
		SetData(h, new_data);
	}

	private void OnFoundationChanged(HandleVector<int>.Handle h)
	{
		Data new_data = GetData(h);
		SimCellOccupier component = new_data.go.GetComponent<SimCellOccupier>();
		if (!(component == null) && !component.IsReady())
		{
			return;
		}
		Rotatable component2 = new_data.go.GetComponent<Rotatable>();
		Orientation orientation = ((component2 != null) ? component2.GetOrientation() : Orientation.Neutral);
		bool flag = BuildingDef.CheckFoundation(new_data.cell, orientation, new_data.buildRule, new_data.width, new_data.height);
		if (!flag && (new_data.buildRule == BuildLocationRule.BuildingAttachPoint || new_data.buildRule == BuildLocationRule.OnFloorOrBuildingAttachPoint))
		{
			List<GameObject> buildings = new List<GameObject>();
			AttachableBuilding.GetAttachedBelow(new_data.go.GetComponent<AttachableBuilding>(), ref buildings);
			if (buildings.Count > 0)
			{
				Operational component3 = buildings.Last().GetComponent<Operational>();
				if (component3 != null && component3.GetFlag(new_data.operationalFlag))
				{
					flag = true;
				}
			}
		}
		UpdateValidFoundationState(flag, ref new_data);
		SetData(h, new_data);
	}

	private void UpdateValidFoundationState(bool is_validFoundation, ref Data data, bool forceUpdate = false)
	{
		if (data.validFoundation != is_validFoundation || forceUpdate)
		{
			data.validFoundation = is_validFoundation;
			Operational component = data.go.GetComponent<Operational>();
			if (component != null)
			{
				component.SetFlag(data.operationalFlag, is_validFoundation);
			}
			AttachableBuilding component2 = data.go.GetComponent<AttachableBuilding>();
			if (component2 != null)
			{
				List<GameObject> buildings = new List<GameObject>();
				AttachableBuilding.GetAttachedAbove(component2, ref buildings);
				AttachableBuilding.NotifyBuildingsNetworkChanged(buildings);
			}
			data.go.GetComponent<KSelectable>().ToggleStatusItem(data.noFoundationStatusItem, !is_validFoundation, this);
		}
	}
}
