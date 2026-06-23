using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;

public class SpeechMonitor : GameStateMachine<SpeechMonitor, SpeechMonitor.Instance, IStateMachineTarget, SpeechMonitor.Def>
{
	public class Playing : State
	{
		public State audioGoverned;

		public State animGoverned;

		public State fallback;
	}

	public class Def : BaseDef
	{
	}

	public class Tuning : TuningData<Tuning>
	{
		public float randomSpeechIntervalMin;

		public float randomSpeechIntervalMax;

		public int speechCount;
	}

	public new class Instance : GameInstance
	{
		public KBatchedAnimController mouth;

		public string speechPrefix = "happy";

		public string voiceEvent;

		public EventInstance ev;

		public string mouthId;

		public KBatchedAnimController AnimController { get; private set; }

		public SymbolOverrideController SymbolOverrideController { get; private set; }

		public MinionIdentity MinionIdentity { get; private set; }

		public KPrefabID Kpid { get; private set; }

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			AnimController = master.GetComponent<KBatchedAnimController>();
			SymbolOverrideController = master.GetComponent<SymbolOverrideController>();
			MinionIdentity = master.GetComponent<MinionIdentity>();
			Kpid = master.GetComponent<KPrefabID>();
		}

		public bool IsPlayingSpeech()
		{
			return IsInsideState(base.sm.talking);
		}

		public void PlaySpeech(string speech_prefix, string voice_event)
		{
			speechPrefix = speech_prefix;
			voiceEvent = voice_event;
			GoTo(base.sm.talking);
		}

		public void DrawMouth()
		{
			KAnim.Anim.FrameElement firstFrameElement = GetFirstFrameElement(base.smi.mouth);
			bool flag = firstFrameElement.symbol != HashedString.Invalid;
			DebugUtil.DevAssert(flag, "Mouth frame element invalid");
			if (flag)
			{
				KAnim.Build build = base.smi.mouth.AnimFiles[0].GetData().build;
				KAnim.Build.Symbol symbol = build.GetSymbol(firstFrameElement.symbol);
				SymbolOverrideController.AddSymbolOverride(HASH_SNAPTO_MOUTH, symbol, 3);
				KBatchGroupData batchGroupData = KAnimBatchManager.Instance().GetBatchGroupData(AnimController.batchGroupID);
				KAnim.Build.Symbol symbol2 = batchGroupData.GetSymbol(HASH_SNAPTO_MOUTH);
				DebugUtil.DevAssert(build == symbol.build, "Mouth build mismatch");
				KBatchGroupData batchGroupData2 = KAnimBatchManager.Instance().GetBatchGroupData(build.batchTag);
				KAnim.Build.SymbolFrameInstance symbol_frame_instance = batchGroupData2.symbolFrameInstances[symbol.firstFrameIdx + firstFrameElement.frame];
				symbol_frame_instance.buildImageIdx = SymbolOverrideController.GetAtlasIdx(build.GetTexture(0));
				AnimController.SetSymbolOverride(symbol2.firstFrameIdx, ref symbol_frame_instance);
			}
		}

		public void SetMouthId()
		{
			Personality personality = Db.Get().Personalities.Get(MinionIdentity.personalityResourceId);
			if (personality.speech_mouth > 0)
			{
				base.smi.mouthId = $"_{personality.speech_mouth:000}";
			}
		}
	}

	public State satisfied;

	public Playing talking;

	public static string PREFIX_SAD = "sad";

	public static string PREFIX_HAPPY = "happy";

	public static string PREFIX_SINGER = "sing";

	public TargetParameter mouth;

	private static HashedString HASH_SNAPTO_MOUTH = "snapto_mouth";

	private static HashedString GENERIC_CONVO_ANIM_NAME = new HashedString("anim_generic_convo_kanim");

	private static KAnim.Anim.FrameElement INVALID_FRAME_ELEMENT = new KAnim.Anim.FrameElement
	{
		symbol = HashedString.Invalid
	};

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = satisfied;
		root.Enter(CreateMouth).Exit(DestroyMouth);
		satisfied.DoNothing();
		talking.Enter(delegate(Instance smi)
		{
			StartAudio(smi);
			smi.mouth.Play(GetRandomSpeechAnim(smi));
			if (smi.Kpid.HasTag(GameTags.DoNotInterruptMe))
			{
				smi.GoTo(talking.animGoverned);
			}
			else if (smi.ev.isValid())
			{
				smi.GoTo(talking.audioGoverned);
			}
			else
			{
				smi.GoTo(talking.fallback);
			}
		}).Exit(delegate(Instance smi)
		{
			smi.SymbolOverrideController.RemoveSymbolOverride(HASH_SNAPTO_MOUTH, 3);
		});
		talking.audioGoverned.Transition(satisfied, IsAudioStopped).Update(LipFlap, UpdateRate.RENDER_EVERY_TICK);
		talking.animGoverned.TagTransition(GameTags.DoNotInterruptMe, satisfied, on_remove: true).Update(LipFlap, UpdateRate.RENDER_EVERY_TICK).Update(delegate(Instance smi, float dt)
		{
			if (IsAudioStopped(smi))
			{
				StartAudio(smi);
			}
		}, UpdateRate.RENDER_EVERY_TICK);
		talking.fallback.Enter(delegate(Instance smi)
		{
			smi.mouth.Queue(GetRandomSpeechAnim(smi));
		}).Target(mouth).OnAnimQueueComplete(satisfied);
	}

	private static void CreateMouth(Instance smi)
	{
		smi.mouth = Util.KInstantiate(Assets.GetPrefab(MouthAnimation.ID)).GetComponent<KBatchedAnimController>();
		smi.mouth.gameObject.SetActive(value: true);
		smi.sm.mouth.Set(smi.mouth.gameObject, smi);
		smi.SetMouthId();
	}

	private static void DestroyMouth(Instance smi)
	{
		if (smi.mouth != null)
		{
			Util.KDestroyGameObject(smi.mouth);
			smi.mouth = null;
		}
	}

	private static string GetRandomSpeechAnim(Instance smi)
	{
		return smi.speechPrefix + Random.Range(1, TuningData<Tuning>.Get().speechCount) + smi.mouthId;
	}

	public static bool IsAllowedToPlaySpeech(KPrefabID prefabID, KBatchedAnimController controller)
	{
		if (prefabID.HasTag(GameTags.Dead))
		{
			return false;
		}
		if (prefabID.HasTag(GameTags.Incapacitated))
		{
			return false;
		}
		KAnim.Anim currentAnim = controller.GetCurrentAnim();
		if (currentAnim == null)
		{
			return true;
		}
		return GameAudioSheets.Get().IsAnimAllowedToPlaySpeech(currentAnim) && CanOverrideHead(controller);
	}

	private static bool CanOverrideHead(KBatchedAnimController kbac)
	{
		bool result = true;
		KAnim.Anim currentAnim = kbac.GetCurrentAnim();
		if (currentAnim == null)
		{
			result = false;
		}
		else if (currentAnim.animFile.name != GENERIC_CONVO_ANIM_NAME)
		{
			int currentFrameIndex = kbac.GetCurrentFrameIndex();
			if (currentFrameIndex <= 0)
			{
				result = false;
			}
			else
			{
				KBatchGroupData batchGroupData = KAnimBatchManager.Instance().GetBatchGroupData(currentAnim.animFile.animBatchTag);
				if (batchGroupData.TryGetFrame(currentFrameIndex, out var frame) && frame.hasHead)
				{
					result = false;
				}
			}
		}
		return result;
	}

	private static KAnim.Anim.FrameElement GetFirstFrameElement(KBatchedAnimController controller)
	{
		int currentFrameIndex = controller.GetCurrentFrameIndex();
		if (currentFrameIndex == -1)
		{
			return INVALID_FRAME_ELEMENT;
		}
		KAnimBatch batch = controller.GetBatch();
		if (batch == null)
		{
			return INVALID_FRAME_ELEMENT;
		}
		if (!batch.group.data.TryGetFrame(currentFrameIndex, out var frame))
		{
			return INVALID_FRAME_ELEMENT;
		}
		List<KAnim.Anim.FrameElement> frameElements = batch.group.data.frameElements;
		for (int i = 0; i < frame.numElements; i++)
		{
			int num = frame.firstElementIdx + i;
			bool flag = num < frameElements.Count;
			DebugUtil.DevAssert(flag, "Frame element index out of range");
			if (flag)
			{
				KAnim.Anim.FrameElement result = frameElements[num];
				if (!(result.symbol == HashedString.Invalid))
				{
					return result;
				}
			}
		}
		return INVALID_FRAME_ELEMENT;
	}

	private static void StartAudio(Instance smi)
	{
		smi.ev.clearHandle();
		if (smi.voiceEvent != null)
		{
			smi.ev = VoiceSoundEvent.PlayVoice(smi.voiceEvent, smi.AnimController, 0f, looping: false);
		}
	}

	private static bool IsAudioStopped(Instance smi)
	{
		if (!smi.ev.isValid())
		{
			return true;
		}
		smi.ev.getPlaybackState(out var state);
		if (state == PLAYBACK_STATE.STOPPING || state == PLAYBACK_STATE.STOPPED)
		{
			smi.ev.clearHandle();
			return true;
		}
		return false;
	}

	private static void LipFlap(Instance smi, float dt)
	{
		if (smi.mouth.IsStopped())
		{
			smi.mouth.Play(GetRandomSpeechAnim(smi));
			DebugUtil.DevAssert(!smi.mouth.IsStopped(), "Mouth animation should be playing");
		}
	}
}
