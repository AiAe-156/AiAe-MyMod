using System;

namespace PeterHan.PLib.Detours;

/// <summary>
/// Stores delegates used to read and write fields or properties.
/// </summary>
/// <typeparam name="P">The containing type of the field or property.</typeparam>
/// <typeparam name="T">The element type of the field or property.</typeparam>
internal sealed class DetouredField<P, T> : IDetouredField<P, T>
{
	/// <summary>
	/// Invoke to get the field/property value.
	/// </summary>
	public Func<P, T> Get { get; }

	/// <summary>
	/// The field name.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Invoke to set the field/property value. Null if the field is const or readonly.
	/// </summary>
	public Action<P, T> Set { get; }

	internal DetouredField(string name, Func<P, T> get, Action<P, T> set)
	{
		Name = name ?? throw new ArgumentNullException("name");
		Get = get;
		Set = set;
	}

	public override string ToString()
	{
		return $"DetouredField[name={Name}]";
	}
}
