using System.Collections.Generic;

namespace Klei.AI;

public class Emote : Resource
{
	private HashedString animSetName = null;

	private KAnimFile animSet = null;

	private HashedString swimAnimSetName = null;

	private KAnimFile swimAnimSet = null;

	private List<EmoteStep> emoteSteps = new List<EmoteStep>();

	public int StepCount => (emoteSteps != null) ? emoteSteps.Count : 0;

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
	}

	public bool IsValidForController(KBatchedAnimController animController)
	{
		bool flag = true;
		KAnimFileData kAnimFileData = ((animSet == null) ? null : animSet.GetData());
		int num = 0;
		while (kAnimFileData != null && flag && num < StepCount)
		{
			bool flag2 = false;
			int num2 = 0;
			while (!flag2 && num2 < kAnimFileData.animCount)
			{
				flag2 = kAnimFileData.GetAnim(num).id == emoteSteps[num].anim;
				num2++;
			}
			flag = flag2;
			num++;
		}
		return flag;
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
		return stepIdx >= 0 && stepIdx < emoteSteps.Count;
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
