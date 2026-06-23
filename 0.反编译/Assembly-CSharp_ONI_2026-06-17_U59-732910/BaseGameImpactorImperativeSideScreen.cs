using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class BaseGameImpactorImperativeSideScreen : SideScreenContent
{
	private MissileLauncher.Instance targetMissileLauncher;

	[SerializeField]
	private Image healthBarFill;

	[SerializeField]
	private Image timeBarFill;

	[SerializeField]
	private LocText healthBarLabel;

	[SerializeField]
	private LocText timeBarLabel;

	[SerializeField]
	private ToolTip healthBarTooltip;

	[SerializeField]
	private ToolTip timeBarTooltip;

	private LargeImpactorStatus.Instance statusMonitor;

	private LargeImpactorStatus.Instance StatusMonitor
	{
		get
		{
			if (statusMonitor == null)
			{
				GameplayEventInstance gameplayEventInstance = GameplayEventManager.Instance.GetGameplayEventInstance(Db.Get().GameplayEvents.LargeImpactor.Id);
				if (gameplayEventInstance != null)
				{
					LargeImpactorEvent.StatesInstance statesInstance = (LargeImpactorEvent.StatesInstance)gameplayEventInstance.smi;
					statusMonitor = statesInstance.impactorInstance.GetSMI<LargeImpactorStatus.Instance>();
				}
			}
			return statusMonitor;
		}
	}

	public override bool IsValidForTarget(GameObject target)
	{
		if (DlcManager.IsExpansion1Active())
		{
			return false;
		}
		MissileLauncher.Instance sMI = target.GetSMI<MissileLauncher.Instance>();
		if (sMI == null)
		{
			return false;
		}
		if (StatusMonitor == null)
		{
			return false;
		}
		return sMI.AmmunitionIsAllowed("MissileLongRange");
	}

	public override void SetTarget(GameObject target)
	{
		base.SetTarget(target);
		targetMissileLauncher = target.GetSMI<MissileLauncher.Instance>();
		Build();
	}

	private void Build()
	{
		if (StatusMonitor != null)
		{
			healthBarFill.fillAmount = Mathf.Max((float)StatusMonitor.Health / (float)StatusMonitor.def.MAX_HEALTH, 0f);
			healthBarTooltip.toolTip = GameUtil.SafeStringFormat(UI.UISIDESCREENS.MISSILESELECTIONSIDESCREEN.VANILLALARGEIMPACTOR.HEALTH_BAR_TOOLTIP, StatusMonitor.Health, StatusMonitor.def.MAX_HEALTH);
			timeBarFill.fillAmount = StatusMonitor.TimeRemainingBeforeCollision / LargeImpactorEvent.GetImpactTime();
			timeBarTooltip.toolTip = GameUtil.SafeStringFormat(UI.UISIDESCREENS.MISSILESELECTIONSIDESCREEN.VANILLALARGEIMPACTOR.TIME_UNTIL_COLLISION_TOOLTIP, GameUtil.GetFormattedCycles(StatusMonitor.TimeRemainingBeforeCollision).Split(' ')[0]);
		}
	}
}
