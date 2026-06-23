using System;
using System.Collections.Generic;
using System.IO;
using FUtility;
using FUtility.SaveData;
using HarmonyLib;
using KMod;
using Rendering;
using TrueTiles.Datagen;
using TrueTiles.Patches;
using TrueTiles.Settings;

namespace TrueTiles;

public class Mod : UserMod2
{
	private const string ADDON_PATH = "true_tiles_addon";

	private static SaveDataManager<Config> config;

	public static Harmony harmonyInstance;

	public static HashSet<string> moddedPacksPaths;

	public static HashSet<string> moddedPackIds;

	public static List<string> addonPacks;

	public static Config Settings => config.Settings;

	public static string ModPath { get; private set; }

	public static string GetExternalSavePath()
	{
		return Path.Combine(Util.RootFolder(), "mods", "tile_texture_packs");
	}

	public static string GetLocalSavePath()
	{
		return Path.Combine(ModPath, "tiles");
	}

	public static void AddPack(string pack)
	{
		if (addonPacks == null)
		{
			addonPacks = new List<string>();
		}
		addonPacks.Add(pack);
	}

	public override void OnLoad(Harmony harmony)
	{
		ModPath = Utils.ModPath;
		config = new SaveDataManager<Config>(((UserMod2)this).path);
		Setup();
		GenerateData(((UserMod2)this).path);
		harmonyInstance = harmony;
		Log.PrintVersion((UserMod2)(object)this);
		((UserMod2)this).OnLoad(harmony);
	}

	public static void SaveConfig()
	{
		config.Write(useExternal: true);
	}

	public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
	{
	}

	private void Setup()
	{
		BlockTileRendererPatch.GetRenderInfoLayerMethod = AccessTools.Method(typeof(BlockTileRenderer), "GetRenderInfoLayer", new Type[2]
		{
			typeof(bool),
			typeof(SimHashes)
		}, (Type[])null);
		BlockTileRendererPatch.GetRenderLayerForTileMethod = AccessTools.Method(typeof(BlockTileRendererPatch), "GetRenderLayerForTile", new Type[3]
		{
			typeof(RenderInfoLayer),
			typeof(BuildingDef),
			typeof(SimHashes)
		}, (Type[])null);
	}

	private void GenerateData(string path)
	{
		string orCreateDirectory = FileUtil.GetOrCreateDirectory(Path.Combine(path, "true_tiles_addon"));
		new PackDataGen(orCreateDirectory);
		new TileDataGen(orCreateDirectory);
	}

	public static void ScanOtherMods()
	{
		moddedPacksPaths = new HashSet<string>();
		moddedPackIds = new HashSet<string>();
		foreach (Mod mod in Global.Instance.modManager.mods)
		{
			string path = Path.Combine(mod.ContentPath, "true_tiles_addon");
			if (Directory.Exists(path))
			{
				string[] directories = Directory.GetDirectories(path);
				foreach (string text in directories)
				{
					Log.Info("Loading a pack from " + mod.staticID + " " + text);
					AddPack(text);
					moddedPacksPaths.Add(mod.ContentPath);
					moddedPackIds.Add(mod.staticID);
				}
			}
		}
	}
}
