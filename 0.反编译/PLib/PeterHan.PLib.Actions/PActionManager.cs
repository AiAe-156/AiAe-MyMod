using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Detours;

namespace PeterHan.PLib.Actions;

/// <summary>
/// Manages PAction functionality which must be single instance.
/// </summary>
public sealed class PActionManager : PForwardedComponent
{
	/// <summary>
	/// Prototypes the required parameters for new BindingEntry() since they changed in
	/// U39-489490.
	/// </summary>
	private delegate BindingEntry NewEntry(string group, GamepadButton button, KKeyCode key_code, Modifier modifier, Action action);

	/// <summary>
	/// The category used for all PLib keys.
	/// </summary>
	public const string CATEGORY = "PLib";

	/// <summary>
	/// Creates a new BindingEntry.
	/// </summary>
	private static readonly NewEntry NEW_BINDING_ENTRY = typeof(BindingEntry).DetourConstructor<NewEntry>();

	/// <summary>
	/// The version of this component. Uses the running PLib version.
	/// </summary>
	internal static readonly Version VERSION = new Version("4.19.0.0");

	/// <summary>
	/// Queued key binds which are resolved on load.
	/// </summary>
	private readonly IList<PAction> actions;

	/// <summary>
	/// The maximum action index of any custom action registered across all mods.
	/// </summary>
	private int maxAction;

	/// <summary>
	/// The instantiated copy of this class.
	/// </summary>
	internal static PActionManager Instance { get; private set; }

	public override Version Version => VERSION;

	/// <summary>
	/// Assigns the key bindings to each Action when they are needed.
	/// </summary>
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

	/// <summary>
	/// Extends the action flags array to the new maximum length.
	/// </summary>
	/// <param name="oldActionFlags">The old flags array.</param>
	/// <param name="newMax">The minimum length.</param>
	/// <returns>The new action flags array.</returns>
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

	/// <summary>
	/// Retrieves a Klei key binding title.
	/// </summary>
	/// <param name="category">The category of the key binding.</param>
	/// <param name="item">The key binding to retrieve.</param>
	/// <returns>The Strings entry describing this key binding.</returns>
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

	/// <summary>
	/// Logs a message encountered by the PLib key binding system.
	/// </summary>
	/// <param name="message">The message.</param>
	internal static void LogKeyBind(string message)
	{
		Debug.LogFormat("[PKeyBinding] {0}", new object[1] { message });
	}

	/// <summary>
	/// Logs a warning encountered by the PLib key binding system.
	/// </summary>
	/// <param name="message">The warning message.</param>
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

	/// <summary>
	/// Creates a new action manager used to create and assign custom actions. Due to the
	/// timing of when the user's key bindings are loaded, all actions must be added in
	/// OnLoad().
	/// </summary>
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

	/// <summary>
	/// Returns the maximum length of the Action enum, including custom actions. If no
	/// actions are defined, returns NumActions - 1 since NumActions is reserved in the
	/// base game.
	///
	/// This value will not be accurate until all mods have loaded and key binds
	/// registered (AfterLayerableLoad or later such as BeforeDbInit).
	/// </summary>
	/// <returns>The maximum length required to represent all Actions.</returns>
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

	/// <summary>
	/// Updates the maximum action for this instance.
	/// </summary>
	private void UpdateMaxAction()
	{
		maxAction = GetSharedData(0);
	}
}
