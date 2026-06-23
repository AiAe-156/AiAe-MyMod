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
		GameObject mainPrintingPod = GameUtil.GetTelepad(worldID);
		int centredCell;
		if (mainPrintingPod != null)
		{
			centredCell = Grid.PosToCell(mainPrintingPod);
		}
		else
		{
			Vector2 worldCentredPosition = (Vector2)world.WorldOffset * Grid.CellSizeInMeters;
			worldCentredPosition.x += (float)world.Width * Grid.CellSizeInMeters * 0.5f;
			worldCentredPosition.y += (float)world.Height * Grid.CellSizeInMeters * 0.5f;
			centredCell = Grid.PosToCell(worldCentredPosition);
		}
		int highestCell = Grid.XYToCell(Grid.CellToXY(centredCell).x, world.WorldOffset.y + world.Height);
		int cell = centredCell;
		_ = Grid.InvalidCell;
		int firstSkyCell = Grid.InvalidCell;
		while (firstSkyCell == Grid.InvalidCell && Grid.CellToXY(cell).y < world.WorldOffset.y + world.Height)
		{
			if (Grid.IsCellBiomeSpaceBiome(cell))
			{
				firstSkyCell = cell;
				break;
			}
			cell = Grid.CellAbove(cell);
		}
		int midSkyCell = Grid.XYToCell(Grid.CellToXY(centredCell).x, (int)((float)(Grid.CellToXY(highestCell).y + Grid.CellToXY(firstSkyCell).y) * 0.5f));
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
		MissileLauncher.Instance nearestMissileLauncher = null;
		float nearestDistance = float.MaxValue;
		Vector3 cameraPosition = CameraController.Instance.overlayCamera.transform.position;
		cameraPosition.z = 0f;
		foreach (MissileLauncher.Instance missileLauncher in Components.MissileLaunchers)
		{
			if (missileLauncher != null && missileLauncher.GetMyWorldId() == worldID)
			{
				Vector3 position = missileLauncher.transform.position;
				position.z = 0f;
				float distanceToCamera = (cameraPosition - position).magnitude;
				if (distanceToCamera < nearestDistance)
				{
					nearestDistance = distanceToCamera;
					nearestMissileLauncher = missileLauncher;
				}
			}
		}
		int keepsakeSpawnCell = Grid.InvalidCell;
		_ = Grid.InvalidCell;
		bool hasMissileLauncher = nearestMissileLauncher != null;
		int keepsakeCameraTargetCell;
		if (hasMissileLauncher)
		{
			keepsakeCameraTargetCell = Grid.PosToCell(nearestMissileLauncher.gameObject);
		}
		else
		{
			keepsakeSpawnCell = Grid.XYToCell(Grid.CellToXY(centredCell).x, world.WorldOffset.y + world.Height);
			keepsakeCameraTargetCell = keepsakeSpawnCell;
		}
		if (hasMissileLauncher)
		{
			int bestOffScreenCell = keepsakeCameraTargetCell;
			int maxY = CameraController.Instance.VisibleArea.CurrentArea.Max.Y;
			while (Grid.CellToXY(bestOffScreenCell).y < maxY)
			{
				int nextCell = Grid.CellAbove(bestOffScreenCell);
				if (Grid.IsValidCellInWorld(nextCell, worldID) && !Grid.Solid[nextCell])
				{
					bestOffScreenCell = nextCell;
					continue;
				}
				break;
			}
			keepsakeSpawnCell = bestOffScreenCell;
		}
		SpawnKeepsake(Grid.CellToPos(keepsakeSpawnCell));
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
		screen.PlayVideo(Assets.GetVideo(Db.Get().ColonyAchievements.AsteroidDestroyed.shortVideoName), unskippable: true, AudioMixerSnapshots.Get().VictoryNISGenericSnapshot);
		screen.QueueVictoryVideoLoop(queue: true, Db.Get().ColonyAchievements.AsteroidDestroyed.messageBody, Db.Get().ColonyAchievements.AsteroidDestroyed.Id, Db.Get().ColonyAchievements.AsteroidDestroyed.loopVideoName);
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
			StateMachine.Instance instance = new UpgradeFX.Instance(gameObject.GetComponent<KMonoBehaviour>(), new Vector3(0f, -0.5f, -0.1f));
			instance.StartSM();
		}
	}
}
