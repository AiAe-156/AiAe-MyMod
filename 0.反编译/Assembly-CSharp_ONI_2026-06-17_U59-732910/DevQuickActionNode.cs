using System;
using TMPro;
using UnityEngine;

public class DevQuickActionNode : MonoBehaviour
{
	public TextMeshProUGUI label;

	protected DevQuickActionNode parentNode;

	public Action<DevQuickActionNode> OnRecycle;

	protected System.Action OnNodeInteractedWith;

	protected float space = 100f;

	public new RectTransform transform => base.transform as RectTransform;

	public void SetChildrenSeparationSpace(float space)
	{
		this.space = space;
	}

	public virtual void Recycle()
	{
		parentNode = null;
		OnNodeInteractedWith = null;
		base.gameObject.SetActive(value: false);
		OnRecycle?.Invoke(this);
	}
}
