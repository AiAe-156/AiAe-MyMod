using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class GridLayouter
{
	public float minCellSize = -1f;

	public float maxCellSize = -1f;

	public List<GridLayoutGroup> targetGridLayouts = null;

	public RectTransform overrideParentForSizeReference = null;

	public System.Action OnSizeGridComplete;

	private Vector2 oldScreenSize;

	private float oldScreenScale;

	private int framesLeftToResizeGrid;

	[Conditional("UNITY_EDITOR")]
	private void ValidateImportantFieldsAreSet()
	{
		Debug.Assert(minCellSize >= 0f, string.Format("[{0} Error] Minimum cell size is invalid. Given: {1}", "GridLayouter", minCellSize));
		Debug.Assert(maxCellSize >= 0f, string.Format("[{0} Error] Maximum cell size is invalid. Given: {1}", "GridLayouter", maxCellSize));
		Debug.Assert(targetGridLayouts != null, string.Format("[{0} Error] Target grid layout is invalid. Given: {1}", "GridLayouter", targetGridLayouts));
	}

	public void CheckIfShouldResizeGrid()
	{
		Vector2 vector = new Vector2(Screen.width, Screen.height);
		if (vector != oldScreenSize)
		{
			RequestGridResize();
		}
		oldScreenSize = vector;
		float num = KPlayerPrefs.GetFloat(KCanvasScaler.UIScalePrefKey);
		if (num != oldScreenScale)
		{
			RequestGridResize();
		}
		oldScreenScale = num;
		ResizeGridIfRequested();
	}

	public void RequestGridResize()
	{
		framesLeftToResizeGrid = 3;
	}

	private void ResizeGridIfRequested()
	{
		if (framesLeftToResizeGrid > 0)
		{
			ImmediateSizeGridToScreenResolution();
			framesLeftToResizeGrid--;
			if (framesLeftToResizeGrid == 0 && OnSizeGridComplete != null)
			{
				OnSizeGridComplete();
			}
		}
	}

	public void ImmediateSizeGridToScreenResolution()
	{
		foreach (GridLayoutGroup targetGridLayout in targetGridLayouts)
		{
			float num = ((overrideParentForSizeReference != null) ? overrideParentForSizeReference.rect.size.x : targetGridLayout.transform.parent.rectTransform().rect.size.x);
			float workingWidth = num - (float)targetGridLayout.padding.left - (float)targetGridLayout.padding.right;
			float x = targetGridLayout.spacing.x;
			int num2 = GetCellCountToFit(maxCellSize, x, workingWidth) + 1;
			int num3 = num2;
			float num4;
			for (num4 = GetCellSize(workingWidth, x, num3); num4 < minCellSize; num4 = Mathf.Min(maxCellSize, GetCellSize(workingWidth, x, num3)))
			{
				num3--;
				if (num3 <= 0)
				{
					num3 = 1;
					num4 = minCellSize;
					break;
				}
			}
			targetGridLayout.childAlignment = ((num3 == 1) ? TextAnchor.UpperCenter : TextAnchor.UpperLeft);
			targetGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			targetGridLayout.constraintCount = num3;
			targetGridLayout.cellSize = Vector2.one * num4;
		}
		static int GetCellCountToFit(float cellSize, float spacingSize, float num7)
		{
			int num5 = 0;
			for (float num6 = cellSize; num6 < num7; num6 += cellSize + spacingSize)
			{
				num5++;
			}
			return num5;
		}
		static float GetCellSize(float num5, float spacingSize, int count)
		{
			return (num5 - (spacingSize * (float)count - 1f)) / (float)count;
		}
	}
}
