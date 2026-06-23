using System.Collections;
using Klei.AI;
using Klei.CustomSettings;
using ProcGen;
using STRINGS;
using UnityEngine;

public class MinionSelectScreen : CharacterSelectionController
{
	[SerializeField]
	private NewBaseScreen newBasePrefab;

	[SerializeField]
	private WattsonMessage wattsonMessagePrefab;

	public const string WattsonGameObjName = "WattsonMessage";

	public KButton backButton;

	protected override void OnPrefabInit()
	{
		base.IsStarterMinion = true;
		base.OnPrefabInit();
		if (MusicManager.instance.SongIsPlaying("Music_FrontEnd"))
		{
			MusicManager.instance.SetSongParameter("Music_FrontEnd", "songSection", 2f);
		}
		GameObject parent = GameObject.Find("ScreenSpaceOverlayCanvas");
		GameObject gameObject = Util.KInstantiateUI(wattsonMessagePrefab.gameObject, parent);
		gameObject.name = "WattsonMessage";
		gameObject.SetActive(value: false);
		Game.Instance.Subscribe(-1992507039, OnBaseAlreadyCreated);
		backButton.onClick += delegate
		{
			LoadScreen.ForceStopGame();
			App.LoadScene("frontend");
		};
		InitializeContainers();
		StartCoroutine(SetDefaultMinionsRoutine());
	}

	private IEnumerator SetDefaultMinionsRoutine()
	{
		yield return SequenceUtil.WaitForNextFrame;
		SettingLevel clusterPath = CustomGameSettings.Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.ClusterLayout);
		ClusterLayout cluster = SettingsCache.clusterLayouts.GetClusterData(clusterPath.id);
		bool aquaticStart = IsAquaticStartWorld(cluster);
		if (cluster.startingMinions != null)
		{
			DebugUtil.Assert(cluster.startingMinions.Length <= 3, "Cannot have more than 3 Minion presets");
			SetupMinion((CharacterContainer)containers[2], (cluster.startingMinions.Length != 0) ? cluster.startingMinions[0] : null);
			SetupMinion((CharacterContainer)containers[1], (cluster.startingMinions.Length > 1) ? cluster.startingMinions[1] : null);
			SetupMinion((CharacterContainer)containers[0], (cluster.startingMinions.Length > 2) ? cluster.startingMinions[2] : null);
		}
		void SetupMinion(CharacterContainer container, string specificMinion)
		{
			if (specificMinion != null)
			{
				container.SetMinion(new MinionStartingStats(Db.Get().Personalities.Get(specificMinion.ToUpper())));
			}
			else
			{
				container.GenerateCharacter(is_starter: true);
			}
			if (aquaticStart)
			{
				EnsureSwimmingSkill(container);
				container.OnReshuffled -= EnsureSwimmingSkill;
				container.OnReshuffled += EnsureSwimmingSkill;
			}
		}
	}

	private static bool IsAquaticStartWorld(ClusterLayout cluster)
	{
		if (cluster == null)
		{
			return false;
		}
		string startWorld = cluster.GetStartWorld();
		if (string.IsNullOrEmpty(startWorld))
		{
			return false;
		}
		ProcGen.World worldData = SettingsCache.worlds.GetWorldData(startWorld);
		return worldData != null && worldData.worldTags != null && worldData.worldTags.Contains("Aquatic");
	}

	private static void EnsureSwimmingSkill(CharacterContainer container)
	{
		if (container == null)
		{
			return;
		}
		MinionStartingStats stats = container.Stats;
		if (stats == null || stats.Traits == null || (stats.personality != null && stats.personality.model == GameTags.Minions.Models.Bionic))
		{
			return;
		}
		foreach (Trait trait2 in stats.Traits)
		{
			if (trait2 != null && (trait2.Id == "GrantSkill_Swimming" || trait2.Id == "GrantSkill_Swimming2"))
			{
				return;
			}
		}
		Trait trait = Db.Get().traits.TryGet("GrantSkill_Swimming");
		if (trait != null)
		{
			int index = ((stats.Traits.Count > 0) ? 1 : 0);
			stats.Traits.Insert(index, trait);
			container.SetMinion(stats);
		}
	}

	public void SetProceedButtonActive(bool state, string tooltip = null)
	{
		if (state)
		{
			EnableProceedButton();
		}
		else
		{
			DisableProceedButton();
		}
		ToolTip component = proceedButton.GetComponent<ToolTip>();
		if (component != null)
		{
			if (tooltip != null)
			{
				component.toolTip = tooltip;
			}
			else
			{
				component.ClearMultiStringTooltip();
			}
		}
	}

	protected override void OnSpawn()
	{
		OnDeliverableAdded();
		EnableProceedButton();
		proceedButton.GetComponentInChildren<LocText>().text = UI.IMMIGRANTSCREEN.EMBARK;
		containers.ForEach(delegate(ITelepadDeliverableContainer container)
		{
			CharacterContainer characterContainer = container as CharacterContainer;
			if (characterContainer != null)
			{
				characterContainer.DisableSelectButton();
			}
		});
	}

	protected override void OnProceed()
	{
		Util.KInstantiateUI(newBasePrefab.gameObject, GameScreenManager.Instance.ssOverlayCanvas);
		MusicManager.instance.StopSong("Music_FrontEnd");
		AudioMixer.instance.Start(AudioMixerSnapshots.Get().NewBaseSetupSnapshot);
		AudioMixer.instance.Stop(AudioMixerSnapshots.Get().FrontEndWorldGenerationSnapshot);
		int num = 0;
		selectedDeliverables.Clear();
		foreach (CharacterContainer container in containers)
		{
			selectedDeliverables.Add(container.Stats);
			if (container.Stats.personality.model == BionicMinionConfig.MODEL)
			{
				num++;
			}
		}
		NewBaseScreen.Instance.Init(SaveLoader.Instance.Cluster, selectedDeliverables.ToArray());
		if (OnProceedEvent != null)
		{
			OnProceedEvent();
		}
		if (Game.IsDlcActiveForCurrentSave("DLC3_ID") && Components.RoleStations.Count > 0)
		{
			BuildingFacade component = Components.RoleStations[0].GetComponent<BuildingFacade>();
			bool flag = !component.IsOriginal;
			if (num == 3 || (!flag && num > 0))
			{
				component.ApplyBuildingFacade(Db.GetBuildingFacades().Get("permit_hqbase_cyberpunk"));
			}
		}
		Game.Instance.Trigger(-838649377);
		BuildWatermark.Instance.gameObject.SetActive(value: false);
		Deactivate();
	}

	private void OnBaseAlreadyCreated(object data)
	{
		Game.Instance.StopFE();
		Game.Instance.StartBE();
		Game.Instance.SetGameStarted();
		Deactivate();
	}

	private void ReshuffleAll()
	{
		if (OnReshuffleEvent != null)
		{
			OnReshuffleEvent(base.IsStarterMinion);
		}
	}

	public override void OnPressBack()
	{
		foreach (ITelepadDeliverableContainer container in containers)
		{
			CharacterContainer characterContainer = container as CharacterContainer;
			if (characterContainer != null)
			{
				characterContainer.ForceStopEditingTitle();
			}
		}
	}
}
