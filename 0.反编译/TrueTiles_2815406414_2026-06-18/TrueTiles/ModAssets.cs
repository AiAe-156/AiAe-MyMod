using FUtility;
using FUtility.FUI;
using UnityEngine;

namespace TrueTiles;

public class ModAssets
{
	public static class Prefabs
	{
		public static GameObject settingsDialog;
	}

	public static class Tags
	{
		public static readonly Tag texturedTile = TagManager.Create("truetiles_texturedTile");
	}

	public static void LateLoadAssets()
	{
		AssetBundle val = FAssets.LoadAssetBundle("truetilesassets");
		Prefabs.settingsDialog = val.LoadAsset<GameObject>("SettingsDialog");
		if ((Object)(object)Prefabs.settingsDialog == (Object)null)
		{
			Log.Warning("Settings Dialog is null :(");
			string[] allAssetNames = val.GetAllAssetNames();
			foreach (string text in allAssetNames)
			{
				Log.Debug(text);
			}
		}
		TMPConverter.ReplaceAllText(Prefabs.settingsDialog);
	}
}
