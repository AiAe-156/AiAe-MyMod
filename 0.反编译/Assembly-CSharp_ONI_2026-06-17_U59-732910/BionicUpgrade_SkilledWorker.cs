using Database;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class BionicUpgrade_SkilledWorker : BionicUpgrade_SM<BionicUpgrade_SkilledWorker, BionicUpgrade_SkilledWorker.Instance>
{
	public new class Def : BionicUpgrade_SM<BionicUpgrade_SkilledWorker, Instance>.Def
	{
		public SkillPerk[] SkillPerksIds;

		public string AttributeId;

		public AttributeModifier[] modifiers;

		public string[] hats;

		public Def(string upgradeID, string attributeID, AttributeModifier[] modifiers = null, SkillPerk[] skillPerks = null, string[] hats = null)
			: base(upgradeID)
		{
			AttributeId = attributeID;
			this.modifiers = modifiers;
			SkillPerksIds = skillPerks;
			this.hats = hats;
		}

		public override string GetDescription()
		{
			string text = "";
			if (SkillPerksIds.Length != 0)
			{
				text += UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.BOOSTER_ASSIGNMENT.HEADER_PERKS;
				for (int i = 0; i < SkillPerksIds.Length; i++)
				{
					text += "\n";
					text += SkillPerk.GetDescription(SkillPerksIds[i].Id);
				}
				if (modifiers.Length != 0)
				{
					text += "\n\n";
				}
			}
			if (modifiers.Length != 0)
			{
				text += UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.BOOSTER_ASSIGNMENT.HEADER_ATTRIBUTES;
				for (int j = 0; j < modifiers.Length; j++)
				{
					text += "\n";
					text = text + modifiers[j].GetName() + ": " + modifiers[j].GetFormattedString();
				}
			}
			return text;
		}
	}

	public new class Instance : BaseInstance
	{
		[MyCmpGet]
		public WorkerBase worker;

		[MyCmpGet]
		public MinionResume resume;

		public BionicAttributeUseFx.Instance fx;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, (BionicUpgrade_SM<BionicUpgrade_SkilledWorker, Instance>.Def)def)
		{
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

		public void ApplyModifiers()
		{
			Klei.AI.Attributes attributes = resume.GetIdentity.GetAttributes();
			AttributeModifier[] modifiers = ((Def)base.smi.def).modifiers;
			foreach (AttributeModifier modifier in modifiers)
			{
				attributes.Add(modifier);
			}
		}

		public void RemoveModifiers()
		{
			Klei.AI.Attributes attributes = resume.GetIdentity.GetAttributes();
			AttributeModifier[] modifiers = ((Def)base.smi.def).modifiers;
			foreach (AttributeModifier modifier in modifiers)
			{
				attributes.Remove(modifier);
			}
		}

		public void ApplyHats()
		{
			string[] hats = ((Def)base.smi.def).hats;
			if (hats != null)
			{
				MinionResume component = GetComponent<MinionResume>();
				string properName = Assets.GetPrefab(base.smi.def.UpgradeID).GetProperName();
				string[] array = hats;
				foreach (string hat in array)
				{
					component.AddAdditionalHat(properName, hat);
				}
			}
		}

		public void RemoveHats()
		{
			string[] hats = ((Def)base.smi.def).hats;
			if (hats != null)
			{
				MinionResume component = GetComponent<MinionResume>();
				string properName = Assets.GetPrefab(base.smi.def.UpgradeID).GetProperName();
				string[] array = hats;
				foreach (string hat in array)
				{
					component.RemoveAdditionalHat(properName, hat);
				}
			}
		}
	}

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = Inactive;
		root.Enter(ApplySkillPerks).Exit(RemoveSkillPerks).Enter(ApplyModifiers)
			.Exit(RemoveModifiers)
			.Enter(ApplyHats)
			.Exit(RemoveHats);
		Inactive.EventTransition(GameHashes.ScheduleBlocksChanged, Active, IsMinionWorkingOnlineAndNotInBatterySaveMode).EventTransition(GameHashes.ScheduleChanged, Active, IsMinionWorkingOnlineAndNotInBatterySaveMode).EventTransition(GameHashes.BionicOnline, Active, IsMinionWorkingOnlineAndNotInBatterySaveMode)
			.EventTransition(GameHashes.StartWork, Active, IsMinionWorkingOnlineAndNotInBatterySaveMode)
			.TriggerOnEnter(GameHashes.BionicUpgradeWattageChanged);
		Active.EventTransition(GameHashes.ScheduleBlocksChanged, Inactive, BionicUpgrade_SM<BionicUpgrade_SkilledWorker, Instance>.IsInBedTimeChore).EventTransition(GameHashes.ScheduleChanged, Inactive, BionicUpgrade_SM<BionicUpgrade_SkilledWorker, Instance>.IsInBedTimeChore).EventTransition(GameHashes.BionicOffline, Inactive)
			.EventTransition(GameHashes.StopWork, Inactive)
			.TriggerOnEnter(GameHashes.BionicUpgradeWattageChanged)
			.Enter(CreateFX)
			.Exit(ClearFX);
	}

	public static void ApplySkillPerks(Instance smi)
	{
		smi.resume.ApplyAdditionalSkillPerks(((Def)smi.def).SkillPerksIds);
	}

	public static void RemoveSkillPerks(Instance smi)
	{
		smi.resume.RemoveAdditionalSkillPerks(((Def)smi.def).SkillPerksIds);
	}

	public static void ApplyModifiers(Instance smi)
	{
		smi.ApplyModifiers();
	}

	public static void RemoveModifiers(Instance smi)
	{
		smi.RemoveModifiers();
	}

	public static void ApplyHats(Instance smi)
	{
		smi.ApplyHats();
	}

	public static void RemoveHats(Instance smi)
	{
		smi.RemoveHats();
	}

	public static bool IsMinionWorkingOnlineAndNotInBatterySaveMode(Instance smi)
	{
		if (BionicUpgrade_SM<BionicUpgrade_SkilledWorker, Instance>.IsOnline(smi) && !BionicUpgrade_SM<BionicUpgrade_SkilledWorker, Instance>.IsInBedTimeChore(smi))
		{
			return IsMinionWorkingWithAttribute(smi);
		}
		return false;
	}

	public static bool IsMinionWorkingWithAttribute(Instance smi)
	{
		Workable workable = smi.worker.GetWorkable();
		if (workable != null && smi.worker.GetState() == WorkerBase.State.Working && workable.GetWorkAttribute() != null)
		{
			return workable.GetWorkAttribute().Id == ((Def)smi.def).AttributeId;
		}
		return false;
	}

	public static void CreateFX(Instance smi)
	{
		CreateAndReturnFX(smi);
	}

	public static BionicAttributeUseFx.Instance CreateAndReturnFX(Instance smi)
	{
		if (!smi.isMasterNull)
		{
			smi.fx = new BionicAttributeUseFx.Instance(smi.GetComponent<KMonoBehaviour>(), new Vector3(0f, 0f, Grid.GetLayerZ(Grid.SceneLayer.FXFront)));
			smi.fx.StartSM();
			return smi.fx;
		}
		return null;
	}

	public static void ClearFX(Instance smi)
	{
		smi.fx.sm.destroyFX.Trigger(smi.fx);
		smi.fx = null;
	}
}
