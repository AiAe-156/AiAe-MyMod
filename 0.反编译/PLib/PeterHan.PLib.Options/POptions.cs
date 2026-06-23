using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using HarmonyLib;
using KMod;
using Newtonsoft.Json;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// Adds an "Options" screen to a mod in the Mods menu.
/// </summary>
public sealed class POptions : PForwardedComponent
{
	/// <summary>
	/// Opens the mod options dialog for a specific mod assembly.
	/// </summary>
	private sealed class ModOptionsHandler
	{
		/// <summary>
		/// The type whose options will be shown.
		/// </summary>
		private readonly Type forType;

		/// <summary>
		/// The options instance that will handle the dialog.
		/// </summary>
		private readonly PForwardedComponent options;

		internal ModOptionsHandler(PForwardedComponent options, Type forType)
		{
			this.forType = forType ?? throw new ArgumentNullException("forType");
			this.options = options ?? throw new ArgumentNullException("options");
		}

		/// <summary>
		/// Shows the options dialog.
		/// </summary>
		internal void ShowDialog(GameObject _)
		{
			options.Process(0u, new OpenDialogArgs(forType, null));
		}

		public override string ToString()
		{
			return "ModOptionsHandler[Type={0}]".F(forType);
		}
	}

	/// <summary>
	/// The arguments to be passed with message SHOW_DIALOG_MOD.
	/// </summary>
	private sealed class OpenDialogArgs
	{
		/// <summary>
		/// The handler (if not null) to be called when the dialog is closed.
		/// </summary>
		public Action<object> OnClose { get; }

		/// <summary>
		/// The mod options type to show.
		/// </summary>
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

	/// <summary>
	/// The configuration file name used by default for classes that do not specify
	/// otherwise. This file name is case sensitive.
	/// </summary>
	public const string CONFIG_FILE_NAME = "config.json";

	/// <summary>
	/// The maximum nested class depth which will be serialized in mod options to avoid
	/// infinite loops.
	/// </summary>
	public const int MAX_SERIALIZATION_DEPTH = 8;

	/// <summary>
	/// The margins around the Options button.
	/// </summary>
	internal static readonly RectOffset OPTION_BUTTON_MARGIN = new RectOffset(11, 11, 5, 5);

	/// <summary>
	/// The shared mod configuration folder, which works between archived versions and
	/// local/dev/Steam.
	/// </summary>
	public const string SHARED_CONFIG_FOLDER = "config";

	/// <summary>
	/// The version of this component. Uses the running PLib version.
	/// </summary>
	internal static readonly Version VERSION = new Version("4.19.0.0");

	/// <summary>
	/// Maps mod static IDs to their options.
	/// </summary>
	private readonly IDictionary<string, Type> modOptions;

	/// <summary>
	/// Maps mod assemblies to handlers that can fire their options. Only populated in
	/// the instantiated copy of POptions.
	/// </summary>
	private readonly IDictionary<string, ModOptionsHandler> registered;

	/// <summary>
	/// The instantiated copy of this class.
	/// </summary>
	internal static POptions Instance { get; private set; }

	public override Version Version => VERSION;

	/// <summary>
	/// Applied to ModsScreen if mod options are registered, after BuildDisplay runs.
	/// </summary>
	private static void BuildDisplay_Postfix(GameObject ___entryPrefab, IEnumerable ___displayedMods)
	{
		if (Instance == null)
		{
			return;
		}
		int num = 0;
		foreach (object ___displayedMod in ___displayedMods)
		{
			Instance.AddModOptions(___displayedMod, num++, ___entryPrefab);
		}
	}

	/// <summary>
	/// Retrieves the configuration file path used by PLib Options for a specified type.
	/// </summary>
	/// <param name="optionsType">The options type stored in the config file.</param>
	/// <returns>The path to the configuration file that will be used by PLib for that
	/// mod's config.</returns>
	public static string GetConfigFilePath(Type optionsType)
	{
		return GetConfigPath(optionsType.GetCustomAttribute<ConfigFileAttribute>(), optionsType.Assembly);
	}

	/// <summary>
	/// Attempts to find the mod which owns the specified type.
	/// </summary>
	/// <param name="optionsType">The type to look up.</param>
	/// <returns>The Mod that owns it, or null if no owning mod could be found, such as for
	/// types in System or Assembly-CSharp.</returns>
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

	/// <summary>
	/// Retrieves the configuration file path used by PLib Options for a specified type.
	/// </summary>
	/// <param name="attr">The config file attribute for that type.</param>
	/// <param name="modAssembly">The assembly to use for determining the path.</param>
	/// <returns>The path to the configuration file that will be used by PLib for that
	/// mod's config.</returns>
	private static string GetConfigPath(ConfigFileAttribute attr, Assembly modAssembly)
	{
		string nameSafe = modAssembly.GetNameSafe();
		return Path.Combine((nameSafe != null && attr != null && attr.UseSharedConfigLocation) ? Path.Combine(Manager.GetDirectory(), "config", nameSafe) : PUtil.GetModPath(modAssembly), attr?.ConfigFileName ?? "config.json");
	}

	/// <summary>
	/// Reads a mod's settings from its configuration file. The assembly defining T is used
	/// to resolve the proper settings folder.
	/// </summary>
	/// <typeparam name="T">The type of the settings object.</typeparam>
	/// <returns>The settings read, or null if they could not be read (e.g. newly installed).</returns>
	public static T ReadSettings<T>() where T : class
	{
		Type typeFromHandle = typeof(T);
		return ReadSettings(GetConfigPath(typeFromHandle.GetCustomAttribute<ConfigFileAttribute>(), typeFromHandle.Assembly), typeFromHandle) as T;
	}

	/// <summary>
	/// Reads a mod's settings from its configuration file.
	/// </summary>
	/// <param name="path">The path to the settings file.</param>
	/// <param name="optionsType">The options type.</param>
	/// <returns>The settings read, or null if they could not be read (e.g. newly installed)</returns>
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

	/// <summary>
	/// Shows a mod options dialog now, as if Options was used inside the Mods menu.
	/// </summary>
	/// <param name="optionsType">The type of the options to show. The mod to configure,
	/// configuration directory, and so forth will be retrieved from the provided type.
	/// This type must be the same type configured in RegisterOptions for the mod.</param>
	/// <param name="onClose">The method to call when the dialog is closed.</param>
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

	/// <summary>
	/// Writes a mod's settings to its configuration file. The assembly defining T is used
	/// to resolve the proper settings folder.
	/// </summary>
	/// <typeparam name="T">The type of the settings object.</typeparam>
	/// <param name="settings">The settings to write.</param>
	public static void WriteSettings<T>(T settings) where T : class
	{
		ConfigFileAttribute customAttribute = typeof(T).GetCustomAttribute<ConfigFileAttribute>();
		WriteSettings(settings, GetConfigPath(customAttribute, typeof(T).Assembly), customAttribute?.IndentOutput ?? false);
	}

	/// <summary>
	/// Writes a mod's settings to its configuration file.
	/// </summary>
	/// <param name="settings">The settings to write.</param>
	/// <param name="path">The path to the settings file.</param>
	/// <param name="indent">true to indent the output, or false to leave it in one line.</param>
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

	/// <summary>
	/// Adds the Options button to the Mods screen.
	/// </summary>
	/// <param name="modEntry">The mod entry where the button should be added.</param>
	/// <param name="fallbackIndex">The index to use if it cannot be determined from the entry.</param>
	/// <param name="parent">The parent where the entries were added, used only if the
	/// fallback index is required.</param>
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

	/// <summary>
	/// Initializes and stores the options table for quicker lookups later.
	/// </summary>
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

	/// <summary>
	/// Registers a class as a mod options class. The type is registered for the mod
	/// instance specified, which is easily available in OnLoad.
	/// </summary>
	/// <param name="mod">The mod for which the type will be registered.</param>
	/// <param name="optionsType">The class which will represent the options for this mod.</param>
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
