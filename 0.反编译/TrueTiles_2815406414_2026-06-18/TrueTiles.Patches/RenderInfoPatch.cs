using System;
using System.Reflection;
using FUtility;
using HarmonyLib;
using Rendering;
using TrueTiles.Cmps;
using UnityEngine;

namespace TrueTiles.Patches;

public class RenderInfoPatch
{
	[HarmonyPatch]
	public static class RenderInfo_Ctor_Patch
	{
		public static MethodBase TargetMethod()
		{
			return AccessTools.Constructor(AccessTools.TypeByName("Rendering.BlockTileRenderer+RenderInfo"), new Type[5]
			{
				typeof(BlockTileRenderer),
				typeof(int),
				typeof(int),
				typeof(BuildingDef),
				typeof(SimHashes)
			}, false);
		}

		public static void Postfix(BuildingDef def, SimHashes element, Material ___material, object ___decorRenderInfo)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			TileAssets.TextureAsset textureAsset = TileAssets.Instance.Get(((Def)def).PrefabID, element);
			if (textureAsset == null)
			{
				return;
			}
			if ((Object)(object)___material == (Object)null)
			{
				Log.Warning("material is null " + ((Def)def).PrefabID);
				return;
			}
			ShaderPropertyUtil.SetMainTexProperty(___material, textureAsset.main);
			ShaderPropertyUtil.SetSpecularProperties(___material, textureAsset.specular, textureAsset.specularFrequency, textureAsset.specularColor);
			if ((Object)(object)textureAsset.top != (Object)null || (Object)(object)textureAsset.topSpecular != (Object)null)
			{
				if (___decorRenderInfo == null)
				{
					Log.Warning("___decorRenderInfo is null " + ((Def)def).PrefabID);
					return;
				}
				Material value = Traverse.Create(___decorRenderInfo).Field<Material>("material").Value;
				if ((Object)(object)value == (Object)null)
				{
					Log.Warning("topMaterial is null " + ((Def)def).PrefabID);
					return;
				}
				ShaderPropertyUtil.SetMainTexProperty(value, textureAsset.top);
				ShaderPropertyUtil.SetSpecularProperties(value, textureAsset.topSpecular, textureAsset.specularFrequency, textureAsset.topSpecularColor);
			}
			if ((Object)(object)textureAsset.normalMap != (Object)null)
			{
				___material.SetTexture("_NormalTex", (Texture)(object)textureAsset.normalMap);
			}
		}
	}
}
