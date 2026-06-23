using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

/// <summary>
/// A class instance that handles events for text fields.
/// </summary>
internal sealed class PTextFieldEvents : KScreen
{
	[MyCmpReq]
	private TMP_InputField textEntry;

	/// <summary>
	/// Whether editing is in progress.
	/// </summary>
	private bool editing;

	/// <summary>
	/// The action to trigger on text change. It is passed the realized source object.
	/// </summary>
	[SerializeField]
	internal PUIDelegates.OnTextChanged OnTextChanged { get; set; }

	/// <summary>
	/// The callback to invoke when validating input.
	/// </summary>
	[SerializeField]
	internal OnValidateInput OnValidate { get; set; }

	/// <summary>
	/// The object to resize on text change.
	/// </summary>
	[SerializeField]
	internal GameObject TextObject { get; set; }

	internal PTextFieldEvents()
	{
		base.activateOnSpawn = true;
		editing = false;
		TextObject = null;
	}

	/// <summary>
	/// Completes the edit process one frame after the data is entered.
	/// </summary>
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

	/// <summary>
	/// Triggered when editing of the text ends (field loses focus).
	/// </summary>
	/// <param name="text">The text entered.</param>
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

	/// <summary>
	/// Triggered when the text field gains focus.
	/// </summary>
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

	/// <summary>
	/// Triggered when the text box value changes.
	/// </summary>
	/// <param name="text">The text entered.</param>
	private void OnValueChanged(string text)
	{
		if ((Object)(object)((Component)this).gameObject != (Object)null && (Object)(object)TextObject != (Object)null)
		{
			RectTransform val = Util.rectTransform(TextObject);
			val.SetSizeWithCurrentAnchors((Axis)1, LayoutUtility.GetPreferredHeight(val));
		}
	}

	/// <summary>
	/// Completes the edit process.
	/// </summary>
	private void StopEditing()
	{
		if ((Object)(object)textEntry != (Object)null && ((Component)textEntry).gameObject.activeInHierarchy)
		{
			textEntry.DeactivateInputField();
		}
		editing = false;
	}
}
