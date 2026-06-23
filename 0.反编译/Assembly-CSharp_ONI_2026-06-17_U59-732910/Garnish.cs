using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class Garnish
{
	public Tag itemTag;

	public string effectId;

	public float consumeRate;

	public int priority;

	public Descriptor descriptor;

	private HashedString overrideAnimName;

	private KAnimHashedString overrideSymbolName;

	private Color fxTintColor = Color.white;

	private static readonly HashedString SALT_SYMBOL = "saltshaker";

	private static readonly HashedString SALT_FG_SYMBOL = "saltshaker_fg";

	private static readonly KAnimHashedString SALT_PARTICLE_SYMBOL = new KAnimHashedString("salt_particle");

	public static readonly List<Garnish> All = new List<Garnish>
	{
		new Garnish
		{
			itemTag = TableSaltConfig.TAG,
			effectId = "MessTableSalt",
			consumeRate = TableSaltTuning.CONSUMABLE_RATE,
			priority = 0,
			descriptor = new Descriptor(string.Format(UI.BUILDINGEFFECTS.MESS_TABLE_SALT, TableSaltTuning.MORALE_MODIFIER), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.MESS_TABLE_SALT, TableSaltTuning.MORALE_MODIFIER))
		},
		new Garnish
		{
			itemTag = CaviarConfig.TAG,
			effectId = "MessCaviar",
			consumeRate = CaviarTuning.CONSUMABLE_RATE,
			priority = 10,
			descriptor = new Descriptor(string.Format(UI.BUILDINGEFFECTS.MESS_CAVIAR, CaviarTuning.MORALE_MODIFIER, CaviarTuning.STRESS_MODIFIER), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.MESS_CAVIAR, CaviarTuning.MORALE_MODIFIER, CaviarTuning.STRESS_MODIFIER)),
			overrideAnimName = "caviarshaker_kanim",
			overrideSymbolName = "object",
			fxTintColor = Color.black
		}
	};

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

	public EffectInstance Activate(Storage storage, GameObject diner)
	{
		storage.ConsumeIgnoringDisease(itemTag, consumeRate);
		EffectInstance result = diner.GetComponent<Effects>().Add(effectId, should_save: true);
		KAnim.Build.Symbol overrideSymbol = GetOverrideSymbol();
		if (overrideSymbol != null && diner.TryGetComponent<SymbolOverrideController>(out var component))
		{
			component.AddSymbolOverride(SALT_SYMBOL, overrideSymbol);
			component.AddSymbolOverride(SALT_FG_SYMBOL, overrideSymbol);
		}
		if (diner.TryGetComponent<KBatchedAnimController>(out var component2))
		{
			component2.SetSymbolTint(SALT_PARTICLE_SYMBOL, fxTintColor);
		}
		return result;
	}

	public static void Deactivate(GameObject diner)
	{
		if (diner.TryGetComponent<SymbolOverrideController>(out var component))
		{
			component.RemoveSymbolOverride(SALT_SYMBOL);
			component.RemoveSymbolOverride(SALT_FG_SYMBOL);
		}
		if (diner.TryGetComponent<KBatchedAnimController>(out var component2))
		{
			component2.SetSymbolTint(SALT_PARTICLE_SYMBOL, Color.white);
		}
	}

	public static void SetDinerVisibility(KAnimControllerBase controller, bool visible)
	{
		controller.SetSymbolVisiblity(SALT_SYMBOL, visible);
		controller.SetSymbolVisiblity(SALT_FG_SYMBOL, visible);
	}

	public static Garnish GetActive(Storage storage)
	{
		if (storage == null)
		{
			return null;
		}
		Garnish garnish = null;
		foreach (Garnish item in All)
		{
			if (!(storage.GetMassAvailable(item.itemTag) < item.consumeRate) && (garnish == null || item.priority > garnish.priority))
			{
				garnish = item;
			}
		}
		return garnish;
	}

	public static bool HasAny(Storage storage)
	{
		return GetActive(storage) != null;
	}
}
