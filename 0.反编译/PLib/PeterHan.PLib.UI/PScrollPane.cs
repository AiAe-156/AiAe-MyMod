using System;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI.Layouts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

/// <summary>
/// A factory for scrollable panes.
/// </summary>
public sealed class PScrollPane : IUIComponent
{
	/// <summary>
	/// Handles layout for scroll panes. Not freezable.
	/// </summary>
	private sealed class PScrollPaneLayout : AbstractLayoutGroup
	{
		/// <summary>
		/// Caches elements when calculating layout to improve performance.
		/// </summary>
		private Component[] calcElements;

		/// <summary>
		/// The calculated horizontal size of the child element.
		/// </summary>
		private LayoutSizes childHorizontal;

		/// <summary>
		/// The calculated vertical size of the child element.
		/// </summary>
		private LayoutSizes childVertical;

		/// <summary>
		/// The child object inside the scroll rect.
		/// </summary>
		private GameObject child;

		/// <summary>
		/// Caches elements when setting layout to improve performance.
		/// </summary>
		private ILayoutController[] setElements;

		/// <summary>
		/// The viewport which clips the child rectangle.
		/// </summary>
		private GameObject viewport;

		internal PScrollPaneLayout()
		{
			base.minHeight = (base.minWidth = 0f);
			child = (viewport = null);
		}

		public override void CalculateLayoutInputHorizontal()
		{
			if ((Object)(object)child == (Object)null)
			{
				UpdateComponents();
			}
			if ((Object)(object)child != (Object)null)
			{
				calcElements = child.GetComponents<Component>();
				childHorizontal = PUIUtils.CalcSizes(child, PanelDirection.Horizontal, calcElements);
				if (childHorizontal.ignore)
				{
					throw new InvalidOperationException("ScrollPane child ignores layout!");
				}
				base.preferredWidth = childHorizontal.preferred;
			}
		}

		public override void CalculateLayoutInputVertical()
		{
			if ((Object)(object)child == (Object)null)
			{
				UpdateComponents();
			}
			if ((Object)(object)child != (Object)null && calcElements != null)
			{
				childVertical = PUIUtils.CalcSizes(child, PanelDirection.Vertical, calcElements);
				base.preferredHeight = childVertical.preferred;
				calcElements = null;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			UpdateComponents();
		}

		public override void SetLayoutHorizontal()
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)viewport != (Object)null && (Object)(object)child != (Object)null)
			{
				float num = childHorizontal.preferred;
				Rect rect = Util.rectTransform(viewport).rect;
				float width = ((Rect)(ref rect)).width;
				if (num < width && childHorizontal.flexible > 0f)
				{
					num = width;
				}
				setElements = child.GetComponents<ILayoutController>();
				Util.rectTransform(child).SetSizeWithCurrentAnchors((Axis)0, num);
				ILayoutController[] array = setElements;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetLayoutHorizontal();
				}
			}
		}

		public override void SetLayoutVertical()
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)viewport != (Object)null && (Object)(object)child != (Object)null && setElements != null)
			{
				float num = childVertical.preferred;
				Rect rect = Util.rectTransform(viewport).rect;
				float height = ((Rect)(ref rect)).height;
				if (num < height && childVertical.flexible > 0f)
				{
					num = height;
				}
				Util.rectTransform(child).SetSizeWithCurrentAnchors((Axis)1, num);
				ILayoutController[] array = setElements;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetLayoutVertical();
				}
				setElements = null;
			}
		}

		/// <summary>
		/// Caches the child component for performance reasons at runtime.
		/// </summary>
		private void UpdateComponents()
		{
			GameObject gameObject = ((Component)this).gameObject;
			ScrollRect val = default(ScrollRect);
			if ((Object)(object)gameObject != (Object)null && gameObject.TryGetComponent<ScrollRect>(ref val))
			{
				RectTransform content = val.content;
				child = ((content != null) ? ((Component)content).gameObject : null);
				RectTransform obj = val.viewport;
				viewport = ((obj != null) ? ((Component)obj).gameObject : null);
			}
			else
			{
				child = (viewport = null);
			}
		}
	}

	/// <summary>
	/// A layout group object that does nothing. While it seems completely pointless,
	/// it allows LayoutRebuilder to pass by the viewport on Scroll Rects on its way up
	/// the tree, thus ensuring that the scroll rect gets rebuilt.
	///
	/// On the way back down, this component gets skipped over by PScrollPaneLayout to
	/// save on processing, and the child layout is built directly.
	/// </summary>
	private sealed class ViewportLayoutGroup : UIBehaviour, ILayoutGroup, ILayoutController
	{
		public void SetLayoutHorizontal()
		{
		}

		public void SetLayoutVertical()
		{
		}
	}

	/// <summary>
	/// The track size of scrollbars is based on the sprite.
	/// </summary>
	private const float DEFAULT_TRACK_SIZE = 16f;

	/// <summary>
	/// Whether the horizontal scrollbar is always visible.
	/// </summary>
	public bool AlwaysShowHorizontal { get; set; }

	/// <summary>
	/// Whether the vertical scrollbar is always visible.
	/// </summary>
	public bool AlwaysShowVertical { get; set; }

	/// <summary>
	/// The background color of this scroll pane.
	/// </summary>
	public Color BackColor { get; set; }

	/// <summary>
	/// The child of this scroll pane.
	/// </summary>
	public IUIComponent Child { get; set; }

	/// <summary>
	/// The flexible size bounds of this component.
	/// </summary>
	public Vector2 FlexSize { get; set; }

	public string Name { get; }

	/// <summary>
	/// Whether horizontal scrolling is allowed.
	/// </summary>
	public bool ScrollHorizontal { get; set; }

	/// <summary>
	/// Whether vertical scrolling is allowed.
	/// </summary>
	public bool ScrollVertical { get; set; }

	/// <summary>
	/// The size of the scrollbar track.
	/// </summary>
	public float TrackSize { get; set; }

	public event PUIDelegates.OnRealize OnRealize;

	public PScrollPane()
		: this(null)
	{
	}

	public PScrollPane(string name)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		AlwaysShowHorizontal = (AlwaysShowVertical = true);
		BackColor = PUITuning.Colors.Transparent;
		Child = null;
		FlexSize = Vector2.zero;
		Name = name ?? "Scroll";
		ScrollHorizontal = false;
		ScrollVertical = false;
		TrackSize = 16f;
	}

	/// <summary>
	/// Adds a handler when this scroll pane is realized.
	/// </summary>
	/// <param name="onRealize">The handler to invoke on realization.</param>
	/// <returns>This scroll pane for call chaining.</returns>
	public PScrollPane AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		OnRealize += onRealize;
		return this;
	}

	public GameObject Build()
	{
		if (Child == null)
		{
			throw new InvalidOperationException("No child component");
		}
		GameObject val = BuildScrollPane(null, Child.Build());
		this.OnRealize?.Invoke(val);
		return val;
	}

	/// <summary>
	/// Builds the actual scroll pane object.
	/// </summary>
	/// <param name="parent">The parent of this scroll pane.</param>
	/// <param name="child">The child element of this scroll pane.</param>
	/// <returns>The realized scroll pane.</returns>
	internal GameObject BuildScrollPane(GameObject parent, GameObject child)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PUIElements.CreateUI(parent, Name);
		if (BackColor.a > 0f)
		{
			((Graphic)val.AddComponent<Image>()).color = BackColor;
		}
		val.SetActive(false);
		KScrollRect val2 = val.AddComponent<KScrollRect>();
		((ScrollRect)val2).horizontal = ScrollHorizontal;
		((ScrollRect)val2).vertical = ScrollVertical;
		GameObject val3 = PUIElements.CreateUI(val, "Viewport");
		Util.rectTransform(val3).pivot = Vector2.up;
		((Behaviour)val3.AddComponent<RectMask2D>()).enabled = true;
		val3.AddComponent<ViewportLayoutGroup>();
		((ScrollRect)val2).viewport = Util.rectTransform(val3);
		EntityTemplateExtensions.AddOrGet<Canvas>(child).pixelPerfect = false;
		EntityTemplateExtensions.AddOrGet<GraphicRaycaster>(child);
		PUIElements.SetAnchors(child.SetParent(val3), PUIAnchoring.Beginning, PUIAnchoring.End);
		((ScrollRect)val2).content = Util.rectTransform(child);
		if (ScrollVertical)
		{
			((ScrollRect)val2).verticalScrollbar = CreateScrollVert(val);
			((ScrollRect)val2).verticalScrollbarVisibility = (ScrollbarVisibility)((!AlwaysShowVertical) ? 2 : 0);
		}
		if (ScrollHorizontal)
		{
			((ScrollRect)val2).horizontalScrollbar = CreateScrollHoriz(val);
			((ScrollRect)val2).horizontalScrollbarVisibility = (ScrollbarVisibility)((!AlwaysShowHorizontal) ? 2 : 0);
		}
		val.SetActive(true);
		PScrollPaneLayout pScrollPaneLayout = val.AddComponent<PScrollPaneLayout>();
		pScrollPaneLayout.flexibleHeight = FlexSize.y;
		pScrollPaneLayout.flexibleWidth = FlexSize.x;
		return val;
	}

	/// <summary>
	/// Creates a horizontal scroll bar.
	/// </summary>
	/// <param name="parent">The parent component.</param>
	/// <returns>The scroll bar component.</returns>
	private Scrollbar CreateScrollHoriz(GameObject parent)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PUIElements.CreateUI(parent, "Scrollbar H", canvas: true, PUIAnchoring.Stretch, PUIAnchoring.Beginning);
		Image obj = val.AddComponent<Image>();
		obj.sprite = PUITuning.Images.ScrollBorderHorizontal;
		obj.type = (Type)1;
		Scrollbar obj2 = val.AddComponent<Scrollbar>();
		((Selectable)obj2).interactable = true;
		((Selectable)obj2).transition = (Transition)1;
		((Selectable)obj2).colors = PUITuning.Colors.ScrollbarColors;
		obj2.SetDirection((Direction)1, true);
		GameObject val2 = PUIElements.CreateUI(val, "Handle", canvas: true, PUIAnchoring.Stretch, PUIAnchoring.End);
		PUIElements.SetAnchorOffsets(val2, 1f, 1f, 1f, 1f);
		obj2.handleRect = Util.rectTransform(val2);
		Image val3 = val2.AddComponent<Image>();
		val3.sprite = PUITuning.Images.ScrollHandleHorizontal;
		val3.type = (Type)1;
		((Selectable)obj2).targetGraphic = (Graphic)(object)val3;
		val.SetActive(true);
		PUIElements.SetAnchorOffsets(val, 2f, 2f, 0f - TrackSize, 0f);
		return obj2;
	}

	/// <summary>
	/// Creates a vertical scroll bar.
	/// </summary>
	/// <param name="parent">The parent component.</param>
	/// <returns>The scroll bar component.</returns>
	private Scrollbar CreateScrollVert(GameObject parent)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PUIElements.CreateUI(parent, "Scrollbar V", canvas: true, PUIAnchoring.End);
		Image obj = val.AddComponent<Image>();
		obj.sprite = PUITuning.Images.ScrollBorderVertical;
		obj.type = (Type)1;
		Scrollbar obj2 = val.AddComponent<Scrollbar>();
		((Selectable)obj2).interactable = true;
		((Selectable)obj2).transition = (Transition)1;
		((Selectable)obj2).colors = PUITuning.Colors.ScrollbarColors;
		obj2.SetDirection((Direction)2, true);
		GameObject val2 = PUIElements.CreateUI(val, "Handle", canvas: true, PUIAnchoring.Stretch, PUIAnchoring.Beginning);
		PUIElements.SetAnchorOffsets(val2, 1f, 1f, 1f, 1f);
		obj2.handleRect = Util.rectTransform(val2);
		Image val3 = val2.AddComponent<Image>();
		val3.sprite = PUITuning.Images.ScrollHandleVertical;
		val3.type = (Type)1;
		((Selectable)obj2).targetGraphic = (Graphic)(object)val3;
		val.SetActive(true);
		PUIElements.SetAnchorOffsets(val, 0f - TrackSize, 0f, 2f, 2f);
		return obj2;
	}

	/// <summary>
	/// Sets the background color to the default Klei dialog blue.
	/// </summary>
	/// <returns>This scroll pane for call chaining.</returns>
	public PScrollPane SetKleiBlueColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
		return this;
	}

	/// <summary>
	/// Sets the background color to the Klei dialog header pink.
	/// </summary>
	/// <returns>This scroll pane for call chaining.</returns>
	public PScrollPane SetKleiPinkColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
		return this;
	}

	public override string ToString()
	{
		return $"PScrollPane[Name={Name},Child={Child}]";
	}
}
