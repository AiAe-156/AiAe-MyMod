using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevQuickActionCategoryNode : DevQuickActionNode
{
	public Sprite pressedSprite;

	public Sprite notPressedSprite;

	private Toggle toggle;

	protected List<DevQuickActionNode> childrenNodes = new List<DevQuickActionNode>();

	private ColorBlock originalColorBlock;

	private ColorBlock pressedColorBlock;

	private bool IsExpanded => toggle.isOn;

	protected void Awake()
	{
		toggle = GetComponent<Toggle>();
		originalColorBlock = toggle.colors;
		pressedColorBlock = toggle.colors;
		pressedColorBlock.normalColor = originalColorBlock.pressedColor;
		toggle.onValueChanged.AddListener(OnToggleValueChanged);
		RefreshVisuals();
	}

	public void Setup(string name, DevQuickActionNode parentNode)
	{
		label.SetText(name);
		base.parentNode = parentNode;
	}

	private void RefreshVisuals()
	{
		(toggle.targetGraphic as Image).sprite = (IsExpanded ? pressedSprite : notPressedSprite);
		toggle.colors = (IsExpanded ? pressedColorBlock : originalColorBlock);
	}

	private void OnToggleValueChanged(bool value)
	{
		RefreshVisuals();
		if (IsExpanded)
		{
			OnExpand();
		}
		else
		{
			OnCollapsed();
		}
		OnNodeInteractedWith?.Invoke();
	}

	public virtual void Expand()
	{
		toggle.isOn = true;
	}

	public void Collapse()
	{
		toggle.isOn = false;
	}

	private void OnExpand()
	{
		Vector2 v = Vector2.up;
		if (parentNode != null)
		{
			v = base.transform.anchoredPosition - parentNode.transform.anchoredPosition;
		}
		int count = childrenNodes.Count;
		float num = 180f / (float)(count + 1);
		for (int i = 0; i < count; i++)
		{
			DevQuickActionNode devQuickActionNode = childrenNodes[i];
			Vector2 vector = RotateVector2Clockwise(v, num * (float)i);
			Vector2 anchoredPosition = base.transform.anchoredPosition + vector.normalized * space;
			devQuickActionNode.transform.anchoredPosition = anchoredPosition;
			devQuickActionNode.gameObject.SetActive(value: true);
		}
	}

	private void OnCollapsed()
	{
		foreach (DevQuickActionNode childrenNode in childrenNodes)
		{
			if (childrenNode is DevQuickActionCategoryNode)
			{
				(childrenNode as DevQuickActionCategoryNode).Collapse();
			}
			childrenNode.gameObject.SetActive(value: false);
		}
	}

	public void AddChildren(DevQuickActionNode node)
	{
		if (!childrenNodes.Contains(node))
		{
			childrenNodes.Add(node);
		}
	}

	private Vector2 RotateVector2Clockwise(Vector2 v, float angleDegrees)
	{
		float f = angleDegrees * (MathF.PI / 180f);
		float num = Mathf.Cos(f);
		float num2 = Mathf.Sin(f);
		return new Vector2(v.x * num + v.y * num2, (0f - v.x) * num2 + v.y * num);
	}

	public override void Recycle()
	{
		foreach (DevQuickActionNode childrenNode in childrenNodes)
		{
			childrenNode.Recycle();
		}
		toggle.SetIsOnWithoutNotify(value: false);
		RefreshVisuals();
		base.Recycle();
		childrenNodes.Clear();
	}
}
