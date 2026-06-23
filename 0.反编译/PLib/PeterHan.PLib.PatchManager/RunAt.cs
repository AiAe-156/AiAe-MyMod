namespace PeterHan.PLib.PatchManager;

/// <summary>
/// Describes when a PLibPatch or PLibMethod should be invoked.
///
/// Due to a bug in ILRepack an enum type in PLib cannot be used as a parameter for a
/// custom attribute. ILmerge does not have this bug.
/// </summary>
public static class RunAt
{
	/// <summary>
	/// Runs the method/patch now.
	///
	/// Note that mods may load in any order and thus not all mods may be initialized at
	/// this time.
	/// </summary>
	public const uint Immediately = 0u;

	/// <summary>
	/// Runs after all mods load, but before most other aspects of the game (including
	/// Assets, Db, and so forth) are initialized. This will run before any other mod
	/// has their UserMod2.AfterModsLoad executed. All PLib components will be initialized
	/// by this point.
	/// </summary>
	public const uint AfterModsLoad = 1u;

	/// <summary>
	/// Runs immediately before Db.Initialize.
	/// </summary>
	public const uint BeforeDbInit = 2u;

	/// <summary>
	/// Runs immediately after Db.Initialize.
	/// </summary>
	public const uint AfterDbInit = 3u;

	/// <summary>
	/// Runs when the main menu has loaded.
	/// </summary>
	public const uint InMainMenu = 4u;

	/// <summary>
	/// Runs when Game.OnPrefabInit has completed.
	/// </summary>
	public const uint OnStartGame = 5u;

	/// <summary>
	/// Runs when Game.DestroyInstances is executed.
	/// </summary>
	public const uint OnEndGame = 6u;

	/// <summary>
	/// Runs after all mod data (including layerable files like world gen and codex/
	/// elements) are loaded. This comes after all UserMod2.AfterModsLoad handlers execute.
	/// All PLib components will be initialized by this point.
	/// </summary>
	public const uint AfterLayerableLoad = 7u;

	/// <summary>
	/// Runs immediately before Db.PostProcess.
	/// </summary>
	public const uint BeforeDbPostProcess = 8u;

	/// <summary>
	/// Runs immediately after Db.PostProcess.
	/// </summary>
	public const uint AfterDbPostProcess = 9u;

	/// <summary>
	/// Runs when DetailsScreen.OnPrefabInit has completed.
	/// </summary>
	public const uint OnDetailsScreenInit = 10u;

	/// <summary>
	/// The string equivalents of each constant for debugging.
	/// </summary>
	private static readonly string[] STRING_VALUES = new string[8] { "Immediately", "AfterModsLoad", "BeforeDbInit", "AfterDbInit", "InMainMenu", "OnStartGame", "OnEndGame", "AfterLayerableLoad" };

	/// <summary>
	/// Gets a human readable representation of a run time constant.
	/// </summary>
	/// <param name="runtime">The time when the patch should be run.</param>
	public static string ToString(uint runtime)
	{
		if (runtime >= STRING_VALUES.Length)
		{
			return runtime.ToString();
		}
		return STRING_VALUES[runtime];
	}
}
