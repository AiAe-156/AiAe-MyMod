using System;
using System.IO;
using System.Threading;
using UnityEngine;

public class LaunchInitializer : MonoBehaviour
{
	private const string PREFIX = "U";

	private const int UPDATE_NUMBER = 59;

	private static readonly string BUILD_PREFIX = "U" + 59;

	public GameObject[] SpawnPrefabs;

	[SerializeField]
	private int numWaitFrames = 1;

	public static string BuildPrefix()
	{
		return BUILD_PREFIX;
	}

	public static int UpdateNumber()
	{
		return 59;
	}

	private void Update()
	{
		if (numWaitFrames > Time.renderedFrameCount)
		{
			return;
		}
		PerformanceCaptureMonitor.Initialize();
		if (!DistributionPlatform.Initialized)
		{
			if (!SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat))
			{
				Debug.LogError("Machine does not support RGBAFloat32");
			}
			GraphicsOptionsScreen.SetSettingsFromPrefs();
			Util.ApplyInvariantCultureToThread(Thread.CurrentThread);
			Debug.Log("Date: " + System.DateTime.Now);
			Debug.Log("Build: " + BuildWatermark.GetBuildText() + " (preview)");
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			KPlayerPrefs.instance.Load();
			DistributionPlatform.Initialize();
		}
		if (!DistributionPlatform.Inst.IsDLCStatusReady())
		{
			return;
		}
		Debug.Log("DistributionPlatform initialized.");
		DebugUtil.LogArgs(DebugUtil.LINE);
		Debug.Log("Build: " + BuildWatermark.GetBuildText() + " (preview)");
		DebugUtil.LogArgs(DebugUtil.LINE);
		DebugUtil.LogArgs("DLC Information");
		foreach (string ownedDLCId in DlcManager.GetOwnedDLCIds())
		{
			Debug.Log($"- {ownedDLCId} loaded: {DlcManager.IsContentSubscribed(ownedDLCId)}");
		}
		DebugUtil.LogArgs(DebugUtil.LINE);
		KFMOD.Initialize();
		for (int i = 0; i < SpawnPrefabs.Length; i++)
		{
			GameObject gameObject = SpawnPrefabs[i];
			if (gameObject != null)
			{
				Util.KInstantiate(gameObject, base.gameObject);
			}
		}
		DeleteLingeringFiles();
		base.enabled = false;
	}

	private static void DeleteLingeringFiles()
	{
		string[] array = new string[3] { "fmod.log", "load_stats_0.json", "OxygenNotIncluded_Data/output_log.txt" };
		string directoryName = Path.GetDirectoryName(Application.dataPath);
		string[] array2 = array;
		foreach (string path in array2)
		{
			string path2 = Path.Combine(directoryName, path);
			try
			{
				if (File.Exists(path2))
				{
					File.Delete(path2);
				}
			}
			catch (Exception obj)
			{
				Debug.LogWarning(obj);
			}
		}
	}
}
