public class DecorPlantMonitor : GameStateMachine<DecorPlantMonitor, DecorPlantMonitor.Instance, IStateMachineTarget, DecorPlantMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public class DomesticStates : State
	{
		public State healthy;

		public State wilted;
	}

	public new class Instance : GameInstance
	{
		private WiltCondition wiltCondition;

		private ReceptacleMonitor.StatesInstance _receptacleMonitor;

		public bool IsWilted => wiltCondition.IsWilting();

		public ReceptacleMonitor.StatesInstance receptacleMonitor
		{
			get
			{
				if (_receptacleMonitor == null)
				{
					_receptacleMonitor = base.gameObject.GetSMI<ReceptacleMonitor.StatesInstance>();
				}
				return _receptacleMonitor;
			}
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			wiltCondition = GetComponent<WiltCondition>();
		}
	}

	public State wildPlanted;

	public DomesticStates domestic;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = wildPlanted;
		wildPlanted.EventTransition(GameHashes.ReceptacleMonitorChange, domestic, IsDomestic);
		domestic.EventTransition(GameHashes.ReceptacleMonitorChange, wildPlanted, GameStateMachine<DecorPlantMonitor, Instance, IStateMachineTarget, Def>.Not(IsDomestic)).DefaultState(domestic.wilted);
		domestic.wilted.EventTransition(GameHashes.WiltRecover, domestic.healthy, GameStateMachine<DecorPlantMonitor, Instance, IStateMachineTarget, Def>.Not(IsWilted)).Enter(TriggerRoomRefresh);
		domestic.healthy.EventTransition(GameHashes.Wilt, domestic.wilted, IsWilted).ToggleTag(GameTags.Decoration).Enter(TriggerRoomRefresh);
	}

	public static bool IsDomestic(Instance smi)
	{
		return smi.receptacleMonitor != null && smi.receptacleMonitor.ReceptacleObject != null;
	}

	public static bool IsWilted(Instance smi)
	{
		return smi.IsWilted;
	}

	public static void TriggerRoomRefresh(Instance smi)
	{
		int cell = Grid.PosToCell(smi);
		Game.Instance.roomProber.TriggerBuildingChangedEvent(cell, smi.gameObject);
	}
}
