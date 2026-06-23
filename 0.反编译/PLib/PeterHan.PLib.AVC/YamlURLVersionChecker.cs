using System;
using System.Collections.Generic;
using KMod;
using Klei;
using UnityEngine;
using UnityEngine.Networking;

namespace PeterHan.PLib.AVC;

/// <summary>
/// Checks the mod version using a URL to a YAML file. The file at this URL must resolve
/// to a YAML file of the same format as the mod_info.yaml class.
/// </summary>
public sealed class YamlURLVersionChecker : IModVersionChecker
{
	/// <summary>
	/// The URL to query for checking the mod version.
	/// </summary>
	public string YamlVersionURL { get; }

	public event PVersionCheck.OnVersionCheckComplete OnVersionCheckCompleted;

	public YamlURLVersionChecker(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			throw new ArgumentNullException("url");
		}
		YamlVersionURL = url;
	}

	public bool CheckVersion(Mod mod)
	{
		if (mod == null)
		{
			throw new ArgumentNullException("mod");
		}
		UnityWebRequest request = UnityWebRequest.Get(YamlVersionURL);
		request.SetRequestHeader("Content-Type", "application/x-yaml");
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
	/// <param name="request">The YAML web request data.</param>
	/// <param name="mod">The mod that needs to be checked.</param>
	private void OnRequestFinished(UnityWebRequest request, Mod mod)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		ModVersionCheckResults result = null;
		if ((int)request.result == 1)
		{
			PackagedModInfo obj = YamlIO.Parse<PackagedModInfo>(request.downloadHandler.text, default(FileHandle), (ErrorHandler)null, (List<Tuple<string, Type>>)null);
			string text = ((obj != null) ? obj.version : null);
			if (obj != null && !string.IsNullOrEmpty(text))
			{
				string currentVersion = PVersionCheck.GetCurrentVersion(mod);
				result = new ModVersionCheckResults(mod.staticID, text == currentVersion, text);
			}
		}
		request.Dispose();
		this.OnVersionCheckCompleted?.Invoke(result);
	}
}
