using System;
using System.Collections.Generic;
using Klei.AI;

namespace Database;

public class Personalities : ResourceSet<Personality>
{
	public class PersonalityLoader : AsyncCsvLoader<PersonalityLoader, PersonalityInfo>
	{
		public PersonalityLoader()
			: base(Assets.instance.personalitiesFile)
		{
		}

		public override void Run()
		{
			base.Run();
		}
	}

	public class PersonalityInfo : Resource
	{
		public int HeadShape;

		public int Mouth;

		public int Neck;

		public int Eyes;

		public int Hair;

		public int Body;

		public int Belt;

		public int Cuff;

		public int Foot;

		public int Hand;

		public int Pelvis;

		public int Leg;

		public int Arm_Skin;

		public int Leg_Skin;

		public int SpeechMouth;

		public string Gender;

		public string PersonalityType;

		public string StressTrait;

		public string JoyTrait;

		public string StickerType;

		public string CongenitalTrait;

		public string Design;

		public bool ValidStarter;

		public string Grave;

		public string Model;

		public string RequiredDlcId;
	}

	public Personalities()
	{
		PersonalityInfo[] entries = AsyncLoadManager<IGlobalAsyncLoader>.AsyncLoader<PersonalityLoader>.Get().entries;
		foreach (PersonalityInfo personalityInfo in entries)
		{
			if (string.IsNullOrEmpty(personalityInfo.RequiredDlcId) || DlcManager.IsContentSubscribed(personalityInfo.RequiredDlcId) || IsCosmeticPersonality(personalityInfo.RequiredDlcId))
			{
				Personality personality = new Personality(personalityInfo.Name.ToUpper(), Strings.Get($"STRINGS.DUPLICANTS.PERSONALITIES.{personalityInfo.Name.ToUpper()}.NAME"), personalityInfo.Gender.ToUpper(), personalityInfo.PersonalityType, personalityInfo.StressTrait, personalityInfo.JoyTrait, personalityInfo.StickerType, personalityInfo.CongenitalTrait, personalityInfo.HeadShape, personalityInfo.Mouth, personalityInfo.Neck, personalityInfo.Eyes, personalityInfo.Hair, personalityInfo.Body, personalityInfo.Belt, personalityInfo.Cuff, personalityInfo.Foot, personalityInfo.Hand, personalityInfo.Pelvis, personalityInfo.Leg, personalityInfo.Leg_Skin, personalityInfo.Arm_Skin, Strings.Get($"STRINGS.DUPLICANTS.PERSONALITIES.{personalityInfo.Name.ToUpper()}.DESC"), personalityInfo.ValidStarter, personalityInfo.Grave, personalityInfo.Model, personalityInfo.SpeechMouth)
				{
					requiredDlcId = personalityInfo.RequiredDlcId
				};
				if (!DlcManager.IsContentSubscribed(personalityInfo.RequiredDlcId) && IsCosmeticPersonality(personalityInfo.RequiredDlcId))
				{
					personality.Disabled = true;
				}
				Add(personality);
			}
		}
	}

	private void AddTrait(Personality personality, string trait_name)
	{
		Trait trait = Db.Get().traits.TryGet(trait_name);
		if (trait != null)
		{
			personality.AddTrait(trait);
		}
	}

	private void SetAttribute(Personality personality, string attribute_name, int value)
	{
		Klei.AI.Attribute attribute = Db.Get().Attributes.TryGet(attribute_name);
		if (attribute == null)
		{
			Debug.LogWarning("Attribute does not exist: " + attribute_name);
		}
		else
		{
			personality.SetAttribute(attribute, value);
		}
	}

	public List<Personality> GetStartingPersonalities()
	{
		return resources.FindAll((Personality x) => x.startingMinion);
	}

	public List<Personality> GetAll(bool onlyEnabledMinions, bool onlyStartingMinions)
	{
		return resources.FindAll(delegate(Personality personality)
		{
			if (onlyStartingMinions && !personality.startingMinion)
			{
				return false;
			}
			if (onlyEnabledMinions && personality.Disabled)
			{
				return false;
			}
			return (!(Game.Instance != null) || Game.IsDlcActiveForCurrentSave(personality.requiredDlcId)) ? true : false;
		});
	}

	public Personality GetRandom(bool onlyEnabledMinions, bool onlyStartingMinions)
	{
		return GetAll(onlyEnabledMinions, onlyStartingMinions).GetRandom();
	}

	public Personality GetRandom(Tag model, bool onlyEnabledMinions, bool onlyStartingMinions)
	{
		return GetAll(onlyEnabledMinions, onlyStartingMinions).FindAll((Personality personality) => personality.model == model || model == null).GetRandom();
	}

	public Personality GetRandom(List<Tag> models, bool onlyEnabledMinions, bool onlyStartingMinions)
	{
		return GetAll(onlyEnabledMinions, onlyStartingMinions).FindAll((Personality personality) => models.Contains(personality.model)).GetRandom();
	}

	public Personality GetPersonalityFromNameStringKey(string name_string_key)
	{
		foreach (Personality resource in Db.Get().Personalities.resources)
		{
			if (resource.nameStringKey.Equals(name_string_key, StringComparison.CurrentCultureIgnoreCase))
			{
				return resource;
			}
		}
		return null;
	}

	private bool IsCosmeticPersonality(string required_dlc_id)
	{
		if (!string.IsNullOrEmpty(required_dlc_id))
		{
			return DlcManager.CONTENT_ONLY_DLC_IDS.Contains(required_dlc_id);
		}
		return false;
	}
}
