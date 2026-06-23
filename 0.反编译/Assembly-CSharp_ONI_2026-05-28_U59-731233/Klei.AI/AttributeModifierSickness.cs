using System.Collections.Generic;
using STRINGS;
using UnityEngine;

namespace Klei.AI;

public class AttributeModifierSickness : Sickness.SicknessComponent
{
	private Dictionary<Tag, AttributeModifier[]> GetAttributeModifierForMinionModel = new Dictionary<Tag, AttributeModifier[]>();

	private AttributeModifier[] attributeModifiers;

	public AttributeModifier[] Modifers => attributeModifiers;

	public AttributeModifierSickness(Tag minionModel, AttributeModifier[] attribute_modifiers)
	{
		GetAttributeModifierForMinionModel[minionModel] = attribute_modifiers;
		attributeModifiers = new AttributeModifier[0];
	}

	public AttributeModifierSickness(AttributeModifier[] attribute_modifiers)
	{
		attributeModifiers = attribute_modifiers;
	}

	public override object OnInfect(GameObject go, SicknessInstance diseaseInstance)
	{
		Attributes attributes = go.GetAttributes();
		Tag key = go.PrefabID();
		if (GetAttributeModifierForMinionModel.ContainsKey(key))
		{
			for (int i = 0; i < GetAttributeModifierForMinionModel[key].Length; i++)
			{
				AttributeModifier modifier = GetAttributeModifierForMinionModel[key][i];
				attributes.Add(modifier);
			}
		}
		for (int j = 0; j < attributeModifiers.Length; j++)
		{
			AttributeModifier modifier2 = attributeModifiers[j];
			attributes.Add(modifier2);
		}
		return null;
	}

	public override void OnCure(GameObject go, object instance_data)
	{
		Attributes attributes = go.GetAttributes();
		Tag key = go.PrefabID();
		if (GetAttributeModifierForMinionModel.ContainsKey(key))
		{
			for (int i = 0; i < GetAttributeModifierForMinionModel[key].Length; i++)
			{
				AttributeModifier modifier = GetAttributeModifierForMinionModel[key][i];
				attributes.Remove(modifier);
			}
		}
		for (int j = 0; j < attributeModifiers.Length; j++)
		{
			AttributeModifier modifier2 = attributeModifiers[j];
			attributes.Remove(modifier2);
		}
	}

	public override List<Descriptor> GetSymptoms(GameObject victim)
	{
		if (victim == null)
		{
			return GetSymptoms();
		}
		List<Descriptor> list = new List<Descriptor>();
		Tag key = victim.PrefabID();
		if (GetAttributeModifierForMinionModel.ContainsKey(key))
		{
			AttributeModifier[] array = GetAttributeModifierForMinionModel[key];
			foreach (AttributeModifier attributeModifier in array)
			{
				Attribute attribute = Db.Get().Attributes.Get(attributeModifier.AttributeId);
				list.Add(new Descriptor(string.Format(DUPLICANTS.DISEASES.ATTRIBUTE_MODIFIER_SYMPTOMS, attribute.Name, attributeModifier.GetFormattedString()), string.Format(DUPLICANTS.DISEASES.ATTRIBUTE_MODIFIER_SYMPTOMS_TOOLTIP, attribute.Name, attributeModifier.GetFormattedString()), Descriptor.DescriptorType.Symptom));
			}
		}
		AttributeModifier[] array2 = attributeModifiers;
		foreach (AttributeModifier attributeModifier2 in array2)
		{
			Attribute attribute2 = Db.Get().Attributes.Get(attributeModifier2.AttributeId);
			list.Add(new Descriptor(string.Format(DUPLICANTS.DISEASES.ATTRIBUTE_MODIFIER_SYMPTOMS, attribute2.Name, attributeModifier2.GetFormattedString()), string.Format(DUPLICANTS.DISEASES.ATTRIBUTE_MODIFIER_SYMPTOMS_TOOLTIP, attribute2.Name, attributeModifier2.GetFormattedString()), Descriptor.DescriptorType.Symptom));
		}
		return list;
	}

	public override List<Descriptor> GetSymptoms()
	{
		List<Descriptor> list = new List<Descriptor>();
		foreach (Tag key in GetAttributeModifierForMinionModel.Keys)
		{
			GameObject prefab = Assets.GetPrefab(key);
			string properName = prefab.GetProperName();
			AttributeModifier[] array = GetAttributeModifierForMinionModel[key];
			foreach (AttributeModifier attributeModifier in array)
			{
				Attribute attribute = Db.Get().Attributes.Get(attributeModifier.AttributeId);
				list.Add(new Descriptor(string.Format(DUPLICANTS.DISEASES.ATTRIBUTE_BY_MODEL_MODIFIER_SYMPTOMS, properName, attribute.Name, attributeModifier.GetFormattedString()), string.Format(DUPLICANTS.DISEASES.ATTRIBUTE_MODIFIER_SYMPTOMS_TOOLTIP, attribute.Name, attributeModifier.GetFormattedString()), Descriptor.DescriptorType.Symptom));
			}
		}
		AttributeModifier[] array2 = attributeModifiers;
		foreach (AttributeModifier attributeModifier2 in array2)
		{
			Attribute attribute2 = Db.Get().Attributes.Get(attributeModifier2.AttributeId);
			list.Add(new Descriptor(string.Format(DUPLICANTS.DISEASES.ATTRIBUTE_MODIFIER_SYMPTOMS, attribute2.Name, attributeModifier2.GetFormattedString()), string.Format(DUPLICANTS.DISEASES.ATTRIBUTE_MODIFIER_SYMPTOMS_TOOLTIP, attribute2.Name, attributeModifier2.GetFormattedString()), Descriptor.DescriptorType.Symptom));
		}
		return list;
	}
}
