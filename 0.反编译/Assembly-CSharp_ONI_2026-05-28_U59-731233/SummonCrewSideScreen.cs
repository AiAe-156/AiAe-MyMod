using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class SummonCrewSideScreen : SideScreenContent, ISim1000ms
{
	private enum CurrentState
	{
		NoCrewFound,
		NoCrewNeeded,
		PublicAccess,
		AwaitingCrew,
		Ready
	}

	public const string READY_ICON_NAME = "ic_checklist";

	public const string NOT_APPLICABLE_ICON_NAME = "rocket_red_icon";

	public const string PUBLIC_ACCESS_ICON_NAME = "status_item_change_door_control_state";

	public const string AWAITING_ICON_NAME = "crew_boarded";

	public Image image;

	public LocText infoLabel;

	public ToolTip infoLabelTooltip;

	public KButton button;

	public LocText buttonLabel;

	public ToolTip buttonTooltip;

	private CraftModuleInterface craftModuleInterface;

	private Color noCrewColor = Color.white;

	private Color defaultColor = new Color(0.5568628f, 0.5568628f, 0.5568628f, 1f);

	private Color readyColor = new Color(0f, 0.58431375f, 0.23137255f, 1f);

	private bool refreshInUpdate = false;

	protected override void OnSpawn()
	{
		button.onClick += OnButtonPressed;
	}

	public override int GetSideScreenSortOrder()
	{
		return 101;
	}

	public override bool IsValidForTarget(GameObject target)
	{
		RocketModuleCluster component = target.GetComponent<RocketModuleCluster>();
		RocketControlStation component2 = target.GetComponent<RocketControlStation>();
		bool flag = component != null && component.GetComponent<PassengerRocketModule>() != null;
		bool flag2 = component != null && component.GetComponent<RoboPilotModule>() != null;
		if (flag || flag2)
		{
			return true;
		}
		if (component2 != null)
		{
			RocketControlStation.StatesInstance sMI = component2.GetSMI<RocketControlStation.StatesInstance>();
			return !sMI.sm.IsInFlight(sMI) && !sMI.sm.IsLaunching(sMI);
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
		Game.Instance.Unsubscribe(586301400, OnMinionsChangedWorld);
		Game.Instance.Unsubscribe(-1123234494, OnAssignmentGroupChanged);
		Game.Instance.Subscribe(586301400, OnMinionsChangedWorld);
		Game.Instance.Subscribe(-1123234494, OnAssignmentGroupChanged);
		Refresh();
	}

	private void OnMinionsChangedWorld(object o)
	{
		Refresh();
	}

	public override void ClearTarget()
	{
		refreshInUpdate = false;
		if (craftModuleInterface != null)
		{
			craftModuleInterface.Unsubscribe(1512695988, OnRocketModuleCountChanged);
		}
		base.ClearTarget();
		Game.Instance.Unsubscribe(-1123234494, OnAssignmentGroupChanged);
		Game.Instance.Unsubscribe(586301400, OnMinionsChangedWorld);
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

	private void OnButtonPressed()
	{
		ToggleCrewRequestState();
		Refresh();
	}

	private void ToggleCrewRequestState()
	{
		PassengerRocketModule passengerModule = craftModuleInterface.GetPassengerModule();
		if (passengerModule != null)
		{
			if (passengerModule.PassengersRequested == PassengerRocketModule.RequestCrewState.Request)
			{
				passengerModule.RequestCrewBoard(PassengerRocketModule.RequestCrewState.Release);
			}
			else
			{
				passengerModule.RequestCrewBoard(PassengerRocketModule.RequestCrewState.Request);
			}
		}
	}

	protected override void OnShow(bool show)
	{
		base.OnShow(show);
	}

	private void Refresh()
	{
		refreshInUpdate = false;
		PassengerRocketModule passengerModule = craftModuleInterface.GetPassengerModule();
		RoboPilotModule robotPilotModule = craftModuleInterface.GetRobotPilotModule();
		int num = ((!(passengerModule == null)) ? passengerModule.GetCrewCount() : 0);
		bool flag = passengerModule != null;
		bool flag2 = num > 0;
		bool flag3 = robotPilotModule != null;
		Tuple<int, int> tuple = null;
		button.isInteractable = passengerModule != null && flag2;
		CurrentState currentState = CurrentState.NoCrewFound;
		if (!flag || !flag2)
		{
			currentState = CurrentState.NoCrewFound;
			if (flag3)
			{
				currentState = CurrentState.NoCrewNeeded;
			}
		}
		else if (passengerModule.PassengersRequested == PassengerRocketModule.RequestCrewState.Release)
		{
			currentState = CurrentState.PublicAccess;
		}
		else
		{
			tuple = passengerModule.GetCrewBoardedFraction();
			currentState = ((tuple.first >= tuple.second) ? CurrentState.Ready : CurrentState.AwaitingCrew);
		}
		Sprite sprite = null;
		Color color = defaultColor;
		string text = "";
		string simpleTooltip = "";
		string text2 = "";
		string simpleTooltip2 = "";
		switch (currentState)
		{
		case CurrentState.NoCrewFound:
			sprite = Assets.GetSprite("rocket_red_icon");
			color = noCrewColor;
			text = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.INFO_LABEL_NO_CREW_FOUND;
			simpleTooltip = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.INFO_LABEL_TOOLTIP_NO_CREW_FOUND;
			text2 = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.SUMMON_CREW_BUTTON_LABEL;
			simpleTooltip2 = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.SUMMON_CREW_BUTTON_TOOLTIP;
			break;
		case CurrentState.NoCrewNeeded:
			sprite = Assets.GetSprite("ic_checklist");
			color = readyColor;
			text = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.INFO_LABEL_NO_CREW_NEEDED;
			simpleTooltip = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.INFO_LABEL_TOOLTIP_NO_CREW_NEEDED;
			text2 = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.SUMMON_CREW_BUTTON_LABEL;
			simpleTooltip2 = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.SUMMON_CREW_BUTTON_TOOLTIP;
			break;
		case CurrentState.PublicAccess:
			sprite = Assets.GetSprite("status_item_change_door_control_state");
			text = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.INFO_LABEL_PUBLIC_ACCESS;
			simpleTooltip = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.INFO_LABEL_TOOLTIP_PUBLIC_ACCESS;
			text2 = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.SUMMON_CREW_BUTTON_LABEL;
			simpleTooltip2 = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.SUMMON_CREW_BUTTON_TOOLTIP;
			break;
		case CurrentState.AwaitingCrew:
			refreshInUpdate = true;
			sprite = Assets.GetSprite("crew_boarded");
			text = GameUtil.SafeStringFormat(UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.INFO_LABEL_AWAITING_CREW, GameUtil.GetFormattedInt(tuple.first), GameUtil.GetFormattedInt(tuple.second));
			simpleTooltip = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.INFO_LABEL_TOOLTIP_AWAITING_CREW;
			text2 = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.CANCEL_BUTTON_LABEL;
			simpleTooltip2 = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.CANCEL_BUTTON_TOOLTIP;
			break;
		case CurrentState.Ready:
			sprite = Assets.GetSprite("ic_checklist");
			color = readyColor;
			text = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.INFO_LABEL_CREW_READY;
			simpleTooltip = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.INFO_LABEL_TOOLTIP_CREW_READY;
			text2 = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.CANCEL_BUTTON_LABEL;
			simpleTooltip2 = UI.UISIDESCREENS.SUMMON_CREW_SIDESCREEN.CANCEL_BUTTON_TOOLTIP;
			break;
		}
		infoLabel.SetText(text);
		infoLabelTooltip.SetSimpleTooltip(simpleTooltip);
		buttonLabel.SetText(text2);
		buttonTooltip.SetSimpleTooltip(simpleTooltip2);
		image.sprite = sprite;
		image.color = color;
	}

	public void Sim1000ms(float dt)
	{
		if (refreshInUpdate)
		{
			Refresh();
		}
	}
}
