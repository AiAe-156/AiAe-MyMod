using System;
using UnityEngine;
using UnityEngine.UI;

public class DevQuickActionTargetFollower : MonoBehaviour
{
	public Toggle toggle;

	public RectTransform targetPivot;

	public RectTransform line;

	private ColorBlock toggleOnColorBlock;

	private ColorBlock toggleOffColorBlock;

	private GameObject Target;

	public Action<bool> OnToggleChanged;

	public new RectTransform transform => base.transform as RectTransform;

	public bool IsToggleOn => toggle.isOn;

	private void Awake()
	{
		toggleOffColorBlock = toggle.colors;
		toggleOnColorBlock = toggle.colors;
		toggleOnColorBlock.normalColor = toggleOffColorBlock.pressedColor;
		toggle.onValueChanged.AddListener(OnToggleValueChanged);
		toggle.SetIsOnWithoutNotify(value: true);
		RefreshToggleVisuals();
	}

	public void ManualToggle(bool val)
	{
		toggle.isOn = val;
	}

	public void OnToggleValueChanged(bool newValue)
	{
		RefreshToggleVisuals();
		OnToggleChanged?.Invoke(newValue);
	}

	public void RefreshToggleVisuals()
	{
		toggle.colors = (toggle.isOn ? toggleOnColorBlock : toggleOffColorBlock);
	}

	public void SetTarget(GameObject target)
	{
		Target = target;
	}

	private void Update()
	{
		Refresh();
	}

	public void Refresh()
	{
		if (Target != null)
		{
			Vector3 vector = CameraController.Instance.overlayCamera.WorldToScreenPoint(Target.transform.position);
			targetPivot.transform.SetPosition(vector);
			Vector3 localPosition = targetPivot.localPosition;
			localPosition.z = 0f;
			targetPivot.localPosition = localPosition;
			Vector3 vector2 = transform.position - vector;
			vector2.z = 0f;
			Vector3 upwards = Vector3.Cross(Vector3.forward, vector2.normalized);
			line.rotation = Quaternion.LookRotation(Vector3.forward, upwards);
			Vector2 sizeDelta = line.sizeDelta;
			sizeDelta.x = targetPivot.localPosition.magnitude;
			line.sizeDelta = sizeDelta;
		}
	}

	public void SetVisibleState(bool visible)
	{
		base.gameObject.SetActive(visible);
	}
}
