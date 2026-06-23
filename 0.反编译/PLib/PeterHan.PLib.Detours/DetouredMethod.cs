using System;

namespace PeterHan.PLib.Detours;

/// <summary>
/// Stores a detoured method, only performing the expensive reflection when the detour is
/// first used.
///
/// This class is not thread safe.
/// <typeparam name="D">The delegate type to be used to call the detour.</typeparam></summary>
public sealed class DetouredMethod<D> where D : Delegate
{
	/// <summary>
	/// The delegate method which will be called.
	/// </summary>
	private D delg;

	/// <summary>
	/// The target type.
	/// </summary>
	private readonly Type type;

	/// <summary>
	/// Emulates the ability of Delegate.Invoke to actually call the method.
	/// </summary>
	public D Invoke
	{
		get
		{
			Initialize();
			return delg;
		}
	}

	/// <summary>
	/// The method name.
	/// </summary>
	public string Name { get; }

	internal DetouredMethod(Type type, string name)
	{
		this.type = type ?? throw new ArgumentNullException("type");
		Name = name ?? throw new ArgumentNullException("name");
		delg = null;
	}

	/// <summary>
	/// Initializes the getter and setter functions immediately if necessary.
	/// </summary>
	public void Initialize()
	{
		if (delg == null)
		{
			delg = type.Detour<D>(Name);
		}
	}

	public override string ToString()
	{
		return string.Format("LazyDetouredMethod[type={1},name={0}]", Name, type.FullName);
	}
}
