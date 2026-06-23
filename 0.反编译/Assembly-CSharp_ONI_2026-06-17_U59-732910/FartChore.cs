using System;
using UnityEngine;

public class FartChore : Chore<FartChore.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, FartChore, object>.GameInstance
	{
		public StatesInstance(FartChore master, GameObject farter)
			: base(master)
		{
			base.sm.farter.Set(farter, base.smi);
		}
	}

	public class States : GameStateMachine<States, StatesInstance, FartChore>
	{
		public TargetParameter farter;

		public State finish;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = root;
			Target(farter);
			root.PlayAnim("fart").ScheduleGoTo(10f, finish).OnAnimQueueComplete(finish);
			finish.Enter(CreateEmission).ReturnSuccess();
		}
	}

	private float mass;

	private SimHashes element_id;

	private byte disease_idx;

	private int disease_count;

	private float overpressureThreshold;

	public FartChore(IStateMachineTarget target, ChoreType chore_type, float mass, SimHashes element_id, byte disease_idx, int disease_count, float overpressureThreshold)
		: base(chore_type, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.compulsory, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new StatesInstance(this, target.gameObject);
		this.mass = mass;
		this.element_id = element_id;
		this.disease_idx = disease_idx;
		this.disease_count = disease_count;
		this.overpressureThreshold = overpressureThreshold;
	}

	private bool CheckIsOverpressure(int cell)
	{
		return Grid.Mass[cell] > overpressureThreshold;
	}

	public static void CreateEmission(StatesInstance smi)
	{
		smi.master.DoFart();
	}

	public void DoFart()
	{
		if (mass <= 0f)
		{
			return;
		}
		Element element = ElementLoader.FindElementByHash(element_id);
		float temperature = base.smi.master.GetComponent<PrimaryElement>().Temperature;
		if (element.IsGas || element.IsLiquid)
		{
			int num = Grid.PosToCell(base.transform.GetPosition());
			if (CheckIsOverpressure(num))
			{
				return;
			}
			SimMessages.AddRemoveSubstance(num, element_id, CellEventLogger.Instance.ElementConsumerSimUpdate, mass, temperature, disease_idx, disease_count);
		}
		else if (element.IsSolid)
		{
			element.substance.SpawnResource(base.transform.GetPosition() + new Vector3(0f, 0.5f, 0f), mass, temperature, disease_idx, disease_count, prevent_merge: false, forceTemperature: true);
		}
		PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Resource, element.name, gameObject.transform);
	}
}
