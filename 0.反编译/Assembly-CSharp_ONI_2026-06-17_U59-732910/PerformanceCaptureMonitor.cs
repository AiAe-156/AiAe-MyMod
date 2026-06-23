using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Klei;
using Unity.Profiling;
using UnityEngine;

public class PerformanceCaptureMonitor
{
	public class PerformanceCaptureData
	{
		[Serializable]
		public class BrainInfo
		{
			public string name;

			public int count;
		}

		public uint Revision;

		public string Patch;

		public string Branch;

		public bool IsDevelopmentBuild;

		public bool IsBaseGame;

		public string[] ActiveDlcsInSave;

		public List<string> LoadedDlcs;

		public List<string> BuildTags;

		public List<BrainInfo> Brains;

		public int Cycle;

		public float MainMenuLoadTimeSec;

		public float MainMenuMemoryMegs;

		public float SaveLoadTimeSec;

		public float SaveLoadMemoryMegs;

		public float EndMemoryMegs;

		public float PerfMonAverageFrameTimeMs;

		public float SWAverageFrameTimeMs;

		public string BuildConfig;

		public int GameSpeed;
	}

	public static PerformanceCaptureData Data = new PerformanceCaptureData();

	private static ProfilerRecorder systemUsedMemory;

	private static Stopwatch loadTimer = new Stopwatch();

	private static Stopwatch captureTimer = new Stopwatch();

	public static void WritePerformanceCaptureData()
	{
		Data.SWAverageFrameTimeMs = (float)(captureTimer.Elapsed.TotalMilliseconds / (double)GenericGameSettings.instance.scriptedProfile.frameCount);
		Data.Revision = 737195u;
		Data.Branch = "release";
		Data.IsBaseGame = !DlcManager.IsExpansion1Active();
		Data.LoadedDlcs = DlcManager.GetActiveDLCIds();
		if (SaveLoader.Instance != null)
		{
			Data.ActiveDlcsInSave = SaveLoader.Instance.GameInfo.dlcIds.ToArray();
			Data.PerfMonAverageFrameTimeMs = SaveLoader.Instance.GetFrameTime() * 1000f;
		}
		if (Game.Instance != null)
		{
			Data.Cycle = GameUtil.GetCurrentCycle();
			Data.Brains = new List<PerformanceCaptureData.BrainInfo>();
			foreach (BrainScheduler.BrainGroup item in Game.BrainScheduler.debugGetBrainGroups())
			{
				Data.Brains.Add(new PerformanceCaptureData.BrainInfo
				{
					name = item.tag.ToString(),
					count = item.BrainCount
				});
			}
		}
		Data.Patch = "";
		Data.BuildTags = new List<string>();
		Data.BuildTags.Add("release");
		Data.BuildConfig = string.Join("_", Data.BuildTags);
		if (!Data.Patch.IsNullOrWhiteSpace())
		{
			Data.BuildTags.Add("PatchedBuild");
		}
		if (SpeedControlScreen.Instance != null)
		{
			Data.GameSpeed = SpeedControlScreen.Instance.GetSpeed() + 1;
		}
		Data.EndMemoryMegs = GetMemoryUsed();
		string contents = JsonUtility.ToJson(Data);
		File.WriteAllText("PerformanceCaptureData.json", contents);
		DebugUtil.LogArgs("Written PerformanceCaptureData.json");
	}

	public static bool IsCapturingPerformance()
	{
		return !GenericGameSettings.instance.scriptedProfile.saveGame.IsNullOrWhiteSpace();
	}

	public static void Initialize()
	{
		if (IsCapturingPerformance())
		{
			Application.targetFrameRate = -1;
			QualitySettings.vSyncCount = 0;
			KProfilerBegin.OnStopCapture = (System.Action)Delegate.Combine(KProfilerBegin.OnStopCapture, new System.Action(WritePerformanceCaptureData));
			KProfilerBegin.OnStartCapture = (System.Action)Delegate.Combine(KProfilerBegin.OnStartCapture, new System.Action(StartCapture));
			systemUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
		}
	}

	public static void StartCapture()
	{
		captureTimer.Restart();
	}

	public static void StartLoadingSave()
	{
		loadTimer.Restart();
	}

	public static IEnumerator FinishedLoadingSave()
	{
		yield return null;
		Data.SaveLoadMemoryMegs = GetMemoryUsed();
		loadTimer.Stop();
		Data.SaveLoadTimeSec = (float)loadTimer.Elapsed.TotalSeconds;
		if (GenericGameSettings.instance.devQuitAfterLoadingSave)
		{
			App.QuitCode(KCrashReporter.hasCrash ? 1 : 0);
		}
	}

	public static void TryRecordMainMenuStats()
	{
		if (Data.MainMenuMemoryMegs == 0f)
		{
			Data.MainMenuMemoryMegs = GetMemoryUsed();
			Data.MainMenuLoadTimeSec = Time.realtimeSinceStartup;
		}
	}

	public static float GetMemoryUsed()
	{
		if (!systemUsedMemory.Valid)
		{
			return 0f;
		}
		return (float)systemUsedMemory.CurrentValue / 1048576f;
	}
}
