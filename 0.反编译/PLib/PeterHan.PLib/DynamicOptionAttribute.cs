using System;
using PeterHan.PLib.Core;

namespace PeterHan.PLib;

/// <summary>
/// An attribute placed on an option property for a class used as mod options in order to
/// make PLib use a custom options handler. The type used for the handler must inherit
/// from IOptionsEntry.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class DynamicOptionAttribute : Attribute
{
	/// <summary>
	/// The option category.
	/// </summary>
	public string Category { get; }

	/// <summary>
	/// The option handler.
	/// </summary>
	public Type Handler { get; }

	/// <summary>
	/// Denotes a mod option field.
	/// </summary>
	/// <param name="type">The type that will handle this dynamic option.</param>
	/// <param name="category">The category to use, or null for the default category.</param>
	public DynamicOptionAttribute(Type type, string category = null)
	{
		Category = category;
		Handler = type ?? throw new ArgumentNullException("type");
	}

	public override string ToString()
	{
		return "DynamicOption[handler={0},category={1}]".F(Handler.FullName, Category);
	}
}
