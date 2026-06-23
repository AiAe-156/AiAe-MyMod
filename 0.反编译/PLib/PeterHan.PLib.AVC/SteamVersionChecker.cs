using System;
using System.Collections;
using System.Reflection;
using KMod;
using PeterHan.PLib.Core;
using UnityEngine;

namespace PeterHan.PLib.AVC;

/// <summary>
/// Checks Steam to see if mods are out of date.
/// </summary>
public sealed class SteamVersionChecker : IModVersionChecker
{
	/// <summary>
	/// A reference to the PublishedFileId_t type, or null if running on the EGS/WeGame
	/// version.
	/// </summary>
	private static readonly Type PUBLISHED_FILE_ID = PPatchTools.GetTypeSafe("Steamworks.PublishedFileId_t");

	/// <summary>
	/// A reference to the SteamUGC type, or null if running on the EGS/WeGame version.
	/// </summary>
	private static readonly Type STEAM_UGC = PPatchTools.GetTypeSafe("Steamworks.SteamUGC");

	/// <summary>
	/// A reference to the game's version of SteamUGCService, or null if running on the
	/// EGS/WeGame version.
	/// </summary>
	private static readonly Type STEAM_UGC_SERVICE = PPatchTools.GetTypeSafe("SteamUGCService", "Assembly-CSharp");

	/// <summary>
	/// Detours requires knowing the types at compile time, which might not be available,
	/// and these methods are only called once at startup.
	/// </summary>
	private static readonly MethodInfo FIND_MOD = STEAM_UGC_SERVICE?.GetMethodSafe("FindMod", false, PUBLISHED_FILE_ID);

	private static readonly MethodInfo GET_ITEM_INSTALL_INFO = STEAM_UGC?.GetMethodSafe("GetItemInstallInfo", true, PUBLISHED_FILE_ID, typeof(ulong).MakeByRefType(), typeof(string).MakeByRefType(), typeof(uint), typeof(uint).MakeByRefType());

	private static readonly ConstructorInfo NEW_PUBLISHED_FILE_ID = PUBLISHED_FILE_ID?.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(ulong) }, null);

	/// <summary>
	/// The number of minutes allowed before a mod is considered out of date.
	/// </summary>
	public const double UPDATE_JITTER = 10.0;

	/// <summary>
	/// The epoch time for Steam time stamps.
	/// </summary>
	private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public event PVersionCheck.OnVersionCheckComplete OnVersionCheckCompleted;

	/// <summary>
	/// Checks to see if Steam is initialized yet.
	/// </summary>
	/// <param name="id">The mod's ID.</param>
	/// <param name="boxedID">The ID converted to a boxed PublishedFileId_t and stored in
	/// a single parameter array (for passing into the reflected function).</param>
	/// <param name="mod">The mod to check.</param>
	/// <returns>The results of the version check, or null if Steam has not populated it yet.</returns>
	private static ModVersionCheckResults CheckSteamInit(ulong id, object[] boxedID, Mod mod)
	{
		SteamUGCService instance = SteamUGCService.Instance;
		ModVersionCheckResults result = null;
		if ((Object)(object)instance != (Object)null)
		{
			object obj = FIND_MOD.Invoke(instance, boxedID);
			Mod val = (Mod)((obj is Mod) ? obj : null);
			if (val != null)
			{
				ulong lastUpdateTime = val.lastUpdateTime;
				DateTime dateTime = ((lastUpdateTime == 0L) ? DateTime.MinValue : UnixEpochToDateTime(lastUpdateTime));
				bool flag = dateTime <= GetLocalLastModified(id).AddMinutes(10.0);
				result = new ModVersionCheckResults(mod.staticID, flag, flag ? null : dateTime.ToString("f"));
			}
		}
		return result;
	}

	/// <summary>
	/// Gets the last modified date of a mod's local files. The time is returned in UTC.
	/// </summary>
	/// <param name="id">The mod ID to check.</param>
	/// <returns>The date and time of its last modification.</returns>
	private static DateTime GetLocalLastModified(ulong id)
	{
		DateTime result = DateTime.UtcNow;
		if (GET_ITEM_INSTALL_INFO != null)
		{
			object[] array = new object[5]
			{
				NEW_PUBLISHED_FILE_ID.Invoke(new object[1] { id }),
				0uL,
				"",
				260u,
				0u
			};
			object obj = GET_ITEM_INSTALL_INFO.Invoke(null, array);
			bool flag = default(bool);
			int num;
			if (obj is bool)
			{
				flag = (bool)obj;
				num = 1;
			}
			else
			{
				num = 0;
			}
			if (((uint)num & (flag ? 1u : 0u)) != 0 && array.Length == 5 && array[4] is uint num2 && num2 != 0)
			{
				result = UnixEpochToDateTime(num2);
			}
			else
			{
				PUtil.LogDebug("Unable to determine last modified date for: " + id);
			}
		}
		return result;
	}

	/// <summary>
	/// Converts a time from Steam (seconds since Unix epoch) to a C# DateTime.
	/// </summary>
	/// <param name="timeSeconds">The timestamp since the epoch.</param>
	/// <returns>The UTC date and time that it represents.</returns>
	public static DateTime UnixEpochToDateTime(ulong timeSeconds)
	{
		return UNIX_EPOCH.AddSeconds(timeSeconds);
	}

	public bool CheckVersion(Mod mod)
	{
		if (FIND_MOD != null && NEW_PUBLISHED_FILE_ID != null)
		{
			return DoCheckVersion(mod);
		}
		return false;
	}

	/// <summary>
	/// Checks the mod on Steam and reports if it is out of date. This helper method
	/// avoids a type load error if a non-Steam version of the game is used to load this
	/// mod.
	/// </summary>
	/// <param name="mod">The mod whose version is being checked.</param>
	/// <returns>true if the version check has started, or false if it could not be
	/// started.</returns>
	private bool DoCheckVersion(Mod mod)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		bool result = false;
		if ((int)mod.label.distribution_platform == 1 && ulong.TryParse(mod.label.id, out var result2))
		{
			((MonoBehaviour)Global.Instance).StartCoroutine(WaitForSteamInit(result2, mod));
			result = true;
		}
		else
		{
			PUtil.LogWarning("SteamVersionChecker cannot check version for non-Steam mod {0}".F(mod.staticID));
		}
		return result;
	}

	/// <summary>
	/// To avoid blowing the stack, waits for Steam to initialize in a coroutine.
	/// </summary>
	/// <param name="id">The Steam file ID of the mod.</param>
	/// <param name="mod">The mod to check for updates.</param>
	private IEnumerator WaitForSteamInit(ulong id, Mod mod)
	{
		object[] boxedID = new object[1] { NEW_PUBLISHED_FILE_ID.Invoke(new object[1] { id }) };
		int timeout = 0;
		ModVersionCheckResults modVersionCheckResults;
		int num;
		do
		{
			yield return null;
			modVersionCheckResults = CheckSteamInit(id, boxedID, mod);
			if (modVersionCheckResults != null)
			{
				break;
			}
			num = timeout + 1;
			timeout = num;
		}
		while (num < 120);
		if (modVersionCheckResults == null)
		{
			PUtil.LogWarning("Unable to check version for mod {0} (SteamUGCService timeout)".F(mod.label.title));
		}
		this.OnVersionCheckCompleted?.Invoke(modVersionCheckResults);
	}
}
