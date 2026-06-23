using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Database;
using HarmonyLib;
using PeterHan.PLib.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace UtilLibs.SharedTweaks;

public sealed class ResearchScreenBetterConnectionLines : PForwardedComponent
{
	private static readonly string crossIcon_base64 = "iVBORw0KGgoAAAANSUhEUgAAAAYAAAA2CAYAAADktujlAAABb2lDQ1BpY2MAACiRdZG/S8NAFMe/bZWKrXTQQcQhQxWHFkRFHKWCXapDW8GqS3JNWiFJwyVFiqvg4lBwEF38Nfgf6Cq4KgiCIoi4uvprkRLfNYUUae94eR++ue/j3TsgmNGZYffMA4bp8Gw6Ja0W1qTwOyIYAChmZGZbS7nFPLqun0cERH5Iilrdz3VckaJqMyDQRzzLLO4QUzfIbDmW4D3iIVaWi8QnxAlODRLfCl3x+E1wyeMvwTyfXQCCoqZUamOljVmZG8QTxHFDr7JWP+ImUdVcyVEeoRiFjSzSSEGCgio2ocNBkrJJM+vsm2z6llEhD6OvhRo4OUookzdBapWqqpQ10lXaOmpi7v/naWvTU171aArofXXdzzEgvA806q77e+q6jTMg9AJcm76/QnOa+ya97mvxYyC2A1ze+JpyAFztAsPPlszlphSiCGoa8HFBj18ABu+B/nVvVq3/OH8C8tv0RHfA4REwTudjG38UdWgTWCvYsgAAAAlwSFlzAAAuIwAALiMBeKU/dgAAAY1JREFUOE/NlN1KAzEQhXcm2S4WoaCgL+CVD+Cl1z6ET+4b+AMKtbaW3YnnzCZLurTglRgIbebLnMxMJiv3D4/aHBlHjdz3B0DiLCApa4Jpkf9zPXkUyCgnUMsROKSxluPuCSyqcw48uhxJMQZ6UWaZ3QlopM3BKkvR2OYZCC6yRwGUdnBVAQbCGQmuZ4AeB6Doj1IpJUglbVKiN6AsUBSNyYbLlEyTmYcqEgJAiPv9bgWj2tB7HqIhCEbcfq2XZibW9wrZoBiwU8rOISXjTJgsIjx2200LA85OfieUqYqay/hffuTm9u7pWLixbReDMTEbmFyDvJkg6t4utoasrRdmXkoise3ONl5ERa0agjDWquuW67HsA8uOynrZNULrPTWmjcWxGUQxAUT1VbDiebl1Or8oLJ4RRQHskKl9ALyRKVWAd8nLqYZ7mwG2kZ/xkQHlyntx8JmvsjyDqdt3+UbLa2IQ/tS+q8s/eFF91QO/AzbrGtzKeAb/lMFN3njzbwk3ucepD1mtMjvp1PIHpY18YaNGfVIAAAAASUVORK5CYII=";

	private static Sprite crossIcon;

	private const float Y_Step = 250f;

	private static Dictionary<Tech, int> techConnectionPoints = null;

	private static bool init = false;

	private static Dictionary<string, Vector2I> lookupTable = new Dictionary<string, Vector2I>();

	private static Dictionary<Vector2I, string> reverseLookupTable = new Dictionary<Vector2I, string>();

	private static int gradient = 0;

	private static GameObject IconPrefab = null;

	public override Version Version => new Version(1, 0, 1, 0);

	private static void LoadIcon()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		byte[] array = Convert.FromBase64String(crossIcon_base64);
		Texture2D val = new Texture2D(6, 54);
		ImageConversion.LoadImage(val, array);
		((Texture)val).filterMode = (FilterMode)1;
		crossIcon = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f), 100f);
	}

	public static void Register()
	{
		new ResearchScreenBetterConnectionLines().RegisterForForwarding();
	}

	public override void Initialize(Harmony plibInstance)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		try
		{
			LoadIcon();
			MethodInfo methodInfo = AccessTools.Method(typeof(ResearchEntry), "SetupLines", (Type[])null, (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(ResearchScreenBetterConnectionLines), "CreateLinesPostfix", (Type[])null, (Type[])null);
			plibInstance.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(methodInfo2, 500, (string[])null, (string[])null, (bool?)null), (HarmonyMethod)null, (HarmonyMethod)null);
			Debug.Log((object)(Assembly.GetExecutingAssembly().GetName().Name + ": " + GetType().ToString() + " successfully patched"));
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)(Assembly.GetExecutingAssembly().GetName().Name + ": " + GetType().ToString() + " patch failed!"));
			Debug.LogWarning((object)ex.Message);
		}
	}

	public static void CreateLinesPostfix(ResearchEntry __instance)
	{
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		RefreshResearchMatrix(__instance);
		if (techConnectionPoints == null)
		{
			techConnectionPoints = new Dictionary<Tech, int>();
			foreach (Tech resource in ((ResourceSet<Tech>)(object)Db.Get().Techs).resources)
			{
				foreach (Tech item in resource.requiredTech)
				{
					if (!techConnectionPoints.ContainsKey(item))
					{
						techConnectionPoints.Add(item, 0);
					}
					techConnectionPoints[item]++;
				}
			}
		}
		foreach (KeyValuePair<Tech, UILineRenderer> item2 in __instance.techLineMap)
		{
			Object.Destroy((Object)(object)((Component)item2.Value).gameObject);
		}
		__instance.techLineMap.Clear();
		List<Tech> list = new List<Tech>();
		List<Tech> list2 = new List<Tech>();
		List<Tech> list3 = new List<Tech>();
		Tech targetTech = __instance.targetTech;
		foreach (Tech item3 in targetTech.requiredTech)
		{
			float num = item3.center.y - targetTech.center.y;
			if (num < -1f)
			{
				list.Add(item3);
			}
			else if (num > 1f)
			{
				list2.Add(item3);
			}
			else
			{
				list3.Add(item3);
			}
		}
		list2.Sort((Tech a, Tech b) => a.center.y.CompareTo(b.center.y));
		list.Sort((Tech a, Tech b) => -a.center.y.CompareTo(b.center.y));
		foreach (Tech item4 in list3)
		{
			CreateTechConnection(__instance, targetTech, item4);
		}
		float num2 = (list3.Any() ? 1f : ((list.Any() && list2.Any()) ? 0.625f : 0f));
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			CreateTechConnection(__instance, targetTech, list[num3], 0f - num2 - (float)num3);
		}
		for (int num4 = 0; num4 < list2.Count; num4++)
		{
			CreateTechConnection(__instance, targetTech, list2[num4], num2 + (float)num4);
		}
		__instance.QueueStateChanged(false);
		if (__instance.targetTech == null)
		{
			return;
		}
		foreach (TechInstance item5 in Research.Instance.GetResearchQueue())
		{
			if (item5.tech == __instance.targetTech)
			{
				__instance.QueueStateChanged(true);
			}
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

	private unsafe static void RefreshResearchMatrix(ResearchEntry __instance)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		if (init)
		{
			return;
		}
		init = true;
		lookupTable.Clear();
		reverseLookupTable.Clear();
		List<KeyValuePair<Tech, ResearchEntry>> list = (from widget in __instance.researchScreen.entryMap
			orderby ((KMonoBehaviour)widget.Value).transform.position.y, ((KMonoBehaviour)widget.Value).transform.position.x
			select widget).ToList();
		float num = float.MinValue;
		int num2 = -1;
		Vector2I val = default(Vector2I);
		foreach (KeyValuePair<Tech, ResearchEntry> item in list)
		{
			ResearchEntry value = item.Value;
			Tech key = item.Key;
			float x = ((KMonoBehaviour)value).transform.position.x;
			float y = ((KMonoBehaviour)value).transform.position.y;
			if (y > num)
			{
				num = y;
				num2++;
			}
			int num3 = num2;
			int tier = key.tier;
			((Vector2I)(ref val))._002Ector(tier, num3);
			if (!lookupTable.ContainsKey(((Resource)key).Id))
			{
				lookupTable.Add(((Resource)key).Id, val);
			}
			else
			{
				SgtLogger.warning("ResearchMatrix duplicate key detected for tech " + ((Resource)key).Id + " and value " + ((object)(*(Vector2I*)(&val))/*cast due to .constrained prefix*/).ToString());
			}
			if (!reverseLookupTable.ContainsKey(val))
			{
				reverseLookupTable.Add(val, ((Resource)key).Id);
			}
			else
			{
				SgtLogger.warning("ResearchMatrix duplicate reverse key detected for value " + ((Resource)key).Id + " and key " + ((object)(*(Vector2I*)(&val))/*cast due to .constrained prefix*/).ToString());
			}
		}
	}

	private static void CreateTechConnection(ResearchEntry __instance, Tech currentTech, Tech requisite, float connectionPointNr = 0f)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		float num = currentTech.width / 2f + 25f;
		float num2 = currentTech.center.x - num - (requisite.center.x + num) + 2f;
		ResearchEntry val = __instance.researchScreen.entryMap[requisite];
		Vector2 zero = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		float num3 = requisite.center.y - currentTech.center.y;
		float num4 = Mathf.Max(0, techConnectionPoints[requisite]);
		float num5 = Mathf.Min(num4, 3f);
		float num6 = Mathf.Clamp(num3 / 250f, 0f - num5, num5);
		num6 = (float)Math.Round(num6 * 4f, MidpointRounding.ToEven) / 4f;
		if (Mathf.Abs(num6) == 1.25f)
		{
			num6 /= 1.25f;
		}
		float num7 = 12f;
		float num8 = currentTech.height / 2f;
		float num9 = Mathf.Clamp(num6 * num7, 0f - num8, num8);
		float num10 = Mathf.Clamp(connectionPointNr * num7, 0f - num8, num8);
		float num11 = requisite.height / 2f;
		float num12 = Mathf.Clamp((0f - num6) * num7, 0f - num11, num11);
		((Vector2)(ref zero2))._002Ector(0f, num10);
		((Vector2)(ref zero))._002Ector(0f, num12);
		float num13 = Mathf.CeilToInt(Mathf.Max(0f, num4 - 3f) / 2f);
		float num14 = 32f - num7 - num13 * num7;
		if (num14 < 0f)
		{
			num14 = 0f;
		}
		float num15 = num14 + Mathf.Abs(num9);
		float num16 = num14 + Mathf.Abs(num12);
		Vector2 val2 = new Vector2(0f, 0f) + zero2;
		Vector2 val3 = new Vector2(0f - num15, 0f) + zero2;
		Vector2 val4 = new Vector2(0f - num16, num3) + zero;
		Vector2 val5 = new Vector2(0f - num2, num3) + zero;
		UILineRenderer component = Util.KInstantiateUI(__instance.linePrefab, ((Component)__instance.lineContainer).gameObject, true).GetComponent<UILineRenderer>();
		component.Points = (Vector2[])(object)new Vector2[4] { val2, val3, val4, val5 };
		component.LineThickness = __instance.lineThickness_inactive;
		((Graphic)component).color = __instance.inactiveLineColor;
		__instance.techLineMap.Add(requisite, component);
		if (!lookupTable.TryGetValue(((Resource)currentTech).Id, out var value) || !lookupTable.TryGetValue(((Resource)requisite).Id, out var value2))
		{
			return;
		}
		int num17 = value.y - value2.y;
		if (Math.Abs(num17) <= 1)
		{
			return;
		}
		bool flag = num17 > 0;
		Techs techs = Db.Get().Techs;
		int num18 = ((!flag) ? 1 : (-1));
		Vector2I key = default(Vector2I);
		Vector2I key2 = default(Vector2I);
		Vector2I key3 = default(Vector2I);
		Vector2I key4 = default(Vector2I);
		Vector2 pos = default(Vector2);
		for (int i = value.y + num18; flag ? (i > value2.y) : (i < value2.y); i += num18)
		{
			((Vector2I)(ref key))._002Ector(value.x, i);
			((Vector2I)(ref key2))._002Ector(value2.x, i);
			((Vector2I)(ref key3))._002Ector(value.x + 1, i);
			((Vector2I)(ref key4))._002Ector(value2.x - 1, i);
			if ((reverseLookupTable.TryGetValue(key, out var value3) || reverseLookupTable.TryGetValue(key3, out value3)) && (reverseLookupTable.TryGetValue(key2, out var destTech) || reverseLookupTable.TryGetValue(key4, out destTech)))
			{
				Tech val6 = ((ResourceSet<Tech>)(object)techs).Get(value3);
				if (val6.requiredTech.Any((Tech t) => ((Resource)t).Id == destTech))
				{
					SgtLogger.l("crossing detected for " + ((Resource)currentTech).Id + " at y level: " + i + ", with connection between " + value3 + " and " + destTech);
					Vector2 val7 = Util.rectTransform((Component)(object)__instance.researchScreen.entryMap[val6]).anchoredPosition - Util.rectTransform((Component)(object)__instance).anchoredPosition;
					((Vector2)(ref pos))._002Ector(0f - num15, val7.y + 1.25f);
					CreateCrossRendererRelative(__instance, pos);
				}
			}
		}
	}

	private static void CreateCrossRenderer(ResearchEntry __instance, Vector2 pos, Color? c = null)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)IconPrefab == (Object)null)
		{
			IconPrefab = Util.KInstantiateUI(((Component)__instance.iconPrefab.transform.Find("Icon_FG")).gameObject, (GameObject)null, false);
			IconPrefab.GetComponent<Image>().sprite = Assets.GetSprite(HashedString.op_Implicit("unknown"));
		}
		GameObject val = Util.KInstantiateUI(IconPrefab, ((Component)__instance.lineContainer).gameObject, true);
		((Object)val).name = ((Resource)__instance.targetTech).Id + "_LinePreventionMeasure";
		TransformExtensions.SetPosition(val.transform, Vector2.op_Implicit(pos));
		Vector3 localScale = val.transform.localScale;
		val.transform.localScale = localScale;
		if (c.HasValue)
		{
			((Graphic)val.GetComponent<Image>()).color = c.Value;
		}
		SgtLogger.l("setting icon pos to " + pos.x + "," + pos.y + " with color: " + c.ToString());
	}

	private static void CreateCrossRendererRelative(ResearchEntry __instance, Vector2 pos, Color? c = null)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)IconPrefab == (Object)null)
		{
			IconPrefab = Util.KInstantiateUI(((Component)__instance.iconPrefab.transform.Find("Icon_FG")).gameObject, (GameObject)null, false);
			IconPrefab.GetComponent<Image>().sprite = crossIcon;
		}
		GameObject val = Util.KInstantiateUI(IconPrefab, ((Component)__instance.lineContainer).gameObject, true);
		((Object)val).name = ((Resource)__instance.targetTech).Id + "_LinePreventionMeasure";
		val.transform.localPosition = Vector2.op_Implicit(pos);
		Vector3 localScale = val.transform.localScale;
		val.transform.localScale = localScale;
		if (c.HasValue)
		{
			((Graphic)val.GetComponent<Image>()).color = c.Value;
		}
		SgtLogger.l("setting icon pos to " + pos.x + "," + pos.y + " with color: " + c.ToString());
	}
}
