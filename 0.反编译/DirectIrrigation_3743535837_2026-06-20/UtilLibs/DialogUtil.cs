using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UtilLibs;

public static class DialogUtil
{
	public static void CreateConfirmDialogFrontend(string title = null, string text = null, string confirm_text = null, Action on_confirm = null, string cancel_text = null, Action on_cancel = null, string configurable_text = null, Action on_configurable_clicked = null, Sprite image_sprite = null, bool useScreenSpaceOverlay = false, GameObject parent = null)
	{
		CreateConfirmDialog(title, text, confirm_text, on_confirm, cancel_text, on_cancel, configurable_text, on_configurable_clicked, image_sprite, frontend: true, useScreenSpaceOverlay, parent);
	}

	public static ConfirmDialogScreen CreateConfirmDialog(string title = null, string text = null, string confirm_text = null, Action on_confirm = null, string cancel_text = null, Action on_cancel = null, string configurable_text = null, Action on_configurable_clicked = null, Sprite image_sprite = null, bool frontend = false, bool useScreenSpaceOverlay = false, GameObject parent = null)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		if ((Object)(object)parent == (Object)null)
		{
			parent = ((frontend && !useScreenSpaceOverlay) ? Global.Instance.globalCanvas : GameScreenManager.Instance.GetParent((UIRenderTarget)2));
		}
		ConfirmDialogScreen val = (ConfirmDialogScreen)KScreenManager.Instance.StartScreen(((Component)ScreenPrefabs.Instance.ConfirmDialogScreen).gameObject, parent);
		if (!frontend)
		{
			((KMonoBehaviour)val).Subscribe(476357528, (Action<object>)delegate
			{
				CameraController.Instance.DisableUserCameraControl = true;
			});
		}
		val.PopupConfirmDialog(text, on_confirm, on_cancel, configurable_text, on_configurable_clicked, title, confirm_text, cancel_text, image_sprite);
		return val;
	}

	private static async Task ExecuteWithDelay(int ms, Action action)
	{
		await Task.Delay(ms);
		action();
	}

	public static FileNameDialog CreateTextInputDialog(string title, string startText = null, string fillerText = null, bool allowEmpty = false, Action<string> onConfirm = null, Action onCancel = null, GameObject parent = null, bool lockCam = true, bool unlockCam = true, bool frontEnd = false, int maxCharCount = 48, bool high = false, bool undoStripping = false)
	{
		if (startText == null)
		{
			startText = string.Empty;
		}
		GameObject val = (((Object)(object)parent != (Object)null) ? parent : GameScreenManager.Instance.GetParent((UIRenderTarget)2));
		FileNameDialog textDialog = Util.KInstantiateUI<FileNameDialog>(((Component)ScreenPrefabs.Instance.FileNameDialog).gameObject, val, false);
		((KMonoBehaviour)textDialog).transform.SetAsLastSibling();
		((Object)textDialog).name = Assembly.GetExecutingAssembly().GetName().Name + "_" + title;
		KInputTextField tmp = textDialog.inputField;
		((TMP_InputField)tmp).richText = false;
		((TMP_InputField)tmp).characterValidation = (CharacterValidation)0;
		((TMP_InputField)tmp).characterLimit = maxCharCount;
		((TMP_InputField)tmp).onValidateInput = null;
		((TMP_InputField)tmp).inputValidator = null;
		((TMP_InputField)tmp).contentType = (ContentType)0;
		((TMP_InputField)tmp).isRichTextEditingAllowed = false;
		if (undoStripping)
		{
			((UnityEventBase)((TMP_InputField)tmp).onValueChanged).RemoveAllListeners();
			((MonoBehaviour)textDialog).StartCoroutine(RemoveListenersDelayer());
		}
		if (fillerText != null)
		{
			FileNameDialog obj = textDialog;
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				KInputTextField inputField = obj.inputField;
				if (inputField == null)
				{
					obj2 = null;
				}
				else
				{
					Transform transform = ((Component)inputField).transform;
					if (transform == null)
					{
						obj2 = null;
					}
					else
					{
						Transform obj3 = transform.Find("Text Area/Placeholder");
						if (obj3 == null)
						{
							obj2 = null;
						}
						else
						{
							LocText component = ((Component)obj3).GetComponent<LocText>();
							obj2 = ((component != null) ? ((TMP_Text)component).text : null);
						}
					}
				}
			}
			string text = (string)obj2;
			if (text != null)
			{
				text = fillerText;
			}
		}
		if (lockCam && !frontEnd)
		{
			CameraController.Instance.DisableUserCameraControl = true;
		}
		TMP_InputField inputField2 = (TMP_InputField)(object)textDialog.inputField;
		KButton confirmButton = textDialog.confirmButton;
		if (!Util.IsNullOrWhiteSpace(startText))
		{
			textDialog.SetTextAndSelect(startText);
		}
		else
		{
			textDialog.SetTextAndSelect(string.Empty);
		}
		if (onConfirm != null)
		{
			FileNameDialog obj4 = textDialog;
			obj4.onConfirm = (Action<string>)Delegate.Combine(obj4.onConfirm, (Action<string>)delegate(string result)
			{
				if (result.EndsWith(".sav"))
				{
					result = result.Substring(0, result.Length - 4);
				}
				onConfirm(result);
			});
		}
		if (allowEmpty && textDialog.onConfirm != null)
		{
			confirmButton.onClick += delegate
			{
				if (inputField2.text.Length == 0)
				{
					textDialog.onConfirm(inputField2.text);
					((KScreen)textDialog).Deactivate();
				}
			};
		}
		if (onCancel != null)
		{
			FileNameDialog obj5 = textDialog;
			obj5.onCancel = (Action)Delegate.Combine(obj5.onCancel, onCancel);
		}
		if (!frontEnd)
		{
			if (unlockCam)
			{
				((KMonoBehaviour)textDialog).Subscribe(476357528, (Action<object>)delegate
				{
					CameraController.Instance.DisableUserCameraControl = false;
				});
			}
			else
			{
				((KMonoBehaviour)textDialog).Subscribe(476357528, (Action<object>)delegate
				{
					CameraController.Instance.DisableUserCameraControl = true;
				});
			}
		}
		Transform obj6 = ((KMonoBehaviour)textDialog).transform.Find("Panel");
		object obj7;
		if (obj6 == null)
		{
			obj7 = null;
		}
		else
		{
			Transform obj8 = obj6.Find("Title_BG");
			obj7 = ((obj8 != null) ? obj8.Find("Title") : null);
		}
		Transform val2 = (Transform)obj7;
		LocText val3 = default(LocText);
		if ((Object)(object)val2 != (Object)null && ((Component)val2).TryGetComponent<LocText>(ref val3))
		{
			((TMP_Text)val3).text = title;
		}
		return textDialog;
		IEnumerator RemoveListenersDelayer()
		{
			yield return null;
			yield return null;
			((UnityEventBase)((TMP_InputField)tmp).onValueChanged).RemoveAllListeners();
		}
	}
}
