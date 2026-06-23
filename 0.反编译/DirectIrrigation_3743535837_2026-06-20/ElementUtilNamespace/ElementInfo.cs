using UnityEngine;
using UtilLibs;

namespace ElementUtilNamespace;

public class ElementInfo
{
	public string id;

	public State state;

	public string anim;

	public Color color;

	public Color uiColor;

	public Color conduitColor;

	public bool isInitialized;

	public SimHashes SimHash { get; private set; }

	public Tag Tag { get; private set; }

	public Tag CreateTag()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Tag;
	}

	public ElementInfo(string id, string anim, State state, Color color)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		this.id = id;
		this.anim = anim;
		this.state = state;
		this.color = color;
		SimHash = SgtElementUtil.RegisterSimHash(id);
		SgtElementUtil.Elements.Add(this);
		Tag = Tag.op_Implicit(id);
	}

	public static implicit operator SimHashes(ElementInfo info)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return info.SimHash;
	}

	public Element Get()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (ElementLoader.elementTagTable == null)
		{
			SgtLogger.dlogwarn("Trying to fetch element too early, elements are not loaded yet.");
			return null;
		}
		return ElementLoader.GetElement(Tag);
	}

	public unsafe Material CreateTintedMaterialCopy(SimHashes originalElement, Color? overrideColor = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Substance substance = Assets.instance.substanceTable.GetSubstance(originalElement);
		if (substance == null)
		{
			SgtLogger.error("No substance found for " + ((object)(*(SimHashes*)(&originalElement))/*cast due to .constrained prefix*/).ToString());
			return null;
		}
		Material val = new Material(substance.material);
		Color tint = (overrideColor.HasValue ? overrideColor.Value : color);
		Texture2D mainTexture = TintTextureWithColor(val.mainTexture, id, tint);
		val.mainTexture = (Texture)(object)mainTexture;
		((Object)val).name = "mat" + id;
		return val;
	}

	private static Texture2D TintTextureWithColor(Texture sourceTexture, string name, Color tint)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Texture2D readableCopy = GetReadableCopy((Texture2D)(object)((sourceTexture is Texture2D) ? sourceTexture : null));
		Color32[] pixels = readableCopy.GetPixels32();
		for (int i = 0; i < pixels.Length; i++)
		{
			Color val = Color32.op_Implicit(pixels[i]);
			float num = ((Color)(ref val)).grayscale * 1.5f;
			pixels[i] = Color32.op_Implicit(tint * num);
		}
		readableCopy.SetPixels32(pixels);
		readableCopy.Apply();
		((Object)readableCopy).name = name;
		return readableCopy;
	}

	public static Texture2D GetReadableCopy(Texture2D source)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)source == (Object)null || ((Texture)source).width == 0 || ((Texture)source).height == 0)
		{
			return null;
		}
		RenderTexture temporary = RenderTexture.GetTemporary(((Texture)source).width, ((Texture)source).height, 0, (RenderTextureFormat)7, (RenderTextureReadWrite)1);
		Graphics.Blit((Texture)(object)source, temporary);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = temporary;
		Texture2D val = new Texture2D(((Texture)source).width, ((Texture)source).height);
		val.ReadPixels(new Rect(0f, 0f, (float)((Texture)temporary).width, (float)((Texture)temporary).height), 0, 0);
		val.Apply();
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
		return val;
	}

	public Substance CreateSubstanceFromElementTinted(SimHashes clonedMaterial, Color? overrideColor = null)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		return CreateSubstance(specular: false, CreateTintedMaterialCopy(clonedMaterial, overrideColor), null, null, null, null, clonedMaterial, overrideColor);
	}

	public Substance CreateSubstance(bool specular = false, Material material = null, Color? uiColor = null, Color? conduitColor = null, Color? specularColor = null, string normal = null, SimHashes cloneMaterialOrigin = (SimHashes)(-1456075980), Color? clonedMaterialColorOverride = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Invalid comparison between Unknown and I4
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (int)cloneMaterialOrigin != -1456075980;
		if ((Object)(object)material == (Object)null)
		{
			material = (((int)state == 3) ? Assets.instance.substanceTable.solidMaterial : Assets.instance.substanceTable.liquidMaterial);
			if (flag && (int)state == 3)
			{
				material = CreateTintedMaterialCopy(cloneMaterialOrigin, clonedMaterialColorOverride);
			}
		}
		isInitialized = true;
		return SgtElementUtil.CreateSubstance(SimHash, specular, anim, state, color, material, (Color)(((_003F?)uiColor) ?? color), (Color)(((_003F?)conduitColor) ?? color), specularColor, normal, flag);
	}

	public Substance CreateSubstance(Color uiColor, Color conduitColor)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return CreateSubstance(specular: false, null, uiColor, conduitColor, null, null, (SimHashes)(-1456075980));
	}

	public static ElementInfo Solid(string id, Color color)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return new ElementInfo(id, id.ToLowerInvariant() + "_kanim", (State)3, color);
	}

	public static ElementInfo Solid(string id, string anim, Color color)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		return new ElementInfo(id, anim, (State)3, color);
	}

	public static ElementInfo Liquid(string id, Color color)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return new ElementInfo(id, "liquid_tank_kanim", (State)2, color);
	}

	public static ElementInfo Gas(string id, Color color)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return new ElementInfo(id, "gas_tank_kanim", (State)1, color);
	}

	public override string ToString()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((object)SimHash/*cast due to .constrained prefix*/).ToString();
	}
}
