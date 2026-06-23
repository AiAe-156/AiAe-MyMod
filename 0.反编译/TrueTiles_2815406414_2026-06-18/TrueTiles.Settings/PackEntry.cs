using System.IO;
using FUtility;
using FUtility.FUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TrueTiles.Settings;

public class PackEntry : KScreen
{
	public string Id;

	private LocText title;

	private LocText description;

	private Image icon;

	private FToggle2 enabledToggle;

	private FButton openFolderButton;

	public override void OnPrefabInit()
	{
		((KScreen)this).OnPrefabInit();
		Helper.ListChildren(((KMonoBehaviour)this).transform);
		title = ((Component)((KMonoBehaviour)this).transform.Find("Title/Text")).GetComponent<LocText>();
		icon = ((Component)((KMonoBehaviour)this).transform.Find("Icon/Image")).GetComponent<Image>();
		description = ((Component)((KMonoBehaviour)this).transform.Find("Info")).GetComponent<LocText>();
		enabledToggle = Util.FindOrAddComponent<FToggle2>((Component)(object)((KMonoBehaviour)this).transform.Find("Enabled"));
		enabledToggle.mark = ((Component)((KMonoBehaviour)enabledToggle).transform.Find("Background/Checkmark")).GetComponent<Image>();
		openFolderButton = Util.FindOrAddComponent<FButton>((Component)(object)((KMonoBehaviour)this).transform.Find("Buttons/Open"));
	}

	public void SetFolder(string path)
	{
		if (!Directory.Exists(path))
		{
			Log.Warning("Invalid path given to texture pack: " + path);
			openFolderButton.SetInteractable(interactable: false);
		}
		else
		{
			openFolderButton.OnClick += delegate
			{
				Application.OpenURL("file://" + path);
			};
		}
	}

	public void SetEnabled(bool enabled)
	{
		if ((Object)(object)enabledToggle == (Object)null)
		{
			Log.Warning("enabledtoggle is null");
		}
		else
		{
			enabledToggle.On = enabled;
		}
	}

	public void SetTooltip(string text)
	{
		StringEntry val = default(StringEntry);
		if (Strings.TryGet(text, ref val))
		{
			Helper.AddSimpleToolTip(((Component)title).gameObject, val.String);
		}
		else
		{
			Helper.AddSimpleToolTip(((Component)title).gameObject, text);
		}
	}

	public void SetTitle(string text)
	{
		StringEntry val = default(StringEntry);
		if (Strings.TryGet(text, ref val))
		{
			((TMP_Text)title).SetText(StringEntry.op_Implicit(val));
		}
		else
		{
			((TMP_Text)title).SetText(text);
		}
	}

	public bool IsEnabled()
	{
		return enabledToggle.On;
	}

	public void SetIcon(Texture2D texture)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)icon == (Object)null)
		{
			Log.Warning("null icon");
		}
		else
		{
			icon.sprite = Sprite.Create(texture, new Rect(0f, 0f, 48f, 48f), new Vector2(0.5f, 0.5f));
		}
	}

	public void SetDescription(string author, int textureCount)
	{
		((TMP_Text)description).SetText(string.Format(LocString.op_Implicit(STRINGS.TEXTUREPACKS.INFO), author, textureCount));
	}
}
