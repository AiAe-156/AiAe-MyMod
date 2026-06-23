using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

/// <summary>
/// A custom UI button factory class.
/// </summary>
public class PButton : PTextComponent
{
	/// <summary>
	/// The default margins around a button.
	/// </summary>
	internal static readonly RectOffset BUTTON_MARGIN = new RectOffset(7, 7, 5, 5);

	/// <summary>
	/// The button's background color.
	/// </summary>
	public ColorStyleSetting Color { get; set; }

	/// <summary>
	/// The action to trigger on click. It is passed the realized source object.
	/// </summary>
	public PUIDelegates.OnButtonPressed OnClick { get; set; }

	internal static void SetupButton(KButton button, KImage bgImage)
	{
		UIDetours.ADDITIONAL_K_IMAGES.Set(button, (KImage[])(object)new KImage[0]);
		UIDetours.SOUND_PLAYER_BUTTON.Set(button, PUITuning.ButtonSounds);
		UIDetours.BG_IMAGE.Set(button, bgImage);
	}

	internal static void SetupButtonBackground(KImage bgImage)
	{
		UIDetours.APPLY_COLOR_STYLE.Invoke(bgImage);
		((Image)bgImage).sprite = PUITuning.Images.ButtonBorder;
		((Image)bgImage).type = (Type)1;
	}

	/// <summary>
	/// Enables or disables a realized button.
	/// </summary>
	/// <param name="obj">The realized button object.</param>
	/// <param name="enabled">true to make it enabled, or false to make it disabled (greyed out).</param>
	public static void SetButtonEnabled(GameObject obj, bool enabled)
	{
		KButton arg = default(KButton);
		if ((Object)(object)obj != (Object)null && obj.TryGetComponent<KButton>(ref arg))
		{
			UIDetours.IS_INTERACTABLE.Set(arg, enabled);
		}
	}

	public PButton()
		: this(null)
	{
	}

	public PButton(string name)
		: base(name ?? "Button")
	{
		base.Margin = BUTTON_MARGIN;
		base.Sprite = null;
		base.Text = null;
		base.ToolTip = "";
	}

	/// <summary>
	/// Adds a handler when this button is realized.
	/// </summary>
	/// <param name="onRealize">The handler to invoke on realization.</param>
	/// <returns>This button for call chaining.</returns>
	public PButton AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		base.OnRealize += onRealize;
		return this;
	}

	public override GameObject Build()
	{
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		GameObject button = PUIElements.CreateUI(null, base.Name);
		GameObject sprite = null;
		GameObject text = null;
		KImage val = button.AddComponent<KImage>();
		ColorStyleSetting arg = Color ?? PUITuning.Colors.ButtonPinkStyle;
		UIDetours.COLOR_STYLE_SETTING.Set(val, arg);
		SetupButtonBackground(val);
		KButton val2 = button.AddComponent<KButton>();
		PUIDelegates.OnButtonPressed evt = OnClick;
		if (evt != null)
		{
			val2.onClick += delegate
			{
				evt?.Invoke(button);
			};
		}
		SetupButton(val2, val);
		if ((Object)(object)base.Sprite != (Object)null)
		{
			Image val3 = PTextComponent.ImageChildHelper(button, this);
			UIDetours.FG_IMAGE.Set(val2, val3);
			sprite = ((Component)val3).gameObject;
		}
		if (!string.IsNullOrEmpty(base.Text))
		{
			text = ((Component)PTextComponent.TextChildHelper(button, base.TextStyle ?? PUITuning.Fonts.UILightStyle, base.Text)).gameObject;
		}
		PUIElements.SetToolTip(button, base.ToolTip).SetActive(true);
		RelativeLayoutGroup relativeLayoutGroup = button.AddComponent<RelativeLayoutGroup>();
		relativeLayoutGroup.Margin = base.Margin;
		PTextComponent.ArrangeComponent(relativeLayoutGroup, WrapTextAndSprite(text, sprite), base.TextAlignment);
		if (!base.DynamicSize)
		{
			relativeLayoutGroup.LockLayout();
		}
		relativeLayoutGroup.flexibleWidth = base.FlexSize.x;
		relativeLayoutGroup.flexibleHeight = base.FlexSize.y;
		DestroyLayoutIfPossible(button);
		InvokeRealize(button);
		return button;
	}

	/// <summary>
	/// Sets the sprite to a leftward facing arrow. Beware the size, scale the button down!
	/// </summary>
	/// <returns>This button for call chaining.</returns>
	public PButton SetImageLeftArrow()
	{
		base.Sprite = PUITuning.Images.Arrow;
		base.SpriteTransform = ImageTransform.FlipHorizontal;
		return this;
	}

	/// <summary>
	/// Sets the sprite to a rightward facing arrow. Beware the size, scale the button
	/// down!
	/// </summary>
	/// <returns>This button for call chaining.</returns>
	public PButton SetImageRightArrow()
	{
		base.Sprite = PUITuning.Images.Arrow;
		base.SpriteTransform = ImageTransform.None;
		return this;
	}

	/// <summary>
	/// Sets the default Klei pink button style as this button's color and text style.
	/// </summary>
	/// <returns>This button for call chaining.</returns>
	public PButton SetKleiPinkStyle()
	{
		base.TextStyle = PUITuning.Fonts.UILightStyle;
		Color = PUITuning.Colors.ButtonPinkStyle;
		return this;
	}

	/// <summary>
	/// Sets the default Klei blue button style as this button's color and text style.
	/// </summary>
	/// <returns>This button for call chaining.</returns>
	public PButton SetKleiBlueStyle()
	{
		base.TextStyle = PUITuning.Fonts.UILightStyle;
		Color = PUITuning.Colors.ButtonBlueStyle;
		return this;
	}
}
