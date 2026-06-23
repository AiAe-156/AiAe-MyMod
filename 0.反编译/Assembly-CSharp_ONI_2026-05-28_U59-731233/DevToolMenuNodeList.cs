using System;
using System.IO;
using ImGuiNET;

public class DevToolMenuNodeList
{
	private DevToolMenuNodeParent root = new DevToolMenuNodeParent("<root>");

	public DevToolMenuNodeParent AddOrGetParentFor(string childPath)
	{
		string[] array = Path.GetDirectoryName(childPath).Split('/');
		string text = "";
		DevToolMenuNodeParent devToolMenuNodeParent = root;
		string[] array2 = array;
		foreach (string split in array2)
		{
			text += devToolMenuNodeParent.GetName();
			IMenuNode menuNode = devToolMenuNodeParent.children.Find((IMenuNode x) => x.GetName() == split);
			DevToolMenuNodeParent devToolMenuNodeParent3;
			if (menuNode != null)
			{
				if (!(menuNode is DevToolMenuNodeParent devToolMenuNodeParent2))
				{
					throw new Exception("Conflict! Both a leaf and parent node exist at path: " + text);
				}
				devToolMenuNodeParent3 = devToolMenuNodeParent2;
			}
			else
			{
				devToolMenuNodeParent3 = new DevToolMenuNodeParent(split);
				devToolMenuNodeParent.AddChild(devToolMenuNodeParent3);
			}
			devToolMenuNodeParent = devToolMenuNodeParent3;
		}
		return devToolMenuNodeParent;
	}

	public DevToolMenuNodeAction AddAction(string path, System.Action onClickFn)
	{
		string fileName = Path.GetFileName(path);
		DevToolMenuNodeAction devToolMenuNodeAction = new DevToolMenuNodeAction(fileName, onClickFn);
		AddOrGetParentFor(path).AddChild(devToolMenuNodeAction);
		return devToolMenuNodeAction;
	}

	public void Draw()
	{
		foreach (IMenuNode child in root.children)
		{
			child.Draw();
		}
	}

	public void DrawFull()
	{
		if (ImGui.BeginMainMenuBar())
		{
			Draw();
			ImGui.EndMainMenuBar();
		}
	}
}
