using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LiquidShaderProperties", menuName = "Klei/Liquid Shader Properties")]
public class LiquidShaderProperties : ScriptableObject
{
	[Serializable]
	public struct Entry
	{
		public Substance.SubstanceTexture texture;

		public float scrollSpeed;
	}

	[SerializeField]
	private Entry[] TextureScrollSpeed = new Entry[7]
	{
		new Entry
		{
			texture = Substance.SubstanceTexture.Magma,
			scrollSpeed = 0.02f
		},
		new Entry
		{
			texture = Substance.SubstanceTexture.MoltenMetal,
			scrollSpeed = 0.02f
		},
		new Entry
		{
			texture = Substance.SubstanceTexture.Polluted,
			scrollSpeed = 0.02f
		},
		new Entry
		{
			texture = Substance.SubstanceTexture.Oil,
			scrollSpeed = 0.02f
		},
		new Entry
		{
			texture = Substance.SubstanceTexture.Thick,
			scrollSpeed = 0.02f
		},
		new Entry
		{
			texture = Substance.SubstanceTexture.Sap,
			scrollSpeed = 0.02f
		},
		new Entry
		{
			texture = Substance.SubstanceTexture.CrystalFragments,
			scrollSpeed = 0.01f
		}
	};

	private static readonly int SubstanceTextureCount = Enum.GetValues(typeof(Substance.SubstanceTexture)).Length;

	private float[] cachedScrollSpeeds;

	public void ApplyToMaterial(Material material)
	{
		if (cachedScrollSpeeds == null)
		{
			cachedScrollSpeeds = new float[SubstanceTextureCount];
		}
		Array.Clear(cachedScrollSpeeds, 0, cachedScrollSpeeds.Length);
		for (int i = 0; i < TextureScrollSpeed.Length; i++)
		{
			int num = (int)(TextureScrollSpeed[i].texture - 1);
			if (num >= 0 && num < SubstanceTextureCount)
			{
				cachedScrollSpeeds[num] = TextureScrollSpeed[i].scrollSpeed;
			}
		}
		material.SetFloatArray("_TextureScrollSpeeds", cachedScrollSpeeds);
	}
}
