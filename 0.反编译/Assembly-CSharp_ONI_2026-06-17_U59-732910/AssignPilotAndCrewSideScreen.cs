using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class AssignPilotAndCrewSideScreen : SideScreenContent
{
	public const string NO_PILOT_SPRITE_NAME = "dreamIcon_Unknown";

	public const string ROBOPILOT_SPRITE_NAME = "Dreamicon_robopilot";

	public LocText infoLabel;

	public ToolTip editCrewTooltip;

	public Image pilotImage;

	public Image copilotImage;

	private Dictionary<KToggle, PassengerRocketModule.RequestCrewState> toggleMap = new Dictionary<KToggle, PassengerRocketModule.RequestCrewState>();

	public KButton editCrewButton;

	public KScreen changeCrewSideScreenPrefab;

	private CraftModuleInterface craftModuleInterface;

	private AssignmentGroupControllerSideScreen activeChangeCrewSideScreen;

	protected override void OnSpawn()
	{
		editCrewButton.onClick += OnChangeCrewButtonPressed;
	}

	public override int GetSideScreenSortOrder()
	{
		return 102;
	}

	public override bool IsValidForTarget(GameObject target)
	{
		RocketModuleCluster component = target.GetComponent<RocketModuleCluster>();
		RocketControlStation component2 = target.GetComponent<RocketControlStation>();
		bool num = component != null && component.GetComponent<PassengerRocketModule>() != null;
		bool flag = component != null && component.GetComponent<RoboPilotModule>() != null;
		if (num || flag)
		{
			return true;
		}
		if (component2 != null)
		{
			RocketControlStation.StatesInstance sMI = component2.GetSMI<RocketControlStation.StatesInstance>();
			if (!sMI.sm.IsInFlight(sMI))
			{
				return !sMI.sm.IsLaunching(sMI);
			}
			return false;
		}
		return false;
	}

	public override void SetTarget(GameObject target)
	{
		RocketModuleCluster component = target.GetComponent<RocketModuleCluster>();
		if (component != null)
		{
			craftModuleInterface = component.CraftInterface;
		}
		else if (target.GetComponent<RocketControlStation>() != null)
		{
			craftModuleInterface = target.GetMyWorld().GetComponent<Clustercraft>().ModuleInterface;
		}
		craftModuleInterface.Unsubscribe(1512695988, OnRocketModuleCountChanged);
		craftModuleInterface.Subscribe(1512695988, OnRocketModuleCountChanged);
		Game.Instance.Unsubscribe(-1123234494, OnAssignmentGroupChanged);
		Game.Instance.Subscribe(-1123234494, OnAssignmentGroupChanged);
		Refresh();
	}

	public override void ClearTarget()
	{
		if (craftModuleInterface != null)
		{
			craftModuleInterface.Unsubscribe(1512695988, OnRocketModuleCountChanged);
		}
		base.ClearTarget();
		Game.Instance.Unsubscribe(-1123234494, OnAssignmentGroupChanged);
		craftModuleInterface = null;
	}

	private void OnRocketModuleCountChanged(object o)
	{
		Refresh();
	}

	private void OnAssignmentGroupChanged(object o)
	{
		Refresh();
	}

	private void OnChangeCrewButtonPressed()
	{
		if (activeChangeCrewSideScreen == null)
		{
			activeChangeCrewSideScreen = (AssignmentGroupControllerSideScreen)DetailsScreen.Instance.SetSecondarySideScreen(changeCrewSideScreenPrefab, UI.UISIDESCREENS.ASSIGNMENTGROUPCONTROLLER.TITLE);
			activeChangeCrewSideScreen.SetTarget(craftModuleInterface.GetPassengerModule().gameObject);
		}
		else
		{
			CloseSecondaryScreen();
		}
	}

	private void CloseSecondaryScreen()
	{
		DetailsScreen.Instance.ClearSecondarySideScreen();
		activeChangeCrewSideScreen = null;
	}

	protected override void OnShow(bool show)
	{
		base.OnShow(show);
		if (!show)
		{
			DetailsScreen.Instance.ClearSecondarySideScreen();
			activeChangeCrewSideScreen = null;
		}
	}

	private void Refresh()
	{
		PassengerRocketModule passengerModule = craftModuleInterface.GetPassengerModule();
		GameObject gameObject = ((passengerModule == null) ? null : passengerModule.GetDupePilot());
		bool num = craftModuleInterface.GetRobotPilotModule() != null;
		bool flag = gameObject != null;
		bool flag2 = num && !flag;
		bool flag3 = num || flag;
		bool flag4 = num && flag;
		if (passengerModule == null && activeChangeCrewSideScreen != null)
		{
			CloseSecondaryScreen();
		}
		if (flag4)
		{
			copilotImage.sprite = Assets.GetSprite("Dreamicon_robopilot");
		}
		copilotImage.gameObject.SetActive(flag4);
		Sprite sprite = null;
		editCrewButton.isInteractable = passengerModule != null;
		editCrewTooltip.SetSimpleTooltip((passengerModule != null) ? UI.UISIDESCREENS.PILOT_AND_CREW_SIDESCREEN.EDIT_CREW_BUTTON_TOOLTIP : UI.UISIDESCREENS.PILOT_AND_CREW_SIDESCREEN.EDIT_CREW_BUTTON_DISABLED_TOOLTIP);
		if (!flag3)
		{
			sprite = Assets.GetSprite("dreamIcon_Unknown");
			infoLabel.SetText(GameUtil.SafeStringFormat(UI.UISIDESCREENS.PILOT_AND_CREW_SIDESCREEN.INFO_LABEL, UI.UISIDESCREENS.PILOT_AND_CREW_SIDESCREEN.NO_ASSIGNED_NAME));
		}
		else
		{
			sprite = (flag2 ? Assets.GetSprite("Dreamicon_robopilot") : Db.Get().Personalities.Get(gameObject.GetComponent<MinionIdentity>().personalityResourceId).GetMiniIcon());
			if (flag2)
			{
				infoLabel.SetText(UI.UISIDESCREENS.PILOT_AND_CREW_SIDESCREEN.INFO_LABEL_ROBOT_ONLY);
			}
			else
			{
				infoLabel.SetText(GameUtil.SafeStringFormat(UI.UISIDESCREENS.PILOT_AND_CREW_SIDESCREEN.INFO_LABEL, gameObject.GetProperName()));
			}
		}
		pilotImage.sprite = sprite;
	}
}
