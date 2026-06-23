using System.Collections.Generic;

namespace PeterHan.PLib.Core;

/// <summary>
/// Stores a list of forwarded component versions and their shared data.
/// </summary>
internal sealed class PVersionList
{
	/// <summary>
	/// The list of registered components.
	/// </summary>
	public List<PForwardedComponent> Components { get; }

	/// <summary>
	/// The data shared between all components.
	/// </summary>
	public object SharedData { get; set; }

	public PVersionList()
	{
		Components = new List<PForwardedComponent>(32);
		SharedData = null;
	}

	public override string ToString()
	{
		return Components.ToString();
	}
}
