using Klei.AI;
using UnityEngine;

public class PollinationVFXMonitor : GameStateMachine<PollinationVFXMonitor, PollinationVFXMonitor.Instance, IStateMachineTarget, PollinationVFXMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		private Effects effects;

		private ParticleSystem pollinationEffect;

		private OccupyArea occupyArea;

		private bool isHangingPlant = false;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			effects = GetComponent<Effects>();
			occupyArea = GetComponent<OccupyArea>();
		}

		public override void StartSM()
		{
			isHangingPlant = base.gameObject.HasTag(GameTags.Hanging);
			base.StartSM();
		}

		public bool IsPollinated()
		{
			if (effects == null)
			{
				return false;
			}
			HashedString[] pollinationEffects = PollinationMonitor.PollinationEffects;
			foreach (HashedString effect_id in pollinationEffects)
			{
				if (effects.HasEffect(effect_id))
				{
					return true;
				}
			}
			return false;
		}

		public void CreatePollinationEffect()
		{
			DestroyPollinationEffect();
			Vector4 vector = new Vector4(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
			CellOffset[] occupiedCellsOffsets = occupyArea.OccupiedCellsOffsets;
			for (int i = 0; i < occupiedCellsOffsets.Length; i++)
			{
				CellOffset cellOffset = occupiedCellsOffsets[i];
				if ((float)cellOffset.x < vector.x)
				{
					vector.x = cellOffset.x;
				}
				if ((float)cellOffset.x > vector.y)
				{
					vector.y = cellOffset.x;
				}
				if ((float)cellOffset.y < vector.z)
				{
					vector.z = cellOffset.y;
				}
				if ((float)cellOffset.y > vector.w)
				{
					vector.w = cellOffset.y;
				}
			}
			int num = 1 + (int)Mathf.Clamp(vector.y - vector.x, 0f, 2.1474836E+09f);
			int num2 = 1 + (int)Mathf.Clamp(vector.w - vector.z, 0f, 2.1474836E+09f);
			Vector3 position = Grid.CellToPosCBC(occupyArea.GetOffsetCellWithRotation(new CellOffset(0, isHangingPlant ? (-num2 + 1) : 0)), Grid.SceneLayer.BuildingFront);
			GameObject gameObject = Util.KInstantiate(EffectPrefabs.Instance.PlantPollinated, position, Quaternion.identity, base.gameObject, "PollinationVFX");
			pollinationEffect = gameObject.GetComponent<ParticleSystem>();
			ParticleSystem.ShapeModule shape = pollinationEffect.shape;
			Vector3 scale = shape.scale;
			Vector3 position2 = shape.position;
			scale.x = num;
			scale.y = num2;
			position2.y = (float)num2 * 0.5f;
			shape.scale = scale;
			shape.position = position2;
		}

		public void DestroyPollinationEffect()
		{
			if (pollinationEffect != null)
			{
				pollinationEffect.DeleteObject();
				pollinationEffect = null;
			}
		}
	}

	private State idle;

	private State pollinated;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = idle;
		idle.EventTransition(GameHashes.EffectAdded, pollinated, IsPollinated);
		pollinated.EventTransition(GameHashes.EffectRemoved, idle, GameStateMachine<PollinationVFXMonitor, Instance, IStateMachineTarget, Def>.Not(IsPollinated)).Toggle("Toggle Pollination VFX", CreatePollinationEffect, DestroyPollinationEffect);
	}

	private static bool IsPollinated(Instance smi)
	{
		return smi.IsPollinated();
	}

	private static void DestroyPollinationEffect(Instance smi)
	{
		smi.DestroyPollinationEffect();
	}

	private static void CreatePollinationEffect(Instance smi)
	{
		smi.CreatePollinationEffect();
	}
}
