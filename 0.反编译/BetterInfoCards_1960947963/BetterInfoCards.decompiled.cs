using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using AzeLib;
using AzeLib.Attributes;
using AzeLib.Extensions;
using BetterInfoCards.Export;
using BetterInfoCards.Util;
using Database;
using HarmonyLib;
using KMod;
using Klei;
using Klei.AI;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using PeterHan.PLib.Core;
using PeterHan.PLib.Detours;
using PeterHan.PLib.Options;
using PeterHan.PLib.PatchManager;
using PeterHan.PLib.UI;
using PeterHan.PLib.UI.Layouts;
using STRINGS;
using TMPro;
using TUNING;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.TextCore;
using UnityEngine.UI;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("BetterInfoCards")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyDescription("Overhauls info cards to be more powerful and user friendly.")]
[assembly: AssemblyFileVersion("2026.4.25.1")]
[assembly: AssemblyInformationalVersion("2026.4.25.1+28ad98fc744223291aec42ffca96868a8fdfda96")]
[assembly: AssemblyProduct("BetterInfoCards")]
[assembly: AssemblyTitle("BetterInfoCards")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("2026.4.25.1")]
[module: UnverifiableCode]
[module: RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace BetterInfoCards
{
	public static class ConverterManager
	{
		public const string title = "Title";

		public const string germs = "Germs";

		public const string temp = "Temp";

		public const string sumSuffix = " <color=#ababab>(Σ)</color>";

		public const string avgSuffix = " <color=#ababab>(μ)</color>";

		private static readonly Dictionary<string, Func<string, string, object, TextInfo>> converters;

		static ConverterManager()
		{
			converters = new Dictionary<string, Func<string, string, object, TextInfo>>();
			AddConverter(string.Empty, (object data) => (object)null);
			AddConverter("Title", delegate(object data)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				GameObject val = (GameObject)((data is GameObject) ? data : null);
				KPrefabID component = val.GetComponent<KPrefabID>();
				return ((Object)(object)component != (Object)null && Assets.IsTagCountable(component.PrefabTag)) ? val.GetComponent<PrimaryElement>().Units : 1f;
			}, (string original, List<float> counts) => original.RemoveCountSuffix() + " x " + counts.Sum());
			ConverterManager.AddConverter<(byte, int)>("Germs", (Func<object, (byte, int)>)delegate(object data)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0054: Unknown result type (might be due to invalid IL or missing references)
				//IL_0060: Expected O, but got Unknown
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				//IL_006b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0082: Unknown result type (might be due to invalid IL or missing references)
				//IL_0087: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					PrimaryElement component = ((GameObject)data).GetComponent<PrimaryElement>();
					return (idx: component.DiseaseIdx, count: component.DiseaseCount);
				}
				catch (NullReferenceException)
				{
					Debug.Log((object)"Issue encountered in germs converter (getValue)");
					Debug.Log((object)("Data: " + data));
					GameObject val = (GameObject)data;
					string? obj = (((int)val != 0) ? ((object)val).ToString() : null);
					GameObject val2 = (GameObject)data;
					Debug.Log((object)("GameObject: " + obj + "; " + (((int)val2 != 0) ? ((Object)val2).name : null)));
					GameObject val3 = (GameObject)data;
					PrimaryElement val4 = (((int)val3 != 0) ? val3.GetComponent<PrimaryElement>() : null);
					Debug.Log((object)("Element: " + (object)val4));
					Debug.Log((object)("Idx: " + ((val4 != null) ? new byte?(val4.DiseaseIdx) : ((byte?)null)) + "; Count: " + ((val4 != null) ? new int?(val4.DiseaseCount) : ((int?)null))));
					Debug.LogError((object)"Hi, you've hit an edge case crash in Better Info Cards.\nPLEASE upload the full player.log to the below issue so I can pin it down.\nhttps://github.com/AzeTheGreat/ONI-Mods/issues/33\n--------------------------------------------------");
					throw;
				}
			}, (Func<string, List<(byte, int)>, string>)delegate(string original, List<(byte idx, int count)> pairs)
			{
				string result = LocString.op_Implicit(DISEASE.NO_DISEASE);
				if (pairs[0].idx != byte.MaxValue)
				{
					result = GameUtil.GetFormattedDisease(pairs[0].idx, pairs.Sum(((byte idx, int count) x) => x.count), true) + " <color=#ababab>(Σ)</color>";
				}
				return result;
			}, new List<(Func<(byte, int), float>, float)>(1) { (((byte idx, int count) dP) => (int)dP.idx, 1f) });
			AddConverter("Temp", (object data) => ((GameObject)data).GetComponent<PrimaryElement>().Temperature, (string original, List<float> temps) => GameUtil.GetFormattedTemperature(temps.Average(), (TimeSlice)0, (TemperatureInterpretation)0, true, false) + " <color=#ababab>(μ)</color>", new List<(Func<float, float>, float)>(1) { ((float x) => x, BaseOptions<Options>.Opts.TemperatureBandWidth) });
		}

		public static void AddConverter<T>(string name, Func<object, T> getValue, Func<string, List<T>, string> getTextOverride = null, List<(Func<T, float>, float)> splitListDefs = null) where T : new()
		{
			if (converters.ContainsKey(name))
			{
				throw new Exception("Attempted to add converter with name: " + name + ", but converter with name is already present.");
			}
			ResetPool<TextInfo<T>> pool = new ResetPool<TextInfo<T>>(ref InterceptHoverDrawer.BeginDrawing.onBeginDrawing);
			converters.Add(name, (string k, string n, object d) => pool.Get().Set(k, n, d, getValue, getTextOverride, splitListDefs));
		}

		private static void AddConverterReflect(string name, object getValue, object getTextOverride, object splitListDefs)
		{
			Type type = getValue.GetType().GetGenericArguments()[1];
			typeof(ConverterManager).GetMethod("AddConverter").MakeGenericMethod(type).Invoke(null, new object[4] { name, getValue, getTextOverride, splitListDefs });
		}

		public static bool TryGetConverter(string id, out Func<string, string, object, TextInfo> converter)
		{
			if (id != string.Empty && converters.TryGetValue(id, out converter))
			{
				return true;
			}
			converter = converters[string.Empty];
			return false;
		}
	}
	public class ExportSelectToolData
	{
		public class GetSelectInfo_Patch
		{
			public static IEnumerable<CodeInstruction> ChildTranspiler(IEnumerable<CodeInstruction> instructions)
			{
				MethodInfo titleTarget = AccessTools.Method(typeof(GameUtil), "GetUnitFormattedName", new Type[2]
				{
					typeof(GameObject),
					typeof(bool)
				}, (Type[])null);
				MethodInfo germTarget = AccessTools.Method(typeof(string), "Format", new Type[3]
				{
					typeof(string),
					typeof(object),
					typeof(object)
				}, (Type[])null);
				MethodInfo tempTarget = AccessTools.Method(typeof(GameUtil), "GetFormattedTemperature", (Type[])null, (Type[])null);
				MethodInfo statusTarget = AccessTools.Method(typeof(Entry), "GetName", (Type[])null, (Type[])null);
				MethodInfo targetGetCompPrimaryElement = AccessTools.Method(typeof(Component), "GetComponent", (Type[])null, (Type[])null).MakeGenericMethod(typeof(PrimaryElement));
				MethodInfo targetDrawText = AccessTools.Method(typeof(HoverTextDrawer), "DrawText", new Type[2]
				{
					typeof(string),
					typeof(TextStyleSetting)
				}, (Type[])null);
				AccessTools.Method(typeof(HoverTextDrawer), "EndShadowBar", (Type[])null, (Type[])null);
				MethodInfo targetElement = AccessTools.Method("HoverTextHelper:MassStringsReadOnly", (Type[])null, (Type[])null) ?? AccessTools.Method("WorldInspector:MassStringsReadOnly", (Type[])null, (Type[])null);
				LocalBuilder titleLocal = null;
				LocalBuilder germLocal = null;
				bool isFirst = true;
				bool afterTarget = false;
				foreach (CodeInstruction i in instructions)
				{
					if (isFirst && CodeInstructionExtensions.Is(i, OpCodes.Callvirt, (MemberInfo)targetGetCompPrimaryElement))
					{
						isFirst = false;
						afterTarget = true;
						object operand = instructions.FindPrior(i, (CodeInstruction x) => x.IsLocalOfType(typeof(KSelectable))).operand;
						yield return new CodeInstruction(OpCodes.Ldloc_S, operand);
						yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(GetSelectInfo_Patch), "ExportSelectable", (Type[])null, (Type[])null));
					}
					else if (afterTarget)
					{
						if (CodeInstructionExtensions.OperandIs(i, (MemberInfo)titleTarget))
						{
							titleLocal = instructions.FindNext(i, (CodeInstruction x) => x.OpCodeIs(OpCodes.Stloc_S)).operand as LocalBuilder;
						}
						else if (CodeInstructionExtensions.OperandIs(i, (MemberInfo)germTarget))
						{
							germLocal = instructions.FindNext(i, (CodeInstruction x) => x.OpCodeIs(OpCodes.Stloc_S)).operand as LocalBuilder;
						}
						else if (CodeInstructionExtensions.Is(i, OpCodes.Callvirt, (MemberInfo)targetDrawText))
						{
							CodeInstruction val = instructions.FindPrior(i, (CodeInstruction x) => DoesPushString(x));
							if (CodeInstructionExtensions.OperandIs(val, (object)titleLocal))
							{
								yield return new CodeInstruction(OpCodes.Ldstr, (object)"Title");
								yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(GetSelectInfo_Patch), "ExportGO", (Type[])null, (Type[])null));
							}
							else if (CodeInstructionExtensions.OperandIs(val, (object)germLocal))
							{
								yield return new CodeInstruction(OpCodes.Ldstr, (object)"Germs");
								yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(GetSelectInfo_Patch), "ExportGO", (Type[])null, (Type[])null));
							}
							else if (CodeInstructionExtensions.OperandIs(val, (MemberInfo)statusTarget))
							{
								object operand2 = instructions.FindPrior(i, (CodeInstruction x) => x.IsLocalOfType(typeof(Entry))).operand;
								yield return new CodeInstruction(OpCodes.Ldloc_S, operand2);
								yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(GetSelectInfo_Patch), "ExportStatus", (Type[])null, (Type[])null));
							}
							else if (CodeInstructionExtensions.OperandIs(val, (MemberInfo)tempTarget))
							{
								yield return new CodeInstruction(OpCodes.Ldstr, (object)"Temp");
								yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(GetSelectInfo_Patch), "ExportGO", (Type[])null, (Type[])null));
							}
						}
						else if (CodeInstructionExtensions.Is(i, OpCodes.Call, (MemberInfo)targetElement))
						{
							yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
							yield return new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(GetSelectInfo_Patch), "ExportSelectableFromList", (Type[])null, (Type[])null));
						}
					}
					yield return i;
				}
			}

			private static bool DoesPushString(CodeInstruction i)
			{
				StackBehaviour stackBehaviourPush = i.opcode.StackBehaviourPush;
				object operand = i.operand;
				if ((stackBehaviourPush == StackBehaviour.Varpush || stackBehaviourPush == StackBehaviour.Push1) && ((operand as MethodInfo)?.ReturnType == typeof(string) || (operand as LocalBuilder)?.LocalType == typeof(string)))
				{
					return true;
				}
				return false;
			}

			private static void ExportSelectableFromList(List<KSelectable> selectables)
			{
				ExportSelectable(selectables.LastOrDefault());
			}

			private static void ExportSelectable(KSelectable selectable)
			{
				curSelectable = selectable;
			}

			private static void Export(string name, object data)
			{
				curTextInfo = (id: name, data: data);
			}

			private static void ExportGO(string name)
			{
				Export(name, ((Component)curSelectable).gameObject);
			}

			private static void ExportStatus(Entry entry)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				Export(((Resource)entry.item).Id, entry.data);
			}
		}

		private static KSelectable curSelectable;

		private static (string id, object data) curTextInfo = (id: string.Empty, data: null);

		public static KSelectable ConsumeSelectable()
		{
			KSelectable result = curSelectable;
			curSelectable = null;
			return result;
		}

		public static (string id, object data) ConsumeTextInfo()
		{
			(string id, object data) result = curTextInfo;
			curTextInfo = (id: string.Empty, data: null);
			return result;
		}
	}
	internal static class InterceptHoverDrawer
	{
		[HarmonyPatch(typeof(HoverTextDrawer), "BeginDrawing")]
		public class BeginDrawing
		{
			public static Action onBeginDrawing;

			private static void Postfix(HoverTextDrawer __instance)
			{
				drawerInstance = __instance;
				IsInterceptMode = true;
				onBeginDrawing?.Invoke();
			}
		}

		[HarmonyPatch(typeof(HoverTextDrawer), "BeginShadowBar")]
		private class BeginShadowBar
		{
			private static ResetPool<InfoCard> pool = new ResetPool<InfoCard>(ref BeginDrawing.onBeginDrawing);

			[HarmonyPriority(800)]
			private static bool Prefix(bool selected)
			{
				if (IsInterceptMode)
				{
					infoCards.Add(curInfoCard = pool.Get().Set(selected));
				}
				return !IsInterceptMode;
			}
		}

		[HarmonyPatch(typeof(HoverTextDrawer), "DrawIcon", new Type[]
		{
			typeof(Sprite),
			typeof(Color),
			typeof(int),
			typeof(int)
		})]
		private class DrawIcon
		{
			private static ResetPool<DrawActions.Icon> pool = new ResetPool<DrawActions.Icon>(ref BeginDrawing.onBeginDrawing);

			[HarmonyPriority(800)]
			private static bool Prefix(Sprite icon, Color color, int image_size, int horizontal_spacing)
			{
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				if (IsInterceptMode)
				{
					curInfoCard.AddDraw(pool.Get().Set(icon, color, image_size, horizontal_spacing));
				}
				return !IsInterceptMode;
			}
		}

		[HarmonyPatch(typeof(HoverTextDrawer), "DrawText", new Type[]
		{
			typeof(string),
			typeof(TextStyleSetting),
			typeof(Color),
			typeof(bool)
		})]
		private class DrawText
		{
			private static ResetPool<DrawActions.Text> pool = new ResetPool<DrawActions.Text>(ref BeginDrawing.onBeginDrawing);

			[HarmonyPriority(800)]
			private static bool Prefix(string text, TextStyleSetting style, Color color, bool override_color)
			{
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				if (IsInterceptMode && !Util.IsNullOrWhiteSpace(text))
				{
					(string id, object data) tuple = ExportSelectToolData.ConsumeTextInfo();
					string item = tuple.id;
					object item2 = tuple.data;
					TextInfo ti = TextInfo.Create(item, text, item2);
					curInfoCard.AddDraw(pool.Get().Set(ti, style, color, override_color), ti);
				}
				return !IsInterceptMode;
			}
		}

		[HarmonyPatch(typeof(HoverTextDrawer), "AddIndent")]
		private class AddIndent
		{
			private static ResetPool<DrawActions.AddIndent> pool = new ResetPool<DrawActions.AddIndent>(ref BeginDrawing.onBeginDrawing);

			[HarmonyPriority(800)]
			private static bool Prefix(int width)
			{
				if (IsInterceptMode)
				{
					curInfoCard.AddDraw(pool.Get().Set(width));
				}
				return !IsInterceptMode;
			}
		}

		[HarmonyPatch(typeof(HoverTextDrawer), "NewLine")]
		private class NewLine
		{
			private static ResetPool<DrawActions.NewLine> pool = new ResetPool<DrawActions.NewLine>(ref BeginDrawing.onBeginDrawing);

			[HarmonyPriority(800)]
			private static bool Prefix(int min_height)
			{
				if (IsInterceptMode)
				{
					curInfoCard.AddDraw(pool.Get().Set(min_height));
				}
				return !IsInterceptMode;
			}
		}

		[HarmonyPatch(typeof(HoverTextDrawer), "EndShadowBar")]
		private class EndShadowBar
		{
			[HarmonyPriority(800)]
			private static bool Prefix()
			{
				if (IsInterceptMode)
				{
					curInfoCard.selectable = ExportSelectToolData.ConsumeSelectable();
				}
				return !IsInterceptMode;
			}
		}

		public static HoverTextDrawer drawerInstance;

		private static InfoCard curInfoCard;

		private static List<InfoCard> infoCards = new List<InfoCard>();

		public static bool IsInterceptMode { get; set; }

		public static List<InfoCard> ConsumeInfoCards()
		{
			List<InfoCard> result = infoCards;
			infoCards = new List<InfoCard>();
			return result;
		}
	}
	public class Column
	{
		private const float isOverlappedThreshold = 10f;

		public float offsetX;

		public List<InfoCardWidgets> cards = new List<InfoCardWidgets>();

		public float maxXInCol;

		public float YMin => cards.Last().YMin;

		public void MoveAndResize(float colToRightYMin)
		{
			foreach (InfoCardWidgets card in cards)
			{
				card.Translate(offsetX);
				if (colToRightYMin < card.YMax - 10f)
				{
					card.SetWidth(maxXInCol);
				}
			}
		}
	}
	public class DisplayCard
	{
		private List<InfoCard> infoCards;

		private int visCardIndex;

		private InfoCard VisCard => infoCards[visCardIndex];

		public DisplayCard(List<InfoCard> infoCards)
		{
			this.infoCards = infoCards;
			visCardIndex = infoCards.FindIndex((InfoCard x) => x.isSelected);
			if (visCardIndex == -1)
			{
				visCardIndex = 0;
			}
		}

		public void Draw()
		{
			VisCard.Draw(infoCards, visCardIndex);
		}

		public List<KSelectable> GetAllSelectables()
		{
			return infoCards.Select((InfoCard x) => x.selectable).ToList();
		}
	}
	public abstract class DrawActions
	{
		public class Text : DrawActions
		{
			private TextInfo ti;

			private TextStyleSetting style;

			private Color color;

			private bool overrideColor;

			public Text Set(TextInfo ti, TextStyleSetting style, Color color, bool overrideColor)
			{
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				this.ti = ti;
				this.style = style;
				this.color = color;
				this.overrideColor = overrideColor;
				return this;
			}

			public override void Draw(List<InfoCard> cards)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				InterceptHoverDrawer.drawerInstance.DrawText(ti.GetTextOverride(cards), style, color, overrideColor);
			}
		}

		public class Icon : DrawActions
		{
			private Sprite icon;

			private Color color;

			private int imageSize;

			private int horizontalSpacing;

			public Icon Set(Sprite icon, Color color, int imageSize, int horizontalSpacing)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				this.icon = icon;
				this.color = color;
				this.imageSize = imageSize;
				this.horizontalSpacing = horizontalSpacing;
				return this;
			}

			public override void Draw(List<InfoCard> _)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				InterceptHoverDrawer.drawerInstance.DrawIcon(icon, color, imageSize, horizontalSpacing);
			}
		}

		public class AddIndent : DrawActions
		{
			private int width;

			public AddIndent Set(int width)
			{
				this.width = width;
				return this;
			}

			public override void Draw(List<InfoCard> _)
			{
				InterceptHoverDrawer.drawerInstance.AddIndent(width);
			}
		}

		public class NewLine : DrawActions
		{
			private int minHeight;

			public NewLine Set(int minHeight)
			{
				this.minHeight = minHeight;
				return this;
			}

			public override void Draw(List<InfoCard> _)
			{
				InterceptHoverDrawer.drawerInstance.NewLine(minHeight);
			}
		}

		public abstract void Draw(List<InfoCard> cards);
	}
	public class Grid
	{
		private const float shadowBarSpacing = 4f;

		private List<Column> columns = new List<Column>();

		private float _minY = float.MaxValue;

		private float MinY
		{
			get
			{
				//IL_001f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				if (_minY == float.MaxValue)
				{
					Canvas componentInParent = ((Component)HoverTextScreen.Instance).gameObject.GetComponentInParent<Canvas>();
					Rect pixelRect = componentInParent.pixelRect;
					_minY = (0f - ((Rect)(ref pixelRect)).height) / componentInParent.scaleFactor;
				}
				return _minY;
			}
		}

		public Grid(List<InfoCardWidgets> cards, float topY)
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			if (cards.Count == 0)
			{
				return;
			}
			Vector2 val = default(Vector2);
			((Vector2)(ref val))..ctor(0f, topY);
			columns.Clear();
			Column column = new Column();
			for (int i = 0; i < cards.Count; i++)
			{
				InfoCardWidgets infoCardWidgets = cards[i];
				if (val.y - infoCardWidgets.Height < MinY && i > 0)
				{
					val.x += column.maxXInCol + 4f;
					columns.Add(column);
					column = new Column
					{
						offsetX = val.x
					};
					val.y = topY;
				}
				infoCardWidgets.offset.y = val.y - infoCardWidgets.YMax;
				val.y -= infoCardWidgets.Height + 4f;
				if (infoCardWidgets.Width > column.maxXInCol)
				{
					column.maxXInCol = infoCardWidgets.Width;
				}
				column.cards.Add(infoCardWidgets);
			}
			columns.Add(column);
		}

		public void MoveAndResizeInfoCards()
		{
			for (int num = columns.Count - 1; num >= 0; num--)
			{
				float colToRightYMin = float.MaxValue;
				if (num != columns.Count - 1)
				{
					colToRightYMin = columns[num + 1].YMin;
				}
				columns[num].MoveAndResize(colToRightYMin);
			}
		}
	}
	public class InfoCard
	{
		public bool isSelected;

		public KSelectable selectable;

		public Dictionary<string, TextInfo> textInfos = new Dictionary<string, TextInfo>();

		private List<DrawActions> drawActions = new List<DrawActions>();

		private (int drawIndex, TextInfo ti) titleDrawer;

		public InfoCard Set(bool isSelected)
		{
			this.isSelected = isSelected;
			selectable = null;
			textInfos.Clear();
			drawActions.Clear();
			titleDrawer = (drawIndex: 0, ti: null);
			return this;
		}

		public void LogCard()
		{
			Debug.Log((object)("  " + GetTitleKey() + "; " + (object)selectable));
			foreach (KeyValuePair<string, TextInfo> textInfo in textInfos)
			{
				Debug.Log((object)("     " + textInfo.Key + "; " + textInfo.Value.ID + ", " + textInfo.Value.Text));
			}
		}

		public string GetTitleKey()
		{
			return titleDrawer.ti?.Text.RemoveCountSuffix() ?? string.Empty;
		}

		public void Draw(List<InfoCard> cards, int visCardIndex)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			if (visCardIndex > 0)
			{
				string empty = string.Empty;
				int num = ++visCardIndex;
				TextInfo ti = TextInfo.Create(empty, " #" + num, null);
				DrawActions.Text item = new DrawActions.Text().Set(ti, ((InterfaceTool)SelectTool.Instance).hoverTextConfiguration.Styles_Title.Standard, Color.white, overrideColor: false);
				drawActions.Insert(++titleDrawer.drawIndex, item);
			}
			InterceptHoverDrawer.drawerInstance.BeginShadowBar(isSelected);
			foreach (DrawActions drawAction in drawActions)
			{
				drawAction.Draw(cards);
			}
			InterceptHoverDrawer.drawerInstance.EndShadowBar();
		}

		public void AddDraw(DrawActions drawAction)
		{
			drawActions.Add(drawAction);
		}

		public void AddDraw(DrawActions drawAction, TextInfo ti)
		{
			if (titleDrawer.ti == null)
			{
				titleDrawer = (drawIndex: drawActions.Count, ti: ti);
			}
			textInfos[ti.ID] = ti;
			AddDraw(drawAction);
		}
	}
	public class InfoCardWidgets
	{
		public List<Entry<MonoBehaviour>> widgets = new List<Entry<MonoBehaviour>>();

		public Entry<MonoBehaviour> shadowBar;

		public Entry<MonoBehaviour> selectBorder;

		public Vector2 offset;

		public float YMax => shadowBar.rect.anchoredPosition.y;

		public float YMin => YMax - Height;

		public float Width
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				Rect rect = shadowBar.rect.rect;
				return ((Rect)(ref rect)).width;
			}
		}

		public float Height
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				Rect rect = shadowBar.rect.rect;
				return ((Rect)(ref rect)).height;
			}
		}

		public void AddWidget(Entry<MonoBehaviour> entry, GameObject prefab)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			Skin skin = HoverTextScreen.Instance.drawer.skin;
			if ((Object)(object)prefab == (Object)(object)((Component)skin.shadowBarWidget).gameObject)
			{
				shadowBar = entry;
			}
			else if ((Object)(object)prefab == (Object)(object)((Component)skin.selectBorderWidget).gameObject)
			{
				selectBorder = entry;
			}
			else
			{
				widgets.Add(entry);
			}
		}

		public void Translate(float x)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = default(Vector2);
			((Vector2)(ref val))..ctor(x, offset.y);
			RectTransform rect = shadowBar.rect;
			rect.anchoredPosition += val;
			if ((Object)(object)selectBorder.rect != (Object)null)
			{
				RectTransform rect2 = selectBorder.rect;
				rect2.anchoredPosition += val;
			}
			foreach (Entry<MonoBehaviour> widget in widgets)
			{
				RectTransform rect3 = widget.rect;
				rect3.anchoredPosition += val;
			}
		}

		public void SetWidth(float width)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			InterceptHoverDrawer.drawerInstance.shadowBars.Draw(shadowBar.rect.anchoredPosition + new Vector2(shadowBar.rect.sizeDelta.x, 0f)).rect.sizeDelta = new Vector2(width - shadowBar.rect.sizeDelta.x, shadowBar.rect.sizeDelta.y);
			if ((Object)(object)selectBorder.rect != (Object)null)
			{
				selectBorder.rect.sizeDelta = new Vector2(width + 2f, selectBorder.rect.sizeDelta.y);
			}
		}
	}
	public abstract class TextInfo
	{
		public string Text { get; protected set; }

		public string ID { get; protected set; }

		public static TextInfo Create(string id, string text, object data)
		{
			id = (ConverterManager.TryGetConverter(id, out var converter) ? id : text);
			return converter(id, text, data);
		}

		public abstract string GetTextOverride(List<InfoCard> cards);

		public abstract List<List<InfoCard>> SplitByTIDefs(List<InfoCard> cards);
	}
	public class TextInfo<T> : TextInfo
	{
		private bool _isResultCached;

		private T _result;

		private object data;

		private Func<object, T> getValue;

		private Func<string, List<T>, string> getTextOverride;

		private List<(Func<T, float>, float)> splitListDefs;

		private T Result
		{
			get
			{
				if (!_isResultCached)
				{
					T item = (_result = getValue(data));
					_isResultCached = true;
					return (item, true).Item1;
				}
				return _result;
			}
		}

		public TextInfo Set(string key, string text, object data, Func<object, T> getValue, Func<string, List<T>, string> getTextOverride, List<(Func<T, float>, float)> splitListDefs)
		{
			this.data = data;
			this.getValue = getValue;
			this.getTextOverride = getTextOverride;
			this.splitListDefs = splitListDefs;
			base.Text = text;
			base.ID = key;
			_result = default(T);
			_isResultCached = false;
			return this;
		}

		public override string GetTextOverride(List<InfoCard> cards)
		{
			if (getTextOverride == null || cards.Count <= 1)
			{
				return base.Text;
			}
			IEnumerable<T> source = cards.Select((InfoCard x) => ((TextInfo<T>)x.textInfos[base.ID]).Result);
			return getTextOverride(base.Text, source.ToList());
		}

		public override List<List<InfoCard>> SplitByTIDefs(List<InfoCard> cards)
		{
			if (splitListDefs != null)
			{
				return cards.SplitBySplitters(splitListDefs, (List<InfoCard> g, (Func<T, float>, float) def) => GetSplitByRange(g, def));
			}
			return new List<List<InfoCard>> { cards };
		}

		private List<List<InfoCard>> GetSplitByRange(List<InfoCard> cards, (Func<T, float>, float) def)
		{
			SortedSet<float> sortedSet = new SortedSet<float>(cards.Select((InfoCard x) => GetTIValue(x)));
			float item = def.Item2;
			if (sortedSet.Max - sortedSet.Min <= item)
			{
				return new List<List<InfoCard>> { cards };
			}
			List<float> breakPoints = new List<float>();
			float num = sortedSet.Min;
			float item2 = 0f;
			foreach (float item3 in sortedSet)
			{
				if (item3 - num > item)
				{
					breakPoints.Add(item2);
					num = item3;
				}
				item2 = item3;
			}
			breakPoints.Add(item2);
			return (from x in cards.SplitByKeyToDict((InfoCard x) => GetBreakIndex(GetTIValue(x), breakPoints))
				orderby x.Key
				select x.Value).ToList();
			static int GetBreakIndex(float f, List<float> list)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (f <= list[i])
					{
						return i;
					}
				}
				return -1;
			}
			float GetTIValue(InfoCard ic)
			{
				return Mathf.Round(def.Item1(((TextInfo<T>)ic.textInfos[base.ID]).Result));
			}
		}
	}
	public class DisplayCards
	{
		public List<DisplayCard> UpdateData(List<InfoCard> infoCards)
		{
			List<DisplayCard> list = new List<DisplayCard>();
			if (infoCards == null || infoCards.Count <= 0)
			{
				return list;
			}
			List<List<InfoCard>> list2 = new List<List<InfoCard>>();
			List<List<InfoCard>> groups = new List<List<InfoCard>>();
			List<List<InfoCard>> list3 = new List<List<InfoCard>>();
			List<List<InfoCard>> list4 = new List<List<InfoCard>>();
			try
			{
				list2 = infoCards.SplitByKey((InfoCard card) => card.GetTitleKey());
				groups = list2.SplitMany((List<InfoCard> cards) => cards.SplitByKey((InfoCard card) => card.textInfos.Count));
				list3 = GetTextKeySplits(groups);
				list4 = list3.SplitMany((List<InfoCard> cards) => cards.SplitBySplitters(cards.First().textInfos.ToList(), (List<InfoCard> group, KeyValuePair<string, TextInfo> ti) => ti.Value.SplitByTIDefs(group)));
			}
			catch (Exception)
			{
				Debug.Log((object)"---------------------------------------------");
				Debug.Log((object)"Better Info Cards CRASH - Grouping Info Cards");
				LogCell();
				Debug.Log((object)"---------------------------------------------");
				Debug.Log((object)"Info Cards");
				Debug.Log((object)"----------------------------------------");
				LogICGroup(infoCards);
				LogDSplit(list2, "1");
				LogDSplit(groups, "2");
				LogDSplit(list3, "3");
				LogDSplit(list4, "4");
				throw;
			}
			foreach (List<InfoCard> item in list4)
			{
				list.Add(new DisplayCard(item));
			}
			return list;
			static List<List<InfoCard>> GetTextKeySplits(List<List<InfoCard>> source, List<string> history = null)
			{
				if (history == null)
				{
					history = new List<string>();
				}
				return source.SplitMany(delegate(List<InfoCard> cards)
				{
					List<string> list5 = cards.First().textInfos.Keys.Except(history).ToList();
					if (!list5.Any())
					{
						return new List<List<InfoCard>>(1) { cards };
					}
					List<string> list6 = new List<string>(history);
					list6.AddRange(list5);
					return GetTextKeySplits(cards.SplitBySplitters(list5.ToList(), (List<InfoCard> group, string ti) => group.SplitByKey((InfoCard card) => card.textInfos.ContainsKey(ti))), list6);
				});
			}
			void LogCell()
			{
				//IL_0044: Unknown result type (might be due to invalid IL or missing references)
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					Transform transform = ((Component)infoCards.First(delegate(InfoCard card)
					{
						KSelectable selectable = card.selectable;
						return (Object)(object)((selectable != null) ? ((Component)selectable).gameObject : null) != (Object)null;
					}).selectable).gameObject.transform;
					Vector3 val = ((transform != null) ? transform.position : Vector3.negativeInfinity);
					Debug.Log((object)$"Issue in cell: {Grid.PosToCell(val)} (raw pos: {val}).");
				}
				catch (Exception ex2)
				{
					Debug.Log((object)"Failed to identify problem cell:");
					Debug.Log((object)ex2);
				}
			}
			static void LogDSplit(List<List<InfoCard>> list5, string name)
			{
				Debug.Log((object)("D Split - " + name));
				Debug.Log((object)"----------------------------------------");
				foreach (List<InfoCard> item2 in list5)
				{
					LogICGroup(item2);
				}
			}
			static void LogICGroup(List<InfoCard> group)
			{
				foreach (InfoCard item3 in group)
				{
					item3.LogCard();
				}
				Debug.Log((object)"---------------");
			}
		}
	}
	public static class ModifyHits
	{
		[HarmonyPatch]
		private static class ChangeHits_Patch
		{
			[HarmonyPatch(typeof(SelectTool), "Select")]
			private class ResetSelection_Patch
			{
				private static void Postfix(KSelectable new_selected)
				{
					if ((Object)(object)new_selected == (Object)null)
					{
						priorSelected = null;
					}
				}
			}

			private static KSelectable priorSelected;

			private static MethodBase TargetMethod()
			{
				return AccessTools.Method(typeof(InterfaceTool), "GetObjectUnderCursor", (Type[])null, (Type[])null).MakeGenericMethod(typeof(KSelectable));
			}

			private static void Postfix(bool cycleSelection, ref KSelectable __result, List<Intersection> ___intersections)
			{
				List<List<KSelectable>> selectables;
				int index;
				if (cycleSelection && displayCards.Any())
				{
					if (___intersections.Count >= 2)
					{
						___intersections.RemoveAt(___intersections.Count - 2);
					}
					selectables = GetPotentialSelectables(BaseOptions<Options>.Opts.UseBaseSelection, ___intersections);
					index = GetIndex(priorSelected, selectables);
					KSelectable val = GetGroupedSel() ?? GetHoverSel() ?? GetCardSel();
					__result = (priorSelected = val);
				}
				KSelectable GetCardSel()
				{
					return GetNextCard(selectables, index);
				}
				KSelectable GetGroupedSel()
				{
					if (!Input.GetKey((KeyCode)304) && !Input.GetKey((KeyCode)303))
					{
						return null;
					}
					return GetNextSelectable(selectables, index);
				}
				KSelectable GetHoverSel()
				{
					if (index != -1 || !BaseOptions<Options>.Opts.ForceFirstSelectionToHover)
					{
						return null;
					}
					return ((InterfaceTool)SelectTool.Instance).hover;
				}
			}

			private static List<List<KSelectable>> GetPotentialSelectables(bool restrictToVanilla, List<Intersection> intersections)
			{
				IEnumerable<List<KSelectable>> dispCardSels = from x in displayCards
					select x.GetAllSelectables() into x
					where !x.Contains(null)
					select x;
				IEnumerable<KSelectable> vanillaSels = intersections.Select(delegate(Intersection x)
				{
					//IL_0000: Unknown result type (might be due to invalid IL or missing references)
					MonoBehaviour component = x.component;
					return (KSelectable)(object)((component is KSelectable) ? component : null);
				});
				if (restrictToVanilla)
				{
					dispCardSels = dispCardSels.Where((List<KSelectable> x) => x.Intersect(vanillaSels).Any());
				}
				IEnumerable<KSelectable> source = vanillaSels.Where((KSelectable x) => !dispCardSels.SelectMany((List<KSelectable> y) => y).Contains(x));
				return dispCardSels.Concat(source.Select((KSelectable x) => new List<KSelectable> { x })).ToList();
			}

			private static KSelectable GetNextCard(List<List<KSelectable>> potentialSelectables, int index)
			{
				if (!potentialSelectables.Any())
				{
					return null;
				}
				if (++index >= potentialSelectables.Count)
				{
					index = 0;
				}
				return potentialSelectables[index].FirstOrDefault();
			}

			private static KSelectable GetNextSelectable(List<List<KSelectable>> potentialSelectables, int index)
			{
				if (!potentialSelectables.Any() || index == -1)
				{
					return null;
				}
				List<KSelectable> list = potentialSelectables[index];
				int num = list.IndexOf(priorSelected);
				if (++num >= list.Count)
				{
					num = 0;
				}
				KSelectable val = list[num];
				if ((Object)(object)val != (Object)(object)priorSelected)
				{
					return val;
				}
				return null;
			}

			private static int GetIndex(KSelectable priorSelected, List<List<KSelectable>> newSelectables)
			{
				for (int i = 0; i < newSelectables.Count; i++)
				{
					if (newSelectables[i].Contains(priorSelected))
					{
						return i;
					}
				}
				return -1;
			}
		}

		private static List<DisplayCard> displayCards = new List<DisplayCard>();

		public static void Update(List<DisplayCard> dispCards)
		{
			displayCards = dispCards;
		}
	}
	[HarmonyPatch(typeof(HoverTextDrawer), "EndDrawing")]
	internal class ProcessHoverInfo
	{
		private static void Prefix()
		{
			List<InfoCard> infoCards = InterceptHoverDrawer.ConsumeInfoCards();
			List<DisplayCard> list = new DisplayCards().UpdateData(infoCards);
			ModifyHits.Update(list);
			InterceptHoverDrawer.IsInterceptMode = false;
			foreach (DisplayCard item in list)
			{
				item.Draw();
			}
			InterceptHoverDrawer.IsInterceptMode = true;
			List<InfoCardWidgets> list2 = ExportWidgets.ConsumeWidgets();
			if (list2.Count > 0)
			{
				new Grid(list2, list2[0].YMax).MoveAndResizeInfoCards();
			}
		}
	}
	public static class CardTweaker
	{
		[HarmonyPatch(/*Could not decode attribute arguments.*/)]
		private class TweakShadowBarPrefab
		{
			private static readonly Vector2 border = new Vector2((float)(BaseOptions<Options>.Opts.InfoCardSize.YPadding + 2), (float)BaseOptions<Options>.Opts.InfoCardSize.YPadding);

			private static readonly float opacity = (float)BaseOptions<Options>.Opts.InfoCardOpacity / 100f;

			private static void Prefix(ref Skin skin)
			{
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				//IL_001f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				if (ShouldTweak)
				{
					skin.shadowBarBorder = border;
				}
				Color color = ((Graphic)skin.shadowBarWidget).color;
				((Graphic)skin.shadowBarWidget).color = new Color(color.r, color.g, color.b, opacity);
			}
		}

		[HarmonyPatch(typeof(HoverTextDrawer), "DrawText", new Type[]
		{
			typeof(string),
			typeof(TextStyleSetting),
			typeof(Color),
			typeof(bool)
		})]
		private class TweakFontSize
		{
			private static readonly int fontSizeChange = BaseOptions<Options>.Opts.InfoCardSize.FontSizeChange;

			private static bool Prepare()
			{
				return ShouldTweak;
			}

			private static void Prefix(ref TextStyleSetting style, out bool __state)
			{
				__state = false;
				if (Object.op_Implicit((Object)(object)style))
				{
					TextStyleSetting obj = style;
					obj.fontSize += fontSizeChange;
					__state = true;
				}
			}

			private static void Postfix(ref TextStyleSetting style, bool __state)
			{
				if (__state)
				{
					TextStyleSetting obj = style;
					obj.fontSize -= fontSizeChange;
				}
			}
		}

		[HarmonyPatch(typeof(HoverTextDrawer), "NewLine")]
		private class TweakNewLineHeight
		{
			private static readonly int lineSpacing = BaseOptions<Options>.Opts.InfoCardSize.LineSpacing;

			private static bool Prepare()
			{
				return ShouldTweak;
			}

			private static void Prefix(HoverTextDrawer __instance, ref int min_height)
			{
				min_height = 0;
				__instance.currentPos.y -= lineSpacing;
			}
		}

		[HarmonyPatch(typeof(HoverTextDrawer), "EndShadowBar")]
		private class FixupLineHeight
		{
			private static readonly int lineSpacing = BaseOptions<Options>.Opts.InfoCardSize.LineSpacing;

			private static bool Prepare()
			{
				return ShouldTweak;
			}

			private static void Prefix(HoverTextDrawer __instance)
			{
				if (!InterceptHoverDrawer.IsInterceptMode)
				{
					__instance.currentPos.y += lineSpacing;
				}
			}
		}

		[HarmonyPatch(typeof(HoverTextDrawer), "DrawIcon", new Type[]
		{
			typeof(Sprite),
			typeof(Color),
			typeof(int),
			typeof(int)
		})]
		private class TweakIconSize
		{
			private static readonly int iconSizeChange = BaseOptions<Options>.Opts.InfoCardSize.IconSizeChange;

			private static bool Prepare()
			{
				return ShouldTweak;
			}

			private static void Prefix(ref int image_size)
			{
				image_size += iconSizeChange;
			}
		}

		private static bool ShouldTweak => BaseOptions<Options>.Opts.InfoCardSize.ShouldOverride;
	}
	[HarmonyPatch(typeof(MiscStatusItems), "CreateStatusItems")]
	internal class ChangeStatusItemOverlays
	{
		private static void Postfix(MiscStatusItems __instance)
		{
			StatusItem oreTemp = __instance.OreTemp;
			oreTemp.status_overlays &= -9;
			__instance.PickupableUnreachable.status_overlays = 0;
			if (BaseOptions<Options>.Opts.HideElementCategories)
			{
				__instance.ElementalCategory.status_overlays = 0;
			}
		}
	}
	public class HideElementCategory
	{
		public static IEnumerable<CodeInstruction> ChildTranspiler(IEnumerable<CodeInstruction> codes)
		{
			MethodInfo getMaterialCategory_method = AccessTools.Method(typeof(Element), "GetMaterialCategoryTag", (Type[])null, (Type[])null);
			MethodInfo isVacuum_getter = AccessTools.PropertyGetter(typeof(Element), "IsVacuum");
			CodeInstruction target = codes.Last((CodeInstruction i) => CodeInstructionExtensions.Calls(i, getMaterialCategory_method)).FindPrior(codes, (CodeInstruction i) => CodeInstructionExtensions.Calls(i, isVacuum_getter));
			return codes.Manipulator((CodeInstruction i) => i == target, (CodeInstruction i) => (IEnumerable<CodeInstruction>)(object)new CodeInstruction[2]
			{
				i,
				CodeInstruction.Call(typeof(HideElementCategory), "Splice", (Type[])null, (Type[])null)
			});
		}

		private static bool Splice(bool isVacuum)
		{
			if (!BaseOptions<Options>.Opts.HideElementCategories)
			{
				return isVacuum;
			}
			return true;
		}
	}
	[HarmonyPatch(typeof(HoverTextDrawer), "DrawIcon", new Type[]
	{
		typeof(Sprite),
		typeof(Color),
		typeof(int),
		typeof(int)
	})]
	internal class NoDrawIconShadows
	{
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			bool inNopRegion = false;
			bool firstTarget = true;
			MethodInfo target = AccessTools.Method(typeof(HoverTextDrawer), "AddIndent", (Type[])null, (Type[])null);
			MethodInfo target2 = AccessTools.Method(typeof(RectTransform), "set_sizeDelta", (Type[])null, (Type[])null);
			foreach (CodeInstruction instruction in instructions)
			{
				if (firstTarget && CodeInstructionExtensions.Is(instruction, OpCodes.Call, (MemberInfo)target))
				{
					inNopRegion = true;
					firstTarget = false;
					yield return instruction;
				}
				else if (inNopRegion && CodeInstructionExtensions.Is(instruction, OpCodes.Callvirt, (MemberInfo)target2))
				{
					inNopRegion = false;
					yield return new CodeInstruction(OpCodes.Nop, (object)null);
				}
				else if (inNopRegion)
				{
					yield return new CodeInstruction(OpCodes.Nop, (object)null);
				}
				else
				{
					yield return instruction;
				}
			}
		}
	}
	[HarmonyPatch(typeof(SelectToolHoverTextCard), "ShouldShowGasConduitOverlay")]
	internal class ShowGasBridges
	{
		private static void Postfix(ref bool __result, KSelectable selectable)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			ConduitBridge component = ((Component)selectable).GetComponent<ConduitBridge>();
			if (component != null)
			{
				__result |= (int)component.type == 1;
			}
		}
	}
	[HarmonyPatch(typeof(SelectToolHoverTextCard), "ShouldShowLiquidConduitOverlay")]
	internal class ShowLiquidBridges
	{
		private static void Postfix(ref bool __result, KSelectable selectable)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			ConduitBridge component = ((Component)selectable).GetComponent<ConduitBridge>();
			if (component != null)
			{
				__result |= (int)component.type == 2;
			}
		}
	}
	public static class DetectRunStart_Patch
	{
		public static IEnumerable<CodeInstruction> ChildTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo targetMethod = AccessTools.Method(typeof(Component), "GetComponent", (Type[])null, (Type[])null).MakeGenericMethod(typeof(ChoreConsumer));
			bool afterTarget = false;
			foreach (CodeInstruction i in instructions)
			{
				if (CodeInstructionExtensions.Is(i, OpCodes.Callvirt, (MemberInfo)targetMethod))
				{
					afterTarget = true;
				}
				if (afterTarget && i.OpCodeIs(OpCodes.Ldc_I4_0))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null)
					{
						labels = new List<Label>(i.labels)
					};
					i.labels.Clear();
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return new CodeInstruction(OpCodes.Ldfld, (object)AccessTools.Field(typeof(SelectToolHoverTextCard), "overlayValidHoverObjects"));
					yield return new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.Method(typeof(DetectRunStart_Patch), "DrawUnreachableCard", (Type[])null, (Type[])null));
					afterTarget = false;
				}
				yield return i;
			}
		}

		private static void DrawUnreachableCard(SelectToolHoverTextCard instance, List<KSelectable> overlayValidHoverObjects)
		{
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			StatusItem unreachable = Db.Get().MiscStatusItems.PickupableUnreachable;
			if (overlayValidHoverObjects.Any((KSelectable x) => x.HasStatusItem(unreachable)))
			{
				HoverTextDrawer drawer = HoverTextScreen.Instance.drawer;
				drawer.BeginShadowBar(false);
				drawer.DrawIcon(unreachable.sprite.sprite, ((HoverTextConfiguration)instance).Styles_BodyText.Standard.textColor, 18, -6);
				drawer.AddIndent(8);
				drawer.DrawText(((Resource)unreachable).Name.ToUpper(), ((HoverTextConfiguration)instance).Styles_Title.Standard);
				drawer.EndShadowBar();
			}
		}
	}
	public static class Extensions
	{
		public static string RemoveCountSuffix(this string s)
		{
			if (!char.IsDigit(s[s.Length - 1]))
			{
				return s;
			}
			int num = s.LastIndexOf(" x ");
			return s.Substring(0, (num != -1) ? num : s.Length);
		}
	}
	[RestartRequired]
	public class Options : BaseOptions<Options>
	{
		[JsonObject(/*Could not decode attribute arguments.*/)]
		public class CardSize
		{
			[Option]
			public bool ShouldOverride { get; set; }

			[Option]
			[Limit(-5.0, 5.0)]
			public int FontSizeChange { get; set; }

			[Option]
			[Limit(0.0, 20.0)]
			public int LineSpacing { get; set; }

			[Option]
			[Limit(-10.0, 10.0)]
			public int IconSizeChange { get; set; }

			[Option]
			[Limit(1.0, 20.0)]
			public int YPadding { get; set; }
		}

		[Option]
		[Limit(0.0, 100.0)]
		public int InfoCardOpacity { get; set; }

		[Option]
		public bool HideElementCategories { get; set; }

		[Option]
		public bool UseBaseSelection { get; set; }

		[Option]
		public bool ForceFirstSelectionToHover { get; set; }

		[Option]
		public float TemperatureBandWidth { get; set; }

		[Option]
		public CardSize InfoCardSize { get; set; }

		public Options()
		{
			InfoCardOpacity = 80;
			HideElementCategories = false;
			UseBaseSelection = false;
			ForceFirstSelectionToHover = true;
			TemperatureBandWidth = 10f;
			InfoCardSize = new CardSize
			{
				ShouldOverride = true,
				FontSizeChange = -2,
				LineSpacing = 3,
				IconSizeChange = -3,
				YPadding = 6
			};
		}
	}
	public class ResetPool<T> where T : new()
	{
		private List<T> pool = new List<T>();

		private int currentlyUsedIndex;

		public ResetPool(ref Action resetOn)
		{
			resetOn = (Action)Delegate.Combine(resetOn, (Action)delegate
			{
				Reset();
			});
		}

		public T Get()
		{
			T result;
			if (currentlyUsedIndex < pool.Count)
			{
				result = pool[currentlyUsedIndex];
			}
			else
			{
				pool.Add(result = new T());
			}
			currentlyUsedIndex++;
			return result;
		}

		private void Reset()
		{
			currentlyUsedIndex = 0;
		}
	}
	internal class OPTIONS : AStrings<OPTIONS>
	{
		public class INFOCARDOPACITY
		{
			public static LocString NAME = LocString.op_Implicit("Info Card Opacity");

			public static LocString TOOLTIP = LocString.op_Implicit("Opactiy of info card backgrounds.  (Base game = 90%)");
		}

		public class TEMPERATUREBANDWIDTH
		{
			public static LocString NAME = LocString.op_Implicit("Temperature Band Width");

			public static LocString TOOLTIP = LocString.op_Implicit("Maximum temperature difference at which info cards will be grouped.");
		}

		public class USEBASESELECTION
		{
			public static LocString NAME = LocString.op_Implicit("Restrict Selections");

			public static LocString TOOLTIP = LocString.op_Implicit("On: Restrict selectable info cards based on context.  (Base game)\nOff: All visible info cards can be selected.");
		}

		public class FORCEFIRSTSELECTIONTOHOVER
		{
			public static LocString NAME = LocString.op_Implicit("First Selection Hover");

			public static LocString TOOLTIP = LocString.op_Implicit("On: First selection selects the highlighted item.  (Base game)\nOff: First selection selects the first info card.");
		}

		public class HIDEELEMENTCATEGORIES
		{
			public static LocString NAME = LocString.op_Implicit("Hide Element Categories");

			public static LocString TOOLTIP = LocString.op_Implicit("On: Remove element categories from info cards.");
		}

		public class INFOCARDSIZE
		{
			public static LocString CATEGORY = LocString.op_Implicit("Info Card Size");
		}

		public class SHOULDOVERRIDE
		{
			public static LocString NAME = LocString.op_Implicit("Override Card Size");

			public static LocString TOOLTIP = LocString.op_Implicit("On: Apply custom Info Card Size values.");
		}

		public class FONTSIZECHANGE
		{
			public static LocString NAME = LocString.op_Implicit("Font Size");

			public static LocString TOOLTIP = LocString.op_Implicit("Increase/decrease font size relative to the base game.");
		}

		public class LINESPACING
		{
			public static LocString NAME = LocString.op_Implicit("Line Spacing");

			public static LocString TOOLTIP = LocString.op_Implicit("Padding between text lines.");
		}

		public class ICONSIZECHANGE
		{
			public static LocString NAME = LocString.op_Implicit("Icon Size");

			public static LocString TOOLTIP = LocString.op_Implicit("Increase/decrease icon size relative to the base game.");
		}

		public class YPADDING
		{
			public static LocString NAME = LocString.op_Implicit("Borders");

			public static LocString TOOLTIP = LocString.op_Implicit("Padding on the borders.");
		}
	}
}
namespace BetterInfoCards.Util
{
	[HarmonyPatch(typeof(SelectToolHoverTextCard), "UpdateHoverElements")]
	internal class GroupedTranspiler
	{
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return DetectRunStart_Patch.ChildTranspiler(ExportSelectToolData.GetSelectInfo_Patch.ChildTranspiler(HideElementCategory.ChildTranspiler(instructions)));
		}
	}
	public static class Splitters
	{
		public static Dictionary<TKey, List<TVal>> SplitByKeyToDict<TKey, TVal>(this List<TVal> source, Func<TVal, TKey> getKey)
		{
			Dictionary<TKey, List<TVal>> dictionary = new Dictionary<TKey, List<TVal>>();
			foreach (TVal item in source)
			{
				dictionary.TryAddToDict(getKey(item), item);
			}
			return dictionary;
		}

		public static List<List<TVal>> SplitByKey<TKey, TVal>(this List<TVal> source, Func<TVal, TKey> getKey)
		{
			if (source.Count <= 1)
			{
				return new List<List<TVal>> { source };
			}
			return source.SplitByKeyToDict(getKey).Values.ToList();
		}

		public static List<List<T>> SplitMany<T>(this List<List<T>> source, Func<List<T>, List<List<T>>> getSplits)
		{
			List<List<T>> list = new List<List<T>>();
			foreach (List<T> item in source)
			{
				if (item.Count <= 1)
				{
					list.Add(item);
				}
				else
				{
					list.AddRange(getSplits(item));
				}
			}
			return list;
		}

		public static List<List<TVal>> SplitBySplitters<TVal, TSplit>(this List<TVal> source, List<TSplit> splitters, Func<List<TVal>, TSplit, List<List<TVal>>> getSplits)
		{
			List<List<TVal>> list = new List<List<TVal>> { source };
			foreach (TSplit splitter in splitters)
			{
				list = list.SplitMany((List<TVal> x) => getSplits(x, splitter));
			}
			return list;
		}

		private static void TryAddToDict<TKey, TVal>(this Dictionary<TKey, List<TVal>> dict, TKey key, TVal item)
		{
			if (!dict.TryGetValue(key, out var value))
			{
				value = (dict[key] = new List<TVal>());
			}
			value.Add(item);
		}
	}
}
namespace BetterInfoCards.Export
{
	public static class ExportWidgets
	{
		[HarmonyPatch(typeof(HoverTextDrawer), "BeginDrawing")]
		private class OnBeginDrawing
		{
			private static void Postfix()
			{
				icWidgets.Clear();
			}
		}

		[HarmonyPatch(typeof(HoverTextDrawer), "BeginShadowBar")]
		private class OnBeginShadowBar
		{
			private static void Postfix()
			{
				if (!InterceptHoverDrawer.IsInterceptMode)
				{
					curICWidgets = new InfoCardWidgets();
					icWidgets.Add(curICWidgets);
				}
			}
		}

		[HarmonyPatch(typeof(Pool<MonoBehaviour>), "Draw")]
		private class GetWidget_Patch
		{
			private static void Postfix(Entry<MonoBehaviour> __result, GameObject ___prefab)
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				curICWidgets.AddWidget(__result, ___prefab);
			}
		}

		private static InfoCardWidgets curICWidgets;

		private static List<InfoCardWidgets> icWidgets = new List<InfoCardWidgets>();

		public static List<InfoCardWidgets> ConsumeWidgets()
		{
			List<InfoCardWidgets> result = icWidgets;
			icWidgets = new List<InfoCardWidgets>();
			return result;
		}
	}
}
namespace BetterInfoCards.Converters
{
	[HarmonyPatch(typeof(Db), "Initialize")]
	internal class StatusConverters
	{
		private static void Postfix()
		{
			StatusItem oreMass = Db.Get().MiscStatusItems.OreMass;
			StatusItem oreTemp = Db.Get().MiscStatusItems.OreTemp;
			ConverterManager.AddConverter(((Resource)oreMass).Id, (object data) => ((GameObject)data).GetComponent<PrimaryElement>().Mass, (string original, List<float> masses) => ((Resource)oreMass).Name.Replace("{Mass}", GameUtil.GetFormattedMass(masses.Sum(), (TimeSlice)0, (MetricMassFormat)0, true, "{0:0.#}")) + " <color=#ababab>(Σ)</color>");
			ConverterManager.AddConverter(((Resource)oreTemp).Id, (object data) => ((GameObject)data).GetComponent<PrimaryElement>().Temperature, (string original, List<float> temps) => ((Resource)oreTemp).Name.Replace("{Temp}", GameUtil.GetFormattedTemperature(temps.Average(), (TimeSlice)0, (TemperatureInterpretation)0, true, false)) + " <color=#ababab>(μ)</color>", new List<(Func<float, float>, float)> { ((float x) => x, BaseOptions<Options>.Opts.TemperatureBandWidth) });
		}
	}
}
namespace AzeLib
{
	public static class AzeMod
	{
		private sealed class AzeUserMod : UserMod2
		{
			public override void OnLoad(Harmony harmony)
			{
				UserMod = (UserMod2)(object)this;
				Debug.Log((object)("    - version: " + UserMod.assembly.GetName().Version));
				PUtil.InitLibrary(logVersion: false);
				foreach (MethodInfo item in (from x in ((UserMod2)this).assembly.GetTypes().SelectMany((Type x) => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
					where x.GetCustomAttribute<OnLoadAttribute>() != null
					select x).ToList())
				{
					if (item.GetParameters().Count() == 1)
					{
						object[] parameters = (object[])(object)new Harmony[1] { harmony };
						item.Invoke(null, parameters);
					}
					else
					{
						item.Invoke(null, null);
					}
				}
				((UserMod2)this).OnLoad(harmony);
			}
		}

		public static UserMod2 UserMod { get; set; }
	}
	public class ComponentMapper : ComponentMapper<object>
	{
		public ComponentMapper(List<(Type flagCmp, Type addCmp)> map)
			: base(map.Select(((Type flagCmp, Type addCmp) x) => ((Type flagCmp, Type addCmp, object))(flagCmp: x.flagCmp, addCmp: x.addCmp, null)).ToList())
		{
		}

		public void ApplyMap(GameObject go)
		{
			ApplyMap(go, (object _) => true);
		}
	}
	public class ComponentMapper<T>
	{
		private readonly List<(Type flagCmp, Type addCmp, T filter)> map;

		public ComponentMapper(List<(Type flagCmp, Type addCmp, T filter)> map)
		{
			this.map = map;
		}

		public void ApplyMap(GameObject go, Func<T, bool> shouldAdd)
		{
			Type typeToAdd = GetTypeToAdd(go, shouldAdd);
			if (typeToAdd != null)
			{
				go.AddComponent(typeToAdd);
			}
		}

		private Type GetTypeToAdd(GameObject go, Func<T, bool> shouldAdd)
		{
			foreach (var (type, result, arg) in map)
			{
				if (type != null && HasComponentOrDef(type, go) && shouldAdd(arg))
				{
					return result;
				}
			}
			return null;
			static bool HasComponentOrDef(Type cmpOrDef, GameObject val)
			{
				Component component = val.GetComponent(cmpOrDef);
				if (component == null)
				{
					return val.GetDef(cmpOrDef) != null;
				}
				return Object.op_Implicit((Object)(object)component);
			}
		}
	}
	internal static class AzeLocalization
	{
		internal const string EmptyTranslationPlaceholder = "-";

		private const string TranslationFolder = "Translations";

		internal static bool TryLoadTranslations(out Dictionary<string, string> translations)
		{
			string path = AzeMod.UserMod.path;
			Locale locale = Localization.GetLocale();
			string text = Path.Combine(path, "Translations", ((locale != null) ? locale.Code : null) + ".po");
			if (File.Exists(text))
			{
				translations = (from kvp in Localization.LoadStringsFile(text, false)
					where kvp.Value != "-"
					select kvp).ToDictionary();
				return true;
			}
			translations = null;
			return false;
		}

		internal static void GeneratePOTemplate(Type rootType, List<IAFieldlessStrings> fieldlessStrings)
		{
		}
	}
	internal static class AzeStrings
	{
		private const string pathPrefix = "STRINGS.";

		internal static string GetParentPath(Type type)
		{
			return "STRINGS." + type.Namespace.ToUpper() + ".";
		}

		internal static string GetFullKey(Type type, string partialKey)
		{
			return "STRINGS." + type.FullName.Replace("+", ".").ToUpper() + "." + partialKey;
		}

		internal static string GetFullKey(string lsNamespace, string potPath)
		{
			return "STRINGS." + potPath.Replace(lsNamespace, lsNamespace.ToUpper());
		}
	}
	public class POTEntry(string partialKey, string comment = null)
	{
		public string PartialKey { get; } = partialKey;

		public string Comment { get; } = comment;
	}
	public abstract class AStrings<T> : AStringsBase<T> where T : AStrings<T>
	{
	}
	public abstract class AFieldlessStrings<T> : AStringsBase<T>, IAFieldlessStrings where T : AFieldlessStrings<T>
	{
		public virtual List<POTEntry> GetPOTEntries()
		{
			return new List<POTEntry>();
		}
	}
	internal interface IAFieldlessStrings
	{
		List<POTEntry> GetPOTEntries();
	}
	public abstract class AStringsBase<T> : SingletonBase<T> where T : AStringsBase<T>
	{
		public static bool TryGetString(string partialKey, out StringEntry translation)
		{
			return Strings.TryGet(AzeStrings.GetFullKey(typeof(T), partialKey), ref translation);
		}
	}
	[HarmonyPatch(typeof(Localization), "Initialize")]
	internal static class RegisterStrings
	{
		private static List<Type> locStringRoots;

		private static List<Type> fieldlessStringRoots;

		private static List<IAFieldlessStrings> fieldlessStrings;

		private static bool Prepare()
		{
			locStringRoots = ReflectionHelpers.GetChildTypesOfGenericType(typeof(AStrings<>)).ToList();
			fieldlessStringRoots = ReflectionHelpers.GetChildTypesOfGenericType(typeof(AFieldlessStrings<>)).ToList();
			fieldlessStrings = fieldlessStringRoots.Select(SingletonHelper<IAFieldlessStrings>.GetInstance).ToList();
			if (!locStringRoots.Any())
			{
				return fieldlessStringRoots.Any();
			}
			return true;
		}

		private static void Postfix()
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			Dictionary<string, string> translations;
			bool flag = AzeLocalization.TryLoadTranslations(out translations);
			Type type = locStringRoots.FirstOrDefault();
			if (type != null)
			{
				Localization.AddAssembly(type.Namespace, type.Assembly);
				if (flag)
				{
					SetLocStringFields(translations);
				}
				SetStringsDBEntries(locStringRoots);
			}
			if ((int)Localization.GetSelectedLanguageType() == 0)
			{
				SetStringsDBEntries(fieldlessStringRoots);
			}
			else if (flag)
			{
				SetFieldlessStringsDBEntries(translations);
			}
			AzeLocalization.GeneratePOTemplate(type, fieldlessStrings);
		}

		private static void SetLocStringFields(Dictionary<string, string> translations)
		{
			Localization.OverloadStrings(translations);
		}

		private static void SetStringsDBEntries(List<Type> types)
		{
			foreach (Type type in types)
			{
				LocString.CreateLocStringKeys(type, AzeStrings.GetParentPath(type));
			}
		}

		private static void SetFieldlessStringsDBEntries(Dictionary<string, string> translations)
		{
			foreach (Type fieldlessStringRoot in fieldlessStringRoots)
			{
				string targetType = fieldlessStringRoot.Name + ".";
				foreach (KeyValuePair<string, string> item in translations.Where((KeyValuePair<string, string> kvp) => kvp.Key.Contains(targetType)))
				{
					string partialKey = item.Key.Substring(item.Key.IndexOf(targetType) + targetType.Length);
					Strings.Add(new string[2]
					{
						AzeStrings.GetFullKey(fieldlessStringRoot, partialKey),
						item.Value
					});
				}
			}
		}
	}
	[ConfigFile("config.json", true, false)]
	[JsonObject(/*Could not decode attribute arguments.*/)]
	public abstract class BaseOptions<T> : IOptions where T : BaseOptions<T>, new()
	{
		private static T _opts;

		public static T Opts
		{
			get
			{
				object obj = _opts;
				if (obj == null)
				{
					obj = Validate(POptions.ReadSettings<T>()) ?? new T();
					_opts = (T)obj;
				}
				return (T)obj;
			}
			set
			{
				_opts = value;
			}
		}

		void IOptions.OnOptionsChanged()
		{
			Opts = Validate((T)this);
		}

		private static T Validate(T settings)
		{
			if (settings != null && !settings.ValidateSettings())
			{
				POptions.WriteSettings(settings);
			}
			return settings;
		}

		protected virtual bool ValidateSettings()
		{
			return true;
		}

		IEnumerable<IOptionsEntry> IOptions.CreateOptions()
		{
			return CreateOptions();
		}

		protected virtual IEnumerable<IOptionsEntry> CreateOptions()
		{
			return new List<IOptionsEntry>();
		}
	}
	internal class OptionsInit
	{
		[OnLoad]
		private static void RegisterOptions()
		{
			Type type = ReflectionHelpers.GetChildTypesOfGenericType(typeof(BaseOptions<>)).FirstOrDefault();
			if ((object)type != null)
			{
				new POptions().RegisterOptions(AzeMod.UserMod, type);
			}
		}
	}
	internal static class ReflectionHelpers
	{
		public static IEnumerable<T> CreateAndGetInstances<T>(this IEnumerable<Type> types)
		{
			return types.Select((Type x) => (T)Activator.CreateInstance(x));
		}

		public static IEnumerable<Type> GetChildTypesOfType<T>()
		{
			return from x in typeof(T).Assembly.GetTypes()
				where x.IsSubclassOf(typeof(T)) && !x.IsAbstract
				select x;
		}

		public static IEnumerable<Type> GetChildTypesOfGenericType(Type t)
		{
			return from x in t.Assembly.GetTypes()
				where x.IsSubclassOfRawGeneric(t)
				select x;
		}

		public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
		{
			if (toCheck == generic)
			{
				return false;
			}
			while (toCheck != null && toCheck != typeof(object))
			{
				Type type = (toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck);
				if (generic == type)
				{
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}
	}
	public abstract class SingletonBase<T> where T : SingletonBase<T>
	{
		private static readonly Lazy<T> Lazy = new Lazy<T>(() => Activator.CreateInstance(typeof(T), nonPublic: true) as T);

		public static T Instance => Lazy.Value;
	}
	internal static class SingletonHelper<T>
	{
		public static T GetInstance(Type type)
		{
			return Traverse.Create(type).Property("Instance", (object[])null).GetValue<T>();
		}
	}
}
namespace AzeLib.Extensions
{
	public static class CodeInstructionExt
	{
		public static bool IsLocalOfType(this CodeInstruction i, Type type)
		{
			if (i.operand is LocalBuilder localBuilder && localBuilder.LocalType == type)
			{
				return true;
			}
			return false;
		}

		public static bool OpCodeIs(this CodeInstruction i, OpCode opCode)
		{
			return i.opcode == opCode;
		}

		public static CodeInstruction GetLoadFromStore(this CodeInstruction i)
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Expected O, but got Unknown
			OpCode opCode = OpCodes.Ldloc;
			if (i.OpCodeIs(OpCodes.Stloc_0))
			{
				opCode = OpCodes.Ldloc_0;
			}
			if (i.OpCodeIs(OpCodes.Stloc_1))
			{
				opCode = OpCodes.Ldloc_1;
			}
			if (i.OpCodeIs(OpCodes.Stloc_2))
			{
				opCode = OpCodes.Ldloc_2;
			}
			if (i.OpCodeIs(OpCodes.Stloc_3))
			{
				opCode = OpCodes.Ldloc_3;
			}
			if (i.OpCodeIs(OpCodes.Stloc_S))
			{
				opCode = OpCodes.Ldloc_S;
			}
			return new CodeInstruction(opCode, i.operand);
		}

		public static CodeInstruction MakeNop(this CodeInstruction i)
		{
			i.opcode = OpCodes.Nop;
			return i;
		}

		public static CodeInstruction FindNext(this CodeInstruction i, IEnumerable<CodeInstruction> codes, Func<CodeInstruction, bool> predicate)
		{
			return codes.FindNext(i, predicate);
		}

		public static CodeInstruction FindPrior(this CodeInstruction i, IEnumerable<CodeInstruction> codes, Func<CodeInstruction, bool> predicate)
		{
			return codes.FindPrior(i, predicate);
		}
	}
	public static class EnumerableExt
	{
		public static T FindNext<T>(this IEnumerable<T> e, T start, Func<T, bool> isTarget) where T : class
		{
			List<T> list = new List<T>(e);
			for (int num = list.FindIndex((T x) => x == start) + 1; num < list.Count; num++)
			{
				T val = list[num];
				if (isTarget(val))
				{
					return val;
				}
			}
			return null;
		}

		public static T FindPrior<T>(this IEnumerable<T> e, T start, Func<T, bool> isTarget) where T : class
		{
			List<T> list = new List<T>(e);
			for (int num = list.FindIndex((T x) => x == start) - 1; num >= 0; num--)
			{
				T val = list[num];
				if (isTarget(val))
				{
					return val;
				}
			}
			return null;
		}

		public static IEnumerable<IEnumerable<int>> GroupConsecutive(this IEnumerable<int> source)
		{
			using IEnumerator<int> e = source.GetEnumerator();
			bool more = e.MoveNext();
			while (more)
			{
				int current = e.Current;
				int num = current;
				while (true)
				{
					bool flag;
					more = (flag = e.MoveNext());
					int current2;
					if (!flag || (current2 = e.Current) <= num || current2 - num != 1)
					{
						break;
					}
					num = current2;
				}
				yield return Enumerable.Range(current, num - current + 1);
			}
		}

		public static IEnumerable<Tout> LinqByValue<Tsrc, Tcomp, Tout>(this IEnumerable<Tsrc> source, Func<IEnumerable<Tsrc>, Tcomp, IEnumerable<Tout>> linqFunction, Func<IEnumerable<Tsrc>, Tcomp> getValue)
		{
			return linqFunction(source, getValue(source));
		}

		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
		{
			return source ?? Array.Empty<T>();
		}

		public static Dictionary<TKey, TVal> ToDictionary<TKey, TVal>(this IEnumerable<KeyValuePair<TKey, TVal>> source)
		{
			return source.ToDictionary((KeyValuePair<TKey, TVal> kvp) => kvp.Key, (KeyValuePair<TKey, TVal> kvp) => kvp.Value);
		}
	}
	public static class GameObjectExt
	{
		public static Component GetReflectionComp(this GameObject go, string typeString)
		{
			Type type = AccessTools.TypeByName(typeString);
			if (type == null)
			{
				return null;
			}
			return go.GetComponent(type);
		}

		public static BaseDef GetDef(this GameObject go, Type type)
		{
			StateMachineController component = go.GetComponent<StateMachineController>();
			if ((Object)(object)component == (Object)null)
			{
				return null;
			}
			return component.cmpdef.defs.FirstOrDefault((BaseDef x) => ((object)x).GetType() == type);
		}
	}
	public static class LogicPortsExt
	{
		public static IEnumerable<CellOffset> GetLogicCellOffsets(this LogicPorts logicPorts)
		{
			return from x in logicPorts.inputPortInfo.EmptyIfNull().Concat(logicPorts.outputPortInfo.EmptyIfNull())
				select x.cellOffset;
		}

		public static IEnumerable<CellOffset> GetLogicCellOffsets(this LogicGateBase logicGateBase)
		{
			return logicGateBase.inputPortOffsets.EmptyIfNull().Concat(logicGateBase.outputPortOffsets.EmptyIfNull()).Concat(logicGateBase.controlPortOffsets.EmptyIfNull());
		}
	}
	public static class ProgressBarExt
	{
		public static void ToggleVisibility(this ProgressBar pb, bool isVisible)
		{
			((Component)pb).GetComponent<CanvasGroup>().alpha = (isVisible ? 0f : 1f);
		}
	}
	public static class StringExt
	{
		public static string Truncate(this string source, int length)
		{
			if (source.Length > length)
			{
				source = source.Substring(0, length);
			}
			return source;
		}

		public static string NullIfEmpty(this string source)
		{
			if (!Util.IsNullOrWhiteSpace(source))
			{
				return source;
			}
			return null;
		}
	}
	public static class TranspilerExt
	{
		public static IEnumerable<CodeInstruction> MethodRemover(this IEnumerable<CodeInstruction> instructions, MethodInfo toRemove)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (CodeInstructionExtensions.OperandIs(instruction, (MemberInfo)toRemove))
				{
					if (!toRemove.IsStatic)
					{
						yield return new CodeInstruction(OpCodes.Pop, (object)null);
					}
					for (int p = 0; p < toRemove.GetParameters().Count(); p++)
					{
						yield return new CodeInstruction(OpCodes.Pop, (object)null);
					}
				}
				else
				{
					yield return instruction;
				}
			}
		}

		public static IEnumerable<CodeInstruction> Manipulator(this IEnumerable<CodeInstruction> codes, Func<CodeInstruction, bool> predicate, Func<CodeInstruction, IEnumerable<CodeInstruction>> function)
		{
			foreach (CodeInstruction code in codes)
			{
				if (predicate(code))
				{
					foreach (CodeInstruction item in function(code))
					{
						yield return item;
					}
				}
				else
				{
					yield return code;
				}
			}
		}

		public static IEnumerable<CodeInstruction> Manipulator(this IEnumerable<CodeInstruction> codes, Func<CodeInstruction, bool> predicate, Action<IEnumerable<CodeInstruction>, CodeInstruction> action)
		{
			return Transpilers.Manipulator(codes, predicate, (Action<CodeInstruction>)delegate(CodeInstruction i)
			{
				action(codes, i);
			});
		}

		public static IEnumerable<CodeInstruction> Manipulator(this IEnumerable<CodeInstruction> codes, object targetOperand, Func<CodeInstruction, IEnumerable<CodeInstruction>> manipulator)
		{
			return codes.Manipulator((CodeInstruction i) => CodeInstructionExtensions.OperandIs(i, targetOperand), manipulator);
		}
	}
	public static class Vector3Ext
	{
		public static Vector3 InverseLocalScale(this RectTransform rectTransform)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			Vector3 localScale = ((Transform)rectTransform).localScale;
			return new Vector3(1f / localScale.x, 1f / localScale.y, 1f / localScale.z);
		}
	}
}
namespace AzeLib.Attributes
{
	public class AMonoBehaviour : KMonoBehaviour
	{
		public override void OnSpawn()
		{
			((KMonoBehaviour)this).OnSpawn();
			FieldInfo[] fields = ((object)this).GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				object[] customAttributes = fieldInfo.GetCustomAttributes(inherit: false);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					if (customAttributes[j].GetType() == typeof(MyIntGetAttribute))
					{
						fieldInfo.SetValue(this, ((Component)this).GetComponent(fieldInfo.FieldType));
					}
				}
			}
		}
	}
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class MyIntGetAttribute : Attribute
	{
	}
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class OnLoadAttribute : Attribute
	{
	}
}
namespace PeterHan.PLib.Actions
{
	public sealed class PAction
	{
		private readonly int id;

		public static Action MaxAction { get; }

		internal PKeyBinding DefaultBinding { get; }

		public string Identifier { get; }

		public LocString Title { get; }

		static PAction()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (!Enum.TryParse<Action>("NumActions", out Action result))
			{
				result = (Action)280;
			}
			MaxAction = result;
		}

		internal PAction(int id, string identifier, LocString title, PKeyBinding binding)
		{
			if (id <= 0)
			{
				throw new ArgumentOutOfRangeException("id");
			}
			DefaultBinding = binding;
			Identifier = identifier;
			this.id = id;
			Title = title;
		}

		public override bool Equals(object obj)
		{
			if (obj is PAction pAction)
			{
				return pAction.id == id;
			}
			return false;
		}

		public Action GetKAction()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return (Action)(MaxAction + id);
		}

		public override int GetHashCode()
		{
			return id;
		}

		public override string ToString()
		{
			return "PAction[" + Identifier + "]: " + LocString.op_Implicit(Title);
		}
	}
	public sealed class PActionManager : PForwardedComponent
	{
		private delegate BindingEntry NewEntry(string group, GamepadButton button, KKeyCode key_code, Modifier modifier, Action action);

		public const string CATEGORY = "PLib";

		private static readonly NewEntry NEW_BINDING_ENTRY = typeof(BindingEntry).DetourConstructor<NewEntry>();

		internal static readonly Version VERSION = new Version("4.24.0.0");

		private readonly IList<PAction> actions;

		private int maxAction;

		internal static PActionManager Instance { get; private set; }

		public override Version Version => VERSION;

		private static void AssignKeyBindings()
		{
			IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(typeof(PActionManager).FullName);
			if (allComponents == null)
			{
				return;
			}
			foreach (PForwardedComponent item in allComponents)
			{
				item.Process(0u, null);
			}
		}

		private static void CKeyDef_Postfix(KeyDef __instance)
		{
			if (Instance != null)
			{
				__instance.mActionFlags = ExtendFlags(__instance.mActionFlags, Instance.GetMaxAction());
			}
		}

		internal static bool[] ExtendFlags(bool[] oldActionFlags, int newMax)
		{
			int num = oldActionFlags.Length;
			bool[] array;
			if (num < newMax)
			{
				array = new bool[newMax];
				Array.Copy(oldActionFlags, array, num);
			}
			else
			{
				array = oldActionFlags;
			}
			return array;
		}

		private static bool FindKeyBinding(IEnumerable<BindingEntry> currentBindings, Action action)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			bool result = false;
			foreach (BindingEntry currentBinding in currentBindings)
			{
				if (currentBinding.mAction == action)
				{
					LogKeyBind("Action {0} already exists; assigned to KeyCode {1}".F(action, currentBinding.mKeyCode));
					result = true;
					break;
				}
			}
			return result;
		}

		private static string GetBindingTitle(string category, string item)
		{
			return "STRINGS.INPUT_BINDINGS." + category.ToUpperInvariant() + "." + item.ToUpperInvariant();
		}

		internal static string GetExtraKeycodeLocalized(KKeyCode code)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Invalid comparison between Unknown and I4
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Expected I4, but got Unknown
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Invalid comparison between Unknown and I4
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Invalid comparison between Unknown and I4
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I4
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Invalid comparison between Unknown and I4
			string result = null;
			if ((int)code <= 127)
			{
				if ((int)code != 19)
				{
					if ((int)code == 127)
					{
						result = LocString.op_Implicit(PLibStrings.KEY_DELETE);
					}
				}
				else
				{
					result = LocString.op_Implicit(PLibStrings.KEY_PAUSE);
				}
			}
			else
			{
				switch (code - 273)
				{
				default:
					if ((int)code != 316)
					{
						if ((int)code == 317)
						{
							result = LocString.op_Implicit(PLibStrings.KEY_SYSRQ);
						}
					}
					else
					{
						result = LocString.op_Implicit(PLibStrings.KEY_PRTSCREEN);
					}
					break;
				case 5:
					result = LocString.op_Implicit(PLibStrings.KEY_HOME);
					break;
				case 6:
					result = LocString.op_Implicit(PLibStrings.KEY_END);
					break;
				case 8:
					result = LocString.op_Implicit(PLibStrings.KEY_PAGEDOWN);
					break;
				case 7:
					result = LocString.op_Implicit(PLibStrings.KEY_PAGEUP);
					break;
				case 3:
					result = LocString.op_Implicit(PLibStrings.KEY_ARROWLEFT);
					break;
				case 0:
					result = LocString.op_Implicit(PLibStrings.KEY_ARROWUP);
					break;
				case 2:
					result = LocString.op_Implicit(PLibStrings.KEY_ARROWRIGHT);
					break;
				case 1:
					result = LocString.op_Implicit(PLibStrings.KEY_ARROWDOWN);
					break;
				case 4:
					break;
				}
			}
			return result;
		}

		private static bool GetKeycodeLocalized_Prefix(KKeyCode key_code, ref string __result)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			string extraKeycodeLocalized = GetExtraKeycodeLocalized(key_code);
			if (extraKeycodeLocalized != null)
			{
				__result = extraKeycodeLocalized;
			}
			return extraKeycodeLocalized == null;
		}

		private static void IsActive_Prefix(ref bool[] ___mActionState)
		{
			if (Instance != null)
			{
				___mActionState = ExtendFlags(___mActionState, Instance.GetMaxAction());
			}
		}

		internal static void LogKeyBind(string message)
		{
			Debug.LogFormat("[PKeyBinding] {0}", new object[1] { message });
		}

		internal static void LogKeyBindWarning(string message)
		{
			Debug.LogWarningFormat("[PKeyBinding] {0}", new object[1] { message });
		}

		private static void QueueButtonEvent_Prefix(ref bool[] ___mActionState, KeyDef key_def)
		{
			if (KInputManager.isFocused && Instance != null)
			{
				int newMax = Instance.GetMaxAction();
				key_def.mActionFlags = ExtendFlags(key_def.mActionFlags, newMax);
				___mActionState = ExtendFlags(___mActionState, newMax);
			}
		}

		private static void SetDefaultKeyBindings_Postfix()
		{
			Instance?.UpdateMaxAction();
		}

		public PActionManager()
		{
			actions = new List<PAction>(8);
			maxAction = 0;
		}

		public override void Bootstrap(Harmony plibInstance)
		{
			plibInstance.Patch(typeof(GameInputMapping), "LoadBindings", PatchMethod("AssignKeyBindings"));
		}

		public PAction CreateAction(string identifier, LocString title, PKeyBinding binding = null)
		{
			RegisterForForwarding();
			int sharedData = GetSharedData(1);
			PAction pAction = new PAction(sharedData, identifier, title, binding);
			SetSharedData(sharedData + 1);
			actions.Add(pAction);
			return pAction;
		}

		public int GetMaxAction()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected I4, but got Unknown
			int num = (int)PAction.MaxAction;
			if (maxAction <= 0)
			{
				return num - 1;
			}
			return maxAction + num;
		}

		public override void Initialize(Harmony plibInstance)
		{
			Instance = this;
			plibInstance.Patch(typeof(GameInputMapping), "SetDefaultKeyBindings", null, PatchMethod("SetDefaultKeyBindings_Postfix"));
			plibInstance.Patch(typeof(GameUtil), "GetKeycodeLocalized", PatchMethod("GetKeycodeLocalized_Prefix"));
			plibInstance.PatchConstructor(typeof(KeyDef), new Type[2]
			{
				typeof(KKeyCode),
				typeof(Modifier)
			}, null, PatchMethod("CKeyDef_Postfix"));
			plibInstance.Patch(typeof(KInputController), "IsActive", PatchMethod("IsActive_Prefix"));
			plibInstance.Patch(typeof(KInputController), "QueueButtonEvent", PatchMethod("QueueButtonEvent_Prefix"));
		}

		public override void PostInitialize(Harmony plibInstance)
		{
			Strings.Add(new string[2]
			{
				GetBindingTitle("PLib", "NAME"),
				LocString.op_Implicit(PLibStrings.KEY_CATEGORY_TITLE)
			});
			IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(base.ID);
			if (allComponents == null)
			{
				return;
			}
			foreach (PForwardedComponent item in allComponents)
			{
				item.Process(1u, null);
			}
		}

		public override void Process(uint operation, object _)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			if (actions.Count <= 0)
			{
				return;
			}
			switch (operation)
			{
			case 0u:
				RegisterKeyBindings();
				break;
			case 1u:
			{
				foreach (PAction action in actions)
				{
					Strings.Add(new string[2]
					{
						GetBindingTitle("PLib", ((object)action.GetKAction()/*cast due to .constrained prefix*/).ToString()),
						LocString.op_Implicit(action.Title)
					});
				}
				break;
			}
			}
		}

		private void RegisterKeyBindings()
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			int count = actions.Count;
			LogKeyBind("Registering {0:D} key bind(s) for mod {1}".F(count, Assembly.GetExecutingAssembly().GetNameSafe() ?? "?"));
			List<BindingEntry> list = new List<BindingEntry>(GameInputMapping.DefaultBindings);
			foreach (PAction action in actions)
			{
				Action kAction = action.GetKAction();
				PKeyBinding pKeyBinding = action.DefaultBinding;
				if (!FindKeyBinding(list, kAction))
				{
					if (pKeyBinding == null)
					{
						pKeyBinding = new PKeyBinding((KKeyCode)0, (Modifier)0, (GamepadButton)16);
					}
					list.Add(NEW_BINDING_ENTRY("PLib", pKeyBinding.GamePadButton, pKeyBinding.Key, pKeyBinding.Modifiers, kAction));
				}
			}
			GameInputMapping.SetDefaultKeyBindings(list.ToArray());
			UpdateMaxAction();
		}

		private void UpdateMaxAction()
		{
			maxAction = GetSharedData(0);
		}
	}
	public sealed class PKeyBinding
	{
		public GamepadButton GamePadButton { get; set; }

		public KKeyCode Key { get; set; }

		public Modifier Modifiers { get; set; }

		public PKeyBinding(KKeyCode keyCode = (KKeyCode)0, Modifier modifiers = (Modifier)0, GamepadButton gamePadButton = (GamepadButton)16)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			GamePadButton = gamePadButton;
			Key = keyCode;
			Modifiers = modifiers;
		}

		public PKeyBinding(PKeyBinding other)
			: this((KKeyCode)0, (Modifier)0, (GamepadButton)16)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			if (other != null)
			{
				GamePadButton = other.GamePadButton;
				Key = other.Key;
				Modifiers = other.Modifiers;
			}
		}

		public override bool Equals(object obj)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (obj is PKeyBinding pKeyBinding && pKeyBinding.Key == Key && pKeyBinding.Modifiers == Modifiers)
			{
				return pKeyBinding.GamePadButton == GamePadButton;
			}
			return false;
		}

		public override int GetHashCode()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			return ((1488379021 + ((object)GamePadButton/*cast due to .constrained prefix*/).GetHashCode()) * -1521134295 + ((object)Key/*cast due to .constrained prefix*/).GetHashCode()) * -1521134295 + ((object)Modifiers/*cast due to .constrained prefix*/).GetHashCode();
		}

		public override string ToString()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			return ((object)Modifiers/*cast due to .constrained prefix*/).ToString() + " " + ((object)Key/*cast due to .constrained prefix*/).ToString();
		}
	}
	public sealed class PToolMode
	{
		public string Key { get; }

		public ToggleState State { get; }

		public LocString Title { get; }

		public static IDictionary<string, ToggleState> PopulateMenu(ToolParameterMenu menu, ICollection<PToolMode> options)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}
			Dictionary<string, ToggleState> dictionary = new Dictionary<string, ToggleState>(options.Count);
			foreach (PToolMode option in options)
			{
				string key = option.Key;
				if (!string.IsNullOrEmpty(LocString.op_Implicit(option.Title)))
				{
					Strings.Add(new string[2]
					{
						"STRINGS.UI.TOOLS.FILTERLAYERS." + key,
						LocString.op_Implicit(option.Title)
					});
				}
				dictionary.Add(key, option.State);
			}
			menu.PopulateMenu(dictionary);
			return dictionary;
		}

		public static void RegisterTool<T>(PlayerController controller) where T : InterfaceTool
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)controller == (Object)null)
			{
				throw new ArgumentNullException("controller");
			}
			PooledList<InterfaceTool, PlayerController> val = ListPool<InterfaceTool, PlayerController>.Allocate();
			((List<InterfaceTool>)(object)val).AddRange((IEnumerable<InterfaceTool>)controller.tools);
			GameObject val2 = new GameObject(typeof(T).Name);
			T val3 = val2.AddComponent<T>();
			val2.transform.SetParent(((Component)controller).gameObject.transform);
			val2.gameObject.SetActive(true);
			val2.gameObject.SetActive(false);
			((List<InterfaceTool>)(object)val).Add((InterfaceTool)(object)val3);
			controller.tools = ((List<InterfaceTool>)(object)val).ToArray();
			val.Recycle();
		}

		public PToolMode(string key, LocString title, ToggleState state = (ToggleState)1)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			Key = key;
			State = state;
			Title = title;
		}

		public override bool Equals(object obj)
		{
			if (obj is PToolMode pToolMode)
			{
				return pToolMode.Key == Key;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Key.GetHashCode();
		}

		public override string ToString()
		{
			return "{0} ({1})".F(Key, Title);
		}
	}
}
namespace PeterHan.PLib.AVC
{
	public interface IModVersionChecker
	{
		event PVersionCheck.OnVersionCheckComplete OnVersionCheckCompleted;

		bool CheckVersion(Mod mod);
	}
	public sealed class JsonURLVersionChecker : IModVersionChecker
	{
		[JsonObject(/*Could not decode attribute arguments.*/)]
		public sealed class ModVersions
		{
			[JsonProperty]
			public List<ModVersion> mods;

			public ModVersions()
			{
				mods = new List<ModVersion>(16);
			}
		}

		public sealed class ModVersion
		{
			public string staticID { get; set; }

			public string version { get; set; }

			public override string ToString()
			{
				return "{0}: version={1}".F(staticID, version);
			}
		}

		public const int REQUEST_TIMEOUT = 8;

		public string JsonVersionURL { get; }

		public event PVersionCheck.OnVersionCheckComplete OnVersionCheckCompleted;

		public JsonURLVersionChecker(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				throw new ArgumentNullException("url");
			}
			JsonVersionURL = url;
		}

		public bool CheckVersion(Mod mod)
		{
			if (mod == null)
			{
				throw new ArgumentNullException("mod");
			}
			UnityWebRequest request = UnityWebRequest.Get(JsonVersionURL);
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("User-Agent", "PLib AVC");
			request.timeout = 8;
			((AsyncOperation)request.SendWebRequest()).completed += delegate
			{
				OnRequestFinished(request, mod);
			};
			return true;
		}

		private void OnRequestFinished(UnityWebRequest request, Mod mod)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Invalid comparison between Unknown and I4
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			ModVersionCheckResults result = null;
			if ((int)request.result == 1)
			{
				ModVersions modVersions;
				using (StreamReader streamReader = new StreamReader(new MemoryStream(request.downloadHandler.data)))
				{
					modVersions = new JsonSerializer
					{
						MaxDepth = 4,
						DateTimeZoneHandling = (DateTimeZoneHandling)1,
						ReferenceLoopHandling = (ReferenceLoopHandling)1
					}.Deserialize<ModVersions>((JsonReader)new JsonTextReader((TextReader)streamReader));
				}
				if (modVersions != null)
				{
					result = ParseModVersion(mod, modVersions);
				}
			}
			request.Dispose();
			this.OnVersionCheckCompleted?.Invoke(result);
		}

		private ModVersionCheckResults ParseModVersion(Mod mod, ModVersions versions)
		{
			ModVersionCheckResults result = null;
			string staticID = mod.staticID;
			if (versions.mods != null)
			{
				foreach (ModVersion mod2 in versions.mods)
				{
					if (mod2 != null && mod2.staticID == staticID)
					{
						string text = mod2.version?.Trim();
						result = ((!string.IsNullOrEmpty(text)) ? new ModVersionCheckResults(staticID, text == PVersionCheck.GetCurrentVersion(mod), text) : new ModVersionCheckResults(staticID, updated: true));
						break;
					}
				}
			}
			return result;
		}
	}
	internal sealed class ModOutdatedWarning : KMonoBehaviour
	{
		private static readonly IDetouredField<MainMenu, KButton> RESUME_GAME = PDetours.DetourFieldLazy<MainMenu, KButton>("Button_ResumeGame");

		private GameObject modsButton;

		internal static ModOutdatedWarning Instance { get; private set; }

		internal ModOutdatedWarning()
		{
			modsButton = null;
		}

		private void FindModsButton(Transform buttonParent)
		{
			int childCount = buttonParent.childCount;
			for (int i = 0; i < childCount; i++)
			{
				GameObject gameObject = ((Component)buttonParent.GetChild(i)).gameObject;
				if ((Object)(object)gameObject != (Object)null)
				{
					LocText componentInChildren = gameObject.GetComponentInChildren<LocText>();
					if (((componentInChildren != null) ? ((TMP_Text)componentInChildren).text : null) == LocString.op_Implicit(MODS.TITLE))
					{
						modsButton = gameObject;
						break;
					}
				}
			}
			if ((Object)(object)modsButton == (Object)null)
			{
				PUtil.LogWarning("Unable to find Mods menu button, main menu update warning will not be functional");
			}
		}

		protected override void OnCleanUp()
		{
			Instance = null;
			((KMonoBehaviour)this).OnCleanUp();
		}

		protected override void OnPrefabInit()
		{
			((KMonoBehaviour)this).OnPrefabInit();
			MainMenu component = ((Component)this).GetComponent<MainMenu>();
			Instance = this;
			try
			{
				KButton val;
				Transform parent;
				if ((Object)(object)component != (Object)null && (Object)(object)(val = RESUME_GAME.Get(component)) != (Object)null && (Object)(object)(parent = ((KMonoBehaviour)val).transform.parent) != (Object)null)
				{
					FindModsButton(parent);
				}
			}
			catch (DetourException)
			{
			}
			UpdateText();
		}

		private void UpdateText()
		{
			PVersionCheck instance = PVersionCheck.Instance;
			if ((Object)(object)modsButton != (Object)null && instance != null)
			{
				LocText componentInChildren = modsButton.GetComponentInChildren<LocText>();
				int outdatedMods = instance.OutdatedMods;
				if (outdatedMods > 0 && (Object)(object)componentInChildren != (Object)null)
				{
					string text = LocString.op_Implicit(MODS.TITLE);
					text = ((outdatedMods != 1) ? (text + string.Format(LocString.op_Implicit(PLibStrings.MAINMENU_UPDATE), outdatedMods)) : (text + LocString.op_Implicit(PLibStrings.MAINMENU_UPDATE_1)));
					((TMP_Text)componentInChildren).text = text;
				}
			}
		}

		public IEnumerator UpdateTextThreaded()
		{
			yield return null;
			UpdateText();
		}
	}
	[JsonObject(/*Could not decode attribute arguments.*/)]
	public sealed class ModVersionCheckResults
	{
		[JsonProperty]
		public bool IsUpToDate { get; set; }

		[JsonProperty]
		public string ModChecked { get; set; }

		[JsonProperty]
		public string NewVersion { get; set; }

		public ModVersionCheckResults()
			: this("", updated: false)
		{
		}

		public ModVersionCheckResults(string id, bool updated, string newVersion = null)
		{
			IsUpToDate = updated;
			ModChecked = id;
			NewVersion = newVersion;
		}

		public override bool Equals(object obj)
		{
			if (obj is ModVersionCheckResults modVersionCheckResults && modVersionCheckResults.ModChecked == ModChecked && IsUpToDate == modVersionCheckResults.IsUpToDate)
			{
				return NewVersion == modVersionCheckResults.NewVersion;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ModChecked.GetHashCode();
		}

		public override string ToString()
		{
			return "ModVersionCheckResults[{0},updated={1},newVersion={2}]".F(ModChecked, IsUpToDate, NewVersion ?? "");
		}
	}
	public sealed class PVersionCheck : PForwardedComponent
	{
		public delegate void OnVersionCheckComplete(ModVersionCheckResults result);

		private sealed class AllVersionCheckTask
		{
			private readonly IList<PForwardedComponent> checkAllVersions;

			private int index;

			private readonly PVersionCheck parent;

			internal AllVersionCheckTask(IEnumerable<PForwardedComponent> allMods, PVersionCheck parent)
			{
				if (allMods == null)
				{
					throw new ArgumentNullException("allMods");
				}
				List<PForwardedComponent> list = new List<PForwardedComponent>(allMods);
				list.Sort(new PComponentComparator());
				checkAllVersions = list;
				index = 0;
				this.parent = parent ?? throw new ArgumentNullException("parent");
			}

			internal void Run()
			{
				int count = checkAllVersions.Count;
				bool flag = true;
				while (index < count)
				{
					PForwardedComponent pForwardedComponent = checkAllVersions[index++];
					if (pForwardedComponent == null)
					{
						PUtil.LogDebug("Invalid version checker reported by PForwardedComponent!");
						continue;
					}
					if (pForwardedComponent.Version.CompareTo(WORKING_VERSION) < 0)
					{
						parent.ReportResults();
						flag = false;
					}
					else
					{
						pForwardedComponent.Process(0u, new Action(Run));
						flag = false;
					}
					break;
				}
				if (index >= count && flag)
				{
					parent.ReportResults();
				}
			}

			public override string ToString()
			{
				return "AllVersionCheckTask for {0:D} mods".F(checkAllVersions.Count);
			}
		}

		private sealed class VersionCheckMethods
		{
			internal IList<IModVersionChecker> Methods { get; }

			internal Mod ModToCheck { get; }

			internal VersionCheckMethods(Mod mod)
			{
				Methods = new List<IModVersionChecker>(8);
				ModToCheck = mod ?? throw new ArgumentNullException("mod");
				PUtil.LogDebug("Registered mod ID {0} for automatic version checking".F(ModToCheck.staticID));
			}

			public override string ToString()
			{
				return ModToCheck.staticID;
			}
		}

		private static readonly Version WORKING_VERSION = new Version(4, 14, 0, 0);

		internal static readonly Version VERSION = new Version("4.24.0.0");

		private readonly IDictionary<string, VersionCheckMethods> checkVersions;

		private readonly ICollection<ModVersionCheckResults> results;

		private readonly IDictionary<string, ModVersionCheckResults> resultsByMod;

		internal static PVersionCheck Instance { get; private set; }

		public int OutdatedMods
		{
			get
			{
				int num = 0;
				foreach (ModVersionCheckResults result in results)
				{
					if (!result.IsUpToDate)
					{
						num++;
					}
				}
				return num;
			}
		}

		public override Version Version => VERSION;

		private static string GetCurrentVersion(Assembly assembly)
		{
			string text = null;
			if (assembly != null)
			{
				text = assembly.GetFileVersion();
				if (string.IsNullOrEmpty(text))
				{
					text = assembly.GetName().Version?.ToString();
				}
			}
			return text;
		}

		public static string GetCurrentVersion(Mod mod)
		{
			if (mod == null)
			{
				throw new ArgumentNullException("mod");
			}
			PackagedModInfo packagedModInfo = mod.packagedModInfo;
			string text = ((packagedModInfo != null) ? packagedModInfo.version : null);
			if (string.IsNullOrEmpty(text))
			{
				Dictionary<Assembly, UserMod2> dictionary = mod.loaded_mod_data?.userMod2Instances;
				ICollection<Assembly> collection = mod.loaded_mod_data?.dlls;
				if (dictionary != null)
				{
					foreach (KeyValuePair<Assembly, UserMod2> item in dictionary)
					{
						text = GetCurrentVersion(item.Key);
						if (!string.IsNullOrEmpty(text))
						{
							break;
						}
					}
				}
				else if (collection != null && collection.Count > 0)
				{
					foreach (Assembly item2 in collection)
					{
						text = GetCurrentVersion(item2);
						if (!string.IsNullOrEmpty(text))
						{
							break;
						}
					}
				}
				else
				{
					text = "";
				}
			}
			return text;
		}

		private static void MainMenu_OnSpawn_Postfix(MainMenu __instance)
		{
			Instance?.RunVersionCheck();
			if ((Object)(object)__instance != (Object)null)
			{
				EntityTemplateExtensions.AddOrGet<ModOutdatedWarning>(((Component)__instance).gameObject);
			}
		}

		private static void ModsScreen_BuildDisplay_Postfix(IEnumerable ___displayedMods)
		{
			if (Instance == null || ___displayedMods == null)
			{
				return;
			}
			foreach (object? ___displayedMod in ___displayedMods)
			{
				Instance.AddWarningIfOutdated(___displayedMod);
			}
		}

		public PVersionCheck()
		{
			checkVersions = new Dictionary<string, VersionCheckMethods>(8);
			results = new List<ModVersionCheckResults>(8);
			resultsByMod = new Dictionary<string, ModVersionCheckResults>(32);
			InstanceData = results;
		}

		private void AddWarningIfOutdated(object modEntry)
		{
			int num = -1;
			modEntry.GetType();
			if (PPatchTools.TryGetFieldValue<int>(modEntry, "mod_index", out var value))
			{
				num = value;
			}
			List<Mod> list = Global.Instance.modManager?.mods;
			if (PPatchTools.TryGetFieldValue<RectTransform>(modEntry, "rect_transform", out var value2) && list != null && num >= 0 && num < list.Count)
			{
				Mod obj = list[num];
				string key;
				HierarchyReferences val = default(HierarchyReferences);
				if (!string.IsNullOrEmpty(key = ((obj != null) ? obj.staticID : null)) && ((Component)value2).TryGetComponent<HierarchyReferences>(ref val) && resultsByMod.TryGetValue(key, out var value3) && value3 != null)
				{
					AddWarningIfOutdated(value3, val.GetReference<LocText>("Version"));
				}
			}
		}

		private void AddWarningIfOutdated(ModVersionCheckResults data, LocText versionText)
		{
			GameObject gameObject;
			if ((Object)(object)versionText != (Object)null && (Object)(object)(gameObject = ((Component)versionText).gameObject) != (Object)null && !data.IsUpToDate)
			{
				string text = ((TMP_Text)versionText).text;
				text = ((!string.IsNullOrEmpty(text)) ? (text + " " + LocString.op_Implicit(PLibStrings.OUTDATED_WARNING)) : LocString.op_Implicit(PLibStrings.OUTDATED_WARNING));
				((TMP_Text)versionText).text = text;
				EntityTemplateExtensions.AddOrGet<ToolTip>(gameObject).toolTip = string.Format(LocString.op_Implicit(PLibStrings.OUTDATED_TOOLTIP), data.NewVersion ?? "");
			}
		}

		public override void Initialize(Harmony plibInstance)
		{
			Instance = this;
			plibInstance.Patch(typeof(MainMenu), "OnSpawn", null, PatchMethod("MainMenu_OnSpawn_Postfix"));
			plibInstance.Patch(typeof(ModsScreen), "BuildDisplay", null, PatchMethod("ModsScreen_BuildDisplay_Postfix"));
		}

		public override void Process(uint operation, object args)
		{
			if (operation != 0 || !(args is Action next))
			{
				return;
			}
			VersionCheckTask versionCheckTask = null;
			VersionCheckTask versionCheckTask2 = null;
			results.Clear();
			foreach (KeyValuePair<string, VersionCheckMethods> checkVersion in checkVersions)
			{
				VersionCheckMethods value = checkVersion.Value;
				foreach (IModVersionChecker method in value.Methods)
				{
					VersionCheckTask versionCheckTask3 = new VersionCheckTask(value.ModToCheck, method, results)
					{
						Next = next
					};
					if (versionCheckTask2 != null)
					{
						versionCheckTask2.Next = versionCheckTask3.Run;
					}
					if (versionCheckTask == null)
					{
						versionCheckTask = versionCheckTask3;
					}
					versionCheckTask2 = versionCheckTask3;
				}
			}
			versionCheckTask?.Run();
		}

		public void Register(UserMod2 mod, IModVersionChecker checker)
		{
			Mod val = ((mod != null) ? mod.mod : null) ?? throw new ArgumentNullException("mod");
			if (checker == null)
			{
				throw new ArgumentNullException("checker");
			}
			RegisterForForwarding();
			string staticID = val.staticID;
			if (!checkVersions.TryGetValue(staticID, out var value))
			{
				checkVersions.Add(staticID, value = new VersionCheckMethods(val));
			}
			value.Methods.Add(checker);
		}

		private void ReportResults()
		{
			IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(base.ID);
			if (allComponents == null)
			{
				return;
			}
			ModOutdatedWarning instance = ModOutdatedWarning.Instance;
			resultsByMod.Clear();
			foreach (PForwardedComponent item in allComponents)
			{
				ICollection<ModVersionCheckResults> instanceDataSerialized = item.GetInstanceDataSerialized<ICollection<ModVersionCheckResults>>();
				if (instanceDataSerialized == null)
				{
					continue;
				}
				foreach (ModVersionCheckResults item2 in instanceDataSerialized)
				{
					string modChecked = item2.ModChecked;
					if (!resultsByMod.ContainsKey(modChecked))
					{
						resultsByMod[modChecked] = item2;
					}
				}
			}
			results.Clear();
			foreach (KeyValuePair<string, ModVersionCheckResults> item3 in resultsByMod)
			{
				results.Add(item3.Value);
			}
			if ((Object)(object)instance != (Object)null)
			{
				((MonoBehaviour)instance).StartCoroutine(instance.UpdateTextThreaded());
			}
		}

		internal void RunVersionCheck()
		{
			IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(base.ID);
			if (!PRegistry.GetData<bool>("PLib.VersionCheck.ModUpdaterActive") && allComponents != null)
			{
				new AllVersionCheckTask(allComponents, this).Run();
			}
		}
	}
	public sealed class SteamVersionChecker : IModVersionChecker
	{
		private static readonly Type PUBLISHED_FILE_ID = PPatchTools.GetTypeSafe("Steamworks.PublishedFileId_t");

		private static readonly Type STEAM_UGC = PPatchTools.GetTypeSafe("Steamworks.SteamUGC");

		private static readonly Type STEAM_UGC_SERVICE = PPatchTools.GetTypeSafe("SteamUGCService", "Assembly-CSharp");

		private static readonly MethodInfo FIND_MOD = STEAM_UGC_SERVICE?.GetMethodSafe("FindMod", false, PUBLISHED_FILE_ID);

		private static readonly MethodInfo GET_ITEM_INSTALL_INFO = STEAM_UGC?.GetMethodSafe("GetItemInstallInfo", true, PUBLISHED_FILE_ID, typeof(ulong).MakeByRefType(), typeof(string).MakeByRefType(), typeof(uint), typeof(uint).MakeByRefType());

		private static readonly ConstructorInfo NEW_PUBLISHED_FILE_ID = PUBLISHED_FILE_ID?.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(ulong) }, null);

		public const double UPDATE_JITTER = 10.0;

		private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public event PVersionCheck.OnVersionCheckComplete OnVersionCheckCompleted;

		private static ModVersionCheckResults CheckSteamInit(ulong id, object[] boxedID, Mod mod)
		{
			SteamUGCService instance = SteamUGCService.Instance;
			ModVersionCheckResults result = null;
			if ((Object)(object)instance != (Object)null)
			{
				object? obj = FIND_MOD.Invoke(instance, boxedID);
				Mod val = (Mod)((obj is Mod) ? obj : null);
				if (val != null)
				{
					ulong lastUpdateTime = val.lastUpdateTime;
					DateTime dateTime = ((lastUpdateTime == 0L) ? DateTime.MinValue : UnixEpochToDateTime(lastUpdateTime));
					bool flag = dateTime <= GetLocalLastModified(id).AddMinutes(10.0);
					result = new ModVersionCheckResults(mod.staticID, flag, flag ? null : dateTime.ToString("f"));
				}
			}
			return result;
		}

		private static DateTime GetLocalLastModified(ulong id)
		{
			DateTime result = DateTime.UtcNow;
			if (GET_ITEM_INSTALL_INFO != null)
			{
				object[] array = new object[5]
				{
					NEW_PUBLISHED_FILE_ID.Invoke(new object[1] { id }),
					0uL,
					"",
					260u,
					0u
				};
				object obj = GET_ITEM_INSTALL_INFO.Invoke(null, array);
				bool flag = default(bool);
				int num;
				if (obj is bool)
				{
					flag = (bool)obj;
					num = 1;
				}
				else
				{
					num = 0;
				}
				if (((uint)num & (flag ? 1u : 0u)) != 0 && array.Length == 5 && array[4] is uint num2 && num2 != 0)
				{
					result = UnixEpochToDateTime(num2);
				}
				else
				{
					PUtil.LogDebug("Unable to determine last modified date for: " + id);
				}
			}
			return result;
		}

		public static DateTime UnixEpochToDateTime(ulong timeSeconds)
		{
			return UNIX_EPOCH.AddSeconds(timeSeconds);
		}

		public bool CheckVersion(Mod mod)
		{
			if (FIND_MOD != null && NEW_PUBLISHED_FILE_ID != null)
			{
				return DoCheckVersion(mod);
			}
			return false;
		}

		private bool DoCheckVersion(Mod mod)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Invalid comparison between Unknown and I4
			bool result = false;
			if ((int)mod.label.distribution_platform == 1 && ulong.TryParse(mod.label.id, out var result2))
			{
				((MonoBehaviour)Global.Instance).StartCoroutine(WaitForSteamInit(result2, mod));
				result = true;
			}
			else
			{
				PUtil.LogWarning("SteamVersionChecker cannot check version for non-Steam mod {0}".F(mod.staticID));
			}
			return result;
		}

		private IEnumerator WaitForSteamInit(ulong id, Mod mod)
		{
			object[] boxedID = new object[1] { NEW_PUBLISHED_FILE_ID.Invoke(new object[1] { id }) };
			int timeout = 0;
			ModVersionCheckResults modVersionCheckResults;
			int num;
			do
			{
				yield return null;
				modVersionCheckResults = CheckSteamInit(id, boxedID, mod);
				if (modVersionCheckResults != null)
				{
					break;
				}
				num = timeout + 1;
				timeout = num;
			}
			while (num < 120);
			if (modVersionCheckResults == null)
			{
				PUtil.LogWarning("Unable to check version for mod {0} (SteamUGCService timeout)".F(mod.label.title));
			}
			this.OnVersionCheckCompleted?.Invoke(modVersionCheckResults);
		}
	}
	internal sealed class VersionCheckTask
	{
		private readonly IModVersionChecker method;

		private readonly Mod mod;

		private readonly ICollection<ModVersionCheckResults> results;

		internal Action Next { get; set; }

		internal VersionCheckTask(Mod mod, IModVersionChecker method, ICollection<ModVersionCheckResults> results)
		{
			this.mod = mod ?? throw new ArgumentNullException("mod");
			this.method = method ?? throw new ArgumentNullException("method");
			this.results = results ?? throw new ArgumentNullException("results");
			Next = null;
		}

		private void OnComplete(ModVersionCheckResults result)
		{
			method.OnVersionCheckCompleted -= OnComplete;
			if (result != null)
			{
				results.Add(result);
				if (!result.IsUpToDate)
				{
					PUtil.LogWarning("Mod {0} is out of date! New version: {1}".F(result.ModChecked, result.NewVersion ?? "unknown"));
				}
			}
			RunNext();
		}

		internal void Run()
		{
			bool flag = false;
			foreach (ModVersionCheckResults result in results)
			{
				if (result.ModChecked == mod.staticID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				method.OnVersionCheckCompleted += OnComplete;
				bool flag2;
				try
				{
					flag2 = method.CheckVersion(mod);
				}
				catch (Exception thrown)
				{
					PUtil.LogWarning("Unable to check version for mod " + mod.label.title + ":");
					PUtil.LogExcWarn(thrown);
					flag2 = false;
				}
				if (!flag2)
				{
					method.OnVersionCheckCompleted -= OnComplete;
					RunNext();
				}
			}
		}

		private void RunNext()
		{
			Next?.Invoke();
		}
	}
	public sealed class YamlURLVersionChecker : IModVersionChecker
	{
		public string YamlVersionURL { get; }

		public event PVersionCheck.OnVersionCheckComplete OnVersionCheckCompleted;

		public YamlURLVersionChecker(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				throw new ArgumentNullException("url");
			}
			YamlVersionURL = url;
		}

		public bool CheckVersion(Mod mod)
		{
			if (mod == null)
			{
				throw new ArgumentNullException("mod");
			}
			UnityWebRequest request = UnityWebRequest.Get(YamlVersionURL);
			request.SetRequestHeader("Content-Type", "application/x-yaml");
			request.SetRequestHeader("User-Agent", "PLib AVC");
			request.timeout = 8;
			((AsyncOperation)request.SendWebRequest()).completed += delegate
			{
				OnRequestFinished(request, mod);
			};
			return true;
		}

		private void OnRequestFinished(UnityWebRequest request, Mod mod)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Invalid comparison between Unknown and I4
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			ModVersionCheckResults result = null;
			if ((int)request.result == 1)
			{
				PackagedModInfo obj = YamlIO.Parse<PackagedModInfo>(request.downloadHandler.text, default(FileHandle), (ErrorHandler)null, (List<Tuple<string, Type>>)null);
				string text = ((obj != null) ? obj.version : null);
				if (obj != null && !string.IsNullOrEmpty(text))
				{
					string currentVersion = PVersionCheck.GetCurrentVersion(mod);
					result = new ModVersionCheckResults(mod.staticID, text == currentVersion, text);
				}
			}
			request.Dispose();
			this.OnVersionCheckCompleted?.Invoke(result);
		}
	}
}
namespace PeterHan.PLib.Buildings
{
	public class BuildIngredient
	{
		public string Material { get; }

		public float Quantity { get; }

		public BuildIngredient(string name, float quantity)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			if (quantity.IsNaNOrInfinity() || quantity <= 0f)
			{
				throw new ArgumentException("quantity");
			}
			Material = name;
			Quantity = quantity;
		}

		public BuildIngredient(string[] material, int tier)
			: this(material[0], tier)
		{
		}

		public BuildIngredient(string material, int tier)
		{
			if (string.IsNullOrEmpty(material))
			{
				throw new ArgumentNullException("material");
			}
			Material = material;
			Quantity = tier switch
			{
				-1 => CONSTRUCTION_MASS_KG.TIER_TINY[0], 
				0 => CONSTRUCTION_MASS_KG.TIER0[0], 
				1 => CONSTRUCTION_MASS_KG.TIER1[0], 
				2 => CONSTRUCTION_MASS_KG.TIER2[0], 
				3 => CONSTRUCTION_MASS_KG.TIER3[0], 
				4 => CONSTRUCTION_MASS_KG.TIER4[0], 
				5 => CONSTRUCTION_MASS_KG.TIER5[0], 
				6 => CONSTRUCTION_MASS_KG.TIER6[0], 
				7 => CONSTRUCTION_MASS_KG.TIER7[0], 
				_ => throw new ArgumentException("tier must be between -1 and 7 inclusive"), 
			};
		}

		public override bool Equals(object obj)
		{
			if (obj is BuildIngredient buildIngredient && buildIngredient.Material == Material)
			{
				return buildIngredient.Quantity == Quantity;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Material.GetHashCode();
		}

		public override string ToString()
		{
			return "Material[Tag={0},Quantity={1:F0}]".F(Material, Quantity);
		}
	}
	public abstract class ColoredRangeVisualizer : KMonoBehaviour
	{
		private delegate ulong RegisterCellChangedHandler(CellChangeMonitor instance, Transform transform, Action<object> callback);

		private delegate void UnregisterCellChangedHandler(CellChangeMonitor instance, ref ulong id);

		protected sealed class VisCellData : IComparable<VisCellData>
		{
			public int Cell { get; }

			public KBatchedAnimController Controller { get; private set; }

			public Color Tint { get; }

			public VisCellData(int cell)
				: this(cell, Color.white)
			{
			}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


			public VisCellData(int cell, Color tint)
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				Cell = cell;
				Controller = null;
				Tint = tint;
			}

			public int CompareTo(VisCellData other)
			{
				if (other == null)
				{
					throw new ArgumentNullException("other");
				}
				return Cell.CompareTo(other.Cell);
			}

			public void CreateController(SceneLayer sceneLayer)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0061: Unknown result type (might be due to invalid IL or missing references)
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				Controller = FXHelpers.CreateEffect("transferarmgrid_kanim", Grid.CellToPosCCC(Cell, sceneLayer), (Transform)null, false, sceneLayer, true);
				((KAnimControllerBase)Controller).destroyOnAnimComplete = false;
				((KAnimControllerBase)Controller).visibilityType = (VisibilityType)2;
				((Component)Controller).gameObject.SetActive(true);
				((KAnimControllerBase)Controller).Play(PRE_ANIMS, (PlayMode)0);
				((KAnimControllerBase)Controller).TintColour = Color32.op_Implicit(Tint);
			}

			public void Destroy()
			{
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)Controller != (Object)null)
				{
					((KAnimControllerBase)Controller).destroyOnAnimComplete = true;
					((KAnimControllerBase)Controller).Play(POST_ANIM, (PlayMode)1, 1f, 0f);
					Controller = null;
				}
			}

			public override bool Equals(object obj)
			{
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				if (obj is VisCellData visCellData && visCellData.Cell == Cell)
				{
					Color tint = Tint;
					return ((Color)(ref tint)).Equals(visCellData.Tint);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return Cell;
			}

			public override string ToString()
			{
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				return "CellData[cell={0:D},color={1}]".F(Cell, Tint);
			}
		}

		private static readonly DetouredMethod<RegisterCellChangedHandler> REGISTER = typeof(CellChangeMonitor).DetourLazy<RegisterCellChangedHandler>("RegisterCellChangedHandler");

		private static readonly DetouredMethod<UnregisterCellChangedHandler> UNREGISTER = typeof(CellChangeMonitor).DetourLazy<UnregisterCellChangedHandler>("UnregisterCellChangedHandler");

		private static readonly Action<object, object> ON_ROTATE = delegate(object context, object _)
		{
			if (context is ColoredRangeVisualizer coloredRangeVisualizer && (Object)(object)coloredRangeVisualizer != (Object)null)
			{
				coloredRangeVisualizer.CreateVisualizers();
			}
		};

		private static readonly Action<object, object> ON_SELECT = delegate(object context, object data)
		{
			if (context is ColoredRangeVisualizer coloredRangeVisualizer && (Object)(object)coloredRangeVisualizer != (Object)null)
			{
				coloredRangeVisualizer.OnSelect(data);
			}
		};

		private const string ANIM_NAME = "transferarmgrid_kanim";

		private static readonly HashedString[] PRE_ANIMS = (HashedString[])(object)new HashedString[2]
		{
			HashedString.op_Implicit("grid_pre"),
			HashedString.op_Implicit("grid_loop")
		};

		private static readonly HashedString POST_ANIM = HashedString.op_Implicit("grid_pst");

		[MyCmpGet]
		protected BuildingPreview preview;

		[MyCmpGet]
		protected Rotatable rotatable;

		private readonly HashSet<VisCellData> cells;

		private ulong handlerID;

		private int onSelectObject;

		private int onRotateObject;

		public SceneLayer Layer { get; set; }

		protected ColoredRangeVisualizer()
		{
			cells = new HashSet<VisCellData>();
			handlerID = 0uL;
			Layer = (SceneLayer)32;
			onRotateObject = 0;
			onSelectObject = 0;
		}

		private void CreateVisualizers()
		{
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			PooledHashSet<VisCellData, ColoredRangeVisualizer> val = HashSetPool<VisCellData, ColoredRangeVisualizer>.Allocate();
			PooledList<VisCellData, ColoredRangeVisualizer> val2 = ListPool<VisCellData, ColoredRangeVisualizer>.Allocate();
			try
			{
				if ((Object)(object)((Component)this).gameObject != (Object)null)
				{
					VisualizeCells((ICollection<VisCellData>)val);
				}
				foreach (VisCellData cell in cells)
				{
					if (((HashSet<VisCellData>)(object)val).Remove(cell))
					{
						((List<VisCellData>)(object)val2).Add(cell);
					}
					else
					{
						cell.Destroy();
					}
				}
				foreach (VisCellData item in (HashSet<VisCellData>)(object)val)
				{
					item.CreateController(Layer);
					((List<VisCellData>)(object)val2).Add(item);
				}
				cells.Clear();
				foreach (VisCellData item2 in (List<VisCellData>)(object)val2)
				{
					cells.Add(item2);
				}
			}
			finally
			{
				val.Recycle();
				val2.Recycle();
			}
		}

		private void OnCellChange(object _)
		{
			CreateVisualizers();
		}

		protected override void OnCleanUp()
		{
			((KMonoBehaviour)this).Unsubscribe(ref onSelectObject);
			if ((Object)(object)preview != (Object)null)
			{
				UNREGISTER?.Invoke(Singleton<CellChangeMonitor>.Instance, ref handlerID);
				if ((Object)(object)rotatable != (Object)null)
				{
					((KMonoBehaviour)this).Unsubscribe(ref onRotateObject);
				}
			}
			RemoveVisualizers();
			((KMonoBehaviour)this).OnCleanUp();
		}

		private void OnSelect(object data)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (data is Boxed<bool> val)
			{
				Vector3 position = ((KMonoBehaviour)this).transform.position;
				if (val.value)
				{
					PGameUtils.PlaySound("RadialGrid_form", position);
					CreateVisualizers();
				}
				else
				{
					PGameUtils.PlaySound("RadialGrid_disappear", position);
					RemoveVisualizers();
				}
			}
		}

		protected override void OnSpawn()
		{
			((KMonoBehaviour)this).OnSpawn();
			onSelectObject = ((KMonoBehaviour)this).Subscribe(-1503271301, ON_SELECT, (object)this);
			if ((Object)(object)preview != (Object)null)
			{
				if (REGISTER != null)
				{
					handlerID = REGISTER.Invoke(Singleton<CellChangeMonitor>.Instance, ((KMonoBehaviour)this).transform, OnCellChange);
				}
				if ((Object)(object)rotatable != (Object)null)
				{
					onRotateObject = ((KMonoBehaviour)this).Subscribe(-1643076535, ON_ROTATE, (object)this);
				}
			}
			else
			{
				handlerID = 0uL;
			}
		}

		private void RemoveVisualizers()
		{
			foreach (VisCellData cell in cells)
			{
				cell.Destroy();
			}
			cells.Clear();
		}

		protected int RotateOffsetCell(int baseCell, CellOffset offset)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)rotatable != (Object)null)
			{
				offset = rotatable.GetRotatedCellOffset(offset);
			}
			return Grid.OffsetCell(baseCell, offset);
		}

		protected abstract void VisualizeCells(ICollection<VisCellData> newCells);
	}
	public class ConduitConnection
	{
		public CellOffset Location { get; }

		public ConduitType Type { get; }

		public ConduitConnection(ConduitType type, CellOffset location)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			Location = location;
			Type = type;
		}

		public override string ToString()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return $"Connection[Type={Type},Location={Location}]";
		}
	}
	public sealed class PBuilding
	{
		private bool addedPlan;

		private bool addedStrings;

		private bool addedTech;

		private static readonly HashedString DEFAULT_CATEGORY = new HashedString("Base");

		public string AddAfter { get; set; }

		public bool AlwaysOperational { get; set; }

		public string Animation { get; set; }

		public string AudioCategory { get; set; }

		public string AudioSize { get; set; }

		public bool Breaks { get; set; }

		public HashedString Category { get; set; }

		public float ConstructionTime { get; set; }

		public EffectorValues Decor { get; set; }

		public string Description { get; set; }

		public string EffectText { get; set; }

		public bool Entombs { get; set; }

		public float ExhaustHeatGeneration { get; set; }

		public bool Floods { get; set; }

		public int? DefaultPriority { get; set; }

		public float HeatGeneration { get; set; }

		public int Height { get; set; }

		public int HP { get; set; }

		public IList<BuildIngredient> Ingredients { get; }

		public string ID { get; }

		public bool IndustrialMachine { get; set; }

		public IList<ConduitConnection> InputConduits { get; }

		public bool IsSolidTile { get; set; }

		public IList<Port> LogicIO { get; }

		public string Name { get; private set; }

		public EffectorValues Noise { get; set; }

		public ObjectLayer ObjectLayer { get; set; }

		public IList<ConduitConnection> OutputConduits { get; }

		public float? OverheatTemperature { get; set; }

		public BuildLocationRule Placement { get; set; }

		public PowerRequirement PowerInput { get; set; }

		public PowerRequirement PowerOutput { get; set; }

		public PermittedRotations RotateMode { get; set; }

		public SceneLayer SceneLayer { get; set; }

		public string SubCategory { get; set; }

		public string Tech { get; set; }

		public HashedString ViewMode { get; set; }

		public int Width { get; set; }

		public PBuilding(string id, string name)
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			AddAfter = null;
			AlwaysOperational = false;
			Animation = "";
			AudioCategory = "Metal";
			AudioSize = "medium";
			Breaks = true;
			Category = DEFAULT_CATEGORY;
			ConstructionTime = 10f;
			Decor = DECOR.NONE;
			DefaultPriority = null;
			Description = "Default Building Description";
			EffectText = "Default Building Effect";
			Entombs = true;
			ExhaustHeatGeneration = 0f;
			Floods = true;
			HeatGeneration = 0f;
			Height = 1;
			Ingredients = new List<BuildIngredient>(4);
			IndustrialMachine = false;
			InputConduits = new List<ConduitConnection>(4);
			HP = 100;
			ID = id;
			LogicIO = new List<Port>(4);
			Name = name;
			Noise = NOISE_POLLUTION.NONE;
			ObjectLayer = PGameUtils.GetObjectLayer("Building", (ObjectLayer)1);
			OutputConduits = new List<ConduitConnection>(4);
			OverheatTemperature = null;
			Placement = (BuildLocationRule)1;
			PowerInput = null;
			PowerOutput = null;
			RotateMode = (PermittedRotations)0;
			SceneLayer = (SceneLayer)19;
			SubCategory = "default";
			Tech = null;
			ViewMode = None.ID;
			Width = 1;
			addedPlan = false;
			addedStrings = false;
			addedTech = false;
		}

		public BuildingDef CreateDef()
		{
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0246: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_0287: Unknown result type (might be due to invalid IL or missing references)
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0293: Unknown result type (might be due to invalid IL or missing references)
			//IL_029a: Unknown result type (might be due to invalid IL or missing references)
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_033a: Unknown result type (might be due to invalid IL or missing references)
			//IL_033f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0347: Unknown result type (might be due to invalid IL or missing references)
			//IL_034c: Unknown result type (might be due to invalid IL or missing references)
			if (Width < 1)
			{
				throw new InvalidOperationException("Building width: " + Width);
			}
			if (Height < 1)
			{
				throw new InvalidOperationException("Building height: " + Height);
			}
			if (HP < 1)
			{
				throw new InvalidOperationException("Building HP: " + HP);
			}
			if (ConstructionTime.IsNaNOrInfinity())
			{
				throw new InvalidOperationException("Construction time: " + ConstructionTime);
			}
			int count = Ingredients.Count;
			if (count < 1)
			{
				throw new InvalidOperationException("No ingredients for build");
			}
			float[] array = new float[count];
			string[] array2 = new string[count];
			for (int i = 0; i < count; i++)
			{
				BuildIngredient buildIngredient = Ingredients[i];
				if (buildIngredient == null)
				{
					throw new ArgumentNullException("ingredient");
				}
				array[i] = buildIngredient.Quantity;
				array2[i] = buildIngredient.Material;
			}
			BuildingDef val = BuildingTemplates.CreateBuildingDef(ID, Width, Height, Animation, HP, Math.Max(0.1f, ConstructionTime), array, array2, 2400f, Placement, Decor, Noise, 0.2f);
			if (IsSolidTile)
			{
				val.BaseTimeUntilRepair = -1f;
				val.UseStructureTemperature = false;
				BuildingTemplates.CreateFoundationTileDef(val);
			}
			val.AudioCategory = AudioCategory;
			val.AudioSize = AudioSize;
			if (OverheatTemperature.HasValue)
			{
				val.Overheatable = true;
				val.OverheatTemperature = OverheatTemperature ?? 348.15f;
			}
			else
			{
				val.Overheatable = false;
			}
			if (PowerInput != null)
			{
				val.RequiresPowerInput = true;
				val.EnergyConsumptionWhenActive = PowerInput.MaxWattage;
				val.PowerInputOffset = PowerInput.PlugLocation;
			}
			if (PowerOutput != null)
			{
				val.RequiresPowerOutput = true;
				val.GeneratorWattageRating = PowerOutput.MaxWattage;
				val.PowerOutputOffset = PowerOutput.PlugLocation;
			}
			val.Breakable = Breaks;
			val.PermittedRotations = RotateMode;
			val.ExhaustKilowattsWhenActive = ExhaustHeatGeneration;
			val.SelfHeatKilowattsWhenActive = HeatGeneration;
			val.Floodable = Floods;
			val.Entombable = Entombs;
			val.ObjectLayer = ObjectLayer;
			val.SceneLayer = SceneLayer;
			val.ViewMode = ViewMode;
			if (InputConduits.Count > 1)
			{
				throw new InvalidOperationException("Only supports one input conduit");
			}
			foreach (ConduitConnection inputConduit in InputConduits)
			{
				val.UtilityInputOffset = inputConduit.Location;
				val.InputConduitType = inputConduit.Type;
			}
			if (OutputConduits.Count > 1)
			{
				throw new InvalidOperationException("Only supports one output conduit");
			}
			foreach (ConduitConnection outputConduit in OutputConduits)
			{
				val.UtilityOutputOffset = outputConduit.Location;
				val.OutputConduitType = outputConduit.Type;
			}
			BUILDINGS.PLANSUBCATEGORYSORTING[ID] = SubCategory;
			return val;
		}

		public void ConfigureBuildingTemplate(GameObject go)
		{
			if (AlwaysOperational)
			{
				ApplyAlwaysOperational(go);
			}
		}

		public void CreateLogicPorts(GameObject go)
		{
			SplitLogicPorts(go);
		}

		public void DoPostConfigureComplete(GameObject go)
		{
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			if (InputConduits.Count == 1)
			{
				ConduitConsumer val = EntityTemplateExtensions.AddOrGet<ConduitConsumer>(go);
				foreach (ConduitConnection inputConduit in InputConduits)
				{
					val.alwaysConsume = true;
					val.conduitType = inputConduit.Type;
					val.wrongElementResult = (WrongElementResult)2;
				}
			}
			if (OutputConduits.Count == 1)
			{
				ConduitDispenser val2 = EntityTemplateExtensions.AddOrGet<ConduitDispenser>(go);
				foreach (ConduitConnection outputConduit in OutputConduits)
				{
					val2.alwaysDispense = true;
					val2.conduitType = outputConduit.Type;
					val2.elementFilter = null;
				}
			}
			KPrefabID val3 = default(KPrefabID);
			if (IndustrialMachine && go.TryGetComponent<KPrefabID>(ref val3))
			{
				val3.AddTag(ConstraintTags.IndustrialMachinery, false);
			}
			if (PowerInput != null)
			{
				EntityTemplateExtensions.AddOrGet<EnergyConsumer>(go);
			}
			if (PowerOutput != null)
			{
				EntityTemplateExtensions.AddOrGet<EnergyGenerator>(go);
			}
			Prioritizable val4 = default(Prioritizable);
			if (DefaultPriority.HasValue && go.TryGetComponent<Prioritizable>(ref val4))
			{
				Prioritizable.AddRef(go);
				val4.SetMasterPriority(new PrioritySetting((PriorityClass)0, DefaultPriority ?? 5));
			}
		}

		public override string ToString()
		{
			return "PBuilding[ID={0}]".F(ID);
		}

		private static void ApplyAlwaysOperational(GameObject go)
		{
			BuildingEnabledButton val = default(BuildingEnabledButton);
			if (go.TryGetComponent<BuildingEnabledButton>(ref val))
			{
				Object.DestroyImmediate((Object)(object)val);
			}
			Operational val2 = default(Operational);
			if (go.TryGetComponent<Operational>(ref val2))
			{
				Object.DestroyImmediate((Object)(object)val2);
			}
			LogicPorts val3 = default(LogicPorts);
			if (go.TryGetComponent<LogicPorts>(ref val3))
			{
				Object.DestroyImmediate((Object)(object)val3);
			}
		}

		public static Port CompatLogicPort(LogicPortSpriteType type, CellOffset offset)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			return new Port(LogicOperationalController.PORT_ID, offset, LocString.op_Implicit(LOGIC_PORTS.CONTROL_OPERATIONAL), LocString.op_Implicit(LOGIC_PORTS.CONTROL_OPERATIONAL_ACTIVE), LocString.op_Implicit(LOGIC_PORTS.CONTROL_OPERATIONAL_INACTIVE), false, type, false);
		}

		public unsafe void AddPlan()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			if (addedPlan)
			{
				return;
			}
			HashedString category = Category;
			if (!((HashedString)(ref category)).IsValid)
			{
				return;
			}
			bool flag = false;
			foreach (PlanInfo item in BUILDINGS.PLANORDER)
			{
				if (item.category == Category)
				{
					AddPlanToCategory(item);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				category = Category;
				PUtil.LogWarning("Unable to find build menu: " + ((object)(*(HashedString*)(&category))/*cast due to .constrained prefix*/).ToString());
			}
			addedPlan = true;
		}

		private void AddPlanToCategory(PlanInfo menu)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			List<KeyValuePair<string, string>> buildingAndSubcategoryData = menu.buildingAndSubcategoryData;
			if (buildingAndSubcategoryData != null)
			{
				string addAfter = AddAfter;
				bool flag = false;
				if (addAfter != null)
				{
					int count = buildingAndSubcategoryData.Count;
					for (int i = 0; i < count - 1; i++)
					{
						if (flag)
						{
							break;
						}
						if (buildingAndSubcategoryData[i].Key == addAfter)
						{
							buildingAndSubcategoryData.Insert(i + 1, new KeyValuePair<string, string>(ID, SubCategory));
							flag = true;
						}
					}
				}
				if (!flag)
				{
					buildingAndSubcategoryData.Add(new KeyValuePair<string, string>(ID, SubCategory));
				}
			}
			else
			{
				PUtil.LogWarning("Build menu " + ((object)Category/*cast due to .constrained prefix*/).ToString() + " has invalid entries!");
			}
		}

		public void AddStrings()
		{
			if (!addedStrings)
			{
				string text = "STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".";
				string text2 = text + "NAME";
				StringEntry val = default(StringEntry);
				if (Strings.TryGet(text2, ref val))
				{
					Name = val.String;
				}
				else
				{
					Strings.Add(new string[2] { text2, Name });
				}
				if (Description != null)
				{
					Strings.Add(new string[2]
					{
						text + "DESC",
						Description
					});
				}
				if (EffectText != null)
				{
					Strings.Add(new string[2]
					{
						text + "EFFECT",
						EffectText
					});
				}
				addedStrings = true;
			}
		}

		public void AddTech()
		{
			if (!addedTech && Tech != null)
			{
				(((ResourceSet<Tech>)(object)Db.Get().Techs)?.TryGet(Tech))?.unlockedItemIDs?.Add(ID);
				addedTech = true;
			}
		}

		private void SplitLogicPorts(GameObject go)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Invalid comparison between Unknown and I4
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			int count = LogicIO.Count;
			List<Port> list = new List<Port>(count);
			List<Port> list2 = new List<Port>(count);
			foreach (Port item in LogicIO)
			{
				if ((int)item.spriteType == 1)
				{
					list2.Add(item);
				}
				else
				{
					list.Add(item);
				}
			}
			LogicPorts val = EntityTemplateExtensions.AddOrGet<LogicPorts>(go);
			if (list.Count > 0)
			{
				val.inputPortInfo = list.ToArray();
			}
			if (list2.Count > 0)
			{
				val.outputPortInfo = list2.ToArray();
			}
		}
	}
	public sealed class PBuildingManager : PForwardedComponent
	{
		private sealed class BuildingTechRegistration : IPatchMethodInstance
		{
			public void Run(Harmony instance)
			{
				Instance?.AddAllTechs();
			}
		}

		internal static readonly Version VERSION = new Version("4.24.0.0");

		private readonly ICollection<PBuilding> buildings;

		internal static PBuildingManager Instance { get; private set; }

		public override Version Version => VERSION;

		public static void AddExistingBuildingToTech(string tech, string id)
		{
			if (string.IsNullOrEmpty(tech))
			{
				throw new ArgumentNullException("tech");
			}
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}
			(((ResourceSet<Tech>)(object)Db.Get().Techs)?.TryGet(id))?.unlockedItemIDs?.Add(tech);
		}

		private static void CreateBuildingDef_Postfix(BuildingDef __result, string anim, string id)
		{
			KAnimFile[] array = __result?.AnimFiles;
			if (array != null && array.Length != 0 && (Object)(object)array[0] == (Object)null)
			{
				Debug.LogWarningFormat("(when looking for KAnim named {0} on building {1})", new object[2] { anim, id });
			}
		}

		private static void CreateEquipmentDef_Postfix(EquipmentDef __result, string Anim, string Id)
		{
			if ((Object)(object)__result?.Anim == (Object)null)
			{
				Debug.LogWarningFormat("(when looking for KAnim named {0} on equipment {1})", new object[2] { Anim, Id });
			}
		}

		internal static void LogBuildingDebug(string message)
		{
			Debug.LogFormat("[PLibBuildings] {0}", new object[1] { message });
		}

		private static void LoadGeneratedBuildings_Prefix()
		{
			Instance?.AddAllStrings();
		}

		public PBuildingManager()
		{
			buildings = new List<PBuilding>(16);
		}

		private void AddAllStrings()
		{
			InvokeAllProcess(0u, null);
		}

		private void AddStrings()
		{
			int count = buildings.Count;
			if (count <= 0)
			{
				return;
			}
			LogBuildingDebug("Register strings for {0:D} building(s) from {1}".F(count, Assembly.GetExecutingAssembly().GetNameSafe() ?? "?"));
			foreach (PBuilding building in buildings)
			{
				if (building != null)
				{
					building.AddStrings();
					building.AddPlan();
				}
			}
		}

		private void AddAllTechs()
		{
			InvokeAllProcess(1u, null);
		}

		private void AddTechs()
		{
			int count = buildings.Count;
			if (count <= 0)
			{
				return;
			}
			LogBuildingDebug("Register techs for {0:D} building(s) from {1}".F(count, Assembly.GetExecutingAssembly().GetNameSafe() ?? "?"));
			foreach (PBuilding building in buildings)
			{
				building?.AddTech();
			}
		}

		public void Register(PBuilding building)
		{
			if (building == null)
			{
				throw new ArgumentNullException("building");
			}
			RegisterForForwarding();
			buildings.Add(building);
		}

		public override void Initialize(Harmony plibInstance)
		{
			Instance = this;
			try
			{
				plibInstance.Patch(typeof(BuildingTemplates), "CreateBuildingDef", null, PatchMethod("CreateBuildingDef_Postfix"));
				plibInstance.Patch(typeof(EquipmentTemplates), "CreateEquipmentDef", null, PatchMethod("CreateEquipmentDef_Postfix"));
			}
			catch (Exception)
			{
			}
			plibInstance.Patch(typeof(GeneratedBuildings), "LoadGeneratedBuildings", PatchMethod("LoadGeneratedBuildings_Prefix"));
			new PPatchManager(plibInstance).RegisterPatch(3u, new BuildingTechRegistration());
		}

		public override void Process(uint operation, object _)
		{
			switch (operation)
			{
			case 0u:
				AddStrings();
				break;
			case 1u:
				AddTechs();
				break;
			}
		}
	}
	public class PowerRequirement
	{
		public float MaxWattage { get; }

		public CellOffset PlugLocation { get; }

		public PowerRequirement(float wattage, CellOffset plugLocation)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			if (wattage.IsNaNOrInfinity() || wattage < 0f)
			{
				throw new ArgumentException("wattage");
			}
			MaxWattage = wattage;
			PlugLocation = plugLocation;
		}

		public override string ToString()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			return "Power[Watts={0:F0},Location={1}]".F(MaxWattage, PlugLocation);
		}
	}
}
namespace PeterHan.PLib
{
	public static class PVersion
	{
		public const string VERSION = "4.24.0.0";
	}
}
namespace PeterHan.PLib.UI
{
	internal static class TextMeshProPatcher
	{
		private const string HARMONY_ID = "TextMeshProPatch";

		private static readonly IDetouredField<TMP_Text, bool> RECT_MASK_FIX = PDetours.TryDetourField<TMP_Text, bool>("ignoreRectMaskCulling");

		private static volatile bool patchChecked = false;

		private static readonly object patchLock = new object();

		private static bool AssignPositioningIfNeeded_Prefix(TMP_InputField __instance, RectTransform ___caretRectTrans, TMP_Text ___m_TextComponent)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			bool result = true;
			try
			{
				if ((Object)(object)___m_TextComponent != (Object)null && (Object)(object)___caretRectTrans != (Object)null && (Object)(object)__instance != (Object)null && ((Behaviour)___m_TextComponent).isActiveAndEnabled)
				{
					RectTransform rectTransform = ___m_TextComponent.rectTransform;
					if (((Transform)___caretRectTrans).localPosition != ((Transform)rectTransform).localPosition || ((Transform)___caretRectTrans).localRotation != ((Transform)rectTransform).localRotation || ((Transform)___caretRectTrans).localScale != ((Transform)rectTransform).localScale || ___caretRectTrans.anchorMin != rectTransform.anchorMin || ___caretRectTrans.anchorMax != rectTransform.anchorMax || ___caretRectTrans.anchoredPosition != rectTransform.anchoredPosition || ___caretRectTrans.sizeDelta != rectTransform.sizeDelta || ___caretRectTrans.pivot != rectTransform.pivot)
					{
						((MonoBehaviour)__instance).StartCoroutine(ResizeCaret(___caretRectTrans, rectTransform));
						result = false;
					}
				}
			}
			catch (Exception thrown)
			{
				PUtil.LogExcWarn(thrown);
			}
			return result;
		}

		private static bool HasOurPatch(IEnumerable<Patch> patchList)
		{
			bool result = false;
			if (patchList != null)
			{
				foreach (Patch patch in patchList)
				{
					if (patch.PatchMethod?.DeclaringType?.Name == "TextMeshProPatcher")
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		private static void InputFieldPatches(Type tmpType)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected O, but got Unknown
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Expected O, but got Unknown
			Harmony val = new Harmony("TextMeshProPatch");
			MethodInfo methodSafe = tmpType.GetMethodSafe("AssignPositioningIfNeeded", isStatic: false, PPatchTools.AnyArguments);
			if (methodSafe != null && !HasOurPatch(Harmony.GetPatchInfo((MethodBase)methodSafe)?.Prefixes))
			{
				val.Patch((MethodBase)methodSafe, new HarmonyMethod(typeof(TextMeshProPatcher), "AssignPositioningIfNeeded_Prefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
			if (RECT_MASK_FIX != null)
			{
				MethodInfo methodSafe2 = tmpType.GetMethodSafe("OnEnable", isStatic: false, PPatchTools.AnyArguments);
				if (methodSafe2 != null && !HasOurPatch(Harmony.GetPatchInfo((MethodBase)methodSafe2)?.Postfixes))
				{
					val.Patch((MethodBase)methodSafe2, (HarmonyMethod)null, new HarmonyMethod(typeof(TextMeshProPatcher), "OnEnable_Postfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
				}
			}
		}

		private static void OnEnable_Postfix(Scrollbar ___m_VerticalScrollbar, TMP_Text ___m_TextComponent)
		{
			if ((Object)(object)___m_TextComponent != (Object)null)
			{
				RECT_MASK_FIX?.Set(___m_TextComponent, (Object)(object)___m_VerticalScrollbar != (Object)null);
			}
		}

		public static void Patch()
		{
			lock (patchLock)
			{
				if (patchChecked)
				{
					return;
				}
				Type typeSafe = PPatchTools.GetTypeSafe("TMPro.TMP_InputField");
				if (typeSafe != null)
				{
					try
					{
						InputFieldPatches(typeSafe);
					}
					catch (Exception)
					{
						PUtil.LogWarning("Unable to patch TextMeshPro bug, text fields may display improperly inside scroll areas");
					}
				}
				patchChecked = true;
			}
		}

		private static IEnumerator ResizeCaret(RectTransform caretTransform, RectTransform textTransform)
		{
			yield return (object)new WaitForEndOfFrame();
			((Transform)caretTransform).localPosition = ((Transform)textTransform).localPosition;
			((Transform)caretTransform).localRotation = ((Transform)textTransform).localRotation;
			((Transform)caretTransform).localScale = ((Transform)textTransform).localScale;
			caretTransform.anchorMin = textTransform.anchorMin;
			caretTransform.anchorMax = textTransform.anchorMax;
			caretTransform.anchoredPosition = textTransform.anchoredPosition;
			caretTransform.sizeDelta = textTransform.sizeDelta;
			caretTransform.pivot = textTransform.pivot;
		}
	}
}
namespace PeterHan.PLib.PatchManager
{
	public interface IPatchMethodInstance
	{
		void Run(Harmony instance);
	}
	internal interface IPLibAnnotation
	{
		uint Runtime { get; }

		IPatchMethodInstance CreateInstance(MethodInfo method);
	}
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class PLibMethodAttribute : Attribute, IPLibAnnotation
	{
		public string RequireAssembly { get; set; }

		public string RequireType { get; set; }

		public uint Runtime { get; }

		public PLibMethodAttribute(uint runtime)
		{
			Runtime = runtime;
		}

		public IPatchMethodInstance CreateInstance(MethodInfo method)
		{
			return new PLibMethodInstance(this, method);
		}

		public override string ToString()
		{
			return "PLibMethod[RunAt={0}]".F(RunAt.ToString(Runtime));
		}
	}
	internal sealed class PLibMethodInstance : IPatchMethodInstance
	{
		public PLibMethodAttribute Descriptor { get; }

		public MethodInfo Method { get; }

		public PLibMethodInstance(PLibMethodAttribute attribute, MethodInfo method)
		{
			Descriptor = attribute ?? throw new ArgumentNullException("attribute");
			Method = method ?? throw new ArgumentNullException("method");
		}

		public void Run(Harmony instance)
		{
			if (!PPatchManager.CheckConditions(Descriptor.RequireAssembly, Descriptor.RequireType, out var requiredType))
			{
				return;
			}
			Type[] parameterTypes = Method.GetParameterTypes();
			int num = parameterTypes.Length;
			if (num <= 0)
			{
				Method.Invoke(null, null);
			}
			else if (parameterTypes[0] == typeof(Harmony))
			{
				switch (num)
				{
				case 1:
					Method.Invoke(null, new object[1] { instance });
					break;
				case 2:
					if (parameterTypes[1] == typeof(Type))
					{
						Method.Invoke(null, new object[2] { instance, requiredType });
					}
					break;
				}
			}
			else
			{
				PUtil.LogWarning("Invalid signature for PLibMethod - must have (), (Harmony), or (Harmony, Type)");
			}
		}
	}
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class PLibPatchAttribute : Attribute, IPLibAnnotation
	{
		public Type[] ArgumentTypes { get; set; }

		public bool IgnoreOnFail { get; set; }

		public string MethodName { get; }

		public HarmonyPatchType PatchType { get; set; }

		public string RequireAssembly { get; set; }

		public string RequireType { get; set; }

		public uint Runtime { get; }

		public Type TargetType { get; }

		public PLibPatchAttribute(uint runtime, Type target, string method)
		{
			ArgumentTypes = null;
			IgnoreOnFail = false;
			MethodName = method;
			PatchType = (HarmonyPatchType)0;
			Runtime = runtime;
			TargetType = target ?? throw new ArgumentNullException("target");
		}

		public PLibPatchAttribute(uint runtime, Type target, string method, params Type[] argTypes)
		{
			ArgumentTypes = argTypes;
			IgnoreOnFail = false;
			MethodName = method;
			PatchType = (HarmonyPatchType)0;
			Runtime = runtime;
			TargetType = target ?? throw new ArgumentNullException("target");
		}

		public PLibPatchAttribute(uint runtime, string method)
		{
			ArgumentTypes = null;
			IgnoreOnFail = false;
			MethodName = method;
			PatchType = (HarmonyPatchType)0;
			Runtime = runtime;
			TargetType = null;
		}

		public PLibPatchAttribute(uint runtime, string method, params Type[] argTypes)
		{
			ArgumentTypes = argTypes;
			IgnoreOnFail = false;
			MethodName = method;
			PatchType = (HarmonyPatchType)0;
			Runtime = runtime;
			TargetType = null;
		}

		public IPatchMethodInstance CreateInstance(MethodInfo method)
		{
			return new PLibPatchInstance(this, method);
		}

		public override string ToString()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			return "PLibPatch[RunAt={0},PatchType={1},MethodName={2}]".F(RunAt.ToString(Runtime), PatchType, MethodName);
		}
	}
	internal sealed class PLibPatchInstance : IPatchMethodInstance
	{
		public PLibPatchAttribute Descriptor { get; }

		public MethodInfo Method { get; }

		public PLibPatchInstance(PLibPatchAttribute attribute, MethodInfo method)
		{
			Descriptor = attribute ?? throw new ArgumentNullException("attribute");
			Method = method ?? throw new ArgumentNullException("method");
		}

		private unsafe HarmonyPatchType GetPatchType()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			HarmonyPatchType val = Descriptor.PatchType;
			if ((int)val == 0)
			{
				string name = Method.Name;
				foreach (object? value in Enum.GetValues(typeof(HarmonyPatchType)))
				{
					if (value is HarmonyPatchType val2 && val2 != val && name.EndsWith(((object)(*(HarmonyPatchType*)(&val2))/*cast due to .constrained prefix*/).ToString(), StringComparison.Ordinal))
					{
						val = val2;
						break;
					}
				}
			}
			return val;
		}

		private MethodBase GetTargetConstructor(Type targetType, Type[] argumentTypes)
		{
			if (argumentTypes == null)
			{
				ConstructorInfo[] constructors = targetType.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (constructors == null || constructors.Length != 1)
				{
					throw new InvalidOperationException("No constructor for {0} found".F(targetType.FullName));
				}
				return constructors[0];
			}
			return targetType.GetConstructor(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, argumentTypes, null);
		}

		private MethodBase GetTargetMethod(Type requiredType)
		{
			Type type = Descriptor.TargetType;
			Type[] argumentTypes = Descriptor.ArgumentTypes;
			string methodName = Descriptor.MethodName;
			if (type == null)
			{
				type = requiredType;
			}
			if (type == null)
			{
				throw new InvalidOperationException("No type specified to patch");
			}
			MethodBase methodBase = ((!string.IsNullOrEmpty(methodName) && !(methodName == ".ctor")) ? ((argumentTypes == null) ? type.GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : type.GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, argumentTypes, null)) : GetTargetConstructor(type, argumentTypes));
			if (methodBase == null)
			{
				throw new InvalidOperationException("Method {0}.{1} not found".F(type.FullName, methodName));
			}
			return methodBase;
		}

		private bool LogIgnoreOnFail(Exception e)
		{
			bool ignoreOnFail = Descriptor.IgnoreOnFail;
			if (ignoreOnFail)
			{
				PUtil.LogDebug("Patch for {0} not applied: {1}".F(Descriptor.MethodName, e.Message));
			}
			return ignoreOnFail;
		}

		public void Run(Harmony instance)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected I4, but got Unknown
			if (!PPatchManager.CheckConditions(Descriptor.RequireAssembly, Descriptor.RequireType, out var requiredType))
			{
				return;
			}
			HarmonyMethod val = new HarmonyMethod(Method);
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			try
			{
				MethodBase targetMethod = GetTargetMethod(requiredType);
				HarmonyPatchType patchType = GetPatchType();
				switch (patchType - 1)
				{
				case 1:
					instance.Patch(targetMethod, (HarmonyMethod)null, val, (HarmonyMethod)null, (HarmonyMethod)null);
					break;
				case 0:
					instance.Patch(targetMethod, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					break;
				case 2:
					instance.Patch(targetMethod, (HarmonyMethod)null, (HarmonyMethod)null, val, (HarmonyMethod)null);
					break;
				default:
					throw new ArgumentOutOfRangeException("HarmonyPatchType");
				}
			}
			catch (AmbiguousMatchException e)
			{
				if (!LogIgnoreOnFail(e))
				{
					throw;
				}
			}
			catch (InvalidOperationException e2)
			{
				if (!LogIgnoreOnFail(e2))
				{
					throw;
				}
			}
		}
	}
	public sealed class PPatchManager : PForwardedComponent
	{
		internal const BindingFlags FLAGS = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;

		internal const BindingFlags FLAGS_EITHER = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		internal static readonly Version VERSION = new Version("4.24.0.0");

		private static volatile bool afterModsLoaded = false;

		private readonly Harmony harmony;

		private readonly IDictionary<uint, ICollection<IPatchMethodInstance>> patches;

		internal static PPatchManager Instance { get; private set; }

		public override Version Version => VERSION;

		private static void Game_DestroyInstances_Postfix()
		{
			Instance?.InvokeAllProcess(6u, null);
		}

		private static void Game_OnPrefabInit_Postfix()
		{
			Instance?.InvokeAllProcess(5u, null);
		}

		private static void Initialize_Prefix()
		{
			Instance?.InvokeAllProcess(2u, null);
		}

		private static void Initialize_Postfix()
		{
			Instance?.InvokeAllProcess(3u, null);
		}

		private static void PostProcess_Prefix()
		{
			Instance?.InvokeAllProcess(8u, null);
		}

		private static void PostProcess_Postfix()
		{
			Instance?.InvokeAllProcess(9u, null);
		}

		private static void Instance_Postfix()
		{
			bool flag = false;
			if (Instance != null)
			{
				lock (VERSION)
				{
					if (!afterModsLoaded)
					{
						flag = (afterModsLoaded = true);
					}
				}
			}
			if (flag)
			{
				Instance.InvokeAllProcess(7u, null);
			}
		}

		private static void MainMenu_OnSpawn_Postfix()
		{
			Instance?.InvokeAllProcess(4u, null);
		}

		private static void DetailsScreen_OnPrefabInit_Postfix()
		{
			Instance?.InvokeAllProcess(10u, null);
		}

		internal static bool CheckConditions(string assemblyName, string typeName, out Type requiredType)
		{
			bool result = false;
			bool flag = string.IsNullOrEmpty(typeName);
			if (string.IsNullOrEmpty(assemblyName))
			{
				if (flag)
				{
					requiredType = null;
					result = true;
				}
				else
				{
					requiredType = PPatchTools.GetTypeSafe(typeName);
					result = requiredType != null;
				}
			}
			else if (flag)
			{
				requiredType = null;
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int i = 0; i < assemblies.Length; i++)
				{
					if (assemblies[i].GetName().Name == assemblyName)
					{
						result = true;
						break;
					}
				}
			}
			else
			{
				requiredType = PPatchTools.GetTypeSafe(typeName, assemblyName);
				result = requiredType != null;
			}
			return result;
		}

		public PPatchManager(Harmony harmony)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			if (harmony == null)
			{
				PUtil.LogWarning("Use the Harmony instance from OnLoad to create PPatchManager");
				harmony = new Harmony("PLib.PostLoad." + Assembly.GetExecutingAssembly().GetNameSafe());
			}
			this.harmony = harmony;
			patches = new Dictionary<uint, ICollection<IPatchMethodInstance>>(11);
			InstanceData = patches;
		}

		private void AddHandler(uint when, IPatchMethodInstance instance)
		{
			if (!patches.TryGetValue(when, out var value))
			{
				patches.Add(when, value = new List<IPatchMethodInstance>(16));
			}
			value.Add(instance);
		}

		public override void Initialize(Harmony plibInstance)
		{
			Instance = this;
			plibInstance.Patch(typeof(Db), "Initialize", PatchMethod("Initialize_Prefix"), PatchMethod("Initialize_Postfix"));
			plibInstance.Patch(typeof(Db), "PostProcess", PatchMethod("PostProcess_Prefix"), PatchMethod("PostProcess_Postfix"));
			plibInstance.Patch(typeof(Game), "DestroyInstances", null, PatchMethod("Game_DestroyInstances_Postfix"));
			plibInstance.Patch(typeof(Game), "OnPrefabInit", null, PatchMethod("Game_OnPrefabInit_Postfix"));
			plibInstance.Patch(typeof(GlobalResources), "Instance", null, PatchMethod("Instance_Postfix"));
			plibInstance.Patch(typeof(MainMenu), "OnSpawn", null, PatchMethod("MainMenu_OnSpawn_Postfix"));
			plibInstance.Patch(typeof(DetailsScreen), "OnPrefabInit", null, PatchMethod("DetailsScreen_OnPrefabInit_Postfix"));
		}

		public override void PostInitialize(Harmony plibInstance)
		{
			InvokeAllProcess(1u, null);
		}

		public override void Process(uint when, object _)
		{
			if (!patches.TryGetValue(when, out var value) || value == null || value.Count <= 0)
			{
				return;
			}
			string text = RunAt.ToString(when);
			foreach (IPatchMethodInstance item in value)
			{
				try
				{
					item.Run(harmony);
				}
				catch (TargetInvocationException ex)
				{
					PUtil.LogError("Error running patches for stage " + text + ":");
					PUtil.LogException(ex.GetBaseException());
				}
				catch (Exception thrown)
				{
					PUtil.LogError("Error running patches for stage " + text + ":");
					PUtil.LogException(thrown);
				}
			}
		}

		public void RegisterPatch(uint when, IPatchMethodInstance patch)
		{
			RegisterForForwarding();
			if (patch == null)
			{
				throw new ArgumentNullException("patch");
			}
			if (when == 0)
			{
				patch.Run(harmony);
			}
			else
			{
				AddHandler(when, patch);
			}
		}

		public void RegisterPatchClass(Type type)
		{
			int num = 0;
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			RegisterForForwarding();
			MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				object[] customAttributes = methodInfo.GetCustomAttributes(inherit: true);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					if (customAttributes[j] is IPLibAnnotation iPLibAnnotation)
					{
						uint runtime = iPLibAnnotation.Runtime;
						IPatchMethodInstance patchMethodInstance = iPLibAnnotation.CreateInstance(methodInfo);
						if (runtime == 0)
						{
							patchMethodInstance.Run(harmony);
						}
						else
						{
							AddHandler(iPLibAnnotation.Runtime, patchMethodInstance);
						}
						num++;
					}
				}
			}
			if (num > 0)
			{
				PRegistry.LogPatchDebug("Registered {0:D} handler(s) for {1}".F(num, Assembly.GetCallingAssembly().GetNameSafe() ?? "?"));
			}
			else
			{
				PRegistry.LogPatchWarning("RegisterPatchClass could not find any handlers!");
			}
		}
	}
	public static class RunAt
	{
		public const uint Immediately = 0u;

		public const uint AfterModsLoad = 1u;

		public const uint BeforeDbInit = 2u;

		public const uint AfterDbInit = 3u;

		public const uint InMainMenu = 4u;

		public const uint OnStartGame = 5u;

		public const uint OnEndGame = 6u;

		public const uint AfterLayerableLoad = 7u;

		public const uint BeforeDbPostProcess = 8u;

		public const uint AfterDbPostProcess = 9u;

		public const uint OnDetailsScreenInit = 10u;

		private static readonly string[] STRING_VALUES = new string[8] { "Immediately", "AfterModsLoad", "BeforeDbInit", "AfterDbInit", "InMainMenu", "OnStartGame", "OnEndGame", "AfterLayerableLoad" };

		public static string ToString(uint runtime)
		{
			if (runtime >= STRING_VALUES.Length)
			{
				return runtime.ToString();
			}
			return STRING_VALUES[runtime];
		}
	}
}
namespace PeterHan.PLib.Detours
{
	internal sealed class DetouredField<P, T> : IDetouredField<P, T>
	{
		public Func<P, T> Get { get; }

		public string Name { get; }

		public Action<P, T> Set { get; }

		internal DetouredField(string name, Func<P, T> get, Action<P, T> set)
		{
			Name = name ?? throw new ArgumentNullException("name");
			Get = get;
			Set = set;
		}

		public override string ToString()
		{
			return $"DetouredField[name={Name}]";
		}
	}
	public sealed class DetouredMethod<D> where D : Delegate
	{
		private D delg;

		private readonly Type type;

		public D Invoke
		{
			get
			{
				Initialize();
				return delg;
			}
		}

		public string Name { get; }

		internal DetouredMethod(Type type, string name)
		{
			this.type = type ?? throw new ArgumentNullException("type");
			Name = name ?? throw new ArgumentNullException("name");
			delg = null;
		}

		public void Initialize()
		{
			if (delg == null)
			{
				delg = type.Detour<D>(Name);
			}
		}

		public override string ToString()
		{
			return string.Format("LazyDetouredMethod[type={1},name={0}]", Name, type.FullName);
		}
	}
	public class DetourException : ArgumentException
	{
		public DetourException(string message)
			: base(message)
		{
		}
	}
	public interface IDetouredField<P, T>
	{
		Func<P, T> Get { get; }

		string Name { get; }

		Action<P, T> Set { get; }
	}
	internal sealed class LazyDetouredField<P, T> : IDetouredField<P, T>
	{
		private Func<P, T> getter;

		private Action<P, T> setter;

		private readonly Type type;

		public Func<P, T> Get
		{
			get
			{
				Initialize();
				return getter;
			}
		}

		public string Name { get; }

		public Action<P, T> Set
		{
			get
			{
				Initialize();
				return setter;
			}
		}

		internal LazyDetouredField(Type type, string name)
		{
			this.type = type ?? throw new ArgumentNullException("type");
			Name = name ?? throw new ArgumentNullException("name");
			getter = null;
			setter = null;
		}

		public void Initialize()
		{
			if (getter == null && setter == null)
			{
				IDetouredField<P, T> detouredField = PDetours.DetourField<P, T>(Name);
				getter = detouredField.Get;
				setter = detouredField.Set;
			}
		}

		public override string ToString()
		{
			return string.Format("LazyDetouredField[type={1},name={0}]", Name, type.FullName);
		}
	}
	public static class PDetours
	{
		private sealed class DelegateInfo
		{
			private readonly Type delegateType;

			public readonly Type[] parameterTypes;

			public readonly Type returnType;

			public static DelegateInfo Create(Type delegateType)
			{
				if (delegateType == null)
				{
					throw new ArgumentNullException("delegateType");
				}
				MethodInfo methodSafe = delegateType.GetMethodSafe("Invoke", isStatic: false, PPatchTools.AnyArguments);
				if (methodSafe == null)
				{
					throw new ArgumentException("Invalid delegate type: " + delegateType);
				}
				return new DelegateInfo(delegateType, methodSafe.GetParameterTypes(), methodSafe.ReturnType);
			}

			private DelegateInfo(Type delegateType, Type[] parameterTypes, Type returnType)
			{
				this.delegateType = delegateType;
				this.parameterTypes = parameterTypes;
				this.returnType = returnType;
			}

			public override string ToString()
			{
				return "DelegateInfo[delegate={0},return={1},parameters={2}]".F(delegateType, returnType, parameterTypes.Join());
			}
		}

		public static D Detour<D>(this Type type) where D : Delegate
		{
			return type.Detour<D>(typeof(D).Name);
		}

		public static D Detour<D>(this Type type, string name) where D : Delegate
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			DelegateInfo expected = DelegateInfo.Create(typeof(D));
			MethodInfo methodInfo = null;
			int num = int.MaxValue;
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo2 in array)
			{
				if (!(methodInfo2.Name == name))
				{
					continue;
				}
				try
				{
					int num2 = ValidateDelegate(expected, methodInfo2, methodInfo2.ReturnType).Length;
					if (num2 < num)
					{
						num = num2;
						methodInfo = methodInfo2;
					}
				}
				catch (DetourException)
				{
				}
			}
			if (methodInfo == null)
			{
				throw new DetourException("No match found for {1}.{0}".F(name, type.FullName));
			}
			return methodInfo.Detour<D>();
		}

		public static D DetourConstructor<D>(this Type type) where D : Delegate
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			DelegateInfo expected = DelegateInfo.Create(typeof(D));
			ConstructorInfo constructorInfo = null;
			int num = int.MaxValue;
			ConstructorInfo[] array = constructors;
			foreach (ConstructorInfo constructorInfo2 in array)
			{
				try
				{
					int num2 = ValidateDelegate(expected, constructorInfo2, type).Length;
					if (num2 < num)
					{
						num = num2;
						constructorInfo = constructorInfo2;
					}
				}
				catch (DetourException)
				{
				}
			}
			if (constructorInfo == null)
			{
				throw new DetourException("No match found for {0} constructor".F(type.FullName));
			}
			return constructorInfo.Detour<D>();
		}

		public static DetouredMethod<D> DetourLazy<D>(this Type type, string name) where D : Delegate
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			return new DetouredMethod<D>(type, name);
		}

		public static D Detour<D>(this MethodInfo target) where D : Delegate
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (target.ContainsGenericParameters)
			{
				throw new ArgumentException("Generic types must have all parameters defined");
			}
			DelegateInfo delegateInfo = DelegateInfo.Create(typeof(D));
			Type declaringType = target.DeclaringType;
			Type[] parameterTypes = delegateInfo.parameterTypes;
			ParameterInfo[] actualParams = ValidateDelegate(delegateInfo, target, target.ReturnType);
			int offset = ((!target.IsStatic) ? 1 : 0);
			if (declaringType == null)
			{
				throw new ArgumentException("Method is not declared by an actual type");
			}
			DynamicMethod dynamicMethod = new DynamicMethod(target.Name + "_Detour", delegateInfo.returnType, parameterTypes, declaringType, skipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			LoadParameters(iLGenerator, actualParams, parameterTypes, offset);
			if (declaringType.IsValueType || target.IsStatic)
			{
				iLGenerator.Emit(OpCodes.Call, target);
			}
			else
			{
				iLGenerator.Emit(OpCodes.Callvirt, target);
			}
			iLGenerator.Emit(OpCodes.Ret);
			FinishDynamicMethod(dynamicMethod, actualParams, parameterTypes, offset);
			return dynamicMethod.CreateDelegate(typeof(D)) as D;
		}

		public static D Detour<D>(this ConstructorInfo target) where D : Delegate
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (target.ContainsGenericParameters)
			{
				throw new ArgumentException("Generic types must have all parameters defined");
			}
			if (target.IsStatic)
			{
				throw new ArgumentException("Static constructors cannot be called manually");
			}
			DelegateInfo delegateInfo = DelegateInfo.Create(typeof(D));
			Type declaringType = target.DeclaringType;
			Type[] parameterTypes = delegateInfo.parameterTypes;
			ParameterInfo[] actualParams = ValidateDelegate(delegateInfo, target, declaringType);
			if (declaringType == null)
			{
				throw new ArgumentException("Method is not declared by an actual type");
			}
			DynamicMethod dynamicMethod = new DynamicMethod("Constructor_Detour", delegateInfo.returnType, parameterTypes, declaringType, skipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			LoadParameters(iLGenerator, actualParams, parameterTypes, 0);
			iLGenerator.Emit(OpCodes.Newobj, target);
			iLGenerator.Emit(OpCodes.Ret);
			FinishDynamicMethod(dynamicMethod, actualParams, parameterTypes, 0);
			return dynamicMethod.CreateDelegate(typeof(D)) as D;
		}

		public static IDetouredField<P, T> DetourField<P, T>(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			Type typeFromHandle = typeof(P);
			FieldInfo field = typeFromHandle.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				try
				{
					PropertyInfo property = typeFromHandle.GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (property == null)
					{
						throw new DetourException("Unable to find {0} on type {1}".F(name, typeof(P).FullName));
					}
					return DetourProperty<P, T>(property);
				}
				catch (AmbiguousMatchException)
				{
					throw new DetourException("Unable to find {0} on type {1}".F(name, typeof(P).FullName));
				}
			}
			if (!typeFromHandle.IsValueType)
			{
				if (typeFromHandle.IsByRef)
				{
					Type? elementType = typeFromHandle.GetElementType();
					if ((object)elementType != null && elementType.IsValueType)
					{
						goto IL_00ca;
					}
				}
				return DetourField<P, T>(field);
			}
			goto IL_00ca;
			IL_00ca:
			throw new ArgumentException("For accessing struct fields, use DetourStructField");
		}

		public static IDetouredField<P, T> DetourFieldLazy<P, T>(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			return new LazyDetouredField<P, T>(typeof(P), name);
		}

		private static IDetouredField<P, T> DetourField<P, T>(FieldInfo target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			Type? declaringType = target.DeclaringType;
			string name = target.Name;
			if (declaringType != typeof(P))
			{
				throw new ArgumentException("Parent type does not match delegate to be created");
			}
			DynamicMethod dynamicMethod = new DynamicMethod(name + "_Detour_Get", typeof(T), new Type[1] { typeof(P) }, restrictedSkipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			if (target.IsStatic)
			{
				iLGenerator.Emit(OpCodes.Ldsfld, target);
			}
			else
			{
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldfld, target);
			}
			iLGenerator.Emit(OpCodes.Ret);
			DynamicMethod dynamicMethod2;
			if (target.IsInitOnly)
			{
				dynamicMethod2 = null;
			}
			else
			{
				dynamicMethod2 = new DynamicMethod(name + "_Detour_Set", null, new Type[2]
				{
					typeof(P),
					typeof(T)
				}, restrictedSkipVisibility: true);
				iLGenerator = dynamicMethod2.GetILGenerator();
				if (target.IsStatic)
				{
					iLGenerator.Emit(OpCodes.Ldarg_1);
					iLGenerator.Emit(OpCodes.Stsfld, target);
				}
				else
				{
					iLGenerator.Emit(OpCodes.Ldarg_0);
					iLGenerator.Emit(OpCodes.Ldarg_1);
					iLGenerator.Emit(OpCodes.Stfld, target);
				}
				iLGenerator.Emit(OpCodes.Ret);
			}
			return new DetouredField<P, T>(name, dynamicMethod.CreateDelegate(typeof(Func<P, T>)) as Func<P, T>, dynamicMethod2?.CreateDelegate(typeof(Action<P, T>)) as Action<P, T>);
		}

		private static IDetouredField<P, T> DetourProperty<P, T>(PropertyInfo target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			Type? declaringType = target.DeclaringType;
			string name = target.Name;
			if (declaringType != typeof(P))
			{
				throw new ArgumentException("Parent type does not match delegate to be created");
			}
			ParameterInfo[] indexParameters = target.GetIndexParameters();
			if (indexParameters != null && indexParameters.Length != 0)
			{
				throw new DetourException("Cannot detour on properties with index arguments");
			}
			MethodInfo getMethod = target.GetGetMethod(nonPublic: true);
			DynamicMethod dynamicMethod;
			if (target.CanRead && getMethod != null)
			{
				dynamicMethod = new DynamicMethod(name + "_Detour_Get", typeof(T), new Type[1] { typeof(P) }, restrictedSkipVisibility: true);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				if (!getMethod.IsStatic)
				{
					iLGenerator.Emit(OpCodes.Ldarg_0);
				}
				iLGenerator.Emit(OpCodes.Call, getMethod);
				iLGenerator.Emit(OpCodes.Ret);
			}
			else
			{
				dynamicMethod = null;
			}
			MethodInfo setMethod = target.GetSetMethod(nonPublic: true);
			DynamicMethod dynamicMethod2;
			if (target.CanWrite && setMethod != null)
			{
				dynamicMethod2 = new DynamicMethod(name + "_Detour_Set", null, new Type[2]
				{
					typeof(P),
					typeof(T)
				}, restrictedSkipVisibility: true);
				ILGenerator iLGenerator2 = dynamicMethod2.GetILGenerator();
				if (!setMethod.IsStatic)
				{
					iLGenerator2.Emit(OpCodes.Ldarg_0);
				}
				iLGenerator2.Emit(OpCodes.Ldarg_1);
				iLGenerator2.Emit(OpCodes.Call, setMethod);
				iLGenerator2.Emit(OpCodes.Ret);
			}
			else
			{
				dynamicMethod2 = null;
			}
			return new DetouredField<P, T>(name, dynamicMethod?.CreateDelegate(typeof(Func<P, T>)) as Func<P, T>, dynamicMethod2?.CreateDelegate(typeof(Action<P, T>)) as Action<P, T>);
		}

		public static IDetouredField<object, T> DetourStructField<T>(this Type parentType, string name)
		{
			if (parentType == null)
			{
				throw new ArgumentNullException("parentType");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			FieldInfo field = parentType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				throw new DetourException("Unable to find {0} on type {1}".F(name, parentType.FullName));
			}
			DynamicMethod dynamicMethod = new DynamicMethod(name + "_Detour_Get", typeof(T), new Type[1] { typeof(object) }, restrictedSkipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Unbox, parentType);
			iLGenerator.Emit(OpCodes.Ldfld, field);
			iLGenerator.Emit(OpCodes.Ret);
			DynamicMethod dynamicMethod2;
			if (field.IsInitOnly)
			{
				dynamicMethod2 = null;
			}
			else
			{
				dynamicMethod2 = new DynamicMethod(name + "_Detour_Set", null, new Type[2]
				{
					typeof(object),
					typeof(T)
				}, restrictedSkipVisibility: true);
				ILGenerator iLGenerator2 = dynamicMethod2.GetILGenerator();
				iLGenerator2.Emit(OpCodes.Ldarg_0);
				iLGenerator2.Emit(OpCodes.Unbox, parentType);
				iLGenerator2.Emit(OpCodes.Ldarg_1);
				iLGenerator2.Emit(OpCodes.Stfld, field);
				iLGenerator2.Emit(OpCodes.Ret);
			}
			return new DetouredField<object, T>(name, dynamicMethod.CreateDelegate(typeof(Func<object, T>)) as Func<object, T>, dynamicMethod2?.CreateDelegate(typeof(Action<object, T>)) as Action<object, T>);
		}

		private static void FinishDynamicMethod(DynamicMethod caller, ParameterInfo[] actualParams, Type[] expectedParams, int offset)
		{
			int num = expectedParams.Length;
			if (offset > 0)
			{
				caller.DefineParameter(1, ParameterAttributes.None, "this");
			}
			for (int i = offset; i < num; i++)
			{
				ParameterInfo parameterInfo = actualParams[i - offset];
				caller.DefineParameter(i + 1, parameterInfo.Attributes, parameterInfo.Name);
			}
		}

		private static void LoadParameters(ILGenerator generator, ParameterInfo[] actualParams, Type[] expectedParams, int offset)
		{
			int num = expectedParams.Length;
			int num2 = actualParams.Length + offset;
			if (num > 0)
			{
				generator.Emit(OpCodes.Ldarg_0);
			}
			if (num > 1)
			{
				generator.Emit(OpCodes.Ldarg_1);
			}
			if (num > 2)
			{
				generator.Emit(OpCodes.Ldarg_2);
			}
			if (num > 3)
			{
				generator.Emit(OpCodes.Ldarg_3);
			}
			for (int i = 4; i < num; i++)
			{
				generator.Emit(OpCodes.Ldarg_S, i);
			}
			for (int j = num; j < num2; j++)
			{
				ParameterInfo parameterInfo = actualParams[j - offset];
				PTranspilerTools.GenerateDefaultLoad(generator, parameterInfo.ParameterType, parameterInfo.DefaultValue);
			}
		}

		public static D TryDetour<D>(this Type type, string name) where D : Delegate
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			try
			{
				return type.Detour<D>(name);
			}
			catch (DetourException)
			{
				return null;
			}
		}

		public static D TryDetourConstructor<D>(this Type type) where D : Delegate
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			try
			{
				return type.DetourConstructor<D>();
			}
			catch (DetourException)
			{
				return null;
			}
		}

		public static D TryDetour<D>(this MethodInfo target) where D : Delegate
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			try
			{
				return target.Detour<D>();
			}
			catch (DetourException)
			{
				return null;
			}
		}

		public static D TryDetour<D>(this ConstructorInfo target) where D : Delegate
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			try
			{
				return target.Detour<D>();
			}
			catch (DetourException)
			{
				return null;
			}
		}

		public static IDetouredField<P, T> TryDetourField<P, T>(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			try
			{
				return DetourField<P, T>(name);
			}
			catch (DetourException)
			{
				return null;
			}
		}

		public static IDetouredField<object, T> TryDetourStructField<T>(this Type parentType, string name)
		{
			if (parentType == null)
			{
				throw new ArgumentNullException("parentType");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			try
			{
				return parentType.DetourStructField<T>(name);
			}
			catch (DetourException)
			{
				return null;
			}
		}

		private static ParameterInfo[] ValidateDelegate(DelegateInfo expected, MethodBase actual, Type actualReturn)
		{
			Type[] parameterTypes = expected.parameterTypes;
			Type returnType = expected.returnType;
			if (!returnType.IsAssignableFrom(actualReturn))
			{
				throw new DetourException("Return type {0} cannot be converted to type {1}".F(actualReturn.FullName, returnType.FullName));
			}
			Type declaringType = actual.DeclaringType;
			if (declaringType == null)
			{
				throw new ArgumentException("Method is not declared by an actual type");
			}
			if (declaringType.ContainsGenericParameters)
			{
				throw new DetourException("Method parent type {0} must have all generic parameters defined".F(declaringType.FullName));
			}
			string text = declaringType.FullName + "." + actual.Name;
			ParameterInfo[] parameters = actual.GetParameters();
			int num = parameters.Length;
			int num2 = parameterTypes.Length;
			Type[] array = new Type[num];
			bool flag = actual.IsStatic || actual.IsConstructor;
			for (int i = 0; i < num; i++)
			{
				array[i] = parameters[i].ParameterType;
			}
			Type[] array2;
			if (flag)
			{
				array2 = array;
			}
			else
			{
				array2 = PTranspilerTools.PushDeclaringType(array, declaringType);
				num++;
			}
			if (num2 > num)
			{
				throw new DetourException("Method {0} has only {1:D} parameters, but {2:D} were supplied".F(actual.ToString(), num, num2));
			}
			for (int j = 0; j < num2; j++)
			{
				Type type = array2[j];
				Type type2 = parameterTypes[j];
				if (!type.IsAssignableFrom(type2))
				{
					throw new DetourException("Argument {0:D} for method {3} cannot be converted from {1} to {2}".F(j, type.FullName, type2.FullName, text));
				}
			}
			int num3 = ((!flag) ? 1 : 0);
			for (int k = num2; k < num; k++)
			{
				ParameterInfo parameterInfo = parameters[k - num3];
				if (!parameterInfo.IsOptional)
				{
					throw new DetourException("New argument {0:D} for method {1} ({2}) is not optional".F(k, text, parameterInfo.ParameterType.FullName));
				}
			}
			return parameters;
		}
	}
}
namespace PeterHan.PLib.Core
{
	public static class AutoUnbox<T> where T : struct
	{
		public static object Box(T data)
		{
			return Boxed<T>.Get(data);
		}

		public static bool Unbox(object data, out T result)
		{
			bool result2 = true;
			if (data is Boxed<T> val)
			{
				result = val.value;
			}
			else if (data is T val2)
			{
				result = val2;
			}
			else
			{
				result = default(T);
				result2 = false;
			}
			return result2;
		}
	}
	public static class ExtensionMethods
	{
		public static string F(this string message, params object[] args)
		{
			return string.Format(message, args);
		}

		public static T GetComponentSafe<T>(this GameObject obj) where T : Component
		{
			if (!((Object)(object)obj == (Object)null))
			{
				return obj.GetComponent<T>();
			}
			return default(T);
		}

		public static string GetNameSafe(this Assembly assembly)
		{
			return assembly?.GetName()?.Name;
		}

		public static string GetFileVersion(this Assembly assembly)
		{
			object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), inherit: true);
			string result = null;
			if (customAttributes != null && customAttributes.Length != 0)
			{
				AssemblyFileVersionAttribute assemblyFileVersionAttribute = (AssemblyFileVersionAttribute)customAttributes[0];
				if (assemblyFileVersionAttribute != null)
				{
					result = assemblyFileVersionAttribute.Version;
				}
			}
			return result;
		}

		public static double InRange(this double value, double min, double max)
		{
			double num = value;
			if (num < min)
			{
				num = min;
			}
			if (num > max)
			{
				num = max;
			}
			return num;
		}

		public static float InRange(this float value, float min, float max)
		{
			float num = value;
			if (num < min)
			{
				num = min;
			}
			if (num > max)
			{
				num = max;
			}
			return num;
		}

		public static int InRange(this int value, int min, int max)
		{
			int num = value;
			if (num < min)
			{
				num = min;
			}
			if (num > max)
			{
				num = max;
			}
			return num;
		}

		public static bool IsFalling(this GameObject obj)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			int num = Grid.PosToCell(obj);
			Navigator val = default(Navigator);
			if (obj.TryGetComponent<Navigator>(ref val) && !val.IsMoving() && Grid.IsValidCell(num) && Grid.IsValidCell(Grid.CellBelow(num)))
			{
				return !val.NavGrid.NavTable.IsValid(num, val.CurrentNavType);
			}
			return false;
		}

		public static bool IsNaNOrInfinity(this double value)
		{
			if (!double.IsNaN(value))
			{
				return double.IsInfinity(value);
			}
			return true;
		}

		public static bool IsNaNOrInfinity(this float value)
		{
			if (!float.IsNaN(value))
			{
				return float.IsInfinity(value);
			}
			return true;
		}

		public static bool IsUsable(this GameObject building)
		{
			Operational val = default(Operational);
			if (building.TryGetComponent<Operational>(ref val))
			{
				return val.IsFunctional;
			}
			return false;
		}

		public static string Join(this IEnumerable values, string delimiter = ",")
		{
			StringBuilder stringBuilder = new StringBuilder(128);
			bool flag = true;
			foreach (object? value in values)
			{
				if (!flag)
				{
					stringBuilder.Append(delimiter);
				}
				stringBuilder.Append(value);
				flag = false;
			}
			return stringBuilder.ToString();
		}

		public static void Patch(this Harmony instance, Type type, string methodName, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (string.IsNullOrEmpty(methodName))
			{
				throw new ArgumentNullException("methodName");
			}
			try
			{
				MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null)
				{
					instance.Patch((MethodBase)method, prefix, postfix, (HarmonyMethod)null, (HarmonyMethod)null);
					return;
				}
				PUtil.LogWarning("Unable to find method {0} on type {1}".F(methodName, type.FullName));
			}
			catch (AmbiguousMatchException thrown)
			{
				PUtil.LogException(thrown);
			}
		}

		public static void PatchConstructor(this Harmony instance, Type type, Type[] arguments, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			try
			{
				ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, arguments, null);
				if (constructor != null)
				{
					instance.Patch((MethodBase)constructor, prefix, postfix, (HarmonyMethod)null, (HarmonyMethod)null);
					return;
				}
				PUtil.LogWarning("Unable to find constructor on type {0}".F(type.FullName));
			}
			catch (ArgumentException thrown)
			{
				PUtil.LogException(thrown);
			}
		}

		public static void PatchTranspile(this Harmony instance, Type type, string methodName, HarmonyMethod transpiler)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (string.IsNullOrEmpty(methodName))
			{
				throw new ArgumentNullException("methodName");
			}
			try
			{
				MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null)
				{
					instance.Patch((MethodBase)method, (HarmonyMethod)null, (HarmonyMethod)null, transpiler, (HarmonyMethod)null);
					return;
				}
				PUtil.LogWarning("Unable to find method {0} on type {1}".F(methodName, type.FullName));
			}
			catch (AmbiguousMatchException thrown)
			{
				PUtil.LogException(thrown);
			}
			catch (FormatException ex)
			{
				PUtil.LogWarning("Unable to transpile method {0}: {1}".F(methodName, ex.Message));
			}
		}

		public static float RoundTo(this float value, float increment)
		{
			float result = value;
			if (increment > 0f && !float.IsInfinity(increment))
			{
				double num = increment;
				result = (float)(Math.Round((double)value / num, 0, MidpointRounding.ToEven) * num);
			}
			return result;
		}

		public static GameObject SetParent(this GameObject child, GameObject parent)
		{
			if ((Object)(object)child == (Object)null)
			{
				throw new ArgumentNullException("child");
			}
			child.transform.SetParent(((Object)(object)parent == (Object)null) ? null : parent.transform, false);
			return child;
		}
	}
	public interface IPLibRegistry
	{
		IDictionary<string, object> ModData { get; }

		void AddCandidateVersion(PForwardedComponent instance);

		PForwardedComponent GetLatestVersion(string id);

		object GetSharedData(string id);

		IEnumerable<PForwardedComponent> GetAllComponents(string id);

		void SetSharedData(string id, object data);
	}
	public interface IRefreshUserMenu
	{
		void OnRefreshUserMenu();
	}
	public abstract class PForwardedComponent : IComparable<PForwardedComponent>
	{
		public sealed class PComponentComparator : IComparer<PForwardedComponent>
		{
			public int Compare(PForwardedComponent a, PForwardedComponent b)
			{
				if (b != null)
				{
					if (a != null)
					{
						return b.Version.CompareTo(a.Version);
					}
					return 1;
				}
				if (a != null)
				{
					return -1;
				}
				return 0;
			}
		}

		public const int MAX_DEPTH = 8;

		private volatile bool registered;

		private readonly object candidateLock;

		protected virtual object InstanceData { get; set; }

		public string ID => GetType().FullName;

		protected JsonSerializer SerializationSettings { get; set; }

		public abstract Version Version { get; }

		protected PForwardedComponent()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			candidateLock = new object();
			InstanceData = null;
			registered = false;
			SerializationSettings = new JsonSerializer
			{
				DateTimeZoneHandling = (DateTimeZoneHandling)3,
				Culture = CultureInfo.InvariantCulture,
				MaxDepth = 8
			};
		}

		public virtual void Bootstrap(Harmony plibInstance)
		{
		}

		public int CompareTo(PForwardedComponent other)
		{
			return Version.CompareTo(other.Version);
		}

		internal virtual object DoInitialize(Harmony plibInstance)
		{
			Initialize(plibInstance);
			return this;
		}

		public T GetInstanceData<T>(T defValue = default(T))
		{
			if (!(InstanceData is T result))
			{
				return defValue;
			}
			return result;
		}

		public T GetInstanceDataSerialized<T>(T defValue = default(T))
		{
			//IL_0097: Expected O, but got Unknown
			object instanceData = InstanceData;
			T result = defValue;
			using (MemoryStream memoryStream = new MemoryStream(1024))
			{
				try
				{
					StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
					SerializationSettings.Serialize((TextWriter)streamWriter, instanceData);
					streamWriter.Flush();
					memoryStream.Position = 0L;
					StreamReader streamReader = new StreamReader(memoryStream, Encoding.UTF8);
					if (SerializationSettings.Deserialize((TextReader)streamReader, typeof(T)) is T val)
					{
						result = val;
					}
				}
				catch (JsonException ex)
				{
					PUtil.LogError("Unable to serialize instance data for component " + ID + ":");
					PUtil.LogException((Exception)ex);
					result = defValue;
				}
			}
			return result;
		}

		public T GetSharedData<T>(T defValue = default(T))
		{
			if (!(PRegistry.Instance.GetSharedData(ID) is T result))
			{
				return defValue;
			}
			return result;
		}

		public T GetSharedDataSerialized<T>(T defValue = default(T))
		{
			//IL_0093: Expected O, but got Unknown
			object sharedData = PRegistry.Instance.GetSharedData(ID);
			T result = defValue;
			using (MemoryStream memoryStream = new MemoryStream(1024))
			{
				try
				{
					SerializationSettings.Serialize((TextWriter)new StreamWriter(memoryStream, Encoding.UTF8), sharedData);
					memoryStream.Position = 0L;
					if (SerializationSettings.Deserialize((TextReader)new StreamReader(memoryStream, Encoding.UTF8), typeof(T)) is T val)
					{
						result = val;
					}
				}
				catch (JsonException ex)
				{
					PUtil.LogError("Unable to serialize shared data for component " + ID + ":");
					PUtil.LogException((Exception)ex);
					result = defValue;
				}
			}
			return result;
		}

		public virtual Assembly GetOwningAssembly()
		{
			return GetType().Assembly;
		}

		public abstract void Initialize(Harmony plibInstance);

		protected void InvokeAllProcess(uint operation, object args)
		{
			IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(ID);
			if (allComponents == null)
			{
				return;
			}
			foreach (PForwardedComponent item in allComponents)
			{
				item.Process(operation, args);
			}
		}

		public HarmonyMethod PatchMethod(string name)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			return new HarmonyMethod(GetType(), name, (Type[])null);
		}

		public virtual void PostInitialize(Harmony plibInstance)
		{
		}

		public virtual void Process(uint operation, object args)
		{
		}

		protected bool RegisterForForwarding()
		{
			bool result = false;
			lock (candidateLock)
			{
				if (!registered)
				{
					PUtil.InitLibrary(logVersion: false);
					PRegistry.Instance.AddCandidateVersion(this);
					result = (registered = true);
				}
			}
			return result;
		}

		public void SetSharedData(object value)
		{
			PRegistry.Instance.SetSharedData(ID, value);
		}
	}
	public static class PGameUtils
	{
		private delegate void CreateSoundDelegate(AudioSheets instance, string file_name, string anim_name, string type, float min_interval, string sound_name, int frame, string dlcId);

		private delegate void InfoRefreshFunction(SimpleInfoScreen instance, bool force);

		private static readonly DetouredMethod<CreateSoundDelegate> CREATE_SOUND = typeof(AudioSheets).DetourLazy<CreateSoundDelegate>("CreateSound");

		private static readonly DetouredMethod<InfoRefreshFunction> REFRESH_INFO_SCREEN = typeof(SimpleInfoScreen).DetourLazy<InfoRefreshFunction>("Refresh");

		public static void CenterAndSelect(KMonoBehaviour entity)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			KSelectable val = default(KSelectable);
			if ((Object)(object)entity != (Object)null && ((Component)entity).TryGetComponent<KSelectable>(ref val))
			{
				SelectTool.Instance.SelectAndFocus(entity.transform.position, val, Vector3.zero);
			}
		}

		public static void CopySoundsToAnim(string dstAnim, string srcAnim)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(dstAnim))
			{
				throw new ArgumentNullException("dstAnim");
			}
			if (string.IsNullOrEmpty(srcAnim))
			{
				throw new ArgumentNullException("srcAnim");
			}
			if ((Object)(object)Assets.GetAnim(HashedString.op_Implicit(dstAnim)) != (Object)null)
			{
				GameAudioSheets val = GameAudioSheets.Get();
				try
				{
					foreach (AudioSheet sheet in ((AudioSheets)val).sheets)
					{
						SoundInfo[] soundInfos = sheet.soundInfos;
						int num = soundInfos.Length;
						for (int i = 0; i < num; i++)
						{
							SoundInfo val2 = soundInfos[i];
							if (IsDLCOwned(val2.RequiredDlcId) && val2.File == srcAnim)
							{
								CreateAllSounds((AudioSheets)(object)val, dstAnim, val2, sheet.defaultType);
							}
						}
					}
					return;
				}
				catch (Exception thrown)
				{
					PUtil.LogWarning("Unable to copy sound files from {0} to {1}:".F(srcAnim, dstAnim));
					PUtil.LogExcWarn(thrown);
					return;
				}
			}
			PUtil.LogWarning("Destination animation \"{0}\" not found!".F(dstAnim));
		}

		private static int CreateSound(AudioSheets sheet, string file, string type, SoundInfo info, string sound, int frame)
		{
			int result = 0;
			if (!string.IsNullOrEmpty(sound) && CREATE_SOUND != null)
			{
				CREATE_SOUND.Invoke(sheet, file, info.Anim, type, info.MinInterval, sound, frame, info.RequiredDlcId);
				result = 1;
			}
			return result;
		}

		private static void CreateAllSounds(AudioSheets sheet, string animFile, SoundInfo info, string defaultType)
		{
			string text = info.Type;
			if (string.IsNullOrEmpty(text))
			{
				text = defaultType;
			}
			_ = CreateSound(sheet, animFile, text, info, info.Name0, info.Frame0) + CreateSound(sheet, animFile, text, info, info.Name1, info.Frame1) + CreateSound(sheet, animFile, text, info, info.Name2, info.Frame2) + CreateSound(sheet, animFile, text, info, info.Name3, info.Frame3) + CreateSound(sheet, animFile, text, info, info.Name4, info.Frame4) + CreateSound(sheet, animFile, text, info, info.Name5, info.Frame5) + CreateSound(sheet, animFile, text, info, info.Name6, info.Frame6) + CreateSound(sheet, animFile, text, info, info.Name7, info.Frame7) + CreateSound(sheet, animFile, text, info, info.Name8, info.Frame8) + CreateSound(sheet, animFile, text, info, info.Name9, info.Frame9) + CreateSound(sheet, animFile, text, info, info.Name10, info.Frame10);
			CreateSound(sheet, animFile, text, info, info.Name11, info.Frame11);
		}

		public static void CreatePopup(Sprite image, string text, int cell)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			CreatePopup(image, text, Grid.CellToPosCBC(cell, (SceneLayer)25));
		}

		public static void CreatePopup(Sprite image, string text, Vector3 position)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			PopFXManager.Instance.SpawnFX(image, text, (Transform)null, position, 1.5f, false, false);
		}

		public static IntraObjectHandler<T> CreateUserMenuHandler<T>() where T : Component, IRefreshUserMenu
		{
			return IntraObjectHandler<T>.op_Implicit((Action<T, object>)delegate(T target, object ignore)
			{
				target.OnRefreshUserMenu();
			});
		}

		public static ObjectLayer GetObjectLayer(string name, ObjectLayer defValue)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			if (!Enum.TryParse<ObjectLayer>(name, out ObjectLayer result))
			{
				return defValue;
			}
			return result;
		}

		public static void HighlightEntity(Component entity, Color highlightColor)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			KAnimControllerBase val = default(KAnimControllerBase);
			if ((Object)(object)entity != (Object)null && entity.TryGetComponent<KAnimControllerBase>(ref val))
			{
				val.HighlightColour = Color32.op_Implicit(highlightColor);
			}
		}

		public static bool IsDLCOwned(string name)
		{
			return DlcManager.IsContentSubscribed(name);
		}

		public static void PlaySound(string name, Vector3 position)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			SoundEvent.PlayOneShot(GlobalAssets.GetSound(name, false), position, 1f);
		}

		public static void RefreshInfoScreen(this SimpleInfoScreen screen, bool force = false)
		{
			if ((Object)(object)screen != (Object)null)
			{
				REFRESH_INFO_SCREEN.Invoke(screen, force);
			}
		}

		public static void SaveMods()
		{
			Global.Instance.modManager.Save();
		}
	}
	internal sealed class PLibCorePatches : PForwardedComponent
	{
		internal static readonly Version VERSION = new Version("4.24.0.0");

		public override Version Version => VERSION;

		private static void Initialize_Postfix()
		{
			Locale locale = Localization.GetLocale();
			if (locale == null)
			{
				return;
			}
			int num = 0;
			string text = locale.Code;
			if (string.IsNullOrEmpty(text))
			{
				text = Localization.GetCurrentLanguageCode();
			}
			IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(typeof(PLibCorePatches).FullName);
			if (allComponents != null)
			{
				foreach (PForwardedComponent item in allComponents)
				{
					item.Process(0u, locale);
				}
			}
			IEnumerable<PForwardedComponent> allComponents2 = PRegistry.Instance.GetAllComponents("PeterHan.PLib.Database.PLocalization");
			if (allComponents2 != null)
			{
				foreach (PForwardedComponent item2 in allComponents2)
				{
					item2.Process(0u, locale);
					num++;
				}
			}
			if (num > 0)
			{
				PRegistry.LogPatchDebug("Localized {0:D} mod(s) to locale {1}".F(num, text));
			}
		}

		private static IEnumerable<CodeInstruction> LoadPreviewImage_Transpile(IEnumerable<CodeInstruction> body)
		{
			MethodInfo methodSafe = typeof(Debug).GetMethodSafe("LogFormat", true, typeof(string), typeof(object[]));
			if (!(methodSafe == null))
			{
				return PPatchTools.RemoveMethodCall(body, methodSafe);
			}
			return body;
		}

		public override void Initialize(Harmony plibInstance)
		{
			TextMeshProPatcher.Patch();
			Type typeSafe = PPatchTools.GetTypeSafe("SteamUGCService", "Assembly-CSharp");
			if (typeSafe != null)
			{
				try
				{
					plibInstance.PatchTranspile(typeSafe, "LoadPreviewImage", PatchMethod("LoadPreviewImage_Transpile"));
				}
				catch (Exception)
				{
				}
			}
			plibInstance.Patch(typeof(Localization), "Initialize", null, PatchMethod("Initialize_Postfix"));
		}

		public override void Process(uint operation, object _)
		{
			Locale locale = Localization.GetLocale();
			if (locale != null && operation == 0)
			{
				PLibLocalization.LocalizeItself(locale);
			}
		}

		internal void Register(IPLibRegistry instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			instance.AddCandidateVersion(this);
		}
	}
	public static class PLibLocalization
	{
		public const string TRANSLATIONS_EXT = ".po";

		private const string TRANSLATIONS_RES_PATH = "PeterHan.PLib.Core.PLibStrings.";

		internal static void LocalizeItself(Locale locale)
		{
			if (locale == null)
			{
				throw new ArgumentNullException("locale");
			}
			Localization.RegisterForTranslation(typeof(PLibStrings));
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string text = locale.Code;
			if (string.IsNullOrEmpty(text))
			{
				text = Localization.GetCurrentLanguageCode();
			}
			try
			{
				using Stream stream = executingAssembly.GetManifestResourceStream("PeterHan.PLib.Core.PLibStrings." + text + ".po");
				if (stream == null)
				{
					return;
				}
				List<string> list = new List<string>(128);
				using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
				{
					string item;
					while ((item = streamReader.ReadLine()) != null)
					{
						list.Add(item);
					}
				}
				Localization.OverloadStrings(Localization.ExtractTranslatedStrings(list.ToArray(), false));
			}
			catch (Exception thrown)
			{
				PUtil.LogWarning("Failed to load {0} localization for PLib Core:".F(text));
				PUtil.LogExcWarn(thrown);
			}
		}
	}
	public static class PLibStrings
	{
		public static LocString BUTTON_MANUAL = LocString.op_Implicit("MANUAL CONFIG");

		public static LocString BUTTON_RESET = LocString.op_Implicit("RESET TO DEFAULT");

		public static LocString BUTTON_OK = OPTIONS_SCREEN.BACK;

		public static LocString BUTTON_OPTIONS = MAINMENU.OPTIONS;

		public static LocString DIALOG_TITLE = LocString.op_Implicit("Options for {0}");

		public static LocString KEY_HOME = LocString.op_Implicit("Home");

		public static LocString KEY_END = LocString.op_Implicit("End");

		public static LocString KEY_DELETE = LocString.op_Implicit("Delete");

		public static LocString KEY_PAGEUP = LocString.op_Implicit("Page Up");

		public static LocString KEY_PAGEDOWN = LocString.op_Implicit("Page Down");

		public static LocString KEY_SYSRQ = LocString.op_Implicit("SysRq");

		public static LocString KEY_PRTSCREEN = LocString.op_Implicit("Print Screen");

		public static LocString KEY_PAUSE = LocString.op_Implicit("Pause");

		public static LocString KEY_ARROWLEFT = LocString.op_Implicit("Left Arrow");

		public static LocString KEY_ARROWUP = LocString.op_Implicit("Up Arrow");

		public static LocString KEY_ARROWRIGHT = LocString.op_Implicit("Right Arrow");

		public static LocString KEY_ARROWDOWN = LocString.op_Implicit("Down Arrow");

		public static LocString KEY_CATEGORY_TITLE = LocString.op_Implicit("Mods");

		public static LocString LABEL_B = LocString.op_Implicit("B");

		public static LocString LABEL_G = LocString.op_Implicit("G");

		public static LocString LABEL_R = LocString.op_Implicit("R");

		public static LocString MOD_ASSEMBLY_VERSION = LocString.op_Implicit("Assembly Version: {0}");

		public static LocString MOD_HOMEPAGE = LocString.op_Implicit("Mod Homepage");

		public static LocString MOD_VERSION = LocString.op_Implicit("Mod Version: {0}");

		public static LocString OPTIONS_FILTERED = LocString.op_Implicit("No Options Available");

		public static LocString RESTART_CANCEL = RESTART.CANCEL;

		public static LocString RESTART_OK = RESTART.OK;

		public static LocString MAINMENU_UPDATE = LocString.op_Implicit("\n\n<color=#FFCC00>{0:D} mods may be out of date</color>");

		public static LocString MAINMENU_UPDATE_1 = LocString.op_Implicit("\n\n<color=#FFCC00>1 mod may be out of date</color>");

		public static LocString OUTDATED_TOOLTIP = LocString.op_Implicit("This mod is out of date!\nNew version: <b>{0}</b>\n\nUpdate local mods manually, or use <b>Mod Updater</b> to force update Steam mods");

		public static LocString OUTDATED_WARNING = LocString.op_Implicit("<b><style=\"logic_off\">Outdated!</style></b>");

		public static LocString RESTART_REQUIRED = LocString.op_Implicit("Oxygen Not Included must be restarted for these options to take effect.");

		public static LocString TOOLTIP_BLUE = LocString.op_Implicit("Blue");

		public static LocString TOOLTIP_CANCEL = LocString.op_Implicit("Discard changes.");

		public static LocString TOOLTIP_GREEN = LocString.op_Implicit("Green");

		public static LocString TOOLTIP_HOMEPAGE = LocString.op_Implicit("Visit the mod's website.");

		public static LocString TOOLTIP_HUE = LocString.op_Implicit("Hue");

		public static LocString TOOLTIP_MANUAL = LocString.op_Implicit("Opens the folder containing the full mod configuration.");

		public static LocString TOOLTIP_NEXT = LocString.op_Implicit("Next");

		public static LocString TOOLTIP_OK = LocString.op_Implicit("Save these options. Some mods may require a restart for the options to take effect.");

		public static LocString TOOLTIP_PREVIOUS = LocString.op_Implicit("Previous");

		public static LocString TOOLTIP_RED = LocString.op_Implicit("Red");

		public static LocString TOOLTIP_RESET = LocString.op_Implicit("Resets the mod configuration to default values.");

		public static LocString TOOLTIP_SATURATION = LocString.op_Implicit("Saturation");

		public static LocString TOOLTIP_TOGGLE = LocString.op_Implicit("Show or hide this options category");

		public static LocString TOOLTIP_VALUE = LocString.op_Implicit("Value");

		public static LocString TOOLTIP_VERSION = LocString.op_Implicit("The currently installed version of this mod.\n\nCompare this version with the mod's Release Notes to see if it is outdated.");
	}
	public static class PPatchTools
	{
		public const BindingFlags BASE_FLAGS = BindingFlags.Public | BindingFlags.NonPublic;

		public static readonly MethodInfo RemoveCall = typeof(PPatchTools).GetMethodSafe("RemoveMethodCallPrivate", true);

		public static Type[] AnyArguments => new Type[1];

		public static T CreateDelegate<T>(this Type type, string method, object caller, params Type[] argumentTypes) where T : Delegate
		{
			T result = null;
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (string.IsNullOrEmpty(method))
			{
				throw new ArgumentNullException("method");
			}
			MethodInfo methodSafe = type.GetMethodSafe(method, isStatic: false, argumentTypes);
			if (methodSafe != null)
			{
				return Delegate.CreateDelegate(typeof(T), caller, methodSafe, throwOnBindFailure: false) as T;
			}
			return result;
		}

		public static T CreateDelegate<T>(this MethodInfo method, object caller) where T : Delegate
		{
			T result = null;
			if (method != null)
			{
				return Delegate.CreateDelegate(typeof(T), caller, method, throwOnBindFailure: false) as T;
			}
			return result;
		}

		public static Func<T> CreateGetDelegate<T>(this Type type, string property, object caller)
		{
			Func<T> result = null;
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (string.IsNullOrEmpty(property))
			{
				throw new ArgumentNullException("property");
			}
			MethodInfo methodInfo = type.GetPropertySafe<T>(property, isStatic: false)?.GetGetMethod(nonPublic: true);
			if (methodInfo != null)
			{
				result = Delegate.CreateDelegate(typeof(Func<T>), caller, methodInfo, throwOnBindFailure: false) as Func<T>;
			}
			return result;
		}

		public static Func<T> CreateGetDelegate<T>(this PropertyInfo property, object caller)
		{
			Func<T> result = null;
			MethodInfo methodInfo = property?.GetGetMethod(nonPublic: true);
			if (methodInfo != null && typeof(T).IsAssignableFrom(property.PropertyType))
			{
				result = Delegate.CreateDelegate(typeof(Func<T>), caller, methodInfo, throwOnBindFailure: false) as Func<T>;
			}
			return result;
		}

		public static Action<T> CreateSetDelegate<T>(this Type type, string property, object caller)
		{
			Action<T> result = null;
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (string.IsNullOrEmpty(property))
			{
				throw new ArgumentNullException("property");
			}
			MethodInfo methodInfo = type.GetPropertySafe<T>(property, isStatic: false)?.GetSetMethod(nonPublic: true);
			if (methodInfo != null)
			{
				result = Delegate.CreateDelegate(typeof(Action<T>), caller, methodInfo, throwOnBindFailure: false) as Action<T>;
			}
			return result;
		}

		public static Action<T> CreateSetDelegate<T>(this PropertyInfo property, object caller)
		{
			Action<T> result = null;
			MethodInfo methodInfo = property?.GetSetMethod(nonPublic: true);
			if (methodInfo != null && property.PropertyType.IsAssignableFrom(typeof(T)))
			{
				result = Delegate.CreateDelegate(typeof(Action<T>), caller, methodInfo, throwOnBindFailure: false) as Action<T>;
			}
			return result;
		}

		public static T CreateStaticDelegate<T>(this Type type, string method, params Type[] argumentTypes) where T : Delegate
		{
			T result = null;
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (string.IsNullOrEmpty(method))
			{
				throw new ArgumentNullException("method");
			}
			MethodInfo methodSafe = type.GetMethodSafe(method, isStatic: true, argumentTypes);
			if (methodSafe != null)
			{
				return Delegate.CreateDelegate(typeof(T), methodSafe, throwOnBindFailure: false) as T;
			}
			return result;
		}

		private static IEnumerable<CodeInstruction> DoReplaceMethodCalls(IEnumerable<CodeInstruction> method, IDictionary<MethodInfo, MethodInfo> translation)
		{
			MethodInfo remove = RemoveCall;
			int replaced = 0;
			foreach (CodeInstruction item in method)
			{
				OpCode opcode = item.opcode;
				if ((opcode == OpCodes.Call || opcode == OpCodes.Calli || opcode == OpCodes.Callvirt) && item.operand is MethodInfo methodInfo && translation.TryGetValue(methodInfo, out var value))
				{
					if (value != null && value != remove)
					{
						item.opcode = (value.IsStatic ? OpCodes.Call : OpCodes.Callvirt);
						item.operand = value;
						yield return item;
					}
					else
					{
						int n = methodInfo.GetParameters().Length;
						if (!methodInfo.IsStatic)
						{
							n++;
						}
						item.opcode = ((n == 0) ? OpCodes.Nop : OpCodes.Pop);
						item.operand = null;
						yield return item;
						for (int i = 0; i < n - 1; i++)
						{
							yield return new CodeInstruction(OpCodes.Pop, (object)null);
						}
					}
					replaced++;
				}
				else
				{
					yield return item;
				}
			}
		}

		public static void DumpMethodBody(IEnumerable<CodeInstruction> opcodes)
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Invalid comparison between Unknown and I4
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			StringBuilder stringBuilder = new StringBuilder(1024);
			stringBuilder.AppendLine("METHOD BODY:");
			foreach (CodeInstruction opcode in opcodes)
			{
				foreach (ExceptionBlock block in opcode.blocks)
				{
					ExceptionBlockType blockType = block.blockType;
					if ((int)blockType == 5)
					{
						stringBuilder.AppendLine("}");
						continue;
					}
					if ((int)blockType != 0)
					{
						stringBuilder.Append("} ");
					}
					stringBuilder.Append(block.blockType);
					stringBuilder.AppendLine(" {");
				}
				foreach (Label label in opcode.labels)
				{
					stringBuilder.Append(label.GetHashCode());
					stringBuilder.Append(": ");
				}
				stringBuilder.Append('\t');
				stringBuilder.Append(opcode.opcode);
				object operand = opcode.operand;
				if (operand != null)
				{
					stringBuilder.Append('\t');
					if (operand is Label)
					{
						stringBuilder.Append(operand.GetHashCode());
					}
					else if (operand is MethodBase method)
					{
						FormatMethodCall(stringBuilder, method);
					}
					else
					{
						stringBuilder.Append(FormatArgument(operand));
					}
				}
				stringBuilder.AppendLine();
			}
			PUtil.LogDebug(stringBuilder.ToString());
		}

		private static string FormatArgument(object argument)
		{
			if (argument == null)
			{
				return "NULL";
			}
			if (argument is MethodBase methodBase)
			{
				return GeneralExtensions.FullDescription(methodBase);
			}
			if (argument is FieldInfo fieldInfo)
			{
				return GeneralExtensions.FullDescription(fieldInfo.FieldType) + " " + GeneralExtensions.FullDescription(fieldInfo.DeclaringType) + "::" + fieldInfo.Name;
			}
			if (argument is Label label)
			{
				return $"Label{label.GetHashCode()}";
			}
			if (argument is Label[] array)
			{
				int num = array.Length;
				string[] array2 = new string[num];
				for (int i = 0; i < num; i++)
				{
					array2[i] = array[i].GetHashCode().ToString();
				}
				return "Labels" + array2.Join();
			}
			if (argument is LocalBuilder localBuilder)
			{
				return $"{localBuilder.LocalIndex} ({localBuilder.LocalType})";
			}
			if (argument is string text)
			{
				return GeneralExtensions.ToLiteral(text, "\"");
			}
			return argument.ToString().Trim();
		}

		private static void FormatMethodCall(StringBuilder result, MethodBase method)
		{
			bool flag = true;
			Type declaringType = method.DeclaringType;
			if (method is MethodInfo methodInfo)
			{
				result.Append(methodInfo.ReturnType.Name);
				result.Append(' ');
			}
			if (declaringType != null)
			{
				result.Append(declaringType.Name);
				result.Append('.');
			}
			result.Append(method.Name);
			result.Append('(');
			ParameterInfo[] parameters = method.GetParameters();
			foreach (ParameterInfo parameterInfo in parameters)
			{
				string name = parameterInfo.Name;
				if (!flag)
				{
					result.Append(", ");
				}
				result.Append(parameterInfo.ParameterType.Name);
				if (!string.IsNullOrEmpty(name))
				{
					result.Append(' ');
					result.Append(name);
				}
				if (parameterInfo.IsOptional)
				{
					result.Append(" = ");
					result.Append(parameterInfo.DefaultValue);
				}
				flag = false;
			}
			result.Append(')');
		}

		public static FieldInfo GetFieldSafe(this Type type, string fieldName, bool isStatic)
		{
			FieldInfo result = null;
			if (type != null && !string.IsNullOrEmpty(fieldName))
			{
				try
				{
					BindingFlags bindingFlags = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
					result = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags);
				}
				catch (AmbiguousMatchException thrown)
				{
					PUtil.LogException(thrown);
				}
			}
			return result;
		}

		public static CodeInstruction GetMatchingStoreInstruction(CodeInstruction load)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Expected O, but got Unknown
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Expected O, but got Unknown
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Expected O, but got Unknown
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Expected O, but got Unknown
			OpCode opcode = load.opcode;
			if (opcode == OpCodes.Ldloc)
			{
				return new CodeInstruction(OpCodes.Stloc, load.operand);
			}
			if (opcode == OpCodes.Ldloc_S)
			{
				return new CodeInstruction(OpCodes.Stloc_S, load.operand);
			}
			if (opcode == OpCodes.Ldloc_0)
			{
				return new CodeInstruction(OpCodes.Stloc_0, (object)null);
			}
			if (opcode == OpCodes.Ldloc_1)
			{
				return new CodeInstruction(OpCodes.Stloc_1, (object)null);
			}
			if (opcode == OpCodes.Ldloc_2)
			{
				return new CodeInstruction(OpCodes.Stloc_2, (object)null);
			}
			if (opcode == OpCodes.Ldloc_3)
			{
				return new CodeInstruction(OpCodes.Stloc_3, (object)null);
			}
			return new CodeInstruction(OpCodes.Pop, (object)null);
		}

		public static MethodInfo GetMethodSafe(this Type type, string methodName, bool isStatic, params Type[] arguments)
		{
			MethodInfo result = null;
			if (type != null && arguments != null && !string.IsNullOrEmpty(methodName))
			{
				try
				{
					BindingFlags bindingFlags = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
					result = ((arguments.Length != 1 || !(arguments[0] == null)) ? type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags, null, arguments, new ParameterModifier[arguments.Length]) : type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags));
				}
				catch (AmbiguousMatchException thrown)
				{
					PUtil.LogException(thrown);
				}
			}
			return result;
		}

		public static MethodInfo GetOverloadWithMostArguments(this Type type, string methodName, bool isStatic, params Type[] arguments)
		{
			MethodInfo result = null;
			if (type != null && arguments != null && !string.IsNullOrEmpty(methodName))
			{
				MethodInfo[] methods = type.GetMethods((BindingFlags)(0x30 | (isStatic ? 8 : 4)));
				int num = methods.Length;
				int num2 = -1;
				for (int i = 0; i < num; i++)
				{
					MethodInfo methodInfo = methods[i];
					int num3;
					if (methodInfo.Name == methodName && (num3 = ParametersMatch(methodInfo, arguments)) > num2)
					{
						num2 = num3;
						result = methodInfo;
					}
				}
			}
			return result;
		}

		public static PropertyInfo GetPropertySafe<T>(this Type type, string propName, bool isStatic)
		{
			PropertyInfo result = null;
			if (type != null && !string.IsNullOrEmpty(propName))
			{
				try
				{
					BindingFlags bindingFlags = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
					result = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags, null, typeof(T), Type.EmptyTypes, null);
				}
				catch (AmbiguousMatchException thrown)
				{
					PUtil.LogException(thrown);
				}
			}
			return result;
		}

		public static PropertyInfo GetPropertyIndexedSafe<T>(this Type type, string propName, bool isStatic, params Type[] arguments)
		{
			PropertyInfo result = null;
			if (type != null && arguments != null && !string.IsNullOrEmpty(propName))
			{
				try
				{
					BindingFlags bindingFlags = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
					result = type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | bindingFlags, null, typeof(T), arguments, new ParameterModifier[arguments.Length]);
				}
				catch (AmbiguousMatchException thrown)
				{
					PUtil.LogException(thrown);
				}
			}
			return result;
		}

		public static Type GetTypeSafe(string name, string assemblyName = null)
		{
			Type type = null;
			if (string.IsNullOrEmpty(assemblyName))
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					try
					{
						type = assembly.GetType(name, throwOnError: false);
					}
					catch (IOException)
					{
					}
					catch (BadImageFormatException)
					{
					}
					if (type != null)
					{
						break;
					}
				}
			}
			else
			{
				try
				{
					type = Type.GetType(name + ", " + assemblyName, throwOnError: false);
				}
				catch (TargetInvocationException thrown)
				{
					PUtil.LogWarning("Unable to load type {0} from assembly {1}:".F(name, assemblyName));
					PUtil.LogExcWarn(thrown);
				}
				catch (ArgumentException thrown2)
				{
					PUtil.LogWarning("Unable to load type {0} from assembly {1}:".F(name, assemblyName));
					PUtil.LogExcWarn(thrown2);
				}
				catch (ReflectionTypeLoadException ex3)
				{
					PUtil.LogWarning("Unable to load type {0} from assembly {1}:".F(name, assemblyName));
					Exception[] loaderExceptions = ex3.LoaderExceptions;
					foreach (Exception ex4 in loaderExceptions)
					{
						if (ex4 != null)
						{
							PUtil.LogExcWarn(ex4);
						}
					}
				}
				catch (IOException)
				{
				}
				catch (BadImageFormatException)
				{
				}
			}
			return type;
		}

		public static bool HasPatchWithMethodName(Harmony instance, MethodBase target, HarmonyPatchType type, string name)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Expected I4, but got Unknown
			bool flag = false;
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			Patches patchInfo = Harmony.GetPatchInfo(target);
			if (patchInfo != null)
			{
				ICollection<Patch> collection;
				switch ((int)type)
				{
				case 1:
					collection = patchInfo.Prefixes;
					break;
				case 2:
					collection = patchInfo.Postfixes;
					break;
				case 3:
					collection = patchInfo.Transpilers;
					break;
				default:
					if (patchInfo.Transpilers != null)
					{
						flag = HasPatchWithMethodName(patchInfo.Transpilers, name);
					}
					if (patchInfo.Prefixes != null)
					{
						flag = flag || HasPatchWithMethodName(patchInfo.Prefixes, name);
					}
					collection = patchInfo.Postfixes;
					break;
				}
				if (collection != null)
				{
					flag = flag || HasPatchWithMethodName(collection, name);
				}
			}
			return flag;
		}

		private static bool HasPatchWithMethodName(IEnumerable<Patch> patchList, string name)
		{
			bool result = false;
			foreach (Patch patch in patchList)
			{
				if (patch.PatchMethod.Name == name)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public static bool IsConditionalBranchInstruction(this OpCode opcode)
		{
			return PTranspilerTools.IsConditionalBranchInstruction(opcode);
		}

		[Obsolete("Do not use this method in production code. Make sure to remove it in release builds, or disable it with #if DEBUG.")]
		public static void LogAllExceptions()
		{
			PUtil.LogWarning("PLib in mod " + Assembly.GetCallingAssembly().GetName()?.Name + " is logging ALL unhandled exceptions!");
			PTranspilerTools.LogAllExceptions();
		}

		[Obsolete("Do not use this method in production code. Make sure to remove it in release builds, or disable it with #if DEBUG.")]
		public static void LogAllFailedAsserts()
		{
			PUtil.LogWarning("PLib in mod " + Assembly.GetCallingAssembly().GetName()?.Name + " is logging ALL failed assertions!");
			PTranspilerTools.LogAllFailedAsserts();
		}

		private static int ParametersMatch(MethodBase method, Type[] required)
		{
			int result = -1;
			ParameterInfo[] parameters = method.GetParameters();
			int num = parameters.Length;
			int num2 = required.Length;
			if (num2 == 1 && required[0] == null)
			{
				result = num;
			}
			else if (num >= num2)
			{
				bool flag = true;
				for (int i = 0; i < num2 && flag; i++)
				{
					flag = parameters[i].ParameterType == required[i];
				}
				if (flag)
				{
					result = num;
				}
			}
			return result;
		}

		public static IEnumerable<CodeInstruction> ReplaceConstant(IEnumerable<CodeInstruction> method, double oldValue, double newValue, bool all = false)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			int replaced = 0;
			foreach (CodeInstruction item in method)
			{
				if (item.opcode == OpCodes.Ldc_R8 && item.operand is double num && num == oldValue)
				{
					if (all || replaced == 0)
					{
						item.operand = newValue;
					}
					replaced++;
				}
				yield return item;
			}
		}

		public static IEnumerable<CodeInstruction> ReplaceConstant(IEnumerable<CodeInstruction> method, float oldValue, float newValue, bool all = false)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			int replaced = 0;
			foreach (CodeInstruction item in method)
			{
				if (item.opcode == OpCodes.Ldc_R4 && item.operand is float num && num == oldValue)
				{
					if (all || replaced == 0)
					{
						item.operand = newValue;
					}
					replaced++;
				}
				yield return item;
			}
		}

		public static IEnumerable<CodeInstruction> ReplaceConstant(IEnumerable<CodeInstruction> method, int oldValue, int newValue, bool all = false)
		{
			int replaced = 0;
			bool quickCode = oldValue >= -1 && oldValue <= 8;
			OpCode qc = OpCodes.Nop;
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			if (quickCode)
			{
				qc = PTranspilerTools.LOAD_INT[oldValue + 1];
			}
			foreach (CodeInstruction item in method)
			{
				OpCode opcode = item.opcode;
				object operand = item.operand;
				if ((opcode == OpCodes.Ldc_I4 && operand is int num && num == oldValue) || (opcode == OpCodes.Ldc_I4_S && ((operand is byte b && b == oldValue) || (operand is sbyte b2 && b2 == oldValue))) || (quickCode && qc == opcode))
				{
					if (all || replaced == 0)
					{
						PTranspilerTools.ModifyLoadI4(item, newValue);
					}
					replaced++;
				}
				yield return item;
			}
		}

		public static IEnumerable<CodeInstruction> ReplaceConstant(IEnumerable<CodeInstruction> method, long oldValue, long newValue, bool all = false)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			int replaced = 0;
			foreach (CodeInstruction item in method)
			{
				if (item.opcode == OpCodes.Ldc_I8 && item.operand is long num && num == oldValue)
				{
					if (all || replaced == 0)
					{
						item.operand = newValue;
					}
					replaced++;
				}
				yield return item;
			}
		}

		public static IEnumerable<CodeInstruction> RemoveMethodCall(IEnumerable<CodeInstruction> method, MethodInfo victim)
		{
			return ReplaceMethodCallSafe(method, new Dictionary<MethodInfo, MethodInfo> { { victim, RemoveCall } });
		}

		private static void RemoveMethodCallPrivate()
		{
		}

		[Obsolete("This method is unsafe. Use the RemoveMethodCall or ReplaceMethodCallSafe versions instead.")]
		public static IEnumerable<CodeInstruction> ReplaceMethodCall(IEnumerable<CodeInstruction> method, MethodInfo victim, MethodInfo newMethod = null)
		{
			if (newMethod == null)
			{
				newMethod = RemoveCall;
			}
			return ReplaceMethodCallSafe(method, new Dictionary<MethodInfo, MethodInfo> { { victim, newMethod } });
		}

		public static IEnumerable<CodeInstruction> ReplaceMethodCallSafe(IEnumerable<CodeInstruction> method, MethodInfo victim, MethodInfo newMethod)
		{
			if (newMethod == null)
			{
				throw new ArgumentNullException("newMethod");
			}
			return ReplaceMethodCallSafe(method, new Dictionary<MethodInfo, MethodInfo> { { victim, newMethod } });
		}

		[Obsolete("This method is unsafe. Use ReplaceMethodCallSafe instead.")]
		public static IEnumerable<CodeInstruction> ReplaceMethodCall(IEnumerable<CodeInstruction> method, IDictionary<MethodInfo, MethodInfo> translation)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			if (translation == null)
			{
				throw new ArgumentNullException("translation");
			}
			foreach (KeyValuePair<MethodInfo, MethodInfo> item in translation)
			{
				MethodInfo key = item.Key;
				MethodInfo value = item.Value;
				if (key == null)
				{
					throw new ArgumentNullException("victim");
				}
				if (value != null)
				{
					PTranspilerTools.CompareMethodParams(key, key.GetParameterTypes(), value);
				}
				else if (key.ReturnType != typeof(void))
				{
					throw new ArgumentException("Cannot remove method {0} with a return value".F(key.Name));
				}
			}
			return DoReplaceMethodCalls(method, translation);
		}

		public static IEnumerable<CodeInstruction> ReplaceMethodCallSafe(IEnumerable<CodeInstruction> method, IDictionary<MethodInfo, MethodInfo> translation)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			if (translation == null)
			{
				throw new ArgumentNullException("translation");
			}
			MethodInfo removeCall = RemoveCall;
			foreach (KeyValuePair<MethodInfo, MethodInfo> item in translation)
			{
				MethodInfo key = item.Key;
				MethodInfo value = item.Value;
				if (key == null)
				{
					throw new ArgumentNullException("victim");
				}
				if (value == null)
				{
					throw new ArgumentNullException("newMethod");
				}
				if (value == removeCall)
				{
					if (key.ReturnType != typeof(void))
					{
						throw new ArgumentException("Cannot remove method {0} with a return value".F(key.Name));
					}
				}
				else
				{
					PTranspilerTools.CompareMethodParams(key, key.GetParameterTypes(), value);
				}
			}
			return DoReplaceMethodCalls(method, translation);
		}

		public static bool TryGetFieldValue<T>(Type type, string name, out T value)
		{
			bool result = false;
			if (type != null && !string.IsNullOrEmpty(name))
			{
				FieldInfo fieldSafe = type.GetFieldSafe(name, isStatic: true);
				if (fieldSafe != null && fieldSafe.GetValue(null) is T val)
				{
					result = true;
					value = val;
				}
				else
				{
					value = default(T);
				}
			}
			else
			{
				value = default(T);
			}
			return result;
		}

		public static bool TryGetFieldValue<T>(object source, string name, out T value)
		{
			if (source != null && !string.IsNullOrEmpty(name))
			{
				FieldInfo fieldSafe = source.GetType().GetFieldSafe(name, isStatic: false);
				if (fieldSafe != null && fieldSafe.GetValue(source) is T val)
				{
					value = val;
					return true;
				}
			}
			value = default(T);
			return false;
		}

		public static bool TryGetPropertyValue<T>(object source, string name, out T value)
		{
			if (source != null && !string.IsNullOrEmpty(name))
			{
				PropertyInfo propertySafe = source.GetType().GetPropertySafe<T>(name, isStatic: false);
				if (propertySafe != null && propertySafe.GetIndexParameters().Length < 1 && propertySafe.GetValue(source, null) is T val)
				{
					value = val;
					return true;
				}
			}
			value = default(T);
			return false;
		}

		public static IEnumerable<CodeInstruction> WrapWithErrorLogger(IEnumerable<CodeInstruction> method, ILGenerator generator)
		{
			MethodInfo logger = typeof(PUtil).GetMethodSafe("LogException", true, typeof(Exception));
			IEnumerator<CodeInstruction> ee = method.GetEnumerator();
			if (ee.MoveNext())
			{
				bool isFirst = true;
				Label endMethod = generator.DefineLabel();
				CodeInstruction last;
				bool hasNext;
				do
				{
					last = ee.Current;
					if (isFirst)
					{
						last.blocks.Add(new ExceptionBlock((ExceptionBlockType)0, (Type)null));
					}
					hasNext = ee.MoveNext();
					isFirst = false;
					if (hasNext)
					{
						yield return last;
					}
				}
				while (hasNext);
				last.opcode = OpCodes.Nop;
				last.operand = null;
				yield return last;
				yield return new CodeInstruction(OpCodes.Leave, (object)endMethod);
				CodeInstruction val = ((logger != null) ? new CodeInstruction(OpCodes.Call, (object)logger) : new CodeInstruction(OpCodes.Pop, (object)null));
				val.blocks.Add(new ExceptionBlock((ExceptionBlockType)1, typeof(Exception)));
				yield return val;
				yield return new CodeInstruction(OpCodes.Rethrow, (object)null);
				CodeInstruction val2 = new CodeInstruction(OpCodes.Leave, (object)endMethod);
				val2.blocks.Add(new ExceptionBlock((ExceptionBlockType)5, (Type)null));
				yield return val2;
				CodeInstruction val3 = new CodeInstruction(OpCodes.Ret, (object)null);
				val3.labels.Add(endMethod);
				yield return val3;
			}
			ee.Dispose();
		}
	}
	public static class PRegistry
	{
		private static IPLibRegistry instance = null;

		private static readonly object instanceLock = new object();

		public static IPLibRegistry Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						Init();
					}
				}
				return instance;
			}
		}

		public static T GetData<T>(string key)
		{
			T result = default(T);
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			IDictionary<string, object> modData = Instance.ModData;
			if (modData != null && modData.TryGetValue(key, out var value) && value is T result2)
			{
				return result2;
			}
			return result;
		}

		private static void Init()
		{
			Global obj = Global.Instance;
			GameObject val = ((obj != null) ? ((Component)obj).gameObject : null);
			if ((Object)(object)val != (Object)null)
			{
				Component component = val.GetComponent("PRegistryComponent");
				if ((Object)(object)component == (Object)null)
				{
					PRegistryComponent pRegistryComponent = val.AddComponent<PRegistryComponent>();
					string name = ((object)pRegistryComponent).GetType().Name;
					if (name != "PRegistryComponent")
					{
						LogPatchWarning("PRegistryComponent has the type name " + name + "; this may be the result of ILMerging PLib more than once!");
					}
					pRegistryComponent.ApplyBootstrapper();
					instance = pRegistryComponent;
				}
				else
				{
					instance = new PRemoteRegistry(component);
				}
			}
			else
			{
				instance = null;
			}
			if (instance != null)
			{
				new PLibCorePatches().Register(instance);
			}
		}

		internal static void LogPatchDebug(string message)
		{
			Debug.LogFormat("[PLibPatches] {0}", new object[1] { message });
		}

		internal static void LogPatchWarning(string message)
		{
			Debug.LogWarningFormat("[PLibPatches] {0}", new object[1] { message });
		}

		public static void PutData(string key, object value)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			IDictionary<string, object> modData = Instance.ModData;
			if (modData != null)
			{
				if (modData.ContainsKey(key))
				{
					modData[key] = value;
				}
				else
				{
					modData.Add(key, value);
				}
			}
		}
	}
	internal sealed class PRegistryComponent : MonoBehaviour, IPLibRegistry
	{
		internal const string PLIB_HARMONY = "PeterHan.PLib";

		private static PRegistryComponent instance;

		private static bool instantiated;

		private readonly ConcurrentDictionary<string, PVersionList> forwardedComponents;

		private readonly ConcurrentDictionary<string, object> instantiatedComponents;

		private readonly ConcurrentDictionary<string, PForwardedComponent> latestComponents;

		public IDictionary<string, object> ModData { get; }

		public Harmony PLibInstance { get; }

		private static void ApplyLatest()
		{
			bool flag = false;
			if ((Object)(object)instance != (Object)null)
			{
				lock (instance)
				{
					if (!instantiated)
					{
						flag = (instantiated = true);
					}
				}
			}
			if (flag)
			{
				instance.Instantiate();
			}
		}

		internal PRegistryComponent()
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			if ((Object)(object)instance == (Object)null)
			{
				instance = this;
			}
			ModData = new ConcurrentDictionary<string, object>(2, 64);
			forwardedComponents = new ConcurrentDictionary<string, PVersionList>(2, 32);
			instantiatedComponents = new ConcurrentDictionary<string, object>(2, 32);
			latestComponents = new ConcurrentDictionary<string, PForwardedComponent>(2, 32);
			PLibInstance = new Harmony("PeterHan.PLib");
		}

		public void AddCandidateVersion(PForwardedComponent instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			AddCandidateVersion(instance.ID, instance);
		}

		private void AddCandidateVersion(string id, PForwardedComponent instance)
		{
			PVersionList orAdd = forwardedComponents.GetOrAdd(id, (string _) => new PVersionList());
			if (orAdd == null)
			{
				PRegistry.LogPatchWarning("Missing version info for component type " + id);
				return;
			}
			List<PForwardedComponent> components = orAdd.Components;
			bool flag = components.Count < 1;
			components.Add(instance);
			if (flag)
			{
				instance.Bootstrap(PLibInstance);
			}
		}

		internal void ApplyBootstrapper()
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Expected O, but got Unknown
			try
			{
				PLibInstance.Patch(typeof(Mod), "PostLoad", new HarmonyMethod(typeof(PRegistryComponent), "ApplyLatest", (Type[])null));
			}
			catch (AmbiguousMatchException thrown)
			{
				PUtil.LogException(thrown);
			}
			catch (ArgumentException thrown2)
			{
				PUtil.LogException(thrown2);
			}
			catch (TypeLoadException thrown3)
			{
				PUtil.LogException(thrown3);
			}
		}

		internal void DoAddCandidateVersion(object instance)
		{
			AddCandidateVersion(instance.GetType().FullName, new PRemoteComponent(instance));
		}

		internal ICollection DoGetAllComponents(string id)
		{
			if (!forwardedComponents.TryGetValue(id, out var value))
			{
				value = null;
			}
			return value?.Components;
		}

		internal object DoGetLatestVersion(string id)
		{
			if (!instantiatedComponents.TryGetValue(id, out var value))
			{
				return null;
			}
			return value;
		}

		public IEnumerable<PForwardedComponent> GetAllComponents(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}
			if (!forwardedComponents.TryGetValue(id, out var value))
			{
				value = null;
			}
			return value?.Components;
		}

		public PForwardedComponent GetLatestVersion(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}
			if (!latestComponents.TryGetValue(id, out var value))
			{
				return null;
			}
			return value;
		}

		public object GetSharedData(string id)
		{
			if (!forwardedComponents.TryGetValue(id, out var value))
			{
				value = null;
			}
			return value?.SharedData;
		}

		public void Instantiate()
		{
			foreach (KeyValuePair<string, PVersionList> forwardedComponent in forwardedComponents)
			{
				List<PForwardedComponent> components = forwardedComponent.Value.Components;
				int count = components.Count;
				if (count > 0)
				{
					string key = forwardedComponent.Key;
					components.Sort();
					PForwardedComponent pForwardedComponent = components[count - 1];
					latestComponents.GetOrAdd(key, pForwardedComponent);
					try
					{
						instantiatedComponents.GetOrAdd(key, pForwardedComponent?.DoInitialize(PLibInstance));
					}
					catch (Exception thrown)
					{
						PRegistry.LogPatchWarning("Error when instantiating component " + key + ":");
						PUtil.LogException(thrown);
					}
				}
			}
			foreach (KeyValuePair<string, PForwardedComponent> latestComponent in latestComponents)
			{
				try
				{
					latestComponent.Value.PostInitialize(PLibInstance);
				}
				catch (Exception thrown2)
				{
					PRegistry.LogPatchWarning("Error when instantiating component " + latestComponent.Key + ":");
					PUtil.LogException(thrown2);
				}
			}
		}

		public void SetSharedData(string id, object data)
		{
			if (forwardedComponents.TryGetValue(id, out var value))
			{
				value.SharedData = data;
			}
		}

		public override string ToString()
		{
			return forwardedComponents.ToString();
		}
	}
	public class PriorityQueue<T> where T : IComparable<T>
	{
		private readonly IList<T> heap;

		public int Count => heap.Count;

		private static int ChildIndex(int index)
		{
			return 2 * index + 1;
		}

		private static int ParentIndex(int index)
		{
			return (index - 1) / 2;
		}

		public PriorityQueue()
			: this(32)
		{
		}

		public PriorityQueue(int capacity)
		{
			if (capacity < 1)
			{
				throw new ArgumentException("capacity > 0");
			}
			heap = new List<T>(Math.Max(capacity, 8));
		}

		public void Clear()
		{
			heap.Clear();
		}

		public bool Contains(T key)
		{
			return heap.Contains(key);
		}

		public T Dequeue()
		{
			int index = 0;
			int count = heap.Count;
			if (count == 0)
			{
				throw new InvalidOperationException("Queue is empty");
			}
			T result = heap[0];
			heap[0] = heap[--count];
			heap.RemoveAt(count);
			int num;
			while ((num = ChildIndex(index)) < count)
			{
				T value = heap[index];
				T val = heap[num];
				if (num < count - 1)
				{
					T val2 = heap[num + 1];
					if (val.CompareTo(val2) > 0)
					{
						num++;
						val = val2;
					}
				}
				if (value.CompareTo(val) < 0)
				{
					break;
				}
				heap[num] = value;
				heap[index] = val;
				index = num;
			}
			return result;
		}

		public void Enqueue(T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			int num = heap.Count;
			heap.Add(item);
			while (num > 0)
			{
				int num2 = ParentIndex(num);
				T value = heap[num];
				T val = heap[num2];
				if (value.CompareTo(val) <= 0)
				{
					heap[num2] = value;
					heap[num] = val;
					num = num2;
					continue;
				}
				break;
			}
		}

		public T Peek()
		{
			if (Count == 0)
			{
				throw new InvalidOperationException("Queue is empty");
			}
			return heap[0];
		}

		public override string ToString()
		{
			return heap.ToString();
		}
	}
	public sealed class PriorityDictionary<K, V> : PriorityQueue<PriorityDictionary<K, V>.PriorityQueuePair> where K : IComparable<K>
	{
		public sealed class PriorityQueuePair : IComparable<PriorityQueuePair>
		{
			public K Key { get; }

			public V Value { get; }

			public PriorityQueuePair(K key, V value)
			{
				if (key == null)
				{
					throw new ArgumentNullException("key");
				}
				Key = key;
				Value = value;
			}

			public int CompareTo(PriorityQueuePair other)
			{
				if (other == null)
				{
					throw new ArgumentNullException("other");
				}
				return Key.CompareTo(other.Key);
			}

			public override bool Equals(object obj)
			{
				if (obj is PriorityQueuePair { Key: var key })
				{
					return key.Equals(Key);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return Key.GetHashCode();
			}

			public override string ToString()
			{
				return "PriorityQueueItem[key=" + Key?.ToString() + ",value=" + Value?.ToString() + "]";
			}
		}

		public PriorityDictionary()
		{
		}

		public PriorityDictionary(int capacity)
			: base(capacity)
		{
		}

		public void Dequeue(out K key, out V value)
		{
			PriorityQueuePair priorityQueuePair = Dequeue();
			key = priorityQueuePair.Key;
			value = priorityQueuePair.Value;
		}

		public void Enqueue(K key, V value)
		{
			Enqueue(new PriorityQueuePair(key, value));
		}

		public void Peek(out K key, out V value)
		{
			PriorityQueuePair priorityQueuePair = Peek();
			key = priorityQueuePair.Key;
			value = priorityQueuePair.Value;
		}
	}
	public static class PStateMachines
	{
		public static State<T, I, IStateMachineTarget, object> CreateState<T, I>(this GameStateMachine<T, I> sm, string name) where T : GameStateMachine<T, I, IStateMachineTarget, object> where I : GameInstance<T, I, IStateMachineTarget, object>
		{
			State<T, I, IStateMachineTarget, object> val = new State<T, I, IStateMachineTarget, object>();
			if (string.IsNullOrEmpty(name))
			{
				name = "State";
			}
			if (sm == null)
			{
				throw new ArgumentNullException("sm");
			}
			((BaseState)val).defaultState = ((StateMachine)sm).GetDefaultState();
			((StateMachine)sm).CreateStates((object)val);
			((StateMachine<T, I, IStateMachineTarget, object>)(object)sm).BindState((State<T, I, IStateMachineTarget, object>)(object)((GameStateMachine<T, I, IStateMachineTarget, object>)(object)sm).root, (State<T, I, IStateMachineTarget, object>)(object)val, name);
			return val;
		}

		public static State<T, I, M, object> CreateState<T, I, M>(this GameStateMachine<T, I, M> sm, string name) where T : GameStateMachine<T, I, M, object> where I : GameInstance<T, I, M, object> where M : IStateMachineTarget
		{
			State<T, I, M, object> val = new State<T, I, M, object>();
			if (string.IsNullOrEmpty(name))
			{
				name = "State";
			}
			if (sm == null)
			{
				throw new ArgumentNullException("sm");
			}
			((BaseState)val).defaultState = ((StateMachine)sm).GetDefaultState();
			((StateMachine)sm).CreateStates((object)val);
			((StateMachine<T, I, M, object>)(object)sm).BindState((State<T, I, M, object>)(object)((GameStateMachine<T, I, M, object>)(object)sm).root, (State<T, I, M, object>)(object)val, name);
			return val;
		}

		public static void ClearEnterActions(this BaseState state)
		{
			state?.enterActions.Clear();
		}

		public static void ClearExitActions(this BaseState state)
		{
			state?.exitActions.Clear();
		}

		public static void ClearTransitions(this BaseState state)
		{
			state?.transitions.Clear();
		}
	}
	internal static class PTranspilerTools
	{
		private static readonly ISet<OpCode> BRANCH_CODES;

		internal static readonly OpCode[] LOAD_INT;

		static PTranspilerTools()
		{
			LOAD_INT = new OpCode[10]
			{
				OpCodes.Ldc_I4_M1,
				OpCodes.Ldc_I4_0,
				OpCodes.Ldc_I4_1,
				OpCodes.Ldc_I4_2,
				OpCodes.Ldc_I4_3,
				OpCodes.Ldc_I4_4,
				OpCodes.Ldc_I4_5,
				OpCodes.Ldc_I4_6,
				OpCodes.Ldc_I4_7,
				OpCodes.Ldc_I4_8
			};
			BRANCH_CODES = new HashSet<OpCode>
			{
				OpCodes.Beq,
				OpCodes.Beq_S,
				OpCodes.Bge,
				OpCodes.Bge_S,
				OpCodes.Bge_Un,
				OpCodes.Bge_Un_S,
				OpCodes.Bgt,
				OpCodes.Bgt_S,
				OpCodes.Bgt_Un,
				OpCodes.Bgt_Un_S,
				OpCodes.Ble,
				OpCodes.Ble_S,
				OpCodes.Ble_Un,
				OpCodes.Ble_Un_S,
				OpCodes.Blt,
				OpCodes.Blt_S,
				OpCodes.Blt_Un,
				OpCodes.Blt_Un_S,
				OpCodes.Bne_Un,
				OpCodes.Bne_Un_S,
				OpCodes.Brfalse,
				OpCodes.Brfalse_S,
				OpCodes.Brtrue,
				OpCodes.Brtrue_S
			};
		}

		internal static void CompareMethodParams(MethodInfo victim, Type[] paramTypes, MethodInfo newMethod)
		{
			Type[] array = newMethod.GetParameterTypes();
			if (!newMethod.IsStatic)
			{
				array = PushDeclaringType(array, newMethod.DeclaringType);
			}
			if (!victim.IsStatic)
			{
				paramTypes = PushDeclaringType(paramTypes, victim.DeclaringType);
			}
			int num = paramTypes.Length;
			if (array.Length != num)
			{
				throw new ArgumentException("New method {0} ({1:D} arguments) does not match method {2} ({3:D} arguments)".F(newMethod.Name, array.Length, victim.Name, num));
			}
			for (int i = 0; i < num; i++)
			{
				if (!array[i].IsAssignableFrom(paramTypes[i]))
				{
					throw new ArgumentException("Argument {0:D}: New method type {1} does not match old method type {2}".F(i, paramTypes[i].FullName, array[i].FullName));
				}
			}
			if (!victim.ReturnType.IsAssignableFrom(newMethod.ReturnType))
			{
				throw new ArgumentException("New method {0} (returns {1}) does not match method {2} (returns {3})".F(newMethod.Name, newMethod.ReturnType, victim.Name, victim.ReturnType));
			}
		}

		private static bool GenerateBasicLoad(ILGenerator generator, Type type, object value)
		{
			bool flag = !type.IsByRef;
			if (flag)
			{
				if (type == typeof(int))
				{
					if (value is int arg)
					{
						generator.Emit(OpCodes.Ldc_I4, arg);
					}
					else
					{
						generator.Emit(OpCodes.Ldc_I4_0);
					}
				}
				else if (type == typeof(char))
				{
					if (value is char arg2)
					{
						generator.Emit(OpCodes.Ldc_I4, arg2);
					}
					else
					{
						generator.Emit(OpCodes.Ldc_I4_0);
					}
				}
				else if (type == typeof(short))
				{
					if (value is short arg3)
					{
						generator.Emit(OpCodes.Ldc_I4, arg3);
					}
					else
					{
						generator.Emit(OpCodes.Ldc_I4_0);
					}
				}
				else if (type == typeof(uint))
				{
					if (value is uint arg4)
					{
						generator.Emit(OpCodes.Ldc_I4, (int)arg4);
					}
					else
					{
						generator.Emit(OpCodes.Ldc_I4_0);
					}
				}
				else if (type == typeof(ushort))
				{
					if (value is ushort arg5)
					{
						generator.Emit(OpCodes.Ldc_I4, arg5);
					}
					else
					{
						generator.Emit(OpCodes.Ldc_I4_0);
					}
				}
				else if (type == typeof(byte))
				{
					if (value is byte arg6)
					{
						generator.Emit(OpCodes.Ldc_I4_S, arg6);
					}
					else
					{
						generator.Emit(OpCodes.Ldc_I4_0);
					}
				}
				else if (type == typeof(sbyte))
				{
					if (value is sbyte arg7)
					{
						generator.Emit(OpCodes.Ldc_I4, arg7);
					}
					else
					{
						generator.Emit(OpCodes.Ldc_I4_0);
					}
				}
				else if (type == typeof(bool))
				{
					bool flag2 = default(bool);
					int num;
					if (value is bool)
					{
						flag2 = (bool)value;
						num = 1;
					}
					else
					{
						num = 0;
					}
					generator.Emit((((uint)num & (flag2 ? 1u : 0u)) != 0) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
				}
				else if (type == typeof(long))
				{
					generator.Emit(OpCodes.Ldc_I8, (value is long num2) ? num2 : 0);
				}
				else if (type == typeof(ulong))
				{
					generator.Emit(OpCodes.Ldc_I8, (long)((value is ulong num3) ? num3 : 0));
				}
				else if (type == typeof(float))
				{
					generator.Emit(OpCodes.Ldc_R4, (value is float num4) ? num4 : 0f);
				}
				else if (type == typeof(double))
				{
					generator.Emit(OpCodes.Ldc_R8, (value is double num5) ? num5 : 0.0);
				}
				else if (type == typeof(string))
				{
					if (value == null)
					{
						generator.Emit(OpCodes.Ldnull);
					}
					else
					{
						generator.Emit(OpCodes.Ldstr, (value is string text) ? text : "");
					}
				}
				else if (type.IsPointer)
				{
					generator.Emit(OpCodes.Ldc_I4_0);
				}
				else if (!type.IsValueType)
				{
					generator.Emit(OpCodes.Ldnull);
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		internal static void GenerateDefaultLoad(ILGenerator generator, Type type, object defaultValue)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (GenerateBasicLoad(generator, type, defaultValue))
			{
				return;
			}
			if (type.IsByRef)
			{
				Type elementType = type.GetElementType();
				int localIndex = generator.DeclareLocal(elementType).LocalIndex;
				if (GenerateBasicLoad(generator, elementType, defaultValue))
				{
					generator.Emit(OpCodes.Stloc_S, localIndex);
				}
				else
				{
					generator.Emit(OpCodes.Ldloca_S, localIndex);
					generator.Emit(OpCodes.Initobj, type);
				}
				generator.Emit(OpCodes.Ldloca_S, localIndex);
			}
			else
			{
				int localIndex2 = generator.DeclareLocal(type).LocalIndex;
				generator.Emit(OpCodes.Ldloca_S, localIndex2);
				generator.Emit(OpCodes.Initobj, type);
				generator.Emit(OpCodes.Ldloc_S, localIndex2);
			}
		}

		internal static Type[] GetParameterTypes(this MethodInfo method)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			ParameterInfo[] parameters = method.GetParameters();
			int num = parameters.Length;
			Type[] array = new Type[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = parameters[i].ParameterType;
			}
			return array;
		}

		internal static bool IsConditionalBranchInstruction(OpCode opcode)
		{
			return BRANCH_CODES.Contains(opcode);
		}

		internal static void LogAllExceptions()
		{
			AppDomain.CurrentDomain.UnhandledException += OnThrown;
		}

		internal static void LogAllFailedAsserts()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			Harmony val = new Harmony("PeterHan.PLib.LogFailedAsserts");
			HarmonyMethod val2 = new HarmonyMethod(typeof(PTranspilerTools), "OnAssertFailed", (Type[])null);
			try
			{
				MethodBase methodSafe = typeof(Debug).GetMethodSafe("Assert", true, typeof(bool));
				if (methodSafe != null)
				{
					val.Patch(methodSafe, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				methodSafe = typeof(Debug).GetMethodSafe("Assert", true, typeof(bool), typeof(object));
				if (methodSafe != null)
				{
					val.Patch(methodSafe, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				methodSafe = typeof(Debug).GetMethodSafe("Assert", true, typeof(bool), typeof(object), typeof(Object));
				if (methodSafe != null)
				{
					val.Patch(methodSafe, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
			}
			catch (Exception thrown)
			{
				PUtil.LogException(thrown);
			}
		}

		internal static void ModifyLoadI4(CodeInstruction instruction, int newValue)
		{
			if (newValue >= -1 && newValue <= 8)
			{
				instruction.opcode = LOAD_INT[newValue + 1];
				instruction.operand = null;
			}
			else if (newValue >= -128 && newValue <= 127)
			{
				instruction.opcode = OpCodes.Ldc_I4_S;
				instruction.operand = (sbyte)newValue;
			}
			else
			{
				instruction.opcode = OpCodes.Ldc_I4;
				instruction.operand = newValue;
			}
		}

		internal static void OnAssertFailed(bool condition)
		{
			if (!condition)
			{
				Debug.LogError((object)"Assert is about to fail:");
				Debug.LogError((object)new StackTrace().ToString());
			}
		}

		internal static void OnThrown(object sender, UnhandledExceptionEventArgs e)
		{
			if (!e.IsTerminating)
			{
				Debug.LogError((object)("Unhandled exception on Thread " + Thread.CurrentThread.Name));
				if (e.ExceptionObject is Exception ex)
				{
					Debug.LogException(ex);
				}
				else
				{
					Debug.LogError(e.ExceptionObject);
				}
			}
		}

		internal static Type[] PushDeclaringType(Type[] types, Type declaringType)
		{
			int num = types.Length;
			Type[] array = new Type[num + 1];
			if (declaringType.IsValueType)
			{
				declaringType = declaringType.MakeByRefType();
			}
			array[0] = declaringType;
			for (int i = 0; i < num; i++)
			{
				array[i + 1] = types[i];
			}
			return array;
		}
	}
	public static class PUtil
	{
		private static volatile bool initialized;

		private static readonly object initializeLock;

		private static readonly HashSet<char> INVALID_FILE_CHARS;

		public static uint GameVersion { get; }

		static PUtil()
		{
			initialized = false;
			initializeLock = new object();
			INVALID_FILE_CHARS = new HashSet<char>(Path.GetInvalidFileNameChars());
			GameVersion = GetGameVersion();
		}

		public static IDictionary<Assembly, Mod> CreateAssemblyToModTable()
		{
			Global instance = Global.Instance;
			Dictionary<Assembly, Mod> dictionary = new Dictionary<Assembly, Mod>(32);
			if ((Object)(object)instance != (Object)null)
			{
				List<Mod> list = instance.modManager?.mods;
				if (list != null)
				{
					foreach (Mod item in list)
					{
						ICollection<Assembly> collection = item?.loaded_mod_data?.dlls;
						if (collection == null)
						{
							continue;
						}
						foreach (Assembly item2 in collection)
						{
							dictionary[item2] = item;
						}
					}
				}
			}
			return dictionary;
		}

		public static float Distance(float x1, float y1, float x2, float y2)
		{
			float num = x2 - x1;
			float num2 = y2 - y1;
			return Mathf.Sqrt(num * num + num2 * num2);
		}

		public static double Distance(double x1, double y1, double x2, double y2)
		{
			double num = x2 - x1;
			double num2 = y2 - y1;
			return Math.Sqrt(num * num + num2 * num2);
		}

		private static uint GetGameVersion()
		{
			uint result = 0u;
			if (PPatchTools.TryGetFieldValue<uint>(typeof(KleiVersion), "ChangeList", out var value))
			{
				result = value;
			}
			return result;
		}

		public static string GetModPath(Assembly modDLL)
		{
			if (modDLL == null)
			{
				throw new ArgumentNullException("modDLL");
			}
			string text = null;
			try
			{
				text = Directory.GetParent(modDLL.Location)?.FullName;
			}
			catch (NotSupportedException thrown)
			{
				LogExcWarn(thrown);
			}
			catch (SecurityException thrown2)
			{
				LogExcWarn(thrown2);
			}
			catch (IOException thrown3)
			{
				LogExcWarn(thrown3);
			}
			return text ?? Path.Combine(Manager.GetDirectory(), modDLL.GetName().Name ?? "");
		}

		public static void InitLibrary(bool logVersion = true)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			lock (initializeLock)
			{
				if (!initialized)
				{
					initialized = true;
					if (logVersion)
					{
						Debug.LogFormat("[PLib] Mod {0} initialized, version {1}", new object[2]
						{
							callingAssembly.GetNameSafe(),
							callingAssembly.GetFileVersion() ?? "Unknown"
						});
					}
				}
			}
		}

		public static bool IsValidFileName(string file)
		{
			bool flag = !string.IsNullOrEmpty(file);
			if (flag)
			{
				int length = file.Length;
				for (int i = 0; i < length && flag; i++)
				{
					if (INVALID_FILE_CHARS.Contains(file[i]))
					{
						flag = false;
					}
				}
			}
			return flag;
		}

		public static void LogDebug(object message)
		{
			Debug.LogFormat("[PLib/{0}] {1}", new object[2]
			{
				Assembly.GetCallingAssembly().GetNameSafe(),
				message
			});
		}

		public static void LogError(object message)
		{
			Debug.LogErrorFormat("[PLib/{0}] {1}", new object[2]
			{
				Assembly.GetCallingAssembly().GetNameSafe() ?? "?",
				message
			});
		}

		public static void LogException(Exception thrown)
		{
			Debug.LogErrorFormat("[PLib/{0}] {1} {2} {3}", new object[4]
			{
				Assembly.GetCallingAssembly().GetNameSafe() ?? "?",
				thrown.GetType(),
				thrown.Message,
				thrown.StackTrace
			});
		}

		public static void LogExcWarn(Exception thrown)
		{
			Debug.LogWarningFormat("[PLib/{0}] {1} {2} {3}", new object[4]
			{
				Assembly.GetCallingAssembly().GetNameSafe() ?? "?",
				thrown.GetType(),
				thrown.Message,
				thrown.StackTrace
			});
		}

		public static void LogWarning(object message)
		{
			Debug.LogWarningFormat("[PLib/{0}] {1}", new object[2]
			{
				Assembly.GetCallingAssembly().GetNameSafe() ?? "?",
				message
			});
		}

		public static void Time(Action code, string header = "Code")
		{
			if (code == null)
			{
				throw new ArgumentNullException("code");
			}
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			code();
			stopwatch.Stop();
			LogDebug("{1} took {0:D} us".F(stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency, header));
		}
	}
	internal sealed class PVersionList
	{
		public List<PForwardedComponent> Components { get; }

		public object SharedData { get; set; }

		public PVersionList()
		{
			Components = new List<PForwardedComponent>(32);
			SharedData = null;
		}

		public override string ToString()
		{
			return Components.ToString();
		}
	}
	internal sealed class PRemoteComponent : PForwardedComponent
	{
		private delegate void InitializeDelegate(Harmony instance);

		private delegate void ProcessDelegate(uint operation, object args);

		private readonly InitializeDelegate doBootstrap;

		private readonly InitializeDelegate doInitialize;

		private readonly InitializeDelegate doPostInitialize;

		private readonly Func<object> getData;

		private readonly ProcessDelegate process;

		private readonly Action<object> setData;

		private readonly Version version;

		private readonly object wrapped;

		protected override object InstanceData
		{
			get
			{
				return getData?.Invoke();
			}
			set
			{
				setData?.Invoke(value);
			}
		}

		public override Version Version => version;

		internal PRemoteComponent(object wrapped)
		{
			this.wrapped = wrapped ?? throw new ArgumentNullException("wrapped");
			if (!PPatchTools.TryGetPropertyValue<Version>(wrapped, "Version", out var value))
			{
				throw new ArgumentException("Remote component missing Version property");
			}
			version = value;
			Type type = wrapped.GetType();
			doInitialize = type.CreateDelegate<InitializeDelegate>("Initialize", wrapped, new Type[1] { typeof(Harmony) });
			if (doInitialize == null)
			{
				throw new ArgumentException("Remote component missing Initialize");
			}
			doBootstrap = type.CreateDelegate<InitializeDelegate>("Bootstrap", wrapped, new Type[1] { typeof(Harmony) });
			doPostInitialize = type.CreateDelegate<InitializeDelegate>("PostInitialize", wrapped, new Type[1] { typeof(Harmony) });
			getData = type.CreateGetDelegate<object>("InstanceData", wrapped);
			setData = type.CreateSetDelegate<object>("InstanceData", wrapped);
			process = type.CreateDelegate<ProcessDelegate>("Process", wrapped, new Type[2]
			{
				typeof(uint),
				typeof(object)
			});
		}

		public override void Bootstrap(Harmony plibInstance)
		{
			doBootstrap?.Invoke(plibInstance);
		}

		internal override object DoInitialize(Harmony plibInstance)
		{
			doInitialize(plibInstance);
			return wrapped;
		}

		public override Assembly GetOwningAssembly()
		{
			return wrapped.GetType().Assembly;
		}

		public override void Initialize(Harmony plibInstance)
		{
			DoInitialize(plibInstance);
		}

		public override void PostInitialize(Harmony plibInstance)
		{
			doPostInitialize?.Invoke(plibInstance);
		}

		public override void Process(uint operation, object args)
		{
			process?.Invoke(operation, args);
		}

		public override string ToString()
		{
			return "PRemoteComponent[ID={0},TargetType={1}]".F(base.ID, wrapped.GetType().AssemblyQualifiedName);
		}
	}
	internal sealed class PRemoteRegistry : IPLibRegistry
	{
		private delegate ICollection GetAllComponentsDelegate(string id);

		private delegate object GetObjectDelegate(string id);

		private delegate void SetObjectDelegate(string id, object value);

		private readonly Action<object> addCandidateVersion;

		private readonly GetAllComponentsDelegate getAllComponents;

		private readonly GetObjectDelegate getLatestVersion;

		private readonly GetObjectDelegate getSharedData;

		private readonly SetObjectDelegate setSharedData;

		private readonly IDictionary<string, PForwardedComponent> remoteComponents;

		public IDictionary<string, object> ModData { get; private set; }

		internal PRemoteRegistry(object instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			remoteComponents = new Dictionary<string, PForwardedComponent>(32);
			if (!PPatchTools.TryGetPropertyValue<IDictionary<string, object>>(instance, "ModData", out var value))
			{
				throw new ArgumentException("Remote instance missing ModData");
			}
			ModData = value;
			Type type = instance.GetType();
			addCandidateVersion = type.CreateDelegate<Action<object>>("DoAddCandidateVersion", instance, new Type[1] { typeof(object) });
			getAllComponents = type.CreateDelegate<GetAllComponentsDelegate>("DoGetAllComponents", instance, new Type[1] { typeof(string) });
			getLatestVersion = type.CreateDelegate<GetObjectDelegate>("DoGetLatestVersion", instance, new Type[1] { typeof(string) });
			if (addCandidateVersion == null || getLatestVersion == null || getAllComponents == null)
			{
				throw new ArgumentException("Remote instance missing candidate versions");
			}
			getSharedData = type.CreateDelegate<GetObjectDelegate>("GetSharedData", instance, new Type[1] { typeof(string) });
			setSharedData = type.CreateDelegate<SetObjectDelegate>("SetSharedData", instance, new Type[2]
			{
				typeof(string),
				typeof(object)
			});
			if (getSharedData == null || setSharedData == null)
			{
				throw new ArgumentException("Remote instance missing shared data");
			}
		}

		public void AddCandidateVersion(PForwardedComponent instance)
		{
			addCandidateVersion(instance);
		}

		public IEnumerable<PForwardedComponent> GetAllComponents(string id)
		{
			ICollection<PForwardedComponent> collection = null;
			ICollection collection2 = getAllComponents(id);
			if (collection2 != null)
			{
				collection = new List<PForwardedComponent>(collection2.Count);
				foreach (object? item2 in collection2)
				{
					if (item2 is PForwardedComponent item)
					{
						collection.Add(item);
					}
					else
					{
						collection.Add(new PRemoteComponent(item2));
					}
				}
			}
			return collection;
		}

		public PForwardedComponent GetLatestVersion(string id)
		{
			if (!remoteComponents.TryGetValue(id, out var value))
			{
				object obj = getLatestVersion(id);
				value = ((obj == null) ? null : ((!(obj is PForwardedComponent pForwardedComponent)) ? new PRemoteComponent(obj) : pForwardedComponent));
				remoteComponents.Add(id, value);
			}
			return value;
		}

		public object GetSharedData(string id)
		{
			return getSharedData(id);
		}

		public void SetSharedData(string id, object data)
		{
			setSharedData(id, data);
		}
	}
}
namespace PeterHan.PLib.Database
{
	public sealed class PCodexManager : PForwardedComponent
	{
		public const string CREATURES_DIR = "codex/Creatures";

		public const string PLANTS_DIR = "codex/Plants";

		public const string STORY_DIR = "codex/StoryTraits";

		public const string CODEX_FILES = "*.yaml";

		public const string CREATURES_CATEGORY = "CREATURES";

		public const string PLANTS_CATEGORY = "PLANTS";

		public const string STORY_CATEGORY = "STORYTRAITS";

		internal static readonly Version VERSION = new Version("4.24.0.0");

		private static readonly FieldInfo WIDGET_TAG_MAPPINGS = typeof(CodexCache).GetFieldSafe("widgetTagMappings", isStatic: true);

		private readonly ISet<string> creaturePaths;

		private readonly ISet<string> plantPaths;

		private readonly ISet<string> storyPaths;

		internal static PCodexManager Instance { get; private set; }

		public override Version Version => VERSION;

		private static void CollectEntries_Postfix(string folder, List<CodexEntry> __result, string ___baseEntryPath)
		{
			if (Instance == null)
			{
				return;
			}
			string obj = (string.IsNullOrEmpty(folder) ? ___baseEntryPath : Path.Combine(___baseEntryPath, folder));
			bool flag = false;
			if (obj.EndsWith("Creatures"))
			{
				__result.AddRange(Instance.LoadEntries("CREATURES"));
				flag = true;
			}
			if (obj.EndsWith("Plants"))
			{
				__result.AddRange(Instance.LoadEntries("PLANTS"));
				flag = true;
			}
			if (obj.EndsWith("StoryTraits"))
			{
				__result.AddRange(Instance.LoadEntries("STORYTRAITS"));
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			foreach (CodexEntry item in __result)
			{
				if (string.IsNullOrEmpty(item.sortString))
				{
					item.sortString = StringEntry.op_Implicit(Strings.Get(item.title));
				}
			}
			__result.Sort((CodexEntry x, CodexEntry y) => string.Compare(x.sortString, y.sortString, StringComparison.CurrentCulture));
		}

		private static void CollectSubEntries_Postfix(List<SubEntry> __result)
		{
			if (Instance == null)
			{
				return;
			}
			int count = __result.Count;
			__result.AddRange(Instance.LoadSubEntries());
			if (__result.Count != count)
			{
				__result.Sort((SubEntry x, SubEntry y) => string.Compare(x.title, y.title, StringComparison.CurrentCulture));
			}
		}

		private static void LoadFromDirectory(ICollection<CodexEntry> entries, string dir, string category)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Expected O, but got Unknown
			string[] array = Array.Empty<string>();
			try
			{
				array = Directory.GetFiles(dir, "*.yaml");
			}
			catch (UnauthorizedAccessException thrown)
			{
				PUtil.LogExcWarn(thrown);
			}
			catch (IOException thrown2)
			{
				PUtil.LogExcWarn(thrown2);
			}
			List<Tuple<string, Type>> list = WIDGET_TAG_MAPPINGS?.GetValue(null) as List<Tuple<string, Type>>;
			if (list == null)
			{
				PDatabaseUtils.LogDatabaseWarning("Unable to load codex files: no tag mappings found");
			}
			string[] array2 = array;
			foreach (string text in array2)
			{
				try
				{
					CodexEntry val = YamlIO.LoadFile<CodexEntry>(text, new ErrorHandler(YamlParseErrorCB), list);
					if (val != null)
					{
						val.category = category;
						entries?.Add(val);
					}
				}
				catch (IOException thrown3)
				{
					PDatabaseUtils.LogDatabaseWarning("Unable to load codex files from {0}:".F(dir));
					PUtil.LogExcWarn(thrown3);
				}
				catch (InvalidDataException thrown4)
				{
					PUtil.LogException(thrown4);
				}
			}
		}

		private static void LoadFromDirectory(ICollection<SubEntry> entries, string dir)
		{
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			string[] array = Array.Empty<string>();
			try
			{
				array = Directory.GetFiles(dir, "*.yaml", SearchOption.AllDirectories);
			}
			catch (UnauthorizedAccessException thrown)
			{
				PUtil.LogExcWarn(thrown);
			}
			catch (IOException thrown2)
			{
				PUtil.LogExcWarn(thrown2);
			}
			List<Tuple<string, Type>> list = WIDGET_TAG_MAPPINGS?.GetValue(null) as List<Tuple<string, Type>>;
			if (list == null)
			{
				PDatabaseUtils.LogDatabaseWarning("Unable to load codex files: no tag mappings found");
			}
			string[] array2 = array;
			foreach (string text in array2)
			{
				try
				{
					SubEntry item = YamlIO.LoadFile<SubEntry>(text, new ErrorHandler(YamlParseErrorCB), list);
					entries?.Add(item);
				}
				catch (IOException thrown3)
				{
					PDatabaseUtils.LogDatabaseWarning("Unable to load codex files from {0}:".F(dir));
					PUtil.LogExcWarn(thrown3);
				}
				catch (InvalidDataException thrown4)
				{
					PUtil.LogException(thrown4);
				}
			}
		}

		internal static void YamlParseErrorCB(Error error, bool _)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			throw new InvalidDataException($"{error.severity} parse error in {error.file.full_path}\n{error.message}", error.inner_exception);
		}

		public PCodexManager()
		{
			creaturePaths = new HashSet<string>();
			plantPaths = new HashSet<string>();
			storyPaths = new HashSet<string>();
			InstanceData = new Dictionary<string, ISet<string>>(4)
			{
				{ "CREATURES", creaturePaths },
				{ "PLANTS", plantPaths },
				{ "STORYTRAITS", storyPaths }
			};
			PUtil.InitLibrary(logVersion: false);
			PRegistry.Instance.AddCandidateVersion(this);
		}

		public override void Initialize(Harmony plibInstance)
		{
			Instance = this;
			plibInstance.Patch(typeof(CodexCache), "CollectEntries", null, PatchMethod("CollectEntries_Postfix"));
			plibInstance.Patch(typeof(CodexCache), "CollectSubEntries", null, PatchMethod("CollectSubEntries_Postfix"));
		}

		private IEnumerable<CodexEntry> LoadEntries(string category)
		{
			List<CodexEntry> list = new List<CodexEntry>(32);
			IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(base.ID);
			if (allComponents != null)
			{
				foreach (PForwardedComponent item in allComponents)
				{
					Dictionary<string, ISet<string>> dictionary = item?.GetInstanceData<Dictionary<string, ISet<string>>>();
					if (dictionary == null || !dictionary.TryGetValue(category, out var value))
					{
						continue;
					}
					foreach (string item2 in value)
					{
						LoadFromDirectory(list, item2, category);
					}
				}
			}
			return list;
		}

		private IEnumerable<SubEntry> LoadSubEntries()
		{
			List<SubEntry> list = new List<SubEntry>(32);
			IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(base.ID);
			if (allComponents != null)
			{
				foreach (PForwardedComponent item in allComponents)
				{
					Dictionary<string, ISet<string>> dictionary = item?.GetInstanceData<Dictionary<string, ISet<string>>>();
					if (dictionary == null)
					{
						continue;
					}
					foreach (KeyValuePair<string, ISet<string>> item2 in dictionary)
					{
						foreach (string item3 in item2.Value)
						{
							LoadFromDirectory(list, item3);
						}
					}
				}
			}
			return list;
		}

		public void RegisterCreatures(Assembly assembly = null)
		{
			if (assembly == null)
			{
				assembly = Assembly.GetCallingAssembly();
			}
			string item = Path.Combine(PUtil.GetModPath(assembly), "codex/Creatures");
			creaturePaths.Add(item);
		}

		public void RegisterPlants(Assembly assembly = null)
		{
			if (assembly == null)
			{
				assembly = Assembly.GetCallingAssembly();
			}
			string item = Path.Combine(PUtil.GetModPath(assembly), "codex/Plants");
			plantPaths.Add(item);
		}

		public void RegisterStory(Assembly assembly = null)
		{
			if (assembly == null)
			{
				assembly = Assembly.GetCallingAssembly();
			}
			string item = Path.Combine(PUtil.GetModPath(assembly), "codex/StoryTraits");
			storyPaths.Add(item);
		}
	}
	public sealed class PColonyAchievement
	{
		private delegate ColonyAchievement NewColonyAchievement(string Id, string platformAchievementId, string Name, string description, bool isVictoryCondition, List<ColonyAchievementRequirement> requirementChecklist, string messageTitle, string messageBody, string videoDataName, string victoryLoopVideo, Action<KMonoBehaviour> VictorySequence);

		private static readonly NewColonyAchievement NEW_COLONY_ACHIEVEMENT = typeof(ColonyAchievement).DetourConstructor<NewColonyAchievement>();

		public string Description { get; set; }

		public string Icon { get; set; }

		public string ID { get; }

		public bool IsVictory { get; set; }

		public string Name { get; set; }

		public Action<KMonoBehaviour> OnVictory { get; set; }

		public List<ColonyAchievementRequirement> Requirements { get; set; }

		[Obsolete("Set victory audio snapshot directly due to Klei changes in the Sweet Dreams update")]
		public string VictoryAudioSnapshot { get; set; }

		public string VictoryMessage { get; set; }

		public string VictoryTitle { get; set; }

		public string VictoryVideoData { get; set; }

		public string VictoryVideoLoop { get; set; }

		public PColonyAchievement(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}
			Description = "";
			Icon = "";
			ID = id;
			IsVictory = false;
			Name = "";
			OnVictory = null;
			Requirements = null;
			VictoryMessage = "";
			VictoryTitle = "";
			VictoryVideoData = "";
			VictoryVideoLoop = "";
		}

		public void AddAchievement()
		{
			if (Requirements == null)
			{
				throw new ArgumentNullException("Requirements");
			}
			ColonyAchievement obj = NEW_COLONY_ACHIEVEMENT(ID, "", Name, Description, IsVictory, Requirements, VictoryTitle, VictoryMessage, VictoryVideoData, VictoryVideoLoop, OnVictory);
			obj.icon = Icon;
			PDatabaseUtils.AddColonyAchievement(obj);
		}

		public override bool Equals(object obj)
		{
			if (obj is PColonyAchievement pColonyAchievement)
			{
				return ID == pColonyAchievement.ID;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ID.GetHashCode();
		}

		public override string ToString()
		{
			return "PColonyAchievement[ID={0},Name={1}]".F(ID, Name);
		}
	}
	public static class PDatabaseUtils
	{
		private delegate AttributeModifier NewModifierFunc(string attributeID, float value, Func<string> getDescription, bool multiplier, bool uiOnly);

		private delegate AttributeModifier NewModifierString(string attributeID, float value, string description, bool multiplier, bool uiOnly, bool readOnly);

		private static readonly NewModifierFunc NEW_MODIFIER_FUNC = typeof(AttributeModifier).DetourConstructor<NewModifierFunc>();

		private static readonly NewModifierString NEW_MODIFIER_STRING = typeof(AttributeModifier).DetourConstructor<NewModifierString>();

		public static void AddColonyAchievement(ColonyAchievement achievement)
		{
			if (achievement == null)
			{
				throw new ArgumentNullException("achievement");
			}
			((ResourceSet<ColonyAchievement>)(object)Db.Get()?.ColonyAchievements)?.resources?.Add(achievement);
		}

		public static void AddStatusItemStrings(string id, string category, string name, string desc)
		{
			string text = id.ToUpperInvariant();
			string text2 = category.ToUpperInvariant();
			Strings.Add(new string[2]
			{
				"STRINGS." + text2 + ".STATUSITEMS." + text + ".NAME",
				name
			});
			Strings.Add(new string[2]
			{
				"STRINGS." + text2 + ".STATUSITEMS." + text + ".TOOLTIP",
				desc
			});
		}

		public static AttributeModifier CreateAttributeModifier(string attributeID, float value, string description = null, bool multiplier = false, bool uiOnly = false, bool readOnly = true)
		{
			return NEW_MODIFIER_STRING(attributeID, value, description, multiplier, uiOnly, readOnly);
		}

		public static AttributeModifier CreateAttributeModifier(string attributeID, float value, Func<string> getDescription = null, bool multiplier = false, bool uiOnly = false)
		{
			return NEW_MODIFIER_FUNC(attributeID, value, getDescription, multiplier, uiOnly);
		}

		internal static void LogDatabaseDebug(string message)
		{
			Debug.LogFormat("[PLibDatabase] {0}", new object[1] { message });
		}

		internal static void LogDatabaseWarning(string message)
		{
			Debug.LogWarningFormat("[PLibDatabase] {0}", new object[1] { message });
		}
	}
	public sealed class PLocalization : PForwardedComponent
	{
		public const string TRANSLATIONS_DIR = "translations";

		internal static readonly Version VERSION = new Version("4.24.0.0");

		private readonly ICollection<Assembly> toLocalize;

		public override Version Version => VERSION;

		private static void Localize(Assembly modAssembly, Locale locale)
		{
			string modPath = PUtil.GetModPath(modAssembly);
			string text = locale.Code;
			if (string.IsNullOrEmpty(text))
			{
				text = Localization.GetCurrentLanguageCode();
			}
			string text2 = Path.Combine(Path.Combine(modPath, "translations"), text + ".po");
			try
			{
				Localization.OverloadStrings(Localization.LoadStringsFile(text2, false));
				RewriteStrings(modAssembly);
			}
			catch (FileNotFoundException)
			{
			}
			catch (DirectoryNotFoundException)
			{
			}
			catch (IOException thrown)
			{
				PDatabaseUtils.LogDatabaseWarning("Failed to load {0} localization for mod {1}:".F(text, modAssembly.GetNameSafe() ?? "?"));
				PUtil.LogExcWarn(thrown);
			}
		}

		internal static void RewriteStrings(Assembly assembly)
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			Type[] types = assembly.GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				FieldInfo[] fields = types[i].GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				foreach (FieldInfo fieldInfo in fields)
				{
					if (fieldInfo.FieldType == typeof(LocString))
					{
						object? value = fieldInfo.GetValue(null);
						LocString val = (LocString)((value is LocString) ? value : null);
						if (val != null)
						{
							Strings.Add(new string[2]
							{
								val.key.String,
								val.text
							});
						}
					}
				}
			}
		}

		public PLocalization()
		{
			toLocalize = new List<Assembly>(4);
			InstanceData = toLocalize;
		}

		internal void DumpAll()
		{
			IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(base.ID);
			if (allComponents == null)
			{
				return;
			}
			foreach (PForwardedComponent item in allComponents)
			{
				ICollection<Assembly> instanceData = item.GetInstanceData<ICollection<Assembly>>();
				if (instanceData == null)
				{
					continue;
				}
				foreach (Assembly item2 in instanceData)
				{
					ModUtil.RegisterForTranslation(item2.GetTypes()[0]);
				}
			}
		}

		public override void Initialize(Harmony plibInstance)
		{
		}

		public override void Process(uint operation, object _)
		{
			Locale locale = Localization.GetLocale();
			if (locale == null || operation != 0)
			{
				return;
			}
			foreach (Assembly item in toLocalize)
			{
				Localize(item, locale);
			}
		}

		public void Register(Assembly assembly = null)
		{
			if (assembly == null)
			{
				assembly = Assembly.GetCallingAssembly();
			}
			Type[] types = assembly.GetTypes();
			if (types == null || types.Length == 0)
			{
				PDatabaseUtils.LogDatabaseWarning("Registered assembly " + assembly.GetNameSafe() + " that had no types for localization!");
				return;
			}
			RegisterForForwarding();
			toLocalize.Add(assembly);
			Localization.RegisterForTranslation(types[0]);
		}
	}
}
namespace PeterHan.PLib.Lighting
{
	public interface ILightShape
	{
		string Identifier { get; }

		LightShape KleiLightShape { get; }

		LightShape RayMode { get; }

		void FillLight(LightingArgs args);
	}
	public interface IRayMode
	{
		void DrawCustomRay(Light2D light, LightBuffer lightBuffer);

		void Prepare(LightBuffer lightBuffer);
	}
	public sealed class LightingArgs : EventArgs, IDictionary<int, float>, ICollection<KeyValuePair<int, float>>, IEnumerable<KeyValuePair<int, float>>, IEnumerable
	{
		public IDictionary<int, float> Brightness { get; }

		public int Range { get; }

		public GameObject Source { get; }

		public int SourceCell { get; }

		public ICollection<int> Keys => Brightness.Keys;

		public ICollection<float> Values => Brightness.Values;

		public int Count => Brightness.Count;

		public bool IsReadOnly => Brightness.IsReadOnly;

		public float this[int key]
		{
			get
			{
				return Brightness[key];
			}
			set
			{
				Brightness[key] = value;
			}
		}

		internal LightingArgs(GameObject source, int cell, int range, IDictionary<int, float> output)
		{
			if ((Object)(object)source == (Object)null)
			{
				throw new ArgumentNullException("source");
			}
			Brightness = output ?? throw new ArgumentNullException("output");
			Range = range;
			Source = source;
			SourceCell = cell;
		}

		public bool ContainsKey(int key)
		{
			return Brightness.ContainsKey(key);
		}

		public void Add(int key, float value)
		{
			Brightness.Add(key, value);
		}

		public bool Remove(int key)
		{
			return Brightness.Remove(key);
		}

		public bool TryGetValue(int key, out float value)
		{
			return Brightness.TryGetValue(key, out value);
		}

		public void Add(KeyValuePair<int, float> item)
		{
			Brightness.Add(item);
		}

		public void Clear()
		{
			Brightness.Clear();
		}

		public bool Contains(KeyValuePair<int, float> item)
		{
			return Brightness.Contains(item);
		}

		public void CopyTo(KeyValuePair<int, float>[] array, int arrayIndex)
		{
			Brightness.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<int, float> item)
		{
			return Brightness.Remove(item);
		}

		public IEnumerator<KeyValuePair<int, float>> GetEnumerator()
		{
			return Brightness.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Brightness.GetEnumerator();
		}

		public override string ToString()
		{
			return $"LightingArgs[source={SourceCell:D},range={Range:D}]";
		}
	}
	internal static class LightingPatches
	{
		private static readonly IDetouredField<Light2D, int> ORIGIN = PDetours.DetourFieldLazy<Light2D, int>("origin");

		private static readonly IDetouredField<LightShapePreview, int> PREVIOUS_CELL = PDetours.DetourFieldLazy<LightShapePreview, int>("previousCell");

		private static bool ComputeExtents_Prefix(Light2D __instance, ref Extents __result)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Invalid comparison between Unknown and I4
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			PLightManager instance = PLightManager.Instance;
			bool result = true;
			if (instance != null && (Object)(object)__instance != (Object)null)
			{
				LightShape shape = __instance.shape;
				int num = Mathf.CeilToInt(__instance.Range);
				instance.AddLight(__instance.emitter, ((Component)__instance).gameObject);
				int num2;
				if ((int)shape > 2 && num > 0 && Grid.IsValidCell(num2 = ORIGIN.Get(__instance)))
				{
					int num3 = default(int);
					int num4 = default(int);
					Grid.CellToXY(num2, ref num3, ref num4);
					__result = new Extents(num3 - num, num4 - num, 2 * num, 2 * num);
					result = false;
				}
			}
			return result;
		}

		public static void ApplyPatches(Harmony plibInstance)
		{
			plibInstance.Patch(typeof(Light2D), "ComputeExtents", PatchMethod("ComputeExtents_Prefix"));
			plibInstance.Patch(typeof(Light2D), "FullRemove", null, PatchMethod("Light2D_FullRemove_Postfix"));
			plibInstance.Patch(typeof(Light2D), "RefreshShapeAndPosition", null, PatchMethod("Light2D_RefreshShapeAndPosition_Postfix"));
			try
			{
				plibInstance.PatchTranspile(typeof(LightBuffer), "LateUpdate", PatchMethod("LightBuffer_LateUpdate_Transpile"));
			}
			catch (Exception thrown)
			{
				PUtil.LogExcWarn(thrown);
			}
			plibInstance.Patch(typeof(LightGridEmitter), "ComputeLux", PatchMethod("ComputeLux_Prefix"));
			plibInstance.Patch(typeof(LightGridEmitter), "UpdateLitCells", PatchMethod("UpdateLitCells_Prefix"));
			plibInstance.Patch((MethodBase)typeof(LightGridManager).GetOverloadWithMostArguments("CreatePreview", true, typeof(int), typeof(float), typeof(LightShape), typeof(int)), PatchMethod("CreatePreview_Prefix"), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			plibInstance.Patch(typeof(LightShapePreview), "Update", PatchMethod("LightShapePreview_Update_Prefix"));
			plibInstance.Patch(typeof(Rotatable), "OrientVisualizer", null, PatchMethod("OrientVisualizer_Postfix"));
		}

		private static bool ComputeLux_Prefix(LightGridEmitter __instance, int cell, State ___state, ref int __result)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			PLightManager instance = PLightManager.Instance;
			if (instance != null)
			{
				return !instance.GetBrightness(__instance, cell, ___state, out __result);
			}
			return true;
		}

		private static bool CreatePreview_Prefix(int origin_cell, float radius, LightShape shape, int lux)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			PLightManager instance = PLightManager.Instance;
			if (instance != null)
			{
				return !instance.PreviewLight(origin_cell, radius, shape, lux);
			}
			return true;
		}

		private static void Light2D_FullRemove_Postfix(Light2D __instance)
		{
			PLightManager instance = PLightManager.Instance;
			if (instance != null && (Object)(object)__instance != (Object)null)
			{
				instance.DestroyLight(__instance.emitter);
			}
		}

		private static void Light2D_RefreshShapeAndPosition_Postfix(Light2D __instance, RefreshResult __result)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			PLightManager instance = PLightManager.Instance;
			if (instance != null && (Object)(object)__instance != (Object)null && (int)__result == 2)
			{
				instance.AddLight(__instance.emitter, ((Component)__instance).gameObject);
			}
		}

		private static IEnumerable<CodeInstruction> LightBuffer_LateUpdate_Transpile(IEnumerable<CodeInstruction> body)
		{
			MethodInfo target = typeof(Light2D).GetPropertySafe<LightShape>("shape", isStatic: false)?.GetGetMethod(nonPublic: true);
			MethodInfo replacement = typeof(PLightManager).GetMethodSafe("LightShapeToRayShape", true, typeof(Light2D));
			MethodInfo cleanup = typeof(PLightManager).GetMethodSafe("Cleanup", true);
			if (target == null || replacement == null)
			{
				PLightManager.LogLightingWarning("Shape property on Light2D not found");
				foreach (CodeInstruction item in body)
				{
					yield return item;
				}
				yield break;
			}
			yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
			yield return new CodeInstruction(OpCodes.Call, (object)typeof(PLightManager).GetMethodSafe("InitRayShapes", true, typeof(LightBuffer)));
			foreach (CodeInstruction instr in body)
			{
				if (instr.opcode == OpCodes.Callvirt && instr.operand is MethodBase methodBase && methodBase == target)
				{
					yield return new CodeInstruction(OpCodes.Call, (object)replacement);
					continue;
				}
				if (instr.opcode == OpCodes.Ret && cleanup != null)
				{
					yield return new CodeInstruction(OpCodes.Call, (object)cleanup);
				}
				yield return instr;
			}
		}

		private static void LightShapePreview_Update_Prefix(LightShapePreview __instance)
		{
			PLightManager instance = PLightManager.Instance;
			if (instance != null && (Object)(object)__instance != (Object)null)
			{
				instance.PreviewObject = ((Component)__instance).gameObject;
			}
		}

		private static void OrientVisualizer_Postfix(Rotatable __instance)
		{
			LightShapePreview arg = default(LightShapePreview);
			if ((Object)(object)__instance != (Object)null && ((Component)__instance).TryGetComponent<LightShapePreview>(ref arg))
			{
				PREVIOUS_CELL.Set(arg, -1);
			}
		}

		private static HarmonyMethod PatchMethod(string name)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected O, but got Unknown
			return new HarmonyMethod(typeof(LightingPatches), name, (Type[])null);
		}

		private static bool UpdateLitCells_Prefix(LightGridEmitter __instance, List<int> ___litCells, State ___state)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			PLightManager instance = PLightManager.Instance;
			if (instance != null)
			{
				return !instance.UpdateLitCells(__instance, ___state, ___litCells);
			}
			return true;
		}
	}
	public sealed class OctantBuilder
	{
		private delegate void ScanOctantFunc(Vector2I cellPos, int range, int depth, Octant octant, double startSlope, double endSlope, List<int> visiblePoints);

		private static readonly ScanOctantFunc OCTANT_SCAN;

		private readonly IDictionary<int, float> destination;

		public float Falloff { get; set; }

		public bool SmoothLight { get; set; }

		public int SourceCell { get; }

		static OctantBuilder()
		{
			OCTANT_SCAN = typeof(DiscreteShadowCaster).Detour<ScanOctantFunc>("ScanOctant");
			if (OCTANT_SCAN == null)
			{
				PLightManager.LogLightingWarning("OctantBuilder cannot find default octant scanner!");
			}
		}

		public OctantBuilder(IDictionary<int, float> destination, int sourceCell)
		{
			if (!Grid.IsValidCell(sourceCell))
			{
				throw new ArgumentOutOfRangeException("sourceCell");
			}
			this.destination = destination ?? throw new ArgumentNullException("destination");
			destination[sourceCell] = 1f;
			Falloff = 0.5f;
			SmoothLight = false;
			SourceCell = sourceCell;
		}

		public OctantBuilder AddOctant(int range, Octant octant)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			PooledList<int, OctantBuilder> val = ListPool<int, OctantBuilder>.Allocate();
			OCTANT_SCAN?.Invoke(Grid.CellToXY(SourceCell), range, 1, octant, 1.0, 0.0, (List<int>)(object)val);
			foreach (int item in (List<int>)(object)val)
			{
				float value = ((!SmoothLight) ? PLightManager.GetDefaultFalloff(Falloff, item, SourceCell) : PLightManager.GetSmoothFalloff(Falloff, item, SourceCell));
				destination[item] = value;
			}
			val.Recycle();
			return this;
		}

		public override string ToString()
		{
			return $"OctantBuilder[Cell {SourceCell:D}, {destination.Count:D} lit]";
		}
	}
	public sealed class PLightManager : PForwardedComponent
	{
		public delegate void CastLightDelegate(LightingArgs args);

		private sealed class CacheEntry
		{
			internal int BaseLux { get; set; }

			internal IDictionary<int, float> Intensity { get; }

			internal GameObject Owner { get; }

			internal CacheEntry(GameObject owner)
			{
				Intensity = new Dictionary<int, float>(64);
				Owner = owner;
			}

			public override string ToString()
			{
				return "Lighting Cache Entry for " + (((Object)(object)Owner == (Object)null) ? "" : ((Object)Owner).name);
			}
		}

		private static readonly List<object> EMPTY_SHAPES = new List<object>(1);

		internal static readonly Version VERSION = new Version("4.24.0.0");

		private const uint OPERATION_PREPARE = 0u;

		private const uint OPERATION_RENDER = 1u;

		private const uint OPERATION_CLEANUP = 2u;

		private readonly ConcurrentDictionary<LightGridEmitter, CacheEntry> brightCache;

		private LightBuffer activeBuffer;

		private readonly IList<IRayMode> rayRenderers;

		private readonly IList<ILightShape> shapes;

		internal static bool ForceSmoothLight { get; set; }

		internal static PLightManager Instance { get; private set; }

		public override Version Version => VERSION;

		internal GameObject PreviewObject { get; set; }

		internal static void Cleanup()
		{
			Instance?.InvokeAllProcess(2u, null);
		}

		public static float GetDefaultFalloff(float falloffRate, int cell, int origin)
		{
			return 1f / Math.Max(1f, Mathf.RoundToInt(falloffRate * (float)Math.Max(Grid.GetCellDistance(origin, cell), 1)));
		}

		public static float GetSmoothFalloff(float falloffRate, int cell, int origin)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			Vector2I val = Grid.CellToXY(cell);
			Vector2I val2 = Grid.CellToXY(origin);
			return 1f / Math.Max(1f, falloffRate * PUtil.Distance(((Vector2I)(ref val2)).X, ((Vector2I)(ref val2)).Y, ((Vector2I)(ref val)).X, ((Vector2I)(ref val)).Y));
		}

		internal static LightShape LightShapeToRayShape(Light2D light)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I4
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			LightShape val = light.shape;
			PLightManager instance = Instance;
			if (instance != null)
			{
				if ((int)val > 2)
				{
					val = instance.GetRayShape(val);
				}
				instance.InvokeAllProcess(1u, light);
			}
			return val;
		}

		internal static void InitRayShapes(LightBuffer instance)
		{
			Instance?.InvokeAllProcess(0u, instance);
		}

		internal static void LogLightingDebug(string message)
		{
			Debug.LogFormat("[PLibLighting] {0}", new object[1] { message });
		}

		internal static void LogLightingWarning(string message)
		{
			Debug.LogWarningFormat("[PLibLighting] {0}", new object[1] { message });
		}

		public PLightManager()
		{
			activeBuffer = null;
			brightCache = new ConcurrentDictionary<LightGridEmitter, CacheEntry>(2, 128);
			PreviewObject = null;
			rayRenderers = new List<IRayMode>(8);
			shapes = new List<ILightShape>(16);
		}

		internal void AddLight(LightGridEmitter source, GameObject owner)
		{
			if ((Object)(object)owner == (Object)null)
			{
				throw new ArgumentNullException("owner");
			}
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			brightCache.TryAdd(source, new CacheEntry(owner));
		}

		public override void Bootstrap(Harmony plibInstance)
		{
			SetSharedData(new List<object>(16));
		}

		internal void DestroyLight(LightGridEmitter source)
		{
			if (source != null)
			{
				brightCache.TryRemove(source, out var _);
			}
		}

		internal bool GetBrightness(LightGridEmitter source, int location, State state, out int result)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Invalid comparison between Unknown and I4
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Invalid comparison between Unknown and I4
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			LightShape shape = state.shape;
			bool flag;
			if ((int)shape < 0 || (int)shape > 2)
			{
				flag = brightCache.TryGetValue(source, out var value);
				if (flag)
				{
					flag = value.Intensity.TryGetValue(location, out var value2);
					if (flag)
					{
						result = Mathf.RoundToInt((float)value.BaseLux * value2);
					}
					else
					{
						result = 0;
					}
				}
				else
				{
					result = 0;
				}
			}
			else if (ForceSmoothLight)
			{
				result = Mathf.RoundToInt((float)state.intensity * GetSmoothFalloff(state.falloffRate, location, state.origin));
				flag = true;
			}
			else
			{
				result = 0;
				flag = false;
			}
			return flag;
		}

		internal LightShape GetRayShape(LightShape shape)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected I4, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Invalid comparison between Unknown and I4
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			int num = shape - 2 - 1;
			ILightShape lightShape;
			if (num >= 0 && num < shapes.Count && (lightShape = shapes[num]) != null)
			{
				LightShape rayMode = lightShape.RayMode;
				if ((int)rayMode >= 0)
				{
					shape = rayMode;
				}
			}
			return shape;
		}

		public override void Initialize(Harmony plibInstance)
		{
			Instance = this;
			shapes.Clear();
			foreach (object sharedDatum in GetSharedData(EMPTY_SHAPES))
			{
				ILightShape lightShape = PRemoteLightWrapper.LightToInstance(sharedDatum);
				shapes.Add(lightShape);
				if (lightShape == null)
				{
					LogLightingWarning("Foreign contaminant in PLightManager!");
				}
			}
			LightingPatches.ApplyPatches(plibInstance);
		}

		internal bool PreviewLight(int origin, float radius, LightShape shape, int lux)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected I4, but got Unknown
			bool result = false;
			GameObject previewObject = PreviewObject;
			int num = shape - 2 - 1;
			if (num >= 0 && num < shapes.Count && (Object)(object)previewObject != (Object)null)
			{
				PooledDictionary<int, float, PLightManager> val = DictionaryPool<int, float, PLightManager>.Allocate();
				shapes[num]?.FillLight(new LightingArgs(previewObject, origin, (int)radius, (IDictionary<int, float>)val));
				foreach (KeyValuePair<int, float> item in (Dictionary<int, float>)(object)val)
				{
					int key = item.Key;
					if (Grid.IsValidCell(key))
					{
						int num2 = Mathf.RoundToInt((float)lux * item.Value);
						LightGridManager.previewLightCells.Add(new Tuple<int, int>(key, num2));
						LightGridManager.previewLux[key] = num2;
					}
				}
				PreviewObject = null;
				result = true;
				val.Recycle();
			}
			return result;
		}

		public override void Process(uint operation, object args)
		{
			int count = rayRenderers.Count;
			LightBuffer val = activeBuffer;
			switch (operation)
			{
			case 0u:
			{
				LightBuffer val3 = (LightBuffer)((args is LightBuffer) ? args : null);
				if (val3 != null)
				{
					activeBuffer = val3;
					for (int j = 0; j < count; j++)
					{
						rayRenderers[j].Prepare(val3);
					}
				}
				break;
			}
			case 1u:
			{
				Light2D val2 = (Light2D)((args is Light2D) ? args : null);
				if (val2 != null && (Object)(object)val != (Object)null)
				{
					for (int i = 0; i < count; i++)
					{
						rayRenderers[i].DrawCustomRay(val2, val);
					}
				}
				break;
			}
			case 2u:
				activeBuffer = null;
				break;
			}
		}

		public void RegisterRayMode(IRayMode mode)
		{
			if (mode == null)
			{
				throw new ArgumentNullException("mode");
			}
			RegisterForForwarding();
			rayRenderers.Add(mode);
		}

		public ILightShape Register(string identifier, CastLightDelegate handler, LightShape rayMode = (LightShape)(-1))
		{
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(identifier))
			{
				throw new ArgumentNullException("identifier");
			}
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			ILightShape lightShape = null;
			RegisterForForwarding();
			List<object> sharedData = GetSharedData(EMPTY_SHAPES);
			int count = sharedData.Count;
			foreach (object item in sharedData)
			{
				ILightShape lightShape2 = PRemoteLightWrapper.LightToInstance(item);
				if (lightShape2 != null && lightShape2.Identifier == identifier)
				{
					LogLightingDebug("Found existing light shape: " + identifier + " from " + (item.GetType().Assembly.GetNameSafe() ?? "?"));
					lightShape = lightShape2;
					break;
				}
			}
			if (lightShape == null)
			{
				lightShape = new PLightShape(count + 1, identifier, handler, rayMode);
				LogLightingDebug("Registered new light shape: " + identifier);
				sharedData.Add(lightShape);
			}
			return lightShape;
		}

		internal bool UpdateLitCells(LightGridEmitter source, State state, IList<int> litCells)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected I4, but got Unknown
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			bool result = false;
			int num = state.shape - 2 - 1;
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (num >= 0 && num < shapes.Count && litCells != null && brightCache.TryGetValue(source, out var value))
			{
				ILightShape lightShape = shapes[num];
				IDictionary<int, float> intensity = value.Intensity;
				intensity.Clear();
				value.BaseLux = state.intensity;
				lightShape.FillLight(new LightingArgs(value.Owner, state.origin, (int)state.radius, intensity));
				foreach (KeyValuePair<int, float> item in intensity)
				{
					litCells.Add(item.Key);
				}
				result = true;
			}
			return result;
		}
	}
	internal sealed class PLightShape : ILightShape
	{
		private readonly PLightManager.CastLightDelegate handler;

		public string Identifier { get; }

		public LightShape KleiLightShape => (LightShape)(ShapeID + 2);

		public LightShape RayMode { get; }

		internal int ShapeID { get; }

		internal PLightShape(int id, string identifier, PLightManager.CastLightDelegate handler, LightShape rayMode)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			this.handler = handler ?? throw new ArgumentNullException("handler");
			Identifier = identifier ?? throw new ArgumentNullException("identifier");
			RayMode = rayMode;
			ShapeID = id;
		}

		public override bool Equals(object obj)
		{
			if (obj is PLightShape pLightShape)
			{
				return pLightShape.Identifier == Identifier;
			}
			return false;
		}

		internal void DoFillLight(GameObject source, int cell, int range, IDictionary<int, float> brightness)
		{
			handler(new LightingArgs(source, cell, range, brightness));
		}

		public void FillLight(LightingArgs args)
		{
			handler(args);
		}

		public override int GetHashCode()
		{
			return Identifier.GetHashCode();
		}

		public override string ToString()
		{
			return "PLightShape[ID=" + Identifier + "]";
		}
	}
	public class PRayMode : IRayMode
	{
		private static readonly IDetouredField<LightBuffer, Camera> LIGHTBUFFER_CAMERA = PDetours.TryDetourField<LightBuffer, Camera>("Camera");

		private static readonly IDetouredField<LightBuffer, Mesh> LIGHTBUFFER_MESH = PDetours.TryDetourField<LightBuffer, Mesh>("Mesh");

		protected readonly LightShape filter;

		protected int layer;

		protected Material material;

		protected readonly Texture2D texture;

		public PRayMode(Texture2D texture, LightShape filter = (LightShape)(-1))
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)texture == (Object)null)
			{
				throw new ArgumentNullException("texture");
			}
			this.filter = filter;
			layer = LayerMask.NameToLayer("Lights");
			material = null;
			this.texture = texture;
		}

		public void DrawCustomRay(Light2D light, LightBuffer lightBuffer)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			if (FilterDrawing(light))
			{
				Vector2 normalized = ((Vector2)(ref light.Direction)).normalized;
				Vector3 val = ((KMonoBehaviour)light).transform.position + Vector2.op_Implicit(light.Offset);
				float num = Mathf.Atan2(normalized.y, normalized.x) * 57.29578f;
				Matrix4x4 matrix = Matrix4x4.Translate(val) * Matrix4x4.Rotate(Quaternion.AngleAxis(num, Vector3.forward));
				Camera val2 = default(Camera);
				if (LIGHTBUFFER_CAMERA == null)
				{
					((Component)lightBuffer).TryGetComponent<Camera>(ref val2);
				}
				else
				{
					val2 = LIGHTBUFFER_CAMERA.Get(lightBuffer);
				}
				Mesh val3 = LIGHTBUFFER_MESH?.Get(lightBuffer);
				if ((Object)(object)val3 != (Object)null && (Object)(object)val2 != (Object)null)
				{
					GetTransformMatrix(ref matrix);
					Graphics.DrawMesh(val3, matrix, material, layer, val2, 0, light.materialPropertyBlock);
				}
			}
		}

		protected virtual bool FilterDrawing(Light2D light)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Invalid comparison between Unknown and I4
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)light != (Object)null)
			{
				if ((int)filter >= 0)
				{
					return light.shape == filter;
				}
				return true;
			}
			return false;
		}

		protected virtual void GetTransformMatrix(ref Matrix4x4 matrix)
		{
		}

		public virtual void Prepare(LightBuffer lightBuffer)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			if ((Object)(object)material == (Object)null)
			{
				CameraController instance = CameraController.Instance;
				if ((Object)(object)instance != (Object)null)
				{
					material = new Material(instance.LightCircleOverlay)
					{
						mainTexture = (Texture)(object)texture
					};
				}
			}
			Material obj = material;
			if (obj != null)
			{
				obj.SetTexture("_PropertyWorldLight", lightBuffer.WorldLight);
			}
		}
	}
	internal sealed class PRemoteLightWrapper : ILightShape
	{
		private delegate void FillLightDelegate(GameObject source, int cell, int range, IDictionary<int, float> brightness);

		private readonly FillLightDelegate fillLight;

		public string Identifier { get; }

		public LightShape KleiLightShape { get; }

		public LightShape RayMode { get; }

		internal static ILightShape LightToInstance(object other)
		{
			if (other != null && !(other.GetType().Name != "PLightShape"))
			{
				if (!(other is ILightShape result))
				{
					return new PRemoteLightWrapper(other);
				}
				return result;
			}
			return null;
		}

		internal PRemoteLightWrapper(object other)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			if (!PPatchTools.TryGetPropertyValue<LightShape>(other, "KleiLightShape", out var value))
			{
				throw new ArgumentException("Light shape is missing KleiLightShape");
			}
			KleiLightShape = value;
			if (!PPatchTools.TryGetPropertyValue<string>(other, "Identifier", out var value2) || value2 == null)
			{
				throw new ArgumentException("Light shape is missing Identifier");
			}
			Identifier = value2;
			if (!PPatchTools.TryGetPropertyValue<LightShape>(other, "RayMode", out var value3))
			{
				value3 = (LightShape)(-1);
			}
			RayMode = value3;
			Type type = other.GetType();
			fillLight = type.CreateDelegate<FillLightDelegate>("DoFillLight", other, new Type[4]
			{
				typeof(GameObject),
				typeof(int),
				typeof(int),
				typeof(IDictionary<int, float>)
			});
			if (fillLight == null)
			{
				throw new ArgumentException("Light shape is missing FillLight");
			}
		}

		public void FillLight(LightingArgs args)
		{
			fillLight(args.Source, args.SourceCell, args.Range, args.Brightness);
		}
	}
}
namespace PeterHan.PLib
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class DynamicOptionAttribute : Attribute
	{
		public string Category { get; }

		public Type Handler { get; }

		public DynamicOptionAttribute(Type type, string category = null)
		{
			Category = category;
			Handler = type ?? throw new ArgumentNullException("type");
		}

		public override string ToString()
		{
			return "DynamicOption[handler={0},category={1}]".F(Handler.FullName, Category);
		}
	}
}
namespace PeterHan.PLib.Options
{
	public class ButtonOptionsEntry : OptionsEntry
	{
		private Action<object> value;

		public override object Value
		{
			get
			{
				return value;
			}
			set
			{
				if (value is Action<object> action)
				{
					this.value = action;
				}
			}
		}

		public ButtonOptionsEntry(string field, IOptionSpec spec)
			: base(field, spec)
		{
		}

		public override void CreateUIEntry(PGridPanel parent, ref int row)
		{
			parent.AddChild(new PButton(base.Field)
			{
				Text = OptionsEntry.LookInStrings(base.Title),
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				OnClick = OnButtonClicked
			}.SetKleiPinkStyle(), new GridComponentSpec(row, 0)
			{
				Margin = OptionsEntry.CONTROL_MARGIN,
				Alignment = (TextAnchor)4,
				ColumnSpan = 2
			});
		}

		private void OnButtonClicked(GameObject _)
		{
			value?.Invoke(null);
		}

		public override GameObject GetUIComponent()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			return new GameObject("Empty");
		}
	}
	internal sealed class CategoryExpandHandler
	{
		private GameObject contents;

		private readonly bool initialState;

		private GameObject toggle;

		public CategoryExpandHandler(bool initialState = true)
		{
			this.initialState = initialState;
		}

		public void OnExpandContract(GameObject _, bool on)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			Vector3 localScale = (on ? Vector3.one : Vector3.zero);
			if ((Object)(object)contents != (Object)null)
			{
				RectTransform val = Util.rectTransform(contents);
				((Transform)val).localScale = localScale;
				if ((Object)(object)val != (Object)null)
				{
					LayoutRebuilder.MarkLayoutForRebuild(val);
				}
			}
		}

		private void OnHeaderClicked()
		{
			if ((Object)(object)toggle != (Object)null)
			{
				bool toggleState = PToggle.GetToggleState(toggle);
				PToggle.SetToggleState(toggle, !toggleState);
			}
		}

		public void OnRealizeHeader(GameObject header)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			Button obj = header.AddComponent<Button>();
			((UnityEvent)obj.onClick).AddListener(new UnityAction(OnHeaderClicked));
			((Selectable)obj).interactable = true;
		}

		public void OnRealizePanel(GameObject panel)
		{
			contents = panel;
			OnExpandContract(null, initialState);
		}

		public void OnRealizeToggle(GameObject toggle)
		{
			this.toggle = toggle;
		}
	}
	public class CheckboxOptionsEntry : OptionsEntry
	{
		private bool check;

		private GameObject checkbox;

		public override object Value
		{
			get
			{
				return check;
			}
			set
			{
				if (value is bool flag)
				{
					check = flag;
					Update();
				}
			}
		}

		public CheckboxOptionsEntry(string field, IOptionSpec spec)
			: base(field, spec)
		{
			check = false;
			checkbox = null;
		}

		public override GameObject GetUIComponent()
		{
			checkbox = new PCheckBox
			{
				OnChecked = delegate(GameObject source, int state)
				{
					check = state == 0;
					Update();
				},
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip)
			}.SetKleiBlueStyle().Build();
			Update();
			return checkbox;
		}

		private void Update()
		{
			PCheckBox.SetCheckState(checkbox, check ? 1 : 0);
		}
	}
	internal class Color32OptionsEntry : ColorBaseOptionsEntry
	{
		public override object Value
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return Color32.op_Implicit(value);
			}
			set
			{
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				if (value is Color32 val)
				{
					base.value = Color32.op_Implicit(val);
					UpdateAll();
				}
			}
		}

		public Color32OptionsEntry(string field, IOptionSpec spec)
			: base(field, spec)
		{
		}
	}
	internal abstract class ColorBaseOptionsEntry : OptionsEntry
	{
		protected static readonly RectOffset ENTRY_MARGIN = new RectOffset(10, 10, 2, 5);

		protected static readonly RectOffset SLIDER_MARGIN = new RectOffset(10, 0, 2, 2);

		protected const float SWATCH_SIZE = 32f;

		protected ColorGradient hueGradient;

		protected KSlider hueSlider;

		protected TMP_InputField blue;

		protected TMP_InputField green;

		protected TMP_InputField red;

		protected ColorGradient satGradient;

		protected KSlider satSlider;

		protected Image swatch;

		protected ColorGradient valGradient;

		protected KSlider valSlider;

		protected Color value;

		protected ColorBaseOptionsEntry(string field, IOptionSpec spec)
			: base(field, spec)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			value = Color.white;
			blue = null;
			green = null;
			red = null;
			hueGradient = null;
			hueSlider = null;
			satGradient = null;
			satSlider = null;
			valGradient = null;
			valSlider = null;
			swatch = null;
		}

		public override void CreateUIEntry(PGridPanel parent, ref int row)
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			base.CreateUIEntry(parent, ref row);
			parent.AddRow(new GridRowSpec());
			PSliderSingle pSliderSingle = new PSliderSingle("Hue")
			{
				ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_HUE),
				MinValue = 0f,
				MaxValue = 1f,
				CustomTrack = true,
				FlexSize = Vector2.right,
				OnValueChanged = OnHueChanged
			}.AddOnRealize(OnHueRealized);
			PSliderSingle pSliderSingle2 = new PSliderSingle("Saturation")
			{
				ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_SATURATION),
				MinValue = 0f,
				MaxValue = 1f,
				CustomTrack = true,
				FlexSize = Vector2.right,
				OnValueChanged = OnSatChanged
			}.AddOnRealize(OnSatRealized);
			PSliderSingle pSliderSingle3 = new PSliderSingle("Value")
			{
				ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_VALUE),
				MinValue = 0f,
				MaxValue = 1f,
				CustomTrack = true,
				FlexSize = Vector2.right,
				OnValueChanged = OnValChanged
			}.AddOnRealize(OnValRealized);
			PLabel pLabel = new PLabel("Swatch")
			{
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				DynamicSize = false,
				Sprite = PUITuning.Images.BoxBorder,
				SpriteMode = (Type)1,
				SpriteSize = new Vector2(32f, 32f)
			}.AddOnRealize(OnSwatchRealized);
			PRelativePanel child = new PRelativePanel("ColorPicker")
			{
				FlexSize = Vector2.right,
				DynamicSize = false
			}.AddChild(pSliderSingle).AddChild(pSliderSingle2).AddChild(pSliderSingle3)
				.AddChild(pLabel)
				.SetRightEdge(pSliderSingle, 1f)
				.SetRightEdge(pSliderSingle2, 1f)
				.SetRightEdge(pSliderSingle3, 1f)
				.SetLeftEdge(pLabel, 0f)
				.SetMargin(pSliderSingle, SLIDER_MARGIN)
				.SetMargin(pSliderSingle2, SLIDER_MARGIN)
				.SetMargin(pSliderSingle3, SLIDER_MARGIN)
				.AnchorYAxis(pLabel)
				.SetLeftEdge(pSliderSingle, -1f, pLabel)
				.SetLeftEdge(pSliderSingle2, -1f, pLabel)
				.SetLeftEdge(pSliderSingle3, -1f, pLabel)
				.SetTopEdge(pSliderSingle, 1f)
				.SetBottomEdge(pSliderSingle3, 0f)
				.SetTopEdge(pSliderSingle2, -1f, pSliderSingle)
				.SetTopEdge(pSliderSingle3, -1f, pSliderSingle2);
			parent.AddChild(child, new GridComponentSpec(++row, 0)
			{
				ColumnSpan = 2,
				Margin = ENTRY_MARGIN
			});
		}

		public override GameObject GetUIComponent()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Color32 val = Color32.op_Implicit(value);
			GameObject result = new PPanel("RGB")
			{
				DynamicSize = false,
				Alignment = (TextAnchor)5,
				Spacing = 5,
				Direction = PanelDirection.Horizontal
			}.AddChild(new PLabel("Red")
			{
				TextStyle = PUITuning.Fonts.TextLightStyle,
				Text = LocString.op_Implicit(PLibStrings.LABEL_R)
			}).AddChild(new PTextField("RedValue")
			{
				OnTextChanged = OnRGBChanged,
				ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_RED),
				Text = val.r.ToString(),
				MinWidth = 32,
				MaxLength = 3,
				Type = PTextField.FieldType.Integer
			}.AddOnRealize(OnRedRealized)).AddChild(new PLabel("Green")
			{
				TextStyle = PUITuning.Fonts.TextLightStyle,
				Text = LocString.op_Implicit(PLibStrings.LABEL_G)
			})
				.AddChild(new PTextField("GreenValue")
				{
					OnTextChanged = OnRGBChanged,
					ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_GREEN),
					Text = val.g.ToString(),
					MinWidth = 32,
					MaxLength = 3,
					Type = PTextField.FieldType.Integer
				}.AddOnRealize(OnGreenRealized))
				.AddChild(new PLabel("Blue")
				{
					TextStyle = PUITuning.Fonts.TextLightStyle,
					Text = LocString.op_Implicit(PLibStrings.LABEL_B)
				})
				.AddChild(new PTextField("BlueValue")
				{
					OnTextChanged = OnRGBChanged,
					ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_BLUE),
					Text = val.b.ToString(),
					MinWidth = 32,
					MaxLength = 3,
					Type = PTextField.FieldType.Integer
				}.AddOnRealize(OnBlueRealized))
				.Build();
			UpdateAll();
			return result;
		}

		private void OnBlueRealized(GameObject realized)
		{
			blue = realized.GetComponentInChildren<TMP_InputField>();
		}

		private void OnGreenRealized(GameObject realized)
		{
			green = realized.GetComponentInChildren<TMP_InputField>();
		}

		private void OnHueChanged(GameObject _, float newHue)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)hueGradient != (Object)null && (Object)(object)hueSlider != (Object)null)
			{
				float a = value.a;
				hueGradient.Position = ((Slider)hueSlider).value;
				value = hueGradient.SelectedColor;
				value.a = a;
				UpdateRGB();
				UpdateSat(moveSlider: false);
				UpdateVal(moveSlider: false);
			}
		}

		private void OnHueRealized(GameObject realized)
		{
			hueGradient = EntityTemplateExtensions.AddOrGet<ColorGradient>(realized);
			realized.TryGetComponent<KSlider>(ref hueSlider);
		}

		private void OnRedRealized(GameObject realized)
		{
			red = realized.GetComponentInChildren<TMP_InputField>();
		}

		protected void OnRGBChanged(GameObject _, string text)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			Color32 val = Color32.op_Implicit(value);
			if (byte.TryParse(red.text, out var result))
			{
				val.r = result;
			}
			if (byte.TryParse(green.text, out var result2))
			{
				val.g = result2;
			}
			if (byte.TryParse(blue.text, out var result3))
			{
				val.b = result3;
			}
			value = Color32.op_Implicit(val);
			UpdateAll();
		}

		private void OnSatChanged(GameObject _, float newSat)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)satGradient != (Object)null && (Object)(object)satSlider != (Object)null)
			{
				float a = value.a;
				satGradient.Position = ((Slider)satSlider).value;
				value = satGradient.SelectedColor;
				value.a = a;
				UpdateRGB();
				UpdateHue(moveSlider: false);
				UpdateVal(moveSlider: false);
			}
		}

		private void OnSatRealized(GameObject realized)
		{
			satGradient = EntityTemplateExtensions.AddOrGet<ColorGradient>(realized);
			realized.TryGetComponent<KSlider>(ref satSlider);
		}

		private void OnSwatchRealized(GameObject realized)
		{
			swatch = realized.GetComponentInChildren<Image>();
		}

		private void OnValChanged(GameObject _, float newValue)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)valGradient != (Object)null && (Object)(object)valSlider != (Object)null)
			{
				float a = value.a;
				valGradient.Position = ((Slider)valSlider).value;
				value = valGradient.SelectedColor;
				value.a = a;
				UpdateRGB();
				UpdateHue(moveSlider: false);
				UpdateSat(moveSlider: false);
			}
		}

		private void OnValRealized(GameObject realized)
		{
			valGradient = EntityTemplateExtensions.AddOrGet<ColorGradient>(realized);
			realized.TryGetComponent<KSlider>(ref valSlider);
		}

		protected void UpdateAll()
		{
			UpdateRGB();
			UpdateHue(moveSlider: true);
			UpdateSat(moveSlider: true);
			UpdateVal(moveSlider: true);
		}

		protected void UpdateHue(bool moveSlider)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)hueGradient != (Object)null && (Object)(object)hueSlider != (Object)null)
			{
				float num = default(float);
				float num2 = default(float);
				float num3 = default(float);
				Color.RGBToHSV(value, ref num, ref num2, ref num3);
				hueGradient.SetRange(0f, 1f, num2, num2, num3, num3);
				hueGradient.SelectedColor = value;
				if (moveSlider)
				{
					((Slider)hueSlider).value = hueGradient.Position;
				}
			}
		}

		protected void UpdateRGB()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			Color32 val = Color32.op_Implicit(value);
			if ((Object)(object)red != (Object)null)
			{
				red.text = val.r.ToString();
			}
			if ((Object)(object)green != (Object)null)
			{
				green.text = val.g.ToString();
			}
			if ((Object)(object)blue != (Object)null)
			{
				blue.text = val.b.ToString();
			}
			if ((Object)(object)swatch != (Object)null)
			{
				((Graphic)swatch).color = value;
			}
		}

		protected void UpdateSat(bool moveSlider)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)satGradient != (Object)null && (Object)(object)satSlider != (Object)null)
			{
				float num = default(float);
				float num2 = default(float);
				float num3 = default(float);
				Color.RGBToHSV(value, ref num, ref num2, ref num3);
				satGradient.SetRange(num, num, 0f, 1f, num3, num3);
				satGradient.SelectedColor = value;
				if (moveSlider)
				{
					((Slider)satSlider).value = satGradient.Position;
				}
			}
		}

		protected void UpdateVal(bool moveSlider)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)valGradient != (Object)null && (Object)(object)valSlider != (Object)null)
			{
				float num = default(float);
				float num2 = default(float);
				float num3 = default(float);
				Color.RGBToHSV(value, ref num, ref num2, ref num3);
				valGradient.SetRange(num, num, num2, num2, 0f, 1f);
				valGradient.SelectedColor = value;
				if (moveSlider)
				{
					((Slider)valSlider).value = valGradient.Position;
				}
			}
		}
	}
	internal sealed class ColorGradient : Image
	{
		private Color current;

		private bool dirty;

		private Vector2 hue;

		private float position;

		private Texture2D preview;

		private Vector2 sat;

		private Vector2 val;

		public float Position
		{
			get
			{
				return position;
			}
			set
			{
				position = Mathf.Clamp01(value);
				SetPosition();
			}
		}

		public Color SelectedColor
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return current;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				current = value;
				EstimatePosition();
			}
		}

		public ColorGradient()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			current = Color.black;
			position = 0f;
			preview = null;
			dirty = true;
			hue = Vector2.right;
			sat = Vector2.right;
			val = Vector2.right;
		}

		internal void EstimatePosition()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			float num = default(float);
			float num2 = default(float);
			float num3 = default(float);
			Color.RGBToHSV(current, ref num, ref num2, ref num3);
			float num4 = 0f;
			float num5 = 0f;
			if (!Mathf.Approximately(hue.x, hue.y))
			{
				num4 += Mathf.Clamp01(Mathf.InverseLerp(hue.x, hue.y, num));
				num5 += 1f;
			}
			if (!Mathf.Approximately(sat.x, sat.y))
			{
				num4 += Mathf.Clamp01(Mathf.InverseLerp(sat.x, sat.y, num2));
				num5 += 1f;
			}
			if (!Mathf.Approximately(val.x, val.y))
			{
				num4 += Mathf.Clamp01(Mathf.InverseLerp(val.x, val.y, num3));
				num5 += 1f;
			}
			if (num5 > 0f)
			{
				position = num4 / num5;
				SetPosition();
			}
		}

		protected override void OnDestroy()
		{
			if ((Object)(object)preview != (Object)null)
			{
				Object.Destroy((Object)(object)preview);
				preview = null;
			}
			if ((Object)(object)((Image)this).sprite != (Object)null)
			{
				Object.Destroy((Object)(object)((Image)this).sprite);
				((Image)this).sprite = null;
			}
			((Graphic)this).OnDestroy();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			((Graphic)this).OnRectTransformDimensionsChange();
			dirty = true;
		}

		internal void SetPosition()
		{
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			float num = Mathf.Clamp01(Mathf.Lerp(hue.x, hue.y, position));
			float num2 = Mathf.Clamp01(Mathf.Lerp(sat.x, sat.y, position));
			float num3 = Mathf.Clamp01(Mathf.Lerp(val.x, val.y, position));
			current = Color.HSVToRGB(num, num2, num3);
		}

		public void SetRange(float hMin, float hMax, float sMin, float sMax, float vMin, float vMax)
		{
			if (hMin > hMax)
			{
				hue.x = hMax;
				hue.y = hMin;
			}
			else
			{
				hue.x = hMin;
				hue.y = hMax;
			}
			if (sMin > sMax)
			{
				sat.x = sMax;
				sat.y = sMin;
			}
			else
			{
				sat.x = sMin;
				sat.y = sMax;
			}
			if (vMin > vMax)
			{
				val.x = vMax;
				val.y = vMin;
			}
			else
			{
				val.x = vMin;
				val.y = vMax;
			}
			EstimatePosition();
			dirty = true;
		}

		protected override void Start()
		{
			((UIBehaviour)this).Start();
			dirty = true;
		}

		internal void Update()
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Expected O, but got Unknown
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			if (!dirty && !((Object)(object)preview == (Object)null) && !((Object)(object)((Image)this).sprite == (Object)null))
			{
				return;
			}
			Rect rect = ((Graphic)this).rectTransform.rect;
			int num = Mathf.RoundToInt(((Rect)(ref rect)).width);
			int num2 = Mathf.RoundToInt(((Rect)(ref rect)).height);
			if (num > 0 && num2 > 0)
			{
				Color[] array = (Color[])(object)new Color[num * num2];
				float x = hue.x;
				float y = hue.y;
				float x2 = sat.x;
				float y2 = sat.y;
				float x3 = val.x;
				float y3 = val.y;
				Sprite sprite = ((Image)this).sprite;
				bool flag = (Object)(object)preview == (Object)null || (Object)(object)sprite == (Object)null || ((Texture)preview).width != num || ((Texture)preview).height != num2;
				if (flag)
				{
					if ((Object)(object)preview != (Object)null)
					{
						Object.Destroy((Object)(object)preview);
					}
					if ((Object)(object)sprite != (Object)null)
					{
						Object.Destroy((Object)(object)sprite);
					}
					preview = new Texture2D(num, num2);
				}
				for (int i = 0; i < num; i++)
				{
					float num3 = (float)i / (float)num;
					array[i] = Color.HSVToRGB(Mathf.Lerp(x, y, num3), Mathf.Lerp(x2, y2, num3), Mathf.Lerp(x3, y3, num3));
				}
				for (int j = 1; j < num2; j++)
				{
					Array.Copy(array, 0, array, j * num, num);
				}
				preview.SetPixels(array);
				preview.Apply();
				if (flag)
				{
					((Image)this).sprite = Sprite.Create(preview, new Rect(0f, 0f, (float)num, (float)num2), new Vector2(0.5f, 0.5f));
				}
			}
			dirty = false;
		}
	}
	internal class ColorOptionsEntry : ColorBaseOptionsEntry
	{
		public override object Value
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return value;
			}
			set
			{
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				if (value is Color val)
				{
					base.value = val;
					UpdateAll();
				}
			}
		}

		public ColorOptionsEntry(string field, IOptionSpec spec)
			: base(field, spec)
		{
		}
	}
	internal class CompositeOptionsEntry : OptionsEntry
	{
		protected readonly IDictionary<PropertyInfo, IOptionsEntry> subOptions;

		protected readonly Type targetType;

		protected object value;

		public int ChildCount => subOptions.Count;

		public override object Value
		{
			get
			{
				return value;
			}
			set
			{
				if (value != null && targetType.IsAssignableFrom(value.GetType()))
				{
					this.value = value;
				}
			}
		}

		internal static CompositeOptionsEntry Create(IOptionSpec spec, PropertyInfo info, int depth)
		{
			Type propertyType = info.PropertyType;
			CompositeOptionsEntry compositeOptionsEntry = new CompositeOptionsEntry(info.Name, spec, propertyType);
			PropertyInfo[] properties = propertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo propertyInfo in properties)
			{
				IOptionsEntry optionsEntry = OptionsEntry.TryCreateEntry(propertyInfo, depth + 1);
				if (optionsEntry != null)
				{
					compositeOptionsEntry.AddField(propertyInfo, optionsEntry);
				}
			}
			if (compositeOptionsEntry.ChildCount <= 0)
			{
				return null;
			}
			return compositeOptionsEntry;
		}

		public CompositeOptionsEntry(string field, IOptionSpec spec, Type fieldType)
			: base(field, spec)
		{
			subOptions = new Dictionary<PropertyInfo, IOptionsEntry>(16);
			targetType = fieldType ?? throw new ArgumentNullException("fieldType");
			value = OptionsDialog.CreateOptions(fieldType);
		}

		public void AddField(PropertyInfo info, IOptionsEntry entry)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			subOptions.Add(info, entry);
		}

		public override void CreateUIEntry(PGridPanel parent, ref int row)
		{
			int row2 = row;
			bool flag = true;
			parent.AddOnRealize(WhenRealized);
			foreach (KeyValuePair<PropertyInfo, IOptionsEntry> subOption in subOptions)
			{
				if (!flag)
				{
					row2++;
					parent.AddRow(new GridRowSpec());
				}
				subOption.Value.CreateUIEntry(parent, ref row2);
				flag = false;
			}
			row = row2;
		}

		public override GameObject GetUIComponent()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			return new GameObject("Empty");
		}

		public override void ReadFrom(object settings)
		{
			base.ReadFrom(settings);
			foreach (KeyValuePair<PropertyInfo, IOptionsEntry> subOption in subOptions)
			{
				subOption.Value.ReadFrom(value);
			}
		}

		public override string ToString()
		{
			return "{1}[field={0},title={2},children=[{3}]]".F(base.Field, GetType().Name, base.Title, subOptions.Join());
		}

		private void WhenRealized(GameObject _)
		{
			foreach (KeyValuePair<PropertyInfo, IOptionsEntry> subOption in subOptions)
			{
				subOption.Value.ReadFrom(value);
			}
		}

		public override bool WriteTo(object settings)
		{
			bool flag = false;
			foreach (KeyValuePair<PropertyInfo, IOptionsEntry> subOption in subOptions)
			{
				flag |= subOption.Value.WriteTo(value);
			}
			return flag | base.WriteTo(settings);
		}
	}
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class ConfigFileAttribute : Attribute
	{
		public string ConfigFileName { get; }

		public bool IndentOutput { get; }

		public bool UseSharedConfigLocation { get; }

		public ConfigFileAttribute(string FileName = "config.json", bool IndentOutput = false, bool SharedConfigLocation = false)
		{
			ConfigFileName = FileName;
			this.IndentOutput = IndentOutput;
			UseSharedConfigLocation = SharedConfigLocation;
		}

		public override string ToString()
		{
			return ConfigFileName;
		}
	}
	public class FloatOptionsEntry : SlidingBaseOptionsEntry
	{
		public const string DEFAULT_FORMAT = "F2";

		private GameObject textField;

		private float value;

		public override object Value
		{
			get
			{
				return value;
			}
			set
			{
				if (value is float num)
				{
					this.value = num;
					Update();
				}
			}
		}

		public FloatOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
			: base(field, spec, limit)
		{
			textField = null;
			value = 0f;
		}

		protected override PSliderSingle GetSlider()
		{
			return new PSliderSingle
			{
				OnValueChanged = OnSliderChanged,
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				MinValue = (float)limits.Minimum,
				MaxValue = (float)limits.Maximum,
				InitialValue = value,
				Increment = (float)limits.Step
			};
		}

		public override GameObject GetUIComponent()
		{
			textField = new PTextField
			{
				OnTextChanged = OnTextChanged,
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				Text = value.ToString(base.Format ?? "F2"),
				MinWidth = 64,
				MaxLength = 16,
				Type = PTextField.FieldType.Float
			}.Build();
			Update();
			return textField;
		}

		private void OnSliderChanged(GameObject _, float newValue)
		{
			if (limits != null)
			{
				value = limits.ClampToRange(newValue);
			}
			else
			{
				value = newValue;
			}
			Update();
		}

		private void OnTextChanged(GameObject _, string text)
		{
			if (float.TryParse(text, out var result))
			{
				if (base.Format != null && base.Format.ToUpperInvariant().IndexOf('P') >= 0)
				{
					result *= 0.01f;
				}
				if (limits != null)
				{
					result = limits.ClampToRange(result);
				}
				value = result;
			}
			Update();
		}

		protected override void Update()
		{
			GameObject obj = textField;
			TMP_InputField val = ((obj != null) ? obj.GetComponentInChildren<TMP_InputField>() : null);
			if ((Object)(object)val != (Object)null)
			{
				val.text = value.ToString(base.Format ?? "F2");
			}
			if ((Object)(object)slider != (Object)null)
			{
				PSliderSingle.SetCurrentValue(slider, value);
			}
		}
	}
	public class IntOptionsEntry : SlidingBaseOptionsEntry
	{
		private GameObject textField;

		private int value;

		public override object Value
		{
			get
			{
				return value;
			}
			set
			{
				if (value is int num)
				{
					this.value = num;
					Update();
				}
			}
		}

		public IntOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
			: base(field, spec, limit)
		{
			textField = null;
			value = 0;
		}

		protected override PSliderSingle GetSlider()
		{
			return new PSliderSingle
			{
				OnValueChanged = OnSliderChanged,
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				MinValue = (float)limits.Minimum,
				MaxValue = (float)limits.Maximum,
				InitialValue = value,
				IntegersOnly = true
			};
		}

		public override GameObject GetUIComponent()
		{
			textField = new PTextField
			{
				OnTextChanged = OnTextChanged,
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				Text = value.ToString(base.Format ?? "D"),
				MinWidth = 64,
				MaxLength = 10,
				Type = PTextField.FieldType.Integer
			}.Build();
			Update();
			return textField;
		}

		private void OnSliderChanged(GameObject _, float newValue)
		{
			int num = Mathf.RoundToInt(newValue);
			if (limits != null)
			{
				num = limits.ClampToRange(num);
			}
			value = num;
			Update();
		}

		private void OnTextChanged(GameObject _, string text)
		{
			if (int.TryParse(text, out var result))
			{
				if (limits != null)
				{
					result = limits.ClampToRange(result);
				}
				value = result;
			}
			Update();
		}

		protected override void Update()
		{
			GameObject obj = textField;
			TMP_InputField val = ((obj != null) ? obj.GetComponentInChildren<TMP_InputField>() : null);
			if ((Object)(object)val != (Object)null)
			{
				val.text = value.ToString(base.Format ?? "D");
			}
			if ((Object)(object)slider != (Object)null)
			{
				PSliderSingle.SetCurrentValue(slider, value);
			}
		}
	}
	public interface IOptions
	{
		IEnumerable<IOptionsEntry> CreateOptions();

		void OnOptionsChanged();
	}
	public interface IOptionsEntry : IOptionSpec
	{
		bool RestartRequired { get; set; }

		void CreateUIEntry(PGridPanel parent, ref int row);

		void ReadFrom(object settings);

		bool WriteTo(object settings);
	}
	public interface IOptionSpec
	{
		string Category { get; }

		string Format { get; }

		string Title { get; }

		string Tooltip { get; }
	}
	public interface IRequireFilter
	{
		bool Filter();
	}
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class LimitAttribute : Attribute
	{
		public double Maximum { get; }

		public double Minimum { get; }

		public double Step { get; set; }

		public LimitAttribute(double min, double max)
			: this(min, max, 0.0)
		{
		}

		public LimitAttribute(double min, double max, double step)
		{
			Minimum = (min.IsNaNOrInfinity() ? 0.0 : min);
			Maximum = ((max.IsNaNOrInfinity() || max < min) ? min : max);
			Step = (step.IsNaNOrInfinity() ? 0.0 : step);
		}

		public float ClampToRange(float value)
		{
			return value.InRange((float)Minimum, (float)Maximum);
		}

		public int ClampToRange(int value)
		{
			return value.InRange((int)Minimum, (int)Maximum);
		}

		public bool InRange(double value)
		{
			if (value >= Minimum)
			{
				return value <= Maximum;
			}
			return false;
		}

		public override string ToString()
		{
			return "{0:F2} to {1:F2}".F(Minimum, Maximum);
		}
	}
	public class LogFloatOptionsEntry : SlidingBaseOptionsEntry
	{
		public const string DEFAULT_FORMAT = "F2";

		private GameObject textField;

		private float value;

		public override object Value
		{
			get
			{
				return value;
			}
			set
			{
				if (value is float num)
				{
					this.value = limits.ClampToRange(num);
					Update();
				}
			}
		}

		public LogFloatOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit)
			: base(field, spec, limit)
		{
			if (limit == null)
			{
				throw new ArgumentNullException("limit");
			}
			if (limit.Minimum <= 0.0 || limit.Maximum <= 0.0)
			{
				throw new ArgumentOutOfRangeException("limit", "Logarithmic values must be positive");
			}
			textField = null;
			value = limit.ClampToRange(1f);
		}

		protected override PSliderSingle GetSlider()
		{
			return new PSliderSingle
			{
				OnValueChanged = OnSliderChanged,
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				MinValue = Mathf.Log((float)limits.Minimum),
				MaxValue = Mathf.Log((float)limits.Maximum),
				InitialValue = Mathf.Log(value),
				Increment = (float)limits.Step
			};
		}

		public override GameObject GetUIComponent()
		{
			textField = new PTextField
			{
				OnTextChanged = OnTextChanged,
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				Text = value.ToString(base.Format ?? "F2"),
				MinWidth = 64,
				MaxLength = 16,
				Type = PTextField.FieldType.Float
			}.Build();
			Update();
			return textField;
		}

		private void OnSliderChanged(GameObject _, float newValue)
		{
			value = limits.ClampToRange(Mathf.Exp(newValue));
			Update();
		}

		private void OnTextChanged(GameObject _, string text)
		{
			if (float.TryParse(text, out var result))
			{
				if (base.Format != null && base.Format.ToUpperInvariant().IndexOf('P') >= 0)
				{
					result *= 0.01f;
				}
				value = limits.ClampToRange(result);
			}
			Update();
		}

		protected override void Update()
		{
			GameObject obj = textField;
			TMP_InputField val = ((obj != null) ? obj.GetComponentInChildren<TMP_InputField>() : null);
			if ((Object)(object)val != (Object)null)
			{
				val.text = value.ToString(base.Format ?? "F2");
			}
			if ((Object)(object)slider != (Object)null)
			{
				PSliderSingle.SetCurrentValue(slider, Mathf.Log(Mathf.Max(float.Epsilon, value)));
			}
		}
	}
	internal sealed class ModDialogInfo
	{
		public string Image { get; }

		public string Title { get; }

		public string URL { get; }

		public string Version { get; }

		private static string GetModVersionText(Type optionsType)
		{
			Assembly assembly = optionsType.Assembly;
			string fileVersion = assembly.GetFileVersion();
			if (string.IsNullOrEmpty(fileVersion))
			{
				return string.Format(LocString.op_Implicit(PLibStrings.MOD_ASSEMBLY_VERSION), assembly.GetName().Version);
			}
			return string.Format(LocString.op_Implicit(PLibStrings.MOD_VERSION), fileVersion);
		}

		internal ModDialogInfo(Type type, string url, string image)
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Invalid comparison between Unknown and I4
			Mod modFromType = POptions.GetModFromType(type);
			Image = image ?? "";
			string text2;
			string text3;
			if (modFromType != null)
			{
				PackagedModInfo packagedModInfo = modFromType.packagedModInfo;
				string text = ((packagedModInfo != null) ? packagedModInfo.version : null);
				text2 = modFromType.title;
				StringEntry val = default(StringEntry);
				if (Strings.TryGet(text2, ref val))
				{
					text2 = val.String;
				}
				if (string.IsNullOrEmpty(url) && (int)modFromType.label.distribution_platform == 1)
				{
					url = "https://steamcommunity.com/sharedfiles/filedetails/?id=" + modFromType.label.id;
				}
				text3 = (string.IsNullOrEmpty(text) ? GetModVersionText(type) : string.Format(LocString.op_Implicit(PLibStrings.MOD_VERSION), text));
			}
			else
			{
				text2 = type.Assembly.GetNameSafe();
				text3 = GetModVersionText(type);
			}
			Title = text2 ?? "";
			URL = url ?? "";
			Version = text3;
		}

		public override string ToString()
		{
			return base.ToString();
		}
	}
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class ModInfoAttribute : Attribute
	{
		public bool ForceCollapseCategories { get; }

		public string Image { get; }

		public string URL { get; }

		public ModInfoAttribute(string url, string image = null, bool collapse = false)
		{
			ForceCollapseCategories = collapse;
			Image = image;
			URL = url;
		}

		public override string ToString()
		{
			return string.Format("ModInfoAttribute[URL={1},Image={2}]", URL, Image);
		}
	}
	public class NullableFloatOptionsEntry : SlidingBaseOptionsEntry
	{
		private GameObject textField;

		private float? value;

		protected virtual string FieldText => value?.ToString(base.Format ?? "F2") ?? string.Empty;

		public override object Value
		{
			get
			{
				return value;
			}
			set
			{
				if (value == null)
				{
					this.value = null;
					Update();
				}
				else if (value is float num)
				{
					this.value = num;
					Update();
				}
			}
		}

		public NullableFloatOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
			: base(field, spec, limit)
		{
			textField = null;
			value = null;
		}

		protected override PSliderSingle GetSlider()
		{
			float num = (float)limits.Minimum;
			float num2 = (float)limits.Maximum;
			return new PSliderSingle
			{
				OnValueChanged = OnSliderChanged,
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				MinValue = num,
				MaxValue = num2,
				InitialValue = 0.5f * (num + num2)
			};
		}

		public override GameObject GetUIComponent()
		{
			textField = new PTextField
			{
				OnTextChanged = OnTextChanged,
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				Text = FieldText,
				MinWidth = 64,
				MaxLength = 16,
				Type = PTextField.FieldType.Float
			}.Build();
			Update();
			return textField;
		}

		private void OnSliderChanged(GameObject _, float newValue)
		{
			if (limits != null)
			{
				value = limits.ClampToRange(newValue);
			}
			else
			{
				value = newValue;
			}
			Update();
		}

		private void OnTextChanged(GameObject _, string text)
		{
			float result;
			if (string.IsNullOrWhiteSpace(text))
			{
				value = null;
			}
			else if (float.TryParse(text, out result))
			{
				if (base.Format != null && base.Format.ToUpperInvariant().IndexOf('P') >= 0)
				{
					result *= 0.01f;
				}
				if (limits != null)
				{
					result = limits.ClampToRange(result);
				}
				value = result;
			}
			Update();
		}

		protected override void Update()
		{
			GameObject obj = textField;
			TMP_InputField val = ((obj != null) ? obj.GetComponentInChildren<TMP_InputField>() : null);
			if ((Object)(object)val != (Object)null)
			{
				val.text = FieldText;
			}
			if ((Object)(object)slider != (Object)null && value.HasValue)
			{
				PSliderSingle.SetCurrentValue(slider, value.Value);
			}
		}
	}
	public class NullableIntOptionsEntry : SlidingBaseOptionsEntry
	{
		private GameObject textField;

		private int? value;

		protected virtual string FieldText => value?.ToString(base.Format ?? "D") ?? string.Empty;

		public override object Value
		{
			get
			{
				return value;
			}
			set
			{
				if (value == null)
				{
					this.value = null;
					Update();
				}
				else if (value is int num)
				{
					this.value = num;
					Update();
				}
			}
		}

		public NullableIntOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
			: base(field, spec, limit)
		{
			textField = null;
			value = null;
		}

		protected override PSliderSingle GetSlider()
		{
			float num = (float)limits.Minimum;
			float maxValue = (float)limits.Maximum;
			return new PSliderSingle
			{
				OnValueChanged = OnSliderChanged,
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				MinValue = num,
				MaxValue = maxValue,
				InitialValue = num,
				IntegersOnly = true
			};
		}

		public override GameObject GetUIComponent()
		{
			textField = new PTextField
			{
				OnTextChanged = OnTextChanged,
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				Text = FieldText,
				MinWidth = 64,
				MaxLength = 10,
				Type = PTextField.FieldType.Integer
			}.Build();
			Update();
			return textField;
		}

		private void OnSliderChanged(GameObject _, float newValue)
		{
			int num = Mathf.RoundToInt(newValue);
			if (limits != null)
			{
				num = limits.ClampToRange(num);
			}
			value = num;
			Update();
		}

		private void OnTextChanged(GameObject _, string text)
		{
			int result;
			if (string.IsNullOrWhiteSpace(text))
			{
				value = null;
			}
			else if (int.TryParse(text, out result))
			{
				if (limits != null)
				{
					result = limits.ClampToRange(result);
				}
				value = result;
			}
			Update();
		}

		protected override void Update()
		{
			GameObject obj = textField;
			TMP_InputField val = ((obj != null) ? obj.GetComponentInChildren<TMP_InputField>() : null);
			if ((Object)(object)val != (Object)null)
			{
				val.text = FieldText;
			}
			if ((Object)(object)slider != (Object)null && value.HasValue)
			{
				PSliderSingle.SetCurrentValue(slider, value.Value);
			}
		}
	}
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class OptionAttribute : Attribute, IOptionSpec
	{
		public string Category { get; }

		public string Format { get; set; }

		public string Title { get; }

		public string Tooltip { get; }

		public OptionAttribute()
		{
			Format = null;
			Title = null;
			Tooltip = null;
			Category = null;
		}

		public OptionAttribute(string title, string tooltip = null, string category = null)
		{
			if (string.IsNullOrEmpty(title))
			{
				throw new ArgumentNullException("title");
			}
			Category = category;
			Format = null;
			Title = title;
			Tooltip = tooltip;
		}

		public override string ToString()
		{
			return Title;
		}
	}
	internal sealed class OptionsDialog
	{
		private static readonly Color CATEGORY_TITLE_COLOR;

		private static readonly TextStyleSetting CATEGORY_TITLE_STYLE;

		private static readonly int CATEGORY_MARGIN;

		private static readonly Vector2 MOD_IMAGE_SIZE;

		private static readonly int OUTER_MARGIN;

		private static readonly Vector2 SETTINGS_DIALOG_SIZE;

		private static readonly Vector2 SETTINGS_DIALOG_MAX_SIZE;

		private static readonly Vector2 TOGGLE_SIZE;

		private readonly bool collapseCategories;

		private readonly ConfigFileAttribute configAttr;

		private KScreen dialog;

		private Sprite modImage;

		private readonly ModDialogInfo displayInfo;

		private readonly IDictionary<string, ICollection<IOptionsEntry>> optionCategories;

		private object options;

		private readonly Type optionsType;

		public Action<object> OnClose { get; set; }

		static OptionsDialog()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			CATEGORY_TITLE_COLOR = Color32.op_Implicit(new Color32((byte)143, (byte)150, (byte)175, byte.MaxValue));
			CATEGORY_MARGIN = 8;
			MOD_IMAGE_SIZE = new Vector2(192f, 192f);
			OUTER_MARGIN = 10;
			SETTINGS_DIALOG_SIZE = new Vector2(320f, 200f);
			SETTINGS_DIALOG_MAX_SIZE = new Vector2(800f, 600f);
			TOGGLE_SIZE = new Vector2(12f, 12f);
			CATEGORY_TITLE_STYLE = PUITuning.Fonts.UILightStyle.DeriveStyle(0, CATEGORY_TITLE_COLOR, (FontStyles)1);
		}

		internal static object CreateOptions(Type type)
		{
			object result = null;
			try
			{
				ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
				if (constructor != null)
				{
					result = constructor.Invoke(null);
				}
			}
			catch (TargetInvocationException ex)
			{
				PUtil.LogExcWarn(ex.GetBaseException());
			}
			catch (AmbiguousMatchException thrown)
			{
				PUtil.LogException(thrown);
			}
			catch (MemberAccessException thrown2)
			{
				PUtil.LogException(thrown2);
			}
			return result;
		}

		private static void SaveAndRestart()
		{
			PGameUtils.SaveMods();
			App.instance.Restart();
		}

		internal OptionsDialog(Type optionsType)
		{
			OnClose = null;
			dialog = null;
			modImage = null;
			this.optionsType = optionsType ?? throw new ArgumentNullException("optionsType");
			optionCategories = OptionsEntry.BuildOptions(optionsType);
			options = null;
			ModInfoAttribute customAttribute = optionsType.GetCustomAttribute<ModInfoAttribute>();
			collapseCategories = customAttribute?.ForceCollapseCategories ?? false;
			configAttr = optionsType.GetCustomAttribute<ConfigFileAttribute>();
			displayInfo = new ModDialogInfo(optionsType, customAttribute?.URL, customAttribute?.Image);
		}

		private void AddCategoryHeader(PGridPanel container, string category, PGridPanel contents)
		{
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Expected O, but got Unknown
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			contents.AddColumn(new GridColumnSpec(0f, 1f)).AddColumn(new GridColumnSpec());
			if (!string.IsNullOrEmpty(category))
			{
				bool initialState = !collapseCategories;
				CategoryExpandHandler categoryExpandHandler = new CategoryExpandHandler(initialState);
				container.AddColumn(new GridColumnSpec()).AddColumn(new GridColumnSpec(0f, 1f)).AddRow(new GridRowSpec())
					.AddRow(new GridRowSpec(0f, 1f));
				container.AddChild(new PLabel("CategoryHeader")
				{
					Text = OptionsEntry.LookInStrings(category),
					TextStyle = CATEGORY_TITLE_STYLE,
					TextAlignment = (TextAnchor)7
				}.AddOnRealize(categoryExpandHandler.OnRealizeHeader), new GridComponentSpec(0, 1)
				{
					Margin = new RectOffset(OUTER_MARGIN, OUTER_MARGIN, 0, 0)
				}).AddChild(new PToggle("CategoryToggle")
				{
					Color = PUITuning.Colors.ComponentDarkStyle,
					InitialState = initialState,
					ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_TOGGLE),
					Size = TOGGLE_SIZE,
					OnStateChanged = categoryExpandHandler.OnExpandContract
				}.AddOnRealize(categoryExpandHandler.OnRealizeToggle), new GridComponentSpec(0, 0));
				contents.OnRealize += categoryExpandHandler.OnRealizePanel;
				container.AddChild(contents, new GridComponentSpec(1, 0)
				{
					ColumnSpan = 2
				});
			}
			else
			{
				container.AddColumn(new GridColumnSpec(0f, 1f)).AddRow(new GridRowSpec(0f, 1f)).AddChild(contents, new GridComponentSpec(0, 0));
			}
		}

		private void AddModInfoScreen(PDialog optionsDialog)
		{
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Expected O, but got Unknown
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Expected O, but got Unknown
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			string image = displayInfo.Image;
			PPanel body = optionsDialog.Body;
			if ((Object)(object)modImage == (Object)null && !string.IsNullOrEmpty(image))
			{
				string modPath = PUtil.GetModPath(optionsType.Assembly);
				modImage = PUIUtils.LoadSpriteFile((modPath == null) ? image : Path.Combine(modPath, image));
			}
			PButton child = new PButton("ModSite")
			{
				Text = LocString.op_Implicit(PLibStrings.MOD_HOMEPAGE),
				ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_HOMEPAGE),
				OnClick = VisitModHomepage,
				Margin = PDialog.BUTTON_MARGIN
			}.SetKleiBlueStyle();
			PLabel child2 = new PLabel("ModVersion")
			{
				Text = displayInfo.Version,
				ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_VERSION),
				TextStyle = PUITuning.Fonts.UILightStyle,
				Margin = new RectOffset(0, 0, OUTER_MARGIN, 0)
			};
			string uRL = displayInfo.URL;
			if ((Object)(object)modImage != (Object)null)
			{
				if (optionCategories.Count > 0)
				{
					body.Direction = PanelDirection.Horizontal;
				}
				PPanel pPanel = new PPanel("ModInfo")
				{
					FlexSize = Vector2.up,
					Direction = PanelDirection.Vertical,
					Alignment = (TextAnchor)1
				}.AddChild(new PLabel("ModImage")
				{
					SpriteSize = MOD_IMAGE_SIZE,
					TextAlignment = (TextAnchor)0,
					Margin = new RectOffset(0, OUTER_MARGIN, 0, OUTER_MARGIN),
					Sprite = modImage
				});
				if (!string.IsNullOrEmpty(uRL))
				{
					pPanel.AddChild(child);
				}
				body.AddChild(pPanel.AddChild(child2));
			}
			else
			{
				if (!string.IsNullOrEmpty(uRL))
				{
					body.AddChild(child);
				}
				body.AddChild(child2);
			}
		}

		private void CloseDialog()
		{
			if ((Object)(object)dialog != (Object)null)
			{
				dialog.Deactivate();
				dialog = null;
			}
			if ((Object)(object)modImage != (Object)null)
			{
				Object.Destroy((Object)(object)modImage);
				modImage = null;
			}
		}

		private void FillModOptions(PDialog optionsDialog)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected O, but got Unknown
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			PPanel body = optionsDialog.Body;
			RectOffset margin = new RectOffset(CATEGORY_MARGIN, CATEGORY_MARGIN, CATEGORY_MARGIN, CATEGORY_MARGIN);
			body.Margin = new RectOffset();
			PPanel pPanel = new PPanel("ScrollContent")
			{
				Spacing = OUTER_MARGIN,
				Direction = PanelDirection.Vertical,
				Alignment = (TextAnchor)1,
				FlexSize = Vector2.right
			};
			IDictionary<string, ICollection<IOptionsEntry>> dictionary = optionCategories;
			IEnumerable<IOptionsEntry> enumerable;
			if (this.options is IOptions options && (enumerable = options.CreateOptions()) != null)
			{
				dictionary = new Dictionary<string, ICollection<IOptionsEntry>>(optionCategories);
				foreach (IOptionsEntry item in enumerable)
				{
					OptionsEntry.AddToCategory(dictionary, item);
				}
			}
			foreach (KeyValuePair<string, ICollection<IOptionsEntry>> item2 in dictionary)
			{
				string key = item2.Key;
				ICollection<IOptionsEntry> value = item2.Value;
				if (value.Count <= 0)
				{
					continue;
				}
				string text = (string.IsNullOrEmpty(key) ? "Default" : key);
				int row = 0;
				PGridPanel pGridPanel = new PGridPanel("Category_" + text)
				{
					Margin = margin,
					BackColor = PUITuning.Colors.DialogDarkBackground,
					FlexSize = Vector2.right
				};
				PGridPanel pGridPanel2 = new PGridPanel("Entries")
				{
					FlexSize = Vector2.right
				};
				AddCategoryHeader(pGridPanel, item2.Key, pGridPanel2);
				foreach (IOptionsEntry item3 in value)
				{
					pGridPanel2.AddRow(new GridRowSpec());
					item3.CreateUIEntry(pGridPanel2, ref row);
					row++;
				}
				pPanel.AddChild(pGridPanel);
			}
			pPanel.AddChild(new PPanel("ConfigButtons")
			{
				Spacing = 10,
				Direction = PanelDirection.Horizontal,
				Alignment = (TextAnchor)4,
				FlexSize = Vector2.right
			}.AddChild(new PButton("ManualConfig")
			{
				Text = LocString.op_Implicit(PLibStrings.BUTTON_MANUAL),
				ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_MANUAL),
				OnClick = OnManualConfig,
				TextAlignment = (TextAnchor)4,
				Margin = PDialog.BUTTON_MARGIN
			}.SetKleiBlueStyle()).AddChild(new PButton("ResetConfig")
			{
				Text = LocString.op_Implicit(PLibStrings.BUTTON_RESET),
				ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_RESET),
				OnClick = OnResetConfig,
				TextAlignment = (TextAnchor)4,
				Margin = PDialog.BUTTON_MARGIN
			}.SetKleiBlueStyle()));
			body.AddChild(new PScrollPane
			{
				ScrollHorizontal = false,
				ScrollVertical = (dictionary.Count > 0),
				Child = pPanel,
				FlexSize = Vector2.right,
				TrackSize = 8f,
				AlwaysShowHorizontal = false,
				AlwaysShowVertical = false
			});
		}

		private void OnManualConfig(GameObject _)
		{
			string text = null;
			string configFilePath = POptions.GetConfigFilePath(optionsType);
			try
			{
				text = new Uri(Path.GetDirectoryName(configFilePath) ?? configFilePath).AbsoluteUri;
			}
			catch (UriFormatException thrown)
			{
				PUtil.LogWarning("Unable to convert parent of " + configFilePath + " to a URI:");
				PUtil.LogExcWarn(thrown);
			}
			if (!string.IsNullOrEmpty(text))
			{
				bool num = WriteOptions();
				CloseDialog();
				PUtil.LogDebug("Opening config folder: " + text);
				Application.OpenURL(text);
				if (num)
				{
					PromptForRestart();
				}
			}
		}

		private void OnOptionsSelected(string action)
		{
			if (!(action == "ok"))
			{
				if (action == "close")
				{
					OnClose?.Invoke(options);
				}
			}
			else if (WriteOptions())
			{
				PromptForRestart();
			}
		}

		private void OnResetConfig(GameObject _)
		{
			options = CreateOptions(optionsType);
			UpdateOptions();
		}

		private void PromptForRestart()
		{
			PUIElements.ShowConfirmDialog(null, LocString.op_Implicit(PLibStrings.RESTART_REQUIRED), SaveAndRestart, null, LocString.op_Implicit(PLibStrings.RESTART_OK), LocString.op_Implicit(PLibStrings.RESTART_CANCEL));
		}

		public void ShowDialog()
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			string title = ((!string.IsNullOrEmpty(displayInfo.Title)) ? string.Format(LocString.op_Implicit(PLibStrings.DIALOG_TITLE), OptionsEntry.LookInStrings(displayInfo.Title)) : LocString.op_Implicit(PLibStrings.BUTTON_OPTIONS));
			CloseDialog();
			PDialog pDialog = new PDialog("ModOptions")
			{
				Title = title,
				Size = SETTINGS_DIALOG_SIZE,
				SortKey = 150f,
				DialogBackColor = PUITuning.Colors.OptionsBackground,
				DialogClosed = OnOptionsSelected,
				MaxSize = SETTINGS_DIALOG_MAX_SIZE,
				RoundToNearestEven = true
			}.AddButton("ok", LocString.op_Implicit(CONFIRMDIALOG.OK), LocString.op_Implicit(PLibStrings.TOOLTIP_OK), PUITuning.Colors.ButtonPinkStyle).AddButton("close", LocString.op_Implicit(CONFIRMDIALOG.CANCEL), LocString.op_Implicit(PLibStrings.TOOLTIP_CANCEL), PUITuning.Colors.ButtonBlueStyle);
			options = POptions.ReadSettings(POptions.GetConfigFilePath(optionsType), optionsType) ?? CreateOptions(optionsType);
			AddModInfoScreen(pDialog);
			FillModOptions(pDialog);
			GameObject obj = pDialog.Build();
			UpdateOptions();
			if (obj.TryGetComponent<KScreen>(ref dialog))
			{
				dialog.Activate();
			}
		}

		private void TriggerUpdateOptions(object newOptions)
		{
			if (newOptions is IOptions options)
			{
				options.OnOptionsChanged();
			}
			OnClose?.Invoke(newOptions);
		}

		private void UpdateOptions()
		{
			if (options == null)
			{
				return;
			}
			foreach (KeyValuePair<string, ICollection<IOptionsEntry>> optionCategory in optionCategories)
			{
				foreach (IOptionsEntry item in optionCategory.Value)
				{
					item.ReadFrom(options);
				}
			}
		}

		private void VisitModHomepage(GameObject _)
		{
			if (!string.IsNullOrWhiteSpace(displayInfo.URL))
			{
				Application.OpenURL(displayInfo.URL);
			}
		}

		private bool WriteOptions()
		{
			bool flag = false;
			if (options != null)
			{
				if (options.GetType().GetCustomAttribute(typeof(RestartRequiredAttribute)) != null)
				{
					flag = true;
				}
				foreach (KeyValuePair<string, ICollection<IOptionsEntry>> optionCategory in optionCategories)
				{
					foreach (IOptionsEntry item in optionCategory.Value)
					{
						flag |= item.WriteTo(options) && item.RestartRequired;
					}
				}
				POptions.WriteSettings(options, POptions.GetConfigFilePath(optionsType), configAttr?.IndentOutput ?? false);
				TriggerUpdateOptions(options);
			}
			return flag;
		}
	}
	public abstract class OptionsEntry : IOptionsEntry, IOptionSpec, IComparable<OptionsEntry>, IUIComponent
	{
		private const BindingFlags INSTANCE_PUBLIC = BindingFlags.Instance | BindingFlags.Public;

		protected static readonly RectOffset CONTROL_MARGIN = new RectOffset(0, 0, 2, 2);

		protected static readonly RectOffset LABEL_MARGIN = new RectOffset(0, 5, 2, 2);

		public string Category { get; }

		public string Field { get; }

		public string Format { get; }

		public virtual string Name => "OptionsEntry";

		public bool RestartRequired { get; set; }

		public string Title { get; protected set; }

		public string Tooltip { get; protected set; }

		public abstract object Value { get; set; }

		public event PUIDelegates.OnRealize OnRealize;

		internal static void AddToCategory(IDictionary<string, ICollection<IOptionsEntry>> entries, IOptionsEntry entry)
		{
			string key = entry.Category ?? "";
			if (!entries.TryGetValue(key, out var value))
			{
				value = new List<IOptionsEntry>(16);
				entries.Add(key, value);
			}
			value.Add(entry);
		}

		internal static IDictionary<string, ICollection<IOptionsEntry>> BuildOptions(Type forType)
		{
			SortedList<string, ICollection<IOptionsEntry>> sortedList = new SortedList<string, ICollection<IOptionsEntry>>(8);
			OptionsHandlers.InitPredefinedOptions();
			PropertyInfo[] properties = forType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			for (int i = 0; i < properties.Length; i++)
			{
				IOptionsEntry optionsEntry = TryCreateEntry(properties[i], 0);
				if (optionsEntry != null)
				{
					AddToCategory(sortedList, optionsEntry);
				}
			}
			return sortedList;
		}

		public static void CreateDefaultUIEntry(IOptionsEntry entry, PGridPanel parent, int row, IUIComponent presenter)
		{
			parent.AddChild(new PLabel("Label")
			{
				Text = LookInStrings(entry.Title),
				ToolTip = LookInStrings(entry.Tooltip),
				TextStyle = PUITuning.Fonts.TextLightStyle
			}, new GridComponentSpec(row, 0)
			{
				Margin = LABEL_MARGIN,
				Alignment = (TextAnchor)3
			});
			parent.AddChild(presenter, new GridComponentSpec(row, 1)
			{
				Alignment = (TextAnchor)5,
				Margin = CONTROL_MARGIN
			});
		}

		private static IOptionsEntry CreateDynamicOption(PropertyInfo prop, Type handler)
		{
			IOptionsEntry optionsEntry = null;
			ConstructorInfo[] constructors = handler.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
			string name = prop.Name;
			int num = constructors.Length;
			for (int i = 0; i < num; i++)
			{
				if (optionsEntry != null)
				{
					break;
				}
				try
				{
					if (ExecuteConstructor(prop, constructors[i]) is IOptionsEntry optionsEntry2)
					{
						optionsEntry = optionsEntry2;
					}
				}
				catch (TargetInvocationException ex)
				{
					PUtil.LogError("Unable to create option handler for property " + name + ":");
					PUtil.LogException(ex.GetBaseException());
				}
				catch (MemberAccessException)
				{
				}
				catch (AmbiguousMatchException)
				{
				}
				catch (TypeLoadException ex4)
				{
					PUtil.LogError("Unable to instantiate option handler for property " + name + ":");
					PUtil.LogException(ex4.GetBaseException());
				}
			}
			if (optionsEntry == null)
			{
				PUtil.LogWarning("Unable to create option handler for property " + name + ", it must have a public constructor");
			}
			return optionsEntry;
		}

		private static object ExecuteConstructor(PropertyInfo prop, ConstructorInfo cons)
		{
			ParameterInfo[] parameters = cons.GetParameters();
			int num = parameters.Length;
			if (num == 0)
			{
				return cons.Invoke(null);
			}
			object[] array = new object[num];
			for (int i = 0; i < num; i++)
			{
				Type parameterType = parameters[i].ParameterType;
				if (typeof(Attribute).IsAssignableFrom(parameterType))
				{
					array[i] = prop.GetCustomAttribute(parameterType);
				}
				else if (parameterType == typeof(IOptionSpec))
				{
					array[i] = prop.GetCustomAttribute<OptionAttribute>();
				}
				else if (parameterType == typeof(string))
				{
					array[i] = prop.Name;
				}
				else
				{
					PUtil.LogWarning("DynamicOption cannot handle constructor parameter of type " + parameterType.FullName);
				}
			}
			return cons.Invoke(array);
		}

		internal static IOptionSpec HandleDefaults(IOptionSpec spec, MemberInfo member)
		{
			string text = "STRINGS.{0}.OPTIONS.{1}.".F(member.DeclaringType?.Namespace?.ToUpperInvariant(), member.Name.ToUpperInvariant());
			string category = "";
			StringEntry val = default(StringEntry);
			if (Strings.TryGet(text + "CATEGORY", ref val))
			{
				category = val.String;
			}
			return new OptionAttribute(text + "NAME", text + "TOOLTIP", category)
			{
				Format = spec.Format
			};
		}

		public static string LookInStrings(string keyOrValue)
		{
			string result = keyOrValue;
			StringEntry val = default(StringEntry);
			if (!string.IsNullOrEmpty(keyOrValue) && Strings.TryGet(keyOrValue, ref val))
			{
				result = UI.StripLinkFormatting(val.String);
			}
			return result;
		}

		internal static IOptionsEntry TryCreateEntry(PropertyInfo prop, int depth)
		{
			IOptionsEntry optionsEntry = null;
			if (prop.GetIndexParameters().Length < 1)
			{
				bool flag = true;
				bool restart = false;
				object[] customAttributes = prop.GetCustomAttributes(inherit: false);
				int num = customAttributes.Length;
				for (int i = 0; i < num; i++)
				{
					object obj = customAttributes[i];
					if (obj is IRequireFilter requireFilter && !requireFilter.Filter())
					{
						flag = false;
					}
					else if (obj is RestartRequiredAttribute)
					{
						restart = true;
					}
				}
				if (flag)
				{
					for (int j = 0; j < num; j++)
					{
						optionsEntry = TryCreateEntry(customAttributes[j], prop, depth, restart);
						if (optionsEntry != null)
						{
							break;
						}
					}
				}
			}
			return optionsEntry;
		}

		private static IOptionsEntry TryCreateEntry(object attribute, PropertyInfo prop, int depth, bool restart)
		{
			IOptionsEntry optionsEntry = null;
			if (prop == null)
			{
				throw new ArgumentNullException("prop");
			}
			IOptionSpec optionSpec = attribute as IOptionSpec;
			if (optionSpec != null)
			{
				if (string.IsNullOrEmpty(optionSpec.Title))
				{
					optionSpec = HandleDefaults(optionSpec, prop);
				}
				Type propertyType = prop.PropertyType;
				optionsEntry = OptionsHandlers.FindOptionClass(optionSpec, prop);
				if (optionsEntry == null && !propertyType.IsValueType && depth < 16 && propertyType != prop.DeclaringType)
				{
					optionsEntry = CompositeOptionsEntry.Create(optionSpec, prop, depth);
				}
			}
			else if (attribute is DynamicOptionAttribute dynamicOptionAttribute && typeof(IOptionsEntry).IsAssignableFrom(dynamicOptionAttribute.Handler))
			{
				optionsEntry = CreateDynamicOption(prop, dynamicOptionAttribute.Handler);
			}
			if (optionsEntry != null)
			{
				optionsEntry.RestartRequired = restart;
			}
			return optionsEntry;
		}

		protected OptionsEntry(string field, IOptionSpec attr)
		{
			if (attr == null)
			{
				throw new ArgumentNullException("attr");
			}
			Field = field;
			Format = attr.Format;
			RestartRequired = false;
			Title = attr.Title ?? throw new ArgumentException("attr.Title is null");
			Tooltip = attr.Tooltip;
			Category = attr.Category;
		}

		public GameObject Build()
		{
			GameObject uIComponent = GetUIComponent();
			this.OnRealize?.Invoke(uIComponent);
			return uIComponent;
		}

		public int CompareTo(OptionsEntry other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			return string.Compare(Category, other.Category, StringComparison.CurrentCultureIgnoreCase);
		}

		public virtual void CreateUIEntry(PGridPanel parent, ref int row)
		{
			CreateDefaultUIEntry(this, parent, row, this);
		}

		public abstract GameObject GetUIComponent();

		public virtual void ReadFrom(object settings)
		{
			if (Field == null || settings == null)
			{
				return;
			}
			try
			{
				PropertyInfo property = settings.GetType().GetProperty(Field, BindingFlags.Instance | BindingFlags.Public);
				if (property != null && property.CanRead)
				{
					Value = property.GetValue(settings, null);
				}
			}
			catch (TargetInvocationException thrown)
			{
				PUtil.LogException(thrown);
			}
			catch (AmbiguousMatchException thrown2)
			{
				PUtil.LogException(thrown2);
			}
			catch (InvalidCastException thrown3)
			{
				PUtil.LogException(thrown3);
			}
		}

		public override string ToString()
		{
			return "{1}[field={0},title={2}]".F(Field, GetType().Name, Title);
		}

		public virtual bool WriteTo(object settings)
		{
			bool result = false;
			if (Field != null && settings != null)
			{
				try
				{
					PropertyInfo property = settings.GetType().GetProperty(Field, BindingFlags.Instance | BindingFlags.Public);
					if (property != null && property.CanWrite)
					{
						object value = Value;
						bool flag = false;
						if (property.CanRead)
						{
							object value2 = property.GetValue(settings, null);
							flag = ((value2 == null) ? (value != null) : (!value2.Equals(value)));
						}
						property.SetValue(settings, value, null);
						result = flag;
					}
				}
				catch (TargetInvocationException thrown)
				{
					PUtil.LogException(thrown);
				}
				catch (AmbiguousMatchException thrown2)
				{
					PUtil.LogException(thrown2);
				}
				catch (InvalidCastException thrown3)
				{
					PUtil.LogException(thrown3);
				}
			}
			return result;
		}
	}
	public static class OptionsHandlers
	{
		private delegate IOptionsEntry CreateOption(string field, IOptionSpec spec);

		private delegate IOptionsEntry CreateOptionType(string field, IOptionSpec spec, Type fieldType);

		private delegate IOptionsEntry CreateOptionLimit(string field, IOptionSpec spec, LimitAttribute limit);

		private static readonly IDictionary<Type, Delegate> OPTIONS_HANDLERS = new Dictionary<Type, Delegate>(64);

		public static void AddOptionClass(Type optionType, Type handlerType)
		{
			if (!(optionType != null) || !(handlerType != null) || OPTIONS_HANDLERS.ContainsKey(optionType) || !typeof(IOptionsEntry).IsAssignableFrom(handlerType))
			{
				return;
			}
			ConstructorInfo[] constructors = handlerType.GetConstructors();
			int num = constructors.Length;
			for (int i = 0; i < num; i++)
			{
				Delegate obj = CreateDelegate(handlerType, constructors[i]);
				if ((object)obj != null)
				{
					OPTIONS_HANDLERS[optionType] = obj;
					break;
				}
			}
		}

		private static Delegate CreateDelegate(Type handlerType, ConstructorInfo constructor)
		{
			ParameterInfo[] parameters = constructor.GetParameters();
			int num = parameters.Length;
			Delegate result = null;
			if (num > 1 && parameters[0].ParameterType.IsAssignableFrom(typeof(string)) && parameters[1].ParameterType.IsAssignableFrom(typeof(IOptionSpec)))
			{
				switch (num)
				{
				case 2:
					result = constructor.Detour<CreateOption>();
					break;
				case 3:
				{
					Type parameterType = parameters[2].ParameterType;
					if (parameterType.IsAssignableFrom(typeof(LimitAttribute)))
					{
						result = constructor.Detour<CreateOptionLimit>();
					}
					else if (parameterType.IsAssignableFrom(typeof(Type)))
					{
						result = constructor.Detour<CreateOptionType>();
					}
					break;
				}
				default:
					PUtil.LogWarning("Constructor on options handler type " + handlerType?.ToString() + " cannot be constructed by OptionsHandlers");
					break;
				}
			}
			return result;
		}

		public static IOptionsEntry FindOptionClass(IOptionSpec spec, PropertyInfo info)
		{
			IOptionsEntry result = null;
			if (spec != null && info != null)
			{
				Type propertyType = info.PropertyType;
				string name = info.Name;
				Delegate value;
				if (propertyType.IsEnum)
				{
					result = new SelectOneOptionsEntry(name, spec, propertyType);
				}
				else if (OPTIONS_HANDLERS.TryGetValue(propertyType, out value))
				{
					if (value is CreateOption createOption)
					{
						result = createOption(name, spec);
					}
					else if (value is CreateOptionLimit createOptionLimit)
					{
						result = createOptionLimit(name, spec, info.GetCustomAttribute<LimitAttribute>());
					}
					else if (value is CreateOptionType createOptionType)
					{
						result = createOptionType(name, spec, propertyType);
					}
				}
			}
			return result;
		}

		internal static void InitPredefinedOptions()
		{
			if (OPTIONS_HANDLERS.Count < 1)
			{
				AddOptionClass(typeof(bool), typeof(CheckboxOptionsEntry));
				AddOptionClass(typeof(int), typeof(IntOptionsEntry));
				AddOptionClass(typeof(int?), typeof(NullableIntOptionsEntry));
				AddOptionClass(typeof(float), typeof(FloatOptionsEntry));
				AddOptionClass(typeof(float?), typeof(NullableFloatOptionsEntry));
				AddOptionClass(typeof(Color32), typeof(Color32OptionsEntry));
				AddOptionClass(typeof(Color), typeof(ColorOptionsEntry));
				AddOptionClass(typeof(string), typeof(StringOptionsEntry));
				AddOptionClass(typeof(Action<object>), typeof(ButtonOptionsEntry));
				AddOptionClass(typeof(LocText), typeof(TextBlockOptionsEntry));
			}
		}
	}
	public sealed class POptions : PForwardedComponent
	{
		private sealed class ModOptionsHandler
		{
			private readonly Type forType;

			private readonly PForwardedComponent options;

			internal ModOptionsHandler(PForwardedComponent options, Type forType)
			{
				this.forType = forType ?? throw new ArgumentNullException("forType");
				this.options = options ?? throw new ArgumentNullException("options");
			}

			internal void ShowDialog(GameObject _)
			{
				options.Process(0u, new OpenDialogArgs(forType, null));
			}

			public override string ToString()
			{
				return "ModOptionsHandler[Type={0}]".F(forType);
			}
		}

		private sealed class OpenDialogArgs
		{
			public Action<object> OnClose { get; }

			public Type OptionsType { get; }

			public OpenDialogArgs(Type optionsType, Action<object> onClose)
			{
				OnClose = onClose;
				OptionsType = optionsType ?? throw new ArgumentNullException("optionsType");
			}

			public override string ToString()
			{
				return "OpenDialogArgs[Type={0}]".F(OptionsType);
			}
		}

		public const string CONFIG_FILE_NAME = "config.json";

		public const int MAX_SERIALIZATION_DEPTH = 8;

		internal static readonly RectOffset OPTION_BUTTON_MARGIN = new RectOffset(11, 11, 5, 5);

		public const string SHARED_CONFIG_FOLDER = "config";

		internal static readonly Version VERSION = new Version("4.24.0.0");

		private readonly IDictionary<string, Type> modOptions;

		private readonly IDictionary<string, ModOptionsHandler> registered;

		internal static POptions Instance { get; private set; }

		public override Version Version => VERSION;

		private static void BuildDisplay_Postfix(GameObject ___entryPrefab, IEnumerable ___displayedMods)
		{
			if (Instance == null)
			{
				return;
			}
			int num = 0;
			foreach (object? ___displayedMod in ___displayedMods)
			{
				Instance.AddModOptions(___displayedMod, num++, ___entryPrefab);
			}
		}

		public static string GetConfigFilePath(Type optionsType)
		{
			return GetConfigPath(optionsType.GetCustomAttribute<ConfigFileAttribute>(), optionsType.Assembly);
		}

		internal static Mod GetModFromType(Type optionsType)
		{
			if (optionsType == null)
			{
				throw new ArgumentNullException("optionsType");
			}
			if (!(PRegistry.Instance.GetSharedData(typeof(POptions).FullName) is IDictionary<Assembly, Mod> dictionary) || !dictionary.TryGetValue(optionsType.Assembly, out var value))
			{
				return null;
			}
			return value;
		}

		private static string GetConfigPath(ConfigFileAttribute attr, Assembly modAssembly)
		{
			string nameSafe = modAssembly.GetNameSafe();
			return Path.Combine((nameSafe != null && attr != null && attr.UseSharedConfigLocation) ? Path.Combine(Manager.GetDirectory(), "config", nameSafe) : PUtil.GetModPath(modAssembly), attr?.ConfigFileName ?? "config.json");
		}

		public static T ReadSettings<T>() where T : class
		{
			Type typeFromHandle = typeof(T);
			return ReadSettings(GetConfigPath(typeFromHandle.GetCustomAttribute<ConfigFileAttribute>(), typeFromHandle.Assembly), typeFromHandle) as T;
		}

		internal static object ReadSettings(string path, Type optionsType)
		{
			//IL_004e: Expected O, but got Unknown
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			object result = null;
			try
			{
				JsonTextReader val = new JsonTextReader((TextReader)File.OpenText(path));
				try
				{
					result = new JsonSerializer
					{
						MaxDepth = 8
					}.Deserialize((JsonReader)(object)val, optionsType);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (FileNotFoundException)
			{
			}
			catch (DirectoryNotFoundException)
			{
			}
			catch (UnauthorizedAccessException thrown)
			{
				PUtil.LogExcWarn(thrown);
			}
			catch (IOException thrown2)
			{
				PUtil.LogExcWarn(thrown2);
			}
			catch (JsonException ex3)
			{
				PUtil.LogExcWarn((Exception)ex3);
			}
			return result;
		}

		public static void ShowDialog(Type optionsType, Action<object> onClose = null)
		{
			OpenDialogArgs args = new OpenDialogArgs(optionsType, onClose);
			IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents(typeof(POptions).FullName);
			if (allComponents == null)
			{
				return;
			}
			foreach (PForwardedComponent item in allComponents)
			{
				item?.Process(0u, args);
			}
		}

		public static void WriteSettings<T>(T settings) where T : class
		{
			ConfigFileAttribute customAttribute = typeof(T).GetCustomAttribute<ConfigFileAttribute>();
			WriteSettings(settings, GetConfigPath(customAttribute, typeof(T).Assembly), customAttribute?.IndentOutput ?? false);
		}

		internal static void WriteSettings(object settings, string path, bool indent = false)
		{
			//IL_0061: Expected O, but got Unknown
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (settings == null)
			{
				return;
			}
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				JsonTextWriter val = new JsonTextWriter((TextWriter)File.CreateText(path));
				try
				{
					new JsonSerializer
					{
						MaxDepth = 8,
						Formatting = (Formatting)(indent ? 1 : 0)
					}.Serialize((JsonWriter)(object)val, settings);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (UnauthorizedAccessException thrown)
			{
				PUtil.LogExcWarn(thrown);
			}
			catch (IOException thrown2)
			{
				PUtil.LogExcWarn(thrown2);
			}
			catch (JsonException ex)
			{
				PUtil.LogExcWarn((Exception)ex);
			}
		}

		public POptions()
		{
			modOptions = new Dictionary<string, Type>(8);
			registered = new Dictionary<string, ModOptionsHandler>(32);
			InstanceData = modOptions;
		}

		private void AddModOptions(object modEntry, int fallbackIndex, GameObject parent)
		{
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			List<Mod> list = Global.Instance.modManager?.mods;
			if (!PPatchTools.TryGetFieldValue<int>(modEntry, "mod_index", out var value))
			{
				value = fallbackIndex;
			}
			if (!PPatchTools.TryGetFieldValue<Transform>(modEntry, "rect_transform", out var value2))
			{
				value2 = parent.transform.GetChild(value);
			}
			if (list == null || value < 0 || value >= list.Count || !((Object)(object)value2 != (Object)null))
			{
				return;
			}
			Mod val = list[value];
			string staticID = val.staticID;
			if (val.IsEnabledForActiveDlc() && registered.TryGetValue(staticID, out var value3))
			{
				string text = val.title;
				StringEntry val2 = default(StringEntry);
				if (Strings.TryGet(text, ref val2))
				{
					text = val2.String;
				}
				PButton pButton = new PButton("ModSettingsButton");
				pButton.FlexSize = Vector2.up;
				pButton.OnClick = value3.ShowDialog;
				pButton.ToolTip = PLibStrings.DIALOG_TITLE.text.F(text);
				pButton.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(PLibStrings.BUTTON_OPTIONS.text.ToLower());
				pButton.Margin = OPTION_BUTTON_MARGIN;
				pButton.SetKleiPinkStyle().AddTo(((Component)value2).gameObject, 4);
			}
		}

		public override void Initialize(Harmony plibInstance)
		{
			Instance = this;
			registered.Clear();
			SetSharedData(PUtil.CreateAssemblyToModTable());
			foreach (PForwardedComponent allComponent in PRegistry.Instance.GetAllComponents(base.ID))
			{
				IDictionary<string, Type> instanceData = allComponent.GetInstanceData<IDictionary<string, Type>>();
				if (instanceData == null)
				{
					continue;
				}
				foreach (KeyValuePair<string, Type> item in instanceData)
				{
					string key = item.Key;
					if (registered.ContainsKey(key))
					{
						PUtil.LogWarning("Mod {0} already has options registered - only one option type per mod".F(key ?? "?"));
					}
					else
					{
						registered.Add(key, new ModOptionsHandler(allComponent, item.Value));
					}
				}
			}
			plibInstance.Patch(typeof(ModsScreen), "BuildDisplay", null, PatchMethod("BuildDisplay_Postfix"));
		}

		public override void Process(uint operation, object args)
		{
			if (operation != 0 || !PPatchTools.TryGetPropertyValue<Type>(args, "OptionsType", out var value))
			{
				return;
			}
			foreach (KeyValuePair<string, Type> modOption in modOptions)
			{
				if (modOption.Value == value)
				{
					OptionsDialog optionsDialog = new OptionsDialog(value);
					if (PPatchTools.TryGetPropertyValue<Action<object>>(args, "OnClose", out var value2))
					{
						optionsDialog.OnClose = value2;
					}
					optionsDialog.ShowDialog();
					break;
				}
			}
		}

		public void RegisterOptions(UserMod2 mod, Type optionsType)
		{
			Mod obj = ((mod != null) ? mod.mod : null);
			if (optionsType == null)
			{
				throw new ArgumentNullException("optionsType");
			}
			if (obj == null)
			{
				throw new ArgumentNullException("mod");
			}
			RegisterForForwarding();
			string staticID = obj.staticID;
			if (modOptions.TryGetValue(staticID, out var value))
			{
				PUtil.LogWarning("Mod {0} already has options type {1}".F(staticID, value.FullName));
			}
			else
			{
				modOptions.Add(staticID, optionsType);
				PUtil.LogDebug("Registered mod options class {0} for {1}".F(optionsType.FullName, staticID));
			}
		}
	}
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public sealed class RequireDLCAttribute : Attribute, IRequireFilter
	{
		public string DlcID { get; }

		public bool Required { get; }

		public RequireDLCAttribute(string dlcID)
		{
			DlcID = dlcID ?? "";
			Required = true;
		}

		public RequireDLCAttribute(string dlcID, bool required)
		{
			DlcID = dlcID ?? "";
			Required = required;
		}

		public bool Filter()
		{
			return PGameUtils.IsDLCOwned(DlcID) == Required;
		}

		public override string ToString()
		{
			return "RequireDLC[DLC={0},require={1}]".F(DlcID, Required);
		}
	}
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public sealed class RequireModAttribute : Attribute, IRequireFilter
	{
		private static readonly ISet<string> ACTIVE_MODS = new HashSet<string>();

		public string StaticID { get; }

		public bool Required { get; }

		private static bool IsModActive(string staticID)
		{
			ISet<string> aCTIVE_MODS = ACTIVE_MODS;
			lock (aCTIVE_MODS)
			{
				if (aCTIVE_MODS.Count <= 0)
				{
					List<Mod> mods = Global.Instance.modManager.mods;
					int count = mods.Count;
					for (int i = 0; i < count; i++)
					{
						Mod val = mods[i];
						if (val.IsActive())
						{
							aCTIVE_MODS.Add(val.staticID);
						}
					}
				}
				return aCTIVE_MODS.Contains(staticID);
			}
		}

		public RequireModAttribute(string staticID)
		{
			StaticID = staticID ?? "";
			Required = true;
		}

		public RequireModAttribute(string staticID, bool required)
		{
			StaticID = staticID ?? "";
			Required = required;
		}

		public bool Filter()
		{
			return IsModActive(StaticID) == Required;
		}

		public override string ToString()
		{
			return "RequireMod[StaticID={0},require={1}]".F(StaticID, Required);
		}
	}
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public sealed class RequireVersionAttribute : Attribute, IRequireFilter
	{
		public uint Version { get; }

		public bool Minimum { get; }

		public RequireVersionAttribute(uint version)
		{
			Version = version;
			Minimum = true;
		}

		public RequireVersionAttribute(uint version, bool minimum)
		{
			Version = version;
			Minimum = minimum;
		}

		public bool Filter()
		{
			return PUtil.GameVersion >= Version == Minimum;
		}

		public override string ToString()
		{
			return "RequireVersion[version={0},minimum={1}]".F(Version, Minimum);
		}
	}
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class RestartRequiredAttribute : Attribute
	{
	}
	public class SelectOneOptionsEntry : OptionsEntry
	{
		protected sealed class EnumOption : ITooltipListableOption, IListableOption
		{
			public string Title { get; }

			public string ToolTip { get; }

			public object Value { get; }

			public EnumOption(string title, string toolTip, object value)
			{
				Title = title ?? throw new ArgumentNullException("title");
				ToolTip = toolTip;
				Value = value;
			}

			public string GetProperName()
			{
				return Title;
			}

			public string GetToolTipText()
			{
				return ToolTip;
			}

			public override string ToString()
			{
				return $"Option[Title={Title},Value={Value}]";
			}
		}

		protected EnumOption chosen;

		private GameObject comboBox;

		protected readonly IList<EnumOption> options;

		public override object Value
		{
			get
			{
				return chosen?.Value;
			}
			set
			{
				string text = value?.ToString() ?? "";
				foreach (EnumOption option in options)
				{
					if (text == option.Value.ToString())
					{
						chosen = option;
						Update();
						break;
					}
				}
			}
		}

		private static EnumOption GetAttribute(object enumValue, Type fieldType)
		{
			if (enumValue == null)
			{
				throw new ArgumentNullException("enumValue");
			}
			string text = enumValue.ToString();
			MemberInfo[] member = fieldType.GetMember(text, BindingFlags.Static | BindingFlags.Public);
			int num = member.Length;
			for (int i = 0; i < num; i++)
			{
				MemberInfo memberInfo = member[i];
				if (memberInfo.DeclaringType == fieldType)
				{
					return CreateOption(enumValue, memberInfo, text);
				}
			}
			return new EnumOption(text, "", enumValue);
		}

		private static EnumOption CreateOption(object enumValue, MemberInfo enumField, string valueName)
		{
			object[] customAttributes = enumField.GetCustomAttributes(inherit: false);
			int num = customAttributes.Length;
			string title = valueName;
			string toolTip = "";
			bool flag = true;
			bool flag2 = false;
			for (int i = 0; i < num && flag; i++)
			{
				object obj = customAttributes[i];
				IOptionSpec optionSpec = obj as IOptionSpec;
				if (optionSpec != null && !flag2)
				{
					if (string.IsNullOrEmpty(optionSpec.Title))
					{
						optionSpec = OptionsEntry.HandleDefaults(optionSpec, enumField);
					}
					title = OptionsEntry.LookInStrings(optionSpec.Title);
					toolTip = OptionsEntry.LookInStrings(optionSpec.Tooltip);
					flag2 = true;
				}
				if (obj is EnumMemberAttribute { IsValueSetExplicitly: not false } enumMemberAttribute && !flag2)
				{
					title = OptionsEntry.LookInStrings(enumMemberAttribute.Value);
					flag2 = true;
				}
				if (obj is IRequireFilter requireFilter && !requireFilter.Filter())
				{
					flag = false;
				}
			}
			if (!flag)
			{
				return null;
			}
			return new EnumOption(title, toolTip, enumValue);
		}

		public SelectOneOptionsEntry(string field, IOptionSpec spec, Type fieldType)
			: base(field, spec)
		{
			Array values = Enum.GetValues(fieldType);
			if (values == null)
			{
				throw new ArgumentException("No values, or invalid values, for enum");
			}
			int length = values.Length;
			if (length == 0)
			{
				throw new ArgumentException("Enum has no declared members");
			}
			chosen = null;
			comboBox = null;
			options = new List<EnumOption>(length);
			for (int i = 0; i < length; i++)
			{
				EnumOption attribute = GetAttribute(values.GetValue(i), fieldType);
				if (attribute != null)
				{
					options.Add(attribute);
				}
			}
			if (options.Count < 1)
			{
				options.Add(new EnumOption(LocString.op_Implicit(PLibStrings.OPTIONS_FILTERED), "", values.GetValue(0)));
			}
		}

		public override GameObject GetUIComponent()
		{
			EnumOption enumOption = null;
			int num = 0;
			foreach (EnumOption option in options)
			{
				int valueOrDefault = (option.Title?.Trim()?.Length).GetValueOrDefault();
				if (enumOption == null && valueOrDefault > 0)
				{
					enumOption = option;
				}
				if (valueOrDefault > num)
				{
					num = valueOrDefault;
				}
			}
			comboBox = new PComboBox<EnumOption>("Select")
			{
				BackColor = PUITuning.Colors.ButtonPinkStyle,
				InitialItem = enumOption,
				Content = options,
				EntryColor = PUITuning.Colors.ButtonBlueStyle,
				TextStyle = PUITuning.Fonts.TextLightStyle,
				OnOptionSelected = UpdateValue
			}.SetMinWidthInCharacters(num).Build();
			Update();
			return comboBox;
		}

		private void Update()
		{
			if ((Object)(object)comboBox != (Object)null && chosen != null)
			{
				PComboBox<EnumOption>.SetSelectedItem(comboBox, (IListableOption)(object)chosen);
			}
		}

		private void UpdateValue(GameObject _, EnumOption selected)
		{
			if (selected != null)
			{
				chosen = selected;
			}
		}
	}
	public abstract class SingletonOptions<T> where T : class, new()
	{
		protected static T instance;

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					instance = POptions.ReadSettings<T>() ?? new T();
				}
				return instance;
			}
			protected set
			{
				if (value != null)
				{
					instance = value;
				}
			}
		}
	}
	public abstract class SlidingBaseOptionsEntry : OptionsEntry
	{
		internal static readonly RectOffset ENTRY_MARGIN = new RectOffset(15, 0, 0, 5);

		internal static readonly RectOffset SLIDER_MARGIN = new RectOffset(10, 10, 0, 0);

		protected readonly LimitAttribute limits;

		protected GameObject slider;

		protected SlidingBaseOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
			: base(field, spec)
		{
			limits = limit;
			slider = null;
		}

		public override void CreateUIEntry(PGridPanel parent, ref int row)
		{
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			base.CreateUIEntry(parent, ref row);
			double minimum;
			double maximum;
			if (limits != null && (minimum = limits.Minimum) > -3.4028234663852886E+38 && (maximum = limits.Maximum) < 3.4028234663852886E+38 && maximum > minimum)
			{
				PSliderSingle pSliderSingle = GetSlider().AddOnRealize(OnRealizeSlider);
				PLabel pLabel = new PLabel("MinValue")
				{
					TextStyle = PUITuning.Fonts.TextLightStyle,
					Text = minimum.ToString(base.Format ?? "G3"),
					TextAlignment = (TextAnchor)5
				};
				PLabel pLabel2 = new PLabel("MaxValue")
				{
					TextStyle = PUITuning.Fonts.TextLightStyle,
					Text = maximum.ToString(base.Format ?? "G3"),
					TextAlignment = (TextAnchor)3
				};
				PRelativePanel child = new PRelativePanel("Slider Grid")
				{
					FlexSize = Vector2.right,
					DynamicSize = false
				}.AddChild(pSliderSingle).AddChild(pLabel).AddChild(pLabel2)
					.AnchorYAxis(pSliderSingle)
					.AnchorYAxis(pLabel)
					.AnchorYAxis(pLabel2)
					.SetLeftEdge(pLabel, 0f)
					.SetRightEdge(pLabel2, 1f)
					.SetLeftEdge(pSliderSingle, -1f, pLabel)
					.SetRightEdge(pSliderSingle, -1f, pLabel2)
					.SetMargin(pSliderSingle, SLIDER_MARGIN);
				parent.AddRow(new GridRowSpec());
				parent.AddChild(child, new GridComponentSpec(++row, 0)
				{
					ColumnSpan = 2,
					Margin = ENTRY_MARGIN
				});
			}
		}

		protected abstract PSliderSingle GetSlider();

		protected void OnRealizeSlider(GameObject realized)
		{
			slider = realized;
			Update();
		}

		protected abstract void Update();
	}
	public class StringOptionsEntry : OptionsEntry
	{
		public const int DEFAULT_WIDTH = 128;

		private readonly int maxLength;

		private GameObject textField;

		private string value;

		public override object Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = ((value == null) ? "" : value.ToString());
				Update();
			}
		}

		public int DisplayWidth { get; set; }

		public StringOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
			: base(field, spec)
		{
			if (limit != null)
			{
				maxLength = Math.Max(2, (int)Math.Round(limit.Maximum));
			}
			else
			{
				maxLength = 256;
			}
			DisplayWidth = 128;
			textField = null;
			value = "";
		}

		public override GameObject GetUIComponent()
		{
			textField = new PTextField
			{
				OnTextChanged = OnTextChanged,
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				Text = value.ToString(),
				MinWidth = Math.Max(8, DisplayWidth),
				Type = PTextField.FieldType.Text,
				MaxLength = maxLength
			}.Build();
			Update();
			return textField;
		}

		private void OnTextChanged(GameObject _, string text)
		{
			value = text;
			Update();
		}

		private void Update()
		{
			TMP_InputField componentInChildren;
			if ((Object)(object)textField != (Object)null && (Object)(object)(componentInChildren = textField.GetComponentInChildren<TMP_InputField>()) != (Object)null)
			{
				componentInChildren.text = value;
			}
		}
	}
	public class TextBlockOptionsEntry : OptionsEntry
	{
		private static readonly TextStyleSetting WRAP_TEXT_STYLE;

		private LocText ignore;

		public override object Value
		{
			get
			{
				return ignore;
			}
			set
			{
				LocText val = (LocText)((value is LocText) ? value : null);
				if (val != null)
				{
					ignore = val;
				}
			}
		}

		static TextBlockOptionsEntry()
		{
			WRAP_TEXT_STYLE = PUITuning.Fonts.TextLightStyle.DeriveStyle();
			WRAP_TEXT_STYLE.enableWordWrapping = true;
		}

		public TextBlockOptionsEntry(string field, IOptionSpec spec)
			: base(field, spec)
		{
		}

		public override void CreateUIEntry(PGridPanel parent, ref int row)
		{
			parent.AddChild(new PLabel(base.Field)
			{
				Text = OptionsEntry.LookInStrings(base.Title),
				ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
				TextStyle = WRAP_TEXT_STYLE
			}, new GridComponentSpec(row, 0)
			{
				Margin = OptionsEntry.CONTROL_MARGIN,
				Alignment = (TextAnchor)4,
				ColumnSpan = 2
			});
		}

		public override GameObject GetUIComponent()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			return new GameObject("Empty");
		}
	}
}
namespace PeterHan.PLib.UI
{
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
	public interface IDynamicSizable : IUIComponent
	{
		bool DynamicSize { get; set; }
	}
	[Flags]
	public enum ImageTransform : uint
	{
		None = 0u,
		FlipHorizontal = 1u,
		FlipVertical = 2u,
		Rotate90 = 4u,
		Rotate180 = 8u,
		Rotate270 = 0xCu
	}
	internal interface ISettableFlexSize : ILayoutGroup, ILayoutController
	{
		float flexibleWidth { get; set; }

		float flexibleHeight { get; set; }
	}
	public interface ITooltipListableOption : IListableOption
	{
		string GetToolTipText();
	}
	public interface IUIComponent
	{
		string Name { get; }

		event PUIDelegates.OnRealize OnRealize;

		GameObject Build();
	}
	internal struct LayoutSizes
	{
		public float flexible;

		public bool ignore;

		public float min;

		public float preferred;

		public readonly GameObject source;

		internal LayoutSizes(GameObject source)
			: this(source, 0f, 0f, 0f)
		{
		}

		internal LayoutSizes(GameObject source, float min, float preferred, float flexible)
		{
			ignore = false;
			this.source = source;
			this.flexible = flexible;
			this.min = min;
			this.preferred = preferred;
		}

		public void Add(LayoutSizes other)
		{
			flexible += other.flexible;
			min += other.min;
			preferred += other.preferred;
		}

		public void Max(LayoutSizes other)
		{
			flexible = Math.Max(flexible, other.flexible);
			min = Math.Max(min, other.min);
			preferred = Math.Max(preferred, other.preferred);
		}

		public override string ToString()
		{
			return $"LayoutSizes[min={min:F2},preferred={preferred:F2},flexible={flexible:F2}]";
		}
	}
	public sealed class BoxLayoutGroup : AbstractLayoutGroup
	{
		private BoxLayoutResults horizontal;

		[SerializeField]
		private BoxLayoutParams parameters;

		private BoxLayoutResults vertical;

		public BoxLayoutParams Params
		{
			get
			{
				return parameters;
			}
			set
			{
				parameters = value ?? throw new ArgumentNullException("Params");
			}
		}

		private static BoxLayoutResults Calc(GameObject obj, BoxLayoutParams args, PanelDirection direction)
		{
			RectTransform val = EntityTemplateExtensions.AddOrGet<RectTransform>(obj);
			int childCount = ((Transform)val).childCount;
			BoxLayoutResults boxLayoutResults = new BoxLayoutResults(direction, childCount);
			PooledList<Component, BoxLayoutGroup> val2 = ListPool<Component, BoxLayoutGroup>.Allocate();
			for (int i = 0; i < childCount; i++)
			{
				Transform child = ((Transform)val).GetChild(i);
				GameObject val3 = ((child != null) ? ((Component)child).gameObject : null);
				if (!((Object)(object)val3 != (Object)null) || !val3.activeInHierarchy)
				{
					continue;
				}
				((List<Component>)(object)val2).Clear();
				val3.GetComponents<Component>((List<Component>)(object)val2);
				LayoutSizes layoutSizes = PUIUtils.CalcSizes(val3, direction, (IEnumerable<Component>)val2);
				if (!layoutSizes.ignore)
				{
					if (args.Direction == direction)
					{
						boxLayoutResults.Accum(layoutSizes, args.Spacing);
					}
					else
					{
						boxLayoutResults.Expand(layoutSizes);
					}
					boxLayoutResults.children.Add(layoutSizes);
				}
			}
			val2.Recycle();
			return boxLayoutResults;
		}

		private static void DoLayout(BoxLayoutParams args, BoxLayoutResults required, float size)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			if (required == null)
			{
				throw new ArgumentNullException("required");
			}
			PanelDirection direction = required.direction;
			BoxLayoutStatus status = new BoxLayoutStatus(direction, (RectOffset)(((object)args.Margin) ?? ((object)new RectOffset())), size);
			if (args.Direction == direction)
			{
				DoLayoutLinear(required, args, status);
			}
			else
			{
				DoLayoutPerp(required, args, status);
			}
		}

		private static void DoLayoutLinear(BoxLayoutResults required, BoxLayoutParams args, BoxLayoutStatus status)
		{
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			LayoutSizes total = required.total;
			PooledList<ILayoutController, BoxLayoutGroup> val = ListPool<ILayoutController, BoxLayoutGroup>.Allocate();
			PanelDirection direction = args.Direction;
			float size = status.size;
			float num = 0f;
			float min = total.min;
			float preferred = total.preferred;
			float num2 = Math.Max(0f, size - preferred);
			float flexible = total.flexible;
			float num3 = status.offset;
			float spacing = args.Spacing;
			if (size > min && preferred > min)
			{
				num = Math.Min(1f, (size - min) / (preferred - min));
			}
			if (num2 > 0f && flexible == 0f)
			{
				num3 += PUIUtils.GetOffset(args.Alignment, status.direction, num2);
			}
			foreach (LayoutSizes child in required.children)
			{
				GameObject source = child.source;
				if (!((Object)(object)source != (Object)null) || !source.activeInHierarchy)
				{
					continue;
				}
				float num4 = child.min;
				if (num > 0f)
				{
					num4 += (child.preferred - child.min) * num;
				}
				if (num2 > 0f && flexible > 0f)
				{
					num4 += num2 * child.flexible / flexible;
				}
				EntityTemplateExtensions.AddOrGet<RectTransform>(source).SetInsetAndSizeFromParentEdge(status.edge, num3, num4);
				num3 += num4 + ((num4 > 0f) ? spacing : 0f);
				((List<ILayoutController>)(object)val).Clear();
				source.GetComponents<ILayoutController>((List<ILayoutController>)(object)val);
				foreach (ILayoutController item in (List<ILayoutController>)(object)val)
				{
					if (direction == PanelDirection.Horizontal)
					{
						item.SetLayoutHorizontal();
					}
					else
					{
						item.SetLayoutVertical();
					}
				}
			}
			val.Recycle();
		}

		private static void DoLayoutPerp(BoxLayoutResults required, BoxLayoutParams args, BoxLayoutStatus status)
		{
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			PooledList<ILayoutController, BoxLayoutGroup> val = ListPool<ILayoutController, BoxLayoutGroup>.Allocate();
			PanelDirection direction = args.Direction;
			float size = status.size;
			foreach (LayoutSizes child in required.children)
			{
				GameObject source = child.source;
				if (!((Object)(object)source != (Object)null) || !source.activeInHierarchy)
				{
					continue;
				}
				float num = size;
				if (child.flexible <= 0f)
				{
					num = Math.Min(num, child.preferred);
				}
				float num2 = ((size > num) ? PUIUtils.GetOffset(args.Alignment, status.direction, size - num) : 0f);
				EntityTemplateExtensions.AddOrGet<RectTransform>(source).SetInsetAndSizeFromParentEdge(status.edge, num2 + status.offset, num);
				((List<ILayoutController>)(object)val).Clear();
				source.GetComponents<ILayoutController>((List<ILayoutController>)(object)val);
				foreach (ILayoutController item in (List<ILayoutController>)(object)val)
				{
					if (direction == PanelDirection.Horizontal)
					{
						item.SetLayoutVertical();
					}
					else
					{
						item.SetLayoutHorizontal();
					}
				}
			}
			val.Recycle();
		}

		internal BoxLayoutGroup()
		{
			horizontal = null;
			base.layoutPriority = 1;
			parameters = new BoxLayoutParams();
			vertical = null;
		}

		public override void CalculateLayoutInputHorizontal()
		{
			if (!locked)
			{
				RectOffset margin = parameters.Margin;
				float num = ((margin == null) ? 0f : ((float)(margin.left + margin.right)));
				horizontal = Calc(((Component)this).gameObject, parameters, PanelDirection.Horizontal);
				LayoutSizes total = horizontal.total;
				base.minWidth = total.min + num;
				base.preferredWidth = total.preferred + num;
			}
		}

		public override void CalculateLayoutInputVertical()
		{
			if (!locked)
			{
				RectOffset margin = parameters.Margin;
				float num = ((margin == null) ? 0f : ((float)(margin.top + margin.bottom)));
				vertical = Calc(((Component)this).gameObject, parameters, PanelDirection.Vertical);
				LayoutSizes total = vertical.total;
				base.minHeight = total.min + num;
				base.preferredHeight = total.preferred + num;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			horizontal = null;
			vertical = null;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			horizontal = null;
			vertical = null;
		}

		public override void SetLayoutHorizontal()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			if (horizontal != null && !locked)
			{
				BoxLayoutParams args = parameters;
				BoxLayoutResults required = horizontal;
				Rect rect = base.rectTransform.rect;
				DoLayout(args, required, ((Rect)(ref rect)).width);
			}
		}

		public override void SetLayoutVertical()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			if (vertical != null && !locked)
			{
				BoxLayoutParams args = parameters;
				BoxLayoutResults required = vertical;
				Rect rect = base.rectTransform.rect;
				DoLayout(args, required, ((Rect)(ref rect)).height);
			}
		}
	}
	[Serializable]
	public sealed class BoxLayoutParams
	{
		public TextAnchor Alignment { get; set; }

		public PanelDirection Direction { get; set; }

		public RectOffset Margin { get; set; }

		public float Spacing { get; set; }

		public BoxLayoutParams()
		{
			Alignment = (TextAnchor)4;
			Direction = PanelDirection.Horizontal;
			Margin = null;
			Spacing = 0f;
		}

		public override string ToString()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return $"BoxLayoutParams[Alignment={Alignment},Direction={Direction},Spacing={Spacing:F2}]";
		}
	}
	public sealed class CardLayoutGroup : AbstractLayoutGroup
	{
		private CardLayoutResults horizontal;

		[SerializeField]
		private RectOffset margin;

		private CardLayoutResults vertical;

		public RectOffset Margin
		{
			get
			{
				return margin;
			}
			set
			{
				margin = value;
			}
		}

		private static CardLayoutResults Calc(GameObject obj, PanelDirection direction)
		{
			RectTransform val = EntityTemplateExtensions.AddOrGet<RectTransform>(obj);
			int childCount = ((Transform)val).childCount;
			CardLayoutResults cardLayoutResults = new CardLayoutResults(direction, childCount);
			PooledList<Component, BoxLayoutGroup> val2 = ListPool<Component, BoxLayoutGroup>.Allocate();
			for (int i = 0; i < childCount; i++)
			{
				Transform child = ((Transform)val).GetChild(i);
				GameObject val3 = ((child != null) ? ((Component)child).gameObject : null);
				if ((Object)(object)val3 != (Object)null)
				{
					bool activeInHierarchy = val3.activeInHierarchy;
					((List<Component>)(object)val2).Clear();
					val3.GetComponents<Component>((List<Component>)(object)val2);
					val3.SetActive(true);
					LayoutSizes layoutSizes = PUIUtils.CalcSizes(val3, direction, (IEnumerable<Component>)val2);
					if (!layoutSizes.ignore)
					{
						cardLayoutResults.Expand(layoutSizes);
						cardLayoutResults.children.Add(layoutSizes);
					}
					val3.SetActive(activeInHierarchy);
				}
			}
			val2.Recycle();
			return cardLayoutResults;
		}

		private static void DoLayout(RectOffset margin, CardLayoutResults required, float size)
		{
			if (required == null)
			{
				throw new ArgumentNullException("required");
			}
			PanelDirection direction = required.direction;
			PooledList<ILayoutController, BoxLayoutGroup> val = ListPool<ILayoutController, BoxLayoutGroup>.Allocate();
			size = ((direction != PanelDirection.Horizontal) ? (size - (float)(margin.top + margin.bottom)) : (size - (float)(margin.left + margin.right)));
			foreach (LayoutSizes child in required.children)
			{
				GameObject source = child.source;
				if (!((Object)(object)source != (Object)null))
				{
					continue;
				}
				float properSize = PUIUtils.GetProperSize(child, size);
				RectTransform val2 = EntityTemplateExtensions.AddOrGet<RectTransform>(source);
				if (direction == PanelDirection.Horizontal)
				{
					val2.SetInsetAndSizeFromParentEdge((Edge)0, (float)margin.left, properSize);
				}
				else
				{
					val2.SetInsetAndSizeFromParentEdge((Edge)2, (float)margin.top, properSize);
				}
				((List<ILayoutController>)(object)val).Clear();
				source.GetComponents<ILayoutController>((List<ILayoutController>)(object)val);
				foreach (ILayoutController item in (List<ILayoutController>)(object)val)
				{
					if (direction == PanelDirection.Horizontal)
					{
						item.SetLayoutHorizontal();
					}
					else
					{
						item.SetLayoutVertical();
					}
				}
			}
			val.Recycle();
		}

		internal CardLayoutGroup()
		{
			horizontal = null;
			base.layoutPriority = 1;
			vertical = null;
		}

		public override void CalculateLayoutInputHorizontal()
		{
			if (!locked)
			{
				RectOffset val = Margin;
				float num = ((val == null) ? 0f : ((float)(val.left + val.right)));
				horizontal = Calc(((Component)this).gameObject, PanelDirection.Horizontal);
				LayoutSizes total = horizontal.total;
				base.minWidth = total.min + num;
				base.preferredWidth = total.preferred + num;
			}
		}

		public override void CalculateLayoutInputVertical()
		{
			if (!locked)
			{
				RectOffset val = Margin;
				float num = ((val == null) ? 0f : ((float)(val.top + val.bottom)));
				vertical = Calc(((Component)this).gameObject, PanelDirection.Vertical);
				LayoutSizes total = vertical.total;
				base.minHeight = total.min + num;
				base.preferredHeight = total.preferred + num;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			horizontal = null;
			vertical = null;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			horizontal = null;
			vertical = null;
		}

		public void SetActiveCard(GameObject card)
		{
			int childCount = ((Component)this).transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = ((Component)this).transform.GetChild(i);
				GameObject val = ((child != null) ? ((Component)child).gameObject : null);
				if ((Object)(object)val != (Object)null)
				{
					val.SetActive((Object)(object)val == (Object)(object)card);
				}
			}
		}

		public void SetActiveCard(int index)
		{
			int childCount = ((Component)this).transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = ((Component)this).transform.GetChild(i);
				GameObject val = ((child != null) ? ((Component)child).gameObject : null);
				if ((Object)(object)val != (Object)null)
				{
					val.SetActive(i == index);
				}
			}
		}

		public override void SetLayoutHorizontal()
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (horizontal != null && !locked)
			{
				object obj = ((object)Margin) ?? ((object)new RectOffset());
				CardLayoutResults required = horizontal;
				Rect rect = base.rectTransform.rect;
				DoLayout((RectOffset)obj, required, ((Rect)(ref rect)).width);
			}
		}

		public override void SetLayoutVertical()
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (vertical != null && !locked)
			{
				object obj = ((object)Margin) ?? ((object)new RectOffset());
				CardLayoutResults required = vertical;
				Rect rect = base.rectTransform.rect;
				DoLayout((RectOffset)obj, required, ((Rect)(ref rect)).height);
			}
		}
	}
	[Serializable]
	internal sealed class GridComponent<T> : GridComponentSpec where T : class
	{
		public T Item { get; }

		internal GridComponent(GridComponentSpec spec, T item)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			base.Alignment = spec.Alignment;
			Item = item;
			base.Column = spec.Column;
			base.ColumnSpan = spec.ColumnSpan;
			base.Margin = spec.Margin;
			base.Row = spec.Row;
			base.RowSpan = spec.RowSpan;
		}
	}
	public class GridComponentSpec
	{
		public TextAnchor Alignment { get; set; }

		public int Column { get; set; }

		public int ColumnSpan { get; set; }

		public RectOffset Margin { get; set; }

		public int Row { get; set; }

		public int RowSpan { get; set; }

		internal GridComponentSpec()
		{
		}

		public GridComponentSpec(int row, int column)
		{
			if (row < 0)
			{
				throw new ArgumentOutOfRangeException("row");
			}
			if (column < 0)
			{
				throw new ArgumentOutOfRangeException("column");
			}
			Alignment = (TextAnchor)4;
			Row = row;
			Column = column;
			Margin = null;
			RowSpan = 1;
			ColumnSpan = 1;
		}

		public override string ToString()
		{
			return $"GridComponentSpec[Row={Row:D},Column={Column:D},RowSpan={RowSpan:D},ColumnSpan={ColumnSpan:D}]";
		}
	}
	[Serializable]
	public sealed class GridColumnSpec
	{
		public float FlexWidth { get; }

		public float Width { get; }

		public GridColumnSpec(float width = 0f, float flex = 0f)
		{
			if (width.IsNaNOrInfinity() || width < 0f)
			{
				throw new ArgumentOutOfRangeException("width");
			}
			if (flex.IsNaNOrInfinity() || flex < 0f)
			{
				throw new ArgumentOutOfRangeException("flex");
			}
			Width = width;
			FlexWidth = flex;
		}

		public override string ToString()
		{
			return $"GridColumnSpec[Width={Width:F2}]";
		}
	}
	[Serializable]
	public sealed class GridRowSpec
	{
		public float FlexHeight { get; }

		public float Height { get; }

		public GridRowSpec(float height = 0f, float flex = 0f)
		{
			if (height.IsNaNOrInfinity() || height < 0f)
			{
				throw new ArgumentOutOfRangeException("height");
			}
			if (flex.IsNaNOrInfinity() || flex < 0f)
			{
				throw new ArgumentOutOfRangeException("flex");
			}
			Height = height;
			FlexHeight = flex;
		}

		public override string ToString()
		{
			return $"GridRowSpec[Height={Height:F2}]";
		}
	}
	public sealed class PGridLayoutGroup : AbstractLayoutGroup
	{
		[SerializeField]
		private IList<GridComponent<GameObject>> children;

		[SerializeField]
		private IList<GridColumnSpec> columns;

		[SerializeField]
		private RectOffset margin;

		private GridLayoutResults results;

		[SerializeField]
		private IList<GridRowSpec> rows;

		public RectOffset Margin
		{
			get
			{
				return margin;
			}
			set
			{
				margin = value;
			}
		}

		private static float[] GetColumnWidths(GridLayoutResults results, float width, RectOffset margin)
		{
			int num = results.Columns;
			float num2 = ((margin != null) ? margin.left : 0);
			float num3 = ((margin != null) ? margin.right : 0);
			float num4 = width - num2 - num3;
			float totalFlexWidth = results.TotalFlexWidth;
			float num5 = ((totalFlexWidth > 0f) ? ((num4 - results.MinWidth) / totalFlexWidth) : 0f);
			float[] array = new float[num + 1];
			for (int i = 0; i < num; i++)
			{
				GridColumnSpec gridColumnSpec = results.ComputedColumnSpecs[i];
				array[i] = num2;
				num2 += gridColumnSpec.Width + gridColumnSpec.FlexWidth * num5;
			}
			array[num] = num2;
			return array;
		}

		private static float[] GetRowHeights(GridLayoutResults results, float height, RectOffset margin)
		{
			int num = results.Rows;
			float num2 = ((margin != null) ? margin.bottom : 0);
			float num3 = ((margin != null) ? margin.top : 0);
			float num4 = height - num2 - num3;
			float totalFlexHeight = results.TotalFlexHeight;
			float num5 = ((totalFlexHeight > 0f) ? ((num4 - results.MinHeight) / totalFlexHeight) : 0f);
			float[] array = new float[num + 1];
			for (int i = 0; i < num; i++)
			{
				GridRowSpec gridRowSpec = results.ComputedRowSpecs[i];
				array[i] = num2;
				num2 += gridRowSpec.Height + gridRowSpec.FlexHeight * num5;
			}
			array[num] = num2;
			return array;
		}

		private static bool SetFinalHeight(SizedGridComponent component, float[] rowY)
		{
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			RectOffset val = component.Margin;
			LayoutSizes verticalSize = component.VerticalSize;
			GameObject source = verticalSize.source;
			int num;
			if (!verticalSize.ignore)
			{
				num = (((Object)(object)source != (Object)null) ? 1 : 0);
				if (num != 0)
				{
					int num2 = rowY.Length - 1;
					int row = component.Row;
					int value = row + component.RowSpan;
					row = row.InRange(0, num2 - 1);
					value = value.InRange(1, num2);
					float num3 = rowY[row];
					float num4 = rowY[value] - num3;
					if (val != null)
					{
						float num5 = val.top + val.bottom;
						num3 += (float)val.top;
						num4 -= num5;
						verticalSize.min -= num5;
						verticalSize.preferred -= num5;
					}
					float properSize = PUIUtils.GetProperSize(verticalSize, num4);
					num3 += PUIUtils.GetOffset(component.Alignment, PanelDirection.Vertical, num4 - properSize);
					Util.rectTransform(source).SetInsetAndSizeFromParentEdge((Edge)2, num3, properSize);
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		}

		private static bool SetFinalWidth(SizedGridComponent component, float[] colX)
		{
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			RectOffset val = component.Margin;
			LayoutSizes horizontalSize = component.HorizontalSize;
			GameObject source = horizontalSize.source;
			int num;
			if (!horizontalSize.ignore)
			{
				num = (((Object)(object)source != (Object)null) ? 1 : 0);
				if (num != 0)
				{
					int num2 = colX.Length - 1;
					int column = component.Column;
					int value = column + component.ColumnSpan;
					column = column.InRange(0, num2 - 1);
					value = value.InRange(1, num2);
					float num3 = colX[column];
					float num4 = colX[value] - num3;
					if (val != null)
					{
						float num5 = val.left + val.right;
						num3 += (float)val.left;
						num4 -= num5;
						horizontalSize.min -= num5;
						horizontalSize.preferred -= num5;
					}
					float properSize = PUIUtils.GetProperSize(horizontalSize, num4);
					num3 += PUIUtils.GetOffset(component.Alignment, PanelDirection.Horizontal, num4 - properSize);
					Util.rectTransform(source).SetInsetAndSizeFromParentEdge((Edge)0, num3, properSize);
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		}

		internal PGridLayoutGroup()
		{
			children = new List<GridComponent<GameObject>>(16);
			columns = new List<GridColumnSpec>(16);
			rows = new List<GridRowSpec>(16);
			base.layoutPriority = 1;
			Margin = null;
			results = null;
		}

		public void AddColumn(GridColumnSpec column)
		{
			if (column == null)
			{
				throw new ArgumentNullException("column");
			}
			columns.Add(column);
		}

		public void AddComponent(GameObject child, GridComponentSpec spec)
		{
			if ((Object)(object)child == (Object)null)
			{
				throw new ArgumentNullException("child");
			}
			if (spec == null)
			{
				throw new ArgumentNullException("spec");
			}
			children.Add(new GridComponent<GameObject>(spec, child));
			child.SetParent(((Component)this).gameObject);
		}

		public void AddRow(GridRowSpec row)
		{
			if (row == null)
			{
				throw new ArgumentNullException("row");
			}
			rows.Add(row);
		}

		public override void CalculateLayoutInputHorizontal()
		{
			if (locked)
			{
				return;
			}
			results = new GridLayoutResults(rows, columns, children);
			PooledList<Component, PGridLayoutGroup> val = ListPool<Component, PGridLayoutGroup>.Allocate();
			foreach (SizedGridComponent component in results.Components)
			{
				GameObject source = component.HorizontalSize.source;
				RectOffset val2 = component.Margin;
				((List<Component>)(object)val).Clear();
				source.GetComponents<Component>((List<Component>)(object)val);
				LayoutSizes horizontalSize = PUIUtils.CalcSizes(source, PanelDirection.Horizontal, (IEnumerable<Component>)val);
				if (!horizontalSize.ignore)
				{
					int num = ((val2 != null) ? (val2.left + val2.right) : 0);
					horizontalSize.min += num;
					horizontalSize.preferred += num;
				}
				component.HorizontalSize = horizontalSize;
			}
			val.Recycle();
			results.CalcBaseWidths();
			float num2 = results.MinWidth;
			if (Margin != null)
			{
				num2 += (float)(Margin.left + Margin.right);
			}
			float num3 = (base.preferredWidth = num2);
			base.minWidth = num3;
			base.flexibleWidth = ((results.TotalFlexWidth > 0f) ? 1f : 0f);
		}

		public override void CalculateLayoutInputVertical()
		{
			if (results == null || locked)
			{
				return;
			}
			PooledList<Component, PGridLayoutGroup> val = ListPool<Component, PGridLayoutGroup>.Allocate();
			foreach (SizedGridComponent component in results.Components)
			{
				GameObject source = component.VerticalSize.source;
				RectOffset val2 = component.Margin;
				((List<Component>)(object)val).Clear();
				source.GetComponents<Component>((List<Component>)(object)val);
				LayoutSizes verticalSize = PUIUtils.CalcSizes(source, PanelDirection.Vertical, (IEnumerable<Component>)val);
				if (!verticalSize.ignore)
				{
					int num = ((val2 != null) ? (val2.top + val2.bottom) : 0);
					verticalSize.min += num;
					verticalSize.preferred += num;
				}
				component.VerticalSize = verticalSize;
			}
			val.Recycle();
			results.CalcBaseHeights();
			float num2 = results.MinHeight;
			if (Margin != null)
			{
				num2 += (float)(Margin.bottom + Margin.top);
			}
			float num3 = (base.preferredHeight = num2);
			base.minHeight = num3;
			base.flexibleHeight = ((results.TotalFlexHeight > 0f) ? 1f : 0f);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			results = null;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			results = null;
		}

		public override void SetLayoutHorizontal()
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			GameObject gameObject = ((Component)this).gameObject;
			if (results == null || !((Object)(object)gameObject != (Object)null) || results.Columns <= 0 || locked)
			{
				return;
			}
			GridLayoutResults gridLayoutResults = results;
			Rect rect = base.rectTransform.rect;
			float[] columnWidths = GetColumnWidths(gridLayoutResults, ((Rect)(ref rect)).width, Margin);
			PooledList<ILayoutController, PGridLayoutGroup> val = ListPool<ILayoutController, PGridLayoutGroup>.Allocate();
			foreach (SizedGridComponent component in results.Components)
			{
				if (!SetFinalWidth(component, columnWidths))
				{
					continue;
				}
				((List<ILayoutController>)(object)val).Clear();
				component.HorizontalSize.source.GetComponents<ILayoutController>((List<ILayoutController>)(object)val);
				foreach (ILayoutController item in (List<ILayoutController>)(object)val)
				{
					item.SetLayoutHorizontal();
				}
			}
			val.Recycle();
		}

		public override void SetLayoutVertical()
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			GameObject gameObject = ((Component)this).gameObject;
			if (results == null || !((Object)(object)gameObject != (Object)null) || results.Rows <= 0 || locked)
			{
				return;
			}
			GridLayoutResults gridLayoutResults = results;
			Rect rect = base.rectTransform.rect;
			float[] rowHeights = GetRowHeights(gridLayoutResults, ((Rect)(ref rect)).height, Margin);
			PooledList<ILayoutController, PGridLayoutGroup> val = ListPool<ILayoutController, PGridLayoutGroup>.Allocate();
			foreach (SizedGridComponent component in results.Components)
			{
				if (SetFinalHeight(component, rowHeights))
				{
					continue;
				}
				((List<ILayoutController>)(object)val).Clear();
				component.VerticalSize.source.GetComponents<ILayoutController>((List<ILayoutController>)(object)val);
				foreach (ILayoutController item in (List<ILayoutController>)(object)val)
				{
					item.SetLayoutVertical();
				}
			}
			val.Recycle();
		}
	}
	public sealed class RelativeLayoutGroup : AbstractLayoutGroup, ISerializationCallbackReceiver
	{
		private readonly IDictionary<GameObject, RelativeLayoutParams> locConstraints;

		[SerializeField]
		private IList<KeyValuePair<GameObject, RelativeLayoutParams>> serialConstraints;

		[SerializeField]
		private RectOffset margin;

		private readonly IList<RelativeLayoutResults> results;

		public RectOffset Margin
		{
			get
			{
				return margin;
			}
			set
			{
				margin = value;
			}
		}

		internal RelativeLayoutGroup()
		{
			base.layoutPriority = 1;
			locConstraints = new Dictionary<GameObject, RelativeLayoutParams>(32);
			results = new List<RelativeLayoutResults>(32);
			serialConstraints = null;
		}

		private RelativeLayoutParams AddOrGet(GameObject item)
		{
			if (!locConstraints.TryGetValue(item, out var value))
			{
				value = (locConstraints[item] = new RelativeLayoutParams());
			}
			return value;
		}

		public RelativeLayoutGroup AnchorXAxis(GameObject item, float anchor = 0.5f)
		{
			SetLeftEdge(item, anchor);
			return SetRightEdge(item, anchor);
		}

		public RelativeLayoutGroup AnchorYAxis(GameObject item, float anchor = 0.5f)
		{
			SetTopEdge(item, anchor);
			return SetBottomEdge(item, anchor);
		}

		public override void CalculateLayoutInputHorizontal()
		{
			GameObject gameObject = ((Component)this).gameObject;
			RectTransform val = ((gameObject != null) ? Util.rectTransform(gameObject) : null);
			if (!((Object)(object)val != (Object)null) || locked)
			{
				return;
			}
			int num;
			int num2;
			if (Margin == null)
			{
				num = (num2 = 0);
			}
			else
			{
				num = Margin.left;
				num2 = Margin.right;
			}
			results.CalcX(val, locConstraints);
			if (results.Count > 0)
			{
				int num3 = 2 * results.Count;
				int i;
				for (i = 0; i < num3; i++)
				{
					if (results.RunPassX())
					{
						break;
					}
				}
				if (i >= num3)
				{
					results.ThrowUnresolvable(i, PanelDirection.Horizontal);
				}
			}
			float num4 = (base.preferredWidth = results.GetMinSizeX() + (float)num + (float)num2);
			base.minWidth = num4;
		}

		public override void CalculateLayoutInputVertical()
		{
			GameObject gameObject = ((Component)this).gameObject;
			if (!((Object)(object)((gameObject != null) ? Util.rectTransform(gameObject) : null) != (Object)null) || locked)
			{
				return;
			}
			int num = 2 * results.Count;
			int num2;
			int num3;
			if (Margin == null)
			{
				num2 = (num3 = 0);
			}
			else
			{
				num2 = Margin.top;
				num3 = Margin.bottom;
			}
			if (results.Count > 0)
			{
				results.CalcY();
				int i;
				for (i = 0; i < num; i++)
				{
					if (results.RunPassY())
					{
						break;
					}
				}
				if (i >= num)
				{
					results.ThrowUnresolvable(i, PanelDirection.Vertical);
				}
			}
			float num4 = (base.preferredHeight = results.GetMinSizeY() + (float)num2 + (float)num3);
			base.minHeight = num4;
		}

		internal void Import(IDictionary<GameObject, RelativeLayoutParams> values)
		{
			locConstraints.Clear();
			foreach (KeyValuePair<GameObject, RelativeLayoutParams> value in values)
			{
				locConstraints[value.Key] = value.Value;
			}
		}

		public void OnBeforeSerialize()
		{
			int count = locConstraints.Count;
			if (count <= 0)
			{
				return;
			}
			serialConstraints = new List<KeyValuePair<GameObject, RelativeLayoutParams>>(count);
			foreach (KeyValuePair<GameObject, RelativeLayoutParams> locConstraint in locConstraints)
			{
				serialConstraints.Add(locConstraint);
			}
		}

		public void OnAfterDeserialize()
		{
			if (serialConstraints == null)
			{
				return;
			}
			locConstraints.Clear();
			foreach (KeyValuePair<GameObject, RelativeLayoutParams> serialConstraint in serialConstraints)
			{
				locConstraints[serialConstraint.Key] = serialConstraint.Value;
			}
			serialConstraints = null;
		}

		public RelativeLayoutGroup OverrideSize(GameObject item, Vector2 size)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)item != (Object)null)
			{
				AddOrGet(item).OverrideSize = size;
			}
			return this;
		}

		public RelativeLayoutGroup SetBottomEdge(GameObject item, float fraction = -1f, GameObject above = null)
		{
			if ((Object)(object)item != (Object)null)
			{
				if ((Object)(object)above == (Object)(object)item)
				{
					throw new ArgumentException("Component cannot refer directly to itself");
				}
				SetEdge(AddOrGet(item).BottomEdge, fraction, above);
			}
			return this;
		}

		protected override void SetDirty()
		{
			if (!locked)
			{
				base.SetDirty();
			}
		}

		private void SetEdge(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, float fraction, GameObject child)
		{
			if (fraction >= 0f && fraction <= 1f)
			{
				edge.Constraint = RelativeConstraintType.ToAnchor;
				edge.FromAnchor = fraction;
				edge.FromComponent = null;
			}
			else if ((Object)(object)child != (Object)null)
			{
				edge.Constraint = RelativeConstraintType.ToComponent;
				edge.FromComponent = child;
			}
			else
			{
				edge.Constraint = RelativeConstraintType.Unconstrained;
				edge.FromComponent = null;
			}
		}

		public override void SetLayoutHorizontal()
		{
			if (!locked && results.Count > 0)
			{
				PooledList<ILayoutController, RelativeLayoutGroup> val = ListPool<ILayoutController, RelativeLayoutGroup>.Allocate();
				int num;
				int num2;
				if (Margin == null)
				{
					num = (num2 = 0);
				}
				else
				{
					num = Margin.left;
					num2 = Margin.right;
				}
				results.ExecuteX((List<ILayoutController>)(object)val, num, num2);
				val.Recycle();
			}
		}

		public override void SetLayoutVertical()
		{
			if (!locked && results.Count > 0)
			{
				PooledList<ILayoutController, RelativeLayoutGroup> val = ListPool<ILayoutController, RelativeLayoutGroup>.Allocate();
				int num;
				int num2;
				if (Margin == null)
				{
					num = (num2 = 0);
				}
				else
				{
					num = Margin.top;
					num2 = Margin.bottom;
				}
				results.ExecuteY((List<ILayoutController>)(object)val, num, num2);
				val.Recycle();
			}
		}

		public RelativeLayoutGroup SetLeftEdge(GameObject item, float fraction = -1f, GameObject toRight = null)
		{
			if ((Object)(object)item != (Object)null)
			{
				if ((Object)(object)toRight == (Object)(object)item)
				{
					throw new ArgumentException("Component cannot refer directly to itself");
				}
				SetEdge(AddOrGet(item).LeftEdge, fraction, toRight);
			}
			return this;
		}

		public RelativeLayoutGroup SetMargin(GameObject item, RectOffset insets)
		{
			if ((Object)(object)item != (Object)null)
			{
				AddOrGet(item).Insets = insets;
			}
			return this;
		}

		internal void SetRaw(GameObject item, RelativeLayoutParams rawParams)
		{
			if ((Object)(object)item != (Object)null && rawParams != null)
			{
				locConstraints[item] = rawParams;
			}
		}

		public RelativeLayoutGroup SetRightEdge(GameObject item, float fraction = -1f, GameObject toLeft = null)
		{
			if ((Object)(object)item != (Object)null)
			{
				if ((Object)(object)toLeft == (Object)(object)item)
				{
					throw new ArgumentException("Component cannot refer directly to itself");
				}
				SetEdge(AddOrGet(item).RightEdge, fraction, toLeft);
			}
			return this;
		}

		public RelativeLayoutGroup SetTopEdge(GameObject item, float fraction = -1f, GameObject below = null)
		{
			if ((Object)(object)item != (Object)null)
			{
				if ((Object)(object)below == (Object)(object)item)
				{
					throw new ArgumentException("Component cannot refer directly to itself");
				}
				SetEdge(AddOrGet(item).TopEdge, fraction, below);
			}
			return this;
		}
	}
	public class PButton : PTextComponent
	{
		internal static readonly RectOffset BUTTON_MARGIN = new RectOffset(7, 7, 5, 5);

		public ColorStyleSetting Color { get; set; }

		public PUIDelegates.OnButtonPressed OnClick { get; set; }

		internal static void SetupButton(KButton button, KImage bgImage)
		{
			UIDetours.ADDITIONAL_K_IMAGES.Set(button, (KImage[])(object)new KImage[0]);
			UIDetours.SOUND_PLAYER_BUTTON.Set(button, PUITuning.ButtonSounds);
			UIDetours.BG_IMAGE.Set(button, bgImage);
		}

		internal static void SetupButtonBackground(KImage bgImage)
		{
			UIDetours.APPLY_COLOR_STYLE.Invoke(bgImage);
			((Image)bgImage).sprite = PUITuning.Images.ButtonBorder;
			((Image)bgImage).type = (Type)1;
		}

		public static void SetButtonEnabled(GameObject obj, bool enabled)
		{
			KButton arg = default(KButton);
			if ((Object)(object)obj != (Object)null && obj.TryGetComponent<KButton>(ref arg))
			{
				UIDetours.IS_INTERACTABLE.Set(arg, enabled);
			}
		}

		public PButton()
			: this(null)
		{
		}

		public PButton(string name)
			: base(name ?? "Button")
		{
			base.Margin = BUTTON_MARGIN;
			base.Sprite = null;
			base.Text = null;
			base.ToolTip = "";
		}

		public PButton AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			base.OnRealize += onRealize;
			return this;
		}

		public override GameObject Build()
		{
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			GameObject button = PUIElements.CreateUI(null, base.Name);
			GameObject sprite = null;
			GameObject text = null;
			KImage val = button.AddComponent<KImage>();
			ColorStyleSetting arg = Color ?? PUITuning.Colors.ButtonPinkStyle;
			UIDetours.COLOR_STYLE_SETTING.Set(val, arg);
			SetupButtonBackground(val);
			KButton val2 = button.AddComponent<KButton>();
			PUIDelegates.OnButtonPressed evt = OnClick;
			if (evt != null)
			{
				val2.onClick += delegate
				{
					evt?.Invoke(button);
				};
			}
			SetupButton(val2, val);
			if ((Object)(object)base.Sprite != (Object)null)
			{
				Image val3 = PTextComponent.ImageChildHelper(button, this);
				UIDetours.FG_IMAGE.Set(val2, val3);
				sprite = ((Component)val3).gameObject;
			}
			if (!string.IsNullOrEmpty(base.Text))
			{
				text = ((Component)PTextComponent.TextChildHelper(button, base.TextStyle ?? PUITuning.Fonts.UILightStyle, base.Text)).gameObject;
			}
			PUIElements.SetToolTip(button, base.ToolTip).SetActive(true);
			RelativeLayoutGroup relativeLayoutGroup = button.AddComponent<RelativeLayoutGroup>();
			relativeLayoutGroup.Margin = base.Margin;
			PTextComponent.ArrangeComponent(relativeLayoutGroup, WrapTextAndSprite(text, sprite), base.TextAlignment);
			if (!base.DynamicSize)
			{
				relativeLayoutGroup.LockLayout();
			}
			relativeLayoutGroup.flexibleWidth = base.FlexSize.x;
			relativeLayoutGroup.flexibleHeight = base.FlexSize.y;
			DestroyLayoutIfPossible(button);
			InvokeRealize(button);
			return button;
		}

		public PButton SetImageLeftArrow()
		{
			base.Sprite = PUITuning.Images.Arrow;
			base.SpriteTransform = ImageTransform.FlipHorizontal;
			return this;
		}

		public PButton SetImageRightArrow()
		{
			base.Sprite = PUITuning.Images.Arrow;
			base.SpriteTransform = ImageTransform.None;
			return this;
		}

		public PButton SetKleiPinkStyle()
		{
			base.TextStyle = PUITuning.Fonts.UILightStyle;
			Color = PUITuning.Colors.ButtonPinkStyle;
			return this;
		}

		public PButton SetKleiBlueStyle()
		{
			base.TextStyle = PUITuning.Fonts.UILightStyle;
			Color = PUITuning.Colors.ButtonBlueStyle;
			return this;
		}
	}
	public class PCheckBox : PTextComponent
	{
		private const float CHECKBOX_MARGIN = 2f;

		public const int STATE_UNCHECKED = 0;

		public const int STATE_CHECKED = 1;

		public const int STATE_PARTIAL = 2;

		public ColorStyleSetting CheckColor { get; set; }

		public Color BackColor { get; set; }

		public Vector2 CheckSize { get; set; }

		public Color ComponentBackColor { get; set; }

		public int InitialState { get; set; }

		public PUIDelegates.OnChecked OnChecked { get; set; }

		private static ToggleState[] GenerateStates(ColorStyleSetting imageColor)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			StatePresentationSetting val = new StatePresentationSetting
			{
				color = imageColor.activeColor,
				use_color_on_hover = true,
				color_on_hover = imageColor.hoverColor,
				image_target = null,
				name = "Partial"
			};
			return (ToggleState[])(object)new ToggleState[3]
			{
				new ToggleState
				{
					color = PUITuning.Colors.Transparent,
					color_on_hover = PUITuning.Colors.Transparent,
					sprite = null,
					use_color_on_hover = false,
					additional_display_settings = (StatePresentationSetting[])(object)new StatePresentationSetting[1]
					{
						new StatePresentationSetting
						{
							color = imageColor.activeColor,
							use_color_on_hover = false,
							image_target = null,
							name = "Unchecked"
						}
					}
				},
				new ToggleState
				{
					color = imageColor.activeColor,
					color_on_hover = imageColor.hoverColor,
					sprite = PUITuning.Images.Checked,
					use_color_on_hover = true,
					additional_display_settings = (StatePresentationSetting[])(object)new StatePresentationSetting[1] { val }
				},
				new ToggleState
				{
					color = imageColor.activeColor,
					color_on_hover = imageColor.hoverColor,
					sprite = PUITuning.Images.Partial,
					use_color_on_hover = true,
					additional_display_settings = (StatePresentationSetting[])(object)new StatePresentationSetting[1] { val }
				}
			};
		}

		public static int GetCheckState(GameObject realized)
		{
			int result = 0;
			if ((Object)(object)realized == (Object)null)
			{
				throw new ArgumentNullException("realized");
			}
			MultiToggle componentInChildren = realized.GetComponentInChildren<MultiToggle>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				result = UIDetours.CURRENT_STATE.Get(componentInChildren);
			}
			return result;
		}

		public static void SetCheckState(GameObject realized, int state)
		{
			if ((Object)(object)realized == (Object)null)
			{
				throw new ArgumentNullException("realized");
			}
			MultiToggle componentInChildren = realized.GetComponentInChildren<MultiToggle>();
			if ((Object)(object)componentInChildren != (Object)null && UIDetours.CURRENT_STATE.Get(componentInChildren) != state)
			{
				UIDetours.CHANGE_STATE.Invoke(componentInChildren, state);
			}
		}

		public PCheckBox()
			: this(null)
		{
		}

		public PCheckBox(string name)
			: base(name ?? "CheckBox")
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			BackColor = PUITuning.Colors.BackgroundLight;
			CheckColor = null;
			CheckSize = new Vector2(16f, 16f);
			ComponentBackColor = PUITuning.Colors.Transparent;
			base.IconSpacing = 3;
			InitialState = 0;
			base.Sprite = null;
			base.Text = null;
			base.ToolTip = "";
		}

		public PCheckBox AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			base.OnRealize += onRealize;
			return this;
		}

		public override GameObject Build()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_021b: Unknown result type (might be due to invalid IL or missing references)
			GameObject checkbox = PUIElements.CreateUI(null, base.Name);
			Vector2 actualSize = CheckSize;
			GameObject sprite = null;
			GameObject text = null;
			if (ComponentBackColor.a > 0f)
			{
				((Graphic)checkbox.AddComponent<Image>()).color = ComponentBackColor;
			}
			ColorStyleSetting val = CheckColor ?? PUITuning.Colors.ComponentLightStyle;
			GameObject val2 = PUIElements.CreateUI(checkbox, "CheckBox");
			((Graphic)val2.AddComponent<Image>()).color = BackColor;
			Image toggle_image = CreateCheckImage(val2, val, ref actualSize);
			val2.SetUISize(new Vector2(actualSize.x + 4f, actualSize.y + 4f), addLayout: true);
			if ((Object)(object)base.Sprite != (Object)null)
			{
				sprite = ((Component)PTextComponent.ImageChildHelper(checkbox, this)).gameObject;
			}
			if (!string.IsNullOrEmpty(base.Text))
			{
				text = ((Component)PTextComponent.TextChildHelper(checkbox, base.TextStyle ?? PUITuning.Fonts.UILightStyle, base.Text)).gameObject;
			}
			MultiToggle mToggle = checkbox.AddComponent<MultiToggle>();
			PUIDelegates.OnChecked evt = OnChecked;
			if (evt != null)
			{
				MultiToggle obj = mToggle;
				obj.onClick = (Action)Delegate.Combine(obj.onClick, (Action)delegate
				{
					evt(checkbox, mToggle.CurrentState);
				});
			}
			UIDetours.PLAY_SOUND_CLICK.Set(mToggle, arg2: true);
			UIDetours.PLAY_SOUND_RELEASE.Set(mToggle, arg2: false);
			mToggle.states = GenerateStates(val);
			mToggle.toggle_image = toggle_image;
			UIDetours.CHANGE_STATE.Invoke(mToggle, InitialState);
			PUIElements.SetToolTip(checkbox, base.ToolTip).SetActive(true);
			GameObject text2 = WrapTextAndSprite(text, sprite);
			RelativeLayoutGroup relativeLayoutGroup = checkbox.AddComponent<RelativeLayoutGroup>();
			relativeLayoutGroup.Margin = base.Margin;
			PTextComponent.ArrangeComponent(relativeLayoutGroup, WrapTextAndSprite(text2, val2), base.TextAlignment);
			if (!base.DynamicSize)
			{
				relativeLayoutGroup.LockLayout();
			}
			relativeLayoutGroup.flexibleWidth = base.FlexSize.x;
			relativeLayoutGroup.flexibleHeight = base.FlexSize.y;
			DestroyLayoutIfPossible(checkbox);
			InvokeRealize(checkbox);
			return checkbox;
		}

		private Image CreateCheckImage(GameObject checkbox, ColorStyleSetting color, ref Vector2 actualSize)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			Image obj = PUIElements.CreateUI(checkbox, "CheckBorder").AddComponent<Image>();
			obj.sprite = PUITuning.Images.CheckBorder;
			((Graphic)obj).color = color.activeColor;
			obj.type = (Type)1;
			GameObject val = PUIElements.CreateUI(checkbox, "CheckMark", canvas: true, PUIAnchoring.Center, PUIAnchoring.Center);
			Image obj2 = val.AddComponent<Image>();
			obj2.sprite = PUITuning.Images.Checked;
			obj2.preserveAspect = true;
			if (actualSize.x <= 0f || actualSize.y <= 0f)
			{
				RectTransform val2 = Util.rectTransform(val);
				actualSize.x = LayoutUtility.GetPreferredWidth(val2);
				actualSize.y = LayoutUtility.GetPreferredHeight(val2);
			}
			val.SetUISize(CheckSize);
			return obj2;
		}

		public PCheckBox SetKleiPinkStyle()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			base.TextStyle = PUITuning.Fonts.UILightStyle;
			BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
			CheckColor = PUITuning.Colors.ComponentDarkStyle;
			return this;
		}

		public PCheckBox SetKleiBlueStyle()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			base.TextStyle = PUITuning.Fonts.UILightStyle;
			BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
			CheckColor = PUITuning.Colors.ComponentDarkStyle;
			return this;
		}
	}
	public sealed class PComboBox<T> : IUIComponent, IDynamicSizable where T : class, IListableOption
	{
		private static readonly RectOffset DEFAULT_ITEM_MARGIN = new RectOffset(3, 3, 3, 3);

		public Vector2 ArrowSize { get; set; }

		public ColorStyleSetting BackColor { get; set; }

		public Vector2 CheckSize { get; set; }

		public IEnumerable<T> Content { get; set; }

		public bool DynamicSize { get; set; }

		public ColorStyleSetting EntryColor { get; set; }

		public Vector2 FlexSize { get; set; }

		public T InitialItem { get; set; }

		public RectOffset ItemMargin { get; set; }

		public RectOffset Margin { get; set; }

		public int MaxRowsShown { get; set; }

		public int MinWidth { get; set; }

		public string Name { get; }

		public PUIDelegates.OnDropdownChanged<T> OnOptionSelected { get; set; }

		public TextAnchor TextAlignment { get; set; }

		public TextStyleSetting TextStyle { get; set; }

		public string ToolTip { get; set; }

		public event PUIDelegates.OnRealize OnRealize;

		public static void SetSelectedItem(GameObject realized, IListableOption option, bool fireListener = false)
		{
			PComboBoxComponent pComboBoxComponent = default(PComboBoxComponent);
			if (option != null && (Object)(object)realized != (Object)null && realized.TryGetComponent<PComboBoxComponent>(ref pComboBoxComponent))
			{
				pComboBoxComponent.SetSelectedItem(option, fireListener);
			}
		}

		public PComboBox()
			: this("Dropdown")
		{
		}

		public PComboBox(string name)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			ArrowSize = new Vector2(8f, 8f);
			BackColor = null;
			CheckSize = new Vector2(12f, 12f);
			Content = null;
			DynamicSize = false;
			FlexSize = Vector2.zero;
			InitialItem = null;
			ItemMargin = DEFAULT_ITEM_MARGIN;
			Margin = PButton.BUTTON_MARGIN;
			MaxRowsShown = 6;
			MinWidth = 0;
			Name = name;
			TextAlignment = (TextAnchor)3;
			TextStyle = null;
			ToolTip = null;
		}

		public PComboBox<T> AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			OnRealize += onRealize;
			return this;
		}

		public GameObject Build()
		{
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a3: Expected O, but got Unknown
			//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c2: Expected O, but got Unknown
			//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_0317: Unknown result type (might be due to invalid IL or missing references)
			//IL_0329: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = PUIElements.CreateUI(null, Name);
			TextStyleSetting val2 = TextStyle ?? PUITuning.Fonts.UILightStyle;
			ColorStyleSetting val3 = EntryColor ?? PUITuning.Colors.ButtonBlueStyle;
			RectOffset margin = Margin;
			RectOffset itemMargin = ItemMargin;
			KImage val4 = val.AddComponent<KImage>();
			ColorStyleSetting arg = BackColor ?? PUITuning.Colors.ButtonBlueStyle;
			UIDetours.COLOR_STYLE_SETTING.Set(val4, arg);
			PButton.SetupButtonBackground(val4);
			GameObject val5 = PUIElements.CreateUI(val, "SelectedItem");
			if (MinWidth > 0)
			{
				val5.SetMinUISize(new Vector2((float)MinWidth, 0f));
			}
			LocText selectedLabel = PUIElements.AddLocText(val5, val2);
			GameObject val6 = PUIElements.CreateUI(null, "Content");
			((HorizontalOrVerticalLayoutGroup)val6.AddComponent<VerticalLayoutGroup>()).childForceExpandWidth = true;
			GameObject val7 = new PScrollPane("PullDown")
			{
				ScrollHorizontal = false,
				ScrollVertical = true,
				AlwaysShowVertical = true,
				FlexSize = Vector2.right,
				TrackSize = 8f,
				BackColor = val3.inactiveColor
			}.BuildScrollPane(val, val6);
			Util.rectTransform(val7).pivot = new Vector2(0.5f, 1f);
			PComboBoxComponent pComboBoxComponent = val.AddComponent<PComboBoxComponent>();
			pComboBoxComponent.CheckColor = val2.textColor;
			pComboBoxComponent.ContentContainer = Util.rectTransform(val6);
			pComboBoxComponent.EntryPrefab = BuildRowPrefab(val2, val3);
			pComboBoxComponent.MaxRowsShown = MaxRowsShown;
			pComboBoxComponent.Pulldown = val7;
			pComboBoxComponent.SelectedLabel = (TMP_Text)(object)selectedLabel;
			pComboBoxComponent.SetItems((IEnumerable<IListableOption>)Content);
			pComboBoxComponent.SetSelectedItem((IListableOption)(object)InitialItem);
			pComboBoxComponent.OnSelectionChanged = delegate(PComboBoxComponent obj, IListableOption item)
			{
				OnOptionSelected?.Invoke(((Component)obj).gameObject, item as T);
			};
			GameObject val8 = PUIElements.CreateUI(val, "OpenImage");
			Image val9 = val8.AddComponent<Image>();
			val9.sprite = PUITuning.Images.Contract;
			((Graphic)val9).color = val2.textColor;
			KButton val10 = val.AddComponent<KButton>();
			PButton.SetupButton(val10, val4);
			UIDetours.FG_IMAGE.Set(val10, val9);
			val10.onClick += pComboBoxComponent.OnClick;
			PUIElements.SetToolTip(val5, ToolTip);
			val.SetActive(true);
			RelativeLayoutGroup relativeLayoutGroup = val.AddComponent<RelativeLayoutGroup>();
			relativeLayoutGroup.AnchorYAxis(val5).SetLeftEdge(val5, 0f).SetRightEdge(val5, -1f, val8)
				.AnchorYAxis(val8)
				.SetRightEdge(val8, 1f)
				.SetMargin(val5, new RectOffset(margin.left, itemMargin.right, margin.top, margin.bottom))
				.SetMargin(val8, new RectOffset(0, margin.right, margin.top, margin.bottom))
				.OverrideSize(val8, ArrowSize)
				.AnchorYAxis(val7, 0f)
				.OverrideSize(val7, Vector2.up);
			relativeLayoutGroup.LockLayout();
			if (DynamicSize)
			{
				relativeLayoutGroup.UnlockLayout();
			}
			EntityTemplateExtensions.AddOrGet<LayoutElement>(val7).ignoreLayout = true;
			val7.SetActive(false);
			relativeLayoutGroup.flexibleWidth = FlexSize.x;
			relativeLayoutGroup.flexibleHeight = FlexSize.y;
			this.OnRealize?.Invoke(val);
			return val;
		}

		private GameObject BuildRowPrefab(TextStyleSetting style, ColorStyleSetting entryColor)
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Expected O, but got Unknown
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			RectOffset itemMargin = ItemMargin;
			GameObject obj = PUIElements.CreateUI(null, "RowEntry");
			KImage val = obj.AddComponent<KImage>();
			UIDetours.COLOR_STYLE_SETTING.Set(val, entryColor);
			UIDetours.APPLY_COLOR_STYLE.Invoke(val);
			GameObject val2 = PUIElements.CreateUI(obj, "Selected");
			Image val3 = val2.AddComponent<Image>();
			((Graphic)val3).color = style.textColor;
			val3.preserveAspect = true;
			val3.sprite = PUITuning.Images.Checked;
			KButton val4 = obj.AddComponent<KButton>();
			PButton.SetupButton(val4, val);
			UIDetours.FG_IMAGE.Set(val4, val3);
			obj.AddComponent<ToolTip>();
			GameObject val5 = PUIElements.CreateUI(obj, "Text");
			((TMP_Text)PUIElements.AddLocText(val5, style)).SetText("X");
			obj.AddComponent<RelativeLayoutGroup>().AnchorYAxis(val2).OverrideSize(val2, CheckSize)
				.SetLeftEdge(val2, 0f)
				.SetMargin(val2, itemMargin)
				.AnchorYAxis(val5)
				.SetLeftEdge(val5, -1f, val2)
				.SetRightEdge(val5, 1f)
				.SetMargin(val5, new RectOffset(0, itemMargin.right, itemMargin.top, itemMargin.bottom))
				.LockLayout();
			obj.SetActive(false);
			return obj;
		}

		public PComboBox<T> SetKleiPinkStyle()
		{
			TextStyle = PUITuning.Fonts.UILightStyle;
			BackColor = PUITuning.Colors.ButtonPinkStyle;
			return this;
		}

		public PComboBox<T> SetKleiBlueStyle()
		{
			TextStyle = PUITuning.Fonts.UILightStyle;
			BackColor = PUITuning.Colors.ButtonBlueStyle;
			return this;
		}

		public PComboBox<T> SetMinWidthInCharacters(int chars)
		{
			int num = Mathf.RoundToInt((float)chars * PUIUtils.GetEmWidth(TextStyle));
			if (num > 0)
			{
				MinWidth = num;
			}
			return this;
		}

		public override string ToString()
		{
			return $"PComboBox[Name={Name}]";
		}
	}
	internal sealed class PComboBoxComponent : KMonoBehaviour
	{
		private struct ComboBoxItem
		{
			public readonly IListableOption data;

			public readonly Image rowImage;

			public readonly GameObject rowInstance;

			public ComboBoxItem(IListableOption data, GameObject rowInstance)
			{
				this.data = data;
				this.rowInstance = rowInstance;
				rowImage = KMonoBehaviourExtensions.GetComponentInChildrenOnly<Image>(rowInstance);
			}
		}

		private sealed class MouseEventHandler : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
		{
			public bool IsOver { get; private set; }

			internal MouseEventHandler()
			{
				IsOver = true;
			}

			public void OnPointerEnter(PointerEventData data)
			{
				IsOver = true;
			}

			public void OnPointerExit(PointerEventData data)
			{
				IsOver = false;
			}
		}

		private readonly IList<ComboBoxItem> currentItems;

		private MouseEventHandler handler;

		private bool open;

		internal RectTransform ContentContainer { get; set; }

		internal Color CheckColor { get; set; }

		internal GameObject EntryPrefab { get; set; }

		internal int MaxRowsShown { get; set; }

		internal Action<PComboBoxComponent, IListableOption> OnSelectionChanged { get; set; }

		internal GameObject Pulldown { get; set; }

		internal TMP_Text SelectedLabel { get; set; }

		internal PComboBoxComponent()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			CheckColor = Color.white;
			currentItems = new List<ComboBoxItem>(32);
			handler = null;
			MaxRowsShown = 8;
			open = false;
		}

		public void Close()
		{
			GameObject pulldown = Pulldown;
			if (pulldown != null)
			{
				pulldown.SetActive(false);
			}
			open = false;
		}

		public void OnClick()
		{
			if (open)
			{
				Close();
			}
			else
			{
				Open();
			}
		}

		protected override void OnPrefabInit()
		{
			((KMonoBehaviour)this).OnPrefabInit();
			handler = EntityTemplateExtensions.AddOrGet<MouseEventHandler>(((Component)this).gameObject);
		}

		public void Open()
		{
			GameObject pulldown = Pulldown;
			if ((Object)(object)pulldown != (Object)null)
			{
				float num = 0f;
				int count = currentItems.Count;
				int num2 = Math.Min(MaxRowsShown, count);
				Canvas val = EntityTemplateExtensions.AddOrGet<Canvas>(pulldown);
				pulldown.SetActive(true);
				if (count > 0)
				{
					RectTransform obj = Util.rectTransform(currentItems[0].rowInstance);
					LayoutRebuilder.ForceRebuildLayoutImmediate(obj);
					num = LayoutUtility.GetPreferredHeight(obj);
				}
				Util.rectTransform(pulldown).SetSizeWithCurrentAnchors((Axis)1, (float)num2 * num);
				ScrollRect val2 = default(ScrollRect);
				if (pulldown.TryGetComponent<ScrollRect>(ref val2))
				{
					val2.vertical = count >= MaxRowsShown;
				}
				if ((Object)(object)val != (Object)null)
				{
					val.overrideSorting = true;
					val.sortingOrder = 2;
				}
			}
			open = true;
		}

		public void SetItems(IEnumerable<IListableOption> items)
		{
			if (items == null)
			{
				return;
			}
			RectTransform contentContainer = ContentContainer;
			_ = Pulldown;
			int childCount = ((Transform)contentContainer).childCount;
			GameObject entryPrefab = EntryPrefab;
			bool flag = open;
			if (flag)
			{
				Close();
			}
			for (int i = 0; i < childCount; i++)
			{
				Object.Destroy((Object)(object)((Transform)contentContainer).GetChild(i));
			}
			currentItems.Clear();
			ToolTip val2 = default(ToolTip);
			foreach (IListableOption item in items)
			{
				string text = "";
				GameObject val = Util.KInstantiate(entryPrefab, ((Component)contentContainer).gameObject, (string)null);
				((TMP_Text)val.GetComponentInChildren<TextMeshProUGUI>()).SetText(item.GetProperName());
				KButton componentInChildren = val.GetComponentInChildren<KButton>();
				componentInChildren.ClearOnClick();
				componentInChildren.onClick += delegate
				{
					SetSelectedItem(item, fireListener: true);
					Close();
				};
				if (item is ITooltipListableOption tooltipListableOption)
				{
					text = tooltipListableOption.GetToolTipText();
				}
				if (val.TryGetComponent<ToolTip>(ref val2))
				{
					if (string.IsNullOrEmpty(text))
					{
						val2.ClearMultiStringTooltip();
					}
					else
					{
						val2.SetSimpleTooltip(text);
					}
				}
				val.SetActive(true);
				currentItems.Add(new ComboBoxItem(item, val));
			}
			TMP_Text selectedLabel = SelectedLabel;
			if (selectedLabel != null)
			{
				selectedLabel.SetText((currentItems.Count > 0) ? currentItems[0].data.GetProperName() : "");
			}
			if (flag)
			{
				Open();
			}
		}

		public void SetSelectedItem(IListableOption option, bool fireListener = false)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			if (option == null)
			{
				return;
			}
			TMP_Text selectedLabel = SelectedLabel;
			if (selectedLabel != null)
			{
				selectedLabel.SetText(option.GetProperName());
			}
			foreach (ComboBoxItem currentItem in currentItems)
			{
				IListableOption data = currentItem.data;
				((Graphic)currentItem.rowImage).color = ((data != null && ((object)data).Equals((object?)option)) ? CheckColor : PUITuning.Colors.Transparent);
			}
			if (fireListener)
			{
				OnSelectionChanged?.Invoke(this, option);
			}
		}

		internal void Update()
		{
			if (open && (Object)(object)handler != (Object)null && !handler.IsOver && (Input.GetMouseButton(0) || Input.GetAxis("Mouse ScrollWheel") != 0f))
			{
				Close();
			}
		}
	}
	public abstract class PContainer : IUIComponent
	{
		public Color BackColor { get; set; }

		public Sprite BackImage { get; set; }

		public Vector2 FlexSize { get; set; }

		public Type ImageMode { get; set; }

		public RectOffset Margin { get; set; }

		public string Name { get; protected set; }

		public event PUIDelegates.OnRealize OnRealize;

		protected PContainer(string name)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			BackColor = PUITuning.Colors.Transparent;
			BackImage = null;
			FlexSize = Vector2.zero;
			ImageMode = (Type)0;
			Margin = null;
			Name = name ?? "Container";
		}

		public abstract GameObject Build();

		protected void InvokeRealize(GameObject obj)
		{
			this.OnRealize?.Invoke(obj);
		}

		protected void SetImage(GameObject panel)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			if (BackColor.a > 0f || (Object)(object)BackImage != (Object)null)
			{
				Image val = panel.AddComponent<Image>();
				((Graphic)val).color = BackColor;
				if ((Object)(object)BackImage != (Object)null)
				{
					val.sprite = BackImage;
					val.type = ImageMode;
				}
			}
		}

		public override string ToString()
		{
			return $"PContainer[Name={Name}]";
		}
	}
	public sealed class PDialog : IUIComponent
	{
		private sealed class DialogButton
		{
			public readonly ColorStyleSetting backColor;

			public readonly string key;

			public readonly string text;

			public readonly TextStyleSetting textColor;

			public readonly string tooltip;

			internal DialogButton(string key, string text, string tooltip, ColorStyleSetting backColor, TextStyleSetting foreColor)
			{
				this.backColor = backColor;
				this.key = key;
				this.text = text;
				this.tooltip = tooltip;
				textColor = foreColor;
			}

			public override string ToString()
			{
				return $"DialogButton[key={key:D},text={text:D}]";
			}
		}

		private sealed class PDialogComp : KScreen
		{
			internal PDialog dialog;

			internal string key;

			internal float sortKey;

			internal PDialogComp()
			{
				key = "close";
				sortKey = 0f;
			}

			internal void DoButton(GameObject source)
			{
				key = ((Object)source).name;
				UIDetours.DEACTIVATE_KSCREEN.Invoke((KScreen)(object)this);
			}

			public override float GetSortKey()
			{
				return sortKey;
			}

			protected override void OnDeactivate()
			{
				if (dialog != null)
				{
					dialog.DialogClosed?.Invoke(key);
				}
				((KScreen)this).OnDeactivate();
				dialog = null;
			}

			public override void OnKeyDown(KButtonEvent e)
			{
				if (e.TryConsume((Action)1))
				{
					UIDetours.DEACTIVATE_KSCREEN.Invoke((KScreen)(object)this);
				}
				else
				{
					((KScreen)this).OnKeyDown(e);
				}
			}
		}

		public static readonly RectOffset BUTTON_MARGIN = new RectOffset(13, 13, 13, 13);

		private static readonly RectOffset CLOSE_ICON_MARGIN = new RectOffset(4, 4, 4, 4);

		private static readonly Vector2 CLOSE_ICON_SIZE = Vector2f.op_Implicit(new Vector2f(16f, 16f));

		public const string DIALOG_KEY_CLOSE = "close";

		private readonly ICollection<DialogButton> buttons;

		public PPanel Body { get; }

		public Color DialogBackColor { get; set; }

		public Vector2 MaxSize { get; set; }

		public string Name { get; }

		public GameObject Parent { get; set; }

		public bool RoundToNearestEven { get; set; }

		public Vector2 Size { get; set; }

		public float SortKey { get; set; }

		public string Title { get; set; }

		public PUIDelegates.OnDialogClosed DialogClosed { get; set; }

		public event PUIDelegates.OnRealize OnRealize;

		public static GameObject GetParentObject()
		{
			GameObject result = null;
			FrontEndManager instance = FrontEndManager.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				result = ((Component)instance).gameObject;
			}
			else
			{
				GameScreenManager instance2 = GameScreenManager.Instance;
				if ((Object)(object)instance2 != (Object)null)
				{
					result = instance2.ssOverlayCanvas;
				}
				else
				{
					PUIUtils.LogUIWarning("No dialog parent found!");
				}
			}
			return result;
		}

		private static float RoundUpSize(float size, float maxSize)
		{
			int num = Mathf.CeilToInt(size);
			if (num % 2 == 1)
			{
				num++;
			}
			if ((float)num > maxSize && maxSize > 0f)
			{
				num -= 2;
			}
			return num;
		}

		public PDialog(string name)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			Body = new PPanel("Body")
			{
				Alignment = (TextAnchor)1,
				FlexSize = Vector2.one,
				Margin = new RectOffset(6, 6, 6, 6)
			};
			DialogBackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
			buttons = new List<DialogButton>(4);
			MaxSize = Vector2.zero;
			Name = name ?? "Dialog";
			Parent = GetParentObject();
			RoundToNearestEven = false;
			Size = Vector2.zero;
			SortKey = 0f;
			Title = "Dialog";
		}

		public PDialog AddButton(string key, string text, string tooltip = null)
		{
			buttons.Add(new DialogButton(key, text, tooltip, null, null));
			return this;
		}

		public PDialog AddButton(string key, string text, string tooltip = null, ColorStyleSetting backColor = null, TextStyleSetting foreColor = null)
		{
			buttons.Add(new DialogButton(key, text, tooltip, backColor, foreColor));
			return this;
		}

		public PDialog AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			OnRealize += onRealize;
			return this;
		}

		public GameObject Build()
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Expected O, but got Unknown
			if ((Object)(object)Parent == (Object)null)
			{
				throw new InvalidOperationException("Parent for dialog may not be null");
			}
			GameObject val = PUIElements.CreateUI(Parent, Name);
			PDialogComp pDialogComp = val.AddComponent<PDialogComp>();
			val.AddComponent<Canvas>();
			val.AddComponent<GraphicRaycaster>();
			Image obj = val.AddComponent<Image>();
			((Graphic)obj).color = DialogBackColor;
			obj.sprite = PUITuning.Images.BoxBorder;
			obj.type = (Type)1;
			PGridLayoutGroup pGridLayoutGroup = val.AddComponent<PGridLayoutGroup>();
			pGridLayoutGroup.AddRow(new GridRowSpec());
			pGridLayoutGroup.AddRow(new GridRowSpec(0f, 1f));
			pGridLayoutGroup.AddRow(new GridRowSpec());
			pGridLayoutGroup.AddColumn(new GridColumnSpec(0f, 1f));
			pGridLayoutGroup.AddColumn(new GridColumnSpec());
			LayoutTitle(pGridLayoutGroup, pDialogComp.DoButton);
			pGridLayoutGroup.AddComponent(Body.Build(), new GridComponentSpec(1, 0)
			{
				ColumnSpan = 2,
				Margin = new RectOffset(10, 10, 10, 10)
			});
			CreateUserButtons(pGridLayoutGroup, pDialogComp.DoButton);
			SetDialogSize(val);
			pDialogComp.dialog = this;
			pDialogComp.sortKey = SortKey;
			this.OnRealize?.Invoke(val);
			return val;
		}

		private void CreateUserButtons(PGridLayoutGroup layout, PUIDelegates.OnButtonPressed onPressed)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected O, but got Unknown
			PPanel pPanel = new PPanel("Buttons")
			{
				Alignment = (TextAnchor)7,
				Spacing = 7,
				Direction = PanelDirection.Horizontal,
				Margin = new RectOffset(5, 5, 0, 10)
			};
			int num = 0;
			foreach (DialogButton button in buttons)
			{
				string key = button.key;
				ColorStyleSetting backColor = button.backColor;
				TextStyleSetting textStyle = button.textColor ?? PUITuning.Fonts.UILightStyle;
				PButton pButton = new PButton(key)
				{
					Text = button.text,
					ToolTip = button.tooltip,
					Margin = BUTTON_MARGIN,
					OnClick = onPressed,
					Color = backColor,
					TextStyle = textStyle
				};
				if ((Object)(object)backColor == (Object)null)
				{
					if (++num >= buttons.Count)
					{
						pButton.SetKleiPinkStyle();
					}
					else
					{
						pButton.SetKleiBlueStyle();
					}
				}
				pPanel.AddChild(pButton);
			}
			layout.AddComponent(pPanel.Build(), new GridComponentSpec(2, 0)
			{
				ColumnSpan = 2
			});
		}

		private void LayoutTitle(PGridLayoutGroup layout, PUIDelegates.OnButtonPressed onClose)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Expected O, but got Unknown
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new PLabel("Title")
			{
				Margin = new RectOffset(3, 4, 0, 0),
				Text = Title,
				FlexSize = Vector2.one
			}.SetKleiPinkColor().Build();
			layout.AddComponent(val, new GridComponentSpec(0, 0)
			{
				Margin = new RectOffset(0, -2, 0, 0)
			});
			Image obj = EntityTemplateExtensions.AddOrGet<Image>(val);
			obj.sprite = PUITuning.Images.BoxBorder;
			obj.type = (Type)1;
			layout.AddComponent(new PButton("close")
			{
				Sprite = PUITuning.Images.Close,
				Margin = CLOSE_ICON_MARGIN,
				OnClick = onClose,
				SpriteSize = CLOSE_ICON_SIZE,
				ToolTip = LocString.op_Implicit(TOOLTIPS.CLOSETOOLTIP)
			}.SetKleiBlueStyle().Build(), new GridComponentSpec(0, 1));
		}

		private void SetDialogSize(GameObject dialog)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			RectTransform val = Util.rectTransform(dialog);
			LayoutRebuilder.ForceRebuildLayoutImmediate(val);
			float num = Math.Max(Size.x, LayoutUtility.GetPreferredWidth(val));
			float num2 = Math.Max(Size.y, LayoutUtility.GetPreferredHeight(val));
			float x = MaxSize.x;
			float y = MaxSize.y;
			if (x > 0f)
			{
				num = Math.Min(num, x);
			}
			if (y > 0f)
			{
				num2 = Math.Min(num2, y);
			}
			if (RoundToNearestEven)
			{
				num = RoundUpSize(num, x);
				num2 = RoundUpSize(num2, y);
			}
			val.SetSizeWithCurrentAnchors((Axis)0, num);
			val.SetSizeWithCurrentAnchors((Axis)1, num2);
		}

		public void Show()
		{
			KScreen obj = default(KScreen);
			if (Build().TryGetComponent<KScreen>(ref obj))
			{
				UIDetours.ACTIVATE_KSCREEN.Invoke(obj);
			}
		}

		public override string ToString()
		{
			return $"PDialog[Name={Name},Title={Title}]";
		}
	}
	public class PGridPanel : PContainer, IDynamicSizable, IUIComponent
	{
		private readonly ICollection<GridComponent<IUIComponent>> children;

		private readonly IList<GridColumnSpec> columns;

		private readonly IList<GridRowSpec> rows;

		public int Columns => columns.Count;

		public bool DynamicSize { get; set; }

		public int Rows => rows.Count;

		public PGridPanel()
			: this(null)
		{
		}

		public PGridPanel(string name)
			: base(name ?? "GridPanel")
		{
			children = new List<GridComponent<IUIComponent>>(16);
			columns = new List<GridColumnSpec>(16);
			rows = new List<GridRowSpec>(16);
			DynamicSize = true;
			base.Margin = null;
		}

		public PGridPanel AddChild(IUIComponent child, GridComponentSpec spec)
		{
			if (child == null)
			{
				throw new ArgumentNullException("child");
			}
			if (spec == null)
			{
				throw new ArgumentNullException("spec");
			}
			children.Add(new GridComponent<IUIComponent>(spec, child));
			return this;
		}

		public PGridPanel AddColumn(GridColumnSpec column)
		{
			if (column == null)
			{
				throw new ArgumentNullException("column");
			}
			columns.Add(column);
			return this;
		}

		public PGridPanel AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			base.OnRealize += onRealize;
			return this;
		}

		public PGridPanel AddRow(GridRowSpec row)
		{
			if (row == null)
			{
				throw new ArgumentNullException("row");
			}
			rows.Add(row);
			return this;
		}

		public override GameObject Build()
		{
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			if (Columns < 1)
			{
				throw new InvalidOperationException("At least one column must be defined");
			}
			if (Rows < 1)
			{
				throw new InvalidOperationException("At least one row must be defined");
			}
			GameObject val = PUIElements.CreateUI(null, base.Name);
			SetImage(val);
			PGridLayoutGroup pGridLayoutGroup = val.AddComponent<PGridLayoutGroup>();
			pGridLayoutGroup.Margin = base.Margin;
			foreach (GridColumnSpec column in columns)
			{
				pGridLayoutGroup.AddColumn(column);
			}
			foreach (GridRowSpec row in rows)
			{
				pGridLayoutGroup.AddRow(row);
			}
			foreach (GridComponent<IUIComponent> child in children)
			{
				pGridLayoutGroup.AddComponent(child.Item.Build(), child);
			}
			if (!DynamicSize)
			{
				pGridLayoutGroup.LockLayout();
			}
			pGridLayoutGroup.flexibleWidth = base.FlexSize.x;
			pGridLayoutGroup.flexibleHeight = base.FlexSize.y;
			InvokeRealize(val);
			return val;
		}

		public override string ToString()
		{
			return $"PGridPanel[Name={base.Name},Rows={Rows:D},Columns={Columns:D}]";
		}
	}
	public class PLabel : PTextComponent
	{
		public Color BackColor { get; set; }

		public PLabel()
			: this(null)
		{
		}

		public PLabel(string name)
			: base(name ?? "Label")
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			BackColor = PUITuning.Colors.Transparent;
		}

		public PLabel AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			base.OnRealize += onRealize;
			return this;
		}

		public override GameObject Build()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = PUIElements.CreateUI(null, base.Name);
			GameObject sprite = null;
			GameObject text = null;
			if (BackColor.a > 0f)
			{
				((Graphic)val.AddComponent<Image>()).color = BackColor;
			}
			if ((Object)(object)base.Sprite != (Object)null)
			{
				sprite = ((Component)PTextComponent.ImageChildHelper(val, this)).gameObject;
			}
			if (!string.IsNullOrEmpty(base.Text))
			{
				text = ((Component)PTextComponent.TextChildHelper(val, base.TextStyle ?? PUITuning.Fonts.UILightStyle, base.Text)).gameObject;
			}
			PUIElements.SetToolTip(val, base.ToolTip).SetActive(true);
			RelativeLayoutGroup relativeLayoutGroup = val.AddComponent<RelativeLayoutGroup>();
			relativeLayoutGroup.Margin = base.Margin;
			PTextComponent.ArrangeComponent(relativeLayoutGroup, WrapTextAndSprite(text, sprite), base.TextAlignment);
			if (!base.DynamicSize)
			{
				relativeLayoutGroup.LockLayout();
			}
			relativeLayoutGroup.flexibleWidth = base.FlexSize.x;
			relativeLayoutGroup.flexibleHeight = base.FlexSize.y;
			DestroyLayoutIfPossible(val);
			InvokeRealize(val);
			return val;
		}

		public PLabel SetKleiBlueColor()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
			return this;
		}

		public PLabel SetKleiPinkColor()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
			return this;
		}
	}
	public class PPanel : PContainer, IDynamicSizable, IUIComponent
	{
		protected readonly ICollection<IUIComponent> children;

		public TextAnchor Alignment { get; set; }

		public PanelDirection Direction { get; set; }

		public bool DynamicSize { get; set; }

		public int Spacing { get; set; }

		public PPanel()
			: this(null)
		{
		}

		public PPanel(string name)
			: base(name ?? "Panel")
		{
			Alignment = (TextAnchor)4;
			children = new List<IUIComponent>();
			Direction = PanelDirection.Vertical;
			DynamicSize = true;
			Spacing = 0;
		}

		public PPanel AddChild(IUIComponent child)
		{
			if (child == null)
			{
				throw new ArgumentNullException("child");
			}
			children.Add(child);
			return this;
		}

		public PPanel AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			base.OnRealize += onRealize;
			return this;
		}

		public override GameObject Build()
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			return Build(default(Vector2), DynamicSize);
		}

		private GameObject Build(Vector2 size, bool dynamic)
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = PUIElements.CreateUI(null, base.Name);
			SetImage(val);
			foreach (IUIComponent child in children)
			{
				GameObject obj = child.Build();
				obj.SetParent(val);
				PUIElements.SetAnchors(obj, PUIAnchoring.Stretch, PUIAnchoring.Stretch);
			}
			BoxLayoutGroup boxLayoutGroup = val.AddComponent<BoxLayoutGroup>();
			boxLayoutGroup.Params = new BoxLayoutParams
			{
				Direction = Direction,
				Alignment = Alignment,
				Spacing = Spacing,
				Margin = base.Margin
			};
			if (!dynamic)
			{
				boxLayoutGroup.LockLayout();
				val.SetMinUISize(size);
			}
			boxLayoutGroup.flexibleWidth = base.FlexSize.x;
			boxLayoutGroup.flexibleHeight = base.FlexSize.y;
			InvokeRealize(val);
			return val;
		}

		public GameObject BuildWithFixedSize(Vector2 size)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return Build(size, dynamic: false);
		}

		public PPanel RemoveChild(IUIComponent child)
		{
			if (child == null)
			{
				throw new ArgumentNullException("child");
			}
			children.Remove(child);
			return this;
		}

		public PPanel SetKleiBlueColor()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			base.BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
			return this;
		}

		public PPanel SetKleiPinkColor()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			base.BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
			return this;
		}

		public override string ToString()
		{
			return $"PPanel[Name={base.Name},Direction={Direction}]";
		}
	}
	public enum PanelDirection
	{
		Horizontal,
		Vertical
	}
	public class PRelativePanel : PContainer, IDynamicSizable, IUIComponent
	{
		private readonly IDictionary<IUIComponent, RelativeLayoutParamsBase<IUIComponent>> constraints;

		public bool DynamicSize { get; set; }

		public PRelativePanel()
			: this(null)
		{
			DynamicSize = true;
		}

		public PRelativePanel(string name)
			: base(name ?? "RelativePanel")
		{
			constraints = new Dictionary<IUIComponent, RelativeLayoutParamsBase<IUIComponent>>(16);
			base.Margin = null;
		}

		public PRelativePanel AddChild(IUIComponent child)
		{
			if (child == null)
			{
				throw new ArgumentNullException("child");
			}
			if (!constraints.ContainsKey(child))
			{
				constraints.Add(child, new RelativeLayoutParamsBase<IUIComponent>());
			}
			return this;
		}

		public PRelativePanel AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			base.OnRealize += onRealize;
			return this;
		}

		public PRelativePanel AnchorXAxis(IUIComponent item, float anchor = 0.5f)
		{
			SetLeftEdge(item, anchor);
			return SetRightEdge(item, anchor);
		}

		public PRelativePanel AnchorYAxis(IUIComponent item, float anchor = 0.5f)
		{
			SetTopEdge(item, anchor);
			return SetBottomEdge(item, anchor);
		}

		public override GameObject Build()
		{
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = PUIElements.CreateUI(null, base.Name);
			PooledDictionary<IUIComponent, GameObject, PRelativePanel> val2 = DictionaryPool<IUIComponent, GameObject, PRelativePanel>.Allocate();
			SetImage(val);
			foreach (KeyValuePair<IUIComponent, RelativeLayoutParamsBase<IUIComponent>> constraint in constraints)
			{
				IUIComponent key = constraint.Key;
				GameObject val3 = key.Build();
				val3.SetParent(val);
				((Dictionary<IUIComponent, GameObject>)(object)val2)[key] = val3;
			}
			RelativeLayoutGroup relativeLayoutGroup = val.AddComponent<RelativeLayoutGroup>();
			relativeLayoutGroup.Margin = base.Margin;
			foreach (KeyValuePair<IUIComponent, RelativeLayoutParamsBase<IUIComponent>> constraint2 in constraints)
			{
				GameObject item = ((Dictionary<IUIComponent, GameObject>)(object)val2)[constraint2.Key];
				RelativeLayoutParamsBase<IUIComponent> value = constraint2.Value;
				RelativeLayoutParams relativeLayoutParams = new RelativeLayoutParams();
				Resolve(relativeLayoutParams.TopEdge, value.TopEdge, (IDictionary<IUIComponent, GameObject>)val2);
				Resolve(relativeLayoutParams.BottomEdge, value.BottomEdge, (IDictionary<IUIComponent, GameObject>)val2);
				Resolve(relativeLayoutParams.LeftEdge, value.LeftEdge, (IDictionary<IUIComponent, GameObject>)val2);
				Resolve(relativeLayoutParams.RightEdge, value.RightEdge, (IDictionary<IUIComponent, GameObject>)val2);
				relativeLayoutParams.OverrideSize = value.OverrideSize;
				relativeLayoutParams.Insets = value.Insets;
				relativeLayoutGroup.SetRaw(item, relativeLayoutParams);
			}
			if (!DynamicSize)
			{
				relativeLayoutGroup.LockLayout();
			}
			val2.Recycle();
			relativeLayoutGroup.flexibleWidth = base.FlexSize.x;
			relativeLayoutGroup.flexibleHeight = base.FlexSize.y;
			InvokeRealize(val);
			return val;
		}

		private RelativeLayoutParamsBase<IUIComponent> GetOrThrow(IUIComponent item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (!constraints.TryGetValue(item, out var value))
			{
				throw new ArgumentException("Components must be added to the panel before using them in a constraint");
			}
			return value;
		}

		public PRelativePanel OverrideSize(IUIComponent item, Vector2 size)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (item != null)
			{
				GetOrThrow(item).OverrideSize = size;
			}
			return this;
		}

		private void Resolve(RelativeLayoutParamsBase<GameObject>.EdgeStatus dest, RelativeLayoutParamsBase<IUIComponent>.EdgeStatus status, IDictionary<IUIComponent, GameObject> mapping)
		{
			IUIComponent fromComponent = status.FromComponent;
			dest.FromAnchor = status.FromAnchor;
			if (fromComponent != null)
			{
				dest.FromComponent = mapping[fromComponent];
			}
			dest.Constraint = status.Constraint;
			dest.Offset = status.Offset;
		}

		public PRelativePanel SetBottomEdge(IUIComponent item, float fraction = -1f, IUIComponent above = null)
		{
			if (item != null)
			{
				if (above == item)
				{
					throw new ArgumentException("Component cannot refer directly to itself");
				}
				SetEdge(GetOrThrow(item).BottomEdge, fraction, above);
			}
			return this;
		}

		private void SetEdge(RelativeLayoutParamsBase<IUIComponent>.EdgeStatus edge, float fraction, IUIComponent child)
		{
			if (fraction >= 0f && fraction <= 1f)
			{
				edge.Constraint = RelativeConstraintType.ToAnchor;
				edge.FromAnchor = fraction;
				edge.FromComponent = null;
			}
			else if (child != null)
			{
				edge.Constraint = RelativeConstraintType.ToComponent;
				edge.FromComponent = child;
			}
			else
			{
				edge.Constraint = RelativeConstraintType.Unconstrained;
				edge.FromComponent = null;
			}
		}

		public PRelativePanel SetLeftEdge(IUIComponent item, float fraction = -1f, IUIComponent toRight = null)
		{
			if (item != null)
			{
				if (toRight == item)
				{
					throw new ArgumentException("Component cannot refer directly to itself");
				}
				SetEdge(GetOrThrow(item).LeftEdge, fraction, toRight);
			}
			return this;
		}

		public PRelativePanel SetMargin(IUIComponent item, RectOffset insets)
		{
			if (item != null)
			{
				GetOrThrow(item).Insets = insets;
			}
			return this;
		}

		public PRelativePanel SetRightEdge(IUIComponent item, float fraction = -1f, IUIComponent toLeft = null)
		{
			if (item != null)
			{
				if (toLeft == item)
				{
					throw new ArgumentException("Component cannot refer directly to itself");
				}
				SetEdge(GetOrThrow(item).RightEdge, fraction, toLeft);
			}
			return this;
		}

		public PRelativePanel SetTopEdge(IUIComponent item, float fraction = -1f, IUIComponent below = null)
		{
			if (item != null)
			{
				if (below == item)
				{
					throw new ArgumentException("Component cannot refer directly to itself");
				}
				SetEdge(GetOrThrow(item).TopEdge, fraction, below);
			}
			return this;
		}

		public override string ToString()
		{
			return $"PRelativePanel[Name={base.Name}]";
		}
	}
	public sealed class PScrollPane : IUIComponent
	{
		private sealed class PScrollPaneLayout : AbstractLayoutGroup
		{
			private Component[] calcElements;

			private LayoutSizes childHorizontal;

			private LayoutSizes childVertical;

			private GameObject child;

			private ILayoutController[] setElements;

			private GameObject viewport;

			internal PScrollPaneLayout()
			{
				base.minHeight = (base.minWidth = 0f);
				child = (viewport = null);
			}

			public override void CalculateLayoutInputHorizontal()
			{
				if ((Object)(object)child == (Object)null)
				{
					UpdateComponents();
				}
				if ((Object)(object)child != (Object)null)
				{
					calcElements = child.GetComponents<Component>();
					childHorizontal = PUIUtils.CalcSizes(child, PanelDirection.Horizontal, calcElements);
					if (childHorizontal.ignore)
					{
						throw new InvalidOperationException("ScrollPane child ignores layout!");
					}
					base.preferredWidth = childHorizontal.preferred;
				}
			}

			public override void CalculateLayoutInputVertical()
			{
				if ((Object)(object)child == (Object)null)
				{
					UpdateComponents();
				}
				if ((Object)(object)child != (Object)null && calcElements != null)
				{
					childVertical = PUIUtils.CalcSizes(child, PanelDirection.Vertical, calcElements);
					base.preferredHeight = childVertical.preferred;
					calcElements = null;
				}
			}

			protected override void OnEnable()
			{
				base.OnEnable();
				UpdateComponents();
			}

			public override void SetLayoutHorizontal()
			{
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)viewport != (Object)null && (Object)(object)child != (Object)null)
				{
					float num = childHorizontal.preferred;
					Rect rect = Util.rectTransform(viewport).rect;
					float width = ((Rect)(ref rect)).width;
					if (num < width && childHorizontal.flexible > 0f)
					{
						num = width;
					}
					setElements = child.GetComponents<ILayoutController>();
					Util.rectTransform(child).SetSizeWithCurrentAnchors((Axis)0, num);
					ILayoutController[] array = setElements;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].SetLayoutHorizontal();
					}
				}
			}

			public override void SetLayoutVertical()
			{
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)viewport != (Object)null && (Object)(object)child != (Object)null && setElements != null)
				{
					float num = childVertical.preferred;
					Rect rect = Util.rectTransform(viewport).rect;
					float height = ((Rect)(ref rect)).height;
					if (num < height && childVertical.flexible > 0f)
					{
						num = height;
					}
					Util.rectTransform(child).SetSizeWithCurrentAnchors((Axis)1, num);
					ILayoutController[] array = setElements;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].SetLayoutVertical();
					}
					setElements = null;
				}
			}

			private void UpdateComponents()
			{
				GameObject gameObject = ((Component)this).gameObject;
				ScrollRect val = default(ScrollRect);
				if ((Object)(object)gameObject != (Object)null && gameObject.TryGetComponent<ScrollRect>(ref val))
				{
					RectTransform content = val.content;
					child = ((content != null) ? ((Component)content).gameObject : null);
					RectTransform obj = val.viewport;
					viewport = ((obj != null) ? ((Component)obj).gameObject : null);
				}
				else
				{
					child = (viewport = null);
				}
			}
		}

		private sealed class ViewportLayoutGroup : UIBehaviour, ILayoutGroup, ILayoutController
		{
			public void SetLayoutHorizontal()
			{
			}

			public void SetLayoutVertical()
			{
			}
		}

		private const float DEFAULT_TRACK_SIZE = 16f;

		public bool AlwaysShowHorizontal { get; set; }

		public bool AlwaysShowVertical { get; set; }

		public Color BackColor { get; set; }

		public IUIComponent Child { get; set; }

		public Vector2 FlexSize { get; set; }

		public string Name { get; }

		public bool ScrollHorizontal { get; set; }

		public bool ScrollVertical { get; set; }

		public float TrackSize { get; set; }

		public event PUIDelegates.OnRealize OnRealize;

		public PScrollPane()
			: this(null)
		{
		}

		public PScrollPane(string name)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			AlwaysShowHorizontal = (AlwaysShowVertical = true);
			BackColor = PUITuning.Colors.Transparent;
			Child = null;
			FlexSize = Vector2.zero;
			Name = name ?? "Scroll";
			ScrollHorizontal = false;
			ScrollVertical = false;
			TrackSize = 16f;
		}

		public PScrollPane AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			OnRealize += onRealize;
			return this;
		}

		public GameObject Build()
		{
			if (Child == null)
			{
				throw new InvalidOperationException("No child component");
			}
			GameObject val = BuildScrollPane(null, Child.Build());
			this.OnRealize?.Invoke(val);
			return val;
		}

		internal GameObject BuildScrollPane(GameObject parent, GameObject child)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = PUIElements.CreateUI(parent, Name);
			if (BackColor.a > 0f)
			{
				((Graphic)val.AddComponent<Image>()).color = BackColor;
			}
			val.SetActive(false);
			KScrollRect val2 = val.AddComponent<KScrollRect>();
			((ScrollRect)val2).horizontal = ScrollHorizontal;
			((ScrollRect)val2).vertical = ScrollVertical;
			GameObject val3 = PUIElements.CreateUI(val, "Viewport");
			Util.rectTransform(val3).pivot = Vector2.up;
			((Behaviour)val3.AddComponent<RectMask2D>()).enabled = true;
			val3.AddComponent<ViewportLayoutGroup>();
			((ScrollRect)val2).viewport = Util.rectTransform(val3);
			EntityTemplateExtensions.AddOrGet<Canvas>(child).pixelPerfect = false;
			EntityTemplateExtensions.AddOrGet<GraphicRaycaster>(child);
			PUIElements.SetAnchors(child.SetParent(val3), PUIAnchoring.Beginning, PUIAnchoring.End);
			((ScrollRect)val2).content = Util.rectTransform(child);
			if (ScrollVertical)
			{
				((ScrollRect)val2).verticalScrollbar = CreateScrollVert(val);
				((ScrollRect)val2).verticalScrollbarVisibility = (ScrollbarVisibility)((!AlwaysShowVertical) ? 2 : 0);
			}
			if (ScrollHorizontal)
			{
				((ScrollRect)val2).horizontalScrollbar = CreateScrollHoriz(val);
				((ScrollRect)val2).horizontalScrollbarVisibility = (ScrollbarVisibility)((!AlwaysShowHorizontal) ? 2 : 0);
			}
			val.SetActive(true);
			PScrollPaneLayout pScrollPaneLayout = val.AddComponent<PScrollPaneLayout>();
			pScrollPaneLayout.flexibleHeight = FlexSize.y;
			pScrollPaneLayout.flexibleWidth = FlexSize.x;
			return val;
		}

		private Scrollbar CreateScrollHoriz(GameObject parent)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = PUIElements.CreateUI(parent, "Scrollbar H", canvas: true, PUIAnchoring.Stretch, PUIAnchoring.Beginning);
			Image obj = val.AddComponent<Image>();
			obj.sprite = PUITuning.Images.ScrollBorderHorizontal;
			obj.type = (Type)1;
			Scrollbar obj2 = val.AddComponent<Scrollbar>();
			((Selectable)obj2).interactable = true;
			((Selectable)obj2).transition = (Transition)1;
			((Selectable)obj2).colors = PUITuning.Colors.ScrollbarColors;
			obj2.SetDirection((Direction)1, true);
			GameObject val2 = PUIElements.CreateUI(val, "Handle", canvas: true, PUIAnchoring.Stretch, PUIAnchoring.End);
			PUIElements.SetAnchorOffsets(val2, 1f, 1f, 1f, 1f);
			obj2.handleRect = Util.rectTransform(val2);
			Image val3 = val2.AddComponent<Image>();
			val3.sprite = PUITuning.Images.ScrollHandleHorizontal;
			val3.type = (Type)1;
			((Selectable)obj2).targetGraphic = (Graphic)(object)val3;
			val.SetActive(true);
			PUIElements.SetAnchorOffsets(val, 2f, 2f, 0f - TrackSize, 0f);
			return obj2;
		}

		private Scrollbar CreateScrollVert(GameObject parent)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = PUIElements.CreateUI(parent, "Scrollbar V", canvas: true, PUIAnchoring.End);
			Image obj = val.AddComponent<Image>();
			obj.sprite = PUITuning.Images.ScrollBorderVertical;
			obj.type = (Type)1;
			Scrollbar obj2 = val.AddComponent<Scrollbar>();
			((Selectable)obj2).interactable = true;
			((Selectable)obj2).transition = (Transition)1;
			((Selectable)obj2).colors = PUITuning.Colors.ScrollbarColors;
			obj2.SetDirection((Direction)2, true);
			GameObject val2 = PUIElements.CreateUI(val, "Handle", canvas: true, PUIAnchoring.Stretch, PUIAnchoring.Beginning);
			PUIElements.SetAnchorOffsets(val2, 1f, 1f, 1f, 1f);
			obj2.handleRect = Util.rectTransform(val2);
			Image val3 = val2.AddComponent<Image>();
			val3.sprite = PUITuning.Images.ScrollHandleVertical;
			val3.type = (Type)1;
			((Selectable)obj2).targetGraphic = (Graphic)(object)val3;
			val.SetActive(true);
			PUIElements.SetAnchorOffsets(val, 0f - TrackSize, 0f, 2f, 2f);
			return obj2;
		}

		public PScrollPane SetKleiBlueColor()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
			return this;
		}

		public PScrollPane SetKleiPinkColor()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
			return this;
		}

		public override string ToString()
		{
			return $"PScrollPane[Name={Name},Child={Child}]";
		}
	}
	public class PSliderSingle : IUIComponent
	{
		public bool CustomTrack { get; set; }

		public Direction Direction { get; set; }

		public Vector2 FlexSize { get; set; }

		public ColorStyleSetting HandleColor { get; set; }

		public float HandleSize { get; set; }

		public float Increment { get; set; }

		public float InitialValue { get; set; }

		public bool IntegersOnly { get; set; }

		public float MaxValue { get; set; }

		public float MinValue { get; set; }

		public string Name { get; }

		public PUIDelegates.OnSliderDrag OnDrag { get; set; }

		public float PreferredLength { get; set; }

		public PUIDelegates.OnSliderChanged OnValueChanged { get; set; }

		public string ToolTip { get; set; }

		public float TrackSize { get; set; }

		public event PUIDelegates.OnRealize OnRealize;

		public static void SetCurrentValue(GameObject realized, float value)
		{
			KSlider val = default(KSlider);
			if ((Object)(object)realized != (Object)null && realized.TryGetComponent<KSlider>(ref val) && !value.IsNaNOrInfinity())
			{
				((Slider)val).value = value.InRange(((Slider)val).minValue, ((Slider)val).maxValue);
			}
		}

		public PSliderSingle()
			: this("SliderSingle")
		{
		}

		public PSliderSingle(string name)
		{
			CustomTrack = false;
			Direction = (Direction)0;
			HandleColor = PUITuning.Colors.ButtonPinkStyle;
			HandleSize = 16f;
			Increment = 0f;
			InitialValue = 0.5f;
			IntegersOnly = false;
			MaxValue = 1f;
			MinValue = 0f;
			Name = name;
			PreferredLength = 100f;
			TrackSize = 12f;
		}

		public PSliderSingle AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			OnRealize += onRealize;
			return this;
		}

		public GameObject Build()
		{
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Invalid comparison between Unknown and I4
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Invalid comparison between Unknown and I4
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
			PUIDelegates.OnSliderChanged onChange = OnValueChanged;
			PUIDelegates.OnSliderDrag onDrag = OnDrag;
			float inc = (IntegersOnly ? 0f : Increment);
			if (MaxValue.IsNaNOrInfinity())
			{
				throw new ArgumentException("MaxValue");
			}
			if (MinValue.IsNaNOrInfinity())
			{
				throw new ArgumentException("MinValue");
			}
			if (MaxValue <= MinValue)
			{
				throw new ArgumentOutOfRangeException("MaxValue");
			}
			GameObject slider = PUIElements.CreateUI(null, Name);
			bool flag = (int)Direction == 2 || (int)Direction == 3;
			ColorStyleSetting val = HandleColor ?? PUITuning.Colors.ButtonBlueStyle;
			slider.SetActive(false);
			if (!CustomTrack)
			{
				Image obj = slider.AddComponent<Image>();
				obj.sprite = (flag ? PUITuning.Images.ScrollBorderVertical : PUITuning.Images.ScrollBorderHorizontal);
				obj.type = (Type)1;
			}
			GameObject val2 = PUIElements.CreateUI(slider, "Fill");
			if (!CustomTrack)
			{
				Image obj2 = val2.AddComponent<Image>();
				obj2.sprite = (flag ? PUITuning.Images.ScrollHandleVertical : PUITuning.Images.ScrollHandleHorizontal);
				((Graphic)obj2).color = val.inactiveColor;
				obj2.type = (Type)1;
			}
			PUIElements.SetAnchorOffsets(val2, 1f, 1f, 1f, 1f);
			KSlider ks = slider.AddComponent<KSlider>();
			((Slider)ks).maxValue = MaxValue;
			((Slider)ks).minValue = MinValue;
			((Slider)ks).value = (InitialValue.IsNaNOrInfinity() ? MinValue : InitialValue.InRange(MinValue, MaxValue));
			((Slider)ks).wholeNumbers = IntegersOnly;
			((Slider)ks).handleRect = Util.rectTransform(CreateHandle(slider));
			((Slider)ks).fillRect = Util.rectTransform(val2);
			((Slider)ks).SetDirection(Direction, true);
			((UnityEvent<float>)(object)((Slider)ks).onValueChanged).AddListener((UnityAction<float>)delegate(float value)
			{
				if (inc > 0f && !float.IsInfinity(inc))
				{
					float num = value.RoundTo(inc).InRange(((Slider)ks).minValue, ((Slider)ks).maxValue);
					if (!Mathf.Approximately(num, value))
					{
						value = num;
						((Slider)ks).SetValueWithoutNotify(num);
					}
				}
				onChange?.Invoke(slider, value);
			});
			if (onDrag != null)
			{
				ks.onDrag += delegate
				{
					onDrag(slider, ((Slider)ks).value);
				};
			}
			string tt = ToolTip;
			if (!string.IsNullOrEmpty(tt))
			{
				ToolTip obj3 = slider.AddComponent<ToolTip>();
				obj3.OnToolTip = () => string.Format(tt, ((Slider)ks).value);
				obj3.refreshWhileHovering = true;
			}
			slider.SetActive(true);
			slider.SetMinUISize(flag ? new Vector2(TrackSize, PreferredLength) : new Vector2(PreferredLength, TrackSize));
			slider.SetFlexUISize(FlexSize);
			this.OnRealize?.Invoke(slider);
			return slider;
		}

		private GameObject CreateHandle(GameObject slider)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected I4, but got Unknown
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = PUIElements.CreateUI(slider, "Handle", canvas: true, PUIAnchoring.Center, PUIAnchoring.Center);
			Image obj = val.AddComponent<Image>();
			obj.sprite = PUITuning.Images.SliderHandle;
			obj.preserveAspect = true;
			val.SetUISize(new Vector2(HandleSize, HandleSize));
			float num = 0f;
			Direction direction = Direction;
			switch (direction - 1)
			{
			case 2:
				num = 90f;
				break;
			case 0:
				num = 180f;
				break;
			case 1:
				num = 270f;
				break;
			}
			if (num != 0f)
			{
				val.transform.Rotate(new Vector3(0f, 0f, num));
			}
			return val;
		}

		public PSliderSingle SetKleiPinkStyle()
		{
			HandleColor = PUITuning.Colors.ButtonPinkStyle;
			return this;
		}

		public PSliderSingle SetKleiBlueStyle()
		{
			HandleColor = PUITuning.Colors.ButtonBlueStyle;
			return this;
		}

		public override string ToString()
		{
			return $"PSliderSingle[Name={Name}]";
		}
	}
	public class PSpacer : IUIComponent
	{
		public Vector2 FlexSize { get; set; }

		public Vector2 PreferredSize { get; set; }

		public string Name { get; }

		public event PUIDelegates.OnRealize OnRealize;

		public PSpacer()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			Name = "Spacer";
			FlexSize = Vector2.one;
			PreferredSize = Vector2.zero;
		}

		public GameObject Build()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject(Name);
			LayoutElement obj = val.AddComponent<LayoutElement>();
			obj.flexibleHeight = FlexSize.y;
			obj.flexibleWidth = FlexSize.x;
			obj.minHeight = 0f;
			obj.minWidth = 0f;
			obj.preferredHeight = PreferredSize.y;
			obj.preferredWidth = PreferredSize.x;
			this.OnRealize?.Invoke(val);
			return val;
		}

		public override string ToString()
		{
			return "PSpacer";
		}
	}
	public sealed class PTextArea : IUIComponent
	{
		public Color BackColor { get; set; }

		public Vector2 FlexSize { get; set; }

		public int LineCount { get; set; }

		public int MaxLength { get; set; }

		public string Name { get; }

		public int MinWidth { get; set; }

		public TextAlignmentOptions TextAlignment { get; set; }

		public string Text { get; set; }

		public TextStyleSetting TextStyle { get; set; }

		public string ToolTip { get; set; }

		public PUIDelegates.OnTextChanged OnTextChanged { get; set; }

		public OnValidateInput OnValidate { get; set; }

		public event PUIDelegates.OnRealize OnRealize;

		public PTextArea()
			: this(null)
		{
		}

		public PTextArea(string name)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			BackColor = PUITuning.Colors.BackgroundLight;
			FlexSize = Vector2.one;
			LineCount = 4;
			MaxLength = 1024;
			MinWidth = 64;
			Name = name ?? "TextArea";
			Text = null;
			TextAlignment = (TextAlignmentOptions)257;
			TextStyle = PUITuning.Fonts.TextDarkStyle;
			ToolTip = "";
		}

		public PTextArea AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			OnRealize += onRealize;
			return this;
		}

		public GameObject Build()
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Expected O, but got Unknown
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = PUIElements.CreateUI(null, Name);
			TextStyleSetting val2 = TextStyle ?? PUITuning.Fonts.TextLightStyle;
			Image obj = val.AddComponent<Image>();
			obj.sprite = PUITuning.Images.BoxBorderWhite;
			obj.type = (Type)1;
			((Graphic)obj).color = val2.textColor;
			GameObject val3 = PUIElements.CreateUI(val, "Text Area", canvas: false);
			((Graphic)val3.AddComponent<Image>()).color = BackColor;
			RectMask2D obj2 = val3.AddComponent<RectMask2D>();
			GameObject val4 = PUIElements.CreateUI(val3, "Text");
			TextMeshProUGUI val5 = PTextField.ConfigureField(val4.AddComponent<TextMeshProUGUI>(), val2, TextAlignment);
			((TMP_Text)val5).enableWordWrapping = true;
			((Graphic)val5).raycastTarget = true;
			val.SetActive(false);
			TMP_InputField val6 = val.AddComponent<TMP_InputField>();
			val6.textComponent = (TMP_Text)(object)val5;
			val6.textViewport = Util.rectTransform(val3);
			val6.text = Text ?? "";
			((TMP_Text)val5).text = Text ?? "";
			ConfigureTextEntry(val6);
			PTextFieldEvents pTextFieldEvents = val.AddComponent<PTextFieldEvents>();
			pTextFieldEvents.OnTextChanged = OnTextChanged;
			pTextFieldEvents.OnValidate = OnValidate;
			pTextFieldEvents.TextObject = val4;
			PUIElements.SetToolTip(val, ToolTip);
			((Behaviour)obj2).enabled = true;
			PUIElements.SetAnchorOffsets(val4, new RectOffset());
			val.SetActive(true);
			LayoutElement obj3 = PUIUtils.InsetChild(val, val3, Vector2.one, new Vector2((float)MinWidth, (float)Math.Max(LineCount, 1) * PUIUtils.GetLineHeight(val2)));
			obj3.flexibleWidth = FlexSize.x;
			obj3.flexibleHeight = FlexSize.y;
			obj3.layoutPriority = 2;
			this.OnRealize?.Invoke(val);
			return val;
		}

		private void ConfigureTextEntry(TMP_InputField textEntry)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			textEntry.characterLimit = Math.Max(1, MaxLength);
			((Behaviour)textEntry).enabled = true;
			textEntry.inputType = (InputType)0;
			((Selectable)textEntry).interactable = true;
			textEntry.isRichTextEditingAllowed = false;
			textEntry.keyboardType = (TouchScreenKeyboardType)0;
			textEntry.lineType = (LineType)2;
			((Selectable)textEntry).navigation = Navigation.defaultNavigation;
			textEntry.richText = false;
			textEntry.selectionColor = PUITuning.Colors.SelectionBackground;
			((Selectable)textEntry).transition = (Transition)0;
			textEntry.restoreOriginalTextOnEscape = true;
		}

		public PTextArea SetKleiPinkStyle()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			TextStyle = PUITuning.Fonts.UILightStyle;
			BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
			return this;
		}

		public PTextArea SetKleiBlueStyle()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			TextStyle = PUITuning.Fonts.UILightStyle;
			BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
			return this;
		}

		public PTextArea SetMinWidthInCharacters(int chars)
		{
			int num = Mathf.RoundToInt((float)chars * PUIUtils.GetEmWidth(TextStyle));
			if (num > 0)
			{
				MinWidth = num;
			}
			return this;
		}

		public override string ToString()
		{
			return $"PTextArea[Name={Name}]";
		}
	}
	public abstract class PTextComponent : IDynamicSizable, IUIComponent
	{
		private static readonly Vector2 CENTER = new Vector2(0.5f, 0.5f);

		public bool DynamicSize { get; set; }

		public Vector2 FlexSize { get; set; }

		public int IconSpacing { get; set; }

		public bool MaintainSpriteAspect { get; set; }

		public RectOffset Margin { get; set; }

		public string Name { get; }

		public Sprite Sprite { get; set; }

		public Type SpriteMode { get; set; }

		public TextAnchor SpritePosition { get; set; }

		public Vector2 SpriteSize { get; set; }

		public Color SpriteTint { get; set; }

		public ImageTransform SpriteTransform { get; set; }

		public string Text { get; set; }

		public TextAnchor TextAlignment { get; set; }

		public TextStyleSetting TextStyle { get; set; }

		public string ToolTip { get; set; }

		public event PUIDelegates.OnRealize OnRealize;

		protected static void ArrangeComponent(RelativeLayoutGroup layout, GameObject target, TextAnchor alignment)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected I4, but got Unknown
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Invalid comparison between Unknown and I4
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Invalid comparison between Unknown and I4
			switch ((int)alignment)
			{
			case 0:
			case 3:
			case 6:
				layout.SetLeftEdge(target, 0f);
				break;
			case 2:
			case 5:
			case 8:
				layout.SetRightEdge(target, 1f);
				break;
			default:
				layout.AnchorXAxis(target);
				break;
			}
			if ((int)alignment > 2)
			{
				if (alignment - 6 <= 2)
				{
					layout.SetBottomEdge(target, 0f);
				}
				else
				{
					layout.AnchorYAxis(target);
				}
			}
			else
			{
				layout.SetTopEdge(target, 1f);
			}
		}

		protected static Image ImageChildHelper(GameObject parent, PTextComponent settings)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = PUIElements.CreateUI(parent, "Image", canvas: true, PUIAnchoring.Beginning, PUIAnchoring.Beginning);
			Util.rectTransform(val).pivot = CENTER;
			Image obj = val.AddComponent<Image>();
			((Graphic)obj).color = settings.SpriteTint;
			obj.sprite = settings.Sprite;
			obj.type = settings.SpriteMode;
			obj.preserveAspect = settings.MaintainSpriteAspect;
			Vector3 one = Vector3.one;
			float num = 0f;
			ImageTransform spriteTransform = settings.SpriteTransform;
			if ((spriteTransform & ImageTransform.FlipHorizontal) != ImageTransform.None)
			{
				one.x = -1f;
			}
			if ((spriteTransform & ImageTransform.FlipVertical) != ImageTransform.None)
			{
				one.y = -1f;
			}
			if ((spriteTransform & ImageTransform.Rotate90) != ImageTransform.None)
			{
				num = 90f;
			}
			if ((spriteTransform & ImageTransform.Rotate180) != ImageTransform.None)
			{
				num += 180f;
			}
			RectTransform obj2 = Util.rectTransform(val);
			((Transform)obj2).localScale = one;
			((Transform)obj2).Rotate(new Vector3(0f, 0f, num));
			Vector2 spriteSize = settings.SpriteSize;
			if (spriteSize.x > 0f && spriteSize.y > 0f)
			{
				val.SetUISize(spriteSize, addLayout: true);
			}
			return obj;
		}

		protected static LocText TextChildHelper(GameObject parent, TextStyleSetting style, string contents = "")
		{
			LocText obj = PUIElements.AddLocText(PUIElements.CreateUI(parent, "Text"), style);
			((TMP_Text)obj).alignment = (TextAlignmentOptions)514;
			((TMP_Text)obj).text = contents;
			return obj;
		}

		protected PTextComponent(string name)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			DynamicSize = false;
			FlexSize = Vector2.zero;
			IconSpacing = 0;
			MaintainSpriteAspect = true;
			Margin = null;
			Name = name;
			Sprite = null;
			SpriteMode = (Type)0;
			SpritePosition = (TextAnchor)3;
			SpriteSize = Vector2.zero;
			SpriteTint = Color.white;
			SpriteTransform = ImageTransform.None;
			Text = null;
			TextAlignment = (TextAnchor)4;
			TextStyle = null;
			ToolTip = "";
		}

		public abstract GameObject Build();

		protected void DestroyLayoutIfPossible(GameObject component)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (FlexSize.x == 0f && FlexSize.y == 0f && !DynamicSize)
			{
				AbstractLayoutGroup.DestroyAndReplaceLayout(component);
			}
		}

		protected void InvokeRealize(GameObject obj)
		{
			this.OnRealize?.Invoke(obj);
		}

		public override string ToString()
		{
			return string.Format("{3}[Name={0},Text={1},Sprite={2}]", Name, Text, Sprite, GetType().Name);
		}

		protected GameObject WrapTextAndSprite(GameObject text, GameObject sprite)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Expected I4, but got Unknown
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Expected O, but got Unknown
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Expected O, but got Unknown
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Invalid comparison between Unknown and I4
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Expected O, but got Unknown
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Invalid comparison between Unknown and I4
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Expected O, but got Unknown
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = null;
			if ((Object)(object)text != (Object)null && (Object)(object)sprite != (Object)null)
			{
				val = PUIElements.CreateUI(text.GetParent(), "AlignmentWrapper");
				text.SetParent(val);
				sprite.SetParent(val);
				RelativeLayoutGroup relativeLayoutGroup = EntityTemplateExtensions.AddOrGet<RelativeLayoutGroup>(val);
				TextAnchor spritePosition = SpritePosition;
				switch ((int)spritePosition)
				{
				case 0:
				case 3:
				case 6:
					relativeLayoutGroup.SetLeftEdge(sprite, 0f).SetLeftEdge(text, -1f, sprite).SetMargin(sprite, new RectOffset(0, IconSpacing, 0, 0));
					break;
				case 2:
				case 5:
				case 8:
					relativeLayoutGroup.SetRightEdge(sprite, 1f).SetRightEdge(text, -1f, sprite).SetMargin(sprite, new RectOffset(IconSpacing, 0, 0, 0));
					break;
				default:
					relativeLayoutGroup.AnchorXAxis(text).AnchorXAxis(sprite);
					break;
				}
				spritePosition = SpritePosition;
				if ((int)spritePosition > 2)
				{
					if (spritePosition - 6 <= 2)
					{
						relativeLayoutGroup.SetBottomEdge(sprite, 0f).SetBottomEdge(text, -1f, sprite).SetMargin(sprite, new RectOffset(0, 0, IconSpacing, 0));
					}
					else
					{
						relativeLayoutGroup.AnchorYAxis(text).AnchorYAxis(sprite);
					}
				}
				else
				{
					relativeLayoutGroup.SetTopEdge(sprite, 1f).SetTopEdge(text, -1f, sprite).SetMargin(sprite, new RectOffset(0, 0, 0, IconSpacing));
				}
				if (!DynamicSize)
				{
					relativeLayoutGroup.LockLayout();
				}
			}
			else if ((Object)(object)text != (Object)null)
			{
				val = text;
			}
			else if ((Object)(object)sprite != (Object)null)
			{
				val = sprite;
			}
			return val;
		}
	}
	public sealed class PTextField : IUIComponent
	{
		public enum FieldType
		{
			Text,
			Integer,
			Float
		}

		public Color BackColor { get; set; }

		private ContentType ContentType => (ContentType)(Type switch
		{
			FieldType.Float => 3, 
			FieldType.Integer => 2, 
			_ => 0, 
		});

		public Vector2 FlexSize { get; set; }

		public int MaxLength { get; set; }

		public int MinWidth { get; set; }

		public TextStyleSetting PlaceholderStyle { get; set; }

		public string PlaceholderText { get; set; }

		public string Name { get; }

		public TextAlignmentOptions TextAlignment { get; set; }

		public string Text { get; set; }

		public TextStyleSetting TextStyle { get; set; }

		public string ToolTip { get; set; }

		public FieldType Type { get; set; }

		public PUIDelegates.OnTextChanged OnTextChanged { get; set; }

		public OnValidateInput OnValidate { get; set; }

		public event PUIDelegates.OnRealize OnRealize;

		internal static TextMeshProUGUI ConfigureField(TextMeshProUGUI component, TextStyleSetting style, TextAlignmentOptions alignment)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			((TMP_Text)component).alignment = alignment;
			((TMP_Text)component).autoSizeTextContainer = false;
			((Behaviour)component).enabled = true;
			((Graphic)component).color = style.textColor;
			((TMP_Text)component).font = style.sdfFont;
			((TMP_Text)component).fontSize = style.fontSize;
			((TMP_Text)component).fontStyle = style.style;
			((TMP_Text)component).overflowMode = (TextOverflowModes)0;
			return component;
		}

		public static string GetText(GameObject textField)
		{
			if ((Object)(object)textField == (Object)null)
			{
				throw new ArgumentNullException("textField");
			}
			TMP_InputField val = default(TMP_InputField);
			if (!textField.TryGetComponent<TMP_InputField>(ref val))
			{
				return "";
			}
			return val.text;
		}

		public PTextField()
			: this(null)
		{
		}

		public PTextField(string name)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			BackColor = PUITuning.Colors.BackgroundLight;
			FlexSize = Vector2.zero;
			MaxLength = 256;
			MinWidth = 32;
			Name = name ?? "TextField";
			PlaceholderText = null;
			Text = null;
			TextAlignment = (TextAlignmentOptions)514;
			TextStyle = PUITuning.Fonts.TextDarkStyle;
			PlaceholderStyle = TextStyle;
			ToolTip = "";
			Type = FieldType.Text;
		}

		public PTextField AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			OnRealize += onRealize;
			return this;
		}

		public GameObject Build()
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Expected O, but got Unknown
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = PUIElements.CreateUI(null, Name);
			TextStyleSetting val2 = TextStyle ?? PUITuning.Fonts.TextLightStyle;
			Image obj = val.AddComponent<Image>();
			obj.sprite = PUITuning.Images.BoxBorderWhite;
			obj.type = (Type)1;
			((Graphic)obj).color = val2.textColor;
			GameObject val3 = PUIElements.CreateUI(val, "Text Area", canvas: false);
			((Graphic)val3.AddComponent<Image>()).color = BackColor;
			RectMask2D obj2 = val3.AddComponent<RectMask2D>();
			GameObject val4 = PUIElements.CreateUI(val3, "Text");
			TextMeshProUGUI val5 = ConfigureField(val4.AddComponent<TextMeshProUGUI>(), val2, TextAlignment);
			((TMP_Text)val5).enableWordWrapping = false;
			((TMP_Text)val5).maxVisibleLines = 1;
			((Graphic)val5).raycastTarget = true;
			val.SetActive(false);
			TMP_InputField val6 = val.AddComponent<TMP_InputField>();
			val6.textComponent = (TMP_Text)(object)val5;
			val6.textViewport = Util.rectTransform(val3);
			val6.text = Text ?? "";
			((TMP_Text)val5).text = Text ?? "";
			if (PlaceholderText != null)
			{
				TextMeshProUGUI val7 = ConfigureField(PUIElements.CreateUI(val3, "Placeholder Text").AddComponent<TextMeshProUGUI>(), PlaceholderStyle ?? val2, TextAlignment);
				((TMP_Text)val7).maxVisibleLines = 1;
				((TMP_Text)val7).text = PlaceholderText;
				val6.placeholder = (Graphic)(object)val7;
			}
			ConfigureTextEntry(val6);
			PTextFieldEvents pTextFieldEvents = val.AddComponent<PTextFieldEvents>();
			pTextFieldEvents.OnTextChanged = OnTextChanged;
			pTextFieldEvents.OnValidate = OnValidate;
			pTextFieldEvents.TextObject = val4;
			PUIElements.SetToolTip(val, ToolTip);
			((Behaviour)obj2).enabled = true;
			PUIElements.SetAnchorOffsets(val4, new RectOffset());
			val.SetActive(true);
			RectTransform val8 = Util.rectTransform(val4);
			LayoutRebuilder.ForceRebuildLayoutImmediate(val8);
			LayoutElement obj3 = PUIUtils.InsetChild(val, val3, Vector2.one, new Vector2((float)MinWidth, LayoutUtility.GetPreferredHeight(val8)));
			obj3.flexibleWidth = FlexSize.x;
			obj3.flexibleHeight = FlexSize.y;
			obj3.layoutPriority = 2;
			this.OnRealize?.Invoke(val);
			return val;
		}

		private void ConfigureTextEntry(TMP_InputField textEntry)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			textEntry.characterLimit = Math.Max(1, MaxLength);
			textEntry.contentType = ContentType;
			((Behaviour)textEntry).enabled = true;
			textEntry.inputType = (InputType)0;
			((Selectable)textEntry).interactable = true;
			textEntry.isRichTextEditingAllowed = false;
			textEntry.keyboardType = (TouchScreenKeyboardType)0;
			textEntry.lineType = (LineType)0;
			((Selectable)textEntry).navigation = Navigation.defaultNavigation;
			textEntry.richText = false;
			textEntry.selectionColor = PUITuning.Colors.SelectionBackground;
			((Selectable)textEntry).transition = (Transition)0;
			textEntry.restoreOriginalTextOnEscape = true;
		}

		public PTextField SetKleiPinkStyle()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			TextStyle = PUITuning.Fonts.UILightStyle;
			BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
			return this;
		}

		public PTextField SetKleiBlueStyle()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			TextStyle = PUITuning.Fonts.UILightStyle;
			BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
			return this;
		}

		public PTextField SetMinWidthInCharacters(int chars)
		{
			int num = Mathf.RoundToInt((float)chars * PUIUtils.GetEmWidth(TextStyle));
			if (num > 0)
			{
				MinWidth = num;
			}
			return this;
		}

		public override string ToString()
		{
			return $"PTextField[Name={Name},Type={Type}]";
		}
	}
	internal sealed class PTextFieldEvents : KScreen
	{
		private delegate void DeactivateInputField(TMP_InputField instance);

		private static readonly DeactivateInputField DEACTIVATE_INPUT = typeof(TMP_InputField).Detour<DeactivateInputField>();

		[MyCmpReq]
		private TMP_InputField textEntry;

		private bool editing;

		[field: SerializeField]
		internal PUIDelegates.OnTextChanged OnTextChanged { get; set; }

		[field: SerializeField]
		internal OnValidateInput OnValidate { get; set; }

		[field: SerializeField]
		internal GameObject TextObject { get; set; }

		internal PTextFieldEvents()
		{
			base.activateOnSpawn = true;
			editing = false;
			TextObject = null;
		}

		private IEnumerator DelayEndEdit()
		{
			yield return (object)new WaitForEndOfFrame();
			StopEditing();
		}

		public override float GetSortKey()
		{
			if (!editing)
			{
				return ((KScreen)this).GetSortKey();
			}
			return 99f;
		}

		protected override void OnCleanUp()
		{
			TMP_InputField obj = textEntry;
			obj.onFocus = (Action)Delegate.Remove(obj.onFocus, new Action(OnFocus));
			((UnityEvent<string>)(object)textEntry.onValueChanged).RemoveListener((UnityAction<string>)OnValueChanged);
			((UnityEvent<string>)(object)textEntry.onEndEdit).RemoveListener((UnityAction<string>)OnEndEdit);
			((KScreen)this).OnCleanUp();
		}

		protected override void OnSpawn()
		{
			((KScreen)this).OnSpawn();
			TMP_InputField obj = textEntry;
			obj.onFocus = (Action)Delegate.Combine(obj.onFocus, new Action(OnFocus));
			((UnityEvent<string>)(object)textEntry.onValueChanged).AddListener((UnityAction<string>)OnValueChanged);
			((UnityEvent<string>)(object)textEntry.onEndEdit).AddListener((UnityAction<string>)OnEndEdit);
			if (OnValidate != null)
			{
				textEntry.onValidateInput = OnValidate;
			}
		}

		private void OnEndEdit(string text)
		{
			GameObject gameObject = ((Component)this).gameObject;
			if ((Object)(object)gameObject != (Object)null)
			{
				OnTextChanged?.Invoke(gameObject, text);
				if (gameObject.activeInHierarchy)
				{
					((MonoBehaviour)this).StartCoroutine(DelayEndEdit());
				}
			}
		}

		private void OnFocus()
		{
			editing = true;
			((Selectable)textEntry).Select();
			textEntry.ActivateInputField();
			KScreenManager.Instance.RefreshStack();
		}

		public override void OnKeyDown(KButtonEvent e)
		{
			if (editing)
			{
				((KInputEvent)e).Consumed = true;
			}
			else
			{
				((KScreen)this).OnKeyDown(e);
			}
		}

		public override void OnKeyUp(KButtonEvent e)
		{
			if (editing)
			{
				((KInputEvent)e).Consumed = true;
			}
			else
			{
				((KScreen)this).OnKeyUp(e);
			}
		}

		private void OnValueChanged(string text)
		{
			if ((Object)(object)((Component)this).gameObject != (Object)null && (Object)(object)TextObject != (Object)null)
			{
				RectTransform val = Util.rectTransform(TextObject);
				val.SetSizeWithCurrentAnchors((Axis)1, LayoutUtility.GetPreferredHeight(val));
			}
		}

		private void StopEditing()
		{
			if ((Object)(object)textEntry != (Object)null && ((Component)textEntry).gameObject.activeInHierarchy)
			{
				DEACTIVATE_INPUT(textEntry);
			}
			editing = false;
		}
	}
	public sealed class PToggle : IDynamicSizable, IUIComponent
	{
		internal static readonly RectOffset TOGGLE_MARGIN = new RectOffset(1, 1, 1, 1);

		public Sprite ActiveSprite { get; set; }

		public ColorStyleSetting Color { get; set; }

		public bool DynamicSize { get; set; }

		public Vector2 FlexSize { get; set; }

		public Sprite InactiveSprite { get; set; }

		public bool InitialState { get; set; }

		public RectOffset Margin { get; set; }

		public string Name { get; }

		public PUIDelegates.OnToggleButton OnStateChanged { get; set; }

		public Vector2 Size { get; set; }

		public string ToolTip { get; set; }

		public event PUIDelegates.OnRealize OnRealize;

		public static bool GetToggleState(GameObject realized)
		{
			KToggle arg = default(KToggle);
			if ((Object)(object)realized != (Object)null && realized.TryGetComponent<KToggle>(ref arg))
			{
				return UIDetours.IS_ON.Get(arg);
			}
			return false;
		}

		public static void SetToggleState(GameObject realized, bool on)
		{
			KToggle arg = default(KToggle);
			if ((Object)(object)realized != (Object)null && realized.TryGetComponent<KToggle>(ref arg))
			{
				UIDetours.IS_ON.Set(arg, on);
			}
		}

		public PToggle()
			: this(null)
		{
		}

		public PToggle(string name)
		{
			ActiveSprite = PUITuning.Images.Contract;
			Color = PUITuning.Colors.ComponentDarkStyle;
			InitialState = false;
			Margin = TOGGLE_MARGIN;
			Name = name ?? "Toggle";
			InactiveSprite = PUITuning.Images.Expand;
			ToolTip = "";
		}

		public PToggle AddOnRealize(PUIDelegates.OnRealize onRealize)
		{
			OnRealize += onRealize;
			return this;
		}

		public GameObject Build()
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected O, but got Unknown
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			GameObject toggle = PUIElements.CreateUI(null, Name);
			KToggle val = toggle.AddComponent<KToggle>();
			PUIDelegates.OnToggleButton evt = OnStateChanged;
			if (evt != null)
			{
				val.onValueChanged += delegate(bool on)
				{
					evt?.Invoke(toggle, on);
				};
			}
			UIDetours.ART_EXTENSION.Set(val, new KToggleArtExtensions());
			UIDetours.SOUND_PLAYER_TOGGLE.Set(val, PUITuning.ToggleSounds);
			Image val2 = toggle.AddComponent<Image>();
			((Graphic)val2).color = Color.activeColor;
			val2.sprite = InactiveSprite;
			toggle.SetActive(false);
			ImageToggleState obj = toggle.AddComponent<ImageToggleState>();
			obj.TargetImage = val2;
			obj.useSprites = true;
			obj.InactiveSprite = InactiveSprite;
			obj.ActiveSprite = ActiveSprite;
			obj.startingState = (State)((!InitialState) ? 1 : 2);
			obj.useStartingState = true;
			obj.ActiveColour = Color.activeColor;
			obj.DisabledActiveColour = Color.disabledActiveColor;
			obj.InactiveColour = Color.inactiveColor;
			obj.DisabledColour = Color.disabledColor;
			obj.HoverColour = Color.hoverColor;
			obj.DisabledHoverColor = Color.disabledhoverColor;
			UIDetours.IS_ON.Set(val, InitialState);
			toggle.SetActive(true);
			if (Size.x > 0f && Size.y > 0f)
			{
				toggle.SetUISize(Size, addLayout: true);
			}
			else
			{
				PUIElements.AddSizeFitter(toggle, DynamicSize, (FitMode)2, (FitMode)2);
			}
			PUIElements.SetToolTip(toggle, ToolTip).SetFlexUISize(FlexSize).SetActive(true);
			this.OnRealize?.Invoke(toggle);
			return toggle;
		}

		public override string ToString()
		{
			return $"PToggle[Name={Name}]";
		}
	}
	public sealed class PUIDelegates
	{
		public delegate void OnDialogClosed(string option);

		public delegate void OnButtonPressed(GameObject source);

		public delegate void OnChecked(GameObject source, int state);

		public delegate void OnDropdownChanged<T>(GameObject source, T choice) where T : class, IListableOption;

		public delegate void OnRealize(GameObject realized);

		public delegate void OnSliderChanged(GameObject source, float newValue);

		public delegate void OnSliderDrag(GameObject source, float newValue);

		public delegate void OnTextChanged(GameObject source, string text);

		public delegate void OnToggleButton(GameObject source, bool on);
	}
	public sealed class PUIElements
	{
		internal static LocText AddLocText(GameObject parent, TextStyleSetting setting = null)
		{
			if ((Object)(object)parent == (Object)null)
			{
				throw new ArgumentNullException("parent");
			}
			bool activeSelf = parent.activeSelf;
			parent.SetActive(false);
			LocText val = parent.AddComponent<LocText>();
			UIDetours.LOCTEXT_KEY.Set(val, string.Empty);
			UIDetours.LOCTEXT_STYLE.Set(val, setting ?? PUITuning.Fonts.UIDarkStyle);
			parent.SetActive(activeSelf);
			return val;
		}

		public static GameObject AddSizeFitter(GameObject uiElement, bool dynamic = false, FitMode modeHoriz = (FitMode)2, FitMode modeVert = (FitMode)2)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)uiElement == (Object)null)
			{
				throw new ArgumentNullException("uiElement");
			}
			if (dynamic)
			{
				ContentSizeFitter obj = EntityTemplateExtensions.AddOrGet<ContentSizeFitter>(uiElement);
				obj.horizontalFit = modeHoriz;
				obj.verticalFit = modeVert;
				((Behaviour)obj).enabled = true;
			}
			else
			{
				FitSizeNow(uiElement, modeHoriz, modeVert);
			}
			return uiElement;
		}

		public static GameObject CreateUI(GameObject parent, string name, bool canvas = true, PUIAnchoring horizAnchor = PUIAnchoring.Stretch, PUIAnchoring vertAnchor = PUIAnchoring.Stretch)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject(name);
			if ((Object)(object)parent != (Object)null)
			{
				val.SetParent(parent);
			}
			RectTransform obj = EntityTemplateExtensions.AddOrGet<RectTransform>(val);
			((Transform)obj).localScale = Vector3.one;
			SetAnchors(obj, horizAnchor, vertAnchor);
			if (canvas)
			{
				val.AddComponent<CanvasRenderer>();
			}
			val.layer = LayerMask.NameToLayer("UI");
			return val;
		}

		private static void DoNothing()
		{
		}

		private static void FitSizeNow(GameObject uiElement, FitMode modeHoriz, FitMode modeVert)
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Invalid comparison between Unknown and I4
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Invalid comparison between Unknown and I4
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Invalid comparison between Unknown and I4
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Invalid comparison between Unknown and I4
			float val = 0f;
			float val2 = 0f;
			if ((Object)(object)uiElement == (Object)null)
			{
				throw new ArgumentNullException("uiElement");
			}
			ILayoutElement[] components = uiElement.GetComponents<ILayoutElement>();
			LayoutElement val3 = EntityTemplateExtensions.AddOrGet<LayoutElement>(uiElement);
			RectTransform val4 = EntityTemplateExtensions.AddOrGet<RectTransform>(uiElement);
			ILayoutElement[] array = components;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CalculateLayoutInputHorizontal();
			}
			if ((int)modeHoriz != 0)
			{
				array = components;
				foreach (ILayoutElement val5 in array)
				{
					if ((int)modeHoriz != 1)
					{
						if ((int)modeHoriz == 2)
						{
							val = Math.Max(val, val5.preferredWidth);
						}
					}
					else
					{
						val = Math.Max(val, val5.minWidth);
					}
				}
				val = Math.Max(val, val3.minWidth);
				val4.SetSizeWithCurrentAnchors((Axis)0, val);
				val3.minWidth = val;
				val3.flexibleWidth = 0f;
			}
			array = components;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CalculateLayoutInputVertical();
			}
			if ((int)modeVert == 0)
			{
				return;
			}
			array = components;
			foreach (ILayoutElement val6 in array)
			{
				if ((int)modeVert != 1)
				{
					if ((int)modeVert == 2)
					{
						val2 = Math.Max(val2, val6.preferredHeight);
					}
				}
				else
				{
					val2 = Math.Max(val2, val6.minHeight);
				}
			}
			val2 = Math.Max(val2, val3.minHeight);
			val4.SetSizeWithCurrentAnchors((Axis)1, val2);
			val3.minHeight = val2;
			val3.flexibleHeight = 0f;
		}

		public static RectTransform SetAnchors(RectTransform transform, PUIAnchoring horizAnchor, PUIAnchoring vertAnchor)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			Vector2 anchorMax = default(Vector2);
			Vector2 anchorMin = default(Vector2);
			Vector2 pivot = default(Vector2);
			if ((Object)(object)transform == (Object)null)
			{
				throw new ArgumentNullException("transform");
			}
			switch (horizAnchor)
			{
			case PUIAnchoring.Center:
				anchorMin.x = 0.5f;
				anchorMax.x = 0.5f;
				pivot.x = 0.5f;
				break;
			case PUIAnchoring.End:
				anchorMin.x = 1f;
				anchorMax.x = 1f;
				pivot.x = 1f;
				break;
			case PUIAnchoring.Stretch:
				anchorMin.x = 0f;
				anchorMax.x = 1f;
				pivot.x = 0.5f;
				break;
			default:
				anchorMin.x = 0f;
				anchorMax.x = 0f;
				pivot.x = 0f;
				break;
			}
			switch (vertAnchor)
			{
			case PUIAnchoring.Center:
				anchorMin.y = 0.5f;
				anchorMax.y = 0.5f;
				pivot.y = 0.5f;
				break;
			case PUIAnchoring.End:
				anchorMin.y = 1f;
				anchorMax.y = 1f;
				pivot.y = 1f;
				break;
			case PUIAnchoring.Stretch:
				anchorMin.y = 0f;
				anchorMax.y = 1f;
				pivot.y = 0.5f;
				break;
			default:
				anchorMin.y = 0f;
				anchorMax.y = 0f;
				pivot.y = 0f;
				break;
			}
			transform.anchorMax = anchorMax;
			transform.anchorMin = anchorMin;
			transform.pivot = pivot;
			transform.anchoredPosition = Vector2.zero;
			transform.offsetMax = Vector2.zero;
			transform.offsetMin = Vector2.zero;
			return transform;
		}

		public static GameObject SetAnchors(GameObject uiElement, PUIAnchoring horizAnchor, PUIAnchoring vertAnchor)
		{
			if ((Object)(object)uiElement == (Object)null)
			{
				throw new ArgumentNullException("uiElement");
			}
			SetAnchors(Util.rectTransform(uiElement), horizAnchor, vertAnchor);
			return uiElement;
		}

		public static GameObject SetAnchorOffsets(GameObject uiElement, RectOffset border)
		{
			return SetAnchorOffsets(uiElement, border.left, border.right, border.top, border.bottom);
		}

		public static GameObject SetAnchorOffsets(GameObject uiElement, float left, float right, float top, float bottom)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)uiElement == (Object)null)
			{
				throw new ArgumentNullException("uiElement");
			}
			RectTransform obj = Util.rectTransform(uiElement);
			obj.offsetMin = new Vector2(left, bottom);
			obj.offsetMax = new Vector2(0f - right, 0f - top);
			return uiElement;
		}

		public static GameObject SetText(GameObject uiElement, string text)
		{
			if ((Object)(object)uiElement == (Object)null)
			{
				throw new ArgumentNullException("uiElement");
			}
			TextMeshProUGUI componentInChildren = uiElement.GetComponentInChildren<TextMeshProUGUI>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				((TMP_Text)componentInChildren).SetText(text ?? string.Empty);
			}
			return uiElement;
		}

		public static GameObject SetToolTip(GameObject uiElement, string tooltip)
		{
			if ((Object)(object)uiElement == (Object)null)
			{
				throw new ArgumentNullException("uiElement");
			}
			if (!string.IsNullOrEmpty(tooltip))
			{
				EntityTemplateExtensions.AddOrGet<ToolTip>(uiElement).toolTip = tooltip;
			}
			return uiElement;
		}

		public static ConfirmDialogScreen ShowConfirmDialog(GameObject parent, string message, Action onConfirm, Action onCancel = null, string confirmText = null, string cancelText = null)
		{
			if ((Object)(object)parent == (Object)null)
			{
				parent = PDialog.GetParentObject();
			}
			GameObject val = Util.KInstantiateUI(((Component)ScreenPrefabs.Instance.ConfirmDialogScreen).gameObject, parent, false);
			ConfirmDialogScreen val2 = default(ConfirmDialogScreen);
			if (val.TryGetComponent<ConfirmDialogScreen>(ref val2))
			{
				UIDetours.POPUP_CONFIRM.Invoke(val2, message, onConfirm, onCancel ?? new Action(DoNothing), null, null, null, confirmText, cancelText);
				val.SetActive(true);
				return val2;
			}
			return null;
		}

		public static ConfirmDialogScreen ShowMessageDialog(GameObject parent, string message)
		{
			return ShowConfirmDialog(parent, message, DoNothing);
		}

		private PUIElements()
		{
		}
	}
	public enum PUIAnchoring
	{
		Stretch,
		Beginning,
		Center,
		End
	}
	public static class PUITuning
	{
		public static class Images
		{
			private static readonly IDictionary<string, Sprite> SPRITES;

			public static Sprite Arrow { get; }

			public static Sprite BoxBorder { get; }

			public static Sprite BoxBorderWhite { get; }

			public static Sprite ButtonBorder { get; }

			public static Sprite CheckBorder { get; }

			public static Sprite Checked { get; }

			public static Sprite Close { get; }

			public static Sprite Contract { get; }

			public static Sprite Expand { get; }

			public static Sprite Partial { get; }

			public static Sprite ScrollBorderHorizontal { get; }

			public static Sprite ScrollHandleHorizontal { get; }

			public static Sprite ScrollBorderVertical { get; }

			public static Sprite ScrollHandleVertical { get; }

			public static Sprite SliderHandle { get; }

			static Images()
			{
				SPRITES = new Dictionary<string, Sprite>(512);
				Sprite[] array = Resources.FindObjectsOfTypeAll<Sprite>();
				foreach (Sprite val in array)
				{
					string text = ((val != null) ? ((Object)val).name : null);
					if (!string.IsNullOrEmpty(text) && !SPRITES.ContainsKey(text))
					{
						SPRITES.Add(text, val);
					}
				}
				Arrow = GetSpriteByName("game_speed_play");
				BoxBorder = GetSpriteByName("web_box");
				BoxBorderWhite = GetSpriteByName("web_border");
				ButtonBorder = GetSpriteByName("web_button");
				CheckBorder = GetSpriteByName("overview_jobs_skill_box");
				Checked = GetSpriteByName("overview_jobs_icon_checkmark");
				Close = GetSpriteByName("cancel");
				Contract = GetSpriteByName("iconDown");
				Expand = GetSpriteByName("iconRight");
				Partial = GetSpriteByName("overview_jobs_icon_mixed");
				ScrollBorderHorizontal = GetSpriteByName("build_menu_scrollbar_frame_horizontal");
				ScrollHandleHorizontal = GetSpriteByName("build_menu_scrollbar_inner_horizontal");
				ScrollBorderVertical = GetSpriteByName("build_menu_scrollbar_frame");
				ScrollHandleVertical = GetSpriteByName("build_menu_scrollbar_inner");
				SliderHandle = GetSpriteByName("game_speed_selected_med");
			}

			public static Sprite GetSpriteByName(string name)
			{
				if (!SPRITES.TryGetValue(name, out var value))
				{
					return null;
				}
				return value;
			}
		}

		public static class Colors
		{
			public static Color BackgroundLight { get; }

			public static ColorStyleSetting ButtonPinkStyle { get; }

			public static ColorStyleSetting ButtonBlueStyle { get; }

			public static ColorStyleSetting ComponentDarkStyle { get; }

			public static ColorStyleSetting ComponentLightStyle { get; }

			public static Color DialogBackground { get; }

			public static Color DialogDarkBackground { get; }

			public static Color OptionsBackground { get; }

			public static ColorBlock ScrollbarColors { get; }

			public static Color SelectionBackground { get; }

			public static Color SelectionForeground { get; }

			public static Color Transparent { get; }

			public static Color UITextDark { get; }

			public static Color UITextLight { get; }

			static Colors()
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				//IL_004a: Unknown result type (might be due to invalid IL or missing references)
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0064: Unknown result type (might be due to invalid IL or missing references)
				//IL_0069: Unknown result type (might be due to invalid IL or missing references)
				//IL_0082: Unknown result type (might be due to invalid IL or missing references)
				//IL_0087: Unknown result type (might be due to invalid IL or missing references)
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0099: Unknown result type (might be due to invalid IL or missing references)
				//IL_009e: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00db: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
				//IL_0141: Unknown result type (might be due to invalid IL or missing references)
				//IL_0142: Unknown result type (might be due to invalid IL or missing references)
				//IL_014c: Unknown result type (might be due to invalid IL or missing references)
				//IL_014d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0157: Unknown result type (might be due to invalid IL or missing references)
				//IL_0158: Unknown result type (might be due to invalid IL or missing references)
				//IL_0162: Unknown result type (might be due to invalid IL or missing references)
				//IL_0163: Unknown result type (might be due to invalid IL or missing references)
				//IL_016d: Unknown result type (might be due to invalid IL or missing references)
				//IL_016e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0178: Unknown result type (might be due to invalid IL or missing references)
				//IL_0179: Unknown result type (might be due to invalid IL or missing references)
				//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
				//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
				//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
				//IL_01af: Unknown result type (might be due to invalid IL or missing references)
				//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
				//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
				//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
				//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
				//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
				//IL_01da: Unknown result type (might be due to invalid IL or missing references)
				//IL_01db: Unknown result type (might be due to invalid IL or missing references)
				//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
				//IL_0203: Unknown result type (might be due to invalid IL or missing references)
				//IL_021c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0221: Unknown result type (might be due to invalid IL or missing references)
				//IL_023a: Unknown result type (might be due to invalid IL or missing references)
				//IL_023f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0249: Unknown result type (might be due to invalid IL or missing references)
				//IL_024e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0267: Unknown result type (might be due to invalid IL or missing references)
				//IL_026c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0285: Unknown result type (might be due to invalid IL or missing references)
				//IL_028a: Unknown result type (might be due to invalid IL or missing references)
				//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
				//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
				//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
				//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
				//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
				//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
				//IL_0307: Unknown result type (might be due to invalid IL or missing references)
				//IL_030c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0325: Unknown result type (might be due to invalid IL or missing references)
				//IL_032a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0343: Unknown result type (might be due to invalid IL or missing references)
				//IL_0348: Unknown result type (might be due to invalid IL or missing references)
				//IL_034f: Unknown result type (might be due to invalid IL or missing references)
				//IL_037e: Unknown result type (might be due to invalid IL or missing references)
				//IL_039e: Unknown result type (might be due to invalid IL or missing references)
				//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
				//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
				//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
				//IL_03de: Unknown result type (might be due to invalid IL or missing references)
				//IL_03df: Unknown result type (might be due to invalid IL or missing references)
				BackgroundLight = Color32.op_Implicit(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				DialogBackground = Color32.op_Implicit(new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue));
				DialogDarkBackground = Color32.op_Implicit(new Color32((byte)48, (byte)52, (byte)67, byte.MaxValue));
				OptionsBackground = Color32.op_Implicit(new Color32((byte)31, (byte)34, (byte)43, byte.MaxValue));
				SelectionBackground = Color32.op_Implicit(new Color32((byte)189, (byte)218, byte.MaxValue, byte.MaxValue));
				SelectionForeground = Color32.op_Implicit(new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue));
				Transparent = Color32.op_Implicit(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)0));
				UITextLight = Color32.op_Implicit(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				UITextDark = Color32.op_Implicit(new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue));
				Color val = default(Color);
				((Color)(ref val))..ctor(0f, 0f, 0f);
				Color val2 = default(Color);
				((Color)(ref val2))..ctor(0.784f, 0.784f, 0.784f, 1f);
				ComponentLightStyle = ScriptableObject.CreateInstance<ColorStyleSetting>();
				ComponentLightStyle.activeColor = val;
				ComponentLightStyle.inactiveColor = val;
				ComponentLightStyle.hoverColor = val;
				ComponentLightStyle.disabledActiveColor = val2;
				ComponentLightStyle.disabledColor = val2;
				ComponentLightStyle.disabledhoverColor = val2;
				((Color)(ref val))..ctor(1f, 1f, 1f);
				ComponentDarkStyle = ScriptableObject.CreateInstance<ColorStyleSetting>();
				ComponentDarkStyle.activeColor = val;
				ComponentDarkStyle.inactiveColor = val;
				ComponentDarkStyle.hoverColor = val;
				ComponentDarkStyle.disabledActiveColor = val2;
				ComponentDarkStyle.disabledColor = val2;
				ComponentDarkStyle.disabledhoverColor = val2;
				ButtonPinkStyle = ScriptableObject.CreateInstance<ColorStyleSetting>();
				ButtonPinkStyle.activeColor = new Color(27f / 34f, 0.4496107f, 0.6242238f);
				ButtonPinkStyle.inactiveColor = new Color(0.5294118f, 0.2724914f, 0.4009516f);
				ButtonPinkStyle.disabledColor = new Color(0.4156863f, 0.4117647f, 0.4f);
				ButtonPinkStyle.disabledActiveColor = Transparent;
				ButtonPinkStyle.hoverColor = new Color(0.6176471f, 0.3315311f, 0.4745891f);
				ButtonPinkStyle.disabledhoverColor = new Color(0.5f, 0.5f, 0.5f);
				ButtonBlueStyle = ScriptableObject.CreateInstance<ColorStyleSetting>();
				ButtonBlueStyle.activeColor = new Color(0.5033521f, 0.5444419f, 95f / 136f);
				ButtonBlueStyle.inactiveColor = new Color(0.2431373f, 0.2627451f, 0.3411765f);
				ButtonBlueStyle.disabledColor = new Color(0.4156863f, 0.4117647f, 0.4f);
				ButtonBlueStyle.disabledActiveColor = new Color(0.625f, 0.6158088f, 0.5882353f);
				ButtonBlueStyle.hoverColor = new Color(0.3461289f, 0.3739619f, 33f / 68f);
				ButtonBlueStyle.disabledhoverColor = new Color(0.5f, 0.4898898f, 0.4595588f);
				ColorBlock val3 = default(ColorBlock);
				((ColorBlock)(ref val3)).colorMultiplier = 1f;
				((ColorBlock)(ref val3)).fadeDuration = 0.1f;
				((ColorBlock)(ref val3)).disabledColor = new Color(0.392f, 0.392f, 0.392f);
				((ColorBlock)(ref val3)).highlightedColor = Color32.op_Implicit(new Color32((byte)161, (byte)163, (byte)174, byte.MaxValue));
				((ColorBlock)(ref val3)).normalColor = Color32.op_Implicit(new Color32((byte)161, (byte)163, (byte)174, byte.MaxValue));
				((ColorBlock)(ref val3)).pressedColor = BackgroundLight;
				ScrollbarColors = val3;
			}
		}

		public static class Fonts
		{
			private const string DEFAULT_FONT_TEXT = "NotoSans-Regular";

			private const string DEFAULT_FONT_UI = "GRAYSTROKE REGULAR SDF";

			private static readonly TMP_FontAsset DefaultTextFont;

			private static readonly TMP_FontAsset DefaultUIFont;

			private static readonly IDictionary<string, TMP_FontAsset> FONTS;

			public static int DefaultSize { get; }

			internal static TMP_FontAsset Text
			{
				get
				{
					//IL_0002: Unknown result type (might be due to invalid IL or missing references)
					TMP_FontAsset val = null;
					if ((int)Localization.GetSelectedLanguageType() != 0)
					{
						val = Localization.FontAsset;
					}
					return val ?? DefaultTextFont;
				}
			}

			public static TextStyleSetting TextDarkStyle { get; }

			public static TextStyleSetting TextLightStyle { get; }

			internal static TMP_FontAsset UI
			{
				get
				{
					//IL_0002: Unknown result type (might be due to invalid IL or missing references)
					TMP_FontAsset val = null;
					if ((int)Localization.GetSelectedLanguageType() != 0)
					{
						val = Localization.FontAsset;
					}
					return val ?? DefaultUIFont;
				}
			}

			public static TextStyleSetting UIDarkStyle { get; }

			public static TextStyleSetting UILightStyle { get; }

			static Fonts()
			{
				//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
				//IL_0147: Unknown result type (might be due to invalid IL or missing references)
				//IL_0151: Unknown result type (might be due to invalid IL or missing references)
				//IL_0156: Unknown result type (might be due to invalid IL or missing references)
				//IL_0161: Unknown result type (might be due to invalid IL or missing references)
				FONTS = new Dictionary<string, TMP_FontAsset>(16);
				TMP_FontAsset[] array = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
				foreach (TMP_FontAsset val in array)
				{
					string text = ((val != null) ? ((Object)val).name : null);
					if (!string.IsNullOrEmpty(text) && !FONTS.ContainsKey(text))
					{
						FONTS.Add(text, val);
					}
				}
				if ((Object)(object)(DefaultTextFont = GetFontByName("NotoSans-Regular")) == (Object)null)
				{
					PUIUtils.LogUIWarning("Unable to find font NotoSans-Regular");
				}
				if ((Object)(object)(DefaultUIFont = GetFontByName("GRAYSTROKE REGULAR SDF")) == (Object)null)
				{
					PUIUtils.LogUIWarning("Unable to find font GRAYSTROKE REGULAR SDF");
				}
				DefaultSize = 14;
				TextDarkStyle = ScriptableObject.CreateInstance<TextStyleSetting>();
				TextDarkStyle.enableWordWrapping = false;
				TextDarkStyle.fontSize = DefaultSize;
				TextDarkStyle.sdfFont = Text;
				TextDarkStyle.style = (FontStyles)0;
				TextDarkStyle.textColor = Colors.UITextDark;
				TextLightStyle = TextDarkStyle.DeriveStyle(0, Colors.UITextLight);
				UIDarkStyle = ScriptableObject.CreateInstance<TextStyleSetting>();
				UIDarkStyle.enableWordWrapping = false;
				UIDarkStyle.fontSize = DefaultSize;
				UIDarkStyle.sdfFont = UI;
				UIDarkStyle.style = (FontStyles)0;
				UIDarkStyle.textColor = Colors.UITextDark;
				UILightStyle = UIDarkStyle.DeriveStyle(0, Colors.UITextLight);
			}

			internal static TMP_FontAsset GetFontByName(string name)
			{
				if (!FONTS.TryGetValue(name, out var value))
				{
					return null;
				}
				return value;
			}
		}

		internal static ButtonSoundPlayer ButtonSounds { get; }

		internal static ToggleSoundPlayer ToggleSounds { get; }

		static PUITuning()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			ButtonSounds = new ButtonSoundPlayer
			{
				Enabled = true
			};
			ToggleSounds = new ToggleSoundPlayer();
		}
	}
	public static class PUIUtils
	{
		private static void AddComponentText(StringBuilder result, Component component)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			FieldInfo[] fields = ((object)component).GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			TMP_Text val = (TMP_Text)(object)((component is TMP_Text) ? component : null);
			if (val != null)
			{
				result.AppendFormat(", Text={0}, Color={1}, Font={2}", val.text, ((Graphic)val).color, val.font);
			}
			else
			{
				Image val2 = (Image)(object)((component is Image) ? component : null);
				if (val2 != null)
				{
					result.AppendFormat(", Color={0}", ((Graphic)val2).color);
					if ((Object)(object)val2.sprite != (Object)null)
					{
						result.AppendFormat(", Sprite={0}", val2.sprite);
					}
				}
				else
				{
					HorizontalOrVerticalLayoutGroup val3 = (HorizontalOrVerticalLayoutGroup)(object)((component is HorizontalOrVerticalLayoutGroup) ? component : null);
					if (val3 != null)
					{
						result.AppendFormat(", Child Align={0}, Control W={1}, Control H={2}", ((LayoutGroup)val3).childAlignment, val3.childControlWidth, val3.childControlHeight);
					}
				}
			}
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				object obj = fieldInfo.GetValue(component) ?? "null";
				if (obj is LayerMask val4)
				{
					obj = "Layer #" + ((LayerMask)(ref val4)).value;
				}
				else if (obj is ICollection values)
				{
					obj = "[" + values.Join() + "]";
				}
				result.AppendFormat(", {0}={1}", fieldInfo.Name, obj);
			}
		}

		private static void AddReferenceText(StringBuilder result, HierarchyReferences hr)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			ElementReference[] references = hr.references;
			int num = references.Length;
			result.Append(" HierarchyReferences[");
			for (int i = 0; i < num; i++)
			{
				ElementReference val = references[i];
				Component behaviour = val.behaviour;
				result.Append(val.Name).Append('=').Append(((object)behaviour).GetType().FullName)
					.Append('[')
					.Append(((Object)behaviour).name)
					.Append(']');
				if (i < num - 1)
				{
					result.Append(", ");
				}
			}
			result.AppendLine("]");
		}

		private static void AddTransformText(StringBuilder result, RectTransform rt)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = rt.rect;
			Vector2 size = ((Rect)(ref rect)).size;
			Vector2 anchorMin = rt.anchorMin;
			Vector2 anchorMax = rt.anchorMax;
			Vector2 offsetMin = rt.offsetMin;
			Vector2 offsetMax = rt.offsetMax;
			Vector2 pivot = rt.pivot;
			result.AppendFormat(" Rect[Size=({0:F2},{1:F2}) Min=({2:F2},{3:F2}) ", size.x, size.y, LayoutUtility.GetMinWidth(rt), LayoutUtility.GetMinHeight(rt));
			result.AppendFormat("Preferred=({0:F2},{1:F2}) Flexible=({2:F2},{3:F2}) ", LayoutUtility.GetPreferredWidth(rt), LayoutUtility.GetPreferredHeight(rt), LayoutUtility.GetFlexibleWidth(rt), LayoutUtility.GetFlexibleHeight(rt));
			result.AppendFormat("Pivot=({4:F2},{5:F2}) AnchorMin=({0:F2},{1:F2}) AnchorMax=({2:F2},{3:F2}) ", anchorMin.x, anchorMin.y, anchorMax.x, anchorMax.y, pivot.x, pivot.y);
			result.AppendFormat("OffsetMin=({0:F2},{1:F2}) OffsetMax=({2:F2},{3:F2})]", offsetMin.x, offsetMin.y, offsetMax.x, offsetMax.y).AppendLine();
		}

		public static void AddPinkOverlay(GameObject parent)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			((Graphic)PUIElements.CreateUI(parent, "Overlay").AddComponent<Image>()).color = new Color(1f, 0f, 1f, 0.2f);
		}

		public static void AddSideScreenContent<T>(GameObject uiPrefab = null) where T : SideScreenContent
		{
			AddSideScreenContentWithOrdering<T>(null, insertBefore: true, uiPrefab);
		}

		public static void AddSideScreenContentWithOrdering<T>(string targetClassName, bool insertBefore = true, GameObject uiPrefab = null) where T : SideScreenContent
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Expected O, but got Unknown
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			DetailsScreen instance = DetailsScreen.Instance;
			if ((Object)(object)instance == (Object)null)
			{
				LogUIWarning("DetailsScreen is not yet initialized, try a postfix on DetailsScreen.OnPrefabInit");
				return;
			}
			List<SideScreenRef> list = UIDetours.SIDE_SCREENS.Get(instance);
			GameObject sideScreenContent = GetSideScreenContent(instance);
			string name = typeof(T).Name;
			if ((Object)(object)sideScreenContent != (Object)null && list != null)
			{
				SideScreenRef val = new SideScreenRef();
				GameObject val2 = PUIElements.CreateUI(sideScreenContent, name);
				val2.AddComponent<BoxLayoutGroup>().Params = new BoxLayoutParams
				{
					Direction = PanelDirection.Vertical,
					Alignment = (TextAnchor)1,
					Margin = new RectOffset(1, 1, 0, 1)
				};
				T val3 = val2.AddComponent<T>();
				if ((Object)(object)uiPrefab != (Object)null)
				{
					UIDetours.SS_CONTENT_CONTAINER.Set((SideScreenContent)(object)val3, uiPrefab);
					uiPrefab.transform.SetParent(val2.transform);
				}
				val.name = name;
				UIDetours.SS_OFFSET.Set(val, Vector2.zero);
				UIDetours.SS_PREFAB.Set(val, (SideScreenContent)(object)val3);
				UIDetours.SS_INSTANCE.Set(val, (SideScreenContent)(object)val3);
				InsertSideScreenContent(list, val, targetClassName, insertBefore);
			}
		}

		public static GameObject AddTo(this IUIComponent component, GameObject parent, int index = -2)
		{
			if (component == null)
			{
				throw new ArgumentNullException("component");
			}
			if ((Object)(object)parent == (Object)null)
			{
				throw new ArgumentNullException("parent");
			}
			GameObject val = component.Build();
			val.SetParent(parent);
			if (index == -1)
			{
				val.transform.SetAsLastSibling();
			}
			else if (index >= 0)
			{
				val.transform.SetSiblingIndex(index);
			}
			return val;
		}

		internal static LayoutSizes CalcSizes(GameObject obj, PanelDirection direction, IEnumerable<Component> components)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			float value = 0f;
			float value2 = 0f;
			float value3 = 0f;
			int pri = int.MinValue;
			int pri2 = int.MinValue;
			int pri3 = int.MinValue;
			Vector3 localScale = obj.transform.localScale;
			float num = Math.Abs((direction == PanelDirection.Horizontal) ? localScale.x : localScale.y);
			bool ignore = false;
			foreach (Component component in components)
			{
				Component obj2 = ((component is ILayoutIgnorer) ? component : null);
				if (obj2 != null && ((ILayoutIgnorer)obj2).ignoreLayout)
				{
					ignore = true;
					break;
				}
				Component obj3 = ((component is Behaviour) ? component : null);
				if (obj3 != null && !((Behaviour)obj3).isActiveAndEnabled)
				{
					continue;
				}
				ILayoutElement val = (ILayoutElement)(object)((component is ILayoutElement) ? component : null);
				if (val != null)
				{
					int layoutPriority = val.layoutPriority;
					if (direction == PanelDirection.Horizontal)
					{
						val.CalculateLayoutInputHorizontal();
						PriValue(ref value, val.minWidth, layoutPriority, ref pri);
						PriValue(ref value2, val.preferredWidth, layoutPriority, ref pri2);
						PriValue(ref value3, val.flexibleWidth, layoutPriority, ref pri3);
					}
					else
					{
						val.CalculateLayoutInputVertical();
						PriValue(ref value, val.minHeight, layoutPriority, ref pri);
						PriValue(ref value2, val.preferredHeight, layoutPriority, ref pri2);
						PriValue(ref value3, val.flexibleHeight, layoutPriority, ref pri3);
					}
				}
			}
			LayoutSizes result = new LayoutSizes(obj, value * num, Math.Max(value, value2) * num, value3);
			result.ignore = ignore;
			return result;
		}

		public static void DebugObjectHierarchy(this GameObject item)
		{
			string text = "null";
			if ((Object)(object)item != (Object)null)
			{
				StringBuilder stringBuilder = new StringBuilder(256);
				do
				{
					Transform parent = item.transform.parent;
					stringBuilder.Append("- ");
					stringBuilder.Append(((Object)item).name);
					if ((Object)(object)parent != (Object)null)
					{
						item = ((Component)parent).gameObject;
						if ((Object)(object)item != (Object)null)
						{
							stringBuilder.AppendLine();
						}
					}
					else
					{
						item = null;
					}
				}
				while ((Object)(object)item != (Object)null);
				text = stringBuilder.ToString();
			}
			LogUIDebug("Object Tree:" + Environment.NewLine + text);
		}

		public static void DebugObjectTree(this GameObject root)
		{
			string text = "null";
			if ((Object)(object)root != (Object)null)
			{
				text = GetObjectTree(root, 0);
			}
			LogUIDebug("Object Dump:" + Environment.NewLine + text);
		}

		public static TextStyleSetting DeriveStyle(this TextStyleSetting root, int size = 0, Color? newColor = null, FontStyles? style = null)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)root == (Object)null)
			{
				throw new ArgumentNullException("root");
			}
			TextStyleSetting obj = ScriptableObject.CreateInstance<TextStyleSetting>();
			obj.enableWordWrapping = root.enableWordWrapping;
			obj.style = ((!style.HasValue) ? root.style : style.Value);
			obj.fontSize = ((size > 0) ? size : root.fontSize);
			obj.sdfFont = root.sdfFont;
			obj.textColor = (Color)(((??)newColor) ?? root.textColor);
			return obj;
		}

		public static GameObject ForceLayoutRebuild(GameObject uiElement)
		{
			if ((Object)(object)uiElement == (Object)null)
			{
				throw new ArgumentNullException("uiElement");
			}
			RectTransform val = Util.rectTransform(uiElement);
			if ((Object)(object)val != (Object)null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(val);
			}
			return uiElement;
		}

		public static float GetEmWidth(TextStyleSetting style)
		{
			float num = 0f;
			if ((Object)(object)style == (Object)null)
			{
				throw new ArgumentNullException("style");
			}
			TMP_FontAsset sdfFont = style.sdfFont;
			if ((Object)(object)sdfFont != (Object)null)
			{
				num = FontSizeCalculator.GetCharWidth('m', sdfFont);
				if (num > 0f)
				{
					FontSizeCalculator.Metrics metrics = FontSizeCalculator.Instance.Get(sdfFont);
					float num2 = (float)style.fontSize / (metrics.pointSize * metrics.scale);
					num = num * num2 + (float)style.fontSize * 0.01f * sdfFont.normalSpacingOffset;
				}
			}
			return num;
		}

		public static float GetLineHeight(TextStyleSetting style)
		{
			float result = 0f;
			if ((Object)(object)style == (Object)null)
			{
				throw new ArgumentNullException("style");
			}
			TMP_FontAsset sdfFont = style.sdfFont;
			if ((Object)(object)sdfFont != (Object)null)
			{
				FontSizeCalculator.Metrics metrics = FontSizeCalculator.Instance.Get(sdfFont);
				result = metrics.lineHeight * (float)style.fontSize / (metrics.scale * metrics.pointSize);
			}
			return result;
		}

		private static string GetObjectTree(GameObject root, int indent)
		{
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			StringBuilder stringBuilder = new StringBuilder(1024);
			StringBuilder stringBuilder2 = new StringBuilder(indent);
			for (int i = 0; i < indent; i++)
			{
				stringBuilder2.Append(' ');
			}
			string value = stringBuilder2.ToString();
			Transform transform = root.transform;
			int childCount = transform.childCount;
			stringBuilder.Append(value).AppendFormat("GameObject[{0}, {1:D} child(ren), Layer {2:D}, Active={3}]", ((Object)root).name, childCount, root.layer, root.activeInHierarchy).AppendLine();
			stringBuilder.Append(value).AppendFormat(" Translation={0} [{3}] Rotation={1} [{4}] Scale={2}", transform.position, transform.rotation, transform.localScale, transform.localPosition, transform.localRotation).AppendLine();
			Component[] components = root.GetComponents<Component>();
			foreach (Component val in components)
			{
				RectTransform val2 = (RectTransform)(object)((val is RectTransform) ? val : null);
				if (val2 != null)
				{
					stringBuilder.Append(value);
					AddTransformText(stringBuilder, val2);
					continue;
				}
				HierarchyReferences val3 = (HierarchyReferences)(object)((val is HierarchyReferences) ? val : null);
				if (val3 != null)
				{
					stringBuilder.Append(value);
					AddReferenceText(stringBuilder, val3);
				}
				else if ((Object)(object)val != (Object)null && !(val is Transform))
				{
					stringBuilder.Append(value).Append(" Component[").Append(((object)val).GetType().FullName);
					AddComponentText(stringBuilder, val);
					stringBuilder.AppendLine("]");
				}
			}
			if (childCount > 0)
			{
				stringBuilder.Append(value).AppendLine(" Children:");
			}
			for (int k = 0; k < childCount; k++)
			{
				GameObject gameObject = ((Component)transform.GetChild(k)).gameObject;
				if ((Object)(object)gameObject != (Object)null)
				{
					stringBuilder.AppendLine(GetObjectTree(gameObject, indent + 2));
				}
			}
			return stringBuilder.ToString().TrimEnd();
		}

		internal static float GetProperSize(LayoutSizes sizes, float allocated)
		{
			float num = sizes.min;
			float num2 = Math.Max(sizes.preferred, num);
			if (allocated > num)
			{
				num = Math.Min(num2, allocated);
			}
			if (allocated > num2 && sizes.flexible > 0f)
			{
				num = allocated;
			}
			return num;
		}

		internal static float GetOffset(TextAnchor alignment, PanelDirection direction, float delta)
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Invalid comparison between Unknown and I4
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected I4, but got Unknown
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Invalid comparison between Unknown and I4
			float result = 0f;
			if (direction == PanelDirection.Horizontal)
			{
				switch (alignment - 1)
				{
				case 0:
				case 3:
				case 6:
					result = delta * 0.5f;
					break;
				case 1:
				case 4:
				case 7:
					result = delta;
					break;
				}
			}
			else if (alignment - 3 > 2)
			{
				if (alignment - 6 <= 2)
				{
					result = delta;
				}
			}
			else
			{
				result = delta * 0.5f;
			}
			return result;
		}

		public static GameObject GetParent(this GameObject child)
		{
			GameObject result = null;
			if ((Object)(object)child != (Object)null)
			{
				Transform parent = child.transform.parent;
				GameObject gameObject;
				if ((Object)(object)parent != (Object)null && (Object)(object)(gameObject = ((Component)parent).gameObject) != (Object)null)
				{
					result = gameObject;
				}
			}
			return result;
		}

		public static GameObject GetSideScreenContent(DetailsScreen screen)
		{
			GameObject result = null;
			if ((Object)(object)screen != (Object)null)
			{
				SidescreenTab val = UIDetours.SS_GET_TAB.Invoke(screen, (SidescreenTabTypes)0);
				if (val != null)
				{
					result = UIDetours.SS_BODY_INSTANCE.Get(val);
				}
			}
			return result;
		}

		internal static LayoutElement InsetChild(GameObject parent, GameObject child, Vector2 insets, Vector2 prefSize = default(Vector2))
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			RectTransform val = Util.rectTransform(child);
			float x = insets.x;
			float y = insets.y;
			float x2 = prefSize.x;
			float y2 = prefSize.y;
			val.offsetMax = new Vector2(0f - x, 0f - y);
			val.offsetMin = insets;
			LayoutElement obj = EntityTemplateExtensions.AddOrGet<LayoutElement>(parent);
			float minWidth = (obj.preferredWidth = ((x2 <= 0f) ? LayoutUtility.GetPreferredWidth(val) : x2) + x * 2f);
			obj.minWidth = minWidth;
			minWidth = (obj.preferredHeight = ((y2 <= 0f) ? LayoutUtility.GetPreferredHeight(val) : y2) + y * 2f);
			obj.minHeight = minWidth;
			return obj;
		}

		private static void InsertSideScreenContent(IList<SideScreenRef> screens, SideScreenRef newScreen, string targetClassName, bool insertBefore)
		{
			if (screens == null)
			{
				throw new ArgumentNullException("screens");
			}
			if (newScreen == null)
			{
				throw new ArgumentNullException("newScreen");
			}
			if (string.IsNullOrEmpty(targetClassName))
			{
				screens.Add(newScreen);
				return;
			}
			int count = screens.Count;
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				SideScreenRef val = screens[i];
				SideScreenContent val2 = UIDetours.SS_PREFAB.Get(val);
				if (!((Object)(object)val2 != (Object)null))
				{
					continue;
				}
				SideScreenContent[] componentsInChildren = ((Component)val2).GetComponentsInChildren<SideScreenContent>();
				if (componentsInChildren == null || componentsInChildren.Length < 1)
				{
					LogUIWarning("Could not find SideScreenContent on side screen: " + val.name);
				}
				else if (((object)componentsInChildren[0]).GetType().FullName == targetClassName)
				{
					if (insertBefore)
					{
						screens.Insert(i, newScreen);
					}
					else if (i >= count - 1)
					{
						screens.Add(newScreen);
					}
					else
					{
						screens.Insert(i + 1, newScreen);
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				LogUIWarning("No side screen with class name {0} found!".F(targetClassName));
				screens.Add(newScreen);
			}
		}

		public static Sprite LoadSprite(string path, Vector4 border = default(Vector4), bool log = true)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return LoadSprite(Assembly.GetCallingAssembly(), path, border, log);
		}

		internal static Sprite LoadSprite(Assembly assembly, string path, Vector4 border = default(Vector4), bool log = false)
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected O, but got Unknown
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				using Stream stream = assembly.GetManifestResourceStream(path);
				if (stream == null)
				{
					throw new ArgumentException("Could not load image: " + path);
				}
				int num = (int)stream.Length;
				byte[] array = new byte[num];
				Texture2D val = new Texture2D(2, 2);
				num = ReadAllBytes(stream, array);
				ImageConversion.LoadImage(val, array, false);
				int width = ((Texture)val).width;
				int height = ((Texture)val).height;
				if (log)
				{
					LogUIDebug("Loaded sprite: {0} ({1:D}x{2:D}, {3:D} bytes)".F(path, width, height, num));
				}
				return Sprite.Create(val, new Rect(0f, 0f, (float)width, (float)height), new Vector2(0.5f, 0.5f), 100f, 0u, (SpriteMeshType)0, border);
			}
			catch (IOException innerException)
			{
				throw new ArgumentException("Could not load image: " + path, innerException);
			}
		}

		public static Sprite LoadSpriteFile(string path, Vector4 border = default(Vector4))
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Expected O, but got Unknown
			Sprite result = null;
			try
			{
				using FileStream fileStream = new FileStream(path, FileMode.Open);
				byte[] array = new byte[(int)fileStream.Length];
				Texture2D val = new Texture2D(2, 2);
				ReadAllBytes(fileStream, array);
				ImageConversion.LoadImage(val, array, false);
				int width = ((Texture)val).width;
				int height = ((Texture)val).height;
				result = Sprite.Create(val, new Rect(0f, 0f, (float)width, (float)height), new Vector2(0.5f, 0.5f), 100f, 0u, (SpriteMeshType)0, border);
			}
			catch (IOException)
			{
			}
			return result;
		}

		internal static Sprite LoadSpriteLegacy(Assembly assembly, string path, int width, int height, Vector4 border = default(Vector4))
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				using Stream stream = assembly.GetManifestResourceStream(path);
				if (stream == null)
				{
					throw new ArgumentException("Could not load image: " + path);
				}
				int num = (int)stream.Length - 128;
				if (num < 0)
				{
					throw new ArgumentException("Image is too small: " + path);
				}
				byte[] array = new byte[num];
				stream.Seek(128L, SeekOrigin.Begin);
				num = ReadAllBytes(stream, array);
				Texture2D val = new Texture2D(width, height, (TextureFormat)12, false);
				val.LoadRawTextureData(array);
				val.Apply(true, true);
				LogUIDebug("Loaded sprite: {0} ({1:D}x{2:D}, {3:D} bytes)".F(path, width, height, num));
				return Sprite.Create(val, new Rect(0f, 0f, (float)width, (float)height), new Vector2(0.5f, 0.5f), 100f, 0u, (SpriteMeshType)0, border);
			}
			catch (IOException innerException)
			{
				throw new ArgumentException("Could not load image: " + path, innerException);
			}
		}

		private static int ReadAllBytes(Stream stream, byte[] data)
		{
			int num = 0;
			int num2 = data.Length;
			int num3;
			do
			{
				num3 = stream.Read(data, num, num2 - num);
				num += num3;
			}
			while (num3 > 0 && num < num2);
			return num;
		}

		internal static void LogUIDebug(string message)
		{
			Debug.LogFormat("[PLib/UI/{0}] {1}", new object[2]
			{
				Assembly.GetCallingAssembly().GetName().Name ?? "?",
				message
			});
		}

		internal static void LogUIWarning(string message)
		{
			Debug.LogWarningFormat("[PLib/UI/{0}] {1}", new object[2]
			{
				Assembly.GetCallingAssembly().GetName().Name ?? "?",
				message
			});
		}

		private static void PriValue(ref float value, float newValue, int newPri, ref int pri)
		{
			int num = pri;
			if (newValue >= 0f)
			{
				if (newPri > num)
				{
					pri = newPri;
					value = newValue;
				}
				else if (newValue > value && newPri == num)
				{
					value = newValue;
				}
			}
		}

		public static GameObject SetFlexUISize(this GameObject uiElement, Vector2 flexSize)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)uiElement == (Object)null)
			{
				throw new ArgumentNullException("uiElement");
			}
			ISettableFlexSize settableFlexSize = default(ISettableFlexSize);
			if (uiElement.TryGetComponent<ISettableFlexSize>(ref settableFlexSize))
			{
				settableFlexSize.flexibleWidth = flexSize.x;
				settableFlexSize.flexibleHeight = flexSize.y;
			}
			else
			{
				LayoutElement obj = EntityTemplateExtensions.AddOrGet<LayoutElement>(uiElement);
				obj.flexibleWidth = flexSize.x;
				obj.flexibleHeight = flexSize.y;
			}
			return uiElement;
		}

		public static GameObject SetMinUISize(this GameObject uiElement, Vector2 minSize)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)uiElement == (Object)null)
			{
				throw new ArgumentNullException("uiElement");
			}
			float x = minSize.x;
			float y = minSize.y;
			if (x > 0f || y > 0f)
			{
				LayoutElement val = EntityTemplateExtensions.AddOrGet<LayoutElement>(uiElement);
				if (x > 0f)
				{
					val.minWidth = x;
				}
				if (y > 0f)
				{
					val.minHeight = y;
				}
			}
			return uiElement;
		}

		public static GameObject SetUISize(this GameObject uiElement, Vector2 size, bool addLayout = false)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)uiElement == (Object)null)
			{
				throw new ArgumentNullException("uiElement");
			}
			RectTransform val = Util.rectTransform(uiElement);
			float x = size.x;
			float y = size.y;
			if ((Object)(object)val != (Object)null)
			{
				if (x >= 0f)
				{
					val.SetSizeWithCurrentAnchors((Axis)0, x);
				}
				if (y >= 0f)
				{
					val.SetSizeWithCurrentAnchors((Axis)1, y);
				}
			}
			if (addLayout)
			{
				LayoutElement obj = EntityTemplateExtensions.AddOrGet<LayoutElement>(uiElement);
				obj.minWidth = x;
				obj.minHeight = y;
				obj.preferredWidth = x;
				obj.preferredHeight = y;
				obj.flexibleHeight = 0f;
				obj.flexibleWidth = 0f;
			}
			return uiElement;
		}
	}
	public static class TextAnchorUtils
	{
		public static bool IsLeft(this TextAnchor anchor)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Invalid comparison between Unknown and I4
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Invalid comparison between Unknown and I4
			if ((int)anchor != 0 && (int)anchor != 3)
			{
				return (int)anchor == 6;
			}
			return true;
		}

		public static bool IsLower(this TextAnchor anchor)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Invalid comparison between Unknown and I4
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Invalid comparison between Unknown and I4
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Invalid comparison between Unknown and I4
			if ((int)anchor != 7 && (int)anchor != 6)
			{
				return (int)anchor == 8;
			}
			return true;
		}

		public static bool IsRight(this TextAnchor anchor)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Invalid comparison between Unknown and I4
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Invalid comparison between Unknown and I4
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Invalid comparison between Unknown and I4
			if ((int)anchor != 2 && (int)anchor != 5)
			{
				return (int)anchor == 8;
			}
			return true;
		}

		public static bool IsUpper(this TextAnchor anchor)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Invalid comparison between Unknown and I4
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Invalid comparison between Unknown and I4
			if ((int)anchor != 1 && (int)anchor != 0)
			{
				return (int)anchor == 2;
			}
			return true;
		}

		public static TextAnchor MirrorHorizontal(this TextAnchor anchor)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Expected I4, but got Unknown
			int num = (int)anchor;
			num = 3 * (num / 3) + 2 - num % 3;
			return (TextAnchor)num;
		}

		public static TextAnchor MirrorVertical(this TextAnchor anchor)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Expected I4, but got Unknown
			int num = (int)anchor;
			num = 6 - 3 * (num / 3) + num % 3;
			return (TextAnchor)num;
		}
	}
	internal static class UIDetours
	{
		public delegate void PCD(ConfirmDialogScreen dialog, string text, Action on_confirm, Action on_cancel, string configurable_text, Action on_configurable_clicked, string title_text, string confirm_text, string cancel_text);

		public delegate SidescreenTab GetTabOfType(DetailsScreen instance, SidescreenTabTypes type);

		public static readonly DetouredMethod<PCD> POPUP_CONFIRM = typeof(ConfirmDialogScreen).DetourLazy<PCD>("PopupConfirmDialog");

		public static readonly IDetouredField<DetailsScreen, List<SideScreenRef>> SIDE_SCREENS = PDetours.DetourFieldLazy<DetailsScreen, List<SideScreenRef>>("sideScreens");

		public static readonly DetouredMethod<GetTabOfType> SS_GET_TAB = typeof(DetailsScreen).DetourLazy<GetTabOfType>("GetTabOfType");

		public static readonly IDetouredField<SidescreenTab, GameObject> SS_BODY_INSTANCE = PDetours.DetourFieldLazy<SidescreenTab, GameObject>("bodyInstance");

		public static readonly IDetouredField<KButton, KImage[]> ADDITIONAL_K_IMAGES = PDetours.DetourFieldLazy<KButton, KImage[]>("additionalKImages");

		public static readonly IDetouredField<KButton, KImage> BG_IMAGE = PDetours.DetourFieldLazy<KButton, KImage>("bgImage");

		public static readonly IDetouredField<KButton, Image> FG_IMAGE = PDetours.DetourFieldLazy<KButton, Image>("fgImage");

		public static readonly IDetouredField<KButton, bool> IS_INTERACTABLE = PDetours.DetourFieldLazy<KButton, bool>("isInteractable");

		public static readonly IDetouredField<KButton, ButtonSoundPlayer> SOUND_PLAYER_BUTTON = PDetours.DetourFieldLazy<KButton, ButtonSoundPlayer>("soundPlayer");

		public static readonly IDetouredField<KImage, ColorStyleSetting> COLOR_STYLE_SETTING = PDetours.DetourFieldLazy<KImage, ColorStyleSetting>("colorStyleSetting");

		public static readonly DetouredMethod<Action<KImage>> APPLY_COLOR_STYLE = typeof(KImage).DetourLazy<Action<KImage>>("ApplyColorStyleSetting");

		public static readonly DetouredMethod<Action<KScreen>> ACTIVATE_KSCREEN = typeof(KScreen).DetourLazy<Action<KScreen>>("Activate");

		public static readonly DetouredMethod<Action<KScreen>> DEACTIVATE_KSCREEN = typeof(KScreen).DetourLazy<Action<KScreen>>("Deactivate");

		public static readonly IDetouredField<KToggle, KToggleArtExtensions> ART_EXTENSION = PDetours.DetourFieldLazy<KToggle, KToggleArtExtensions>("artExtension");

		public static readonly IDetouredField<KToggle, bool> IS_ON = PDetours.DetourFieldLazy<KToggle, bool>("isOn");

		public static readonly IDetouredField<KToggle, ToggleSoundPlayer> SOUND_PLAYER_TOGGLE = PDetours.DetourFieldLazy<KToggle, ToggleSoundPlayer>("soundPlayer");

		public static readonly IDetouredField<LocText, string> LOCTEXT_KEY = PDetours.DetourFieldLazy<LocText, string>("key");

		public static readonly IDetouredField<LocText, TextStyleSetting> LOCTEXT_STYLE = PDetours.DetourFieldLazy<LocText, TextStyleSetting>("textStyleSetting");

		public static readonly IDetouredField<MultiToggle, int> CURRENT_STATE = PDetours.DetourFieldLazy<MultiToggle, int>("CurrentState");

		public static readonly IDetouredField<MultiToggle, bool> PLAY_SOUND_CLICK = PDetours.DetourFieldLazy<MultiToggle, bool>("play_sound_on_click");

		public static readonly IDetouredField<MultiToggle, bool> PLAY_SOUND_RELEASE = PDetours.DetourFieldLazy<MultiToggle, bool>("play_sound_on_release");

		public static readonly DetouredMethod<Action<MultiToggle, int>> CHANGE_STATE = typeof(MultiToggle).DetourLazy<Action<MultiToggle, int>>("ChangeState");

		public static readonly IDetouredField<SideScreenContent, GameObject> SS_CONTENT_CONTAINER = PDetours.DetourFieldLazy<SideScreenContent, GameObject>("ContentContainer");

		public static readonly IDetouredField<SideScreenRef, Vector2> SS_OFFSET = PDetours.DetourFieldLazy<SideScreenRef, Vector2>("offset");

		public static readonly IDetouredField<SideScreenRef, SideScreenContent> SS_PREFAB = PDetours.DetourFieldLazy<SideScreenRef, SideScreenContent>("screenPrefab");

		public static readonly IDetouredField<SideScreenRef, SideScreenContent> SS_INSTANCE = PDetours.DetourFieldLazy<SideScreenRef, SideScreenContent>("screenInstance");
	}
}
namespace PeterHan.PLib.UI.Layouts
{
	public abstract class AbstractLayoutGroup : UIBehaviour, ISettableFlexSize, ILayoutGroup, ILayoutController, ILayoutElement
	{
		[SerializeField]
		protected bool locked;

		[SerializeField]
		private float mMinWidth;

		[SerializeField]
		private float mMinHeight;

		[SerializeField]
		private float mPreferredWidth;

		[SerializeField]
		private float mPreferredHeight;

		[SerializeField]
		private float mFlexibleWidth;

		[SerializeField]
		private float mFlexibleHeight;

		[SerializeField]
		private int mLayoutPriority;

		private RectTransform cachedTransform;

		public float minWidth
		{
			get
			{
				return mMinWidth;
			}
			set
			{
				mMinWidth = value;
			}
		}

		public float preferredWidth
		{
			get
			{
				return mPreferredWidth;
			}
			set
			{
				mPreferredWidth = value;
			}
		}

		public float flexibleWidth
		{
			get
			{
				return mFlexibleWidth;
			}
			set
			{
				mFlexibleWidth = value;
			}
		}

		public float minHeight
		{
			get
			{
				return mMinHeight;
			}
			set
			{
				mMinHeight = value;
			}
		}

		public float preferredHeight
		{
			get
			{
				return mPreferredHeight;
			}
			set
			{
				mPreferredHeight = value;
			}
		}

		public float flexibleHeight
		{
			get
			{
				return mFlexibleHeight;
			}
			set
			{
				mFlexibleHeight = value;
			}
		}

		public int layoutPriority
		{
			get
			{
				return mLayoutPriority;
			}
			set
			{
				mLayoutPriority = value;
			}
		}

		protected RectTransform rectTransform
		{
			get
			{
				if ((Object)(object)cachedTransform == (Object)null)
				{
					cachedTransform = Util.rectTransform(((Component)this).gameObject);
				}
				return cachedTransform;
			}
		}

		internal static IEnumerator DelayedSetDirty(RectTransform transform)
		{
			yield return null;
			LayoutRebuilder.MarkLayoutForRebuild(transform);
		}

		internal static void DestroyAndReplaceLayout(GameObject component)
		{
			AbstractLayoutGroup abstractLayoutGroup = default(AbstractLayoutGroup);
			if ((Object)(object)component != (Object)null && component.TryGetComponent<AbstractLayoutGroup>(ref abstractLayoutGroup))
			{
				LayoutElement obj = EntityTemplateExtensions.AddOrGet<LayoutElement>(component);
				obj.flexibleHeight = abstractLayoutGroup.flexibleHeight;
				obj.flexibleWidth = abstractLayoutGroup.flexibleWidth;
				obj.layoutPriority = abstractLayoutGroup.layoutPriority;
				obj.minHeight = abstractLayoutGroup.minHeight;
				obj.minWidth = abstractLayoutGroup.minWidth;
				obj.preferredHeight = abstractLayoutGroup.preferredHeight;
				obj.preferredWidth = abstractLayoutGroup.preferredWidth;
				Object.DestroyImmediate((Object)(object)abstractLayoutGroup);
			}
		}

		protected AbstractLayoutGroup()
		{
			cachedTransform = null;
			locked = false;
			mLayoutPriority = 1;
		}

		public abstract void CalculateLayoutInputHorizontal();

		public abstract void CalculateLayoutInputVertical();

		public virtual Vector2 LockLayout()
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			RectTransform val = Util.rectTransform(((Component)this).gameObject);
			if ((Object)(object)val != (Object)null)
			{
				locked = false;
				CalculateLayoutInputHorizontal();
				val.SetSizeWithCurrentAnchors((Axis)0, minWidth);
				SetLayoutHorizontal();
				CalculateLayoutInputVertical();
				val.SetSizeWithCurrentAnchors((Axis)1, minHeight);
				SetLayoutVertical();
				locked = true;
			}
			return new Vector2(minWidth, minHeight);
		}

		protected override void OnDidApplyAnimationProperties()
		{
			((UIBehaviour)this).OnDidApplyAnimationProperties();
			SetDirty();
		}

		protected override void OnDisable()
		{
			((UIBehaviour)this).OnDisable();
			SetDirty();
		}

		protected override void OnEnable()
		{
			((UIBehaviour)this).OnEnable();
			SetDirty();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			((UIBehaviour)this).OnRectTransformDimensionsChange();
			SetDirty();
		}

		protected virtual void SetDirty()
		{
			if ((Object)(object)((Component)this).gameObject != (Object)null && ((UIBehaviour)this).IsActive())
			{
				if (CanvasUpdateRegistry.IsRebuildingLayout())
				{
					LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
				}
				else
				{
					((MonoBehaviour)this).StartCoroutine(DelayedSetDirty(rectTransform));
				}
			}
		}

		public abstract void SetLayoutHorizontal();

		public abstract void SetLayoutVertical();

		public virtual void UnlockLayout()
		{
			locked = false;
			LayoutRebuilder.MarkLayoutForRebuild(Util.rectTransform(((Component)this).gameObject));
		}
	}
	internal sealed class BoxLayoutResults
	{
		public readonly ICollection<LayoutSizes> children;

		public readonly PanelDirection direction;

		private bool haveMinSpace;

		private bool havePrefSpace;

		public LayoutSizes total;

		internal BoxLayoutResults(PanelDirection direction, int presize)
		{
			children = new List<LayoutSizes>(presize);
			this.direction = direction;
			haveMinSpace = false;
			havePrefSpace = false;
			total = default(LayoutSizes);
		}

		public void Accum(LayoutSizes sizes, float spacing)
		{
			float num = sizes.min;
			float num2 = sizes.preferred;
			if (num > 0f)
			{
				if (haveMinSpace)
				{
					num += spacing;
				}
				haveMinSpace = true;
			}
			total.min += num;
			if (num2 > 0f)
			{
				if (havePrefSpace)
				{
					num2 += spacing;
				}
				havePrefSpace = true;
			}
			total.preferred += num2;
			total.flexible += sizes.flexible;
		}

		public void Expand(LayoutSizes sizes)
		{
			float min = sizes.min;
			float preferred = sizes.preferred;
			float flexible = sizes.flexible;
			if (min > total.min)
			{
				total.min = min;
			}
			if (preferred > total.preferred)
			{
				total.preferred = preferred;
			}
			if (flexible > total.flexible)
			{
				total.flexible = flexible;
			}
		}

		public override string ToString()
		{
			string text = direction.ToString();
			LayoutSizes layoutSizes = total;
			return text + " " + layoutSizes.ToString();
		}
	}
	internal sealed class BoxLayoutStatus
	{
		public readonly PanelDirection direction;

		public readonly Edge edge;

		public readonly float offset;

		public readonly float size;

		internal BoxLayoutStatus(PanelDirection direction, RectOffset margins, float size)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			this.direction = direction;
			switch (direction)
			{
			case PanelDirection.Horizontal:
				edge = (Edge)0;
				offset = margins.left;
				this.size = size - offset - (float)margins.right;
				break;
			case PanelDirection.Vertical:
				edge = (Edge)2;
				offset = margins.top;
				this.size = size - offset - (float)margins.bottom;
				break;
			default:
				throw new ArgumentException("direction");
			}
		}
	}
	internal sealed class CardLayoutResults
	{
		public readonly ICollection<LayoutSizes> children;

		public readonly PanelDirection direction;

		public LayoutSizes total;

		internal CardLayoutResults(PanelDirection direction, int presize)
		{
			children = new List<LayoutSizes>(presize);
			this.direction = direction;
			total = default(LayoutSizes);
		}

		public void Expand(LayoutSizes sizes)
		{
			float min = sizes.min;
			float preferred = sizes.preferred;
			float flexible = sizes.flexible;
			if (min > total.min)
			{
				total.min = min;
			}
			if (preferred > total.preferred)
			{
				total.preferred = preferred;
			}
			if (flexible > total.flexible)
			{
				total.flexible = flexible;
			}
		}

		public override string ToString()
		{
			string text = direction.ToString();
			LayoutSizes layoutSizes = total;
			return text + " " + layoutSizes.ToString();
		}
	}
	internal sealed class GridLayoutResults
	{
		public IList<GridColumnSpec> ColumnSpecs { get; }

		public ICollection<SizedGridComponent> Components { get; }

		public int Columns { get; }

		public IList<GridColumnSpec> ComputedColumnSpecs { get; }

		public IList<GridRowSpec> ComputedRowSpecs { get; }

		public float MinHeight { get; private set; }

		public float MinWidth { get; private set; }

		public ICollection<SizedGridComponent>[,] Matrix { get; }

		public IList<GridRowSpec> RowSpecs { get; }

		public int Rows { get; }

		public float TotalFlexHeight { get; private set; }

		public float TotalFlexWidth { get; private set; }

		private static ICollection<SizedGridComponent>[,] GetMatrix(int rows, int columns, ICollection<SizedGridComponent> components)
		{
			ICollection<SizedGridComponent>[,] array = new ICollection<SizedGridComponent>[rows, columns];
			foreach (SizedGridComponent component in components)
			{
				int row = component.Row;
				int column = component.Column;
				if (row >= 0 && row < rows && column >= 0 && column < columns)
				{
					ICollection<SizedGridComponent> collection = array[row, column];
					if (collection == null)
					{
						collection = (array[row, column] = new List<SizedGridComponent>(8));
					}
					collection.Add(component);
				}
			}
			return array;
		}

		internal GridLayoutResults(IList<GridRowSpec> rows, IList<GridColumnSpec> columns, ICollection<GridComponent<GameObject>> components)
		{
			if (rows == null)
			{
				throw new ArgumentNullException("rows");
			}
			if (columns == null)
			{
				throw new ArgumentNullException("columns");
			}
			if (components == null)
			{
				throw new ArgumentNullException("components");
			}
			Columns = columns.Count;
			Rows = rows.Count;
			ColumnSpecs = columns;
			MinHeight = (MinWidth = 0f);
			RowSpecs = rows;
			ComputedColumnSpecs = new List<GridColumnSpec>(Columns);
			ComputedRowSpecs = new List<GridRowSpec>(Rows);
			TotalFlexHeight = (TotalFlexWidth = 0f);
			Components = new List<SizedGridComponent>(Math.Max(components.Count, 4));
			foreach (GridComponent<GameObject> component in components)
			{
				GameObject item = component.Item;
				if ((Object)(object)item != (Object)null)
				{
					Components.Add(new SizedGridComponent(component, item));
				}
			}
			Matrix = GetMatrix(Rows, Columns, Components);
		}

		internal void CalcBaseHeights()
		{
			int rows = Rows;
			float minHeight = (TotalFlexHeight = 0f);
			MinHeight = minHeight;
			ComputedRowSpecs.Clear();
			for (int i = 0; i < rows; i++)
			{
				GridRowSpec gridRowSpec = RowSpecs[i];
				float num2 = gridRowSpec.Height;
				float flexHeight = gridRowSpec.FlexHeight;
				if (num2 <= 0f)
				{
					for (int j = 0; j < Columns; j++)
					{
						num2 = Math.Max(num2, PreferredHeightAt(i, j));
					}
				}
				if (flexHeight > 0f)
				{
					TotalFlexHeight += flexHeight;
				}
				ComputedRowSpecs.Add(new GridRowSpec(num2, flexHeight));
			}
			foreach (SizedGridComponent component in Components)
			{
				if (component.RowSpan > 1)
				{
					ExpandMultiRow(component);
				}
			}
			for (int k = 0; k < rows; k++)
			{
				MinHeight += ComputedRowSpecs[k].Height;
			}
		}

		internal void CalcBaseWidths()
		{
			int columns = Columns;
			float minWidth = (TotalFlexWidth = 0f);
			MinWidth = minWidth;
			ComputedColumnSpecs.Clear();
			for (int i = 0; i < columns; i++)
			{
				GridColumnSpec gridColumnSpec = ColumnSpecs[i];
				float num2 = gridColumnSpec.Width;
				float flexWidth = gridColumnSpec.FlexWidth;
				if (num2 <= 0f)
				{
					for (int j = 0; j < Rows; j++)
					{
						num2 = Math.Max(num2, PreferredWidthAt(j, i));
					}
				}
				if (flexWidth > 0f)
				{
					TotalFlexWidth += flexWidth;
				}
				ComputedColumnSpecs.Add(new GridColumnSpec(num2, flexWidth));
			}
			foreach (SizedGridComponent component in Components)
			{
				if (component.ColumnSpan > 1)
				{
					ExpandMultiColumn(component);
				}
			}
			for (int k = 0; k < columns; k++)
			{
				MinWidth += ComputedColumnSpecs[k].Width;
			}
		}

		private void ExpandMultiColumn(SizedGridComponent component)
		{
			float num = component.HorizontalSize.preferred;
			float num2 = 0f;
			int column = component.Column;
			int num3 = column + component.ColumnSpan;
			for (int i = column; i < num3; i++)
			{
				GridColumnSpec gridColumnSpec = ComputedColumnSpecs[i];
				if (gridColumnSpec.FlexWidth > 0f)
				{
					num2 += gridColumnSpec.FlexWidth;
				}
				num -= gridColumnSpec.Width;
			}
			if (!(num > 0f) || !(num2 > 0f))
			{
				return;
			}
			for (int j = column; j < num3; j++)
			{
				GridColumnSpec gridColumnSpec2 = ComputedColumnSpecs[j];
				float flexWidth = gridColumnSpec2.FlexWidth;
				if (flexWidth > 0f)
				{
					ComputedColumnSpecs[j] = new GridColumnSpec(gridColumnSpec2.Width + flexWidth * num / num2, flexWidth);
				}
			}
		}

		private void ExpandMultiRow(SizedGridComponent component)
		{
			float num = component.VerticalSize.preferred;
			float num2 = 0f;
			int row = component.Row;
			int num3 = row + component.RowSpan;
			for (int i = row; i < num3; i++)
			{
				GridRowSpec gridRowSpec = ComputedRowSpecs[i];
				if (gridRowSpec.FlexHeight > 0f)
				{
					num2 += gridRowSpec.FlexHeight;
				}
				num -= gridRowSpec.Height;
			}
			if (!(num > 0f) || !(num2 > 0f))
			{
				return;
			}
			for (int j = row; j < num3; j++)
			{
				GridRowSpec gridRowSpec2 = ComputedRowSpecs[j];
				float flexHeight = gridRowSpec2.FlexHeight;
				if (flexHeight > 0f)
				{
					ComputedRowSpecs[j] = new GridRowSpec(gridRowSpec2.Height + flexHeight * num / num2, flexHeight);
				}
			}
		}

		private float PreferredHeightAt(int row, int column)
		{
			float num = 0f;
			ICollection<SizedGridComponent> collection = Matrix[row, column];
			if (collection != null && collection.Count > 0)
			{
				foreach (SizedGridComponent item in collection)
				{
					LayoutSizes verticalSize = item.VerticalSize;
					if (item.RowSpan < 2)
					{
						num = Math.Max(num, verticalSize.preferred);
					}
				}
			}
			return num;
		}

		private float PreferredWidthAt(int row, int column)
		{
			float num = 0f;
			ICollection<SizedGridComponent> collection = Matrix[row, column];
			if (collection != null && collection.Count > 0)
			{
				foreach (SizedGridComponent item in collection)
				{
					LayoutSizes horizontalSize = item.HorizontalSize;
					if (item.ColumnSpan < 2)
					{
						num = Math.Max(num, horizontalSize.preferred);
					}
				}
			}
			return num;
		}
	}
	internal sealed class SizedGridComponent : GridComponentSpec
	{
		public LayoutSizes HorizontalSize { get; set; }

		public LayoutSizes VerticalSize { get; set; }

		internal SizedGridComponent(GridComponentSpec spec, GameObject item)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			base.Alignment = spec.Alignment;
			base.Column = spec.Column;
			base.ColumnSpan = spec.ColumnSpan;
			base.Margin = spec.Margin;
			base.Row = spec.Row;
			base.RowSpan = spec.RowSpan;
			HorizontalSize = new LayoutSizes(item);
			VerticalSize = new LayoutSizes(item);
		}
	}
	[Serializable]
	internal sealed class RelativeLayoutParams : RelativeLayoutParamsBase<GameObject>
	{
	}
	internal class RelativeLayoutParamsBase<T>
	{
		[Serializable]
		internal sealed class EdgeStatus
		{
			internal RelativeConstraintType Constraint;

			internal float FromAnchor;

			internal T FromComponent;

			internal float Offset;

			public bool Locked => Constraint == RelativeConstraintType.Locked;

			public bool Unconstrained => Constraint == RelativeConstraintType.Unconstrained;

			internal EdgeStatus()
			{
				FromAnchor = 0f;
				FromComponent = default(T);
				Offset = 0f;
				Constraint = RelativeConstraintType.Unconstrained;
			}

			internal void CopyFrom(EdgeStatus other)
			{
				if (other == null)
				{
					throw new ArgumentNullException("other");
				}
				switch (Constraint = other.Constraint)
				{
				case RelativeConstraintType.ToComponent:
					FromComponent = other.FromComponent;
					break;
				case RelativeConstraintType.ToAnchor:
					FromAnchor = other.FromAnchor;
					break;
				case RelativeConstraintType.Locked:
					FromAnchor = other.FromAnchor;
					Offset = other.Offset;
					break;
				case RelativeConstraintType.Unconstrained:
					break;
				}
			}

			public override bool Equals(object obj)
			{
				if (obj is EdgeStatus edgeStatus && edgeStatus.FromAnchor == FromAnchor && edgeStatus.Offset == Offset)
				{
					return Constraint == edgeStatus.Constraint;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return 37 * (37 * FromAnchor.GetHashCode() + Offset.GetHashCode()) + Constraint.GetHashCode();
			}

			public void Reset()
			{
				Constraint = RelativeConstraintType.Unconstrained;
				Offset = 0f;
				FromAnchor = 0f;
				FromComponent = default(T);
			}

			public override string ToString()
			{
				return string.Format("EdgeStatus[Constraints={2},Anchor={0:F2},Offset={1:F2}]", FromAnchor, Offset, Constraint);
			}
		}

		internal EdgeStatus BottomEdge { get; }

		internal RectOffset Insets { get; set; }

		internal EdgeStatus LeftEdge { get; }

		internal Vector2 OverrideSize { get; set; }

		internal EdgeStatus RightEdge { get; }

		internal EdgeStatus TopEdge { get; }

		internal RelativeLayoutParamsBase()
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			Insets = null;
			BottomEdge = new EdgeStatus();
			LeftEdge = new EdgeStatus();
			RightEdge = new EdgeStatus();
			TopEdge = new EdgeStatus();
			OverrideSize = Vector2.zero;
		}
	}
	internal enum RelativeConstraintType
	{
		Unconstrained,
		ToComponent,
		ToAnchor,
		Locked
	}
	internal sealed class RelativeLayoutResults : RelativeLayoutParamsBase<GameObject>
	{
		private static readonly RectOffset ZERO = new RectOffset();

		private Vector2 prefSize;

		internal RelativeLayoutResults BottomParams { get; set; }

		internal float EffectiveHeight { get; private set; }

		internal float EffectiveWidth { get; private set; }

		internal RelativeLayoutResults LeftParams { get; set; }

		internal float PreferredHeight
		{
			get
			{
				return prefSize.y;
			}
			set
			{
				prefSize.y = value;
				EffectiveHeight = value + (float)base.Insets.top + (float)base.Insets.bottom;
			}
		}

		internal float PreferredWidth
		{
			get
			{
				return prefSize.x;
			}
			set
			{
				prefSize.x = value;
				EffectiveWidth = value + (float)base.Insets.left + (float)base.Insets.right;
			}
		}

		internal RelativeLayoutResults RightParams { get; set; }

		internal RelativeLayoutResults TopParams { get; set; }

		internal RectTransform Transform { get; set; }

		internal bool UseSizeDeltaX { get; set; }

		internal bool UseSizeDeltaY { get; set; }

		internal RelativeLayoutResults(RectTransform transform, RelativeLayoutParams other)
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			Transform = transform ?? throw new ArgumentNullException("transform");
			if (other != null)
			{
				base.BottomEdge.CopyFrom(other.BottomEdge);
				base.TopEdge.CopyFrom(other.TopEdge);
				base.RightEdge.CopyFrom(other.RightEdge);
				base.LeftEdge.CopyFrom(other.LeftEdge);
				base.Insets = other.Insets ?? ZERO;
				base.OverrideSize = other.OverrideSize;
			}
			else
			{
				base.Insets = ZERO;
			}
			BottomParams = (LeftParams = (TopParams = (RightParams = null)));
			PreferredWidth = (PreferredHeight = 0f);
			UseSizeDeltaX = (UseSizeDeltaY = false);
		}

		public override string ToString()
		{
			GameObject gameObject = ((Component)Transform).gameObject;
			return string.Format("component={0} {1:F2}x{2:F2}", ((Object)(object)gameObject == (Object)null) ? "null" : ((Object)gameObject).name, prefSize.x, prefSize.y);
		}
	}
	internal static class RelativeLayoutUtil
	{
		internal static void CalcX(this ICollection<RelativeLayoutResults> children, RectTransform all, IDictionary<GameObject, RelativeLayoutParams> constraints)
		{
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			PooledList<Component, RelativeLayoutGroup> val = ListPool<Component, RelativeLayoutGroup>.Allocate();
			PooledDictionary<GameObject, RelativeLayoutResults, RelativeLayoutGroup> val2 = DictionaryPool<GameObject, RelativeLayoutResults, RelativeLayoutGroup>.Allocate();
			int childCount = ((Transform)all).childCount;
			children.Clear();
			for (int i = 0; i < childCount; i++)
			{
				Transform child = ((Transform)all).GetChild(i);
				GameObject val3 = ((child != null) ? ((Component)child).gameObject : null);
				if (!((Object)(object)val3 != (Object)null))
				{
					continue;
				}
				((List<Component>)(object)val).Clear();
				val3.GetComponents<Component>((List<Component>)(object)val);
				LayoutSizes layoutSizes = PUIUtils.CalcSizes(val3, PanelDirection.Horizontal, (IEnumerable<Component>)val);
				if (layoutSizes.ignore)
				{
					continue;
				}
				float preferredWidth = layoutSizes.preferred;
				RelativeLayoutResults relativeLayoutResults;
				if (constraints.TryGetValue(val3, out var value))
				{
					relativeLayoutResults = new RelativeLayoutResults(Util.rectTransform(val3), value);
					Vector2 overrideSize = relativeLayoutResults.OverrideSize;
					if (overrideSize.x > 0f)
					{
						preferredWidth = overrideSize.x;
					}
					((Dictionary<GameObject, RelativeLayoutResults>)(object)val2)[val3] = relativeLayoutResults;
				}
				else
				{
					relativeLayoutResults = new RelativeLayoutResults(Util.rectTransform(val3), null);
				}
				relativeLayoutResults.PreferredWidth = preferredWidth;
				children.Add(relativeLayoutResults);
			}
			foreach (RelativeLayoutResults child2 in children)
			{
				child2.TopParams = InitResolve(child2.TopEdge, (IDictionary<GameObject, RelativeLayoutResults>)val2);
				child2.BottomParams = InitResolve(child2.BottomEdge, (IDictionary<GameObject, RelativeLayoutResults>)val2);
				child2.LeftParams = InitResolve(child2.LeftEdge, (IDictionary<GameObject, RelativeLayoutResults>)val2);
				child2.RightParams = InitResolve(child2.RightEdge, (IDictionary<GameObject, RelativeLayoutResults>)val2);
			}
			val2.Recycle();
			val.Recycle();
		}

		internal static void CalcY(this ICollection<RelativeLayoutResults> children)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			PooledList<Component, RelativeLayoutGroup> val = ListPool<Component, RelativeLayoutGroup>.Allocate();
			foreach (RelativeLayoutResults child in children)
			{
				GameObject gameObject = ((Component)child.Transform).gameObject;
				Vector2 overrideSize = child.OverrideSize;
				((List<Component>)(object)val).Clear();
				gameObject.gameObject.GetComponents<Component>((List<Component>)(object)val);
				float preferredHeight = PUIUtils.CalcSizes(gameObject, PanelDirection.Vertical, (IEnumerable<Component>)val).preferred;
				if (overrideSize.y > 0f)
				{
					preferredHeight = overrideSize.y;
				}
				child.PreferredHeight = preferredHeight;
			}
			val.Recycle();
		}

		internal static float ElbowRoom(RelativeLayoutParamsBase<GameObject>.EdgeStatus min, RelativeLayoutParamsBase<GameObject>.EdgeStatus max, float effective)
		{
			float fromAnchor = min.FromAnchor;
			float fromAnchor2 = max.FromAnchor;
			float offset = min.Offset;
			float offset2 = max.Offset;
			if (fromAnchor2 > fromAnchor)
			{
				return (effective + offset - offset2) / (fromAnchor2 - fromAnchor);
			}
			return Math.Max(effective, Math.Max(Math.Abs(offset), Math.Abs(offset2)));
		}

		internal static void ExecuteX(this IEnumerable<RelativeLayoutResults> children, List<ILayoutController> scratch, float mLeft = 0f, float mRight = 0f)
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			foreach (RelativeLayoutResults child in children)
			{
				RectTransform transform = child.Transform;
				RectOffset insets = child.Insets;
				RelativeLayoutParamsBase<GameObject>.EdgeStatus leftEdge = child.LeftEdge;
				RelativeLayoutParamsBase<GameObject>.EdgeStatus rightEdge = child.RightEdge;
				transform.anchorMin = new Vector2(leftEdge.FromAnchor, 0f);
				transform.anchorMax = new Vector2(rightEdge.FromAnchor, 1f);
				if (child.UseSizeDeltaX)
				{
					transform.SetSizeWithCurrentAnchors((Axis)0, child.PreferredWidth);
				}
				else
				{
					transform.offsetMin = new Vector2(leftEdge.Offset + (float)insets.left + ((leftEdge.FromAnchor <= 0f) ? mLeft : 0f), transform.offsetMin.y);
					transform.offsetMax = new Vector2(rightEdge.Offset - (float)insets.right - ((rightEdge.FromAnchor >= 1f) ? mRight : 0f), transform.offsetMax.y);
				}
				scratch.Clear();
				((Component)transform).gameObject.GetComponents<ILayoutController>(scratch);
				foreach (ILayoutController item in scratch)
				{
					item.SetLayoutHorizontal();
				}
			}
		}

		internal static void ExecuteY(this IEnumerable<RelativeLayoutResults> children, List<ILayoutController> scratch, float mBottom = 0f, float mTop = 0f)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			foreach (RelativeLayoutResults child in children)
			{
				RectTransform transform = child.Transform;
				RectOffset insets = child.Insets;
				RelativeLayoutParamsBase<GameObject>.EdgeStatus topEdge = child.TopEdge;
				RelativeLayoutParamsBase<GameObject>.EdgeStatus bottomEdge = child.BottomEdge;
				transform.anchorMin = new Vector2(transform.anchorMin.x, bottomEdge.FromAnchor);
				transform.anchorMax = new Vector2(transform.anchorMax.x, topEdge.FromAnchor);
				if (child.UseSizeDeltaY)
				{
					transform.SetSizeWithCurrentAnchors((Axis)1, child.PreferredHeight);
				}
				else
				{
					transform.offsetMin = new Vector2(transform.offsetMin.x, bottomEdge.Offset + (float)insets.bottom + ((bottomEdge.FromAnchor <= 0f) ? mBottom : 0f));
					transform.offsetMax = new Vector2(transform.offsetMax.x, topEdge.Offset - (float)insets.top - ((topEdge.FromAnchor >= 1f) ? mTop : 0f));
				}
				scratch.Clear();
				((Component)transform).gameObject.GetComponents<ILayoutController>(scratch);
				foreach (ILayoutController item in scratch)
				{
					item.SetLayoutVertical();
				}
			}
		}

		internal static float GetMinSizeX(this IEnumerable<RelativeLayoutResults> children)
		{
			float num = 0f;
			foreach (RelativeLayoutResults child in children)
			{
				float num2 = ElbowRoom(child.LeftEdge, child.RightEdge, child.EffectiveWidth);
				if (num2 > num)
				{
					num = num2;
				}
			}
			return num;
		}

		internal static float GetMinSizeY(this IEnumerable<RelativeLayoutResults> children)
		{
			float num = 0f;
			foreach (RelativeLayoutResults child in children)
			{
				float num2 = ElbowRoom(child.BottomEdge, child.TopEdge, child.EffectiveHeight);
				if (num2 > num)
				{
					num = num2;
				}
			}
			return num;
		}

		private static RelativeLayoutResults InitResolve(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, IDictionary<GameObject, RelativeLayoutResults> lookup)
		{
			RelativeLayoutResults value = null;
			if (edge.Constraint == RelativeConstraintType.ToComponent && !lookup.TryGetValue(edge.FromComponent, out value))
			{
				edge.Constraint = RelativeConstraintType.Unconstrained;
			}
			return value;
		}

		private static bool LockEdgeAnchor(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, RelativeLayoutParamsBase<GameObject>.EdgeStatus otherEdge)
		{
			int num;
			if (edge.Constraint == RelativeConstraintType.ToAnchor && otherEdge.Constraint == RelativeConstraintType.ToAnchor)
			{
				num = ((edge.FromAnchor == otherEdge.FromAnchor) ? 1 : 0);
				if (num != 0)
				{
					edge.Constraint = RelativeConstraintType.Locked;
					otherEdge.Constraint = RelativeConstraintType.Locked;
					edge.Offset = 0f;
					otherEdge.Offset = 0f;
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		}

		private static void LockEdgeAnchor(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge)
		{
			if (edge.Constraint == RelativeConstraintType.ToAnchor)
			{
				edge.Constraint = RelativeConstraintType.Locked;
				edge.Offset = 0f;
			}
		}

		private static void LockEdgeComponent(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, RelativeLayoutParamsBase<GameObject>.EdgeStatus otherEdge)
		{
			if (edge.Constraint == RelativeConstraintType.ToComponent && otherEdge.Locked)
			{
				edge.Constraint = RelativeConstraintType.Locked;
				edge.FromAnchor = otherEdge.FromAnchor;
				edge.Offset = otherEdge.Offset;
			}
		}

		private static void LockEdgeRelative(RelativeLayoutParamsBase<GameObject>.EdgeStatus edge, float size, RelativeLayoutParamsBase<GameObject>.EdgeStatus opposing)
		{
			if (edge.Constraint == RelativeConstraintType.Unconstrained)
			{
				if (opposing.Locked)
				{
					edge.Constraint = RelativeConstraintType.Locked;
					edge.FromAnchor = opposing.FromAnchor;
					edge.Offset = opposing.Offset + size;
				}
				else if (opposing.Constraint == RelativeConstraintType.Unconstrained)
				{
					edge.Constraint = RelativeConstraintType.Locked;
					edge.FromAnchor = 0f;
					edge.Offset = 0f;
					opposing.Constraint = RelativeConstraintType.Locked;
					opposing.FromAnchor = 1f;
					opposing.Offset = 0f;
				}
			}
		}

		internal static bool RunPassX(this IEnumerable<RelativeLayoutResults> children)
		{
			bool result = true;
			foreach (RelativeLayoutResults child in children)
			{
				float effectiveWidth = child.EffectiveWidth;
				RelativeLayoutParamsBase<GameObject>.EdgeStatus leftEdge = child.LeftEdge;
				RelativeLayoutParamsBase<GameObject>.EdgeStatus rightEdge = child.RightEdge;
				if (LockEdgeAnchor(leftEdge, rightEdge))
				{
					child.UseSizeDeltaX = true;
				}
				LockEdgeAnchor(leftEdge);
				LockEdgeAnchor(rightEdge);
				LockEdgeRelative(leftEdge, 0f - effectiveWidth, rightEdge);
				LockEdgeRelative(rightEdge, effectiveWidth, leftEdge);
				if (child.LeftParams != null)
				{
					LockEdgeComponent(leftEdge, child.LeftParams.RightEdge);
				}
				if (child.RightParams != null)
				{
					LockEdgeComponent(rightEdge, child.RightParams.LeftEdge);
				}
				if (!leftEdge.Locked || !rightEdge.Locked)
				{
					result = false;
				}
			}
			return result;
		}

		internal static bool RunPassY(this IEnumerable<RelativeLayoutResults> children)
		{
			bool result = true;
			foreach (RelativeLayoutResults child in children)
			{
				float effectiveHeight = child.EffectiveHeight;
				RelativeLayoutParamsBase<GameObject>.EdgeStatus topEdge = child.TopEdge;
				RelativeLayoutParamsBase<GameObject>.EdgeStatus bottomEdge = child.BottomEdge;
				if (LockEdgeAnchor(topEdge, bottomEdge))
				{
					child.UseSizeDeltaY = true;
				}
				LockEdgeAnchor(bottomEdge);
				LockEdgeAnchor(topEdge);
				LockEdgeRelative(bottomEdge, 0f - effectiveHeight, topEdge);
				LockEdgeRelative(topEdge, effectiveHeight, bottomEdge);
				if (child.BottomParams != null)
				{
					LockEdgeComponent(bottomEdge, child.BottomParams.TopEdge);
				}
				if (child.TopParams != null)
				{
					LockEdgeComponent(topEdge, child.TopParams.BottomEdge);
				}
				if (!topEdge.Locked || !bottomEdge.Locked)
				{
					result = false;
				}
			}
			return result;
		}

		internal static void ThrowUnresolvable(this IEnumerable<RelativeLayoutResults> children, int limit, PanelDirection direction)
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			stringBuilder.Append("After ").Append(limit);
			stringBuilder.Append(" passes, unable to complete resolution of RelativeLayout:");
			foreach (RelativeLayoutResults child in children)
			{
				string name = ((Object)((Component)child.Transform).gameObject).name;
				if (direction == PanelDirection.Horizontal)
				{
					if (!child.LeftEdge.Locked)
					{
						stringBuilder.Append(' ');
						stringBuilder.Append(name);
						stringBuilder.Append(".Left");
					}
					if (!child.RightEdge.Locked)
					{
						stringBuilder.Append(' ');
						stringBuilder.Append(name);
						stringBuilder.Append(".Right");
					}
				}
				else
				{
					if (!child.BottomEdge.Locked)
					{
						stringBuilder.Append(' ');
						stringBuilder.Append(name);
						stringBuilder.Append(".Bottom");
					}
					if (!child.TopEdge.Locked)
					{
						stringBuilder.Append(' ');
						stringBuilder.Append(name);
						stringBuilder.Append(".Top");
					}
				}
			}
			throw new InvalidOperationException(stringBuilder.ToString());
		}
	}
}
