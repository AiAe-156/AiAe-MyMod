public class PlantBranchGrowerBase<StateMachineType, StateMachineInstanceType, MasterType, DefType> : GameStateMachine<StateMachineType, StateMachineInstanceType, MasterType, DefType> where StateMachineType : GameStateMachine<StateMachineType, StateMachineInstanceType, MasterType, DefType> where StateMachineInstanceType : GameStateMachine<StateMachineType, StateMachineInstanceType, MasterType, DefType>.GameInstance where MasterType : IStateMachineTarget where DefType : PlantBranchGrowerBase<StateMachineType, StateMachineInstanceType, MasterType, DefType>.PlantBranchGrowerBaseDef
{
	public class PlantBranchGrowerBaseDef : BaseDef, IPlantBranchGrower
	{
		public int MAX_BRANCH_COUNT;

		public string BRANCH_PREFAB_NAME;

		public string GetPlantBranchPrefabName()
		{
			return BRANCH_PREFAB_NAME;
		}

		public int GetMaxBranchCount()
		{
			return MAX_BRANCH_COUNT;
		}
	}
}
