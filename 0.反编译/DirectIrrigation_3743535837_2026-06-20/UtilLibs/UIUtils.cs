using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using PeterHan.PLib.Actions;
using STRINGS;
using TMPro;
using UnityEngine;

namespace UtilLibs;

public static class UIUtils
{
	public static readonly List<Color> RainbowColors = new List<Color>
	{
		Color.HSVToRGB(0f, 1f, 1f),
		Color.HSVToRGB(0.125f, 1f, 1f),
		Color.HSVToRGB(0.25f, 1f, 1f),
		Color.HSVToRGB(0.375f, 1f, 1f),
		Color.HSVToRGB(0.5f, 1f, 1f),
		Color.HSVToRGB(0.625f, 1f, 1f),
		Color.HSVToRGB(0.75f, 1f, 1f),
		Color.HSVToRGB(0.875f, 1f, 1f)
	};

	public static readonly List<Color> RainbowColorsDesaturated = new List<Color>
	{
		Color.HSVToRGB(0f, 0.65f, 1f),
		Color.HSVToRGB(0.125f, 0.65f, 1f),
		Color.HSVToRGB(0.25f, 0.65f, 1f),
		Color.HSVToRGB(0.375f, 0.65f, 1f),
		Color.HSVToRGB(0.5f, 0.65f, 1f),
		Color.HSVToRGB(0.625f, 0.65f, 1f),
		Color.HSVToRGB(0.75f, 0.65f, 1f),
		Color.HSVToRGB(0.875f, 0.65f, 1f)
	};

	public static Color number_green = Lighten(Util.ColorFromHex("367d48"), 20f);

	public static Color number_red = Lighten(Util.ColorFromHex("802024"), 20f);

	private static Dictionary<Tuple<KAnimFile, string>, Symbol> knownSymbols = new Dictionary<Tuple<KAnimFile, string>, Symbol>();

	public static string GetFormattedPActionDescription(this PAction action)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (action == null)
		{
			return string.Empty;
		}
		return UI.FormatAsHotkey("[" + GameUtil.GetActionString(action.GetKAction()) + "]");
	}

	public static ColorStyleSetting BuildColorStyleFromColor(Color color)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		ColorStyleSetting val = (ColorStyleSetting)ScriptableObject.CreateInstance("ColorStyleSetting");
		val.inactiveColor = color;
		val.hoverColor = Lighten(color, 20f);
		val.activeColor = Lighten(color, 40f);
		val.disabledColor = Lerp(color, Color.gray, 50f);
		return val;
	}

	public static void GiveAllChildObjects(GameObject start)
	{
		Object[] componentsInChildren = start.GetComponentsInChildren<Object>();
		Object[] array = componentsInChildren;
		foreach (Object val in array)
		{
			Debug.Log((object)val);
		}
	}

	public static Color rgb(float r, float g, float b)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		return new Color(r / 255f, g / 255f, b / 255f);
	}

	public static Color rgba(float r, float g, float b, double a)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		return new Color(r / 255f, g / 255f, b / 255f, (float)a);
	}

	public static Color Lerp(Color a, Color b, float percentage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return Color.Lerp(a, b, percentage / 100f);
	}

	public static Color Lighten(Color original, float percentage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Color.Lerp(original, Color.white, percentage / 100f);
	}

	public static Color Darken(Color original, float percentage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Color.Lerp(original, Color.black, percentage / 100f);
	}

	public static Color HSVShift(Color original, float percentage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		float num2 = default(float);
		float num3 = default(float);
		float num = default(float);
		Color.RGBToHSV(original, ref num, ref num2, ref num3);
		num = (num + percentage / 100f) % 1f;
		return Color.HSVToRGB(num, num2, num3);
	}

	public static bool AddActionToButton(Transform parent, string subCompName, Action action, bool clearOnclick = true)
	{
		Transform val = parent.Find(subCompName);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		KButton val2 = TryFindComponent<KButton>(val);
		if ((Object)(object)val2 == (Object)null)
		{
			return false;
		}
		if (clearOnclick)
		{
			val2.ClearOnClick();
		}
		val2.onClick += action;
		return true;
	}

	public static T TryFindComponent<T>(Transform parent, string subCompName = "")
	{
		Transform val = ((!(subCompName != "")) ? parent : parent.Find(subCompName));
		if ((Object)(object)val == (Object)null)
		{
			return default(T);
		}
		T result = default(T);
		((Component)val).TryGetComponent<T>(ref result);
		return result;
	}

	public static bool TryChangeText(Transform transform, string subCompName, string newText)
	{
		Transform val = ((!(subCompName != "")) ? transform : transform.Find(subCompName));
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		LocText component = ((Component)val).gameObject.GetComponent<LocText>();
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		component.key = string.Empty;
		((TMP_Text)component).text = newText;
		return true;
	}

	public static ToolTip AddSimpleTooltipToObject(Transform go, string tooltip, bool alignCenter = true, float wrapWidth = 0f, bool onBottom = true)
	{
		return AddSimpleTooltipToObject(((Component)go).gameObject, tooltip, alignCenter, wrapWidth, onBottom);
	}

	public static ToolTip AddSimpleTooltipToObject(GameObject go, string tooltip, bool alignCenter = true, float wrapWidth = 0f, bool onBottom = true)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)go == (Object)null)
		{
			return null;
		}
		ToolTip val = EntityTemplateExtensions.AddOrGet<ToolTip>(go);
		val.UseFixedStringKey = false;
		((Behaviour)val).enabled = true;
		val.tooltipPivot = (alignCenter ? new Vector2(0.5f, onBottom ? 1f : 0f) : new Vector2(1f, onBottom ? 1f : 0f));
		val.tooltipPositionOffset = (onBottom ? new Vector2(0f, -20f) : new Vector2(0f, 20f));
		val.parentPositionAnchor = new Vector2(0.5f, 0.5f);
		if (wrapWidth > 0f)
		{
			val.WrapWidth = wrapWidth;
			val.SizingSetting = (ToolTipSizeSetting)0;
		}
		ToolTipScreen.Instance.SetToolTip(val);
		val.SetSimpleTooltip(tooltip);
		return val;
	}

	public static void RemoveSimpleTooltipOnObject(Transform go)
	{
		ToolTip val = default(ToolTip);
		if (!((Object)(object)go == (Object)null) && ((Component)go).gameObject.TryGetComponent<ToolTip>(ref val))
		{
			Object.Destroy((Object)(object)val);
		}
	}

	public static Transform GetShellWithoutFunction(Transform origin, string subCompName = "", string copyName = "copy")
	{
		Transform val = origin.Find(subCompName);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		GameObject val2 = Util.KInstantiateUI(((Component)val).gameObject, (GameObject)null, false);
		val2.SetActive(false);
		Transform transform = val2.transform;
		Object.Destroy((Object)(object)val2);
		GameObject val3 = Util.KInstantiateUI(((Component)transform).gameObject, ((Component)origin).gameObject, true);
		((Object)val3).name = copyName;
		return val3.transform;
	}

	public static Transform TryInsertNamedCopy(Transform parent, string subCompName = "", string copyName = "copy")
	{
		Transform val = parent.Find(subCompName);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		GameObject val2 = Util.KInstantiateUI(((Component)((Component)val).transform).gameObject, ((Component)parent).gameObject, true);
		((Object)val2).name = copyName;
		return val2.transform;
	}

	public static bool FindAndRemove<T>(Transform parent, string subCompName = "")
	{
		object obj = TryFindComponent<T>(parent, subCompName);
		Object val = (Object)((obj is Object) ? obj : null);
		if (val != (Object)null)
		{
			Debug.Log((object)("Removing " + subCompName));
			Object.Destroy(val);
			return true;
		}
		return false;
	}

	public static bool FindAndDisable(Transform parent, string name)
	{
		Transform val = parent.Find(name);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		((Component)val).gameObject.SetActive(false);
		return true;
	}

	public static bool FindAndDestroy(Transform parent, string name, bool force = false)
	{
		Debug.Log((object)("Destroying " + name));
		Transform val = parent.Find(name);
		if ((Object)(object)val == (Object)null)
		{
			SgtLogger.warning("Failure to delete " + name + " in " + ((object)parent).ToString());
			return false;
		}
		if (force)
		{
			Object.DestroyImmediate((Object)(object)((Component)val).gameObject);
		}
		else
		{
			Object.Destroy((Object)(object)((Component)val).gameObject);
		}
		return true;
	}

	public static void ListComponents(GameObject instance)
	{
		Console.WriteLine(instance.GetComponents(typeof(Component)).Count() + " objects found");
		Component[] components = instance.GetComponents(typeof(Object));
		foreach (Component val in components)
		{
			if ((Object)(object)val != (Object)null)
			{
				Console.WriteLine("Type: " + ((object)val).GetType().ToString() + ", Name ->" + ((Object)val).name);
			}
		}
	}

	public static void ListAllChildren(Transform parent, int level = 0, int maxDepth = 10, string PreAmblel = "")
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		if (level >= maxDepth)
		{
			return;
		}
		if (level == 0 && PreAmblel.Length > 0)
		{
			SgtLogger.l(PreAmblel);
		}
		foreach (Transform item in parent)
		{
			Transform val = item;
			Console.WriteLine(string.Concat(string.Concat(Enumerable.Repeat('-', level)), ((Object)val).name));
			ListAllChildren(val, level + 1);
		}
	}

	public static void ListAllChildrenPath(Transform parent, string path = "/", int level = 0, int maxDepth = 10)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (level >= maxDepth)
		{
			return;
		}
		foreach (Transform item in parent)
		{
			Transform val = item;
			string text = string.Concat(path + ((Object)val).name + "/");
			Console.WriteLine(text);
			ListAllChildrenPath(val, text, level + 1);
		}
	}

	public static void ListAllChildrenWithComponents(Transform parent, int level = 0, int maxDepth = 10)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		if (level >= maxDepth)
		{
			return;
		}
		ListComponents(((Component)parent).gameObject);
		foreach (Transform item in parent)
		{
			Transform val = item;
			Console.WriteLine(string.Concat(string.Concat(Enumerable.Repeat('-', level)), ((Object)val).name));
			ListAllChildrenWithComponents(val, level + 1);
		}
	}

	public static void ListChildrenHR(HierarchyReferences parent, int level = 0, int maxDepth = 10)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (level < maxDepth)
		{
			ElementReference[] references = parent.references;
			foreach (ElementReference val in references)
			{
				Console.WriteLine(string.Concat(string.Concat(Enumerable.Repeat('-', level)), val.Name, ", ", ((object)val.behaviour).ToString()));
				ListAllChildren(val.behaviour.transform, level + 1);
			}
		}
	}

	public static GameObject AddClonedSideScreen<T>(string name, string originalName, Type originalType, SidescreenTabTypes targetTab = (SidescreenTabTypes)0)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (GetElements(out var screens, out var tabs))
		{
			GameObject contentBodyForTab = GetContentBodyForTab(targetTab, tabs);
			SideScreenContent original = FindOriginal(originalName, screens);
			SideScreenContent prefab = Copy<T>(original, contentBodyForTab, name, originalType);
			screens.Add(NewSideScreen(name, prefab, targetTab));
			return contentBodyForTab;
		}
		return null;
	}

	public static void AddCustomSideScreen<T>(string name, GameObject prefab, SidescreenTabTypes targetTab = (SidescreenTabTypes)0)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (GetElements(out var screens, out var _))
		{
			Component obj = prefab.AddComponent(typeof(T));
			SideScreenContent prefab2 = (SideScreenContent)(object)((obj is SideScreenContent) ? obj : null);
			screens.Add(NewSideScreen(name, prefab2, targetTab));
		}
		else
		{
			SgtLogger.error("Couldnt add custom sidescreen " + name + ", sidescreen vars not found");
		}
	}

	private unsafe static GameObject GetContentBodyForTab(SidescreenTabTypes targetTab, List<SidescreenTab> tabs)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		foreach (SidescreenTab tab in tabs)
		{
			if (tab.type == targetTab)
			{
				return tab.bodyInstance;
			}
		}
		SgtLogger.logerror(((object)(*(SidescreenTabTypes*)(&targetTab))/*cast due to .constrained prefix*/).ToString() + " not found!");
		return null;
	}

	private static bool GetElements(out List<SideScreenRef> screens, out List<SidescreenTab> tabs)
	{
		Traverse val = Traverse.Create((object)DetailsScreen.Instance);
		screens = val.Field("sideScreens").GetValue<List<SideScreenRef>>();
		tabs = val.Field("sidescreenTabs").GetValue<SidescreenTab[]>().ToList();
		return screens != null && tabs != null;
	}

	private static SideScreenContent FindOriginal(string name, List<SideScreenRef> screens)
	{
		SideScreenContent screenPrefab = screens.Find((SideScreenRef s) => s.name == name).screenPrefab;
		if ((Object)(object)screenPrefab == (Object)null)
		{
			SgtLogger.logwarning("Could not find a sidescreen with the name " + name, "SgtImalasUtils");
		}
		return screenPrefab;
	}

	private static SideScreenContent Copy<T>(SideScreenContent original, GameObject contentBody, string name, Type originalType)
	{
		GameObject gameObject = ((Component)Util.KInstantiateUI<SideScreenContent>(((Component)original).gameObject, contentBody, false)).gameObject;
		Object.Destroy((Object)(object)gameObject.GetComponent(originalType));
		Component obj = gameObject.AddComponent(typeof(T));
		SideScreenContent val = (SideScreenContent)(object)((obj is SideScreenContent) ? obj : null);
		((Object)val).name = name.Trim();
		gameObject.SetActive(false);
		return val;
	}

	private static SideScreenRef NewSideScreen(string name, SideScreenContent prefab, SidescreenTabTypes targetTab)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		return new SideScreenRef
		{
			name = name,
			offset = Vector2.zero,
			screenPrefab = prefab,
			tab = targetTab
		};
	}

	public static Color GetRainbowColorForIndex(int i, bool desaturated = false)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return (!desaturated) ? RainbowColors[i % RainbowColors.Count] : RainbowColorsDesaturated[i % RainbowColorsDesaturated.Count];
	}

	public static string ColorNumber(float number, bool invert = false)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (invert)
		{
			return ColorNumber(number * -1f);
		}
		if (number > 0f)
		{
			return ColorText(number.ToString(), number_green);
		}
		if (number < 0f)
		{
			return ColorText(number.ToString(), number_red);
		}
		return number.ToString();
	}

	public static string EmboldenText(string text)
	{
		return "<b>" + text + "</b>";
	}

	public static string ColorText(string text, string hex)
	{
		hex = hex.Replace("#", string.Empty);
		return "<color=#" + hex + ">" + text + "</color>";
	}

	public static string ColorText(string text, Color color)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return ColorText(text, Util.ToHexString(color));
	}

	public static string RainbowColorText(string text, bool useTrueColors = false)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		char[] array = text.ToCharArray();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(ColorText(array[i].ToString(), useTrueColors ? RainbowColors[i % RainbowColors.Count] : RainbowColorsDesaturated[i % RainbowColorsDesaturated.Count]));
		}
		return stringBuilder.ToString();
	}

	public unsafe static Symbol GetSymbolFromMultiObjectAnim(KAnimFile animFile, string animName = "ui", string symbolName = "")
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		Tuple<KAnimFile, string> key = new Tuple<KAnimFile, string>(animFile, animName);
		if (knownSymbols.ContainsKey(key))
		{
			return knownSymbols[key];
		}
		if ((Object)(object)animFile == (Object)null)
		{
			DebugUtil.LogWarningArgs(new object[2] { animName, "missing Anim File" });
			return null;
		}
		KAnimFileData data = animFile.GetData();
		if (data == null)
		{
			DebugUtil.LogWarningArgs(new object[2] { animName, "KAnimFileData is null" });
			return null;
		}
		if (data.build == null)
		{
			return null;
		}
		Frame val = default(Frame);
		for (int i = 0; i < data.animCount; i++)
		{
			Anim anim = data.GetAnim(i);
			if (anim.name == animName)
			{
				anim.TryGetFrame(data.batchTag, 0, ref val);
			}
		}
		if (((object)(*(Frame*)(&val))/*cast due to .constrained prefix*/).Equals((object?)null))
		{
			DebugUtil.LogWarningArgs(new object[1] { $"missing '{animName}' anim in '{animFile}'" });
			return null;
		}
		if (data.elementCount == 0)
		{
			return null;
		}
		FrameElement val2 = default(FrameElement);
		if (string.IsNullOrEmpty(symbolName))
		{
			symbolName = animName;
		}
		KAnimHashedString val3 = default(KAnimHashedString);
		((KAnimHashedString)(ref val3))._002Ector(symbolName);
		Symbol symbol = data.build.GetSymbol(val3);
		if (symbol == null)
		{
			DebugUtil.LogWarningArgs(new object[5]
			{
				((Object)animFile).name,
				animName,
				"placeSymbol [",
				val2.symbol,
				"] is missing"
			});
			return null;
		}
		knownSymbols[key] = symbol;
		return symbol;
	}
}
