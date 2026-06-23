using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FUtility.SaveData;

public class SaveDataManager<T> where T : IUserSetting, new()
{
	public Action<T> OnRead;

	public bool localExists;

	public bool externalExists;

	public bool saveExternal;

	private readonly string localPath;

	private readonly string externalPath;

	private readonly string externalFolder;

	private readonly string externalRoot;

	private readonly string localFolder;

	private FileSystemWatcher watcher;

	public T Settings { get; set; }

	public SaveDataManager(string localPath)
		: this(localPath, readImmediately: true, writeIfDoesntExist: true, "settings", (JsonConverter[])(object)new JsonConverter[1] { (JsonConverter)new StringEnumConverter() })
	{
	}//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	//IL_0017: Expected O, but got Unknown


	public SaveDataManager(string localPath, bool readImmediately = true, bool writeIfDoesntExist = true, string filename = "settings", JsonConverter[] converters = null)
	{
		this.localPath = Path.Combine(localPath, filename + ".json");
		externalFolder = Path.Combine(Util.RootFolder(), "mods", "settings", "akismods", Log.modName.ToLowerInvariant());
		externalPath = Path.Combine(externalFolder, filename + ".json");
		localFolder = localPath;
		Log.Debuglog("external path set to", externalPath);
		if (readImmediately)
		{
			Settings = Read();
		}
		if (writeIfDoesntExist)
		{
			WriteIfDoesntExist(useExternal: false, converters);
		}
	}

	public void WatchForChanges()
	{
		if (watcher == null)
		{
			Log.Debuglog(GetPath());
			watcher = new FileSystemWatcher
			{
				Path = Path.GetDirectoryName(GetPath()),
				Filter = Path.GetFileName(GetPath()),
				EnableRaisingEvents = true
			};
		}
		watcher.Changed += delegate
		{
			Settings = Read();
		};
		watcher.Changed += delegate
		{
			Log.Info("Settings reloaded.");
		};
	}

	public void OnFileChanged(Action<object, FileSystemEventArgs> sender)
	{
		watcher.Changed += sender.Invoke;
	}

	public void WriteIfDoesntExist(bool useExternal, JsonConverter[] converters)
	{
		if ((useExternal && !externalExists) || (!useExternal && !localExists))
		{
			Write(useExternal, converters);
		}
		if (!useExternal)
		{
			_ = externalExists;
		}
	}

	private void DeleteExternalFolder()
	{
		try
		{
			if (Directory.Exists(externalFolder))
			{
				Directory.Delete(externalFolder, recursive: true);
				string directoryName = Path.GetDirectoryName(externalFolder);
				if (DeleteDirIfEmpty(directoryName))
				{
					string path = Path.Combine(directoryName);
					DeleteDirIfEmpty(path);
				}
			}
		}
		catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
		{
			Log.Warning("Tried deleting external mod settings folder, but could not: " + ex.Message);
		}
	}

	private bool DeleteDirIfEmpty(string path)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		if (!Directory.EnumerateFileSystemEntries(path).Any())
		{
			Log.Debuglog("Deleting folder: " + path);
			directoryInfo.Delete(recursive: false);
			return true;
		}
		return false;
	}

	public T Read()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		string result;
		T val = (ReadJson(out result) ? JsonConvert.DeserializeObject<T>(result, new JsonSerializerSettings
		{
			ObjectCreationHandling = (ObjectCreationHandling)2
		}) : new T());
		OnRead?.Invoke(val);
		return val;
	}

	private string GetPath()
	{
		if (externalExists)
		{
			return externalPath;
		}
		if (localExists)
		{
			return localPath;
		}
		if (File.Exists(externalPath))
		{
			externalExists = true;
			return externalPath;
		}
		if (File.Exists(localPath))
		{
			localExists = true;
			return localPath;
		}
		return null;
	}

	private bool ReadJson(out string result)
	{
		result = null;
		string path = GetPath();
		if (!Util.IsNullOrWhiteSpace(path))
		{
			result = TryReadFile(path);
			Log.Debuglog("Reading configurations from ", path);
		}
		return !Util.IsNullOrWhiteSpace(result);
	}

	private string TryReadFile(string path)
	{
		try
		{
			return File.ReadAllText(path);
		}
		catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
		{
			Log.Warning("Config file found, but could not be read: ", ex.Message);
			return null;
		}
	}

	public void Write(bool useExternal = false, JsonConverter[] converters = null, bool cleanUp = true)
	{
		try
		{
			string path = (useExternal ? externalFolder : localFolder);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string contents = JsonConvert.SerializeObject((object)Settings, (Formatting)1, converters);
			string text = (useExternal ? externalPath : localPath);
			File.WriteAllText(text, contents);
			Log.Debuglog("saved config to " + text);
		}
		catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
		{
			Log.Warning("Could not write configuration file: " + ex.Message);
		}
	}
}
