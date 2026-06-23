using System;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UtilLibs.UIcmp;

public class FInputField2 : KScreen, IInputHandler
{
	[MyCmpReq]
	public TMP_InputField inputField;

	[SerializeField]
	public string textPath = "Text";

	[SerializeField]
	public string placeHolderPath = "Placeholder";

	private bool allowInputs = true;

	private bool initialized;

	private bool DataTextUpdate = false;

	public bool AllowInputs => allowInputs;

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
				SgtLogger.debuglog("rehooking text input references");
				inputField.textComponent = (TMP_Text)(object)EntityTemplateExtensions.AddOrGet<LocText>(((Component)((Component)inputField.textViewport).transform.Find(textPath)).gameObject);
				inputField.placeholder = (Graphic)(object)EntityTemplateExtensions.AddOrGet<LocText>(((Component)((Component)inputField.textViewport).transform.Find(placeHolderPath)).gameObject);
				initialized = true;
			}
			SgtLogger.Assert("inputField", inputField);
			SgtLogger.Assert("textViewport", inputField.textViewport);
			SgtLogger.Assert("textcomponent", inputField.textComponent);
			SgtLogger.Assert("placeholder", inputField.placeholder);
			inputField.text = value;
		}
	}

	public OnChangeEvent OnValueChanged => inputField.onValueChanged;

	public static void Postfix(CameraController __instance, ref bool __result)
	{
		if (!__result)
		{
			EventSystem current = EventSystem.current;
			if (!((Object)(object)current == (Object)null) && !((Object)(object)current.currentSelectedGameObject == (Object)null) && (Object)(object)current.currentSelectedGameObject.GetComponent("FInputField2") != (Object)null)
			{
				__result = true;
			}
		}
	}

	static FInputField2()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		string text = "FInputField2";
		if (PRegistry.GetData<bool>(text))
		{
			return;
		}
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(CameraController), "WithinInputField", (Type[])null, (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(FInputField2), "Postfix", (Type[])null, (Type[])null);
			new Harmony(text).Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(methodInfo2), (HarmonyMethod)null, (HarmonyMethod)null);
			PRegistry.PutData(text, true);
		}
		catch (Exception ex)
		{
			SgtLogger.error("Caught error while patching CameraController.WithinInputField:\n" + ex.Message);
		}
	}

	public void SetTextFromData(string newText, bool forceRefresh = false)
	{
		DataTextUpdate = true;
		Text = newText;
		if (forceRefresh)
		{
			inputField.ForceLabelUpdate();
		}
		DataTextUpdate = false;
	}

	public bool IsEditing()
	{
		return ((KScreen)this).isEditing;
	}

	public void AddListener(Action<string> onValueChangedEvent)
	{
		((UnityEvent<string>)(object)inputField.onValueChanged).AddListener((UnityAction<string>)delegate(string e)
		{
			if (!DataTextUpdate)
			{
				onValueChangedEvent(e);
			}
		});
	}

	public override void OnPrefabInit()
	{
		((KScreen)this).OnPrefabInit();
	}

	public override void OnSpawn()
	{
		((KScreen)this).OnSpawn();
		TMP_InputField obj = inputField;
		obj.onFocus = (Action)Delegate.Combine(obj.onFocus, new Action(OnEditStart));
		((UnityEvent<string>)(object)inputField.onEndEdit).AddListener((UnityAction<string>)OnEditEnd);
		((KScreen)this).Activate();
	}

	public override void OnShow(bool show)
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

	public void ExternalStartEditing()
	{
		OnEditStart();
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
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between Unknown and I4
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
		if (e.TryConsume((Action)230) && (int)inputField.lineType == 0)
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

	public void SetInteractable(bool interactable)
	{
		allowInputs = interactable;
		if ((Object)(object)inputField != (Object)null)
		{
			((Selectable)inputField).interactable = allowInputs;
		}
	}
}
