using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

/// <summary>
/// The abstract parent of PLib UI objects that are meant to contain other UI objects.
/// </summary>
public abstract class PContainer : IUIComponent
{
	/// <summary>
	/// The background color of this panel.
	/// </summary>
	public Color BackColor { get; set; }

	/// <summary>
	/// The background image of this panel. Tinted by the background color, acts as all
	/// white if left null.
	///
	/// Note that the default background color is transparent, so unless it is set to
	/// some other color this image will be invisible!
	/// </summary>
	public Sprite BackImage { get; set; }

	/// <summary>
	/// The flexible size bounds of this component.
	/// </summary>
	public Vector2 FlexSize { get; set; }

	/// <summary>
	/// The mode to use when displaying the background image.
	/// </summary>
	public Type ImageMode { get; set; }

	/// <summary>
	/// The margin left around the contained components in pixels. If null, no margin will
	/// be used.
	/// </summary>
	public RectOffset Margin { get; set; }

	public string Name { get; protected set; }

	public event PUIDelegates.OnRealize OnRealize;

	protected PContainer(string name)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		BackColor = PUITuning.Colors.Transparent;
		BackImage = null;
		FlexSize = Vector2.zero;
		ImageMode = (Type)0;
		Margin = null;
		Name = name ?? "Container";
	}

	public abstract GameObject Build();

	/// <summary>
	/// Invokes the OnRealize event.
	/// </summary>
	/// <param name="obj">The realized text component.</param>
	protected void InvokeRealize(GameObject obj)
	{
		this.OnRealize?.Invoke(obj);
	}

	/// <summary>
	/// Configures the background color and/or image for this panel.
	/// </summary>
	/// <param name="panel">The realized panel object.</param>
	protected void SetImage(GameObject panel)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (BackColor.a > 0f || (Object)(object)BackImage != (Object)null)
		{
			Image val = panel.AddComponent<Image>();
			((Graphic)val).color = BackColor;
			if ((Object)(object)BackImage != (Object)null)
			{
				val.sprite = BackImage;
				val.type = ImageMode;
			}
		}
	}

	public override string ToString()
	{
		return $"PContainer[Name={Name}]";
	}
}
