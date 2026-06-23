using System;

namespace PeterHan.PLib.Detours;

/// <summary>
/// An interface that describes a detoured field, which stores delegates used to read and
/// write fields or properties.
/// </summary>
/// <typeparam name="P">The containing type of the field or property.</typeparam>
/// <typeparam name="T">The element type of the field or property.</typeparam>
public interface IDetouredField<P, T>
{
	/// <summary>
	/// Invoke to get the field/property value.
	/// </summary>
	Func<P, T> Get { get; }

	/// <summary>
	/// The field name.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Invoke to set the field/property value.
	/// </summary>
	Action<P, T> Set { get; }
}
