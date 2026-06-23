using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class CodexWidget<SubClass> : ICodexWidget, IHasDlcRestrictions
{
	public int preferredWidth { get; set; }

	public int preferredHeight { get; set; }

	public string[] requiredAtLeastOneDlcIds { get; set; }

	public string[] requiredDlcIds { get; set; }

	public string[] forbiddenDlcIds { get; set; }

	protected CodexWidget()
	{
		preferredWidth = -1;
		preferredHeight = -1;
	}

	protected CodexWidget(int preferredWidth, int preferredHeight)
	{
		this.preferredWidth = preferredWidth;
		this.preferredHeight = preferredHeight;
	}

	public abstract void Configure(GameObject contentGameObject, Transform displayPane, Dictionary<CodexTextStyle, TextStyleSetting> textStyles);

	protected void ConfigurePreferredLayout(GameObject contentGameObject)
	{
		LayoutElement componentInChildren = contentGameObject.GetComponentInChildren<LayoutElement>();
		componentInChildren.minWidth = preferredWidth;
		componentInChildren.minHeight = preferredHeight;
		componentInChildren.preferredHeight = preferredHeight;
		componentInChildren.preferredWidth = preferredWidth;
	}

	public string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public string[] GetForbiddenDlcIds()
	{
		return forbiddenDlcIds;
	}

	public string[] GetAnyRequiredDlcIds()
	{
		return requiredAtLeastOneDlcIds;
	}
}
