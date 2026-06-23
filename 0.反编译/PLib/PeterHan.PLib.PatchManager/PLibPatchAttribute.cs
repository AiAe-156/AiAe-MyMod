using System;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.PatchManager;

/// <summary>
/// Represents a method that will be patched by PLib at a specific time to allow
/// conditional integration with other mods.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PLibPatchAttribute : Attribute, IPLibAnnotation
{
	/// <summary>
	/// The required argument types. If null, any matching method name is patched, or an
	/// exception thrown if more than one matches.
	/// </summary>
	public Type[] ArgumentTypes { get; set; }

	/// <summary>
	/// If this flag is set, the patch will emit only at DEBUG level if the target method
	/// is not found or matches ambiguously.
	/// </summary>
	public bool IgnoreOnFail { get; set; }

	/// <summary>
	/// The name of the method to patch.
	/// </summary>
	public string MethodName { get; }

	/// <summary>
	/// The type of patch to apply through Harmony.
	/// </summary>
	public HarmonyPatchType PatchType { get; set; }

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

	/// <summary>
	/// The type to patch. If null, the patcher will try to use the required type from the
	/// RequireType parameter.
	/// </summary>
	public Type TargetType { get; }

	/// <summary>
	/// Patches a concrete type and method.
	///
	/// Passing null as the method name will attempt to patch a constructor. Only one
	/// declared constructor may be present, or the call will fail at patch time.
	/// </summary>
	/// <param name="runtime">When to apply the patch.</param>
	/// <param name="target">The type to patch.</param>
	/// <param name="method">The method name to patch.</param>
	public PLibPatchAttribute(uint runtime, Type target, string method)
	{
		ArgumentTypes = null;
		IgnoreOnFail = false;
		MethodName = method;
		PatchType = (HarmonyPatchType)0;
		Runtime = runtime;
		TargetType = target ?? throw new ArgumentNullException("target");
	}

	/// <summary>
	/// Patches a concrete type and overloaded method.
	///
	/// Passing null as the method name will attempt to patch a constructor.
	/// </summary>
	/// <param name="runtime">When to apply the patch.</param>
	/// <param name="target">The type to patch.</param>
	/// <param name="method">The method name to patch.</param>
	/// <param name="argTypes">The types of the overload to patch.</param>
	public PLibPatchAttribute(uint runtime, Type target, string method, params Type[] argTypes)
	{
		ArgumentTypes = argTypes;
		IgnoreOnFail = false;
		MethodName = method;
		PatchType = (HarmonyPatchType)0;
		Runtime = runtime;
		TargetType = target ?? throw new ArgumentNullException("target");
	}

	/// <summary>
	/// Patches a method only if a specified type is available. Use optional parameters to
	/// specify the type to patch using RequireType / RequireAssembly.
	///
	/// Passing null as the method name will attempt to patch a constructor. Only one
	/// declared constructor may be present, or the call will fail at patch time.
	/// </summary>
	/// <param name="runtime">When to apply the patch.</param>
	/// <param name="method">The method name to patch.</param>
	public PLibPatchAttribute(uint runtime, string method)
	{
		ArgumentTypes = null;
		IgnoreOnFail = false;
		MethodName = method;
		PatchType = (HarmonyPatchType)0;
		Runtime = runtime;
		TargetType = null;
	}

	/// <summary>
	/// Patches an overloaded method only if a specified type is available. Use optional
	/// parameters to specify the type to patch using RequireType / RequireAssembly.
	///
	/// Passing null as the method name will attempt to patch a constructor.
	/// </summary>
	/// <param name="runtime">When to apply the patch.</param>
	/// <param name="method">The method name to patch.</param>
	/// <param name="argTypes">The types of the overload to patch.</param>
	public PLibPatchAttribute(uint runtime, string method, params Type[] argTypes)
	{
		ArgumentTypes = argTypes;
		IgnoreOnFail = false;
		MethodName = method;
		PatchType = (HarmonyPatchType)0;
		Runtime = runtime;
		TargetType = null;
	}

	/// <summary>
	/// Creates a new patch method instance.
	/// </summary>
	/// <param name="method">The method that was attributed.</param>
	/// <returns>An instance that can execute this patch.</returns>
	public IPatchMethodInstance CreateInstance(MethodInfo method)
	{
		return new PLibPatchInstance(this, method);
	}

	public override string ToString()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return "PLibPatch[RunAt={0},PatchType={1},MethodName={2}]".F(RunAt.ToString(Runtime), PatchType, MethodName);
	}
}
