using System;
using System.Collections.Generic;
using System.Diagnostics;
using KSerialization;
using UnityEngine;

namespace Klei.AI;

[SerializationConfig(MemberSerialization.OptIn)]
[DebuggerDisplay("{amount.Name} {value} ({deltaAttribute.value}/{minAttribute.value}/{maxAttribute.value})")]
public class AmountInstance : ModifierInstance<Amount>, ISaveLoadable, ISim200ms
{
	private struct BatchUpdateContext
	{
		public struct Result
		{
			public AmountInstance amount_instance;

			public float previous;

			public float delta;
		}

		public List<UpdateBucketWithUpdater<ISim200ms>.Entry> amount_instances;

		public float time_delta;

		public static BatchUpdateContext EmptyContext = new BatchUpdateContext(null, 0f);

		public BatchUpdateContext(List<UpdateBucketWithUpdater<ISim200ms>.Entry> amount_instances, float time_delta)
		{
			this.amount_instances = amount_instances;
			this.time_delta = time_delta;
		}
	}

	private class AmmountInstanceBatchUpdateDispatcher : WorkItemCollectionWithThreadContex<BatchUpdateContext, List<BatchUpdateContext.Result>>
	{
		private const int kBatchSize = 512;

		private static AmmountInstanceBatchUpdateDispatcher instance;

		public static AmmountInstanceBatchUpdateDispatcher Instance
		{
			get
			{
				if (instance == null || instance.threadContexts.Count != GlobalJobManager.ThreadCount)
				{
					instance = new AmmountInstanceBatchUpdateDispatcher();
				}
				return instance;
			}
		}

		public AmmountInstanceBatchUpdateDispatcher()
		{
			threadContexts = new List<List<BatchUpdateContext.Result>>(GlobalJobManager.ThreadCount);
			for (int i = 0; i < GlobalJobManager.ThreadCount; i++)
			{
				threadContexts.Add(new List<BatchUpdateContext.Result>());
			}
		}

		public void Reset(BatchUpdateContext context)
		{
			sharedData = context;
			if (context.amount_instances == null)
			{
				count = 0;
			}
			else
			{
				count = (context.amount_instances.Count + 512 - 1) / 512;
			}
		}

		public override void RunItem(int item, ref BatchUpdateContext shared_data, List<BatchUpdateContext.Result> thread_context, int threadIndex)
		{
			int num = item * 512;
			int num2 = Mathf.Min(num + 512, shared_data.amount_instances.Count);
			for (int i = num; i < num2; i++)
			{
				AmountInstance amountInstance = (AmountInstance)shared_data.amount_instances[i].data;
				float num3 = amountInstance.GetDelta() * shared_data.time_delta;
				if (num3 != 0f)
				{
					thread_context.Add(new BatchUpdateContext.Result
					{
						amount_instance = amountInstance,
						previous = amountInstance.value,
						delta = num3
					});
					amountInstance.SetValue(amountInstance.value + num3);
				}
			}
		}

		public void Finish()
		{
			foreach (List<BatchUpdateContext.Result> threadContext in threadContexts)
			{
				foreach (BatchUpdateContext.Result item in threadContext)
				{
					item.amount_instance.Publish(item.delta, item.previous);
				}
				threadContext.Clear();
			}
		}
	}

	[Serialize]
	public float value;

	public AttributeInstance minAttribute;

	public AttributeInstance maxAttribute;

	public AttributeInstance deltaAttribute;

	public Action<float> OnDelta;

	public System.Action OnMaxValueReached;

	public bool hide;

	private bool _paused;

	public Amount amount => modifier;

	public bool paused
	{
		get
		{
			return _paused;
		}
		set
		{
			_paused = paused;
			if (_paused)
			{
				Deactivate();
			}
			else
			{
				Activate();
			}
		}
	}

	public float GetMin()
	{
		return minAttribute.GetTotalValue();
	}

	public float GetMax()
	{
		return maxAttribute.GetTotalValue();
	}

	public float GetDelta()
	{
		return deltaAttribute.GetTotalValue();
	}

	public AmountInstance(Amount amount, GameObject game_object)
		: base(game_object, amount)
	{
		Attributes attributes = game_object.GetAttributes();
		minAttribute = attributes.Add(amount.minAttribute);
		maxAttribute = attributes.Add(amount.maxAttribute);
		deltaAttribute = attributes.Add(amount.deltaAttribute);
	}

	public float SetValue(float value)
	{
		this.value = Mathf.Min(Mathf.Max(value, GetMin()), GetMax());
		return this.value;
	}

	public void Publish(float delta, float previous_value)
	{
		if (OnDelta != null)
		{
			OnDelta(delta);
		}
		if (OnMaxValueReached != null && previous_value < GetMax() && value >= GetMax())
		{
			OnMaxValueReached();
		}
	}

	public float ApplyDelta(float delta)
	{
		float previous_value = value;
		SetValue(value + delta);
		Publish(delta, previous_value);
		return value;
	}

	public string GetValueString()
	{
		return amount.GetValueString(this);
	}

	public string GetDescription()
	{
		return amount.GetDescription(this);
	}

	public string GetTooltip()
	{
		return amount.GetTooltip(this);
	}

	public Sprite GetSprite()
	{
		return amount.GetSprite(this);
	}

	public void Activate()
	{
		SimAndRenderScheduler.instance.Add(this);
	}

	public void Sim200ms(float dt)
	{
	}

	public static void BatchUpdate(List<UpdateBucketWithUpdater<ISim200ms>.Entry> amount_instances, float time_delta)
	{
		if (time_delta != 0f)
		{
			BatchUpdateContext context = new BatchUpdateContext(amount_instances, time_delta);
			AmmountInstanceBatchUpdateDispatcher.Instance.Reset(context);
			GlobalJobManager.Run(AmmountInstanceBatchUpdateDispatcher.Instance);
			AmmountInstanceBatchUpdateDispatcher.Instance.Finish();
			AmmountInstanceBatchUpdateDispatcher.Instance.Reset(BatchUpdateContext.EmptyContext);
		}
	}

	public void Deactivate()
	{
		SimAndRenderScheduler.instance.Remove(this);
	}
}
