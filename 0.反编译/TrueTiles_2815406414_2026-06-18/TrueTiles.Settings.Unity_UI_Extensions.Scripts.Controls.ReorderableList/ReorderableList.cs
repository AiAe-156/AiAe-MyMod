using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TrueTiles.Settings.Unity_UI_Extensions.Scripts.Controls.ReorderableList;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
[AddComponentMenu("UI/Extensions/Re-orderable list")]
public class ReorderableList : MonoBehaviour
{
	[Serializable]
	public struct ReorderableListEventStruct
	{
		public GameObject DroppedObject;

		public int FromIndex;

		public ReorderableList FromList;

		public bool IsAClone;

		public GameObject SourceObject;

		public int ToIndex;

		public ReorderableList ToList;

		public void Cancel()
		{
			SourceObject.GetComponent<ReorderableListElement>().isValid = false;
		}
	}

	[Serializable]
	public class ReorderableListHandler : UnityEvent<ReorderableListEventStruct>
	{
	}

	[Tooltip("Child container with re-orderable items in a layout group")]
	public LayoutGroup ContentLayout;

	[Tooltip("Parent area to draw the dragged element on top of containers. Defaults to the root Canvas")]
	public RectTransform DraggableArea;

	[Tooltip("Can items be dragged from the container?")]
	public bool IsDraggable = true;

	[Tooltip("Should the draggable components be removed or cloned?")]
	public bool CloneDraggedObject;

	[Tooltip("Can new draggable items be dropped in to the container?")]
	public bool IsDropable = true;

	[Tooltip("Should dropped items displace a current item if the list is full?\n Depending on the dropped items origin list, the displaced item may be added, dropped in space or deleted.")]
	public bool IsDisplacable;

	[Tooltip("Should items being dragged over this list have their sizes equalized?")]
	public bool EqualizeSizesOnDrag;

	public int maxItems = int.MaxValue;

	[Header("UI Re-orderable Events")]
	public ReorderableListHandler OnElementDropped = new ReorderableListHandler();

	public ReorderableListHandler OnElementGrabbed = new ReorderableListHandler();

	public ReorderableListHandler OnElementRemoved = new ReorderableListHandler();

	public ReorderableListHandler OnElementAdded = new ReorderableListHandler();

	public ReorderableListHandler OnElementDisplacedFrom = new ReorderableListHandler();

	public ReorderableListHandler OnElementDisplacedTo = new ReorderableListHandler();

	public ReorderableListHandler OnElementDisplacedFromReturned = new ReorderableListHandler();

	public ReorderableListHandler OnElementDisplacedToReturned = new ReorderableListHandler();

	public ReorderableListHandler OnElementDroppedWithMaxItems = new ReorderableListHandler();

	private RectTransform _content;

	private ReorderableListContent _listContent;

	public RectTransform Content
	{
		get
		{
			if ((Object)(object)_content == (Object)null)
			{
				_content = ((Component)ContentLayout).GetComponent<RectTransform>();
			}
			return _content;
		}
	}

	private Canvas GetCanvas()
	{
		Transform val = ((Component)this).transform;
		Canvas val2 = null;
		int num = 100;
		int num2 = 0;
		while ((Object)(object)val2 == (Object)null && num2 < num)
		{
			val2 = ((Component)val).gameObject.GetComponent<Canvas>();
			if ((Object)(object)val2 == (Object)null)
			{
				val = val.parent;
			}
			num2++;
		}
		return val2;
	}

	public void Refresh()
	{
		_listContent = EntityTemplateExtensions.AddOrGet<ReorderableListContent>(((Component)ContentLayout).gameObject);
		_listContent.Init(this);
	}

	private void Start()
	{
		if ((Object)(object)ContentLayout == (Object)null)
		{
			Debug.LogError((object)("You need to have a child LayoutGroup content set for the list: " + ((Object)this).name), (Object)(object)((Component)this).gameObject);
			return;
		}
		if ((Object)(object)DraggableArea == (Object)null)
		{
			DraggableArea = ((Component)((Component)((Component)this).transform.root).GetComponentInChildren<Canvas>()).GetComponent<RectTransform>();
		}
		if (IsDropable && !Object.op_Implicit((Object)(object)((Component)this).GetComponent<Graphic>()))
		{
			Debug.LogError((object)("You need to have a Graphic control (such as an Image) for the list [" + ((Object)this).name + "] to be droppable"), (Object)(object)((Component)this).gameObject);
		}
		else
		{
			Refresh();
		}
	}

	public void TestReOrderableListTarget(ReorderableListEventStruct item)
	{
		Debug.Log((object)"Event Received");
		Debug.Log((object)("Hello World, is my item a clone? [" + item.IsAClone + "]"));
	}
}
