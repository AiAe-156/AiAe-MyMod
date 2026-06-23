using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeterHan.PLib.UI.Layouts;

/// <summary>
/// The abstract parent of most layout groups.
/// </summary>
public abstract class AbstractLayoutGroup : UIBehaviour, ISettableFlexSize, ILayoutGroup, ILayoutController, ILayoutElement
{
	/// <summary>
	/// Whether the layout is currently locked.
	/// </summary>
	[SerializeField]
	protected bool locked;

	[SerializeField]
	private float mMinWidth;

	[SerializeField]
	private float mMinHeight;

	[SerializeField]
	private float mPreferredWidth;

	[SerializeField]
	private float mPreferredHeight;

	[SerializeField]
	private float mFlexibleWidth;

	[SerializeField]
	private float mFlexibleHeight;

	[SerializeField]
	private int mLayoutPriority;

	/// <summary>
	/// The cached rect transform to speed up layout.
	/// </summary>
	private RectTransform cachedTransform;

	public float minWidth
	{
		get
		{
			return mMinWidth;
		}
		set
		{
			mMinWidth = value;
		}
	}

	public float preferredWidth
	{
		get
		{
			return mPreferredWidth;
		}
		set
		{
			mPreferredWidth = value;
		}
	}

	/// <summary>
	/// The flexible width of the completed layout group can be set.
	/// </summary>
	public float flexibleWidth
	{
		get
		{
			return mFlexibleWidth;
		}
		set
		{
			mFlexibleWidth = value;
		}
	}

	public float minHeight
	{
		get
		{
			return mMinHeight;
		}
		set
		{
			mMinHeight = value;
		}
	}

	public float preferredHeight
	{
		get
		{
			return mPreferredHeight;
		}
		set
		{
			mPreferredHeight = value;
		}
	}

	/// <summary>
	/// The flexible height of the completed layout group can be set.
	/// </summary>
	public float flexibleHeight
	{
		get
		{
			return mFlexibleHeight;
		}
		set
		{
			mFlexibleHeight = value;
		}
	}

	/// <summary>
	/// The priority of this layout group.
	/// </summary>
	public int layoutPriority
	{
		get
		{
			return mLayoutPriority;
		}
		set
		{
			mLayoutPriority = value;
		}
	}

	protected RectTransform rectTransform
	{
		get
		{
			if ((Object)(object)cachedTransform == (Object)null)
			{
				cachedTransform = Util.rectTransform(((Component)this).gameObject);
			}
			return cachedTransform;
		}
	}

	/// <summary>
	/// Sets an object's layout dirty on the next frame.
	/// </summary>
	/// <param name="transform">The transform to set dirty.</param>
	/// <returns>A coroutine to set it dirty.</returns>
	internal static IEnumerator DelayedSetDirty(RectTransform transform)
	{
		yield return null;
		LayoutRebuilder.MarkLayoutForRebuild(transform);
	}

	/// <summary>
	/// Removes and destroys any PLib layouts on the component. They will be replaced with
	/// a static LayoutElement containing the old size of the component.
	/// </summary>
	/// <param name="component">The component to cleanse.</param>
	internal static void DestroyAndReplaceLayout(GameObject component)
	{
		AbstractLayoutGroup abstractLayoutGroup = default(AbstractLayoutGroup);
		if ((Object)(object)component != (Object)null && component.TryGetComponent<AbstractLayoutGroup>(ref abstractLayoutGroup))
		{
			LayoutElement obj = EntityTemplateExtensions.AddOrGet<LayoutElement>(component);
			obj.flexibleHeight = abstractLayoutGroup.flexibleHeight;
			obj.flexibleWidth = abstractLayoutGroup.flexibleWidth;
			obj.layoutPriority = abstractLayoutGroup.layoutPriority;
			obj.minHeight = abstractLayoutGroup.minHeight;
			obj.minWidth = abstractLayoutGroup.minWidth;
			obj.preferredHeight = abstractLayoutGroup.preferredHeight;
			obj.preferredWidth = abstractLayoutGroup.preferredWidth;
			Object.DestroyImmediate((Object)(object)abstractLayoutGroup);
		}
	}

	protected AbstractLayoutGroup()
	{
		cachedTransform = null;
		locked = false;
		mLayoutPriority = 1;
	}

	public abstract void CalculateLayoutInputHorizontal();

	public abstract void CalculateLayoutInputVertical();

	/// <summary>
	/// Triggers a layout with the current parent, and then locks the layout size. Further
	/// attempts to automatically lay out the component, unless UnlockLayout is called,
	/// will not trigger any action.
	///
	/// The resulting layout has very good performance, but cannot adapt to changes in the
	/// size of its children or its own size.
	/// </summary>
	/// <returns>The computed size of this component when locked.</returns>
	public virtual Vector2 LockLayout()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = Util.rectTransform(((Component)this).gameObject);
		if ((Object)(object)val != (Object)null)
		{
			locked = false;
			CalculateLayoutInputHorizontal();
			val.SetSizeWithCurrentAnchors((Axis)0, minWidth);
			SetLayoutHorizontal();
			CalculateLayoutInputVertical();
			val.SetSizeWithCurrentAnchors((Axis)1, minHeight);
			SetLayoutVertical();
			locked = true;
		}
		return new Vector2(minWidth, minHeight);
	}

	protected override void OnDidApplyAnimationProperties()
	{
		((UIBehaviour)this).OnDidApplyAnimationProperties();
		SetDirty();
	}

	protected override void OnDisable()
	{
		((UIBehaviour)this).OnDisable();
		SetDirty();
	}

	protected override void OnEnable()
	{
		((UIBehaviour)this).OnEnable();
		SetDirty();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		((UIBehaviour)this).OnRectTransformDimensionsChange();
		SetDirty();
	}

	/// <summary>
	/// Sets this layout as dirty.
	/// </summary>
	protected virtual void SetDirty()
	{
		if ((Object)(object)((Component)this).gameObject != (Object)null && ((UIBehaviour)this).IsActive())
		{
			if (CanvasUpdateRegistry.IsRebuildingLayout())
			{
				LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
			}
			else
			{
				((MonoBehaviour)this).StartCoroutine(DelayedSetDirty(rectTransform));
			}
		}
	}

	public abstract void SetLayoutHorizontal();

	public abstract void SetLayoutVertical();

	/// <summary>
	/// Unlocks the layout, allowing it to again dynamically resize when component sizes
	/// are changed.
	/// </summary>
	public virtual void UnlockLayout()
	{
		locked = false;
		LayoutRebuilder.MarkLayoutForRebuild(Util.rectTransform(((Component)this).gameObject));
	}
}
