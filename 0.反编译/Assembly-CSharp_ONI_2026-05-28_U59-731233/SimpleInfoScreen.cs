using System;
using System.Collections.Generic;
using System.Diagnostics;
using Klei.AI;
using ProcGen;
using STRINGS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleInfoScreen : DetailScreenTab, ISim4000ms, ISim1000ms
{
	[DebuggerDisplay("{item.item.Name}")]
	public class StatusItemEntry : IRenderEveryTick
	{
		private enum FadeStage
		{
			IN,
			WAIT,
			OUT
		}

		public StatusItemGroup.Entry item;

		public StatusItemCategory category;

		public Color color;

		public TextStyleSetting style;

		public Action<StatusItemEntry> onDestroy;

		private LayoutElement spacerLayout;

		private GameObject widget;

		private ToolTip toolTip;

		private TextStyleSetting tooltipStyle;

		private Image image;

		private LocText text;

		private KButton button;

		private FadeStage fadeStage;

		private float fade = 0f;

		private float fadeInTime = 0f;

		private float fadeOutTime = 1.8f;

		public Image GetImage => image;

		public StatusItemEntry(StatusItemGroup.Entry item, StatusItemCategory category, GameObject status_item_prefab, Transform parent, TextStyleSetting tooltip_style, Color color, TextStyleSetting style, bool skip_fade, Action<StatusItemEntry> onDestroy)
		{
			this.item = item;
			this.category = category;
			tooltipStyle = tooltip_style;
			this.onDestroy = onDestroy;
			this.color = color;
			this.style = style;
			widget = Util.KInstantiateUI(status_item_prefab, parent.gameObject);
			text = widget.GetComponentInChildren<LocText>(includeInactive: true);
			SetTextStyleSetting.ApplyStyle(text, style);
			toolTip = widget.GetComponentInChildren<ToolTip>(includeInactive: true);
			image = widget.GetComponentInChildren<Image>(includeInactive: true);
			item.SetIcon(image);
			widget.SetActive(value: true);
			toolTip.OnToolTip = OnToolTip;
			button = widget.GetComponentInChildren<KButton>();
			if (item.item.statusItemClickCallback != null)
			{
				button.onClick += OnClick;
			}
			else
			{
				button.enabled = false;
			}
			fadeStage = (skip_fade ? FadeStage.WAIT : FadeStage.IN);
			SimAndRenderScheduler.instance.Add(this);
			Refresh();
			SetColor();
		}

		internal void SetSprite(TintedSprite sprite)
		{
			if (sprite != null)
			{
				image.sprite = sprite.sprite;
			}
		}

		public int GetIndex()
		{
			return widget.transform.GetSiblingIndex();
		}

		public void SetIndex(int index)
		{
			widget.transform.SetSiblingIndex(index);
		}

		public void RenderEveryTick(float dt)
		{
			switch (fadeStage)
			{
			case FadeStage.IN:
			{
				fade = Mathf.Min(fade + Time.deltaTime / fadeInTime, 1f);
				float num2 = fade;
				SetColor(num2);
				if (fade >= 1f)
				{
					fadeStage = FadeStage.WAIT;
				}
				break;
			}
			case FadeStage.WAIT:
				break;
			case FadeStage.OUT:
			{
				float num = fade;
				SetColor(num);
				fade = Mathf.Max(fade - Time.deltaTime / fadeOutTime, 0f);
				if (fade <= 0f)
				{
					Destroy(immediate: true);
				}
				break;
			}
			}
		}

		private string OnToolTip()
		{
			item.ShowToolTip(toolTip, tooltipStyle);
			return "";
		}

		private void OnClick()
		{
			item.OnClick();
		}

		public void Refresh()
		{
			string name = item.GetName();
			if (name != text.text)
			{
				text.text = name;
				SetColor();
			}
		}

		private void SetColor(float alpha = 1f)
		{
			Color color = new Color(this.color.r, this.color.g, this.color.b, alpha);
			image.color = color;
			text.color = color;
		}

		public void Destroy(bool immediate)
		{
			if (toolTip != null)
			{
				toolTip.OnToolTip = null;
			}
			if (button != null && button.enabled)
			{
				button.onClick -= OnClick;
			}
			if (immediate)
			{
				if (onDestroy != null)
				{
					onDestroy(this);
				}
				SimAndRenderScheduler.instance.Remove(this);
				UnityEngine.Object.Destroy(widget);
			}
			else
			{
				fade = 0.5f;
				fadeStage = FadeStage.OUT;
			}
		}
	}

	private class StorageCollapsibleRowData
	{
		public int prefabHashCode;

		public IStorage[] storages;
	}

	private class StoredItemCategoryData
	{
		public float mass;

		public float massPerUnit;

		public string name;

		public Vector2 temperatureRanges;

		public int instancesFound = 0;

		public KPrefabID lastInstance;

		public PrimaryElement lastPEInstance;

		public bool usingUnits => massPerUnit > 1f;

		public StoredItemCategoryData(string name, float m, float massPerUnit)
		{
			temperatureRanges = new Vector2(float.MaxValue, float.MinValue);
			this.name = name;
			mass = m;
			this.massPerUnit = massPerUnit;
		}

		public void ClearData()
		{
			name = null;
			mass = 0f;
			massPerUnit = 1f;
			temperatureRanges = new Vector2(float.MaxValue, float.MinValue);
			instancesFound = 0;
			lastInstance = null;
			lastPEInstance = null;
		}
	}

	public GameObject iconLabelRow;

	public GameObject spacerRow;

	[SerializeField]
	private GameObject attributesLabelTemplate;

	[SerializeField]
	private GameObject attributesLabelButtonTemplate;

	[SerializeField]
	private DescriptorPanel DescriptorContentPrefab;

	[SerializeField]
	private GameObject VitalsPanelTemplate;

	[SerializeField]
	private GameObject StatusItemPrefab;

	[SerializeField]
	private Sprite statusWarningIcon;

	[SerializeField]
	private HierarchyReferences processConditionHeader;

	[SerializeField]
	private GameObject processConditionRow;

	[SerializeField]
	private Text StatusPanelCurrentActionLabel;

	[SerializeField]
	private GameObject bigIconLabelRow;

	[SerializeField]
	private TextStyleSetting ToolTipStyle_Property;

	[SerializeField]
	private TextStyleSetting StatusItemStyle_Main;

	[SerializeField]
	private TextStyleSetting StatusItemStyle_Other;

	[SerializeField]
	private Color statusItemTextColor_regular = Color.black;

	[SerializeField]
	private Color statusItemTextColor_old = new Color(0.8235294f, 0.8235294f, 0.8235294f);

	private CollapsibleDetailContentPanel statusItemPanel;

	private MinionVitalsPanel vitalsPanel;

	private CollapsibleDetailContentPanel fertilityPanel;

	private CollapsibleDetailContentPanel mooFertilityPanel;

	private CollapsibleDetailContentPanel rocketStatusContainer;

	private CollapsibleDetailContentPanel worldLifePanel;

	private CollapsibleDetailContentPanel worldElementsPanel;

	private CollapsibleDetailContentPanel worldBiomesPanel;

	private CollapsibleDetailContentPanel worldGeysersPanel;

	private CollapsibleDetailContentPanel worldMeteorShowersPanel;

	private CollapsibleDetailContentPanel spacePOIPanel;

	private CollapsibleDetailContentPanel spaceHexCellStoragePanel;

	private CollapsibleDetailContentPanel worldTraitsPanel;

	private CollapsibleDetailContentPanel processConditionContainer;

	private CollapsibleDetailContentPanel requirementsPanel;

	private CollapsibleDetailContentPanel effectsPanel;

	private CollapsibleDetailContentPanel stressPanel;

	private CollapsibleDetailContentPanel infoPanel;

	private CollapsibleDetailContentPanel movePanel;

	private DescriptorPanel effectsContent;

	private DescriptorPanel requirementContent;

	private RocketSimpleInfoPanel rocketSimpleInfoPanel;

	private SpacePOISimpleInfoPanel spaceSimpleInfoPOIPanel;

	private StarmapHexCellInventoryInfoPanel starmapHexCellStorageInfoPanel;

	private DetailsPanelDrawer stressDrawer;

	private bool TargetIsMinion;

	private GameObject lastTarget;

	private GameObject statusItemsFolder;

	private Dictionary<Tag, GameObject> lifeformRows = new Dictionary<Tag, GameObject>();

	private Dictionary<Tag, GameObject> biomeRows = new Dictionary<Tag, GameObject>();

	private Dictionary<Tag, GameObject> geyserRows = new Dictionary<Tag, GameObject>();

	private Dictionary<Tag, GameObject> meteorShowerRows = new Dictionary<Tag, GameObject>();

	private List<GameObject> worldTraitRows = new List<GameObject>();

	private List<GameObject> surfaceConditionRows = new List<GameObject>();

	private List<StatusItemEntry> statusItems = new List<StatusItemEntry>();

	private List<StatusItemEntry> oldStatusItems = new List<StatusItemEntry>();

	private List<GameObject> processConditionRows = new List<GameObject>();

	private static readonly EventSystem.IntraObjectHandler<SimpleInfoScreen> OnRefreshDataDelegate = new EventSystem.IntraObjectHandler<SimpleInfoScreen>(delegate(SimpleInfoScreen component, object data)
	{
		component.OnRefreshData(data);
	});

	private const string STORAGE_ROW_ID_PREFIX = "storage_";

	private const string STORAGE_GROUP_ROW_ID_PREFIX = "storage_group_";

	private const int MAXStoreItemNameCharacterCount = 15;

	private const string TRIMMED_STRING = "…";

	private static List<int> storageItemPrefabDataIndexesFound = new List<int>();

	public CollapsibleDetailContentPanel StoragePanel { get; private set; }

	public override bool IsValidForTarget(GameObject target)
	{
		return true;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		processConditionContainer = CreateCollapsableSection(UI.DETAILTABS.PROCESS_CONDITIONS.NAME);
		statusItemPanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_STATUS);
		statusItemPanel.Content.GetComponent<VerticalLayoutGroup>().padding.bottom = 10;
		statusItemPanel.scalerMask.hoverLock = true;
		statusItemsFolder = statusItemPanel.Content.gameObject;
		spaceSimpleInfoPOIPanel = new SpacePOISimpleInfoPanel(this);
		spacePOIPanel = CreateCollapsableSection();
		starmapHexCellStorageInfoPanel = new StarmapHexCellInventoryInfoPanel(this);
		spaceHexCellStoragePanel = CreateCollapsableSection();
		rocketSimpleInfoPanel = new RocketSimpleInfoPanel(this);
		rocketStatusContainer = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_ROCKET);
		vitalsPanel = Util.KInstantiateUI(VitalsPanelTemplate, base.gameObject).GetComponent<MinionVitalsPanel>();
		fertilityPanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_FERTILITY);
		mooFertilityPanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_MOO_FERTILITY);
		infoPanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_DESCRIPTION);
		requirementsPanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_REQUIREMENTS);
		requirementContent = Util.KInstantiateUI<DescriptorPanel>(DescriptorContentPrefab.gameObject, requirementsPanel.Content.gameObject);
		effectsPanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_EFFECTS);
		effectsContent = Util.KInstantiateUI<DescriptorPanel>(DescriptorContentPrefab.gameObject, effectsPanel.Content.gameObject);
		worldMeteorShowersPanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_METEORSHOWERS);
		worldElementsPanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_ELEMENTS);
		worldGeysersPanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_GEYSERS);
		worldTraitsPanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_WORLDTRAITS);
		worldBiomesPanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_BIOMES);
		worldLifePanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_LIFE);
		StoragePanel = CreateCollapsableSection();
		stressPanel = CreateCollapsableSection();
		stressDrawer = new DetailsPanelDrawer(attributesLabelTemplate, stressPanel.Content.gameObject);
		movePanel = CreateCollapsableSection(UI.DETAILTABS.SIMPLEINFO.GROUPNAME_MOVABLE);
		Subscribe(-1514841199, OnRefreshDataDelegate);
	}

	protected override void OnSelectTarget(GameObject target)
	{
		base.OnSelectTarget(target);
		Subscribe(target, -1697596308, TriggerRefreshStorage);
		Subscribe(target, -1197125120, TriggerRefreshStorage);
		Subscribe(target, 1059811075, OnBreedingChanceChanged);
		Subscribe(target, 1105317911, OnMooSongChanceChanged);
		KSelectable component = target.GetComponent<KSelectable>();
		if (component != null)
		{
			StatusItemGroup statusItemGroup = component.GetStatusItemGroup();
			if (statusItemGroup != null)
			{
				statusItemGroup.OnAddStatusItem = (Action<StatusItemGroup.Entry, StatusItemCategory>)Delegate.Combine(statusItemGroup.OnAddStatusItem, new Action<StatusItemGroup.Entry, StatusItemCategory>(OnAddStatusItem));
				statusItemGroup.OnRemoveStatusItem = (Action<StatusItemGroup.Entry, bool>)Delegate.Combine(statusItemGroup.OnRemoveStatusItem, new Action<StatusItemGroup.Entry, bool>(OnRemoveStatusItem));
				foreach (StatusItemGroup.Entry item in statusItemGroup)
				{
					if (item.category != null && item.category.Id == "Main")
					{
						DoAddStatusItem(item, item.category);
					}
				}
				foreach (StatusItemGroup.Entry item2 in statusItemGroup)
				{
					if (item2.category == null || item2.category.Id != "Main")
					{
						DoAddStatusItem(item2, item2.category);
					}
				}
			}
		}
		statusItemPanel.gameObject.SetActive(value: true);
		statusItemPanel.scalerMask.UpdateSize();
		Refresh(force: true);
		RefreshWorldPanel();
		RefreshProcessConditionsPanel();
		spaceSimpleInfoPOIPanel.Refresh(spacePOIPanel, selectedTarget);
		starmapHexCellStorageInfoPanel.Refresh(spaceHexCellStoragePanel, selectedTarget);
	}

	public override void OnDeselectTarget(GameObject target)
	{
		base.OnDeselectTarget(target);
		if (target != null)
		{
			Unsubscribe(target, -1697596308, TriggerRefreshStorage);
			Unsubscribe(target, -1197125120, TriggerRefreshStorage);
			Unsubscribe(target, 1059811075, OnBreedingChanceChanged);
			Unsubscribe(target, 1105317911, OnMooSongChanceChanged);
		}
		KSelectable component = target.GetComponent<KSelectable>();
		if (!(component != null))
		{
			return;
		}
		StatusItemGroup statusItemGroup = component.GetStatusItemGroup();
		if (statusItemGroup == null)
		{
			return;
		}
		statusItemGroup.OnAddStatusItem = (Action<StatusItemGroup.Entry, StatusItemCategory>)Delegate.Remove(statusItemGroup.OnAddStatusItem, new Action<StatusItemGroup.Entry, StatusItemCategory>(OnAddStatusItem));
		statusItemGroup.OnRemoveStatusItem = (Action<StatusItemGroup.Entry, bool>)Delegate.Remove(statusItemGroup.OnRemoveStatusItem, new Action<StatusItemGroup.Entry, bool>(OnRemoveStatusItem));
		foreach (StatusItemEntry statusItem in statusItems)
		{
			statusItem.Destroy(immediate: true);
		}
		statusItems.Clear();
		foreach (StatusItemEntry oldStatusItem in oldStatusItems)
		{
			oldStatusItem.onDestroy = null;
			oldStatusItem.Destroy(immediate: true);
		}
		oldStatusItems.Clear();
	}

	private void OnStorageChange(object data)
	{
		RefreshStoragePanel(StoragePanel, selectedTarget);
	}

	private void OnBreedingChanceChanged(object data)
	{
		RefreshFertilityPanel(fertilityPanel, selectedTarget);
	}

	private void OnMooSongChanceChanged(object data)
	{
		RefreshMooSongPanel(fertilityPanel, selectedTarget);
	}

	private void OnAddStatusItem(StatusItemGroup.Entry status_item, StatusItemCategory category)
	{
		DoAddStatusItem(status_item, category);
	}

	private void DoAddStatusItem(StatusItemGroup.Entry status_item, StatusItemCategory category, bool show_immediate = false)
	{
		if (status_item.item.showInHoverCardOnly)
		{
			return;
		}
		GameObject gameObject = statusItemsFolder;
		Color color = ((status_item.item.notificationType == NotificationType.BadMinor || status_item.item.notificationType == NotificationType.Bad || status_item.item.notificationType == NotificationType.DuplicantThreatening) ? ((Color)GlobalAssets.Instance.colorSet.statusItemBad) : ((status_item.item.notificationType == NotificationType.Event) ? ((Color)GlobalAssets.Instance.colorSet.statusItemEvent) : ((status_item.item.notificationType != NotificationType.MessageImportant) ? statusItemTextColor_regular : ((Color)GlobalAssets.Instance.colorSet.statusItemMessageImportant))));
		TextStyleSetting style = ((category == Db.Get().StatusItemCategories.Main) ? StatusItemStyle_Main : StatusItemStyle_Other);
		StatusItemEntry statusItemEntry = new StatusItemEntry(status_item, category, StatusItemPrefab, gameObject.transform, ToolTipStyle_Property, color, style, show_immediate, OnStatusItemDestroy);
		statusItemEntry.SetSprite(status_item.item.sprite);
		if (category != null)
		{
			int num = -1;
			List<StatusItemEntry> list = oldStatusItems.FindAll((StatusItemEntry e) => e.category == category);
			foreach (StatusItemEntry item in list)
			{
				num = item.GetIndex();
				item.Destroy(immediate: true);
				oldStatusItems.Remove(item);
			}
			if (category == Db.Get().StatusItemCategories.Main)
			{
				num = 0;
			}
			if (num != -1)
			{
				statusItemEntry.SetIndex(num);
			}
		}
		statusItems.Add(statusItemEntry);
	}

	private void OnRemoveStatusItem(StatusItemGroup.Entry status_item, bool immediate = false)
	{
		DoRemoveStatusItem(status_item, immediate);
	}

	private void DoRemoveStatusItem(StatusItemGroup.Entry status_item, bool destroy_immediate = false)
	{
		for (int i = 0; i < statusItems.Count; i++)
		{
			if (statusItems[i].item.item == status_item.item)
			{
				StatusItemEntry statusItemEntry = statusItems[i];
				statusItems.RemoveAt(i);
				oldStatusItems.Add(statusItemEntry);
				statusItemEntry.Destroy(destroy_immediate);
				break;
			}
		}
	}

	private void OnStatusItemDestroy(StatusItemEntry item)
	{
		oldStatusItems.Remove(item);
	}

	private void OnRefreshData(object obj)
	{
		Refresh();
	}

	protected override void Refresh(bool force = false)
	{
		if (selectedTarget != lastTarget || force)
		{
			lastTarget = selectedTarget;
		}
		int count = statusItems.Count;
		statusItemPanel.gameObject.SetActive(count > 0);
		for (int i = 0; i < count; i++)
		{
			statusItems[i].Refresh();
		}
		RefreshStressPanel(stressPanel, selectedTarget);
		RefreshStoragePanel(StoragePanel, selectedTarget);
		RefreshMovePanel(movePanel, selectedTarget);
		RefreshFertilityPanel(fertilityPanel, selectedTarget);
		RefreshMooSongPanel(mooFertilityPanel, selectedTarget);
		RefreshRequirementsAndEffectsPanels(requirementsPanel, effectsPanel, selectedTarget, requirementContent, effectsContent);
		RefreshInfoPanel(infoPanel, selectedTarget);
		vitalsPanel.Refresh(selectedTarget);
		rocketSimpleInfoPanel.Refresh(rocketStatusContainer, selectedTarget);
	}

	public void Sim1000ms(float dt)
	{
		if (selectedTarget != null && selectedTarget.GetComponent<IProcessConditionSet>() != null)
		{
			RefreshProcessConditionsPanel();
		}
	}

	public void Sim4000ms(float dt)
	{
		RefreshWorldPanel();
		spaceSimpleInfoPOIPanel.Refresh(spacePOIPanel, selectedTarget);
		starmapHexCellStorageInfoPanel.Refresh(spaceHexCellStoragePanel, selectedTarget);
	}

	private static void RefreshInfoPanel(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
	{
		string text = "";
		string text2 = "";
		MinionIdentity component = targetEntity.GetComponent<MinionIdentity>();
		InfoDescription component2 = targetEntity.GetComponent<InfoDescription>();
		BuildingComplete component3 = targetEntity.GetComponent<BuildingComplete>();
		BuildingUnderConstruction component4 = targetEntity.GetComponent<BuildingUnderConstruction>();
		Edible component5 = targetEntity.GetComponent<Edible>();
		PrimaryElement component6 = targetEntity.GetComponent<PrimaryElement>();
		ICellSelectionProxy component7 = targetEntity.GetComponent<ICellSelectionProxy>();
		if ((bool)component)
		{
			text = "";
		}
		else
		{
			if ((bool)component2)
			{
				text = component2.description;
				text2 = component2.effect;
			}
			else if (component3 != null)
			{
				text = component3.DescFlavour + "\n\n" + component3.Desc;
			}
			else if (component4 != null)
			{
				text = component4.DescFlavour + "\n\n" + component4.Desc;
			}
			else if (component5 != null)
			{
				EdiblesManager.FoodInfo foodInfo = component5.FoodInfo;
				text += string.Format(UI.GAMEOBJECTEFFECTS.CALORIES, GameUtil.GetFormattedCalories(foodInfo.CaloriesPerUnit));
			}
			else if (component7 != null && component7.Element != null)
			{
				text = component7.Element.FullDescription(addHardnessColor: false);
			}
			else if (component6 != null)
			{
				Element element = ElementLoader.FindElementByHash(component6.ElementID);
				text = ((element != null) ? element.FullDescription(addHardnessColor: false) : "");
			}
			if (!string.IsNullOrEmpty(text))
			{
				targetPanel.SetLabel("Description", text, "");
			}
			bool flag = !string.IsNullOrEmpty(text2) && text2 != "\n";
			string text3 = "\n" + text2;
			if (flag)
			{
				targetPanel.SetLabel("Flavour", text3, "");
			}
			string[] roomClassForObject = CodexEntryGenerator.GetRoomClassForObject(targetEntity);
			if (roomClassForObject != null)
			{
				string text4 = string.Concat("\n", CODEX.HEADERS.BUILDINGTYPE, ":");
				foreach (string text5 in roomClassForObject)
				{
					text4 = text4 + "\n    • " + text5;
				}
				targetPanel.SetLabel("RoomClass", text4, "");
			}
		}
		targetPanel.Commit();
	}

	private static void RefreshRequirementsAndEffectsPanels(CollapsibleDetailContentPanel requirementsTargetPanel, CollapsibleDetailContentPanel effectsTargetPanel, GameObject targetEntity, DescriptorPanel requirementContent, DescriptorPanel effectsContent)
	{
		MinionIdentity component = targetEntity.GetComponent<MinionIdentity>();
		if (component != null)
		{
			requirementsTargetPanel.SetActive(active: false);
			effectsTargetPanel.SetActive(active: false);
			return;
		}
		WiltCondition component2 = targetEntity.GetComponent<WiltCondition>();
		CreatureBrain component3 = targetEntity.GetComponent<CreatureBrain>();
		bool flag = component2 != null || component3 != null;
		BuildingUnderConstruction component4 = targetEntity.GetComponent<BuildingUnderConstruction>();
		GameObject buildingComplete = (component4 ? component4.Def.BuildingComplete : targetEntity);
		ListPool<(ElementConverter, List<Descriptor>), SimpleInfoScreen>.PooledList pooledList = ListPool<(ElementConverter, List<Descriptor>), SimpleInfoScreen>.Allocate();
		ListPool<Descriptor, SimpleInfoScreen>.PooledList pooledList2 = ListPool<Descriptor, SimpleInfoScreen>.Allocate();
		ListPool<Descriptor, SimpleInfoScreen>.PooledList pooledList3 = ListPool<Descriptor, SimpleInfoScreen>.Allocate();
		GameUtil.PartitionBuildingDescriptors(buildingComplete, simpleInfoScreen: true, out var _, pooledList, pooledList2, pooledList3, out var hasConverterReqs);
		if (flag)
		{
			requirementsTargetPanel.SetActive(active: false);
		}
		else
		{
			ListPool<Descriptor, SimpleInfoScreen>.PooledList pooledList4 = ListPool<Descriptor, SimpleInfoScreen>.Allocate();
			GameUtil.BuildPartitionedRequirements(pooledList4, pooledList2, pooledList, hasConverterReqs);
			bool flag2 = pooledList4.Count > 0;
			requirementContent.gameObject.SetActive(flag2);
			if (flag2)
			{
				requirementContent.SetDescriptors(pooledList4);
			}
			requirementsTargetPanel.SetActive(flag2);
			pooledList4.Recycle();
		}
		ListPool<Descriptor, SimpleInfoScreen>.PooledList pooledList5 = ListPool<Descriptor, SimpleInfoScreen>.Allocate();
		GameUtil.BuildPartitionedEffects(pooledList5, pooledList3, pooledList);
		bool flag3 = pooledList5.Count > 0;
		effectsContent.gameObject.SetActive(flag3);
		if (flag3)
		{
			effectsContent.SetDescriptors(pooledList5);
		}
		effectsTargetPanel.SetActive(targetEntity != null && flag3);
		pooledList5.Recycle();
		pooledList2.Recycle();
		pooledList3.Recycle();
		pooledList.Recycle();
	}

	private static void RefreshFertilityPanel(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
	{
		FertilityMonitor.Instance sMI = targetEntity.GetSMI<FertilityMonitor.Instance>();
		if (sMI != null)
		{
			int num = 0;
			foreach (FertilityMonitor.BreedingChance breedingChance in sMI.breedingChances)
			{
				List<FertilityModifier> forTag = Db.Get().FertilityModifiers.GetForTag(breedingChance.egg);
				if (forTag.Count > 0)
				{
					string text = "";
					foreach (FertilityModifier item in forTag)
					{
						text += string.Format(UI.DETAILTABS.EGG_CHANCES.CHANCE_MOD_FORMAT, item.GetTooltip());
					}
					targetPanel.SetLabel("breeding_" + num++, string.Format(UI.DETAILTABS.EGG_CHANCES.CHANCE_FORMAT, breedingChance.egg.ProperName(), GameUtil.GetFormattedPercent(breedingChance.weight * 100f)), string.Format(UI.DETAILTABS.EGG_CHANCES.CHANCE_FORMAT_TOOLTIP, breedingChance.egg.ProperName(), GameUtil.GetFormattedPercent(breedingChance.weight * 100f), text));
				}
				else
				{
					targetPanel.SetLabel("breeding_" + num++, string.Format(UI.DETAILTABS.EGG_CHANCES.CHANCE_FORMAT, breedingChance.egg.ProperName(), GameUtil.GetFormattedPercent(breedingChance.weight * 100f)), string.Format(UI.DETAILTABS.EGG_CHANCES.CHANCE_FORMAT_TOOLTIP_NOMOD, breedingChance.egg.ProperName(), GameUtil.GetFormattedPercent(breedingChance.weight * 100f)));
				}
			}
		}
		targetPanel.Commit();
	}

	private static void RefreshMooSongPanel(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
	{
		BeckoningMonitor.Instance sMI = targetEntity.GetSMI<BeckoningMonitor.Instance>();
		if (sMI != null)
		{
			int num = 0;
			foreach (BeckoningMonitor.SongChance songChance in sMI.songChances)
			{
				List<MooSongModifier> forTag = Db.Get().MooSongModifiers.GetForTag(songChance.meteorID);
				if (forTag.Count > 0)
				{
					string text = "";
					foreach (MooSongModifier item in forTag)
					{
						text += string.Format(UI.DETAILTABS.MOO_SONG_CHANCES.CHANCE_MOD_FORMAT, item.GetTooltip());
					}
					targetPanel.SetLabel("breeding_" + num++, string.Format(UI.DETAILTABS.MOO_SONG_CHANCES.CHANCE_FORMAT, songChance.meteorID.ProperName(), GameUtil.GetFormattedPercent(songChance.weight * 100f)), string.Format(UI.DETAILTABS.MOO_SONG_CHANCES.CHANCE_FORMAT_TOOLTIP, songChance.meteorID.ProperName(), GameUtil.GetFormattedPercent(songChance.weight * 100f), text));
				}
				else
				{
					targetPanel.SetLabel("breeding_" + num++, string.Format(UI.DETAILTABS.MOO_SONG_CHANCES.CHANCE_FORMAT, songChance.meteorID.ProperName(), GameUtil.GetFormattedPercent(songChance.weight * 100f)), string.Format(UI.DETAILTABS.MOO_SONG_CHANCES.CHANCE_FORMAT_TOOLTIP_NOMOD, songChance.meteorID.ProperName(), GameUtil.GetFormattedPercent(songChance.weight * 100f)));
				}
			}
		}
		targetPanel.Commit();
	}

	private void TriggerRefreshStorage(object data = null)
	{
		RefreshStoragePanel(StoragePanel, selectedTarget);
	}

	private static void RefreshStoragePanel(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
	{
		if (targetEntity == null)
		{
			targetPanel.gameObject.SetActive(value: false);
			targetPanel.Commit();
			return;
		}
		IStorage[] componentsInChildren = targetEntity.GetComponentsInChildren<IStorage>();
		if (componentsInChildren == null)
		{
			targetPanel.gameObject.SetActive(value: false);
			targetPanel.Commit();
			return;
		}
		componentsInChildren = Array.FindAll(componentsInChildren, (IStorage n) => n.ShouldShowInUI());
		if (componentsInChildren.Length == 0)
		{
			targetPanel.gameObject.SetActive(value: false);
			targetPanel.Commit();
			return;
		}
		string title = ((targetEntity.GetComponent<MinionIdentity>() != null) ? UI.DETAILTABS.DETAILS.GROUPNAME_MINION_CONTENTS : UI.DETAILTABS.DETAILS.GROUPNAME_CONTENTS);
		targetPanel.gameObject.SetActive(value: true);
		targetPanel.SetTitle(title);
		DictionaryPool<int, StoredItemCategoryData, SimpleInfoScreen>.PooledDictionary pooledDictionary = DictionaryPool<int, StoredItemCategoryData, SimpleInfoScreen>.Allocate();
		storageItemPrefabDataIndexesFound.Clear();
		IStorage[] array = componentsInChildren;
		foreach (IStorage storage in array)
		{
			foreach (GameObject item in storage.GetItems())
			{
				if (item == null)
				{
					continue;
				}
				KPrefabID component = item.GetComponent<KPrefabID>();
				PrimaryElement component2 = item.GetComponent<PrimaryElement>();
				if (!(component2 != null) || component2.Mass != 0f)
				{
					int hashCode = component.GetHashCode();
					float mass = component2.Mass;
					if (!pooledDictionary.TryGetValue(hashCode, out var value))
					{
						value = (pooledDictionary[hashCode] = new StoredItemCategoryData(component.GetProperName(), 0f, component2.MassPerUnit));
						storageItemPrefabDataIndexesFound.Add(hashCode);
					}
					value.mass += mass;
					value.temperatureRanges.x = Mathf.Min(value.temperatureRanges.x, component2.Temperature);
					value.temperatureRanges.y = Mathf.Max(value.temperatureRanges.y, component2.Temperature);
					value.instancesFound++;
					value.lastInstance = component;
					value.lastPEInstance = component2;
				}
			}
		}
		int num2 = 0;
		foreach (int item2 in storageItemPrefabDataIndexesFound)
		{
			StoredItemCategoryData storedItemCategoryData2 = pooledDictionary[item2];
			int prefabHashCode = item2;
			string text = "";
			string tooltip = "";
			string temperatureText = "";
			string massText = "";
			text = storedItemCategoryData2.name;
			if (storedItemCategoryData2.instancesFound == 1)
			{
				ForgeNameAndTooltipForStoredItem(storedItemCategoryData2.lastInstance, storedItemCategoryData2.lastPEInstance, out text, out temperatureText, out massText, out tooltip, trim: false);
				text = "• " + text;
				KSelectable itemSelectable = storedItemCategoryData2.lastInstance.GetComponent<KSelectable>();
				DetailLabelWithButton detailLabelWithButton = targetPanel.SetLabelWithButton("storage_" + num2, text, temperatureText, massText, tooltip, delegate
				{
					SelectTool.Instance.Select(itemSelectable);
				});
				detailLabelWithButton.transform.SetAsFirstSibling();
			}
			else
			{
				massText = (storedItemCategoryData2.usingUnits ? GameUtil.GetFormattedUnits(storedItemCategoryData2.mass / storedItemCategoryData2.massPerUnit) : GameUtil.GetFormattedMass(storedItemCategoryData2.mass));
				DetailCollapsableLabel detailCollapsableLabel = targetPanel.SetCollapsableLabel("storage_group_" + prefabHashCode, text, massText, tooltip, new StorageCollapsibleRowData
				{
					prefabHashCode = prefabHashCode,
					storages = componentsInChildren
				}, OnStorageCollapsibleRowExpanded, OnStorageCollapsibleRowCollapsed);
			}
			storedItemCategoryData2.ClearData();
			num2++;
		}
		if (num2 == 0)
		{
			targetPanel.SetLabel("storage_empty", UI.DETAILTABS.DETAILS.STORAGE_EMPTY, "");
		}
		pooledDictionary.Recycle();
		targetPanel.Commit();
	}

	private static string GetIconsForItemName(string itemName, KPrefabID item, PrimaryElement pe, int maxIconsAllowed)
	{
		string text = itemName;
		bool flag = maxIconsAllowed <= 0;
		char c = ' ';
		if (!flag && item.HasTag(GameTags.RotModifierTags.Refrigerated))
		{
			text = text + c + UI.DETAILTABS.TEXTICONDATA.REFRIGERATED.ICON;
			flag = --maxIconsAllowed <= 0;
		}
		else if (!flag && item.HasTag(GameTags.RotModifierTags.DeepFrozen))
		{
			text = text + c + UI.DETAILTABS.TEXTICONDATA.DEEPFROZEN.ICON;
			flag = --maxIconsAllowed <= 0;
		}
		if (!flag && item.HasTag(GameTags.SpicedFood))
		{
			text = text + c + UI.DETAILTABS.TEXTICONDATA.SPICEDFOOD.ICON;
			flag = --maxIconsAllowed <= 0;
		}
		if (!flag && item.HasTag(GameTags.RotModifierTags.Fresh))
		{
			text = text + c + UI.DETAILTABS.TEXTICONDATA.FRESH.ICON;
			flag = --maxIconsAllowed <= 0;
		}
		else if (!flag && item.HasTag(GameTags.RotModifierTags.Stale))
		{
			text = text + c + UI.DETAILTABS.TEXTICONDATA.STALE.ICON;
			flag = --maxIconsAllowed <= 0;
		}
		if (!flag && pe.DiseaseIdx != byte.MaxValue)
		{
			Disease disease = Db.Get().Diseases[pe.DiseaseIdx];
			text = text + c + UI.DETAILTABS.TEXTICONDATA.GERMS.ICON;
			flag = --maxIconsAllowed <= 0;
		}
		if (flag)
		{
			text += "…";
		}
		return text;
	}

	private static string GetIconsLegendForItem(KPrefabID item, PrimaryElement pe, Rottable.Instance rottable, bool addTabs = false)
	{
		string text = "";
		string text2 = (addTabs ? "\n  " : "\n");
		char c = ' ';
		if (item.HasTag(GameTags.SpicedFood))
		{
			text = text + text2 + UI.DETAILTABS.TEXTICONDATA.SPICEDFOOD.ICON + c + UI.DETAILTABS.TEXTICONDATA.SPICEDFOOD.NAME;
		}
		if (item.HasTag(GameTags.RotModifierTags.Refrigerated))
		{
			text = text + text2 + UI.DETAILTABS.TEXTICONDATA.REFRIGERATED.ICON + c + UI.DETAILTABS.TEXTICONDATA.REFRIGERATED.NAME;
		}
		else if (item.HasTag(GameTags.RotModifierTags.DeepFrozen))
		{
			text = text + text2 + UI.DETAILTABS.TEXTICONDATA.DEEPFROZEN.ICON + c + UI.DETAILTABS.TEXTICONDATA.DEEPFROZEN.NAME;
		}
		if (item.HasTag(GameTags.RotModifierTags.Fresh))
		{
			string text3 = ((rottable == null) ? ((string)UI.DETAILTABS.TEXTICONDATA.FRESH.NAME) : rottable.StateString());
			text = text + text2 + UI.DETAILTABS.TEXTICONDATA.FRESH.ICON + c + text3;
		}
		else if (item.HasTag(GameTags.RotModifierTags.Stale))
		{
			string text4 = ((rottable == null) ? ((string)UI.DETAILTABS.TEXTICONDATA.STALE.NAME) : rottable.StateString());
			text = text + text2 + UI.DETAILTABS.TEXTICONDATA.STALE.ICON + c + text4;
		}
		if (pe.DiseaseIdx != byte.MaxValue)
		{
			Disease disease = Db.Get().Diseases[pe.DiseaseIdx];
			text = text + text2 + UI.DETAILTABS.TEXTICONDATA.GERMS.ICON + c + GameUtil.SafeStringFormat(UI.OVERLAYS.DISEASE.DISEASE_FORMAT, disease.Name, GameUtil.GetFormattedDiseaseAmount(pe.DiseaseCount));
		}
		return text.TrimStart('\n');
	}

	private static string GetTrimmedString(string value, int maxCharacterCount)
	{
		if (value.Length <= maxCharacterCount)
		{
			return value;
		}
		return value.Substring(0, maxCharacterCount) + "…";
	}

	private static void ForgeNameAndTooltipForStoredItem(KPrefabID itemPrefabID, PrimaryElement pe, out string nameText, out string temperatureText, out string massText, out string tooltip, bool trim)
	{
		GameObject gameObject = itemPrefabID.gameObject;
		Rottable.Instance sMI = gameObject.GetSMI<Rottable.Instance>();
		HighEnergyParticleStorage component = gameObject.GetComponent<HighEnergyParticleStorage>();
		pe = ((pe == null) ? gameObject.GetComponent<PrimaryElement>() : pe);
		string text = UI.StripLinkFormatting(gameObject.GetProperName());
		nameText = (trim ? GetTrimmedString(text, 15) : text);
		tooltip = "";
		massText = "";
		temperatureText = "";
		if (pe != null && component == null)
		{
			massText = ((pe.MassPerUnit > 1f) ? GameUtil.GetFormattedUnits(pe.Mass / pe.MassPerUnit) : GameUtil.GetFormattedMass(pe.Mass));
			temperatureText = GameUtil.GetFormattedTemperature(pe.Temperature);
			nameText = GetIconsForItemName(nameText, itemPrefabID, pe, trim ? (15 - nameText.Length) : 99999);
			string iconsLegendForItem = GetIconsLegendForItem(itemPrefabID, pe, sMI, addTabs: true);
			tooltip = text + "\n";
			tooltip += iconsLegendForItem;
			tooltip += (string.IsNullOrEmpty(iconsLegendForItem) ? "" : "\n\n");
		}
		if (component != null)
		{
			nameText = ITEMS.RADIATION.HIGHENERGYPARITCLE.NAME;
			temperatureText = GameUtil.GetFormattedHighEnergyParticles(component.Particles);
		}
		if (sMI != null)
		{
			tooltip += sMI.GetToolTip();
		}
		tooltip.TrimEnd('\n');
	}

	private static void OnStorageCollapsibleRowExpanded(DetailCollapsableLabel collapsableLabel)
	{
		collapsableLabel.MarkAllRowsUnused();
		StorageCollapsibleRowData storageCollapsibleRowData = collapsableLabel.Data as StorageCollapsibleRowData;
		IStorage[] storages = storageCollapsibleRowData.storages;
		foreach (IStorage storage in storages)
		{
			foreach (GameObject go in storage.GetItems())
			{
				if (go == null)
				{
					continue;
				}
				KPrefabID component = go.GetComponent<KPrefabID>();
				int hashCode = component.GetHashCode();
				if (hashCode == storageCollapsibleRowData.prefabHashCode)
				{
					string nameText = "";
					string massText = "";
					string temperatureText = "";
					string tooltip = "";
					ForgeNameAndTooltipForStoredItem(component, null, out nameText, out temperatureText, out massText, out tooltip, trim: false);
					DetailLabelWithButton detailLabelWithButton = collapsableLabel.AddOrGetAvailableContentRow();
					detailLabelWithButton.label.SetText(nameText);
					detailLabelWithButton.label2.SetText(temperatureText);
					detailLabelWithButton.label3.SetText(massText);
					detailLabelWithButton.RefreshLabelsVisibility();
					detailLabelWithButton.toolTip.SetSimpleTooltip(tooltip.TrimEnd('\n'));
					detailLabelWithButton.button.ClearOnClick();
					detailLabelWithButton.button.onClick += delegate
					{
						SelectTool.Instance.Select(go.GetComponent<KSelectable>());
					};
				}
			}
		}
		collapsableLabel.RefreshRowVisibilityState();
	}

	private static void OnStorageCollapsibleRowCollapsed(DetailCollapsableLabel collapsableLabel)
	{
		collapsableLabel.MarkAllRowsUnused();
		collapsableLabel.RefreshRowVisibilityState();
	}

	private void CreateWorldTraitRow()
	{
		GameObject gameObject = Util.KInstantiateUI(iconLabelRow, worldTraitsPanel.Content.gameObject, force_active: true);
		worldTraitRows.Add(gameObject);
		HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
		component.GetReference<Image>("Icon").gameObject.SetActive(value: false);
		component.GetReference<LocText>("ValueLabel").gameObject.SetActive(value: false);
	}

	private static void RefreshMovePanel(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
	{
		CancellableMove component = targetEntity.GetComponent<CancellableMove>();
		Movable moving = targetEntity.GetComponent<Movable>();
		if (component != null)
		{
			List<Ref<Movable>> movingObjects = component.movingObjects;
			int num = 0;
			foreach (Ref<Movable> item in movingObjects)
			{
				Movable movable = item.Get();
				GameObject go = ((movable != null) ? movable.gameObject : null);
				if (go == null)
				{
					continue;
				}
				PrimaryElement component2 = go.GetComponent<PrimaryElement>();
				if (component2 != null && component2.Mass == 0f)
				{
					continue;
				}
				Rottable.Instance sMI = go.GetSMI<Rottable.Instance>();
				HighEnergyParticleStorage component3 = go.GetComponent<HighEnergyParticleStorage>();
				string text = "";
				string text2 = "";
				if (component2 != null && component3 == null)
				{
					text = GameUtil.GetUnitFormattedName(go);
					text = string.Format(UI.DETAILTABS.DETAILS.CONTENTS_MASS, text, GameUtil.GetFormattedMass(component2.Mass));
					text = string.Format(UI.DETAILTABS.DETAILS.CONTENTS_TEMPERATURE, text, GameUtil.GetFormattedTemperature(component2.Temperature));
				}
				if (component3 != null)
				{
					text = ITEMS.RADIATION.HIGHENERGYPARITCLE.NAME;
					text = string.Format(UI.DETAILTABS.DETAILS.CONTENTS_MASS, text, GameUtil.GetFormattedHighEnergyParticles(component3.Particles));
				}
				if (sMI != null)
				{
					string text3 = sMI.StateString();
					if (!string.IsNullOrEmpty(text3))
					{
						text += string.Format(UI.DETAILTABS.DETAILS.CONTENTS_ROTTABLE, text3);
					}
					text2 += sMI.GetToolTip();
				}
				if (component2.DiseaseIdx != byte.MaxValue)
				{
					text += string.Format(UI.DETAILTABS.DETAILS.CONTENTS_DISEASED, GameUtil.GetFormattedDisease(component2.DiseaseIdx, component2.DiseaseCount));
					string formattedDisease = GameUtil.GetFormattedDisease(component2.DiseaseIdx, component2.DiseaseCount, color: true);
					text2 += formattedDisease;
				}
				targetPanel.SetLabelWithButton("move_" + num, text, text2, delegate
				{
					SelectTool.Instance.SelectAndFocus(go.transform.GetPosition(), go.GetComponent<KSelectable>(), new Vector3(5f, 0f, 0f));
				});
				num++;
			}
		}
		else if (moving != null && moving.IsMarkedForMove)
		{
			targetPanel.SetLabelWithButton("moveplacer", MISC.PLACERS.MOVEPICKUPABLEPLACER.PLACER_STATUS, MISC.PLACERS.MOVEPICKUPABLEPLACER.PLACER_STATUS_TOOLTIP, delegate
			{
				SelectTool.Instance.SelectAndFocus(moving.StorageProxy.transform.GetPosition(), moving.StorageProxy.GetComponent<KSelectable>(), new Vector3(5f, 0f, 0f));
			});
		}
		targetPanel.Commit();
	}

	private void RefreshWorldPanel()
	{
		WorldContainer worldContainer = ((selectedTarget == null) ? null : selectedTarget.GetComponent<WorldContainer>());
		AsteroidGridEntity asteroidGridEntity = ((selectedTarget == null) ? null : selectedTarget.GetComponent<AsteroidGridEntity>());
		bool flag = ManagementMenu.Instance.IsScreenOpen(ClusterMapScreen.Instance) && worldContainer != null && asteroidGridEntity != null;
		worldBiomesPanel.gameObject.SetActive(flag);
		worldGeysersPanel.gameObject.SetActive(flag);
		worldMeteorShowersPanel.gameObject.SetActive(flag);
		worldTraitsPanel.gameObject.SetActive(flag);
		if (!flag)
		{
			return;
		}
		foreach (KeyValuePair<Tag, GameObject> biomeRow in biomeRows)
		{
			biomeRow.Value.SetActive(value: false);
		}
		if (worldContainer.Biomes != null)
		{
			foreach (string biome in worldContainer.Biomes)
			{
				Sprite biomeSprite = GameUtil.GetBiomeSprite(biome);
				if (!biomeRows.ContainsKey(biome))
				{
					biomeRows.Add(biome, Util.KInstantiateUI(bigIconLabelRow, worldBiomesPanel.Content.gameObject, force_active: true));
					HierarchyReferences component = biomeRows[biome].GetComponent<HierarchyReferences>();
					component.GetReference<Image>("Icon").sprite = biomeSprite;
					component.GetReference<LocText>("NameLabel").SetText(UI.FormatAsLink(Strings.Get("STRINGS.SUBWORLDS." + biome.ToUpper() + ".NAME"), "BIOME" + biome.ToUpper()));
					component.GetReference<LocText>("DescriptionLabel").SetText(Strings.Get("STRINGS.SUBWORLDS." + biome.ToUpper() + ".DESC"));
				}
				biomeRows[biome].SetActive(value: true);
			}
		}
		else
		{
			worldBiomesPanel.gameObject.SetActive(value: false);
		}
		List<Tag> list = new List<Tag>();
		foreach (Geyser item in Components.Geysers.GetItems(worldContainer.id))
		{
			list.Add(item.PrefabID());
		}
		list.AddRange(SaveGame.Instance.worldGenSpawner.GetUnspawnedWithType<Geyser>(worldContainer.id));
		list.AddRange(SaveGame.Instance.worldGenSpawner.GetSpawnersWithTag("OilWell", worldContainer.id, includeSpawned: true));
		foreach (KeyValuePair<Tag, GameObject> geyserRow in geyserRows)
		{
			geyserRow.Value.SetActive(value: false);
		}
		foreach (Tag item2 in list)
		{
			Tuple<Sprite, Color> uISprite = Def.GetUISprite(item2);
			if (!geyserRows.ContainsKey(item2))
			{
				geyserRows.Add(item2, Util.KInstantiateUI(iconLabelRow, worldGeysersPanel.Content.gameObject, force_active: true));
				HierarchyReferences component2 = geyserRows[item2].GetComponent<HierarchyReferences>();
				component2.GetReference<Image>("Icon").sprite = uISprite.first;
				component2.GetReference<Image>("Icon").color = uISprite.second;
				component2.GetReference<LocText>("NameLabel").SetText(Assets.GetPrefab(item2).GetProperName());
				component2.GetReference<LocText>("ValueLabel").gameObject.SetActive(value: false);
			}
			geyserRows[item2].SetActive(value: true);
		}
		int count = SaveGame.Instance.worldGenSpawner.GetSpawnersWithTag("GeyserGeneric", worldContainer.id).Count;
		if (count > 0)
		{
			Tuple<Sprite, Color> uISprite2 = Def.GetUISprite("GeyserGeneric");
			Tag key = "GeyserGeneric";
			if (!geyserRows.ContainsKey(key))
			{
				geyserRows.Add(key, Util.KInstantiateUI(iconLabelRow, worldGeysersPanel.Content.gameObject, force_active: true));
			}
			HierarchyReferences component3 = geyserRows[key].GetComponent<HierarchyReferences>();
			component3.GetReference<Image>("Icon").sprite = uISprite2.first;
			component3.GetReference<Image>("Icon").color = uISprite2.second;
			component3.GetReference<LocText>("NameLabel").SetText(UI.DETAILTABS.SIMPLEINFO.UNKNOWN_GEYSERS.Replace("{num}", count.ToString()));
			component3.GetReference<LocText>("ValueLabel").gameObject.SetActive(value: false);
			geyserRows[key].SetActive(value: true);
		}
		Tag key2 = "NoGeysers";
		if (!geyserRows.ContainsKey(key2))
		{
			geyserRows.Add(key2, Util.KInstantiateUI(iconLabelRow, worldGeysersPanel.Content.gameObject, force_active: true));
			HierarchyReferences component4 = geyserRows[key2].GetComponent<HierarchyReferences>();
			component4.GetReference<Image>("Icon").sprite = Assets.GetSprite("icon_action_cancel");
			component4.GetReference<LocText>("NameLabel").SetText(UI.DETAILTABS.SIMPLEINFO.NO_GEYSERS);
			component4.GetReference<LocText>("ValueLabel").gameObject.SetActive(value: false);
		}
		geyserRows[key2].gameObject.SetActive(list.Count == 0 && count == 0);
		foreach (KeyValuePair<Tag, GameObject> meteorShowerRow in meteorShowerRows)
		{
			meteorShowerRow.Value.SetActive(value: false);
		}
		bool flag2 = false;
		foreach (string seasonId in worldContainer.GetSeasonIds())
		{
			GameplaySeason gameplaySeason = Db.Get().GameplaySeasons.TryGet(seasonId);
			if (gameplaySeason == null)
			{
				continue;
			}
			foreach (GameplayEvent @event in gameplaySeason.events)
			{
				if (@event.tags.Contains(GameTags.SpaceDanger) && @event is MeteorShowerEvent)
				{
					flag2 = true;
					MeteorShowerEvent meteorShowerEvent = @event as MeteorShowerEvent;
					string id = meteorShowerEvent.Id;
					Tuple<Sprite, Color> uISprite3 = Def.GetUISprite(meteorShowerEvent.GetClusterMapMeteorShowerID());
					if (!meteorShowerRows.ContainsKey(id))
					{
						meteorShowerRows.Add(id, Util.KInstantiateUI(iconLabelRow, worldMeteorShowersPanel.Content.gameObject, force_active: true));
						HierarchyReferences component5 = meteorShowerRows[id].GetComponent<HierarchyReferences>();
						component5.GetReference<Image>("Icon").sprite = uISprite3.first;
						component5.GetReference<Image>("Icon").color = uISprite3.second;
						component5.GetReference<LocText>("NameLabel").SetText(Assets.GetPrefab(meteorShowerEvent.GetClusterMapMeteorShowerID()).GetProperName());
						component5.GetReference<LocText>("ValueLabel").gameObject.SetActive(value: false);
					}
					meteorShowerRows[id].SetActive(value: true);
				}
			}
		}
		Tag key3 = "NoMeteorShowers";
		if (!meteorShowerRows.ContainsKey(key3))
		{
			meteorShowerRows.Add(key3, Util.KInstantiateUI(iconLabelRow, worldMeteorShowersPanel.Content.gameObject, force_active: true));
			HierarchyReferences component6 = meteorShowerRows[key3].GetComponent<HierarchyReferences>();
			component6.GetReference<Image>("Icon").sprite = Assets.GetSprite("icon_action_cancel");
			component6.GetReference<LocText>("NameLabel").SetText(UI.DETAILTABS.SIMPLEINFO.NO_METEORSHOWERS);
			component6.GetReference<LocText>("ValueLabel").gameObject.SetActive(value: false);
		}
		meteorShowerRows[key3].gameObject.SetActive(!flag2);
		List<string> worldTraitIds = worldContainer.WorldTraitIds;
		if (worldTraitIds != null)
		{
			for (int i = 0; i < worldTraitIds.Count; i++)
			{
				if (i > worldTraitRows.Count - 1)
				{
					CreateWorldTraitRow();
				}
				WorldTrait cachedWorldTrait = SettingsCache.GetCachedWorldTrait(worldTraitIds[i], assertMissingTrait: false);
				Image reference = worldTraitRows[i].GetComponent<HierarchyReferences>().GetReference<Image>("Icon");
				if (cachedWorldTrait != null)
				{
					string text = cachedWorldTrait.filePath.Substring(cachedWorldTrait.filePath.LastIndexOf("/") + 1);
					Sprite sprite = Assets.GetSprite(text);
					reference.gameObject.SetActive(value: true);
					reference.sprite = ((sprite == null) ? Assets.GetSprite("unknown") : sprite);
					reference.color = Util.ColorFromHex(cachedWorldTrait.colorHex);
					worldTraitRows[i].GetComponent<HierarchyReferences>().GetReference<LocText>("NameLabel").SetText(Strings.Get(cachedWorldTrait.name));
					worldTraitRows[i].AddOrGet<ToolTip>().SetSimpleTooltip(Strings.Get(cachedWorldTrait.description));
				}
				else
				{
					Sprite sprite2 = Assets.GetSprite("NoTraits");
					reference.gameObject.SetActive(value: true);
					reference.sprite = sprite2;
					reference.color = Color.white;
					worldTraitRows[i].GetComponent<HierarchyReferences>().GetReference<LocText>("NameLabel").SetText(WORLD_TRAITS.MISSING_TRAIT);
					worldTraitRows[i].AddOrGet<ToolTip>().SetSimpleTooltip("");
				}
			}
			for (int j = 0; j < worldTraitRows.Count; j++)
			{
				worldTraitRows[j].SetActive(j < worldTraitIds.Count);
			}
			if (worldTraitIds.Count == 0)
			{
				if (worldTraitRows.Count < 1)
				{
					CreateWorldTraitRow();
				}
				Image reference2 = worldTraitRows[0].GetComponent<HierarchyReferences>().GetReference<Image>("Icon");
				Sprite sprite3 = Assets.GetSprite("NoTraits");
				reference2.gameObject.SetActive(value: true);
				reference2.sprite = sprite3;
				reference2.color = Color.black;
				worldTraitRows[0].GetComponent<HierarchyReferences>().GetReference<LocText>("NameLabel").SetText(WORLD_TRAITS.NO_TRAITS.NAME_SHORTHAND);
				worldTraitRows[0].AddOrGet<ToolTip>().SetSimpleTooltip(WORLD_TRAITS.NO_TRAITS.DESCRIPTION);
				worldTraitRows[0].SetActive(value: true);
			}
		}
		for (int num = surfaceConditionRows.Count - 1; num >= 0; num--)
		{
			Util.KDestroyGameObject(surfaceConditionRows[num]);
		}
		surfaceConditionRows.Clear();
		GameObject gameObject = Util.KInstantiateUI(iconLabelRow, worldTraitsPanel.Content.gameObject, force_active: true);
		HierarchyReferences component7 = gameObject.GetComponent<HierarchyReferences>();
		component7.GetReference<Image>("Icon").sprite = Assets.GetSprite("overlay_lights");
		component7.GetReference<LocText>("NameLabel").SetText(UI.CLUSTERMAP.ASTEROIDS.SURFACE_CONDITIONS.LIGHT);
		component7.GetReference<LocText>("ValueLabel").SetText(GameUtil.GetFormattedLux(worldContainer.SunlightFixedTraits[worldContainer.sunlightFixedTrait]));
		component7.GetReference<LocText>("ValueLabel").alignment = TextAlignmentOptions.MidlineRight;
		surfaceConditionRows.Add(gameObject);
		GameObject gameObject2 = Util.KInstantiateUI(iconLabelRow, worldTraitsPanel.Content.gameObject, force_active: true);
		component7 = gameObject2.GetComponent<HierarchyReferences>();
		component7.GetReference<Image>("Icon").sprite = Assets.GetSprite("overlay_radiation");
		component7.GetReference<LocText>("NameLabel").SetText(UI.CLUSTERMAP.ASTEROIDS.SURFACE_CONDITIONS.RADIATION);
		component7.GetReference<LocText>("ValueLabel").SetText(GameUtil.GetFormattedRads(worldContainer.CosmicRadiationFixedTraits[worldContainer.cosmicRadiationFixedTrait]));
		component7.GetReference<LocText>("ValueLabel").alignment = TextAlignmentOptions.MidlineRight;
		surfaceConditionRows.Add(gameObject2);
	}

	private void RefreshProcessConditionsPanel()
	{
		foreach (GameObject processConditionRow in processConditionRows)
		{
			Util.KDestroyGameObject(processConditionRow);
		}
		processConditionRows.Clear();
		processConditionContainer.SetActive(selectedTarget.GetComponent<IProcessConditionSet>() != null);
		if (!DlcManager.FeatureClusterSpaceEnabled())
		{
			if (selectedTarget.GetComponent<LaunchableRocket>() != null)
			{
				RefreshProcessConditionsForType(selectedTarget, ProcessCondition.ProcessConditionType.RocketPrep);
				RefreshProcessConditionsForType(selectedTarget, ProcessCondition.ProcessConditionType.RocketStorage);
				RefreshProcessConditionsForType(selectedTarget, ProcessCondition.ProcessConditionType.RocketBoard);
			}
			else
			{
				RefreshProcessConditionsForType(selectedTarget, ProcessCondition.ProcessConditionType.All);
			}
		}
		else if (selectedTarget.GetComponent<LaunchPad>() != null || selectedTarget.GetComponent<RocketProcessConditionDisplayTarget>() != null)
		{
			RefreshProcessConditionsForType(selectedTarget, ProcessCondition.ProcessConditionType.RocketFlight);
			RefreshProcessConditionsForType(selectedTarget, ProcessCondition.ProcessConditionType.RocketPrep);
			RefreshProcessConditionsForType(selectedTarget, ProcessCondition.ProcessConditionType.RocketStorage);
			RefreshProcessConditionsForType(selectedTarget, ProcessCondition.ProcessConditionType.RocketBoard);
		}
		else
		{
			RefreshProcessConditionsForType(selectedTarget, ProcessCondition.ProcessConditionType.All);
		}
	}

	private static void RefreshStressPanel(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
	{
		MinionIdentity minionIdentity = ((targetEntity != null) ? targetEntity.GetComponent<MinionIdentity>() : null);
		if (minionIdentity != null)
		{
			List<ReportManager.ReportEntry.Note> stressNotes = new List<ReportManager.ReportEntry.Note>();
			targetPanel.gameObject.SetActive(value: true);
			targetPanel.SetTitle(UI.DETAILTABS.STATS.GROUPNAME_STRESS);
			ReportManager.ReportEntry reportEntry = ReportManager.Instance.TodaysReport.reportEntries.Find((ReportManager.ReportEntry entry) => entry.reportType == ReportManager.ReportType.StressDelta);
			float num = 0f;
			stressNotes.Clear();
			ReportManager.ReportEntry reportEntry2 = reportEntry.FindEntryFromGameObject(targetEntity);
			if (reportEntry2 != null)
			{
				reportEntry2.IterateNotes(delegate(ReportManager.ReportEntry.Note note)
				{
					stressNotes.Add(note);
				});
				stressNotes.Sort((ReportManager.ReportEntry.Note a, ReportManager.ReportEntry.Note b) => a.value.CompareTo(b.value));
				for (int num2 = 0; num2 < stressNotes.Count; num2++)
				{
					string text = (float.IsNegativeInfinity(stressNotes[num2].value) ? UI.NEG_INFINITY.ToString() : Util.FormatTwoDecimalPlace(stressNotes[num2].value));
					targetPanel.SetLabel("stressNotes_" + num2, ((stressNotes[num2].value > 0f) ? UIConstants.ColorPrefixRed : "") + stressNotes[num2].note + ": " + text + "%" + ((stressNotes[num2].value > 0f) ? UIConstants.ColorSuffix : ""), "");
					num += stressNotes[num2].value;
				}
			}
			string arg = (float.IsNegativeInfinity(num) ? UI.NEG_INFINITY.ToString() : Util.FormatTwoDecimalPlace(num));
			targetPanel.SetLabel("net_stress", ((num > 0f) ? UIConstants.ColorPrefixRed : "") + string.Format(UI.DETAILTABS.DETAILS.NET_STRESS, arg) + ((num > 0f) ? UIConstants.ColorSuffix : ""), "");
		}
		targetPanel.Commit();
	}

	private void RefreshProcessConditionsForType(GameObject target, ProcessCondition.ProcessConditionType conditionType)
	{
		IProcessConditionSet component = target.GetComponent<IProcessConditionSet>();
		if (component == null)
		{
			return;
		}
		List<ProcessCondition> v;
		using (ProcessCondition.ListPool.Get(out v))
		{
			if (component.PopulateConditionSet(conditionType, v) == 0)
			{
				return;
			}
			HierarchyReferences hierarchyReferences = Util.KInstantiateUI<HierarchyReferences>(processConditionHeader.gameObject, processConditionContainer.Content.gameObject, force_active: true);
			hierarchyReferences.GetReference<LocText>("Label").text = Strings.Get("STRINGS.UI.DETAILTABS.PROCESS_CONDITIONS." + conditionType.ToString().ToUpper());
			hierarchyReferences.GetComponent<ToolTip>().toolTip = Strings.Get("STRINGS.UI.DETAILTABS.PROCESS_CONDITIONS." + conditionType.ToString().ToUpper() + "_TOOLTIP");
			processConditionRows.Add(hierarchyReferences.gameObject);
			List<ProcessCondition> list = new List<ProcessCondition>();
			foreach (ProcessCondition condition in v)
			{
				if (condition.ShowInUI() && (condition.GetType() == typeof(RequireAttachedComponent) || list.Find((ProcessCondition match) => match.GetType() == condition.GetType()) == null))
				{
					list.Add(condition);
					GameObject gameObject = Util.KInstantiateUI(processConditionRow, processConditionContainer.Content.gameObject, force_active: true);
					processConditionRows.Add(gameObject);
					ConditionListSideScreen.SetRowState(gameObject, condition);
				}
			}
		}
	}
}
