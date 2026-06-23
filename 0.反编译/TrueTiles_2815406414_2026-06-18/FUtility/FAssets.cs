using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace FUtility;

public class FAssets
{
	public static Texture2D LoadTexture(string name, string directory)
	{
		if (directory == null)
		{
			directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets");
		}
		return LoadTexture(Path.Combine(directory, name + ".png"));
	}

	public static bool TryLoadTexture(string path, out Texture2D texture)
	{
		texture = LoadTexture(path, warnIfFailed: false);
		return (Object)(object)texture != (Object)null;
	}

	public static Texture2D LoadTexture(string path, bool warnIfFailed = true)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		Texture2D val = null;
		if (File.Exists(path))
		{
			byte[] array = TryReadFile(path);
			val = new Texture2D(1, 1);
			ImageConversion.LoadImage(val, array);
		}
		else if (warnIfFailed)
		{
			Log.Warning("Could not load texture at path " + path + ".");
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
			Log.Warning("Could not read file: " + ex);
			return null;
		}
	}

	public static TextureAtlas GetCustomAtlas(string filePath, TextureAtlas tileAtlas)
	{
		Texture2D val = LoadTexture(filePath);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		TextureAtlas obj = ScriptableObject.CreateInstance<TextureAtlas>();
		obj.texture = val;
		obj.scaleFactor = tileAtlas.scaleFactor;
		obj.items = tileAtlas.items;
		((Object)obj).name = Path.GetFileNameWithoutExtension(filePath) + "_atlas";
		return obj;
	}

	public static TextureAtlas GetCustomAtlas(string fileName, string folder, TextureAtlas tileAtlas)
	{
		return GetCustomAtlas(Path.Combine(Utils.ModPath, folder, fileName + ".png"), tileAtlas);
	}

	public static TextureAtlas GetCustomAtlas(Texture2D texture, TextureAtlas tileAtlas)
	{
		TextureAtlas obj = ScriptableObject.CreateInstance<TextureAtlas>();
		obj.texture = texture;
		obj.scaleFactor = tileAtlas.scaleFactor;
		obj.items = tileAtlas.items;
		return obj;
	}

	public static AssetBundle LoadAssetBundle(string assetBundleName, string path = null, bool platformSpecific = false)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Invalid comparison between Unknown and I4
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Invalid comparison between Unknown and I4
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
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
			if ((int)platform != 1)
			{
				if ((int)platform != 2)
				{
					if ((int)platform == 13)
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
		AssetBundle val = AssetBundle.LoadFromFile(path);
		if ((Object)(object)val == (Object)null)
		{
			Log.Warning("Failed to load AssetBundle from path " + path);
			return null;
		}
		return val;
	}

	public static Mesh Square(GameObject parent, float width = 1f, float height = 1f)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		MeshFilter val = parent.AddComponent<MeshFilter>();
		Mesh val2 = new Mesh();
		val2.vertices = (Vector3[])(object)new Vector3[4]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(width, 0f, 0f),
			new Vector3(0f, height, 0f),
			new Vector3(width, height, 0f)
		};
		val2.triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
		val2.normals = (Vector3[])(object)new Vector3[4]
		{
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward
		};
		val2.uv = (Vector2[])(object)new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		return val.mesh = val2;
	}

	public static void SaveImage(Texture textureToWrite, string path)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		Texture2D val = new Texture2D(textureToWrite.width, textureToWrite.height, (TextureFormat)4, false);
		RenderTexture val2 = new RenderTexture(textureToWrite.width, textureToWrite.height, 32);
		Graphics.Blit(textureToWrite, val2);
		val.ReadPixels(new Rect(0f, 0f, (float)((Texture)val2).width, (float)((Texture)val2).height), 0, 0);
		val.Apply();
		byte[] bytes = ImageConversion.EncodeToPNG(val);
		File.WriteAllBytes(path, bytes);
		Log.Info("Saved image to " + path);
	}
}
