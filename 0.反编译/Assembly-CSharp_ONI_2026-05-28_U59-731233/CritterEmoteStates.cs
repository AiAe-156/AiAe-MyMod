using Database;
using UnityEngine;

public class CritterEmoteStates : GameStateMachine<CritterEmoteStates, CritterEmoteStates.Instance, IStateMachineTarget, CritterEmoteStates.Def>
{
	public class Def : BaseDef
	{
		public KAnimFile emoteBuildFile;

		public Def(KAnimFile emoteBuildFile)
		{
			this.emoteBuildFile = emoteBuildFile;
		}
	}

	public new class Instance : GameInstance
	{
		public KAnimFile emoteBuildFile;

		public CritterEmotion emotion;

		public bool hasSetThoughtBubble = false;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.Behaviours.CritterEmoteBehaviour);
			emoteBuildFile = def.emoteBuildFile;
		}
	}

	public State playing;

	public State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = root;
		root.Enter(delegate(Instance smi)
		{
			smi.emotion = smi.GetSMI<CritterEmoteMonitor.Instance>().GetCritterEmotion();
			if (smi.emotion != null)
			{
				smi.GoTo(playing);
			}
			else
			{
				smi.GoTo(behaviourcomplete);
			}
		});
		playing.ToggleAnims((Instance smi) => smi.emoteBuildFile).PlayAnims((Instance smi) => (!smi.emotion.isPositiveEmotion) ? new HashedString[1] { "react_neg" } : new HashedString[1] { "react_pos" }).ScheduleGoTo(10f, behaviourcomplete)
			.OnAnimQueueComplete(behaviourcomplete)
			.Enter(delegate(Instance smi)
			{
				CritterEmoteMonitor.Instance sMI = smi.GetSMI<CritterEmoteMonitor.Instance>();
				smi.emotion = sMI.GetCritterEmotion();
				if (!sMI.cooldowns.ContainsKey(smi.emotion))
				{
					sMI.cooldowns.Add(smi.emotion, Time.timeSinceLevelLoad);
				}
				else
				{
					sMI.cooldowns[smi.emotion] = Time.timeSinceLevelLoad;
				}
				if (smi.emotion.sprite != null)
				{
					NameDisplayScreen.Instance.SetThoughtBubbleDisplay(smi.gameObject, bVisible: true, "", Assets.GetSprite("bubble_alert"), smi.emotion.sprite);
					smi.hasSetThoughtBubble = true;
				}
			})
			.Exit(delegate(Instance smi)
			{
				if (smi.hasSetThoughtBubble)
				{
					NameDisplayScreen.Instance.SetThoughtBubbleDisplay(smi.gameObject, bVisible: false, null, null, null);
					smi.hasSetThoughtBubble = false;
				}
			});
		behaviourcomplete.PlayAnim("idle_loop", KAnim.PlayMode.Loop).BehaviourComplete(GameTags.Creatures.Behaviours.CritterEmoteBehaviour);
	}
}
