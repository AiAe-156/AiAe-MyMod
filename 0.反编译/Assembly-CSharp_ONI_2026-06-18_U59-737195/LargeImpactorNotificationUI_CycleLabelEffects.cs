using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LargeImpactorNotificationUI_CycleLabelEffects : MonoBehaviour
{
	public ToolTip notificationTooltipComponent;

	public Image cyclesLabelBackground;

	public LocText numberOfCyclesLabel;

	private Coroutine cycleLabelFocusCoroutine;

	private float cycleFocusSpeed = 0.2f;

	private float cycleUnfocusSpeed = 0.4f;

	public void InitializeCycleLabelFocusMonitor()
	{
		AbortCycleLabelFocusMonitor();
		cycleLabelFocusCoroutine = StartCoroutine(CycleLabelFocusMonitor());
	}

	public void AbortCycleLabelFocusMonitor()
	{
		if (cycleLabelFocusCoroutine != null)
		{
			StopCoroutine(cycleLabelFocusCoroutine);
			cycleLabelFocusCoroutine = null;
		}
	}

	private IEnumerator CycleLabelFocusMonitor()
	{
		float previousVisibleValue = -1f;
		float visibleValue = 0f;
		while (true)
		{
			visibleValue = Mathf.Clamp(visibleValue + Time.unscaledDeltaTime / (notificationTooltipComponent.isHovering ? cycleFocusSpeed : cycleUnfocusSpeed) * (float)(notificationTooltipComponent.isHovering ? 1 : (-1)), 0f, 1f);
			if (visibleValue != previousVisibleValue)
			{
				previousVisibleValue = visibleValue;
				cyclesLabelBackground.Opacity(visibleValue);
				numberOfCyclesLabel.Opacity(visibleValue);
			}
			yield return null;
		}
	}
}
