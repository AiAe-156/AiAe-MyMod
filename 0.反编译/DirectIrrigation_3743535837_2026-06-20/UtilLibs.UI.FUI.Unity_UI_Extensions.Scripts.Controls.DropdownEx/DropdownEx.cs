using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UtilLibs.UI.FUI.Unity_UI_Extensions.Scripts.Animation;
using UtilLibs.UI.FUI.Unity_UI_Extensions.Scripts.Utilities;

namespace UtilLibs.UI.FUI.Unity_UI_Extensions.Scripts.Controls.DropdownEx;

public class DropdownEx : Selectable, IPointerClickHandler, IEventSystemHandler, ISubmitHandler, ICancelHandler
{
	public class DropdownItem : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ICancelHandler
	{
		[SerializeField]
		private Text m_Text;

		[SerializeField]
		private Image m_Image;

		[SerializeField]
		private RectTransform m_RectTransform;

		[SerializeField]
		private Toggle m_Toggle;

		public Text text
		{
			get
			{
				return m_Text;
			}
			set
			{
				m_Text = value;
			}
		}

		public Image image
		{
			get
			{
				return m_Image;
			}
			set
			{
				m_Image = value;
			}
		}

		public RectTransform rectTransform
		{
			get
			{
				return m_RectTransform;
			}
			set
			{
				m_RectTransform = value;
			}
		}

		public Toggle toggle
		{
			get
			{
				return m_Toggle;
			}
			set
			{
				m_Toggle = value;
			}
		}

		public virtual void OnPointerEnter(PointerEventData eventData)
		{
		}

		public virtual void OnCancel(BaseEventData eventData)
		{
			Dropdown componentInParent = ((Component)this).GetComponentInParent<Dropdown>();
			if (Object.op_Implicit((Object)(object)componentInParent))
			{
				componentInParent.Hide();
			}
		}
	}

	[Serializable]
	public class OptionData
	{
		[SerializeField]
		private string m_Text;

		[SerializeField]
		private Sprite m_Image;

		[SerializeField]
		private bool m_Selected;

		public string text
		{
			get
			{
				return m_Text;
			}
			internal set
			{
				m_Text = value;
			}
		}

		public Sprite image
		{
			get
			{
				return m_Image;
			}
			internal set
			{
				m_Image = value;
			}
		}

		public bool selected
		{
			get
			{
				return m_Selected;
			}
			internal set
			{
				m_Selected = value;
			}
		}

		public OptionData()
		{
		}

		public OptionData(string text)
		{
			this.text = text;
		}

		public OptionData(Sprite image)
		{
			this.image = image;
		}

		public OptionData(string text, Sprite image)
		{
			this.text = text;
			this.image = image;
		}

		public OptionData(string text, bool selected)
		{
			this.text = text;
			this.selected = selected;
		}

		public OptionData(string text, Sprite image, bool selected)
		{
			this.text = text;
			this.image = image;
			this.selected = selected;
		}
	}

	[Serializable]
	public class DropdownEvent : UnityEvent<uint>
	{
	}

	[SerializeField]
	private RectTransform m_Template;

	[SerializeField]
	private LocText m_CaptionText;

	[SerializeField]
	private Image m_CaptionImage;

	[Space]
	[SerializeField]
	private Text m_ItemText;

	[SerializeField]
	private Image m_ItemImage;

	[Space]
	[SerializeField]
	private uint m_Value;

	[Header("Multi-Select Support")]
	[SerializeField]
	private bool _multiSelect = false;

	public string MultipleSelectedText = "Multiple Selected";

	public string NothingSelectedText = "Nothing Selected";

	[Space]
	[SerializeField]
	private List<OptionData> m_Options = new List<OptionData>();

	[Space]
	[SerializeField]
	private DropdownEvent m_OnValueChanged = new DropdownEvent();

	[SerializeField]
	private DropdownEvent m_OnItemSelected = new DropdownEvent();

	[SerializeField]
	private DropdownEvent m_OnItemDeselected = new DropdownEvent();

	private GameObject m_Dropdown;

	private GameObject m_Blocker;

	private List<DropdownItem> m_Items = new List<DropdownItem>();

	private TweenRunner<FloatTween> m_AlphaTweenRunner;

	private bool validTemplate = false;

	private static OptionData s_NoOptionData = new OptionData();

	public RectTransform template
	{
		get
		{
			return m_Template;
		}
		set
		{
			m_Template = value;
			RefreshShownValue();
		}
	}

	public LocText captionText
	{
		get
		{
			return m_CaptionText;
		}
		set
		{
			m_CaptionText = value;
			RefreshShownValue();
		}
	}

	public Image captionImage
	{
		get
		{
			return m_CaptionImage;
		}
		set
		{
			m_CaptionImage = value;
			RefreshShownValue();
		}
	}

	public Text itemText
	{
		get
		{
			return m_ItemText;
		}
		set
		{
			m_ItemText = value;
			RefreshShownValue();
		}
	}

	public Image itemImage
	{
		get
		{
			return m_ItemImage;
		}
		set
		{
			m_ItemImage = value;
			RefreshShownValue();
		}
	}

	public bool AllowMultiSelect
	{
		get
		{
			return _multiSelect;
		}
		set
		{
			_multiSelect = value;
			this.value = 0u;
			RefreshShownValue();
		}
	}

	public IReadOnlyList<OptionData> options => m_Options;

	public DropdownEvent onValueChanged
	{
		get
		{
			return m_OnValueChanged;
		}
		set
		{
			m_OnValueChanged = value;
		}
	}

	public DropdownEvent onItemSelected
	{
		get
		{
			return m_OnItemSelected;
		}
		set
		{
			m_OnItemSelected = value;
		}
	}

	public DropdownEvent onItemDeselected
	{
		get
		{
			return m_OnItemDeselected;
		}
		set
		{
			m_OnItemDeselected = value;
		}
	}

	public uint value
	{
		get
		{
			return m_Value;
		}
		set
		{
			if (Application.isPlaying && (value == m_Value || options.Count == 0))
			{
				return;
			}
			if (AllowMultiSelect)
			{
				uint added = ~m_Value & value;
				uint removed = m_Value & ~value;
				updateOptionsState(added, removed);
				if ((Object)(object)m_Dropdown != (Object)null)
				{
					Hide();
				}
			}
			else
			{
				((UnityEvent<uint>)m_OnItemDeselected).Invoke(m_Value);
				((UnityEvent<uint>)m_OnItemSelected).Invoke(value);
			}
			m_Value = value;
			RefreshShownValue();
			UISystemProfilerApi.AddMarker("DropdownEx.value", (Object)(object)this);
			((UnityEvent<uint>)m_OnValueChanged).Invoke(m_Value);
		}
	}

	public IEnumerable<OptionData> SelectedOptions
	{
		get
		{
			foreach (OptionData option in options)
			{
				if (option.selected)
				{
					yield return option;
				}
			}
		}
	}

	public uint SelectedCount
	{
		get
		{
			if (!AllowMultiSelect)
			{
				return 1u;
			}
			return countBits(m_Value);
		}
	}

	protected virtual void updateOptionsState(uint added, uint removed)
	{
		uint num = 0u;
		while (added != 0)
		{
			if ((added & 1) == 1)
			{
				options[(int)num].selected = true;
				((UnityEvent<uint>)m_OnItemSelected).Invoke(num);
			}
			num++;
			added >>= 1;
		}
		num = 0u;
		while (removed != 0)
		{
			if ((removed & 1) == 1)
			{
				options[(int)num].selected = false;
				((UnityEvent<uint>)m_OnItemDeselected).Invoke(num);
			}
			num++;
			removed >>= 1;
		}
	}

	private uint IndexOfBit(uint src)
	{
		uint num = 0u;
		while (src > 1)
		{
			src >>= 1;
			num++;
		}
		return num;
	}

	private uint countBits(uint v)
	{
		uint num = 0u;
		while (v != 0)
		{
			v &= v - 1;
			num++;
		}
		return num;
	}

	protected Toggle getToggleForIndex(int i)
	{
		Show();
		string arg = (string.IsNullOrEmpty(options[i].text) ? "" : options[i].text);
		GameObject val = GameObject.Find($"Item {i}: {arg}");
		return val.GetComponent<Toggle>();
	}

	protected DropdownEx()
	{
	}

	protected override void Awake()
	{
		m_AlphaTweenRunner = new TweenRunner<FloatTween>();
		m_AlphaTweenRunner.Init((MonoBehaviour)(object)this);
		if (Object.op_Implicit((Object)(object)m_CaptionImage))
		{
			((Behaviour)m_CaptionImage).enabled = (Object)(object)m_CaptionImage.sprite != (Object)null;
		}
		if (Object.op_Implicit((Object)(object)m_Template))
		{
			((Component)m_Template).gameObject.SetActive(false);
		}
	}

	protected override void Start()
	{
		((UIBehaviour)this).Start();
		RefreshShownValue();
	}

	public void RefreshShownValue()
	{
		if (options.Count == 0)
		{
			if ((Object)(object)m_CaptionText != (Object)null)
			{
				((TMP_Text)m_CaptionText).text = ((!string.IsNullOrEmpty(s_NoOptionData.text)) ? s_NoOptionData.text : "");
			}
			if ((Object)(object)m_CaptionImage != (Object)null)
			{
				m_CaptionImage.sprite = s_NoOptionData.image;
				((Behaviour)m_CaptionImage).enabled = (Object)(object)m_CaptionImage.sprite != (Object)null;
			}
			return;
		}
		OptionData optionData = null;
		uint num = 1u;
		for (int i = 0; i < options.Count; i++)
		{
			options[i].selected = false;
		}
		if (AllowMultiSelect)
		{
			num = SelectedCount;
			if (1 == num)
			{
				int index = (int)IndexOfBit(m_Value);
				optionData = options[index];
				optionData.selected = true;
			}
			else
			{
				for (int j = 0; j < options.Count; j++)
				{
					int num2 = 1 << j;
					options[j].selected = (m_Value & num2) == num2;
				}
			}
		}
		else
		{
			optionData = options[(int)m_Value];
			optionData.selected = true;
		}
		if ((Object)(object)m_CaptionText != (Object)null)
		{
			if (num == 0)
			{
				((TMP_Text)m_CaptionText).text = ((!string.IsNullOrEmpty(NothingSelectedText)) ? NothingSelectedText : "");
			}
			else if (1 == num)
			{
				((TMP_Text)m_CaptionText).text = ((!string.IsNullOrEmpty(optionData.text)) ? optionData.text : "");
			}
			else
			{
				((TMP_Text)m_CaptionText).text = ((!string.IsNullOrEmpty(MultipleSelectedText)) ? MultipleSelectedText : "");
			}
		}
		if ((Object)(object)m_CaptionImage != (Object)null)
		{
			if (num == 0)
			{
				m_CaptionImage.sprite = null;
				((Behaviour)m_CaptionImage).enabled = false;
			}
			else if (1 == num)
			{
				m_CaptionImage.sprite = optionData.image;
				((Behaviour)m_CaptionImage).enabled = (Object)(object)m_CaptionImage.sprite != (Object)null;
			}
			else
			{
				m_CaptionImage.sprite = null;
				((Behaviour)m_CaptionImage).enabled = false;
			}
		}
	}

	public void AddOptions(IEnumerable<OptionData> options)
	{
		m_Options.AddRange(options);
		RefreshShownValue();
	}

	public void AddOptions(List<string> options)
	{
		for (int i = 0; i < options.Count; i++)
		{
			m_Options.Add(new OptionData(options[i]));
		}
		RefreshShownValue();
	}

	public void AddOptions(List<Sprite> options)
	{
		for (int i = 0; i < options.Count; i++)
		{
			m_Options.Add(new OptionData(options[i]));
		}
		RefreshShownValue();
	}

	public void ClearOptions()
	{
		m_Options.Clear();
		RefreshShownValue();
	}

	private void SetupTemplate()
	{
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Expected O, but got Unknown
		validTemplate = false;
		if (!Object.op_Implicit((Object)(object)m_Template))
		{
			Debug.LogError((object)"The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a Toggle component serving as the item.", (Object)(object)this);
			return;
		}
		GameObject gameObject = ((Component)m_Template).gameObject;
		gameObject.SetActive(true);
		Toggle componentInChildren = ((Component)m_Template).GetComponentInChildren<Toggle>();
		validTemplate = true;
		if (!Object.op_Implicit((Object)(object)componentInChildren) || (Object)(object)((Component)componentInChildren).transform == (Object)(object)template)
		{
			validTemplate = false;
			Debug.LogError((object)"The dropdown template is not valid. The template must have a child GameObject with a Toggle component serving as the item.", (Object)(object)template);
		}
		else if (!(((Component)componentInChildren).transform.parent is RectTransform))
		{
			validTemplate = false;
			Debug.LogError((object)"The dropdown template is not valid. The child GameObject with a Toggle component (the item) must have a RectTransform on its parent.", (Object)(object)template);
		}
		else if ((Object)(object)itemText != (Object)null && !((Component)itemText).transform.IsChildOf(((Component)componentInChildren).transform))
		{
			validTemplate = false;
			Debug.LogError((object)"The dropdown template is not valid. The Item Text must be on the item GameObject or children of it.", (Object)(object)template);
		}
		else if ((Object)(object)itemImage != (Object)null && !((Component)itemImage).transform.IsChildOf(((Component)componentInChildren).transform))
		{
			validTemplate = false;
			Debug.LogError((object)"The dropdown template is not valid. The Item Image must be on the item GameObject or children of it.", (Object)(object)template);
		}
		if (!validTemplate)
		{
			gameObject.SetActive(false);
			return;
		}
		DropdownItem dropdownItem = ((Component)componentInChildren).gameObject.AddComponent<DropdownItem>();
		dropdownItem.text = m_ItemText;
		dropdownItem.image = m_ItemImage;
		dropdownItem.toggle = componentInChildren;
		dropdownItem.rectTransform = (RectTransform)((Component)componentInChildren).transform;
		Canvas orAddComponent = GetOrAddComponent<Canvas>(gameObject);
		orAddComponent.overrideSorting = true;
		orAddComponent.sortingOrder = 30000;
		GetOrAddComponent<GraphicRaycaster>(gameObject);
		GetOrAddComponent<CanvasGroup>(gameObject);
		gameObject.SetActive(false);
		validTemplate = true;
	}

	private static T GetOrAddComponent<T>(GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (!Object.op_Implicit((Object)(object)val))
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		Show();
	}

	public virtual void OnSubmit(BaseEventData eventData)
	{
		Show();
	}

	public virtual void OnCancel(BaseEventData eventData)
	{
		Hide();
	}

	public void Show()
	{
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0540: Unknown result type (might be due to invalid IL or missing references)
		//IL_054f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		if (!((UIBehaviour)this).IsActive() || !((Selectable)this).IsInteractable() || (Object)(object)m_Dropdown != (Object)null)
		{
			return;
		}
		if (!validTemplate)
		{
			SetupTemplate();
			if (!validTemplate)
			{
				return;
			}
		}
		List<Canvas> list = ListPool<Canvas>.Get();
		((Component)this).gameObject.GetComponentsInParent<Canvas>(false, list);
		if (list.Count == 0)
		{
			return;
		}
		Canvas val = list[0];
		ListPool<Canvas>.Release(list);
		((Component)m_Template).gameObject.SetActive(true);
		m_Dropdown = CreateDropdownList(((Component)m_Template).gameObject);
		((Object)m_Dropdown).name = "Dropdown List";
		m_Dropdown.SetActive(true);
		Transform transform = m_Dropdown.transform;
		RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		((Transform)val2).SetParent(((Component)m_Template).transform.parent, false);
		DropdownItem componentInChildren = m_Dropdown.GetComponentInChildren<DropdownItem>();
		GameObject gameObject = ((Component)((Transform)componentInChildren.rectTransform).parent).gameObject;
		Transform transform2 = gameObject.transform;
		RectTransform val3 = (RectTransform)(object)((transform2 is RectTransform) ? transform2 : null);
		((Component)componentInChildren.rectTransform).gameObject.SetActive(true);
		Rect rect = val3.rect;
		Rect rect2 = componentInChildren.rectTransform.rect;
		Vector2 val4 = ((Rect)(ref rect2)).min - ((Rect)(ref rect)).min + Vector2.op_Implicit(((Transform)componentInChildren.rectTransform).localPosition);
		Vector2 val5 = ((Rect)(ref rect2)).max - ((Rect)(ref rect)).max + Vector2.op_Implicit(((Transform)componentInChildren.rectTransform).localPosition);
		Vector2 size = ((Rect)(ref rect2)).size;
		m_Items.Clear();
		Toggle val6 = null;
		for (int i = 0; i < options.Count; i++)
		{
			OptionData optionData = options[i];
			DropdownItem item = AddItem(optionData, componentInChildren, m_Items);
			if (!((Object)(object)item == (Object)null))
			{
				item.toggle.isOn = optionData.selected;
				((UnityEvent<bool>)(object)item.toggle.onValueChanged).AddListener((UnityAction<bool>)delegate
				{
					OnSelectItem(item.toggle);
				});
				if (item.toggle.isOn)
				{
					((Selectable)item.toggle).Select();
				}
				if ((Object)(object)val6 != (Object)null)
				{
					Navigation navigation = ((Selectable)val6).navigation;
					Navigation navigation2 = ((Selectable)item.toggle).navigation;
					((Navigation)(ref navigation)).mode = (Mode)4;
					((Navigation)(ref navigation2)).mode = (Mode)4;
					((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)item.toggle;
					((Navigation)(ref navigation)).selectOnRight = (Selectable)(object)item.toggle;
					((Navigation)(ref navigation2)).selectOnLeft = (Selectable)(object)val6;
					((Navigation)(ref navigation2)).selectOnUp = (Selectable)(object)val6;
					((Selectable)val6).navigation = navigation;
					((Selectable)item.toggle).navigation = navigation2;
				}
				val6 = item.toggle;
			}
		}
		Vector2 sizeDelta = val3.sizeDelta;
		sizeDelta.y = size.y * (float)m_Items.Count + val4.y - val5.y;
		val3.sizeDelta = sizeDelta;
		Rect rect3 = val2.rect;
		float height = ((Rect)(ref rect3)).height;
		rect3 = val3.rect;
		float num = height - ((Rect)(ref rect3)).height;
		if (num > 0f)
		{
			val2.sizeDelta = new Vector2(val2.sizeDelta.x, val2.sizeDelta.y - num);
		}
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		val2.GetWorldCorners(array);
		Transform transform3 = ((Component)val).transform;
		RectTransform val7 = (RectTransform)(object)((transform3 is RectTransform) ? transform3 : null);
		Rect rect4 = val7.rect;
		for (int num2 = 0; num2 < 2; num2++)
		{
			bool flag = false;
			int num3 = 0;
			while (num3 < 4)
			{
				Vector3 val8 = ((Transform)val7).InverseTransformPoint(array[num3]);
				float num4 = ((Vector3)(ref val8))[num2];
				Vector2 val9 = ((Rect)(ref rect4)).min;
				if (!(num4 < ((Vector2)(ref val9))[num2]))
				{
					float num5 = ((Vector3)(ref val8))[num2];
					val9 = ((Rect)(ref rect4)).max;
					if (!(num5 > ((Vector2)(ref val9))[num2]))
					{
						num3++;
						continue;
					}
				}
				flag = true;
				break;
			}
			if (flag)
			{
				RectTransformUtility.FlipLayoutOnAxis(val2, num2, false, false);
			}
		}
		for (int num6 = 0; num6 < m_Items.Count; num6++)
		{
			RectTransform rectTransform = m_Items[num6].rectTransform;
			rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, 0f);
			rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, 0f);
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, val4.y + size.y * (float)(m_Items.Count - 1 - num6) + size.y * rectTransform.pivot.y);
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, size.y);
		}
		AlphaFadeList(0.15f, 0f, 1f);
		((Component)m_Template).gameObject.SetActive(false);
		((Component)componentInChildren).gameObject.SetActive(false);
		m_Blocker = CreateBlocker(val);
	}

	protected virtual GameObject CreateBlocker(Canvas rootCanvas)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		GameObject val = new GameObject("Blocker");
		RectTransform val2 = val.AddComponent<RectTransform>();
		((Transform)val2).SetParent(((Component)rootCanvas).transform, false);
		val2.anchorMin = Vector2.op_Implicit(Vector3.zero);
		val2.anchorMax = Vector2.op_Implicit(Vector3.one);
		val2.sizeDelta = Vector2.zero;
		Canvas val3 = val.AddComponent<Canvas>();
		val3.overrideSorting = true;
		Canvas component = m_Dropdown.GetComponent<Canvas>();
		val3.sortingLayerID = component.sortingLayerID;
		val3.sortingOrder = component.sortingOrder - 1;
		val.AddComponent<GraphicRaycaster>();
		Image val4 = val.AddComponent<Image>();
		((Graphic)val4).color = Color.clear;
		Button val5 = val.AddComponent<Button>();
		((UnityEvent)val5.onClick).AddListener(new UnityAction(Hide));
		return val;
	}

	protected virtual void DestroyBlocker(GameObject blocker)
	{
		Object.Destroy((Object)(object)blocker);
	}

	protected virtual GameObject CreateDropdownList(GameObject template)
	{
		return Object.Instantiate<GameObject>(template);
	}

	protected virtual void DestroyDropdownList(GameObject dropdownList)
	{
		Object.Destroy((Object)(object)dropdownList);
	}

	protected virtual DropdownItem CreateItem(DropdownItem itemTemplate)
	{
		return Object.Instantiate<DropdownItem>(itemTemplate);
	}

	protected virtual void DestroyItem(DropdownItem item)
	{
	}

	private DropdownItem AddItem(OptionData data, DropdownItem itemTemplate, List<DropdownItem> items)
	{
		DropdownItem dropdownItem = CreateItem(itemTemplate);
		((Transform)dropdownItem.rectTransform).SetParent(((Transform)itemTemplate.rectTransform).parent, false);
		((Component)dropdownItem).gameObject.SetActive(true);
		((Object)((Component)dropdownItem).gameObject).name = "Item " + items.Count + ((data.text != null) ? (": " + data.text) : "");
		if ((Object)(object)dropdownItem.toggle != (Object)null)
		{
			dropdownItem.toggle.isOn = data.selected;
		}
		if (Object.op_Implicit((Object)(object)dropdownItem.text))
		{
			dropdownItem.text.text = data.text;
		}
		if (Object.op_Implicit((Object)(object)dropdownItem.image))
		{
			dropdownItem.image.sprite = data.image;
			((Behaviour)dropdownItem.image).enabled = (Object)(object)dropdownItem.image.sprite != (Object)null;
		}
		items.Add(dropdownItem);
		return dropdownItem;
	}

	private void AlphaFadeList(float duration, float alpha)
	{
		CanvasGroup component = m_Dropdown.GetComponent<CanvasGroup>();
		AlphaFadeList(duration, component.alpha, alpha);
	}

	private void AlphaFadeList(float duration, float start, float end)
	{
		if (!end.Equals(start))
		{
			FloatTween info = new FloatTween
			{
				duration = duration,
				startValue = start,
				targetValue = end
			};
			info.AddOnChangedCallback(SetAlpha);
			info.ignoreTimeScale = true;
			m_AlphaTweenRunner.StartTween(info);
		}
	}

	private void SetAlpha(float alpha)
	{
		if (Object.op_Implicit((Object)(object)m_Dropdown))
		{
			CanvasGroup component = m_Dropdown.GetComponent<CanvasGroup>();
			component.alpha = alpha;
		}
	}

	public void Hide()
	{
		if ((Object)(object)m_Dropdown != (Object)null)
		{
			AlphaFadeList(0.15f, 0f);
			if (((UIBehaviour)this).IsActive())
			{
				((MonoBehaviour)this).StartCoroutine(DelayedDestroyDropdownList(0.15f));
			}
		}
		if ((Object)(object)m_Blocker != (Object)null)
		{
			DestroyBlocker(m_Blocker);
		}
		m_Blocker = null;
		((Selectable)this).Select();
	}

	private IEnumerator DelayedDestroyDropdownList(float delay)
	{
		yield return (object)new WaitForSecondsRealtime(delay);
		for (int i = 0; i < m_Items.Count; i++)
		{
			if ((Object)(object)m_Items[i] != (Object)null)
			{
				DestroyItem(m_Items[i]);
			}
		}
		m_Items.Clear();
		if ((Object)(object)m_Dropdown != (Object)null)
		{
			DestroyDropdownList(m_Dropdown);
		}
		m_Dropdown = null;
	}

	private void OnSelectItem(Toggle toggle)
	{
		if (!toggle.isOn && !AllowMultiSelect)
		{
			toggle.isOn = true;
		}
		int num = -1;
		Transform transform = ((Component)toggle).transform;
		Transform parent = transform.parent;
		for (int i = 0; i < parent.childCount; i++)
		{
			if ((Object)(object)parent.GetChild(i) == (Object)(object)transform)
			{
				num = i - 1;
				break;
			}
		}
		if (num < 0)
		{
			return;
		}
		if (toggle.isOn)
		{
			if (AllowMultiSelect)
			{
				value |= (uint)(1 << num);
			}
			else
			{
				value = (uint)num;
			}
		}
		else if (AllowMultiSelect)
		{
			value &= (uint)(~(1 << num));
		}
		else
		{
			value = (uint)num;
		}
		Hide();
	}

	public void DeselectAll()
	{
		if (AllowMultiSelect)
		{
			value = 0u;
		}
	}
}
