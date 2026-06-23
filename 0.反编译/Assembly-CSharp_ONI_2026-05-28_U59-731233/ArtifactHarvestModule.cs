public class ArtifactHarvestModule : GameStateMachine<ArtifactHarvestModule, ArtifactHarvestModule.StatesInstance, IStateMachineTarget, ArtifactHarvestModule.Def>
{
	public class Def : BaseDef
	{
	}

	public class NotGroundedStates : State
	{
		public State not_harvesting;

		public State harvesting;
	}

	public class StatesInstance : GameInstance
	{
		[MyCmpReq]
		private Storage storage;

		[MyCmpReq]
		private SingleEntityReceptacle receptacle;

		public StatesInstance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
		}

		public void HarvestFromHexCell(float dt)
		{
			Clustercraft component = GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<Clustercraft>();
			StarmapHexCellInventory starmapHexCellInventory = ClusterGrid.Instance.AddOrGetHexCellInventory(component.Location);
			StarmapHexCellInventory.SerializedItem serializedItem = starmapHexCellInventory.Items.Find((StarmapHexCellInventory.SerializedItem item) => item.IsEntity && Assets.GetPrefab(item.ID).HasTag(GameTags.Artifact));
			if (serializedItem != null)
			{
				PrimaryElement primaryElement = starmapHexCellInventory.ExtractAndSpawnItem(serializedItem.ID);
				receptacle.ForceDeposit(primaryElement.gameObject);
				storage.Store(primaryElement.gameObject);
			}
		}

		public bool CheckIfCanHarvest()
		{
			Clustercraft component = GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<Clustercraft>();
			if (component == null)
			{
				return false;
			}
			if (receptacle.Occupant != null)
			{
				base.sm.canHarvest.Set(value: false, this);
				return false;
			}
			ClusterGridEntity pOIAtCurrentLocation = component.GetPOIAtCurrentLocation();
			StarmapHexCellInventory starmapHexCellInventory = ClusterGrid.Instance.AddOrGetHexCellInventory(component.Location);
			StarmapHexCellInventory.SerializedItem serializedItem = starmapHexCellInventory.Items.Find((StarmapHexCellInventory.SerializedItem item) => item.IsEntity && Assets.GetPrefab(item.ID).HasTag(GameTags.Artifact));
			if (serializedItem != null)
			{
				base.sm.canHarvest.Set(value: true, this);
				return true;
			}
			if (pOIAtCurrentLocation != null && ((bool)pOIAtCurrentLocation.GetComponent<ArtifactPOIClusterGridEntity>() || (bool)pOIAtCurrentLocation.GetComponent<HarvestablePOIClusterGridEntity>()))
			{
				ArtifactPOIStates.Instance sMI = pOIAtCurrentLocation.GetSMI<ArtifactPOIStates.Instance>();
				if (sMI != null && sMI.HasArtifactAvailableInHexCell())
				{
					base.sm.canHarvest.Set(value: true, this);
					return true;
				}
			}
			base.sm.canHarvest.Set(value: false, this);
			return false;
		}
	}

	public BoolParameter canHarvest;

	public TargetParameter entityTarget;

	public State grounded;

	public NotGroundedStates not_grounded;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = grounded;
		root.Enter(delegate(StatesInstance smi)
		{
			smi.CheckIfCanHarvest();
		});
		grounded.TagTransition(GameTags.RocketNotOnGround, not_grounded);
		not_grounded.DefaultState(not_grounded.not_harvesting).EventHandler(GameHashes.ClusterLocationChanged, (StatesInstance smi) => Game.Instance, OnAnythingChangingLocationsInSpace).EventHandler(GameHashes.OnStorageChange, delegate(StatesInstance smi)
		{
			smi.CheckIfCanHarvest();
		})
			.TagTransition(GameTags.RocketNotOnGround, grounded, on_remove: true);
		not_grounded.not_harvesting.PlayAnim("loaded").ParamTransition(canHarvest, not_grounded.harvesting, GameStateMachine<ArtifactHarvestModule, StatesInstance, IStateMachineTarget, Def>.IsTrue);
		not_grounded.harvesting.PlayAnim("deploying").Update(delegate(StatesInstance smi, float dt)
		{
			smi.HarvestFromHexCell(dt);
		}, UpdateRate.SIM_4000ms).ParamTransition(canHarvest, not_grounded.not_harvesting, GameStateMachine<ArtifactHarvestModule, StatesInstance, IStateMachineTarget, Def>.IsFalse);
	}

	private static void OnAnythingChangingLocationsInSpace(StatesInstance smi, object obj)
	{
		if (obj != null)
		{
			ClusterLocationChangedEvent clusterLocationChangedEvent = (ClusterLocationChangedEvent)obj;
			Clustercraft component = smi.GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<Clustercraft>();
			if (clusterLocationChangedEvent.entity == component)
			{
				smi.CheckIfCanHarvest();
			}
		}
	}
}
