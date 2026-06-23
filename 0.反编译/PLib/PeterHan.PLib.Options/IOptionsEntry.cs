using PeterHan.PLib.UI;

namespace PeterHan.PLib.Options;

/// <summary>
/// All options handlers, including user dynamic option handlers, implement this type.
/// </summary>
public interface IOptionsEntry : IOptionSpec
{
	/// <summary>
	/// Stores whether changing this option requires a restart.
	/// </summary>
	bool RestartRequired { get; set; }

	/// <summary>
	/// Creates UI components that will present this option.
	/// </summary>
	/// <param name="parent">The parent panel where the components should be added.</param>
	/// <param name="row">The row index where the component should be placed. If multiple
	/// rows of components are added, increment this value for each additional row.</param>
	void CreateUIEntry(PGridPanel parent, ref int row);

	/// <summary>
	/// Reads the option value into the UI from the provided settings object.
	/// </summary>
	/// <param name="settings">The settings object.</param>
	void ReadFrom(object settings);

	/// <summary>
	/// Writes the option value from the UI into the provided settings object.
	/// </summary>
	/// <param name="settings">The settings object.</param>
	/// <returns>true if the value changed, or false otherwise.</returns>
	bool WriteTo(object settings);
}
