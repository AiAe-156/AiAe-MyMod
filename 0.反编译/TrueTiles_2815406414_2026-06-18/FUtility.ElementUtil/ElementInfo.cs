using UnityEngine;

namespace FUtility.ElementUtil;

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

	public ElementInfo(string id, string anim, State state, Color color)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		this.id = id;
		this.anim = anim;
		this.state = state;
		this.color = color;
		SimHash = ElementUtil.RegisterSimHash(id);
		ElementUtil.elements.Add(this);
		Tag = Tag.op_Implicit(id);
	}

	public static implicit operator SimHashes(ElementInfo info)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return info.SimHash;
	}

	public Element Get()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (ElementLoader.elementTagTable == null)
		{
			Log.Warning("Trying to fetch element too early, elements are not loaded yet.");
			return null;
		}
		return ElementLoader.GetElement(Tag);
	}

	public Substance CreateSubstance(bool specular = false, Material material = null, Color? uiColor = null, Color? conduitColor = null, Color? specularColor = null, string normal = null)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		Log.Debug("creating substance for " + id);
		if ((Object)(object)material == (Object)null)
		{
			material = (((int)state == 3) ? Assets.instance.substanceTable.solidMaterial : Assets.instance.substanceTable.liquidMaterial);
		}
		Log.Assert("material", material);
		isInitialized = true;
		return ElementUtil.CreateSubstance(SimHash, specular, anim, state, color, material, (Color)(((_003F?)uiColor) ?? color), (Color)(((_003F?)conduitColor) ?? color), specularColor, normal);
	}

	public Substance CreateSubstance(Color uiColor, Color conduitColor)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return CreateSubstance(specular: false, null, uiColor, conduitColor);
	}

	public static ElementInfo Solid(string id, Color color)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new ElementInfo(id, id.ToLowerInvariant() + "_kanim", (State)3, color);
	}

	public static ElementInfo Liquid(string id, Color color)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new ElementInfo(id, "liquid_tank_kanim", (State)2, color);
	}

	public static ElementInfo Gas(string id, Color color)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new ElementInfo(id, "gas_tank_kanim", (State)1, color);
	}

	public override string ToString()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((object)SimHash/*cast due to .constrained prefix*/).ToString();
	}
}
