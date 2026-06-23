using System.Collections.Generic;
using FMODUnity;
using KSerialization;
using UnityEngine;

public class PassengerRocketModule : KMonoBehaviour
{
	public enum RequestCrewState
	{
		Release,
		Request
	}

	public EventReference interiorReverbSnapshot;

	[Serialize]
	private RequestCrewState passengersRequested = RequestCrewState.Release;

	private static readonly EventSystem.IntraObjectHandler<PassengerRocketModule> OnRocketOnGroundTagDelegate = GameUtil.CreateHasTagHandler(GameTags.RocketOnGround, delegate(PassengerRocketModule component, object data)
	{
		component.RequestCrewBoard(RequestCrewState.Release);
	});

	private static readonly EventSystem.IntraObjectHandler<PassengerRocketModule> OnClustercraftStateChanged = new EventSystem.IntraObjectHandler<PassengerRocketModule>(delegate(PassengerRocketModule cmp, object data)
	{
		cmp.RefreshClusterStateForAudio();
	});

	private static EventSystem.IntraObjectHandler<PassengerRocketModule> RefreshDelegate = new EventSystem.IntraObjectHandler<PassengerRocketModule>(delegate(PassengerRocketModule cmp, object data)
	{
		cmp.RefreshOrders();
		cmp.RefreshClusterStateForAudio();
	});

	private static EventSystem.IntraObjectHandler<PassengerRocketModule> OnLaunchDelegate = new EventSystem.IntraObjectHandler<PassengerRocketModule>(delegate(PassengerRocketModule component, object data)
	{
		component.ClearMinionAssignments(data);
	});

	private static readonly EventSystem.IntraObjectHandler<PassengerRocketModule> OnReachableChangedDelegate = new EventSystem.IntraObjectHandler<PassengerRocketModule>(delegate(PassengerRocketModule component, object data)
	{
		component.OnReachableChanged(data);
	});

	public RequestCrewState PassengersRequested => passengersRequested;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Game.Instance.Subscribe(-1123234494, OnAssignmentGroupChanged);
		GameUtil.SubscribeToTags(this, OnRocketOnGroundTagDelegate, triggerImmediately: false);
		Subscribe(-1547247383, OnClustercraftStateChanged);
		Subscribe(1655598572, RefreshDelegate);
		Subscribe(191901966, RefreshDelegate);
		Subscribe(-71801987, RefreshDelegate);
		Subscribe(-1277991738, OnLaunchDelegate);
		Subscribe(-1432940121, OnReachableChangedDelegate);
		ReachabilityMonitor.Instance instance = new ReachabilityMonitor.Instance(GetComponent<Workable>());
		instance.StartSM();
	}

	protected override void OnCleanUp()
	{
		Game.Instance.Unsubscribe(-1123234494, OnAssignmentGroupChanged);
		base.OnCleanUp();
	}

	private void OnAssignmentGroupChanged(object data)
	{
		RefreshOrders();
	}

	private void RefreshClusterStateForAudio()
	{
		if (!(ClusterManager.Instance != null))
		{
			return;
		}
		WorldContainer activeWorld = ClusterManager.Instance.activeWorld;
		if (activeWorld != null && activeWorld.IsModuleInterior)
		{
			CraftModuleInterface craftInterface = GetComponent<RocketModuleCluster>().CraftInterface;
			Clustercraft component = activeWorld.GetComponent<Clustercraft>();
			if (craftInterface == component.ModuleInterface)
			{
				ClusterManager.Instance.UpdateRocketInteriorAudio();
			}
		}
	}

	private void OnReachableChanged(object data)
	{
		bool value = ((Boxed<bool>)data).value;
		KSelectable component = GetComponent<KSelectable>();
		if (value)
		{
			component.RemoveStatusItem(Db.Get().BuildingStatusItems.PassengerModuleUnreachable);
		}
		else
		{
			component.AddStatusItem(Db.Get().BuildingStatusItems.PassengerModuleUnreachable, this);
		}
	}

	public void RequestCrewBoard(RequestCrewState requestBoard)
	{
		passengersRequested = requestBoard;
		RefreshOrders();
	}

	public bool ShouldCrewGetIn()
	{
		CraftModuleInterface craftInterface = GetComponent<RocketModuleCluster>().CraftInterface;
		return passengersRequested == RequestCrewState.Request || (craftInterface.IsLaunchRequested() && craftInterface.CheckPreppedForLaunch());
	}

	private void RefreshOrders()
	{
		if (!this.HasTag(GameTags.RocketOnGround) || !GetComponent<ClustercraftExteriorDoor>().HasTargetWorld())
		{
			return;
		}
		int cell = GetComponent<NavTeleporter>().GetCell();
		int num = GetComponent<ClustercraftExteriorDoor>().TargetCell();
		bool flag = ShouldCrewGetIn();
		if (flag)
		{
			foreach (MinionIdentity item in Components.LiveMinionIdentities.Items)
			{
				bool flag2 = Game.Instance.assignmentManager.assignment_groups[GetComponent<AssignmentGroupController>().AssignmentGroupID].HasMember(item.assignableProxy.Get());
				bool flag3 = item.GetMyWorldId() == Grid.WorldIdx[num];
				RocketPassengerMonitor.Instance sMI = item.GetSMI<RocketPassengerMonitor.Instance>();
				if (sMI != null)
				{
					if (!flag3 && flag2)
					{
						sMI.SetMoveTarget(num);
					}
					else if (flag3 && !flag2)
					{
						sMI.SetMoveTarget(cell);
					}
					else
					{
						sMI.ClearMoveTarget(num);
					}
				}
			}
		}
		else
		{
			foreach (MinionIdentity item2 in Components.LiveMinionIdentities.Items)
			{
				RocketPassengerMonitor.Instance sMI2 = item2.GetSMI<RocketPassengerMonitor.Instance>();
				if (sMI2 != null)
				{
					sMI2.ClearMoveTarget(cell);
					sMI2.ClearMoveTarget(num);
				}
			}
		}
		for (int i = 0; i < Components.LiveMinionIdentities.Count; i++)
		{
			RefreshAccessStatus(Components.LiveMinionIdentities[i], flag);
		}
	}

	private void RefreshAccessStatus(MinionIdentity minion, bool restrict)
	{
		ClustercraftInteriorDoor interiorDoor = GetComponent<ClustercraftExteriorDoor>().GetInteriorDoor();
		AccessControl component = GetComponent<AccessControl>();
		AccessControl component2 = interiorDoor.GetComponent<AccessControl>();
		if (restrict)
		{
			if (Game.Instance.assignmentManager.assignment_groups[GetComponent<AssignmentGroupController>().AssignmentGroupID].HasMember(minion.assignableProxy.Get()))
			{
				component.SetPermission(minion.assignableProxy.Get(), AccessControl.Permission.Both);
				component2.SetPermission(minion.assignableProxy.Get(), AccessControl.Permission.Neither);
			}
			else
			{
				component.SetPermission(minion.assignableProxy.Get(), AccessControl.Permission.Neither);
				component2.SetPermission(minion.assignableProxy.Get(), AccessControl.Permission.Both);
			}
		}
		else
		{
			component.SetPermission(minion.assignableProxy.Get(), AccessControl.Permission.Both);
			component2.SetPermission(minion.assignableProxy.Get(), AccessControl.Permission.Both);
		}
	}

	public bool CheckPilotBoarded()
	{
		GameObject dupePilot = GetDupePilot();
		return dupePilot != null && dupePilot.GetMyWorldId() == Grid.WorldIdx[GetComponent<ClustercraftExteriorDoor>().TargetCell()];
	}

	public GameObject GetDupePilot()
	{
		ICollection<IAssignableIdentity> members = GetComponent<AssignmentGroupController>().GetMembers();
		if (members.Count == 0)
		{
			return null;
		}
		List<IAssignableIdentity> list = new List<IAssignableIdentity>();
		foreach (IAssignableIdentity item in members)
		{
			MinionAssignablesProxy minionAssignablesProxy = (MinionAssignablesProxy)item;
			if (minionAssignablesProxy != null)
			{
				MinionResume component = minionAssignablesProxy.GetTargetGameObject().GetComponent<MinionResume>();
				if (component != null && component.HasPerk(Db.Get().SkillPerks.CanUseRocketControlStation))
				{
					list.Add(item);
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		GameObject gameObject = null;
		foreach (IAssignableIdentity item2 in list)
		{
			gameObject = ((MinionAssignablesProxy)item2).GetTargetGameObject();
			if (gameObject.GetMyWorldId() == Grid.WorldIdx[GetComponent<ClustercraftExteriorDoor>().TargetCell()])
			{
				return gameObject;
			}
		}
		return ((MinionAssignablesProxy)list[0]).GetTargetGameObject();
	}

	public Tuple<int, int> GetCrewBoardedFraction()
	{
		ICollection<IAssignableIdentity> members = GetComponent<AssignmentGroupController>().GetMembers();
		if (members.Count == 0)
		{
			return new Tuple<int, int>(0, 0);
		}
		int num = 0;
		foreach (IAssignableIdentity item in members)
		{
			if (((MinionAssignablesProxy)item).GetTargetGameObject().GetMyWorldId() != Grid.WorldIdx[GetComponent<ClustercraftExteriorDoor>().TargetCell()])
			{
				num++;
			}
		}
		return new Tuple<int, int>(members.Count - num, members.Count);
	}

	public bool HasCrewAssigned()
	{
		ICollection<IAssignableIdentity> members = GetComponent<AssignmentGroupController>().GetMembers();
		if (members.Count > 0)
		{
			return true;
		}
		return false;
	}

	public int GetCrewCount()
	{
		ICollection<IAssignableIdentity> members = GetComponent<AssignmentGroupController>().GetMembers();
		return members.Count;
	}

	public bool CheckPassengersBoarded(bool require_pilot = true)
	{
		ICollection<IAssignableIdentity> members = GetComponent<AssignmentGroupController>().GetMembers();
		if (members.Count == 0)
		{
			return false;
		}
		if (require_pilot)
		{
			bool flag = false;
			foreach (IAssignableIdentity item in members)
			{
				MinionAssignablesProxy minionAssignablesProxy = (MinionAssignablesProxy)item;
				if (minionAssignablesProxy != null)
				{
					MinionResume component = minionAssignablesProxy.GetTargetGameObject().GetComponent<MinionResume>();
					if (component != null && component.HasPerk(Db.Get().SkillPerks.CanUseRocketControlStation))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		foreach (IAssignableIdentity item2 in members)
		{
			if (((MinionAssignablesProxy)item2).GetTargetGameObject().GetMyWorldId() != Grid.WorldIdx[GetComponent<ClustercraftExteriorDoor>().TargetCell()])
			{
				return false;
			}
		}
		return true;
	}

	public bool CheckExtraPassengers()
	{
		ClustercraftExteriorDoor component = GetComponent<ClustercraftExteriorDoor>();
		if (component.HasTargetWorld())
		{
			byte worldId = Grid.WorldIdx[component.TargetCell()];
			List<MinionIdentity> worldItems = Components.LiveMinionIdentities.GetWorldItems(worldId);
			string assignmentGroupID = GetComponent<AssignmentGroupController>().AssignmentGroupID;
			for (int i = 0; i < worldItems.Count; i++)
			{
				if (!Game.Instance.assignmentManager.assignment_groups[assignmentGroupID].HasMember(worldItems[i].assignableProxy.Get()))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveRocketPassenger(MinionIdentity minion)
	{
		if (minion != null)
		{
			string assignmentGroupID = GetComponent<AssignmentGroupController>().AssignmentGroupID;
			MinionAssignablesProxy member = minion.assignableProxy.Get();
			if (Game.Instance.assignmentManager.assignment_groups[assignmentGroupID].HasMember(member))
			{
				Game.Instance.assignmentManager.assignment_groups[assignmentGroupID].RemoveMember(member);
			}
			RefreshOrders();
		}
	}

	public void RemovePassengersOnOtherWorlds()
	{
		ClustercraftExteriorDoor component = GetComponent<ClustercraftExteriorDoor>();
		if (!component.HasTargetWorld())
		{
			return;
		}
		int myWorldId = component.GetMyWorldId();
		string assignmentGroupID = GetComponent<AssignmentGroupController>().AssignmentGroupID;
		foreach (MinionIdentity item in Components.LiveMinionIdentities.Items)
		{
			MinionAssignablesProxy member = item.assignableProxy.Get();
			if (Game.Instance.assignmentManager.assignment_groups[assignmentGroupID].HasMember(member) && item.GetMyParentWorldId() != myWorldId)
			{
				Game.Instance.assignmentManager.assignment_groups[assignmentGroupID].RemoveMember(member);
			}
		}
	}

	public void ClearMinionAssignments(object data)
	{
		string assignmentGroupID = GetComponent<AssignmentGroupController>().AssignmentGroupID;
		foreach (IAssignableIdentity member in Game.Instance.assignmentManager.assignment_groups[assignmentGroupID].GetMembers())
		{
			Game.Instance.assignmentManager.RemoveFromWorld(member, this.GetMyWorldId());
		}
	}
}
