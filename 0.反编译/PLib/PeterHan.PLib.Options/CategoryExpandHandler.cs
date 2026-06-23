using PeterHan.PLib.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PeterHan.PLib.Options;

/// <summary>
/// Handles events for expanding and contracting options categories.
/// </summary>
internal sealed class CategoryExpandHandler
{
	/// <summary>
	/// The realized panel containing the options.
	/// </summary>
	private GameObject contents;

	/// <summary>
	/// The initial state of the button.
	/// </summary>
	private readonly bool initialState;

	/// <summary>
	/// The realized toggle button.
	/// </summary>
	private GameObject toggle;

	/// <summary>
	/// Creates a new options category.
	/// </summary>
	/// <param name="initialState">true to start expanded, or false to start collapsed.</param>
	public CategoryExpandHandler(bool initialState = true)
	{
		this.initialState = initialState;
	}

	/// <summary>
	/// Fired when the options category is expanded or contracted.
	/// </summary>
	/// <param name="on">true if the button is on, or false if it is off.</param>
	public void OnExpandContract(GameObject _, bool on)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localScale = (on ? Vector3.one : Vector3.zero);
		if ((Object)(object)contents != (Object)null)
		{
			RectTransform val = Util.rectTransform(contents);
			((Transform)val).localScale = localScale;
			if ((Object)(object)val != (Object)null)
			{
				LayoutRebuilder.MarkLayoutForRebuild(val);
			}
		}
	}

	/// <summary>
	/// Fired when the header is clicked.
	/// </summary>
	private void OnHeaderClicked()
	{
		if ((Object)(object)toggle != (Object)null)
		{
			bool toggleState = PToggle.GetToggleState(toggle);
			PToggle.SetToggleState(toggle, !toggleState);
		}
	}

	/// <summary>
	/// Fired when the category label is realized.
	/// </summary>
	/// <param name="header">The realized header label of the category.</param>
	public void OnRealizeHeader(GameObject header)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		Button obj = header.AddComponent<Button>();
		((UnityEvent)obj.onClick).AddListener(new UnityAction(OnHeaderClicked));
		((Selectable)obj).interactable = true;
	}

	/// <summary>
	/// Fired when the body is realized.
	/// </summary>
	/// <param name="panel">The realized body of the category.</param>
	public void OnRealizePanel(GameObject panel)
	{
		contents = panel;
		OnExpandContract(null, initialState);
	}

	/// <summary>
	/// Fired when the toggle button is realized.
	/// </summary>
	/// <param name="toggle">The realized expand/contract button.</param>
	public void OnRealizeToggle(GameObject toggle)
	{
		this.toggle = toggle;
	}
}
