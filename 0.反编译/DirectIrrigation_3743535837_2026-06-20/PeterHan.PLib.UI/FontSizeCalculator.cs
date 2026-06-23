using System;
using System.Collections.Generic;
using System.Reflection;
using PeterHan.PLib.Core;
using TMPro;
using UnityEngine.TextCore;

namespace PeterHan.PLib.UI;

internal sealed class FontSizeCalculator
{
	internal sealed class Metrics
	{
		public readonly float lineHeight;

		public readonly float pointSize;

		public readonly float scale;

		public Metrics(float lineHeight, float pointSize, float scale)
		{
			this.lineHeight = lineHeight;
			this.pointSize = pointSize;
			this.scale = scale;
		}

		public override string ToString()
		{
			return "FontMetrics[lineHeight={0:F2},pointSize={1:F2},scale={2:F2}]".F(lineHeight, pointSize, scale);
		}
	}

	internal static readonly FontSizeCalculator Instance;

	private static readonly PropertyInfo FACE_SIZE_NEW;

	private static readonly PropertyInfo GLYPH_NEW;

	private static readonly PropertyInfo GLYPH_DICTIONARY_NEW;

	private static readonly MethodInfo GLYPH_LOOKUP_NEW;

	private static readonly Type FACE_INFO_OLD;

	private static readonly FieldInfo FACE_HEIGHT_OLD;

	private static readonly FieldInfo FACE_SCALE_OLD;

	private static readonly FieldInfo FACE_SIZE_OLD;

	private static readonly PropertyInfo GET_INFO;

	private static readonly FieldInfo GLYPH_WIDTH_OLD;

	private static readonly PropertyInfo GLYPH_DICTIONARY_OLD;

	private static readonly MethodInfo GLYPH_LOOKUP_OLD;

	private readonly IDictionary<TMP_FontAsset, Metrics> fontMetrics;

	static FontSizeCalculator()
	{
		Instance = new FontSizeCalculator();
		FACE_INFO_OLD = PPatchTools.GetTypeSafe("TMPro.FaceInfo");
		if (FACE_INFO_OLD != null)
		{
			Type typeSafe = PPatchTools.GetTypeSafe("TMPro.TMP_Glyph");
			FACE_HEIGHT_OLD = FACE_INFO_OLD.GetFieldSafe("LineHeight", isStatic: false);
			FACE_SCALE_OLD = FACE_INFO_OLD.GetFieldSafe("Scale", isStatic: false);
			FACE_SIZE_OLD = FACE_INFO_OLD.GetFieldSafe("PointSize", isStatic: false);
			GLYPH_DICTIONARY_OLD = typeof(TMP_FontAsset).GetProperty("characterDictionary", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			GET_INFO = typeof(TMP_FontAsset).GetProperty("fontInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (typeSafe != null)
			{
				GLYPH_LOOKUP_OLD = typeof(Dictionary<, >).MakeGenericType(typeof(int), typeSafe).GetMethodSafe("TryGetValue", false, typeof(int), typeSafe.MakeByRefType());
				GLYPH_WIDTH_OLD = typeSafe.GetFieldSafe("width", isStatic: false);
			}
			else
			{
				GLYPH_LOOKUP_OLD = null;
				GLYPH_WIDTH_OLD = null;
			}
		}
		else
		{
			PPatchTools.GetTypeSafe("UnityEngine.TextCore.GlyphMetrics");
			Type typeSafe2 = PPatchTools.GetTypeSafe("TMPro.TMP_Character");
			FACE_SIZE_NEW = typeof(FaceInfo).GetPropertySafe<float>("pointSize", isStatic: false);
			GLYPH_NEW = typeof(TMP_TextElement).GetPropertySafe<Glyph>("glyph", isStatic: false);
			GLYPH_DICTIONARY_NEW = typeof(TMP_FontAsset).GetProperty("characterLookupTable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			GET_INFO = typeof(TMP_Asset).GetProperty("faceInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (typeSafe2 != null)
			{
				GLYPH_LOOKUP_NEW = typeof(Dictionary<, >).MakeGenericType(typeof(uint), typeSafe2).GetMethodSafe("TryGetValue", false, typeof(uint), typeSafe2.MakeByRefType());
			}
			else
			{
				GLYPH_LOOKUP_NEW = null;
			}
		}
	}

	internal static float GetCharWidth(char ch, TMP_FontAsset font)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		float result = 0f;
		if (GLYPH_DICTIONARY_NEW != null)
		{
			object value = GLYPH_DICTIONARY_NEW.GetValue(font);
			object[] array = new object[2]
			{
				(uint)ch,
				null
			};
			bool flag = default(bool);
			int num;
			if (value != null)
			{
				object obj = GLYPH_LOOKUP_NEW?.Invoke(value, array);
				if (obj is bool)
				{
					flag = (bool)obj;
					num = 1;
				}
				else
				{
					num = 0;
				}
			}
			else
			{
				num = 0;
			}
			object obj2;
			if (((uint)num & (flag ? 1u : 0u)) != 0 && (obj2 = array[1]) != null)
			{
				object? obj3 = GLYPH_NEW?.GetValue(obj2);
				Glyph val = (Glyph)((obj3 is Glyph) ? obj3 : null);
				if (val != null)
				{
					GlyphMetrics metrics = val.metrics;
					result = ((GlyphMetrics)(ref metrics)).width;
				}
			}
		}
		else if (GLYPH_DICTIONARY_OLD != null)
		{
			object value2 = GLYPH_DICTIONARY_OLD.GetValue(font);
			object[] array2 = new object[2]
			{
				(int)ch,
				null
			};
			bool flag2 = default(bool);
			int num2;
			if (value2 != null)
			{
				object obj = GLYPH_LOOKUP_OLD?.Invoke(value2, array2);
				if (obj is bool)
				{
					flag2 = (bool)obj;
					num2 = 1;
				}
				else
				{
					num2 = 0;
				}
			}
			else
			{
				num2 = 0;
			}
			object obj2;
			if (((uint)num2 & (flag2 ? 1u : 0u)) != 0 && (obj2 = array2[1]) != null && GLYPH_WIDTH_OLD?.GetValue(obj2) is float num3)
			{
				result = num3;
			}
		}
		return result;
	}

	private FontSizeCalculator()
	{
		fontMetrics = new Dictionary<TMP_FontAsset, Metrics>(32);
	}

	private Metrics CalculateMetrics(TMP_FontAsset font)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		float lineHeight = 0f;
		float pointSize = 0f;
		float scale = 0f;
		object obj = GET_INFO?.GetValue(font);
		if (obj is FaceInfo val)
		{
			lineHeight = ((FaceInfo)(ref val)).lineHeight;
			if (FACE_SIZE_NEW.GetValue(obj) is float num)
			{
				pointSize = num;
			}
			scale = ((FaceInfo)(ref val)).scale;
		}
		else if (obj != null && FACE_HEIGHT_OLD != null)
		{
			if (FACE_HEIGHT_OLD.GetValue(obj) is float num2)
			{
				lineHeight = num2;
			}
			if (FACE_SIZE_OLD.GetValue(obj) is float num3)
			{
				pointSize = num3;
			}
			if (FACE_SCALE_OLD.GetValue(obj) is float num4)
			{
				scale = num4;
			}
		}
		return new Metrics(lineHeight, pointSize, scale);
	}

	internal void Cleanup()
	{
		fontMetrics.Clear();
	}

	internal Metrics Get(TMP_FontAsset font)
	{
		if (!fontMetrics.TryGetValue(font, out var value))
		{
			value = CalculateMetrics(font);
			fontMetrics.Add(font, value);
		}
		return value;
	}
}
