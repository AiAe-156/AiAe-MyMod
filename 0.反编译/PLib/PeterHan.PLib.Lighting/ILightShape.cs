namespace PeterHan.PLib.Lighting;

/// <summary>
/// An interface describing local and remote instances of PLightShape.
/// </summary>
public interface ILightShape
{
	/// <summary>
	/// The light shape identifier.
	/// </summary>
	string Identifier { get; }

	/// <summary>
	/// The Klei LightShape represented by this light shape, used in Light2D definitions.
	/// </summary>
	LightShape KleiLightShape { get; }

	/// <summary>
	/// The raycast mode used by this light shape. (-1) if no rays are to be emitted.
	/// </summary>
	LightShape RayMode { get; }

	/// <summary>
	/// Invokes the light handler with the provided light information.
	/// </summary>
	/// <param name="args">The arguments passed to the user light handler.</param>
	void FillLight(LightingArgs args);
}
