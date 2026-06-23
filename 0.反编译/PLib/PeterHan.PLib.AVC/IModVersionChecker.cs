using KMod;

namespace PeterHan.PLib.AVC;

/// <summary>
/// Implemented by classes which can check the current mod version and detect if it is out
/// of date.
/// </summary>
public interface IModVersionChecker
{
	/// <summary>
	/// The event to subscribe for when the check completes.
	/// </summary>
	event PVersionCheck.OnVersionCheckComplete OnVersionCheckCompleted;

	/// <summary>
	/// Checks the mod and reports if it is out of date. The mod's current version as
	/// reported by its mod_info.yaml file is available on the packagedModInfo member.
	///
	/// This method might not be run on the foreground thread. Do not create new behaviors
	/// or components without a coroutine to an existing GameObject.
	/// </summary>
	/// <param name="mod">The mod whose version is being checked.</param>
	/// <returns>true if the version check has started, or false if it could not be
	/// started, which will trigger the next version checker in line.</returns>
	bool CheckVersion(Mod mod);
}
