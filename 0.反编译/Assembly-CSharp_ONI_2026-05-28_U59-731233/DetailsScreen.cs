#define ENABLE_PROFILER
using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class DetailsScreen : KTabMenu
{
	[Serializable]
	private struct Screens
	{
		public string name;

		public string displayName;

		public string tooltip;

		public Sprite icon;

		public TargetPanel screen;

		public int displayOrderPriority;

		public bool hideWhenDead;

		public HashedString focusInViewMode;

		[HideInInspector]
		public int tabIdx;
	}

	public enum SidescreenTabTypes
	{
		Config,
		Errands,
		Material,
		Blueprints
	}

	[Serializable]
	public class SidescreenTab
	{
		public SidescreenTabTypes type;

		public string Title_Key;

		public string Tooltip_Key;

		public Sprite Icon;

		public GameObject OverrideBody;

		public Func<GameObject, SidescreenTab, bool> ValidateTargetCallback;

		public System.Action OnClicked;

		[NonSerialized]
		public MultiToggle tabInstance;

		[NonSerialized]
		public GameObject bodyInstance;

		private HierarchyReferences bodyReferences;

		private const string bodyRef_Title = "Title";

		private const string bodyRef_TitleLabel = "TitleLabel";

		private const string bodyRef_NoConfigMessage = "NoConfigMessage";

		public bool IsVisible { get; private set; }

		public bool IsSelected { get; private set; }

		private void OnTabClicked()
		{
			OnClicked?.Invoke();
		}

		public void Initiate(GameObject originalTabInstance, GameObject originalBodyInstance, Action<SidescreenTab> on_tab_clicked_callback)
		{
			if (on_tab_clicked_callback != null)
			{
				OnClicked = delegate
				{
					on_tab_clicked_callback(this);
				};
			}
			originalBodyInstance.gameObject.SetActive(value: false);
			if (OverrideBody == null)
			{
				bodyInstance = UnityEngine.Object.Instantiate(originalBodyInstance);
				bodyInstance.name = type.ToString() + " Tab - body instance";
				bodyInstance.SetActive(value: true);
				bodyInstance.transform.SetParent(originalBodyInstance.transform.parent, worldPositionStays: false);
			}
			else
			{
				bodyInstance = OverrideBody;
			}
			bodyReferences = bodyInstance.GetComponent<HierarchyReferences>();
			originalTabInstance.gameObject.SetActive(value: false);
			if (tabInstance == null)
			{
				tabInstance = UnityEngine.Object.Instantiate(originalTabInstance.gameObject).GetComponent<MultiToggle>();
				tabInstance.name = type.ToString() + " Tab Instance";
				tabInstance.gameObject.SetActive(value: true);
				tabInstance.transform.SetParent(originalTabInstance.transform.parent, worldPositionStays: false);
				MultiToggle multiToggle = tabInstance;
				multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, new System.Action(OnTabClicked));
				HierarchyReferences component = tabInstance.GetComponent<HierarchyReferences>();
				component.GetReference<LocText>("label").SetText(Strings.Get(Title_Key));
				component.GetReference<Image>("icon").sprite = Icon;
				tabInstance.GetComponent<ToolTip>().SetSimpleTooltip(Strings.Get(Tooltip_Key));
			}
		}

		public void SetSelected(bool isSelected)
		{
			IsSelected = isSelected;
			tabInstance.ChangeState(isSelected ? 1 : 0);
			bodyInstance.SetActive(isSelected);
		}

		public void SetTitle(string title)
		{
			if (bodyReferences != null && bodyReferences.HasReference("TitleLabel"))
			{
				LocText reference = bodyReferences.GetReference<LocText>("TitleLabel");
				reference.SetText(title);
			}
		}

		public void SetTitleVisibility(bool visible)
		{
			if (bodyReferences != null && bodyReferences.HasReference("Title"))
			{
				Component reference = bodyReferences.GetReference("Title");
				reference.gameObject.SetActive(visible);
				reference.transform.parent.GetComponent<LayoutGroup>().padding.top = (visible ? 24 : 0);
			}
		}

		public void SetNoConfigMessageVisibility(bool visible)
		{
			if (bodyReferences != null && bodyReferences.HasReference("NoConfigMessage"))
			{
				Component reference = bodyReferences.GetReference("NoConfigMessage");
				reference.gameObject.SetActive(visible);
			}
		}

		public void RepositionTitle()
		{
			if (bodyReferences != null && bodyReferences.GetReference("Title") != null)
			{
				Component reference = bodyReferences.GetReference("Title");
				reference.transform.SetSiblingIndex(0);
			}
		}

		public void SetVisible(bool visible)
		{
			IsVisible = visible;
			tabInstance.gameObject.SetActive(visible);
			bodyInstance.SetActive(IsSelected && IsVisible);
		}

		public bool ValidateTarget(GameObject target)
		{
			if (target == null)
			{
				return false;
			}
			if (ValidateTargetCallback != null)
			{
				return ValidateTargetCallback(target, this);
			}
			return true;
		}
	}

	[Serializable]
	public class SideScreenRef
	{
		public string name;

		public SideScreenContent screenPrefab;

		public Vector2 offset;

		public SidescreenTabTypes tab = SidescreenTabTypes.Config;

		[HideInInspector]
		public SideScreenContent screenInstance;
	}

	public static DetailsScreen Instance;

	[SerializeField]
	private KButton CodexEntryButton;

	[SerializeField]
	private KButton PinResourceButton;

	[Header("Panels")]
	public Transform UserMenuPanel;

	[Header("Name Editing (disabled)")]
	[SerializeField]
	private KButton CloseButton;

	[Header("Tabs")]
	[SerializeField]
	private DetailTabHeader tabHeader;

	[SerializeField]
	private EditableTitleBar TabTitle;

	[SerializeField]
	private Screens[] screens;

	[SerializeField]
	private GameObject tabHeaderContainer;

	[Header("Side Screen Tabs")]
	[SerializeField]
	private SidescreenTab[] sidescreenTabs;

	[SerializeField]
	private GameObject sidescreenTabHeader;

	[SerializeField]
	private GameObject original_tab;

	[SerializeField]
	private GameObject original_tab_body;

	[Header("Side Screens")]
	[SerializeField]
	private GameObject sideScreen;

	[SerializeField]
	private List<SideScreenRef> sideScreens;

	[SerializeField]
	private LayoutElement tabBodyLayoutElement;

	[Header("Secondary Side Screens")]
	[SerializeField]
	private GameObject sideScreen2ContentBody;

	[SerializeField]
	private GameObject sideScreen2;

	[SerializeField]
	private LocText sideScreen2Title;

	private KScreen activeSideScreen2 = null;

	private Tag previousTargetID = null;

	private bool HasActivated;

	private SidescreenTabTypes selectedSidescreenTabID = SidescreenTabTypes.Config;

	private Dictionary<KScreen, KScreen> instantiatedSecondarySideScreens = new Dictionary<KScreen, KScreen>();

	private static readonly EventSystem.IntraObjectHandler<DetailsScreen> OnRefreshDataDelegate = new EventSystem.IntraObjectHandler<DetailsScreen>(delegate(DetailsScreen component, object data)
	{
		component.OnRefreshData(data);
	});

	private List<KeyValuePair<SideScreenRef, int>> sortedSideScreens = new List<KeyValuePair<SideScreenRef, int>>();

	private int setRocketTitleHandle = -1;

	public GameObject target { get; private set; }

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		SortScreenOrder();
		base.ConsumeMouseScroll = true;
		Debug.Assert(Instance == null);
		Instance = this;
		InitiateSidescreenTabs();
		DeactivateSideContent();
		Show(show: false);
		Subscribe(Game.Instance.gameObject, -1503271301, OnSelectObject);
		tabHeader.Init();
	}

	public bool CanObjectDisplayTabOfType(GameObject obj, SidescreenTabTypes type)
	{
		for (int i = 0; i < sidescreenTabs.Length; i++)
		{
			SidescreenTab sidescreenTab = sidescreenTabs[i];
			if (sidescreenTab.type == type)
			{
				return sidescreenTab.ValidateTarget(obj);
			}
		}
		return false;
	}

	public SidescreenTab GetTabOfType(SidescreenTabTypes type)
	{
		for (int i = 0; i < sidescreenTabs.Length; i++)
		{
			SidescreenTab sidescreenTab = sidescreenTabs[i];
			if (sidescreenTab.type == type)
			{
				return sidescreenTab;
			}
		}
		return null;
	}

	public void InitiateSidescreenTabs()
	{
		for (int i = 0; i < sidescreenTabs.Length; i++)
		{
			SidescreenTab sidescreenTab = sidescreenTabs[i];
			sidescreenTab.Initiate(original_tab, original_tab_body, delegate(SidescreenTab _tab)
			{
				SelectSideScreenTab(_tab.type);
			});
			switch (sidescreenTab.type)
			{
			case SidescreenTabTypes.Errands:
				sidescreenTab.ValidateTargetCallback = delegate(GameObject target, SidescreenTab _tab)
				{
					MinionIdentity component = target.GetComponent<MinionIdentity>();
					return component != null;
				};
				break;
			case SidescreenTabTypes.Material:
				sidescreenTab.ValidateTargetCallback = delegate(GameObject target, SidescreenTab _tab)
				{
					Reconstructable component = target.GetComponent<Reconstructable>();
					return component != null && component.AllowReconstruct;
				};
				break;
			case SidescreenTabTypes.Blueprints:
				sidescreenTab.ValidateTargetCallback = delegate(GameObject target, SidescreenTab _tab)
				{
					MinionIdentity component = target.GetComponent<MinionIdentity>();
					BuildingFacade component2 = target.GetComponent<BuildingFacade>();
					return component != null || component2 != null;
				};
				break;
			}
		}
	}

	private void OnSelectObject(object data)
	{
		if (data == null)
		{
			previouslyActiveTab = -1;
			SelectSideScreenTab(SidescreenTabTypes.Config);
			return;
		}
		KPrefabID component = ((GameObject)data).GetComponent<KPrefabID>();
		if (component == null || previousTargetID != component.PrefabID())
		{
			if (component != null && (bool)component.GetComponent<MinionIdentity>())
			{
				SelectSideScreenTab(SidescreenTabTypes.Errands);
			}
			else
			{
				SelectSideScreenTab(SidescreenTabTypes.Config);
			}
		}
		else
		{
			SelectSideScreenTab(selectedSidescreenTabID);
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		CodexEntryButton.onClick += CodexEntryButton_OnClick;
		PinResourceButton.onClick += PinResourceButton_OnClick;
		CloseButton.onClick += DeselectAndClose;
		TabTitle.OnNameChanged += OnNameChanged;
		TabTitle.OnStartedEditing += OnStartedEditing;
		sideScreen2.SetActive(value: false);
		Subscribe(-1514841199, OnRefreshDataDelegate);
	}

	private void OnStartedEditing()
	{
		base.isEditing = true;
		KScreenManager.Instance.RefreshStack();
	}

	private void OnNameChanged(string newName)
	{
		base.isEditing = false;
		if (string.IsNullOrEmpty(newName))
		{
			return;
		}
		MinionIdentity component = target.GetComponent<MinionIdentity>();
		UserNameable component2 = target.GetComponent<UserNameable>();
		ClustercraftExteriorDoor component3 = target.GetComponent<ClustercraftExteriorDoor>();
		CommandModule component4 = target.GetComponent<CommandModule>();
		if (component != null)
		{
			component.SetName(newName);
			if (ScheduleScreen.Instance != null)
			{
				ScheduleScreen.Instance.Trigger(1980521255);
			}
		}
		else if (component4 != null)
		{
			SpacecraftManager.instance.GetSpacecraftFromLaunchConditionManager(component4.GetComponent<LaunchConditionManager>()).SetRocketName(newName);
		}
		else if (component3 != null)
		{
			component3.GetTargetWorld().GetComponent<UserNameable>().SetName(newName);
		}
		else if (component2 != null)
		{
			component2.SetName(newName);
		}
		TabTitle.UpdateRenameTooltip(target);
	}

	protected override void OnDeactivate()
	{
		if (target != null && setRocketTitleHandle != -1)
		{
			target.Unsubscribe(setRocketTitleHandle);
		}
		setRocketTitleHandle = -1;
		DeactivateSideContent();
		base.OnDeactivate();
	}

	protected override void OnShow(bool show)
	{
		if (!show)
		{
			DeactivateSideContent();
		}
		else
		{
			MaskSideContent(hide: false);
			AudioMixer.instance.Start(AudioMixerSnapshots.Get().MenuOpenHalfEffect);
		}
		base.OnShow(show);
	}

	protected override void OnCmpDisable()
	{
		DeactivateSideContent();
		base.OnCmpDisable();
	}

	public override void OnKeyUp(KButtonEvent e)
	{
		if (!base.isEditing && target != null && PlayerController.Instance.ConsumeIfNotDragging(e, Action.MouseRight))
		{
			DeselectAndClose();
		}
	}

	private static Component GetComponent(GameObject go, string name)
	{
		Component component = null;
		Type type = Type.GetType(name);
		if (type != null)
		{
			return go.GetComponent(type);
		}
		return go.GetComponent(name);
	}

	private static bool IsExcludedPrefabTag(GameObject go, Tag[] excluded_tags)
	{
		if (excluded_tags == null || excluded_tags.Length == 0)
		{
			return false;
		}
		bool result = false;
		KPrefabID component = go.GetComponent<KPrefabID>();
		foreach (Tag tag in excluded_tags)
		{
			if (component.PrefabTag == tag)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private string CodexEntryButton_GetCodexId()
	{
		string text = "";
		Debug.Assert(target != null, "Details Screen has no target");
		KSelectable component = target.GetComponent<KSelectable>();
		DebugUtil.AssertArgs(component != null, "Details Screen target is not a KSelectable", target);
		CodexEntryRedirector component2 = component.GetComponent<CodexEntryRedirector>();
		BuildingUnderConstruction component3 = component.GetComponent<BuildingUnderConstruction>();
		CreatureBrain component4 = component.GetComponent<CreatureBrain>();
		PlantableSeed component5 = component.GetComponent<PlantableSeed>();
		ICellSelectionProxy component6 = component.GetComponent<ICellSelectionProxy>();
		if (component6 != null && component6.Element != null)
		{
			text = CodexCache.FormatLinkID(component6.Element.id.ToString());
		}
		else if (component2 != null && !string.IsNullOrEmpty(component2.CodexID))
		{
			text = CodexCache.FormatLinkID(component2.CodexID);
		}
		else if (component3 != null)
		{
			text = CodexCache.FormatLinkID(component3.Def.PrefabID);
		}
		else if (component4 != null)
		{
			text = CodexCache.FormatLinkID(component.PrefabID().ToString());
			text = text.Replace("BABY", "");
		}
		else if (component5 != null)
		{
			string linkID = component5.PrefabID().ToString();
			text = CodexCache.FormatLinkID(linkID);
		}
		else
		{
			text = UI.ExtractLinkID(component.GetProperName());
			if (string.IsNullOrEmpty(text))
			{
				text = CodexCache.FormatLinkID(component.PrefabID().ToString());
			}
		}
		if (CodexCache.entries.ContainsKey(text) || CodexCache.FindSubEntry(text) != null)
		{
			return text;
		}
		return "";
	}

	private void CodexEntryButton_Refresh()
	{
		string text = CodexEntryButton_GetCodexId();
		CodexEntryButton.isInteractable = text != "";
		CodexEntryButton.GetComponent<ToolTip>().SetSimpleTooltip(CodexEntryButton.isInteractable ? UI.TOOLTIPS.OPEN_CODEX_ENTRY : UI.TOOLTIPS.NO_CODEX_ENTRY);
	}

	public void CodexEntryButton_OnClick()
	{
		string text = CodexEntryButton_GetCodexId();
		if (text != "")
		{
			ManagementMenu.Instance.OpenCodexToEntry(text);
		}
	}

	private bool PinResourceButton_TryGetResourceTagAndProperName(out Tag targetTag, out string targetProperName)
	{
		KPrefabID component = target.GetComponent<KPrefabID>();
		if (component != null && ShouldUse(component.PrefabTag))
		{
			targetTag = component.PrefabTag;
			targetProperName = component.GetProperName();
			return true;
		}
		ICellSelectionProxy component2 = target.GetComponent<ICellSelectionProxy>();
		if (component2 != null && component2.Element != null && ShouldUse(component2.Element.tag))
		{
			targetTag = component2.Element.tag;
			targetProperName = component2.Element.name;
			return true;
		}
		targetTag = null;
		targetProperName = null;
		return false;
		static bool ShouldUse(Tag item)
		{
			foreach (Tag materialCategory in GameTags.MaterialCategories)
			{
				if (DiscoveredResources.Instance.GetDiscoveredResourcesFromTag(materialCategory).Contains(item))
				{
					return true;
				}
			}
			foreach (Tag calorieCategory in GameTags.CalorieCategories)
			{
				if (DiscoveredResources.Instance.GetDiscoveredResourcesFromTag(calorieCategory).Contains(item))
				{
					return true;
				}
			}
			foreach (Tag unitCategory in GameTags.UnitCategories)
			{
				if (DiscoveredResources.Instance.GetDiscoveredResourcesFromTag(unitCategory).Contains(item))
				{
					return true;
				}
			}
			return false;
		}
	}

	private void PinResourceButton_Refresh()
	{
		if (PinResourceButton_TryGetResourceTagAndProperName(out var targetTag, out var targetProperName))
		{
			bool flag = ClusterManager.Instance.activeWorld.worldInventory.pinnedResources.Contains(targetTag);
			if (!AllResourcesScreen.Instance.units.TryGetValue(targetTag, out var value))
			{
				value = GameUtil.MeasureUnit.quantity;
			}
			string arg = value switch
			{
				GameUtil.MeasureUnit.mass => GameUtil.GetFormattedMass(ClusterManager.Instance.activeWorld.worldInventory.GetAmount(targetTag, includeRelatedWorlds: false)), 
				GameUtil.MeasureUnit.quantity => GameUtil.GetFormattedUnits(ClusterManager.Instance.activeWorld.worldInventory.GetAmount(targetTag, includeRelatedWorlds: false)), 
				GameUtil.MeasureUnit.kcal => GameUtil.GetFormattedCalories(WorldResourceAmountTracker<RationTracker>.Get().CountAmountForItemWithID(targetTag.Name, ClusterManager.Instance.activeWorld.worldInventory)), 
				_ => "", 
			};
			PinResourceButton.gameObject.SetActive(value: true);
			PinResourceButton.GetComponent<ToolTip>().SetSimpleTooltip(string.Format(UI.TOOLTIPS.OPEN_RESOURCE_INFO, arg, targetProperName));
		}
		else
		{
			PinResourceButton.gameObject.SetActive(value: false);
		}
	}

	public void PinResourceButton_OnClick()
	{
		if (PinResourceButton_TryGetResourceTagAndProperName(out var _, out var targetProperName))
		{
			AllResourcesScreen.Instance.SetFilter(UI.StripLinkFormatting(targetProperName));
			AllResourcesScreen.Instance.Show();
		}
	}

	public void OnRefreshData(object obj)
	{
		RefreshTitle();
		for (int i = 0; i < tabs.Count; i++)
		{
			if (tabs[i].gameObject.activeInHierarchy)
			{
				tabs[i].Trigger(-1514841199, obj);
			}
		}
	}

	public void Refresh(GameObject go)
	{
		if (screens == null)
		{
			return;
		}
		if (target != go)
		{
			if (setRocketTitleHandle != -1)
			{
				target.Unsubscribe(setRocketTitleHandle);
				setRocketTitleHandle = -1;
			}
			if (target != null)
			{
				KPrefabID component = target.GetComponent<KPrefabID>();
				if (component != null)
				{
					previousTargetID = component.PrefabID();
				}
				else
				{
					previousTargetID = null;
				}
			}
		}
		target = go;
		sortedSideScreens.Clear();
		target.GetComponent<ICellSelectionProxy>()?.OnObjectSelected(null);
		UpdateTitle();
		tabHeader.RefreshTabDisplayForTarget(target);
		if (sideScreens != null && sideScreens.Count > 0)
		{
			bool flag = false;
			foreach (SideScreenRef sideScreen in sideScreens)
			{
				if (!sideScreen.screenPrefab.IsValidForTarget(target))
				{
					if (sideScreen.screenInstance != null && sideScreen.screenInstance.gameObject.activeSelf)
					{
						sideScreen.screenInstance.gameObject.SetActive(value: false);
					}
					continue;
				}
				flag = true;
				if (sideScreen.screenInstance == null)
				{
					SidescreenTab tabOfType = GetTabOfType(sideScreen.tab);
					sideScreen.screenInstance = Util.KInstantiateUI<SideScreenContent>(sideScreen.screenPrefab.gameObject, tabOfType.bodyInstance);
				}
				if (!this.sideScreen.activeSelf)
				{
					this.sideScreen.SetActive(value: true);
				}
				sideScreen.screenInstance.SetTarget(target);
				sideScreen.screenInstance.Show();
				int sideScreenSortOrder = sideScreen.screenInstance.GetSideScreenSortOrder();
				sortedSideScreens.Add(new KeyValuePair<SideScreenRef, int>(sideScreen, sideScreenSortOrder));
			}
			if (!flag)
			{
				if (!CanObjectDisplayTabOfType(target, SidescreenTabTypes.Material) && !CanObjectDisplayTabOfType(target, SidescreenTabTypes.Blueprints))
				{
					this.sideScreen.SetActive(value: false);
				}
				else
				{
					this.sideScreen.SetActive(value: true);
				}
			}
		}
		sortedSideScreens.Sort((KeyValuePair<SideScreenRef, int> x, KeyValuePair<SideScreenRef, int> y) => (x.Value <= y.Value) ? 1 : (-1));
		for (int num = 0; num < sortedSideScreens.Count; num++)
		{
			sortedSideScreens[num].Key.screenInstance.transform.SetSiblingIndex(num);
		}
		for (int num2 = 0; num2 < sidescreenTabs.Length; num2++)
		{
			SidescreenTab tab = sidescreenTabs[num2];
			tab.RepositionTitle();
			KeyValuePair<SideScreenRef, int> keyValuePair = sortedSideScreens.Find((KeyValuePair<SideScreenRef, int> t) => t.Key.tab == tab.type);
			tab.SetNoConfigMessageVisibility(keyValuePair.Key == null);
		}
		RefreshTitle();
	}

	public void RefreshTitle()
	{
		if (target == null)
		{
			return;
		}
		TabTitle.SetTitle(target.GetProperName());
		for (int i = 0; i < sidescreenTabs.Length; i++)
		{
			SidescreenTab tab = sidescreenTabs[i];
			if (tab.IsVisible)
			{
				KeyValuePair<SideScreenRef, int> keyValuePair = sortedSideScreens.Find((KeyValuePair<SideScreenRef, int> match) => match.Key.tab == tab.type);
				if (keyValuePair.Key != null)
				{
					tab.SetTitleVisibility(keyValuePair.Key.screenInstance.CheckShouldShowTopTitle == null || keyValuePair.Key.screenInstance.CheckShouldShowTopTitle());
					tab.SetTitle(keyValuePair.Key.screenInstance.GetTitle());
				}
				else
				{
					tab.SetTitle(UI.UISIDESCREENS.NOCONFIG.TITLE);
					tab.SetTitleVisibility(tab.type == SidescreenTabTypes.Config || tab.type == SidescreenTabTypes.Errands);
				}
			}
			if (tab.type == SidescreenTabTypes.Config)
			{
				if (target.GetComponent<MinionIdentity>() == null)
				{
					tab.Tooltip_Key = "STRINGS.UI.DETAILTABS.CONFIGURATION.TOOLTIP";
				}
				else
				{
					tab.Tooltip_Key = "STRINGS.UI.DETAILTABS.CONFIGURATION.TOOLTIP_DUPLICANT";
				}
				tab.tabInstance.GetComponent<ToolTip>().SetSimpleTooltip(Strings.Get(tab.Tooltip_Key));
			}
		}
	}

	private void SelectSideScreenTab(SidescreenTabTypes tabID)
	{
		selectedSidescreenTabID = tabID;
		RefreshSideScreenTabs();
	}

	private void RefreshSideScreenTabs()
	{
		int num = 1;
		for (int i = 0; i < sidescreenTabs.Length; i++)
		{
			SidescreenTab sidescreenTab = sidescreenTabs[i];
			bool flag = sidescreenTab.ValidateTarget(target);
			sidescreenTab.SetVisible(flag);
			sidescreenTab.SetSelected(selectedSidescreenTabID == sidescreenTab.type);
			num += (flag ? 1 : 0);
		}
		RefreshTitle();
		switch (selectedSidescreenTabID)
		{
		case SidescreenTabTypes.Material:
		{
			SidescreenTab tabOfType2 = GetTabOfType(SidescreenTabTypes.Material);
			tabOfType2.bodyInstance.GetComponentInChildren<DetailsScreenMaterialPanel>().SetTarget(target);
			break;
		}
		case SidescreenTabTypes.Blueprints:
		{
			SidescreenTab tabOfType = GetTabOfType(SidescreenTabTypes.Blueprints);
			CosmeticsPanel reference = tabOfType.bodyInstance.GetComponent<HierarchyReferences>().GetReference<CosmeticsPanel>("CosmeticsPanel");
			reference.SetTarget(target);
			reference.Refresh();
			CosmeticsPanel reference2 = tabOfType.OverrideBody.GetComponent<HierarchyReferences>().GetReference<CosmeticsPanel>("CosmeticsPanel");
			LayoutRebuilder.ForceRebuildLayoutImmediate(reference2.GetComponent<RectTransform>());
			float num2 = Mathf.Min(384f, reference2.GetComponent<RectTransform>().sizeDelta.y + 16f);
			tabOfType.OverrideBody.GetComponent<LayoutElement>().minHeight = num2;
			tabOfType.OverrideBody.GetComponent<LayoutElement>().preferredHeight = num2;
			break;
		}
		}
		sidescreenTabHeader.SetActive(num > 1);
	}

	public KScreen SetSecondarySideScreen(KScreen secondaryPrefab, string title)
	{
		ClearSecondarySideScreen();
		Profiler.BeginSample("SetSecondarySideScreen");
		if (instantiatedSecondarySideScreens.ContainsKey(secondaryPrefab))
		{
			activeSideScreen2 = instantiatedSecondarySideScreens[secondaryPrefab];
			activeSideScreen2.gameObject.SetActive(value: true);
		}
		else
		{
			activeSideScreen2 = KScreenManager.Instance.InstantiateScreen(secondaryPrefab.gameObject, sideScreen2ContentBody);
			activeSideScreen2.Activate();
			instantiatedSecondarySideScreens.Add(secondaryPrefab, activeSideScreen2);
		}
		sideScreen2Title.text = title;
		sideScreen2.SetActive(value: true);
		Profiler.BeginSample("EndSample");
		return activeSideScreen2;
	}

	public void ClearSecondarySideScreen()
	{
		Profiler.BeginSample("ClearSecondarySideScreen");
		if (activeSideScreen2 != null)
		{
			activeSideScreen2.gameObject.SetActive(value: false);
			activeSideScreen2 = null;
		}
		sideScreen2.SetActive(value: false);
		Profiler.BeginSample("EndSample");
	}

	public void DeactivateSideContent()
	{
		if (SideDetailsScreen.Instance != null && SideDetailsScreen.Instance.gameObject.activeInHierarchy)
		{
			SideDetailsScreen.Instance.Show(show: false);
		}
		if (sideScreens != null && sideScreens.Count > 0)
		{
			sideScreens.ForEach(delegate(SideScreenRef scn)
			{
				if (scn.screenInstance != null)
				{
					scn.screenInstance.ClearTarget();
					scn.screenInstance.Show(show: false);
				}
			});
		}
		SidescreenTab tabOfType = GetTabOfType(SidescreenTabTypes.Material);
		SidescreenTab tabOfType2 = GetTabOfType(SidescreenTabTypes.Blueprints);
		tabOfType.bodyInstance.GetComponentInChildren<DetailsScreenMaterialPanel>().SetTarget(null);
		tabOfType2.bodyInstance.GetComponentInChildren<CosmeticsPanel>().SetTarget(null);
		AudioMixer.instance.Stop(AudioMixerSnapshots.Get().MenuOpenHalfEffect);
		sideScreen.SetActive(value: false);
	}

	public void MaskSideContent(bool hide)
	{
		if (hide)
		{
			sideScreen.transform.localScale = Vector3.zero;
		}
		else
		{
			sideScreen.transform.localScale = Vector3.one;
		}
	}

	public void DeselectAndClose()
	{
		if (base.gameObject.activeInHierarchy)
		{
			KMonoBehaviour.PlaySound(GlobalAssets.GetSound("Back"));
		}
		if (GetActiveTab() != null)
		{
			GetActiveTab().SetTarget(null);
		}
		SelectTool.Instance.Select(null);
		ClusterMapSelectTool.Instance.Select(null);
		if (!(target == null))
		{
			target = null;
			previousTargetID = null;
			DeactivateSideContent();
			Show(show: false);
		}
	}

	private void SortScreenOrder()
	{
		Array.Sort(screens, (Screens x, Screens y) => x.displayOrderPriority.CompareTo(y.displayOrderPriority));
	}

	public void UpdatePortrait(GameObject target)
	{
		KSelectable component = target.GetComponent<KSelectable>();
		if (component == null)
		{
			return;
		}
		TabTitle.portrait.ClearPortrait();
		Building component2 = component.GetComponent<Building>();
		if ((bool)component2)
		{
			Sprite sprite = null;
			sprite = component2.Def.GetUISprite();
			if (sprite != null)
			{
				TabTitle.portrait.SetPortrait(sprite);
				return;
			}
		}
		MinionIdentity component3 = target.GetComponent<MinionIdentity>();
		if ((bool)component3)
		{
			TabTitle.SetPortrait(component.gameObject);
			return;
		}
		Edible component4 = target.GetComponent<Edible>();
		if (component4 != null)
		{
			KBatchedAnimController component5 = component4.GetComponent<KBatchedAnimController>();
			Sprite uISpriteFromMultiObjectAnim = Def.GetUISpriteFromMultiObjectAnim(component5.AnimFiles[0]);
			TabTitle.portrait.SetPortrait(uISpriteFromMultiObjectAnim);
			return;
		}
		PrimaryElement component6 = target.GetComponent<PrimaryElement>();
		if (component6 != null)
		{
			TabTitle.portrait.SetPortrait(Def.GetUISpriteFromMultiObjectAnim(ElementLoader.FindElementByHash(component6.ElementID).substance.anim));
			return;
		}
		CellSelectionObject component7 = target.GetComponent<CellSelectionObject>();
		if (component7 != null)
		{
			string animName = (component7.element.IsSolid ? "ui" : component7.element.substance.name);
			Sprite uISpriteFromMultiObjectAnim2 = Def.GetUISpriteFromMultiObjectAnim(component7.element.substance.anim, animName);
			TabTitle.portrait.SetPortrait(uISpriteFromMultiObjectAnim2);
			return;
		}
		ICellSelectionProxy component8 = target.GetComponent<ICellSelectionProxy>();
		if (component8 != null && component8.Element != null && component8.Element.substance != null)
		{
			string animName2 = (component8.Element.IsSolid ? "ui" : component8.Element.substance.name);
			Sprite uISpriteFromMultiObjectAnim3 = Def.GetUISpriteFromMultiObjectAnim(component8.Element.substance.anim, animName2);
			TabTitle.portrait.SetPortrait(uISpriteFromMultiObjectAnim3);
		}
	}

	public bool CompareTargetWith(GameObject compare)
	{
		return target == compare;
	}

	public void UpdateTitle()
	{
		CodexEntryButton_Refresh();
		PinResourceButton_Refresh();
		TabTitle.SetTitle(target.GetProperName());
		if (TabTitle != null)
		{
			TabTitle.SetTitle(target.GetProperName());
			MinionIdentity minionIdentity = null;
			UserNameable userNameable = null;
			ClustercraftExteriorDoor clustercraftExteriorDoor = null;
			CommandModule commandModule = null;
			if (target != null)
			{
				minionIdentity = target.gameObject.GetComponent<MinionIdentity>();
				userNameable = target.gameObject.GetComponent<UserNameable>();
				clustercraftExteriorDoor = target.gameObject.GetComponent<ClustercraftExteriorDoor>();
				commandModule = target.gameObject.GetComponent<CommandModule>();
			}
			if (minionIdentity != null)
			{
				TabTitle.SetSubText(minionIdentity.GetComponent<MinionResume>().GetSkillsSubtitle());
				TabTitle.SetUserEditable(editable: true);
			}
			else if (userNameable != null)
			{
				TabTitle.SetSubText("");
				TabTitle.SetUserEditable(editable: true);
			}
			else if (commandModule != null)
			{
				TrySetRocketTitle(commandModule);
				TabTitle.SetSubText(commandModule.GetComponent<BuildingComplete>().Def.Name);
			}
			else if (clustercraftExteriorDoor != null)
			{
				TrySetRocketTitle(clustercraftExteriorDoor);
				TabTitle.SetSubText(clustercraftExteriorDoor.GetComponent<BuildingComplete>().Def.Name);
			}
			else
			{
				TabTitle.SetSubText("");
				TabTitle.SetUserEditable(editable: false);
			}
			TabTitle.UpdateRenameTooltip(target);
		}
	}

	private void TrySetRocketTitle(ClustercraftExteriorDoor clusterCraftDoor)
	{
		if (clusterCraftDoor.HasTargetWorld())
		{
			WorldContainer targetWorld = clusterCraftDoor.GetTargetWorld();
			TabTitle.SetTitle(targetWorld.GetComponent<ClusterGridEntity>().Name);
			TabTitle.SetUserEditable(editable: true);
			clusterCraftDoor.GetComponent<KSelectable>().SetName(targetWorld.GetComponent<ClusterGridEntity>().Name);
			setRocketTitleHandle = -1;
		}
		else if (setRocketTitleHandle == -1)
		{
			setRocketTitleHandle = target.Subscribe(-71801987, delegate
			{
				OnRefreshData(null);
				target.Unsubscribe(setRocketTitleHandle);
				setRocketTitleHandle = -1;
			});
		}
	}

	private void TrySetRocketTitle(CommandModule commandModule)
	{
		if (commandModule != null)
		{
			string rocketName = SpacecraftManager.instance.GetSpacecraftFromLaunchConditionManager(commandModule.GetComponent<LaunchConditionManager>()).GetRocketName();
			commandModule.GetComponent<KSelectable>().SetName(rocketName);
			TabTitle.SetTitle(rocketName);
			TabTitle.SetUserEditable(editable: true);
		}
	}

	public TargetPanel GetActiveTab()
	{
		return tabHeader.ActivePanel;
	}
}
