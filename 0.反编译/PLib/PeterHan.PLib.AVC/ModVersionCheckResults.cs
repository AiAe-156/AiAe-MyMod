using Newtonsoft.Json;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.AVC;

/// <summary>
/// The results of checking the mod version.
/// </summary>
[JsonObject(/*Could not decode attribute arguments.*/)]
public sealed class ModVersionCheckResults
{
	/// <summary>
	/// true if the mod is up to date, or false if it is out of date.
	/// </summary>
	[JsonProperty]
	public bool IsUpToDate { get; set; }

	/// <summary>
	/// The mod whose version was queried. The current mod version is available on this
	/// mod through its packagedModInfo.
	/// </summary>
	[JsonProperty]
	public string ModChecked { get; set; }

	/// <summary>
	/// The new version of this mod. If it is not available, it can be null, even if
	/// IsUpdated is false. Not relevant if IsUpToDate reports true.
	/// </summary>
	[JsonProperty]
	public string NewVersion { get; set; }

	public ModVersionCheckResults()
		: this("", updated: false)
	{
	}

	public ModVersionCheckResults(string id, bool updated, string newVersion = null)
	{
		IsUpToDate = updated;
		ModChecked = id;
		NewVersion = newVersion;
	}

	public override bool Equals(object obj)
	{
		if (obj is ModVersionCheckResults modVersionCheckResults && modVersionCheckResults.ModChecked == ModChecked && IsUpToDate == modVersionCheckResults.IsUpToDate)
		{
			return NewVersion == modVersionCheckResults.NewVersion;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ModChecked.GetHashCode();
	}

	public override string ToString()
	{
		return "ModVersionCheckResults[{0},updated={1},newVersion={2}]".F(ModChecked, IsUpToDate, NewVersion ?? "");
	}
}
