using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class AccessControlSideScreen : SideScreenContent
{
	[SerializeField]
	private GameObject entityCategoryPrefab;

	[SerializeField]
	private GameObject rowPrefab;

	[SerializeField]
	private GameObject disabledOverlay;

	[SerializeField]
	private KImage headerBG;

	[SerializeField]
	private GameObject scrollContents;

	private GameObject standardMinionSectionHeader;

	private GameObject standardMinionSectionContent;

	private GameObject bionicMinionSectionHeader;

	private GameObject bionicMinionSectionContent;

	private GameObject robotSectionHeader;

	private GameObject robotSectionContent;

	private AccessControl target;

	private Door doorTarget;

	private bool containersSpawned = false;

	private List<GameObject> inactiveRowPool = new List<GameObject>();

	private Dictionary<MinionAssignablesProxy, GameObject> minionIdentityRows = new Dictionary<MinionAssignablesProxy, GameObject>();

	private Dictionary<Tag, GameObject> robotRows = new Dictionary<Tag, GameObject>();

	private static Dictionary<Tag, string> categoryNames = new Dictionary<Tag, string>
	{
		{
			GameTags.Minions.Models.Standard,
			DUPLICANTS.MODEL.STANDARD.NAME_ADJECTIVE
		},
		{
			GameTags.Minions.Models.Bionic,
			DUPLICANTS.MODEL.BIONIC.NAME_ADJECTIVE
		},
		{
			GameTags.Robot,
			ROBOTS.CATEGORY_NAME
		}
	};

	private List<GameObject> setInactiveQueue = new List<GameObject>();

	private bool robotsHasEverBeenOpened = false;

	public override string GetTitle()
	{
		if (target != null)
		{
			return string.Format(base.GetTitle(), target.GetProperName());
		}
		return base.GetTitle();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		SpawnContainers();
		Game.Instance.Subscribe(586301400, OnMinionsChanged);
		Components.LiveMinionIdentities.OnAdd += OnMinionsChanged;
		Components.LiveMinionIdentities.OnRemove += OnMinionsChanged;
	}

	private void OnMinionsChanged(object data)
	{
		if (!(target == null))
		{
			Refresh();
		}
	}

	private void SpawnContainers()
	{
		if (!containersSpawned)
		{
			standardMinionSectionHeader = Util.KInstantiateUI(entityCategoryPrefab, scrollContents, force_active: true);
			standardMinionSectionContent = standardMinionSectionHeader.GetComponent<HierarchyReferences>().GetReference<RectTransform>("Content").gameObject;
			bionicMinionSectionHeader = Util.KInstantiateUI(entityCategoryPrefab, scrollContents, force_active: true);
			bionicMinionSectionContent = bionicMinionSectionHeader.GetComponent<HierarchyReferences>().GetReference<RectTransform>("Content").gameObject;
			robotSectionHeader = Util.KInstantiateUI(entityCategoryPrefab, scrollContents, force_active: true);
			robotSectionContent = robotSectionHeader.GetComponent<HierarchyReferences>().GetReference<RectTransform>("Content").gameObject;
			containersSpawned = true;
		}
	}

	public override bool IsValidForTarget(GameObject target)
	{
		return target.GetComponent<AccessControl>() != null && target.GetComponent<AccessControl>().controlEnabled;
	}

	public override void SetTarget(GameObject target)
	{
		if (this.target != null)
		{
			ClearTarget();
		}
		SpawnContainers();
		this.target = target.GetComponent<AccessControl>();
		doorTarget = target.GetComponent<Door>();
		if (!(this.target == null))
		{
			target.Subscribe(1734268753, OnDoorStateChanged);
			target.Subscribe(-1525636549, OnAccessControlChanged);
			base.gameObject.SetActive(value: true);
			RefreshContainerObjects();
			Refresh();
		}
	}

	public override void ClearTarget()
	{
		base.ClearTarget();
		if (target != null)
		{
			target.Unsubscribe(1734268753, OnDoorStateChanged);
			target.Unsubscribe(-1525636549, OnAccessControlChanged);
		}
	}

	private void Refresh()
	{
		Rotatable component = target.GetComponent<Rotatable>();
		bool flag = component != null && component.IsRotated;
		ClearOldRows();
		PopulateRows();
		GameObject gameObject = standardMinionSectionHeader.GetComponent<HierarchyReferences>().GetReference<RectTransform>("EmptyRow").gameObject;
		gameObject.SetActive(standardMinionSectionContent.transform.childCount <= 1);
		if (standardMinionSectionContent.transform.childCount <= 1)
		{
			ToggleCategoryCollapsed(targetState: false, standardMinionSectionContent.rectTransform(), standardMinionSectionHeader.GetComponent<HierarchyReferences>().GetReference<MultiToggle>("CollapseToggle"));
		}
		GameObject gameObject2 = bionicMinionSectionHeader.GetComponent<HierarchyReferences>().GetReference<RectTransform>("EmptyRow").gameObject;
		gameObject2.SetActive(bionicMinionSectionContent.transform.childCount <= 1);
		if (bionicMinionSectionContent.transform.childCount <= 1)
		{
			ToggleCategoryCollapsed(targetState: false, bionicMinionSectionContent.rectTransform(), bionicMinionSectionHeader.GetComponent<HierarchyReferences>().GetReference<MultiToggle>("CollapseToggle"));
		}
		GameObject gameObject3 = robotSectionHeader.GetComponent<HierarchyReferences>().GetReference<RectTransform>("EmptyRow").gameObject;
		gameObject3.SetActive(robotSectionContent.transform.childCount <= 1);
		if (!robotsHasEverBeenOpened)
		{
			ToggleCategoryCollapsed(targetState: false, robotSectionContent.rectTransform(), robotSectionHeader.GetComponent<HierarchyReferences>().GetReference<MultiToggle>("CollapseToggle"));
		}
		foreach (GameObject item in setInactiveQueue)
		{
			item.SetActive(value: false);
		}
		disabledOverlay.SetActive(target.GetComponent<AccessControl>().overrideAccess == Door.ControlState.Locked);
	}

	private void ClearOldRows()
	{
		foreach (KeyValuePair<MinionAssignablesProxy, GameObject> minionIdentityRow in minionIdentityRows)
		{
			inactiveRowPool.Add(minionIdentityRow.Value);
			setInactiveQueue.Add(minionIdentityRow.Value);
		}
		minionIdentityRows.Clear();
		foreach (KeyValuePair<Tag, GameObject> robotRow in robotRows)
		{
			inactiveRowPool.Add(robotRow.Value);
			setInactiveQueue.Add(robotRow.Value);
		}
		robotRows.Clear();
	}

	private void RefreshContainerObjects()
	{
		RefreshContainer(standardMinionSectionHeader, GameTags.Minions.Models.Standard, enabled: true);
		RefreshContainer(bionicMinionSectionHeader, GameTags.Minions.Models.Bionic, Game.IsDlcActiveForCurrentSave("DLC3_ID"));
		RefreshContainer(robotSectionHeader, GameTags.Robot, enabled: true);
		void RefreshContainer(GameObject container, Tag containerTag, bool enabled)
		{
			if (!enabled)
			{
				container.SetActive(value: false);
			}
			else
			{
				container.SetActive(value: true);
				HierarchyReferences component = container.GetComponent<HierarchyReferences>();
				MultiToggle reference = component.GetReference<MultiToggle>("ToggleLeft");
				MultiToggle reference2 = component.GetReference<MultiToggle>("ToggleRight");
				LocText reference3 = component.GetReference<LocText>("CategoryLabel");
				MultiToggle collapseToggle = component.GetReference<MultiToggle>("CollapseToggle");
				RectTransform content = component.GetReference<RectTransform>("Content");
				component.GetReference<LocText>("CategoryLabel").SetText(categoryNames[containerTag]);
				component.GetReference<ToolTip>("HeaderTooltip").SetSimpleTooltip(UI.UISIDESCREENS.ACCESS_CONTROL_SIDE_SCREEN.CATEGORY_HEADER_TOOLTIP);
				AccessControl.Permission defaultPermission = target.GetDefaultPermission(containerTag);
				bool flag = defaultPermission == AccessControl.Permission.Both || defaultPermission == AccessControl.Permission.GoLeft;
				bool flag2 = defaultPermission == AccessControl.Permission.Both || defaultPermission == AccessControl.Permission.GoRight;
				reference.ChangeState((!flag) ? 1 : 0);
				reference2.ChangeState((!flag2) ? 1 : 0);
				reference.onClick = delegate
				{
					switch (target.GetDefaultPermission(containerTag))
					{
					case AccessControl.Permission.Both:
						target.SetDefaultPermission(containerTag, AccessControl.Permission.GoRight);
						break;
					case AccessControl.Permission.Neither:
						target.SetDefaultPermission(containerTag, AccessControl.Permission.GoLeft);
						break;
					case AccessControl.Permission.GoLeft:
						target.SetDefaultPermission(containerTag, AccessControl.Permission.Neither);
						break;
					case AccessControl.Permission.GoRight:
						target.SetDefaultPermission(containerTag, AccessControl.Permission.Both);
						break;
					}
					RefreshContainerObjects();
				};
				reference2.onClick = delegate
				{
					switch (target.GetDefaultPermission(containerTag))
					{
					case AccessControl.Permission.Both:
						target.SetDefaultPermission(containerTag, AccessControl.Permission.GoLeft);
						break;
					case AccessControl.Permission.Neither:
						target.SetDefaultPermission(containerTag, AccessControl.Permission.GoRight);
						break;
					case AccessControl.Permission.GoLeft:
						target.SetDefaultPermission(containerTag, AccessControl.Permission.Both);
						break;
					case AccessControl.Permission.GoRight:
						target.SetDefaultPermission(containerTag, AccessControl.Permission.Neither);
						break;
					}
					RefreshContainerObjects();
				};
				collapseToggle.onClick = delegate
				{
					if (containerTag == GameTags.Robot)
					{
						robotsHasEverBeenOpened = true;
					}
					ToggleCategoryCollapsed(!content.gameObject.activeSelf, content, collapseToggle);
				};
			}
		}
	}

	private void ToggleCategoryCollapsed(bool targetState, RectTransform content, MultiToggle collapseToggle)
	{
		content.gameObject.SetActive(targetState);
		collapseToggle.ChangeState(content.gameObject.activeSelf ? 1 : 0);
	}

	private GameObject InstantiateIndentityRow(GameObject parent)
	{
		if (inactiveRowPool.Count > 0)
		{
			GameObject gameObject = inactiveRowPool[0];
			inactiveRowPool.Remove(gameObject);
			if (gameObject.transform.parent != parent.transform)
			{
				gameObject.transform.SetParent(parent.transform);
			}
			gameObject.transform.SetAsLastSibling();
			gameObject.SetActive(value: true);
			if (setInactiveQueue.Contains(gameObject))
			{
				setInactiveQueue.Remove(gameObject);
			}
			return gameObject;
		}
		return Util.KInstantiateUI(rowPrefab, parent, force_active: true);
	}

	private void PopulateRows()
	{
		for (int i = 0; i < Components.MinionAssignablesProxy.Count; i++)
		{
			MinionAssignablesProxy minionAssignablesProxy = Components.MinionAssignablesProxy[i];
			if (!minionAssignablesProxy.HasTag(GameTags.Dead))
			{
				ConfigureRow(minionAssignablesProxy);
			}
		}
		if (Game.IsDlcActiveForCurrentSave("DLC3_ID"))
		{
			ConfigureRow(GameTags.Robots.Models.FetchDrone);
		}
		if (Game.IsDlcActiveForCurrentSave("EXPANSION1_ID"))
		{
			ConfigureRow(GameTags.Robots.Models.ScoutRover);
		}
		ConfigureRow(GameTags.Robots.Models.MorbRover);
	}

	private void ConfigureRow(object entity)
	{
		GameObject parent = null;
		MinionAssignablesProxy minion = entity as MinionAssignablesProxy;
		Tag robotTag = GameTags.Robot;
		if (entity is Tag)
		{
			robotTag = (Tag)entity;
		}
		if (minion != null)
		{
			GameObject targetGameObject = minion.GetTargetGameObject();
			StoredMinionIdentity component = targetGameObject.GetComponent<StoredMinionIdentity>();
			if (component != null)
			{
				if (component.model == GameTags.Minions.Models.Standard)
				{
					parent = standardMinionSectionContent;
				}
				else if (component.model == GameTags.Minions.Models.Bionic)
				{
					parent = bionicMinionSectionContent;
				}
			}
			else if (targetGameObject.HasTag(GameTags.Minions.Models.Standard))
			{
				parent = standardMinionSectionContent;
			}
			else if (targetGameObject.HasTag(GameTags.Minions.Models.Bionic))
			{
				parent = bionicMinionSectionContent;
			}
		}
		else
		{
			parent = robotSectionContent;
		}
		GameObject gameObject = InstantiateIndentityRow(parent);
		HierarchyReferences component2 = gameObject.GetComponent<HierarchyReferences>();
		CrewPortrait reference = component2.GetReference<CrewPortrait>("Portrait");
		RectTransform reference2 = component2.GetReference<RectTransform>("Icon");
		if (minion != null)
		{
			if ((Object)reference.identityObject != minion)
			{
				reference.SetIdentityObject(minion, jobEnabled: false);
			}
			reference.transform.parent.gameObject.SetActive(value: true);
			reference2.gameObject.SetActive(value: false);
		}
		else
		{
			reference.transform.parent.gameObject.SetActive(value: false);
			reference2.gameObject.SetActive(value: true);
			reference2.GetComponent<Image>().sprite = Def.GetUISprite(robotTag).first;
			component2.GetReference<LocText>("NameLabel").SetText(robotTag.ProperName());
		}
		MultiToggle reference3 = component2.GetReference<MultiToggle>("UseDefaultButton");
		reference3.GetComponent<ToolTip>().SetSimpleTooltip(UI.UISIDESCREENS.ACCESS_CONTROL_SIDE_SCREEN.MINION_SELECT_TOOLTIP);
		if (minion != null)
		{
			reference3.ChangeState(target.IsDefaultPermission(minion) ? 1 : 0);
			component2.GetReference<LocText>("AccessSettingLabel").SetText(target.IsDefaultPermission(minion) ? UI.UISIDESCREENS.ACCESS_CONTROL_SIDE_SCREEN.USING_DEFAULT : UI.UISIDESCREENS.ACCESS_CONTROL_SIDE_SCREEN.USING_CUSTOM);
		}
		else
		{
			reference3.ChangeState(target.IsDefaultPermission(robotTag) ? 1 : 0);
			component2.GetReference<LocText>("AccessSettingLabel").SetText(target.IsDefaultPermission(robotTag) ? UI.UISIDESCREENS.ACCESS_CONTROL_SIDE_SCREEN.USING_DEFAULT : UI.UISIDESCREENS.ACCESS_CONTROL_SIDE_SCREEN.USING_CUSTOM);
		}
		reference3.onClick = delegate
		{
			if (minion != null)
			{
				if (target.IsDefaultPermission(minion))
				{
					target.SetPermission(minion, target.GetDefaultPermission(minion.GetMinionModel()));
				}
				else
				{
					target.ClearPermission(minion);
				}
			}
			else if (target.IsDefaultPermission(robotTag))
			{
				target.ClearPermission(robotTag, GameTags.Robot);
				target.SetPermission(robotTag, target.GetDefaultPermission(robotTag));
			}
			else
			{
				target.ClearPermission(robotTag, GameTags.Robot);
			}
			Refresh();
		};
		MultiToggle reference4 = component2.GetReference<MultiToggle>("ToggleLeft");
		MultiToggle reference5 = component2.GetReference<MultiToggle>("ToggleRight");
		AccessControl.Permission permission = AccessControl.Permission.Both;
		permission = ((!(minion != null)) ? target.GetSetPermission(robotTag) : target.GetSetPermission(minion));
		bool flag = permission == AccessControl.Permission.Both || permission == AccessControl.Permission.GoLeft;
		bool flag2 = permission == AccessControl.Permission.Both || permission == AccessControl.Permission.GoRight;
		reference4.ChangeState((!flag) ? 1 : 0);
		reference5.ChangeState((!flag2) ? 1 : 0);
		reference4.onClick = delegate
		{
			if (minion != null)
			{
				switch (target.GetSetPermission(minion))
				{
				case AccessControl.Permission.Both:
					target.SetPermission(minion, AccessControl.Permission.GoRight);
					break;
				case AccessControl.Permission.Neither:
					target.SetPermission(minion, AccessControl.Permission.GoLeft);
					break;
				case AccessControl.Permission.GoLeft:
					target.SetPermission(minion, AccessControl.Permission.Neither);
					break;
				case AccessControl.Permission.GoRight:
					target.SetPermission(minion, AccessControl.Permission.Both);
					break;
				}
			}
			else
			{
				switch (target.GetSetPermission(robotTag))
				{
				case AccessControl.Permission.Both:
					target.SetPermission(robotTag, AccessControl.Permission.GoRight);
					break;
				case AccessControl.Permission.Neither:
					target.SetPermission(robotTag, AccessControl.Permission.GoLeft);
					break;
				case AccessControl.Permission.GoLeft:
					target.SetPermission(robotTag, AccessControl.Permission.Neither);
					break;
				case AccessControl.Permission.GoRight:
					target.SetPermission(robotTag, AccessControl.Permission.Both);
					break;
				}
			}
			Refresh();
		};
		reference5.onClick = delegate
		{
			if (minion != null)
			{
				switch (target.GetSetPermission(minion))
				{
				case AccessControl.Permission.Both:
					target.SetPermission(minion, AccessControl.Permission.GoLeft);
					break;
				case AccessControl.Permission.Neither:
					target.SetPermission(minion, AccessControl.Permission.GoRight);
					break;
				case AccessControl.Permission.GoLeft:
					target.SetPermission(minion, AccessControl.Permission.Both);
					break;
				case AccessControl.Permission.GoRight:
					target.SetPermission(minion, AccessControl.Permission.Neither);
					break;
				}
			}
			else
			{
				switch (target.GetSetPermission(robotTag))
				{
				case AccessControl.Permission.Both:
					target.SetPermission(robotTag, AccessControl.Permission.GoLeft);
					break;
				case AccessControl.Permission.Neither:
					target.SetPermission(robotTag, AccessControl.Permission.GoRight);
					break;
				case AccessControl.Permission.GoLeft:
					target.SetPermission(robotTag, AccessControl.Permission.Both);
					break;
				case AccessControl.Permission.GoRight:
					target.SetPermission(robotTag, AccessControl.Permission.Neither);
					break;
				}
			}
			Refresh();
		};
		GameObject gameObject2 = component2.GetReference<RectTransform>("DirectionToggles").gameObject;
		RectTransform reference6 = component2.GetReference<RectTransform>("DittoMark");
		if (minion != null)
		{
			gameObject2.SetActive(!target.IsDefaultPermission(minion));
			reference6.gameObject.SetActive(target.IsDefaultPermission(minion));
		}
		else
		{
			gameObject2.SetActive(!target.IsDefaultPermission(robotTag));
			reference6.gameObject.SetActive(target.IsDefaultPermission(robotTag));
		}
		if (minion != null)
		{
			minionIdentityRows.Add(minion, gameObject);
		}
		else
		{
			robotRows.Add(robotTag, gameObject);
		}
	}

	private void OnDoorStateChanged(object data)
	{
		Refresh();
	}

	private void OnAccessControlChanged(object data)
	{
		Refresh();
	}
}
