using System;
using System.Collections.Generic;
using KSerialization;
using Klei.AI;
using Klei.CustomSettings;
using STRINGS;
using TUNING;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/MinionIdentity")]
public class MinionIdentity : KMonoBehaviour, ISaveLoadable, IAssignableIdentity, IListableOption, ISim1000ms
{
	private class NameList
	{
		private List<string> names = new List<string>();

		private int idx;

		public NameList(TextAsset file)
		{
			string[] array = file.text.Replace("  ", " ").Replace("\r\n", "\n").Split('\n');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(' ');
				if (array2[^1] != "" && array2[^1] != null)
				{
					names.Add(array2[^1]);
				}
			}
			names.Shuffle();
		}

		public string Next()
		{
			return names[idx++ % names.Count];
		}
	}

	public const string HairAlwaysSymbol = "snapto_hair_always";

	[MyCmpReq]
	private KSelectable selectable;

	[MyCmpReq]
	public Modifiers modifiers;

	public int femaleVoiceCount;

	public int maleVoiceCount;

	[Serialize]
	public Tag model;

	[Serialize]
	private new string name;

	[Serialize]
	public string gender;

	[Serialize]
	public string stickerType;

	[Serialize]
	[ReadOnly]
	public float arrivalTime;

	[Serialize]
	public int voiceIdx;

	[Serialize]
	public Ref<MinionAssignablesProxy> assignableProxy;

	private Navigator navigator;

	private ChoreDriver choreDriver;

	public float timeLastSpoke;

	private string voiceId;

	private KAnimHashedString overrideExpression;

	private KAnimHashedString expression;

	public bool addToIdentityList = true;

	private static NameList maleNameList;

	private static NameList femaleNameList;

	private static readonly EventSystem.IntraObjectHandler<MinionIdentity> OnDeadTagAddedDelegate = GameUtil.CreateHasTagHandler(GameTags.Dead, delegate(MinionIdentity component, object data)
	{
		component.OnDied(data);
	});

	private static readonly EventSystem.IntraObjectHandler<MinionIdentity> OnQueueDestroyObjectDelegate = new EventSystem.IntraObjectHandler<MinionIdentity>(delegate(MinionIdentity component, object data)
	{
		component.OnQueueDestroyObject();
	});

	[Serialize]
	public string genderStringKey { get; set; }

	[Serialize]
	public string nameStringKey { get; set; }

	[Serialize]
	public HashedString personalityResourceId { get; set; }

	public static void DestroyStatics()
	{
		maleNameList = null;
		femaleNameList = null;
	}

	protected override void OnPrefabInit()
	{
		if (name == null)
		{
			name = ChooseRandomName();
		}
		if (GameClock.Instance != null)
		{
			arrivalTime = GameClock.Instance.GetCycle();
		}
		KAnimControllerBase component = GetComponent<KAnimControllerBase>();
		if (component != null)
		{
			component.OnUpdateBounds = (Action<Bounds>)Delegate.Combine(component.OnUpdateBounds, new Action<Bounds>(OnUpdateBounds));
		}
		GameUtil.SubscribeToTags(this, OnDeadTagAddedDelegate, triggerImmediately: true);
		Subscribe(1502190696, OnQueueDestroyObjectDelegate);
	}

	protected override void OnSpawn()
	{
		if (addToIdentityList)
		{
			ValidateProxy();
			CleanupLimboMinions();
		}
		Navigator component = GetComponent<Navigator>();
		if (component != null)
		{
			component.reportOccupation = true;
		}
		SetName(name);
		if (nameStringKey == null)
		{
			nameStringKey = name;
		}
		SetGender(gender);
		if (genderStringKey == null)
		{
			genderStringKey = "NB";
		}
		if (personalityResourceId == HashedString.Invalid)
		{
			Personality personalityFromNameStringKey = Db.Get().Personalities.GetPersonalityFromNameStringKey(nameStringKey);
			if (personalityFromNameStringKey != null)
			{
				personalityResourceId = personalityFromNameStringKey.Id;
			}
		}
		if (!model.IsValid)
		{
			Personality personality = Db.Get().Personalities.Get(personalityResourceId);
			if (personality != null)
			{
				model = personality.model;
			}
		}
		if (addToIdentityList)
		{
			Components.MinionIdentities.Add(this);
			if (!Components.MinionIdentitiesByModel.ContainsKey(model))
			{
				Components.MinionIdentitiesByModel[model] = new Components.Cmps<MinionIdentity>();
			}
			Components.MinionIdentitiesByModel[model].Add(this);
			if (!base.gameObject.HasTag(GameTags.Dead))
			{
				Components.LiveMinionIdentities.Add(this);
				if (!Components.LiveMinionIdentitiesByModel.ContainsKey(model))
				{
					Components.LiveMinionIdentitiesByModel[model] = new Components.Cmps<MinionIdentity>();
				}
				Components.LiveMinionIdentitiesByModel[model].Add(this);
				Game.Instance.Trigger(2144209314, (object)this);
			}
		}
		SymbolOverrideController component2 = GetComponent<SymbolOverrideController>();
		if (component2 != null)
		{
			Accessorizer component3 = base.gameObject.GetComponent<Accessorizer>();
			if (component3 != null)
			{
				string text = HashCache.Get().Get(component3.GetAccessory(Db.Get().AccessorySlots.Mouth).symbol.hash);
				string text2 = text.Replace("mouth", "cheek");
				component2.AddSymbolOverride("snapto_cheek", Assets.GetAnim("head_swap_kanim").GetData().build.GetSymbol(text2), 1);
				component2.AddSymbolOverride("snapto_hair_always", component3.GetAccessory(Db.Get().AccessorySlots.Hair).symbol, 1);
				component2.AddSymbolOverride(Db.Get().AccessorySlots.HatHair.targetSymbolId, Db.Get().AccessorySlots.HatHair.Lookup("hat_" + HashCache.Get().Get(component3.GetAccessory(Db.Get().AccessorySlots.Hair).symbol.hash)).symbol, 1);
			}
		}
		voiceId = (voiceIdx + 1).ToString("D2");
		Prioritizable component4 = GetComponent<Prioritizable>();
		if (component4 != null)
		{
			component4.showIcon = false;
		}
		Pickupable component5 = GetComponent<Pickupable>();
		if (component5 != null)
		{
			component5.carryAnimOverride = Assets.GetAnim("anim_incapacitated_carrier_kanim");
		}
		ApplyCustomGameSettings();
	}

	public void ValidateProxy()
	{
		assignableProxy = MinionAssignablesProxy.InitAssignableProxy(assignableProxy, this);
	}

	private void CleanupLimboMinions()
	{
		KPrefabID component = GetComponent<KPrefabID>();
		if (component.InstanceID == -1)
		{
			DebugUtil.LogWarningArgs("Minion with an invalid kpid! Attempting to recover...", name);
			if (KPrefabIDTracker.Get().GetInstance(component.InstanceID) != null)
			{
				KPrefabIDTracker.Get().Unregister(component);
			}
			component.InstanceID = KPrefabID.GetUniqueID();
			KPrefabIDTracker.Get().Register(component);
			DebugUtil.LogWarningArgs("Restored as:", component.InstanceID);
		}
		if (component.conflicted)
		{
			DebugUtil.LogWarningArgs("Minion with a conflicted kpid! Attempting to recover... ", component.InstanceID, name);
			if (KPrefabIDTracker.Get().GetInstance(component.InstanceID) != null)
			{
				KPrefabIDTracker.Get().Unregister(component);
			}
			component.InstanceID = KPrefabID.GetUniqueID();
			KPrefabIDTracker.Get().Register(component);
			DebugUtil.LogWarningArgs("Restored as:", component.InstanceID);
		}
		assignableProxy.Get().SetTarget(this, base.gameObject);
	}

	public string GetProperName()
	{
		return selectable.GetProperName();
	}

	public string GetVoiceId()
	{
		return voiceId;
	}

	public void SetName(string name)
	{
		this.name = name;
		if (selectable != null)
		{
			selectable.SetName(name);
		}
		base.gameObject.name = name;
		NameDisplayScreen.Instance.UpdateName(base.gameObject);
	}

	public void SetStickerType(string stickerType)
	{
		this.stickerType = stickerType;
	}

	public bool IsNull()
	{
		return this == null;
	}

	public void SetGender(string gender)
	{
		this.gender = gender;
		selectable.SetGender(gender);
	}

	public static string ChooseRandomName()
	{
		if (femaleNameList == null)
		{
			maleNameList = new NameList(Game.Instance.maleNamesFile);
			femaleNameList = new NameList(Game.Instance.femaleNamesFile);
		}
		if (UnityEngine.Random.value > 0.5f)
		{
			return maleNameList.Next();
		}
		return femaleNameList.Next();
	}

	private void OnQueueDestroyObject()
	{
		RemoveFromComponentsLists();
	}

	private void RemoveFromComponentsLists()
	{
		Components.MinionIdentities.Remove(this);
		if (Components.MinionIdentitiesByModel.ContainsKey(model))
		{
			Components.MinionIdentitiesByModel[model].Remove(this);
		}
		Components.LiveMinionIdentities.Remove(this);
		if (Components.LiveMinionIdentitiesByModel.ContainsKey(model))
		{
			Components.LiveMinionIdentitiesByModel[model].Remove(this);
		}
	}

	protected override void OnCleanUp()
	{
		if (assignableProxy != null)
		{
			MinionAssignablesProxy minionAssignablesProxy = assignableProxy.Get();
			if ((bool)minionAssignablesProxy && minionAssignablesProxy.target == this)
			{
				Util.KDestroyGameObject(minionAssignablesProxy.gameObject);
			}
		}
		RemoveFromComponentsLists();
		Game.Instance.Trigger(2144209314, (object)this);
	}

	private void OnUpdateBounds(Bounds bounds)
	{
		KBoxCollider2D component = GetComponent<KBoxCollider2D>();
		component.offset = bounds.center;
		component.size = bounds.extents;
	}

	private void OnDied(object data)
	{
		GetSoleOwner().UnassignAll();
		GetEquipment().UnequipAll();
		Components.LiveMinionIdentities.Remove(this);
		if (Components.LiveMinionIdentitiesByModel.ContainsKey(model))
		{
			Components.LiveMinionIdentitiesByModel[model].Remove(this);
		}
		Game.Instance.Trigger(-1523247426, (object)this);
		Game.Instance.Trigger(2144209314, (object)this);
	}

	public List<Ownables> GetOwners()
	{
		return assignableProxy.Get().ownables;
	}

	public Ownables GetSoleOwner()
	{
		return assignableProxy.Get().GetComponent<Ownables>();
	}

	public bool HasOwner(Assignables owner)
	{
		return GetOwners().Contains(owner as Ownables);
	}

	public int NumOwners()
	{
		return GetOwners().Count;
	}

	public Equipment GetEquipment()
	{
		return assignableProxy.Get().GetComponent<Equipment>();
	}

	public void Sim1000ms(float dt)
	{
		if (this == null)
		{
			return;
		}
		if (navigator == null)
		{
			navigator = GetComponent<Navigator>();
		}
		if (navigator != null && !navigator.IsMoving())
		{
			return;
		}
		if (choreDriver == null)
		{
			choreDriver = GetComponent<ChoreDriver>();
		}
		if (!(choreDriver != null))
		{
			return;
		}
		Chore currentChore = choreDriver.GetCurrentChore();
		if (currentChore != null && currentChore is FetchAreaChore)
		{
			MinionResume component = GetComponent<MinionResume>();
			if (component != null)
			{
				component.AddExperienceWithAptitude(Db.Get().SkillGroups.Hauling.Id, dt, SKILLS.ALL_DAY_EXPERIENCE);
			}
		}
	}

	private void ApplyCustomGameSettings()
	{
		SettingLevel currentQualitySetting = CustomGameSettings.Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.ImmuneSystem);
		if (currentQualitySetting.id == "Compromised")
		{
			Db.Get().Attributes.DiseaseCureSpeed.Lookup(this).Add(new AttributeModifier(Db.Get().Attributes.DiseaseCureSpeed.Id, -0.3333f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.IMMUNESYSTEM.LEVELS.COMPROMISED.ATTRIBUTE_MODIFIER_NAME));
			Db.Get().Attributes.GermResistance.Lookup(this).Add(new AttributeModifier(Db.Get().Attributes.GermResistance.Id, -2f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.IMMUNESYSTEM.LEVELS.COMPROMISED.ATTRIBUTE_MODIFIER_NAME));
		}
		else if (currentQualitySetting.id == "Weak")
		{
			Db.Get().Attributes.GermResistance.Lookup(this).Add(new AttributeModifier(Db.Get().Attributes.GermResistance.Id, -1f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.IMMUNESYSTEM.LEVELS.WEAK.ATTRIBUTE_MODIFIER_NAME));
		}
		else if (currentQualitySetting.id == "Strong")
		{
			Db.Get().Attributes.DiseaseCureSpeed.Lookup(this).Add(new AttributeModifier(Db.Get().Attributes.DiseaseCureSpeed.Id, 2f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.IMMUNESYSTEM.LEVELS.STRONG.ATTRIBUTE_MODIFIER_NAME));
			Db.Get().Attributes.GermResistance.Lookup(this).Add(new AttributeModifier(Db.Get().Attributes.GermResistance.Id, 2f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.IMMUNESYSTEM.LEVELS.STRONG.ATTRIBUTE_MODIFIER_NAME));
		}
		else if (currentQualitySetting.id == "Invincible")
		{
			Db.Get().Attributes.DiseaseCureSpeed.Lookup(this).Add(new AttributeModifier(Db.Get().Attributes.DiseaseCureSpeed.Id, 100000000f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.IMMUNESYSTEM.LEVELS.INVINCIBLE.ATTRIBUTE_MODIFIER_NAME));
			Db.Get().Attributes.GermResistance.Lookup(this).Add(new AttributeModifier(Db.Get().Attributes.GermResistance.Id, 200f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.IMMUNESYSTEM.LEVELS.INVINCIBLE.ATTRIBUTE_MODIFIER_NAME));
		}
		SettingLevel currentQualitySetting2 = CustomGameSettings.Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.Stress);
		if (currentQualitySetting2.id == "Doomed")
		{
			Db.Get().Amounts.Stress.deltaAttribute.Lookup(this).Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 1f / 30f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.STRESS.LEVELS.DOOMED.ATTRIBUTE_MODIFIER_NAME));
		}
		else if (currentQualitySetting2.id == "Pessimistic")
		{
			Db.Get().Amounts.Stress.deltaAttribute.Lookup(this).Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 1f / 60f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.STRESS.LEVELS.PESSIMISTIC.ATTRIBUTE_MODIFIER_NAME));
		}
		else if (currentQualitySetting2.id == "Optimistic")
		{
			Db.Get().Amounts.Stress.deltaAttribute.Lookup(this).Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, -1f / 60f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.STRESS.LEVELS.OPTIMISTIC.ATTRIBUTE_MODIFIER_NAME));
		}
		else if (currentQualitySetting2.id == "Indomitable")
		{
			Db.Get().Amounts.Stress.deltaAttribute.Lookup(this).Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, float.NegativeInfinity, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.STRESS.LEVELS.INDOMITABLE.ATTRIBUTE_MODIFIER_NAME));
		}
		SettingLevel currentQualitySetting3 = CustomGameSettings.Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.CalorieBurn);
		if (currentQualitySetting3.id == "VeryHard")
		{
			Db.Get().Amounts.Calories.deltaAttribute.Lookup(this).Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, DUPLICANTSTATS.STANDARD.BaseStats.CALORIES_BURNED_PER_SECOND * 1f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.CALORIE_BURN.LEVELS.VERYHARD.ATTRIBUTE_MODIFIER_NAME));
		}
		else if (currentQualitySetting3.id == "Hard")
		{
			Db.Get().Amounts.Calories.deltaAttribute.Lookup(this).Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, DUPLICANTSTATS.STANDARD.BaseStats.CALORIES_BURNED_PER_SECOND * 0.5f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.CALORIE_BURN.LEVELS.HARD.ATTRIBUTE_MODIFIER_NAME));
		}
		else if (currentQualitySetting3.id == "Easy")
		{
			Db.Get().Amounts.Calories.deltaAttribute.Lookup(this).Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, DUPLICANTSTATS.STANDARD.BaseStats.CALORIES_BURNED_PER_SECOND * -0.5f, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.CALORIE_BURN.LEVELS.EASY.ATTRIBUTE_MODIFIER_NAME));
		}
		else if (currentQualitySetting3.id == "Disabled")
		{
			Db.Get().Amounts.Calories.deltaAttribute.Lookup(this).Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, float.PositiveInfinity, UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.CALORIE_BURN.LEVELS.DISABLED.ATTRIBUTE_MODIFIER_NAME));
		}
	}

	public static float GetCalorieBurnMultiplier()
	{
		float result = 1f;
		SettingLevel currentQualitySetting = CustomGameSettings.Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.CalorieBurn);
		if (currentQualitySetting.id == "VeryHard")
		{
			result = 2f;
		}
		else if (currentQualitySetting.id == "Hard")
		{
			result = 1.5f;
		}
		else if (currentQualitySetting.id == "Easy")
		{
			result = 0.5f;
		}
		else if (currentQualitySetting.id == "Disabled")
		{
			result = 0f;
		}
		return result;
	}
}
