using UnityEngine;

public class JettisonableCargoModule : GameStateMachine<JettisonableCargoModule, JettisonableCargoModule.StatesInstance, IStateMachineTarget, JettisonableCargoModule.Def>
{
	public class Def : BaseDef
	{
		public DefComponent<Storage> landerContainer;

		public Tag landerPrefabID;

		public Vector3 cargoDropOffset;

		public string clusterMapFXPrefabID;
	}

	public class GroundedStates : State
	{
		public State loaded;

		public State empty;
	}

	public class NotGroundedStates : State
	{
		public State loaded;

		public State emptying;

		public State empty;
	}

	public class StatesInstance : GameInstance, IEmptyableCargo
	{
		private Storage landerContainer;

		private bool landerPlaced;

		private MinionIdentity chosenDuplicant;

		private int landerPlacementCell;

		public GameObject clusterMapFX;

		public bool AutoDeploy { get; set; }

		public bool CanAutoDeploy => false;

		public bool ChooseDuplicant
		{
			get
			{
				GameObject gameObject = landerContainer.FindFirst(base.def.landerPrefabID);
				if (gameObject == null)
				{
					return false;
				}
				MinionStorage component = gameObject.GetComponent<MinionStorage>();
				return component != null;
			}
		}

		public bool ModuleDeployed => landerPlaced;

		public MinionIdentity ChosenDuplicant
		{
			get
			{
				return chosenDuplicant;
			}
			set
			{
				chosenDuplicant = value;
			}
		}

		public StatesInstance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			landerContainer = def.landerContainer.Get(this);
		}

		private void ChooseLanderLocation()
		{
			RocketModuleCluster component = base.master.GetComponent<RocketModuleCluster>();
			Clustercraft component2 = component.CraftInterface.GetComponent<Clustercraft>();
			ClusterGridEntity stableOrbitAsteroid = component2.GetStableOrbitAsteroid();
			if (stableOrbitAsteroid != null)
			{
				WorldContainer component3 = stableOrbitAsteroid.GetComponent<WorldContainer>();
				GameObject gameObject = landerContainer.FindFirst(base.def.landerPrefabID);
				Placeable component4 = gameObject.GetComponent<Placeable>();
				component4.restrictWorldId = component3.id;
				component3.LookAtSurface();
				ClusterManager.Instance.SetActiveWorld(component3.id);
				ManagementMenu.Instance.CloseAll();
				PlaceTool.Instance.Activate(component4, OnLanderPlaced);
			}
		}

		private void OnLanderPlaced(Placeable lander, int cell)
		{
			landerPlaced = true;
			landerPlacementCell = cell;
			if (lander.GetComponent<MinionStorage>() != null)
			{
				OpenMoveChoreForChosenDuplicant();
			}
			ManagementMenu.Instance.ToggleClusterMap();
			base.sm.emptyCargo.Trigger(base.smi);
			ClusterMapScreen.Instance.SelectEntity(GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<ClusterGridEntity>(), frameDelay: true);
		}

		private void OpenMoveChoreForChosenDuplicant()
		{
			RocketPassengerMonitor.Instance sMI = ChosenDuplicant.GetSMI<RocketPassengerMonitor.Instance>();
			if (sMI != null)
			{
				RocketModuleCluster component = base.master.GetComponent<RocketModuleCluster>();
				Clustercraft craft = component.CraftInterface.GetComponent<Clustercraft>();
				MinionStorage storage = landerContainer.FindFirst(base.def.landerPrefabID).GetComponent<MinionStorage>();
				EnableTeleport(enable: true);
				sMI.SetModuleDeployChore(landerPlacementCell, delegate
				{
					Game.Instance.assignmentManager.RemoveFromWorld(ChosenDuplicant.assignableProxy.Get(), craft.ModuleInterface.GetInteriorWorld().id);
					storage.SerializeMinion(ChosenDuplicant.gameObject);
					EnableTeleport(enable: false);
				});
			}
		}

		private void EnableTeleport(bool enable)
		{
			RocketModuleCluster component = base.master.GetComponent<RocketModuleCluster>();
			Clustercraft component2 = component.CraftInterface.GetComponent<Clustercraft>();
			ClustercraftExteriorDoor component3 = component2.ModuleInterface.GetPassengerModule().GetComponent<ClustercraftExteriorDoor>();
			ClustercraftInteriorDoor interiorDoor = component3.GetInteriorDoor();
			AccessControl component4 = component3.GetInteriorDoor().GetComponent<AccessControl>();
			NavTeleporter component5 = GetComponent<NavTeleporter>();
			if (enable)
			{
				component5.SetOverrideCell(landerPlacementCell);
				interiorDoor.GetComponent<NavTeleporter>().SetTarget(component5);
				component5.SetTarget(interiorDoor.GetComponent<NavTeleporter>());
				{
					foreach (MinionIdentity worldItem in Components.MinionIdentities.GetWorldItems(interiorDoor.GetMyWorldId()))
					{
						component4.SetPermission(worldItem.assignableProxy.Get(), (!(worldItem == ChosenDuplicant)) ? AccessControl.Permission.Neither : AccessControl.Permission.Both);
					}
					return;
				}
			}
			component5.SetOverrideCell(-1);
			interiorDoor.GetComponent<NavTeleporter>().SetTarget(null);
			component5.SetTarget(null);
			component4.SetPermission(ChosenDuplicant.assignableProxy.Get(), AccessControl.Permission.Neither);
		}

		public void FinalDeploy()
		{
			landerPlaced = false;
			Placeable component = landerContainer.FindFirst(base.def.landerPrefabID).GetComponent<Placeable>();
			GameObject gameObject = landerContainer.FindFirst(base.def.landerPrefabID);
			landerContainer.Drop(component.gameObject);
			TreeFilterable component2 = GetComponent<TreeFilterable>();
			TreeFilterable component3 = component.GetComponent<TreeFilterable>();
			if (component3 != null)
			{
				component3.UpdateFilters(component2.AcceptedTags);
			}
			Storage component4 = component.GetComponent<Storage>();
			if (component4 != null)
			{
				Storage[] components = base.gameObject.GetComponents<Storage>();
				foreach (Storage storage in components)
				{
					storage.Transfer(component4, block_events: false, hide_popups: true);
				}
			}
			Vector3 position = Grid.CellToPosCBC(landerPlacementCell, Grid.SceneLayer.Building);
			component.transform.SetPosition(position);
			component.gameObject.SetActive(value: true);
			RocketModuleCluster component5 = base.master.GetComponent<RocketModuleCluster>();
			Clustercraft component6 = component5.CraftInterface.GetComponent<Clustercraft>();
			component6.gameObject.Trigger(1792516731, (object)component);
			component.Trigger(1792516731, (object)base.gameObject);
			GameObject gameObject2 = Assets.TryGetPrefab(base.smi.def.clusterMapFXPrefabID);
			if (!(gameObject2 != null))
			{
				return;
			}
			clusterMapFX = GameUtil.KInstantiate(gameObject2, Grid.SceneLayer.Background);
			clusterMapFX.SetActive(value: true);
			ClusterFXEntity component7 = clusterMapFX.GetComponent<ClusterFXEntity>();
			component7.Init(component.GetMyWorldLocation(), Vector3.zero);
			component.Subscribe(1969584890, delegate
			{
				if (!clusterMapFX.IsNullOrDestroyed())
				{
					Util.KDestroyGameObject(clusterMapFX);
				}
			});
			component.Subscribe(1591811118, delegate
			{
				if (!clusterMapFX.IsNullOrDestroyed())
				{
					Util.KDestroyGameObject(clusterMapFX);
				}
			});
		}

		public bool CheckReadyForFinalDeploy()
		{
			GameObject gameObject = landerContainer.FindFirst(base.def.landerPrefabID);
			MinionStorage component = gameObject.GetComponent<MinionStorage>();
			if (component != null)
			{
				return component.GetStoredMinionInfo().Count > 0;
			}
			return true;
		}

		public void CancelPendingDeploy()
		{
			landerPlaced = false;
			if (ChosenDuplicant != null && CheckIfLoaded())
			{
				ChosenDuplicant.GetSMI<RocketPassengerMonitor.Instance>().CancelModuleDeployChore();
			}
		}

		public bool CheckIfLoaded()
		{
			bool flag = false;
			foreach (GameObject item in landerContainer.items)
			{
				if (item.PrefabID() == base.def.landerPrefabID)
				{
					flag = true;
					break;
				}
			}
			if (flag != base.sm.hasCargo.Get(this))
			{
				base.sm.hasCargo.Set(flag, this);
			}
			return flag;
		}

		public bool IsValidDropLocation()
		{
			Clustercraft component = GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<Clustercraft>();
			return component.GetStableOrbitAsteroid() != null;
		}

		public void EmptyCargo()
		{
			ChooseLanderLocation();
		}

		public bool CanEmptyCargo()
		{
			return base.sm.hasCargo.Get(base.smi) && IsValidDropLocation() && (!ChooseDuplicant || (ChosenDuplicant != null && !ChosenDuplicant.HasTag(GameTags.Dead))) && !landerPlaced;
		}
	}

	public BoolParameter hasCargo;

	public Signal emptyCargo;

	public GroundedStates grounded;

	public NotGroundedStates not_grounded;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = grounded;
		root.Enter(delegate(StatesInstance smi)
		{
			smi.CheckIfLoaded();
		}).EventHandler(GameHashes.OnStorageChange, delegate(StatesInstance smi)
		{
			smi.CheckIfLoaded();
		});
		grounded.DefaultState(grounded.loaded).TagTransition(GameTags.RocketNotOnGround, not_grounded);
		grounded.loaded.PlayAnim("loaded").ParamTransition(hasCargo, grounded.empty, GameStateMachine<JettisonableCargoModule, StatesInstance, IStateMachineTarget, Def>.IsFalse);
		grounded.empty.PlayAnim("deployed").ParamTransition(hasCargo, grounded.loaded, GameStateMachine<JettisonableCargoModule, StatesInstance, IStateMachineTarget, Def>.IsTrue);
		not_grounded.DefaultState(not_grounded.loaded).TagTransition(GameTags.RocketNotOnGround, grounded, on_remove: true);
		not_grounded.loaded.PlayAnim("loaded").ParamTransition(hasCargo, not_grounded.empty, GameStateMachine<JettisonableCargoModule, StatesInstance, IStateMachineTarget, Def>.IsFalse).OnSignal(emptyCargo, not_grounded.emptying);
		not_grounded.emptying.PlayAnim("deploying").Update(delegate(StatesInstance smi, float dt)
		{
			if (smi.CheckReadyForFinalDeploy())
			{
				smi.FinalDeploy();
				smi.GoTo(smi.sm.not_grounded.empty);
			}
		}).EventTransition(GameHashes.ClusterLocationChanged, (StatesInstance smi) => Game.Instance, not_grounded)
			.Exit(delegate(StatesInstance smi)
			{
				smi.CancelPendingDeploy();
			});
		not_grounded.empty.PlayAnim("deployed").ParamTransition(hasCargo, not_grounded.loaded, GameStateMachine<JettisonableCargoModule, StatesInstance, IStateMachineTarget, Def>.IsTrue);
	}
}
