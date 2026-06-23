using System.Collections;
using UnityEngine;

public static class LargeImpactorRevealSequence
{
	private const string SongName = "Stinger_LargeImpactor_Reveal";

	public static Coroutine Start(KMonoBehaviour controller, LargeImpactorSequenceUIReticle reticle, WorldContainer world)
	{
		return controller.StartCoroutine(Sequence(controller, reticle, world));
	}

	private static IEnumerator Sequence(KMonoBehaviour controller, LargeImpactorSequenceUIReticle reticle, WorldContainer world)
	{
		LargeImpactorCrashStamp stamp = controller.GetComponent<LargeImpactorCrashStamp>();
		LargeImpactorVisualizer visualizer = controller.GetComponent<LargeImpactorVisualizer>();
		ParallaxBackgroundObject parallaxBackgroundObject = controller.GetComponent<ParallaxBackgroundObject>();
		float scaleMin = parallaxBackgroundObject.scaleMin;
		parallaxBackgroundObject.scaleMin = 0f;
		int landingZoneCentreCell = Grid.XYToCell(stamp.stampLocation.x, stamp.stampLocation.y);
		int midSkyCell = Grid.FindMidSkyCellAlignedWithCellInWorld(landingZoneCentreCell, world.id);
		Vector3 templatePosition = Grid.CellToPos(landingZoneCentreCell);
		if (!SpeedControlScreen.Instance.IsPaused)
		{
			SpeedControlScreen.Instance.Pause(playSound: false);
		}
		CameraController.Instance.SetWorldInteractive(state: false);
		AudioMixer.instance.Stop(AudioMixerSnapshots.Get().VictoryMessageSnapshot);
		ManagementMenu.Instance.CloseAll();
		StoryMessageScreen.HideInterface(hide: true);
		OverlayScreen.Instance.ToggleOverlay(OverlayModes.None.ID, allowSound: false);
		CameraController.Instance.SetOverrideZoomSpeed(0.6f);
		CameraController.Instance.SetTargetPos(Grid.CellToPos(midSkyCell), 20f, playSound: false);
		yield return null;
		RootMenu.Instance.canTogglePauseScreen = false;
		CameraController.Instance.DisableUserCameraControl = true;
		MusicManager.instance.PlaySong("Stinger_LargeImpactor_Reveal");
		do
		{
			yield return 0;
			parallaxBackgroundObject.scaleMin += Time.unscaledDeltaTime * 0.04f;
		}
		while (!(parallaxBackgroundObject.scaleMin >= 0.25f));
		bool reticleSequenceCompleted = false;
		bool reticlePhase1SequenceCompleted = false;
		reticle.Run(delegate
		{
			reticlePhase1SequenceCompleted = true;
		}, delegate
		{
			reticleSequenceCompleted = true;
			reticle.Hide();
		});
		yield return new WaitUntil(() => reticlePhase1SequenceCompleted);
		yield return SequenceUtil.WaitForSecondsRealtime(1f);
		float templateWidth = visualizer.RangeMax.x - visualizer.RangeMin.x;
		float orthogonalSize = templateWidth * 0.6f;
		CameraController.Instance.SetOverrideZoomSpeed(0.4f);
		CameraController.Instance.SetMaxOrthographicSize(orthogonalSize);
		CameraController.Instance.SetTargetPos(templatePosition, orthogonalSize, playSound: false);
		yield return SequenceUtil.WaitForSecondsRealtime(1f);
		visualizer.Active = true;
		visualizer.SetFoldedState(shouldBeFolded: false);
		visualizer.BeginEntryEffect(3.5f);
		KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Imperative_calculating"));
		yield return new WaitUntil(() => reticleSequenceCompleted);
		MusicManager.instance.StopSong("Stinger_LargeImpactor_Reveal");
		StoryMessageScreen.HideInterface(hide: false);
		RootMenu.Instance.canTogglePauseScreen = true;
		CameraController.Instance.DisableUserCameraControl = false;
		CameraController.Instance.SetOverrideZoomSpeed(1f);
		CameraController.Instance.SetMaxOrthographicSize(20f);
		CameraController.Instance.SetWorldInteractive(state: true);
		HoverTextScreen.Instance.Show();
		RootMenu.Instance.canTogglePauseScreen = true;
		CameraController.Instance.SetTargetPos(templatePosition, 20f, playSound: true);
		controller.Trigger(-467702038);
		if (SpeedControlScreen.Instance.IsPaused)
		{
			SpeedControlScreen.Instance.Unpause(playSound: false);
			SpeedControlScreen.Instance.SetSpeed(0);
		}
		parallaxBackgroundObject.scaleMin = scaleMin;
	}
}
