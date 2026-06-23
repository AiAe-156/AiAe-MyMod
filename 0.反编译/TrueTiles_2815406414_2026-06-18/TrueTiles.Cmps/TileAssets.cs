using System.Collections.Generic;
using UnityEngine;

namespace TrueTiles.Cmps;

public class TileAssets : KMonoBehaviour
{
	public class TextureAsset
	{
		public Texture2D main;

		public Texture2D specular;

		public Color specularColor = Color.white;

		public Texture2D top;

		public Texture2D topSpecular;

		public Color topSpecularColor = Color.white;

		public Texture2D normalMap;

		public float specularFrequency = 1f;
	}

	private Dictionary<string, Dictionary<SimHashes, TextureAsset>> textureAssets;

	public static TileAssets Instance { get; private set; }

	public override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		Instance = this;
		textureAssets = new Dictionary<string, Dictionary<SimHashes, TextureAsset>>();
	}

	public void Clear()
	{
		foreach (Dictionary<SimHashes, TextureAsset> value in textureAssets.Values)
		{
			foreach (TextureAsset value2 in value.Values)
			{
				value2.top = null;
				value2.normalMap = null;
				value2.topSpecular = null;
				value2.main = null;
				value2.specular = null;
			}
		}
		textureAssets.Clear();
	}

	public void UnloadTextures()
	{
		foreach (KeyValuePair<string, Dictionary<SimHashes, TextureAsset>> textureAsset in textureAssets)
		{
			foreach (TextureAsset value in textureAsset.Value.Values)
			{
				value.top = null;
				value.normalMap = null;
				value.main = null;
				value.specular = null;
				value.topSpecular = null;
			}
		}
	}

	public TextureAsset Get(string def, SimHashes material)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (textureAssets != null && textureAssets.TryGetValue(def, out var value) && value.TryGetValue(material, out var value2))
		{
			return value2;
		}
		return null;
	}

	public void Add(string def, SimHashes material, TextureAsset asset)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!textureAssets.ContainsKey(def))
		{
			textureAssets.Add(def, new Dictionary<SimHashes, TextureAsset>());
		}
		textureAssets[def][material] = asset;
	}

	public bool ContainsDef(string prefabID)
	{
		return textureAssets.ContainsKey(prefabID);
	}

	public override void OnCleanUp()
	{
		((KMonoBehaviour)this).OnCleanUp();
		textureAssets = null;
		Instance = null;
	}
}
