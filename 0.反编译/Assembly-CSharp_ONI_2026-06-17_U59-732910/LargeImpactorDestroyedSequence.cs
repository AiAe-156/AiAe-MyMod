using System;
using System.Collections;
using UnityEngine;

public static class LargeImpactorDestroyedSequence
{
	private const string SongName = "Music_Victory_02_NIS";

	private const string Sound_Destroyed_Victory_Start_Sequence = "Asteroid_destroyed_start";

	public static Coroutine Start()
	{
		GameplayEventInstance gameplayEventInstance = GameplayEventManager.Instance.GetGameplayEventInstance(Db.Get().GameplayEvents.LargeImpactor.Id);
		if (gameplayEventInstance != null)
		{
			LargeImpactorEvent.StatesInstance statesInstance = (LargeImpactorEvent.StatesInstance)gameplayEventInstance.smi;
			if (statesInstance != null && statesInstance.impactorInstance != null)
			{
				LargeImpactorCrashStamp component = statesInstance.impactorInstance.GetComponent<LargeImpactorCrashStamp>();
				return component.StartCoroutine(Sequence(component, statesInstance.eventInstance.worldId));
			}
		}
		return null;
	}

	private static IEnumerator Sequence(KMonoBehaviour controller, int worldID)
	{
		yield return null;
		WorldContainer world = ClusterManager.Instance.GetWorld(worldID);
		ParallaxBackgroundObject parallaxBackgroundObj = controller.GetComponent<ParallaxBackgroundObject>();
		GameObject telepad = GameUtil.GetTelepad(worldID);
		int centredCell;
		if (telepad != null)
		{
			centredCell = Grid.PosToCell(telepad);
		}
		else
		{
			Vector2 pos = (Vector2)world.WorldOffset * Grid.CellSizeInMeters;
			pos.x += (float)world.Width * Grid.CellSizeInMeters * 0.5f;
			pos.y += (float)world.Height * Grid.CellSizeInMeters * 0.5f;
			centredCell = Grid.PosToCell(pos);
		}
		int cell = Grid.XYToCell(Grid.CellToXY(centredCell).x, world.WorldOffset.y + world.Height);
		int num = centredCell;
		_ = Grid.InvalidCell;
		int num2 = Grid.InvalidCell;
		while (num2 == Grid.InvalidCell && Grid.CellToXY(num).y < world.WorldOffset.y + world.Height)
		{
			if (Grid.IsCellBiomeSpaceBiome(num))
			{
				num2 = num;
				break;
			}
			num = Grid.CellAbove(num);
		}
		int midSkyCell = Grid.XYToCell(Grid.CellToXY(centredCell).x, (int)((float)(Grid.CellToXY(cell).y + Grid.CellToXY(num2).y) * 0.5f));
		if (!SpeedControlScreen.Instance.IsPaused)
		{
			SpeedControlScreen.Instance.Pause(playSound: false);
		}
		RootMenu.Instance.canTogglePauseScreen = false;
		CameraController.Instance.DisableUserCameraControl = true;
		CameraController.Instance.SetWorldInteractive(state: false);
		AudioMixer.instance.Stop(AudioMixerSnapshots.Get().VictoryMessageSnapshot);
		ManagementMenu.Instance.CloseAll();
		StoryMessageScreen.HideInterface(hide: true);
		OverlayScreen.Instance.ToggleOverlay(OverlayModes.None.ID, allowSound: false);
		CameraController.Instance.SetOverrideZoomSpeed(0.6f);
		yield return null;
		CameraController.Instance.FadeIn();
		AudioMixer.instance.Start(Db.Get().ColonyAchievements.ReachedDistantPlanet.victoryNISSnapshot);
		MusicManager.instance.PlaySong("Music_Victory_02_NIS");
		KFMOD.PlayUISound(GlobalAssets.GetSound("Asteroid_destroyed_start"));
		CameraController.Instance.SetTargetPos(Grid.CellToPos(midSkyCell), 20f, playSound: false);
		yield return SequenceUtil.WaitForSecondsRealtime(4f);
		parallaxBackgroundObj.PlayExplosion();
		yield return SequenceUtil.WaitForSecondsRealtime(2.2f);
		TerrainBG.preventLargeImpactorFragmentsFromProgressing = false;
		bool fadeOutCompleted = false;
		CameraController.Instance.FadeOutColor(Color.white, 0f, 1f, 1f, delegate
		{
			fadeOutCompleted = true;
		});
		yield return new WaitUntil(() => fadeOutCompleted);
		MissileLauncher.Instance instance = null;
		float num3 = float.MaxValue;
		Vector3 position = CameraController.Instance.overlayCamera.transform.position;
		position.z = 0f;
		foreach (MissileLauncher.Instance missileLauncher in Components.MissileLaunchers)
		{
			if (missileLauncher != null && missileLauncher.GetMyWorldId() == worldID)
			{
				Vector3 position2 = missileLauncher.transform.position;
				position2.z = 0f;
				float magnitude = (position - position2).magnitude;
				if (magnitude < num3)
				{
					num3 = magnitude;
					instance = missileLauncher;
				}
			}
		}
		int num4 = Grid.InvalidCell;
		_ = Grid.InvalidCell;
		bool num5 = instance != null;
		int num6;
		if (num5)
		{
			num6 = Grid.PosToCell(instance.gameObject);
		}
		else
		{
			num4 = Grid.XYToCell(Grid.CellToXY(centredCell).x, world.WorldOffset.y + world.Height);
			num6 = num4;
		}
		if (num5)
		{
			int num7 = num6;
			int y = CameraController.Instance.VisibleArea.CurrentArea.Max.Y;
			while (Grid.CellToXY(num7).y < y)
			{
				int num8 = Grid.CellAbove(num7);
				if (!Grid.IsValidCellInWorld(num8, worldID) || Grid.Solid[num8])
				{
					break;
				}
				num7 = num8;
			}
			num4 = num7;
		}
		SpawnKeepsake(Grid.CellToPos(num4));
		yield return SequenceUtil.WaitForSecondsRealtime(2f);
		MusicManager.instance.StopSong("Music_Victory_02_NIS");
		AudioMixer.instance.Stop(Db.Get().ColonyAchievements.ReachedDistantPlanet.victoryNISSnapshot);
		yield return null;
		bool videoCompleted = false;
		AudioMixer.instance.Start(AudioMixerSnapshots.Get().VictoryCinematicSnapshot);
		if (!SpeedControlScreen.Instance.IsPaused)
		{
			SpeedControlScreen.Instance.Pause(playSound: false);
		}
		VideoScreen screen = GameScreenManager.Instance.StartScreen(ScreenPrefabs.Instance.VideoScreen.gameObject).GetComponent<VideoScreen>();
		screen.PlayShortWithVictoryLoop(Db.Get().ColonyAchievements.AsteroidDestroyed.shortVideoName, Db.Get().ColonyAchievements.AsteroidDestroyed.messageBody, Db.Get().ColonyAchievements.AsteroidDestroyed.Id, Db.Get().ColonyAchievements.AsteroidDestroyed.loopVideoName, showAchievements: true, AudioMixerSnapshots.Get().VictoryNISGenericSnapshot);
		System.Action onVideoCompletedCallback = delegate
		{
			videoCompleted = true;
		};
		screen.OnStop = (System.Action)Delegate.Combine(screen.OnStop, onVideoCompletedCallback);
		yield return new WaitUntil(() => videoCompleted);
		screen.OnStop = (System.Action)Delegate.Remove(screen.OnStop, onVideoCompletedCallback);
		SpeedControlScreen.Instance.SetSpeed(0);
		CameraController.Instance.FadeIn();
		CameraController.Instance.SetOverrideZoomSpeed(1f);
		CameraController.Instance.SetWorldInteractive(state: true);
		CameraController.Instance.DisableUserCameraControl = false;
		CameraController.Instance.SetMaxOrthographicSize(20f);
		AudioMixer.instance.Stop(AudioMixerSnapshots.Get().VictoryCinematicSnapshot);
		AudioMixer.instance.Stop(AudioMixerSnapshots.Get().MuteDynamicMusicSnapshot);
		RootMenu.Instance.canTogglePauseScreen = true;
		HoverTextScreen.Instance.Show();
		StoryMessageScreen.HideInterface(hide: false);
		Game.Instance.Subscribe(-821118536, OnScreenClosed);
		controller.Trigger(-467702038);
	}

	private static void OnScreenClosed(object screenData)
	{
		if (screenData != null && screenData is RetiredColonyInfoScreen)
		{
			OnAchievementScreenClosed();
		}
	}

	private static void OnAchievementScreenClosed()
	{
		if (SpeedControlScreen.Instance != null && SpeedControlScreen.Instance.IsPaused)
		{
			SpeedControlScreen.Instance.Unpause(playSound: false);
			SpeedControlScreen.Instance.SetSpeed(0);
		}
		Game.Instance.Unsubscribe(-821118536, OnScreenClosed);
	}

	private static void SpawnKeepsake(Vector3 position)
	{
		GameObject prefab = Assets.GetPrefab("keepsake_largeimpactor");
		if (prefab != null)
		{
			position.z = Grid.GetLayerZ(Grid.SceneLayer.Ore);
			GameObject gameObject = Util.KInstantiate(prefab, position);
			gameObject.SetActive(value: true);
			new UpgradeFX.Instance(gameObject.GetComponent<KMonoBehaviour>(), new Vector3(0f, -0.5f, -0.1f)).StartSM();
		}
	}
}
