using UnityEngine;

namespace UtilLibs.UIcmp;

public class FScreen : KScreen
{
	public const float SCREEN_SORT_KEY = 300f;

	private bool ConsumeMouseScroll = true;

	private bool shown = false;

	public bool pause = true;

	public bool lockCam = true;

	public override void OnPrefabInit()
	{
		base.activateOnSpawn = true;
		((Component)this).gameObject.SetActive(true);
	}

	public virtual void ShowDialog()
	{
		if ((Object)(object)((Component)((KMonoBehaviour)this).transform.parent).GetComponent<Canvas>() == (Object)null && (Object)(object)((KMonoBehaviour)this).transform.parent.parent != (Object)null)
		{
			((KMonoBehaviour)this).transform.SetParent(((KMonoBehaviour)this).transform.parent.parent);
		}
		((KMonoBehaviour)this).transform.SetAsLastSibling();
	}

	public virtual void OnClickCancel()
	{
		SgtLogger.debuglog("cancel");
		Reset();
		((KScreen)this).Deactivate();
	}

	public virtual void Reset()
	{
	}

	public virtual void OnClickApply()
	{
	}

	public override void OnCmpEnable()
	{
		((KMonoBehaviour)this).OnCmpEnable();
		if (lockCam && (Object)(object)CameraController.Instance != (Object)null)
		{
			CameraController.Instance.DisableUserCameraControl = true;
		}
	}

	public override void OnCmpDisable()
	{
		((KMonoBehaviour)this).OnCmpDisable();
		if (lockCam && (Object)(object)CameraController.Instance != (Object)null)
		{
			CameraController.Instance.DisableUserCameraControl = false;
		}
		((KMonoBehaviour)this).Trigger(476357528, (object)null);
	}

	public override bool IsModal()
	{
		return true;
	}

	public override float GetSortKey()
	{
		return 300f;
	}

	public override void OnActivate()
	{
		((KScreen)this).OnShow(true);
	}

	public override void OnDeactivate()
	{
		((KScreen)this).OnShow(false);
	}

	public override void OnShow(bool show)
	{
		((KScreen)this).OnShow(show);
		if (pause && (Object)(object)SpeedControlScreen.Instance != (Object)null)
		{
			if (show && !shown)
			{
				SpeedControlScreen.Instance.Pause(false, false);
			}
			else if (!show && shown)
			{
				SpeedControlScreen.Instance.Unpause(false);
			}
			shown = show;
		}
	}

	public override void OnKeyUp(KButtonEvent e)
	{
		if (!((KInputEvent)e).Consumed)
		{
			KScrollRect componentInChildren = ((Component)this).GetComponentInChildren<KScrollRect>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				componentInChildren.OnKeyUp(e);
			}
		}
		((KInputEvent)e).Consumed = true;
	}
}
