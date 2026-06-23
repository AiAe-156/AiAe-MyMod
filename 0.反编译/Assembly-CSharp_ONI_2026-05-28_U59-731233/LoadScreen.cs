using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ProcGen;
using ProcGenGame;
using STRINGS;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class LoadScreen : KModalScreen
{
	private struct SaveGameFileDetails
	{
		public string BaseName;

		public string FileName;

		public string UniqueID;

		public System.DateTime FileDate;

		public SaveGame.Header FileHeader;

		public SaveGame.GameInfo FileInfo;

		public long Size;
	}

	private class SelectedSave
	{
		public string filename;

		public List<string> dlcIds;

		public KButton button;
	}

	private const int MAX_CLOUD_TUTORIALS = 5;

	private const string CLOUD_TUTORIAL_KEY = "LoadScreenCloudTutorialTimes";

	private const int ITEMS_PER_PAGE = 20;

	[SerializeField]
	private KButton closeButton;

	[SerializeField]
	private GameObject saveButtonRoot;

	[SerializeField]
	private GameObject colonyListRoot;

	[SerializeField]
	private GameObject colonyViewRoot;

	[SerializeField]
	private HierarchyReferences migrationPanelRefs;

	[SerializeField]
	private HierarchyReferences saveButtonPrefab;

	[SerializeField]
	private KButton loadMoreButton;

	[Space]
	[SerializeField]
	private KButton colonyCloudButton;

	[SerializeField]
	private KButton colonyLocalButton;

	[SerializeField]
	private KButton colonyInfoButton;

	[SerializeField]
	private Sprite localToCloudSprite;

	[SerializeField]
	private Sprite cloudToLocalSprite;

	[SerializeField]
	private Sprite errorSprite;

	[SerializeField]
	private Sprite infoSprite;

	[SerializeField]
	private Bouncer cloudTutorialBouncer;

	public bool requireConfirmation = true;

	private SelectedSave selectedSave = null;

	private List<SaveGameFileDetails> currentColony = null;

	private UIPool<HierarchyReferences> colonyListPool;

	private ConfirmDialogScreen confirmScreen;

	private InfoDialogScreen infoScreen;

	private InfoDialogScreen errorInfoScreen;

	private ConfirmDialogScreen errorScreen;

	private InspectSaveScreen inspectScreenInstance;

	private int displayedPageCount = 1;

	public static LoadScreen Instance { get; private set; }

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		Debug.Assert(Instance == null);
		Instance = this;
		base.OnPrefabInit();
		saveButtonPrefab.gameObject.SetActive(value: false);
		colonyListPool = new UIPool<HierarchyReferences>(saveButtonPrefab);
		if (SpeedControlScreen.Instance != null)
		{
			SpeedControlScreen.Instance.Pause(playSound: false);
		}
		if (closeButton != null)
		{
			closeButton.onClick += delegate
			{
				Deactivate();
			};
		}
		if (colonyCloudButton != null)
		{
			colonyCloudButton.onClick += delegate
			{
				ConvertAllToCloud();
			};
		}
		if (colonyLocalButton != null)
		{
			colonyLocalButton.onClick += delegate
			{
				ConvertAllToLocal();
			};
		}
		if (colonyInfoButton != null)
		{
			colonyInfoButton.onClick += delegate
			{
				ShowSaveInfo();
			};
		}
		if (loadMoreButton != null)
		{
			loadMoreButton.onClick += delegate
			{
				displayedPageCount++;
				RefreshColonyList();
				ShowColonyList();
			};
		}
	}

	private bool IsInMenu()
	{
		return App.GetCurrentSceneName() == "frontend";
	}

	private bool CloudSavesVisible()
	{
		if (!SaveLoader.GetCloudSavesAvailable())
		{
			return false;
		}
		return IsInMenu();
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		WorldGen.LoadSettings();
		SetCloudSaveInfoActive(CloudSavesVisible());
		displayedPageCount = 1;
		RefreshColonyList();
		ShowColonyList();
		bool cloudSavesAvailable = SaveLoader.GetCloudSavesAvailable();
		cloudTutorialBouncer.gameObject.SetActive(cloudSavesAvailable);
		if (cloudSavesAvailable && !cloudTutorialBouncer.IsBouncing())
		{
			int num = KPlayerPrefs.GetInt("LoadScreenCloudTutorialTimes", 0);
			if (num < 5)
			{
				cloudTutorialBouncer.Bounce();
				KPlayerPrefs.SetInt("LoadScreenCloudTutorialTimes", num + 1);
				int num2 = KPlayerPrefs.GetInt("LoadScreenCloudTutorialTimes", 0);
			}
			else
			{
				cloudTutorialBouncer.gameObject.SetActive(value: false);
			}
		}
		if (DistributionPlatform.Initialized && SteamUtils.IsSteamRunningOnSteamDeck())
		{
			colonyInfoButton.gameObject.SetActive(value: false);
		}
	}

	private Dictionary<string, List<SaveGameFileDetails>> GetColoniesDetails(List<SaveLoader.SaveFileEntry> files)
	{
		Dictionary<string, List<SaveGameFileDetails>> dictionary = new Dictionary<string, List<SaveGameFileDetails>>();
		if (files.Count <= 0)
		{
			return dictionary;
		}
		for (int i = 0; i < files.Count; i++)
		{
			if (IsFileValid(files[i].path))
			{
				Tuple<SaveGame.Header, SaveGame.GameInfo> fileInfo = SaveGame.GetFileInfo(files[i].path);
				SaveGame.Header first = fileInfo.first;
				SaveGame.GameInfo second = fileInfo.second;
				System.DateTime timeStamp = files[i].timeStamp;
				long size = 0L;
				try
				{
					FileInfo fileInfo2 = new FileInfo(files[i].path);
					size = fileInfo2.Length;
				}
				catch (Exception ex)
				{
					Debug.LogWarning("Failed to get size for file: " + files[i].ToString() + "\n" + ex.ToString());
				}
				SaveGameFileDetails item = new SaveGameFileDetails
				{
					BaseName = second.baseName,
					FileName = files[i].path,
					FileDate = timeStamp,
					FileHeader = first,
					FileInfo = second,
					Size = size,
					UniqueID = SaveGame.GetSaveUniqueID(second)
				};
				if (!dictionary.ContainsKey(item.UniqueID))
				{
					dictionary.Add(item.UniqueID, new List<SaveGameFileDetails>());
				}
				dictionary[item.UniqueID].Add(item);
			}
		}
		return dictionary;
	}

	private Dictionary<string, List<SaveGameFileDetails>> GetColonies(bool sort)
	{
		List<SaveLoader.SaveFileEntry> allFiles = SaveLoader.GetAllFiles(sort);
		return GetColoniesDetails(allFiles);
	}

	private Dictionary<string, List<SaveGameFileDetails>> GetLocalColonies(bool sort)
	{
		List<SaveLoader.SaveFileEntry> allFiles = SaveLoader.GetAllFiles(sort, SaveLoader.SaveType.local);
		return GetColoniesDetails(allFiles);
	}

	private Dictionary<string, List<SaveGameFileDetails>> GetCloudColonies(bool sort)
	{
		List<SaveLoader.SaveFileEntry> allFiles = SaveLoader.GetAllFiles(sort, SaveLoader.SaveType.cloud);
		return GetColoniesDetails(allFiles);
	}

	private bool IsFileValid(string filename)
	{
		bool result = false;
		try
		{
			result = SaveLoader.LoadHeader(filename, out var _).saveMajorVersion >= 7;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Corrupted save file: " + filename + "\n" + ex.ToString());
		}
		return result;
	}

	private void CheckCloudLocalOverlap()
	{
		if (!SaveLoader.GetCloudSavesAvailable())
		{
			return;
		}
		string cloudSavePrefix = SaveLoader.GetCloudSavePrefix();
		if (cloudSavePrefix == null)
		{
			return;
		}
		Dictionary<string, List<SaveGameFileDetails>> colonies = GetColonies(sort: false);
		foreach (KeyValuePair<string, List<SaveGameFileDetails>> item in colonies)
		{
			bool flag = false;
			List<SaveGameFileDetails> list = new List<SaveGameFileDetails>();
			foreach (SaveGameFileDetails item2 in item.Value)
			{
				if (SaveLoader.IsSaveCloud(item2.FileName))
				{
					flag = true;
				}
				else
				{
					list.Add(item2);
				}
			}
			if (!flag || list.Count == 0)
			{
				continue;
			}
			string baseName = list[0].BaseName;
			string path = System.IO.Path.Combine(SaveLoader.GetSavePrefix(), baseName);
			string text = System.IO.Path.Combine(cloudSavePrefix, baseName);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			Debug.Log("Saves / Found overlapped cloud/local saves for colony '" + baseName + "', moving to cloud...");
			foreach (SaveGameFileDetails item3 in list)
			{
				string fileName = item3.FileName;
				string source = System.IO.Path.ChangeExtension(fileName, "png");
				string path2 = text;
				if (SaveLoader.IsSaveAuto(fileName))
				{
					string text2 = System.IO.Path.Combine(path2, "auto_save");
					if (!Directory.Exists(text2))
					{
						Directory.CreateDirectory(text2);
					}
					path2 = text2;
				}
				string text3 = System.IO.Path.Combine(path2, System.IO.Path.GetFileName(fileName));
				if (FileMatch(fileName, text3, out var _))
				{
					Debug.Log("Saves / file match found for `" + fileName + "`...");
					MigrateFile(fileName, text3);
					string dest = System.IO.Path.ChangeExtension(text3, "png");
					MigrateFile(source, dest, ignoreMissing: true);
				}
				else
				{
					Debug.Log("Saves / no file match found for `" + fileName + "`... move as copy");
					string nextUsableSavePath = SaveLoader.GetNextUsableSavePath(text3);
					MigrateFile(fileName, nextUsableSavePath);
					string dest2 = System.IO.Path.ChangeExtension(nextUsableSavePath, "png");
					MigrateFile(source, dest2, ignoreMissing: true);
				}
			}
			RemoveEmptyFolder(path);
		}
	}

	private void DeleteFileAndEmptyFolder(string file)
	{
		if (File.Exists(file))
		{
			File.Delete(file);
		}
		RemoveEmptyFolder(System.IO.Path.GetDirectoryName(file));
	}

	private void RemoveEmptyFolder(string path)
	{
		if (!Directory.Exists(path) || !File.GetAttributes(path).HasFlag(FileAttributes.Directory) || Directory.EnumerateFileSystemEntries(path).Any())
		{
			return;
		}
		try
		{
			Directory.Delete(path);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to remove empty directory `" + path + "`...");
			Debug.LogWarning(ex);
		}
	}

	private void RefreshColonyList()
	{
		if (colonyListPool != null)
		{
			colonyListPool.ClearAll();
		}
		CheckCloudLocalOverlap();
		Dictionary<string, List<SaveGameFileDetails>> colonies = GetColonies(sort: true);
		if (colonies.Count <= 0)
		{
			return;
		}
		int num = 0;
		foreach (KeyValuePair<string, List<SaveGameFileDetails>> item in colonies)
		{
			if (num >= displayedPageCount * 20)
			{
				break;
			}
			AddColonyToList(item.Value);
			num++;
		}
		loadMoreButton.gameObject.SetActive(colonies.Count != num);
		loadMoreButton.gameObject.transform.SetAsLastSibling();
	}

	private string GetFileHash(string path)
	{
		using MD5 mD = MD5.Create();
		using FileStream inputStream = File.OpenRead(path);
		byte[] array = mD.ComputeHash(inputStream);
		return BitConverter.ToString(array).Replace("-", "").ToLowerInvariant();
	}

	private bool FileMatch(string file, string other_file, out Tuple<bool, bool> matches)
	{
		matches = new Tuple<bool, bool>(a: false, b: false);
		if (!File.Exists(file))
		{
			return false;
		}
		if (!File.Exists(other_file))
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		try
		{
			string fileHash = GetFileHash(file);
			string fileHash2 = GetFileHash(other_file);
			FileInfo fileInfo = new FileInfo(file);
			FileInfo fileInfo2 = new FileInfo(other_file);
			flag = fileInfo.Length == fileInfo2.Length;
			flag2 = fileHash == fileHash2;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("FileMatch / file match failed for `" + file + "` vs `" + other_file + "`!");
			Debug.LogWarning(ex);
			return false;
		}
		matches.first = flag;
		matches.second = flag2;
		return flag && flag2;
	}

	private bool MigrateFile(string source, string dest, bool ignoreMissing = false)
	{
		Debug.Log("Migration / moving `" + source + "` to `" + dest + "` ...");
		if (dest == source)
		{
			Debug.Log("Migration / ignored `" + source + "` to `" + dest + "` ... same location");
			return true;
		}
		if (FileMatch(source, dest, out var _))
		{
			Debug.Log("Migration / dest and source are identical size + hash ... removing original");
			try
			{
				DeleteFileAndEmptyFolder(source);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Migration / removing original failed for `" + source + "`!");
				Debug.LogWarning(ex);
				throw ex;
			}
			return true;
		}
		try
		{
			Debug.Log("Migration / copying...");
			File.Copy(source, dest, overwrite: false);
		}
		catch (FileNotFoundException) when (ignoreMissing)
		{
			Debug.Log("Migration / File `" + source + "` wasn't found but we're ignoring that.");
			return true;
		}
		catch (Exception ex3)
		{
			Debug.LogWarning("Migration / copy failed for `" + source + "`! Leaving it alone");
			Debug.LogWarning(ex3);
			Debug.LogWarning("failed to convert colony: " + ex3.ToString());
			throw ex3;
		}
		Debug.Log("Migration / copy ok ...");
		if (!FileMatch(source, dest, out var matches2))
		{
			Debug.LogWarning("Migration / failed to match dest file for `" + source + "`!");
			Debug.LogWarning($"Migration / did hash match? {matches2.second} did size match? {matches2.first}");
			throw new Exception("Hash/Size didn't match for source and destination");
		}
		Debug.Log("Migration / hash validation ok ... removing original");
		try
		{
			DeleteFileAndEmptyFolder(source);
		}
		catch (Exception ex4)
		{
			Debug.LogWarning("Migration / removing original failed for `" + source + "`!");
			Debug.LogWarning(ex4);
			throw ex4;
		}
		Debug.Log("Migration / moved ok for `" + source + "`!");
		return true;
	}

	private bool MigrateSave(string dest_root, string file, bool is_auto_save, out string saveError)
	{
		saveError = null;
		Tuple<SaveGame.Header, SaveGame.GameInfo> fileInfo = SaveGame.GetFileInfo(file);
		SaveGame.Header first = fileInfo.first;
		SaveGame.GameInfo second = fileInfo.second;
		string path = second.baseName.TrimEnd(' ');
		string fileName = System.IO.Path.GetFileName(file);
		string text = System.IO.Path.Combine(dest_root, path);
		if (!Directory.Exists(text))
		{
			text = Directory.CreateDirectory(text).FullName;
		}
		string path2 = text;
		if (is_auto_save)
		{
			string text2 = System.IO.Path.Combine(text, "auto_save");
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			path2 = text2;
		}
		string text3 = System.IO.Path.Combine(path2, fileName);
		string source = System.IO.Path.ChangeExtension(file, "png");
		string dest = System.IO.Path.ChangeExtension(text3, "png");
		try
		{
			MigrateFile(file, text3);
			MigrateFile(source, dest, ignoreMissing: true);
		}
		catch (Exception ex)
		{
			saveError = ex.Message;
			return false;
		}
		return true;
	}

	private (int, int, ulong) GetSavesSizeAndCounts(List<SaveGameFileDetails> list)
	{
		ulong num = 0uL;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < list.Count; i++)
		{
			SaveGameFileDetails saveGameFileDetails = list[i];
			num += (ulong)saveGameFileDetails.Size;
			if (saveGameFileDetails.FileInfo.isAutoSave)
			{
				num3++;
			}
			else
			{
				num2++;
			}
		}
		return (num2, num3, num);
	}

	private int CountValidSaves(string path, SearchOption searchType = SearchOption.AllDirectories)
	{
		int num = 0;
		List<SaveLoader.SaveFileEntry> saveFiles = SaveLoader.GetSaveFiles(path, sort: false, searchType);
		for (int i = 0; i < saveFiles.Count; i++)
		{
			if (IsFileValid(saveFiles[i].path))
			{
				num++;
			}
		}
		return num;
	}

	private (int, int) GetMigrationSaveCounts()
	{
		int item = CountValidSaves(SaveLoader.GetSavePrefixAndCreateFolder(), SearchOption.TopDirectoryOnly);
		int item2 = CountValidSaves(SaveLoader.GetAutoSavePrefix());
		return (item, item2);
	}

	private (int, int) MigrateSaves(out string errorColony, out string errorMessage)
	{
		errorColony = null;
		errorMessage = null;
		int num = 0;
		string savePrefixAndCreateFolder = SaveLoader.GetSavePrefixAndCreateFolder();
		List<SaveLoader.SaveFileEntry> saveFiles = SaveLoader.GetSaveFiles(savePrefixAndCreateFolder, sort: false, SearchOption.TopDirectoryOnly);
		for (int i = 0; i < saveFiles.Count; i++)
		{
			SaveLoader.SaveFileEntry saveFileEntry = saveFiles[i];
			if (IsFileValid(saveFileEntry.path))
			{
				if (MigrateSave(savePrefixAndCreateFolder, saveFileEntry.path, is_auto_save: false, out var saveError))
				{
					num++;
				}
				else if (errorColony == null)
				{
					errorColony = saveFileEntry.path;
					errorMessage = saveError;
				}
			}
		}
		int num2 = 0;
		string autoSavePrefix = SaveLoader.GetAutoSavePrefix();
		List<SaveLoader.SaveFileEntry> saveFiles2 = SaveLoader.GetSaveFiles(autoSavePrefix, sort: false);
		for (int j = 0; j < saveFiles2.Count; j++)
		{
			SaveLoader.SaveFileEntry saveFileEntry2 = saveFiles2[j];
			if (IsFileValid(saveFileEntry2.path))
			{
				if (MigrateSave(savePrefixAndCreateFolder, saveFileEntry2.path, is_auto_save: true, out var saveError2))
				{
					num2++;
				}
				else if (errorColony == null)
				{
					errorColony = saveFileEntry2.path;
					errorMessage = saveError2;
				}
			}
		}
		return (num, num2);
	}

	public void ShowMigrationIfNecessary(bool fromMainMenu)
	{
		var (saveCount, autoCount) = GetMigrationSaveCounts();
		if (saveCount == 0 && autoCount == 0)
		{
			if (fromMainMenu)
			{
				Deactivate();
			}
			return;
		}
		Activate();
		migrationPanelRefs.gameObject.SetActive(value: true);
		KButton migrateButton = migrationPanelRefs.GetReference<RectTransform>("MigrateSaves").GetComponent<KButton>();
		KButton continueButton = migrationPanelRefs.GetReference<RectTransform>("Continue").GetComponent<KButton>();
		KButton moreInfoButton = migrationPanelRefs.GetReference<RectTransform>("MoreInfo").GetComponent<KButton>();
		KButton component = migrationPanelRefs.GetReference<RectTransform>("OpenSaves").GetComponent<KButton>();
		LocText statsText = migrationPanelRefs.GetReference<RectTransform>("CountText").GetComponent<LocText>();
		LocText infoText = migrationPanelRefs.GetReference<RectTransform>("InfoText").GetComponent<LocText>();
		migrateButton.gameObject.SetActive(value: true);
		continueButton.gameObject.SetActive(value: false);
		moreInfoButton.gameObject.SetActive(value: false);
		statsText.text = string.Format(UI.FRONTEND.LOADSCREEN.MIGRATE_COUNT, saveCount, autoCount);
		component.ClearOnClick();
		component.onClick += delegate
		{
			App.OpenWebURL(SaveLoader.GetSavePrefixAndCreateFolder());
		};
		migrateButton.ClearOnClick();
		migrateButton.onClick += delegate
		{
			migrateButton.gameObject.SetActive(value: false);
			string errorColony;
			string errorMessage;
			(int, int) tuple2 = MigrateSaves(out errorColony, out errorMessage);
			int item = tuple2.Item1;
			int item2 = tuple2.Item2;
			bool flag = errorColony == null;
			string format = (flag ? UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT.text : UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES.Replace("{ErrorColony}", errorColony).Replace("{ErrorMessage}", errorMessage));
			statsText.text = string.Format(format, item, saveCount, item2, autoCount);
			infoText.gameObject.SetActive(value: false);
			if (flag)
			{
				continueButton.gameObject.SetActive(value: true);
			}
			else
			{
				moreInfoButton.gameObject.SetActive(value: true);
			}
			MainMenu.Instance.RefreshResumeButton();
			RefreshColonyList();
		};
		continueButton.ClearOnClick();
		continueButton.onClick += delegate
		{
			migrationPanelRefs.gameObject.SetActive(value: false);
			cloudTutorialBouncer.Bounce();
		};
		moreInfoButton.ClearOnClick();
		moreInfoButton.onClick += delegate
		{
			if (DistributionPlatform.Initialized && SteamUtils.IsSteamRunningOnSteamDeck())
			{
				InfoDialogScreen infoDialogScreen = Util.KInstantiateUI<InfoDialogScreen>(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, base.gameObject).SetHeader(UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES_MORE_INFO_TITLE).AddPlainText(UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES_MORE_INFO_PRE)
					.AddLineItem(UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES_MORE_INFO_ITEM1, "")
					.AddLineItem(UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES_MORE_INFO_ITEM2, "")
					.AddLineItem(UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES_MORE_INFO_ITEM3, "")
					.AddPlainText(UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES_MORE_INFO_POST)
					.AddOption(UI.CONFIRMDIALOG.OK, delegate(InfoDialogScreen d)
					{
						migrationPanelRefs.gameObject.SetActive(value: false);
						cloudTutorialBouncer.Bounce();
						d.Deactivate();
					}, rightSide: true);
				infoDialogScreen.Activate();
			}
			else
			{
				InfoDialogScreen infoDialogScreen2 = Util.KInstantiateUI<InfoDialogScreen>(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, base.gameObject).SetHeader(UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES_MORE_INFO_TITLE).AddPlainText(UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES_MORE_INFO_PRE)
					.AddLineItem(UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES_MORE_INFO_ITEM1, "")
					.AddLineItem(UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES_MORE_INFO_ITEM2, "")
					.AddLineItem(UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES_MORE_INFO_ITEM3, "")
					.AddPlainText(UI.FRONTEND.LOADSCREEN.MIGRATE_RESULT_FAILURES_MORE_INFO_POST)
					.AddOption(UI.FRONTEND.LOADSCREEN.MIGRATE_FAILURES_FORUM_BUTTON, delegate
					{
						App.OpenWebURL("https://forums.kleientertainment.com/klei-bug-tracker/oni/");
					})
					.AddOption(UI.CONFIRMDIALOG.OK, delegate(InfoDialogScreen d)
					{
						migrationPanelRefs.gameObject.SetActive(value: false);
						cloudTutorialBouncer.Bounce();
						d.Deactivate();
					}, rightSide: true);
				infoDialogScreen2.Activate();
			}
		};
	}

	private void SetCloudSaveInfoActive(bool active)
	{
		colonyCloudButton.gameObject.SetActive(active);
		colonyLocalButton.gameObject.SetActive(active);
	}

	private bool ConvertToLocalOrCloud(string fromRoot, string destRoot, string colonyName)
	{
		string text = System.IO.Path.Combine(fromRoot, colonyName);
		string text2 = System.IO.Path.Combine(destRoot, colonyName);
		Debug.Log("Convert / Colony '" + colonyName + "' from `" + text + "` => `" + text2 + "`");
		try
		{
			Directory.Move(text, text2);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("failed to convert colony: " + ex.ToString());
			string message = UI.FRONTEND.LOADSCREEN.CONVERT_ERROR.Replace("{Colony}", colonyName).Replace("{Error}", ex.Message);
			ShowConvertError(message);
		}
		return false;
	}

	private bool ConvertColonyToCloud(string colonyName)
	{
		string savePrefix = SaveLoader.GetSavePrefix();
		string cloudSavePrefix = SaveLoader.GetCloudSavePrefix();
		if (cloudSavePrefix == null)
		{
			Debug.LogWarning("Failed to move colony to cloud, no cloud save prefix found (usually a userID is missing, not logged in?)");
			return false;
		}
		return ConvertToLocalOrCloud(savePrefix, cloudSavePrefix, colonyName);
	}

	private bool ConvertColonyToLocal(string colonyName)
	{
		string savePrefix = SaveLoader.GetSavePrefix();
		string cloudSavePrefix = SaveLoader.GetCloudSavePrefix();
		if (cloudSavePrefix == null)
		{
			Debug.LogWarning("Failed to move colony from cloud, no cloud save prefix found (usually a userID is missing, not logged in?)");
			return false;
		}
		return ConvertToLocalOrCloud(cloudSavePrefix, savePrefix, colonyName);
	}

	private void DoConvertAllToLocal()
	{
		Dictionary<string, List<SaveGameFileDetails>> cloudColonies = GetCloudColonies(sort: false);
		if (cloudColonies.Count == 0)
		{
			return;
		}
		bool flag = true;
		foreach (KeyValuePair<string, List<SaveGameFileDetails>> item in cloudColonies)
		{
			flag &= ConvertColonyToLocal(item.Value[0].BaseName);
		}
		if (flag)
		{
			string replacement = UI.PLATFORMS.STEAM;
			ShowSimpleDialog(UI.FRONTEND.LOADSCREEN.CONVERT_TO_LOCAL, UI.FRONTEND.LOADSCREEN.CONVERT_ALL_TO_LOCAL_SUCCESS.Replace("{Client}", replacement));
		}
		RefreshColonyList();
		MainMenu.Instance.RefreshResumeButton();
		SaveLoader.SetCloudSavesDefault(value: false);
	}

	private void DoConvertAllToCloud()
	{
		Dictionary<string, List<SaveGameFileDetails>> localColonies = GetLocalColonies(sort: false);
		if (localColonies.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, List<SaveGameFileDetails>> item in localColonies)
		{
			string baseName = item.Value[0].BaseName;
			if (!list.Contains(baseName))
			{
				list.Add(baseName);
			}
		}
		bool flag = true;
		foreach (string item2 in list)
		{
			flag &= ConvertColonyToCloud(item2);
		}
		if (flag)
		{
			string replacement = UI.PLATFORMS.STEAM;
			ShowSimpleDialog(UI.FRONTEND.LOADSCREEN.CONVERT_TO_CLOUD, UI.FRONTEND.LOADSCREEN.CONVERT_ALL_TO_CLOUD_SUCCESS.Replace("{Client}", replacement));
		}
		RefreshColonyList();
		MainMenu.Instance.RefreshResumeButton();
		SaveLoader.SetCloudSavesDefault(value: true);
	}

	private void ConvertAllToCloud()
	{
		string message = $"{UI.FRONTEND.LOADSCREEN.CONVERT_TO_CLOUD_DETAILS}\n{UI.FRONTEND.LOADSCREEN.CONVERT_ALL_WARNING}\n";
		KPlayerPrefs.SetInt("LoadScreenCloudTutorialTimes", 5);
		ConfirmCloudSaveMigrations(message, UI.FRONTEND.LOADSCREEN.CONVERT_TO_CLOUD, UI.FRONTEND.LOADSCREEN.CONVERT_ALL_COLONIES, UI.FRONTEND.LOADSCREEN.OPEN_SAVE_FOLDER, delegate
		{
			DoConvertAllToCloud();
		}, delegate
		{
			App.OpenWebURL(SaveLoader.GetSavePrefix());
		}, localToCloudSprite);
	}

	private void ConvertAllToLocal()
	{
		string message = $"{UI.FRONTEND.LOADSCREEN.CONVERT_TO_LOCAL_DETAILS}\n{UI.FRONTEND.LOADSCREEN.CONVERT_ALL_WARNING}\n";
		KPlayerPrefs.SetInt("LoadScreenCloudTutorialTimes", 5);
		ConfirmCloudSaveMigrations(message, UI.FRONTEND.LOADSCREEN.CONVERT_TO_LOCAL, UI.FRONTEND.LOADSCREEN.CONVERT_ALL_COLONIES, UI.FRONTEND.LOADSCREEN.OPEN_SAVE_FOLDER, delegate
		{
			DoConvertAllToLocal();
		}, delegate
		{
			App.OpenWebURL(SaveLoader.GetCloudSavePrefix());
		}, cloudToLocalSprite);
	}

	private void ShowSaveInfo()
	{
		if (!(infoScreen == null))
		{
			return;
		}
		infoScreen = Util.KInstantiateUI<InfoDialogScreen>(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, base.gameObject).SetHeader(UI.FRONTEND.LOADSCREEN.SAVE_INFO_DIALOG_TITLE).AddSprite(infoSprite)
			.AddPlainText(UI.FRONTEND.LOADSCREEN.SAVE_INFO_DIALOG_TEXT)
			.AddOption(UI.FRONTEND.LOADSCREEN.OPEN_SAVE_FOLDER, delegate
			{
				App.OpenWebURL(SaveLoader.GetSavePrefix());
			}, rightSide: true)
			.AddDefaultCancel();
		string cloudRoot = SaveLoader.GetCloudSavePrefix();
		if (cloudRoot != null && CloudSavesVisible())
		{
			infoScreen.AddOption(UI.FRONTEND.LOADSCREEN.OPEN_CLOUDSAVE_FOLDER, delegate
			{
				App.OpenWebURL(cloudRoot);
			}, rightSide: true);
		}
		infoScreen.gameObject.SetActive(value: true);
	}

	protected override void OnDeactivate()
	{
		if (SpeedControlScreen.Instance != null)
		{
			SpeedControlScreen.Instance.Unpause(playSound: false);
		}
		selectedSave = null;
		base.OnDeactivate();
	}

	private void ShowColonyList()
	{
		colonyListRoot.SetActive(value: true);
		colonyViewRoot.SetActive(value: false);
		currentColony = null;
		selectedSave = null;
	}

	private bool CheckSaveVersion(SaveGameFileDetails save, LocText display)
	{
		if (IsSaveFileFromUnsupportedFutureBuild(save.FileHeader, save.FileInfo))
		{
			if (display != null)
			{
				display.text = string.Format(UI.FRONTEND.LOADSCREEN.SAVE_TOO_NEW, save.FileName, save.FileHeader.buildVersion, save.FileInfo.saveMinorVersion, 732169u, 37);
			}
			return false;
		}
		if (save.FileInfo.saveMajorVersion < 7)
		{
			if (display != null)
			{
				display.text = string.Format(UI.FRONTEND.LOADSCREEN.UNSUPPORTED_SAVE_VERSION, save.FileName, save.FileInfo.saveMajorVersion, save.FileInfo.saveMinorVersion, 7, 37);
			}
			return false;
		}
		return true;
	}

	private bool CheckSaveDLCsCompatable(SaveGameFileDetails save)
	{
		HashSet<string> dlcIdsToEnable;
		HashSet<string> dlcIdToDisable;
		return save.FileInfo.IsCompatableWithCurrentDlcConfiguration(out dlcIdsToEnable, out dlcIdToDisable);
	}

	private string GetSaveDLCIncompatabilityTooltip(SaveGameFileDetails save)
	{
		string text = "";
		if (save.FileInfo.IsCompatableWithCurrentDlcConfiguration(out var dlcIdsToEnable, out var dlcIdToDisable))
		{
			text = null;
		}
		else
		{
			text = UI.FRONTEND.LOADSCREEN.TOOLTIP_SAVE_INCOMPATABLE_DLC_CONFIGURATION;
			foreach (string item in dlcIdsToEnable)
			{
				text = text + "\n" + string.Format(UI.FRONTEND.LOADSCREEN.TOOLTIP_SAVE_INCOMPATABLE_DLC_CONFIGURATION_ASK_TO_ENABLE, DlcManager.GetDlcTitle(item));
			}
			foreach (string item2 in dlcIdToDisable)
			{
				text = text + "\n" + string.Format(UI.FRONTEND.LOADSCREEN.TOOLTIP_SAVE_INCOMPATABLE_DLC_CONFIGURATION_ASK_TO_DISABLE, DlcManager.GetDlcTitle(item2));
			}
		}
		return text;
	}

	private void ShowColonySave(SaveGameFileDetails save)
	{
		HierarchyReferences component = colonyViewRoot.GetComponent<HierarchyReferences>();
		LocText component2 = component.GetReference<RectTransform>("Title").GetComponent<LocText>();
		component2.text = save.BaseName;
		LocText component3 = component.GetReference<RectTransform>("Date").GetComponent<LocText>();
		component3.text = string.Format("{0:H:mm:ss} - " + Localization.GetFileDateFormat(0), save.FileDate.ToLocalTime());
		string text = save.FileInfo.clusterId;
		if (text != null && !SettingsCache.clusterLayouts.clusterCache.ContainsKey(text))
		{
			string text2 = SettingsCache.GetScope("EXPANSION1_ID") + text;
			if (SettingsCache.clusterLayouts.clusterCache.ContainsKey(text2))
			{
				text = text2;
			}
			else
			{
				DebugUtil.LogWarningArgs("Failed to find cluster " + text + " including the scoped path, setting to default cluster name.");
				Debug.Log("ClusterCache: " + string.Join(",", SettingsCache.clusterLayouts.clusterCache.Keys));
				text = WorldGenSettings.ClusterDefaultName;
			}
		}
		ProcGen.World world = ((text != null) ? SettingsCache.clusterLayouts.GetWorldData(text, 0) : null);
		string arg = ((world != null) ? ((string)Strings.Get(world.name)) : " - ");
		LocText reference = component.GetReference<LocText>("InfoWorld");
		reference.text = string.Format(UI.FRONTEND.LOADSCREEN.COLONY_INFO_FMT, UI.FRONTEND.LOADSCREEN.WORLD_NAME, arg);
		LocText reference2 = component.GetReference<LocText>("InfoCycles");
		reference2.text = string.Format(UI.FRONTEND.LOADSCREEN.COLONY_INFO_FMT, UI.FRONTEND.LOADSCREEN.CYCLES_SURVIVED, save.FileInfo.numberOfCycles);
		LocText reference3 = component.GetReference<LocText>("InfoDupes");
		reference3.text = string.Format(UI.FRONTEND.LOADSCREEN.COLONY_INFO_FMT, UI.FRONTEND.LOADSCREEN.DUPLICANTS_ALIVE, save.FileInfo.numberOfDuplicants);
		LocText component4 = component.GetReference<RectTransform>("FileSize").GetComponent<LocText>();
		string formattedBytes = GameUtil.GetFormattedBytes((ulong)save.Size);
		component4.text = string.Format(UI.FRONTEND.LOADSCREEN.COLONY_FILE_SIZE, formattedBytes);
		LocText component5 = component.GetReference<RectTransform>("Filename").GetComponent<LocText>();
		component5.text = string.Format(UI.FRONTEND.LOADSCREEN.COLONY_FILE_NAME, System.IO.Path.GetFileName(save.FileName));
		LocText component6 = component.GetReference<RectTransform>("AutoInfo").GetComponent<LocText>();
		component6.gameObject.SetActive(!CheckSaveVersion(save, component6));
		Image component7 = component.GetReference<RectTransform>("Preview").GetComponent<Image>();
		SetPreview(save.FileName, save.BaseName, component7);
		KButton component8 = component.GetReference<RectTransform>("DeleteButton").GetComponent<KButton>();
		component8.ClearOnClick();
		component8.onClick += delegate
		{
			Delete(delegate
			{
				int num = currentColony.IndexOf(save);
				currentColony.Remove(save);
				ShowColony(currentColony, num - 1);
			});
		};
	}

	private void ShowColony(List<SaveGameFileDetails> saves, int selectIndex = -1)
	{
		if (saves.Count <= 0)
		{
			RefreshColonyList();
			ShowColonyList();
			return;
		}
		currentColony = saves;
		colonyListRoot.SetActive(value: false);
		colonyViewRoot.SetActive(value: true);
		string baseName = saves[0].BaseName;
		HierarchyReferences component = colonyViewRoot.GetComponent<HierarchyReferences>();
		KButton component2 = component.GetReference<RectTransform>("Back").GetComponent<KButton>();
		component2.ClearOnClick();
		component2.onClick += delegate
		{
			ShowColonyList();
		};
		LocText component3 = component.GetReference<RectTransform>("ColonyTitle").GetComponent<LocText>();
		component3.text = string.Format(UI.FRONTEND.LOADSCREEN.COLONY_TITLE, baseName);
		GameObject gameObject = component.GetReference<RectTransform>("Content").gameObject;
		RectTransform reference = component.GetReference<RectTransform>("SaveTemplate");
		for (int num = 0; num < gameObject.transform.childCount; num++)
		{
			GameObject gameObject2 = gameObject.transform.GetChild(num).gameObject;
			if (gameObject2 != null && gameObject2.name.Contains("Clone"))
			{
				UnityEngine.Object.Destroy(gameObject2);
			}
		}
		if (selectIndex < 0)
		{
			selectIndex = 0;
		}
		if (selectIndex > saves.Count - 1)
		{
			selectIndex = saves.Count - 1;
		}
		for (int num2 = 0; num2 < saves.Count; num2++)
		{
			SaveGameFileDetails save = saves[num2];
			RectTransform rectTransform = UnityEngine.Object.Instantiate(reference, gameObject.transform);
			HierarchyReferences component4 = rectTransform.GetComponent<HierarchyReferences>();
			rectTransform.gameObject.SetActive(value: true);
			RectTransform reference2 = component4.GetReference<RectTransform>("AutoLabel");
			reference2.gameObject.SetActive(save.FileInfo.isAutoSave);
			LocText component5 = component4.GetReference<RectTransform>("SaveText").GetComponent<LocText>();
			component5.text = System.IO.Path.GetFileNameWithoutExtension(save.FileName);
			LocText component6 = component4.GetReference<RectTransform>("DateText").GetComponent<LocText>();
			component6.text = string.Format("{0:H:mm:ss} - " + Localization.GetFileDateFormat(0), save.FileDate.ToLocalTime());
			RectTransform reference3 = component4.GetReference<RectTransform>("NewestLabel");
			reference3.gameObject.SetActive(num2 == 0);
			RectTransform reference4 = component4.GetReference<RectTransform>("DLCIconPrefab");
			foreach (string dlcId in save.FileInfo.dlcIds)
			{
				if (DlcManager.IsUnknownDlc(dlcId))
				{
					GameObject gameObject3 = Util.KInstantiateUI(reference4.gameObject, reference4.transform.parent.gameObject, force_active: true);
					Image component7 = gameObject3.GetComponent<Image>();
					component7.sprite = Assets.GetSprite(DlcManager.GetDlcSmallLogo(dlcId));
					gameObject3.GetComponent<ToolTip>().SetSimpleTooltip(string.Format(UI.FRONTEND.LOADSCREEN.TOOLTIP_SAVE_USES_DLC, UI.UNKNOWN_DLC.NAME));
				}
			}
			for (int num3 = DlcManager.RELEASED_VERSIONS.Count - 1; num3 >= 0; num3--)
			{
				string text = DlcManager.RELEASED_VERSIONS[num3];
				if (!DlcManager.IsVanillaId(text) && save.FileInfo.dlcIds.Contains(text))
				{
					GameObject gameObject4 = Util.KInstantiateUI(reference4.gameObject, reference4.transform.parent.gameObject, force_active: true);
					gameObject4.GetComponent<Image>().sprite = Assets.GetSprite(DlcManager.GetDlcSmallLogo(text));
					gameObject4.GetComponent<ToolTip>().SetSimpleTooltip(string.Format(UI.FRONTEND.LOADSCREEN.TOOLTIP_SAVE_USES_DLC, DlcManager.GetDlcTitle(text)));
				}
			}
			bool flag = CheckSaveVersion(save, null) && CheckSaveDLCsCompatable(save);
			KButton button = rectTransform.GetComponent<KButton>();
			button.ClearOnClick();
			button.onClick += delegate
			{
				UpdateSelected(button, save.FileName, save.FileInfo.dlcIds);
				ShowColonySave(save);
			};
			if (flag)
			{
				button.onDoubleClick += delegate
				{
					UpdateSelected(button, save.FileName, save.FileInfo.dlcIds);
					Load();
				};
			}
			KButton component8 = component4.GetReference<RectTransform>("LoadButton").GetComponent<KButton>();
			component8.ClearOnClick();
			if (!flag)
			{
				component8.isInteractable = false;
				component8.GetComponent<ImageToggleState>().SetState(ImageToggleState.State.Disabled);
				component8.GetComponent<ToolTip>().SetSimpleTooltip(GetSaveDLCIncompatabilityTooltip(save));
			}
			else
			{
				component8.onClick += delegate
				{
					UpdateSelected(button, save.FileName, save.FileInfo.dlcIds);
					Load();
				};
			}
			if (num2 == selectIndex)
			{
				UpdateSelected(button, save.FileName, save.FileInfo.dlcIds);
				ShowColonySave(save);
			}
		}
	}

	private void AddColonyToList(List<SaveGameFileDetails> saves)
	{
		if (saves.Count == 0)
		{
			return;
		}
		HierarchyReferences freeElement = colonyListPool.GetFreeElement(saveButtonRoot, forceActive: true);
		saves.Sort((SaveGameFileDetails x, SaveGameFileDetails y) => y.FileDate.CompareTo(x.FileDate));
		SaveGameFileDetails save = saves[0];
		string colonyName = save.BaseName;
		(int, int, ulong) savesSizeAndCounts = GetSavesSizeAndCounts(saves);
		int item = savesSizeAndCounts.Item1;
		int item2 = savesSizeAndCounts.Item2;
		ulong item3 = savesSizeAndCounts.Item3;
		string formattedBytes = GameUtil.GetFormattedBytes(item3);
		LocText component = freeElement.GetReference<RectTransform>("HeaderTitle").GetComponent<LocText>();
		component.text = colonyName;
		LocText component2 = freeElement.GetReference<RectTransform>("HeaderDate").GetComponent<LocText>();
		component2.text = string.Format("{0:H:mm:ss} - " + Localization.GetFileDateFormat(0), save.FileDate.ToLocalTime());
		LocText component3 = freeElement.GetReference<RectTransform>("SaveTitle").GetComponent<LocText>();
		component3.text = string.Format(UI.FRONTEND.LOADSCREEN.SAVE_INFO, item, item2, formattedBytes);
		Image component4 = freeElement.GetReference<RectTransform>("Preview").GetComponent<Image>();
		SetPreview(save.FileName, colonyName, component4, fallbackToTimelapse: true);
		List<(Sprite, string)> list = new List<(Sprite, string)>();
		foreach (string rELEASED_VERSION in DlcManager.RELEASED_VERSIONS)
		{
			if (!DlcManager.IsVanillaId(rELEASED_VERSION) && save.FileInfo.dlcIds.Contains(rELEASED_VERSION))
			{
				list.Add((Assets.GetSprite(DlcManager.GetDlcSmallLogo(rELEASED_VERSION)), string.Format(UI.FRONTEND.LOADSCREEN.TOOLTIP_SAVE_USES_DLC, DlcManager.GetDlcTitle(rELEASED_VERSION))));
			}
		}
		foreach (string dlcId in save.FileInfo.dlcIds)
		{
			if (DlcManager.IsUnknownDlc(dlcId))
			{
				list.Add((Assets.GetSprite(DlcManager.GetDlcSmallLogo("unknown")), string.Format(UI.FRONTEND.LOADSCREEN.TOOLTIP_SAVE_USES_DLC, UI.UNKNOWN_DLC.NAME)));
			}
		}
		GameObject gameObject = freeElement.transform.Find("Header").Find("DlcIcons").Find("Prefab_DlcIcon")
			.gameObject;
		gameObject.SetActive(value: false);
		for (int num = 0; num < gameObject.transform.parent.childCount; num++)
		{
			GameObject gameObject2 = gameObject.transform.parent.GetChild(num).gameObject;
			if (gameObject2 != gameObject)
			{
				UnityEngine.Object.Destroy(gameObject2);
			}
		}
		foreach (var item4 in list)
		{
			GameObject gameObject3 = Util.KInstantiateUI(gameObject, gameObject.transform.parent.gameObject, force_active: true);
			Image component5 = gameObject3.GetComponent<Image>();
			ToolTip component6 = gameObject3.GetComponent<ToolTip>();
			(component5.sprite, _) = item4;
			component6.SetSimpleTooltip(item4.Item2);
		}
		RectTransform reference = freeElement.GetReference<RectTransform>("LocationIcons");
		bool flag = CloudSavesVisible();
		reference.gameObject.SetActive(flag);
		if (flag)
		{
			LocText locationText = freeElement.GetReference<RectTransform>("LocationText").GetComponent<LocText>();
			bool isLocal = SaveLoader.IsSaveLocal(save.FileName);
			locationText.text = (isLocal ? UI.FRONTEND.LOADSCREEN.LOCAL_SAVE : UI.FRONTEND.LOADSCREEN.CLOUD_SAVE);
			KButton cloudButton = freeElement.GetReference<RectTransform>("CloudButton").GetComponent<KButton>();
			KButton localButton = freeElement.GetReference<RectTransform>("LocalButton").GetComponent<KButton>();
			cloudButton.gameObject.SetActive(!isLocal);
			cloudButton.ClearOnClick();
			cloudButton.onClick += delegate
			{
				string message = $"{UI.FRONTEND.LOADSCREEN.CONVERT_TO_LOCAL_DETAILS}\n";
				ConfirmCloudSaveMigrations(message, UI.FRONTEND.LOADSCREEN.CONVERT_TO_LOCAL, UI.FRONTEND.LOADSCREEN.CONVERT_COLONY, null, delegate
				{
					cloudButton.gameObject.SetActive(value: false);
					isLocal = true;
					locationText.text = (isLocal ? UI.FRONTEND.LOADSCREEN.LOCAL_SAVE : UI.FRONTEND.LOADSCREEN.CLOUD_SAVE);
					ConvertColonyToLocal(colonyName);
					RefreshColonyList();
					MainMenu.Instance.RefreshResumeButton();
				}, null, cloudToLocalSprite);
			};
			localButton.gameObject.SetActive(isLocal);
			localButton.ClearOnClick();
			localButton.onClick += delegate
			{
				string message = $"{UI.FRONTEND.LOADSCREEN.CONVERT_TO_CLOUD_DETAILS}\n";
				ConfirmCloudSaveMigrations(message, UI.FRONTEND.LOADSCREEN.CONVERT_TO_CLOUD, UI.FRONTEND.LOADSCREEN.CONVERT_COLONY, null, delegate
				{
					localButton.gameObject.SetActive(value: false);
					isLocal = false;
					locationText.text = (isLocal ? UI.FRONTEND.LOADSCREEN.LOCAL_SAVE : UI.FRONTEND.LOADSCREEN.CLOUD_SAVE);
					ConvertColonyToCloud(colonyName);
					RefreshColonyList();
					MainMenu.Instance.RefreshResumeButton();
				}, null, localToCloudSprite);
			};
		}
		string saveDLCIncompatabilityTooltip = GetSaveDLCIncompatabilityTooltip(save);
		KButton component7 = freeElement.GetReference<RectTransform>("Button").GetComponent<KButton>();
		component7.onClick += delegate
		{
			ShowColony(saves);
		};
		freeElement.transform.SetAsLastSibling();
	}

	private void SetPreview(string filename, string basename, Image preview, bool fallbackToTimelapse = false)
	{
		preview.color = Color.black;
		preview.gameObject.SetActive(value: false);
		try
		{
			Sprite sprite = RetireColonyUtility.LoadColonyPreview(filename, basename, fallbackToTimelapse);
			if (!(sprite == null))
			{
				Rect rect = preview.rectTransform.parent.rectTransform().rect;
				preview.sprite = sprite;
				preview.color = (sprite ? Color.white : Color.black);
				float num = sprite.bounds.size.x / sprite.bounds.size.y;
				if ((double)num >= 1.77777777777778)
				{
					preview.rectTransform.sizeDelta = new Vector2(rect.height * num, rect.height);
				}
				else
				{
					preview.rectTransform.sizeDelta = new Vector2(rect.width, rect.width / num);
				}
				preview.gameObject.SetActive(value: true);
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex);
		}
	}

	public static void ForceStopGame()
	{
		ThreadedHttps<KleiMetrics>.Instance.ClearGameFields();
		ThreadedHttps<KleiMetrics>.Instance.SendProfileStats();
		Game.Instance.SetIsLoading();
		Grid.CellCount = 0;
		Sim.Shutdown();
	}

	private static bool IsSaveFileFromUnsupportedFutureBuild(SaveGame.Header header, SaveGame.GameInfo gameInfo)
	{
		if (gameInfo.saveMajorVersion > 7 || (gameInfo.saveMajorVersion == 7 && gameInfo.saveMinorVersion > 37))
		{
			return true;
		}
		return header.buildVersion > 732169;
	}

	private void UpdateSelected(KButton button, string filename, List<string> dlcIds)
	{
		if (selectedSave != null && selectedSave.button != null)
		{
			selectedSave.button.GetComponent<ImageToggleState>().SetState(ImageToggleState.State.Inactive);
		}
		if (selectedSave == null)
		{
			selectedSave = new SelectedSave();
		}
		selectedSave.button = button;
		selectedSave.filename = filename;
		selectedSave.dlcIds = dlcIds;
		if (selectedSave.button != null)
		{
			selectedSave.button.GetComponent<ImageToggleState>().SetState(ImageToggleState.State.Active);
		}
	}

	private void Load()
	{
		if (!DlcManager.IsAllContentSubscribed(selectedSave.dlcIds))
		{
			string message = (selectedSave.dlcIds.Contains("") ? UI.FRONTEND.LOADSCREEN.VANILLA_RESTART : UI.FRONTEND.LOADSCREEN.EXPANSION1_RESTART);
			ConfirmDoAction(message, delegate
			{
				KPlayerPrefs.SetString("AutoResumeSaveFile", selectedSave.filename);
				DlcManager.ToggleDLC("EXPANSION1_ID");
			});
		}
		else
		{
			LoadingOverlay.Load(DoLoad);
		}
	}

	private void DoLoad()
	{
		if (selectedSave != null)
		{
			DoLoad(selectedSave.filename);
			Deactivate();
		}
	}

	public static void DoLoad(string filename)
	{
		KCrashReporter.MOST_RECENT_SAVEFILE = filename;
		bool flag = true;
		SaveGame.Header header;
		SaveGame.GameInfo gameInfo = SaveLoader.LoadHeader(filename, out header);
		string arg = null;
		string arg2 = null;
		if (header.buildVersion > 732169)
		{
			arg = header.buildVersion.ToString();
			arg2 = 732169u.ToString();
		}
		else if (gameInfo.saveMajorVersion < 7)
		{
			arg = $"v{gameInfo.saveMajorVersion}.{gameInfo.saveMinorVersion}";
			arg2 = $"v{7}.{37}";
		}
		if (!flag)
		{
			GameObject parent = ((FrontEndManager.Instance == null) ? GameScreenManager.Instance.ssOverlayCanvas : FrontEndManager.Instance.gameObject);
			ConfirmDialogScreen component = Util.KInstantiateUI(ScreenPrefabs.Instance.ConfirmDialogScreen.gameObject, parent, force_active: true).GetComponent<ConfirmDialogScreen>();
			component.PopupConfirmDialog(string.Format(UI.CRASHSCREEN.LOADFAILED, "Version Mismatch", arg, arg2), null, null);
			return;
		}
		if (Game.Instance != null)
		{
			ForceStopGame();
		}
		SaveLoader.SetActiveSaveFilePath(filename);
		Time.timeScale = 0f;
		SaveLoader.LoadScene();
	}

	private void MoreInfo()
	{
		App.OpenWebURL("http://support.kleientertainment.com/customer/portal/articles/2776550");
	}

	private void Delete(System.Action onDelete)
	{
		if (selectedSave == null || string.IsNullOrEmpty(selectedSave.filename))
		{
			Debug.LogError("The path provided is not valid and cannot be deleted.");
			return;
		}
		ConfirmDoAction(string.Format(UI.FRONTEND.LOADSCREEN.CONFIRMDELETE, System.IO.Path.GetFileName(selectedSave.filename)), delegate
		{
			try
			{
				DeleteFileAndEmptyFolder(selectedSave.filename);
				string file = System.IO.Path.ChangeExtension(selectedSave.filename, "png");
				DeleteFileAndEmptyFolder(file);
				if (onDelete != null)
				{
					onDelete();
				}
				MainMenu.Instance.RefreshResumeButton();
			}
			catch (SystemException ex)
			{
				Debug.LogError(ex.ToString());
			}
		});
	}

	private void ShowSimpleDialog(string title, string message)
	{
		InfoDialogScreen infoDialogScreen = Util.KInstantiateUI<InfoDialogScreen>(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, base.gameObject).SetHeader(title).AddPlainText(message)
			.AddDefaultOK();
		infoDialogScreen.Activate();
	}

	private void ConfirmCloudSaveMigrations(string message, string title, string confirmText, string backupText, System.Action commitAction, System.Action backupAction, Sprite sprite)
	{
		InfoDialogScreen infoDialogScreen = Util.KInstantiateUI<InfoDialogScreen>(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, base.gameObject).SetHeader(title).AddSprite(sprite)
			.AddPlainText(message)
			.AddDefaultCancel()
			.AddOption(confirmText, delegate(InfoDialogScreen d)
			{
				d.Deactivate();
				commitAction();
			}, rightSide: true);
		infoDialogScreen.Activate();
	}

	private void ShowConvertError(string message)
	{
		if (!(errorInfoScreen == null))
		{
			return;
		}
		if (DistributionPlatform.Initialized && SteamUtils.IsSteamRunningOnSteamDeck())
		{
			errorInfoScreen = Util.KInstantiateUI<InfoDialogScreen>(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, base.gameObject).SetHeader(UI.FRONTEND.LOADSCREEN.CONVERT_ERROR_TITLE).AddSprite(errorSprite)
				.AddPlainText(message)
				.AddDefaultOK();
			errorInfoScreen.Activate();
			return;
		}
		errorInfoScreen = Util.KInstantiateUI<InfoDialogScreen>(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, base.gameObject).SetHeader(UI.FRONTEND.LOADSCREEN.CONVERT_ERROR_TITLE).AddSprite(errorSprite)
			.AddPlainText(message)
			.AddOption(UI.FRONTEND.LOADSCREEN.MIGRATE_FAILURES_FORUM_BUTTON, delegate
			{
				App.OpenWebURL("https://forums.kleientertainment.com/klei-bug-tracker/oni/");
			})
			.AddDefaultOK();
		errorInfoScreen.Activate();
	}

	private void ConfirmDoAction(string message, System.Action action)
	{
		if (confirmScreen == null)
		{
			confirmScreen = Util.KInstantiateUI<ConfirmDialogScreen>(ScreenPrefabs.Instance.ConfirmDialogScreen.gameObject, base.gameObject);
			confirmScreen.PopupConfirmDialog(message, action, delegate
			{
			});
			confirmScreen.gameObject.SetActive(value: true);
		}
	}

	public override void OnKeyDown(KButtonEvent e)
	{
		if (currentColony != null && e.TryConsume(Action.Escape))
		{
			ShowColonyList();
		}
		base.OnKeyDown(e);
	}
}
