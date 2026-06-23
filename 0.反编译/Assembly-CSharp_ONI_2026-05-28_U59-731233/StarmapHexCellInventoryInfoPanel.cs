using System.Collections.Generic;
using STRINGS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarmapHexCellInventoryInfoPanel : SimpleInfoPanel
{
	private Dictionary<Tag, GameObject> itemRows = new Dictionary<Tag, GameObject>();

	public StarmapHexCellInventoryInfoPanel(SimpleInfoScreen simpleInfoScreen)
		: base(simpleInfoScreen)
	{
	}

	public override void Refresh(CollapsibleDetailContentPanel panel, GameObject selectedTarget)
	{
		if (!IsValidTarget(selectedTarget, out var hexCellInventory))
		{
			panel.gameObject.SetActive(value: false);
			return;
		}
		panel.SetTitle(UI.CLUSTERMAP.HEXCELL_INVENTORY.UI_PANEL.TITLE);
		RefreshElements(panel, hexCellInventory);
		panel.gameObject.SetActive(value: true);
	}

	private void RefreshElements(CollapsibleDetailContentPanel panel, StarmapHexCellInventory hexCellInventory)
	{
		foreach (KeyValuePair<Tag, GameObject> itemRow in itemRows)
		{
			if (itemRow.Value != null)
			{
				itemRow.Value.SetActive(value: false);
			}
		}
		if (hexCellInventory == null)
		{
			return;
		}
		List<StarmapHexCellInventory.SerializedItem> list = new List<StarmapHexCellInventory.SerializedItem>(hexCellInventory.Items);
		list.Sort((StarmapHexCellInventory.SerializedItem a, StarmapHexCellInventory.SerializedItem b) => b.Mass.CompareTo(a.Mass));
		foreach (StarmapHexCellInventory.SerializedItem item in list)
		{
			Tag iD = item.ID;
			if (!itemRows.TryGetValue(iD, out var value))
			{
				value = Util.KInstantiateUI(simpleInfoRoot.iconLabelRow, panel.Content.gameObject, force_active: true);
				itemRows.Add(iD, value);
			}
			value.SetActive(value: true);
			HierarchyReferences component = value.GetComponent<HierarchyReferences>();
			Tuple<Sprite, Color> uISprite = Def.GetUISprite(iD);
			component.GetReference<Image>("Icon").sprite = uISprite.first;
			component.GetReference<Image>("Icon").color = uISprite.second;
			component.GetReference<LocText>("NameLabel").text = (item.IsEntity ? item.ID.ProperName() : ElementLoader.GetElement(iD).name);
			component.GetReference<LocText>("ValueLabel").text = (item.IsEntity ? GameUtil.GetFormattedUnits(item.Mass) : GameUtil.GetFormattedMass(item.Mass));
			component.GetReference<LocText>("ValueLabel").alignment = TextAlignmentOptions.MidlineRight;
		}
	}

	public bool IsValidTarget(GameObject go, out StarmapHexCellInventory hexCellInventory)
	{
		hexCellInventory = null;
		if (go == null)
		{
			return false;
		}
		hexCellInventory = go.GetComponent<StarmapHexCellInventory>();
		return hexCellInventory != null;
	}
}
