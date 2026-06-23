namespace PeterHan.PLib.Buildings;

/// <summary>
/// Represents a pipe connection to a building.
/// </summary>
public class ConduitConnection
{
	/// <summary>
	/// The conduit location.
	/// </summary>
	public CellOffset Location { get; }

	/// <summary>
	/// The conduit type.
	/// </summary>
	public ConduitType Type { get; }

	public ConduitConnection(ConduitType type, CellOffset location)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Location = location;
		Type = type;
	}

	public override string ToString()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return $"Connection[Type={Type},Location={Location}]";
	}
}
