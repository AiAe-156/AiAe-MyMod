using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

/// <summary>
/// A dialog root for UI components.
/// </summary>
public sealed class PDialog : IUIComponent
{
	/// <summary>
	/// Stores information about a dialog button in this dialog.
	/// </summary>
	private sealed class DialogButton
	{
		/// <summary>
		/// The color to use when displaying the button. If null, the default color will
		/// be used.
		/// </summary>
		public readonly ColorStyleSetting backColor;

		/// <summary>
		/// The button key used to indicate that it was selected.
		/// </summary>
		public readonly string key;

		/// <summary>
		/// The text to display for the button.
		/// </summary>
		public readonly string text;

		/// <summary>
		/// The color to use when displaying the button text. If null, the default color
		/// will be used.
		/// </summary>
		public readonly TextStyleSetting textColor;

		/// <summary>
		/// The tooltip for this button.
		/// </summary>
		public readonly string tooltip;

		internal DialogButton(string key, string text, string tooltip, ColorStyleSetting backColor, TextStyleSetting foreColor)
		{
			this.backColor = backColor;
			this.key = key;
			this.text = text;
			this.tooltip = tooltip;
			textColor = foreColor;
		}

		public override string ToString()
		{
			return $"DialogButton[key={key:D},text={text:D}]";
		}
	}

	/// <summary>
	/// The Klei component which backs the dialog.
	/// </summary>
	private sealed class PDialogComp : KScreen
	{
		/// <summary>
		/// The events to invoke when the dialog is closed.
		/// </summary>
		internal PDialog dialog;

		/// <summary>
		/// The key selected by the user.
		/// </summary>
		internal string key;

		/// <summary>
		/// The sort order of this dialog.
		/// </summary>
		internal float sortKey;

		internal PDialogComp()
		{
			key = "close";
			sortKey = 0f;
		}

		/// <summary>
		/// A delegate which closes the dialog on prompt.
		/// </summary>
		/// <param name="source">The button source.</param>
		internal void DoButton(GameObject source)
		{
			key = ((Object)source).name;
			UIDetours.DEACTIVATE_KSCREEN.Invoke((KScreen)(object)this);
		}

		public override float GetSortKey()
		{
			return sortKey;
		}

		protected override void OnDeactivate()
		{
			if (dialog != null)
			{
				dialog.DialogClosed?.Invoke(key);
			}
			((KScreen)this).OnDeactivate();
			dialog = null;
		}

		public override void OnKeyDown(KButtonEvent e)
		{
			if (e.TryConsume((Action)1))
			{
				UIDetours.DEACTIVATE_KSCREEN.Invoke((KScreen)(object)this);
			}
			else
			{
				((KScreen)this).OnKeyDown(e);
			}
		}
	}

	/// <summary>
	/// The margin around dialog buttons.
	/// </summary>
	public static readonly RectOffset BUTTON_MARGIN = new RectOffset(13, 13, 13, 13);

	/// <summary>
	/// The margin inside the dialog close button.
	/// </summary>
	private static readonly RectOffset CLOSE_ICON_MARGIN = new RectOffset(4, 4, 4, 4);

	/// <summary>
	/// The size of the dialog close button's icon.
	/// </summary>
	private static readonly Vector2 CLOSE_ICON_SIZE = Vector2f.op_Implicit(new Vector2f(16f, 16f));

	/// <summary>
	/// The dialog key returned if the user closes the dialog with [ESC] or the X.
	/// </summary>
	public const string DIALOG_KEY_CLOSE = "close";

	/// <summary>
	/// The allowable button choices for the dialog.
	/// </summary>
	private readonly ICollection<DialogButton> buttons;

	/// <summary>
	/// The dialog body panel. To add custom components to the dialog, use AddChild on
	/// this panel. Its direction, margin, and spacing can also be customized.
	/// </summary>
	public PPanel Body { get; }

	/// <summary>
	/// The background color of the dialog itself (including button panel).
	/// </summary>
	public Color DialogBackColor { get; set; }

	/// <summary>
	/// The dialog's maximum size. If the dialog preferred size is bigger than this size,
	/// the dialog will be decreased in size to fit. If either axis is zero, the dialog
	/// gets its preferred size in that axis, at least the value in Size.
	/// </summary>
	public Vector2 MaxSize { get; set; }

	public string Name { get; }

	/// <summary>
	/// The dialog's parent.
	/// </summary>
	public GameObject Parent { get; set; }

	/// <summary>
	/// If a dialog with an odd width/height is displayed, all offsets will end up on a
	/// half pixel offset, which may cause unusual display artifacts as Banker's Rounding
	/// will round values that are supposed to be 1.0 units apart into integer values 2
	/// units apart. If set, this flag will cause Build to round the dialog's size up to
	/// the nearest even integer. If the dialog is already at its maximum size and is still
	/// an odd integer in size, it is rounded down one instead.
	/// </summary>
	public bool RoundToNearestEven { get; set; }

	/// <summary>
	/// The dialog's minimum size. If the dialog preferred size is bigger than this size,
	/// the dialog will be increased in size to fit. If either axis is zero, the dialog
	/// gets its preferred size in that axis, up until the value in MaxSize.
	/// </summary>
	public Vector2 Size { get; set; }

	/// <summary>
	/// The dialog sort order which determines which other dialogs this one is on top of.
	/// </summary>
	public float SortKey { get; set; }

	/// <summary>
	/// The dialog's title.
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// The events to invoke when the dialog is closed.
	/// </summary>
	public PUIDelegates.OnDialogClosed DialogClosed { get; set; }

	public event PUIDelegates.OnRealize OnRealize;

	/// <summary>
	/// Returns a suitable parent object for a dialog.
	/// </summary>
	/// <returns>A game object that can be used as a dialog parent depending on the game
	/// stage, or null if none is available.</returns>
	public static GameObject GetParentObject()
	{
		GameObject result = null;
		FrontEndManager instance = FrontEndManager.Instance;
		if ((Object)(object)instance != (Object)null)
		{
			result = ((Component)instance).gameObject;
		}
		else
		{
			GameScreenManager instance2 = GameScreenManager.Instance;
			if ((Object)(object)instance2 != (Object)null)
			{
				result = instance2.ssOverlayCanvas;
			}
			else
			{
				PUIUtils.LogUIWarning("No dialog parent found!");
			}
		}
		return result;
	}

	/// <summary>
	/// Rounds the size up to the nearest even integer.
	/// </summary>
	/// <param name="size">The current size.</param>
	/// <param name="maxSize">The maximum allowed size.</param>
	/// <returns>The rounded size.</returns>
	private static float RoundUpSize(float size, float maxSize)
	{
		int num = Mathf.CeilToInt(size);
		if (num % 2 == 1)
		{
			num++;
		}
		if ((float)num > maxSize && maxSize > 0f)
		{
			num -= 2;
		}
		return num;
	}

	public PDialog(string name)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		Body = new PPanel("Body")
		{
			Alignment = (TextAnchor)1,
			FlexSize = Vector2.one,
			Margin = new RectOffset(6, 6, 6, 6)
		};
		DialogBackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
		buttons = new List<DialogButton>(4);
		MaxSize = Vector2.zero;
		Name = name ?? "Dialog";
		Parent = GetParentObject();
		RoundToNearestEven = false;
		Size = Vector2.zero;
		SortKey = 0f;
		Title = "Dialog";
	}

	/// <summary>
	/// Adds a button to the dialog. The button will use a blue background with white text
	/// in the default UI font, except for the last button which will be pink.
	/// </summary>
	/// <param name="key">The key to report if this button is selected.</param>
	/// <param name="text">The button text.</param>
	/// <param name="tooltip">The tooltip to display on the button (optional)</param>
	/// <returns>This dialog for call chaining.</returns>
	public PDialog AddButton(string key, string text, string tooltip = null)
	{
		buttons.Add(new DialogButton(key, text, tooltip, null, null));
		return this;
	}

	public PDialog AddButton(string key, string text, string tooltip = null, ColorStyleSetting backColor = null, TextStyleSetting foreColor = null)
	{
		buttons.Add(new DialogButton(key, text, tooltip, backColor, foreColor));
		return this;
	}

	/// <summary>
	/// Adds a handler when this dialog is realized.
	/// </summary>
	/// <param name="onRealize">The handler to invoke on realization.</param>
	/// <returns>This dialog for call chaining.</returns>
	public PDialog AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		OnRealize += onRealize;
		return this;
	}

	public GameObject Build()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		if ((Object)(object)Parent == (Object)null)
		{
			throw new InvalidOperationException("Parent for dialog may not be null");
		}
		GameObject val = PUIElements.CreateUI(Parent, Name);
		PDialogComp pDialogComp = val.AddComponent<PDialogComp>();
		val.AddComponent<Canvas>();
		val.AddComponent<GraphicRaycaster>();
		Image obj = val.AddComponent<Image>();
		((Graphic)obj).color = DialogBackColor;
		obj.sprite = PUITuning.Images.BoxBorder;
		obj.type = (Type)1;
		PGridLayoutGroup pGridLayoutGroup = val.AddComponent<PGridLayoutGroup>();
		pGridLayoutGroup.AddRow(new GridRowSpec());
		pGridLayoutGroup.AddRow(new GridRowSpec(0f, 1f));
		pGridLayoutGroup.AddRow(new GridRowSpec());
		pGridLayoutGroup.AddColumn(new GridColumnSpec(0f, 1f));
		pGridLayoutGroup.AddColumn(new GridColumnSpec());
		LayoutTitle(pGridLayoutGroup, pDialogComp.DoButton);
		pGridLayoutGroup.AddComponent(Body.Build(), new GridComponentSpec(1, 0)
		{
			ColumnSpan = 2,
			Margin = new RectOffset(10, 10, 10, 10)
		});
		CreateUserButtons(pGridLayoutGroup, pDialogComp.DoButton);
		SetDialogSize(val);
		pDialogComp.dialog = this;
		pDialogComp.sortKey = SortKey;
		this.OnRealize?.Invoke(val);
		return val;
	}

	/// <summary>
	/// Creates the user buttons.
	/// </summary>
	/// <param name="layout">The location to add the buttons.</param>
	/// <param name="onPressed">The handler to call when any button is pressed.</param>
	private void CreateUserButtons(PGridLayoutGroup layout, PUIDelegates.OnButtonPressed onPressed)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		PPanel pPanel = new PPanel("Buttons")
		{
			Alignment = (TextAnchor)7,
			Spacing = 7,
			Direction = PanelDirection.Horizontal,
			Margin = new RectOffset(5, 5, 0, 10)
		};
		int num = 0;
		foreach (DialogButton button in buttons)
		{
			string key = button.key;
			ColorStyleSetting backColor = button.backColor;
			TextStyleSetting textStyle = button.textColor ?? PUITuning.Fonts.UILightStyle;
			PButton pButton = new PButton(key)
			{
				Text = button.text,
				ToolTip = button.tooltip,
				Margin = BUTTON_MARGIN,
				OnClick = onPressed,
				Color = backColor,
				TextStyle = textStyle
			};
			if ((Object)(object)backColor == (Object)null)
			{
				if (++num >= buttons.Count)
				{
					pButton.SetKleiPinkStyle();
				}
				else
				{
					pButton.SetKleiBlueStyle();
				}
			}
			pPanel.AddChild(pButton);
		}
		layout.AddComponent(pPanel.Build(), new GridComponentSpec(2, 0)
		{
			ColumnSpan = 2
		});
	}

	/// <summary>
	/// Lays out the dialog title bar and close button.
	/// </summary>
	/// <param name="layout">The layout manager for the dialog.</param>
	/// <param name="onClose">The action to invoke when close is pressed.</param>
	private void LayoutTitle(PGridLayoutGroup layout, PUIDelegates.OnButtonPressed onClose)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new PLabel("Title")
		{
			Margin = new RectOffset(3, 4, 0, 0),
			Text = Title,
			FlexSize = Vector2.one
		}.SetKleiPinkColor().Build();
		layout.AddComponent(val, new GridComponentSpec(0, 0)
		{
			Margin = new RectOffset(0, -2, 0, 0)
		});
		Image obj = EntityTemplateExtensions.AddOrGet<Image>(val);
		obj.sprite = PUITuning.Images.BoxBorder;
		obj.type = (Type)1;
		layout.AddComponent(new PButton("close")
		{
			Sprite = PUITuning.Images.Close,
			Margin = CLOSE_ICON_MARGIN,
			OnClick = onClose,
			SpriteSize = CLOSE_ICON_SIZE,
			ToolTip = LocString.op_Implicit(TOOLTIPS.CLOSETOOLTIP)
		}.SetKleiBlueStyle().Build(), new GridComponentSpec(0, 1));
	}

	/// <summary>
	/// Sets the final size of the dialog using its current position.
	/// </summary>
	/// <param name="dialog">The realized dialog with all components populated.</param>
	private void SetDialogSize(GameObject dialog)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = Util.rectTransform(dialog);
		LayoutRebuilder.ForceRebuildLayoutImmediate(val);
		float num = Math.Max(Size.x, LayoutUtility.GetPreferredWidth(val));
		float num2 = Math.Max(Size.y, LayoutUtility.GetPreferredHeight(val));
		float x = MaxSize.x;
		float y = MaxSize.y;
		if (x > 0f)
		{
			num = Math.Min(num, x);
		}
		if (y > 0f)
		{
			num2 = Math.Min(num2, y);
		}
		if (RoundToNearestEven)
		{
			num = RoundUpSize(num, x);
			num2 = RoundUpSize(num2, y);
		}
		val.SetSizeWithCurrentAnchors((Axis)0, num);
		val.SetSizeWithCurrentAnchors((Axis)1, num2);
	}

	/// <summary>
	/// Builds and shows this dialog.
	/// </summary>
	public void Show()
	{
		KScreen obj = default(KScreen);
		if (Build().TryGetComponent<KScreen>(ref obj))
		{
			UIDetours.ACTIVATE_KSCREEN.Invoke(obj);
		}
	}

	public override string ToString()
	{
		return $"PDialog[Name={Name},Title={Title}]";
	}
}
