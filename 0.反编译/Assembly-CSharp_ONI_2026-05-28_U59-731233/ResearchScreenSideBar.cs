using System;
using System.Collections.Generic;
using Database;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class ResearchScreenSideBar : KScreen
{
	private enum CompletionState
	{
		All,
		Available,
		Completed
	}

	[Header("Containers")]
	[SerializeField]
	private GameObject queueContainer;

	[SerializeField]
	private GameObject projectsContainer;

	[SerializeField]
	private GameObject searchFiltersContainer;

	[Header("Prefabs")]
	[SerializeField]
	private GameObject headerTechTypePrefab;

	[SerializeField]
	private GameObject filterButtonPrefab;

	[SerializeField]
	private GameObject techWidgetRootPrefab;

	[SerializeField]
	private GameObject techWidgetRootAltPrefab;

	[SerializeField]
	private GameObject techItemPrefab;

	[SerializeField]
	private GameObject techWidgetUnlockedItemPrefab;

	[SerializeField]
	private GameObject techWidgetRowPrefab;

	[SerializeField]
	private GameObject techCategoryPrefab;

	[SerializeField]
	private GameObject techCategoryPrefabAlt;

	[Header("Other references")]
	[SerializeField]
	private KInputTextField searchBox;

	[SerializeField]
	private MultiToggle allFilter;

	[SerializeField]
	private MultiToggle availableFilter;

	[SerializeField]
	private MultiToggle completedFilter;

	[SerializeField]
	private ResearchScreen researchScreen;

	[SerializeField]
	private KButton clearSearchButton;

	[SerializeField]
	private Color evenRowColor;

	[SerializeField]
	private Color oddRowColor;

	private GraphicRaycaster raycaster;

	private CompletionState completionFilter;

	private Dictionary<string, bool> filterStates = new Dictionary<string, bool>();

	private Dictionary<string, bool> categoryExpanded = new Dictionary<string, bool>();

	private string currentSearchString = "";

	private string currentSearchStringUpper = "";

	private const string SEARCH_RESULTS_CATEGORY_ID = "SearchResults";

	private Dictionary<string, SearchUtil.TechCache> techCaches;

	private readonly Dictionary<string, SearchUtil.TechItemCache> techItemCaches = new Dictionary<string, SearchUtil.TechItemCache>();

	private readonly List<GameObject> orderedTechs = new List<GameObject>();

	private Dictionary<string, GameObject> queueTechs = new Dictionary<string, GameObject>();

	private Dictionary<string, GameObject> projectTechs = new Dictionary<string, GameObject>();

	private Dictionary<string, GameObject> projectCategories = new Dictionary<string, GameObject>();

	private Dictionary<string, GameObject> filterButtons = new Dictionary<string, GameObject>();

	private Dictionary<string, Dictionary<string, GameObject>> projectTechItems = new Dictionary<string, Dictionary<string, GameObject>>();

	private Dictionary<string, List<Tag>> filterPresets = new Dictionary<string, List<Tag>>
	{
		{
			"Oxygen",
			new List<Tag>()
		},
		{
			"Food",
			new List<Tag>()
		},
		{
			"Water",
			new List<Tag>()
		},
		{
			"Power",
			new List<Tag>()
		},
		{
			"Morale",
			new List<Tag>()
		},
		{
			"Ranching",
			new List<Tag>()
		},
		{
			"Filter",
			new List<Tag>()
		},
		{
			"Tile",
			new List<Tag>()
		},
		{
			"Transport",
			new List<Tag>()
		},
		{
			"Automation",
			new List<Tag>()
		},
		{
			"Medicine",
			new List<Tag>()
		},
		{
			"Rocket",
			new List<Tag>()
		}
	};

	private List<GameObject> QueuedActivations = new List<GameObject>();

	private List<GameObject> QueuedDeactivations = new List<GameObject>();

	public ButtonSoundPlayer soundPlayer;

	[SerializeField]
	private int activationPerFrame = 5;

	private Comparer<Tuple<GameObject, string>> techWidgetComparer;

	private bool evenRow = false;

	private Comparer<Tuple<GameObject, string>> TechWidgetComparer
	{
		get
		{
			if (techWidgetComparer == null)
			{
				techWidgetComparer = Comparer<Tuple<GameObject, string>>.Create(CompareTechScores);
			}
			return techWidgetComparer;
		}
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		raycaster = projectsContainer.GetComponent<GraphicRaycaster>();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		PopulateProjects();
		PopulateFilterButtons();
		RefreshCategoriesContentExpanded();
		RefreshWidgets();
		searchBox.OnValueChangesPaused = delegate
		{
			SetTextFilter(searchBox.text, suppressUpdate: false);
		};
		KInputTextField kInputTextField = searchBox;
		kInputTextField.onFocus = (System.Action)Delegate.Combine(kInputTextField.onFocus, (System.Action)delegate
		{
			base.isEditing = true;
			UISounds.PlaySound(UISounds.Sound.Find);
		});
		searchBox.onEndEdit.AddListener(delegate
		{
			base.isEditing = false;
		});
		clearSearchButton.onClick += delegate
		{
			ResetFilter();
		};
		ConfigCompletionFilters();
		base.ConsumeMouseScroll = true;
		Game.Instance.Subscribe(-107300940, UpdateProjectFilter);
	}

	private void Update()
	{
		for (int i = 0; i < Math.Min(QueuedActivations.Count, activationPerFrame); i++)
		{
			QueuedActivations[i].SetActive(value: true);
		}
		QueuedActivations.RemoveRange(0, Math.Min(QueuedActivations.Count, activationPerFrame));
		for (int j = 0; j < Math.Min(QueuedDeactivations.Count, activationPerFrame); j++)
		{
			QueuedDeactivations[j].SetActive(value: false);
		}
		QueuedDeactivations.RemoveRange(0, Math.Min(QueuedDeactivations.Count, activationPerFrame));
	}

	public override bool IsScreenActive()
	{
		return researchScreen.IsScreenActive();
	}

	private void ConfigCompletionFilters()
	{
		MultiToggle multiToggle = allFilter;
		multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, (System.Action)delegate
		{
			SetCompletionFilter(CompletionState.All, suppressUpdate: false);
		});
		MultiToggle multiToggle2 = completedFilter;
		multiToggle2.onClick = (System.Action)Delegate.Combine(multiToggle2.onClick, (System.Action)delegate
		{
			SetCompletionFilter(CompletionState.Completed, suppressUpdate: false);
		});
		MultiToggle multiToggle3 = availableFilter;
		multiToggle3.onClick = (System.Action)Delegate.Combine(multiToggle3.onClick, (System.Action)delegate
		{
			SetCompletionFilter(CompletionState.Available, suppressUpdate: false);
		});
		SetCompletionFilter(CompletionState.All, suppressUpdate: false);
	}

	private void SetCompletionFilter(CompletionState state, bool suppressUpdate)
	{
		completionFilter = state;
		allFilter.GetComponent<MultiToggle>().ChangeState((completionFilter == CompletionState.All) ? 1 : 0);
		completedFilter.GetComponent<MultiToggle>().ChangeState((completionFilter == CompletionState.Completed) ? 1 : 0);
		availableFilter.GetComponent<MultiToggle>().ChangeState((completionFilter == CompletionState.Available) ? 1 : 0);
		if (!suppressUpdate)
		{
			UpdateProjectFilter();
		}
	}

	public override float GetSortKey()
	{
		if (base.isEditing)
		{
			return 50f;
		}
		return 21f;
	}

	public override void OnKeyDown(KButtonEvent e)
	{
		if (researchScreen != null && (bool)researchScreen.canvas && !researchScreen.canvas.enabled)
		{
			return;
		}
		if (base.isEditing)
		{
			e.Consumed = true;
		}
		else if (!e.Consumed)
		{
			Vector2 vector = base.transform.rectTransform().InverseTransformPoint(KInputManager.GetMousePos());
			if (vector.x >= 0f && vector.x <= base.transform.rectTransform().rect.width && !e.TryConsume(Action.MouseRight) && !e.TryConsume(Action.MouseLeft) && !KInputManager.currentControllerIsGamepad && !e.TryConsume(Action.ZoomIn) && !e.TryConsume(Action.ZoomOut))
			{
			}
		}
	}

	protected override void OnShow(bool show)
	{
		base.OnShow(show);
		raycaster.enabled = show;
		RefreshWidgets();
	}

	public override void Show(bool show = true)
	{
		mouseOver = false;
		OnShow(show);
	}

	private void SetTextFilter(string newValue, bool suppressUpdate)
	{
		if (base.isEditing)
		{
			foreach (KeyValuePair<string, GameObject> filterButton in filterButtons)
			{
				filterStates[filterButton.Key] = false;
				filterButton.Value.GetComponent<MultiToggle>().ChangeState(0);
			}
		}
		bool flag = IsTextFilterActive();
		currentSearchString = newValue;
		currentSearchStringUpper = currentSearchString.ToUpper().Trim();
		if (IsTextFilterActive())
		{
			Transform reference = projectCategories["SearchResults"].GetComponent<HierarchyReferences>().GetReference<Transform>("Content");
			foreach (KeyValuePair<string, GameObject> projectTech in projectTechs)
			{
				projectTechs[projectTech.Key].transform.SetParent(reference);
			}
		}
		else if (flag)
		{
			foreach (KeyValuePair<string, GameObject> projectTech2 in projectTechs)
			{
				Transform reference2 = projectCategories[Db.Get().Techs.Get(projectTech2.Key).category].GetComponent<HierarchyReferences>().GetReference<Transform>("Content");
				projectTechs[projectTech2.Key].transform.SetParent(reference2);
			}
		}
		if (!suppressUpdate)
		{
			UpdateProjectFilter();
		}
	}

	private void UpdateProjectFilter(object data = null)
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		foreach (KeyValuePair<string, GameObject> projectCategory in projectCategories)
		{
			dictionary.Add(projectCategory.Key, value: false);
		}
		bool flag = IsTextFilterActive();
		if (flag)
		{
			dictionary["SearchResults"] = true;
			categoryExpanded["SearchResults"] = true;
		}
		RefreshProjectsActive();
		foreach (KeyValuePair<string, GameObject> projectTech in projectTechs)
		{
			if ((projectTech.Value.activeSelf || QueuedActivations.Contains(projectTech.Value)) && !QueuedDeactivations.Contains(projectTech.Value))
			{
				dictionary[Db.Get().Techs.Get(projectTech.Key).category] = !flag;
				categoryExpanded[Db.Get().Techs.Get(projectTech.Key).category] = true;
			}
		}
		foreach (KeyValuePair<string, bool> item in dictionary)
		{
			ChangeGameObjectActive(projectCategories[item.Key], item.Value);
		}
		RefreshCategoriesContentExpanded();
		foreach (GameObject orderedTech in orderedTechs)
		{
			orderedTech.transform.SetAsLastSibling();
		}
	}

	private int CompareTechScores(Tuple<GameObject, string> a, Tuple<GameObject, string> b)
	{
		int techMatchScore = GetTechMatchScore(a.second);
		int techMatchScore2 = GetTechMatchScore(b.second);
		int num = -techMatchScore.CompareTo(techMatchScore2);
		if (num != 0)
		{
			return num;
		}
		if (!IsTextFilterActive())
		{
			return num;
		}
		return techCaches[a.second].CompareTo(techCaches[b.second]);
	}

	private void RefreshProjectsActive()
	{
		if (projectTechItems.Count == 0)
		{
			return;
		}
		Techs techs = Db.Get().Techs;
		if (techCaches == null)
		{
			techCaches = SearchUtil.CacheTechs();
		}
		if (IsTextFilterActive())
		{
			foreach (KeyValuePair<string, SearchUtil.TechCache> techCache2 in techCaches)
			{
				try
				{
					techCache2.Value.Bind(currentSearchStringUpper);
				}
				catch (Exception ex)
				{
					KCrashReporter.ReportDevNotification("Fuzzy score bind failed", Environment.StackTrace, ex.Message);
					techCache2.Value.Reset();
				}
			}
		}
		else
		{
			foreach (KeyValuePair<string, SearchUtil.TechCache> techCache3 in techCaches)
			{
				techCache3.Value.Reset();
			}
		}
		for (int i = 0; i != techs.Count; i++)
		{
			Tech tech = (Tech)techs.GetResource(i);
			SearchUtil.TechCache techCache = techCaches[tech.Id];
			foreach (KeyValuePair<string, GameObject> item in projectTechItems[tech.Id])
			{
				int techItemMatchScore = GetTechItemMatchScore(techCache, item.Key);
				bool flag = SearchUtil.IsPassingScore(techItemMatchScore);
				HierarchyReferences component = item.Value.GetComponent<HierarchyReferences>();
				component.GetReference<LocText>("Label").color = (flag ? Color.white : Color.grey);
				component.GetReference<Image>("Icon").color = (flag ? Color.white : new Color(1f, 1f, 1f, 0.5f));
			}
		}
		ListPool<Tuple<int, int>, ResearchScreen>.PooledList pooledList = ListPool<Tuple<int, int>, ResearchScreen>.Allocate();
		for (int j = 0; j != techs.Count; j++)
		{
			Tech tech2 = (Tech)techs.GetResource(j);
			pooledList.Add(new Tuple<int, int>(j, tech2.tier));
		}
		pooledList.Sort((Tuple<int, int> a, Tuple<int, int> b) => a.second.CompareTo(b.second));
		ListPool<Tuple<GameObject, string>, ResearchScreenSideBar>.PooledList pooledList2 = ListPool<Tuple<GameObject, string>, ResearchScreenSideBar>.Allocate();
		foreach (Tuple<int, int> item2 in pooledList)
		{
			Tech tech3 = (Tech)techs.GetResource(item2.first);
			GameObject gameObject = projectTechs[tech3.Id];
			int techMatchScore = GetTechMatchScore(tech3.Id);
			bool flag2 = SearchUtil.IsPassingScore(techMatchScore);
			ChangeGameObjectActive(gameObject, flag2);
			researchScreen.GetEntry(tech3).UpdateFilterState(flag2);
			if (flag2)
			{
				Tuple<GameObject, string> tuple = new Tuple<GameObject, string>(gameObject, tech3.Id);
				int num = pooledList2.BinarySearch(tuple, TechWidgetComparer);
				if (num < 0)
				{
					num = ~num;
				}
				for (; num < pooledList2.Count && CompareTechScores(tuple, pooledList2[num]) == 0; num++)
				{
				}
				pooledList2.Insert(num, tuple);
			}
		}
		pooledList.Recycle();
		orderedTechs.Clear();
		foreach (Tuple<GameObject, string> item3 in pooledList2)
		{
			orderedTechs.Add(item3.first);
		}
		pooledList2.Recycle();
	}

	private void RefreshCategoriesContentExpanded()
	{
		foreach (KeyValuePair<string, GameObject> projectCategory in projectCategories)
		{
			projectCategory.Value.GetComponent<HierarchyReferences>().GetReference<RectTransform>("Content").gameObject.SetActive(categoryExpanded[projectCategory.Key]);
			projectCategory.Value.GetComponent<HierarchyReferences>().GetReference<MultiToggle>("Toggle").ChangeState(categoryExpanded[projectCategory.Key] ? 1 : 0);
		}
	}

	private void CreateCategory(string categoryID, string title = null)
	{
		GameObject gameObject = Util.KInstantiateUI(techCategoryPrefabAlt, projectsContainer, force_active: true);
		gameObject.name = categoryID;
		if (title == null)
		{
			title = Strings.Get("STRINGS.RESEARCH.TREES.TITLE" + categoryID.ToUpper());
		}
		gameObject.GetComponent<HierarchyReferences>().GetReference<LocText>("Label").SetText(title);
		categoryExpanded.Add(categoryID, value: false);
		projectCategories.Add(categoryID, gameObject);
		gameObject.GetComponent<HierarchyReferences>().GetReference<MultiToggle>("Toggle").onClick = delegate
		{
			categoryExpanded[categoryID] = !categoryExpanded[categoryID];
			RefreshCategoriesContentExpanded();
		};
	}

	private void PopulateProjects()
	{
		ListPool<Tuple<Tuple<string, GameObject>, int>, ResearchScreen>.PooledList pooledList = ListPool<Tuple<Tuple<string, GameObject>, int>, ResearchScreen>.Allocate();
		for (int i = 0; i < Db.Get().Techs.Count; i++)
		{
			Tech tech = (Tech)Db.Get().Techs.GetResource(i);
			if (!projectCategories.ContainsKey(tech.category))
			{
				CreateCategory(tech.category);
			}
			GameObject gameObject = SpawnTechWidget(tech.Id, projectCategories[tech.category]);
			pooledList.Add(new Tuple<Tuple<string, GameObject>, int>(new Tuple<string, GameObject>(tech.Id, gameObject), tech.tier));
			projectTechs.Add(tech.Id, gameObject);
			gameObject.GetComponent<ToolTip>().SetSimpleTooltip(tech.desc);
			MultiToggle component = gameObject.GetComponent<MultiToggle>();
			component.onEnter = (System.Action)Delegate.Combine(component.onEnter, (System.Action)delegate
			{
				researchScreen.TurnEverythingOff();
				researchScreen.GetEntry(tech).OnHover(entered: true, tech);
				soundPlayer.Play(1);
			});
			MultiToggle component2 = gameObject.GetComponent<MultiToggle>();
			component2.onExit = (System.Action)Delegate.Combine(component2.onExit, (System.Action)delegate
			{
				researchScreen.TurnEverythingOff();
			});
		}
		CreateCategory("SearchResults", UI.RESEARCHSCREEN.SEARCH_RESULTS_CATEGORY);
		foreach (KeyValuePair<string, GameObject> projectTech in projectTechs)
		{
			Transform reference = projectCategories[Db.Get().Techs.Get(projectTech.Key).category].GetComponent<HierarchyReferences>().GetReference<Transform>("Content");
			projectTechs[projectTech.Key].transform.SetParent(reference);
		}
		pooledList.Sort((Tuple<Tuple<string, GameObject>, int> a, Tuple<Tuple<string, GameObject>, int> b) => a.second.CompareTo(b.second));
		foreach (Tuple<Tuple<string, GameObject>, int> item in pooledList)
		{
			item.first.second.transform.SetAsLastSibling();
		}
		pooledList.Recycle();
	}

	private void PopulateFilterButtons()
	{
		foreach (KeyValuePair<string, List<Tag>> kvp in filterPresets)
		{
			GameObject gameObject = Util.KInstantiateUI(filterButtonPrefab, searchFiltersContainer, force_active: true);
			filterButtons.Add(kvp.Key, gameObject);
			filterStates.Add(kvp.Key, value: false);
			MultiToggle toggle = gameObject.GetComponent<MultiToggle>();
			LocText componentInChildren = gameObject.GetComponentInChildren<LocText>();
			StringEntry text = Strings.Get("STRINGS.UI.RESEARCHSCREEN.FILTER_BUTTONS." + kvp.Key.ToUpper());
			componentInChildren.SetText(text);
			MultiToggle multiToggle = toggle;
			multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, (System.Action)delegate
			{
				foreach (KeyValuePair<string, GameObject> filterButton in filterButtons)
				{
					if (filterButton.Key != kvp.Key)
					{
						filterStates[filterButton.Key] = false;
						filterButtons[filterButton.Key].GetComponent<MultiToggle>().ChangeState(filterStates[filterButton.Key] ? 1 : 0);
					}
				}
				filterStates[kvp.Key] = !filterStates[kvp.Key];
				toggle.ChangeState(filterStates[kvp.Key] ? 1 : 0);
				searchBox.text = (filterStates[kvp.Key] ? text.String : "");
			});
		}
	}

	public void RefreshQueue()
	{
	}

	private void RefreshWidgets()
	{
		List<TechInstance> researchQueue = Research.Instance.GetResearchQueue();
		foreach (KeyValuePair<string, GameObject> kvp in projectTechs)
		{
			if (Db.Get().Techs.Get(kvp.Key).IsComplete())
			{
				kvp.Value.GetComponent<MultiToggle>().ChangeState(2);
			}
			else if (researchQueue.Find((TechInstance match) => match.tech.Id == kvp.Key) != null)
			{
				kvp.Value.GetComponent<MultiToggle>().ChangeState(1);
			}
			else
			{
				kvp.Value.GetComponent<MultiToggle>().ChangeState(0);
			}
		}
	}

	private void RefreshWidgetProgressBars(string techID, GameObject widget)
	{
		HierarchyReferences component = widget.GetComponent<HierarchyReferences>();
		ResearchPointInventory progressInventory = Research.Instance.GetTechInstance(techID).progressInventory;
		int num = 0;
		for (int i = 0; i < Research.Instance.researchTypes.Types.Count; i++)
		{
			if (Research.Instance.GetTechInstance(techID).tech.costsByResearchTypeID.ContainsKey(Research.Instance.researchTypes.Types[i].id) && Research.Instance.GetTechInstance(techID).tech.costsByResearchTypeID[Research.Instance.researchTypes.Types[i].id] > 0f)
			{
				Transform child = component.GetReference<RectTransform>("BarRows").GetChild(1 + num);
				HierarchyReferences component2 = child.GetComponent<HierarchyReferences>();
				float num2 = progressInventory.PointsByTypeID[Research.Instance.researchTypes.Types[i].id] / Research.Instance.GetTechInstance(techID).tech.costsByResearchTypeID[Research.Instance.researchTypes.Types[i].id];
				RectTransform rectTransform = component2.GetReference<Image>("Bar").rectTransform;
				rectTransform.sizeDelta = new Vector2(rectTransform.parent.rectTransform().rect.width * num2, rectTransform.sizeDelta.y);
				component2.GetReference<LocText>("Label").SetText(progressInventory.PointsByTypeID[Research.Instance.researchTypes.Types[i].id] + "/" + Research.Instance.GetTechInstance(techID).tech.costsByResearchTypeID[Research.Instance.researchTypes.Types[i].id]);
				num++;
			}
		}
	}

	private GameObject SpawnTechWidget(string techID, GameObject parentContainer)
	{
		GameObject gameObject = Util.KInstantiateUI(techWidgetRootAltPrefab, parentContainer, force_active: true);
		HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
		gameObject.name = Db.Get().Techs.Get(techID).Name;
		component.GetReference<LocText>("Label").SetText(Db.Get().Techs.Get(techID).Name);
		if (!projectTechItems.ContainsKey(techID))
		{
			projectTechItems.Add(techID, new Dictionary<string, GameObject>());
		}
		RectTransform reference = component.GetReference<RectTransform>("UnlockContainer");
		foreach (TechItem unlockedItem in Db.Get().Techs.Get(techID).unlockedItems)
		{
			if (Game.IsCorrectDlcActiveForCurrentSave(unlockedItem))
			{
				GameObject gameObject2 = Util.KInstantiateUI(techItemPrefab, reference.gameObject, force_active: true);
				gameObject2.GetComponentsInChildren<Image>()[1].sprite = unlockedItem.UISprite();
				gameObject2.GetComponentsInChildren<LocText>()[0].SetText(unlockedItem.Name);
				MultiToggle component2 = gameObject2.GetComponent<MultiToggle>();
				component2.onClick = (System.Action)Delegate.Combine(component2.onClick, (System.Action)delegate
				{
					researchScreen.ZoomToTech(techID);
				});
				gameObject2.GetComponentsInChildren<Image>()[0].color = (evenRow ? evenRowColor : oddRowColor);
				evenRow = !evenRow;
				if (!projectTechItems[techID].ContainsKey(unlockedItem.Id))
				{
					projectTechItems[techID].Add(unlockedItem.Id, gameObject2);
				}
			}
		}
		MultiToggle component3 = gameObject.GetComponent<MultiToggle>();
		component3.onClick = (System.Action)Delegate.Combine(component3.onClick, (System.Action)delegate
		{
			researchScreen.ZoomToTech(techID);
		});
		return gameObject;
	}

	private void ChangeGameObjectActive(GameObject target, bool targetActiveState)
	{
		if (target.activeSelf == targetActiveState)
		{
			return;
		}
		if (targetActiveState)
		{
			QueuedActivations.Add(target);
			if (QueuedDeactivations.Contains(target))
			{
				QueuedDeactivations.Remove(target);
			}
		}
		else
		{
			QueuedDeactivations.Add(target);
			if (QueuedActivations.Contains(target))
			{
				QueuedActivations.Remove(target);
			}
		}
	}

	private bool IsTextFilterActive()
	{
		return !string.IsNullOrEmpty(currentSearchString);
	}

	private bool AnyFilterActive()
	{
		return completionFilter != CompletionState.All || IsTextFilterActive();
	}

	private int GetTechItemMatchScore(SearchUtil.TechCache techCache, string techItemID)
	{
		TechItem techItem = Db.Get().TechItems.Get(techItemID);
		if (!Game.IsCorrectDlcActiveForCurrentSave(techItem))
		{
			return 0;
		}
		switch (completionFilter)
		{
		case CompletionState.Available:
			if (techItem.IsComplete())
			{
				return 0;
			}
			if (!techItem.ParentTech.ArePrerequisitesComplete())
			{
				return 0;
			}
			break;
		case CompletionState.Completed:
			if (!techItem.IsComplete())
			{
				return 0;
			}
			break;
		}
		return IsTextFilterActive() ? techCache.techItems[techItemID].Score : 100;
	}

	private int GetTechMatchScore(string techID)
	{
		Tech tech = Db.Get().Techs.Get(techID);
		switch (completionFilter)
		{
		case CompletionState.Available:
			if (tech.IsComplete())
			{
				return 0;
			}
			if (!tech.ArePrerequisitesComplete())
			{
				return 0;
			}
			break;
		case CompletionState.Completed:
			if (!tech.IsComplete())
			{
				return 0;
			}
			break;
		}
		return IsTextFilterActive() ? techCaches[techID].Score : 100;
	}

	public void ResetFilter()
	{
		SetTextFilter("", suppressUpdate: true);
		searchBox.text = "";
		foreach (KeyValuePair<string, GameObject> filterButton in filterButtons)
		{
			filterStates[filterButton.Key] = false;
			filterButtons[filterButton.Key].GetComponent<MultiToggle>().ChangeState(filterStates[filterButton.Key] ? 1 : 0);
		}
		SetCompletionFilter(CompletionState.All, suppressUpdate: true);
		UpdateProjectFilter();
	}

	public void FocusSearchBox()
	{
		searchBox.Select();
	}

	public void SetSearch(string newSearch)
	{
		newSearch = UI.StripLinkFormatting(newSearch);
		searchBox.text = newSearch;
		SetTextFilter(newSearch, suppressUpdate: false);
	}
}
