using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using UnityEngine;

public class DevToolAccessControl : DevTool
{
	private struct MinionPermissions
	{
		public bool Left;

		public bool Right;

		public MinionAssignablesProxy Proxy;
	}

	public static DevToolAccessControl Instance;

	private bool initialized;

	private Dictionary<Tag, List<MinionAssignablesProxy>> minionsByType = new Dictionary<Tag, List<MinionAssignablesProxy>>();

	private AccessControl selectedAccessControl;

	private bool lockSelected;

	private List<Tag> robotTypes;

	public DevToolAccessControl()
	{
		Instance = this;
	}

	private bool Init()
	{
		if (Game.Instance == null)
		{
			return false;
		}
		if (!initialized)
		{
			initialized = true;
			foreach (Tag item in (from e in Assets.GetPrefabsWithTag(GameTags.BaseMinion)
				select e.GetComponent<KPrefabID>().PrefabTag).ToList())
			{
				minionsByType.Add(item, new List<MinionAssignablesProxy>());
			}
			robotTypes = (from e in Assets.GetPrefabsWithTag(GameTags.Robots.Behaviours.HasDoorPermissions)
				select e.GetComponent<KPrefabID>().PrefabTag).ToList();
		}
		return true;
	}

	protected override void RenderTo(DevPanel panel)
	{
		if (Init())
		{
			if (DoorSelected())
			{
				ImGui.Checkbox("Lock Selection", ref lockSelected);
				MinionContents();
				RobotContents();
			}
			GridRestrictionSerializerContents();
		}
		else
		{
			ImGui.Text("No Access Control selected");
		}
	}

	private bool DoorSelected()
	{
		if (SelectTool.Instance != null && SelectTool.Instance.selected != null)
		{
			return SetDoorAccessControl() != null;
		}
		return false;
	}

	private AccessControl SetDoorAccessControl()
	{
		AccessControl component = SelectTool.Instance.selected.GetComponent<AccessControl>();
		if (component != selectedAccessControl && !lockSelected)
		{
			selectedAccessControl = component;
		}
		return selectedAccessControl;
	}

	private void MinionContents()
	{
		foreach (MinionAssignablesProxy item in Components.MinionAssignablesProxy.Items)
		{
			Tag minionModel = item.GetMinionModel();
			if (!minionsByType[minionModel].Contains(item))
			{
				minionsByType[minionModel].Add(item);
			}
		}
		foreach (Tag key in minionsByType.Keys)
		{
			ImGui.PushID(key.Name);
			ImGui.Text(key.Name);
			AccessControl.Permission setPermission = selectedAccessControl.GetSetPermission(Tag.Invalid.GetHashCode(), key);
			bool v = setPermission == AccessControl.Permission.GoLeft || setPermission == AccessControl.Permission.Both;
			bool v2 = setPermission == AccessControl.Permission.GoRight || setPermission == AccessControl.Permission.Both;
			ImGui.SameLine();
			if (ImGui.Checkbox("Left", ref v))
			{
				UpdateAccess(key, v, v2);
			}
			ImGui.SameLine();
			if (ImGui.Checkbox("Right", ref v2))
			{
				UpdateAccess(key, v, v2);
			}
			ImGui.PopID();
			ImGui.Indent();
			ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.75f);
			foreach (MinionAssignablesProxy item2 in minionsByType[key])
			{
				ImGui.PushID(item2.TargetInstanceID);
				ImGui.Text(item2.target.GetProperName());
				AccessControl.Permission setPermission2 = selectedAccessControl.GetSetPermission(item2);
				ImGui.SameLine();
				bool v3 = setPermission2 == AccessControl.Permission.GoLeft || setPermission2 == AccessControl.Permission.Both;
				bool v4 = setPermission2 == AccessControl.Permission.GoRight || setPermission2 == AccessControl.Permission.Both;
				if (ImGui.Checkbox("Left", ref v3))
				{
					UpdateMinionAccess(item2, v3, v4);
				}
				ImGui.SameLine();
				if (ImGui.Checkbox("Right", ref v4))
				{
					UpdateMinionAccess(item2, v3, v4);
				}
				ImGui.PopID();
			}
			ImGui.PopStyleVar();
			ImGui.Unindent();
		}
	}

	private void RobotContents()
	{
		ImGui.PushID(GameTags.Robot.Name);
		ImGui.Text(GameTags.Robot.Name);
		AccessControl.Permission setPermission = selectedAccessControl.GetSetPermission(Tag.Invalid.GetHashCode(), GameTags.Robot);
		bool v = setPermission == AccessControl.Permission.GoLeft || setPermission == AccessControl.Permission.Both;
		bool v2 = setPermission == AccessControl.Permission.GoRight || setPermission == AccessControl.Permission.Both;
		ImGui.SameLine();
		if (ImGui.Checkbox("Left", ref v))
		{
			UpdateAccess(GameTags.Robot, v, v2);
		}
		ImGui.SameLine();
		if (ImGui.Checkbox("Right", ref v2))
		{
			UpdateAccess(GameTags.Robot, v, v2);
		}
		ImGui.PopID();
		ImGui.Indent();
		ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.75f);
		foreach (Tag robotType in robotTypes)
		{
			ImGui.PushID(robotType.Name);
			ImGui.Text(robotType.Name);
			AccessControl.Permission setPermission2 = selectedAccessControl.GetSetPermission(robotType);
			bool v3 = setPermission2 == AccessControl.Permission.GoLeft || setPermission2 == AccessControl.Permission.Both;
			bool v4 = setPermission2 == AccessControl.Permission.GoRight || setPermission2 == AccessControl.Permission.Both;
			ImGui.SameLine();
			if (ImGui.Checkbox("Left", ref v3))
			{
				UpdateAccess(robotType, v3, v4);
			}
			ImGui.SameLine();
			if (ImGui.Checkbox("Right", ref v4))
			{
				UpdateAccess(robotType, v3, v4);
			}
			ImGui.PopID();
		}
		ImGui.PopStyleVar();
		ImGui.Unindent();
	}

	private void GridRestrictionSerializerContents()
	{
	}

	private void UpdateMinionAccess(MinionAssignablesProxy proxy, bool left, bool right)
	{
		AccessControl.Permission permission = (left ? ((!right) ? AccessControl.Permission.GoLeft : AccessControl.Permission.Both) : ((!right) ? AccessControl.Permission.Neither : AccessControl.Permission.GoRight));
		selectedAccessControl.SetPermission(proxy, permission);
	}

	private void UpdateAccess(Tag id, bool left, bool right)
	{
		AccessControl.Permission permission = (left ? ((!right) ? AccessControl.Permission.GoLeft : AccessControl.Permission.Both) : ((!right) ? AccessControl.Permission.Neither : AccessControl.Permission.GoRight));
		selectedAccessControl.SetPermission(id, permission);
	}
}
