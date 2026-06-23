using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReceptacleSideScreen : SideScreenContent, IRender1000ms
{
	protected class SelectableEntity
	{
		public Tag tag;

		public SingleEntityReceptacle.ReceptacleDirection direction;

		public GameObject asset;

		public float lastAmount = -1f;
	}

	protected bool ALLOW_ORDER_IGNORING_WOLRD_NEED = true;

	[SerializeField]
	protected KButton requestSelectedEntityBtn;

	[SerializeField]
	private string requestStringDeposit;

	[SerializeField]
	private string requestStringCancelDeposit;

	[SerializeField]
	private string requestStringRemove;

	[SerializeField]
	private string requestStringCancelRemove;

	public GameObject activeEntityContainer;

	public GameObject nothingDiscoveredContainer;

	[SerializeField]
	private bool categoryStartExpanded;

	[SerializeField]
	private GameObject categoryContainerPrefab;

	private Dictionary<Tag, GameObject> contentContainers = new Dictionary<Tag, GameObject>();

	[SerializeField]
	protected LocText descriptionLabel;

	protected Dictionary<SingleEntityReceptacle, int> entityPreviousSelectionMap = new Dictionary<SingleEntityReceptacle, int>();

	[SerializeField]
	private string subtitleStringSelect;

	[SerializeField]
	private string subtitleStringSelectDescription;

	[SerializeField]
	private string subtitleStringAwaitingSelection;

	[SerializeField]
	private string subtitleStringAwaitingDelivery;

	[SerializeField]
	private string subtitleStringEntityDeposited;

	[SerializeField]
	private string subtitleStringAwaitingRemoval;

	[SerializeField]
	private LocText subtitleLabel;

	[SerializeField]
	private List<DescriptorPanel> descriptorPanels;

	public Material defaultMaterial;

	public Material desaturatedMaterial;

	[SerializeField]
	private GameObject requestObjectListContainer;

	[SerializeField]
	private GameObject requestObjectListContainerContent;

	[SerializeField]
	private GameObject scrollBarContainer;

	[SerializeField]
	private GameObject entityToggle;

	[SerializeField]
	private Sprite buttonSelectedBG;

	[SerializeField]
	private Sprite buttonNormalBG;

	[SerializeField]
	private Sprite elementPlaceholderSpr;

	[SerializeField]
	private bool hideUndiscoveredEntities;

	protected ReceptacleToggle selectedEntityToggle;

	protected SingleEntityReceptacle targetReceptacle;

	protected Tag selectedDepositObjectTag;

	protected Tag selectedDepositObjectAdditionalTag;

	protected Dictionary<ReceptacleToggle, SelectableEntity> depositObjectMap;

	protected List<ReceptacleToggle> entityToggles = new List<ReceptacleToggle>();

	private List<GameObject> recycledEntityToggles = new List<GameObject>();

	private Dictionary<Tag, bool> categoryExpandedStatus = new Dictionary<Tag, bool>();

	private int onObjectDestroyedHandle = -1;

	private int onOccupantValidChangedHandle = -1;

	private int onStorageChangedHandle = -1;

	public override string GetTitle()
	{
		if (targetReceptacle == null)
		{
			return Strings.Get(titleKey).ToString().Replace("{0}", "");
		}
		return string.Format(Strings.Get(titleKey), targetReceptacle.GetProperName());
	}

	private void RecycleToggle(GameObject toggle)
	{
		toggle.SetActive(value: false);
		recycledEntityToggles.Add(toggle);
	}

	private GameObject SpawnToggle(GameObject parent)
	{
		if (recycledEntityToggles.Count > 0)
		{
			GameObject obj = recycledEntityToggles[recycledEntityToggles.Count - 1];
			recycledEntityToggles.RemoveAt(recycledEntityToggles.Count - 1);
			obj.transform.SetParent(parent.transform);
			obj.SetActive(value: true);
			return obj;
		}
		return Util.KInstantiateUI(entityToggle, parent, force_active: true);
	}

	private void RefreshCategoryOpen(GameObject categoryHeader, GameObject categoryGrid, Tag tag)
	{
		categoryHeader.GetComponent<MultiToggle>().ChangeState((!categoryExpandedStatus[tag]) ? 1 : 0);
		categoryGrid.gameObject.SetActive(categoryExpandedStatus[tag]);
	}

	public void Initialize(SingleEntityReceptacle target)
	{
		if (target == null)
		{
			Debug.LogError("SingleObjectReceptacle provided was null.");
			return;
		}
		targetReceptacle = target;
		base.gameObject.SetActive(value: true);
		depositObjectMap = new Dictionary<ReceptacleToggle, SelectableEntity>();
		entityToggles.ForEach(delegate(ReceptacleToggle rbi)
		{
			RecycleToggle(rbi.gameObject);
		});
		entityToggles.Clear();
		List<GameObject> list = new List<GameObject>();
		if (targetReceptacle.possibleDepositObjectTags.Count == 1)
		{
			categoryStartExpanded = true;
		}
		foreach (Tag tag in targetReceptacle.possibleDepositObjectTags)
		{
			List<GameObject> prefabsWithTag = Assets.GetPrefabsWithTag(tag);
			int num = prefabsWithTag.Count;
			if (categoryExpandedStatus.ContainsKey(tag))
			{
				categoryExpandedStatus[tag] = categoryStartExpanded;
			}
			if (!contentContainers.ContainsKey(tag))
			{
				GameObject gameObject = Util.KInstantiateUI(categoryContainerPrefab, requestObjectListContainerContent, force_active: true);
				contentContainers.Add(tag, gameObject);
				HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
				component.GetReference<LocText>("HeaderLabel").SetText(tag.ProperName());
				categoryExpandedStatus.Add(tag, categoryStartExpanded);
				MultiToggle toggle = gameObject.GetComponent<HierarchyReferences>().GetReference<MultiToggle>("HeaderToggle");
				GridLayoutGroup grid = component.GetReference<GridLayoutGroup>("GridLayout");
				MultiToggle multiToggle = toggle;
				multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, (System.Action)delegate
				{
					categoryExpandedStatus[tag] = !categoryExpandedStatus[tag];
					RefreshCategoryOpen(toggle.gameObject, grid.gameObject, tag);
				});
				RefreshCategoryOpen(toggle.gameObject, grid.gameObject, tag);
			}
			RefreshCategoryOpen(contentContainers[tag].GetComponent<HierarchyReferences>().GetReference<MultiToggle>("HeaderToggle").gameObject, contentContainers[tag].GetComponent<HierarchyReferences>().GetReference<GridLayoutGroup>("GridLayout").gameObject, tag);
			List<IHasSortOrder> list2 = new List<IHasSortOrder>();
			foreach (GameObject item in prefabsWithTag)
			{
				if (!targetReceptacle.IsValidEntity(item) || list.Contains(item))
				{
					num--;
					continue;
				}
				IHasSortOrder component2 = item.GetComponent<IHasSortOrder>();
				if (component2 != null)
				{
					list.Add(item);
					list2.Add(component2);
				}
			}
			Debug.Assert(list2.Count == num, "Not all entities in this receptacle implement IHasSortOrder!");
			list2.Sort((IHasSortOrder a, IHasSortOrder b) => a.sortOrder - b.sortOrder);
			foreach (IHasSortOrder item2 in list2)
			{
				GameObject gameObject2 = (item2 as MonoBehaviour).gameObject;
				GameObject gameObject3 = SpawnToggle(contentContainers[tag].GetComponent<HierarchyReferences>().GetReference("GridLayout").gameObject);
				gameObject3.transform.SetAsLastSibling();
				gameObject3.SetActive(value: true);
				ReceptacleToggle newToggle = gameObject3.GetComponent<ReceptacleToggle>();
				IReceptacleDirection component3 = gameObject2.GetComponent<IReceptacleDirection>();
				string entityName = GetEntityName(gameObject2.PrefabID());
				newToggle.title.text = entityName;
				Sprite entityIcon = GetEntityIcon(gameObject2.PrefabID());
				if (entityIcon == null)
				{
					entityIcon = elementPlaceholderSpr;
				}
				newToggle.image.sprite = entityIcon;
				if (newToggle.toggle == null)
				{
					newToggle.toggle = newToggle.GetComponentInChildren<MultiToggle>();
				}
				MultiToggle toggle2 = newToggle.toggle;
				toggle2.onClick = (System.Action)Delegate.Combine(toggle2.onClick, (System.Action)delegate
				{
					ToggleClicked(newToggle);
				});
				ToolTip component4 = newToggle.GetComponent<ToolTip>();
				if (component4 != null)
				{
					component4.SetSimpleTooltip(GetEntityTooltip(gameObject2.PrefabID()));
				}
				depositObjectMap.Add(newToggle, new SelectableEntity
				{
					tag = gameObject2.PrefabID(),
					direction = (component3?.Direction ?? SingleEntityReceptacle.ReceptacleDirection.Top),
					asset = gameObject2
				});
				entityToggles.Add(newToggle);
			}
		}
		RestoreSelectionFromOccupant();
		selectedEntityToggle = null;
		if (entityToggles.Count > 0)
		{
			if (entityPreviousSelectionMap.ContainsKey(targetReceptacle))
			{
				int index = entityPreviousSelectionMap[targetReceptacle];
				ToggleClicked(entityToggles[index]);
			}
			else
			{
				subtitleLabel.SetText(Strings.Get(subtitleStringSelect).ToString());
				requestSelectedEntityBtn.isInteractable = false;
				descriptionLabel.SetText(Strings.Get(subtitleStringSelectDescription).ToString());
				HideAllDescriptorPanels();
			}
		}
		onStorageChangedHandle = targetReceptacle.gameObject.Subscribe(-1697596308, CheckAmountsAndUpdate);
		onOccupantValidChangedHandle = targetReceptacle.gameObject.Subscribe(-1820564715, OnOccupantValidChanged);
		UpdateState(null);
		SimAndRenderScheduler.instance.Add(this);
	}

	protected virtual void UpdateState(object data)
	{
		requestSelectedEntityBtn.ClearOnClick();
		if (targetReceptacle == null)
		{
			return;
		}
		if (CheckReceptacleOccupied())
		{
			Uprootable uprootable = targetReceptacle.Occupant.GetComponent<Uprootable>();
			if (uprootable != null && uprootable.IsMarkedForUproot)
			{
				requestSelectedEntityBtn.onClick += delegate
				{
					uprootable.ForceCancelUproot();
					UpdateState(null);
				};
				requestSelectedEntityBtn.GetComponentInChildren<LocText>().text = Strings.Get(requestStringCancelRemove).ToString();
				subtitleLabel.SetText(string.Format(Strings.Get(subtitleStringAwaitingRemoval).ToString(), targetReceptacle.Occupant.GetProperName()));
			}
			else
			{
				requestSelectedEntityBtn.onClick += delegate
				{
					targetReceptacle.OrderRemoveOccupant();
					UpdateState(null);
				};
				requestSelectedEntityBtn.GetComponentInChildren<LocText>().text = Strings.Get(requestStringRemove).ToString();
				subtitleLabel.SetText(string.Format(Strings.Get(subtitleStringEntityDeposited).ToString(), targetReceptacle.Occupant.GetProperName()));
			}
			requestSelectedEntityBtn.isInteractable = true;
			ToggleObjectPicker(Show: false);
			Tag tag = targetReceptacle.Occupant.GetComponent<KSelectable>().PrefabID();
			ConfigureActiveEntity(tag);
			SetResultDescriptions(targetReceptacle.Occupant);
		}
		else if (targetReceptacle.GetActiveRequest != null)
		{
			requestSelectedEntityBtn.onClick += delegate
			{
				targetReceptacle.CancelActiveRequest();
				ClearSelection();
				UpdateAvailableAmounts(null);
				UpdateState(null);
			};
			requestSelectedEntityBtn.GetComponentInChildren<LocText>().text = Strings.Get(requestStringCancelDeposit).ToString();
			requestSelectedEntityBtn.isInteractable = true;
			ToggleObjectPicker(Show: false);
			ConfigureActiveEntity(targetReceptacle.GetActiveRequest.tagsFirst);
			GameObject prefab = Assets.GetPrefab(targetReceptacle.GetActiveRequest.tagsFirst);
			if (prefab != null)
			{
				subtitleLabel.SetText(string.Format(Strings.Get(subtitleStringAwaitingDelivery).ToString(), prefab.GetProperName()));
				SetResultDescriptions(prefab);
			}
		}
		else if (selectedEntityToggle != null)
		{
			requestSelectedEntityBtn.onClick += delegate
			{
				targetReceptacle.CreateOrder(selectedDepositObjectTag, selectedDepositObjectAdditionalTag);
				UpdateAvailableAmounts(null);
				UpdateState(null);
			};
			requestSelectedEntityBtn.GetComponentInChildren<LocText>().text = Strings.Get(requestStringDeposit).ToString();
			targetReceptacle.SetPreview(depositObjectMap[selectedEntityToggle].tag);
			bool isInteractable = CanDepositEntity(depositObjectMap[selectedEntityToggle], runAdditionalCanDepositTest: true);
			requestSelectedEntityBtn.isInteractable = isInteractable;
			ToggleObjectPicker(Show: true);
			GameObject prefab2 = Assets.GetPrefab(selectedDepositObjectTag);
			if (prefab2 != null)
			{
				subtitleLabel.SetText(string.Format(Strings.Get(subtitleStringAwaitingSelection).ToString(), prefab2.GetProperName()));
				SetResultDescriptions(prefab2);
			}
		}
		else
		{
			requestSelectedEntityBtn.GetComponentInChildren<LocText>().text = Strings.Get(requestStringDeposit).ToString();
			requestSelectedEntityBtn.isInteractable = false;
			ToggleObjectPicker(Show: true);
		}
		UpdateAvailableAmounts(null);
		RefreshToggleStates();
		UpdateListeners();
	}

	private void UpdateListeners()
	{
		if (CheckReceptacleOccupied())
		{
			if (onObjectDestroyedHandle == -1)
			{
				onObjectDestroyedHandle = targetReceptacle.Occupant.gameObject.Subscribe(1969584890, delegate
				{
					UpdateState(null);
				});
			}
		}
		else if (onObjectDestroyedHandle != -1)
		{
			onObjectDestroyedHandle = -1;
		}
	}

	private void OnOccupantValidChanged(object _)
	{
		if (!(targetReceptacle == null) && !CheckReceptacleOccupied() && targetReceptacle.GetActiveRequest != null)
		{
			bool flag = false;
			if (depositObjectMap.TryGetValue(selectedEntityToggle, out var value))
			{
				flag = CanDepositEntity(value, runAdditionalCanDepositTest: true);
			}
			if (!flag)
			{
				targetReceptacle.CancelActiveRequest();
				ClearSelection();
				UpdateState(null);
				UpdateAvailableAmounts(null);
			}
		}
	}

	protected bool CanDepositEntity(SelectableEntity entity, bool runAdditionalCanDepositTest = false)
	{
		if (ValidRotationForDeposit(entity.direction) && (!RequiresAvailableAmountToDeposit() || GetAvailableAmount(entity.tag) > 0f))
		{
			if (runAdditionalCanDepositTest)
			{
				return AdditionalCanDepositTest();
			}
			return true;
		}
		return false;
	}

	protected virtual bool AdditionalCanDepositTest()
	{
		return true;
	}

	protected virtual bool RequiresAvailableAmountToDeposit()
	{
		return true;
	}

	private void ClearSelection()
	{
		selectedEntityToggle = null;
		RefreshToggleStates();
	}

	private void ToggleObjectPicker(bool Show)
	{
		requestObjectListContainer.SetActive(Show);
		if (scrollBarContainer != null)
		{
			scrollBarContainer.SetActive(Show);
		}
		requestObjectListContainer.SetActive(Show);
		activeEntityContainer.SetActive(!Show);
	}

	private void ConfigureActiveEntity(Tag tag)
	{
		string properName = Assets.GetPrefab(tag).GetProperName();
		HierarchyReferences component = activeEntityContainer.GetComponent<HierarchyReferences>();
		component.GetReference<LocText>("Label").text = properName;
		component.GetReference<Image>("Icon").sprite = GetEntityIcon(tag);
	}

	protected virtual string GetEntityName(Tag prefabTag)
	{
		return Assets.GetPrefab(prefabTag).GetProperName();
	}

	protected virtual string GetEntityTooltip(Tag prefabTag)
	{
		InfoDescription component = Assets.GetPrefab(prefabTag).GetComponent<InfoDescription>();
		string text = GetEntityName(prefabTag);
		if (component != null)
		{
			text = text + "\n\n" + component.description;
		}
		return text;
	}

	protected virtual Sprite GetEntityIcon(Tag prefabTag)
	{
		return Def.GetUISprite(Assets.GetPrefab(prefabTag)).first;
	}

	public override bool IsValidForTarget(GameObject target)
	{
		SingleEntityReceptacle component = target.GetComponent<SingleEntityReceptacle>();
		if (component != null && component.enabled && target.GetComponent<PlantablePlot>() == null && target.GetComponent<EggIncubator>() == null)
		{
			return target.GetComponent<SpecialCargoBayClusterReceptacle>() == null;
		}
		return false;
	}

	public override void SetTarget(GameObject target)
	{
		SingleEntityReceptacle component = target.GetComponent<SingleEntityReceptacle>();
		if (component == null)
		{
			Debug.LogError("The object selected doesn't have a SingleObjectReceptacle!");
			return;
		}
		Initialize(component);
		UpdateState(null);
	}

	protected virtual void RestoreSelectionFromOccupant()
	{
	}

	public override void ClearTarget()
	{
		if (targetReceptacle != null)
		{
			if (CheckReceptacleOccupied())
			{
				targetReceptacle.Occupant.gameObject.Unsubscribe(onObjectDestroyedHandle);
				onObjectDestroyedHandle = -1;
			}
			targetReceptacle.Unsubscribe(onStorageChangedHandle);
			onStorageChangedHandle = -1;
			targetReceptacle.Unsubscribe(onOccupantValidChangedHandle);
			onOccupantValidChangedHandle = -1;
			if (targetReceptacle.GetActiveRequest == null)
			{
				targetReceptacle.SetPreview(Tag.Invalid);
			}
			SimAndRenderScheduler.instance.Remove(this);
			targetReceptacle = null;
		}
	}

	protected void RefreshToggleStates()
	{
		foreach (KeyValuePair<ReceptacleToggle, SelectableEntity> item in depositObjectMap)
		{
			if (selectedEntityToggle != item.Key)
			{
				if (CanDepositEntity(item.Value))
				{
					SetToggleState(item.Key.toggle, ImageToggleState.State.Inactive);
				}
				else
				{
					SetToggleState(item.Key.toggle, ImageToggleState.State.Disabled);
				}
			}
			else if (CanDepositEntity(item.Value))
			{
				SetToggleState(item.Key.toggle, ImageToggleState.State.Active);
			}
			else
			{
				SetToggleState(item.Key.toggle, ImageToggleState.State.DisabledActive);
			}
		}
	}

	protected void SetToggleState(MultiToggle toggle, ImageToggleState.State state)
	{
		switch (state)
		{
		case ImageToggleState.State.Active:
			toggle.ChangeState(1);
			toggle.gameObject.GetComponentsInChildrenOnly<Image>()[1].material = defaultMaterial;
			break;
		case ImageToggleState.State.Inactive:
			toggle.ChangeState(0);
			toggle.gameObject.GetComponentsInChildrenOnly<Image>()[1].material = defaultMaterial;
			break;
		case ImageToggleState.State.Disabled:
			toggle.ChangeState(2);
			toggle.gameObject.GetComponentsInChildrenOnly<Image>()[1].material = desaturatedMaterial;
			break;
		case ImageToggleState.State.DisabledActive:
			toggle.ChangeState(3);
			toggle.gameObject.GetComponentsInChildrenOnly<Image>()[1].material = desaturatedMaterial;
			break;
		}
	}

	public void Render1000ms(float dt)
	{
		CheckAmountsAndUpdate(null);
	}

	private void CheckAmountsAndUpdate(object data)
	{
		if (!(targetReceptacle == null) && UpdateAvailableAmounts(null))
		{
			UpdateState(null);
		}
	}

	private bool UpdateAvailableAmounts(object data)
	{
		bool result = false;
		foreach (KeyValuePair<ReceptacleToggle, SelectableEntity> item in depositObjectMap)
		{
			if (!DebugHandler.InstantBuildMode && hideUndiscoveredEntities && !DiscoveredResources.Instance.IsDiscovered(item.Value.tag))
			{
				item.Key.gameObject.SetActive(value: false);
			}
			else if (!item.Key.gameObject.activeSelf)
			{
				item.Key.gameObject.SetActive(value: true);
			}
			float availableAmount = GetAvailableAmount(item.Value.tag);
			if (item.Value.lastAmount != availableAmount)
			{
				result = true;
				item.Value.lastAmount = availableAmount;
				item.Key.amount.text = availableAmount.ToString();
			}
			if (!ValidRotationForDeposit(item.Value.direction) || availableAmount <= 0f)
			{
				if (selectedEntityToggle != item.Key)
				{
					item.Key.toggle.ChangeState(2);
				}
				else
				{
					item.Key.toggle.ChangeState(3);
				}
			}
			else if (selectedEntityToggle != item.Key)
			{
				item.Key.toggle.ChangeState(0);
			}
			else
			{
				item.Key.toggle.ChangeState(1);
			}
		}
		foreach (KeyValuePair<Tag, GameObject> contentContainer in contentContainers)
		{
			Transform transform = contentContainer.Value.GetComponent<HierarchyReferences>().GetReference<GridLayoutGroup>("GridLayout").transform;
			bool flag = false;
			for (int i = 0; i < transform.childCount; i++)
			{
				if (transform.GetChild(i).gameObject.activeSelf)
				{
					flag = true;
					break;
				}
			}
			if (contentContainer.Value.activeSelf != flag)
			{
				contentContainer.Value.SetActive(flag);
			}
		}
		return result;
	}

	protected float GetAvailableAmount(Tag tag)
	{
		if (ALLOW_ORDER_IGNORING_WOLRD_NEED)
		{
			ICollection<Pickupable> pickupables = targetReceptacle.GetMyWorld().worldInventory.GetPickupables(tag, includeRelatedWorlds: true);
			float num = 0f;
			{
				foreach (Pickupable item in pickupables)
				{
					num += (float)Mathf.CeilToInt(item.TotalAmount);
				}
				return num;
			}
		}
		return targetReceptacle.GetMyWorld().worldInventory.GetAmount(tag, includeRelatedWorlds: true);
	}

	private bool ValidRotationForDeposit(SingleEntityReceptacle.ReceptacleDirection depositDir)
	{
		if (!(targetReceptacle.rotatable == null))
		{
			return depositDir == targetReceptacle.Direction;
		}
		return true;
	}

	protected virtual void ToggleClicked(ReceptacleToggle toggle)
	{
		if (!depositObjectMap.ContainsKey(toggle))
		{
			Debug.LogError("Recipe not found on recipe list.");
			return;
		}
		selectedEntityToggle = toggle;
		entityPreviousSelectionMap[targetReceptacle] = entityToggles.IndexOf(toggle);
		selectedDepositObjectTag = depositObjectMap[toggle].tag;
		MutantPlant component = depositObjectMap[toggle].asset.GetComponent<MutantPlant>();
		selectedDepositObjectAdditionalTag = (component ? component.SubSpeciesID : Tag.Invalid);
		RefreshToggleStates();
		UpdateAvailableAmounts(null);
		UpdateState(null);
	}

	private void CreateOrder(bool isInfinite)
	{
		targetReceptacle.CreateOrder(selectedDepositObjectTag, selectedDepositObjectAdditionalTag);
	}

	protected bool CheckReceptacleOccupied()
	{
		if (targetReceptacle != null && targetReceptacle.Occupant != null)
		{
			return true;
		}
		return false;
	}

	protected virtual void SetResultDescriptions(GameObject go)
	{
		string text = "";
		InfoDescription component = go.GetComponent<InfoDescription>();
		if ((bool)component)
		{
			text = component.description;
		}
		else
		{
			KPrefabID component2 = go.GetComponent<KPrefabID>();
			if (component2 != null)
			{
				Element element = ElementLoader.GetElement(component2.PrefabID());
				if (element != null)
				{
					text = element.Description();
				}
			}
			else
			{
				text = go.GetProperName();
			}
		}
		descriptionLabel.SetText(text);
	}

	protected virtual void HideAllDescriptorPanels()
	{
		for (int i = 0; i < descriptorPanels.Count; i++)
		{
			descriptorPanels[i].gameObject.SetActive(value: false);
		}
	}
}
