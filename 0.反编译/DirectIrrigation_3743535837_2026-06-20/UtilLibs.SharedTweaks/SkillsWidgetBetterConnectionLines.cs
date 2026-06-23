using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Database;
using HarmonyLib;
using PeterHan.PLib.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace UtilLibs.SharedTweaks;

public sealed class SkillsWidgetBetterConnectionLines : PForwardedComponent
{
	private static float Y_Step = 102.3f;

	private static bool debug = false;

	private static Tag LastModel = Tag.op_Implicit((string)null);

	private static Dictionary<string, Vector2I> lookupTable = new Dictionary<string, Vector2I>();

	private static Dictionary<Vector2I, string> reverseLookupTable = new Dictionary<Vector2I, string>();

	private static int gradient = 0;

	public override Version Version => new Version(1, 0, 0, 0);

	public static void Register()
	{
		new SkillsWidgetBetterConnectionLines().RegisterForForwarding();
	}

	public override void Initialize(Harmony plibInstance)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(SkillWidget), "RefreshLines", (Type[])null, (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(SkillsWidgetBetterConnectionLines), "RefreshLinesPostfix", (Type[])null, (Type[])null);
			plibInstance.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(methodInfo2, 500, (string[])null, (string[])null, (bool?)null), (HarmonyMethod)null, (HarmonyMethod)null);
			Debug.Log((object)(GetType().ToString() + " successfully patched"));
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)(GetType().ToString() + " patch failed!"));
			Debug.LogWarning((object)ex.Message);
		}
	}

	public static void RefreshLinesPostfix(SkillWidget __instance)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		UILineRenderer[] lines = __instance.lines;
		foreach (UILineRenderer val in lines)
		{
			Object.Destroy((Object)(object)((Component)val).gameObject);
		}
		__instance.linePoints.Clear();
		List<Tuple<string, Vector2>> list = new List<Tuple<string, Vector2>>();
		List<Tuple<string, Vector2>> list2 = new List<Tuple<string, Vector2>>();
		List<Tuple<string, Vector2>> list3 = new List<Tuple<string, Vector2>>();
		Skill val2 = ((ResourceSet<Skill>)(object)Db.Get().Skills).Get(__instance.skillID);
		foreach (string priorSkill in val2.priorSkills)
		{
			Vector2 skillWidgetLineTargetPosition = __instance.skillsScreen.GetSkillWidgetLineTargetPosition(priorSkill);
			Vector3 position = TransformExtensions.GetPosition((Transform)(object)__instance.lines_left);
			float num = skillWidgetLineTargetPosition.y - position.y;
			Tuple<string, Vector2> item = new Tuple<string, Vector2>(priorSkill, skillWidgetLineTargetPosition);
			if (num < -1f)
			{
				list.Add(item);
			}
			else if (num > 1f)
			{
				list2.Add(item);
			}
			else
			{
				list3.Add(item);
			}
		}
		list2.Sort((Tuple<string, Vector2> a, Tuple<string, Vector2> b) => a.second.y.CompareTo(b.second.y));
		list.Sort((Tuple<string, Vector2> a, Tuple<string, Vector2> b) => -a.second.y.CompareTo(b.second.y));
		__instance.lines = Array.Empty<UILineRenderer>();
		foreach (Tuple<string, Vector2> item2 in list3)
		{
			if (debug)
			{
				SgtLogger.l(__instance.skillID + " even: " + item2.first);
			}
			CreateSkillConnection(__instance, val2, item2);
		}
		float num2 = 1f;
		if (!list3.Any() && list.Any() && list2.Any())
		{
			num2 = 0.5f;
		}
		float num3 = 1f;
		if (!list3.Any() && list.Any() && list2.Any())
		{
			num3 = 0.5f;
		}
		if (!list3.Any() && list.Any() != list2.Any())
		{
			num3 = 0f;
		}
		for (int num4 = 0; num4 < list.Count; num4++)
		{
			if (debug)
			{
				SgtLogger.l(__instance.skillID + " below: " + list[num4].first);
			}
			CreateSkillConnection(__instance, val2, list[num4], list.Count, 0f - num3 - (float)num4, list.Count > 1, 0f - num2 - (float)num4);
		}
		for (int num5 = 0; num5 < list2.Count; num5++)
		{
			if (debug)
			{
				SgtLogger.l(__instance.skillID + " above: " + list2[num5].first);
			}
			CreateSkillConnection(__instance, val2, list2[num5], list2.Count, num3 + (float)num5, list2.Count > 1, num2 + (float)num5);
		}
	}

	private static void PrintMatrix()
	{
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		if (reverseLookupTable.Count == 0)
		{
			Console.WriteLine("Matrix is empty.");
			return;
		}
		int num = reverseLookupTable.Keys.Min((Vector2I v) => ((Vector2I)(ref v)).X);
		int num2 = reverseLookupTable.Keys.Max((Vector2I v) => ((Vector2I)(ref v)).X);
		int num3 = reverseLookupTable.Keys.Min((Vector2I v) => ((Vector2I)(ref v)).Y);
		int num4 = reverseLookupTable.Keys.Max((Vector2I v) => ((Vector2I)(ref v)).Y);
		Console.WriteLine($"Matrix from X={num}..{num2}, Y={num3}..{num4}\n");
		Console.Write("|||");
		for (int num5 = num; num5 <= num2; num5++)
		{
			Console.Write($"     {num5}     |");
		}
		Console.WriteLine();
		Vector2I key = default(Vector2I);
		for (int num6 = num4; num6 >= num3; num6--)
		{
			Console.Write(num6 + ((num6 < 10) ? " |" : "|"));
			for (int num7 = num; num7 <= num2; num7++)
			{
				((Vector2I)(ref key))._002Ector(num7, num6);
				if (reverseLookupTable.TryGetValue(key, out var value))
				{
					value = ((value.Length > 10) ? value.Substring(0, 10) : value.PadRight(10, ' '));
					Console.Write("[" + value + "]");
				}
				else
				{
					Console.Write("[          ]");
				}
			}
			Console.WriteLine();
		}
	}

	private static void RefreshSkillScreenMatrix(SkillWidget __instance)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		if (LastModel == __instance.skillsScreen.SelectedMinionModel())
		{
			return;
		}
		lookupTable.Clear();
		reverseLookupTable.Clear();
		List<KeyValuePair<string, GameObject>> list = (from widget in __instance.skillsScreen.skillWidgets
			orderby widget.Value.transform.position.y, widget.Value.transform.position.x
			select widget).ToList();
		Tag model = __instance.skillsScreen.SelectedMinionModel();
		Skills skills = Db.Get().Skills;
		list.RemoveAll((KeyValuePair<string, GameObject> id) => ((ResourceSet<Skill>)(object)skills).Get(id.Key).requiredDuplicantModel != null && Tag.op_Implicit(((ResourceSet<Skill>)(object)skills).Get(id.Key).requiredDuplicantModel) != model);
		float num = float.MinValue;
		int num2 = -1;
		Vector2I val2 = default(Vector2I);
		foreach (KeyValuePair<string, GameObject> item in list)
		{
			GameObject value = item.Value;
			Skill val = ((ResourceSet<Skill>)(object)skills).Get(item.Key);
			float x = value.transform.position.x;
			float y = value.transform.position.y;
			if (y > num)
			{
				num = y;
				num2++;
			}
			int num3 = num2;
			int tier = val.tier;
			((Vector2I)(ref val2))._002Ector(tier, num3);
			lookupTable.Add(((Resource)val).Id, val2);
			reverseLookupTable.Add(val2, ((Resource)val).Id);
		}
		if (reverseLookupTable.Count < 2)
		{
			return;
		}
		int num4 = 0;
		string text = null;
		string text2 = null;
		int compareStart = 0;
		while ((text == null || text2 == null) && num4 < 30)
		{
			num4++;
			KeyValuePair<Vector2I, string> keyValuePair = reverseLookupTable.FirstOrDefault(delegate(KeyValuePair<Vector2I, string> e)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				Vector2I key = e.Key;
				return ((Vector2I)(ref key)).Y == compareStart;
			});
			if (keyValuePair.Equals(null))
			{
				int num5 = compareStart + 1;
				compareStart = num5;
				continue;
			}
			text = keyValuePair.Value;
			int compareEnd = compareStart + 1;
			KeyValuePair<Vector2I, string> keyValuePair2 = reverseLookupTable.FirstOrDefault(delegate(KeyValuePair<Vector2I, string> e)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				Vector2I key = e.Key;
				return ((Vector2I)(ref key)).Y == compareEnd;
			});
			if (!keyValuePair2.Equals(null))
			{
				text2 = keyValuePair2.Value;
			}
		}
		GameObject val3 = __instance.skillsScreen.skillWidgets[text];
		GameObject val4 = __instance.skillsScreen.skillWidgets[text2];
		Vector3 position = val3.transform.position;
		Vector3 position2 = val4.transform.position;
		float num6 = Mathf.Abs(position.y - position2.y);
		if (num6 != Y_Step)
		{
			Debug.Log((object)("Adjusting dynamic Y_Step of skill screen: " + Y_Step + "+ -> " + num6));
			Y_Step = num6;
		}
	}

	private static bool IsAdvancedConnection(Skill src, string dst_id)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!lookupTable.TryGetValue(((Resource)src).Id, out var value) || !lookupTable.TryGetValue(dst_id, out var value2))
		{
			return false;
		}
		if (Math.Abs(value.y - value2.y) <= 1)
		{
			return false;
		}
		return true;
	}

	private static void CreateSkillConnection(SkillWidget __instance, Skill currentSkill, Tuple<string, Vector2> requisite, float totalConnections = 1f, float connectionPointNr = 0f, bool invertXStepSrc = false, float xOffset = 0f)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		gradient = 0;
		RefreshSkillScreenMatrix(__instance);
		string first = requisite.first;
		GameObject val = __instance.skillsScreen.skillWidgets[first];
		Vector2 second = requisite.second;
		Vector3 position = TransformExtensions.GetPosition((Transform)(object)__instance.lines_left);
		if (debug)
		{
			SgtLogger.l("TotalDistance; " + ((object)Vector2.op_Implicit(position)/*cast due to .constrained prefix*/).ToString() + " -> " + ((object)Unsafe.As<Vector2, Vector2>(ref requisite.second)/*cast due to .constrained prefix*/).ToString());
		}
		float num = position.x - second.x;
		Vector2 zero = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		float num2 = second.y - position.y;
		float num3 = Mathf.Min(totalConnections, 3f);
		float num4 = Mathf.Clamp(num2 / Y_Step, 0f - num3, num3);
		num4 = (float)Math.Round(num4 * 4f, MidpointRounding.ToEven) / 4f;
		float num5 = 10f;
		float num6 = Y_Step / 3f;
		float num7 = Y_Step / 3f;
		float num8 = Mathf.Clamp((0f - num4) * num5, 0f - num6, num6);
		float num9 = Mathf.Clamp(num4 * num5, 0f - num7, num7);
		float num10 = Mathf.Clamp(connectionPointNr * num5, 0f - num7, num7);
		float num11 = Mathf.Abs(xOffset);
		num11 -= 1f;
		if (invertXStepSrc)
		{
			num11 *= -1f;
		}
		float num12 = num11 * num5;
		((Vector2)(ref zero2))._002Ector(0f, num10);
		((Vector2)(ref zero))._002Ector(0f, num8);
		float num13 = num / 2f;
		if (debug)
		{
			SgtLogger.l(" - verticalStepDiff: " + num4 + " for " + ((Resource)currentSkill).Id + " to " + requisite.first + ", number: " + connectionPointNr + "; Midpoint: " + num13 + ", xdiff: " + num12 + ",Total Cons: " + totalConnections + ", relativeYDiffTarget " + num9 + " , halfTechHeightTarget: " + num7 + ", relativeYDiffSource: " + num8);
		}
		float num14 = num13 + num12;
		Vector2 src = new Vector2(0f, 0f) + zero2;
		Vector2 p = new Vector2(0f - num14, 0f) + zero2;
		Vector2 p2 = new Vector2(0f - num14, num2) + zero;
		Vector2 dst = new Vector2(0f - num, num2) + zero;
		if (IsAdvancedConnection(currentSkill, requisite.first))
		{
			CreateNotSoSimpleConnection(__instance, currentSkill, first, src, p, p2, dst);
		}
		else
		{
			CreateSimpleConnection(__instance, src, p, p2, dst);
		}
	}

	private unsafe static void CreateNotSoSimpleConnection(SkillWidget instance, Skill srcSkill, string dst_id, Vector2 src, Vector2 p1, Vector2 p2, Vector2 dst)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		Skills skills = Db.Get().Skills;
		if (debug)
		{
			PrintMatrix();
		}
		if (!lookupTable.TryGetValue(((Resource)srcSkill).Id, out var value) || !lookupTable.TryGetValue(dst_id, out var value2))
		{
			CreateSimpleConnection(instance, src, p1, p2, dst);
			return;
		}
		float y_Step = Y_Step;
		float num = 6f;
		UILineRenderer val = CreateLineRenderer(((Component)instance.lines_left).transform);
		val.Points = (Vector2[])(object)new Vector2[2] { src, p1 };
		if (debug)
		{
			((Graphic)val).color = UIUtils.GetRainbowColorForIndex(gradient++);
		}
		List<UILineRenderer> list = new List<UILineRenderer>(1) { val };
		Vector2 val2 = p1;
		Vector2 val3 = p1;
		bool flag = value.y > value2.y;
		val3.y = 0f;
		if (debug)
		{
			SgtLogger.l("StartPoint: " + ((object)(*(Vector2I*)(&value))/*cast due to .constrained prefix*/).ToString() + ", DestinationPoint: " + ((object)(*(Vector2I*)(&value2))/*cast due to .constrained prefix*/).ToString());
		}
		int num2 = ((!flag) ? 1 : (-1));
		if (debug)
		{
			SgtLogger.l("segmentEnd: " + ((object)(*(Vector2*)(&val3))/*cast due to .constrained prefix*/).ToString());
		}
		Vector2I val4 = default(Vector2I);
		Vector2I val5 = default(Vector2I);
		for (int i = value.y + num2; flag ? (i > value2.y) : (i < value2.y); i += num2)
		{
			((Vector2I)(ref val4))._002Ector(value.x, i);
			((Vector2I)(ref val5))._002Ector(value2.x, i);
			if (debug)
			{
				Vector2I val6 = val4;
				string? text = ((object)(*(Vector2I*)(&val6))/*cast due to .constrained prefix*/).ToString();
				val6 = val5;
				SgtLogger.l("Checking connection between " + text + " and " + ((object)(*(Vector2I*)(&val6))/*cast due to .constrained prefix*/).ToString());
			}
			val3.y += y_Step * (float)num2;
			if (debug)
			{
				SgtLogger.l("segmentEnd: " + ((object)(*(Vector2*)(&val3))/*cast due to .constrained prefix*/).ToString());
			}
			if (!reverseLookupTable.TryGetValue(val4, out var value3) || !reverseLookupTable.TryGetValue(val5, out var value4))
			{
				if (debug)
				{
					SgtLogger.l("no skil found on one of the sides");
				}
				continue;
			}
			Skill val7 = ((ResourceSet<Skill>)(object)skills).Get(value3);
			bool flag2 = val7.priorSkills.Contains(value4);
			if (debug)
			{
				SgtLogger.l(value3 + " has connection to " + value4 + ": " + flag2);
			}
			if (flag2)
			{
				val3.y -= num * (float)num2;
				UILineRenderer val8 = CreateLineRenderer(((Component)instance.lines_left).transform);
				if (debug)
				{
					((Graphic)val8).color = UIUtils.GetRainbowColorForIndex(gradient++);
				}
				val8.Points = (Vector2[])(object)new Vector2[2] { val2, val3 };
				if (debug)
				{
					SgtLogger.l("adding segment between " + ((object)(*(Vector2*)(&val2))/*cast due to .constrained prefix*/).ToString() + " and " + ((object)(*(Vector2*)(&val3))/*cast due to .constrained prefix*/).ToString());
				}
				list.Add(val8);
				val3.y += num * (float)num2;
				val2 = val3;
				val2.y += num * (float)num2;
			}
		}
		UILineRenderer val9 = CreateLineRenderer(((Component)instance.lines_left).transform);
		val9.Points = (Vector2[])(object)new Vector2[3] { val2, p2, dst };
		if (debug)
		{
			((Graphic)val9).color = UIUtils.GetRainbowColorForIndex(gradient++);
		}
		list.Add(val9);
		instance.lines = CollectionExtensions.AddRangeToArray<UILineRenderer>(instance.lines, list.ToArray());
	}

	private static void CreateSimpleConnection(SkillWidget instance, Vector2 src, Vector2 p1, Vector2 p2, Vector2 dst)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		UILineRenderer val = CreateLineRenderer(((Component)instance.lines_left).transform);
		val.Points = (Vector2[])(object)new Vector2[4] { src, p1, p2, dst };
		instance.lines = Util.Append<UILineRenderer>(instance.lines, val);
	}

	private static UILineRenderer CreateLineRenderer(Transform parent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("Line");
		val.AddComponent<RectTransform>();
		val.transform.SetParent(parent);
		TransformExtensions.SetLocalPosition(val.transform, Vector3.zero);
		Util.rectTransform(val).sizeDelta = Vector2.zero;
		UILineRenderer val2 = val.AddComponent<UILineRenderer>();
		((Graphic)val2).color = new Color(0.6509804f, 0.6509804f, 0.6509804f, 1f);
		return val2;
	}
}
