using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("KMonoBehaviour/scripts/TableRow")]
public class TableRow : KMonoBehaviour
{
	public enum RowType
	{
		Header,
		Default,
		Minion,
		StoredMinon,
		WorldDivider
	}

	public RowType rowType;

	private IAssignableIdentity minion;

	private Dictionary<TableColumn, GameObject> widgets = new Dictionary<TableColumn, GameObject>();

	private Dictionary<string, GameObject> scrollers = new Dictionary<string, GameObject>();

	private Dictionary<string, GameObject> scrollerBorders = new Dictionary<string, GameObject>();

	public bool isDefault;

	public KButton selectMinionButton;

	[SerializeField]
	private ColorStyleSetting style_setting_default;

	[SerializeField]
	private ColorStyleSetting style_setting_minion;

	[SerializeField]
	private GameObject scrollerPrefab;

	[SerializeField]
	private Scrollbar scrollbar;

	public ScrollRect scroll_rect;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		if (selectMinionButton != null)
		{
			selectMinionButton.onClick += SelectMinion;
			selectMinionButton.onDoubleClick += SelectAndFocusMinion;
		}
	}

	public GameObject GetScroller(string scrollerID)
	{
		return scrollers[scrollerID];
	}

	public GameObject GetScrollerBorder(string scrolledID)
	{
		return scrollerBorders[scrolledID];
	}

	public void SelectMinion()
	{
		MinionIdentity minionIdentity = minion as MinionIdentity;
		if (!(minionIdentity == null))
		{
			SelectTool.Instance.Select(minionIdentity.GetComponent<KSelectable>());
		}
	}

	public void SelectAndFocusMinion()
	{
		MinionIdentity minionIdentity = minion as MinionIdentity;
		if (!(minionIdentity == null))
		{
			SelectTool.Instance.SelectAndFocus(minionIdentity.transform.GetPosition(), minionIdentity.GetComponent<KSelectable>(), new Vector3(8f, 0f, 0f));
		}
	}

	public void ConfigureAsWorldDivider(Dictionary<string, TableColumn> columns, TableScreen screen)
	{
		HierarchyReferences component = GetComponent<HierarchyReferences>();
		scroll_rect = component.GetReference<ScrollRect>("ScrollerScrollRect");
		rowType = RowType.WorldDivider;
		foreach (KeyValuePair<string, TableColumn> column in columns)
		{
			if (column.Value.scrollerID != "")
			{
				_ = column.Value;
				break;
			}
		}
		scroll_rect.onValueChanged.AddListener(delegate
		{
			if (!screen.CheckScrollersDirty())
			{
				screen.SetScrollersDirty(scroll_rect.horizontalNormalizedPosition);
			}
		});
	}

	public void ConfigureContent(IAssignableIdentity minion, Dictionary<string, TableColumn> columns, TableScreen screen)
	{
		this.minion = minion;
		KImage componentInChildren = GetComponentInChildren<KImage>(includeInactive: true);
		componentInChildren.colorStyleSetting = ((minion == null) ? style_setting_default : style_setting_minion);
		componentInChildren.ColorState = KImage.ColorSelector.Inactive;
		CanvasGroup component = GetComponent<CanvasGroup>();
		if (component != null && minion as StoredMinionIdentity != null)
		{
			component.alpha = 0.6f;
		}
		foreach (KeyValuePair<string, TableColumn> column in columns)
		{
			GameObject parent = base.gameObject;
			if (column.Value.scrollerID != "")
			{
				foreach (string column_scroller in column.Value.screen.column_scrollers)
				{
					if (column_scroller != column.Value.scrollerID)
					{
						continue;
					}
					if (!scrollers.ContainsKey(column_scroller))
					{
						GameObject gameObject = Util.KInstantiateUI(scrollerPrefab, base.gameObject, force_active: true);
						scroll_rect = gameObject.GetComponent<ScrollRect>();
						scrollbar = gameObject.GetComponentInChildren<Scrollbar>();
						scroll_rect.horizontalScrollbar = scrollbar;
						scroll_rect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
						scroll_rect.onValueChanged.AddListener(delegate
						{
							if (!screen.CheckScrollersDirty())
							{
								screen.SetScrollersDirty(scroll_rect.horizontalNormalizedPosition);
							}
						});
						scrollers.Add(column_scroller, scroll_rect.content.gameObject);
						if (scroll_rect.content.transform.parent.Find("Border") != null)
						{
							scrollerBorders.Add(column_scroller, scroll_rect.content.transform.parent.Find("Border").gameObject);
						}
					}
					parent = scrollers[column_scroller];
				}
			}
			GameObject gameObject2 = null;
			gameObject2 = ((minion != null) ? column.Value.GetMinionWidget(parent) : ((!isDefault) ? column.Value.GetHeaderWidget(parent) : column.Value.GetDefaultWidget(parent)));
			widgets.Add(column.Value, gameObject2);
			column.Value.widgets_by_row.Add(this, gameObject2);
		}
		RefreshColumns(columns);
		if (minion != null)
		{
			base.gameObject.name = minion.GetProperName();
		}
		else if (isDefault)
		{
			base.gameObject.name = "defaultRow";
		}
		if ((bool)selectMinionButton)
		{
			selectMinionButton.transform.SetAsLastSibling();
		}
	}

	public void PositionScrollerBorders()
	{
		foreach (KeyValuePair<string, GameObject> scrollerBorder in scrollerBorders)
		{
			RectTransform rectTransform = scrollerBorder.Value.rectTransform();
			float width = rectTransform.rect.width;
			scrollerBorder.Value.transform.SetParent(base.gameObject.transform);
			Vector2 anchorMin = (rectTransform.anchorMax = new Vector2(0f, 1f));
			rectTransform.anchorMin = anchorMin;
			rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
			RectTransform rectTransform2 = scrollers[scrollerBorder.Key].transform.parent.rectTransform();
			Vector3 vector2 = scrollers[scrollerBorder.Key].transform.parent.rectTransform().GetLocalPosition() - new Vector3(rectTransform2.sizeDelta.x / 2f, -1f * (rectTransform2.sizeDelta.y / 2f), 0f);
			vector2.y = 0f;
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 374f);
			rectTransform.SetLocalPosition(vector2 + Vector3.up * rectTransform.GetLocalPosition().y + Vector3.up * (0f - rectTransform.anchoredPosition.y));
		}
	}

	public void RefreshColumns(Dictionary<string, TableColumn> columns)
	{
		foreach (KeyValuePair<string, TableColumn> column in columns)
		{
			if (column.Value.on_load_action != null)
			{
				column.Value.on_load_action(minion, column.Value.widgets_by_row[this]);
			}
		}
	}

	public void RefreshScrollers()
	{
		foreach (KeyValuePair<string, GameObject> scroller in scrollers)
		{
			ScrollRect component = scroller.Value.transform.parent.GetComponent<ScrollRect>();
			component.GetComponent<LayoutElement>().minWidth = Mathf.Min(768f, component.content.sizeDelta.x);
		}
		foreach (KeyValuePair<string, GameObject> scrollerBorder in scrollerBorders)
		{
			RectTransform rectTransform = scrollerBorder.Value.rectTransform();
			rectTransform.sizeDelta = new Vector2(scrollers[scrollerBorder.Key].transform.parent.GetComponent<LayoutElement>().minWidth, rectTransform.sizeDelta.y);
		}
	}

	public GameObject GetWidget(TableColumn column)
	{
		if (widgets.ContainsKey(column) && widgets[column] != null)
		{
			return widgets[column];
		}
		Debug.LogWarning("Widget is null or row does not contain widget for column " + column);
		return null;
	}

	public IAssignableIdentity GetIdentity()
	{
		return minion;
	}

	public bool ContainsWidget(GameObject widget)
	{
		return widgets.ContainsValue(widget);
	}

	public void Clear()
	{
		foreach (KeyValuePair<TableColumn, GameObject> widget in widgets)
		{
			widget.Key.widgets_by_row.Remove(this);
		}
		Object.Destroy(base.gameObject);
	}
}
