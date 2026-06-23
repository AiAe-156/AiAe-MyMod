using System;
using UnityEngine.UI;

public class DevQuickActionEndNode : DevQuickActionNode
{
	private Button button;

	protected void Awake()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(ButtonClicked);
	}

	private void ButtonClicked()
	{
		OnNodeInteractedWith?.Invoke();
	}

	public void Setup(string name, DevQuickActionNode parentNode, System.Action clickCB)
	{
		label.SetText(name);
		base.parentNode = parentNode;
		OnNodeInteractedWith = clickCB;
	}
}
