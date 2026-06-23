using System.Collections.Generic;
using Klei.AI;
using STRINGS;

namespace Database;

public class LungCapacityPerk : SkillPerk
{
	private HashSet<MinionResume> breathBoostConsumed = new HashSet<MinionResume>();

	private AttributeModifier modifier;

	private float bonus;

	public LungCapacityPerk(string id, float modifierBonus, string modifierDesc)
		: base(id, "", null, null, null)
	{
		bonus = modifierBonus;
		string id2 = Db.Get().Amounts.Breath.maxAttribute.Id;
		Attribute attribute = Db.Get().Attributes.Get(id2);
		modifier = new AttributeModifier(id2, bonus, modifierDesc);
		Name = string.Format(UI.ROLES_SCREEN.PERKS.ATTRIBUTE_EFFECT_FMT, modifier.GetFormattedString(), attribute.Name);
		base.OnApply = ApplyPerk;
		base.OnRemove = RemovePerk;
	}

	private void ApplyPerk(MinionResume identity)
	{
		ArrayRef<AttributeModifier> modifiers = identity.GetAttributes().Get(modifier.AttributeId).Modifiers;
		bool flag = false;
		for (int i = 0; i != modifiers.Count; i++)
		{
			if (modifiers[i] == modifier)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			identity.GetAttributes().Add(modifier);
		}
		if (breathBoostConsumed.Add(identity))
		{
			AmountInstance amountInstance = Db.Get().Amounts.Breath.Lookup(identity);
			amountInstance?.SetValue(amountInstance.value + bonus);
		}
	}

	private void RemovePerk(MinionResume identity)
	{
		identity.GetAttributes().Remove(modifier);
		breathBoostConsumed.Add(identity);
	}
}
