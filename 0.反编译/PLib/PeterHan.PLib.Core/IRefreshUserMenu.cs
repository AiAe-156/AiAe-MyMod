namespace PeterHan.PLib.Core;

/// <summary>
/// Implemented by classes which want to use the utility user menu refresh to save some
/// boilerplate code.
/// </summary>
public interface IRefreshUserMenu
{
	/// <summary>
	/// Called when the user button menu in the info panel is refreshed. Since the
	/// arguments are always null, no parameter is passed.
	/// </summary>
	void OnRefreshUserMenu();
}
