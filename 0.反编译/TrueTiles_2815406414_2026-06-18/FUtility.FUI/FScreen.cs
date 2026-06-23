using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace FUtility.FUI;

public class FScreen : KScreen
{
	public const float SCREEN_SORT_KEY = 300f;

	private bool ConsumeMouseScroll = true;

	private bool shown;

	public bool pause = true;

	public FButton cancelButton;

	public FButton confirmButton;

	public FButton XButton;

	public FButton SteamButton;

	public FButton GithubButton;

	protected override void OnPrefabInit()
	{
		SetObjects();
		base.activateOnSpawn = true;
		((Component)this).gameObject.SetActive(true);
	}

	public virtual void SetObjects()
	{
		Transform val = ((KMonoBehaviour)this).transform.Find("SettingsDialogData");
		Text val2 = default(Text);
		if ((Object)(object)val != (Object)null && ((Component)val).gameObject.TryGetComponent<Text>(ref val2))
		{
			Dictionary<string, string> buttonRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(val2.text);
			cancelButton = SetButton("cancel", buttonRefs);
			confirmButton = SetButton("apply", buttonRefs);
			XButton = SetButton("close", buttonRefs);
			SteamButton = SetButton("steam", buttonRefs);
			GithubButton = SetButton("github", buttonRefs);
			Object.Destroy((Object)(object)((Component)val).gameObject);
		}
	}

	private FButton SetButton(string key, Dictionary<string, string> buttonRefs)
	{
		if (buttonRefs.TryGetValue(key, out var value))
		{
			Transform val = ((KMonoBehaviour)this).transform.Find(value);
			if ((Object)(object)val != (Object)null)
			{
				return ((Component)val).gameObject.AddComponent<FButton>();
			}
		}
		return null;
	}

	public virtual void ShowDialog()
	{
		if ((Object)(object)((Component)((KMonoBehaviour)this).transform.parent).GetComponent<Canvas>() == (Object)null && (Object)(object)((KMonoBehaviour)this).transform.parent.parent != (Object)null)
		{
			((KMonoBehaviour)this).transform.SetParent(((KMonoBehaviour)this).transform.parent.parent);
		}
		((KMonoBehaviour)this).transform.SetAsLastSibling();
		if ((Object)(object)cancelButton != (Object)null)
		{
			cancelButton.OnClick += OnClickCancel;
		}
		if ((Object)(object)XButton != (Object)null)
		{
			XButton.OnClick += OnClickCancel;
		}
		if ((Object)(object)confirmButton != (Object)null)
		{
			confirmButton.OnClick += OnClickApply;
		}
		if ((Object)(object)GithubButton != (Object)null)
		{
			GithubButton.OnClick += OnClickGithub;
		}
		if ((Object)(object)SteamButton != (Object)null)
		{
			SteamButton.OnClick += OnClickSteam;
		}
	}

	public void OnClickGithub()
	{
		Application.OpenURL("https://github.com/aki-art/ONI-Mods");
	}

	public void OnClickSteam()
	{
		Application.OpenURL("https://steamcommunity.com/id/akisnothere/myworkshopfiles/?appid=457140");
	}

	public virtual void OnClickCancel()
	{
		Reset();
		((KScreen)this).Deactivate();
	}

	public virtual void Reset()
	{
	}

	public virtual void OnClickApply()
	{
	}

	protected override void OnCmpEnable()
	{
		((KMonoBehaviour)this).OnCmpEnable();
		if ((Object)(object)CameraController.Instance != (Object)null)
		{
			CameraController.Instance.DisableUserCameraControl = true;
		}
	}

	protected override void OnCmpDisable()
	{
		((KMonoBehaviour)this).OnCmpDisable();
		if ((Object)(object)CameraController.Instance != (Object)null)
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

	protected override void OnActivate()
	{
		((KScreen)this).OnShow(true);
	}

	protected override void OnDeactivate()
	{
		((KScreen)this).OnShow(false);
	}

	protected override void OnShow(bool show)
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

	public override void OnKeyDown(KButtonEvent e)
	{
		if (e.TryConsume((Action)1))
		{
			OnClickCancel();
		}
		else
		{
			((KScreen)this).OnKeyDown(e);
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
