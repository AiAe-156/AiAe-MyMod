using UnityEngine;

namespace TUNING;

public class GarnishInfo
{
	public Tag itemTag;

	public string effectId;

	public float consumeRate;

	public float storageCapacity;

	public int priority;

	public Descriptor descriptor;

	public HashedString overrideAnimName;

	public KAnimHashedString overrideSymbolName;

	public Color fxTintColor = Color.white;

	public KAnim.Build.Symbol GetOverrideSymbol()
	{
		if (!overrideAnimName.IsValid)
		{
			return null;
		}
		KAnimFile anim = Assets.GetAnim(overrideAnimName);
		if (anim == null)
		{
			return null;
		}
		return anim.GetData().build.GetSymbol(overrideSymbolName);
	}
}
