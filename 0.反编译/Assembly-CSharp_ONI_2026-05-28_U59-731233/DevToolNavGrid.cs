using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ImGuiNET;
using UnityEngine;

public class DevToolNavGrid : DevTool
{
	private const string INVALID_OVERLAY_MODE_STR = "None";

	private string[] navGridNames = null;

	private int selectedNavGrid = 0;

	private bool drawLinks = false;

	private Dictionary<NavType, bool> drawLinkTypes = new Dictionary<NavType, bool>();

	public static DevToolNavGrid Instance;

	private int[] linkStats;

	private int highestLinkCell;

	private int highestLinkCount;

	private int selectedCell = Grid.InvalidCell;

	private bool follow = false;

	private GameObject lockObject;

	public DevToolNavGrid()
	{
		Instance = this;
		drawLinkTypes = new Dictionary<NavType, bool>(11);
		foreach (NavType value in Enum.GetValues(typeof(NavType)))
		{
			drawLinkTypes.Add(value, value: true);
		}
	}

	private bool Init()
	{
		if (Pathfinding.Instance == null)
		{
			return false;
		}
		if (navGridNames != null)
		{
			return true;
		}
		navGridNames = (from x in Pathfinding.Instance.GetNavGrids()
			select x.id).ToArray();
		return true;
	}

	protected override void RenderTo(DevPanel panel)
	{
		if (Init())
		{
			Contents();
		}
		else
		{
			ImGui.Text("Game not initialized");
		}
	}

	public void SetCell(int cell)
	{
		selectedCell = cell;
	}

	private void Contents()
	{
		ImGui.Combo("Nav Grid ID", ref selectedNavGrid, navGridNames, navGridNames.Length);
		NavGrid navGrid = Pathfinding.Instance.GetNavGrid(navGridNames[selectedNavGrid]);
		ImGui.Text("Max Links per cell: " + navGrid.maxLinksPerCell);
		ImGui.Spacing();
		int num = Marshal.SizeOf<NavGrid.Link>();
		long num2 = (long)navGrid.maxLinksPerCell * (long)Grid.CellCount * num;
		long num3 = (long)Grid.CellCount * 2L;
		long num4 = (Grid.CellCount + 7) / 8;
		long num5 = num2 + num3 + num4;
		ImGui.Text($"Memory usage: {num5 / 1048576} MB (Links: {num2 / 1048576} MB [{num}B per link], NavTable: {num3 / 1024} KB)");
		if (ImGui.Button("Calculate Stats"))
		{
			linkStats = new int[navGrid.maxLinksPerCell];
			highestLinkCell = 0;
			highestLinkCount = 0;
			for (int i = 0; i < Grid.CellCount; i++)
			{
				int num6 = 0;
				for (int j = 0; j < navGrid.maxLinksPerCell; j++)
				{
					int num7 = i * navGrid.maxLinksPerCell + j;
					NavGrid.Link link = navGrid.Links[num7];
					if (link.link == Grid.InvalidCell)
					{
						break;
					}
					num6++;
				}
				if (num6 > highestLinkCount)
				{
					highestLinkCell = i;
					highestLinkCount = num6;
				}
				linkStats[num6]++;
			}
		}
		ImGui.SameLine();
		if (ImGui.Button("Clear"))
		{
			linkStats = null;
		}
		ImGui.SameLine();
		if (ImGui.Button("Rescan"))
		{
			navGrid.InitializeGraph();
		}
		ImGui.SameLine();
		if (ImGui.Button("Dirty") && selectedCell != Grid.InvalidCell)
		{
			Pathfinding.Instance.AddDirtyNavGridCell(selectedCell);
		}
		if (linkStats != null)
		{
			ImGui.Text("Highest link count: " + highestLinkCount);
			ImGui.Text($"Utilized percentage: {(float)highestLinkCount / (float)navGrid.maxLinksPerCell * 100f} %");
			ImGui.SameLine();
			if (ImGui.Button($"Select {highestLinkCell}"))
			{
				selectedCell = highestLinkCell;
			}
			for (int k = 0; k < linkStats.Length; k++)
			{
				if (linkStats[k] > 0)
				{
					ImGui.Text($"\t{k}: {linkStats[k]}");
				}
			}
		}
		if (Camera.main != null && SelectTool.Instance != null)
		{
			GameObject gameObject = null;
			ImGui.Checkbox("Lock", ref follow);
			ImGui.Checkbox("DrawDebugPath", ref DebugHandler.DebugPathFinding);
			if (follow)
			{
				if (lockObject == null && SelectTool.Instance.selected != null)
				{
					lockObject = SelectTool.Instance.selected.gameObject;
				}
				gameObject = lockObject;
			}
			else if (SelectTool.Instance.selected != null)
			{
				gameObject = SelectTool.Instance.selected.gameObject;
				lockObject = null;
			}
			if (gameObject != null)
			{
				Navigator component = gameObject.GetComponent<Navigator>();
				if (component != null)
				{
					Vector2 positionFor = DevToolEntity.GetPositionFor(component.gameObject);
					ImGui.GetBackgroundDrawList().AddCircleFilled(positionFor, 10f, ImGui.GetColorU32(Color.green));
					Vector2 screenPosition = DevToolEntity.GetScreenPosition(component.GetComponent<KBatchedAnimController>().GetPivotSymbolPosition());
					ImGui.GetBackgroundDrawList().AddCircleFilled(screenPosition, 10f, ImGui.GetColorU32(Color.blue));
					TransitionDriver transitionDriver = component.transitionDriver;
					if (transitionDriver.GetTransition != null)
					{
						Vector3 position = component.transform.GetPosition();
						Vector2 vector = gameObject.GetComponent<KBoxCollider2D>().size / 2f;
						if (transitionDriver.GetTransition.x > 0)
						{
							position.x += vector.x;
						}
						else if (transitionDriver.GetTransition.x < 0)
						{
							position.x -= vector.x;
						}
						Vector2 screenPosition2 = DevToolEntity.GetScreenPosition(position);
						ImGui.GetBackgroundDrawList().AddCircleFilled(screenPosition2, 10f, ImGui.GetColorU32(Color.magenta));
					}
					if (DebugHandler.DebugPathFinding)
					{
						int mouseCell = DebugHandler.GetMouseCell();
						if (Grid.IsValidCell(mouseCell))
						{
							PathFinder.PotentialPath potential_path = new PathFinder.PotentialPath(Grid.PosToCell(component), component.CurrentNavType, component.flags);
							PathFinder.Path path = default(PathFinder.Path);
							if (!component.PathGrid.BuildPath(component.cachedCell, mouseCell, component.CurrentNavType, ref path))
							{
								PathFinder.UpdatePath(component.NavGrid, component.GetCurrentAbilities(), potential_path, PathFinderQueries.cellQuery.Reset(mouseCell), ref path);
							}
							if (path.nodes != null)
							{
								for (int l = 0; l < path.nodes.Count - 1; l++)
								{
									if (Grid.WorldIdx[path.nodes[l].cell] == ClusterManager.Instance.activeWorldId)
									{
										NavGrid.Transition transition = navGrid.transitions[path.nodes[l].transitionId];
										ImGui.Text($"   {transition.start} -> {transition.end} x:{transition.x} y:{transition.y} anim:{transition.anim} cost:{transition.cost}");
									}
								}
							}
							else
							{
								ImGui.Text("No valid path");
							}
						}
					}
				}
			}
		}
		ImGui.Spacing();
		ImGui.Checkbox("Draw Links", ref drawLinks);
		if (drawLinks)
		{
			ImGui.Indent();
			foreach (NavType item in drawLinkTypes.Keys.ToList())
			{
				bool v = drawLinkTypes[item];
				if (item != NavType.NumNavTypes)
				{
					ImGui.PushID(item.ToString());
					if (ImGui.Checkbox(item.ToString(), ref v))
					{
						drawLinkTypes[item] = v;
					}
					ImGui.PopID();
				}
			}
			if (ImGui.Button("Deselect All"))
			{
				foreach (NavType value in Enum.GetValues(typeof(NavType)))
				{
					drawLinkTypes[value] = false;
				}
			}
			ImGui.SameLine();
			if (ImGui.Button("Select All"))
			{
				foreach (NavType value2 in Enum.GetValues(typeof(NavType)))
				{
					drawLinkTypes[value2] = true;
				}
			}
			ImGui.Unindent();
			DebugDrawLinks(navGrid);
		}
		ImGui.Spacing();
		Grid.CellToXY(selectedCell, out var x, out var y);
		ImGui.Text($"Selected Cell: {selectedCell} ({x},{y})");
		if (!Grid.IsValidCell(selectedCell))
		{
			return;
		}
		DrawNavTypes(selectedCell, navGrid);
		if (navGrid.Links == null || navGrid.Links.Length <= navGrid.maxLinksPerCell * selectedCell)
		{
			return;
		}
		for (int m = 0; m < navGrid.maxLinksPerCell; m++)
		{
			int num8 = selectedCell * navGrid.maxLinksPerCell + m;
			NavGrid.Link l2 = navGrid.Links[num8];
			if (l2.link == Grid.InvalidCell)
			{
				break;
			}
			DrawLink(m, l2, navGrid);
		}
	}

	private void DrawLink(int idx, NavGrid.Link l, NavGrid navGrid)
	{
		NavGrid.Transition transition = navGrid.transitions[l.transitionId];
		ImGui.Text($"   {transition.start} -> {transition.end} x:{transition.x} y:{transition.y} anim:{transition.anim} cost:{transition.cost}");
	}

	private void DrawNavTypes(int cell, NavGrid navGrid)
	{
		for (byte b = 0; b < 11; b++)
		{
			NavType navType = (NavType)b;
			if (navGrid.NavTable.IsValid(cell, navType))
			{
				ImGui.Text($"{navType}");
			}
		}
	}

	private void DebugDrawLinks(NavGrid navGrid)
	{
		if (Camera.main == null)
		{
			return;
		}
		Camera main = Camera.main;
		int pixelHeight = main.pixelHeight;
		Color color = Color.white;
		for (int i = 0; i < Grid.CellCount; i++)
		{
			int num = i * navGrid.maxLinksPerCell;
			for (int link = navGrid.Links[num].link; link != NavGrid.InvalidCell; link = navGrid.Links[num].link)
			{
				if (DrawNavTypeLink(navGrid, num, ref color))
				{
					Vector3 navPos = NavTypeHelper.GetNavPos(i, navGrid.Links[num].startNavType);
					Vector3 navPos2 = NavTypeHelper.GetNavPos(link, navGrid.Links[num].endNavType);
					if (IsInCameraView(main, navPos) && IsInCameraView(main, navPos2))
					{
						Vector2 start = main.WorldToScreenPoint(navPos);
						Vector2 end = main.WorldToScreenPoint(navPos2);
						start.y = (float)pixelHeight - start.y;
						end.y = (float)pixelHeight - end.y;
						uint colorU = ImGui.GetColorU32(color);
						DrawArrowLink(start, end, colorU);
					}
				}
				num++;
			}
		}
	}

	private bool IsInCameraView(Camera camera, Vector3 pos)
	{
		Vector3 vector = camera.WorldToViewportPoint(pos);
		return vector.x >= 0f && vector.y >= 0f && vector.x <= 1f && vector.y <= 1f;
	}

	private bool DrawNavTypeLink(NavGrid navGrid, int end_cell_idx, ref Color color)
	{
		for (int i = 0; i < navGrid.ValidNavTypes.Length; i++)
		{
			if (navGrid.ValidNavTypes[i] == navGrid.Links[end_cell_idx].startNavType)
			{
				color = NavGrid.NavTypeColor(navGrid.Links[end_cell_idx].startNavType);
				return drawLinkTypes[navGrid.Links[end_cell_idx].startNavType] || drawLinkTypes[navGrid.Links[end_cell_idx].endNavType];
			}
			if (navGrid.ValidNavTypes[i] == navGrid.Links[end_cell_idx].endNavType)
			{
				color = NavGrid.NavTypeColor(navGrid.Links[end_cell_idx].endNavType);
				return drawLinkTypes[navGrid.Links[end_cell_idx].startNavType] || drawLinkTypes[navGrid.Links[end_cell_idx].endNavType];
			}
		}
		return false;
	}

	private void DrawArrowLink(Vector2 start, Vector2 end, uint color)
	{
		ImDrawListPtr backgroundDrawList = ImGui.GetBackgroundDrawList();
		Vector2 vector = end - start;
		float magnitude = vector.magnitude;
		if (magnitude > 0f)
		{
			vector *= 1f / Mathf.Sqrt(magnitude);
		}
		Vector2 p = end - vector * 1f + new Vector2(0f - vector.y, vector.x) * 1f;
		Vector2 p2 = end - vector * 1f - new Vector2(0f - vector.y, vector.x) * 1f;
		backgroundDrawList.AddLine(start, end, color);
		backgroundDrawList.AddTriangleFilled(end, p, p2, color);
	}
}
