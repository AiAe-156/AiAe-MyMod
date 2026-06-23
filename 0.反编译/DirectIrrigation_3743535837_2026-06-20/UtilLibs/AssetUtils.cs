using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UtilLibs;

public class AssetUtils
{
	public static string baseAtlasFolder = Path.Combine("assets", "customatlastiles");

	public static Sprite AddSpriteToAssets(FileInfo file, Assets instance = null, bool overrideExisting = false)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)instance == (Object)null)
		{
			instance = Assets.instance;
		}
		string spriteId = Path.GetFileNameWithoutExtension(file.Name);
		TryLoadTexture(file.FullName, out var texture);
		Sprite val = Sprite.Create(texture, new Rect(0f, 0f, (float)((Texture)texture).width, (float)((Texture)texture).height), Vector2.op_Implicit(Vector3.zero));
		((Object)val).name = spriteId;
		if (!overrideExisting && instance.SpriteAssets.Any((Sprite spritef) => (Object)(object)spritef != (Object)null && ((Object)spritef).name == spriteId))
		{
			SgtLogger.l("Sprite " + spriteId + " was already existent in the sprite assets");
			return null;
		}
		if (overrideExisting)
		{
			instance.SpriteAssets.RemoveAll((Sprite foundsprite2) => (Object)(object)foundsprite2 != (Object)null && ((Object)foundsprite2).name == spriteId);
		}
		instance.SpriteAssets.Add(val);
		HashedString key = default(HashedString);
		((HashedString)(ref key))._002Ector(((Object)val).name);
		if (Assets.Sprites != null)
		{
			Assets.Sprites[key] = val;
		}
		return val;
	}

	public static Sprite AddSpriteToAssets(Assets instance, string spriteid, bool overrideExisting = false, TextureWrapMode mode = (TextureWrapMode)0)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		string directory = Path.Combine(UtilMethods.ModPath, "assets");
		Texture2D val = LoadTexture(spriteid, directory);
		((Texture)val).wrapMode = mode;
		Sprite val2 = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), Vector2.op_Implicit(Vector3.zero));
		((Object)val2).name = spriteid;
		if (!overrideExisting && instance.SpriteAssets.Any((Sprite spritef) => (Object)(object)spritef != (Object)null && ((Object)spritef).name == spriteid))
		{
			SgtLogger.l("Sprite " + spriteid + " was already existent in the sprite assets");
			return null;
		}
		if (overrideExisting)
		{
			instance.SpriteAssets.RemoveAll((Sprite foundsprite2) => (Object)(object)foundsprite2 != (Object)null && ((Object)foundsprite2).name == spriteid);
		}
		instance.SpriteAssets.Add(val2);
		return val2;
	}

	public static void OverrideSpriteTextures(Assets instance, FileInfo file)
	{
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		string spriteId = Path.GetFileNameWithoutExtension(file.Name);
		Texture2D val = LoadTexture(file.FullName);
		List<Texture2D> textureAssets = instance.TextureAssets;
		if (textureAssets != null && textureAssets.Any((Texture2D foundsprite) => (Object)(object)foundsprite != (Object)null && ((Object)foundsprite).name == spriteId))
		{
			SgtLogger.l("removed existing TextureAsset: " + spriteId);
			instance.TextureAssets.RemoveAll((Texture2D foundsprite2) => (Object)(object)foundsprite2 != (Object)null && ((Object)foundsprite2).name == spriteId);
		}
		instance.TextureAssets?.Add(val);
		List<Texture2D> textures = Assets.Textures;
		if (textures != null && textures.Any((Texture2D foundsprite) => (Object)(object)foundsprite != (Object)null && ((Object)foundsprite).name == spriteId))
		{
			SgtLogger.l("removed existing Texture: " + spriteId);
			Assets.Textures?.RemoveAll((Texture2D foundsprite2) => (Object)(object)foundsprite2 != (Object)null && ((Object)foundsprite2).name == spriteId);
		}
		Assets.Textures?.Add(val);
		List<TextureAtlas> textureAtlasAssets = instance.TextureAtlasAssets;
		if (textureAtlasAssets != null && textureAtlasAssets.Any((TextureAtlas TextureAtlas) => (Object)(object)TextureAtlas != (Object)null && (Object)(object)TextureAtlas.texture != (Object)null && ((Object)TextureAtlas.texture).name == spriteId))
		{
			SgtLogger.l("replaced Texture Atlas Asset texture: " + spriteId);
			TextureAtlas val2 = instance.TextureAtlasAssets.First((TextureAtlas TextureAtlas) => (Object)(object)TextureAtlas != (Object)null && (Object)(object)TextureAtlas.texture != (Object)null && ((Object)TextureAtlas.texture).name == spriteId);
			if ((Object)(object)val2 != (Object)null)
			{
				val2.texture = val;
			}
		}
		List<TextureAtlas> textureAtlases = Assets.TextureAtlases;
		if (textureAtlases != null && textureAtlases.Any((TextureAtlas TextureAtlas) => (Object)(object)TextureAtlas != (Object)null && (Object)(object)TextureAtlas.texture != (Object)null && ((Object)TextureAtlas.texture).name == spriteId))
		{
			TextureAtlas val3 = Assets.TextureAtlases.First((TextureAtlas TextureAtlas) => (Object)(object)TextureAtlas != (Object)null && (Object)(object)TextureAtlas.texture != (Object)null && ((Object)TextureAtlas.texture).name == spriteId);
			if ((Object)(object)val3 != (Object)null)
			{
				val3.texture = val;
			}
		}
		Sprite val4 = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), Vector2.op_Implicit(Vector3.zero));
		((Object)val4).name = spriteId;
		List<Sprite> spriteAssets = instance.SpriteAssets;
		if (spriteAssets != null && spriteAssets.Any((Sprite foundsprite) => (Object)(object)foundsprite != (Object)null && ((Object)foundsprite).name == spriteId))
		{
			SgtLogger.l("removed existing SpriteAsset" + spriteId);
			instance.SpriteAssets.RemoveAll((Sprite foundsprite2) => (Object)(object)foundsprite2 != (Object)null && ((Object)foundsprite2).name == spriteId);
		}
		instance.SpriteAssets?.Add(val4);
		Dictionary<HashedString, Sprite> sprites = Assets.Sprites;
		if (sprites != null && sprites.ContainsKey(HashedString.op_Implicit(spriteId)))
		{
			SgtLogger.l("removed existing Sprite" + spriteId);
			Assets.Sprites.Remove(HashedString.op_Implicit(spriteId));
		}
		List<TintedSprite> tintedSprites = Assets.TintedSprites;
		if (tintedSprites != null && tintedSprites.Any((TintedSprite foundsprite) => foundsprite != null && foundsprite.name == spriteId))
		{
			Assets.TintedSprites.First((TintedSprite foundsprite) => foundsprite != null && foundsprite.name == spriteId).sprite = val4;
		}
		Assets.Sprites?.Add(HashedString.op_Implicit(spriteId), val4);
	}

	public static bool TryLoadTexture(string path, out Texture2D texture)
	{
		texture = LoadTexture(path);
		return (Object)(object)texture != (Object)null;
	}

	public static Texture2D LoadTexture(string name, string directory)
	{
		if (directory == null)
		{
			directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets");
		}
		string path = Path.Combine(directory, name + ".png");
		return LoadTexture(path);
	}

	public static Texture2D LoadTexture(string path, bool warnIfFailed = true, int customTextureWidth = 1, int customTextureHeight = 1)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		Texture2D val = null;
		if (File.Exists(path))
		{
			byte[] array = TryReadFile(path);
			val = new Texture2D(customTextureWidth, customTextureHeight);
			ImageConversion.LoadImage(val, array);
		}
		else if (warnIfFailed)
		{
			SgtLogger.logwarning("Could not load texture at path " + path + ".", "SgtImalasUtils");
		}
		return val;
	}

	public static byte[] TryReadFile(string texFile)
	{
		try
		{
			return File.ReadAllBytes(texFile);
		}
		catch (Exception ex)
		{
			SgtLogger.logwarning("Could not read file: " + ex, "SgtImalasUtils");
			return null;
		}
	}

	public static void AddCustomTileTops(BuildingDef def, string name, bool shiny = false, string decorInfo = "tiles_glass_tops_decor_info", string existingPlaceID = null, string existingSpecID = null)
	{
		BlockTileDecorInfo val = Object.Instantiate<BlockTileDecorInfo>(Assets.GetBlockTileDecorInfo(decorInfo));
		string text = (name.Contains("_tops") ? name : (name + "_tiles_tops"));
		if (val != null)
		{
			val.atlas = GetCustomAtlas(text, baseAtlasFolder, val.atlas);
			def.DecorBlockTileInfo = val;
		}
		if (Util.IsNullOrWhiteSpace(existingPlaceID))
		{
			BlockTileDecorInfo val2 = Object.Instantiate<BlockTileDecorInfo>(Assets.GetBlockTileDecorInfo(decorInfo));
			val2.atlas = GetCustomAtlas(text + "_place", baseAtlasFolder, val2.atlas);
			def.DecorPlaceBlockTileInfo = val2;
		}
		else
		{
			def.DecorPlaceBlockTileInfo = Assets.GetBlockTileDecorInfo(existingPlaceID);
		}
		if (shiny)
		{
			string fileName = (Util.IsNullOrWhiteSpace(existingSpecID) ? (text + "_spec") : existingSpecID);
			val.atlasSpec = GetCustomAtlas(fileName, baseAtlasFolder, val.atlasSpec);
		}
	}

	public static void AddCustomTileAtlas(BuildingDef def, string name, bool shiny = false, string referenceAtlas = "tiles_metal")
	{
		TextureAtlas textureAtlas = Assets.GetTextureAtlas(referenceAtlas);
		string text = (name.Contains("tiles") ? name : (name + "_tiles"));
		def.BlockTileAtlas = GetCustomAtlas(text, baseAtlasFolder, textureAtlas);
		def.BlockTilePlaceAtlas = GetCustomAtlas(text + "_place", baseAtlasFolder, textureAtlas);
		if (shiny)
		{
			def.BlockTileShineAtlas = GetCustomAtlas(text + "_spec", baseAtlasFolder, textureAtlas);
		}
	}

	public static TextureAtlas GetCustomAtlas(string fileName, string folder, TextureAtlas tileAtlas)
	{
		string text = UtilMethods.ModPath;
		if (folder != null)
		{
			text = Path.Combine(text, folder);
		}
		Texture2D val = LoadTexture(fileName, text);
		if ((Object)(object)val == (Object)null)
		{
			SgtLogger.error("could not load custom tile atlas texture: " + fileName + " in folder: " + text);
			return null;
		}
		TextureAtlas val2 = ScriptableObject.CreateInstance<TextureAtlas>();
		val2.texture = val;
		val2.scaleFactor = tileAtlas.scaleFactor;
		val2.items = tileAtlas.items;
		return val2;
	}

	public static AssetBundle LoadAssetBundle(string assetBundleName, string path = null, bool platformSpecific = false)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Invalid comparison between Unknown and I4
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Invalid comparison between Unknown and I4
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Invalid comparison between Unknown and I4
		foreach (AssetBundle allLoadedAssetBundle in AssetBundle.GetAllLoadedAssetBundles())
		{
			if (((Object)allLoadedAssetBundle).name == assetBundleName)
			{
				return allLoadedAssetBundle;
			}
		}
		if (Util.IsNullOrWhiteSpace(path))
		{
			path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets");
		}
		if (platformSpecific)
		{
			RuntimePlatform platform = Application.platform;
			RuntimePlatform val = platform;
			if ((int)val != 1)
			{
				if ((int)val != 2)
				{
					if ((int)val == 13)
					{
						path = Path.Combine(path, "linux");
					}
				}
				else
				{
					path = Path.Combine(path, "windows");
				}
			}
			else
			{
				path = Path.Combine(path, "mac");
			}
		}
		path = Path.Combine(path, assetBundleName);
		AssetBundle val2 = AssetBundle.LoadFromFile(path);
		if ((Object)(object)val2 == (Object)null)
		{
			SgtLogger.warning("Failed to load AssetBundle from path " + path);
			return null;
		}
		return val2;
	}
}
