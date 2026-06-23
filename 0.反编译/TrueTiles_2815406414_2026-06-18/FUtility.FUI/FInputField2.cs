using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FUtility.FUI;

public class FInputField2 : KScreen, IInputHandler
{
	[MyCmpReq]
	public TMP_InputField inputField;

	[SerializeField]
	public string textPath = "Text";

	[SerializeField]
	public string placeHolderPath = "Placeholder";

	private bool initialized;

	public string Text
	{
		get
		{
			return inputField.text;
		}
		set
		{
			if (!initialized)
			{
				inputField = ((Component)this).GetComponent<TMP_InputField>();
				if ((Object)(object)inputField == (Object)null)
				{
					Log.Warning("No inputfield on FInputField2");
				}
				inputField.textComponent = (TMP_Text)(object)EntityTemplateExtensions.AddOrGet<LocText>(((Component)((Component)inputField.textViewport).transform.Find(textPath)).gameObject);
				inputField.placeholder = (Graphic)(object)EntityTemplateExtensions.AddOrGet<LocText>(((Component)((Component)inputField.textViewport).transform.Find(placeHolderPath)).gameObject);
				initialized = true;
			}
			Log.Assert("inputField", inputField);
			Log.Assert("textViewport", inputField.textViewport);
			Log.Assert("textcomponent", inputField.textComponent);
			Log.Assert("placeholder", inputField.placeholder);
			inputField.text = value;
		}
	}

	public OnChangeEvent OnValueChanged => inputField.onValueChanged;

	public bool IsEditing()
	{
		return ((KScreen)this).isEditing;
	}

	protected override void OnPrefabInit()
	{
		((KScreen)this).OnPrefabInit();
		inputField = ((Component)this).GetComponent<TMP_InputField>();
	}

	protected override void OnSpawn()
	{
		((KScreen)this).OnSpawn();
		TMP_InputField obj = inputField;
		obj.onFocus = (Action)Delegate.Combine(obj.onFocus, new Action(OnEditStart));
		((UnityEvent<string>)(object)inputField.onEndEdit).AddListener((UnityAction<string>)OnEditEnd);
		((Behaviour)inputField).enabled = false;
		((Behaviour)inputField).enabled = true;
		((KScreen)this).Activate();
	}

	protected override void OnShow(bool show)
	{
		((KScreen)this).OnShow(show);
		if (show)
		{
			((KScreen)this).Activate();
			inputField.ActivateInputField();
		}
		else
		{
			((KScreen)this).Deactivate();
		}
	}

	public void Submit()
	{
		inputField.OnSubmit((BaseEventData)null);
	}

	private void OnEditEnd(string input)
	{
		((KScreen)this).isEditing = false;
		inputField.DeactivateInputField();
	}

	private void OnEditStart()
	{
		((KScreen)this).isEditing = true;
		((Selectable)inputField).Select();
		inputField.ActivateInputField();
		KScreenManager.Instance.RefreshStack();
	}

	public override void OnKeyDown(KButtonEvent e)
	{
		if (!((KScreen)this).isEditing)
		{
			((KScreen)this).OnKeyDown(e);
			return;
		}
		if (e.TryConsume((Action)1))
		{
			inputField.DeactivateInputField();
			((KInputEvent)e).Consumed = true;
			((KScreen)this).isEditing = false;
		}
		if (e.TryConsume((Action)230))
		{
			((KInputEvent)e).Consumed = true;
			inputField.OnSubmit((BaseEventData)null);
		}
		if (((KScreen)this).isEditing)
		{
			((KInputEvent)e).Consumed = true;
		}
		else if (!((KInputEvent)e).Consumed)
		{
			((KScreen)this).OnKeyDown(e);
		}
	}
}
