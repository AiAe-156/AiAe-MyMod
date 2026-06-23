using System.Collections.Generic;
using Database;
using Klei.AI;
using UnityEngine;

public class CritterEmoteMonitor : GameStateMachine<CritterEmoteMonitor, CritterEmoteMonitor.Instance, IStateMachineTarget, CritterEmoteMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance, IDevQuickAction
	{
		public Emote emotePositive;

		public Emote emoteNegative;

		public List<CritterEmotion> currentNegativeEmotions = new List<CritterEmotion>();

		public List<CritterEmotion> currentPositiveEmotions = new List<CritterEmotion>();

		public const float SPECIFIC_EMOTE_COOLDOWN = 30f;

		public Dictionary<CritterEmotion, float> cooldowns = new Dictionary<CritterEmotion, float>();

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			emotePositive = Db.Get().Emotes.Critter.Positive;
			emoteNegative = Db.Get().Emotes.Critter.Negative;
		}

		public CritterEmotion GetCritterEmotion()
		{
			if (currentNegativeEmotions.Count > 0)
			{
				float num = float.PositiveInfinity;
				CritterEmotion result = null;
				foreach (CritterEmotion currentNegativeEmotion in currentNegativeEmotions)
				{
					if (!cooldowns.ContainsKey(currentNegativeEmotion))
					{
						return currentNegativeEmotion;
					}
					float num2 = cooldowns[currentNegativeEmotion];
					if (num2 < num)
					{
						num = num2;
						result = currentNegativeEmotion;
					}
				}
				return result;
			}
			if (currentPositiveEmotions.Count > 0)
			{
				float num3 = 0f;
				CritterEmotion result2 = null;
				foreach (CritterEmotion currentPositiveEmotion in currentPositiveEmotions)
				{
					if (!cooldowns.ContainsKey(currentPositiveEmotion))
					{
						return currentPositiveEmotion;
					}
					float num4 = cooldowns[currentPositiveEmotion];
					if (num4 < num3)
					{
						num3 = num4;
						result2 = currentPositiveEmotion;
					}
				}
				return result2;
			}
			return null;
		}

		public void AddCritterEmotion(CritterEmotion emotion)
		{
			BabyMonitor.Instance sMI = base.smi.GetSMI<BabyMonitor.Instance>();
			if (sMI != null)
			{
				return;
			}
			if (!emotion.isPositiveEmotion)
			{
				if (currentNegativeEmotions.Contains(emotion))
				{
					return;
				}
				currentNegativeEmotions.Add(emotion);
			}
			else
			{
				if (currentPositiveEmotions.Contains(emotion))
				{
					return;
				}
				currentPositiveEmotions.Add(emotion);
			}
			if (base.smi.IsInsideState(base.sm.cooldown) && !cooldowns.ContainsKey(emotion))
			{
				base.smi.GoTo(base.sm.express);
			}
		}

		public void RemoveCritterEmotion(CritterEmotion emotion)
		{
			currentNegativeEmotions.RemoveAll((CritterEmotion e) => e.id == emotion.id);
			currentPositiveEmotions.RemoveAll((CritterEmotion e) => e.id == emotion.id);
		}

		public List<DevQuickActionInstruction> GetDevInstructions()
		{
			return new List<DevQuickActionInstruction>
			{
				new DevQuickActionInstruction("Emote/Play", delegate
				{
					base.smi.GoTo(base.smi.sm.express);
				})
			};
		}
	}

	public State cooldown;

	public State express;

	public static bool ShouldEmote(Instance smi)
	{
		return true;
	}

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = cooldown;
		base.serializable = SerializeType.ParamsOnly;
		List<CritterEmotion> cooldownsToRemove = new List<CritterEmotion>();
		cooldown.ScheduleGoTo((Instance smi) => Random.Range(37.5f, 75f), express).Enter(delegate(Instance smi)
		{
			NameDisplayScreen.Instance.SetThoughtBubbleDisplay(smi.gameObject, bVisible: false, null, null, null);
		}).Update(delegate(Instance smi, float dt)
		{
			foreach (KeyValuePair<CritterEmotion, float> cooldown in smi.cooldowns)
			{
				if (Time.timeSinceLevelLoad > smi.cooldowns[cooldown.Key] + 30f)
				{
					cooldownsToRemove.Add(cooldown.Key);
				}
			}
			foreach (CritterEmotion item in cooldownsToRemove)
			{
				smi.cooldowns.Remove(item);
			}
			cooldownsToRemove.Clear();
		});
		express.ToggleBehaviour(GameTags.Creatures.Behaviours.CritterEmoteBehaviour, ShouldEmote, delegate(Instance smi)
		{
			smi.GoTo(cooldown);
		});
	}
}
