using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class ModuleFlightUtilitySideScreen : SideScreenContent
{
	private Clustercraft targetCraft;

	public GameObject moduleContentContainer;

	public GameObject modulePanelPrefab;

	public ColorStyleSetting repeatOff;

	public ColorStyleSetting repeatOn;

	private Dictionary<IEmptyableCargo, HierarchyReferences> modulePanels = new Dictionary<IEmptyableCargo, HierarchyReferences>();

	[SerializeField]
	private LayoutElement scrollRectLayout;

	private List<int> refreshHandle = new List<int>();

	private CraftModuleInterface craftModuleInterface => targetCraft.GetComponent<CraftModuleInterface>();

	protected override void OnShow(bool show)
	{
		base.OnShow(show);
		base.ConsumeMouseScroll = true;
	}

	public override float GetSortKey()
	{
		return 21f;
	}

	public override bool IsValidForTarget(GameObject target)
	{
		if (target.GetComponent<Clustercraft>() != null && HasFlightUtilityModule(target.GetComponent<CraftModuleInterface>()))
		{
			return true;
		}
		RocketControlStation component = target.GetComponent<RocketControlStation>();
		if (component != null)
		{
			return HasFlightUtilityModule(component.GetMyWorld().GetComponent<Clustercraft>().ModuleInterface);
		}
		return false;
	}

	private bool HasFlightUtilityModule(CraftModuleInterface craftModuleInterface)
	{
		foreach (Ref<RocketModuleCluster> clusterModule in craftModuleInterface.ClusterModules)
		{
			if (clusterModule.Get().GetSMI<IEmptyableCargo>() != null)
			{
				return true;
			}
		}
		return false;
	}

	public override void SetTarget(GameObject target)
	{
		if (target != null)
		{
			foreach (int item in refreshHandle)
			{
				target.Unsubscribe(item);
			}
			refreshHandle.Clear();
		}
		base.SetTarget(target);
		targetCraft = target.GetComponent<Clustercraft>();
		if (targetCraft == null && target.GetComponent<RocketControlStation>() != null)
		{
			targetCraft = target.GetMyWorld().GetComponent<Clustercraft>();
		}
		refreshHandle.Add(targetCraft.gameObject.Subscribe(-1298331547, RefreshAll));
		refreshHandle.Add(targetCraft.gameObject.Subscribe(1792516731, RefreshAll));
		BuildModules();
	}

	private void ClearModules()
	{
		foreach (KeyValuePair<IEmptyableCargo, HierarchyReferences> modulePanel in modulePanels)
		{
			Util.KDestroyGameObject(modulePanel.Value.gameObject);
		}
		modulePanels.Clear();
	}

	private void BuildModules()
	{
		ClearModules();
		foreach (Ref<RocketModuleCluster> clusterModule in craftModuleInterface.ClusterModules)
		{
			IEmptyableCargo sMI = clusterModule.Get().GetSMI<IEmptyableCargo>();
			if (sMI != null)
			{
				HierarchyReferences value = Util.KInstantiateUI<HierarchyReferences>(modulePanelPrefab, moduleContentContainer, force_active: true);
				modulePanels.Add(sMI, value);
				RefreshModulePanel(sMI);
			}
		}
		LayoutElement layoutElement = scrollRectLayout;
		float preferredHeight = (scrollRectLayout.minHeight = Mathf.Min(modulePanels.Count, 2.5f) * modulePanelPrefab.GetComponent<RectTransform>().rect.height);
		layoutElement.preferredHeight = preferredHeight;
	}

	private void RefreshAll(object data = null)
	{
		BuildModules();
	}

	private void RefreshModulePanel(IEmptyableCargo module)
	{
		HierarchyReferences hierarchyReferences = modulePanels[module];
		hierarchyReferences.GetReference<Image>("icon").sprite = Def.GetUISprite(module.master.gameObject).first;
		hierarchyReferences.GetReference<RectTransform>("targetButtons").gameObject.SetActive(module.CanTargetClusterGridEntities);
		if (module.CanTargetClusterGridEntities)
		{
			KButton reference = hierarchyReferences.GetReference<KButton>("selectTargetButton");
			reference.onClick += delegate
			{
				ClusterMapScreen.Instance.ShowInSelectDestinationMode(module.master.GetComponent<ClusterDestinationSelector>());
			};
			KButton reference2 = hierarchyReferences.GetReference<KButton>("clearTargetButton");
			reference2.GetComponentInChildren<ToolTip>().SetSimpleTooltip(UI.UISIDESCREENS.MODULEFLIGHTUTILITYSIDESCREEN.CLEAR_TARGET_BUTTON_TOOLTIP);
			reference2.onClick += delegate
			{
				module.master.GetComponent<EntityClusterDestinationSelector>().SetDestination(AxialI.INVALID);
				RefreshModulePanel(module);
			};
			if (module.master.GetComponent<EntityClusterDestinationSelector>().GetClusterEntityTarget() != null)
			{
				reference.GetComponentInChildren<LocText>().text = (module as StateMachine.Instance).GetMaster().GetComponent<EntityClusterDestinationSelector>().GetClusterEntityTarget()
					.GetProperName();
				reference.isInteractable = false;
			}
			else
			{
				reference.GetComponentInChildren<LocText>().text = UI.UISIDESCREENS.MODULEFLIGHTUTILITYSIDESCREEN.SELECT_TARGET_BUTTON;
				reference.isInteractable = true;
			}
		}
		KButton reference3 = hierarchyReferences.GetReference<KButton>("button");
		reference3.isInteractable = module.CanEmptyCargo();
		reference3.GetComponentInChildren<LocText>().text = module.GetButtonText;
		reference3.GetComponentInChildren<ToolTip>().SetSimpleTooltip(module.GetButtonToolip);
		reference3.ClearOnClick();
		reference3.onClick += module.EmptyCargo;
		KButton reference4 = hierarchyReferences.GetReference<KButton>("repeatButton");
		if (module.CanAutoDeploy)
		{
			StyleRepeatButton(module);
			reference4.ClearOnClick();
			reference4.onClick += delegate
			{
				OnRepeatClicked(module);
			};
			reference4.gameObject.SetActive(value: true);
		}
		else
		{
			reference4.gameObject.SetActive(value: false);
		}
		DropDown reference5 = hierarchyReferences.GetReference<DropDown>("dropDown");
		reference5.targetDropDownContainer = GameScreenManager.Instance.ssOverlayCanvas;
		reference5.Close();
		CrewPortrait reference6 = hierarchyReferences.GetReference<CrewPortrait>("selectedPortrait");
		WorldContainer component = (module as StateMachine.Instance).GetMaster().GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<WorldContainer>();
		if (component != null && module.ChooseDuplicant)
		{
			if (module.ChosenDuplicant != null && module.ChosenDuplicant.HasTag(GameTags.Dead))
			{
				module.ChosenDuplicant = null;
			}
			int id = component.id;
			reference5.gameObject.SetActive(value: true);
			reference5.Initialize(Components.LiveMinionIdentities.GetWorldItems(id), OnDuplicantEntryClick, null, DropDownEntryRefreshAction, displaySelectedValueWhenClosed: true, module);
			reference5.selectedLabel.text = ((module.ChosenDuplicant != null) ? GetDuplicantRowName(module.ChosenDuplicant) : UI.UISIDESCREENS.MODULEFLIGHTUTILITYSIDESCREEN.SELECT_DUPLICANT.ToString());
			reference6.gameObject.SetActive(value: true);
			reference6.SetIdentityObject(module.ChosenDuplicant, jobEnabled: false);
			reference5.openButton.isInteractable = !module.ModuleDeployed;
		}
		else
		{
			reference5.gameObject.SetActive(value: false);
			reference6.gameObject.SetActive(value: false);
		}
		hierarchyReferences.GetReference<LocText>("label").SetText(module.master.gameObject.GetProperName());
	}

	private string GetDuplicantRowName(MinionIdentity minion)
	{
		MinionResume component = minion.GetComponent<MinionResume>();
		if (component != null && component.HasPerk(Db.Get().SkillPerks.CanUseRocketControlStation))
		{
			return string.Format(UI.UISIDESCREENS.MODULEFLIGHTUTILITYSIDESCREEN.PILOT_FMT, minion.GetProperName());
		}
		return minion.GetProperName();
	}

	private void OnRepeatClicked(IEmptyableCargo module)
	{
		module.AutoDeploy = !module.AutoDeploy;
		StyleRepeatButton(module);
	}

	private void OnDuplicantEntryClick(IListableOption option, object data)
	{
		MinionIdentity chosenDuplicant = (MinionIdentity)option;
		IEmptyableCargo emptyableCargo = (IEmptyableCargo)data;
		emptyableCargo.ChosenDuplicant = chosenDuplicant;
		HierarchyReferences hierarchyReferences = modulePanels[emptyableCargo];
		hierarchyReferences.GetReference<DropDown>("dropDown").selectedLabel.text = ((emptyableCargo.ChosenDuplicant != null) ? GetDuplicantRowName(emptyableCargo.ChosenDuplicant) : UI.UISIDESCREENS.MODULEFLIGHTUTILITYSIDESCREEN.SELECT_DUPLICANT.ToString());
		hierarchyReferences.GetReference<CrewPortrait>("selectedPortrait").SetIdentityObject(emptyableCargo.ChosenDuplicant, jobEnabled: false);
		RefreshAll();
	}

	private void DropDownEntryRefreshAction(DropDownEntry entry, object targetData)
	{
		MinionIdentity minionIdentity = (MinionIdentity)entry.entryData;
		entry.label.text = GetDuplicantRowName(minionIdentity);
		entry.portrait.SetIdentityObject(minionIdentity, jobEnabled: false);
		bool flag = false;
		foreach (Ref<RocketModuleCluster> clusterModule in targetCraft.ModuleInterface.ClusterModules)
		{
			RocketModuleCluster rocketModuleCluster = clusterModule.Get();
			if (!(rocketModuleCluster == null))
			{
				IEmptyableCargo sMI = rocketModuleCluster.GetSMI<IEmptyableCargo>();
				if (sMI != null && !(((IEmptyableCargo)targetData).ChosenDuplicant == minionIdentity))
				{
					flag = flag || sMI.ChosenDuplicant == minionIdentity;
				}
			}
		}
		entry.button.isInteractable = !flag;
	}

	private void StyleRepeatButton(IEmptyableCargo module)
	{
		KButton reference = modulePanels[module].GetReference<KButton>("repeatButton");
		reference.bgImage.colorStyleSetting = (module.AutoDeploy ? repeatOn : repeatOff);
		reference.bgImage.ApplyColorStyleSetting();
	}
}
