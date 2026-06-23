using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Database;
using KSerialization;
using Klei.CustomSettings;
using ProcGen;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/CustomGameSettings")]
public class CustomGameSettings : KMonoBehaviour
{
	public enum CustomGameMode
	{
		Survival = 0,
		Nosweat = 1,
		Custom = 255
	}

	public struct MetricSettingsData
	{
		public string Name;

		public string Value;
	}

	private static CustomGameSettings instance;

	public const long NO_COORDINATE_RANGE = -1L;

	private const int NUM_STORY_LEVELS = 3;

	public const string STORY_DISABLED_LEVEL = "Disabled";

	public const string STORY_GUARANTEED_LEVEL = "Guaranteed";

	[Serialize]
	public bool is_custom_game = false;

	[Serialize]
	public CustomGameMode customGameMode = CustomGameMode.Survival;

	[Serialize]
	private Dictionary<string, string> CurrentQualityLevelsBySetting = new Dictionary<string, string>();

	[Serialize]
	private Dictionary<string, string> CurrentMixingLevelsBySetting = new Dictionary<string, string>();

	private Dictionary<string, string> currentStoryLevelsBySetting = new Dictionary<string, string>();

	public List<string> CoordinatedQualitySettings = new List<string>();

	public Dictionary<string, SettingConfig> QualitySettings = new Dictionary<string, SettingConfig>();

	public List<string> CoordinatedStorySettings = new List<string>();

	public Dictionary<string, SettingConfig> StorySettings = new Dictionary<string, SettingConfig>();

	public List<string> CoordinatedMixingSettings = new List<string>();

	public Dictionary<string, SettingConfig> MixingSettings = new Dictionary<string, SettingConfig>();

	private const string coordinatePatern = "(.*)-(\\d*)-(.*)-(.*)-(.*)";

	private string hexChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	public static CustomGameSettings Instance => instance;

	public IReadOnlyDictionary<string, string> CurrentStoryLevelsBySetting => currentStoryLevelsBySetting;

	public event Action<SettingConfig, SettingLevel> OnQualitySettingChanged;

	public event Action<SettingConfig, SettingLevel> OnStorySettingChanged;

	public event Action<SettingConfig, SettingLevel> OnMixingSettingChanged;

	[OnDeserialized]
	private void OnDeserialized()
	{
		if (SaveLoader.Instance.GameInfo.IsVersionOlderThan(7, 6))
		{
			customGameMode = (is_custom_game ? CustomGameMode.Custom : CustomGameMode.Survival);
		}
		if (CurrentQualityLevelsBySetting.ContainsKey("CarePackages "))
		{
			if (!CurrentQualityLevelsBySetting.ContainsKey(CustomGameSettingConfigs.CarePackages.id))
			{
				CurrentQualityLevelsBySetting.Add(CustomGameSettingConfigs.CarePackages.id, CurrentQualityLevelsBySetting["CarePackages "]);
			}
			CurrentQualityLevelsBySetting.Remove("CarePackages ");
		}
		CurrentQualityLevelsBySetting.Remove("Expansion1Active");
		CurrentQualityLevelsBySetting.TryGetValue(CustomGameSettingConfigs.ClusterLayout.id, out var value);
		if (value.IsNullOrWhiteSpace())
		{
			if (!DlcManager.IsExpansion1Active())
			{
				DebugUtil.LogWarningArgs("Deserializing CustomGameSettings.ClusterLayout: ClusterLayout is blank, using default cluster instead");
			}
			value = WorldGenSettings.ClusterDefaultName;
			SetQualitySetting(CustomGameSettingConfigs.ClusterLayout, value);
		}
		if (!SettingsCache.clusterLayouts.clusterCache.ContainsKey(value))
		{
			Debug.Log("Deserializing CustomGameSettings.ClusterLayout: '" + value + "' doesn't exist in the clusterCache, trying to rewrite path to scoped path.");
			string text = SettingsCache.GetScope("EXPANSION1_ID") + value;
			if (SettingsCache.clusterLayouts.clusterCache.ContainsKey(text))
			{
				Debug.Log("Deserializing CustomGameSettings.ClusterLayout: Success in rewriting ClusterLayout '" + value + "' to '" + text + "'");
				SetQualitySetting(CustomGameSettingConfigs.ClusterLayout, text);
			}
			else
			{
				Debug.LogWarning("Deserializing CustomGameSettings.ClusterLayout: Failed to find cluster '" + value + "' including the scoped path, setting to default cluster name.");
				Debug.Log("ClusterCache: " + string.Join(",", SettingsCache.clusterLayouts.clusterCache.Keys));
				SetQualitySetting(CustomGameSettingConfigs.ClusterLayout, WorldGenSettings.ClusterDefaultName);
			}
		}
		CheckCustomGameMode();
	}

	private void AddMissingQualitySettings()
	{
		foreach (KeyValuePair<string, SettingConfig> qualitySetting in QualitySettings)
		{
			SettingConfig value = qualitySetting.Value;
			if (Game.IsCorrectDlcActiveForCurrentSave(value) && !CurrentQualityLevelsBySetting.ContainsKey(value.id))
			{
				if (value.missing_content_default != "")
				{
					DebugUtil.LogArgs("QualitySetting '" + value.id + "' is missing, setting it to missing_content_default '" + value.missing_content_default + "'.");
					SetQualitySetting(value, value.missing_content_default);
				}
				else
				{
					DebugUtil.DevLogError("QualitySetting '" + value.id + "' is missing in this save. Either provide a missing_content_default or handle it in OnDeserialized.");
				}
			}
		}
	}

	protected override void OnPrefabInit()
	{
		bool flag = DlcManager.IsExpansion1Active();
		Action<SettingConfig> action = delegate(SettingConfig setting)
		{
			AddQualitySettingConfig(setting);
			if (setting.coordinate_range >= 0)
			{
				CoordinatedQualitySettings.Add(setting.id);
			}
		};
		Action<SettingConfig> action2 = delegate(SettingConfig setting)
		{
			AddStorySettingConfig(setting);
			if (setting.coordinate_range >= 0)
			{
				CoordinatedStorySettings.Add(setting.id);
			}
		};
		Action<SettingConfig> action3 = delegate(SettingConfig setting)
		{
			AddMixingSettingsConfig(setting);
			if (setting.coordinate_range >= 0)
			{
				CoordinatedMixingSettings.Add(setting.id);
			}
		};
		instance = this;
		action(CustomGameSettingConfigs.ClusterLayout);
		action(CustomGameSettingConfigs.WorldgenSeed);
		action(CustomGameSettingConfigs.ImmuneSystem);
		action(CustomGameSettingConfigs.CalorieBurn);
		action(CustomGameSettingConfigs.Morale);
		action(CustomGameSettingConfigs.Durability);
		action(CustomGameSettingConfigs.MeteorShowers);
		action(CustomGameSettingConfigs.Radiation);
		action(CustomGameSettingConfigs.Stress);
		action(CustomGameSettingConfigs.StressBreaks);
		action(CustomGameSettingConfigs.CarePackages);
		action(CustomGameSettingConfigs.SandboxMode);
		action(CustomGameSettingConfigs.FastWorkersMode);
		action(CustomGameSettingConfigs.SaveToCloud);
		action(CustomGameSettingConfigs.Teleporters);
		action(CustomGameSettingConfigs.BionicWattage);
		action(CustomGameSettingConfigs.DemoliorDifficulty);
		action3(CustomMixingSettingsConfigs.DLC2Mixing);
		action3(CustomMixingSettingsConfigs.IceCavesMixing);
		action3(CustomMixingSettingsConfigs.CarrotQuarryMixing);
		action3(CustomMixingSettingsConfigs.SugarWoodsMixing);
		action3(CustomMixingSettingsConfigs.CeresAsteroidMixing);
		action3(CustomMixingSettingsConfigs.DLC3Mixing);
		action3(CustomMixingSettingsConfigs.DLC4Mixing);
		action3(CustomMixingSettingsConfigs.GardenMixing);
		action3(CustomMixingSettingsConfigs.RaptorMixing);
		action3(CustomMixingSettingsConfigs.WetlandsMixing);
		action3(CustomMixingSettingsConfigs.PrehistoricAsteroidMixing);
		action3(CustomMixingSettingsConfigs.DLC5Mixing);
		action3(CustomMixingSettingsConfigs.BeachMixing);
		action3(CustomMixingSettingsConfigs.ReefMixing);
		action3(CustomMixingSettingsConfigs.KelpForestMixing);
		action3(CustomMixingSettingsConfigs.AbyssMixing);
		action3(CustomMixingSettingsConfigs.AquaticAsteroidMixing);
		foreach (Story item in Db.Get().Stories.GetStoriesSortedByCoordinateOrder())
		{
			int num = ((item.kleiUseOnlyCoordinateOrder == -1) ? (-1) : 3);
			SettingConfig settingConfig = new ListSettingConfig(item.Id, "", "", new List<SettingLevel>
			{
				new SettingLevel("Disabled", "", "", 0L),
				new SettingLevel("Guaranteed", "", "", 1L)
			}, "Disabled", "Disabled", num, debug_only: false, triggers_custom_game: false);
			action2(settingConfig);
		}
		foreach (KeyValuePair<string, SettingConfig> mixingSetting in MixingSettings)
		{
			if (mixingSetting.Value is DlcMixingSettingConfig dlcMixingSettingConfig && DlcManager.IsContentSubscribed(dlcMixingSettingConfig.id))
			{
				SetMixingSetting(dlcMixingSettingConfig, "Enabled");
			}
		}
		VerifySettingCoordinates();
	}

	public void DisableAllStories()
	{
		foreach (KeyValuePair<string, SettingConfig> storySetting in StorySettings)
		{
			SetStorySetting(storySetting.Value, value: false);
		}
	}

	public void SetSurvivalDefaults()
	{
		customGameMode = CustomGameMode.Survival;
		foreach (KeyValuePair<string, SettingConfig> qualitySetting in QualitySettings)
		{
			SetQualitySetting(qualitySetting.Value, qualitySetting.Value.GetDefaultLevelId());
		}
	}

	public void SetNosweatDefaults()
	{
		customGameMode = CustomGameMode.Nosweat;
		foreach (KeyValuePair<string, SettingConfig> qualitySetting in QualitySettings)
		{
			SetQualitySetting(qualitySetting.Value, qualitySetting.Value.GetNoSweatDefaultLevelId());
		}
	}

	public SettingLevel CycleQualitySettingLevel(ListSettingConfig config, int direction)
	{
		SetQualitySetting(config, config.CycleSettingLevelID(CurrentQualityLevelsBySetting[config.id], direction));
		return config.GetLevel(CurrentQualityLevelsBySetting[config.id]);
	}

	public SettingLevel ToggleQualitySettingLevel(ToggleSettingConfig config)
	{
		SetQualitySetting(config, config.ToggleSettingLevelID(CurrentQualityLevelsBySetting[config.id]));
		return config.GetLevel(CurrentQualityLevelsBySetting[config.id]);
	}

	private void CheckCustomGameMode()
	{
		bool flag = true;
		bool flag2 = true;
		foreach (KeyValuePair<string, string> item in CurrentQualityLevelsBySetting)
		{
			if (!QualitySettings.ContainsKey(item.Key))
			{
				DebugUtil.LogWarningArgs("Quality settings missing " + item.Key);
			}
			else if (QualitySettings[item.Key].triggers_custom_game)
			{
				if (item.Value != QualitySettings[item.Key].GetDefaultLevelId())
				{
					flag = false;
				}
				if (item.Value != QualitySettings[item.Key].GetNoSweatDefaultLevelId())
				{
					flag2 = false;
				}
				if (!flag && !flag2)
				{
					break;
				}
			}
		}
		CustomGameMode customGameMode = ((!flag) ? (flag2 ? CustomGameMode.Nosweat : CustomGameMode.Custom) : CustomGameMode.Survival);
		if (customGameMode != this.customGameMode)
		{
			DebugUtil.LogArgs("Game mode changed from", this.customGameMode, "to", customGameMode);
			this.customGameMode = customGameMode;
		}
	}

	public void SetQualitySetting(SettingConfig config, string value)
	{
		SetQualitySetting(config, value, notify: true);
	}

	public void SetQualitySetting(SettingConfig config, string value, bool notify)
	{
		CurrentQualityLevelsBySetting[config.id] = value;
		CheckCustomGameMode();
		if (notify && this.OnQualitySettingChanged != null)
		{
			this.OnQualitySettingChanged(config, GetCurrentQualitySetting(config));
		}
	}

	public SettingLevel GetCurrentQualitySetting(SettingConfig setting)
	{
		return GetCurrentQualitySetting(setting.id);
	}

	public SettingLevel GetCurrentQualitySetting(string setting_id)
	{
		SettingConfig settingConfig = QualitySettings[setting_id];
		if (customGameMode == CustomGameMode.Survival && settingConfig.triggers_custom_game)
		{
			return settingConfig.GetLevel(settingConfig.GetDefaultLevelId());
		}
		if (customGameMode == CustomGameMode.Nosweat && settingConfig.triggers_custom_game)
		{
			return settingConfig.GetLevel(settingConfig.GetNoSweatDefaultLevelId());
		}
		if (!CurrentQualityLevelsBySetting.ContainsKey(setting_id))
		{
			CurrentQualityLevelsBySetting[setting_id] = QualitySettings[setting_id].GetDefaultLevelId();
		}
		string level_id = (DlcManager.IsAllContentSubscribed(settingConfig.required_content) ? CurrentQualityLevelsBySetting[setting_id] : settingConfig.GetDefaultLevelId());
		return QualitySettings[setting_id].GetLevel(level_id);
	}

	public string GetCurrentQualitySettingLevelId(SettingConfig config)
	{
		return CurrentQualityLevelsBySetting[config.id];
	}

	public string GetSettingLevelLabel(string setting_id, string level_id)
	{
		SettingConfig settingConfig = QualitySettings[setting_id];
		if (settingConfig != null)
		{
			SettingLevel level = settingConfig.GetLevel(level_id);
			if (level != null)
			{
				return level.label;
			}
		}
		Debug.LogWarning("No label string for setting: " + setting_id + " level: " + level_id);
		return "";
	}

	public string GetQualitySettingLevelTooltip(string setting_id, string level_id)
	{
		SettingConfig settingConfig = QualitySettings[setting_id];
		if (settingConfig != null)
		{
			SettingLevel level = settingConfig.GetLevel(level_id);
			if (level != null)
			{
				return level.tooltip;
			}
		}
		Debug.LogWarning("No tooltip string for setting: " + setting_id + " level: " + level_id);
		return "";
	}

	public void AddQualitySettingConfig(SettingConfig config)
	{
		QualitySettings.Add(config.id, config);
		if (!CurrentQualityLevelsBySetting.ContainsKey(config.id) || string.IsNullOrEmpty(CurrentQualityLevelsBySetting[config.id]))
		{
			CurrentQualityLevelsBySetting[config.id] = config.GetDefaultLevelId();
		}
	}

	public void AddStorySettingConfig(SettingConfig config)
	{
		StorySettings.Add(config.id, config);
		if (!currentStoryLevelsBySetting.ContainsKey(config.id) || string.IsNullOrEmpty(currentStoryLevelsBySetting[config.id]))
		{
			currentStoryLevelsBySetting[config.id] = config.GetDefaultLevelId();
		}
	}

	public void SetStorySetting(SettingConfig config, string value)
	{
		SetStorySetting(config, value == "Guaranteed");
	}

	public void SetStorySetting(SettingConfig config, bool value)
	{
		currentStoryLevelsBySetting[config.id] = (value ? "Guaranteed" : "Disabled");
		if (this.OnStorySettingChanged != null)
		{
			this.OnStorySettingChanged(config, GetCurrentStoryTraitSetting(config));
		}
	}

	public void ParseAndApplyStoryTraitSettingsCode(string code)
	{
		BigInteger bigInteger = Base36toBinary(code);
		Dictionary<SettingConfig, string> dictionary = new Dictionary<SettingConfig, string>();
		foreach (string item in Util.Reverse(CoordinatedStorySettings))
		{
			SettingConfig settingConfig = StorySettings[item];
			if (settingConfig.coordinate_range == -1)
			{
				continue;
			}
			long num = (long)(bigInteger % settingConfig.coordinate_range);
			bigInteger /= (BigInteger)settingConfig.coordinate_range;
			foreach (SettingLevel level in settingConfig.GetLevels())
			{
				if (level.coordinate_value == num)
				{
					dictionary[settingConfig] = level.id;
					break;
				}
			}
		}
		foreach (KeyValuePair<SettingConfig, string> item2 in dictionary)
		{
			SetStorySetting(item2.Key, item2.Value);
		}
	}

	private string GetStoryTraitSettingsCode()
	{
		BigInteger input = 0;
		foreach (string coordinatedStorySetting in CoordinatedStorySettings)
		{
			SettingConfig settingConfig = StorySettings[coordinatedStorySetting];
			input *= (BigInteger)settingConfig.coordinate_range;
			input += (BigInteger)settingConfig.GetLevel(currentStoryLevelsBySetting[coordinatedStorySetting]).coordinate_value;
		}
		return BinarytoBase36(input);
	}

	public SettingLevel GetCurrentStoryTraitSetting(SettingConfig setting)
	{
		return GetCurrentStoryTraitSetting(setting.id);
	}

	public SettingLevel GetCurrentStoryTraitSetting(string settingId)
	{
		SettingConfig settingConfig = StorySettings[settingId];
		if (customGameMode == CustomGameMode.Survival && settingConfig.triggers_custom_game)
		{
			return settingConfig.GetLevel(settingConfig.GetDefaultLevelId());
		}
		if (customGameMode == CustomGameMode.Nosweat && settingConfig.triggers_custom_game)
		{
			return settingConfig.GetLevel(settingConfig.GetNoSweatDefaultLevelId());
		}
		if (!currentStoryLevelsBySetting.ContainsKey(settingId))
		{
			currentStoryLevelsBySetting[settingId] = StorySettings[settingId].GetDefaultLevelId();
		}
		string level_id = (DlcManager.IsAllContentSubscribed(settingConfig.required_content) ? currentStoryLevelsBySetting[settingId] : settingConfig.GetDefaultLevelId());
		return StorySettings[settingId].GetLevel(level_id);
	}

	public List<string> GetCurrentStories()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> item in currentStoryLevelsBySetting)
		{
			if (IsStoryActive(item.Key, item.Value))
			{
				list.Add(item.Key);
			}
		}
		return list;
	}

	public bool IsStoryActive(string id, string level)
	{
		if (!StorySettings.TryGetValue(id, out var value))
		{
			return false;
		}
		return value != null && level == "Guaranteed";
	}

	public void SetMixingSetting(SettingConfig config, string value)
	{
		SetMixingSetting(config, value, notify: true);
	}

	public void SetMixingSetting(SettingConfig config, string value, bool notify)
	{
		CurrentMixingLevelsBySetting[config.id] = value;
		if (notify && this.OnMixingSettingChanged != null)
		{
			this.OnMixingSettingChanged(config, GetCurrentMixingSettingLevel(config));
		}
	}

	public void AddMixingSettingsConfig(SettingConfig config)
	{
		MixingSettings.Add(config.id, config);
		if (!CurrentMixingLevelsBySetting.ContainsKey(config.id) || string.IsNullOrEmpty(CurrentMixingLevelsBySetting[config.id]))
		{
			CurrentMixingLevelsBySetting[config.id] = config.GetDefaultLevelId();
		}
	}

	public SettingLevel GetCurrentMixingSettingLevel(SettingConfig setting)
	{
		return GetCurrentMixingSettingLevel(setting.id);
	}

	public SettingConfig GetWorldMixingSettingForWorldgenFile(string file)
	{
		foreach (KeyValuePair<string, SettingConfig> mixingSetting in MixingSettings)
		{
			if (mixingSetting.Value is WorldMixingSettingConfig worldMixingSettingConfig && worldMixingSettingConfig.worldgenPath == file)
			{
				return mixingSetting.Value;
			}
		}
		return null;
	}

	public SettingConfig GetSubworldMixingSettingForWorldgenFile(string file)
	{
		foreach (KeyValuePair<string, SettingConfig> mixingSetting in MixingSettings)
		{
			if (mixingSetting.Value is SubworldMixingSettingConfig subworldMixingSettingConfig && subworldMixingSettingConfig.worldgenPath == file)
			{
				return mixingSetting.Value;
			}
		}
		return null;
	}

	public void DisableAllMixing()
	{
		foreach (SettingConfig value in MixingSettings.Values)
		{
			SetMixingSetting(value, value.GetDefaultLevelId());
		}
	}

	public List<SubworldMixingSettingConfig> GetActiveSubworldMixingSettings()
	{
		List<SubworldMixingSettingConfig> list = new List<SubworldMixingSettingConfig>();
		foreach (SettingConfig value in MixingSettings.Values)
		{
			if (value is SubworldMixingSettingConfig item && GetCurrentMixingSettingLevel(value).id != "Disabled")
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<WorldMixingSettingConfig> GetActiveWorldMixingSettings()
	{
		List<WorldMixingSettingConfig> list = new List<WorldMixingSettingConfig>();
		foreach (SettingConfig value in MixingSettings.Values)
		{
			if (value is WorldMixingSettingConfig item && GetCurrentMixingSettingLevel(value).id != "Disabled")
			{
				list.Add(item);
			}
		}
		return list;
	}

	public SettingLevel CycleMixingSettingLevel(ListSettingConfig config, int direction)
	{
		SetMixingSetting(config, config.CycleSettingLevelID(CurrentMixingLevelsBySetting[config.id], direction));
		return config.GetLevel(CurrentMixingLevelsBySetting[config.id]);
	}

	public SettingLevel ToggleMixingSettingLevel(ToggleSettingConfig config)
	{
		SetMixingSetting(config, config.ToggleSettingLevelID(CurrentMixingLevelsBySetting[config.id]));
		return config.GetLevel(CurrentMixingLevelsBySetting[config.id]);
	}

	public SettingLevel GetCurrentMixingSettingLevel(string settingId)
	{
		SettingConfig settingConfig = MixingSettings[settingId];
		if (!CurrentMixingLevelsBySetting.ContainsKey(settingId))
		{
			CurrentMixingLevelsBySetting[settingId] = MixingSettings[settingId].GetDefaultLevelId();
		}
		string level_id = (DlcManager.IsAllContentSubscribed(settingConfig.required_content) ? CurrentMixingLevelsBySetting[settingId] : settingConfig.GetDefaultLevelId());
		return MixingSettings[settingId].GetLevel(level_id);
	}

	public List<string> GetCurrentDlcMixingIds()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, SettingConfig> mixingSetting in MixingSettings)
		{
			if (mixingSetting.Value is DlcMixingSettingConfig dlcMixingSettingConfig && dlcMixingSettingConfig.IsOnLevel(GetCurrentMixingSettingLevel(dlcMixingSettingConfig.id).id))
			{
				list.Add(dlcMixingSettingConfig.id);
			}
		}
		return list;
	}

	public void ParseAndApplyMixingSettingsCode(string code)
	{
		BigInteger bigInteger = Base36toBinary(code);
		Dictionary<SettingConfig, string> dictionary = new Dictionary<SettingConfig, string>();
		foreach (string item in Util.Reverse(CoordinatedMixingSettings))
		{
			SettingConfig settingConfig = MixingSettings[item];
			if (settingConfig.coordinate_range == -1)
			{
				continue;
			}
			long num = (long)(bigInteger % settingConfig.coordinate_range);
			bigInteger /= (BigInteger)settingConfig.coordinate_range;
			foreach (SettingLevel level in settingConfig.GetLevels())
			{
				if (level.coordinate_value == num)
				{
					dictionary[settingConfig] = level.id;
					break;
				}
			}
		}
		foreach (KeyValuePair<SettingConfig, string> item2 in dictionary)
		{
			SetMixingSetting(item2.Key, item2.Value);
		}
	}

	private string GetMixingSettingsCode()
	{
		BigInteger input = 0;
		foreach (string coordinatedMixingSetting in CoordinatedMixingSettings)
		{
			SettingConfig settingConfig = MixingSettings[coordinatedMixingSetting];
			input *= (BigInteger)settingConfig.coordinate_range;
			input += (BigInteger)settingConfig.GetLevel(GetCurrentMixingSettingLevel(settingConfig).id).coordinate_value;
		}
		return BinarytoBase36(input);
	}

	public void RemoveInvalidMixingSettings()
	{
		ClusterLayout currentClusterLayout = GetCurrentClusterLayout();
		foreach (KeyValuePair<string, SettingConfig> mixingSetting in MixingSettings)
		{
			if (mixingSetting.Value is DlcMixingSettingConfig dlcMixingSettingConfig && currentClusterLayout.requiredDlcIds.Contains(dlcMixingSettingConfig.id))
			{
				SetMixingSetting(mixingSetting.Value, "Disabled");
			}
		}
		List<string> availableDlcs = GetCurrentDlcMixingIds();
		availableDlcs.AddRange(currentClusterLayout.requiredDlcIds);
		foreach (KeyValuePair<string, SettingConfig> mixingSetting2 in MixingSettings)
		{
			SettingConfig value = mixingSetting2.Value;
			SettingConfig settingConfig = value;
			if (!(settingConfig is WorldMixingSettingConfig worldMixingSettingConfig))
			{
				if (settingConfig is SubworldMixingSettingConfig subworldMixingSettingConfig && (!HasRequiredContent(subworldMixingSettingConfig.required_content) || currentClusterLayout.HasAnyTags(subworldMixingSettingConfig.forbiddenClusterTags)))
				{
					SetMixingSetting(mixingSetting2.Value, "Disabled");
				}
			}
			else if (!HasRequiredContent(worldMixingSettingConfig.required_content) || currentClusterLayout.HasAnyTags(worldMixingSettingConfig.forbiddenClusterTags))
			{
				SetMixingSetting(mixingSetting2.Value, "Disabled");
			}
		}
		bool HasRequiredContent(string[] requiredContent)
		{
			foreach (string text in requiredContent)
			{
				if (!(text == "") && !availableDlcs.Contains(text))
				{
					return false;
				}
			}
			return true;
		}
	}

	public ClusterLayout GetCurrentClusterLayout()
	{
		SettingLevel currentQualitySetting = Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.ClusterLayout);
		if (currentQualitySetting == null)
		{
			return null;
		}
		return SettingsCache.clusterLayouts.GetClusterData(currentQualitySetting.id);
	}

	public int GetCurrentWorldgenSeed()
	{
		SettingLevel currentQualitySetting = Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.WorldgenSeed);
		if (currentQualitySetting == null)
		{
			return 0;
		}
		return int.Parse(currentQualitySetting.id);
	}

	public void LoadClusters()
	{
		Dictionary<string, ClusterLayout> clusterCache = SettingsCache.clusterLayouts.clusterCache;
		List<SettingLevel> list = new List<SettingLevel>(clusterCache.Count);
		foreach (KeyValuePair<string, ClusterLayout> item in clusterCache)
		{
			StringEntry result;
			string label = (Strings.TryGet(new StringKey(item.Value.name), out result) ? result.ToString() : item.Value.name);
			string tooltip = (Strings.TryGet(new StringKey(item.Value.description), out result) ? result.ToString() : item.Value.description);
			list.Add(new SettingLevel(item.Key, label, tooltip, 0L));
		}
		CustomGameSettingConfigs.ClusterLayout.StompLevels(list, WorldGenSettings.ClusterDefaultName, WorldGenSettings.ClusterDefaultName);
	}

	public void Print()
	{
		string text = "Custom Settings: ";
		foreach (KeyValuePair<string, string> item in CurrentQualityLevelsBySetting)
		{
			text = text + item.Key + "=" + item.Value + ",";
		}
		Debug.Log(text);
		text = "Story Settings: ";
		foreach (KeyValuePair<string, string> item2 in currentStoryLevelsBySetting)
		{
			text = text + item2.Key + "=" + item2.Value + ",";
		}
		Debug.Log(text);
		text = "Mixing Settings: ";
		foreach (KeyValuePair<string, string> item3 in CurrentMixingLevelsBySetting)
		{
			text = text + item3.Key + "=" + item3.Value + ",";
		}
		Debug.Log(text);
	}

	private bool AllValuesMatch(Dictionary<string, string> data, CustomGameMode mode)
	{
		bool result = true;
		foreach (KeyValuePair<string, SettingConfig> qualitySetting in QualitySettings)
		{
			if (!(qualitySetting.Key == CustomGameSettingConfigs.WorldgenSeed.id))
			{
				string text = null;
				switch (mode)
				{
				case CustomGameMode.Nosweat:
					text = qualitySetting.Value.GetNoSweatDefaultLevelId();
					break;
				case CustomGameMode.Survival:
					text = qualitySetting.Value.GetDefaultLevelId();
					break;
				}
				if (data.ContainsKey(qualitySetting.Key) && data[qualitySetting.Key] != text)
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	public List<MetricSettingsData> GetSettingsForMetrics()
	{
		List<MetricSettingsData> list = new List<MetricSettingsData>();
		list.Add(new MetricSettingsData
		{
			Name = "CustomGameMode",
			Value = this.customGameMode.ToString()
		});
		foreach (KeyValuePair<string, string> item2 in CurrentQualityLevelsBySetting)
		{
			list.Add(new MetricSettingsData
			{
				Name = item2.Key,
				Value = item2.Value
			});
		}
		MetricSettingsData item = new MetricSettingsData
		{
			Name = "CustomGameModeActual",
			Value = CustomGameMode.Custom.ToString()
		};
		foreach (CustomGameMode value in Enum.GetValues(typeof(CustomGameMode)))
		{
			if (value == CustomGameMode.Custom || !AllValuesMatch(CurrentQualityLevelsBySetting, value))
			{
				continue;
			}
			item.Value = value.ToString();
			break;
		}
		list.Add(item);
		return list;
	}

	public List<MetricSettingsData> GetSettingsForMixingMetrics()
	{
		List<MetricSettingsData> list = new List<MetricSettingsData>();
		foreach (KeyValuePair<string, string> item in CurrentMixingLevelsBySetting)
		{
			if (MixingSettings.TryGetValue(item.Key, out var value) && DlcManager.IsAllContentSubscribed(value.required_content))
			{
				list.Add(new MetricSettingsData
				{
					Name = item.Key,
					Value = item.Value
				});
			}
		}
		return list;
	}

	public bool VerifySettingCoordinates()
	{
		bool flag = VerifySettingsDictionary(QualitySettings);
		bool flag2 = VerifySettingsDictionary(StorySettings);
		return flag || flag2;
	}

	private bool VerifySettingsDictionary(Dictionary<string, SettingConfig> configs)
	{
		bool result = false;
		foreach (KeyValuePair<string, SettingConfig> config in configs)
		{
			if (config.Value.coordinate_range < 0)
			{
				continue;
			}
			List<SettingLevel> levels = config.Value.GetLevels();
			if (config.Value.coordinate_range < levels.Count)
			{
				result = true;
				Debug.Assert(condition: false, config.Value.id + ": Range between coordinate min and max insufficient for all levels (" + config.Value.coordinate_range + "<" + levels.Count + ")");
			}
			foreach (SettingLevel item in levels)
			{
				Dictionary<long, string> dictionary = new Dictionary<long, string>();
				string text = config.Value.id + " > " + item.id;
				if (config.Value.coordinate_range <= item.coordinate_value)
				{
					result = true;
					Debug.Assert(condition: false, string.Format("%s: Level coordinate value (%u) exceedes range (%u)", text, item.coordinate_value, config.Value.coordinate_range));
				}
				if (item.coordinate_value < 0)
				{
					result = true;
					Debug.Assert(condition: false, text + ": Level coordinate value must be >= 0");
					continue;
				}
				if (item.coordinate_value == 0)
				{
					if (item.id != config.Value.GetDefaultLevelId())
					{
						result = true;
						Debug.Assert(condition: false, text + ": Only the default level should have a coordinate value of 0");
					}
					continue;
				}
				string value;
				bool flag = !dictionary.TryGetValue(item.coordinate_value, out value);
				dictionary[item.coordinate_value] = text;
				if (item.id == config.Value.GetDefaultLevelId())
				{
					result = true;
					Debug.Assert(condition: false, text + ": Default level must be a coordinate value of 0");
				}
				if (!flag)
				{
					result = true;
					Debug.Assert(condition: false, text + ": Combined coordinate conflicts with another coordinate (" + value + "). Ensure this SettingConfig's min and max don't overlap with another SettingConfig's");
				}
			}
		}
		return result;
	}

	public static string[] ParseSettingCoordinate(string coord)
	{
		Regex regex = new Regex("(.*)-(\\d*)-(.*)-(.*)-(.*)");
		Match match = regex.Match(coord);
		for (int i = 1; i <= 2; i++)
		{
			if (match.Groups.Count == 1)
			{
				regex = new Regex("(.*)-(\\d*)-(.*)-(.*)-(.*)".Remove("(.*)-(\\d*)-(.*)-(.*)-(.*)".Length - i * 5));
				match = regex.Match(coord);
			}
		}
		string[] array = new string[match.Groups.Count];
		for (int j = 0; j < match.Groups.Count; j++)
		{
			array[j] = match.Groups[j].Value;
		}
		return array;
	}

	public string GetSettingsCoordinate()
	{
		SettingLevel currentQualitySetting = Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.ClusterLayout);
		if (currentQualitySetting == null)
		{
			DebugUtil.DevLogError("GetSettingsCoordinate: clusterLayoutSetting is null, returning '0' coordinate");
			Instance.Print();
			Debug.Log("ClusterCache: " + string.Join(",", SettingsCache.clusterLayouts.clusterCache.Keys));
			return "0-0-0-0-0";
		}
		ClusterLayout clusterData = SettingsCache.clusterLayouts.GetClusterData(currentQualitySetting.id);
		SettingLevel currentQualitySetting2 = Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.WorldgenSeed);
		string otherSettingsCode = GetOtherSettingsCode();
		string storyTraitSettingsCode = GetStoryTraitSettingsCode();
		string mixingSettingsCode = GetMixingSettingsCode();
		return $"{clusterData.GetCoordinatePrefix()}-{currentQualitySetting2.id}-{otherSettingsCode}-{storyTraitSettingsCode}-{mixingSettingsCode}";
	}

	public void ParseAndApplySettingsCode(string code)
	{
		BigInteger bigInteger = Base36toBinary(code);
		Dictionary<SettingConfig, string> dictionary = new Dictionary<SettingConfig, string>();
		foreach (string item in Util.Reverse(CoordinatedQualitySettings))
		{
			if (!QualitySettings.ContainsKey(item))
			{
				continue;
			}
			SettingConfig settingConfig = QualitySettings[item];
			if (settingConfig.coordinate_range == -1)
			{
				continue;
			}
			long num = (long)(bigInteger % settingConfig.coordinate_range);
			bigInteger /= (BigInteger)settingConfig.coordinate_range;
			foreach (SettingLevel level in settingConfig.GetLevels())
			{
				if (level.coordinate_value == num)
				{
					dictionary[settingConfig] = level.id;
					break;
				}
			}
		}
		foreach (KeyValuePair<SettingConfig, string> item2 in dictionary)
		{
			SetQualitySetting(item2.Key, item2.Value);
		}
	}

	private string GetOtherSettingsCode()
	{
		BigInteger input = 0;
		foreach (string coordinatedQualitySetting in CoordinatedQualitySettings)
		{
			SettingConfig settingConfig = QualitySettings[coordinatedQualitySetting];
			input *= (BigInteger)settingConfig.coordinate_range;
			input += (BigInteger)settingConfig.GetLevel(GetCurrentQualitySetting(coordinatedQualitySetting).id).coordinate_value;
		}
		return BinarytoBase36(input);
	}

	private BigInteger Base36toBinary(string input)
	{
		if (input == "0")
		{
			return 0;
		}
		BigInteger bigInteger = 0;
		for (int num = input.Length - 1; num >= 0; num--)
		{
			bigInteger *= (BigInteger)36;
			long num2 = hexChars.IndexOf(input[num]);
			bigInteger += (BigInteger)num2;
		}
		DebugUtil.LogArgs("tried converting", input, ", got", bigInteger, "and returns to", BinarytoBase36(bigInteger));
		return bigInteger;
	}

	private string BinarytoBase36(BigInteger input)
	{
		if (input == 0L)
		{
			return "0";
		}
		BigInteger bigInteger = input;
		string text = "";
		for (; bigInteger > 0L; bigInteger /= (BigInteger)36)
		{
			text += hexChars[(int)(bigInteger % 36)];
		}
		return text;
	}
}
