using System;
using UnityEngine;

public class TurboModeSideScreen : SideScreenContent
{
	public MultiToggle toggle;

	public LocText label;

	private SpaceHeater target;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		MultiToggle multiToggle = toggle;
		multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, new System.Action(OnClick));
	}

	private void Refresh()
	{
		toggle.ChangeState((target.UserSliderSetting != 0f) ? 1 : 0);
	}

	private void OnClick()
	{
		target.SetUserSpecifiedPowerConsumptionValue((target.UserSliderSetting == 0f) ? target.maxPower : target.minPower);
		Refresh();
	}

	public override bool IsValidForTarget(GameObject target)
	{
		SpaceHeater component = target.GetComponent<SpaceHeater>();
		if (component != null)
		{
			return component.heatLiquid;
		}
		return false;
	}

	public override void SetTarget(GameObject target)
	{
		base.SetTarget(target);
		if (target == null)
		{
			Debug.LogError("The target object provided was null");
			return;
		}
		this.target = target.GetComponent<SpaceHeater>();
		if (this.target == null)
		{
			Debug.LogError("The target provided does not have an ICheckboxControl component");
		}
		else
		{
			Refresh();
		}
	}

	public override void ClearTarget()
	{
		base.ClearTarget();
		target = null;
	}
}
