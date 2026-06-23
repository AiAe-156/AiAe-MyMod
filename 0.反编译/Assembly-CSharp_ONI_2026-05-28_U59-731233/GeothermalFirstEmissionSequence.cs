using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeothermalFirstEmissionSequence
{
	public static void Start(GeothermalController controller)
	{
		controller.StartCoroutine(Sequence(controller));
	}

	private static IEnumerator Sequence(GeothermalController controller)
	{
		List<GeothermalVent> vents = Components.GeothermalVents.GetItems(controller.GetMyWorldId());
		GeothermalVent vent = null;
		foreach (GeothermalVent ve in vents)
		{
			if (ve != null && ve.IsVentConnected() && ve.HasMaterial())
			{
				vent = ve;
				break;
			}
		}
		if (vent != null)
		{
			if (!SpeedControlScreen.Instance.IsPaused)
			{
				SpeedControlScreen.Instance.Pause(playSound: false);
			}
			CameraController.Instance.SetWorldInteractive(state: false);
			AudioMixer.instance.Stop(AudioMixerSnapshots.Get().VictoryMessageSnapshot);
			CameraController.Instance.FadeOut();
			yield return SequenceUtil.WaitForSecondsRealtime(1f);
			CameraController.Instance.SetTargetPos(vent.transform.position + Vector3.up * 3f, 10f, playSound: false);
			CameraController.Instance.SetOverrideZoomSpeed(10f);
			yield return SequenceUtil.WaitForSecondsRealtime(1f);
			CameraController.Instance.FadeIn();
			if (SpeedControlScreen.Instance.IsPaused)
			{
				SpeedControlScreen.Instance.Unpause(playSound: false);
			}
			SpeedControlScreen.Instance.SetSpeed(0);
		}
		yield return SequenceUtil.WaitForSecondsRealtime(1f);
		CameraController.Instance.SetWorldInteractive(state: true);
	}
}
