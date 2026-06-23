using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class RocketInteriorSectionSideScreen : SideScreenContent
{
	public Image interiorModuleIcon;

	public KButton button;

	public LocText buttonLabel;

	public ToolTip tooltip;

	private CraftModuleInterface moduleInterface;

	public const string NOT_APPLICABLE_ICON_NAME = "rocket_no_habitat_module";

	public const string HABITAT_MODULE_SEE_INTERIOR_ICON_NAME = "rocket_small_habitat_open";

	public const string HABITAT_MODULE_SEE_EXTERIOR_ICON_NAME = "rocket_small_habitat_open_out";

	public Color noPassengerModuleImageColor = new Color(43f / 51f, 0.23529412f, 0.23529412f, 1f);

	private bool IsInterior = false;

	public override int GetSideScreenSortOrder()
	{
		return 105;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		button.onClick += ClickViewInterior;
	}

	public override bool IsValidForTarget(GameObject target)
	{
		RocketModuleCluster component = target.GetComponent<RocketModuleCluster>();
		ClustercraftInteriorDoor component2 = target.GetComponent<ClustercraftInteriorDoor>();
		RocketControlStation component3 = target.GetComponent<RocketControlStation>();
		Clustercraft component4 = target.GetComponent<Clustercraft>();
		return component != null || component3 != null || component2 != null || component4 != null;
	}

	public override void SetTarget(GameObject new_target)
	{
		if (new_target == null)
		{
			Debug.LogError("Invalid gameObject received");
			return;
		}
		RocketModuleCluster component = new_target.GetComponent<RocketModuleCluster>();
		RocketControlStation component2 = new_target.GetComponent<RocketControlStation>();
		ClustercraftInteriorDoor component3 = new_target.GetComponent<ClustercraftInteriorDoor>();
		Clustercraft component4 = new_target.GetComponent<Clustercraft>();
		IsInterior = component4 == null && component == null && (component2 != null || component3 != null);
		if (component != null)
		{
			moduleInterface = component.CraftInterface;
		}
		else if (component4 != null)
		{
			moduleInterface = component4.ModuleInterface;
		}
		else
		{
			moduleInterface = new_target.GetMyWorld().GetComponent<Clustercraft>().ModuleInterface;
		}
		moduleInterface.Unsubscribe(1512695988, OnRocketModuleCountChanged);
		moduleInterface.Subscribe(1512695988, OnRocketModuleCountChanged);
		Refresh();
	}

	public override void ClearTarget()
	{
		if (moduleInterface != null)
		{
			moduleInterface.Unsubscribe(1512695988, OnRocketModuleCountChanged);
		}
		base.ClearTarget();
	}

	private void OnRocketModuleCountChanged(object o)
	{
		Refresh();
	}

	public void Refresh()
	{
		PassengerRocketModule passengerModule = GetPassengerModule();
		ClustercraftExteriorDoor clustercraftExteriorDoor = ((passengerModule == null) ? null : passengerModule.GetComponent<ClustercraftExteriorDoor>());
		bool flag = clustercraftExteriorDoor != null && clustercraftExteriorDoor.GetMyWorld() != null;
		button.isInteractable = passengerModule != null && (!IsInterior || flag);
		buttonLabel.SetText(IsInterior ? UI.UISIDESCREENS.ROCKETVIEWINTERIORSECTION.BUTTONVIEWEXTERIOR.LABEL : UI.UISIDESCREENS.ROCKETVIEWINTERIORSECTION.BUTTONVIEWINTERIOR.LABEL);
		tooltip.SetSimpleTooltip((!(passengerModule != null)) ? UI.UISIDESCREENS.ROCKETVIEWINTERIORSECTION.BUTTONVIEWINTERIOR.INVALID.text : ((!IsInterior) ? UI.UISIDESCREENS.ROCKETVIEWINTERIORSECTION.BUTTONVIEWINTERIOR.DESC.text : (flag ? UI.UISIDESCREENS.ROCKETVIEWINTERIORSECTION.BUTTONVIEWEXTERIOR.DESC.text : UI.UISIDESCREENS.ROCKETVIEWINTERIORSECTION.BUTTONVIEWEXTERIOR.INVALID.text)));
		Sprite sprite = null;
		sprite = ((!(passengerModule != null)) ? Assets.GetSprite("rocket_no_habitat_module") : Assets.GetSprite(IsInterior ? "rocket_small_habitat_open_out" : "rocket_small_habitat_open"));
		interiorModuleIcon.sprite = sprite;
		interiorModuleIcon.color = Color.white;
	}

	private PassengerRocketModule GetPassengerModule()
	{
		return (moduleInterface == null) ? null : moduleInterface.GetPassengerModule();
	}

	private void ClickViewInterior()
	{
		PassengerRocketModule passengerModule = GetPassengerModule();
		if (passengerModule == null)
		{
			Refresh();
			return;
		}
		ClustercraftExteriorDoor component = passengerModule.GetComponent<ClustercraftExteriorDoor>();
		WorldContainer targetWorld = component.GetTargetWorld();
		WorldContainer myWorld = component.GetMyWorld();
		if (ClusterManager.Instance.activeWorld == targetWorld)
		{
			if (myWorld != null && myWorld.id != 255)
			{
				AudioMixer.instance.Stop(passengerModule.interiorReverbSnapshot);
				AudioMixer.instance.PauseSpaceVisibleSnapshot(pause: false);
				ClusterManager.Instance.SetActiveWorld(myWorld.id);
				SelectTool.Instance.Select(passengerModule.GetComponent<KSelectable>());
			}
		}
		else
		{
			AudioMixer.instance.Start(passengerModule.interiorReverbSnapshot);
			AudioMixer.instance.PauseSpaceVisibleSnapshot(pause: true);
			ClusterManager.Instance.SetActiveWorld(targetWorld.id);
			bool flag = false;
			if (Components.RocketControlStations != null)
			{
				List<RocketControlStation> worldItems = Components.RocketControlStations.GetWorldItems(targetWorld.id);
				if (worldItems != null && worldItems.Count > 0)
				{
					RocketControlStation rocketControlStation = worldItems[0];
					SelectTool.Instance.Select(rocketControlStation.GetComponent<KSelectable>());
					flag = true;
				}
			}
			if (!flag)
			{
				ClustercraftInteriorDoor interiorDoor = component.GetInteriorDoor();
				if (interiorDoor != null)
				{
					SelectTool.Instance.Select(interiorDoor.GetComponent<KSelectable>());
				}
			}
		}
		DetailsScreen.Instance.ClearSecondarySideScreen();
		ManagementMenu.Instance.CloseClusterMap();
		Refresh();
	}
}
