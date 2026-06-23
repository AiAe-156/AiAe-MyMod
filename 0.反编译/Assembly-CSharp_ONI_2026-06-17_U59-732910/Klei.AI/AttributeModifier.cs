using System;
using System.Diagnostics;

namespace Klei.AI;

[DebuggerDisplay("{AttributeId}")]
public class AttributeModifier
{
	public Func<string> NameCB;

	public string Description;

	public Func<string> DescriptionCB;

	public string AttributeId { get; private set; }

	public float Value { get; private set; }

	public bool IsMultiplier { get; private set; }

	public GameUtil.TimeSlice? OverrideTimeSlice { get; set; }

	public bool UIOnly { get; private set; }

	public bool IsReadonly { get; private set; }

	public AttributeModifier(string attribute_id, float value, string description = null, bool is_multiplier = false, bool uiOnly = false, bool is_readonly = true)
	{
		AttributeId = attribute_id;
		Value = value;
		Description = ((description == null) ? attribute_id : description);
		DescriptionCB = null;
		IsMultiplier = is_multiplier;
		UIOnly = uiOnly;
		IsReadonly = is_readonly;
		OverrideTimeSlice = null;
	}

	public AttributeModifier(string attribute_id, float value, Func<string> description_cb, bool is_multiplier = false, bool uiOnly = false)
		: this(attribute_id, value, null, description_cb, is_multiplier, uiOnly)
	{
	}

	public AttributeModifier(string attribute_id, float value, Func<string> name_cb, Func<string> description_cb, bool is_multiplier = false, bool uiOnly = false)
	{
		AttributeId = attribute_id;
		Value = value;
		NameCB = name_cb;
		DescriptionCB = description_cb;
		Description = null;
		IsMultiplier = is_multiplier;
		UIOnly = uiOnly;
		OverrideTimeSlice = null;
		if (description_cb == null)
		{
			Debug.LogWarning("AttributeModifier being constructed without a description callback: " + attribute_id);
		}
	}

	public void Reconstruct(string attribute_id, float value, string description = null, bool is_multiplier = false, bool uiOnly = false, bool is_readonly = true)
	{
		AttributeId = attribute_id;
		Value = value;
		Description = ((description == null) ? attribute_id : description);
		DescriptionCB = null;
		NameCB = null;
		IsMultiplier = is_multiplier;
		UIOnly = uiOnly;
		IsReadonly = is_readonly;
		OverrideTimeSlice = null;
	}

	public void SetValue(float value)
	{
		Value = value;
	}

	public static Attribute FetchAttribute(string attributeId)
	{
		Attribute attribute = Db.Get().Attributes.TryGet(attributeId);
		if (attribute != null)
		{
			return attribute;
		}
		Attribute attribute2 = Db.Get().BuildingAttributes.TryGet(attributeId);
		if (attribute2 != null)
		{
			return attribute2;
		}
		Attribute attribute3 = Db.Get().PlantAttributes.TryGet(attributeId);
		if (attribute3 != null)
		{
			return attribute3;
		}
		Attribute attribute4 = Db.Get().CritterAttributes.TryGet(attributeId);
		if (attribute4 != null)
		{
			return attribute4;
		}
		return null;
	}

	private Attribute FetchAttribute()
	{
		return FetchAttribute(AttributeId);
	}

	public string GetName()
	{
		Attribute attribute = FetchAttribute();
		if (attribute != null && attribute.ShowInUI != Attribute.Display.Never)
		{
			if (NameCB != null)
			{
				return NameCB();
			}
			return attribute.Name;
		}
		return "";
	}

	public string GetDescription()
	{
		if (DescriptionCB == null)
		{
			return Description;
		}
		return DescriptionCB();
	}

	public string GetFormattedString()
	{
		Attribute attribute = FetchAttribute();
		IAttributeFormatter attributeFormatter = ((!IsMultiplier && attribute != null) ? attribute.formatter : null);
		string text = "";
		text = ((attributeFormatter != null) ? attributeFormatter.GetFormattedModifier(this) : ((!IsMultiplier) ? (text + GameUtil.GetFormattedSimple(Value)) : (text + GameUtil.GetFormattedPercent(Value * 100f))));
		if (text != null && text.Length > 0 && text[0] != '-' && OverrideTimeSlice != GameUtil.TimeSlice.None)
		{
			text = GameUtil.AddPositiveSign(text, Value > 0f);
		}
		return text;
	}

	public AttributeModifier Clone()
	{
		return new AttributeModifier(AttributeId, Value, Description);
	}
}
