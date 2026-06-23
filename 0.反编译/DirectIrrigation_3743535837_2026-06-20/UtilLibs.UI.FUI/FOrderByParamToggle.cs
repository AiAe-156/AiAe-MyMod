using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilLibs.UIcmp;

namespace UtilLibs.UI.FUI;

public class FOrderByParamToggle : KMonoBehaviour
{
	[SerializeField]
	public string Label;

	[SerializeField]
	public bool StartDescending;

	[SerializeField]
	public Image Default;

	[SerializeField]
	public Image Ascending;

	[SerializeField]
	public Image Descending;

	[SerializeField]
	public Image CompactIcon;

	[SerializeField]
	public FButton Toggle;

	[SerializeField]
	public float CompactSwitchSize = -1f;

	[SerializeField]
	public Sprite CompactIconSprite = null;

	[SerializeField]
	public LocText Text = null;

	private int state = 0;

	public Action OnAscending;

	public Action OnDescending;

	private bool init = false;

	private Rect lastSize;

	private RectTransform rectTransform;

	private void Init()
	{
		if (!init)
		{
			init = true;
			Default = ((Component)((KMonoBehaviour)this).transform.Find("SortButton/IconInactive")).gameObject.GetComponent<Image>();
			Ascending = ((Component)((KMonoBehaviour)this).transform.Find("SortButton/IconAscending")).gameObject.GetComponent<Image>();
			Descending = ((Component)((KMonoBehaviour)this).transform.Find("SortButton/IconDescending")).gameObject.GetComponent<Image>();
			Transform obj = ((KMonoBehaviour)this).transform.Find("SmallSizeIcon");
			object compactIcon;
			if (obj == null)
			{
				compactIcon = null;
			}
			else
			{
				GameObject gameObject = ((Component)obj).gameObject;
				compactIcon = ((gameObject != null) ? gameObject.GetComponent<Image>() : null);
			}
			CompactIcon = (Image)compactIcon;
			Toggle = EntityTemplateExtensions.AddOrGet<FButton>(((Component)((KMonoBehaviour)this).transform.Find("SortButton")).gameObject);
			Toggle.ClearOnClick();
			Toggle.OnClick += OnToggleClicked;
			SetText();
			UpdateImages();
		}
	}

	public override void OnPrefabInit()
	{
		Init();
		rectTransform = Util.rectTransform((Component)(object)((KMonoBehaviour)this).transform);
		((KMonoBehaviour)this).OnPrefabInit();
	}

	public override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
		((MonoBehaviour)this).Invoke("OnResize", 0.01f);
		SetText();
	}

	private void SetText()
	{
		if (!Util.IsNullOrWhiteSpace(Label))
		{
			Text = ((Component)((KMonoBehaviour)this).transform).GetComponentInChildren<LocText>();
			LocText text = Text;
			if (text != null)
			{
				((TMP_Text)text).SetText(Label);
			}
			if ((Object)(object)CompactIcon != (Object)null)
			{
				UIUtils.AddSimpleTooltipToObject(((Component)CompactIcon).gameObject, Label);
			}
		}
	}

	public void SetActions(Action sortAscended, Action sortDescended)
	{
		Init();
		OnAscending = sortAscended;
		OnDescending = sortDescended;
	}

	public void SetCompactIcon(Sprite compactIcon, float compactSwitchWidth)
	{
		CompactIconSprite = compactIcon;
		CompactSwitchSize = compactSwitchWidth;
	}

	private void OnToggleClicked()
	{
		int num = state;
		if (1 == 0)
		{
		}
		int num2 = num switch
		{
			0 => (!StartDescending) ? 1 : 2, 
			1 => 2, 
			2 => 1, 
			_ => 0, 
		};
		if (1 == 0)
		{
		}
		state = num2;
		UpdateImages();
		if (state == 1)
		{
			OnAscending?.Invoke();
		}
		if (state == 2)
		{
			OnDescending?.Invoke();
		}
	}

	public void ActivateToggle(int stateOverride = -1)
	{
		Init();
		state = ((!StartDescending) ? 1 : 2);
		if (stateOverride >= 0)
		{
			state = stateOverride;
		}
		UpdateImages();
	}

	public void DeactivateToggle()
	{
		Init();
		state = 0;
		UpdateImages();
	}

	private void UpdateImages()
	{
		((Component)Default).gameObject.SetActive(state == 0);
		((Component)Ascending).gameObject.SetActive(state == 1);
		((Component)Descending).gameObject.SetActive(state == 2);
	}

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (rectTransform.rect != lastSize)
		{
			OnResize();
		}
	}

	private void OnResize()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		lastSize = rectTransform.rect;
		if (!((Object)(object)Text == (Object)null) && !((Object)(object)CompactIcon == (Object)null) && !((Object)(object)CompactIconSprite == (Object)null) && !(CompactSwitchSize < 0f))
		{
			Rect rect = rectTransform.rect;
			bool flag = ((Rect)(ref rect)).width <= CompactSwitchSize;
			CompactIcon.sprite = CompactIconSprite;
			((Component)CompactIcon).gameObject.SetActive(flag);
			((Component)Text).gameObject.SetActive(!flag);
		}
	}
}
