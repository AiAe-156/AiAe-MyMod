using System.Collections.Generic;
using STRINGS;
using UnityEngine;

[SkipSaveFileSerialization]
public class ReceptacleMonitor : StateMachineComponent<ReceptacleMonitor.StatesInstance>, IGameObjectEffectDescriptor, IWiltCause
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, ReceptacleMonitor, object>.GameInstance
	{
		public SingleEntityReceptacle ReceptacleObject => base.sm.receptacle.Get(this);

		public StatesInstance(ReceptacleMonitor master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, ReceptacleMonitor>
	{
		public class DomesticState : State
		{
			public State simple;

			public OperationalState operationalExist;
		}

		public class OperationalState : State
		{
			public State inoperational;

			public State operational;
		}

		public ObjectParameter<SingleEntityReceptacle> receptacle;

		public State wild;

		public DomesticState domestic;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = wild;
			base.serializable = SerializeType.Never;
			wild.ParamTransition(receptacle, domestic, (StatesInstance smi, SingleEntityReceptacle p) => p != null);
			domestic.ParamTransition(receptacle, wild, (StatesInstance smi, SingleEntityReceptacle p) => p == null).EnterTransition(domestic.operationalExist, HasReceptacleOperationalComponent).EnterTransition(domestic.simple, GameStateMachine<States, StatesInstance, ReceptacleMonitor, object>.Not(HasReceptacleOperationalComponent));
			domestic.simple.DoNothing();
			domestic.operationalExist.EnterTransition(domestic.operationalExist.operational, IsReceptacleOperational).EnterGoTo(domestic.operationalExist.inoperational);
			domestic.operationalExist.inoperational.EventHandlerTransition(GameHashes.ReceptacleOperational, domestic.operationalExist.operational, IsReceptacleOperational);
			domestic.operationalExist.operational.EventHandlerTransition(GameHashes.ReceptacleInoperational, domestic.operationalExist.inoperational, IsReceptacle_NOT_Operational);
		}
	}

	private bool replanted;

	public bool Replanted => replanted;

	WiltCondition.Condition[] IWiltCause.Conditions => new WiltCondition.Condition[1] { WiltCondition.Condition.Receptacle };

	public string WiltStateString
	{
		get
		{
			string text = "";
			if (base.smi.IsInsideState(base.smi.sm.domestic.operationalExist.inoperational))
			{
				text += CREATURES.STATUSITEMS.RECEPTACLEINOPERATIONAL.NAME;
			}
			return text;
		}
	}

	private static bool HasReceptacleOperationalComponent(StatesInstance smi)
	{
		if (smi.ReceptacleObject != null)
		{
			return smi.ReceptacleObject.GetComponent<Operational>() != null;
		}
		return false;
	}

	private static bool IsReceptacleOperational(StatesInstance smi)
	{
		if (HasReceptacleOperationalComponent(smi))
		{
			return smi.ReceptacleObject.GetComponent<Operational>().IsOperational;
		}
		return false;
	}

	private static bool IsReceptacleOperational(StatesInstance smi, object obj)
	{
		return IsReceptacleOperational(smi);
	}

	private static bool IsReceptacle_NOT_Operational(StatesInstance smi, object obj)
	{
		return !IsReceptacleOperational(smi);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}

	public PlantablePlot GetReceptacle()
	{
		return (PlantablePlot)base.smi.sm.receptacle.Get(base.smi);
	}

	public void SetReceptacle(PlantablePlot plot = null)
	{
		if (plot == null)
		{
			base.smi.sm.receptacle.Set(null, base.smi);
			replanted = false;
		}
		else
		{
			base.smi.sm.receptacle.Set(plot, base.smi);
			replanted = true;
		}
		Trigger(-1636776682);
	}

	public bool HasReceptacle()
	{
		return !base.smi.IsInsideState(base.smi.sm.wild);
	}

	public bool HasOperationalReceptacle()
	{
		return base.smi.IsInsideState(base.smi.sm.domestic.operationalExist.operational);
	}

	public List<Descriptor> GetDescriptors(GameObject go)
	{
		return new List<Descriptor>
		{
			new Descriptor(UI.GAMEOBJECTEFFECTS.REQUIRES_RECEPTACLE, UI.GAMEOBJECTEFFECTS.TOOLTIPS.REQUIRES_RECEPTACLE, Descriptor.DescriptorType.Requirement)
		};
	}
}
