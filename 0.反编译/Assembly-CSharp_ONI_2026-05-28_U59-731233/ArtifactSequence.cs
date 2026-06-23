using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArtifactSequence
{
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
		AudioMixer.instance.Start(Db.Get().ColonyAchievements.CollectedArtifacts.victoryNISSnapshot);
		MusicManager.instance.PlaySong("Music_Victory_02_NIS");
		GameObject cameraTaget = null;
		foreach (Telepad pad in Components.Telepads)
		{
			if (pad != null)
			{
				cameraTaget = pad.gameObject;
			}
		}
		CameraController.Instance.FadeOut(1f, 2f);
		yield return SequenceUtil.WaitForSecondsRealtime(1f);
		CameraController.Instance.SetTargetPos(cameraTaget.transform.position, 10f, playSound: false);
		CameraController.Instance.SetOverrideZoomSpeed(10f);
		yield return SequenceUtil.WaitForSecondsRealtime(0.6f);
		if (SpeedControlScreen.Instance.IsPaused)
		{
			SpeedControlScreen.Instance.Unpause(playSound: false);
		}
		SpeedControlScreen.Instance.SetSpeed(1);
		CameraController.Instance.SetOverrideZoomSpeed(0.05f);
		CameraController.Instance.SetTargetPos(cameraTaget.transform.position, 20f, playSound: false);
		CameraController.Instance.FadeIn(0f, 2f);
		foreach (MinionIdentity minion in Components.LiveMinionIdentities)
		{
			if (minion != null)
			{
				minion.GetComponent<Facing>().Face(cameraTaget.transform.position.x);
				Db db = Db.Get();
				new EmoteChore(minion.GetComponent<ChoreProvider>(), db.ChoreTypes.EmoteHighPriority, db.Emotes.Minion.Cheer, 4);
			}
		}
		yield return SequenceUtil.WaitForSecondsRealtime(0.5f);
		yield return SequenceUtil.WaitForSecondsRealtime(3f);
		List<SpaceArtifact> selectedArtifacts = new List<SpaceArtifact>();
		foreach (SpaceArtifact candidateArtifact in Components.SpaceArtifacts)
		{
			if (candidateArtifact != null && candidateArtifact.HasTag(GameTags.Stored) && !candidateArtifact.HasTag(GameTags.CharmedArtifact))
			{
				bool addCandidate = true;
				foreach (SpaceArtifact compare in selectedArtifacts)
				{
					if (compare == candidateArtifact || (!(candidateArtifact.GetMyWorld() == compare.GetMyWorld()) && Grid.GetCellDistance(Grid.PosToCell(candidateArtifact), Grid.PosToCell(compare)) >= 10))
					{
						continue;
					}
					addCandidate = false;
					break;
				}
				if (addCandidate)
				{
					selectedArtifacts.Add(candidateArtifact);
				}
			}
			if (selectedArtifacts.Count >= 3)
			{
				break;
			}
		}
		if (selectedArtifacts.Count < 3)
		{
			foreach (SpaceArtifact candidateArtifact2 in Components.SpaceArtifacts)
			{
				if (selectedArtifacts.Contains(candidateArtifact2))
				{
					continue;
				}
				if (candidateArtifact2 != null && !candidateArtifact2.HasTag(GameTags.CharmedArtifact))
				{
					if (selectedArtifacts.Count == 0)
					{
						selectedArtifacts.Add(candidateArtifact2);
					}
					else
					{
						bool addCandidate2 = true;
						foreach (SpaceArtifact compare2 in selectedArtifacts)
						{
							if (compare2 == candidateArtifact2 || Grid.GetCellDistance(Grid.PosToCell(candidateArtifact2), Grid.PosToCell(compare2)) >= 10)
							{
								continue;
							}
							addCandidate2 = false;
							break;
						}
						if (addCandidate2)
						{
							selectedArtifacts.Add(candidateArtifact2);
						}
					}
				}
				if (selectedArtifacts.Count < 3)
				{
					continue;
				}
				break;
			}
		}
		foreach (SpaceArtifact artifact in selectedArtifacts)
		{
			GameObject cameraTarget = artifact.gameObject;
			CameraController.Instance.FadeOut(1f, 2f);
			yield return SequenceUtil.WaitForSecondsRealtime(1f);
			CameraController.Instance.SetTargetPos(cameraTarget.transform.position, 4f, playSound: false);
			CameraController.Instance.SetOverrideZoomSpeed(10f);
			yield return SequenceUtil.WaitForSecondsRealtime(0.5f);
			CameraController.Instance.FadeIn(0f, 2f);
			foreach (MinionIdentity minion2 in Components.LiveMinionIdentities)
			{
				if (minion2 != null)
				{
					minion2.GetComponent<Facing>().Face(cameraTarget.transform.position.x);
					Db db2 = Db.Get();
					ChoreProvider provider = minion2.GetComponent<ChoreProvider>();
					new EmoteChore(provider, db2.ChoreTypes.EmoteHighPriority, db2.Emotes.Minion.Cheer, 4);
				}
			}
			yield return SequenceUtil.WaitForSecondsRealtime(0.5f);
			CameraController.Instance.SetOverrideZoomSpeed(0.04f);
			CameraController.Instance.SetTargetPos(cameraTarget.transform.position, 8f, playSound: false);
			yield return SequenceUtil.WaitForSecondsRealtime(3f);
		}
		CameraController.Instance.FadeOut();
		MusicManager.instance.StopSong("Music_Victory_02_NIS");
		AudioMixer.instance.Stop(Db.Get().ColonyAchievements.CollectedArtifacts.victoryNISSnapshot);
		yield return SequenceUtil.WaitForSecondsRealtime(2f);
		AudioMixer.instance.Start(AudioMixerSnapshots.Get().VictoryCinematicSnapshot);
		if (!SpeedControlScreen.Instance.IsPaused)
		{
			SpeedControlScreen.Instance.Pause(playSound: false);
		}
		VideoScreen screen = GameScreenManager.Instance.StartScreen(ScreenPrefabs.Instance.VideoScreen.gameObject).GetComponent<VideoScreen>();
		screen.PlayVideo(Assets.GetVideo(Db.Get().ColonyAchievements.CollectedArtifacts.shortVideoName), unskippable: true, AudioMixerSnapshots.Get().VictoryCinematicSnapshot);
		screen.QueueVictoryVideoLoop(queue: true, Db.Get().ColonyAchievements.CollectedArtifacts.messageBody, Db.Get().ColonyAchievements.CollectedArtifacts.Id, Db.Get().ColonyAchievements.CollectedArtifacts.loopVideoName);
		screen.OnStop = (System.Action)Delegate.Combine(screen.OnStop, (System.Action)delegate
		{
			StoryMessageScreen.HideInterface(hide: false);
			CameraController.Instance.FadeIn();
			CameraController.Instance.SetWorldInteractive(state: true);
			CameraController.Instance.SetOverrideZoomSpeed(1f);
			HoverTextScreen.Instance.Show();
			AudioMixer.instance.Stop(AudioMixerSnapshots.Get().VictoryCinematicSnapshot);
			AudioMixer.instance.Stop(AudioMixerSnapshots.Get().MuteDynamicMusicSnapshot);
			RootMenu.Instance.canTogglePauseScreen = true;
		});
	}
}
