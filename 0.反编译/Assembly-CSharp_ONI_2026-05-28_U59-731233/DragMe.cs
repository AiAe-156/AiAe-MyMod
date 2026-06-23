using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragMe : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	public interface IDragListener
	{
		void OnBeginDrag(Vector2 position);

		void OnEndDrag(Vector2 position);
	}

	public bool dragOnSurfaces = true;

	private GameObject m_DraggingIcon;

	private RectTransform m_DraggingPlane;

	private float x;

	public IDragListener listener;

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
		{
			return;
		}
		Canvas canvas = FindInParents<Canvas>(base.gameObject);
		if (!(canvas == null))
		{
			m_DraggingIcon = Object.Instantiate(base.gameObject, canvas.transform, worldPositionStays: false);
			GraphicRaycaster component = m_DraggingIcon.GetComponent<GraphicRaycaster>();
			if (component != null)
			{
				component.enabled = false;
			}
			m_DraggingIcon.name = "dragObj";
			m_DraggingIcon.transform.SetAsLastSibling();
			RectTransform component2 = m_DraggingIcon.GetComponent<RectTransform>();
			component2.pivot = Vector2.zero;
			component2.sizeDelta = GetComponent<RectTransform>().rect.size;
			x = m_DraggingIcon.transform.position.x;
			Canvas component3 = m_DraggingIcon.GetComponent<Canvas>();
			component3.overrideSorting = true;
			component3.sortingOrder = 99;
			if (dragOnSurfaces)
			{
				m_DraggingPlane = base.transform as RectTransform;
			}
			else
			{
				m_DraggingPlane = canvas.transform as RectTransform;
			}
			SetDraggedPosition(eventData);
			listener.OnBeginDrag(eventData.position);
		}
	}

	public void OnDrag(PointerEventData data)
	{
		if (m_DraggingIcon != null)
		{
			SetDraggedPosition(data);
		}
	}

	private void SetDraggedPosition(PointerEventData data)
	{
		if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
		{
			m_DraggingPlane = data.pointerEnter.transform as RectTransform;
		}
		RectTransform component = m_DraggingIcon.GetComponent<RectTransform>();
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out var worldPoint))
		{
			worldPoint.x = x + 5f;
			worldPoint.y -= component.sizeDelta.y / 2f;
			component.position = worldPoint;
			component.rotation = m_DraggingPlane.rotation;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		listener.OnEndDrag(eventData.position);
		if (m_DraggingIcon != null)
		{
			Object.Destroy(m_DraggingIcon);
		}
	}

	public static T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null)
		{
			return null;
		}
		T val = null;
		Transform parent = go.transform.parent;
		while (parent != null && val == null)
		{
			val = parent.gameObject.GetComponent<T>();
			parent = parent.parent;
		}
		return val;
	}
}
