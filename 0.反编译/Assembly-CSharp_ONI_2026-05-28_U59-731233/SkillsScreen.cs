using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Database;
using Klei.AI;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class SkillsScreen : KModalScreen
{
	[SerializeField]
	private KButton CloseButton;

	[Header("Prefabs")]
	[SerializeField]
	private GameObject Prefab_skillWidget;

	[SerializeField]
	private GameObject Prefab_skillColumn;

	[SerializeField]
	private GameObject Prefab_minion;

	[SerializeField]
	private GameObject Prefab_minionLayout;

	[SerializeField]
	private GameObject Prefab_tableLayout;

	[SerializeField]
	private GameObject Prefab_worldDivider;

	[Header("Sort Toggles")]
	[SerializeField]
	private MultiToggle dupeSortingToggle;

	[SerializeField]
	private MultiToggle experienceSortingToggle;

	[SerializeField]
	private MultiToggle moraleSortingToggle;

	private MultiToggle activeSortToggle;

	private bool sortReversed = false;

	private Comparison<IAssignableIdentity> active_sort_method;

	[Header("Duplicant Animation")]
	[SerializeField]
	private FullBodyUIMinionWidget minionAnimWidget;

	[Header("Progress Bars")]
	[SerializeField]
	private ToolTip expectationsTooltip;

	[SerializeField]
	private LocText moraleProgressLabel;

	[SerializeField]
	private GameObject moraleWarning;

	[SerializeField]
	private GameObject moraleNotch;

	[SerializeField]
	private Color moraleNotchColor;

	private List<GameObject> moraleNotches = new List<GameObject>();

	[SerializeField]
	private LocText expectationsProgressLabel;

	[SerializeField]
	private GameObject expectationWarning;

	[SerializeField]
	private GameObject expectationNotch;

	[SerializeField]
	private Color expectationNotchColor;

	[SerializeField]
	private Color expectationNotchProspectColor;

	private List<GameObject> expectationNotches = new List<GameObject>();

	[SerializeField]
	private ToolTip experienceBarTooltip;

	[SerializeField]
	private Image experienceProgressFill;

	[SerializeField]
	private LocText EXPCount;

	[SerializeField]
	private LocText duplicantLevelIndicator;

	[SerializeField]
	private KScrollRect scrollRect;

	[SerializeField]
	private float scrollSpeed = 7f;

	[SerializeField]
	private DropDown hatDropDown;

	[SerializeField]
	public Image selectedHat;

	[SerializeField]
	private GameObject skillsContainer;

	[SerializeField]
	private GameObject boosterPanel;

	[SerializeField]
	private GameObject boosterHeader;

	[SerializeField]
	private GameObject boosterContentGrid;

	[SerializeField]
	private GameObject boosterPrefab;

	private Dictionary<Tag, HierarchyReferences> boosterWidgets = new Dictionary<Tag, HierarchyReferences>();

	[SerializeField]
	private LocText equippedBoostersHeaderLabel;

	[SerializeField]
	private LocText assignedBoostersCountLabel;

	[SerializeField]
	private GameObject boosterSlotIconPrefab;

	private List<GameObject> boosterSlotIcons = new List<GameObject>();

	private IAssignableIdentity currentlySelectedMinion;

	private List<GameObject> rows = new List<GameObject>();

	private List<SkillMinionWidget> sortableRows = new List<SkillMinionWidget>();

	private Dictionary<int, GameObject> worldDividers = new Dictionary<int, GameObject>();

	private string hoveredSkillID = "";

	private Dictionary<string, GameObject> skillWidgets = new Dictionary<string, GameObject>();

	private Dictionary<string, int> skillGroupRow = new Dictionary<string, int>();

	private List<GameObject> skillColumns = new List<GameObject>();

	private bool dirty = false;

	private bool linesPending = false;

	private int layoutRowHeight = 80;

	private Coroutine delayRefreshRoutine;

	protected Comparison<IAssignableIdentity> compareByExperience = delegate(IAssignableIdentity a, IAssignableIdentity b)
	{
		GameObject targetGameObject = ((MinionAssignablesProxy)a).GetTargetGameObject();
		GameObject targetGameObject2 = ((MinionAssignablesProxy)b).GetTargetGameObject();
		if (targetGameObject == null && targetGameObject2 == null)
		{
			return 0;
		}
		if (targetGameObject == null)
		{
			return -1;
		}
		if (targetGameObject2 == null)
		{
			return 1;
		}
		MinionResume component = targetGameObject.GetComponent<MinionResume>();
		MinionResume component2 = targetGameObject2.GetComponent<MinionResume>();
		if (component == null && component2 == null)
		{
			return 0;
		}
		if (component == null)
		{
			return -1;
		}
		if (component2 == null)
		{
			return 1;
		}
		float num = component.AvailableSkillpoints;
		float value = component2.AvailableSkillpoints;
		return num.CompareTo(value);
	};

	protected Comparison<IAssignableIdentity> compareByMinion = (IAssignableIdentity a, IAssignableIdentity b) => a.GetProperName().CompareTo(b.GetProperName());

	protected Comparison<IAssignableIdentity> compareByMorale = delegate(IAssignableIdentity a, IAssignableIdentity b)
	{
		GameObject targetGameObject = ((MinionAssignablesProxy)a).GetTargetGameObject();
		GameObject targetGameObject2 = ((MinionAssignablesProxy)b).GetTargetGameObject();
		if (targetGameObject == null && targetGameObject2 == null)
		{
			return 0;
		}
		if (targetGameObject == null)
		{
			return -1;
		}
		if (targetGameObject2 == null)
		{
			return 1;
		}
		MinionResume component = targetGameObject.GetComponent<MinionResume>();
		MinionResume component2 = targetGameObject2.GetComponent<MinionResume>();
		if (component == null && component2 == null)
		{
			return 0;
		}
		if (component == null)
		{
			return -1;
		}
		if (component2 == null)
		{
			return 1;
		}
		AttributeInstance attributeInstance = Db.Get().Attributes.QualityOfLife.Lookup(component);
		AttributeInstance attributeInstance2 = Db.Get().Attributes.QualityOfLifeExpectation.Lookup(component);
		AttributeInstance attributeInstance3 = Db.Get().Attributes.QualityOfLife.Lookup(component2);
		AttributeInstance attributeInstance4 = Db.Get().Attributes.QualityOfLifeExpectation.Lookup(component2);
		float totalValue = attributeInstance.GetTotalValue();
		float totalValue2 = attributeInstance3.GetTotalValue();
		return totalValue.CompareTo(totalValue2);
	};

	public IAssignableIdentity CurrentlySelectedMinion
	{
		get
		{
			if (currentlySelectedMinion == null || currentlySelectedMinion.IsNull())
			{
				return null;
			}
			return currentlySelectedMinion;
		}
		set
		{
			currentlySelectedMinion = value;
			if (IsActive())
			{
				RefreshSelectedMinion();
				RefreshSkillWidgets();
				RefreshBoosters();
			}
		}
	}

	public override float GetSortKey()
	{
		if (base.isEditing)
		{
			return 50f;
		}
		return 20f;
	}

	protected override void OnSpawn()
	{
		ClusterManager.Instance.Subscribe(-1078710002, WorldRemoved);
	}

	protected override void OnActivate()
	{
		base.ConsumeMouseScroll = true;
		base.OnActivate();
		BuildMinions();
		RefreshAll();
		SortRows((active_sort_method == null) ? compareByMinion : active_sort_method);
		Components.LiveMinionIdentities.OnAdd += OnAddMinionIdentity;
		Components.LiveMinionIdentities.OnRemove += OnRemoveMinionIdentity;
		CloseButton.onClick += delegate
		{
			ManagementMenu.Instance.CloseAll();
		};
		MultiToggle multiToggle = dupeSortingToggle;
		multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, (System.Action)delegate
		{
			SortRows(compareByMinion);
		});
		MultiToggle multiToggle2 = moraleSortingToggle;
		multiToggle2.onClick = (System.Action)Delegate.Combine(multiToggle2.onClick, (System.Action)delegate
		{
			SortRows(compareByMorale);
		});
		MultiToggle multiToggle3 = experienceSortingToggle;
		multiToggle3.onClick = (System.Action)Delegate.Combine(multiToggle3.onClick, (System.Action)delegate
		{
			SortRows(compareByExperience);
		});
	}

	protected override void OnShow(bool show)
	{
		if (show)
		{
			if (CurrentlySelectedMinion == null && Components.LiveMinionIdentities.Count > 0)
			{
				CurrentlySelectedMinion = Components.LiveMinionIdentities.Items[0];
			}
			BuildMinions();
			if (boosterWidgets.Count == 0)
			{
				PopulateBoosters();
			}
			RefreshAll();
			SortRows((active_sort_method == null) ? compareByMinion : active_sort_method);
		}
		base.OnShow(show);
	}

	public void RefreshAll()
	{
		dirty = false;
		RefreshSkillWidgets();
		RefreshSelectedMinion();
		RefreshBoosters();
		linesPending = true;
	}

	private void RefreshSelectedMinion()
	{
		minionAnimWidget.SetPortraitAnimator(currentlySelectedMinion);
		RefreshProgressBars();
		RefreshHat();
	}

	public void GetMinionIdentity(IAssignableIdentity assignableIdentity, out MinionIdentity minionIdentity, out StoredMinionIdentity storedMinionIdentity)
	{
		if (assignableIdentity is MinionAssignablesProxy)
		{
			minionIdentity = ((MinionAssignablesProxy)assignableIdentity).GetTargetGameObject().GetComponent<MinionIdentity>();
			storedMinionIdentity = ((MinionAssignablesProxy)assignableIdentity).GetTargetGameObject().GetComponent<StoredMinionIdentity>();
		}
		else
		{
			minionIdentity = assignableIdentity as MinionIdentity;
			storedMinionIdentity = assignableIdentity as StoredMinionIdentity;
		}
	}

	private void RefreshProgressBars()
	{
		if (currentlySelectedMinion == null || currentlySelectedMinion.IsNull())
		{
			return;
		}
		GetMinionIdentity(currentlySelectedMinion, out var minionIdentity, out var storedMinionIdentity);
		HierarchyReferences component = expectationsTooltip.GetComponent<HierarchyReferences>();
		component.GetReference("Labels").gameObject.SetActive(minionIdentity != null);
		component.GetReference("MoraleBar").gameObject.SetActive(minionIdentity != null);
		component.GetReference("ExpectationBar").gameObject.SetActive(minionIdentity != null);
		component.GetReference("StoredMinion").gameObject.SetActive(minionIdentity == null);
		experienceProgressFill.gameObject.SetActive(minionIdentity != null);
		if (minionIdentity == null)
		{
			expectationsTooltip.SetSimpleTooltip(string.Format(UI.TABLESCREENS.INFORMATION_NOT_AVAILABLE_TOOLTIP, storedMinionIdentity.GetStorageReason(), currentlySelectedMinion.GetProperName()));
			experienceBarTooltip.SetSimpleTooltip(string.Format(UI.TABLESCREENS.INFORMATION_NOT_AVAILABLE_TOOLTIP, storedMinionIdentity.GetStorageReason(), currentlySelectedMinion.GetProperName()));
			EXPCount.text = "";
			duplicantLevelIndicator.text = UI.TABLESCREENS.NA;
			return;
		}
		MinionResume component2 = minionIdentity.GetComponent<MinionResume>();
		float num = MinionResume.CalculatePreviousExperienceBar(component2.TotalSkillPointsGained);
		float num2 = MinionResume.CalculateNextExperienceBar(component2.TotalSkillPointsGained);
		float fillAmount = (component2.TotalExperienceGained - num) / (num2 - num);
		EXPCount.text = Mathf.RoundToInt(component2.TotalExperienceGained - num) + " / " + Mathf.RoundToInt(num2 - num);
		duplicantLevelIndicator.text = component2.AvailableSkillpoints.ToString();
		experienceProgressFill.fillAmount = fillAmount;
		experienceBarTooltip.SetSimpleTooltip(string.Format(UI.SKILLS_SCREEN.EXPERIENCE_TOOLTIP, Mathf.RoundToInt(num2 - num) - Mathf.RoundToInt(component2.TotalExperienceGained - num)));
		AttributeInstance attributeInstance = Db.Get().Attributes.QualityOfLife.Lookup(component2);
		AttributeInstance attributeInstance2 = Db.Get().Attributes.QualityOfLifeExpectation.Lookup(component2);
		float num3 = 0f;
		float num4 = 0f;
		if (!string.IsNullOrEmpty(hoveredSkillID) && !component2.HasMasteredSkill(hoveredSkillID))
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			list.Add(hoveredSkillID);
			while (list.Count > 0)
			{
				for (int num5 = list.Count - 1; num5 >= 0; num5--)
				{
					if (!component2.HasMasteredSkill(list[num5]))
					{
						num3 += (float)(Db.Get().Skills.Get(list[num5]).tier + 1);
						if (component2.AptitudeBySkillGroup.ContainsKey(Db.Get().Skills.Get(list[num5]).skillGroup) && component2.AptitudeBySkillGroup[Db.Get().Skills.Get(list[num5]).skillGroup] > 0f)
						{
							num4 += 1f;
						}
						foreach (string priorSkill in Db.Get().Skills.Get(list[num5]).priorSkills)
						{
							list2.Add(priorSkill);
						}
					}
				}
				list.Clear();
				list.AddRange(list2);
				list2.Clear();
			}
		}
		float num6 = attributeInstance.GetTotalValue() + num4 / (attributeInstance2.GetTotalValue() + num3);
		float f = Mathf.Max(attributeInstance.GetTotalValue() + num4, attributeInstance2.GetTotalValue() + num3);
		while (moraleNotches.Count < Mathf.RoundToInt(f))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(moraleNotch, moraleNotch.transform.parent);
			gameObject.SetActive(value: true);
			moraleNotches.Add(gameObject);
		}
		while (moraleNotches.Count > Mathf.RoundToInt(f))
		{
			GameObject item = moraleNotches[moraleNotches.Count - 1];
			moraleNotches.Remove(item);
			UnityEngine.Object.Destroy(item);
		}
		for (int i = 0; i < moraleNotches.Count; i++)
		{
			if ((float)i < attributeInstance.GetTotalValue() + num4)
			{
				moraleNotches[i].GetComponentsInChildren<Image>()[1].color = moraleNotchColor;
			}
			else
			{
				moraleNotches[i].GetComponentsInChildren<Image>()[1].color = Color.clear;
			}
		}
		moraleProgressLabel.text = string.Concat(UI.SKILLS_SCREEN.MORALE, ": ", attributeInstance.GetTotalValue().ToString());
		if (num4 > 0f)
		{
			LocText locText = moraleProgressLabel;
			locText.text = locText.text + " + " + GameUtil.ApplyBoldString(GameUtil.ColourizeString(moraleNotchColor, num4.ToString()));
		}
		while (expectationNotches.Count < Mathf.RoundToInt(f))
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(expectationNotch, expectationNotch.transform.parent);
			gameObject2.SetActive(value: true);
			expectationNotches.Add(gameObject2);
		}
		while (expectationNotches.Count > Mathf.RoundToInt(f))
		{
			GameObject item2 = expectationNotches[expectationNotches.Count - 1];
			expectationNotches.Remove(item2);
			UnityEngine.Object.Destroy(item2);
		}
		for (int j = 0; j < expectationNotches.Count; j++)
		{
			if ((float)j < attributeInstance2.GetTotalValue() + num3)
			{
				if ((float)j < attributeInstance2.GetTotalValue())
				{
					expectationNotches[j].GetComponentsInChildren<Image>()[1].color = expectationNotchColor;
				}
				else
				{
					expectationNotches[j].GetComponentsInChildren<Image>()[1].color = expectationNotchProspectColor;
				}
			}
			else
			{
				expectationNotches[j].GetComponentsInChildren<Image>()[1].color = Color.clear;
			}
		}
		expectationsProgressLabel.text = string.Concat(UI.SKILLS_SCREEN.MORALE_EXPECTATION, ": ", attributeInstance2.GetTotalValue().ToString());
		if (num3 > 0f)
		{
			LocText locText2 = expectationsProgressLabel;
			locText2.text = locText2.text + " + " + GameUtil.ApplyBoldString(GameUtil.ColourizeString(expectationNotchColor, num3.ToString()));
		}
		if (num6 < 1f)
		{
			expectationWarning.SetActive(value: true);
			moraleWarning.SetActive(value: false);
		}
		else
		{
			expectationWarning.SetActive(value: false);
			moraleWarning.SetActive(value: true);
		}
		string text = "";
		List<Tuple<string, float>> list3 = new List<Tuple<string, float>>();
		text = text + GameUtil.ApplyBoldString(UI.SKILLS_SCREEN.MORALE) + ": " + attributeInstance.GetTotalValue() + "\n";
		for (int k = 0; k < attributeInstance.Modifiers.Count; k++)
		{
			list3.Add(new Tuple<string, float>(attributeInstance.Modifiers[k].GetDescription(), attributeInstance.Modifiers[k].Value));
		}
		List<Tuple<string, float>> list4 = list3.ToList();
		list4.Sort((Tuple<string, float> pair1, Tuple<string, float> pair2) => pair2.second.CompareTo(pair1.second));
		foreach (Tuple<string, float> item3 in list4)
		{
			text = text + "    • " + item3.first + ": " + ((item3.second > 0f) ? UIConstants.ColorPrefixGreen : UIConstants.ColorPrefixRed) + item3.second + UIConstants.ColorSuffix + "\n";
		}
		text += "\n";
		text = text + GameUtil.ApplyBoldString(UI.SKILLS_SCREEN.MORALE_EXPECTATION) + ": " + attributeInstance2.GetTotalValue() + "\n";
		for (int num7 = 0; num7 < attributeInstance2.Modifiers.Count; num7++)
		{
			text = text + "    • " + attributeInstance2.Modifiers[num7].GetDescription() + ": " + ((attributeInstance2.Modifiers[num7].Value > 0f) ? UIConstants.ColorPrefixRed : UIConstants.ColorPrefixGreen) + attributeInstance2.Modifiers[num7].GetFormattedString() + UIConstants.ColorSuffix + "\n";
		}
		expectationsTooltip.SetSimpleTooltip(text);
	}

	private Tag SelectedMinionModel()
	{
		GetMinionIdentity(currentlySelectedMinion, out var minionIdentity, out var storedMinionIdentity);
		if (minionIdentity != null)
		{
			return Db.Get().Personalities.Get(minionIdentity.personalityResourceId).model;
		}
		if (storedMinionIdentity != null)
		{
			return Db.Get().Personalities.Get(storedMinionIdentity.personalityResourceId).model;
		}
		return null;
	}

	private void RefreshHat()
	{
		if (currentlySelectedMinion == null || currentlySelectedMinion.IsNull())
		{
			return;
		}
		List<IListableOption> list = new List<IListableOption>();
		string text = "";
		GetMinionIdentity(currentlySelectedMinion, out var minionIdentity, out var storedMinionIdentity);
		if (minionIdentity != null)
		{
			MinionResume component = minionIdentity.GetComponent<MinionResume>();
			text = (string.IsNullOrEmpty(component.TargetHat) ? component.CurrentHat : component.TargetHat);
			foreach (MinionResume.HatInfo allHat in component.GetAllHats())
			{
				list.Add(new HatListable(allHat.Source, allHat.Hat));
			}
			hatDropDown.Initialize(list, OnHatDropEntryClick, hatDropDownSort, hatDropEntryRefreshAction, displaySelectedValueWhenClosed: false, currentlySelectedMinion);
		}
		else
		{
			text = (string.IsNullOrEmpty(storedMinionIdentity.targetHat) ? storedMinionIdentity.currentHat : storedMinionIdentity.targetHat);
		}
		hatDropDown.openButton.enabled = minionIdentity != null;
		selectedHat.transform.Find("Arrow").gameObject.SetActive(minionIdentity != null);
		selectedHat.sprite = Assets.GetSprite(string.IsNullOrEmpty(text) ? "hat_role_none" : text);
	}

	private void OnHatDropEntryClick(IListableOption skill, object data)
	{
		GetMinionIdentity(currentlySelectedMinion, out var minionIdentity, out var _);
		if (minionIdentity == null)
		{
			return;
		}
		MinionResume component = minionIdentity.GetComponent<MinionResume>();
		string text = "hat_role_none";
		if (skill != null)
		{
			selectedHat.sprite = Assets.GetSprite((skill as HatListable).hat);
			if (component != null)
			{
				text = (skill as HatListable).hat;
				component.SetHats(component.CurrentHat, text);
				if (component.OwnsHat(text))
				{
					new PutOnHatChore(component, Db.Get().ChoreTypes.SwitchHat);
				}
			}
		}
		else
		{
			selectedHat.sprite = Assets.GetSprite(text);
			if (component != null)
			{
				component.SetHats(component.CurrentHat, null);
				component.ApplyTargetHat();
			}
		}
		IAssignableIdentity assignableIdentity = minionIdentity.assignableProxy.Get();
		foreach (SkillMinionWidget sortableRow in sortableRows)
		{
			if (sortableRow.assignableIdentity == assignableIdentity)
			{
				sortableRow.RefreshHat(component.TargetHat);
			}
		}
	}

	private void hatDropEntryRefreshAction(DropDownEntry entry, object targetData)
	{
		if (entry.entryData != null)
		{
			HatListable hatListable = entry.entryData as HatListable;
			entry.image.sprite = Assets.GetSprite(hatListable.hat);
		}
	}

	private int hatDropDownSort(IListableOption a, IListableOption b, object targetData)
	{
		return 0;
	}

	public override void OnKeyDown(KButtonEvent e)
	{
		if (!e.Consumed && e.TryConsume(Action.DebugInstantBuildMode))
		{
			DebugHandler.ToggleInstantBuildMode();
		}
		else
		{
			base.OnKeyDown(e);
		}
	}

	private void Update()
	{
		if (dirty)
		{
			RefreshAll();
		}
		if (linesPending)
		{
			foreach (GameObject value in skillWidgets.Values)
			{
				value.GetComponent<SkillWidget>().RefreshLines();
			}
			linesPending = false;
		}
		if (KInputManager.currentControllerIsGamepad)
		{
			scrollRect.AnalogUpdate(KInputManager.steamInputInterpreter.GetSteamCameraMovement() * scrollSpeed);
		}
	}

	private void PopulateBoosters()
	{
		foreach (GameObject item in Assets.GetPrefabsWithTag(GameTags.BionicUpgrade))
		{
			Tag id = item.GetComponent<KPrefabID>().PrefabID();
			GameObject gameObject = Util.KInstantiate(boosterPrefab, boosterContentGrid, item.name);
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(value: true);
			HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
			boosterWidgets.Add(item.PrefabID(), component);
			Image reference = component.GetReference<Image>("Icon");
			reference.sprite = Def.GetUISprite(item).first;
			gameObject.GetComponentInChildren<LocText>().SetText(item.GetProperName());
			KButton reference2 = component.GetReference<KButton>("AssignmentIncrementButton");
			reference2.ClearOnClick();
			reference2.onClick += delegate
			{
				IncrementBoosterAssignment(id);
			};
			KButton reference3 = component.GetReference<KButton>("AssignmentDecrementButton");
			reference3.ClearOnClick();
			reference3.onClick += delegate
			{
				DecrementBoosterAssignment(id);
			};
			foreach (GameObject boosterSlotIcon in boosterSlotIcons)
			{
				Util.KDestroyGameObject(boosterSlotIcon);
			}
			boosterSlotIcons.Clear();
			for (int num = 0; num < 8; num++)
			{
				GameObject gameObject2 = Util.KInstantiateUI(boosterSlotIconPrefab, boosterSlotIconPrefab.transform.parent.gameObject);
				boosterSlotIcons.Add(gameObject2);
				int slotIdx = num;
				gameObject2.transform.GetChild(0).GetComponent<MultiToggle>().onClick = delegate
				{
					GetMinionIdentity(currentlySelectedMinion, out var minionIdentity, out var _);
					if (!(minionIdentity == null))
					{
						BionicUpgradesMonitor.Instance sMI = minionIdentity.GetSMI<BionicUpgradesMonitor.Instance>();
						sMI.upgradeComponentSlots[slotIdx].GetAssignableSlotInstance().Unassign();
						RefreshBoosters();
					}
				};
			}
		}
	}

	private void IncrementBoosterAssignment(Tag boosterType)
	{
		BionicUpgradeComponent bionicUpgradeComponent = FindAvailableBoosterOfType(boosterType);
		if (bionicUpgradeComponent != null)
		{
			bionicUpgradeComponent.Assign(CurrentlySelectedMinion);
		}
		RefreshBoosters();
	}

	private void DecrementBoosterAssignment(Tag boosterType)
	{
		GetMinionIdentity(currentlySelectedMinion, out var minionIdentity, out var _);
		BionicUpgradesMonitor.Instance sMI = minionIdentity.GetSMI<BionicUpgradesMonitor.Instance>();
		if (sMI == null)
		{
			bool flag = false;
			for (int num = sMI.upgradeComponentSlots.Length - 1; num >= 0; num--)
			{
				BionicUpgradesMonitor.UpgradeComponentSlot upgradeComponentSlot = sMI.upgradeComponentSlots[num];
				BionicUpgradeComponent assignedUpgradeComponent = upgradeComponentSlot.assignedUpgradeComponent;
				if (assignedUpgradeComponent != null && upgradeComponentSlot.assignedUpgradeComponent.PrefabID() == boosterType && upgradeComponentSlot.HasUpgradeInstalled && upgradeComponentSlot.AssignedUpgradeMatchesInstalledUpgrade)
				{
					upgradeComponentSlot.GetAssignableSlotInstance().Unassign();
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				for (int num2 = sMI.upgradeComponentSlots.Length - 1; num2 >= 0; num2--)
				{
					BionicUpgradesMonitor.UpgradeComponentSlot upgradeComponentSlot2 = sMI.upgradeComponentSlots[num2];
					BionicUpgradeComponent assignedUpgradeComponent2 = upgradeComponentSlot2.assignedUpgradeComponent;
					if (assignedUpgradeComponent2 != null && upgradeComponentSlot2.assignedUpgradeComponent.PrefabID() == boosterType)
					{
						upgradeComponentSlot2.GetAssignableSlotInstance().Unassign();
						flag = true;
						break;
					}
				}
			}
		}
		RefreshBoosters();
	}

	private BionicUpgradeComponent FindAvailableBoosterOfType(Tag boosterType)
	{
		GetMinionIdentity(currentlySelectedMinion, out var minionIdentity, out var _);
		if (minionIdentity == null)
		{
			return null;
		}
		List<Pickupable> list = ClusterManager.Instance.GetWorld(minionIdentity.GetMyWorldId()).worldInventory.CreatePickupablesList(boosterType);
		if (list == null || list.Count == 0)
		{
			return null;
		}
		list = list.FindAll((Pickupable match) => match.GetComponent<BionicUpgradeComponent>().assignee == null);
		if (list == null || list.Count == 0)
		{
			return null;
		}
		using (List<Pickupable>.Enumerator enumerator = list.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				Pickupable current = enumerator.Current;
				return current.GetComponent<BionicUpgradeComponent>();
			}
		}
		return null;
	}

	private void RefreshBoosters()
	{
		BionicUpgradesMonitor.Instance instance = null;
		GetMinionIdentity(currentlySelectedMinion, out var minionIdentity, out var _);
		bool flag = SelectedMinionModel() == GameTags.Minions.Models.Bionic && minionIdentity != null;
		if (flag)
		{
			instance = minionIdentity.GetSMI<BionicUpgradesMonitor.Instance>();
			if (instance == null)
			{
				flag = false;
			}
		}
		if (flag)
		{
			equippedBoostersHeaderLabel.SetText(GameUtil.SafeStringFormat(UI.SKILLS_SCREEN.ASSIGNED_BOOSTERS_HEADER, CurrentlySelectedMinion.GetProperName()));
			assignedBoostersCountLabel.SetText(GameUtil.SafeStringFormat(UI.SKILLS_SCREEN.ASSIGNED_BOOSTERS_COUNT_LABEL, instance.AssignedSlotCount, instance.UnlockedSlotCount));
			boosterPanel.SetActive(value: true);
			boosterHeader.SetActive(value: true);
			float canvasScale = GameScreenManager.Instance.ssOverlayCanvas.GetComponent<KCanvasScaler>().GetCanvasScale();
			float num = (float)Screen.height / canvasScale * 0.4f;
			float num2 = 96f;
			skillsContainer.rectTransform().sizeDelta = new Vector2(0f, -1f * (num + num2));
			boosterPanel.rectTransform().sizeDelta = new Vector2(0f, num);
			boosterHeader.rectTransform().anchoredPosition = new Vector2(0f, num);
			for (int i = 0; i < boosterSlotIcons.Count; i++)
			{
				BionicUpgradesMonitor.UpgradeComponentSlot upgradeComponentSlot = instance.upgradeComponentSlots[7 - i];
				boosterSlotIcons[i].SetActive(value: true);
				if (i >= instance.upgradeComponentSlots.Length || instance.upgradeComponentSlots[i].IsLocked)
				{
					boosterSlotIcons[i].GetComponent<Image>().sprite = Assets.GetSprite("bionicUpgradeSlotLocked");
					boosterSlotIcons[i].GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
					boosterSlotIcons[i].GetComponent<ToolTip>().SetSimpleTooltip(UI.SKILLS_SCREEN.BIONIC_UPGRADE_SLOT_LOCKED);
					boosterSlotIcons[i].transform.GetChild(0).gameObject.SetActive(value: false);
				}
				else if (instance.upgradeComponentSlots[i].assignedUpgradeComponent != null)
				{
					boosterSlotIcons[i].GetComponent<Image>().sprite = Def.GetUISprite(instance.upgradeComponentSlots[i].assignedUpgradeComponent.PrefabID()).first;
					boosterSlotIcons[i].GetComponent<ToolTip>().SetSimpleTooltip(instance.upgradeComponentSlots[i].assignedUpgradeComponent.GetProperName() + "\n\n" + UI.SKILLS_SCREEN.BIONIC_UPGRADE_SLOT_UNASSIGN);
					boosterSlotIcons[i].GetComponent<Image>().color = Color.white;
					boosterSlotIcons[i].transform.GetChild(0).gameObject.SetActive(value: true);
				}
				else
				{
					boosterSlotIcons[i].GetComponent<Image>().sprite = Assets.GetSprite("bionicUpgradeSlot");
					boosterSlotIcons[i].GetComponent<ToolTip>().SetSimpleTooltip(UI.SKILLS_SCREEN.BIONIC_UPGRADE_SLOT_AVAILABLE);
					boosterSlotIcons[i].GetComponent<Image>().color = Color.white;
					boosterSlotIcons[i].transform.GetChild(0).gameObject.SetActive(value: false);
				}
			}
			{
				foreach (KeyValuePair<Tag, HierarchyReferences> widget in boosterWidgets)
				{
					int num3 = 0;
					if (instance != null && instance.upgradeComponentSlots != null)
					{
						BionicUpgradesMonitor.UpgradeComponentSlot[] upgradeComponentSlots = instance.upgradeComponentSlots;
						foreach (BionicUpgradesMonitor.UpgradeComponentSlot upgradeComponentSlot2 in upgradeComponentSlots)
						{
							if (upgradeComponentSlot2.assignedUpgradeComponent != null && upgradeComponentSlot2.assignedUpgradeComponent.PrefabID() == widget.Key)
							{
								num3++;
							}
						}
					}
					GameObject prefab = Assets.GetPrefab(widget.Key);
					LocText reference = widget.Value.GetReference<LocText>("Label");
					string properName = prefab.GetProperName();
					reference.SetText(properName);
					float num4 = 0f;
					List<Pickupable> list = ClusterManager.Instance.GetWorld(minionIdentity.GetMyWorldId()).worldInventory.CreatePickupablesList(widget.Key);
					if (list != null && list.Count > 0)
					{
						list = list.FindAll((Pickupable match) => match.GetComponent<Assignable>().assignee == null);
						num4 = list.Count;
					}
					if (num4 > 0f)
					{
						widget.Value.GetReference<Image>("Icon").material = GlobalResources.Instance().AnimUIMaterial;
						widget.Value.GetReference<Image>("Icon").color = new Color(1f, 1f, 1f, 1f);
					}
					else
					{
						widget.Value.GetReference<Image>("Icon").material = GlobalResources.Instance().AnimMaterialUIDesaturated;
						widget.Value.GetReference<Image>("Icon").color = new Color(1f, 1f, 1f, 0.5f);
					}
					string text = GameUtil.SafeStringFormat(UI.SKILLS_SCREEN.AVAILABLE_BOOSTERS_LABEL, num4.ToString());
					LocText reference2 = widget.Value.GetReference<LocText>("AvailableLabel");
					reference2.SetText(text);
					reference2.color = ((num4 > 0f) ? new Color(0.53f, 0.83f, 0.53f) : new Color(0.65f, 0.65f, 0.65f));
					string text2 = GameUtil.SafeStringFormat(UI.SKILLS_SCREEN.ASSIGNED_BOOSTERS_LABEL, num3);
					widget.Value.GetReference<LocText>("EquipCountLabel").SetText(text2);
					ToolTip reference3 = widget.Value.GetReference<ToolTip>("Tooltip");
					reference3.SetSimpleTooltip("<b>" + prefab.GetProperName() + "</b>\n\n" + BionicUpgradeComponentConfig.UpgradesData[widget.Key].stateMachineDescription + "\n\n" + BionicUpgradeComponentConfig.GetColonyBoosterAssignmentString(widget.Key.Name));
					bool flag2 = instance.AssignedSlotCount < instance.UnlockedSlotCount;
					bool flag3 = num4 > 0f;
					bool flag4 = num3 > 0;
					MultiToggle component = widget.Value.gameObject.GetComponent<MultiToggle>();
					component.onClick = null;
					if (flag3 && flag2)
					{
						component.onClick = (System.Action)Delegate.Combine(component.onClick, (System.Action)delegate
						{
							IncrementBoosterAssignment(widget.Key);
						});
					}
				}
				return;
			}
		}
		boosterPanel.SetActive(value: false);
		boosterHeader.SetActive(value: false);
		skillsContainer.rectTransform().sizeDelta = new Vector2(0f, 0f);
	}

	private void RefreshSkillWidgets()
	{
		int num = 1;
		foreach (SkillGroup resource in Db.Get().SkillGroups.resources)
		{
			List<Skill> skillsBySkillGroup = GetSkillsBySkillGroup(resource.Id);
			if (skillsBySkillGroup.Count <= 0)
			{
				continue;
			}
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int i = 0; i < skillsBySkillGroup.Count; i++)
			{
				Skill skill = skillsBySkillGroup[i];
				if (skill.deprecated || !Game.IsCorrectDlcActiveForCurrentSave(skill))
				{
					continue;
				}
				if (!skillWidgets.ContainsKey(skill.Id))
				{
					while (skill.tier >= skillColumns.Count)
					{
						GameObject gameObject = Util.KInstantiateUI(Prefab_skillColumn, Prefab_tableLayout, force_active: true);
						skillColumns.Add(gameObject);
						HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
						if (skillColumns.Count % 2 == 0)
						{
							component.GetReference("BG").gameObject.SetActive(value: false);
						}
					}
					int value = 0;
					dictionary.TryGetValue(skill.tier, out value);
					dictionary[skill.tier] = value + 1;
					GameObject value2 = Util.KInstantiateUI(Prefab_skillWidget, skillColumns[skill.tier], force_active: true);
					skillWidgets.Add(skill.Id, value2);
				}
				skillWidgets[skill.Id].GetComponent<SkillWidget>().Refresh(skill.Id);
			}
			if (skillGroupRow.ContainsKey(resource.Id))
			{
				continue;
			}
			int num2 = 1;
			foreach (KeyValuePair<int, int> item in dictionary)
			{
				num2 = Mathf.Max(num2, item.Value);
			}
			skillGroupRow.Add(resource.Id, num);
			num += num2;
		}
		foreach (KeyValuePair<string, GameObject> skillWidget in skillWidgets)
		{
			if (Db.Get().Skills.Get(skillWidget.Key).requiredDuplicantModel != null)
			{
				skillWidget.Value.SetActive(Db.Get().Skills.Get(skillWidget.Key).requiredDuplicantModel == SelectedMinionModel());
			}
		}
		foreach (SkillMinionWidget sortableRow in sortableRows)
		{
			sortableRow.Refresh();
		}
		RefreshWidgetPositions();
	}

	public void HoverSkill(string skillID)
	{
		hoveredSkillID = skillID;
		if (delayRefreshRoutine != null)
		{
			StopCoroutine(delayRefreshRoutine);
			delayRefreshRoutine = null;
		}
		if (string.IsNullOrEmpty(hoveredSkillID))
		{
			delayRefreshRoutine = StartCoroutine(DelayRefreshProgressBars());
		}
		else
		{
			RefreshProgressBars();
		}
	}

	private IEnumerator DelayRefreshProgressBars()
	{
		yield return SequenceUtil.WaitForSecondsRealtime(0.1f);
		RefreshProgressBars();
	}

	public void RefreshWidgetPositions()
	{
		float num = 0f;
		foreach (KeyValuePair<string, GameObject> skillWidget in skillWidgets)
		{
			if (!(Db.Get().Skills.Get(skillWidget.Key).requiredDuplicantModel != SelectedMinionModel()))
			{
				float rowPosition = GetRowPosition(skillWidget.Key);
				num = Mathf.Max(rowPosition, num);
				skillWidget.Value.rectTransform().anchoredPosition = Vector2.down * rowPosition;
			}
		}
		num = Mathf.Max(num, layoutRowHeight);
		float num2 = layoutRowHeight;
		foreach (GameObject skillColumn in skillColumns)
		{
			skillColumn.GetComponent<LayoutElement>().minHeight = num + num2;
		}
		linesPending = true;
	}

	public float GetRowPosition(string skillID)
	{
		Skill skill = Db.Get().Skills.Get(skillID);
		int num = skillGroupRow[skill.skillGroup];
		int num2 = num;
		foreach (KeyValuePair<string, int> item in skillGroupRow)
		{
			if (item.Value <= num && SelectedMinionModel() != GetSkillsBySkillGroup(item.Key)[0].requiredDuplicantModel)
			{
				num2--;
			}
		}
		num = num2;
		List<Skill> skillsBySkillGroup = GetSkillsBySkillGroup(skill.skillGroup);
		int num3 = 0;
		foreach (Skill item2 in skillsBySkillGroup)
		{
			if (item2 == skill)
			{
				break;
			}
			if (item2.tier == skill.tier)
			{
				num3++;
			}
		}
		return layoutRowHeight * (num3 + num - 1);
	}

	private void OnAddMinionIdentity(MinionIdentity add)
	{
		BuildMinions();
		RefreshAll();
	}

	private void OnRemoveMinionIdentity(MinionIdentity remove)
	{
		if (remove != null)
		{
			if (CurrentlySelectedMinion == remove)
			{
				CurrentlySelectedMinion = null;
			}
			if (remove.assignableProxy.Get() == CurrentlySelectedMinion)
			{
				CurrentlySelectedMinion = null;
			}
		}
		BuildMinions();
		RefreshAll();
	}

	private void BuildMinions()
	{
		for (int num = sortableRows.Count - 1; num >= 0; num--)
		{
			sortableRows[num].DeleteObject();
		}
		sortableRows.Clear();
		foreach (MinionIdentity item in Components.LiveMinionIdentities.Items)
		{
			GameObject gameObject = Util.KInstantiateUI(Prefab_minion, Prefab_minionLayout, force_active: true);
			gameObject.GetComponent<SkillMinionWidget>().SetMinon(item.assignableProxy.Get());
			sortableRows.Add(gameObject.GetComponent<SkillMinionWidget>());
		}
		foreach (MinionStorage item2 in Components.MinionStorages.Items)
		{
			foreach (MinionStorage.Info item3 in item2.GetStoredMinionInfo())
			{
				if (item3.serializedMinion != null)
				{
					StoredMinionIdentity storedMinionIdentity = item3.serializedMinion.Get<StoredMinionIdentity>();
					GameObject gameObject2 = Util.KInstantiateUI(Prefab_minion, Prefab_minionLayout, force_active: true);
					gameObject2.GetComponent<SkillMinionWidget>().SetMinon(storedMinionIdentity.assignableProxy.Get());
					sortableRows.Add(gameObject2.GetComponent<SkillMinionWidget>());
				}
			}
		}
		foreach (int item4 in ClusterManager.Instance.GetWorldIDsSorted())
		{
			if (ClusterManager.Instance.GetWorld(item4).IsDiscovered)
			{
				AddWorldDivider(item4);
			}
		}
		foreach (KeyValuePair<int, GameObject> worldDivider in worldDividers)
		{
			worldDivider.Value.SetActive(ClusterManager.Instance.GetWorld(worldDivider.Key).IsDiscovered && DlcManager.FeatureClusterSpaceEnabled());
			Component reference = worldDivider.Value.GetComponent<HierarchyReferences>().GetReference("NobodyRow");
			reference.gameObject.SetActive(value: true);
			foreach (MinionAssignablesProxy item5 in Components.MinionAssignablesProxy)
			{
				if (item5.GetTargetGameObject().GetComponent<KMonoBehaviour>().GetMyWorld()
					.id == worldDivider.Key)
				{
					reference.gameObject.SetActive(value: false);
					break;
				}
			}
		}
		if (CurrentlySelectedMinion == null && Components.LiveMinionIdentities.Count > 0)
		{
			CurrentlySelectedMinion = Components.LiveMinionIdentities.Items[0];
		}
	}

	protected void AddWorldDivider(int worldId)
	{
		if (!worldDividers.ContainsKey(worldId))
		{
			GameObject gameObject = Util.KInstantiateUI(Prefab_worldDivider, Prefab_minionLayout, force_active: true);
			gameObject.GetComponentInChildren<Image>().color = ClusterManager.worldColors[worldId % ClusterManager.worldColors.Length];
			ClusterGridEntity component = ClusterManager.Instance.GetWorld(worldId).GetComponent<ClusterGridEntity>();
			gameObject.GetComponentInChildren<LocText>().SetText(component.Name);
			gameObject.GetComponent<HierarchyReferences>().GetReference<Image>("Icon").sprite = component.GetUISprite();
			worldDividers.Add(worldId, gameObject);
		}
	}

	private void WorldRemoved(object worldId)
	{
		int value = ((Boxed<int>)worldId).value;
		if (worldDividers.TryGetValue(value, out var value2))
		{
			UnityEngine.Object.Destroy(value2);
			worldDividers.Remove(value);
		}
	}

	public Vector2 GetSkillWidgetLineTargetPosition(string skillID)
	{
		return skillWidgets[skillID].GetComponent<SkillWidget>().lines_right.GetPosition();
	}

	public SkillWidget GetSkillWidget(string skill)
	{
		return skillWidgets[skill].GetComponent<SkillWidget>();
	}

	public List<Skill> GetSkillsBySkillGroup(string skillGrp)
	{
		List<Skill> list = new List<Skill>();
		foreach (Skill resource in Db.Get().Skills.resources)
		{
			if (resource.skillGroup == skillGrp && !resource.deprecated)
			{
				list.Add(resource);
			}
		}
		return list;
	}

	private void SelectSortToggle(MultiToggle toggle)
	{
		dupeSortingToggle.ChangeState(0);
		experienceSortingToggle.ChangeState(0);
		moraleSortingToggle.ChangeState(0);
		if (toggle != null)
		{
			if (activeSortToggle == toggle)
			{
				sortReversed = !sortReversed;
			}
			activeSortToggle = toggle;
		}
		activeSortToggle.ChangeState((!sortReversed) ? 1 : 2);
	}

	private void SortRows(Comparison<IAssignableIdentity> comparison)
	{
		active_sort_method = comparison;
		Dictionary<IAssignableIdentity, SkillMinionWidget> dictionary = new Dictionary<IAssignableIdentity, SkillMinionWidget>();
		foreach (SkillMinionWidget sortableRow in sortableRows)
		{
			dictionary.Add(sortableRow.assignableIdentity, sortableRow);
		}
		Dictionary<int, List<IAssignableIdentity>> minionsByWorld = ClusterManager.Instance.MinionsByWorld;
		sortableRows.Clear();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<int, List<IAssignableIdentity>> item2 in minionsByWorld)
		{
			dictionary2.Add(item2.Key, num);
			num++;
			List<IAssignableIdentity> list = new List<IAssignableIdentity>();
			foreach (IAssignableIdentity item3 in item2.Value)
			{
				list.Add(item3);
			}
			if (comparison != null)
			{
				list.Sort(comparison);
				if (sortReversed)
				{
					list.Reverse();
				}
			}
			num += list.Count;
			num2 += list.Count;
			for (int i = 0; i < list.Count; i++)
			{
				IAssignableIdentity key = list[i];
				SkillMinionWidget item = dictionary[key];
				sortableRows.Add(item);
			}
		}
		for (int j = 0; j < sortableRows.Count; j++)
		{
			sortableRows[j].gameObject.transform.SetSiblingIndex(j);
		}
		foreach (KeyValuePair<int, int> item4 in dictionary2)
		{
			worldDividers[item4.Key].transform.SetSiblingIndex(item4.Value);
		}
	}
}
