using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FUtility;
using Newtonsoft.Json;
using UnityEngine;

namespace TrueTiles.Cmps;

public class TileAssetLoader : KMonoBehaviour
{
	public static TileAssetLoader Instance;

	private Dictionary<string, Dictionary<string, TileData>> tiles;

	private bool finishedLoading;

	private static readonly string[] delimiter = new string[1] { "::" };

	private const string RESET = "reset";

	public void ReloadAssets()
	{
		finishedLoading = false;
		tiles = new Dictionary<string, Dictionary<string, TileData>>();
		TileAssets.Instance.UnloadTextures();
		TexturePacksManager.Instance.SortPacks();
		LoadEnabledPacks(TexturePacksManager.Instance.packs);
		LoadOverrides();
	}

	public void LoadEnabledPacks(List<PackData> data)
	{
		foreach (PackData item in data.Where((PackData p) => p.Enabled))
		{
			LoadAssets(item);
		}
	}

	public override void OnPrefabInit()
	{
		Instance = this;
	}

	public static void LoadAssets(PackData packData)
	{
		string text = Path.Combine(packData.Root, "tiles.json");
		if (!File.Exists(text))
		{
			Log.Warning("No data at " + text);
			return;
		}
		if (Util.IsNullOrWhiteSpace(packData.AssetBundle))
		{
			Instance.OverLoadFromJson(text);
		}
		else
		{
			Instance.OverLoadFromAssetBundle(text, packData.AssetBundle, packData.AssetBundleRoot);
		}
		Log.Info("Loaded tile art overrides from " + text.Normalize().Replace(Path.Combine(Util.RootFolder(), "mods").Normalize(), ""));
	}

	public override void OnCleanUp()
	{
		tiles = null;
		((KMonoBehaviour)this).OnCleanUp();
		Instance = null;
	}

	private void OverLoadFromJson(string path)
	{
		string root = Path.Combine(Path.GetDirectoryName(path), "textures");
		Dictionary<string, Dictionary<string, TileData>> dictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, TileData>>>(File.ReadAllText(path));
		TurnPathsAbsolute(root, dictionary);
		if (tiles == null)
		{
			tiles = new Dictionary<string, Dictionary<string, TileData>>();
		}
		Merge(tiles, dictionary);
		Log.Debug($"Loaded json {tiles.Count} {path}");
	}

	private void OverLoadFromAssetBundle(string path, string assetBundleName, string assetPath)
	{
		Dictionary<string, Dictionary<string, TileData>> dictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, TileData>>>(File.ReadAllText(path));
		foreach (KeyValuePair<string, Dictionary<string, TileData>> item in dictionary)
		{
			foreach (KeyValuePair<string, TileData> item2 in item.Value)
			{
				item2.Value.AssetBundle = assetBundleName;
				item2.Value.Root = Path.GetDirectoryName(path);
				item2.Value.AssetRoot = assetPath;
			}
		}
		if (tiles == null)
		{
			tiles = new Dictionary<string, Dictionary<string, TileData>>();
		}
		Merge(tiles, dictionary);
		Log.Debug($"Loaded json {tiles.Count} {path}");
	}

	private void Merge(Dictionary<string, Dictionary<string, TileData>> first, Dictionary<string, Dictionary<string, TileData>> second)
	{
		foreach (KeyValuePair<string, Dictionary<string, TileData>> item in second)
		{
			if (first.ContainsKey(item.Key))
			{
				foreach (KeyValuePair<string, TileData> item2 in item.Value)
				{
					Dictionary<string, TileData> dictionary = first[item.Key];
					if (dictionary.ContainsKey(item2.Key))
					{
						dictionary[item2.Key] = item2.Value;
					}
					else
					{
						dictionary.Add(item2.Key, item2.Value);
					}
				}
			}
			else
			{
				first.Add(item.Key, item.Value);
			}
		}
	}

	private void TurnPathsAbsolute(string root, Dictionary<string, Dictionary<string, TileData>> dataDict)
	{
		foreach (KeyValuePair<string, Dictionary<string, TileData>> item in dataDict)
		{
			foreach (KeyValuePair<string, TileData> item2 in item.Value)
			{
				item2.Value.MainTex = GetPathEntry(root, item2.Value.MainTex);
				item2.Value.MainSpecular = GetPathEntry(root, item2.Value.MainSpecular);
				item2.Value.TopTex = GetPathEntry(root, item2.Value.TopTex);
				item2.Value.TopSpecular = GetPathEntry(root, item2.Value.TopSpecular);
				item2.Value.NormalTex = GetPathEntry(root, item2.Value.NormalTex);
				item2.Value.Root = root;
			}
		}
	}

	private string GetPathEntry(string root, string path)
	{
		if (!(path == "reset"))
		{
			return GetAbsolutePath(root, path);
		}
		return path;
	}

	private string GetAbsolutePath(string root, string path)
	{
		if (Util.IsNullOrWhiteSpace(path))
		{
			return null;
		}
		string[] array = path.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
		if (array != null && array.Length > 1)
		{
			string key = array[0];
			if (TexturePacksManager.Instance.roots.TryGetValue(key, out var value))
			{
				return Path.Combine(value, "textures", path + ".png");
			}
		}
		return Path.Combine(root, path + ".png");
	}

	public void Reload()
	{
		finishedLoading = false;
		TileAssets.Instance.Clear();
		LoadOverrides();
	}

	public void LoadOverrides()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		if (finishedLoading)
		{
			Log.Warning("LoadOverrides is being called a second time, this should not happen!");
			return;
		}
		if (tiles == null)
		{
			Log.Warning("There are no tile overrides enabled.");
			return;
		}
		if (ElementLoader.elements == null || ElementLoader.elements.Count == 0)
		{
			Log.Warning("Loading overrides too early, ElementLoader.elements don't exist yet");
			return;
		}
		foreach (GameObject item in Assets.GetPrefabsWithTag(ModAssets.Tags.texturedTile))
		{
			KPrefabIDExtensions.RemoveTag(item, ModAssets.Tags.texturedTile);
		}
		foreach (KeyValuePair<string, Dictionary<string, TileData>> tile in tiles)
		{
			Log.Debug("Loading tile: " + tile.Key);
			string key = tile.Key;
			BuildingDef buildingDef = Assets.GetBuildingDef(key);
			if ((Object)(object)Assets.GetBuildingDef(key) == (Object)null)
			{
				Log.Debug("Building ID not present, skipping: " + key);
				continue;
			}
			KPrefabIDExtensions.AddTag(buildingDef.BuildingComplete, ModAssets.Tags.texturedTile);
			foreach (KeyValuePair<string, TileData> item2 in tile.Value)
			{
				Log.Debug("   " + item2.Key);
				Element element = ElementLoader.GetElement(Tag.op_Implicit(item2.Key));
				if (element != null)
				{
					TileData value = item2.Value;
					string assetBundle = item2.Value.AssetBundle;
					TileAssets.TextureAsset asset = new TileAssets.TextureAsset
					{
						main = LoadTex(value.MainTex, assetBundle, value.Root, value.AssetRoot),
						specular = LoadTex(value.MainSpecular, assetBundle, value.Root, value.AssetRoot),
						specularColor = GetColor(value.MainSpecularColor),
						top = LoadTex(value.TopTex, assetBundle, value.Root, value.AssetRoot),
						topSpecular = LoadTex(value.TopSpecular, assetBundle, value.Root, value.AssetRoot),
						topSpecularColor = GetColor(value.TopSpecularColor),
						normalMap = LoadTex(value.NormalTex, assetBundle, value.Root, value.AssetRoot),
						specularFrequency = value.Frequency
					};
					TileAssets.Instance.Add(key, element.id, asset);
				}
			}
		}
		stopwatch.Stop();
		TimeSpan elapsed = stopwatch.Elapsed;
		string text = $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:00}";
		Log.Info("Loaded texture assets. Loading took " + text);
		finishedLoading = true;
	}

	private Color GetColor(float[] values)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (values != null && values.Length == 4)
		{
			return new Color(values[0], values[1], values[2], values[3]);
		}
		return Color.white;
	}

	private Texture2D LoadTex(string path, string assetBundle, string root, string assetFolder)
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		if (Util.IsNullOrWhiteSpace(assetBundle))
		{
			if (!(path == "reset"))
			{
				return FAssets.LoadTexture(path);
			}
			return null;
		}
		return FAssets.LoadAssetBundle(assetBundle, root).LoadAsset<Texture2D>(assetFolder + path + ".png");
	}
}
