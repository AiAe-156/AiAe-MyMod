using System;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class CritterTemperatureMonitor : GameStateMachine<CritterTemperatureMonitor, CritterTemperatureMonitor.Instance, IStateMachineTarget, CritterTemperatureMonitor.Def>
{
	public class Def : BaseDef
	{
		public float temperatureHotDeadly = float.MaxValue;

		public float temperatureHotUncomfortable = float.MaxValue;

		public float temperatureColdDeadly = float.MinValue;

		public float temperatureColdUncomfortable = float.MinValue;

		public float secondsUntilDamageStarts = 1f;

		public float damagePerSecond = 0.25f;

		public float GetIdealTemperature()
		{
			return (temperatureHotUncomfortable + temperatureColdUncomfortable) / 2f;
		}
	}

	public class TemperatureStates : State
	{
		public State uncomfortable;

		public State deadly;
	}

	public new class Instance : GameInstance
	{
		public AmountInstance temperature;

		public Health health;

		public OccupyArea occupyArea;

		public PrimaryElement primaryElement;

		public Pickupable pickupable;

		public float secondsUntilDamage;

		public Action<float, float> OnUpdate_GetTemperatureInternal;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			health = master.GetComponent<Health>();
			occupyArea = master.GetComponent<OccupyArea>();
			primaryElement = master.GetComponent<PrimaryElement>();
			temperature = Db.Get().Amounts.CritterTemperature.Lookup(base.gameObject);
			pickupable = master.GetComponent<Pickupable>();
		}

		public void ResetDamageCooldown()
		{
			secondsUntilDamage = base.def.secondsUntilDamageStarts;
		}

		public void TryDamage(float deltaSeconds)
		{
			if (secondsUntilDamage <= 0f)
			{
				health.Damage(base.def.damagePerSecond);
				secondsUntilDamage = 1f;
			}
			else
			{
				secondsUntilDamage -= deltaSeconds;
			}
		}

		public BaseState GetTargetState()
		{
			bool flag = IsEntirelyInVaccum();
			float temperatureExternal = GetTemperatureExternal();
			float temperatureInternal = GetTemperatureInternal();
			if (pickupable.KPrefabID.HasTag(GameTags.Dead))
			{
				return base.sm.dead;
			}
			if (!flag && temperatureExternal > base.def.temperatureHotDeadly)
			{
				return base.sm.hot.deadly;
			}
			if (!flag && temperatureExternal < base.def.temperatureColdDeadly)
			{
				return base.sm.cold.deadly;
			}
			if (temperatureInternal > base.def.temperatureHotUncomfortable)
			{
				return base.sm.hot.uncomfortable;
			}
			if (temperatureInternal < base.def.temperatureColdUncomfortable)
			{
				return base.sm.cold.uncomfortable;
			}
			return base.sm.comfortable;
		}

		public bool IsEntirelyInVaccum()
		{
			int cachedCell = pickupable.cachedCell;
			bool result;
			if (occupyArea != null)
			{
				result = true;
				for (int i = 0; i < occupyArea.OccupiedCellsOffsets.Length; i++)
				{
					int num = Grid.OffsetCell(cachedCell, occupyArea.OccupiedCellsOffsets[i]);
					if (!Grid.IsValidCell(num) || !Grid.Element[num].IsVacuum)
					{
						result = false;
						break;
					}
				}
			}
			else
			{
				result = !Grid.IsValidCell(cachedCell) || Grid.Element[cachedCell].IsVacuum;
			}
			return result;
		}

		public float GetTemperatureInternal()
		{
			return primaryElement.Temperature;
		}

		public float GetTemperatureExternal()
		{
			int cachedCell = pickupable.cachedCell;
			if (occupyArea != null)
			{
				float num = 0f;
				int num2 = 0;
				for (int i = 0; i < occupyArea.OccupiedCellsOffsets.Length; i++)
				{
					int num3 = Grid.OffsetCell(cachedCell, occupyArea.OccupiedCellsOffsets[i]);
					if (Grid.IsValidCell(num3))
					{
						bool flag = Grid.Element[num3].id == SimHashes.Vacuum || Grid.Element[num3].id == SimHashes.Void;
						num2++;
						num += (flag ? GetTemperatureInternal() : Grid.Temperature[num3]);
					}
				}
				return num / (float)Mathf.Max(1, num2);
			}
			if (Grid.Element[cachedCell].id != SimHashes.Vacuum && Grid.Element[cachedCell].id != SimHashes.Void)
			{
				return Grid.Temperature[cachedCell];
			}
			return GetTemperatureInternal();
		}
	}

	public State comfortable;

	public State dead;

	public TemperatureStates hot;

	public TemperatureStates cold;

	public Effect uncomfortableEffect;

	public Effect deadlyEffect;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = comfortable;
		uncomfortableEffect = new Effect("EffectCritterTemperatureUncomfortable", CREATURES.MODIFIERS.CRITTER_TEMPERATURE_UNCOMFORTABLE.NAME, CREATURES.MODIFIERS.CRITTER_TEMPERATURE_UNCOMFORTABLE.TOOLTIP, 0f, show_in_ui: false, trigger_floating_text: false, is_bad: true);
		uncomfortableEffect.Add(new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, -1f, CREATURES.MODIFIERS.CRITTER_TEMPERATURE_UNCOMFORTABLE.NAME));
		deadlyEffect = new Effect("EffectCritterTemperatureDeadly", CREATURES.MODIFIERS.CRITTER_TEMPERATURE_DEADLY.NAME, CREATURES.MODIFIERS.CRITTER_TEMPERATURE_DEADLY.TOOLTIP, 0f, show_in_ui: false, trigger_floating_text: false, is_bad: true);
		deadlyEffect.Add(new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, -2f, CREATURES.MODIFIERS.CRITTER_TEMPERATURE_DEADLY.NAME));
		root.Enter(RefreshInternalTemperature).Update(delegate(Instance smi, float dt)
		{
			BaseState targetState = smi.GetTargetState();
			if (smi.GetCurrentState() != targetState)
			{
				smi.GoTo(targetState);
			}
		}).Update(UpdateInternalTemperature, UpdateRate.SIM_1000ms);
		hot.TagTransition(GameTags.Dead, dead).ToggleCritterEmotion(Db.Get().CritterEmotions.Hot);
		cold.TagTransition(GameTags.Dead, dead).ToggleCritterEmotion(Db.Get().CritterEmotions.Cold);
		hot.uncomfortable.ToggleStatusItem(Db.Get().CreatureStatusItems.TemperatureHotUncomfortable).ToggleEffect((Instance smi) => uncomfortableEffect);
		hot.deadly.ToggleStatusItem(Db.Get().CreatureStatusItems.TemperatureHotDeadly).ToggleEffect((Instance smi) => deadlyEffect).Enter(delegate(Instance smi)
		{
			smi.ResetDamageCooldown();
		})
			.Update(delegate(Instance smi, float dt)
			{
				smi.TryDamage(dt);
			});
		cold.uncomfortable.ToggleStatusItem(Db.Get().CreatureStatusItems.TemperatureColdUncomfortable).ToggleEffect((Instance smi) => uncomfortableEffect);
		cold.deadly.ToggleStatusItem(Db.Get().CreatureStatusItems.TemperatureColdDeadly).ToggleEffect((Instance smi) => deadlyEffect).Enter(delegate(Instance smi)
		{
			smi.ResetDamageCooldown();
		})
			.Update(delegate(Instance smi, float dt)
			{
				smi.TryDamage(dt);
			});
		dead.DoNothing();
	}

	public static void UpdateInternalTemperature(Instance smi, float dt)
	{
		RefreshInternalTemperature(smi);
		if (smi.OnUpdate_GetTemperatureInternal != null)
		{
			smi.OnUpdate_GetTemperatureInternal(dt, smi.GetTemperatureInternal());
		}
	}

	public static void RefreshInternalTemperature(Instance smi)
	{
		if (smi.temperature != null)
		{
			smi.temperature.SetValue(smi.GetTemperatureInternal());
		}
	}
}
