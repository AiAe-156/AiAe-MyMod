using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineLayer : GraphLayer
{
	[Serializable]
	public struct LineFormat
	{
		public Color color;

		public int thickness;
	}

	public enum DataScalingType
	{
		Average,
		Max,
		DropValues
	}

	private const int WIDTH = 96;

	private const int HEIGHT = 32;

	private static Color32[] s_pixelBuffer = new Color32[3072];

	[Header("Lines")]
	public LineFormat[] line_formatting;

	public Image areaFill;

	public GameObject prefab_line;

	public GameObject line_container;

	private List<GraphedLine> lines = new List<GraphedLine>();

	protected float fillAlphaMin = 0.33f;

	protected float fillFadePixels = 15f;

	public bool fillAreaUnderLine = false;

	private Texture2D areaTexture;

	private int compressDataToPointCount = 256;

	private DataScalingType compressType = DataScalingType.DropValues;

	private RectTransform rectTransform;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		InitAreaTexture();
		rectTransform = base.gameObject.GetComponent<RectTransform>();
	}

	private void InitAreaTexture()
	{
		if (!(areaTexture != null))
		{
			areaTexture = new Texture2D(96, 32);
			areaFill.sprite = Sprite.Create(areaTexture, new Rect(0f, 0f, 96f, 32f), new Vector2(0.5f, 0.5f), 100f);
			areaTexture.filterMode = FilterMode.Point;
		}
	}

	public virtual GraphedLine NewLine(Tuple<float, float>[] points, string ID = "")
	{
		Vector2[] array = new Vector2[points.Length];
		for (int i = 0; i < points.Length; i++)
		{
			array[i] = new Vector2(points[i].first, points[i].second);
		}
		if (fillAreaUnderLine)
		{
			Vector2 vector = CalculateMin(points);
			Vector2 vector2 = CalculateMax(points);
			Vector2 vector3 = vector2 - vector;
			for (int j = 0; j < 96; j++)
			{
				float num = vector.x + vector3.x * ((float)j / 96f);
				if (points.Length <= 1)
				{
					continue;
				}
				int num2 = 1;
				for (int k = 1; k < points.Length; k++)
				{
					if (points[k].first >= num)
					{
						num2 = k;
						break;
					}
				}
				Vector2 vector4 = new Vector2(points[num2].first - points[num2 - 1].first, points[num2].second - points[num2 - 1].second);
				float num3 = (num - points[num2 - 1].first) / vector4.x;
				bool flag = false;
				int num4 = -1;
				for (int num5 = 31; num5 >= 0; num5--)
				{
					if (!flag && vector.y + vector3.y * ((float)num5 / 32f) < points[num2 - 1].second + vector4.y * num3)
					{
						flag = true;
						num4 = num5;
					}
					Color32 color = (flag ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)(255f * Mathf.Lerp(1f, fillAlphaMin, Mathf.Clamp((float)(num4 - num5) / fillFadePixels, 0f, 1f)))) : ((Color32)Color.clear));
					s_pixelBuffer[num5 * 96 + j] = color;
				}
			}
			InitAreaTexture();
			areaTexture.SetPixels32(s_pixelBuffer);
			areaTexture.Apply();
			areaFill.color = line_formatting[0].color;
		}
		return NewLine(array, ID);
	}

	private GraphedLine FindLine(string ID)
	{
		string text = $"line_{ID}";
		foreach (GraphedLine line in lines)
		{
			if (line.name == text)
			{
				return line.GetComponent<GraphedLine>();
			}
		}
		GameObject gameObject = Util.KInstantiateUI(prefab_line, line_container, force_active: true);
		gameObject.name = text;
		GraphedLine component = gameObject.GetComponent<GraphedLine>();
		lines.Add(component);
		return component;
	}

	public virtual void RefreshLine(Tuple<float, float>[] data, string ID)
	{
		FillArea(data);
		Vector2[] array2;
		if (data.Length > compressDataToPointCount)
		{
			Vector2[] array = new Vector2[compressDataToPointCount];
			if (compressType == DataScalingType.DropValues)
			{
				float num = data.Length - compressDataToPointCount + 1;
				float num2 = (float)data.Length / num;
				int num3 = 0;
				float num4 = 0f;
				for (int i = 0; i < data.Length; i++)
				{
					num4 += 1f;
					if (num4 >= num2)
					{
						num4 -= num2;
						continue;
					}
					array[num3] = new Vector2(data[i].first, data[i].second);
					num3++;
				}
				if (array[compressDataToPointCount - 1] == Vector2.zero)
				{
					array[compressDataToPointCount - 1] = array[compressDataToPointCount - 2];
				}
			}
			else
			{
				int num5 = data.Length / compressDataToPointCount;
				for (int j = 0; j < compressDataToPointCount; j++)
				{
					if (j <= 0)
					{
						continue;
					}
					float num6 = 0f;
					switch (compressType)
					{
					case DataScalingType.Max:
					{
						for (int l = 0; l < num5; l++)
						{
							num6 = Mathf.Max(num6, data[j * num5 - l].second);
						}
						break;
					}
					case DataScalingType.Average:
					{
						for (int k = 0; k < num5; k++)
						{
							num6 += data[j * num5 - k].second;
						}
						num6 /= (float)num5;
						break;
					}
					}
					array[j] = new Vector2(data[j * num5].first, num6);
				}
			}
			array2 = array;
		}
		else
		{
			array2 = new Vector2[data.Length];
			for (int m = 0; m < data.Length; m++)
			{
				array2[m] = new Vector2(data[m].first, data[m].second);
			}
		}
		GraphedLine graphedLine = FindLine(ID);
		graphedLine.SetPoints(array2);
		graphedLine.line_renderer.color = line_formatting[lines.Count % line_formatting.Length].color;
		graphedLine.line_renderer.LineThickness = line_formatting[lines.Count % line_formatting.Length].thickness;
	}

	private void FillArea(Tuple<float, float>[] points)
	{
		if (!fillAreaUnderLine)
		{
			return;
		}
		CalculateMinMax(points, out var min, out var max);
		Vector2 vector = max - min;
		Color32 color = new Color32(0, 0, 0, 0);
		Vector2 vector2 = default(Vector2);
		for (int i = 0; i < 96; i++)
		{
			float num = min.x + vector.x * ((float)i / 96f);
			if (points.Length <= 1)
			{
				continue;
			}
			int num2 = 1;
			for (int j = 1; j < points.Length; j++)
			{
				if (points[j].first >= num)
				{
					num2 = j;
					break;
				}
			}
			vector2.x = points[num2].first - points[num2 - 1].first;
			vector2.y = points[num2].second - points[num2 - 1].second;
			float num3 = (num - points[num2 - 1].first) / vector2.x;
			bool flag = false;
			int num4 = -1;
			for (int num5 = 31; num5 >= 0; num5--)
			{
				if (!flag && min.y + vector.y * ((float)num5 / 32f) < points[num2 - 1].second + vector2.y * num3)
				{
					flag = true;
					num4 = num5;
				}
				Color32 color2 = (flag ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)(255f * Mathf.Lerp(1f, fillAlphaMin, Mathf.Clamp((float)(num4 - num5) / fillFadePixels, 0f, 1f)))) : color);
				s_pixelBuffer[num5 * 96 + i] = color2;
			}
		}
		areaTexture.SetPixels32(s_pixelBuffer);
		areaTexture.Apply();
		areaFill.color = line_formatting[0].color;
	}

	private void CalculateMinMax(Tuple<float, float>[] points, out Vector2 min, out Vector2 max)
	{
		max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
		min = new Vector2(float.PositiveInfinity, 0f);
		for (int i = 0; i < points.Length; i++)
		{
			max = new Vector2(Mathf.Max(points[i].first, max.x), Mathf.Max(points[i].second, max.y));
			min = new Vector2(Mathf.Min(points[i].first, min.x), Mathf.Min(points[i].second, min.y));
		}
	}

	protected Vector2 CalculateMax(Tuple<float, float>[] points)
	{
		Vector2 result = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
		for (int i = 0; i < points.Length; i++)
		{
			result = new Vector2(Mathf.Max(points[i].first, result.x), Mathf.Max(points[i].second, result.y));
		}
		return result;
	}

	protected Vector2 CalculateMin(Tuple<float, float>[] points)
	{
		Vector2 result = new Vector2(float.PositiveInfinity, 0f);
		for (int i = 0; i < points.Length; i++)
		{
			result = new Vector2(Mathf.Min(points[i].first, result.x), Mathf.Min(points[i].second, result.y));
		}
		return result;
	}

	public GraphedLine NewLine(Vector2[] points, string ID = "")
	{
		GameObject gameObject = Util.KInstantiateUI(prefab_line, line_container, force_active: true);
		if (ID == "")
		{
			ID = lines.Count.ToString();
		}
		gameObject.name = $"line_{ID}";
		GraphedLine component = gameObject.GetComponent<GraphedLine>();
		if (points.Length > compressDataToPointCount)
		{
			Vector2[] array = new Vector2[compressDataToPointCount];
			if (compressType == DataScalingType.DropValues)
			{
				float num = points.Length - compressDataToPointCount + 1;
				float num2 = (float)points.Length / num;
				int num3 = 0;
				float num4 = 0f;
				for (int i = 0; i < points.Length; i++)
				{
					num4 += 1f;
					if (num4 >= num2)
					{
						num4 -= num2;
						continue;
					}
					array[num3] = points[i];
					num3++;
				}
				if (array[compressDataToPointCount - 1] == Vector2.zero)
				{
					array[compressDataToPointCount - 1] = array[compressDataToPointCount - 2];
				}
			}
			else
			{
				int num5 = points.Length / compressDataToPointCount;
				for (int j = 0; j < compressDataToPointCount; j++)
				{
					if (j <= 0)
					{
						continue;
					}
					float num6 = 0f;
					switch (compressType)
					{
					case DataScalingType.Max:
					{
						for (int l = 0; l < num5; l++)
						{
							num6 = Mathf.Max(num6, points[j * num5 - l].y);
						}
						break;
					}
					case DataScalingType.Average:
					{
						for (int k = 0; k < num5; k++)
						{
							num6 += points[j * num5 - k].y;
						}
						num6 /= (float)num5;
						break;
					}
					}
					array[j] = new Vector2(points[j * num5].x, num6);
				}
			}
			points = array;
		}
		component.SetPoints(points);
		component.line_renderer.color = line_formatting[lines.Count % line_formatting.Length].color;
		component.line_renderer.LineThickness = line_formatting[lines.Count % line_formatting.Length].thickness;
		lines.Add(component);
		return component;
	}

	public void ClearLines()
	{
		foreach (GraphedLine line in lines)
		{
			if (line != null && line.gameObject != null)
			{
				UnityEngine.Object.DestroyImmediate(line.gameObject);
			}
		}
		lines.Clear();
	}

	private void Update()
	{
		if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
		{
			for (int i = 0; i < lines.Count; i++)
			{
				lines[i].HidePointHighlight();
			}
			return;
		}
		Vector2 localPoint = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint);
		localPoint += rectTransform.sizeDelta / 2f;
		for (int j = 0; j < lines.Count; j++)
		{
			if (lines[j].PointCount != 0)
			{
				Vector2 closestDataToPointOnXAxis = lines[j].GetClosestDataToPointOnXAxis(localPoint);
				if (!float.IsInfinity(closestDataToPointOnXAxis.x) && !float.IsNaN(closestDataToPointOnXAxis.x) && !float.IsInfinity(closestDataToPointOnXAxis.y) && !float.IsNaN(closestDataToPointOnXAxis.y))
				{
					lines[j].SetPointHighlight(closestDataToPointOnXAxis);
				}
				else
				{
					lines[j].HidePointHighlight();
				}
			}
		}
	}
}
