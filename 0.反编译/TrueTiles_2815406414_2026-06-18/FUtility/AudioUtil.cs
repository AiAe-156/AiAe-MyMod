using System.Collections.Generic;
using FMOD;
using FMODUnity;
using UnityEngine;

namespace FUtility;

public static class AudioUtil
{
	public static float soundMultiplier = -10f;

	private static readonly Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();

	public static bool LoadSound(string key, string soundFile, bool looping = false, bool oneAtATime = false)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
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
		RESULT val2 = ((System)(ref coreSystem)).createSound(soundFile, val, ref value);
		if ((int)val2 == 0)
		{
			sounds.Add(key, value);
			return true;
		}
		Debug.LogError((object)$"AutoUtil: Failed to create sound. (key={key}, error={val2})");
		return false;
	}

	public static int PlayGlobalSound(string key, float volume = 1f)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
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
					VECTOR val4 = RuntimeUtils.ToFMODVector(new Vector3(((Component)SoundListenerController.Instance).transform.position.x, ((Component)SoundListenerController.Instance).transform.position.y, ((Component)SoundListenerController.Instance).transform.position.z));
					VECTOR val5 = default(VECTOR);
					((Channel)(ref val3)).set3DAttributes(ref val4, ref val5);
					((Channel)(ref val3)).setVolume(volume);
					((Channel)(ref val3)).setMode((MODE)8);
					((Channel)(ref val3)).setPaused(false);
					int result = default(int);
					((Channel)(ref val3)).getIndex(ref result);
					return result;
				}
				Debug.LogError((object)$"AutoUtil: Failed to create sound instance. (key={key}, error={val2})");
			}
			else
			{
				Debug.LogError((object)$"AutoUtil: Failed to get master channel group. (key={key}, error={masterChannelGroup})");
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
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
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
				Debug.LogError((object)$"AutoUtil: Failed to create sound instance. (key={key}, error={val2})");
			}
			else
			{
				Debug.LogError((object)$"AutoUtil: Failed to get master channel group. (key={key}, error={masterChannelGroup})");
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
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
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
				Debug.LogError((object)$"AutoUtil: Failed to create sound instance. (key={key}, error={val2})");
			}
			else
			{
				Debug.LogError((object)$"AutoUtil: Failed to get master channel group. (key={key}, error={masterChannelGroup})");
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
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (channelID >= 0)
		{
			System coreSystem = RuntimeManager.CoreSystem;
			Channel val = default(Channel);
			if ((int)((System)(ref coreSystem)).getChannel(channelID, ref val) == 0)
			{
				((Channel)(ref val)).stop();
				((Channel)(ref val)).clearHandle();
			}
		}
	}
}
