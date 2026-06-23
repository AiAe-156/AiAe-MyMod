using System.Collections.Generic;
using UnityEngine;

public class CancelTool : FilteredDragTool
{
	public static CancelTool Instance;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
	}

	protected override void GetDefaultFilters(out ToolParameterMenu.ToggleData[] filters)
	{
		base.GetDefaultFilters(out filters);
		List<ToolParameterMenu.ToggleData> list = new List<ToolParameterMenu.ToggleData>(filters);
		list.Add(new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.CLEANANDCLEAR, ToolParameterMenu.ToggleState.Off));
		list.Add(new ToolParameterMenu.ToggleData(ToolParameterMenu.FILTERLAYERS.DIGPLACER, ToolParameterMenu.ToggleState.Off));
		filters = list.ToArray();
	}

	protected override string GetConfirmSound()
	{
		return "Tile_Confirm_NegativeTool";
	}

	protected override string GetDragSound()
	{
		return "Tile_Drag_NegativeTool";
	}

	protected override void OnDragTool(int cell, int distFromOrigin)
	{
		for (int i = 0; i < 45; i++)
		{
			GameObject gameObject = Grid.Objects[cell, i];
			if (gameObject != null)
			{
				string filterLayerFromGameObject = GetFilterLayerFromGameObject(gameObject);
				if (IsActiveLayer(filterLayerFromGameObject))
				{
					gameObject.Trigger(2127324410);
				}
			}
		}
	}

	protected override void OnDragComplete(Vector3 downPos, Vector3 upPos)
	{
		Vector2 regularizedPos = GetRegularizedPos(Vector2.Min((Vector2)downPos, (Vector2)upPos), minimize: true);
		Vector2 regularizedPos2 = GetRegularizedPos(Vector2.Max((Vector2)downPos, (Vector2)upPos), minimize: false);
		AttackTool.MarkForAttack(regularizedPos, regularizedPos2, mark: false);
		CaptureTool.MarkForCapture(regularizedPos, regularizedPos2, mark: false);
	}
}
