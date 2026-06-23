using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailCollapsableLabel : MonoBehaviour
{
	public class ContentRow
	{
		public bool inUse;

		public DetailLabelWithButton label;
	}

	public Image arrowImage;

	public LocText nameLabel;

	public LocText valueLabel;

	public ToolTip toolTip;

	public KToggle toggle;

	[SerializeField]
	private GameObject contentRowPrefab;

	[Header("Icons")]
	public Sprite collapsedIcon;

	public Sprite unfoldedIcon;

	public Action<DetailCollapsableLabel> OnExpanded;

	public Action<DetailCollapsableLabel> OnCollapsed;

	private int lastKnownRowAvailable;

	public List<ContentRow> contentRows = new List<ContentRow>();

	private object data;

	public bool IsExpanded => toggle.isOn;

	public object Data => data;

	private void OnDisable()
	{
		MarkAllRowsUnused();
		RefreshRowVisibilityState();
		toggle.SetIsOnWithoutNotify(value: false);
		RefreshArrowIcon();
	}

	public void SetData(object data)
	{
		this.data = data;
	}

	public void ClearToggleCallbacks()
	{
		toggle.ClearOnValueChanged();
		toggle.onValueChanged += OnToggleChanged;
		OnExpanded = null;
		OnCollapsed = null;
	}

	public void MarkAllRowsUnused()
	{
		lastKnownRowAvailable = 0;
		foreach (ContentRow contentRow in contentRows)
		{
			contentRow.inUse = false;
		}
	}

	public void RefreshRowVisibilityState()
	{
		bool flag = false;
		foreach (ContentRow contentRow in contentRows)
		{
			if (contentRow.label.gameObject.activeInHierarchy != contentRow.inUse)
			{
				contentRow.label.gameObject.SetActive(contentRow.inUse);
				flag = true;
			}
		}
	}

	public DetailLabelWithButton AddOrGetAvailableContentRow()
	{
		ContentRow contentRow = ((lastKnownRowAvailable < contentRows.Count && !contentRows[lastKnownRowAvailable].inUse) ? contentRows[lastKnownRowAvailable] : null);
		int siblingIndex = base.transform.GetSiblingIndex();
		if (contentRow == null)
		{
			contentRow = new ContentRow
			{
				label = Util.KInstantiateUI(contentRowPrefab, base.transform.parent.gameObject).GetComponent<DetailLabelWithButton>()
			};
			contentRows.Add(contentRow);
		}
		contentRow.inUse = true;
		contentRow.label.transform.SetSiblingIndex(siblingIndex + 1);
		lastKnownRowAvailable++;
		return contentRow.label;
	}

	private void OnToggleChanged(bool expanded)
	{
		RefreshArrowIcon();
		if (expanded)
		{
			if (OnExpanded != null)
			{
				OnExpanded(this);
			}
		}
		else if (OnCollapsed != null)
		{
			OnCollapsed(this);
		}
	}

	public void ManualTriggerOnExpanded()
	{
		if (OnExpanded != null)
		{
			OnExpanded(this);
		}
	}

	private void RefreshArrowIcon()
	{
		arrowImage.sprite = (toggle.isOn ? unfoldedIcon : collapsedIcon);
		arrowImage.enabled = false;
		arrowImage.enabled = true;
	}
}
