using System;
using System.Collections.Generic;
using UnityEngine;

public class GridVisibleArea
{
	public struct Callback
	{
		public System.Action OnUpdate;

		public string Name;
	}

	private GridArea[] VisibleAreas = new GridArea[3];

	private GridArea[] VisibleAreasExtended = new GridArea[3];

	private List<Callback> Callbacks = new List<Callback>();

	public bool debugFreezeVisibleArea;

	public bool debugFreezeVisibleAreasExtended;

	private readonly int _padding;

	public GridArea CurrentArea => VisibleAreas[0];

	public GridArea PreviousArea => VisibleAreas[1];

	public GridArea PreviousPreviousArea => VisibleAreas[2];

	public GridArea CurrentAreaExtended => VisibleAreasExtended[0];

	public GridArea PreviousAreaExtended => VisibleAreasExtended[1];

	public GridArea PreviousPreviousAreaExtended => VisibleAreasExtended[2];

	public GridVisibleArea()
	{
	}

	public GridVisibleArea(int padding)
	{
		_padding = padding;
	}

	public void Update()
	{
		if (!debugFreezeVisibleArea)
		{
			VisibleAreas[2] = VisibleAreas[1];
			VisibleAreas[1] = VisibleAreas[0];
			VisibleAreas[0] = GetVisibleArea();
		}
		if (!debugFreezeVisibleAreasExtended)
		{
			VisibleAreasExtended[2] = VisibleAreasExtended[1];
			VisibleAreasExtended[1] = VisibleAreasExtended[0];
			VisibleAreasExtended[0] = GetVisibleAreaExtended(_padding);
		}
		foreach (Callback callback in Callbacks)
		{
			callback.OnUpdate();
		}
	}

	public void AddCallback(string name, System.Action on_update)
	{
		Callback item = new Callback
		{
			Name = name,
			OnUpdate = on_update
		};
		Callbacks.Add(item);
	}

	public void Run(Action<int> in_view)
	{
		if (in_view != null)
		{
			CurrentArea.Run(in_view);
		}
	}

	public void RunExtended(Action<int> in_view)
	{
		if (in_view != null)
		{
			CurrentAreaExtended.Run(in_view);
		}
	}

	public void Run(Action<int> outside_view, Action<int> inside_view, Action<int> inside_view_second_time)
	{
		if (outside_view != null)
		{
			PreviousArea.RunOnDifference(CurrentArea, outside_view);
		}
		if (inside_view != null)
		{
			CurrentArea.RunOnDifference(PreviousArea, inside_view);
		}
		if (inside_view_second_time != null)
		{
			PreviousArea.RunOnDifference(PreviousPreviousArea, inside_view_second_time);
		}
	}

	public void RunExtended(Action<int> outside_view, Action<int> inside_view, Action<int> inside_view_second_time)
	{
		if (outside_view != null)
		{
			PreviousAreaExtended.RunOnDifference(CurrentAreaExtended, outside_view);
		}
		if (inside_view != null)
		{
			CurrentAreaExtended.RunOnDifference(PreviousAreaExtended, inside_view);
		}
		if (inside_view_second_time != null)
		{
			PreviousAreaExtended.RunOnDifference(PreviousPreviousAreaExtended, inside_view_second_time);
		}
	}

	public void RunIfVisible(int cell, Action<int> action)
	{
		CurrentArea.RunIfInside(cell, action);
	}

	public void RunIfVisibleExtended(int cell, Action<int> action)
	{
		CurrentAreaExtended.RunIfInside(cell, action);
	}

	public static GridArea GetVisibleArea()
	{
		return GetVisibleAreaExtended(0);
	}

	public static GridArea GetVisibleAreaExtended(int padding)
	{
		GridArea result = default(GridArea);
		Camera mainCamera = Game.MainCamera;
		if (mainCamera != null)
		{
			Vector3 vector = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, mainCamera.transform.GetPosition().z));
			Vector3 vector2 = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, mainCamera.transform.GetPosition().z));
			vector.x += padding;
			vector.y += padding;
			vector2.x -= padding;
			vector2.y -= padding;
			if (CameraController.Instance != null)
			{
				CameraController.Instance.GetWorldCamera(out var worldOffset, out var worldSize);
				result.SetExtents(Math.Max((int)(vector2.x - 0.5f), worldOffset.x), Math.Max((int)(vector2.y - 0.5f), worldOffset.y), Math.Min((int)(vector.x + 1.5f), worldSize.x + worldOffset.x), Math.Min((int)(vector.y + 1.5f), worldSize.y + worldOffset.y));
			}
			else
			{
				result.SetExtents(Math.Max((int)(vector2.x - 0.5f), 0), Math.Max((int)(vector2.y - 0.5f), 0), Math.Min((int)(vector.x + 1.5f), Grid.WidthInCells), Math.Min((int)(vector.y + 1.5f), Grid.HeightInCells));
			}
		}
		return result;
	}
}
