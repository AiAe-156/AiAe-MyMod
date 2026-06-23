using UnityEngine;

public class WideFarmTile : GameStateMachine<WideFarmTile, WideFarmTile.Instance, IStateMachineTarget, WideFarmTile.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		private MeterController liquidMeter;

		private Storage storage;

		private ConduitConsumer conduitConsumer;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			liquidMeter = new MeterController(component, "meter_target", "meter", Meter.Offset.NoChange, Grid.SceneLayer.Building);
			conduitConsumer = GetComponent<ConduitConsumer>();
			storage = GetComponent<Storage>();
		}

		public override void StartSM()
		{
			base.StartSM();
			RefreshLiquidMeter();
		}

		public void RefreshLiquidMeter()
		{
			liquidMeter.SetPositionPercent(conduitConsumer.stored_mass / conduitConsumer.capacityKG);
			GameObject gameObject = storage.FindFirst(GameTags.Liquid);
			if (!(gameObject == null))
			{
				PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
				Element element = component.Element;
				GameUtil.TintLiquidSymbolOnBuilding("meter_fill", liquidMeter.meterController, element);
			}
		}
	}

	private const string LIQUID_METER_ANIM_NAME = "meter";

	private const string LIQUID_METER_TARGET_NAME = "meter_target";

	private const string LIQUID_METER_TINT_SYMBOL_NAME = "meter_fill";

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = root;
		root.EventHandler(GameHashes.OnStorageChange, RefreshLiquidMeter);
	}

	private static void RefreshLiquidMeter(Instance smi)
	{
		smi.RefreshLiquidMeter();
	}
}
