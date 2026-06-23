using System;
using STRINGS;

public class BionicUpgrade_OnGoingEffect : BionicUpgrade_SM<BionicUpgrade_OnGoingEffect, BionicUpgrade_OnGoingEffect.Instance>
{
	public new class Def : BionicUpgrade_SM<BionicUpgrade_OnGoingEffect, Instance>.Def
	{
		public string EFFECT_NAME;

		public string[] SKILLS_IDS;

		public Def(string upgradeID, string effectID, string[] skills = null)
			: base(upgradeID)
		{
			EFFECT_NAME = effectID;
			SKILLS_IDS = skills;
		}

		public override string GetDescription()
		{
			return "BionicUpgrade_OnGoingEffect.Def description not implemented";
		}
	}

	public new class Instance : BaseInstance
	{
		private MinionResume resume;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, (BionicUpgrade_SM<BionicUpgrade_OnGoingEffect, Instance>.Def)def)
		{
			resume = GetComponent<MinionResume>();
		}

		public override float GetCurrentWattageCost()
		{
			if (IsInsideState(base.sm.Active))
			{
				return base.Data.WattageCost;
			}
			return 0f;
		}

		public override string GetCurrentWattageCostName()
		{
			float currentWattageCost = GetCurrentWattageCost();
			if (IsInsideState(base.sm.Active))
			{
				string text = "<b>" + ((currentWattageCost >= 0f) ? "+" : "-") + "</b>";
				return string.Format(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.STANDARD_ACTIVE_TEMPLATE, upgradeComponent.GetProperName(), text + GameUtil.GetFormattedWattage(currentWattageCost));
			}
			return string.Format(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.STANDARD_INACTIVE_TEMPLATE, upgradeComponent.GetProperName(), GameUtil.GetFormattedWattage(upgradeComponent.PotentialWattage));
		}

		public void ApplySkills()
		{
			Def def = (Def)base.def;
			if (def.SKILLS_IDS != null)
			{
				for (int i = 0; i < def.SKILLS_IDS.Length; i++)
				{
					string skillId = def.SKILLS_IDS[i];
					resume.GrantSkill(skillId);
				}
			}
		}

		public void RemoveSkills()
		{
			Def def = (Def)base.def;
			if (def.SKILLS_IDS != null)
			{
				for (int i = 0; i < def.SKILLS_IDS.Length; i++)
				{
					string skillId = def.SKILLS_IDS[i];
					resume.UngrantSkill(skillId);
				}
			}
		}
	}

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = Inactive;
		Inactive.EventTransition(GameHashes.ScheduleBlocksChanged, Active, IsOnlineAndNotInBatterySaveMode).EventTransition(GameHashes.ScheduleChanged, Active, IsOnlineAndNotInBatterySaveMode).EventTransition(GameHashes.BionicOnline, Active, IsOnlineAndNotInBatterySaveMode)
			.TriggerOnEnter(GameHashes.BionicUpgradeWattageChanged);
		Active.ToggleEffect((Func<Instance, string>)GetEffectName).Enter(ApplySkills).Exit(RemoveSkills)
			.EventTransition(GameHashes.ScheduleBlocksChanged, Inactive, BionicUpgrade_SM<BionicUpgrade_OnGoingEffect, Instance>.IsInBedTimeChore)
			.EventTransition(GameHashes.ScheduleChanged, Inactive, BionicUpgrade_SM<BionicUpgrade_OnGoingEffect, Instance>.IsInBedTimeChore)
			.EventTransition(GameHashes.BionicOffline, Inactive, GameStateMachine<BionicUpgrade_OnGoingEffect, Instance, IStateMachineTarget, BionicUpgrade_SM<BionicUpgrade_OnGoingEffect, Instance>.Def>.Not(BionicUpgrade_SM<BionicUpgrade_OnGoingEffect, Instance>.IsOnline))
			.TriggerOnEnter(GameHashes.BionicUpgradeWattageChanged);
	}

	public static string GetEffectName(Instance smi)
	{
		return ((Def)smi.def).EFFECT_NAME;
	}

	public static void ApplySkills(Instance smi)
	{
		smi.ApplySkills();
	}

	public static void RemoveSkills(Instance smi)
	{
		smi.RemoveSkills();
	}

	public static bool IsOnlineAndNotInBatterySaveMode(Instance smi)
	{
		if (BionicUpgrade_SM<BionicUpgrade_OnGoingEffect, Instance>.IsOnline(smi))
		{
			return !BionicUpgrade_SM<BionicUpgrade_OnGoingEffect, Instance>.IsInBedTimeChore(smi);
		}
		return false;
	}
}
