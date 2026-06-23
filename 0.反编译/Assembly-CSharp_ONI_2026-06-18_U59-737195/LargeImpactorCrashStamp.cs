using System.Collections.Generic;
using KSerialization;
using ProcGen;
using TemplateClasses;
using UnityEngine;

public class LargeImpactorCrashStamp : KMonoBehaviour
{
	public string largeStampTemplate;

	[Serialize]
	public Vector2I stampLocation = Vector2I.zero;

	public Dictionary<int, CellOffset> TemplateBottomCellsOffsets = new Dictionary<int, CellOffset>();

	private List<int> asteroidCellIndices;

	private int targetWorldId;

	public TemplateContainer asteroidTemplate { get; private set; }

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (DlcManager.IsExpansion1Active())
		{
			GetComponent<ClusterDestinationSelector>();
			ClusterMapLargeImpactor.Def def = this.GetDef<ClusterMapLargeImpactor.Def>();
			targetWorldId = def.destinationWorldID;
		}
		else
		{
			targetWorldId = ClusterManager.Instance.GetStartWorld().id;
		}
		InitializeData();
	}

	private void InitializeData()
	{
		asteroidTemplate = TemplateCache.GetTemplate(largeStampTemplate);
		if (stampLocation == Vector2I.zero)
		{
			stampLocation = FindIdealLocation(ClusterManager.Instance.GetWorld(targetWorldId), asteroidTemplate);
		}
		ConvertToCellIndices();
		SetVisualization();
	}

	private Vector2I FindIdealLocation(WorldContainer world, TemplateContainer template)
	{
		RectInt templateBounds = template.GetTemplateBounds();
		int num = world.WorldSize.y + world.WorldOffset.y - (templateBounds.height + templateBounds.yMin) - 1;
		Vector2I result = world.WorldOffset;
		int num2 = result.X + world.WorldSize.x / 2;
		foreach (Telepad item in Components.Telepads.Items)
		{
			if (item.GetMyWorldId() != world.id)
			{
				continue;
			}
			Vector2I vector2I = Grid.PosToXY(item.transform.position);
			vector2I.Y += templateBounds.height / 2 + 20;
			if (!Grid.IsValidCell(Grid.XYToCell(vector2I.X, vector2I.Y)))
			{
				continue;
			}
			result = vector2I;
			goto IL_01b2;
		}
		while (true)
		{
			if (num > world.WorldOffset.y)
			{
				foreach (Cell cell2 in template.cells)
				{
					if (!((float)cell2.location_y < templateBounds.center.y))
					{
						int cell = Grid.XYToCell(num2 + cell2.location_x, num + cell2.location_y);
						if (Grid.IsValidCell(cell) && World.Instance.zoneRenderData.GetSubWorldZoneType(cell) != SubWorld.ZoneType.Space)
						{
							result = new Vector2I(num2, num - 50);
							goto end_IL_0173;
						}
					}
				}
				num--;
				continue;
			}
			return new Vector2I(num2, world.WorldSize.y + world.WorldOffset.y - (templateBounds.yMax - templateBounds.yMin));
			continue;
			end_IL_0173:
			break;
		}
		goto IL_01b2;
		IL_01b2:
		return result;
	}

	private void ConvertToCellIndices()
	{
		asteroidCellIndices = new List<int>(asteroidTemplate.cells.Count);
		foreach (Cell cell in asteroidTemplate.cells)
		{
			if (!TemplateBottomCellsOffsets.TryGetValue(cell.location_x, out var value))
			{
				value = new CellOffset(cell.location_x, int.MaxValue);
			}
			if (cell.location_y < value.y)
			{
				value.y = cell.location_y;
			}
			TemplateBottomCellsOffsets[cell.location_x] = value;
			int item = Grid.XYToCell(stampLocation.X + cell.location_x, stampLocation.Y + cell.location_y);
			asteroidCellIndices.Add(item);
		}
	}

	private bool IsCellOutsideOfImpactSite(int cell)
	{
		return !asteroidCellIndices.Contains(cell);
	}

	public void RevealFogOfWar(int revealRadiusPerCell)
	{
		int cell = Grid.XYToCell(stampLocation.x, stampLocation.y);
		RectInt templateBounds = asteroidTemplate.GetTemplateBounds();
		for (int i = templateBounds.xMin; i < templateBounds.xMax; i++)
		{
			for (int j = templateBounds.yMin; j < templateBounds.yMax; j++)
			{
				int cell2 = Grid.OffsetCell(cell, i, j);
				if (Grid.IsValidCell(cell2))
				{
					Vector2I vector2I = Grid.CellToXY(cell2);
					GridVisibility.Reveal(vector2I.x, vector2I.y, revealRadiusPerCell, 1f);
				}
			}
		}
	}

	private void SetVisualization()
	{
		LargeImpactorVisualizer component = GetComponent<LargeImpactorVisualizer>();
		Vector2I vector2I = Grid.PosToXY(component.gameObject.transform.position);
		RectInt templateBounds = asteroidTemplate.GetTemplateBounds();
		component.RangeMin.x = templateBounds.xMin;
		component.RangeMin.y = templateBounds.yMin;
		component.RangeMax.x = templateBounds.xMax;
		component.RangeMax.y = templateBounds.yMax;
		component.TexSize = new Vector2I(templateBounds.size.x + 1, templateBounds.size.y + 1);
		component.OriginOffset = stampLocation - vector2I;
		component.BlockingCb = (int cell) => IsCellOutsideOfImpactSite(cell);
	}
}
