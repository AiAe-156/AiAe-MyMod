using System;
using System.Collections.Generic;
using System.IO;
using KMod;
using Newtonsoft.Json;
using PeterHan.PLib.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace PeterHan.PLib.AVC;

/// <summary>
/// Checks the mod version using a URL to a JSON file. The file at this URL must resolve
/// to a JSON file which can deserialize to the JsonURLVersionChecker.ModVersions class.
/// </summary>
public sealed class JsonURLVersionChecker : IModVersionChecker
{
	/// <summary>
	/// The serialization type for JSONURLVersionChecker. Allows multiple mods to query
	/// the same URL.
	/// </summary>
	[JsonObject(/*Could not decode attribute arguments.*/)]
	public sealed class ModVersions
	{
		[JsonProperty]
		public List<ModVersion> mods;

		public ModVersions()
		{
			mods = new List<ModVersion>(16);
		}
	}

	/// <summary>
	/// Represents the current version of each mod.
	/// </summary>
	public sealed class ModVersion
	{
		/// <summary>
		/// The mod's static ID, as reported by its mod.yaml. If a mod does not specify its
		/// static ID, it gets the default ID mod.label.id + "_" + mod.label.
		/// distribution_platform.
		/// </summary>
		public string staticID { get; set; }

		/// <summary>
		/// The mod's current version.
		/// </summary>
		public string version { get; set; }

		public override string ToString()
		{
			return "{0}: version={1}".F(staticID, version);
		}
	}

	/// <summary>
	/// The timeout in seconds for the web request before declaring the check as failed.
	/// </summary>
	public const int REQUEST_TIMEOUT = 8;

	/// <summary>
	/// The URL to query for checking the mod version.
	/// </summary>
	public string JsonVersionURL { get; }

	public event PVersionCheck.OnVersionCheckComplete OnVersionCheckCompleted;

	public JsonURLVersionChecker(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			throw new ArgumentNullException("url");
		}
		JsonVersionURL = url;
	}

	public bool CheckVersion(Mod mod)
	{
		if (mod == null)
		{
			throw new ArgumentNullException("mod");
		}
		UnityWebRequest request = UnityWebRequest.Get(JsonVersionURL);
		request.SetRequestHeader("Content-Type", "application/json");
		request.SetRequestHeader("User-Agent", "PLib AVC");
		request.timeout = 8;
		((AsyncOperation)request.SendWebRequest()).completed += delegate
		{
			OnRequestFinished(request, mod);
		};
		return true;
	}

	/// <summary>
	/// When a web request completes, triggers the handler for the next updater.
	/// </summary>
	/// <param name="request">The JSON web request data.</param>
	/// <param name="mod">The mod that needs to be checked.</param>
	private void OnRequestFinished(UnityWebRequest request, Mod mod)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		ModVersionCheckResults result = null;
		if ((int)request.result == 1)
		{
			ModVersions modVersions;
			using (StreamReader streamReader = new StreamReader(new MemoryStream(request.downloadHandler.data)))
			{
				modVersions = new JsonSerializer
				{
					MaxDepth = 4,
					DateTimeZoneHandling = (DateTimeZoneHandling)1,
					ReferenceLoopHandling = (ReferenceLoopHandling)1
				}.Deserialize<ModVersions>((JsonReader)new JsonTextReader((TextReader)streamReader));
			}
			if (modVersions != null)
			{
				result = ParseModVersion(mod, modVersions);
			}
		}
		request.Dispose();
		this.OnVersionCheckCompleted?.Invoke(result);
	}

	/// <summary>
	/// Parses the JSON file and looks up the version for the specified mod.
	/// </summary>
	/// <param name="mod">The mod's static ID.</param>
	/// <param name="versions">The data from the web JSON file.</param>
	/// <returns>The results of the update, or null if the mod could not be found in the
	/// JSON.</returns>
	private ModVersionCheckResults ParseModVersion(Mod mod, ModVersions versions)
	{
		ModVersionCheckResults result = null;
		string staticID = mod.staticID;
		if (versions.mods != null)
		{
			foreach (ModVersion mod2 in versions.mods)
			{
				if (mod2 != null && mod2.staticID == staticID)
				{
					string text = mod2.version?.Trim();
					result = ((!string.IsNullOrEmpty(text)) ? new ModVersionCheckResults(staticID, text == PVersionCheck.GetCurrentVersion(mod), text) : new ModVersionCheckResults(staticID, updated: true));
					break;
				}
			}
		}
		return result;
	}
}
