using UnityEngine;

namespace PeterHan.PLib.UI;

/// <summary>
/// Implemented by PLib UI components.
/// </summary>
public interface IUIComponent
{
	/// <summary>
	/// The component name.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Actions invoked when the UI component is actually realized.
	/// </summary>
	event PUIDelegates.OnRealize OnRealize;

	/// <summary>
	/// Creates a physical game object embodying this component.
	/// </summary>
	/// <returns>The game object representing this UI component. Multiple invocations return
	/// unique objects.</returns>
	GameObject Build();
}
