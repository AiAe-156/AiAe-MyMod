using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ImGuiNET;
using UnityEngine;

public class DevToolSceneInspector : DevTool
{
	private class StackItem
	{
		public object Obj;

		public string Filter;
	}

	private class ViewInfo
	{
		public string Name;

		public Action<object, string> Callback;

		public ViewInfo(string s, Action<object, string> a)
		{
			Name = s;
			Callback = a;
		}
	}

	private List<StackItem> Stack = new List<StackItem>();

	private int StackIndex = -1;

	private Dictionary<Type, ViewInfo> CustomTypeViews;

	public DevToolSceneInspector()
	{
		drawFlags = ImGuiWindowFlags.MenuBar;
		CustomTypeViews = new Dictionary<Type, ViewInfo>
		{
			{
				typeof(GameObject),
				new ViewInfo("Components", delegate(object o, string f)
				{
					CustomGameObjectDisplay(o, f);
				})
			},
			{
				typeof(KPrefabID),
				new ViewInfo("Prefab tags", delegate(object o, string f)
				{
					CustomPrefabTagView(o, f);
				})
			}
		};
	}

	public static void Inspect(object obj)
	{
		DevToolManager.Instance.panels.AddOrGetDevTool<DevToolSceneInspector>().PushObject(obj);
	}

	public void PushObject(object obj)
	{
		if (obj != null && (StackIndex < 0 || StackIndex >= Stack.Count || obj != Stack[StackIndex].Obj))
		{
			if (Stack.Count > StackIndex + 1)
			{
				Stack.RemoveRange(StackIndex + 1, Stack.Count - (StackIndex + 1));
			}
			StackItem stackItem = new StackItem();
			stackItem.Obj = obj;
			stackItem.Filter = "";
			Stack.Add(stackItem);
			StackIndex++;
		}
	}

	protected override void RenderTo(DevPanel panel)
	{
		for (int num = Stack.Count - 1; num >= 0; num--)
		{
			StackItem stackItem = Stack[num];
			if (stackItem.Obj.IsNullOrDestroyed())
			{
				Stack.RemoveAt(num);
				if (StackIndex >= num)
				{
					StackIndex--;
				}
			}
		}
		if (ImGui.BeginMenuBar())
		{
			if (ImGui.BeginMenu("Utils"))
			{
				if (ImGui.MenuItem("Goto current selection") && SelectTool.Instance?.selected?.gameObject != null)
				{
					PushObject(SelectTool.Instance?.selected?.gameObject);
				}
				ImGui.EndMenu();
			}
			ImGui.EndMenuBar();
		}
		if (ImGui.Button(" < ") && StackIndex > 0)
		{
			StackIndex--;
		}
		ImGui.SameLine();
		if (ImGui.Button(" > ") && StackIndex + 1 < Stack.Count)
		{
			StackIndex++;
		}
		if (Stack.Count == 0)
		{
			ImGui.Text("No Selection.");
			return;
		}
		StackItem stackItem2 = Stack[StackIndex];
		object obj = stackItem2.Obj;
		Type type = obj.GetType();
		ImGui.LabelText("Type", type.Name);
		if (ImGui.Button("Clear"))
		{
			stackItem2.Filter = "";
		}
		ImGui.SameLine();
		ImGui.InputText("Filter", ref stackItem2.Filter, 64u);
		ImGui.PushID(StackIndex);
		if (ImGui.BeginTabBar("##tabs", ImGuiTabBarFlags.None))
		{
			if (CustomTypeViews.TryGetValue(type, out var value) && ImGui.BeginTabItem(value.Name))
			{
				value.Callback(obj, stackItem2.Filter);
				ImGui.EndTabItem();
			}
			if (ImGui.BeginTabItem("Raw view"))
			{
				ImGui.BeginChild("ScrollRegion", new Vector2(0f, 0f), border: true, ImGuiWindowFlags.None);
				if (obj is IEnumerable)
				{
					IEnumerable enumerable = obj as IEnumerable;
					IEnumerator enumerator = enumerable.GetEnumerator();
					int num2 = 0;
					while (enumerator.MoveNext())
					{
						object obj2 = enumerator.Current;
						DisplayField("[" + num2 + "]", obj2.GetType(), ref obj2);
						num2++;
					}
				}
				else
				{
					FieldInfo[] fields = type.GetFields();
					foreach (FieldInfo fieldInfo in fields)
					{
						object obj3 = fieldInfo.GetValue(obj);
						Type fieldType = fieldInfo.FieldType;
						if (fieldInfo.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false).Length == 0 && (!(stackItem2.Filter != "") || fieldInfo.Name.IndexOf(stackItem2.Filter, 0, StringComparison.CurrentCultureIgnoreCase) != -1 || fieldType.Name.IndexOf(stackItem2.Filter, 0, StringComparison.CurrentCultureIgnoreCase) != -1) && DisplayField(fieldInfo.Name, fieldType, ref obj3) && !fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
						{
							fieldInfo.SetValue(obj, obj3);
						}
					}
					BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
					PropertyInfo[] properties = type.GetProperties(bindingAttr);
					foreach (PropertyInfo propertyInfo in properties)
					{
						if (!propertyInfo.CanRead)
						{
							ImGui.LabelText(propertyInfo.Name, "Unreadable");
						}
						else if (propertyInfo.GetIndexParameters().Length == 0 && propertyInfo.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false).Length == 0)
						{
							Type propertyType = propertyInfo.PropertyType;
							object obj4 = propertyInfo.GetValue(obj);
							if ((!(stackItem2.Filter != "") || propertyInfo.Name.IndexOf(stackItem2.Filter, 0, StringComparison.CurrentCultureIgnoreCase) != -1 || propertyType.Name.IndexOf(stackItem2.Filter, 0, StringComparison.CurrentCultureIgnoreCase) != -1) && DisplayField(propertyInfo.Name, propertyType, ref obj4) && propertyInfo.CanWrite)
							{
								propertyInfo.SetValue(obj, obj4);
							}
						}
					}
				}
				ImGui.EndChild();
				ImGui.EndTabItem();
			}
			ImGui.EndTabBar();
		}
		ImGui.PopID();
	}

	private bool DisplayField(string name, Type ft, ref object obj)
	{
		bool result = false;
		if (obj == null)
		{
			ImGui.LabelText(name, "null");
		}
		else if (ft == typeof(int))
		{
			int v = (int)obj;
			if (ImGui.InputInt(name, ref v))
			{
				obj = v;
				result = true;
			}
		}
		else if (ft == typeof(uint))
		{
			int v2 = (int)(uint)obj;
			if (ImGui.InputInt(name, ref v2))
			{
				obj = (uint)Math.Max(v2, 0);
				result = true;
			}
		}
		else if (ft == typeof(bool))
		{
			bool v3 = (bool)obj;
			if (ImGui.Checkbox(name, ref v3))
			{
				obj = v3;
				result = true;
			}
		}
		else if (ft == typeof(float))
		{
			float v4 = (float)obj;
			if (ImGui.InputFloat(name, ref v4))
			{
				obj = v4;
				result = true;
			}
		}
		else if (ft == typeof(Vector2))
		{
			Vector2 v5 = (Vector2)obj;
			if (ImGui.InputFloat2(name, ref v5))
			{
				obj = v5;
				result = true;
			}
		}
		else if (ft == typeof(Vector3))
		{
			Vector3 v6 = (Vector3)obj;
			if (ImGui.InputFloat3(name, ref v6))
			{
				obj = v6;
				result = true;
			}
		}
		else if (ft == typeof(string))
		{
			string input = (string)obj;
			if (ImGui.InputText(name, ref input, 256u))
			{
				obj = input;
				result = true;
			}
		}
		else if (ImGui.Selectable(name + " (" + ft.Name + ")", selected: false, ImGuiSelectableFlags.AllowDoubleClick) && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
		{
			PushObject(obj);
		}
		return result;
	}

	private void CustomGameObjectDisplay(object obj, string filter)
	{
		GameObject gameObject = (GameObject)obj;
		ImGui.BeginChild("ScrollRegion", new Vector2(0f, 0f), border: true, ImGuiWindowFlags.None);
		int num = 0;
		Behaviour[] components = gameObject.GetComponents<Behaviour>();
		foreach (Behaviour behaviour in components)
		{
			Type type = behaviour.GetType();
			if (!(filter != "") || type.Name.IndexOf(filter, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
			{
				ImGui.PushID(num++);
				bool v = behaviour.enabled;
				if (ImGui.Checkbox("", ref v))
				{
					behaviour.enabled = v;
				}
				ImGui.PopID();
				ImGui.SameLine();
				if (ImGui.Selectable(type.Name, selected: false, ImGuiSelectableFlags.AllowDoubleClick) && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
				{
					PushObject(behaviour);
				}
			}
		}
		ImGui.EndChild();
	}

	private void CustomPrefabTagView(object obj, string filter)
	{
		KPrefabID kPrefabID = (KPrefabID)obj;
		ImGui.BeginChild("ScrollRegion", new Vector2(0f, 0f), border: true, ImGuiWindowFlags.None);
		string input = kPrefabID.PrefabTag.Name;
		ImGui.InputText("PrefabID: ", ref input, 128u);
		int num = 0;
		foreach (Tag tag in kPrefabID.Tags)
		{
			string input2 = tag.Name;
			if (!(filter != "") || input2.IndexOf(filter, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
			{
				ImGui.InputText("[" + num + "]", ref input2, 128u);
				num++;
			}
		}
		ImGui.EndChild();
	}
}
