using System.IO;

namespace TrueTiles.Datagen;

public class PackDataGen : DataGen
{
	private const string AUTHOR = "Aki";

	public PackDataGen(string path)
		: base(path)
	{
		ConfigureMetaData("Default", enabled: true, "truetiles_default", "assets/tiles/default_textures/", -10, "1.1.0.0");
		ConfigureMetaData("CutesyCarpet", enabled: false, "truetiles_cutesy_carpets", "assets/tiles/cutesy_carpet_textures/", -5, "1.1.0.0");
		ConfigureMetaData("AltAirflow", enabled: false, "truetiles_altairflow", "assets/tiles/airflow_textures/", -5, "1.1.0.0");
	}

	private void ConfigureMetaData(string id, bool enabled, string assetBundleName = null, string assetFolderRoot = null, int order = 0, string version = "1.0.0.0")
	{
		DataGen.Write(Path.Combine(path, id), "metadata", new PackData
		{
			Id = id,
			Name = "TrueTiles.STRINGS.TEXTUREPACKS." + id.ToUpperInvariant() + ".NAME",
			Description = "TrueTiles.STRINGS.TEXTUREPACKS." + id.ToUpperInvariant() + ".DESCRIPTION",
			Author = "Aki",
			Order = order,
			Enabled = enabled,
			AssetBundle = assetBundleName,
			AssetBundleRoot = assetFolderRoot,
			Version = version
		});
	}
}
