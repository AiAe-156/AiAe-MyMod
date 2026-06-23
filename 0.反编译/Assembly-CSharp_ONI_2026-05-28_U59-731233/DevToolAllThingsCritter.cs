using ImGuiNET;
using UnityEngine;

public class DevToolAllThingsCritter : DevTool
{
	private bool follow = false;

	private GameObject lockObject;

	private bool drawNavDots = true;

	private bool drawOccupyArea;

	private bool drawCollider;

	protected override void RenderTo(DevPanel panel)
	{
		if (SelectTool.Instance.selected != null || lockObject != null)
		{
			Contents();
		}
		else
		{
			ImGui.Text("No Critter Selected");
		}
	}

	private void Contents()
	{
		ImGui.Spacing();
		if (!(Camera.main != null) || !(SelectTool.Instance != null))
		{
			return;
		}
		GameObject gameObject = null;
		ImGui.Checkbox("Lock", ref follow);
		if (follow)
		{
			if (lockObject == null && SelectTool.Instance.selected != null && SelectTool.Instance.selected.GetComponent<KPrefabID>() != null && SelectTool.Instance.selected.HasTag(GameTags.Creature))
			{
				lockObject = SelectTool.Instance.selected.gameObject;
			}
			gameObject = lockObject;
		}
		else if (SelectTool.Instance.selected != null)
		{
			if (SelectTool.Instance.selected.GetComponent<KPrefabID>() != null && SelectTool.Instance.selected.HasTag(GameTags.Creature))
			{
				gameObject = SelectTool.Instance.selected.gameObject;
			}
			lockObject = null;
		}
		if (gameObject != null)
		{
			ImGuiEx.SimpleField("Name", DevToolEntity.GetNameFor(gameObject));
			Vector3 position = gameObject.transform.GetPosition();
			string field_value = $"X={position.x:F2}, Y={position.y:F2}, Z={position.z:F2}";
			ImGuiEx.SimpleField("Position", field_value);
			ImGuiEx.SimpleField("Cell", Grid.PosToCell(gameObject));
			NavigatorContents(position, gameObject);
			OccupyAreaContents(gameObject);
			ColliderContents(gameObject);
			CritterTemperatureMonitorContents(gameObject);
		}
	}

	private void NavigatorContents(Vector3 pos, GameObject go)
	{
		Navigator component = go.GetComponent<Navigator>();
		if (!(component != null) || !ImGui.CollapsingHeader("Navigator", ImGuiTreeNodeFlags.DefaultOpen))
		{
			return;
		}
		ImGui.Checkbox("Draw", ref drawNavDots);
		Vector2 positionFor = DevToolEntity.GetPositionFor(component.gameObject);
		string text = $"X={pos.x:F2}, Y={pos.y:F2}";
		ImGui.TextColored(Color.green, "World: " + text);
		if (drawNavDots)
		{
			ImGui.GetBackgroundDrawList().AddCircleFilled(positionFor, 10f, ImGui.GetColorU32(Color.green));
		}
		Vector2 vector = component.GetComponent<KBatchedAnimController>().GetPivotSymbolPosition();
		Vector2 screenPosition = DevToolEntity.GetScreenPosition(vector);
		string text2 = $"X={vector.x:F2}, Y={vector.y:F2}";
		ImGui.TextColored(Color.blue, "Pivot: " + text2);
		if (drawNavDots)
		{
			ImGui.GetBackgroundDrawList().AddCircleFilled(screenPosition, 10f, ImGui.GetColorU32(Color.blue));
		}
		TransitionDriver transitionDriver = component.transitionDriver;
		if (transitionDriver.GetTransition == null)
		{
			return;
		}
		if (transitionDriver.GetTransition.navGridTransition.useXOffset)
		{
			Vector2 vector2 = go.GetComponent<KBoxCollider2D>().size / 2f;
			if (transitionDriver.GetTransition.x > 0)
			{
				pos.x += vector2.x;
			}
			else if (transitionDriver.GetTransition.x < 0)
			{
				pos.x -= vector2.x;
			}
			Vector2 screenPosition2 = DevToolEntity.GetScreenPosition(pos);
			string text3 = $"X={pos.x:F2}, Y={pos.y:F2}";
			ImGui.TextColored(Color.magenta, "Nav Transition: " + text3);
			if (drawNavDots)
			{
				ImGui.GetBackgroundDrawList().AddCircleFilled(screenPosition2, 10f, ImGui.GetColorU32(Color.magenta));
			}
		}
		ImGuiEx.SimpleField("Transition", transitionDriver.GetTransition.navGridTransition.ToString());
	}

	private void OccupyAreaContents(GameObject go)
	{
		OccupyArea component = go.GetComponent<OccupyArea>();
		if (component != null && ImGui.CollapsingHeader("Occupy Area", ImGuiTreeNodeFlags.DefaultOpen))
		{
			Extents extents = component.GetExtents();
			ImGui.Checkbox("Draw Occupy Area", ref drawOccupyArea);
			if (drawOccupyArea)
			{
				int cell = Grid.PosToCell(go);
				int cell2 = Grid.OffsetCell(cell, extents.width, extents.height);
				Vector3 pos = Grid.CellToPos(cell2);
				Vector2 screenPosition = DevToolEntity.GetScreenPosition(pos);
				int cell3 = Grid.XYToCell(extents.x, extents.y);
				Vector3 pos2 = Grid.CellToPos(cell3);
				Vector2 screenPosition2 = DevToolEntity.GetScreenPosition(pos2);
				(Vector2, Vector2) screenRect = (screenPosition2, screenPosition);
				DevToolEntity.DrawScreenRect(screenRect, go.name, Color.cyan, new Color(0f, 1f, 1f, 0.33f));
			}
			string fmt = $"X={extents.x:F2}, Y={extents.y:F2}";
			ImGui.Text(fmt);
			string fmt2 = $"Width={extents.width:F2}, Height={extents.height:F2}";
			ImGui.Text(fmt2);
		}
	}

	private void ColliderContents(GameObject go)
	{
		KCollider2D component = go.GetComponent<KCollider2D>();
		if (component != null && ImGui.CollapsingHeader("Collider", ImGuiTreeNodeFlags.DefaultOpen))
		{
			ImGui.Checkbox("Draw Collider", ref drawCollider);
			if (drawCollider)
			{
				Vector2 screenPosition = DevToolEntity.GetScreenPosition(component.bounds.min);
				Vector2 screenPosition2 = DevToolEntity.GetScreenPosition(component.bounds.max);
				(Vector2, Vector2) screenRect = (screenPosition, screenPosition2);
				DevToolEntity.DrawScreenRect(screenRect, go.name, Color.green, new Color(0f, 1f, 0f, 0.33f));
			}
			string field_value = $"X={component.offset.x:F2}, Y={component.offset.y:F2}";
			ImGuiEx.SimpleField("Offset", field_value);
			ImGui.Text("Bounds");
			ImGuiEx.SimpleField("Offset", field_value);
			string field_value2 = $"{component.bounds.min} Cell: {Grid.PosToCell(component.bounds.min)}";
			ImGuiEx.SimpleField("Min", field_value2);
			string field_value3 = $"{component.bounds.max} Cell: {Grid.PosToCell(component.bounds.max)}";
			ImGuiEx.SimpleField("Max", field_value3);
			ImGuiEx.SimpleField("Center", component.bounds.center);
		}
	}

	private void CritterTemperatureMonitorContents(GameObject go)
	{
		CritterTemperatureMonitor.Instance sMI = go.GetSMI<CritterTemperatureMonitor.Instance>();
		if (sMI != null && ImGui.CollapsingHeader("Temperature Monitor", ImGuiTreeNodeFlags.DefaultOpen))
		{
			ImGuiEx.SimpleField("Current State", sMI.GetCurrentState().name.Replace("root.", ""));
			ImGuiEx.SimpleField("External", sMI.GetTemperatureExternal());
			ImGuiEx.SimpleField("Internal", sMI.GetTemperatureInternal());
		}
	}
}
