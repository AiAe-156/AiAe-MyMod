using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using STRINGS;
using UnityEngine;

public static class LargeImpactorLandingSequence
{
	private const string SongName = "Stinger_Demolior_Falling";

	private const string IncomingSFX = "Asteroid_incoming_LP";

	private const string ImpactSFX = "Asteroid_explode";

	public static Coroutine Start(KMonoBehaviour controller, LargeComet comet, LargeImpactorCrashStamp stamp, int worldID)
	{
		return controller.StartCoroutine(Sequence(controller, comet, stamp, worldID));
	}

	private static IEnumerator Sequence(KMonoBehaviour controller, LargeComet comet, LargeImpactorCrashStamp stamp, int worldID)
	{
		yield return null;
		LargeImpactorVisualizer component = controller.GetComponent<LargeImpactorVisualizer>();
		Vector3 templatePosition = Grid.CellToPos(Grid.XYToCell(stamp.stampLocation.x, stamp.stampLocation.y));
		bool cometImpacted = false;
		LargeComet largeComet = comet;
		largeComet.OnImpact = (System.Action)Delegate.Combine(largeComet.OnImpact, (System.Action)delegate
		{
			cometImpacted = true;
		});
		if (SpeedControlScreen.Instance.IsPaused)
		{
			SpeedControlScreen.Instance.Unpause(playSound: false);
		}
		SpeedControlScreen.Instance.SetSpeed(0);
		RootMenu.Instance.canTogglePauseScreen = false;
		CameraController.Instance.DisableUserCameraControl = true;
		CameraController.Instance.SetWorldInteractive(state: false);
		AudioMixer.instance.Stop(AudioMixerSnapshots.Get().VictoryMessageSnapshot);
		ManagementMenu.Instance.CloseAll();
		StoryMessageScreen.HideInterface(hide: true);
		OverlayScreen.Instance.ToggleOverlay(OverlayModes.None.ID, allowSound: false);
		CameraController.Instance.SetOverrideZoomSpeed(0.6f);
		float templateWidth = component.RangeMax.x - component.RangeMin.x;
		float initialOrthogonalSize = templateWidth * 0.72f;
		float finalOrthogonalSize = templateWidth * 0.62f;
		yield return null;
		AudioMixer.instance.Start(Db.Get().ColonyAchievements.ReachedDistantPlanet.victoryNISSnapshot);
		MusicManager.instance.PlaySong("Stinger_Demolior_Falling");
		EventInstance incomingSFXInstance = KFMOD.BeginOneShot(GlobalAssets.GetSound("Asteroid_incoming_LP"), Vector3.zero);
		incomingSFXInstance.start();
		CameraController.Instance.SetMaxOrthographicSize(finalOrthogonalSize);
		CameraController.Instance.SetTargetPos(comet.transform.position, initialOrthogonalSize, playSound: false);
		yield return new WaitUntil(delegate
		{
			float num2 = ((comet == null) ? 1f : comet.LandingProgress);
			Vector3 pos = ((comet == null) ? templatePosition : (Grid.IsValidCellInWorld(Grid.PosToCell(comet.VisualPosition), worldID) ? comet.VisualPosition : comet.transform.position));
			float orthographic_size2 = Mathf.Lerp(initialOrthogonalSize, finalOrthogonalSize, (num2 <= 0f) ? 0f : Mathf.Pow(num2, 2f));
			CameraController.Instance.SetTargetPos(pos, orthographic_size2, playSound: false);
			return cometImpacted;
		});
		incomingSFXInstance.stop(STOP_MODE.IMMEDIATE);
		incomingSFXInstance.release();
		KFMOD.PlayUISound(GlobalAssets.GetSound("Asteroid_explode"));
		CameraController.Instance.FadeOutColor(Color.white);
		bool templateSpawned = false;
		TemplateLoader.Stamp(stamp.asteroidTemplate, stamp.stampLocation, delegate
		{
			templateSpawned = true;
		});
		List<WorldGenSpawner.Spawnable> unspawnedGeysers = new List<WorldGenSpawner.Spawnable>();
		foreach (WorldGenSpawner.Spawnable item in SaveGame.Instance.worldGenSpawner.GeInfoOfUnspawnedWithType<Geyser>(worldID))
		{
			unspawnedGeysers.Add(item);
		}
		yield return null;
		foreach (WorldGenSpawner.Spawnable item2 in SaveGame.Instance.worldGenSpawner.GetSpawnablesWithTag("GeyserGeneric", worldID))
		{
			unspawnedGeysers.Add(item2);
		}
		yield return null;
		yield return SequenceUtil.WaitForSecondsRealtime(1.8f);
		yield return new WaitUntil(() => templateSpawned);
		float orthographicSize = templateWidth * 0.3f;
		CameraController.Instance.SetPosition(templatePosition);
		CameraController.Instance.OrthographicSize = orthographicSize;
		float orthographic_size = templateWidth * 0.68f;
		CameraController.Instance.SetOverrideZoomSpeed(0.1f);
		CameraController.Instance.SetTargetPos(templatePosition, orthographic_size, playSound: false);
		bool fadeOutCompleted = false;
		CameraController.Instance.FadeInColor(Color.white, 0f, 1f, delegate
		{
			fadeOutCompleted = true;
		});
		yield return new WaitUntil(() => fadeOutCompleted);
		yield return SequenceUtil.WaitForSecondsRealtime(8f);
		MusicManager.instance.StopSong("Stinger_Demolior_Falling");
		AudioMixer.instance.Stop(Db.Get().ColonyAchievements.ReachedDistantPlanet.victoryNISSnapshot);
		StoryMessageScreen.HideInterface(hide: false);
		foreach (WorldGenSpawner.Spawnable item3 in unspawnedGeysers)
		{
			Vector2I vector2I = Grid.CellToXY(Grid.OffsetCell(item3.cell, 0, 2));
			GridVisibility.Reveal(vector2I.x, vector2I.y, 6, 1f);
			SimMessages.Dig(item3.cell);
		}
		yield return null;
		List<Geyser> geysers = Components.Geysers.GetItems(worldID);
		geysers.Sort(delegate(Geyser a, Geyser b)
		{
			float magnitude = (a.transform.position - templatePosition).magnitude;
			float magnitude2 = (b.transform.position - templatePosition).magnitude;
			return magnitude.CompareTo(magnitude2);
		});
		float geyserRevealTimer = 0f;
		int geyserCount = geysers.Count;
		Action<int, int> MakeGeyserRangeErupt = delegate(int from_notInclusive, int to)
		{
			for (int i = 0; i < geyserCount; i++)
			{
				if (i > from_notInclusive && i <= to)
				{
					Geyser geyser = geysers[i];
					UnentombGeyser(geyser);
					geyser.ShiftTimeTo(Geyser.TimeShiftStep.ActiveState, shouldSurviveSaveLoad: true);
					Game.Instance.SpawnFX(SpawnFXHashes.MeteorImpactMetal, new Vector3(geyser.transform.position.x, geyser.transform.position.y + 2f, geyser.transform.position.z - 0.1f), 0f);
					CreateGeyserEruptionNotification(geyser);
				}
			}
		};
		int lastGeyserIndexRevealed = -1;
		int num;
		while (geyserRevealTimer < 8f)
		{
			num = Mathf.FloorToInt(Mathf.Pow(geyserRevealTimer / 8f, 4f) * (float)geyserCount);
			MakeGeyserRangeErupt(lastGeyserIndexRevealed, num);
			lastGeyserIndexRevealed = num;
			geyserRevealTimer += Time.deltaTime;
			yield return null;
		}
		num = geyserCount;
		if (lastGeyserIndexRevealed != num)
		{
			MakeGeyserRangeErupt(lastGeyserIndexRevealed, num);
		}
		yield return null;
		RootMenu.Instance.canTogglePauseScreen = true;
		CameraController.Instance.DisableUserCameraControl = false;
		CameraController.Instance.SetOverrideZoomSpeed(1f);
		CameraController.Instance.SetMaxOrthographicSize(20f);
		CameraController.Instance.SetWorldInteractive(state: true);
		HoverTextScreen.Instance.Show();
		RootMenu.Instance.canTogglePauseScreen = true;
		CameraController.Instance.SetTargetPos(templatePosition, 20f, playSound: true);
		controller.Trigger(-467702038);
	}

	private static void CreateGeyserEruptionNotification(Geyser geyser)
	{
		Vector3 pos = geyser.transform.GetPosition();
		Notifier notifier = geyser.gameObject.AddOrGet<Notifier>();
		Notification notification = new Notification(MISC.NOTIFICATIONS.LARGE_IMPACTOR_GEYSER_ERUPTION.NAME, NotificationType.Bad, (List<Notification> notificationList, object data) => string.Concat(MISC.NOTIFICATIONS.LARGE_IMPACTOR_GEYSER_ERUPTION.TOOLTIP, notificationList.ReduceMessages(countNames: false)), "/t• " + notifier.GetProperName(), expires: false, 0f, delegate
		{
			GameUtil.FocusCamera(pos);
		}, null, null, volume_attenuation: true, clear_on_click: true);
		notifier.Add(notification);
	}

	private static void UnentombGeyser(Geyser geyser)
	{
		geyser.Unentomb();
		int cell = Grid.PosToCell(geyser);
		Vector3 vector = Grid.CellToPos(cell);
		int globalWorldSeed = SaveLoader.Instance.clusterDetailSave.globalWorldSeed;
		for (int i = -6; i < 6; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				int num = Grid.OffsetCell(cell, i, j);
				float magnitude = (Grid.CellToPos(num) - vector).magnitude;
				float num2 = (float)new KRandom(globalWorldSeed + num).Next() / 2.1474836E+09f;
				float num3 = Mathf.Clamp01(1f - (magnitude - 4f) / 2f);
				if ((magnitude < 4f || num2 <= 1f * num3) && Grid.IsSolidCell(num) && !Grid.Foundation[num] && Grid.Element[num].id != SimHashes.Unobtanium)
				{
					SimMessages.Dig(num);
				}
			}
		}
	}
}
