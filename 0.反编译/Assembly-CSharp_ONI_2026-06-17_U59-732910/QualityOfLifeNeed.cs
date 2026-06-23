using Klei.AI;
using Klei.CustomSettings;
using STRINGS;
using UnityEngine;

[SkipSaveFileSerialization]
public class QualityOfLifeNeed : Need, ISim4000ms
{
	private AttributeInstance qolAttribute;

	public bool skipUpdate;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Attributes attributes = base.gameObject.GetAttributes();
		expectationAttribute = attributes.Add(Db.Get().Attributes.QualityOfLifeExpectation);
		base.Name = DUPLICANTS.NEEDS.QUALITYOFLIFE.NAME;
		base.ExpectationTooltip = string.Format(DUPLICANTS.NEEDS.QUALITYOFLIFE.EXPECTATION_TOOLTIP, Db.Get().Attributes.QualityOfLifeExpectation.Lookup(this).GetTotalValue());
		stressBonus = new ModifierType
		{
			modifier = new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 0f, DUPLICANTS.NEEDS.QUALITYOFLIFE.GOOD_MODIFIER, is_multiplier: false, uiOnly: false, is_readonly: false)
		};
		stressNeutral = new ModifierType
		{
			modifier = new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, -1f / 120f, DUPLICANTS.NEEDS.QUALITYOFLIFE.NEUTRAL_MODIFIER)
		};
		stressPenalty = new ModifierType
		{
			modifier = new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 0f, DUPLICANTS.NEEDS.QUALITYOFLIFE.BAD_MODIFIER, is_multiplier: false, uiOnly: false, is_readonly: false),
			statusItem = Db.Get().DuplicantStatusItems.PoorQualityOfLife
		};
		qolAttribute = Db.Get().Attributes.QualityOfLife.Lookup(base.gameObject);
	}

	public void Sim4000ms(float dt)
	{
		if (skipUpdate)
		{
			return;
		}
		float num = 0.004166667f;
		float b = 1f / 24f;
		SettingLevel currentQualitySetting = CustomGameSettings.Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.Morale);
		if (currentQualitySetting.id == "Disabled")
		{
			SetModifier(null);
			return;
		}
		if (currentQualitySetting.id == "Easy")
		{
			num = 0.0033333334f;
			b = 1f / 60f;
		}
		else if (currentQualitySetting.id == "Hard")
		{
			num = 1f / 120f;
			b = 0.05f;
		}
		else if (currentQualitySetting.id == "VeryHard")
		{
			num = 1f / 60f;
			b = 1f / 12f;
		}
		float totalValue = qolAttribute.GetTotalValue();
		float totalValue2 = expectationAttribute.GetTotalValue();
		float num2 = totalValue2 - totalValue;
		if (totalValue < totalValue2)
		{
			stressPenalty.modifier.SetValue(Mathf.Min(num2 * num, b));
			SetModifier(stressPenalty);
		}
		else if (totalValue > totalValue2)
		{
			stressBonus.modifier.SetValue(Mathf.Max((0f - num2) * (-1f / 60f), -1f / 30f));
			SetModifier(stressBonus);
		}
		else
		{
			SetModifier(stressNeutral);
		}
	}
}
