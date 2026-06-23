namespace KMod;

internal static class StatusExtensions
{
	public static bool ShouldTryInstall(this Mod.Status status)
	{
		if (status != Mod.Status.NotInstalled)
		{
			return status == Mod.Status.CannotInstall;
		}
		return true;
	}
}
