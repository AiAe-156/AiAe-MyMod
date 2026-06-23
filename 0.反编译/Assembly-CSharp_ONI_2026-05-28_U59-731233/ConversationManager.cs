using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/ConversationManager")]
public class ConversationManager : KMonoBehaviour, ISim200ms
{
	public class Tuning : TuningData<Tuning>
	{
		public float cyclesBeforeFirstConversation;

		public float maxDistance;

		public int maxDupesPerConvo;

		public float minionCooldownTime;

		public float speakTime;

		public float delayBetweenUtterances;

		public float delayBeforeStart;

		public int maxUtterances;
	}

	public class StartedTalkingEvent
	{
		public GameObject talker;

		public string anim;
	}

	private List<Conversation> conversations;

	private Dictionary<MinionIdentity, float> lastConvoTimeByMinion;

	private readonly Dictionary<MinionIdentity, Conversation> minionConversations = new Dictionary<MinionIdentity, Conversation>();

	private List<Type> convoTypes = new List<Type>
	{
		typeof(RecentThingConversation),
		typeof(AmountStateConversation),
		typeof(CurrentJobConversation)
	};

	private static readonly Tag[] invalidConvoTags = new Tag[5]
	{
		GameTags.Asleep,
		GameTags.BionicBedTime,
		GameTags.HoldingBreath,
		GameTags.Dead,
		GameTags.SuppressConversation
	};

	protected override void OnPrefabInit()
	{
		conversations = new List<Conversation>();
		lastConvoTimeByMinion = new Dictionary<MinionIdentity, float>();
		simRenderLoadBalance = true;
	}

	public void Sim200ms(float dt)
	{
		for (int num = conversations.Count - 1; num >= 0; num--)
		{
			Conversation conversation = conversations[num];
			for (int num2 = conversation.minions.Count - 1; num2 >= 0; num2--)
			{
				MinionIdentity minionIdentity = conversation.minions[num2];
				if (!ValidMinionTags(minionIdentity) || !MinionCloseEnoughToConvo(minionIdentity, conversation))
				{
					conversation.minions.RemoveAt(num2);
					if (conversation.lastTalked == minionIdentity)
					{
						conversation.lastTalked = null;
					}
				}
				else
				{
					minionConversations[minionIdentity] = conversation;
				}
			}
			if (conversation.minions.Count <= 1)
			{
				conversations.RemoveAt(num);
			}
			else if (!(conversation.lastTalked != null) || !conversation.lastTalked.GetComponent<KPrefabID>().HasTag(GameTags.DoNotInterruptMe))
			{
				bool flag = conversation.minions.Find((MinionIdentity match) => match.HasTag(GameTags.CommunalDining)) != null;
				bool flag2 = true;
				if (!flag && conversation.numUtterances >= TuningData<Tuning>.Get().maxUtterances)
				{
					flag2 = false;
				}
				else
				{
					bool flag3 = conversation.numUtterances == 0;
					bool flag4 = conversation.minions.Find((MinionIdentity match) => !match.HasTag(GameTags.Partying)) == null;
					float num3 = (flag3 ? TuningData<Tuning>.Get().delayBeforeStart : TuningData<Tuning>.Get().delayBetweenUtterances);
					if (flag4)
					{
						num3 = 0f;
					}
					float num4 = (flag3 ? 0f : TuningData<Tuning>.Get().speakTime);
					if (flag4)
					{
						num4 /= 4f;
					}
					num4 += num3;
					if (GameClock.Instance.GetTime() > conversation.lastTalkedTime + num4)
					{
						flag2 = TryContinueConversation(conversation, flag3);
					}
				}
				if (!flag2)
				{
					conversations.RemoveAt(num);
				}
			}
		}
		foreach (MinionIdentity item in Components.LiveMinionIdentities.Items)
		{
			if (!ValidMinionTags(item) || minionConversations.ContainsKey(item) || MinionOnCooldown(item))
			{
				continue;
			}
			foreach (MinionIdentity item2 in Components.LiveMinionIdentities.Items)
			{
				if (item2 == item || !ValidMinionTags(item2))
				{
					continue;
				}
				if (minionConversations.TryGetValue(item2, out var value))
				{
					if (value.minions.Count < TuningData<Tuning>.Get().maxDupesPerConvo)
					{
						Vector3 centroid = GetCentroid(value);
						float magnitude = (centroid - item.transform.GetPosition()).magnitude;
						if (!(magnitude >= TuningData<Tuning>.Get().maxDistance * 0.5f))
						{
							value.minions.Add(item);
							minionConversations[item] = value;
							break;
						}
					}
				}
				else if (!MinionOnCooldown(item2))
				{
					float magnitude2 = (item2.transform.GetPosition() - item.transform.GetPosition()).magnitude;
					if (!(magnitude2 >= TuningData<Tuning>.Get().maxDistance))
					{
						value = new Conversation();
						value.minions.Add(item);
						value.minions.Add(item2);
						Type type = convoTypes[UnityEngine.Random.Range(0, convoTypes.Count)];
						value.conversationType = (ConversationType)Activator.CreateInstance(type);
						value.lastTalkedTime = GameClock.Instance.GetTime();
						conversations.Add(value);
						minionConversations[item] = value;
						minionConversations[item2] = value;
						break;
					}
				}
			}
		}
		minionConversations.Clear();
	}

	private bool TryContinueConversation(Conversation conversation, bool isOpeningLine)
	{
		ListPool<int, ConversationManager>.PooledList pooledList = ListPool<int, ConversationManager>.Allocate();
		int num = -1;
		pooledList.Capacity = Math.Max(pooledList.Capacity, conversation.minions.Count);
		for (int i = 0; i != conversation.minions.Count; i++)
		{
			if (conversation.minions[i] == conversation.lastTalked)
			{
				num = i;
			}
			else
			{
				pooledList.Add(i);
			}
		}
		pooledList.Shuffle();
		if (num != -1)
		{
			pooledList.Add(num);
		}
		if (isOpeningLine)
		{
			MinionIdentity speaker = conversation.minions[pooledList[0]];
			conversation.conversationType.NewTarget(speaker);
		}
		bool result = false;
		foreach (int item in pooledList)
		{
			MinionIdentity new_speaker = conversation.minions[item];
			if (DoTalking(conversation, new_speaker))
			{
				result = true;
				break;
			}
		}
		pooledList.Recycle();
		return result;
	}

	private bool DoTalking(Conversation conversation, MinionIdentity new_speaker)
	{
		DebugUtil.Assert(conversation != null, "conversation was null");
		DebugUtil.Assert(new_speaker != null, "new_speaker was null");
		DebugUtil.Assert(conversation.conversationType != null, "conversation.conversationType was null");
		Conversation.Topic nextTopic = conversation.conversationType.GetNextTopic(new_speaker, conversation.lastTopic);
		if (nextTopic == null || nextTopic.mode == Conversation.ModeType.End)
		{
			return false;
		}
		Thought thoughtForTopic = GetThoughtForTopic(conversation, nextTopic);
		if (thoughtForTopic == null)
		{
			return false;
		}
		ThoughtGraph.Instance sMI = new_speaker.GetSMI<ThoughtGraph.Instance>();
		if (sMI == null)
		{
			return false;
		}
		if (conversation.lastTalked != null)
		{
			conversation.lastTalked.Trigger(25860745, (object)conversation.lastTalked.gameObject);
		}
		sMI.AddThought(thoughtForTopic);
		conversation.lastTopic = nextTopic;
		conversation.lastTalked = new_speaker;
		conversation.lastTalkedTime = GameClock.Instance.GetTime();
		DebugUtil.Assert(lastConvoTimeByMinion != null, "lastConvoTimeByMinion was null");
		lastConvoTimeByMinion[conversation.lastTalked] = GameClock.Instance.GetTime();
		Effects component = conversation.lastTalked.GetComponent<Effects>();
		DebugUtil.Assert(component != null, "effects was null");
		component.Add("GoodConversation", should_save: true);
		Conversation.Mode mode = Conversation.Topic.Modes[(int)nextTopic.mode];
		DebugUtil.Assert(mode != null, "mode was null");
		StartedTalkingEvent data = new StartedTalkingEvent
		{
			talker = new_speaker.gameObject,
			anim = mode.anim
		};
		foreach (MinionIdentity minion in conversation.minions)
		{
			if (!minion)
			{
				DebugUtil.DevAssert(test: false, "minion in conversation.minions was null");
			}
			else
			{
				minion.Trigger(-594200555, (object)data);
			}
		}
		conversation.numUtterances++;
		return true;
	}

	public bool TryGetConversation(MinionIdentity minion, out Conversation conversation)
	{
		return minionConversations.TryGetValue(minion, out conversation);
	}

	private Vector3 GetCentroid(Conversation conversation)
	{
		Vector3 zero = Vector3.zero;
		foreach (MinionIdentity minion in conversation.minions)
		{
			if (!(minion == null))
			{
				zero += minion.transform.GetPosition();
			}
		}
		return zero / conversation.minions.Count;
	}

	private Thought GetThoughtForTopic(Conversation conversation, Conversation.Topic topic)
	{
		if (string.IsNullOrEmpty(topic.topic))
		{
			DebugUtil.DevAssert(test: false, "topic.topic was null");
			return null;
		}
		Sprite sprite = conversation.conversationType.GetSprite(topic.topic);
		if (sprite != null)
		{
			Conversation.Mode mode = Conversation.Topic.Modes[(int)topic.mode];
			return new Thought("Topic_" + topic.topic, null, sprite, mode.icon, mode.voice, "bubble_chatter", mode.mouth, DUPLICANTS.THOUGHTS.CONVERSATION.TOOLTIP, show_immediately: true, TuningData<Tuning>.Get().speakTime);
		}
		return null;
	}

	private bool ValidMinionTags(MinionIdentity minion)
	{
		if (minion == null)
		{
			return false;
		}
		KPrefabID component = minion.GetComponent<KPrefabID>();
		return !component.HasAnyTags(invalidConvoTags);
	}

	private bool MinionCloseEnoughToConvo(MinionIdentity minion, Conversation conversation)
	{
		Vector3 centroid = GetCentroid(conversation);
		float magnitude = (centroid - minion.transform.GetPosition()).magnitude;
		return magnitude < TuningData<Tuning>.Get().maxDistance * 0.5f;
	}

	private bool MinionOnCooldown(MinionIdentity minion)
	{
		KPrefabID component = minion.GetComponent<KPrefabID>();
		if (component.HasTag(GameTags.AlwaysConverse))
		{
			return false;
		}
		if (!lastConvoTimeByMinion.TryGetValue(minion, out var value))
		{
			return false;
		}
		float num = GameClock.Instance.GetTime() - TuningData<Tuning>.Get().minionCooldownTime;
		return value > num;
	}
}
