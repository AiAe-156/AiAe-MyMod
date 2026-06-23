using System;
using System.Collections;
using UnityEngine;

public static class EnterTemporalTearSequence
{
	public static GameObject tearOpenerGameObject;

	public static void Start(KMonoBehaviour controller)
	{
		controller.StartCoroutine(Sequence());
	}

	private static IEnumerator Sequence()
	{
		if (!SpeedControlScreen.Instance.IsPaused)
		{
			SpeedControlScreen.Instance.Pause(playSound: false);
		}
		CameraController.Instance.SetWorldInteractive(state: false);
		AudioMixer.instance.Stop(AudioMixerSnapshots.Get().VictoryMessageSnapshot);
		CameraController.Instance.FadeOut();
		yield return SequenceUtil.WaitForSecondsRealtime(3f);
		ManagementMenu.Instance.CloseAll();
		AudioMixer.instance.Start(Db.Get().ColonyAchievements.ReachedDistantPlanet.victoryNISSnapshot);
		MusicManager.instance.PlaySong("Music_Victory_02_NIS");
		Vector3 cameraBiasUp = Vector3.up * 5f;
		GameObject cameraTaget = tearOpenerGameObject;
		if (cameraTaget != null)
		{
			CameraController.Instance.SetTargetPos(cameraTaget.transform.position + cameraBiasUp, 10f, playSound: false);
			CameraController.Instance.SetOverrideZoomSpeed(10f);
			yield return SequenceUtil.WaitForSecondsRealtime(0.4f);
			if (SpeedControlScreen.Instance.IsPaused)
			{
				SpeedControlScreen.Instance.Unpause(playSound: false);
			}
			SpeedControlScreen.Instance.SetSpeed(1);
			CameraController.Instance.SetOverrideZoomSpeed(0.1f);
			CameraController.Instance.SetTargetPos(cameraTaget.transform.position + cameraBiasUp, 20f, playSound: false);
			CameraController.Instance.FadeIn(0f, 2f);
			foreach (MinionIdentity minion in Components.LiveMinionIdentities)
			{
				if (minion != null)
				{
					minion.GetComponent<Facing>().Face(cameraTaget.transform.position.x);
					Db db = Db.Get();
					ChoreProvider provider = minion.GetComponent<ChoreProvider>();
					new EmoteChore(provider, db.ChoreTypes.EmoteHighPriority, db.Emotes.Minion.Cheer, 2);
				}
			}
			yield return SequenceUtil.WaitForSecondsRealtime(0.5f);
			yield return SequenceUtil.WaitForSecondsRealtime(1.5f);
			CameraController.Instance.FadeOut();
			yield return SequenceUtil.WaitForSecondsRealtime(1.5f);
		}
		foreach (Telepad pad in Components.Telepads)
		{
			if (!(pad != null))
			{
				continue;
			}
			GameObject cameraTaget2 = pad.gameObject;
			CameraController.Instance.SetTargetPos(cameraTaget2.transform.position, 10f, playSound: false);
			CameraController.Instance.SetOverrideZoomSpeed(10f);
			yield return SequenceUtil.WaitForSecondsRealtime(0.4f);
			if (SpeedControlScreen.Instance.IsPaused)
			{
				SpeedControlScreen.Instance.Unpause(playSound: false);
			}
			SpeedControlScreen.Instance.SetSpeed(1);
			CameraController.Instance.SetOverrideZoomSpeed(0.05f);
			CameraController.Instance.SetTargetPos(cameraTaget2.transform.position, 20f, playSound: false);
			CameraController.Instance.FadeIn(0f, 2f);
			foreach (MinionIdentity minion2 in Components.LiveMinionIdentities)
			{
				if (minion2 != null)
				{
					minion2.GetComponent<Facing>().Face(cameraTaget2.transform.position.x);
					Db db2 = Db.Get();
					ChoreProvider provider2 = minion2.GetComponent<ChoreProvider>();
					new EmoteChore(provider2, db2.ChoreTypes.EmoteHighPriority, db2.Emotes.Minion.Cheer, 2);
				}
			}
			yield return SequenceUtil.WaitForSecondsRealtime(0.5f);
			yield return SequenceUtil.WaitForSecondsRealtime(1.5f);
			CameraController.Instance.FadeOut();
			yield return SequenceUtil.WaitForSecondsRealtime(1.5f);
		}
		MusicManager.instance.StopSong("Music_Victory_02_NIS");
		AudioMixer.instance.Stop(Db.Get().ColonyAchievements.ReachedDistantPlanet.victoryNISSnapshot);
		yield return SequenceUtil.WaitForSecondsRealtime(2f);
		AudioMixer.instance.Start(AudioMixerSnapshots.Get().VictoryCinematicSnapshot);
		if (!SpeedControlScreen.Instance.IsPaused)
		{
			SpeedControlScreen.Instance.Pause(playSound: false);
		}
		VideoScreen screen = GameScreenManager.Instance.StartScreen(ScreenPrefabs.Instance.VideoScreen.gameObject).GetComponent<VideoScreen>();
		screen.PlayVideo(Assets.GetVideo(Db.Get().ColonyAchievements.ReachedDistantPlanet.shortVideoName), unskippable: true, AudioMixerSnapshots.Get().VictoryCinematicSnapshot);
		screen.QueueVictoryVideoLoop(queue: true, Db.Get().ColonyAchievements.ReachedDistantPlanet.messageBody, Db.Get().ColonyAchievements.ReachedDistantPlanet.Id, Db.Get().ColonyAchievements.ReachedDistantPlanet.loopVideoName);
		screen.OnStop = (System.Action)Delegate.Combine(screen.OnStop, (System.Action)delegate
		{
			StoryMessageScreen.HideInterface(hide: false);
			CameraController.Instance.FadeIn();
			CameraController.Instance.SetWorldInteractive(state: true);
			HoverTextScreen.Instance.Show();
			CameraController.Instance.SetOverrideZoomSpeed(1f);
			AudioMixer.instance.Stop(AudioMixerSnapshots.Get().VictoryCinematicSnapshot);
			AudioMixer.instance.Stop(AudioMixerSnapshots.Get().MuteDynamicMusicSnapshot);
			RootMenu.Instance.canTogglePauseScreen = true;
		});
	}
}
