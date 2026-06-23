using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilLibs.UIcmp;

namespace UtilLibs.UI.FUI;

public class PasswordInputVisibilityToggle : KMonoBehaviour
{
	private FButton PasswordToggle;

	private Image SlashImage;

	private FInputField2 PasswordInput;

	public bool PasswordVisible = false;

	public void InitEyeToggle(FInputField2 input, string slashImagePath = "Slash")
	{
		PasswordInput = input;
		SlashImage = ((Component)((KMonoBehaviour)this).transform.Find(slashImagePath)).GetComponent<Image>();
		PasswordToggle = EntityTemplateExtensions.AddOrGet<FButton>(((Component)this).gameObject);
		PasswordToggle.OnClick += TogglePasswordVisibility;
		SetPasswordVisibility(passwordVisible: false);
	}

	public void TogglePasswordVisibility()
	{
		SetPasswordVisibility(!PasswordVisible);
	}

	public void SetPasswordVisibility(bool passwordVisible)
	{
		PasswordVisible = passwordVisible;
		((Component)SlashImage).gameObject.SetActive(!passwordVisible);
		PasswordInput.inputField.contentType = (ContentType)((!passwordVisible) ? 7 : 0);
		PasswordInput.inputField.ForceLabelUpdate();
	}
}
