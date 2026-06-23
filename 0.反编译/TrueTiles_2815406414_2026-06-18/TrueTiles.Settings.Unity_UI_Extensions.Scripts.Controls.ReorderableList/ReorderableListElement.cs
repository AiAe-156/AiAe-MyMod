using System;
using System.Collections.Generic;
using FUtility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TrueTiles.Settings.Unity_UI_Extensions.Scripts.Controls.ReorderableList;

[RequireComponent(typeof(RectTransform), typeof(LayoutElement))]
public class ReorderableListElement : MonoBehaviour, IDragHandler, IEventSystemHandler, IBeginDragHandler, IEndDragHandler
{
	[Tooltip("Can this element be dragged?")]
	[SerializeField]
	public bool IsGrabbable = true;

	[Tooltip("Can this element be transfered to another list")]
	[SerializeField]
	public bool _isTransferable = true;

	[Tooltip("Can this element be dropped in space?")]
	[SerializeField]
	public bool isDroppableInSpace;

	private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

	private ReorderableList _currentReorderableListRaycasted;

	private int _fromIndex;

	private RectTransform _draggingObject;

	private LayoutElement _draggingObjectLE;

	private Vector2 _draggingObjectOriginalSize;

	private RectTransform _fakeElement;

	private LayoutElement _fakeElementLE;

	private int _displacedFromIndex;

	private RectTransform _displacedObject;

	private LayoutElement _displacedObjectLE;

	private Vector2 _displacedObjectOriginalSize;

	private ReorderableList _displacedObjectOriginList;

	private bool _isDragging;

	private RectTransform _rect;

	private ReorderableList _reorderableList;

	private CanvasGroup _canvasGroup;

	internal bool isValid;

	public bool IsTransferable
	{
		get
		{
			return _isTransferable;
		}
		set
		{
			_canvasGroup = EntityTemplateExtensions.AddOrGet<CanvasGroup>(((Component)this).gameObject);
			_canvasGroup.blocksRaycasts = value;
			_isTransferable = value;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)_canvasGroup))
		{
			_canvasGroup = EntityTemplateExtensions.AddOrGet<CanvasGroup>(((Component)this).gameObject);
		}
		_canvasGroup.blocksRaycasts = false;
		isValid = true;
		if ((Object)(object)_reorderableList == (Object)null)
		{
			return;
		}
		if (!_reorderableList.IsDraggable || !IsGrabbable)
		{
			_draggingObject = null;
			return;
		}
		if (!_reorderableList.CloneDraggedObject)
		{
			_draggingObject = _rect;
			_fromIndex = ((Transform)_rect).GetSiblingIndex();
			_displacedFromIndex = -1;
			if (_reorderableList.OnElementRemoved != null)
			{
				((UnityEvent<ReorderableList.ReorderableListEventStruct>)_reorderableList.OnElementRemoved).Invoke(new ReorderableList.ReorderableListEventStruct
				{
					DroppedObject = ((Component)_draggingObject).gameObject,
					IsAClone = _reorderableList.CloneDraggedObject,
					SourceObject = (_reorderableList.CloneDraggedObject ? ((Component)this).gameObject : ((Component)_draggingObject).gameObject),
					FromList = _reorderableList,
					FromIndex = _fromIndex
				});
			}
			if (!isValid)
			{
				_draggingObject = null;
				return;
			}
		}
		else
		{
			GameObject val = Object.Instantiate<GameObject>(((Component)this).gameObject);
			_draggingObject = val.GetComponent<RectTransform>();
		}
		Rect rect = ((Component)this).gameObject.GetComponent<RectTransform>().rect;
		_draggingObjectOriginalSize = ((Rect)(ref rect)).size;
		_draggingObjectLE = ((Component)_draggingObject).GetComponent<LayoutElement>();
		((Transform)_draggingObject).SetParent((Transform)(object)_reorderableList.DraggableArea, true);
		((Transform)_draggingObject).SetAsLastSibling();
		_reorderableList.Refresh();
		_fakeElement = new GameObject("Fake").AddComponent<RectTransform>();
		_fakeElementLE = ((Component)_fakeElement).gameObject.AddComponent<LayoutElement>();
		RefreshSizes();
		if (_reorderableList.OnElementGrabbed != null)
		{
			((UnityEvent<ReorderableList.ReorderableListEventStruct>)_reorderableList.OnElementGrabbed).Invoke(new ReorderableList.ReorderableListEventStruct
			{
				DroppedObject = ((Component)_draggingObject).gameObject,
				IsAClone = _reorderableList.CloneDraggedObject,
				SourceObject = (_reorderableList.CloneDraggedObject ? ((Component)this).gameObject : ((Component)_draggingObject).gameObject),
				FromList = _reorderableList,
				FromIndex = _fromIndex
			});
			if (!isValid)
			{
				CancelDrag();
				return;
			}
		}
		_isDragging = true;
	}

	public void OnDrag(PointerEventData eventData)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		if (!_isDragging)
		{
			return;
		}
		if (!isValid)
		{
			CancelDrag();
			return;
		}
		Canvas componentInParent = ((Component)_draggingObject).GetComponentInParent<Canvas>();
		Vector3 val = default(Vector3);
		RectTransformUtility.ScreenPointToWorldPointInRectangle(((Component)componentInParent).GetComponent<RectTransform>(), eventData.position, ((int)componentInParent.renderMode != 0) ? componentInParent.worldCamera : null, ref val);
		((Transform)_draggingObject).position = val;
		ReorderableList currentReorderableListRaycasted = _currentReorderableListRaycasted;
		Log.Assert("CURRENT", EventSystem.current);
		EventSystem.current.RaycastAll(eventData, _raycastResults);
		for (int i = 0; i < _raycastResults.Count; i++)
		{
			RaycastResult val2 = _raycastResults[i];
			_currentReorderableListRaycasted = ((RaycastResult)(ref val2)).gameObject.GetComponent<ReorderableList>();
			if ((Object)(object)_currentReorderableListRaycasted != (Object)null)
			{
				break;
			}
		}
		if ((Object)(object)_currentReorderableListRaycasted == (Object)null || !_currentReorderableListRaycasted.IsDropable || ((((Object)(object)((Transform)_fakeElement).parent == (Object)(object)_currentReorderableListRaycasted.Content) ? (((Transform)_currentReorderableListRaycasted.Content).childCount - 1) : ((Transform)_currentReorderableListRaycasted.Content).childCount) >= _currentReorderableListRaycasted.maxItems && !_currentReorderableListRaycasted.IsDisplacable) || _currentReorderableListRaycasted.maxItems <= 0)
		{
			RefreshSizes();
			((Component)_fakeElement).transform.SetParent((Transform)(object)_reorderableList.DraggableArea, false);
			if ((Object)(object)_displacedObject != (Object)null)
			{
				revertDisplacedElement();
			}
			return;
		}
		if (((Transform)_currentReorderableListRaycasted.Content).childCount < _currentReorderableListRaycasted.maxItems && (Object)(object)((Transform)_fakeElement).parent != (Object)(object)_currentReorderableListRaycasted.Content)
		{
			((Transform)_fakeElement).SetParent((Transform)(object)_currentReorderableListRaycasted.Content, false);
		}
		float num = float.PositiveInfinity;
		int num2 = 0;
		float num3 = 0f;
		for (int j = 0; j < ((Transform)_currentReorderableListRaycasted.Content).childCount; j++)
		{
			RectTransform component = ((Component)((Transform)_currentReorderableListRaycasted.Content).GetChild(j)).GetComponent<RectTransform>();
			if (_currentReorderableListRaycasted.ContentLayout is VerticalLayoutGroup)
			{
				num3 = Mathf.Abs(((Transform)component).position.y - val.y);
			}
			else if (_currentReorderableListRaycasted.ContentLayout is HorizontalLayoutGroup)
			{
				num3 = Mathf.Abs(((Transform)component).position.x - val.x);
			}
			else if (_currentReorderableListRaycasted.ContentLayout is GridLayoutGroup)
			{
				num3 = Mathf.Abs(((Transform)component).position.x - val.x) + Mathf.Abs(((Transform)component).position.y - val.y);
			}
			if (num3 < num)
			{
				num = num3;
				num2 = j;
			}
		}
		if (((Object)(object)_currentReorderableListRaycasted != (Object)(object)currentReorderableListRaycasted || num2 != _displacedFromIndex) && ((Transform)_currentReorderableListRaycasted.Content).childCount == _currentReorderableListRaycasted.maxItems)
		{
			Transform child = ((Transform)_currentReorderableListRaycasted.Content).GetChild(num2);
			if ((Object)(object)_displacedObject != (Object)null)
			{
				revertDisplacedElement();
				if (((Transform)_currentReorderableListRaycasted.Content).childCount > _currentReorderableListRaycasted.maxItems)
				{
					displaceElement(num2, child);
				}
			}
			else if ((Object)(object)((Transform)_fakeElement).parent != (Object)(object)_currentReorderableListRaycasted.Content)
			{
				((Transform)_fakeElement).SetParent((Transform)(object)_currentReorderableListRaycasted.Content, false);
				displaceElement(num2, child);
			}
		}
		RefreshSizes();
		((Transform)_fakeElement).SetSiblingIndex(num2);
		((Component)_fakeElement).gameObject.SetActive(true);
	}

	private void displaceElement(int targetIndex, Transform displaced)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		_displacedFromIndex = targetIndex;
		_displacedObjectOriginList = _currentReorderableListRaycasted;
		_displacedObject = ((Component)displaced).GetComponent<RectTransform>();
		_displacedObjectLE = ((Component)_displacedObject).GetComponent<LayoutElement>();
		Rect rect = _displacedObject.rect;
		_displacedObjectOriginalSize = ((Rect)(ref rect)).size;
		ReorderableList.ReorderableListEventStruct reorderableListEventStruct = new ReorderableList.ReorderableListEventStruct
		{
			DroppedObject = ((Component)_displacedObject).gameObject,
			FromList = _currentReorderableListRaycasted,
			FromIndex = targetIndex
		};
		int num = (((Object)(object)((Transform)_fakeElement).parent == (Object)(object)_reorderableList.Content) ? (((Transform)_reorderableList.Content).childCount - 1) : ((Transform)_reorderableList.Content).childCount);
		if (_reorderableList.IsDropable && num < _reorderableList.maxItems && ((Component)_displacedObject).GetComponent<ReorderableListElement>().IsTransferable)
		{
			_displacedObjectLE.preferredWidth = _draggingObjectOriginalSize.x;
			_displacedObjectLE.preferredHeight = _draggingObjectOriginalSize.y;
			((Transform)_displacedObject).SetParent((Transform)(object)_reorderableList.Content, false);
			((Transform)_displacedObject).rotation = ((Component)_reorderableList).transform.rotation;
			((Transform)_displacedObject).SetSiblingIndex(_fromIndex);
			_reorderableList.Refresh();
			_currentReorderableListRaycasted.Refresh();
			reorderableListEventStruct.ToList = _reorderableList;
			reorderableListEventStruct.ToIndex = _fromIndex;
			((UnityEvent<ReorderableList.ReorderableListEventStruct>)_reorderableList.OnElementDisplacedTo).Invoke(reorderableListEventStruct);
			((UnityEvent<ReorderableList.ReorderableListEventStruct>)_reorderableList.OnElementAdded).Invoke(reorderableListEventStruct);
		}
		else if (((Component)_displacedObject).GetComponent<ReorderableListElement>().isDroppableInSpace)
		{
			((Transform)_displacedObject).SetParent((Transform)(object)_currentReorderableListRaycasted.DraggableArea, true);
			_currentReorderableListRaycasted.Refresh();
			RectTransform displacedObject = _displacedObject;
			((Transform)displacedObject).position = ((Transform)displacedObject).position + new Vector3(_draggingObjectOriginalSize.x / 2f, _draggingObjectOriginalSize.y / 2f, 0f);
		}
		else
		{
			((Transform)_displacedObject).SetParent((Transform)null, true);
			_displacedObjectOriginList.Refresh();
			((Component)_displacedObject).gameObject.SetActive(false);
		}
		((UnityEvent<ReorderableList.ReorderableListEventStruct>)_displacedObjectOriginList.OnElementDisplacedFrom).Invoke(reorderableListEventStruct);
		((UnityEvent<ReorderableList.ReorderableListEventStruct>)_reorderableList.OnElementRemoved).Invoke(reorderableListEventStruct);
	}

	private void revertDisplacedElement()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		ReorderableList.ReorderableListEventStruct reorderableListEventStruct = new ReorderableList.ReorderableListEventStruct
		{
			DroppedObject = ((Component)_displacedObject).gameObject,
			FromList = _displacedObjectOriginList,
			FromIndex = _displacedFromIndex
		};
		if ((Object)(object)((Transform)_displacedObject).parent != (Object)null)
		{
			reorderableListEventStruct.ToList = _reorderableList;
			reorderableListEventStruct.ToIndex = _fromIndex;
		}
		_displacedObjectLE.preferredWidth = _displacedObjectOriginalSize.x;
		_displacedObjectLE.preferredHeight = _displacedObjectOriginalSize.y;
		((Transform)_displacedObject).SetParent((Transform)(object)_displacedObjectOriginList.Content, false);
		((Transform)_displacedObject).rotation = ((Component)_displacedObjectOriginList).transform.rotation;
		((Transform)_displacedObject).SetSiblingIndex(_displacedFromIndex);
		((Component)_displacedObject).gameObject.SetActive(true);
		_reorderableList.Refresh();
		_displacedObjectOriginList.Refresh();
		if ((Object)(object)reorderableListEventStruct.ToList != (Object)null)
		{
			((UnityEvent<ReorderableList.ReorderableListEventStruct>)_reorderableList.OnElementDisplacedToReturned).Invoke(reorderableListEventStruct);
			((UnityEvent<ReorderableList.ReorderableListEventStruct>)_reorderableList.OnElementRemoved).Invoke(reorderableListEventStruct);
		}
		((UnityEvent<ReorderableList.ReorderableListEventStruct>)_displacedObjectOriginList.OnElementDisplacedFromReturned).Invoke(reorderableListEventStruct);
		((UnityEvent<ReorderableList.ReorderableListEventStruct>)_displacedObjectOriginList.OnElementAdded).Invoke(reorderableListEventStruct);
		_displacedFromIndex = -1;
		_displacedObjectOriginList = null;
		_displacedObject = null;
		_displacedObjectLE = null;
	}

	public void finishDisplacingElement()
	{
		if ((Object)(object)((Transform)_displacedObject).parent == (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_displacedObject).gameObject);
		}
		_displacedFromIndex = -1;
		_displacedObjectOriginList = null;
		_displacedObject = null;
		_displacedObjectLE = null;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		_isDragging = false;
		if ((Object)(object)_draggingObject != (Object)null)
		{
			if ((Object)(object)_currentReorderableListRaycasted != (Object)null && (Object)(object)((Transform)_fakeElement).parent == (Object)(object)_currentReorderableListRaycasted.Content)
			{
				ReorderableList.ReorderableListEventStruct reorderableListEventStruct = new ReorderableList.ReorderableListEventStruct
				{
					DroppedObject = ((Component)_draggingObject).gameObject,
					IsAClone = _reorderableList.CloneDraggedObject,
					SourceObject = (_reorderableList.CloneDraggedObject ? ((Component)this).gameObject : ((Component)_draggingObject).gameObject),
					FromList = _reorderableList,
					FromIndex = _fromIndex,
					ToList = _currentReorderableListRaycasted,
					ToIndex = ((Transform)_fakeElement).GetSiblingIndex()
				};
				if (Object.op_Implicit((Object)(object)_reorderableList) && _reorderableList.OnElementDropped != null)
				{
					((UnityEvent<ReorderableList.ReorderableListEventStruct>)_reorderableList.OnElementDropped).Invoke(reorderableListEventStruct);
				}
				if (!isValid)
				{
					CancelDrag();
					return;
				}
				RefreshSizes();
				((Transform)_draggingObject).SetParent((Transform)(object)_currentReorderableListRaycasted.Content, false);
				((Transform)_draggingObject).rotation = ((Component)_currentReorderableListRaycasted).transform.rotation;
				((Transform)_draggingObject).SetSiblingIndex(((Transform)_fakeElement).GetSiblingIndex());
				if (IsTransferable)
				{
					((Component)_draggingObject).GetComponent<CanvasGroup>().blocksRaycasts = true;
				}
				_reorderableList.Refresh();
				_currentReorderableListRaycasted.Refresh();
				((UnityEvent<ReorderableList.ReorderableListEventStruct>)_reorderableList.OnElementAdded).Invoke(reorderableListEventStruct);
				if ((Object)(object)_displacedObject != (Object)null)
				{
					finishDisplacingElement();
				}
				if (!isValid)
				{
					throw new Exception("It's too late to cancel the Transfer! Do so in OnElementDropped!");
				}
			}
			else
			{
				if (isDroppableInSpace)
				{
					((UnityEvent<ReorderableList.ReorderableListEventStruct>)_reorderableList.OnElementDropped).Invoke(new ReorderableList.ReorderableListEventStruct
					{
						DroppedObject = ((Component)_draggingObject).gameObject,
						IsAClone = _reorderableList.CloneDraggedObject,
						SourceObject = (_reorderableList.CloneDraggedObject ? ((Component)this).gameObject : ((Component)_draggingObject).gameObject),
						FromList = _reorderableList,
						FromIndex = _fromIndex
					});
				}
				else
				{
					CancelDrag();
				}
				if ((Object)(object)_currentReorderableListRaycasted != (Object)null && ((((Transform)_currentReorderableListRaycasted.Content).childCount >= _currentReorderableListRaycasted.maxItems && !_currentReorderableListRaycasted.IsDisplacable) || _currentReorderableListRaycasted.maxItems <= 0))
				{
					GameObject gameObject = ((Component)_draggingObject).gameObject;
					((UnityEvent<ReorderableList.ReorderableListEventStruct>)_reorderableList.OnElementDroppedWithMaxItems).Invoke(new ReorderableList.ReorderableListEventStruct
					{
						DroppedObject = gameObject,
						IsAClone = _reorderableList.CloneDraggedObject,
						SourceObject = (_reorderableList.CloneDraggedObject ? ((Component)this).gameObject : gameObject),
						FromList = _reorderableList,
						ToList = _currentReorderableListRaycasted,
						FromIndex = _fromIndex
					});
				}
			}
		}
		if ((Object)(object)_fakeElement != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_fakeElement).gameObject);
			_fakeElement = null;
		}
		_canvasGroup.blocksRaycasts = true;
	}

	private void CancelDrag()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		_isDragging = false;
		if (_reorderableList.CloneDraggedObject)
		{
			Object.Destroy((Object)(object)((Component)_draggingObject).gameObject);
		}
		else
		{
			RefreshSizes();
			((Transform)_draggingObject).SetParent((Transform)(object)_reorderableList.Content, false);
			((Transform)_draggingObject).rotation = ((Component)_reorderableList.Content).transform.rotation;
			((Transform)_draggingObject).SetSiblingIndex(_fromIndex);
			ReorderableList.ReorderableListEventStruct reorderableListEventStruct = new ReorderableList.ReorderableListEventStruct
			{
				DroppedObject = ((Component)_draggingObject).gameObject,
				IsAClone = _reorderableList.CloneDraggedObject,
				SourceObject = (_reorderableList.CloneDraggedObject ? ((Component)this).gameObject : ((Component)_draggingObject).gameObject),
				FromList = _reorderableList,
				FromIndex = _fromIndex,
				ToList = _reorderableList,
				ToIndex = _fromIndex
			};
			_reorderableList.Refresh();
			((UnityEvent<ReorderableList.ReorderableListEventStruct>)_reorderableList.OnElementAdded).Invoke(reorderableListEventStruct);
			if (!isValid)
			{
				throw new Exception("Transfer is already Canceled.");
			}
		}
		if ((Object)(object)_fakeElement != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_fakeElement).gameObject);
			_fakeElement = null;
		}
		if ((Object)(object)_displacedObject != (Object)null)
		{
			revertDisplacedElement();
		}
		_canvasGroup.blocksRaycasts = true;
	}

	private void RefreshSizes()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = _draggingObjectOriginalSize;
		if ((Object)(object)_currentReorderableListRaycasted != (Object)null && _currentReorderableListRaycasted.IsDropable && ((Transform)_currentReorderableListRaycasted.Content).childCount > 0 && _currentReorderableListRaycasted.EqualizeSizesOnDrag)
		{
			Transform child = ((Transform)_currentReorderableListRaycasted.Content).GetChild(0);
			if ((Object)(object)child != (Object)null)
			{
				Rect rect = ((Component)child).GetComponent<RectTransform>().rect;
				val = ((Rect)(ref rect)).size;
			}
		}
		_draggingObject.sizeDelta = val;
		LayoutElement fakeElementLE = _fakeElementLE;
		float preferredHeight = (_draggingObjectLE.preferredHeight = val.y);
		fakeElementLE.preferredHeight = preferredHeight;
		LayoutElement fakeElementLE2 = _fakeElementLE;
		preferredHeight = (_draggingObjectLE.preferredWidth = val.x);
		fakeElementLE2.preferredWidth = preferredHeight;
		((Component)_fakeElement).GetComponent<RectTransform>().sizeDelta = val;
	}

	public void Init(ReorderableList reorderableList)
	{
		_reorderableList = reorderableList;
		_rect = ((Component)this).GetComponent<RectTransform>();
		_canvasGroup = EntityTemplateExtensions.AddOrGet<CanvasGroup>(((Component)this).gameObject);
	}
}
