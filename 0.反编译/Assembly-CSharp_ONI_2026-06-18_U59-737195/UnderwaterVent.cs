using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class UnderwaterVent : GameStateMachine<UnderwaterVent, UnderwaterVent.Instance, IStateMachineTarget, UnderwaterVent.Def>
{
	public struct Data
	{
		public Vector3 BubbleSpawnOffset;

		public Vector3 SolidSpawnOffset;

		public SimHashes BubbleElement;

		public float BubbleTemp;

		public float BubbleMassRate;

		public SimHashes SolidElement;

		public float SolidMass;

		public float SolidTemp;

		public float BuildUpDuration;

		public Data(Vector3 bubbleSpawnOffset, Vector3 solidSpawnOffset, SimHashes bubbleElement, float bubbleTemp, float bubbleMassPerSecond, SimHashes solidElement, float solidMass, float solidTemp, float buildUpDuration)
		{
			BubbleSpawnOffset = bubbleSpawnOffset;
			SolidSpawnOffset = solidSpawnOffset;
			BubbleElement = bubbleElement;
			BubbleTemp = bubbleTemp;
			BubbleMassRate = bubbleMassPerSecond;
			SolidElement = solidElement;
			SolidMass = solidMass;
			SolidTemp = solidTemp;
			BuildUpDuration = buildUpDuration;
		}
	}

	public class Def : BaseDef, IGameObjectEffectDescriptor
	{
		public Data data;

		private List<Descriptor> cachedDescriptor;

		public List<Descriptor> GetDescriptors(GameObject go)
		{
			if (cachedDescriptor == null)
			{
				cachedDescriptor = new List<Descriptor>();
				cachedDescriptor.Add(new Descriptor(GameUtil.SafeStringFormat(UI.BUILDINGEFFECTS.UNDERWATERVENT_SHEARING, GameUtil.GetFormattedMass(data.SolidMass)), GameUtil.SafeStringFormat(UI.BUILDINGEFFECTS.TOOLTIPS.UNDERWATERVENT_SHEARING, data.SolidElement.CreateTag().ProperName())));
			}
			return cachedDescriptor;
		}
	}

	public class OnStates : State
	{
		public State erupting;

		public BlockStates blocked;
	}

	public class BlockStates : State
	{
		public State idle;

		public State unblock;
	}

	public new class Instance : GameInstance
	{
		private EntombVulnerable entombVulnerable;

		private Submergable submergable;

		private MeterController buildUpMeter;

		public float BuildUpProgress => base.sm.BuildUp.Get(this);

		public bool IsBlocked => BuildUpProgress >= 1f;

		public bool IsSubmerged => submergable.IsSubmerged;

		public bool IsEntombed => entombVulnerable.GetEntombed;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			entombVulnerable = GetComponent<EntombVulnerable>();
			submergable = GetComponent<Submergable>();
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			buildUpMeter = new MeterController(component, "target_meter", "meter", Meter.Offset.Infront, Grid.SceneLayer.BuildingBack);
			base.sm.MeterController.Set(buildUpMeter.meterController.gameObject, this);
		}

		public override void StartSM()
		{
			base.StartSM();
			RefreshBuildUpMeter();
		}

		public void EruptionUpdate(float dt)
		{
			float num = dt / base.def.data.BuildUpDuration;
			float num2 = base.def.data.BubbleMassRate * dt;
			if (num2 >= 1E-09f)
			{
				float bubbleTemp = base.def.data.BubbleTemp;
				Vector3 vector = Grid.CellToPos(Grid.PosToCell(base.gameObject)) + base.def.data.BubbleSpawnOffset;
				SimHashes bubbleElement = base.def.data.BubbleElement;
				BubbleManager.instance.SpawnBubble(bubbleElement, vector, num2, bubbleTemp, BubbleManager.Disease.None);
			}
			base.sm.BuildUp.Set(BuildUpProgress + num, this);
			RefreshBuildUpMeter();
		}

		public void RefreshBuildUpMeter()
		{
			if (buildUpMeter.meterController.currentAnim != "meter")
			{
				buildUpMeter.meterController.Play("meter", KAnim.PlayMode.Paused);
			}
			buildUpMeter.SetPositionPercent(BuildUpProgress);
		}

		public void SpawnSolidDebri()
		{
			KBatchedAnimController meterController = buildUpMeter.meterController;
			List<Vector3> list = new List<Vector3>(ROCK_SYMBOLS_NAME.Length);
			float layerZ = Grid.GetLayerZ(Grid.SceneLayer.Ore);
			for (int i = 0; i < ROCK_SYMBOLS_NAME.Length; i++)
			{
				string text = ROCK_SYMBOLS_NAME[i];
				bool symbolVisible;
				Matrix4x4 symbolTransform = meterController.GetSymbolTransform(text, out symbolVisible);
				if (symbolVisible)
				{
					Vector3 item = symbolTransform.GetColumn(3);
					item.z = layerZ;
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				Vector3 item2 = Grid.CellToPos(Grid.PosToCell(base.gameObject)) + base.def.data.SolidSpawnOffset;
				item2.z = layerZ;
				list.Add(item2);
			}
			float massPerRock = base.def.data.SolidMass / (float)list.Count;
			for (int j = 0; j < list.Count; j++)
			{
				Vector3 spawnPos = list[j];
				SpawnRockDebri(spawnPos, massPerRock);
			}
		}

		private void SpawnRockDebri(Vector3 spawnPos, float massPerRock)
		{
			GameObject obj = GameUtil.KInstantiate(Assets.GetPrefab(base.def.data.SolidElement.CreateTag()), Grid.SceneLayer.Ore);
			obj.transform.position = spawnPos;
			PrimaryElement component = obj.GetComponent<PrimaryElement>();
			component.Mass = massPerRock;
			component.Temperature = base.def.data.SolidTemp;
			obj.gameObject.SetActive(value: true);
		}

		public void Unblock()
		{
			base.sm.BuildUp.Set(0f, this);
		}
	}

	private const string IDLE_ANIM_NAME = "off";

	private const string ERUPTING_ANIM_NAME = "erupting";

	private const string BLOCKED_ANIM_NAME = "blocked";

	private const string METER_TARGET_NAME = "target_meter";

	private const string METER_ANIM_NAME = "meter";

	private const string METER_ANIM_COLLAPSE_NAME = "collapsed";

	private static readonly string[] ROCK_SYMBOLS_NAME = new string[4] { "rock_1", "rock_2", "rock_3", "rock_4" };

	public State off;

	public OnStates on;

	public FloatParameter BuildUp;

	public TargetParameter MeterController;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = off;
		off.EventTransition(GameHashes.EntombedChanged, on, GameStateMachine<UnderwaterVent, Instance, IStateMachineTarget, Def>.Not(ShouldBeOff)).EventTransition(GameHashes.SubmergedStateChanged, on, GameStateMachine<UnderwaterVent, Instance, IStateMachineTarget, Def>.Not(ShouldBeOff)).PlayAnim("off");
		on.EventTransition(GameHashes.EntombedChanged, off, ShouldBeOff).EventTransition(GameHashes.SubmergedStateChanged, off, ShouldBeOff).DefaultState(on.erupting);
		on.erupting.ParamTransition(BuildUp, on.blocked, GameStateMachine<UnderwaterVent, Instance, IStateMachineTarget, Def>.IsGTEOne).PlayAnim("erupting", KAnim.PlayMode.Loop).ToggleStatusItem(Db.Get().MiscStatusItems.UnderwaterVentEmiting)
			.ToggleStatusItem(Db.Get().MiscStatusItems.UnderwaterVentBuildUpProgress)
			.Enter(RefreshBuildUpMeter)
			.Update(EruptionUpdate, UpdateRate.SIM_1000ms);
		on.blocked.TriggerOnEnter(GameHashes.VentBlocked).DefaultState(on.blocked.idle);
		on.blocked.idle.ParamTransition(BuildUp, on.blocked.unblock, GameStateMachine<UnderwaterVent, Instance, IStateMachineTarget, Def>.IsZero).PlayAnim("blocked", KAnim.PlayMode.Once).ToggleStatusItem(Db.Get().MiscStatusItems.UnderwaterVentBlocked);
		on.blocked.unblock.Target(MeterController).PlayAnim("collapsed", KAnim.PlayMode.Once).OnAnimQueueComplete(on.erupting)
			.Target(masterTarget)
			.Exit(SpawnSolidDebri);
	}

	private static bool ShouldBeOff(Instance smi)
	{
		if (!IsEntombed(smi))
		{
			return !IsSubmerged(smi);
		}
		return true;
	}

	private static bool IsEntombed(Instance smi)
	{
		return smi.IsEntombed;
	}

	private static bool IsSubmerged(Instance smi)
	{
		return smi.IsSubmerged;
	}

	private static void RefreshBuildUpMeter(Instance smi)
	{
		smi.RefreshBuildUpMeter();
	}

	private static void SpawnSolidDebri(Instance smi)
	{
		smi.SpawnSolidDebri();
	}

	private static void EruptionUpdate(Instance smi, float dt)
	{
		smi.EruptionUpdate(dt);
	}
}
