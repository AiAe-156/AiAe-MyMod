using System.Collections.Generic;
using Klei.AI;
using UnityEngine;

public class CurrentJobConversation : ConversationType
{
	public static Dictionary<Conversation.ModeType, List<Conversation.ModeType>> transitions = new Dictionary<Conversation.ModeType, List<Conversation.ModeType>>
	{
		{
			Conversation.ModeType.Query,
			new List<Conversation.ModeType> { Conversation.ModeType.Statement }
		},
		{
			Conversation.ModeType.Satisfaction,
			new List<Conversation.ModeType> { Conversation.ModeType.Agreement }
		},
		{
			Conversation.ModeType.Nominal,
			new List<Conversation.ModeType> { Conversation.ModeType.Musing }
		},
		{
			Conversation.ModeType.Dissatisfaction,
			new List<Conversation.ModeType> { Conversation.ModeType.Disagreement }
		},
		{
			Conversation.ModeType.Stressing,
			new List<Conversation.ModeType> { Conversation.ModeType.Disagreement }
		},
		{
			Conversation.ModeType.Agreement,
			new List<Conversation.ModeType>
			{
				Conversation.ModeType.Query,
				Conversation.ModeType.End
			}
		},
		{
			Conversation.ModeType.Disagreement,
			new List<Conversation.ModeType>
			{
				Conversation.ModeType.Query,
				Conversation.ModeType.End
			}
		},
		{
			Conversation.ModeType.Musing,
			new List<Conversation.ModeType>
			{
				Conversation.ModeType.Query,
				Conversation.ModeType.End
			}
		}
	};

	public CurrentJobConversation()
	{
		id = "CurrentJobConversation";
	}

	public override void NewTarget(MinionIdentity speaker)
	{
		target = "hows_role";
	}

	public override Conversation.Topic GetNextTopic(MinionIdentity speaker, Conversation.Topic lastTopic)
	{
		if (lastTopic == null)
		{
			return new Conversation.Topic(target, Conversation.ModeType.Query);
		}
		List<Conversation.ModeType> list = transitions[lastTopic.mode];
		Conversation.ModeType modeType = list[Random.Range(0, list.Count)];
		if (modeType == Conversation.ModeType.Statement)
		{
			target = GetRoleForSpeaker(speaker);
			Conversation.ModeType modeForRole = GetModeForRole(speaker, target);
			return new Conversation.Topic(target, modeForRole);
		}
		return new Conversation.Topic(target, modeType);
	}

	public override Sprite GetSprite(string topic)
	{
		if (topic == "hows_role")
		{
			return Assets.GetSprite("crew_state_role");
		}
		if (Db.Get().Skills.TryGet(topic) != null)
		{
			return Assets.GetSprite(Db.Get().Skills.Get(topic).hat);
		}
		return null;
	}

	private unsafe Conversation.ModeType GetModeForRole(MinionIdentity speaker, string roleId)
	{
		if (!speaker.TryGetComponent<MinionResume>(out var _))
		{
			return Conversation.ModeType.Nominal;
		}
		AttributeInstance attributeInstance = Db.Get().Attributes.QualityOfLife.Lookup(speaker);
		if (attributeInstance == null)
		{
			return Conversation.ModeType.Nominal;
		}
		AttributeInstance attributeInstance2 = Db.Get().Attributes.QualityOfLifeExpectation.Lookup(speaker);
		if (attributeInstance2 == null)
		{
			return Conversation.ModeType.Nominal;
		}
		float totalValue = attributeInstance2.GetTotalValue();
		if (totalValue <= 0f)
		{
			return Conversation.ModeType.Nominal;
		}
		float* ptr = stackalloc float[3] { 0.5f, 0.25f, 0.25f };
		float num = attributeInstance.GetTotalValue() / totalValue;
		for (int i = 0; i != 3; i++)
		{
			float num2 = ptr[i];
			num -= num2;
			if (num < 0f)
			{
				switch (i)
				{
				case 0:
					return Conversation.ModeType.Stressing;
				case 1:
					return Conversation.ModeType.Dissatisfaction;
				case 2:
					return Conversation.ModeType.Nominal;
				}
			}
		}
		return Conversation.ModeType.Satisfaction;
	}

	private string GetRoleForSpeaker(MinionIdentity speaker)
	{
		return speaker.GetComponent<MinionResume>().CurrentRole;
	}
}
