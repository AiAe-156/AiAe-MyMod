using System;

namespace PeterHan.PLib.Detours;

/// <summary>
/// Stores delegates used to read and write fields or properties. This version is lazy and
/// only calculates the destination when it is first used.
///
/// This class is not thread safe.
/// </summary>
/// <typeparam name="P">The containing type of the field or property.</typeparam>
/// <typeparam name="T">The element type of the field or property.</typeparam>
internal sealed class LazyDetouredField<P, T> : IDetouredField<P, T>
{
	/// <summary>
	/// The function to get the field value.
	/// </summary>
	private Func<P, T> getter;

	/// <summary>
	/// The function to set the field value.
	/// </summary>
	private Action<P, T> setter;

	/// <summary>
	/// The target type.
	/// </summary>
	private readonly Type type;

	/// <summary>
	/// Invoke to get the field/property value.
	/// </summary>
	public Func<P, T> Get
	{
		get
		{
			Initialize();
			return getter;
		}
	}

	/// <summary>
	/// The field name.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Invoke to set the field/property value.
	/// </summary>
	public Action<P, T> Set
	{
		get
		{
			Initialize();
			return setter;
		}
	}

	internal LazyDetouredField(Type type, string name)
	{
		this.type = type ?? throw new ArgumentNullException("type");
		Name = name ?? throw new ArgumentNullException("name");
		getter = null;
		setter = null;
	}

	/// <summary>
	/// Initializes the getter and setter functions immediately if necessary.
	/// </summary>
	public void Initialize()
	{
		if (getter == null && setter == null)
		{
			IDetouredField<P, T> detouredField = PDetours.DetourField<P, T>(Name);
			getter = detouredField.Get;
			setter = detouredField.Set;
		}
	}

	public override string ToString()
	{
		return string.Format("LazyDetouredField[type={1},name={0}]", Name, type.FullName);
	}
}
