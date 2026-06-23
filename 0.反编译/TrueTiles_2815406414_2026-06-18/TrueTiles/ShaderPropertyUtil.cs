using UnityEngine;

namespace TrueTiles;

public class ShaderPropertyUtil
{
	public static void SetMainTexProperty(Material material, Texture2D texture)
	{
		if ((Object)(object)texture != (Object)null && (Object)(object)material != (Object)null)
		{
			material.SetTexture("_MainTex", (Texture)(object)texture);
		}
	}

	public static void SetSpecularProperties(Material material, Texture2D texture, float frequency, Color color)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)material == (Object)null))
		{
			if ((Object)(object)texture != (Object)null)
			{
				material.SetTexture("_SpecularTex", (Texture)(object)texture);
				material.EnableKeyword("ENABLE_SHINE");
			}
			material.SetFloat("_Frequency", frequency);
			material.SetColor("_ShineColour", color);
		}
	}
}
