using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UtilLibs.UI.FUI.Unity_UI_Extensions.Scripts.Utilities;

namespace UtilLibs.UI.FUI.Unity_UI_Extensions.Scripts.Controls.Sliders;

public class MinMaxSlider : Selectable, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	private enum DragState
	{
		Both,
		Min,
		Max
	}

	[Serializable]
	public class SliderEvent : UnityEvent<float, float>
	{
	}

	[Header("UI Controls")]
	[SerializeField]
	private Camera customCamera = null;

	[SerializeField]
	private RectTransform sliderBounds = null;

	[SerializeField]
	private RectTransform minHandle = null;

	[SerializeField]
	private RectTransform maxHandle = null;

	[SerializeField]
	private RectTransform middleGraphic = null;

	[Header("Display Text (Optional)")]
	[SerializeField]
	private TextMeshProUGUI minText = null;

	[SerializeField]
	private TextMeshProUGUI maxText = null;

	[SerializeField]
	private string textFormat = "0";

	[Header("Limits")]
	[SerializeField]
	private float minLimit = 0f;

	[SerializeField]
	private float maxLimit = 100f;

	[Header("Values")]
	public bool wholeNumbers;

	[SerializeField]
	private float minValue = 25f;

	[SerializeField]
	private float maxValue = 75f;

	public LocText MinText;

	public LocText MaxText;

	public LocText MinMaxText;

	public string MinMaxTextFormat = "{0}, {1}";

	public SliderEvent onValueChanged = new SliderEvent();

	private Vector2 dragStartPosition;

	private float dragStartMinValue01;

	private float dragStartMaxValue01;

	private DragState dragState;

	private bool passDragEvents;

	private Camera mainCamera;

	private Canvas parentCanvas;

	private bool isOverlayCanvas;

	private MinMaxSliderAudio AudioComponent;

	public float MinLimit => minLimit;

	public float MaxLimit => maxLimit;

	public float MinValue => minValue;

	public float MaxValue => maxValue;

	public MinMaxValues Values => new MinMaxValues(minValue, maxValue, minLimit, maxLimit);

	public RectTransform SliderBounds
	{
		get
		{
			return sliderBounds;
		}
		set
		{
			sliderBounds = value;
		}
	}

	public RectTransform MinHandle
	{
		get
		{
			return minHandle;
		}
		set
		{
			minHandle = value;
		}
	}

	public RectTransform MaxHandle
	{
		get
		{
			return maxHandle;
		}
		set
		{
			maxHandle = value;
		}
	}

	public RectTransform MiddleGraphic
	{
		get
		{
			return middleGraphic;
		}
		set
		{
			middleGraphic = value;
		}
	}

	protected override void Start()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		((UIBehaviour)this).Start();
		if (!Object.op_Implicit((Object)(object)sliderBounds))
		{
			ref RectTransform reference = ref sliderBounds;
			Transform transform = ((Component)this).transform;
			reference = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
		parentCanvas = ((Component)this).GetComponentInParent<Canvas>();
		isOverlayCanvas = (int)parentCanvas.renderMode == 0;
		mainCamera = (((Object)(object)customCamera != (Object)null) ? customCamera : Camera.main);
		AudioComponent = EntityTemplateExtensions.AddOrGet<MinMaxSliderAudio>(((Component)this).gameObject);
	}

	public void SetLimits(float minLimit, float maxLimit)
	{
		this.minLimit = (wholeNumbers ? ((float)Mathf.RoundToInt(minLimit)) : minLimit);
		this.maxLimit = (wholeNumbers ? ((float)Mathf.RoundToInt(maxLimit)) : maxLimit);
	}

	public void SetValues(MinMaxValues values, bool notify = true)
	{
		SetValues(values.minValue, values.maxValue, values.minLimit, values.maxLimit, notify);
	}

	public void SetValues(float minValue, float maxValue, bool notify = true)
	{
		SetValues(minValue, maxValue, minLimit, maxLimit, notify);
	}

	public void SetValues(float minValue, float maxValue, float minLimit, float maxLimit, bool notify = true)
	{
		this.minValue = (wholeNumbers ? ((float)Mathf.RoundToInt(minValue)) : minValue);
		this.maxValue = (wholeNumbers ? ((float)Mathf.RoundToInt(maxValue)) : maxValue);
		SetLimits(minLimit, maxLimit);
		RefreshSliders();
		UpdateMiddleGraphic();
		UpdateText();
		if (notify)
		{
			((UnityEvent<float, float>)onValueChanged).Invoke(this.minValue, this.maxValue);
		}
	}

	private void RefreshSliders()
	{
		SetSliderAnchors();
		float input = Mathf.Clamp(minValue, minLimit, maxLimit);
		SetMinHandleValue01(minHandle, GetPercentage(minLimit, maxLimit, input));
		float input2 = Mathf.Clamp(maxValue, minLimit, maxLimit);
		SetMaxHandleValue01(maxHandle, GetPercentage(minLimit, maxLimit, input2));
	}

	private void SetSliderAnchors()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		minHandle.anchorMin = new Vector2(0f, 0.5f);
		minHandle.anchorMax = new Vector2(0f, 0.5f);
		minHandle.pivot = new Vector2(0.5f, 0.5f);
		maxHandle.anchorMin = new Vector2(1f, 0.5f);
		maxHandle.anchorMax = new Vector2(1f, 0.5f);
		maxHandle.pivot = new Vector2(0.5f, 0.5f);
	}

	private void UpdateText()
	{
		if ((Object)(object)minText != (Object)null)
		{
			((TMP_Text)minText).SetText(minValue.ToString(textFormat));
		}
		if ((Object)(object)maxText != (Object)null)
		{
			((TMP_Text)maxText).SetText(maxValue.ToString(textFormat));
		}
		if ((Object)(object)MinMaxText != (Object)null)
		{
			((TMP_Text)MinMaxText).SetText(string.Format(MinMaxTextFormat, minValue, MaxValue));
		}
	}

	private void UpdateMiddleGraphic()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)middleGraphic))
		{
			middleGraphic.anchorMin = Vector2.zero;
			middleGraphic.anchorMax = Vector2.one;
			middleGraphic.offsetMin = new Vector2(minHandle.anchoredPosition.x, 0f);
			middleGraphic.offsetMax = new Vector2(maxHandle.anchoredPosition.x, 0f);
		}
	}

	public void SetInteractable(bool setTointeractable)
	{
		((Selectable)this).interactable = setTointeractable;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Selectable)this).interactable)
		{
			return;
		}
		passDragEvents = Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y);
		AudioComponent.OnDragStart();
		if (passDragEvents)
		{
			PassDragEvents<IBeginDragHandler>((Action<IBeginDragHandler>)delegate(IBeginDragHandler x)
			{
				x.OnBeginDrag(eventData);
			});
			return;
		}
		Camera val = (isOverlayCanvas ? null : mainCamera);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderBounds, eventData.position, val, ref dragStartPosition);
		float valueOfPointInSliderBounds = GetValueOfPointInSliderBounds01(dragStartPosition);
		dragStartMinValue01 = GetMinHandleValue01(minHandle);
		dragStartMaxValue01 = GetMaxHandleValue01(maxHandle);
		if (valueOfPointInSliderBounds < dragStartMinValue01 || RectTransformUtility.RectangleContainsScreenPoint(minHandle, eventData.position, val))
		{
			dragState = DragState.Min;
			((Transform)minHandle).SetAsLastSibling();
		}
		else if (valueOfPointInSliderBounds > dragStartMaxValue01 || RectTransformUtility.RectangleContainsScreenPoint(maxHandle, eventData.position, val))
		{
			dragState = DragState.Max;
			((Transform)maxHandle).SetAsLastSibling();
		}
		else
		{
			dragState = DragState.Both;
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		if (!((Selectable)this).interactable)
		{
			return;
		}
		if (passDragEvents)
		{
			PassDragEvents<IDragHandler>((Action<IDragHandler>)delegate(IDragHandler x)
			{
				x.OnDrag(eventData);
			});
		}
		else
		{
			if (!Object.op_Implicit((Object)(object)minHandle) || !Object.op_Implicit((Object)(object)maxHandle))
			{
				return;
			}
			Vector2 val = default(Vector2);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderBounds, eventData.position, isOverlayCanvas ? null : mainCamera, ref val);
			SetSliderAnchors();
			if (dragState == DragState.Min || dragState == DragState.Max)
			{
				float valueOfPointInSliderBounds = GetValueOfPointInSliderBounds01(val);
				float minHandleValue = GetMinHandleValue01(minHandle);
				float maxHandleValue = GetMaxHandleValue01(maxHandle);
				if (dragState == DragState.Min)
				{
					SetMinHandleValue01(minHandle, Mathf.Clamp(valueOfPointInSliderBounds, 0f, maxHandleValue));
				}
				else if (dragState == DragState.Max)
				{
					SetMaxHandleValue01(maxHandle, Mathf.Clamp(valueOfPointInSliderBounds, minHandleValue, 1f));
				}
			}
			else
			{
				float num = val.x - dragStartPosition.x;
				Rect rect = sliderBounds.rect;
				float num2 = num / ((Rect)(ref rect)).width;
				SetMinHandleValue01(minHandle, dragStartMinValue01 + num2);
				SetMaxHandleValue01(maxHandle, dragStartMaxValue01 + num2);
			}
			AudioComponent.OnDrag(dragState == DragState.Max);
			float num3 = Mathf.Lerp(minLimit, maxLimit, GetMinHandleValue01(minHandle));
			float num4 = Mathf.Lerp(minLimit, maxLimit, GetMaxHandleValue01(maxHandle));
			SetValues(num3, num4);
			UpdateText();
			UpdateMiddleGraphic();
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (!((Selectable)this).interactable)
		{
			return;
		}
		AudioComponent.OnDragEnd();
		if (passDragEvents)
		{
			PassDragEvents<IEndDragHandler>((Action<IEndDragHandler>)delegate(IEndDragHandler x)
			{
				x.OnEndDrag(eventData);
			});
			return;
		}
		float minHandleValue = GetMinHandleValue01(minHandle);
		float maxHandleValue = GetMaxHandleValue01(maxHandle);
		if (Math.Abs(minHandleValue) < 0.01f && Math.Abs(maxHandleValue) < 0.01f)
		{
			((Transform)maxHandle).SetAsLastSibling();
		}
		else if (Math.Abs(minHandleValue - 1f) < 0.01f && Math.Abs(maxHandleValue - 1f) < 0.01f)
		{
			((Transform)minHandle).SetAsLastSibling();
		}
	}

	private void PassDragEvents<T>(Action<T> callback) where T : IEventSystemHandler
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Transform parent = ((Component)this).transform.parent;
		while ((Object)(object)parent != (Object)null)
		{
			Component[] components = ((Component)parent).GetComponents<Component>();
			foreach (Component val in components)
			{
				if (val is T)
				{
					callback((T)(IEventSystemHandler)val);
					return;
				}
			}
			parent = parent.parent;
		}
	}

	private void SetMaxHandleValue01(RectTransform handle, float value01)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = sliderBounds.rect;
		float num = value01 * ((Rect)(ref rect)).width;
		rect = sliderBounds.rect;
		handle.anchoredPosition = new Vector2(num - ((Rect)(ref rect)).width + sliderBounds.offsetMax.x, handle.anchoredPosition.y);
	}

	private void SetMinHandleValue01(RectTransform handle, float value01)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = sliderBounds.rect;
		handle.anchoredPosition = new Vector2(value01 * ((Rect)(ref rect)).width + sliderBounds.offsetMin.x, handle.anchoredPosition.y);
	}

	private float GetMaxHandleValue01(RectTransform handle)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		float num = handle.anchoredPosition.x - sliderBounds.offsetMax.x;
		Rect rect = sliderBounds.rect;
		return 1f + num / ((Rect)(ref rect)).width;
	}

	private float GetMinHandleValue01(RectTransform handle)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		float num = handle.anchoredPosition.x - sliderBounds.offsetMin.x;
		Rect rect = sliderBounds.rect;
		return num / ((Rect)(ref rect)).width;
	}

	private float GetValueOfPointInSliderBounds01(Vector2 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = sliderBounds.rect;
		float width = ((Rect)(ref rect)).width;
		return Mathf.Clamp((position.x + width / 2f) / width, 0f, 1f);
	}

	private static float GetPercentage(float min, float max, float input)
	{
		return (input - min) / (max - min);
	}
}
