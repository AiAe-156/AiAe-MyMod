using Klei.AI;
using UnityEngine;

public class HappySinger : GameStateMachine<HappySinger, HappySinger.Instance>
{
	public class OverjoyedStates : State
	{
		public State idle;

		public State moving;
	}

	public new class Instance : GameInstance
	{
		private Reactable passerbyReactable;

		public GameObject musicParticleFX;

		private SpeechMonitor.Instance speechMonitorInstance;

		public KPrefabID Kpid { get; private set; }

		public KBatchedAnimController AnimController { get; private set; }

		public SpeechMonitor.Instance SpeechMonitorInstance
		{
			get
			{
				if (speechMonitorInstance == null)
				{
					speechMonitorInstance = base.master.gameObject.GetSMI<SpeechMonitor.Instance>();
				}
				return speechMonitorInstance;
			}
		}

		public Instance(IStateMachineTarget master)
			: base(master)
		{
			Kpid = master.GetComponent<KPrefabID>();
			AnimController = master.GetComponent<KBatchedAnimController>();
		}

		public void CreatePasserbyReactable()
		{
			if (passerbyReactable == null)
			{
				EmoteReactable emoteReactable = new EmoteReactable(base.gameObject, "WorkPasserbyAcknowledgement", Db.Get().ChoreTypes.Emote, 5, 5, 0f, 600f);
				Emote sing = Db.Get().Emotes.Minion.Sing;
				emoteReactable.SetEmote(sing).SetThought(Db.Get().Thoughts.CatchyTune).AddPrecondition(ReactorIsOnFloor);
				emoteReactable.RegisterEmoteStepCallbacks("react", AddReactionEffect, null);
				passerbyReactable = emoteReactable;
			}
		}

		private void AddReactionEffect(GameObject reactor)
		{
			reactor.Trigger(-1278274506);
		}

		private bool ReactorIsOnFloor(GameObject reactor, Navigator.ActiveTransition transition)
		{
			return transition.end == NavType.Floor;
		}

		public void ClearPasserbyReactable()
		{
			if (passerbyReactable != null)
			{
				passerbyReactable.Cleanup();
				passerbyReactable = null;
			}
		}
	}

	private Vector3 offset = new Vector3(0f, 0f, 0.1f);

	public State neutral;

	public OverjoyedStates overjoyed;

	public string soundPath = GlobalAssets.GetSound("DupeSinging_NotesFX_LP");

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = neutral;
		root.TagTransition(GameTags.Dead, null);
		neutral.TagTransition(GameTags.Overjoyed, overjoyed);
		overjoyed.DefaultState(overjoyed.idle).TagTransition(GameTags.Overjoyed, neutral, on_remove: true).ToggleEffect("IsJoySinger")
			.ToggleLoopingSound(soundPath)
			.ToggleAnims("anim_loco_singer_kanim")
			.ToggleAnims("anim_idle_singer_kanim")
			.EventHandler(GameHashes.TagsChanged, delegate(Instance smi, object obj)
			{
				if (smi.musicParticleFX != null)
				{
					smi.musicParticleFX.SetActive(!smi.HasTag(GameTags.Asleep));
				}
			})
			.Enter(delegate(Instance smi)
			{
				smi.musicParticleFX = Util.KInstantiate(EffectPrefabs.Instance.HappySingerFX, smi.master.transform.GetPosition() + offset);
				smi.musicParticleFX.transform.SetParent(smi.master.transform);
				smi.CreatePasserbyReactable();
				smi.musicParticleFX.SetActive(!smi.HasTag(GameTags.Asleep));
			})
			.Update(delegate(Instance smi, float dt)
			{
				if (!smi.SpeechMonitorInstance.IsPlayingSpeech() && SpeechMonitor.IsAllowedToPlaySpeech(smi.Kpid, smi.AnimController))
				{
					Db.Get().Thoughts.CatchyTune.PlayAsSpeech(smi.SpeechMonitorInstance);
				}
			}, UpdateRate.SIM_1000ms)
			.Exit(delegate(Instance smi)
			{
				smi.musicParticleFX.SetActive(value: false);
				Util.KDestroyGameObject(smi.musicParticleFX);
				smi.ClearPasserbyReactable();
			});
	}
}
