using System;
using System.Collections.Generic;
using UnityEngine;

public class DevQuickActionsScreen : MonoBehaviour
{
	public const float DEFAULT_SPACE = 60f;

	public const float ROOT_SPACE = 50f;

	public const char CATEGORY_DIVIDER = '/';

	public DevQuickActionNode originalCategoryDevNode;

	public DevQuickActionNode originalEndNode;

	public DevQuickActionTargetFollower Pointer;

	public Stack<DevQuickActionEndNode> recycledEndNodes = new Stack<DevQuickActionEndNode>();

	public Stack<DevQuickActionCategoryNode> recycledCategoriesNodes = new Stack<DevQuickActionCategoryNode>();

	private Dictionary<string, DevQuickActionCategoryNode> registeredCategoryNodes = new Dictionary<string, DevQuickActionCategoryNode>();

	private GameObject Target;

	private DevQuickActionCategoryNode RootNode;

	public static DevQuickActionsScreen Instance;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	private void Awake()
	{
		Instance = this;
		Pointer.SetVisibleState(visible: false);
		DevQuickActionTargetFollower pointer = Pointer;
		pointer.OnToggleChanged = (Action<bool>)Delegate.Combine(pointer.OnToggleChanged, new Action<bool>(OnPointerToggleClicked));
		originalEndNode.gameObject.SetActive(value: false);
		originalCategoryDevNode.gameObject.SetActive(value: false);
	}

	private void OnPointerToggleClicked(bool val)
	{
		if (Target != null && RootNode != null)
		{
			if (val)
			{
				RootNode.Expand();
			}
			else
			{
				RootNode.Collapse();
			}
		}
	}

	public void Toggle(GameObject target)
	{
		if (target == null)
		{
			Close();
		}
		else if (Target != target)
		{
			Open(target);
		}
		else
		{
			Close();
		}
	}

	public void Open(GameObject target)
	{
		if (Target != null && Target != target)
		{
			Close();
		}
		Target = target;
		if (target == null)
		{
			return;
		}
		Vector3 position = CameraController.Instance.overlayCamera.WorldToScreenPoint(target.transform.position);
		RootNode = GetUnsedCategoryNode();
		RootNode.Setup(target.GetProperName(), null);
		RootNode.transform.SetPosition(position);
		RootNode.SetChildrenSeparationSpace(50f);
		Target.Subscribe(1502190696, OnTargetLost);
		List<IDevQuickAction> list = new List<IDevQuickAction>(Target.GetComponents<IDevQuickAction>());
		list.AddRange(Target.GetAllSMI<IDevQuickAction>());
		foreach (IDevQuickAction item in list)
		{
			foreach (DevQuickActionInstruction devInstruction in item.GetDevInstructions())
			{
				string[] array = devInstruction.Address.Split('/');
				DevQuickActionCategoryNode devQuickActionCategoryNode = RootNode;
				for (int i = 0; i < array.Length; i++)
				{
					string key = array[i];
					if (i < array.Length - 1)
					{
						DevQuickActionCategoryNode value = null;
						if (!registeredCategoryNodes.TryGetValue(key, out value))
						{
							value = GetUnsedCategoryNode();
							value.Setup(key, devQuickActionCategoryNode);
							registeredCategoryNodes.Add(key, value);
							devQuickActionCategoryNode.AddChildren(value);
						}
						devQuickActionCategoryNode = value;
					}
					else
					{
						DevQuickActionEndNode unsedEndNode = GetUnsedEndNode();
						unsedEndNode.Setup(key, devQuickActionCategoryNode, devInstruction.Action);
						devQuickActionCategoryNode.AddChildren(unsedEndNode);
						unsedEndNode.gameObject.SetActive(value: false);
					}
				}
			}
		}
		RootNode.Collapse();
		if (Pointer.IsToggleOn)
		{
			RootNode.Expand();
		}
		RootNode.gameObject.SetActive(value: false);
		Pointer.transform.position = RootNode.transform.position;
		Pointer.SetTarget(Target);
		Pointer.SetVisibleState(visible: true);
	}

	public void Close()
	{
		if (Target != null)
		{
			Target.Unsubscribe(1502190696, OnTargetLost);
		}
		Target = null;
		if (RootNode != null)
		{
			RootNode.Recycle();
			RootNode = null;
		}
		registeredCategoryNodes.Clear();
		Pointer.SetTarget(null);
		Pointer.SetVisibleState(visible: false);
	}

	private void OnTargetLost(object o)
	{
		Close();
	}

	private DevQuickActionEndNode GetUnsedEndNode()
	{
		DevQuickActionEndNode result = null;
		if (!recycledEndNodes.TryPop(out result))
		{
			result = Util.KInstantiateUI(originalEndNode.gameObject, originalEndNode.transform.parent.gameObject).GetComponent<DevQuickActionEndNode>();
		}
		SetupUnusedNodeForUse(result);
		return result;
	}

	private DevQuickActionCategoryNode GetUnsedCategoryNode()
	{
		DevQuickActionCategoryNode result = null;
		if (!recycledCategoriesNodes.TryPop(out result))
		{
			result = Util.KInstantiateUI(originalCategoryDevNode.gameObject, originalCategoryDevNode.transform.parent.gameObject).GetComponent<DevQuickActionCategoryNode>();
		}
		SetupUnusedNodeForUse(result);
		return result;
	}

	private void SetupUnusedNodeForUse(DevQuickActionNode node)
	{
		node.OnRecycle = OnNodeRecycled;
		node.SetChildrenSeparationSpace(60f);
		node.gameObject.SetActive(value: true);
	}

	private void OnNodeRecycled(DevQuickActionNode node)
	{
		if (node is DevQuickActionCategoryNode)
		{
			recycledCategoriesNodes.Push(node as DevQuickActionCategoryNode);
		}
		else if (node is DevQuickActionEndNode)
		{
			recycledEndNodes.Push(node as DevQuickActionEndNode);
		}
	}
}
