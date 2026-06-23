using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Database;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterContainer : KScreen, ITelepadDeliverableContainer
{
	[Serializable]
	public struct ProfessionIcon
	{
		public string professionName;

		public Sprite iconImg;
	}

	private struct PortraitBgAnimInfo
	{
		public string animFileName;

		public string foregroundAnimFileName;

		public bool hasPreAnim;
	}

	private class MinionModelOption : IListableOption
	{
		private string properName;

		public List<Tag> permittedModels;

		public Sprite sprite;

		public MinionModelOption(string name, List<Tag> permittedModels, Sprite sprite)
		{
			properName = name;
			this.permittedModels = permittedModels;
			this.sprite = sprite;
		}

		public string GetProperName()
		{
			return properName;
		}
	}

	public const string SHUFFLE_BUTTON_DEFAULT_SOUND_NAME_ON_USE = "DupeShuffle";

	public const string SHUFFLE_BUTTON_BIONIC_SOUND_NAME_ON_USE = "DupeShuffle_bionic";

	private static readonly Dictionary<int, string> defaultShirtIdxToDefaultOutfitID = new Dictionary<int, string>
	{
		{ 1, "StandardRed" },
		{ 2, "StandardBlue" },
		{ 3, "StandardYellow" },
		{ 4, "StandardGreen" },
		{ 5, "permit_standard_bionic_outfit" },
		{ 414842661, "permit_standard_regal_neutronium_outfit" },
		{ 890344243, "permit_standard_swim_outfit" }
	};

	[SerializeField]
	private GameObject contentBody;

	[SerializeField]
	private LocText characterName;

	[SerializeField]
	private EditableTitleBar characterNameTitle;

	[SerializeField]
	private LocText characterJob;

	[SerializeField]
	private LocText traitHeaderLabel;

	public GameObject selectedBorder;

	[SerializeField]
	private Image titleBar;

	[SerializeField]
	private Color selectedTitleColor;

	[SerializeField]
	private Color deselectedTitleColor;

	[SerializeField]
	private KButton reshuffleButton;

	private KBatchedAnimController animController;

	[SerializeField]
	private KBatchedAnimController bgAnimController;

	[SerializeField]
	private KBatchedAnimController fgAnimController;

	[SerializeField]
	private GameObject iconGroup;

	private List<GameObject> iconGroups;

	[SerializeField]
	private LocText goodTrait;

	[SerializeField]
	private LocText badTrait;

	[SerializeField]
	private GameObject aptitudeContainer;

	[SerializeField]
	private GameObject aptitudeEntry;

	[SerializeField]
	private Transform aptitudeLabel;

	[SerializeField]
	private Transform attributeLabelAptitude;

	[SerializeField]
	private Transform attributeLabelTrait;

	[SerializeField]
	private LocText expectationRight;

	private List<LocText> expectationLabels;

	[SerializeField]
	private DropDown archetypeDropDown;

	[SerializeField]
	private Image selectedArchetypeIcon;

	[SerializeField]
	private Sprite noArchetypeIcon;

	[SerializeField]
	private Sprite dropdownArrowIcon;

	private string guaranteedAptitudeID;

	private List<GameObject> aptitudeEntries;

	private List<GameObject> traitEntries;

	[SerializeField]
	private LocText description;

	[SerializeField]
	private Image selectedModelIcon;

	[SerializeField]
	private DropDown modelDropDown;

	[SerializeField]
	private HierarchyReferences outfitSelectorReferences;

	private List<Tag> permittedModels = new List<Tag>
	{
		GameTags.Minions.Models.Standard,
		GameTags.Minions.Models.Bionic
	};

	[SerializeField]
	private KToggle selectButton;

	[SerializeField]
	private KBatchedAnimController fxAnim;

	private string allModelSprite = "ui_duplicant_any_selection";

	private static Dictionary<Tag, PortraitBgAnimInfo> portraitBGAnimsByModel = new Dictionary<Tag, PortraitBgAnimInfo>
	{
		{
			GameTags.Minions.Models.Standard,
			new PortraitBgAnimInfo
			{
				animFileName = "crewselect_backdrop_kanim",
				hasPreAnim = false,
				foregroundAnimFileName = ""
			}
		},
		{
			GameTags.Minions.Models.Bionic,
			new PortraitBgAnimInfo
			{
				animFileName = "updated_crewSelect_bionic_backdrop_kanim",
				hasPreAnim = false,
				foregroundAnimFileName = ""
			}
		}
	};

	private MinionStartingStats stats;

	private CharacterSelectionController controller;

	private static List<CharacterContainer> containers;

	private KAnimFile idle_anim;

	[HideInInspector]
	public bool addMinionToIdentityList = true;

	[SerializeField]
	private Sprite enabledSpr;

	[SerializeField]
	private KScrollRect scroll_rect;

	private static readonly Dictionary<HashedString, string[]> traitForcedIdleAnims = new Dictionary<HashedString, string[]> { 
	{
		"character_select_swim_kanim",
		new string[2] { "GrantSkill_Swimming", "GrantSkill_Swimming2" }
	} };

	private static readonly Dictionary<HashedString, string[]> traitIdleAnims = new Dictionary<HashedString, string[]>
	{
		{
			"anim_idle_food_kanim",
			new string[1] { "Foodie" }
		},
		{
			"anim_idle_animal_lover_kanim",
			new string[1] { "RanchingUp" }
		},
		{
			"anim_idle_loner_kanim",
			new string[1] { "Loner" }
		},
		{
			"anim_idle_mole_hands_kanim",
			new string[1] { "MoleHands" }
		},
		{
			"anim_idle_buff_kanim",
			new string[1] { "StrongArm" }
		},
		{
			"anim_idle_distracted_kanim",
			new string[4] { "CantResearch", "CantBuild", "CantCook", "CantDig" }
		},
		{
			"anim_idle_coaster_kanim",
			new string[1] { "HappySinger" }
		}
	};

	private List<Tag> allMinionModels = new List<Tag>
	{
		GameTags.Minions.Models.Standard,
		GameTags.Minions.Models.Bionic
	};

	private static readonly HashedString[] idleAnims = new HashedString[6] { "anim_idle_healthy_kanim", "anim_idle_susceptible_kanim", "anim_idle_keener_kanim", "anim_idle_fastfeet_kanim", "anim_idle_breatherdeep_kanim", "anim_idle_breathershallow_kanim" };

	public float baseCharacterScale = 0.38f;

	private List<Option<ClothingOutfitTarget>> allAvailableClothingOutfits;

	private int outfitSelectorIndex = 0;

	private bool outfitSelectorExpanded = false;

	public MinionStartingStats Stats => stats;

	public event Action<CharacterContainer> OnReshuffled;

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		allAvailableClothingOutfits = new List<Option<ClothingOutfitTarget>>();
		foreach (ClothingOutfitTarget item in from outfit in ClothingOutfitTarget.GetAllTemplates()
			where outfit.OutfitType == ClothingOutfitUtility.OutfitType.Clothing
			select outfit)
		{
			bool flag = false;
			string[] array = item.ReadItems();
			foreach (string id in array)
			{
				ClothingItemResource clothingItemResource = Db.Get().Permits.ClothingItems.TryGet(id);
				if (clothingItemResource != null && !clothingItemResource.IsUnlocked())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				allAvailableClothingOutfits.Add(item);
			}
		}
		Initialize();
		characterNameTitle.OnStartedEditing += OnStartedEditing;
		characterNameTitle.OnNameChanged += OnNameChanged;
		reshuffleButton.onClick += delegate
		{
			Reshuffle(is_starter: true);
		};
		List<IListableOption> list = new List<IListableOption>();
		List<SkillGroup> list2 = new List<SkillGroup>(Db.Get().SkillGroups.resources);
		foreach (SkillGroup item2 in list2)
		{
			if (item2.allowAsAptitude)
			{
				list.Add(item2);
			}
		}
		archetypeDropDown.Initialize(list, OnArchetypeEntryClick, archetypeDropDownSort, archetypeDropEntryRefreshAction, displaySelectedValueWhenClosed: false);
		archetypeDropDown.CustomizeEmptyRow(Strings.Get("STRINGS.UI.CHARACTERCONTAINER_NOARCHETYPESELECTED"), noArchetypeIcon);
		List<IListableOption> contentKeys = new List<IListableOption>
		{
			new MinionModelOption(DUPLICANTS.MODEL.STANDARD.NAME, new List<Tag> { GameTags.Minions.Models.Standard }, Assets.GetSprite("ui_duplicant_minion_selection")),
			new MinionModelOption(DUPLICANTS.MODEL.BIONIC.NAME, new List<Tag> { GameTags.Minions.Models.Bionic }, Assets.GetSprite("ui_duplicant_bionicminion_selection"))
		};
		modelDropDown.Initialize(contentKeys, OnModelEntryClick, modelDropDownSort, modelDropEntryRefreshAction);
		modelDropDown.CustomizeEmptyRow(UI.CHARACTERCONTAINER_ALL_MODELS, Assets.GetSprite(allModelSprite));
		StartCoroutine(DelayedGeneration());
	}

	public void ForceStopEditingTitle()
	{
		characterNameTitle.ForceStopEditing();
	}

	public override float GetSortKey()
	{
		return 50f;
	}

	private IEnumerator DelayedGeneration()
	{
		yield return SequenceUtil.WaitForEndOfFrame;
		GenerateCharacter(controller.IsStarterMinion);
	}

	protected override void OnCmpDisable()
	{
		base.OnCmpDisable();
		if (animController != null)
		{
			animController.gameObject.DeleteObject();
			animController = null;
		}
	}

	protected override void OnForcedCleanUp()
	{
		containers.Remove(this);
		base.OnForcedCleanUp();
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		if (controller != null)
		{
			CharacterSelectionController characterSelectionController = controller;
			characterSelectionController.OnLimitReachedEvent = (System.Action)Delegate.Remove(characterSelectionController.OnLimitReachedEvent, new System.Action(OnCharacterSelectionLimitReached));
			CharacterSelectionController characterSelectionController2 = controller;
			characterSelectionController2.OnLimitUnreachedEvent = (System.Action)Delegate.Remove(characterSelectionController2.OnLimitUnreachedEvent, new System.Action(OnCharacterSelectionLimitUnReached));
			CharacterSelectionController characterSelectionController3 = controller;
			characterSelectionController3.OnReshuffleEvent = (Action<bool>)Delegate.Remove(characterSelectionController3.OnReshuffleEvent, new Action<bool>(Reshuffle));
		}
	}

	private void Initialize()
	{
		iconGroups = new List<GameObject>();
		traitEntries = new List<GameObject>();
		expectationLabels = new List<LocText>();
		aptitudeEntries = new List<GameObject>();
		if (containers == null)
		{
			containers = new List<CharacterContainer>();
		}
		containers.Add(this);
	}

	private void OnNameChanged(string newName)
	{
		stats.Name = newName;
		stats.personality.Name = newName;
		description.text = stats.personality.description;
	}

	private void OnStartedEditing()
	{
		KScreenManager.Instance.RefreshStack();
	}

	public void SetMinion(MinionStartingStats statsProposed)
	{
		if (controller != null && controller.IsSelected(stats))
		{
			DeselectDeliverable();
		}
		stats = statsProposed;
		if (animController != null)
		{
			UnityEngine.Object.Destroy(animController.gameObject);
			animController = null;
		}
		SetAnimator();
		SetInfoText();
		StartCoroutine(SetAttributes());
		selectButton.ClearOnClick();
		if (!controller.IsStarterMinion)
		{
			selectButton.enabled = true;
			selectButton.onClick += delegate
			{
				SelectDeliverable();
			};
		}
		UpdateDefaultOutfitSelector(stats.personality);
	}

	public void GenerateCharacter(bool is_starter, string guaranteedAptitudeID = null)
	{
		int num = 0;
		do
		{
			stats = new MinionStartingStats(permittedModels, is_starter, guaranteedAptitudeID);
			num++;
		}
		while (IsCharacterInvalid() && num < 20);
		if (animController != null)
		{
			UnityEngine.Object.Destroy(animController.gameObject);
			animController = null;
		}
		SetAnimator();
		SetInfoText();
		StartCoroutine(SetAttributes());
		selectButton.ClearOnClick();
		if (!controller.IsStarterMinion)
		{
			selectButton.enabled = true;
			selectButton.onClick += delegate
			{
				SelectDeliverable();
			};
		}
		UpdateDefaultOutfitSelector(stats.personality);
	}

	private void UpdateDefaultOutfitSelector(Personality personality)
	{
		Option<ClothingOutfitTarget> selectedOutfit = ClothingOutfitTarget.TryFromTemplateId(stats.personality.GetSelectedTemplateOutfitId(ClothingOutfitUtility.OutfitType.Clothing));
		if (selectedOutfit.IsSome())
		{
			outfitSelectorIndex = allAvailableClothingOutfits.FindIndex((Option<ClothingOutfitTarget> outfit) => outfit.Unwrap().OutfitId == selectedOutfit.Unwrap().OutfitId);
		}
		else
		{
			outfitSelectorIndex = allAvailableClothingOutfits.FindIndex((Option<ClothingOutfitTarget> outfit) => outfit.Unwrap().OutfitId == stats.personality.GetSelectedTemplateOutfitId(ClothingOutfitUtility.OutfitType.Clothing));
		}
		if (outfitSelectorIndex == -1)
		{
			outfitSelectorIndex = allAvailableClothingOutfits.FindIndex((Option<ClothingOutfitTarget> outfit) => outfit.Unwrap().OutfitId == defaultShirtIdxToDefaultOutfitID[stats.personality.body]);
		}
		RefreshOutfitSelector();
	}

	private void SetAnimator()
	{
		if (animController == null)
		{
			animController = Util.KInstantiateUI(Assets.GetPrefab(GameTags.MinionSelectPreview), contentBody.gameObject).GetComponent<KBatchedAnimController>();
			animController.gameObject.SetActive(value: true);
			animController.animScale = baseCharacterScale;
		}
		BaseMinionConfig.ConfigureSymbols(animController.gameObject);
		stats.ApplyTraits(animController.gameObject);
		stats.ApplyRace(animController.gameObject);
		stats.ApplyAccessories(animController.gameObject);
		stats.ApplyOutfit(stats.personality, animController.gameObject, stats.GetSelectedOutfitOption());
		stats.ApplyJoyResponseOutfit(stats.personality, animController.gameObject);
		stats.ApplyExperience(animController.gameObject);
		HashedString idleAnim = GetIdleAnim(stats);
		idle_anim = Assets.GetAnim(idleAnim);
		if (idle_anim != null)
		{
			animController.AddAnimOverrides(idle_anim);
			animController.randomiseLoopedOffset = true;
		}
		HashedString hashedString = new HashedString("crewSelect_fx_kanim");
		KAnimFile anim = Assets.GetAnim(hashedString);
		PortraitBgAnimInfo bGAnimInfo = GetBGAnimInfo(stats);
		KAnimFile anim2 = Assets.GetAnim(bGAnimInfo.animFileName);
		KAnimFile kAnimFile = (string.IsNullOrEmpty(bGAnimInfo.foregroundAnimFileName) ? null : Assets.GetAnim(bGAnimInfo.foregroundAnimFileName));
		bool flag = kAnimFile != null;
		bgAnimController.SwapAnims(new KAnimFile[1] { anim2 });
		if (flag)
		{
			fgAnimController.gameObject.SetActive(value: true);
			fgAnimController.SwapAnims(new KAnimFile[1] { kAnimFile });
		}
		else
		{
			fgAnimController.gameObject.SetActive(value: false);
		}
		bool flag2 = false;
		KAnimFileData data = anim2.GetData();
		if (data != null)
		{
			for (int i = 0; i < data.animCount; i++)
			{
				KAnim.Anim anim3 = data.GetAnim(i);
				if (anim3.name.EndsWith("_pre"))
				{
					bgAnimController.Play(anim3.name);
					if (flag)
					{
						fgAnimController.Play(anim3.name);
					}
					flag2 = true;
					break;
				}
			}
		}
		if (flag2)
		{
			bgAnimController.Queue("crewSelect_bg", KAnim.PlayMode.Loop);
			if (flag)
			{
				fgAnimController.Queue("crewSelect_bg", KAnim.PlayMode.Loop);
			}
		}
		else
		{
			bgAnimController.Play("crewSelect_bg", KAnim.PlayMode.Loop);
			if (flag)
			{
				fgAnimController.Play("crewSelect_bg", KAnim.PlayMode.Loop);
			}
		}
		if (anim != null)
		{
			animController.AddAnimOverrides(anim);
		}
		animController.Queue("idle_default", KAnim.PlayMode.Loop);
	}

	private PortraitBgAnimInfo GetBGAnimInfo(MinionStartingStats minionStartingStats)
	{
		if (minionStartingStats.personality.model == GameTags.Minions.Models.Standard)
		{
			foreach (Trait trait in minionStartingStats.Traits)
			{
				if (trait.Id == "GrantSkill_Swimming" || trait.Id == "GrantSkill_Swimming2")
				{
					return new PortraitBgAnimInfo
					{
						animFileName = "crewselect_backdrop_swim_kanim",
						hasPreAnim = true,
						foregroundAnimFileName = "crewselect_backdrop_swim_fg_kanim"
					};
				}
			}
		}
		return portraitBGAnimsByModel[minionStartingStats.personality.model];
	}

	private HashedString GetIdleAnim(MinionStartingStats minionStartingStats)
	{
		List<HashedString> list = new List<HashedString>();
		foreach (KeyValuePair<HashedString, string[]> traitForcedIdleAnim in traitForcedIdleAnims)
		{
			foreach (Trait trait in minionStartingStats.Traits)
			{
				if (traitForcedIdleAnim.Value.Contains(trait.Id))
				{
					return traitForcedIdleAnim.Key;
				}
			}
			if (traitForcedIdleAnim.Value.Contains(minionStartingStats.joyTrait.Id) || traitForcedIdleAnim.Value.Contains(minionStartingStats.stressTrait.Id))
			{
				return traitForcedIdleAnim.Key;
			}
		}
		foreach (KeyValuePair<HashedString, string[]> traitIdleAnim in traitIdleAnims)
		{
			foreach (Trait trait2 in minionStartingStats.Traits)
			{
				if (traitIdleAnim.Value.Contains(trait2.Id))
				{
					list.Add(traitIdleAnim.Key);
				}
			}
			if (traitIdleAnim.Value.Contains(minionStartingStats.joyTrait.Id) || traitIdleAnim.Value.Contains(minionStartingStats.stressTrait.Id))
			{
				list.Add(traitIdleAnim.Key);
			}
		}
		if (list.Count > 0)
		{
			return list.ToArray()[UnityEngine.Random.Range(0, list.Count)];
		}
		return idleAnims[UnityEngine.Random.Range(0, idleAnims.Length)];
	}

	private string GetOutfitName(int index)
	{
		if (index == -1)
		{
			return Strings.Get("STRINGS.UI.CHARACTERCONTAINER_NO_OUTFIT");
		}
		return allAvailableClothingOutfits[index].Unwrap().ReadName();
	}

	private void RefreshOutfitSelector()
	{
		Image reference = outfitSelectorReferences.GetReference<Image>("CurrentOutfitIcon");
		Image reference2 = outfitSelectorReferences.GetReference<Image>("NextOutfitIcon");
		MultiToggle component = reference2.transform.parent.GetComponent<MultiToggle>();
		Image reference3 = outfitSelectorReferences.GetReference<Image>("PreviousOutfitIcon");
		MultiToggle component2 = reference3.transform.parent.GetComponent<MultiToggle>();
		MultiToggle reference4 = outfitSelectorReferences.GetReference<MultiToggle>("PreviousOutfitButton");
		MultiToggle reference5 = outfitSelectorReferences.GetReference<MultiToggle>("NextOutfitButton");
		RectTransform expandedMenu = outfitSelectorReferences.GetReference<RectTransform>("ExpandedMenu");
		MultiToggle expandButton = outfitSelectorReferences.GetReference<MultiToggle>("CollapsedButton");
		expandButton.onClick = null;
		MultiToggle multiToggle = expandButton;
		multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, (System.Action)delegate
		{
			outfitSelectorExpanded = !outfitSelectorExpanded;
			expandButton.gameObject.SetActive(!outfitSelectorExpanded);
			expandedMenu.gameObject.SetActive(outfitSelectorExpanded);
			RefreshOutfitSelector();
		});
		MultiToggle reference6 = outfitSelectorReferences.GetReference<MultiToggle>("CurrentOutfitButton");
		reference6.onClick = null;
		reference6.onClick = (System.Action)Delegate.Combine(reference6.onClick, (System.Action)delegate
		{
			outfitSelectorExpanded = !outfitSelectorExpanded;
			expandButton.gameObject.SetActive(!outfitSelectorExpanded);
			expandedMenu.gameObject.SetActive(outfitSelectorExpanded);
			RefreshOutfitSelector();
		});
		reference.sprite = GetClothingIcon(0);
		expandButton.gameObject.GetComponentInChildrenOnly<Image>().sprite = reference.sprite;
		reference2.sprite = GetClothingIcon(-1);
		reference3.sprite = GetClothingIcon(1);
		expandButton.GetComponent<ToolTip>().SetSimpleTooltip(GameUtil.SafeStringFormat(Strings.Get("STRINGS.UI.CHARACTERCONTAINER_EXPAND_OUTFIT_SELECTOR_BUTTON"), GetOutfitName(outfitSelectorIndex)));
		string outfitName = GetOutfitName(outfitSelectorIndex);
		reference.transform.parent.GetComponent<ToolTip>().SetSimpleTooltip(outfitName + "\n\n" + UI.CHARACTERCONTAINER_CONFIRM_OUTFIT_SELECTION_TOOLTIP);
		reference4.onClick = null;
		reference4.onClick = (System.Action)Delegate.Combine(reference4.onClick, (System.Action)delegate
		{
			outfitSelectorIndex = GetOutfitSelectorIndex(1);
			RefreshOutfitSelector();
			stats.ApplyOutfit(stats.personality, animController.gameObject, (outfitSelectorIndex == -1) ? default(Option<ClothingOutfitTarget>) : allAvailableClothingOutfits[outfitSelectorIndex]);
			if (fxAnim != null)
			{
				fxAnim.Play("loop");
			}
			UISounds.Instance.PlaySound3D(GlobalAssets.GetSound("DupeShuffle"));
		});
		component2.onClick = reference4.onClick;
		int index = GetOutfitSelectorIndex(1);
		string outfitName2 = GetOutfitName(index);
		reference3.transform.parent.GetComponent<ToolTip>().SetSimpleTooltip(outfitName2);
		reference5.onClick = null;
		reference5.onClick = (System.Action)Delegate.Combine(reference5.onClick, (System.Action)delegate
		{
			outfitSelectorIndex = GetOutfitSelectorIndex(-1);
			RefreshOutfitSelector();
			stats.ApplyOutfit(stats.personality, animController.gameObject, (outfitSelectorIndex == -1) ? default(Option<ClothingOutfitTarget>) : allAvailableClothingOutfits[outfitSelectorIndex]);
			if (fxAnim != null)
			{
				fxAnim.Play("loop");
			}
			UISounds.Instance.PlaySound3D(GlobalAssets.GetSound("DupeShuffle"));
		});
		component.onClick = reference5.onClick;
		int index2 = GetOutfitSelectorIndex(-1);
		string outfitName3 = GetOutfitName(index2);
		reference2.transform.parent.GetComponent<ToolTip>().SetSimpleTooltip(outfitName3);
		stats.overrideOutfitID = ((outfitSelectorIndex == -1) ? null : allAvailableClothingOutfits[outfitSelectorIndex].Unwrap().OutfitId);
		Sprite GetClothingIcon(int offset)
		{
			int num = GetOutfitSelectorIndex(offset);
			if (num == -1)
			{
				return KleiItemsUI.GetNoneClothingItemIcon(PermitCategory.DupeTops, stats.personality);
			}
			string text = allAvailableClothingOutfits[num].Unwrap().ReadItems().FindFirst((string item) => Db.Get().Permits.ClothingItems.Get(item).Category == PermitCategory.DupeTops);
			if (text == null)
			{
				string[] array = allAvailableClothingOutfits[num].Unwrap().ReadItems();
				if (array == null || array.Length == 0)
				{
					return KleiItemsUI.GetNoneClothingItemIcon(PermitCategory.DupeTops, stats.personality);
				}
				text = array[0];
			}
			return Db.Get().Permits.ClothingItems.Get(text).GetPermitPresentationInfo().sprite;
		}
	}

	private int GetOutfitSelectorIndex(int indexOffset)
	{
		int count = allAvailableClothingOutfits.Count;
		int num = outfitSelectorIndex + indexOffset;
		if (num >= count)
		{
			num = -1;
		}
		if (num < -1)
		{
			num = count - 1;
		}
		return num;
	}

	private void SetInfoText()
	{
		traitEntries.ForEach(delegate(GameObject tl)
		{
			UnityEngine.Object.Destroy(tl.gameObject);
		});
		traitEntries.Clear();
		characterNameTitle.SetTitle(stats.Name);
		traitHeaderLabel.SetText((stats.personality.model == GameTags.Minions.Models.Bionic) ? UI.CHARACTERCONTAINER_TRAITS_TITLE_BIONIC : UI.CHARACTERCONTAINER_TRAITS_TITLE);
		for (int num = 1; num < stats.Traits.Count; num++)
		{
			Trait trait = stats.Traits[num];
			LocText locText = (trait.PositiveTrait ? goodTrait : badTrait);
			LocText locText2 = Util.KInstantiateUI<LocText>(locText.gameObject, locText.transform.parent.gameObject);
			locText2.gameObject.SetActive(value: true);
			locText2.text = stats.Traits[num].GetName();
			locText2.color = (trait.PositiveTrait ? Constants.POSITIVE_COLOR : Constants.NEGATIVE_COLOR);
			locText2.GetComponent<ToolTip>().SetSimpleTooltip(trait.GetTooltip());
			for (int num2 = 0; num2 < trait.SelfModifiers.Count; num2++)
			{
				GameObject gameObject = Util.KInstantiateUI(attributeLabelTrait.gameObject, locText.transform.parent.gameObject);
				gameObject.SetActive(value: true);
				LocText componentInChildren = gameObject.GetComponentInChildren<LocText>();
				string format = ((trait.SelfModifiers[num2].Value > 0f) ? UI.CHARACTERCONTAINER_ATTRIBUTEMODIFIER_INCREASED : UI.CHARACTERCONTAINER_ATTRIBUTEMODIFIER_DECREASED);
				componentInChildren.text = string.Format(format, Strings.Get("STRINGS.DUPLICANTS.ATTRIBUTES." + trait.SelfModifiers[num2].AttributeId.ToUpper() + ".NAME"));
				if (trait.SelfModifiers[num2].AttributeId == "GermResistance")
				{
				}
				Klei.AI.Attribute attribute = Db.Get().Attributes.Get(trait.SelfModifiers[num2].AttributeId);
				string text = attribute.Description;
				text = string.Concat(text, "\n\n", Strings.Get("STRINGS.DUPLICANTS.ATTRIBUTES." + trait.SelfModifiers[num2].AttributeId.ToUpper() + ".NAME"), ": ", trait.SelfModifiers[num2].GetFormattedString());
				List<AttributeConverter> convertersForAttribute = Db.Get().AttributeConverters.GetConvertersForAttribute(attribute);
				for (int num3 = 0; num3 < convertersForAttribute.Count; num3++)
				{
					string text2 = convertersForAttribute[num3].DescriptionFromAttribute(convertersForAttribute[num3].multiplier * trait.SelfModifiers[num2].Value, null);
					if (text2 != "")
					{
						text = text + "\n    • " + text2;
					}
				}
				componentInChildren.GetComponent<ToolTip>().SetSimpleTooltip(text);
				traitEntries.Add(gameObject);
			}
			if (trait.disabledChoreGroups != null)
			{
				GameObject gameObject2 = Util.KInstantiateUI(attributeLabelTrait.gameObject, locText.transform.parent.gameObject);
				gameObject2.SetActive(value: true);
				LocText componentInChildren2 = gameObject2.GetComponentInChildren<LocText>();
				componentInChildren2.text = trait.GetDisabledChoresString(list_entry: false);
				string text3 = "";
				string text4 = "";
				for (int num4 = 0; num4 < trait.disabledChoreGroups.Length; num4++)
				{
					if (num4 > 0)
					{
						text3 += ", ";
						text4 += "\n";
					}
					text3 += trait.disabledChoreGroups[num4].Name;
					text4 += trait.disabledChoreGroups[num4].description;
				}
				componentInChildren2.GetComponent<ToolTip>().SetSimpleTooltip(string.Format(DUPLICANTS.TRAITS.CANNOT_DO_TASK_TOOLTIP, text3, text4));
				traitEntries.Add(gameObject2);
			}
			if (trait.ignoredEffects != null && trait.ignoredEffects.Length != 0)
			{
				GameObject gameObject3 = Util.KInstantiateUI(attributeLabelTrait.gameObject, locText.transform.parent.gameObject);
				gameObject3.SetActive(value: true);
				LocText componentInChildren3 = gameObject3.GetComponentInChildren<LocText>();
				componentInChildren3.text = trait.GetIgnoredEffectsString(list_entry: false);
				string text5 = "";
				for (int num5 = 0; num5 < trait.ignoredEffects.Length; num5++)
				{
					if (num5 > 0)
					{
						text5 += "\n";
					}
					text5 += string.Format(DUPLICANTS.TRAITS.IGNORED_EFFECTS_TOOLTIP, Strings.Get("STRINGS.DUPLICANTS.MODIFIERS." + trait.ignoredEffects[num5].ToUpper() + ".NAME"), Strings.Get("STRINGS.DUPLICANTS.MODIFIERS." + trait.ignoredEffects[num5].ToUpper() + ".CAUSE"));
					if (num5 < trait.ignoredEffects.Length - 1)
					{
						text5 += ",";
					}
				}
				componentInChildren3.GetComponent<ToolTip>().SetSimpleTooltip(text5);
				traitEntries.Add(gameObject3);
			}
			StringEntry result = null;
			if (trait.ShortDescCB != null || Strings.TryGet("STRINGS.DUPLICANTS.TRAITS." + trait.Id.ToUpper() + ".SHORT_DESC", out result))
			{
				string text6 = ((trait.ShortDescCB != null) ? trait.ShortDescCB() : result.String);
				string simpleTooltip = ((trait.ShortDescTooltipCB != null) ? trait.ShortDescTooltipCB() : ((string)Strings.Get("STRINGS.DUPLICANTS.TRAITS." + trait.Id.ToUpper() + ".SHORT_DESC_TOOLTIP")));
				GameObject gameObject4 = Util.KInstantiateUI(attributeLabelTrait.gameObject, locText.transform.parent.gameObject);
				gameObject4.SetActive(value: true);
				LocText componentInChildren4 = gameObject4.GetComponentInChildren<LocText>();
				componentInChildren4.text = text6;
				componentInChildren4.GetComponent<ToolTip>().SetSimpleTooltip(simpleTooltip);
				traitEntries.Add(gameObject4);
			}
			traitEntries.Add(locText2.gameObject);
		}
		aptitudeEntries.ForEach(delegate(GameObject al)
		{
			UnityEngine.Object.Destroy(al.gameObject);
		});
		aptitudeEntries.Clear();
		expectationLabels.ForEach(delegate(LocText el)
		{
			UnityEngine.Object.Destroy(el.gameObject);
		});
		expectationLabels.Clear();
		if (stats.personality.model == GameTags.Minions.Models.Bionic)
		{
			aptitudeContainer.SetActive(value: false);
		}
		else
		{
			aptitudeContainer.SetActive(value: true);
			List<string> list = new List<string>();
			foreach (KeyValuePair<SkillGroup, float> skillAptitude in stats.skillAptitudes)
			{
				if (skillAptitude.Value == 0f)
				{
					continue;
				}
				SkillGroup skillGroup = Db.Get().SkillGroups.Get(skillAptitude.Key.IdHash);
				if (skillGroup == null)
				{
					Debug.LogWarningFormat("Role group not found for aptitude: {0}", skillAptitude.Key);
					continue;
				}
				GameObject gameObject5 = Util.KInstantiateUI(aptitudeEntry.gameObject, aptitudeContainer);
				LocText locText3 = Util.KInstantiateUI<LocText>(aptitudeLabel.gameObject, gameObject5);
				locText3.gameObject.SetActive(value: true);
				locText3.text = skillGroup.Name;
				string text7 = "";
				if (skillGroup.choreGroupID != "")
				{
					ChoreGroup choreGroup = Db.Get().ChoreGroups.Get(skillGroup.choreGroupID);
					text7 = string.Format(DUPLICANTS.ROLES.GROUPS.APTITUDE_DESCRIPTION_CHOREGROUP, skillGroup.Name, DUPLICANTSTATS.APTITUDE_BONUS, choreGroup.description);
				}
				else
				{
					text7 = string.Format(DUPLICANTS.ROLES.GROUPS.APTITUDE_DESCRIPTION, skillGroup.Name, DUPLICANTSTATS.APTITUDE_BONUS);
				}
				locText3.GetComponent<ToolTip>().SetSimpleTooltip(text7);
				string id = skillAptitude.Key.relevantAttributes[0].Id;
				float num6 = stats.StartingLevels[id];
				LocText locText4 = Util.KInstantiateUI<LocText>(attributeLabelAptitude.gameObject, gameObject5);
				locText4.gameObject.SetActive(!list.Contains(id));
				locText4.text = "+" + num6 + " " + skillAptitude.Key.relevantAttributes[0].Name;
				string text8 = skillAptitude.Key.relevantAttributes[0].Description;
				text8 = text8 + "\n\n" + skillAptitude.Key.relevantAttributes[0].Name + ": +" + num6;
				List<AttributeConverter> convertersForAttribute2 = Db.Get().AttributeConverters.GetConvertersForAttribute(skillAptitude.Key.relevantAttributes[0]);
				for (int num7 = 0; num7 < convertersForAttribute2.Count; num7++)
				{
					text8 = text8 + "\n    • " + convertersForAttribute2[num7].DescriptionFromAttribute(convertersForAttribute2[num7].multiplier * num6, null);
				}
				list.Add(id);
				locText4.GetComponent<ToolTip>().SetSimpleTooltip(text8);
				gameObject5.gameObject.SetActive(value: true);
				aptitudeEntries.Add(gameObject5);
			}
		}
		if (stats.stressTrait != null)
		{
			LocText locText5 = Util.KInstantiateUI<LocText>(expectationRight.gameObject, expectationRight.transform.parent.gameObject);
			locText5.gameObject.SetActive(value: true);
			locText5.text = string.Format(UI.CHARACTERCONTAINER_STRESSTRAIT, stats.stressTrait.GetName());
			locText5.GetComponent<ToolTip>().SetSimpleTooltip(stats.stressTrait.GetTooltip());
			expectationLabels.Add(locText5);
		}
		if (stats.joyTrait != null)
		{
			LocText locText6 = Util.KInstantiateUI<LocText>(expectationRight.gameObject, expectationRight.transform.parent.gameObject);
			locText6.gameObject.SetActive(value: true);
			locText6.text = string.Format(UI.CHARACTERCONTAINER_JOYTRAIT, stats.joyTrait.GetName());
			locText6.GetComponent<ToolTip>().SetSimpleTooltip(stats.joyTrait.GetTooltip());
			expectationLabels.Add(locText6);
		}
		description.text = stats.personality.description;
	}

	private IEnumerator SetAttributes()
	{
		yield return null;
		iconGroups.ForEach(delegate(GameObject icg)
		{
			UnityEngine.Object.Destroy(icg);
		});
		iconGroups.Clear();
		Klei.AI.Attributes attr = animController.gameObject.GetAttributes();
		List<AttributeInstance> attributes = new List<AttributeInstance>(attr.AttributeTable);
		attributes.RemoveAll((AttributeInstance at) => at.Attribute.ShowInUI != Klei.AI.Attribute.Display.Skill);
		attributes = attributes.OrderBy((AttributeInstance at) => at.Name).ToList();
		for (int i = 0; i < attributes.Count; i++)
		{
			GameObject newIconGroup = Util.KInstantiateUI(iconGroup.gameObject, iconGroup.transform.parent.gameObject);
			LocText label = newIconGroup.GetComponentInChildren<LocText>();
			newIconGroup.SetActive(value: true);
			float totalValue = attributes[i].GetTotalValue();
			if (totalValue > 0f)
			{
				label.color = Constants.POSITIVE_COLOR;
			}
			else if (totalValue == 0f)
			{
				label.color = Constants.NEUTRAL_COLOR;
			}
			else
			{
				label.color = Constants.NEGATIVE_COLOR;
			}
			label.text = string.Format(UI.CHARACTERCONTAINER_SKILL_VALUE, GameUtil.AddPositiveSign(totalValue.ToString(), totalValue > 0f), attributes[i].Name);
			AttributeInstance attribute = attributes[i];
			string tooltip = attribute.Description;
			if (attribute.Attribute.converters.Count > 0)
			{
				tooltip += "\n";
				foreach (AttributeConverter converter in attribute.Attribute.converters)
				{
					AttributeConverterInstance converter_instance = animController.gameObject.GetComponent<Klei.AI.AttributeConverters>().GetConverter(converter.Id);
					string instance_details = converter_instance.DescriptionFromAttribute(converter_instance.Evaluate(), converter_instance.gameObject);
					if (instance_details != null)
					{
						tooltip = tooltip + "\n" + instance_details;
					}
				}
			}
			newIconGroup.GetComponent<ToolTip>().SetSimpleTooltip(tooltip);
			iconGroups.Add(newIconGroup);
		}
	}

	public void SelectDeliverable()
	{
		if (controller != null)
		{
			controller.AddDeliverable(stats);
		}
		if (MusicManager.instance.SongIsPlaying("Music_SelectDuplicant"))
		{
			MusicManager.instance.SetSongParameter("Music_SelectDuplicant", "songSection", 1f);
		}
		selectButton.GetComponent<ImageToggleState>().SetActive();
		selectButton.ClearOnClick();
		selectButton.onClick += delegate
		{
			DeselectDeliverable();
			if (MusicManager.instance.SongIsPlaying("Music_SelectDuplicant"))
			{
				MusicManager.instance.SetSongParameter("Music_SelectDuplicant", "songSection", 0f);
			}
		};
		selectedBorder.SetActive(value: true);
		titleBar.color = selectedTitleColor;
		animController.Play("cheer_pre");
		animController.Play("cheer_loop", KAnim.PlayMode.Loop);
	}

	public void DeselectDeliverable()
	{
		if (controller != null)
		{
			controller.RemoveDeliverable(stats);
		}
		selectButton.GetComponent<ImageToggleState>().SetInactive();
		selectButton.Deselect();
		selectButton.ClearOnClick();
		selectButton.onClick += delegate
		{
			SelectDeliverable();
		};
		selectedBorder.SetActive(value: false);
		titleBar.color = deselectedTitleColor;
		animController.Queue("cheer_pst");
		animController.Queue("idle_default", KAnim.PlayMode.Loop);
	}

	private void OnReplacedEvent(ITelepadDeliverable deliverable)
	{
		if (deliverable == stats)
		{
			DeselectDeliverable();
		}
	}

	private void OnCharacterSelectionLimitReached()
	{
		if (!(controller != null) || !controller.IsSelected(stats))
		{
			selectButton.ClearOnClick();
			if (controller.AllowsReplacing)
			{
				selectButton.onClick += ReplaceCharacterSelection;
			}
			else
			{
				selectButton.onClick += CantSelectCharacter;
			}
		}
	}

	private void CantSelectCharacter()
	{
		KMonoBehaviour.PlaySound(GlobalAssets.GetSound("Negative"));
	}

	private void ReplaceCharacterSelection()
	{
		if (!(controller == null))
		{
			controller.RemoveLast();
			SelectDeliverable();
		}
	}

	private void OnCharacterSelectionLimitUnReached()
	{
		if (!(controller != null) || !controller.IsSelected(stats))
		{
			selectButton.ClearOnClick();
			selectButton.onClick += delegate
			{
				SelectDeliverable();
			};
		}
	}

	public void SetReshufflingState(bool enable)
	{
		reshuffleButton.gameObject.SetActive(enable);
		archetypeDropDown.gameObject.SetActive(enable);
		modelDropDown.transform.parent.gameObject.SetActive(enable && Game.IsDlcActiveForCurrentSave("DLC3_ID"));
	}

	public void Reshuffle(bool is_starter)
	{
		if (controller != null && controller.IsSelected(stats))
		{
			DeselectDeliverable();
		}
		if (fxAnim != null)
		{
			fxAnim.Play("loop");
		}
		GenerateCharacter(is_starter, guaranteedAptitudeID);
		this.OnReshuffled?.Invoke(this);
	}

	public void SetController(CharacterSelectionController csc)
	{
		if (!(csc == controller))
		{
			controller = csc;
			CharacterSelectionController characterSelectionController = controller;
			characterSelectionController.OnLimitReachedEvent = (System.Action)Delegate.Combine(characterSelectionController.OnLimitReachedEvent, new System.Action(OnCharacterSelectionLimitReached));
			CharacterSelectionController characterSelectionController2 = controller;
			characterSelectionController2.OnLimitUnreachedEvent = (System.Action)Delegate.Combine(characterSelectionController2.OnLimitUnreachedEvent, new System.Action(OnCharacterSelectionLimitUnReached));
			CharacterSelectionController characterSelectionController3 = controller;
			characterSelectionController3.OnReshuffleEvent = (Action<bool>)Delegate.Combine(characterSelectionController3.OnReshuffleEvent, new Action<bool>(Reshuffle));
			CharacterSelectionController characterSelectionController4 = controller;
			characterSelectionController4.OnReplacedEvent = (Action<ITelepadDeliverable>)Delegate.Combine(characterSelectionController4.OnReplacedEvent, new Action<ITelepadDeliverable>(OnReplacedEvent));
		}
	}

	public void DisableSelectButton()
	{
		selectButton.soundPlayer.AcceptClickCondition = () => false;
		selectButton.GetComponent<ImageToggleState>().SetDisabled();
		selectButton.soundPlayer.Enabled = false;
	}

	private bool IsCharacterInvalid()
	{
		CharacterContainer characterContainer = containers.Find((CharacterContainer container) => container != null && container.stats != null && container != this && container.stats.personality.Id == stats.personality.Id && container.stats.IsValid);
		if (characterContainer != null)
		{
			return true;
		}
		if (Game.Instance != null && !Game.IsDlcActiveForCurrentSave(stats.personality.requiredDlcId))
		{
			return true;
		}
		if (stats.personality.model != GameTags.Minions.Models.Bionic && Components.LiveMinionIdentities.Items.Any((MinionIdentity id) => id.personalityResourceId == stats.personality.Id))
		{
			return true;
		}
		return false;
	}

	public string GetValueColor(bool isPositive)
	{
		return isPositive ? "<color=green>" : "<color=#ff2222ff>";
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		scroll_rect.mouseIsOver = true;
		base.OnPointerEnter(eventData);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		scroll_rect.mouseIsOver = false;
		base.OnPointerExit(eventData);
	}

	public override void OnKeyDown(KButtonEvent e)
	{
		if (e.IsAction(Action.Escape) || e.IsAction(Action.MouseRight))
		{
			characterNameTitle.ForceStopEditing();
			controller.OnPressBack();
			archetypeDropDown.scrollRect.gameObject.SetActive(value: false);
		}
		if (KInputManager.currentControllerIsGamepad)
		{
			if (archetypeDropDown.scrollRect.activeInHierarchy)
			{
				KScrollRect component = archetypeDropDown.scrollRect.GetComponent<KScrollRect>();
				Vector2 point = component.rectTransform().InverseTransformPoint(KInputManager.GetMousePos());
				if (component.rectTransform().rect.Contains(point))
				{
					component.mouseIsOver = true;
				}
				else
				{
					component.mouseIsOver = false;
				}
				component.OnKeyDown(e);
			}
			else
			{
				scroll_rect.OnKeyDown(e);
			}
		}
		else
		{
			e.Consumed = true;
		}
	}

	public override void OnKeyUp(KButtonEvent e)
	{
		if (KInputManager.currentControllerIsGamepad)
		{
			if (archetypeDropDown.scrollRect.activeInHierarchy)
			{
				KScrollRect component = archetypeDropDown.scrollRect.GetComponent<KScrollRect>();
				Vector2 point = component.rectTransform().InverseTransformPoint(KInputManager.GetMousePos());
				if (component.rectTransform().rect.Contains(point))
				{
					component.mouseIsOver = true;
				}
				else
				{
					component.mouseIsOver = false;
				}
				component.OnKeyUp(e);
			}
			else
			{
				scroll_rect.OnKeyUp(e);
			}
		}
		else
		{
			e.Consumed = true;
		}
	}

	protected override void OnCmpEnable()
	{
		base.OnActivate();
		if (stats != null)
		{
			SetAnimator();
		}
	}

	protected override void OnShow(bool show)
	{
		base.OnShow(show);
		characterNameTitle.ForceStopEditing();
	}

	private void OnArchetypeEntryClick(IListableOption skill, object data)
	{
		if (skill != null)
		{
			SkillGroup skillGroup = skill as SkillGroup;
			guaranteedAptitudeID = skillGroup.Id;
			selectedArchetypeIcon.sprite = Assets.GetSprite(skillGroup.archetypeIcon);
			Reshuffle(is_starter: true);
		}
		else
		{
			guaranteedAptitudeID = null;
			selectedArchetypeIcon.sprite = dropdownArrowIcon;
			Reshuffle(is_starter: true);
		}
	}

	private int archetypeDropDownSort(IListableOption a, IListableOption b, object targetData)
	{
		if (b.Equals("Random"))
		{
			return -1;
		}
		return b.GetProperName().CompareTo(a.GetProperName());
	}

	private void archetypeDropEntryRefreshAction(DropDownEntry entry, object targetData)
	{
		if (entry.entryData != null)
		{
			SkillGroup skillGroup = entry.entryData as SkillGroup;
			entry.image.sprite = Assets.GetSprite(skillGroup.archetypeIcon);
		}
	}

	private void OnModelEntryClick(IListableOption listItem, object data)
	{
		bool flag = false;
		if (listItem == null)
		{
			permittedModels = allMinionModels;
			selectedModelIcon.sprite = Assets.GetSprite(allModelSprite);
			Reshuffle(is_starter: true);
		}
		else if (listItem is MinionModelOption minionModelOption)
		{
			flag = minionModelOption.permittedModels.Count == 1 && minionModelOption.permittedModels[0] == GameTags.Minions.Models.Bionic;
			permittedModels = minionModelOption.permittedModels;
			selectedModelIcon.sprite = minionModelOption.sprite;
			Reshuffle(is_starter: true);
		}
		reshuffleButton.soundPlayer.widget_sound_events()[0].OverrideAssetName = (flag ? "DupeShuffle_bionic" : "DupeShuffle");
	}

	private int modelDropDownSort(IListableOption a, IListableOption b, object targetData)
	{
		return a.GetProperName().CompareTo(b.GetProperName());
	}

	private void modelDropEntryRefreshAction(DropDownEntry entry, object targetData)
	{
		if (entry.entryData != null)
		{
			MinionModelOption minionModelOption = entry.entryData as MinionModelOption;
			entry.image.sprite = minionModelOption.sprite;
		}
	}
}
