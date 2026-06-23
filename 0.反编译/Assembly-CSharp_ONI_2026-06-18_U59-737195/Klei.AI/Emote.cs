using System.Collections.Generic;

namespace Klei.AI;

public class Emote : Resource
{
	private HashedString animSetName = null;

	private KAnimFile animSet;

	private HashedString swimAnimSetName = null;

	private KAnimFile swimAnimSet;

	private List<EmoteStep> emoteSteps = new List<EmoteStep>();

	public int StepCount
	{
		get
		{
			if (emoteSteps != null)
			{
				return emoteSteps.Count;
			}
			return 0;
		}
	}

	public KAnimFile AnimSet
	{
		get
		{
			if (animSetName != HashedString.Invalid && animSet == null)
			{
				animSet = Assets.GetAnim(animSetName);
			}
			return animSet;
		}
	}

	public bool IsValid { get; private set; }

	public EmoteStep this[int stepIdx]
	{
		get
		{
			if (!IsValidStep(stepIdx))
			{
				return null;
			}
			return emoteSteps[stepIdx];
		}
	}

	public KAnimFile ManifestSwimAnimSet()
	{
		if (swimAnimSetName != null && swimAnimSet == null)
		{
			swimAnimSet = Assets.GetAnim(swimAnimSetName);
		}
		return swimAnimSet;
	}

	public Emote(ResourceSet parent, string emoteId, EmoteStep[] defaultSteps, string animSetName = null, string swimAnimSetName = null)
		: base(emoteId, parent)
	{
		emoteSteps.AddRange(defaultSteps);
		this.animSetName = animSetName;
		this.swimAnimSetName = swimAnimSetName;
		IsValid = Validate();
	}

	private bool Validate()
	{
		KAnimFileData kAnimFileData = ((AnimSet == null) ? null : AnimSet.GetData());
		if (kAnimFileData == null)
		{
			return false;
		}
		for (int i = 0; i < StepCount; i++)
		{
			bool flag = false;
			for (int j = 0; j < kAnimFileData.animCount; j++)
			{
				if (kAnimFileData.GetAnim(j).name == emoteSteps[i].anim)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Debug.LogWarningFormat("Emote AnimFile [{0}] does not have animations for emote step [{1}]", animSetName, emoteSteps[i].anim);
				return false;
			}
		}
		return true;
	}

	public void ApplyAnimOverrides(KBatchedAnimController animController, KAnimFile overrideSet)
	{
		KAnimFile kAnimFile = ((overrideSet != null) ? overrideSet : AnimSet);
		if (!(kAnimFile == null) && !(animController == null))
		{
			animController.AddAnimOverrides(kAnimFile);
		}
	}

	public void RemoveAnimOverrides(KBatchedAnimController animController, KAnimFile overrideSet)
	{
		KAnimFile kAnimFile = ((overrideSet != null) ? overrideSet : AnimSet);
		if (!(kAnimFile == null) && !(animController == null))
		{
			animController.RemoveAnimOverrides(kAnimFile);
		}
	}

	public void CollectStepAnims(out HashedString[] emoteAnims, int iterations)
	{
		emoteAnims = new HashedString[emoteSteps.Count * iterations];
		for (int i = 0; i < emoteAnims.Length; i++)
		{
			emoteAnims[i] = emoteSteps[i % emoteSteps.Count].anim;
		}
	}

	public bool IsValidStep(int stepIdx)
	{
		if (stepIdx >= 0)
		{
			return stepIdx < emoteSteps.Count;
		}
		return false;
	}

	public int GetStepIndex(HashedString animName)
	{
		int i = 0;
		bool condition = false;
		for (; i < emoteSteps.Count; i++)
		{
			if (emoteSteps[i].anim == animName)
			{
				condition = true;
				break;
			}
		}
		Debug.Assert(condition, $"Could not find emote step {animName} for emote {Id}!");
		return i;
	}
}
