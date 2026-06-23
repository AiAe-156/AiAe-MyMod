using TMPro;
using UnityEngine;

namespace FUtility.FUI;

public class FSettingsScreen : FScreen
{
	private Transform checkboxPanelPrefab;

	public FToggle2 AddCheckBox(string label)
	{
		Transform obj = Object.Instantiate<Transform>(checkboxPanelPrefab);
		FToggle2 fToggle = ((Component)obj).gameObject.AddComponent<FToggle2>();
		fToggle.SetCheckmark("Background/Checkmark");
		((TMP_Text)((Component)obj.Find("Label")).GetComponent<LocText>()).SetText(label);
		return fToggle;
	}

	public override void SetObjects()
	{
		checkboxPanelPrefab = ((KMonoBehaviour)this).transform.Find("Content/TogglePanel");
		base.SetObjects();
	}

	protected override void OnShow(bool show)
	{
		base.OnShow(show);
	}
}
