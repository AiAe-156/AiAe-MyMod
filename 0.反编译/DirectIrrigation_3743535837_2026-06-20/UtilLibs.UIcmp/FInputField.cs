using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UtilLibs.UIcmp;

public class FInputField : KScreen
{
	public InputField inputField;

	public ContentType contentType = (ContentType)4;

	private bool isEditing;

	public string Value => inputField.text;

	public event Action OnStartEdit;

	public event Action OnEndEdit;

	public event Action<string> OnValueChanged;

	public override void OnPrefabInit()
	{
		((KScreen)this).OnPrefabInit();
		inputField = ((Component)this).gameObject.GetComponent<InputField>();
	}

	public override void OnSpawn()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		((KScreen)this).OnSpawn();
		((UnityEvent<string>)(object)inputField.onEndEdit).AddListener((UnityAction<string>)OnEditEnd);
		((UnityEvent<string>)(object)inputField.onValueChanged).AddListener((UnityAction<string>)OnChangeValue);
		inputField.contentType = contentType;
	}

	private void OnChangeValue(string input)
	{
		if (!isEditing)
		{
			isEditing = true;
			KScreenManager.Instance.RefreshStack();
			this.OnStartEdit?.Invoke();
		}
		else
		{
			ProcessInput(input);
		}
		this.OnValueChanged?.DynamicInvoke(input);
	}

	private void OnEditEnd(string input)
	{
		((MonoBehaviour)this).StartCoroutine(DelayedEndEdit());
	}

	private IEnumerator DelayedEndEdit()
	{
		if (isEditing)
		{
			yield return (object)new WaitForEndOfFrame();
			StopEditing();
		}
	}

	public override void OnKeyDown(KButtonEvent e)
	{
		if (isEditing)
		{
			((KInputEvent)e).Consumed = true;
		}
	}

	private void StopEditing()
	{
		isEditing = false;
		inputField.DeactivateInputField();
		this.OnEndEdit?.Invoke();
	}

	protected virtual void ProcessInput(object input)
	{
		string text = input.ToString();
		if (!Util.IsNullOrWhiteSpace(text))
		{
			SetDisplayValue(text);
		}
	}

	public void SetDisplayValue(object input, bool triggerEdit = false)
	{
		if ((Object)(object)inputField != (Object)null)
		{
			inputField.text = input.ToString();
			if (triggerEdit)
			{
				this.OnEndEdit?.Invoke();
			}
		}
	}
}
