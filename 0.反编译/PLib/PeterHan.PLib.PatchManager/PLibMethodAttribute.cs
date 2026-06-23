using System;
using System.Reflection;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.PatchManager;

/// <summary>
/// Represents a method that will be run by PLib at a specific time to reduce the number
/// of patches required and allow conditional integration with other mods.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PLibMethodAttribute : Attribute, IPLibAnnotation
{
	/// <summary>
	/// Requires the specified assembly to be loaded for this method to run. If RequireType
	/// is null or empty, no particular types need to be defined in the assembly. The
	/// assembly name is required, but the version is optional (strong named assemblies
	/// can never load in ONI, since neither Unity nor Klei types are strong named...)
	/// </summary>
	public string RequireAssembly { get; set; }

	/// <summary>
	/// Requires the specified type full name (not assembly qualified name) to exist for
	/// this method to run. If RequireAssembly is null or empty, a type in any assembly
	/// will satisfy the requirement.
	/// </summary>
	public string RequireType { get; set; }

	/// <summary>
	/// When this method is run.
	/// </summary>
	public uint Runtime { get; }

	public PLibMethodAttribute(uint runtime)
	{
		Runtime = runtime;
	}

	/// <summary>
	/// Creates a new patch method instance.
	/// </summary>
	/// <param name="method">The method that was attributed.</param>
	/// <returns>An instance that can execute this patch.</returns>
	public IPatchMethodInstance CreateInstance(MethodInfo method)
	{
		return new PLibMethodInstance(this, method);
	}

	public override string ToString()
	{
		return "PLibMethod[RunAt={0}]".F(RunAt.ToString(Runtime));
	}
}
