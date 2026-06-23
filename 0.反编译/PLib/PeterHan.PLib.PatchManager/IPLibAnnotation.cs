using System.Reflection;

namespace PeterHan.PLib.PatchManager;

/// <summary>
/// The commmon parent of [PLibPatch] and [PLibMethod].
/// </summary>
internal interface IPLibAnnotation
{
	/// <summary>
	/// When this method is run.
	/// </summary>
	uint Runtime { get; }

	/// <summary>
	/// Creates a new patch method instance.
	/// </summary>
	/// <param name="method">The method that was attributed.</param>
	/// <returns>An instance that can execute this patch.</returns>
	IPatchMethodInstance CreateInstance(MethodInfo method);
}
