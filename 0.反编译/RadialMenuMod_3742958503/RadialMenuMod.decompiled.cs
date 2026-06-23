using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using HarmonyLib;
using KMod;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]
[assembly: AssemblyVersion("0.0.0.0")]
[module: RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Embedded]
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
public class RadialMenuConfig
{
	public bool HideVanillaHud { get; set; } = false;
}
public static class SettingsManager
{
	private static string ConfigPath;

	public static RadialMenuConfig Config { get; private set; } = new RadialMenuConfig();

	public static void Load(string modPath)
	{
		try
		{
			ConfigPath = Path.Combine(modPath, "config.json");
			if (File.Exists(ConfigPath))
			{
				string text = File.ReadAllText(ConfigPath);
				Config = JsonConvert.DeserializeObject<RadialMenuConfig>(text) ?? new RadialMenuConfig();
				Debug.Log((object)("RadialMenuMod: Loaded settings. HideVanillaHud = " + Config.HideVanillaHud));
			}
			else
			{
				Save();
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("RadialMenuMod: Failed to load config, using defaults. " + ex.Message));
		}
	}

	public static void Save()
	{
		try
		{
			if (ConfigPath != null)
			{
				string contents = JsonConvert.SerializeObject((object)Config, (Formatting)1);
				File.WriteAllText(ConfigPath, contents);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("RadialMenuMod: Failed to save config. " + ex.Message));
		}
	}

	public static void ApplyHudVisibility()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		bool hideVanillaHud = Config.HideVanillaHud;
		bool visible = !hideVanillaHud;
		if ((Object)(object)OverlayMenu.Instance != (Object)null)
		{
			SetScreenVisible((MonoBehaviour)(object)OverlayMenu.Instance, visible);
		}
		if ((Object)(object)ToolMenu.Instance != (Object)null)
		{
			foreach (Transform item in ((KMonoBehaviour)ToolMenu.Instance).transform)
			{
				Transform val = item;
				string text = ((Object)val).name.ToLower();
				if (text.Contains("priority") || text.Contains("parameter") || text.Contains("filter"))
				{
					SetObjectVisible(((Component)val).gameObject, visible: true);
				}
				else
				{
					SetObjectVisible(((Component)val).gameObject, visible);
				}
			}
		}
		if (!((Object)(object)PlanScreen.Instance != (Object)null))
		{
			return;
		}
		foreach (Transform item2 in ((KMonoBehaviour)PlanScreen.Instance).transform)
		{
			Transform val2 = item2;
			string text2 = ((Object)val2).name.ToLower();
			if (text2.Contains("info") || text2.Contains("recipe") || text2.Contains("product") || text2.Contains("material"))
			{
				SetObjectVisible(((Component)val2).gameObject, visible: true);
			}
			else
			{
				SetObjectVisible(((Component)val2).gameObject, visible);
			}
		}
	}

	private static void SetObjectVisible(GameObject go, bool visible)
	{
		if ((Object)(object)go == (Object)null)
		{
			return;
		}
		try
		{
			CanvasGroup val = go.GetComponent<CanvasGroup>();
			if ((Object)(object)val == (Object)null)
			{
				val = go.AddComponent<CanvasGroup>();
			}
			val.alpha = (visible ? 1f : 0f);
			val.blocksRaycasts = visible;
			val.interactable = visible;
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("RadialMenuMod: Error setting visibility on GameObject " + ((Object)go).name + ": " + ex.Message));
		}
	}

	private static void SetScreenVisible(MonoBehaviour screen, bool visible)
	{
		if ((Object)(object)screen == (Object)null || (Object)(object)((Component)screen).gameObject == (Object)null)
		{
			return;
		}
		try
		{
			CanvasGroup val = ((Component)screen).GetComponent<CanvasGroup>();
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)screen).gameObject.AddComponent<CanvasGroup>();
			}
			val.alpha = (visible ? 1f : 0f);
			val.blocksRaycasts = visible;
			val.interactable = visible;
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("RadialMenuMod: Error setting visibility on " + ((Object)screen).name + ": " + ex.Message));
		}
	}
}
public class ModLoad : UserMod2
{
	public override void OnLoad(Harmony harmony)
	{
		((UserMod2)this).OnLoad(harmony);
		string text = null;
		if (((UserMod2)this).mod != null)
		{
			text = ((UserMod2)this).mod.ContentPath;
		}
		if (string.IsNullOrEmpty(text))
		{
			text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}
		SettingsManager.Load(text);
		Strings.Add(new string[2] { "STRINGS.INPUT_BINDINGS.RADIALMENU.NAME", "Radial Menu" });
		Strings.Add(new string[2] { "STRINGS.INPUT_BINDINGS.RADIALMENU.800", "Open Radial Menu" });
		harmony.PatchAll(Assembly.GetExecutingAssembly());
		Debug.Log((object)"RadialMenuMod: Loaded and Harmony patches applied successfully!");
	}
}
[HarmonyPatch(typeof(ToolMenu), "OnSpawn")]
public static class ToolMenu_OnSpawn_Patch
{
	public static void Postfix(ToolMenu __instance)
	{
		if ((Object)(object)((Component)__instance).gameObject.GetComponent<RadialMenuController>() == (Object)null)
		{
			((Component)__instance).gameObject.AddComponent<RadialMenuController>();
			Debug.Log((object)"RadialMenuMod: Instantiated RadialMenuController on ToolMenu!");
		}
		SettingsManager.ApplyHudVisibility();
	}
}
[HarmonyPatch(typeof(OverlayMenu), "OnSpawn")]
public static class OverlayMenu_OnSpawn_Patch
{
	public static void Postfix()
	{
		SettingsManager.ApplyHudVisibility();
	}
}
[HarmonyPatch(typeof(PlanScreen), "OnSpawn")]
public static class PlanScreen_OnSpawn_Patch
{
	public static void Postfix(PlanScreen __instance)
	{
		SettingsManager.ApplyHudVisibility();
		try
		{
			FieldInfo field = typeof(PlanScreen).GetField("toggleEntries", BindingFlags.Instance | BindingFlags.NonPublic);
			if (!(field != null) || !(field.GetValue(__instance) is IList list))
			{
				return;
			}
			using StreamWriter streamWriter = new StreamWriter("output_all_toggleinfos.txt");
			foreach (object item in list)
			{
				object arg = item.GetType().GetField("planCategory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(item);
				streamWriter.WriteLine($"Category: {arg}");
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("RadialMenuMod: Error printing categories: " + ex));
		}
	}
}
public static class RadialMenuGuard
{
	public static bool CanOpenWheel()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PlayerController.Instance == (Object)null || !(PlayerController.Instance.ActiveTool is SelectTool))
		{
			return false;
		}
		if ((Object)(object)OverlayScreen.Instance != (Object)null && OverlayScreen.Instance.GetMode() != None.ID)
		{
			return false;
		}
		if ((Object)(object)BuildMenu.Instance != (Object)null && ((KScreen)BuildMenu.Instance).IsScreenActive())
		{
			return false;
		}
		Type type = Type.GetType("DetailsScreen, Assembly-CSharp");
		if (type != null)
		{
			FieldInfo field = type.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
			if (field != null)
			{
				object value = field.GetValue(null);
				MonoBehaviour val = (MonoBehaviour)((value is MonoBehaviour) ? value : null);
				if ((Object)(object)val != (Object)null && ((Component)val).gameObject.activeInHierarchy)
				{
					return false;
				}
			}
		}
		Type type2 = Type.GetType("PlanScreen, Assembly-CSharp");
		if (type2 != null)
		{
			FieldInfo field2 = type2.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
			if (field2 != null)
			{
				object value2 = field2.GetValue(null);
				if (value2 != null)
				{
					PropertyInfo property = type2.GetProperty("ActiveCategoryToggleInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (property != null)
					{
						object value3 = property.GetValue(value2);
						if (value3 != null)
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}
}
[HarmonyPatch(typeof(InterfaceTool), "OnLeftClickDown")]
public static class InterfaceTool_OnLeftClickDown_Patch
{
	public static bool Prefix(InterfaceTool __instance, Vector3 cursor_pos)
	{
		if ((Object)(object)RadialMenuController.Instance != (Object)null)
		{
			if (RadialMenuController.Instance.IsMenuOpen)
			{
				return false;
			}
			if (RadialMenuController.Instance.ShouldBlockClickThisFrame())
			{
				return false;
			}
		}
		return true;
	}
}
[HarmonyPatch(typeof(InterfaceTool), "OnLeftClickUp")]
public static class InterfaceTool_OnLeftClickUp_Patch
{
	public static bool Prefix(InterfaceTool __instance, Vector3 cursor_pos)
	{
		if ((Object)(object)RadialMenuController.Instance != (Object)null)
		{
			if (RadialMenuController.Instance.IsMenuOpen)
			{
				return false;
			}
			if (RadialMenuController.Instance.ShouldBlockClickThisFrame())
			{
				return false;
			}
		}
		return true;
	}
}
[HarmonyPatch(typeof(InterfaceTool), "OnRightClickDown")]
public static class InterfaceTool_OnRightClickDown_Patch
{
	public static bool Prefix(InterfaceTool __instance, Vector3 cursor_pos, KButtonEvent e)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Invalid comparison between Unknown and I4
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)RadialMenuController.Instance != (Object)null)
		{
			if (RadialMenuController.Instance.IsMenuOpen)
			{
				RadialMenuController.Instance.OnRightClickDown();
				e.TryConsume((Action)800);
				return false;
			}
			bool flag = false;
			try
			{
				BindingEntry val = GameInputMapping.FindEntry((Action)800);
				if ((int)val.mKeyCode == 324 && (int)val.mModifier == 0)
				{
					flag = true;
				}
			}
			catch
			{
			}
			if (flag && RadialMenuGuard.CanOpenWheel())
			{
				RadialMenuController.Instance.ToggleMenu(cursor_pos);
				e.TryConsume((Action)800);
				return false;
			}
		}
		return true;
	}
}
[HarmonyPatch(typeof(InterfaceTool), "OnRightClickUp")]
public static class InterfaceTool_OnRightClickUp_Patch
{
	public static bool Prefix(InterfaceTool __instance, Vector3 cursor_pos)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		if ((Object)(object)RadialMenuController.Instance != (Object)null)
		{
			if (RadialMenuController.Instance.IsMenuOpen)
			{
				return false;
			}
			bool flag = false;
			try
			{
				BindingEntry val = GameInputMapping.FindEntry((Action)800);
				if ((int)val.mKeyCode == 324 && (int)val.mModifier == 0)
				{
					flag = true;
				}
			}
			catch
			{
			}
			if (flag && RadialMenuGuard.CanOpenWheel())
			{
				return false;
			}
		}
		return true;
	}
}
[HarmonyPatch(typeof(InterfaceTool), "OnKeyDown")]
public static class InterfaceTool_OnKeyDown_Patch
{
	public static bool Prefix(InterfaceTool __instance, KButtonEvent e)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (((KInputEvent)e).Consumed)
		{
			return true;
		}
		if ((Object)(object)RadialMenuController.Instance != (Object)null && e.IsAction((Action)800))
		{
			if (RadialMenuController.Instance.IsMenuOpen)
			{
				RadialMenuController.Instance.OnRightClickDown();
				e.TryConsume((Action)800);
				return false;
			}
			if (RadialMenuGuard.CanOpenWheel())
			{
				Vector3 cursorPos = PlayerController.GetCursorPos(KInputManager.GetMousePos());
				RadialMenuController.Instance.ToggleMenu(cursorPos);
				e.TryConsume((Action)800);
				return false;
			}
		}
		return true;
	}
}
[HarmonyPatch(typeof(InterfaceTool), "OnKeyUp")]
public static class InterfaceTool_OnKeyUp_Patch
{
	public static bool Prefix(InterfaceTool __instance, KButtonEvent e)
	{
		if ((Object)(object)RadialMenuController.Instance != (Object)null && e.IsAction((Action)800))
		{
			e.TryConsume((Action)800);
			return false;
		}
		return true;
	}
}
[HarmonyPatch(typeof(GameInputMapping), "SetDefaultKeyBindings")]
public static class GameInputMapping_SetDefaultKeyBindings_Patch
{
	public static void Prefix(ref BindingEntry[] default_keybindings)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		if (default_keybindings != null)
		{
			for (int i = 0; i < default_keybindings.Length; i++)
			{
				if ((int)default_keybindings[i].mAction == 800)
				{
					return;
				}
			}
		}
		List<BindingEntry> list = new List<BindingEntry>(default_keybindings);
		BindingEntry item = default(BindingEntry);
		((BindingEntry)(ref item))..ctor("RadialMenu", (GamepadButton)16, (KKeyCode)324, (Modifier)0, (Action)800, true, true);
		list.Add(item);
		default_keybindings = list.ToArray();
	}
}
[HarmonyPatch(/*Could not decode attribute arguments.*/)]
public static class KInputController_Constructor_Patch
{
	public static void Postfix(KInputController __instance, ref bool[] ___mActionState)
	{
		___mActionState = new bool[1000];
	}
}
[HarmonyPatch(/*Could not decode attribute arguments.*/)]
public static class KeyDef_Constructor_Patch
{
	public static void Postfix(KeyDef __instance)
	{
		__instance.mActionFlags = new bool[1000];
	}
}
[HarmonyPatch(typeof(KButtonEvent), "IsAction")]
public static class KButtonEvent_IsAction_Patch
{
	public static bool Prefix(KButtonEvent __instance, Action action, ref bool __result, bool[] ___mIsAction, Action ___mAction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if ((int)action >= 280)
		{
			if (___mIsAction != null)
			{
				if ((int)action < ___mIsAction.Length)
				{
					__result = ___mIsAction[action];
				}
				else
				{
					__result = false;
				}
			}
			else
			{
				__result = ___mAction == action;
			}
			return false;
		}
		return true;
	}
}
[HarmonyPatch(typeof(ProductInfoScreen), "CheckMouseOver")]
public static class ProductInfoScreen_CheckMouseOver_Patch
{
	public static bool Prefix(ProductInfoScreen __instance, PointerEventData data)
	{
		if ((Object)(object)BuildingGroupScreen.Instance == (Object)null)
		{
			bool flag = ((KScreen)__instance).GetMouseOver;
			if (!flag && (Object)(object)PlanScreen.Instance != (Object)null && ((KScreen)PlanScreen.Instance).IsScreenActive() && ((KScreen)PlanScreen.Instance).GetMouseOver)
			{
				flag = true;
			}
			if (!flag && (Object)(object)BuildMenu.Instance != (Object)null && ((KScreen)BuildMenu.Instance).IsScreenActive() && ((KScreen)BuildMenu.Instance).GetMouseOver)
			{
				flag = true;
			}
			MethodInfo method = typeof(ProductInfoScreen).GetMethod("ToggleExpandedInfo", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(__instance, new object[1] { flag });
			}
			return false;
		}
		return true;
	}
}
[HarmonyPatch(typeof(PlanScreen), "OnRecipeElementsFullySelected")]
public static class PlanScreen_OnRecipeElementsFullySelected_Patch
{
	public static bool Prefix(PlanScreen __instance)
	{
		object obj = typeof(KIconToggleMenu).GetField("currentlySelectedToggle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(__instance);
		KToggle val = (KToggle)((obj is KToggle) ? obj : null);
		if ((Object)(object)val != (Object)null && ((Object)val).name == "RadialMenuDummyToggle")
		{
			ProductInfoScreen productInfoScreen = __instance.ProductInfoScreen;
			if ((Object)(object)productInfoScreen != (Object)null && (Object)(object)productInfoScreen.currentDef != (Object)null)
			{
				BuildingDef currentDef = productInfoScreen.currentDef;
				IList<Tag> list = (((Object)(object)productInfoScreen.materialSelectionPanel != (Object)null) ? productInfoScreen.materialSelectionPanel.GetSelectedElementAsList : null);
				string text = (((Object)(object)productInfoScreen.FacadeSelectionPanel != (Object)null) ? productInfoScreen.FacadeSelectionPanel.SelectedFacade : null);
				if (currentDef.isKAnimTile && currentDef.isUtility)
				{
					Type type = Type.GetType("Wire, Assembly-CSharp");
					if (type != null && (Object)(object)currentDef.BuildingComplete.GetComponent(type) != (Object)null)
					{
						Type type2 = Type.GetType("WireBuildTool, Assembly-CSharp");
						object obj2 = (type2?.GetField("Instance", BindingFlags.Static | BindingFlags.Public))?.GetValue(null);
						(type2?.GetMethod("Activate", new Type[3]
						{
							typeof(BuildingDef),
							typeof(IList<Tag>),
							typeof(string)
						}))?.Invoke(obj2, new object[3] { currentDef, list, text });
					}
					else
					{
						Type type3 = Type.GetType("UtilityBuildTool, Assembly-CSharp");
						object obj3 = (type3?.GetField("Instance", BindingFlags.Static | BindingFlags.Public))?.GetValue(null);
						(type3?.GetMethod("Activate", new Type[3]
						{
							typeof(BuildingDef),
							typeof(IList<Tag>),
							typeof(string)
						}))?.Invoke(obj3, new object[3] { currentDef, list, text });
					}
				}
				else
				{
					Type type4 = Type.GetType("BuildTool, Assembly-CSharp");
					object obj4 = (type4?.GetField("Instance", BindingFlags.Static | BindingFlags.Public))?.GetValue(null);
					(type4?.GetMethod("Activate", new Type[3]
					{
						typeof(BuildingDef),
						typeof(IList<Tag>),
						typeof(string)
					}))?.Invoke(obj4, new object[3] { currentDef, list, text });
				}
			}
			return false;
		}
		return true;
	}
}
[HarmonyPatch(typeof(PlanCategoryNotifications), "ToggleAttention")]
public static class PlanCategoryNotifications_ToggleAttention_Patch
{
	public static bool Prefix(PlanCategoryNotifications __instance, bool active)
	{
		if ((Object)(object)__instance == (Object)null)
		{
			return false;
		}
		try
		{
			FieldInfo field = typeof(PlanCategoryNotifications).GetField("AttentionImage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				object value = field.GetValue(__instance);
				Image val = (Image)((value is Image) ? value : null);
				if ((Object)(object)val == (Object)null || (Object)(object)((Component)val).gameObject == (Object)null)
				{
					return false;
				}
			}
		}
		catch
		{
			return false;
		}
		return true;
	}
}
[HarmonyPatch(typeof(InputBindingsScreen), "Update")]
public static class InputBindingsScreen_Update_Patch
{
	public static bool Prefix(InputBindingsScreen __instance, ref bool ___waitingForKeyPress, ref Action ___actionToRebind, ref KButton ___activeButton)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Invalid comparison between Unknown and I4
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		if (___waitingForKeyPress)
		{
			Modifier val = (Modifier)0;
			val = (Modifier)(val | ((Input.GetKey((KeyCode)308) || Input.GetKey((KeyCode)307)) ? 1 : 0));
			val = (Modifier)(val | ((Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305)) ? 2 : 0));
			val = (Modifier)(val | ((Input.GetKey((KeyCode)304) || Input.GetKey((KeyCode)303)) ? 4 : 0));
			val = (Modifier)(val | (Input.GetKey((KeyCode)301) ? 8 : 0));
			val = (Modifier)(val | (Input.GetKey((KeyCode)96) ? 16 : 0));
			if (Input.GetKeyDown((KeyCode)27))
			{
				for (int i = 0; i < GameInputMapping.KeyBindings.Length; i++)
				{
					BindingEntry val2 = GameInputMapping.KeyBindings[i];
					if ((int)val2.mAction == (int)___actionToRebind)
					{
						val2.mKeyCode = (KKeyCode)0;
						val2.mModifier = (Modifier)0;
						GameInputMapping.KeyBindings[i] = val2;
						break;
					}
				}
				Global.GetInputManager().RebindControls();
				___waitingForKeyPress = false;
				___actionToRebind = (Action)280;
				___activeButton = null;
				typeof(InputBindingsScreen).GetMethod("BuildDisplay", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(__instance, null);
				return false;
			}
			for (int j = 1; j <= 6; j++)
			{
				if (Input.GetMouseButtonDown(j))
				{
					KKeyCode val3 = (KKeyCode)(323 + j);
					typeof(InputBindingsScreen).GetMethod("Bind", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(__instance, new object[2] { val3, val });
					return false;
				}
			}
		}
		return true;
	}
}
public class RadialMenuController : MonoBehaviour
{
	private enum WheelState
	{
		Categories,
		Buildings
	}

	private class RadialOption
	{
		public string Name;

		public Action TriggerAction;

		public RadialOption(string name, Action triggerAction)
		{
			Name = name;
			TriggerAction = triggerAction;
		}
	}

	private GameObject canvasGO;

	private RectTransform panelRect;

	private Text centerText;

	private Image backgroundCircle;

	private Image centerHubCircle;

	private Vector3 startMousePos;

	private bool isMenuOpen = false;

	private WheelState currentState = WheelState.Categories;

	private string[] mainCategories = new string[3] { "Tools", "Overlays", "Build Menus" };

	private int hoveredCategoryIndex = 0;

	private int hoveredOptionIndex = -1;

	private int hoveredBuildingIndex = -1;

	private float maxDistance = 340f;

	private int blockClickFrame = -1;

	private int openFrame = -1;

	private List<RadialOption> toolOptions;

	private List<RadialOption> overlayOptions;

	private string[] buildCategories = new string[14]
	{
		"Base", "Oxygen", "Power", "Food", "Plumbing", "Ventilation", "Refine", "Medical", "Furniture", "Equipment",
		"Utilities", "Automation", "SolidConveyor", "Rockets"
	};

	private List<RadialOption> buildCategoryOptions;

	private List<BuildingDef> currentBuildingDefs = new List<BuildingDef>();

	private List<int> currentBuildingStates = new List<int>();

	private List<Text> outerTexts = new List<Text>();

	private List<Image> outerButtonBGs = new List<Image>();

	private List<Image> outerIcons = new List<Image>();

	private List<Image> dividerLines = new List<Image>();

	private List<ToolTip> outerTooltips = new List<ToolTip>();

	private Sprite circleSprite;

	private Sprite buttonSprite;

	private Sprite hubSprite;

	private Font gameFont;

	private GameObject dummyToggleGO;

	private float scaleProgress = 0f;

	private float mouseAngle = 0f;

	private float mouseDistance = 0f;

	private Image pointerLine;

	private static MethodInfo planScreenIsDefResearchedMethod;

	private static MethodInfo planScreenIsDefBuildableMethod;

	private static FieldInfo planScreenToggleEntriesField;

	private static FieldInfo planScreenCategoryInteractiveField;

	private static FieldInfo planScreenTagCategoryMapField;

	public static RadialMenuController Instance { get; private set; }

	public bool IsMenuOpen => isMenuOpen;

	public bool ShouldBlockClickThisFrame()
	{
		return Time.frameCount == blockClickFrame;
	}

	private void Awake()
	{
		Instance = this;
		SetupOptions();
		CreateSprites();
		CreateUI();
		HideMenu();
	}

	private void SetupOptions()
	{
		toolOptions = new List<RadialOption>
		{
			new RadialOption("Dig", delegate
			{
				ActivateTool("DigTool");
			}),
			new RadialOption("Mop", delegate
			{
				ActivateTool("MopTool");
			}),
			new RadialOption("Sweep", delegate
			{
				ActivateTool("ClearTool");
			}),
			new RadialOption("Deconstruct", delegate
			{
				ActivateTool("DeconstructTool");
			}),
			new RadialOption("Prioritize", delegate
			{
				ActivateTool("PrioritizeTool");
			}),
			new RadialOption("Attack", delegate
			{
				ActivateTool("AttackTool");
			}),
			new RadialOption("Disinfect", delegate
			{
				ActivateTool("DisinfectTool");
			}),
			new RadialOption("Harvest", delegate
			{
				ActivateTool("HarvestTool");
			}),
			new RadialOption("Empty Pipe", delegate
			{
				ActivateTool("EmptyPipeTool");
			}),
			new RadialOption("Disconnect", delegate
			{
				ActivateTool("DisconnectTool");
			}),
			new RadialOption("Wrangle", delegate
			{
				ActivateTool("CaptureTool");
			}),
			new RadialOption("Cancel", delegate
			{
				ActivateTool("CancelTool");
			})
		};
		overlayOptions = new List<RadialOption>
		{
			new RadialOption("Oxygen", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(Oxygen.ID);
			}),
			new RadialOption("Power", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(Power.ID);
			}),
			new RadialOption("Temperature", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(Temperature.ID);
			}),
			new RadialOption("Gas Pipe", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(GasConduits.ID);
			}),
			new RadialOption("Liquid Pipe", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(LiquidConduits.ID);
			}),
			new RadialOption("Automation", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(Logic.ID);
			}),
			new RadialOption("Shipping", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(SolidConveyor.ID);
			}),
			new RadialOption("Decor", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(Decor.ID);
			}),
			new RadialOption("Light", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(Light.ID);
			}),
			new RadialOption("Germs", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(Disease.ID);
			}),
			new RadialOption("Agriculture", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(Crop.ID);
			}),
			new RadialOption("Rooms", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(Rooms.ID);
			}),
			new RadialOption("Suits", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(Suit.ID);
			}),
			new RadialOption("Materials", delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				ToggleOverlay(TileMode.ID);
			})
		};
		buildCategoryOptions = new List<RadialOption>
		{
			new RadialOption("Base", delegate
			{
				EnterBuildingSelection("Base");
			}),
			new RadialOption("Oxygen", delegate
			{
				EnterBuildingSelection("Oxygen");
			}),
			new RadialOption("Power", delegate
			{
				EnterBuildingSelection("Power");
			}),
			new RadialOption("Food", delegate
			{
				EnterBuildingSelection("Food");
			}),
			new RadialOption("Plumbing", delegate
			{
				EnterBuildingSelection("Plumbing");
			}),
			new RadialOption("Ventilation", delegate
			{
				EnterBuildingSelection("HVAC");
			}),
			new RadialOption("Refinement", delegate
			{
				EnterBuildingSelection("Refining");
			}),
			new RadialOption("Medicine", delegate
			{
				EnterBuildingSelection("Medical");
			}),
			new RadialOption("Furniture", delegate
			{
				EnterBuildingSelection("Furniture");
			}),
			new RadialOption("Equipment", delegate
			{
				EnterBuildingSelection("Equipment");
			}),
			new RadialOption("Utilities", delegate
			{
				EnterBuildingSelection("Utilities");
			}),
			new RadialOption("Automation", delegate
			{
				EnterBuildingSelection("Automation");
			}),
			new RadialOption("Shipping", delegate
			{
				EnterBuildingSelection("Conveyance");
			}),
			new RadialOption("Rocketry", delegate
			{
				EnterBuildingSelection("Rockets");
			})
		};
	}

	private void CreateSprites()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(256, 256);
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 256; j++)
			{
				float num = (float)i - 128f;
				float num2 = (float)j - 128f;
				float num3 = Mathf.Sqrt(num2 * num2 + num * num);
				if (num3 <= 128f)
				{
					float num4 = 0.88f;
					if (num3 > 120f)
					{
						num4 *= (128f - num3) / 8f;
					}
					val.SetPixel(j, i, new Color(0.03f, 0.04f, 0.05f, num4));
				}
				else
				{
					val.SetPixel(j, i, Color.clear);
				}
			}
		}
		val.Apply();
		circleSprite = Sprite.Create(val, new Rect(0f, 0f, 256f, 256f), new Vector2(0.5f, 0.5f));
		int num5 = 128;
		int num6 = 128;
		Texture2D val2 = new Texture2D(num5, num6);
		float num7 = 16f;
		for (int k = 0; k < num6; k++)
		{
			for (int l = 0; l < num5; l++)
			{
				float num8 = 0f;
				if ((float)l < num7)
				{
					num8 = num7 - (float)l;
				}
				else if ((float)l > (float)num5 - num7)
				{
					num8 = (float)l - ((float)num5 - num7);
				}
				float num9 = 0f;
				if ((float)k < num7)
				{
					num9 = num7 - (float)k;
				}
				else if ((float)k > (float)num6 - num7)
				{
					num9 = (float)k - ((float)num6 - num7);
				}
				float num10 = Mathf.Sqrt(num8 * num8 + num9 * num9);
				if (num10 > num7)
				{
					val2.SetPixel(l, k, Color.clear);
				}
				else if (num10 > num7 - 2.5f)
				{
					val2.SetPixel(l, k, new Color(1f, 1f, 1f, 0.85f));
				}
				else
				{
					val2.SetPixel(l, k, new Color(0.12f, 0.15f, 0.18f, 0.75f));
				}
			}
		}
		val2.Apply();
		buttonSprite = Sprite.Create(val2, new Rect(0f, 0f, (float)num5, (float)num6), new Vector2(0.5f, 0.5f));
		int num11 = 128;
		Texture2D val3 = new Texture2D(num11, num11);
		float num12 = (float)num11 / 2f;
		for (int m = 0; m < num11; m++)
		{
			for (int n = 0; n < num11; n++)
			{
				float num13 = (float)m - num12;
				float num14 = (float)n - num12;
				float num15 = Mathf.Sqrt(num14 * num14 + num13 * num13);
				if (num15 <= num12)
				{
					if (num15 > num12 - 3f)
					{
						val3.SetPixel(n, m, new Color(1f, 1f, 1f, 0.8f));
					}
					else
					{
						val3.SetPixel(n, m, new Color(0.12f, 0.15f, 0.18f, 0.95f));
					}
				}
				else
				{
					val3.SetPixel(n, m, Color.clear);
				}
			}
		}
		val3.Apply();
		hubSprite = Sprite.Create(val3, new Rect(0f, 0f, (float)num11, (float)num11), new Vector2(0.5f, 0.5f));
	}

	private void CreateUI()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Expected O, but got Unknown
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Expected O, but got Unknown
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Expected O, but got Unknown
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Expected O, but got Unknown
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0513: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Expected O, but got Unknown
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_058d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		gameFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
		canvasGO = new GameObject("RadialMenuCanvas");
		Canvas val = canvasGO.AddComponent<Canvas>();
		val.renderMode = (RenderMode)0;
		val.sortingOrder = 12000;
		canvasGO.AddComponent<CanvasScaler>();
		canvasGO.AddComponent<GraphicRaycaster>();
		GameObject val2 = new GameObject("RadialPanel");
		val2.transform.SetParent(canvasGO.transform, false);
		panelRect = val2.AddComponent<RectTransform>();
		panelRect.sizeDelta = new Vector2(620f, 620f);
		backgroundCircle = val2.AddComponent<Image>();
		backgroundCircle.sprite = circleSprite;
		((Graphic)backgroundCircle).color = Color.white;
		GameObject val3 = new GameObject("PointerLine");
		val3.transform.SetParent(val2.transform, false);
		pointerLine = val3.AddComponent<Image>();
		pointerLine.sprite = null;
		((Graphic)pointerLine).color = new Color(0.2f, 0.75f, 1f, 0f);
		RectTransform val4 = Util.rectTransform((Component)(object)pointerLine);
		val4.anchorMin = new Vector2(0.5f, 0.5f);
		val4.anchorMax = new Vector2(0.5f, 0.5f);
		val4.pivot = new Vector2(0.5f, 0f);
		val4.sizeDelta = new Vector2(3f, 180f);
		val4.anchoredPosition = Vector2.zero;
		val3.SetActive(true);
		GameObject val5 = new GameObject("RadialCenterHub");
		val5.transform.SetParent(val2.transform, false);
		centerHubCircle = val5.AddComponent<Image>();
		centerHubCircle.sprite = hubSprite;
		((Graphic)centerHubCircle).color = new Color(0.2f, 0.75f, 1f);
		RectTransform val6 = Util.rectTransform((Component)(object)centerHubCircle);
		val6.sizeDelta = new Vector2(140f, 140f);
		val6.anchoredPosition = Vector2.zero;
		GameObject val7 = new GameObject("CenterText");
		val7.transform.SetParent(val5.transform, false);
		centerText = val7.AddComponent<Text>();
		centerText.font = gameFont;
		centerText.fontSize = 20;
		centerText.fontStyle = (FontStyle)1;
		centerText.alignment = (TextAnchor)4;
		((Graphic)centerText).color = Color.white;
		RectTransform component = ((Component)centerText).GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(110f, 80f);
		component.anchoredPosition = Vector2.zero;
		for (int i = 0; i < 32; i++)
		{
			GameObject val8 = new GameObject("ButtonBG_" + i);
			val8.transform.SetParent(val2.transform, false);
			Image val9 = val8.AddComponent<Image>();
			val9.sprite = buttonSprite;
			((Graphic)val9).color = new Color(1f, 1f, 1f, 0.15f);
			RectTransform val10 = Util.rectTransform((Component)(object)val9);
			val10.sizeDelta = new Vector2(88f, 88f);
			outerButtonBGs.Add(val9);
			ToolTip item = val8.AddComponent<ToolTip>();
			outerTooltips.Add(item);
			GameObject val11 = new GameObject("ButtonIcon_" + i);
			val11.transform.SetParent(val8.transform, false);
			Image val12 = val11.AddComponent<Image>();
			val12.preserveAspect = true;
			RectTransform val13 = Util.rectTransform((Component)(object)val12);
			val13.sizeDelta = new Vector2(38f, 38f);
			val13.anchoredPosition = new Vector2(0f, 16f);
			outerIcons.Add(val12);
			GameObject val14 = new GameObject("ButtonText_" + i);
			val14.transform.SetParent(val8.transform, false);
			Text val15 = val14.AddComponent<Text>();
			val15.font = gameFont;
			val15.fontSize = 11;
			val15.fontStyle = (FontStyle)1;
			val15.alignment = (TextAnchor)4;
			val15.horizontalOverflow = (HorizontalWrapMode)0;
			val15.verticalOverflow = (VerticalWrapMode)1;
			((Graphic)val15).color = Color.white;
			RectTransform component2 = ((Component)val15).GetComponent<RectTransform>();
			component2.sizeDelta = new Vector2(80f, 32f);
			component2.anchoredPosition = new Vector2(0f, -22f);
			outerTexts.Add(val15);
			val8.SetActive(false);
		}
		for (int j = 0; j < 32; j++)
		{
			GameObject val16 = new GameObject("DividerLine_" + j);
			val16.transform.SetParent(val2.transform, false);
			Image val17 = val16.AddComponent<Image>();
			val17.sprite = null;
			((Graphic)val17).color = new Color(1f, 1f, 1f, 0.15f);
			RectTransform val18 = Util.rectTransform((Component)(object)val17);
			val18.anchorMin = new Vector2(0.5f, 0.5f);
			val18.anchorMax = new Vector2(0.5f, 0.5f);
			val18.pivot = new Vector2(0.5f, 0f);
			val18.sizeDelta = new Vector2(1.5f, 150f);
			val18.anchoredPosition = Vector2.zero;
			dividerLines.Add(val17);
			val16.SetActive(false);
		}
	}

	private GameObject GetDummyToggleGO()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		if ((Object)(object)dummyToggleGO == (Object)null)
		{
			dummyToggleGO = new GameObject("RadialMenuDummyToggle");
			dummyToggleGO.AddComponent<KToggle>();
			Object.DontDestroyOnLoad((Object)(object)dummyToggleGO);
		}
		return dummyToggleGO;
	}

	public void ToggleMenu(Vector3 cursor_pos)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (isMenuOpen)
		{
			HideMenu();
		}
		else
		{
			ShowMenu(cursor_pos);
		}
	}

	private void EnterBuildingSelection(string category)
	{
		LoadBuildingsForCategory(category);
		if (currentBuildingDefs.Count > 0)
		{
			currentState = WheelState.Buildings;
			hoveredBuildingIndex = -1;
			UpdateUI();
		}
		else
		{
			OpenBuildCategory(category);
			HideMenu();
		}
	}

	public void OnLeftClickDown()
	{
		if (!isMenuOpen)
		{
			return;
		}
		if (currentState == WheelState.Buildings)
		{
			if (hoveredBuildingIndex >= 0 && hoveredBuildingIndex < currentBuildingDefs.Count)
			{
				SelectBuildingDef(currentBuildingDefs[hoveredBuildingIndex]);
			}
			HideMenu();
			return;
		}
		List<RadialOption> currentOptionsList = GetCurrentOptionsList();
		if (hoveredOptionIndex >= 0 && hoveredOptionIndex < currentOptionsList.Count)
		{
			currentOptionsList[hoveredOptionIndex].TriggerAction();
			if (hoveredCategoryIndex == 0)
			{
				HideMenu();
			}
		}
		else
		{
			HideMenu();
		}
	}

	public void OnRightClickDown()
	{
		if (isMenuOpen)
		{
			if (currentState == WheelState.Buildings)
			{
				currentState = WheelState.Categories;
				hoveredBuildingIndex = -1;
				UpdateUI();
			}
			else
			{
				HideMenu();
			}
		}
	}

	private List<RadialOption> GetCurrentOptionsList()
	{
		if (hoveredCategoryIndex == 0)
		{
			return toolOptions;
		}
		if (hoveredCategoryIndex == 1)
		{
			return overlayOptions;
		}
		return buildCategoryOptions;
	}

	private void Update()
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		if ((Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305)) && Input.GetKeyDown((KeyCode)104))
		{
			SettingsManager.Config.HideVanillaHud = !SettingsManager.Config.HideVanillaHud;
			SettingsManager.Save();
			SettingsManager.ApplyHudVisibility();
			Debug.Log((object)("RadialMenuMod: Toggled HideVanillaHud via keybind. New value: " + SettingsManager.Config.HideVanillaHud));
		}
		if (scaleProgress < 1f)
		{
			scaleProgress = Mathf.Min(1f, scaleProgress + Time.unscaledDeltaTime * 7.5f);
			float num = 0.5f + 0.5f * Mathf.Sin(scaleProgress * (float)Math.PI * 0.5f);
			((Transform)panelRect).localScale = new Vector3(num, num, 1f);
			CanvasGroup val = canvasGO.GetComponent<CanvasGroup>();
			if ((Object)(object)val == (Object)null)
			{
				val = canvasGO.AddComponent<CanvasGroup>();
			}
			val.alpha = scaleProgress;
		}
		if (!isMenuOpen)
		{
			return;
		}
		if (Input.GetMouseButtonDown(0))
		{
			OnLeftClickDown();
			return;
		}
		if (Input.GetMouseButtonDown(1) && Time.frameCount != openFrame)
		{
			OnRightClickDown();
			return;
		}
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (Mathf.Abs(axis) > 0.01f)
		{
			if (currentState == WheelState.Buildings)
			{
				currentState = WheelState.Categories;
				hoveredBuildingIndex = -1;
				UpdateUI();
			}
			else
			{
				if (axis > 0.01f)
				{
					hoveredCategoryIndex = (hoveredCategoryIndex + 1) % 3;
				}
				else
				{
					hoveredCategoryIndex = (hoveredCategoryIndex - 1 + 3) % 3;
				}
				hoveredOptionIndex = -1;
				UpdateUI();
			}
		}
		if (Input.GetKeyDown((KeyCode)27))
		{
			HideMenu();
			return;
		}
		Vector3 mousePosition = Input.mousePosition;
		Vector2 val2 = Vector2.op_Implicit(mousePosition - startMousePos);
		float num2 = (mouseDistance = ((Vector2)(ref val2)).magnitude);
		if (num2 > maxDistance)
		{
			HideMenu();
			return;
		}
		int num3 = ((currentState == WheelState.Buildings) ? currentBuildingDefs.Count : GetCurrentOptionsList().Count);
		if (num3 > 0 && num2 > 40f)
		{
			float num4 = Mathf.Atan2(val2.y, val2.x) * 57.29578f;
			if (num4 < 0f)
			{
				num4 += 360f;
			}
			mouseAngle = num4;
			float num5 = 360f / (float)num3;
			float num6 = num5 / 2f;
			int num7 = Mathf.FloorToInt((num4 + num6) / num5) % num3;
			if (currentState == WheelState.Buildings)
			{
				hoveredBuildingIndex = num7;
			}
			else
			{
				hoveredOptionIndex = num7;
			}
			UpdateUI();
		}
		else
		{
			hoveredOptionIndex = -1;
			hoveredBuildingIndex = -1;
			UpdateUI();
		}
		if ((Object)(object)pointerLine != (Object)null)
		{
			((Component)pointerLine).gameObject.SetActive(false);
		}
	}

	private static void CacheReflection()
	{
		try
		{
			if (planScreenIsDefResearchedMethod == null)
			{
				planScreenIsDefResearchedMethod = typeof(PlanScreen).GetMethod("IsDefResearched", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			if (planScreenIsDefBuildableMethod == null)
			{
				planScreenIsDefBuildableMethod = typeof(PlanScreen).GetMethod("IsDefBuildable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			if (planScreenToggleEntriesField == null)
			{
				planScreenToggleEntriesField = typeof(PlanScreen).GetField("toggleEntries", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			if (planScreenCategoryInteractiveField == null)
			{
				planScreenCategoryInteractiveField = typeof(PlanScreen).GetField("CategoryInteractive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			if (planScreenTagCategoryMapField == null)
			{
				planScreenTagCategoryMapField = typeof(PlanScreen).GetField("tagCategoryMap", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("RadialMenuMod: Error caching reflection fields: " + ex.Message));
		}
	}

	private int GetBuildingRequirementsState(BuildingDef def)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected I4, but got Unknown
		if ((Object)(object)BuildMenu.Instance != (Object)null)
		{
			return (int)BuildMenu.Instance.BuildableState(def);
		}
		return 3;
	}

	private bool IsBuildingResearched(BuildingDef def)
	{
		if ((Object)(object)PlanScreen.Instance == (Object)null)
		{
			return true;
		}
		CacheReflection();
		if (planScreenIsDefResearchedMethod != null)
		{
			return (bool)planScreenIsDefResearchedMethod.Invoke(PlanScreen.Instance, new object[1] { def });
		}
		return true;
	}

	private bool IsDefBuildable(BuildingDef def)
	{
		if ((Object)(object)PlanScreen.Instance == (Object)null)
		{
			return true;
		}
		CacheReflection();
		if (planScreenIsDefBuildableMethod != null)
		{
			return (bool)planScreenIsDefBuildableMethod.Invoke(PlanScreen.Instance, new object[1] { def });
		}
		return true;
	}

	private string GetInternalCategoryName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return name;
		}
		if (name.Equals("Ventilation", StringComparison.OrdinalIgnoreCase))
		{
			return "HVAC";
		}
		if (name.Equals("Refinement", StringComparison.OrdinalIgnoreCase) || name.Equals("Refine", StringComparison.OrdinalIgnoreCase))
		{
			return "Refining";
		}
		if (name.Equals("Medicine", StringComparison.OrdinalIgnoreCase) || name.Equals("Medical", StringComparison.OrdinalIgnoreCase))
		{
			return "Medical";
		}
		if (name.Equals("Shipping", StringComparison.OrdinalIgnoreCase) || name.Equals("SolidConveyor", StringComparison.OrdinalIgnoreCase))
		{
			return "Conveyance";
		}
		if (name.Equals("Rocketry", StringComparison.OrdinalIgnoreCase) || name.Equals("Rockets", StringComparison.OrdinalIgnoreCase))
		{
			return "Rockets";
		}
		return name;
	}

	private unsafe bool IsCategoryBuildable(string categoryName)
	{
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		categoryName = GetInternalCategoryName(categoryName);
		if ((Object)(object)PlanScreen.Instance == (Object)null)
		{
			return true;
		}
		CacheReflection();
		object obj = null;
		try
		{
			if (planScreenToggleEntriesField != null && planScreenToggleEntriesField.GetValue(PlanScreen.Instance) is IList list)
			{
				HashedString val = default(HashedString);
				((HashedString)(ref val))..ctor(categoryName);
				foreach (object item in list)
				{
					if (item == null)
					{
						continue;
					}
					FieldInfo field = item.GetType().GetField("planCategory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (!(field != null))
					{
						continue;
					}
					HashedString val2 = (HashedString)field.GetValue(item);
					if (val2 == val)
					{
						FieldInfo field2 = item.GetType().GetField("toggleInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (field2 != null)
						{
							obj = field2.GetValue(item);
							break;
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("RadialMenuMod: Error scanning toggleEntries: " + ex.Message));
		}
		try
		{
			if (planScreenCategoryInteractiveField != null && obj != null)
			{
				if (planScreenCategoryInteractiveField.GetValue(PlanScreen.Instance) is IDictionary dictionary)
				{
					if (!dictionary.Contains(obj))
					{
						return false;
					}
					if (!(bool)dictionary[obj])
					{
						return false;
					}
				}
			}
			else if (planScreenCategoryInteractiveField != null && obj == null)
			{
				return false;
			}
		}
		catch (Exception ex2)
		{
			Debug.LogWarning((object)("RadialMenuMod: Error checking CategoryInteractive state: " + ex2.Message));
		}
		Dictionary<Tag, HashedString> dictionary2 = null;
		if (planScreenTagCategoryMapField != null)
		{
			dictionary2 = planScreenTagCategoryMapField.GetValue(PlanScreen.Instance) as Dictionary<Tag, HashedString>;
		}
		if (dictionary2 == null || Assets.BuildingDefs == null)
		{
			return true;
		}
		HashedString val3 = default(HashedString);
		((HashedString)(ref val3))..ctor(categoryName);
		HashedString val4 = HashedString.Invalid;
		foreach (HashedString value2 in dictionary2.Values)
		{
			if (value2 == val3)
			{
				val4 = value2;
				break;
			}
			string text = ((object)(*(HashedString*)(&value2))/*cast due to .constrained prefix*/).ToString();
			if (text.Equals(categoryName, StringComparison.OrdinalIgnoreCase) || text.IndexOf(categoryName, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				val4 = value2;
				break;
			}
		}
		if (val4 == HashedString.Invalid)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		foreach (BuildingDef buildingDef in Assets.BuildingDefs)
		{
			if ((Object)(object)buildingDef != (Object)null && buildingDef.ShowInBuildMenu && !buildingDef.Deprecated && !buildingDef.DebugOnly && dictionary2.TryGetValue(((Def)buildingDef).Tag, out var value) && value == val4 && (IsBuildingResearched(buildingDef) || DebugHandler.InstantBuildMode))
			{
				flag = true;
				if (IsDefBuildable(buildingDef))
				{
					flag2 = true;
					break;
				}
			}
		}
		return flag && flag2;
	}

	private void UpdateUI()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0823: Unknown result type (might be due to invalid IL or missing references)
		//IL_0829: Unknown result type (might be due to invalid IL or missing references)
		//IL_082f: Unknown result type (might be due to invalid IL or missing references)
		//IL_083a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0880: Unknown result type (might be due to invalid IL or missing references)
		//IL_089f: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05af: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_072f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0756: Unknown result type (might be due to invalid IL or missing references)
		//IL_077d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0654: Unknown result type (might be due to invalid IL or missing references)
		//IL_066c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0684: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_060c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0624: Unknown result type (might be due to invalid IL or missing references)
		Color val = ((hoveredCategoryIndex == 0) ? new Color(0.2f, 0.75f, 1f) : ((hoveredCategoryIndex != 1) ? new Color(1f, 0.75f, 0.1f) : new Color(0.1f, 1f, 0.6f)));
		((Graphic)centerHubCircle).color = val;
		int num = ((currentState == WheelState.Buildings) ? currentBuildingDefs.Count : GetCurrentOptionsList().Count);
		float num2 = Mathf.Max(240f, 180f + (float)num * 8f);
		maxDistance = num2 + 130f;
		float num3 = (num2 + 95f) * 2f;
		panelRect.sizeDelta = new Vector2(num3, num3);
		Util.rectTransform((Component)(object)backgroundCircle).sizeDelta = new Vector2(num3, num3);
		if (currentState == WheelState.Buildings)
		{
			if (hoveredBuildingIndex >= 0 && hoveredBuildingIndex < currentBuildingDefs.Count)
			{
				centerText.text = StripRichText(((Def)currentBuildingDefs[hoveredBuildingIndex]).Name);
			}
			else
			{
				centerText.text = "Buildings";
			}
		}
		else
		{
			List<RadialOption> currentOptionsList = GetCurrentOptionsList();
			if (hoveredOptionIndex >= 0 && hoveredOptionIndex < currentOptionsList.Count)
			{
				centerText.text = currentOptionsList[hoveredOptionIndex].Name;
			}
			else
			{
				centerText.text = mainCategories[hoveredCategoryIndex];
			}
		}
		Vector2 val2 = default(Vector2);
		Vector2 val7 = default(Vector2);
		for (int i = 0; i < 32; i++)
		{
			if (i < num)
			{
				((Component)outerButtonBGs[i]).gameObject.SetActive(true);
				float num4 = (float)i * (360f / (float)num);
				float num5 = num4 * ((float)Math.PI / 180f);
				((Vector2)(ref val2))..ctor(Mathf.Cos(num5) * num2, Mathf.Sin(num5) * num2);
				Util.rectTransform((Component)(object)outerButtonBGs[i]).anchoredPosition = val2;
				string text = "";
				Sprite val3 = null;
				bool flag = true;
				if (currentState == WheelState.Buildings)
				{
					BuildingDef val4 = currentBuildingDefs[i];
					int num6 = currentBuildingStates[i];
					text = StripRichText(((Def)val4).Name);
					val3 = val4.GetUISprite("ui", false);
					flag = IsDefBuildable(val4);
				}
				else
				{
					RadialOption radialOption = GetCurrentOptionsList()[i];
					text = radialOption.Name;
					if (hoveredCategoryIndex == 0)
					{
						val3 = GetToolSprite(radialOption.Name);
					}
					else if (hoveredCategoryIndex == 1)
					{
						val3 = GetOverlaySprite(radialOption.Name);
					}
					else
					{
						val3 = GetCategorySprite(radialOption.Name);
						flag = IsCategoryBuildable(radialOption.Name);
					}
				}
				outerTexts[i].text = text;
				string text2 = "";
				if (currentState == WheelState.Buildings)
				{
					BuildingDef val5 = currentBuildingDefs[i];
					text2 = "<b>" + ((Def)val5).Name + "</b>\n" + val5.Effect + "\n\n" + val5.Desc;
				}
				else
				{
					RadialOption radialOption2 = GetCurrentOptionsList()[i];
					text2 = radialOption2.Name;
				}
				outerTooltips[i].SetSimpleTooltip(text2);
				Vector2 val6 = Vector2.op_Implicit(startMousePos) + val2;
				((Vector2)(ref val7))..ctor(40f, 220f);
				outerTooltips[i].tooltipPivot = new Vector2(0f, 0f);
				outerTooltips[i].toolTipPosition = (TooltipPosition)6;
				outerTooltips[i].tooltipPositionOffset = val7 - val6;
				if ((Object)(object)val3 != (Object)null)
				{
					outerIcons[i].sprite = val3;
					((Component)outerIcons[i]).gameObject.SetActive(true);
					Util.rectTransform((Component)(object)outerTexts[i]).anchoredPosition = new Vector2(0f, -22f);
					Util.rectTransform((Component)(object)outerTexts[i]).sizeDelta = new Vector2(80f, 32f);
				}
				else
				{
					((Component)outerIcons[i]).gameObject.SetActive(false);
					Util.rectTransform((Component)(object)outerTexts[i]).anchoredPosition = Vector2.zero;
					Util.rectTransform((Component)(object)outerTexts[i]).sizeDelta = new Vector2(80f, 70f);
				}
				bool flag2 = ((currentState == WheelState.Buildings) ? (i == hoveredBuildingIndex) : (i == hoveredOptionIndex));
				if (flag2)
				{
					((Transform)Util.rectTransform((Component)(object)outerButtonBGs[i])).localScale = new Vector3(1.15f, 1.15f, 1f);
				}
				else
				{
					((Transform)Util.rectTransform((Component)(object)outerButtonBGs[i])).localScale = new Vector3(1f, 1f, 1f);
				}
				if (flag)
				{
					if (flag2)
					{
						((Graphic)outerButtonBGs[i]).color = new Color(val.r, val.g, val.b, 0.9f);
						((Graphic)outerTexts[i]).color = Color.white;
						((Graphic)outerIcons[i]).color = Color.white;
					}
					else
					{
						((Graphic)outerButtonBGs[i]).color = new Color(1f, 1f, 1f, 0.15f);
						((Graphic)outerTexts[i]).color = Color.white;
						((Graphic)outerIcons[i]).color = Color.white;
					}
				}
				else if (flag2)
				{
					((Graphic)outerButtonBGs[i]).color = new Color(0.45f, 0.45f, 0.45f, 0.9f);
					((Graphic)outerTexts[i]).color = Color.white;
					((Graphic)outerIcons[i]).color = new Color(0.8f, 0.8f, 0.8f);
				}
				else
				{
					((Graphic)outerButtonBGs[i]).color = new Color(0.35f, 0.35f, 0.35f, 0.35f);
					((Graphic)outerTexts[i]).color = new Color(0.55f, 0.55f, 0.55f);
					((Graphic)outerIcons[i]).color = new Color(0.4f, 0.4f, 0.4f);
				}
			}
			else
			{
				((Component)outerButtonBGs[i]).gameObject.SetActive(false);
			}
		}
		float num7 = 72f;
		float num8 = num2 - 48f;
		float num9 = Mathf.Max(0f, num8 - num7);
		Vector2 anchoredPosition = default(Vector2);
		for (int j = 0; j < 32; j++)
		{
			if (j < num && num > 1)
			{
				((Component)dividerLines[j]).gameObject.SetActive(true);
				((Graphic)dividerLines[j]).color = new Color(val.r, val.g, val.b, 0.22f);
				float num10 = ((float)j + 0.5f) * (360f / (float)num);
				float num11 = num10 * ((float)Math.PI / 180f);
				RectTransform val8 = Util.rectTransform((Component)(object)dividerLines[j]);
				val8.sizeDelta = new Vector2(1.5f, num9);
				((Transform)val8).localRotation = Quaternion.Euler(0f, 0f, num10 - 90f);
				((Vector2)(ref anchoredPosition))..ctor(Mathf.Cos(num11) * num7, Mathf.Sin(num11) * num7);
				val8.anchoredPosition = anchoredPosition;
			}
			else
			{
				((Component)dividerLines[j]).gameObject.SetActive(false);
			}
		}
	}

	private void ShowMenu(Vector3 cursor_pos)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		isMenuOpen = true;
		currentState = WheelState.Categories;
		openFrame = Time.frameCount;
		startMousePos = Input.mousePosition;
		((Transform)panelRect).position = startMousePos;
		((Transform)panelRect).localScale = new Vector3(0.5f, 0.5f, 1f);
		scaleProgress = 0f;
		canvasGO.SetActive(true);
		CanvasGroup component = canvasGO.GetComponent<CanvasGroup>();
		if ((Object)(object)component != (Object)null)
		{
			component.alpha = 0f;
		}
		hoveredOptionIndex = -1;
		hoveredBuildingIndex = -1;
		UpdateUI();
		if ((Object)(object)CameraController.Instance != (Object)null)
		{
			CameraController.Instance.DisableUserCameraControl = true;
		}
	}

	public void HideMenu()
	{
		isMenuOpen = false;
		if ((Object)(object)canvasGO != (Object)null)
		{
			canvasGO.SetActive(false);
		}
		if ((Object)(object)CameraController.Instance != (Object)null)
		{
			CameraController.Instance.DisableUserCameraControl = false;
		}
	}

	private void ActivateTool(string toolTypeName)
	{
		if ((Object)(object)PlayerController.Instance == (Object)null)
		{
			return;
		}
		InterfaceTool val = PlayerController.Instance.tools.FirstOrDefault((InterfaceTool t) => ((object)t).GetType().Name == toolTypeName);
		if (!((Object)(object)val != (Object)null))
		{
			return;
		}
		PlayerController.Instance.ActivateTool(val);
		if (!((Object)(object)ToolMenu.Instance != (Object)null))
		{
			return;
		}
		FieldInfo field = typeof(ToolMenu).GetField("basicTools", BindingFlags.Instance | BindingFlags.Public);
		if (!(field != null) || !(field.GetValue(ToolMenu.Instance) is IList list))
		{
			return;
		}
		object obj = null;
		foreach (object item in list)
		{
			FieldInfo field2 = item.GetType().GetField("tools", BindingFlags.Instance | BindingFlags.Public);
			if (field2 != null && field2.GetValue(item) is IList list2)
			{
				foreach (object item2 in list2)
				{
					FieldInfo field3 = item2.GetType().GetField("tool", BindingFlags.Instance | BindingFlags.Public);
					if (field3 != null && field3.GetValue(item2) == val)
					{
						obj = item2;
						break;
					}
				}
			}
			if (obj != null)
			{
				break;
			}
		}
		if (obj != null)
		{
			MethodInfo method = typeof(ToolMenu).GetMethod("ChooseTool", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(ToolMenu.Instance, new object[1] { obj });
			}
		}
	}

	private void ToggleOverlay(HashedString overlayMode)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)OverlayScreen.Instance != (Object)null)
		{
			OverlayScreen.Instance.ToggleOverlay(overlayMode, true);
		}
	}

	private void OpenCategoryByName(string categoryName)
	{
		if (!((Object)(object)PlanScreen.Instance == (Object)null))
		{
			MethodInfo method = typeof(PlanScreen).GetMethod("OpenCategoryByName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(PlanScreen.Instance, new object[1] { categoryName });
			}
		}
	}

	private void OpenBuildCategory(string category)
	{
		OpenCategoryByName(category);
	}

	private unsafe void SelectBuildingDef(BuildingDef buildingDef)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)buildingDef == (Object)null || (Object)(object)PlanScreen.Instance == (Object)null)
		{
			return;
		}
		HashedString value = HashedString.Invalid;
		Dictionary<Tag, HashedString> dictionary = null;
		FieldInfo field = typeof(PlanScreen).GetField("tagCategoryMap", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field != null)
		{
			dictionary = field.GetValue(PlanScreen.Instance) as Dictionary<Tag, HashedString>;
		}
		dictionary?.TryGetValue(((Def)buildingDef).Tag, out value);
		if (value != HashedString.Invalid)
		{
			OpenCategoryByName(((object)(*(HashedString*)(&value))/*cast due to .constrained prefix*/).ToString());
		}
		GameObject val = null;
		FieldInfo field2 = typeof(PlanScreen).GetField("allBuildingToggles", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field2 != null && field2.GetValue(PlanScreen.Instance) is IDictionary dictionary2 && dictionary2.Contains(((Def)buildingDef).Tag))
		{
			object obj = dictionary2[((Def)buildingDef).Tag];
			if (obj != null)
			{
				PropertyInfo property = obj.GetType().GetProperty("gameObject");
				if (property != null)
				{
					object value2 = property.GetValue(obj);
					val = (GameObject)((value2 is GameObject) ? value2 : null);
				}
			}
		}
		bool flag = false;
		if ((Object)(object)val == (Object)null)
		{
			val = GetDummyToggleGO();
			flag = true;
		}
		if ((Object)(object)val != (Object)null)
		{
			if ((Object)(object)val.GetComponent<PlanCategoryNotifications>() == (Object)null)
			{
				val.AddComponent<PlanCategoryNotifications>();
			}
			MethodInfo method = typeof(PlanScreen).GetMethod("OnSelectBuilding", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(PlanScreen.Instance, new object[3] { val, buildingDef, null });
			}
		}
		blockClickFrame = Time.frameCount;
	}

	private unsafe void LoadBuildingsForCategory(string category)
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		category = GetInternalCategoryName(category);
		currentBuildingDefs.Clear();
		currentBuildingStates.Clear();
		Dictionary<Tag, HashedString> dictionary = null;
		if ((Object)(object)PlanScreen.Instance != (Object)null)
		{
			FieldInfo field = typeof(PlanScreen).GetField("tagCategoryMap", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field != null)
			{
				dictionary = field.GetValue(PlanScreen.Instance) as Dictionary<Tag, HashedString>;
			}
		}
		if (dictionary == null && (Object)(object)BuildMenu.Instance != (Object)null)
		{
			FieldInfo field2 = typeof(BuildMenu).GetField("tagCategoryMap", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field2 != null)
			{
				dictionary = field2.GetValue(BuildMenu.Instance) as Dictionary<Tag, HashedString>;
			}
		}
		if (dictionary == null || Assets.BuildingDefs == null)
		{
			return;
		}
		HashedString val = default(HashedString);
		((HashedString)(ref val))..ctor(category);
		HashedString val2 = HashedString.Invalid;
		foreach (HashedString value2 in dictionary.Values)
		{
			if (value2 == val)
			{
				val2 = value2;
				break;
			}
			string text = ((object)(*(HashedString*)(&value2))/*cast due to .constrained prefix*/).ToString();
			if (text.Equals(category, StringComparison.OrdinalIgnoreCase) || text.IndexOf(category, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				val2 = value2;
				break;
			}
		}
		if (!(val2 != HashedString.Invalid))
		{
			return;
		}
		int num = 0;
		foreach (BuildingDef buildingDef in Assets.BuildingDefs)
		{
			if (!((Object)(object)buildingDef != (Object)null) || !buildingDef.ShowInBuildMenu || buildingDef.Deprecated || buildingDef.DebugOnly || (!IsBuildingResearched(buildingDef) && !DebugHandler.InstantBuildMode) || !dictionary.TryGetValue(((Def)buildingDef).Tag, out var value) || !(value == val2))
			{
				continue;
			}
			int buildingRequirementsState = GetBuildingRequirementsState(buildingDef);
			if (buildingRequirementsState != 1)
			{
				currentBuildingDefs.Add(buildingDef);
				currentBuildingStates.Add(buildingRequirementsState);
				num++;
				if (num >= 32)
				{
					break;
				}
			}
		}
	}

	private string StripRichText(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return Regex.Replace(input, "<[^>]*>", "");
	}

	private Sprite GetToolSprite(string name)
	{
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		if (name.Equals("Dig", StringComparison.OrdinalIgnoreCase))
		{
			text = "icon_action_dig";
		}
		else if (name.Equals("Mop", StringComparison.OrdinalIgnoreCase))
		{
			text = "icon_action_mop";
		}
		else if (name.Equals("Sweep", StringComparison.OrdinalIgnoreCase))
		{
			text = "icon_action_store";
		}
		else if (name.Equals("Deconstruct", StringComparison.OrdinalIgnoreCase))
		{
			text = "icon_action_deconstruct";
		}
		else if (name.Equals("Prioritize", StringComparison.OrdinalIgnoreCase))
		{
			text = "icon_action_prioritize";
		}
		else if (name.Equals("Attack", StringComparison.OrdinalIgnoreCase))
		{
			text = "icon_action_attack";
		}
		else if (name.Equals("Disinfect", StringComparison.OrdinalIgnoreCase))
		{
			text = "icon_action_disinfect";
		}
		else if (name.Equals("Harvest", StringComparison.OrdinalIgnoreCase))
		{
			text = "icon_action_harvest";
		}
		else if (name.Equals("Empty Pipe", StringComparison.OrdinalIgnoreCase))
		{
			text = "icon_action_empty_pipes";
		}
		else if (name.Equals("Disconnect", StringComparison.OrdinalIgnoreCase))
		{
			text = "icon_action_disconnect";
		}
		else if (name.Equals("Wrangle", StringComparison.OrdinalIgnoreCase))
		{
			text = "icon_action_capture";
		}
		else if (name.Equals("Cancel", StringComparison.OrdinalIgnoreCase))
		{
			text = "icon_action_cancel";
		}
		if (!string.IsNullOrEmpty(text))
		{
			Sprite sprite = Assets.GetSprite(HashedString.op_Implicit(text));
			if ((Object)(object)sprite != (Object)null)
			{
				return sprite;
			}
		}
		return null;
	}

	private Sprite GetOverlaySprite(string name)
	{
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		if (name.Equals("Oxygen", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_oxygen";
		}
		else if (name.Equals("Power", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_power";
		}
		else if (name.Equals("Temperature", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_temperature";
		}
		else if (name.Equals("Gas Pipe", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_gasvent";
		}
		else if (name.Equals("Liquid Pipe", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_liquidvent";
		}
		else if (name.Equals("Automation", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_logic";
		}
		else if (name.Equals("Shipping", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_conveyor";
		}
		else if (name.Equals("Decor", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_decor";
		}
		else if (name.Equals("Light", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_lights";
		}
		else if (name.Equals("Germs", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_disease";
		}
		else if (name.Equals("Agriculture", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_farming";
		}
		else if (name.Equals("Rooms", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_rooms";
		}
		else if (name.Equals("Suits", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_suit";
		}
		else if (name.Equals("Materials", StringComparison.OrdinalIgnoreCase))
		{
			text = "overlay_materials";
		}
		if (!string.IsNullOrEmpty(text))
		{
			Sprite sprite = Assets.GetSprite(HashedString.op_Implicit(text));
			if ((Object)(object)sprite != (Object)null)
			{
				return sprite;
			}
		}
		return null;
	}

	private Sprite GetCategorySprite(string name)
	{
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (PlanScreen.IconNameMap != null)
		{
			foreach (KeyValuePair<HashedString, string> item in PlanScreen.IconNameMap)
			{
				string text = ((object)item.Key/*cast due to .constrained prefix*/).ToString();
				bool flag = false;
				if (name.Equals("Ventilation", StringComparison.OrdinalIgnoreCase) && text.Equals("HVAC", StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
				}
				else if (name.Equals("Refinement", StringComparison.OrdinalIgnoreCase) && text.Equals("Refining", StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
				}
				else if (name.Equals("Shipping", StringComparison.OrdinalIgnoreCase) && text.Equals("Conveyance", StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
				}
				else if (text.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
				}
				if (flag)
				{
					Sprite sprite = Assets.GetSprite(HashedString.op_Implicit(item.Value));
					if ((Object)(object)sprite != (Object)null)
					{
						return sprite;
					}
				}
			}
		}
		string text2 = "icon_category_base";
		if (name.Equals("Base", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_base";
		}
		else if (name.Equals("Oxygen", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_oxygen";
		}
		else if (name.Equals("Power", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_electrical";
		}
		else if (name.Equals("Food", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_food";
		}
		else if (name.Equals("Plumbing", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_plumbing";
		}
		else if (name.Equals("Ventilation", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_ventilation";
		}
		else if (name.Equals("Refinement", StringComparison.OrdinalIgnoreCase) || name.Equals("Refine", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_refinery";
		}
		else if (name.Equals("Medicine", StringComparison.OrdinalIgnoreCase) || name.Equals("Medical", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_medical";
		}
		else if (name.Equals("Furniture", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_furniture";
		}
		else if (name.Equals("Equipment", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_equipment";
		}
		else if (name.Equals("Utilities", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_utilities";
		}
		else if (name.Equals("Automation", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_automation";
		}
		else if (name.Equals("Shipping", StringComparison.OrdinalIgnoreCase) || name.Equals("SolidConveyor", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_shipping";
		}
		else if (name.Equals("Rocketry", StringComparison.OrdinalIgnoreCase) || name.Equals("Rockets", StringComparison.OrdinalIgnoreCase))
		{
			text2 = "icon_category_rocketry";
		}
		return Assets.GetSprite(HashedString.op_Implicit(text2));
	}
}
