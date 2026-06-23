using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/BrainScheduler")]
public class BrainScheduler : KMonoBehaviour, IRenderEveryTick
{
	public abstract class BrainGroup
	{
		protected List<Brain> brains = new List<Brain>();

		protected Queue<Brain> priorityBrains = new Queue<Brain>();

		private string increaseLoadLabel;

		private string decreaseLoadLabel;

		public bool debugFreezeLoadAdustment;

		public int debugMaxPriorityBrainCountSeen;

		protected int nextUpdateBrain;

		public Tag tag { get; private set; }

		public int BrainCount => brains.Count;

		protected BrainGroup(Tag tag)
		{
			this.tag = tag;
			string text = tag.ToString();
			increaseLoadLabel = "IncLoad" + text;
			decreaseLoadLabel = "DecLoad" + text;
		}

		public void AddBrain(Brain brain)
		{
			brains.Add(brain);
		}

		public void RemoveBrain(Brain brain)
		{
			int num = brains.IndexOf(brain);
			if (num != -1)
			{
				brains.RemoveAt(num);
				OnRemoveBrain(num, ref nextUpdateBrain);
			}
			if (priorityBrains.Contains(brain))
			{
				List<Brain> list = new List<Brain>(priorityBrains);
				list.Remove(brain);
				priorityBrains = new Queue<Brain>(list);
			}
		}

		public void PrioritizeBrain(Brain brain)
		{
			if (!priorityBrains.Contains(brain))
			{
				priorityBrains.Enqueue(brain);
			}
		}

		private void IncrementBrainIndex(ref int brainIndex)
		{
			brainIndex++;
			if (brainIndex == brains.Count)
			{
				brainIndex = 0;
			}
		}

		private void ClampBrainIndex(ref int brainIndex)
		{
			brainIndex = MathUtil.Clamp(0, brains.Count - 1, brainIndex);
		}

		private void OnRemoveBrain(int removedIndex, ref int brainIndex)
		{
			if (removedIndex < brainIndex)
			{
				brainIndex--;
			}
			else if (brainIndex == brains.Count)
			{
				brainIndex = 0;
			}
		}

		public void RenderEveryTick(float dt)
		{
			BeginBrainGroupUpdate();
			int num = InitialProbeCount();
			for (int i = 0; i != brains.Count; i++)
			{
				if (num == 0)
				{
					break;
				}
				ClampBrainIndex(ref nextUpdateBrain);
				debugMaxPriorityBrainCountSeen = Mathf.Max(debugMaxPriorityBrainCountSeen, priorityBrains.Count);
				Brain brain;
				if (AllowPriorityBrains() && priorityBrains.Count > 0)
				{
					brain = priorityBrains.Dequeue();
				}
				else
				{
					brain = brains[nextUpdateBrain];
					IncrementBrainIndex(ref nextUpdateBrain);
				}
				if (brain.IsRunning())
				{
					brain.UpdateBrain();
					num--;
				}
			}
			EndBrainGroupUpdate();
		}

		public virtual void PostRenderEveryTick(float dt)
		{
		}

		protected abstract int InitialProbeCount();

		protected abstract int InitialProbeSize();

		protected abstract int MinProbeSize();

		protected abstract int IdealProbeSize();

		protected abstract int ProbeSizeStep();

		public abstract float GetEstimatedFrameTime();

		public abstract float LoadBalanceThreshold();

		public abstract bool AllowPriorityBrains();

		public virtual void BeginBrainGroupUpdate()
		{
		}

		public virtual void EndBrainGroupUpdate()
		{
		}
	}

	private class DupeBrainGroup : BrainGroup
	{
		public class Tuning : TuningData<Tuning>
		{
			public int initialProbeCount = 1;

			public int initialProbeSize = 1000;

			public int minProbeSize = 100;

			public int idealProbeSize = 1000;

			public int probeSizeStep = 100;

			public float estimatedFrameTime = 2f;

			public float loadBalanceThreshold = 0.1f;
		}

		private bool usePriorityBrain = true;

		public DupeBrainGroup()
			: base(GameTags.DupeBrain)
		{
		}

		protected override int InitialProbeCount()
		{
			return TuningData<Tuning>.Get().initialProbeCount;
		}

		protected override int InitialProbeSize()
		{
			return TuningData<Tuning>.Get().initialProbeSize;
		}

		protected override int MinProbeSize()
		{
			return TuningData<Tuning>.Get().minProbeSize;
		}

		protected override int IdealProbeSize()
		{
			return TuningData<Tuning>.Get().idealProbeSize;
		}

		protected override int ProbeSizeStep()
		{
			return TuningData<Tuning>.Get().probeSizeStep;
		}

		public override float GetEstimatedFrameTime()
		{
			return TuningData<Tuning>.Get().estimatedFrameTime;
		}

		public override float LoadBalanceThreshold()
		{
			return TuningData<Tuning>.Get().loadBalanceThreshold;
		}

		public override bool AllowPriorityBrains()
		{
			return usePriorityBrain;
		}

		public override void BeginBrainGroupUpdate()
		{
			base.BeginBrainGroupUpdate();
			usePriorityBrain = !usePriorityBrain;
		}
	}

	private class CreatureBrainGroup : BrainGroup
	{
		public class Tuning : TuningData<Tuning>
		{
			public int initialProbeCount = 5;

			public int initialProbeSize = 1000;

			public int minProbeSize = 100;

			public int idealProbeSize = 300;

			public int probeSizeStep = 100;

			public float estimatedFrameTime = 1f;

			public float loadBalanceThreshold = 0.1f;
		}

		public CreatureBrainGroup()
			: base(GameTags.CreatureBrain)
		{
		}

		protected override int InitialProbeCount()
		{
			return TuningData<Tuning>.Get().initialProbeCount;
		}

		protected override int InitialProbeSize()
		{
			return TuningData<Tuning>.Get().initialProbeSize;
		}

		protected override int MinProbeSize()
		{
			return TuningData<Tuning>.Get().minProbeSize;
		}

		protected override int IdealProbeSize()
		{
			return TuningData<Tuning>.Get().idealProbeSize;
		}

		protected override int ProbeSizeStep()
		{
			return TuningData<Tuning>.Get().probeSizeStep;
		}

		public override float GetEstimatedFrameTime()
		{
			return TuningData<Tuning>.Get().estimatedFrameTime;
		}

		public override float LoadBalanceThreshold()
		{
			return TuningData<Tuning>.Get().loadBalanceThreshold;
		}

		public override void PostRenderEveryTick(float dt)
		{
			int num = InitialProbeCount();
			for (int i = 0; i < brains.Count && i < num; i++)
			{
				int index = (nextUpdateBrain + i) % brains.Count;
				if (brains[index].IsRunning())
				{
					Navigator component = brains[index].GetComponent<Navigator>();
					if (component != null && component.NavGrid != null)
					{
						component.NavGrid.AddDirtyCell(component.cachedCell);
					}
				}
			}
		}

		public override bool AllowPriorityBrains()
		{
			return true;
		}
	}

	public const float millisecondsPerFrame = 33.33333f;

	public const float secondsPerFrame = 0.033333328f;

	public const float framesPerSecond = 30.000006f;

	private List<BrainGroup> brainGroups = new List<BrainGroup>();

	public List<BrainGroup> debugGetBrainGroups()
	{
		return brainGroups;
	}

	protected override void OnPrefabInit()
	{
		brainGroups.Add(new DupeBrainGroup());
		brainGroups.Add(new CreatureBrainGroup());
		Components.Brains.Register(OnAddBrain, OnRemoveBrain);
	}

	private void OnAddBrain(Brain brain)
	{
		bool test = false;
		foreach (BrainGroup brainGroup in brainGroups)
		{
			if (brain.HasTag(brainGroup.tag))
			{
				brainGroup.AddBrain(brain);
				test = true;
			}
		}
		DebugUtil.Assert(test);
	}

	private void OnRemoveBrain(Brain brain)
	{
		bool test = false;
		foreach (BrainGroup brainGroup in brainGroups)
		{
			if (brain.HasTag(brainGroup.tag))
			{
				test = true;
				brainGroup.RemoveBrain(brain);
			}
			Navigator component = brain.GetComponent<Navigator>();
			if (component != null)
			{
				component.executePathProbeTaskAsync = false;
			}
		}
		DebugUtil.Assert(test);
	}

	public void PrioritizeBrain(Brain brain)
	{
		foreach (BrainGroup brainGroup in brainGroups)
		{
			if (brain.HasTag(brainGroup.tag))
			{
				brainGroup.PrioritizeBrain(brain);
			}
		}
	}

	public void RenderEveryTick(float dt)
	{
		if (Game.IsQuitting() || KMonoBehaviour.isLoadingScene)
		{
			return;
		}
		foreach (BrainGroup brainGroup in brainGroups)
		{
			brainGroup.RenderEveryTick(dt);
			brainGroup.PostRenderEveryTick(dt);
		}
	}

	protected override void OnForcedCleanUp()
	{
		base.OnForcedCleanUp();
	}
}
