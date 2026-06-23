using HarmonyLib;

namespace PeterHan.PLib.PatchManager;

/// <summary>
/// Refers to a single instance of the annotation, with its annotated method.
/// </summary>
public interface IPatchMethodInstance
{
	/// <summary>
	/// Runs the patch or method if the conditions are met. This method should check its
	/// preconditions before executing the target.
	/// </summary>
	/// <param name="instance">The Harmony instance to use.</param>
	void Run(Harmony instance);
}
