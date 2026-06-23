using System;
using FUtility;
using Newtonsoft.Json;
using UnityEngine;

namespace TrueTiles;

[Serializable]
public class PackData
{
	[JsonIgnore]
	public static readonly Version FIRST = new Version(1, 1, 0, 0);

	public string Id { get; set; }

	public string Name { get; set; }

	public string Author { get; set; }

	public string Description { get; set; }

	public int Order { get; set; } = 999;

	public bool Enabled { get; set; }

	public string AssetBundle { get; set; }

	public string AssetBundleRoot { get; set; }

	public string Root { get; set; }

	public string Version { get; set; }

	[JsonIgnore]
	public Texture2D Icon { get; set; }

	[JsonIgnore]
	public int TextureCount { get; set; }

	[JsonIgnore]
	public bool IsValid { get; internal set; }

	public Version GetSystemVersion()
	{
		if (Util.IsNullOrWhiteSpace(Version))
		{
			return FIRST;
		}
		if (System.Version.TryParse(Version, out Version result))
		{
			return result;
		}
		Log.Warning("Issue with Tile Texture Pack data \"" + Id + "\": " + Version + " is not in the correct format. Expected: X.X.X.X");
		return FIRST;
	}
}
