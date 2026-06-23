using System.Text;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class BionicBatteryDisplayer : StandardAmountDisplayer
{
	private enum ElectrobankState
	{
		Unexistent,
		Damaged,
		Depleated,
		Charged
	}

	public class BionicBatteryAttributeFormatter : StandardAttributeFormatter
	{
		public BionicBatteryAttributeFormatter()
			: base(GameUtil.UnitClass.Energy, GameUtil.TimeSlice.PerSecond)
		{
		}
	}

	private const float criticalIconFlashFrequency = 0.45f;

	private string GetIconForState(ElectrobankState state)
	{
		string text = "";
		return state switch
		{
			ElectrobankState.Unexistent => BionicBatteryMonitor.EmptySlotBatteryIcon, 
			ElectrobankState.Charged => BionicBatteryMonitor.ChargedBatteryIcon, 
			_ => BionicBatteryMonitor.DischargedBatteryIcon, 
		};
	}

	public override string GetTooltip(Amount master, AmountInstance instance)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		BionicBatteryMonitor.Instance sMI = instance.gameObject.GetSMI<BionicBatteryMonitor.Instance>();
		float num = instance.deltaAttribute.GetTotalDisplayValue();
		if (sMI != null)
		{
			float wattage = sMI.Wattage;
			num += wattage;
		}
		if (master.description.IndexOf("{1}") > -1)
		{
			stringBuilder.AppendFormat(master.description, formatter.GetFormattedValue(instance.value), GameUtil.GetIdentityDescriptor(instance.gameObject, tense));
		}
		else
		{
			stringBuilder.AppendFormat(master.description, formatter.GetFormattedValue(instance.value));
		}
		if (sMI != null)
		{
			int electrobankCount = sMI.ElectrobankCount;
			int electrobankCountCapacity = sMI.ElectrobankCountCapacity;
			stringBuilder.Append("\n\n");
			stringBuilder.AppendFormat(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.ELECTROBANK_DETAILS_LABEL, GameUtil.GetFormattedInt(electrobankCount), GameUtil.GetFormattedInt(electrobankCountCapacity));
			if (electrobankCount > 0)
			{
				for (int i = 0; i < sMI.storage.items.Count; i++)
				{
					GameObject gameObject = sMI.storage.items[i];
					Electrobank component = gameObject.GetComponent<Electrobank>();
					ElectrobankState state = ((component == null) ? ElectrobankState.Damaged : ((component.Charge <= 0f) ? ElectrobankState.Depleated : ElectrobankState.Charged));
					string iconForState = GetIconForState(state);
					float joules = ((component == null) ? 0f : component.Charge);
					stringBuilder.Append("\n");
					stringBuilder.Append("    • ");
					stringBuilder.AppendFormat(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.ELECTROBANK_ROW, iconForState, gameObject.GetProperName(), GameUtil.GetFormattedJoules(joules));
				}
			}
			if (electrobankCount < electrobankCountCapacity)
			{
				for (int j = 0; j < electrobankCountCapacity - electrobankCount; j++)
				{
					stringBuilder.Append("\n");
					stringBuilder.Append("    • ");
					stringBuilder.AppendFormat(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.ELECTROBANK_EMPTY_ROW, GetIconForState(ElectrobankState.Unexistent));
				}
			}
		}
		stringBuilder.Append("\n\n");
		stringBuilder.AppendFormat(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.CURRENT_WATTAGE_LABEL, formatter.GetFormattedValue(num, formatter.DeltaTimeSlice));
		if (sMI != null)
		{
			StringBuilder stringBuilder2 = GlobalStringBuilderPool.Alloc();
			stringBuilder2.Append("<b>+</b>");
			GameUtil.AppendFormattedWattage(stringBuilder2, sMI.GetBaseWattage());
			stringBuilder.Append("\n");
			stringBuilder.Append("    • ");
			stringBuilder.AppendFormat(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.STANDARD_ACTIVE_TEMPLATE, DUPLICANTS.MODIFIERS.BIONIC_WATTS.BASE_NAME, stringBuilder2.ToString());
			stringBuilder2.Clear();
			float num2 = 0f;
			foreach (BionicBatteryMonitor.WattageModifier modifier in sMI.Modifiers)
			{
				if (modifier.value != 0f)
				{
					stringBuilder.Append("\n");
					stringBuilder.Append("    • ");
					stringBuilder.Append(modifier.name);
				}
				else if (modifier.potentialValue > 0f)
				{
					stringBuilder2.Append("\n");
					stringBuilder2.Append("    • ");
					stringBuilder2.Append(modifier.name);
					num2 += modifier.potentialValue;
				}
			}
			if (stringBuilder2.Length != 0)
			{
				stringBuilder.Append("\n\n");
				stringBuilder.AppendFormat(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.POTENTIAL_EXTRA_WATTAGE_LABEL, formatter.GetFormattedValue(num2, formatter.DeltaTimeSlice));
				stringBuilder.Append(stringBuilder2.ToString());
			}
			GlobalStringBuilderPool.Free(stringBuilder2);
		}
		Debug.Assert(instance.deltaAttribute.Modifiers.Count <= 0, "Bionic Battery Displayer has found an invalid AttributeModifier. This particular Amount should not use AttributeModifiers, instead, use BionicBatteryMonitor.Instance.Modifiers");
		float seconds = ((num == 0f) ? 0f : (sMI.CurrentCharge / num));
		stringBuilder.Append("\n\n");
		stringBuilder.AppendFormat(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.ESTIMATED_LIFE_TIME_REMAINING, GameUtil.GetFormattedCycles(seconds));
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public override string GetValueString(Amount master, AmountInstance instance)
	{
		return base.GetValueString(master, instance);
	}

	public BionicBatteryDisplayer()
		: base(GameUtil.UnitClass.Energy, GameUtil.TimeSlice.PerSecond)
	{
		formatter = new BionicBatteryAttributeFormatter();
	}
}
