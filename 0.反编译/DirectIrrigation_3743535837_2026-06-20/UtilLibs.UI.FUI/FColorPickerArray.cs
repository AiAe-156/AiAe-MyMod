using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UtilLibs.UIcmp;

namespace UtilLibs.UI.FUI;

public class FColorPickerArray : KMonoBehaviour
{
	private class FColorPickerEntry : KMonoBehaviour
	{
		private Image highlight;

		private Image bgColor;

		private FButton button;

		private Color TargetColor;

		private Action<Color> OnColorSelect = null;

		public override void OnPrefabInit()
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			((KMonoBehaviour)this).OnPrefabInit();
			highlight = ((Component)this).GetComponent<Image>();
			bgColor = ((Component)((KMonoBehaviour)this).transform.Find("Image")).gameObject.GetComponent<Image>();
			button = EntityTemplateExtensions.AddOrGet<FButton>(((Component)this).gameObject);
			button.OnClick += ColorEntryClicked;
			Refresh(Color.white);
		}

		public void Init(Color target, Action<Color> onColorSelect)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			TargetColor = target;
			OnColorSelect = onColorSelect;
		}

		public void Refresh(Color selectedColor)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)highlight == (Object)null))
			{
				((Graphic)highlight).color = ((selectedColor == TargetColor) ? Color.white : Color.black);
				if (((Graphic)bgColor).color != TargetColor)
				{
					((Graphic)bgColor).color = TargetColor;
				}
			}
		}

		private void ColorEntryClicked()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			OnColorSelect?.Invoke(TargetColor);
		}
	}

	private FColorPickerEntry Prefab;

	private Dictionary<Color, FColorPickerEntry> PalletteEntries = new Dictionary<Color, FColorPickerEntry>();

	public Color SelectedColor = Color.white;

	public int Width = 240;

	public int PrefabWidth = 15;

	public event Action<Color> OnColorChange;

	public void SetSelected(Color selectedColor)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		OnSelectColor(selectedColor);
	}

	public override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		InitPallette();
	}

	public override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
		RefreshPallette();
	}

	private void InitPallette()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		Prefab = ((Component)((KMonoBehaviour)this).transform.Find("Item")).gameObject.AddComponent<FColorPickerEntry>();
		((Component)Prefab).gameObject.SetActive(false);
		for (int num = ((KMonoBehaviour)this).transform.childCount - 1; num >= 0; num--)
		{
			Transform child = ((KMonoBehaviour)this).transform.GetChild(num);
			if ((Object)(object)((Component)child).gameObject != (Object)(object)((Component)Prefab).gameObject)
			{
				Object.Destroy((Object)(object)((Component)child).gameObject);
			}
		}
		List<Color> list = new List<Color>(5)
		{
			Color.white,
			new Color(0.75f, 0.75f, 0.75f),
			Color.gray,
			new Color(0.25f, 0.25f, 0.25f),
			Color.black
		};
		RectTransform val = Util.rectTransform((Component)(object)Prefab);
		RectTransform val2 = Util.rectTransform((Component)(object)this);
		float num2 = Width / PrefabWidth - 1;
		for (float num3 = 0f; num3 < num2; num3 += 1f)
		{
			float num4 = num3 / num2;
			list.Add(Color.HSVToRGB(num4, 0.33f, 1f));
			list.Add(Color.HSVToRGB(num4, 0.66f, 1f));
			list.Add(Color.HSVToRGB(num4, 1f, 1f));
			list.Add(Color.HSVToRGB(num4, 1f, 0.66f));
			list.Add(Color.HSVToRGB(num4, 1f, 0.33f));
		}
		foreach (Color item in list)
		{
			FColorPickerEntry fColorPickerEntry = Util.KInstantiateUI<FColorPickerEntry>(((Component)Prefab).gameObject, ((Component)this).gameObject, false);
			fColorPickerEntry.Init(item, OnSelectColor);
			((Component)fColorPickerEntry).gameObject.SetActive(true);
			PalletteEntries[item] = fColorPickerEntry;
		}
	}

	private void RefreshPallette()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Color, FColorPickerEntry> palletteEntry in PalletteEntries)
		{
			palletteEntry.Value.Refresh(SelectedColor);
		}
	}

	private void OnSelectColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (color != SelectedColor)
		{
			SelectedColor = color;
			this.OnColorChange?.Invoke(SelectedColor);
			RefreshPallette();
		}
	}
}
