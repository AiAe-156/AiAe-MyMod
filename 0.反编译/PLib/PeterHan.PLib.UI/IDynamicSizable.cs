namespace PeterHan.PLib.UI;

/// <summary>
/// A UI component which can be dynamically resized for its content.
/// </summary>
public interface IDynamicSizable : IUIComponent
{
	/// <summary>
	/// Whether the component should dynamically resize for its content. This adds more
	/// components and more layout depth, so should only be enabled if necessary.
	///
	/// Defaults to false. Must be set to true for components with a nonzero flex size.
	/// </summary>
	bool DynamicSize { get; set; }
}
