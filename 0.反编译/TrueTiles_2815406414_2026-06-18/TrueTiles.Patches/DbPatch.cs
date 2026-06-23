using HarmonyLib;
using TrueTiles.Cmps;
using UnityEngine;

namespace TrueTiles.Patches;

public class DbPatch
{
	[HarmonyPatch(typeof(Db), "Initialize")]
	public static class Db_Initialize_Patch
	{
		public static void Prefix()
		{
			Mod.ScanOtherMods();
			ModAssets.LateLoadAssets();
			GameObject gameObject = ((Component)Global.Instance).gameObject;
			gameObject.AddComponent<TileAssets>();
			gameObject.AddComponent<TileAssetLoader>();
			gameObject.AddComponent<TexturePacksManager>();
			if (Mod.addonPacks != null)
			{
				foreach (string addonPack in Mod.addonPacks)
				{
					TexturePacksManager.Instance.LoadPack(addonPack);
				}
			}
			TexturePacksManager.Instance.LoadExteriorPacks();
			TexturePacksManager.Instance.SortPacks();
			TileAssetLoader.Instance.LoadEnabledPacks(TexturePacksManager.Instance.packs);
		}
	}
}
