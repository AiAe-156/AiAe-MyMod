using UnityEngine;

public class FoodSmoker : GameStateMachine<FoodSmoker, FoodSmoker.StatesInstance, IStateMachineTarget, FoodSmoker.Def>
{
	public class Def : BaseDef
	{
	}

	public class StatesInstance : GameInstance
	{
		[MyCmpAdd]
		public ManuallySetRemoteWorkTargetComponent remoteChore;

		[MyCmpReq]
		public Operational operational;

		[MyCmpReq]
		public ComplexFabricator complexFabricator;

		public StatesInstance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
		}

		public bool RequiresEmptying()
		{
			return !complexFabricator.outStorage.IsEmpty();
		}

		public void OnEmptyComplete(Chore obj)
		{
			Vector3 position = Grid.CellToPosLCC(Grid.PosToCell(this), Grid.SceneLayer.Ore);
			complexFabricator.outStorage.DropAll(position, vent_gas: false, dump_liquid: true);
		}
	}

	private static readonly Operational.Flag foodSmokerFlag = new Operational.Flag("food_smoker", Operational.Flag.Type.Requirement);

	private State working;

	private State requestEmpty;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = working;
		working.Enter(delegate(StatesInstance smi)
		{
			smi.complexFabricator.SetQueueDirty();
			smi.operational.SetFlag(foodSmokerFlag, value: true);
		}).EnterTransition(requestEmpty, (StatesInstance smi) => smi.RequiresEmptying()).EventHandlerTransition(GameHashes.FabricatorOrderCompleted, requestEmpty, (StatesInstance smi, object data) => smi.RequiresEmptying());
		requestEmpty.ToggleRecurringChore(CreateChore, SetRemoteChore, (StatesInstance smi) => smi.RequiresEmptying()).EventHandlerTransition(GameHashes.OnStorageChange, working, (StatesInstance smi, object data) => !smi.RequiresEmptying()).Enter(delegate(StatesInstance smi)
		{
			smi.operational.SetFlag(foodSmokerFlag, value: false);
		})
			.ToggleStatusItem(Db.Get().BuildingStatusItems.AwaitingEmptyBuilding);
	}

	private static void SetRemoteChore(StatesInstance smi, Chore chore)
	{
		smi.remoteChore.SetChore(chore);
	}

	private Chore CreateChore(StatesInstance smi)
	{
		WorkChore<FoodSmokerWorkableEmpty> workChore = new WorkChore<FoodSmokerWorkableEmpty>(Db.Get().ChoreTypes.Cook, smi.master.GetComponent<FoodSmokerWorkableEmpty>(), null, run_until_complete: true, smi.OnEmptyComplete, null, null, allow_in_red_alert: true, null, ignore_schedule_block: false, only_when_operational: false);
		workChore.AddPrecondition(ChorePreconditions.instance.IsNotARobot);
		workChore.AddPrecondition(ChorePreconditions.instance.HasSkillPerk, Db.Get().SkillPerks.CanGasRange.Id);
		return workChore;
	}
}
