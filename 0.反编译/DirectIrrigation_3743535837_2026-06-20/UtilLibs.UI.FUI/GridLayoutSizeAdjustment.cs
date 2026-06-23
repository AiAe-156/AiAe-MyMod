using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UtilLibs.UI.FUI;

public class GridLayoutSizeAdjustment : KMonoBehaviour
{
	[MyCmpReq]
	private GridLayoutGroup referencedLayoutGroup;

	[MyCmpGet]
	private RectTransform rectTransform;

	private int paddingTop;

	private int paddingBottom;

	private int paddingLeft;

	private int paddingRight;

	private float WidthToHeightRatio;

	public int minSize = 80;

	public int maxSize = 120;

	public bool allignWithWidth = true;

	private float spacingX;

	private float spacingY;

	private bool sizeRatioSet = false;

	public override void OnSpawn()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		((KMonoBehaviour)this).OnSpawn();
		paddingBottom = ((LayoutGroup)referencedLayoutGroup).padding.bottom;
		paddingLeft = ((LayoutGroup)referencedLayoutGroup).padding.left;
		paddingTop = ((LayoutGroup)referencedLayoutGroup).padding.top;
		paddingRight = ((LayoutGroup)referencedLayoutGroup).padding.right;
		spacingX = referencedLayoutGroup.spacing.x;
		spacingY = referencedLayoutGroup.spacing.y;
		((MonoBehaviour)this).StartCoroutine(GetRatio());
	}

	private IEnumerator GetRatio()
	{
		yield return (object)new WaitForEndOfFrame();
		WidthToHeightRatio = referencedLayoutGroup.cellSize.x / referencedLayoutGroup.cellSize.y;
		sizeRatioSet = true;
		RequestGridResize();
	}

	public void SetValues(int minSize, int maxSize, bool allignWithWidth = true)
	{
		this.minSize = Math.Min(minSize, maxSize);
		this.maxSize = Math.Max(maxSize, minSize);
		this.allignWithWidth = allignWithWidth;
		RequestGridResize();
	}

	public unsafe void RequestGridResize()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		if (!sizeRatioSet)
		{
			return;
		}
		Rect rect;
		float num;
		if (!allignWithWidth)
		{
			rect = rectTransform.rect;
			num = ((Rect)(ref rect)).height - (float)paddingTop - (float)paddingBottom + spacingY - 1f;
		}
		else
		{
			rect = rectTransform.rect;
			num = ((Rect)(ref rect)).width - (float)paddingLeft - (float)paddingRight + spacingX - 1f;
		}
		float num2 = num;
		string[] obj = new string[5]
		{
			((LayoutGroup)referencedLayoutGroup).padding.left.ToString(),
			",",
			((LayoutGroup)referencedLayoutGroup).padding.right.ToString(),
			",",
			null
		};
		rect = rectTransform.rect;
		obj[4] = ((Rect)(ref rect)).width.ToString();
		SgtLogger.l(string.Concat(obj));
		SgtLogger.l(num2.ToString(), "SIZE");
		List<Tuple<int, float>> list = new List<Tuple<int, float>>();
		for (int i = minSize; i <= maxSize; i++)
		{
			float num3 = (allignWithWidth ? ((float)i + spacingX) : ((float)i + spacingY));
			float num4 = num2 / num3;
			float num5 = (float)Math.Truncate(num4);
			float num6 = num4 - num5;
			list.Add(new Tuple<int, float>(i, num6));
			SgtLogger.l(num3 + " : " + num4 + " : " + num5 + " : " + num6);
		}
		list = list.OrderBy((Tuple<int, float> tu) => tu.second).ToList();
		foreach (Tuple<int, float> item in list)
		{
			SgtLogger.l(item.second.ToString(), item.first.ToString());
		}
		int first = list.First().first;
		SgtLogger.l(first + ", sizeRatio " + WidthToHeightRatio, "NEW");
		Vector2 cellSize = default(Vector2);
		((Vector2)(ref cellSize))._002Ector(allignWithWidth ? ((float)first) : ((float)first * WidthToHeightRatio), (!allignWithWidth) ? ((float)first) : ((float)first / WidthToHeightRatio));
		SgtLogger.l(((object)(*(Vector2*)(&cellSize))/*cast due to .constrained prefix*/).ToString(), "NEWSIZE");
		referencedLayoutGroup.cellSize = cellSize;
	}
}
