using System.Collections.Generic;
using Klei.AI;

namespace FUtility;

public class EffectBuilder
{
	private readonly string ID;

	private string name;

	private string description;

	private readonly float duration;

	private bool triggerFloatingText;

	private bool showInUI;

	private readonly bool isBad;

	private List<AttributeModifier> modifiers;

	private string emoteAnim;

	private float emoteCooldown;

	private string customIcon;

	private string stompGroup;

	private List<ReactablePrecondition> emotePreconditions;

	public EffectBuilder(string ID, float duration, bool isBad)
	{
		name = StringEntry.op_Implicit(Strings.Get("STRINGS.DUPLICANTS.MODIFIERS." + ID.ToUpper() + ".NAME"));
		description = StringEntry.op_Implicit(Strings.Get("STRINGS.DUPLICANTS.MODIFIERS." + ID.ToUpper() + ".TOOLTIP"));
		triggerFloatingText = true;
		showInUI = true;
		this.duration = duration;
		this.isBad = isBad;
		this.ID = ID;
		customIcon = "";
	}

	public EffectBuilder Icon(string icon)
	{
		customIcon = icon;
		return this;
	}

	public EffectBuilder Name(string name)
	{
		this.name = name;
		return this;
	}

	public EffectBuilder Description(string description)
	{
		this.description = description;
		return this;
	}

	public EffectBuilder Modifier(string id, float value, bool isMultiplier, bool uiOnly = false, bool readOnly = true)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		modifiers = modifiers ?? new List<AttributeModifier>();
		modifiers.Add(new AttributeModifier(id, value, name, isMultiplier, uiOnly, readOnly));
		return this;
	}

	public EffectBuilder Modifier(string id, float value)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		modifiers = modifiers ?? new List<AttributeModifier>();
		modifiers.Add(new AttributeModifier(id, value, name, false, false, true));
		return this;
	}

	public EffectBuilder Emote(string emoteAnim, float emoteCooldown)
	{
		this.emoteAnim = emoteAnim;
		this.emoteCooldown = emoteCooldown;
		return this;
	}

	public EffectBuilder EmotePrecondition(ReactablePrecondition condition)
	{
		emotePreconditions = emotePreconditions ?? new List<ReactablePrecondition>();
		emotePreconditions.Add(condition);
		return this;
	}

	public EffectBuilder HideFloatingText()
	{
		triggerFloatingText = false;
		return this;
	}

	public EffectBuilder HideInUI()
	{
		showInUI = false;
		return this;
	}

	public void Add(ModifierSet set)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		Effect val = new Effect(ID, name, description, duration, showInUI, triggerFloatingText, isBad, emoteAnim, emoteCooldown, stompGroup, customIcon);
		if (modifiers != null)
		{
			((Modifier)val).SelfModifiers = modifiers;
		}
		if (emotePreconditions != null)
		{
			val.emotePreconditions = emotePreconditions;
		}
		set.effects.Add(val);
	}
}
