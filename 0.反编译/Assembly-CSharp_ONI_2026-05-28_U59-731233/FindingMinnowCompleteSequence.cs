using System;
using System.Collections;
using UnityEngine;

public static class FindingMinnowCompleteSequence
{
	public static void Start(KMonoBehaviour controller)
	{
		controller.StartCoroutine(Sequence());
	}

	private static IEnumerator Sequence()
	{
		bool videoCompleted = false;
		AudioMixer.instance.Start(AudioMixerSnapshots.Get().VictoryCinematicSnapshot);
		if (!SpeedControlScreen.Instance.IsPaused)
		{
			SpeedControlScreen.Instance.Pause(playSound: false);
		}
		VideoScreen screen = GameScreenManager.Instance.StartScreen(ScreenPrefabs.Instance.VideoScreen.gameObject).GetComponent<VideoScreen>();
		screen.PlayVideo(Assets.GetVideo(Db.Get().ColonyAchievements.MinnowRecruited.shortVideoName), unskippable: true, AudioMixerSnapshots.Get().VictoryCinematicSnapshot);
		screen.QueueVictoryVideoLoop(queue: true, Db.Get().ColonyAchievements.MinnowRecruited.messageBody, Db.Get().ColonyAchievements.MinnowRecruited.Id, Db.Get().ColonyAchievements.MinnowRecruited.loopVideoName);
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
	}
}
