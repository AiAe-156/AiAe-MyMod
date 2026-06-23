using Klei.AI;
using STRINGS;
using TUNING;

public class LightMonitor : GameStateMachine<LightMonitor, LightMonitor.Instance>
{
	public class UnburntStates : State
	{
		public SafeStates safe;

		public State burning;
	}

	public class SafeStates : State
	{
		public State unlit;

		public State normal_light;

		public State sunlight;
	}

	public new class Instance : GameInstance
	{
		public Effects effects;

		public Instance(IStateMachineTarget master)
			: base(master)
		{
			effects = GetComponent<Effects>();
		}
	}

	public const float BURN_RESIST_RECOVERY_FACTOR = 0.25f;

	public FloatParameter lightLevel;

	public FloatParameter burnResistance = new FloatParameter(DUPLICANTSTATS.STANDARD.Light.SUNBURN_DELAY_TIME);

	public UnburntStates unburnt;

	public State get_burnt;

	public State burnt;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = unburnt;
		root.EventTransition(GameHashes.SicknessAdded, burnt, (Instance smi) => smi.gameObject.GetSicknesses().Has(Db.Get().Sicknesses.Sunburn)).Update(CheckLightLevel, UpdateRate.SIM_1000ms);
		unburnt.DefaultState(unburnt.safe).ParamTransition(burnResistance, get_burnt, GameStateMachine<LightMonitor, Instance, IStateMachineTarget, object>.IsLTEZero);
		unburnt.safe.DefaultState(unburnt.safe.unlit).Update(delegate(Instance smi, float dt)
		{
			smi.sm.burnResistance.DeltaClamp(dt * 0.25f, 0f, DUPLICANTSTATS.STANDARD.Light.SUNBURN_DELAY_TIME, smi);
		});
		unburnt.safe.unlit.ParamTransition(lightLevel, unburnt.safe.normal_light, GameStateMachine<LightMonitor, Instance, IStateMachineTarget, object>.IsGTZero);
		unburnt.safe.normal_light.ParamTransition(lightLevel, unburnt.safe.unlit, GameStateMachine<LightMonitor, Instance, IStateMachineTarget, object>.IsLTEZero).ParamTransition(lightLevel, unburnt.safe.sunlight, (Instance smi, float p) => p >= (float)DUPLICANTSTATS.STANDARD.Light.LUX_PLEASANT_LIGHT);
		unburnt.safe.sunlight.ParamTransition(lightLevel, unburnt.safe.normal_light, (Instance smi, float p) => p < (float)DUPLICANTSTATS.STANDARD.Light.LUX_PLEASANT_LIGHT).ParamTransition(lightLevel, unburnt.burning, (Instance smi, float p) => p >= (float)DUPLICANTSTATS.STANDARD.Light.LUX_SUNBURN).ToggleEffect("Sunlight_Pleasant");
		unburnt.burning.ParamTransition(lightLevel, unburnt.safe.sunlight, (Instance smi, float p) => p < (float)DUPLICANTSTATS.STANDARD.Light.LUX_SUNBURN).Update(delegate(Instance smi, float dt)
		{
			smi.sm.burnResistance.DeltaClamp(0f - dt, 0f, DUPLICANTSTATS.STANDARD.Light.SUNBURN_DELAY_TIME, smi);
		}).ToggleEffect("Sunlight_Burning");
		get_burnt.Enter(delegate(Instance smi)
		{
			smi.gameObject.GetSicknesses().Infect(new SicknessExposureInfo(Db.Get().Sicknesses.Sunburn.Id, DUPLICANTS.DISEASES.SUNBURNSICKNESS.SUNEXPOSURE));
		}).GoTo(burnt);
		burnt.EventTransition(GameHashes.SicknessCured, unburnt, (Instance smi) => !smi.gameObject.GetSicknesses().Has(Db.Get().Sicknesses.Sunburn)).Exit(delegate(Instance smi)
		{
			smi.sm.burnResistance.Set(DUPLICANTSTATS.STANDARD.Light.SUNBURN_DELAY_TIME, smi);
		});
	}

	private static void CheckLightLevel(Instance smi, float dt)
	{
		KPrefabID component = smi.GetComponent<KPrefabID>();
		if (component != null && component.HasTag(GameTags.Shaded))
		{
			smi.sm.lightLevel.Set(0f, smi);
			return;
		}
		int num = Grid.PosToCell(smi.gameObject);
		if (Grid.IsValidCell(num))
		{
			smi.sm.lightLevel.Set(Grid.LightIntensity[num], smi);
		}
	}
}
