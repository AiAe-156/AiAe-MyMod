using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class BionicUpgradeComponent : Assignable, IGameObjectEffectDescriptor
{
	public interface IWattageController
	{
		float GetCurrentWattageCost();

		string GetCurrentWattageCostName();
	}

	private BionicUpgradeComponentConfig.BionicUpgradeData data;

	public System.Action OnWattageCostChanged;

	private Guid unassignedStatusItem = Guid.Empty;

	public IWattageController WattageController { get; private set; }

	public float CurrentWattage
	{
		get
		{
			if (!HasWattageController)
			{
				return 0f;
			}
			return WattageController.GetCurrentWattageCost();
		}
	}

	public string CurrentWattageName
	{
		get
		{
			if (!HasWattageController)
			{
				return string.Format(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.STANDARD_INACTIVE_TEMPLATE, this.GetProperName(), GameUtil.GetFormattedWattage(PotentialWattage));
			}
			return WattageController.GetCurrentWattageCostName();
		}
	}

	public bool HasWattageController => WattageController != null;

	public float PotentialWattage => data.WattageCost;

	public BionicUpgradeComponentConfig.BoosterType Booster => data.Booster;

	public Func<StateMachine.Instance, StateMachine.Instance> StateMachine => data.stateMachine;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		data = BionicUpgradeComponentConfig.UpgradesData[base.gameObject.PrefabID()];
		AddAssignPrecondition(AssignablePrecondition_OnlyOnBionics);
		AddAssignPrecondition(AssignablePrecondition_HasAvailableSlots);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Game.Instance.assignmentManager.Remove(this);
		customAssignmentUITooltipFunc = GetTooltipForBoosterAssignment;
		customAssignablesUITooltipFunc = GetTooltipForMinionAssigment;
		Subscribe(856640610, RefreshStatusItem);
		RefreshStatusItem();
	}

	private void RefreshStatusItem(object _ = null)
	{
		if (assignee == null && !base.gameObject.HasTag(GameTags.Stored))
		{
			if (unassignedStatusItem == Guid.Empty)
			{
				unassignedStatusItem = GetComponent<KSelectable>().AddStatusItem(Db.Get().MiscStatusItems.UnassignedBionicBooster);
			}
		}
		else if (unassignedStatusItem != Guid.Empty)
		{
			unassignedStatusItem = GetComponent<KSelectable>().RemoveStatusItem(unassignedStatusItem);
		}
	}

	public string GetTooltipForMinionAssigment(Assignables assignables)
	{
		MinionAssignablesProxy component = assignables.GetComponent<MinionAssignablesProxy>();
		if (component == null)
		{
			return "ERROR N/A";
		}
		GameObject targetGameObject = component.GetTargetGameObject();
		if (targetGameObject == null)
		{
			return "ERROR N/A";
		}
		BionicUpgradesMonitor.Instance sMI = targetGameObject.GetSMI<BionicUpgradesMonitor.Instance>();
		if (sMI == null)
		{
			return "This Duplicant cannot install boosters";
		}
		int num = sMI.CountBoosterAssignments(this.PrefabID());
		string text = ((num == 0) ? string.Format(UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.BOOSTER_ASSIGNMENT.NOT_ALREADY_ASSIGNED, sMI.gameObject.GetProperName(), num) : string.Format(UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.BOOSTER_ASSIGNMENT.ALREADY_ASSIGNED, sMI.gameObject.GetProperName(), num));
		string text2 = string.Format((sMI.AssignedSlotCount < sMI.UnlockedSlotCount) ? UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.BOOSTER_ASSIGNMENT.AVAILABLE_SLOTS : UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.BOOSTER_ASSIGNMENT.NO_AVAILABLE_SLOTS, targetGameObject.GetProperName(), sMI.AssignedSlotCount, sMI.UnlockedSlotCount);
		string text3 = "";
		List<AttributeInstance> list = new List<AttributeInstance>(targetGameObject.GetAttributes().AttributeTable).FindAll((AttributeInstance a) => a.Attribute.ShowInUI == Klei.AI.Attribute.Display.Skill);
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			string text4 = UIConstants.ColorPrefixWhite;
			if (list[num2].GetTotalValue() > 0f)
			{
				text4 = UIConstants.ColorPrefixGreen;
			}
			else if (list[num2].GetTotalValue() < 0f)
			{
				text4 = UIConstants.ColorPrefixRed;
			}
			text3 += $"{list[num2].Name}: {text4 + list[num2].GetFormattedValue() + UIConstants.ColorSuffix}";
			if (num2 != list.Count - 1)
			{
				text3 += "\n";
			}
		}
		return targetGameObject.GetProperName() + "\n\n" + text + "\n\n" + text2 + "\n\n" + text3;
	}

	public string GetTooltipForBoosterAssignment(Assignables assignables)
	{
		MinionAssignablesProxy component = assignables.GetComponent<MinionAssignablesProxy>();
		if (component == null)
		{
			return "ERROR N/A";
		}
		GameObject targetGameObject = component.GetTargetGameObject();
		if (targetGameObject == null)
		{
			return "ERROR N/A";
		}
		BionicUpgradesMonitor.Instance sMI = targetGameObject.GetSMI<BionicUpgradesMonitor.Instance>();
		if (sMI == null)
		{
			return "ERROR N/A";
		}
		int num = sMI.CountBoosterAssignments(this.PrefabID());
		string text = ((num == 0) ? string.Format(UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.BOOSTER_ASSIGNMENT.NOT_ALREADY_ASSIGNED, sMI.gameObject.GetProperName(), num) : string.Format(UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.BOOSTER_ASSIGNMENT.ALREADY_ASSIGNED, sMI.gameObject.GetProperName(), num));
		return BionicUpgradeComponentConfig.GenerateTooltipForBooster(this) + "\n\n" + text;
	}

	public void InformOfWattageChanged()
	{
		OnWattageCostChanged?.Invoke();
	}

	public void SetWattageController(IWattageController wattageController)
	{
		WattageController = wattageController;
	}

	public override void Assign(IAssignableIdentity new_assignee)
	{
		AssignableSlotInstance specificSlotInstance = null;
		if (new_assignee == assignee)
		{
			return;
		}
		if (new_assignee != assignee && (new_assignee is MinionIdentity || new_assignee is StoredMinionIdentity || new_assignee is MinionAssignablesProxy))
		{
			Ownables soleOwner = new_assignee.GetSoleOwner();
			if (soleOwner != null)
			{
				BionicUpgradesMonitor.Instance sMI = soleOwner.GetComponent<MinionAssignablesProxy>().GetTargetGameObject().GetSMI<BionicUpgradesMonitor.Instance>();
				if (sMI != null)
				{
					BionicUpgradesMonitor.UpgradeComponentSlot firstEmptyAvailableSlot = sMI.GetFirstEmptyAvailableSlot();
					if (firstEmptyAvailableSlot != null)
					{
						specificSlotInstance = firstEmptyAvailableSlot.GetAssignableSlotInstance();
					}
				}
			}
		}
		base.Assign(new_assignee, specificSlotInstance);
		Trigger(1980521255);
		RefreshStatusItem();
	}

	public override void Unassign()
	{
		base.Unassign();
		Trigger(1980521255);
		RefreshStatusItem();
	}

	private bool AssignablePrecondition_OnlyOnBionics(MinionAssignablesProxy worker)
	{
		return worker.GetMinionModel() == BionicMinionConfig.MODEL;
	}

	private bool AssignablePrecondition_HasAvailableSlots(MinionAssignablesProxy worker)
	{
		if (SelectTool.Instance.selected != null && SelectTool.Instance.selected.gameObject == worker.GetTargetGameObject())
		{
			return true;
		}
		MinionIdentity minionIdentity = worker.target as MinionIdentity;
		if (minionIdentity != null)
		{
			BionicUpgradesMonitor.Instance sMI = minionIdentity.GetSMI<BionicUpgradesMonitor.Instance>();
			if (sMI == null)
			{
				return true;
			}
			BionicUpgradesMonitor.UpgradeComponentSlot[] upgradeComponentSlots = sMI.upgradeComponentSlots;
			foreach (BionicUpgradesMonitor.UpgradeComponentSlot upgradeComponentSlot in upgradeComponentSlots)
			{
				if (!upgradeComponentSlot.IsLocked && (upgradeComponentSlot.assignedUpgradeComponent == null || upgradeComponentSlot.assignedUpgradeComponent == this))
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<Descriptor> GetDescriptors(GameObject go)
	{
		return new List<Descriptor>
		{
			new Descriptor(BionicUpgradeComponentConfig.UpgradesData[base.gameObject.PrefabID()].stateMachineDescription, null)
		};
	}
}
