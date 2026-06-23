using UnityEngine;

namespace FUtility;

public class Tiles
{
	public static string baseFolder = "assets/tiles";

	public static void AddCustomTileAtlas(BuildingDef def, string name, bool shiny = false, string referenceAtlas = "tiles_metal")
	{
		TextureAtlas textureAtlas = Assets.GetTextureAtlas(referenceAtlas);
		def.BlockTileAtlas = FAssets.GetCustomAtlas(name + "_tiles", baseFolder, textureAtlas);
		def.BlockTilePlaceAtlas = FAssets.GetCustomAtlas(name + "_tiles_place", baseFolder, textureAtlas);
		if (shiny)
		{
			def.BlockTileShineAtlas = FAssets.GetCustomAtlas(name + "_tiles_spec", baseFolder, textureAtlas);
		}
	}

	public static void AddCustomTileTops(BuildingDef def, string name, bool shiny = false, string decorInfo = "tiles_glass_tops_decor_info", string existingPlaceID = null, string existingSpecID = null)
	{
		BlockTileDecorInfo val = Object.Instantiate<BlockTileDecorInfo>(Assets.GetBlockTileDecorInfo(decorInfo));
		if (val != null)
		{
			val.atlas = FAssets.GetCustomAtlas(name + "_tiles_tops", baseFolder, val.atlas);
			def.DecorBlockTileInfo = val;
		}
		if (Util.IsNullOrWhiteSpace(existingPlaceID))
		{
			BlockTileDecorInfo val2 = Object.Instantiate<BlockTileDecorInfo>(Assets.GetBlockTileDecorInfo(decorInfo));
			val2.atlas = FAssets.GetCustomAtlas(name + "_tiles_tops_place", baseFolder, val2.atlas);
			def.DecorPlaceBlockTileInfo = val2;
		}
		else
		{
			def.DecorPlaceBlockTileInfo = Assets.GetBlockTileDecorInfo(existingPlaceID);
		}
		if (shiny)
		{
			string fileName = (Util.IsNullOrWhiteSpace(existingSpecID) ? (name + "_tiles_tops_spec") : existingSpecID);
			val.atlasSpec = FAssets.GetCustomAtlas(fileName, baseFolder, val.atlasSpec);
		}
	}
}
