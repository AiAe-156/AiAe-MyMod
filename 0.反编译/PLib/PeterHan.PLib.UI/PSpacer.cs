using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

/// <summary>
/// A spacer to add into layouts. Has a large flexible width/height by default to eat all
/// the extra space.
/// </summary>
public class PSpacer : IUIComponent
{
	/// <summary>
	/// The flexible size of this spacer. Defaults to (1, 1) but can be set to (0, 0) to
	/// make this spacer a fixed size area instead.
	/// </summary>
	public Vector2 FlexSize { get; set; }

	/// <summary>
	/// The preferred size of this spacer.
	/// </summary>
	public Vector2 PreferredSize { get; set; }

	public string Name { get; }

	public event PUIDelegates.OnRealize OnRealize;

	public PSpacer()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Name = "Spacer";
		FlexSize = Vector2.one;
		PreferredSize = Vector2.zero;
	}

	public GameObject Build()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject(Name);
		LayoutElement obj = val.AddComponent<LayoutElement>();
		obj.flexibleHeight = FlexSize.y;
		obj.flexibleWidth = FlexSize.x;
		obj.minHeight = 0f;
		obj.minWidth = 0f;
		obj.preferredHeight = PreferredSize.y;
		obj.preferredWidth = PreferredSize.x;
		this.OnRealize?.Invoke(val);
		return val;
	}

	public override string ToString()
	{
		return "PSpacer";
	}
}
