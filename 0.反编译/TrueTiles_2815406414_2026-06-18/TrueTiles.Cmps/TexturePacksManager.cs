using System;
using System.Collections.Generic;
using System.IO;
using FUtility;
using Newtonsoft.Json;

namespace TrueTiles.Cmps;

public class TexturePacksManager : KMonoBehaviour
{
	public static TexturePacksManager Instance;

	public List<PackData> packs;

	public Dictionary<string, string> roots;

	public Dictionary<string, PackData> userDefinedData;

	public string exteriorPath;

	public override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		Instance = this;
		packs = new List<PackData>();
		roots = new Dictionary<string, string>();
		exteriorPath = Mod.GetExternalSavePath();
	}

	public override void OnCleanUp()
	{
		((KMonoBehaviour)this).OnCleanUp();
		Instance = null;
	}

	public void LoadExteriorPacks()
	{
		if (!Directory.Exists(exteriorPath))
		{
			Log.Debug("This path does not exist: " + exteriorPath);
			return;
		}
		string[] directories = Directory.GetDirectories(exteriorPath);
		foreach (string path in directories)
		{
			LoadPack(path);
		}
		string root = (Mod.Settings.SaveExternally ? Mod.GetExternalSavePath() : Mod.GetLocalSavePath());
		Instance.SavePacks(root);
	}

	public void LoadAllPacksFromFolder(string path)
	{
		if (!Directory.Exists(path))
		{
			Log.Warning("This path does not exist: " + path);
			return;
		}
		string[] directories = Directory.GetDirectories(path);
		foreach (string path2 in directories)
		{
			LoadPack(path2);
		}
	}

	public void ReadUserSettings()
	{
		string text = exteriorPath;
		userDefinedData = new Dictionary<string, PackData>();
		if (Directory.Exists(text))
		{
			string path = Path.Combine(text, "metadata.json");
			if (File.Exists(path) && FileUtil.TryReadFile(path, out var result))
			{
				PackData packData = JsonConvert.DeserializeObject<PackData>(result);
				userDefinedData.Add(packData.Id, packData);
			}
		}
	}

	public PackData LoadPack(string path)
	{
		Log.Debug("LOADING " + path);
		if (!Directory.Exists(path))
		{
			Log.Warning("This path does not exist: " + path);
			return null;
		}
		string path2 = Path.Combine(path, "metadata.json");
		if (!File.Exists(path2))
		{
			Log.Warning("Folder marked as texture pack, but has no metadata.json set: " + path);
			return null;
		}
		if (FileUtil.TryReadFile(path2, out var result))
		{
			PackData packData = JsonConvert.DeserializeObject<PackData>(result);
			if (Util.IsNullOrWhiteSpace(packData.Root) || !Directory.Exists(packData.Root))
			{
				packData.Root = path;
			}
			if (Path.GetDirectoryName(packData.Root).EndsWith("tiles"))
			{
				packData.Root = path;
			}
			roots[packData.Id] = path;
			SetTextureCountFromPNGs(packData);
			if (IsPackValid(packData))
			{
				int num = packs.FindIndex((PackData p) => p.Id == packData.Id);
				if (num != -1)
				{
					packs.RemoveAt(num);
				}
				PackData packData2 = packs.Find((PackData p) => p.Id == packData.Id);
				if (packData2 != null)
				{
					Version systemVersion = packData.GetSystemVersion();
					Version systemVersion2 = packData2.GetSystemVersion();
					if (systemVersion <= systemVersion2)
					{
						Log.Warning("Duplicate pack found: " + packData.Id + ". Keeping " + packData2.Root + ", discarding " + packData.Root + ".");
						return null;
					}
					Log.Warning("Duplicate pack found: " + packData.Id + ". Keeping " + packData.Root + ", discarding " + packData2.Root + ".");
				}
				TryLoadIcon(packData.Root, packData);
				packs.Add(packData);
				packData.IsValid = true;
				return packData;
			}
			Log.Warning("Pack missing: " + packData.Id);
			packData.IsValid = false;
		}
		return null;
	}

	public void SortPacks()
	{
		packs.Sort((PackData p1, PackData p2) => p1.Order.CompareTo(p2.Order));
	}

	public void SavePacks(string root)
	{
		foreach (PackData pack in packs)
		{
			string contents = JsonConvert.SerializeObject((object)pack, (Formatting)1);
			File.WriteAllText(Path.Combine(FileUtil.GetOrCreateDirectory(Path.Combine(root, pack.Id)), "metadata.json"), contents);
		}
	}

	private void SetTextureCountFromPNGs(PackData packData)
	{
		string path = Path.Combine(packData.Root, "textures");
		packData.TextureCount = (Directory.Exists(path) ? Directory.GetFiles(path, "*.png", SearchOption.AllDirectories).Length : 0);
	}

	private void TryLoadIcon(string path, PackData packData)
	{
		if (File.Exists(Path.Combine(path, "icon.png")))
		{
			packData.Icon = FAssets.LoadTexture("icon", path);
		}
	}

	public bool IsPackValid(PackData pack)
	{
		if (!Directory.Exists(pack.Root))
		{
			return false;
		}
		bool num = !Util.IsNullOrWhiteSpace(pack.AssetBundle);
		string text = (num ? Path.Combine(pack.Root, pack.AssetBundle) : null);
		if (num)
		{
			Log.Debug("asset bundle pack");
		}
		else
		{
			Log.Debug("loads " + pack.TextureCount + " textures");
		}
		bool flag = !Util.IsNullOrWhiteSpace(text) && File.Exists(text);
		Log.Debug($"has asset data: {flag} {text}");
		if (pack.TextureCount == 0 && !flag)
		{
			return false;
		}
		return true;
	}
}
