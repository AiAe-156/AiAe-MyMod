using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.AVC;

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
