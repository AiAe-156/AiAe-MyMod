using System;
using System.Collections;
using PeterHan.PLib.Detours;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

internal sealed class PTextFieldEvents : KScreen
{
	private delegate void DeactivateInputField(TMP_InputField instance);

	private static readonly DeactivateInputField DEACTIVATE_INPUT = typeof(TMP_InputField).Detour<DeactivateInputField>();

	[MyCmpReq]
	private TMP_InputField textEntry;

	private bool editing;

	[field: SerializeField]
	internal PUIDelegates.OnTextChanged OnTextChanged { get; set; }

	[field: SerializeField]
	internal OnValidateInput OnValidate { get; set; }

	[field: SerializeField]
	internal GameObject TextObject { get; set; }

	internal PTextFieldEvents()
	{
		base.activateOnSpawn = true;
		editing = false;
		TextObject = null;
	}

	private IEnumerator DelayEndEdit()
	{
		yield return (object)new WaitForEndOfFrame();
		StopEditing();
	}

	public override float GetSortKey()
	{
		if (!editing)
		{
			return ((KScreen)this).GetSortKey();
		}
		return 99f;
	}

	protected override void OnCleanUp()
	{
		TMP_InputField obj = textEntry;
		obj.onFocus = (Action)Delegate.Remove(obj.onFocus, new Action(OnFocus));
		((UnityEvent<string>)(object)textEntry.onValueChanged).RemoveListener((UnityAction<string>)OnValueChanged);
		((UnityEvent<string>)(object)textEntry.onEndEdit).RemoveListener((UnityAction<string>)OnEndEdit);
		((KScreen)this).OnCleanUp();
	}

	protected override void OnSpawn()
	{
		((KScreen)this).OnSpawn();
		TMP_InputField obj = textEntry;
		obj.onFocus = (Action)Delegate.Combine(obj.onFocus, new Action(OnFocus));
		((UnityEvent<string>)(object)textEntry.onValueChanged).AddListener((UnityAction<string>)OnValueChanged);
		((UnityEvent<string>)(object)textEntry.onEndEdit).AddListener((UnityAction<string>)OnEndEdit);
		if (OnValidate != null)
		{
			textEntry.onValidateInput = OnValidate;
		}
	}

	private void OnEndEdit(string text)
	{
		GameObject gameObject = ((Component)this).gameObject;
		if ((Object)(object)gameObject != (Object)null)
		{
			OnTextChanged?.Invoke(gameObject, text);
			if (gameObject.activeInHierarchy)
			{
				((MonoBehaviour)this).StartCoroutine(DelayEndEdit());
			}
		}
	}

	private void OnFocus()
	{
		editing = true;
		((Selectable)textEntry).Select();
		textEntry.ActivateInputField();
		KScreenManager.Instance.RefreshStack();
	}

	public override void OnKeyDown(KButtonEvent e)
	{
		if (editing)
		{
			((KInputEvent)e).Consumed = true;
		}
		else
		{
			((KScreen)this).OnKeyDown(e);
		}
	}

	public override void OnKeyUp(KButtonEvent e)
	{
		if (editing)
		{
			((KInputEvent)e).Consumed = true;
		}
		else
		{
			((KScreen)this).OnKeyUp(e);
		}
	}

	private void OnValueChanged(string text)
	{
		if ((Object)(object)((Component)this).gameObject != (Object)null && (Object)(object)TextObject != (Object)null)
		{
			RectTransform val = Util.rectTransform(TextObject);
			val.SetSizeWithCurrentAnchors((Axis)1, LayoutUtility.GetPreferredHeight(val));
		}
	}

	private void StopEditing()
	{
		if ((Object)(object)textEntry != (Object)null && ((Component)textEntry).gameObject.activeInHierarchy)
		{
			DEACTIVATE_INPUT(textEntry);
		}
		editing = false;
	}
}
