using System;
using System.Collections.Generic;
using System.IO;
using FMOD;
using FMODUnity;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Detours;
using UnityEngine;

namespace UtilLibs;

public static class SoundUtils
{
	private delegate void CreateSoundDelegate(AudioSheets instance, string file_name, string anim_name, string type, float min_interval, string sound_name, int frame, string dlcId);

	private static readonly DetouredMethod<CreateSoundDelegate> CREATE_SOUND = typeof(AudioSheets).DetourLazy<CreateSoundDelegate>("CreateSound");

	public static float soundMultiplier = -10f;

	private static readonly Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();

	public static void DumpAllGetSounds()
	{
		Traverse val = Traverse.Create((object)GlobalAssets.Instance);
		Dictionary<string, string> value = val.Field("SoundTable").GetValue<Dictionary<string, string>>();
		Debug.Log((object)("Dic found? - " + (value.Count > 0)));
		foreach (KeyValuePair<string, string> item in value)
		{
			Debug.Log((object)item);
		}
	}

	public static void CopySoundsToAnim(string dstAnim, string srcAnim)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(dstAnim))
		{
			throw new ArgumentNullException("dstAnim");
		}
		if (string.IsNullOrEmpty(srcAnim))
		{
			throw new ArgumentNullException("srcAnim");
		}
		if ((Object)(object)Assets.GetAnim(HashedString.op_Implicit(dstAnim)) != (Object)null)
		{
			GameAudioSheets val = GameAudioSheets.Get();
			try
			{
				foreach (AudioSheet sheet in ((AudioSheets)val).sheets)
				{
					SoundInfo[] soundInfos = sheet.soundInfos;
					int num = soundInfos.Length;
					for (int i = 0; i < num; i++)
					{
						SoundInfo val2 = soundInfos[i];
						if (DlcManager.IsContentSubscribed(val2.RequiredDlcId) && val2.File == srcAnim)
						{
							CreateAllSounds((AudioSheets)(object)val, dstAnim, val2, sheet.defaultType);
						}
					}
				}
				return;
			}
			catch (Exception thrown)
			{
				PUtil.LogWarning("Unable to copy sound files from {0} to {1}:".F(srcAnim, dstAnim));
				PUtil.LogExcWarn(thrown);
				return;
			}
		}
		PUtil.LogWarning("Destination animation \"{0}\" not found!".F(dstAnim));
	}

	private static int CreateSound(AudioSheets sheet, string file, string type, SoundInfo info, string sound, int frame)
	{
		int result = 0;
		if (!string.IsNullOrEmpty(sound) && CREATE_SOUND != null)
		{
			CREATE_SOUND.Invoke(sheet, file, info.Anim, type, info.MinInterval, sound, frame, info.RequiredDlcId);
			result = 1;
		}
		return result;
	}

	private static void CreateAllSounds(AudioSheets sheet, string animFile, SoundInfo info, string defaultType)
	{
		string text = info.Type;
		if (string.IsNullOrEmpty(text))
		{
			text = defaultType;
		}
		int num = CreateSound(sheet, animFile, text, info, info.Name0, info.Frame0);
		num += CreateSound(sheet, animFile, text, info, info.Name1, info.Frame1);
		num += CreateSound(sheet, animFile, text, info, info.Name2, info.Frame2);
		num += CreateSound(sheet, animFile, text, info, info.Name3, info.Frame3);
		num += CreateSound(sheet, animFile, text, info, info.Name4, info.Frame4);
		num += CreateSound(sheet, animFile, text, info, info.Name5, info.Frame5);
		num += CreateSound(sheet, animFile, text, info, info.Name6, info.Frame6);
		num += CreateSound(sheet, animFile, text, info, info.Name7, info.Frame7);
		num += CreateSound(sheet, animFile, text, info, info.Name8, info.Frame8);
		num += CreateSound(sheet, animFile, text, info, info.Name9, info.Frame9);
		num += CreateSound(sheet, animFile, text, info, info.Name10, info.Frame10);
		num += CreateSound(sheet, animFile, text, info, info.Name11, info.Frame11);
	}

	public static float GetSFXVolume()
	{
		return KPlayerPrefs.GetFloat("Volume_SFX") * KPlayerPrefs.GetFloat("Volume_Master");
	}

	public static bool LoadSound(string key, string fileName, bool looping = false, bool oneAtATime = false)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		string path = Path.Combine(UtilMethods.ModPath, "assets");
		string text = Path.Combine(path, fileName);
		if (!File.Exists(text))
		{
			SgtLogger.error("Sound file does not exist: " + text);
		}
		MODE val = (MODE)524568;
		if (looping)
		{
			val = (MODE)(val | 2);
		}
		if (oneAtATime)
		{
			val = (MODE)(val | 0x20000);
		}
		System coreSystem = RuntimeManager.CoreSystem;
		Sound value = default(Sound);
		RESULT val2 = ((System)(ref coreSystem)).createSound(text, val, ref value);
		if ((int)val2 == 0)
		{
			sounds.Add(key, value);
			return true;
		}
		SgtLogger.error($"AutoUtil: Failed to create sound. (key={key}, error={val2})");
		return false;
	}

	public static int PlaySound(string key, float volume = 1f, bool global = false, GameObject attached = null, Vector3 position = default(Vector3))
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (sounds.TryGetValue(key, out var value))
		{
			System coreSystem = RuntimeManager.CoreSystem;
			ChannelGroup val = default(ChannelGroup);
			RESULT masterChannelGroup = ((System)(ref coreSystem)).getMasterChannelGroup(ref val);
			if ((int)masterChannelGroup == 0)
			{
				coreSystem = RuntimeManager.CoreSystem;
				Channel val3 = default(Channel);
				RESULT val2 = ((System)(ref coreSystem)).playSound(value, val, true, ref val3);
				if ((int)val2 == 0)
				{
					if (position == default(Vector3))
					{
						position = (((Object)(object)attached == (Object)null) ? ((Component)SoundListenerController.Instance).transform.position : attached.transform.position);
					}
					VECTOR val4 = RuntimeUtils.ToFMODVector(position);
					VECTOR val5 = default(VECTOR);
					((Channel)(ref val3)).set3DAttributes(ref val4, ref val5);
					((Channel)(ref val3)).setVolume(volume);
					((Channel)(ref val3)).setMode((MODE)(global ? 8 : 524288));
					((Channel)(ref val3)).setPaused(false);
					int result = default(int);
					((Channel)(ref val3)).getIndex(ref result);
					return result;
				}
				SgtLogger.error($"AutoUtil: Failed to create sound instance. (key={key}, error={val2})");
			}
			else
			{
				SgtLogger.error($"AutoUtil: Failed to get master channel group. (key={key}, error={masterChannelGroup})");
			}
		}
		else
		{
			Debug.LogWarning((object)("AudioUtil: Tried to play sound that does not exist. (key=" + key + ")"));
		}
		return -1;
	}

	public static int PlaySound(string key, Vector3 position, float volume = 1f)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (sounds.TryGetValue(key, out var value))
		{
			System coreSystem = RuntimeManager.CoreSystem;
			ChannelGroup val = default(ChannelGroup);
			RESULT masterChannelGroup = ((System)(ref coreSystem)).getMasterChannelGroup(ref val);
			if ((int)masterChannelGroup == 0)
			{
				coreSystem = RuntimeManager.CoreSystem;
				Channel val3 = default(Channel);
				RESULT val2 = ((System)(ref coreSystem)).playSound(value, val, true, ref val3);
				if ((int)val2 == 0)
				{
					VECTOR val4 = RuntimeUtils.ToFMODVector(CameraController.Instance.GetVerticallyScaledPosition(position, false));
					VECTOR val5 = default(VECTOR);
					((Channel)(ref val3)).set3DAttributes(ref val4, ref val5);
					((Channel)(ref val3)).setVolume(volume);
					((Channel)(ref val3)).setPaused(false);
					int result = default(int);
					((Channel)(ref val3)).getIndex(ref result);
					return result;
				}
				SgtLogger.error($"AutoUtil: Failed to create sound instance. (key={key}, error={val2})");
			}
			else
			{
				SgtLogger.error($"AutoUtil: Failed to get master channel group. (key={key}, error={masterChannelGroup})");
			}
		}
		else
		{
			Debug.LogWarning((object)("AudioUtil: Tried to play sound that does not exist. (key=" + key + ")"));
		}
		return -1;
	}

	public static Channel CreateSound(string key)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (sounds.TryGetValue(key, out var value))
		{
			System coreSystem = RuntimeManager.CoreSystem;
			ChannelGroup val = default(ChannelGroup);
			RESULT masterChannelGroup = ((System)(ref coreSystem)).getMasterChannelGroup(ref val);
			if ((int)masterChannelGroup == 0)
			{
				coreSystem = RuntimeManager.CoreSystem;
				Channel result = default(Channel);
				RESULT val2 = ((System)(ref coreSystem)).playSound(value, val, true, ref result);
				if ((int)val2 == 0)
				{
					return result;
				}
				SgtLogger.error($"AutoUtil: Failed to create sound instance. (key={key}, error={val2})");
			}
			else
			{
				SgtLogger.error($"AutoUtil: Failed to get master channel group. (key={key}, error={masterChannelGroup})");
			}
		}
		else
		{
			Debug.LogWarning((object)("AudioUtil: Tried to play sound that does not exist. (key=" + key + ")"));
		}
		return default(Channel);
	}

	public static void StopSound(int channelID)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (channelID >= 0)
		{
			System coreSystem = RuntimeManager.CoreSystem;
			Channel val = default(Channel);
			RESULT channel = ((System)(ref coreSystem)).getChannel(channelID, ref val);
			if ((int)channel == 0)
			{
				((Channel)(ref val)).stop();
				((Channel)(ref val)).clearHandle();
			}
		}
	}
}
