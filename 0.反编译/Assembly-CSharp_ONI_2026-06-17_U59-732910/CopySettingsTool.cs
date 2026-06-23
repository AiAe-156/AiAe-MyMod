using System.Collections.Generic;
using UnityEngine;

public class CopySettingsTool : DragTool
{
	public static CopySettingsTool Instance;

	public GameObject Placer;

	private GameObject sourceGameObject;

	private readonly Dictionary<GameObject, KPrefabID> targets = new Dictionary<GameObject, KPrefabID>();

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
	}

	public void Activate()
	{
		PlayerController.Instance.ActivateTool(this);
	}

	public void SetSourceObject(GameObject sourceGameObject)
	{
		this.sourceGameObject = sourceGameObject;
	}

	protected override void OnDragTool(int cell, int _distFromOrigin)
	{
		if (!(sourceGameObject == null))
		{
			DebugUtil.DevAssert(Grid.IsValidCell(cell), "DragTool only calls us with valid cells");
			KPrefabID kPrefabID = CopyBuildingSettings.ResolveTarget(CopyBuildingSettings.ResolveLayer(sourceGameObject), cell);
			if (kPrefabID != null && kPrefabID.gameObject != sourceGameObject)
			{
				targets.TryAdd(kPrefabID.gameObject, kPrefabID);
			}
		}
	}

	protected override void OnDragComplete(Vector3 _cursorDown, Vector3 _cursorUp)
	{
		if (sourceGameObject != null)
		{
			sourceGameObject.TryGetComponent<KPrefabID>(out var component);
			sourceGameObject.TryGetComponent<CopyBuildingSettings>(out var component2);
			if (component != null && component2 != null)
			{
				foreach (KPrefabID value in targets.Values)
				{
					CopyBuildingSettings.ApplyCopy(value, sourceGameObject, component, component2);
				}
			}
		}
		targets.Clear();
	}

	protected override void OnActivateTool()
	{
		base.OnActivateTool();
	}

	protected override void OnDeactivateTool(InterfaceTool new_tool)
	{
		base.OnDeactivateTool(new_tool);
		sourceGameObject = null;
	}
}
