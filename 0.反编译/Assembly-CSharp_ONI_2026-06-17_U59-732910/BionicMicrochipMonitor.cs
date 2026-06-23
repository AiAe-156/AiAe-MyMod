using UnityEngine;

public class BionicMicrochipMonitor : GameStateMachine<BionicMicrochipMonitor, BionicMicrochipMonitor.Instance, IStateMachineTarget, BionicMicrochipMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public class ProductionStates : State
	{
		public State charging;

		public State produceOne;
	}

	public new class Instance : GameInstance
	{
		public ProgressBar progressBar;

		public float Progress => base.sm.Progress.Get(this);

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
		}

		public void CreateMicrochip()
		{
			Util.KInstantiate(Assets.GetPrefab(PowerStationToolsConfig.tag), Grid.CellToPos(Grid.PosToCell(base.smi.gameObject), CellAlignment.Top, Grid.SceneLayer.Ore)).SetActive(value: true);
		}

		public void CreateProgressBar()
		{
			progressBar = ProgressBar.CreateProgressBar(base.gameObject, () => Progress);
			base.smi.progressBar.SetVisibility(visible: true);
			base.smi.progressBar.barColor = Color.green;
		}

		public void ClearProgressBar()
		{
			if (progressBar != null)
			{
				Util.KDestroyGameObject(base.smi.progressBar.gameObject);
				progressBar = null;
			}
		}
	}

	public const float MICROCHIP_PRODUCTION_TIME = 150f;

	public State idle;

	public ProductionStates production;

	public FloatParameter Progress;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = idle;
		idle.TagTransition(GameTags.BionicBedTime, production);
		production.TagTransition(GameTags.BionicBedTime, idle, on_remove: true).Enter(CreateProgresesBar).Exit(ClearProgressBar)
			.ToggleStatusItem(Db.Get().DuplicantStatusItems.BionicMicrochipGeneration)
			.DefaultState(production.charging);
		production.charging.ParamTransition(Progress, production.produceOne, GameStateMachine<BionicMicrochipMonitor, Instance, IStateMachineTarget, Def>.IsGTEOne).Update(ProgressUpdate);
		production.produceOne.Enter(CreateMicrochip).Enter(ResetProgress).GoTo(production.charging);
	}

	public static void ClearProgressBar(Instance smi)
	{
		smi.ClearProgressBar();
	}

	public static void CreateProgresesBar(Instance smi)
	{
		smi.CreateProgressBar();
	}

	public static void ResetProgress(Instance smi)
	{
		smi.sm.Progress.Set(0f, smi);
	}

	public static void CreateMicrochip(Instance smi)
	{
		smi.CreateMicrochip();
	}

	public static void ProgressUpdate(Instance smi, float dt)
	{
		float num = dt / 150f;
		float progress = smi.Progress;
		smi.sm.Progress.Set(progress + num, smi);
	}
}
