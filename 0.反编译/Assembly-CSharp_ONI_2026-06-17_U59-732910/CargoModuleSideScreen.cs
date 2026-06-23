using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class CargoModuleSideScreen : SideScreenContent, ISimEveryTick
{
	private Clustercraft targetCraft;

	private Dictionary<IHexCellCollector, GameObject> modulePanels = new Dictionary<IHexCellCollector, GameObject>();

	public GameObject moduleContentContainer;

	public GameObject modulePanelPrefab;

	[SerializeField]
	private LayoutElement scrollRectLayout;

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
		if (target.GetComponent<Clustercraft>() != null)
		{
			return GetCollectionModules(target.GetComponent<Clustercraft>()).Length != 0;
		}
		return false;
	}

	public override void SetTarget(GameObject target)
	{
		base.SetTarget(target);
		targetCraft = target.GetComponent<Clustercraft>();
		RefreshModulePanel(targetCraft);
	}

	private IHexCellCollector[] GetCollectionModules(Clustercraft craft)
	{
		List<IHexCellCollector> list = new List<IHexCellCollector>();
		foreach (Ref<RocketModuleCluster> clusterModule in craft.ModuleInterface.ClusterModules)
		{
			IHexCellCollector hexCellCollector = clusterModule.Get().GetComponent<IHexCellCollector>();
			if (hexCellCollector == null)
			{
				hexCellCollector = clusterModule.Get().GetSMI<IHexCellCollector>();
			}
			if (hexCellCollector != null)
			{
				list.Add(hexCellCollector);
			}
		}
		return list.ToArray();
	}

	private void RefreshModulePanel(Clustercraft module)
	{
		ClearModules();
		IHexCellCollector[] collectionModules = GetCollectionModules(module);
		foreach (IHexCellCollector hexCellCollector in collectionModules)
		{
			GameObject gameObject = Util.KInstantiateUI(modulePanelPrefab, moduleContentContainer, force_active: true);
			modulePanels.Add(hexCellCollector, gameObject);
			HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
			component.GetReference<Image>("icon").sprite = hexCellCollector.GetUISprite();
			component.GetReference<LocText>("label").SetText(hexCellCollector.GetProperName());
		}
		RefreshProgressBars();
		LayoutElement layoutElement = scrollRectLayout;
		float preferredHeight = (scrollRectLayout.minHeight = Mathf.Min(modulePanels.Count, 2.5f) * modulePanelPrefab.GetComponent<RectTransform>().rect.height);
		layoutElement.preferredHeight = preferredHeight;
	}

	private void ClearModules()
	{
		foreach (KeyValuePair<IHexCellCollector, GameObject> modulePanel in modulePanels)
		{
			Util.KDestroyGameObject(modulePanel.Value.gameObject);
		}
		modulePanels.Clear();
	}

	private void RefreshProgressBars()
	{
		if (targetCraft.IsNullOrDestroyed() || ClusterMapSelectTool.Instance.GetSelected() == null || !IsValidForTarget(ClusterMapSelectTool.Instance.GetSelected().gameObject))
		{
			return;
		}
		foreach (KeyValuePair<IHexCellCollector, GameObject> modulePanel in modulePanels)
		{
			HierarchyReferences component = modulePanel.Value.GetComponent<HierarchyReferences>();
			GenericUIProgressBar reference = component.GetReference<GenericUIProgressBar>("gatheringProgressBar");
			float num = 4f;
			float num2 = 0f;
			float num3 = modulePanel.Key.GetCapacity() - modulePanel.Key.GetMassStored();
			if (modulePanel.Key.CheckIsCollecting())
			{
				num2 = modulePanel.Key.TimeInState() % num;
				if (num3 > 0f)
				{
					reference.SetFillPercentage(num2 / num);
					reference.label.SetText(UI.UISIDESCREENS.CARGOMODULESIDESCREEN.GATHERING_IN_PROGRESS);
				}
			}
			else if (num3 == 0f)
			{
				reference.SetFillPercentage(0f);
				reference.label.SetText(UI.UISIDESCREENS.CARGOMODULESIDESCREEN.GATHERING_FULL);
			}
			else
			{
				reference.SetFillPercentage(0f);
				reference.label.SetText(UI.UISIDESCREENS.CARGOMODULESIDESCREEN.GATHERING_STOPPED);
			}
			GenericUIProgressBar reference2 = component.GetReference<GenericUIProgressBar>("capacityProgressBar");
			float fillPercentage = modulePanel.Key.GetMassStored() / modulePanel.Key.GetCapacity();
			reference2.SetFillPercentage(fillPercentage);
			reference2.label.SetText(modulePanel.Key.GetCapacityBarText());
		}
	}

	public void SimEveryTick(float dt)
	{
		RefreshProgressBars();
	}
}
