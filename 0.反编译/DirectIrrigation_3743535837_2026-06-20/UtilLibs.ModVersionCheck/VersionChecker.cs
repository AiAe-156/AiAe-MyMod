using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using KMod;
using Newtonsoft.Json;
using PeterHan.PLib.AVC;
using PeterHan.PLib.Core;

namespace UtilLibs.ModVersionCheck;

public class VersionChecker
{
	public const string Dev_File_Local = "ImalasModVersionData.json";

	public const string ModVersionDataKey_Server = "Sgt_Imalas_ServerVersionData";

	public const string ModVersionDataKey_Client = "Sgt_Imalas_ClientVersionData";

	public const string VersionCheckerVersion = "Sgt_Imalas_UI_VersionData";

	public const string UIInitializedKey = "Sgt_Imalas_UI_Initialized";

	public const string MainMenuPatchInitializedKey = "Sgt_Imalas_MainMenuPatch_Initialized";

	public const string CurrentlyFetchingKey = "Sgt_Imalas_ModVersionData_CurrentlyFetching";

	public const string VersionDataURL = "https://raw.githubusercontent.com/Sgt-Imalas/Sgt_Imalas-Oni-Mods/master/ModVersionData.json";

	public const int CurrentVersion = 11;

	private static UserMod2 This;

	public static bool OlderVersion => 11 <= PRegistry.GetData<int>("Sgt_Imalas_UI_VersionData");

	public static bool HasPatch(out string harmonyID)
	{
		harmonyID = PRegistry.GetData<string>("Sgt_Imalas_MainMenuPatch_Initialized");
		return !Util.IsNullOrWhiteSpace(harmonyID);
	}

	public static void SetIsPatched(string harmonyID)
	{
		PRegistry.PutData("Sgt_Imalas_MainMenuPatch_Initialized", harmonyID);
	}

	public static void RegisterCurrentVersion(UserMod2 userMod)
	{
		Dictionary<string, string> dictionary = PRegistry.GetData<Dictionary<string, string>>("Sgt_Imalas_ClientVersionData");
		if (dictionary == null)
		{
			dictionary = new Dictionary<string, string>();
		}
		dictionary[userMod.mod.staticID] = userMod.mod.packagedModInfo.version;
		PRegistry.PutData("Sgt_Imalas_ClientVersionData", dictionary);
		int data = PRegistry.GetData<int>("Sgt_Imalas_UI_VersionData");
		if (data < 11)
		{
			PRegistry.PutData("Sgt_Imalas_UI_VersionData", 11);
		}
		if (!userMod.mod.IsDev)
		{
			return;
		}
		string text = Path.Combine(IO_Utils.ConfigsFolder, "ImalasModVersionData.json");
		SgtLogger.l("version data filepath: " + text);
		try
		{
			IO_Utils.ReadFromFile<JsonURLVersionChecker.ModVersions>(text, out var output);
			if (output == null)
			{
				output = new JsonURLVersionChecker.ModVersions();
			}
			JsonURLVersionChecker.ModVersion versionData = new JsonURLVersionChecker.ModVersion
			{
				staticID = userMod.mod.staticID,
				version = userMod.mod.packagedModInfo.version
			};
			output.mods.RemoveAll((JsonURLVersionChecker.ModVersion mod) => mod.staticID == versionData.staticID);
			output.mods.Add(versionData);
			IO_Utils.WriteToFile(output, text);
		}
		catch (Exception ex)
		{
			SgtLogger.l(ex.Message);
		}
	}

	public static void HandleVersionChecking(UserMod2 userMod, Harmony harmony)
	{
		RegisterCurrentVersion(userMod);
		OutdatedVersionInfoPatches.MainMenuMissingModsContainerInit.InitMainMenuInfoPatch();
		Task.Run(delegate
		{
			HandleDataFetching(userMod);
		});
	}

	public static async void HandleDataFetching(UserMod2 userMod)
	{
		Dictionary<string, string> data = PRegistry.GetData<Dictionary<string, string>>("Sgt_Imalas_ServerVersionData");
		if (data == null && !PRegistry.GetData<bool>("Sgt_Imalas_ModVersionData_CurrentlyFetching"))
		{
			PRegistry.PutData("Sgt_Imalas_ModVersionData_CurrentlyFetching", true);
			SgtLogger.l("Mod Version Data was null, trying to fetch it", "SgtImalas_VersionCheck");
			using WebClient client = new WebClient();
			Task<string> fetched = client.DownloadStringTaskAsync("https://raw.githubusercontent.com/Sgt-Imalas/Sgt_Imalas-Oni-Mods/master/ModVersionData.json");
			SgtLogger.l("mod version data fetched from github", "SgtImalas_VersionCheck");
			await fetched;
			ParseData(fetched.Result);
		}
	}

	private static void ParseData(string data)
	{
		SgtLogger.l("parsing version data", "SgtImalas_VersionCheck");
		if (string.IsNullOrEmpty(data))
		{
			return;
		}
		JsonURLVersionChecker.ModVersions modVersions = JsonConvert.DeserializeObject<JsonURLVersionChecker.ModVersions>(data);
		if (modVersions != null)
		{
			Dictionary<string, string> VersionData = new Dictionary<string, string>();
			modVersions.mods.ForEach(delegate(JsonURLVersionChecker.ModVersion x)
			{
				VersionData[x.staticID] = x.version;
			});
			PRegistry.PutData("Sgt_Imalas_ServerVersionData", VersionData);
		}
		PRegistry.PutData("Sgt_Imalas_ModVersionData_CurrentlyFetching", false);
		SgtLogger.l("version data recieved", "SgtImalas_VersionCheck");
	}

	private static List<ModVersionCheckResults> PlibVersionChecks()
	{
		IEnumerable<PForwardedComponent> allComponents = PRegistry.Instance.GetAllComponents("PeterHan.PLib.AVC.PVersionCheck");
		List<ModVersionCheckResults> list = new List<ModVersionCheckResults>();
		List<string> list2 = new List<string>();
		if (allComponents != null)
		{
			foreach (PForwardedComponent item in allComponents)
			{
				ICollection<ModVersionCheckResults> instanceDataSerialized = item.GetInstanceDataSerialized<ICollection<ModVersionCheckResults>>();
				if (instanceDataSerialized == null)
				{
					continue;
				}
				foreach (ModVersionCheckResults item2 in instanceDataSerialized)
				{
					if (!list2.Contains(item2.ModChecked))
					{
						list2.Add(item2.ModChecked);
						list.Add(item2);
					}
				}
			}
		}
		SgtLogger.l(list.Count.ToString(), "plib mod count");
		return list;
	}

	private static void AppendOutdatedMod(StringBuilder stringBuilder, string modTitle, string latestModVersion, string currentModVersion, ref int linecount, ref int modsOverLineCount, int maxLines)
	{
		if (linecount < maxLines)
		{
			stringBuilder.Append("<b>");
			stringBuilder.Append(modTitle);
			stringBuilder.Append(":</b>");
			stringBuilder.AppendLine();
			stringBuilder.Append("installed: ");
			stringBuilder.Append(currentModVersion);
			stringBuilder.Append(", latest: ");
			stringBuilder.AppendLine(latestModVersion);
			linecount += 2;
		}
		else
		{
			modsOverLineCount++;
		}
	}

	public static bool ModsOutOfDate(int maxLines, out string missingModsInfo, out int linecount)
	{
		Dictionary<string, string> data = PRegistry.GetData<Dictionary<string, string>>("Sgt_Imalas_ServerVersionData");
		Dictionary<string, string> data2 = PRegistry.GetData<Dictionary<string, string>>("Sgt_Imalas_ClientVersionData");
		SgtLogger.Assert("local data was null", data2);
		SgtLogger.Assert("server data was null", data);
		linecount = 0;
		int modsOverLineCount = 0;
		missingModsInfo = string.Empty;
		bool result = false;
		if (data2 != null && data2.Count > 0 && data != null && data.Count > 0)
		{
			SgtLogger.l("starting version check", "Sgt_Imalas-VersionChecker");
			Manager modManager = Global.Instance.modManager;
			StringBuilder stringBuilder = new StringBuilder();
			SgtLogger.l("checking for outdated plib version checkers", "Sgt_Imalas-VersionChecker");
			foreach (ModVersionCheckResults versionEntry in PlibVersionChecks())
			{
				if (!versionEntry.IsUpToDate)
				{
					Mod val = modManager.mods.Find((Mod mod) => mod.staticID == versionEntry.ModChecked);
					if (val != null)
					{
						AppendOutdatedMod(stringBuilder, val.title, versionEntry.NewVersion, val.label.version.ToString(), ref linecount, ref modsOverLineCount, maxLines);
						result = true;
					}
				}
			}
			int num = 0;
			foreach (string localModId in data.Keys)
			{
				List<Mod> list = modManager.mods.FindAll((Mod modEntry) => modEntry.staticID == localModId);
				if (list == null || list.Count == 0)
				{
					continue;
				}
				foreach (Mod item in list)
				{
					if (item == null || item.packagedModInfo == null)
					{
						continue;
					}
					num++;
					string text = item.packagedModInfo.version;
					if (text.Count((char c) => c == '.') < 3)
					{
						text += ".0";
					}
					string text2 = data[localModId];
					if (text2.Count((char c) => c == '.') < 3)
					{
						text2 += ".0";
					}
					if (Version.TryParse(text, out Version result2) && Version.TryParse(text2, out Version result3))
					{
						if (result2.CompareTo(result3) < 0)
						{
							AppendOutdatedMod(stringBuilder, item.title, result3.ToString(), result2.ToString(), ref linecount, ref modsOverLineCount, maxLines);
							SgtLogger.warning(item.title + " is outdated! Found local version is " + result2.ToString() + ", but latest is " + result3.ToString());
							result = true;
						}
					}
					else if (item.packagedModInfo.version != data[localModId])
					{
						string text3 = data[localModId];
						string version = item.packagedModInfo.version;
						AppendOutdatedMod(stringBuilder, item.title, text3, version, ref linecount, ref modsOverLineCount, maxLines);
						SgtLogger.warning(item.title + " is not the target version! Found local version is " + version + ", but target is " + text3.ToString());
						result = true;
					}
				}
			}
			if (modsOverLineCount > 0)
			{
				linecount++;
				stringBuilder.AppendLine($"<b>...and {modsOverLineCount} other</b>");
			}
			SgtLogger.l("version checked " + num + " mods.", "Sgt_Imalas-VersionChecker");
			missingModsInfo = stringBuilder.ToString();
		}
		return result;
	}

	public static void CheckVersion(UserMod2 userMod)
	{
		using WebClient webClient = new WebClient();
		try
		{
			string text = webClient.DownloadString("https://raw.githubusercontent.com/Sgt-Imalas/Sgt_Imalas-Oni-Mods/master/ModVersionData.json");
			if (text == null)
			{
				return;
			}
			JsonURLVersionChecker.ModVersions modVersions = JsonConvert.DeserializeObject<JsonURLVersionChecker.ModVersions>(text);
			if (modVersions == null)
			{
				return;
			}
			JsonURLVersionChecker.ModVersion modVersion = modVersions.mods.First((JsonURLVersionChecker.ModVersion mod) => mod.staticID == userMod.mod.staticID);
			if (modVersion != null && Version.TryParse(modVersion.version, out Version result) && Version.TryParse(userMod.mod.packagedModInfo.version, out Version result2))
			{
				SgtLogger.l(modVersion.version + "<->" + userMod.mod.packagedModInfo.version);
				SgtLogger.l(result2.CompareTo(result).ToString(), "comparison");
				if (result2.CompareTo(result) < 0)
				{
					SgtLogger.warning(userMod.mod.label.title + " is outdated!");
				}
			}
		}
		catch (Exception ex)
		{
			SgtLogger.warning(ex.Message ?? "");
		}
	}

	internal static bool UI_Built()
	{
		return PRegistry.GetData<bool>("Sgt_Imalas_UI_Initialized");
	}

	internal static void SetUIConstructed(bool constructed)
	{
		PRegistry.PutData("Sgt_Imalas_UI_Initialized", constructed);
	}

	internal static void FixVersionPatch(UserMod2 usermod, Harmony harmony)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		This = usermod;
		MethodInfo methodInfo = AccessTools.Method(typeof(Mod), "ScanContent", (Type[])null, (Type[])null);
		if (methodInfo == null)
		{
			SgtLogger.warning("KMod.Mod.ScanContent was null!");
			return;
		}
		MethodInfo methodInfo2 = AccessTools.Method(typeof(VersionChecker), "FixVersionPostfix", (Type[])null, (Type[])null);
		harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(methodInfo2, 300, (string[])null, (string[])null, (bool?)null), (HarmonyMethod)null, (HarmonyMethod)null);
	}

	private static void FixVersionPostfix(Mod __instance)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (__instance.label.id == This.mod.label.id && __instance.label.distribution_platform == This.mod.label.distribution_platform)
		{
			__instance.packagedModInfo.version = This.assembly.GetFileVersion();
		}
	}
}
