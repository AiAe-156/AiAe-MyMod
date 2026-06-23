using FUtility.SaveData;

namespace TrueTiles.Settings;

[External(new string[] { "mods", "tile_texture_packs" })]
public class Config : IUserSetting
{
	public bool SaveExternally { get; set; } = true;
}
