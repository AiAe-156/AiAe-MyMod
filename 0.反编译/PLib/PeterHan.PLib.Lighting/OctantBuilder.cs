using System;
using System.Collections.Generic;
using PeterHan.PLib.Detours;

namespace PeterHan.PLib.Lighting;

/// <summary>
/// A builder class which creates default light patterns based on octants.
/// </summary>
public sealed class OctantBuilder
{
	/// <summary>
	/// The delegate type called to run the default DiscreteShadowCaster.ScanOctant.
	/// </summary>
	private delegate void ScanOctantFunc(Vector2I cellPos, int range, int depth, Octant octant, double startSlope, double endSlope, List<int> visiblePoints);

	/// <summary>
	/// The method to call to scan octants.
	/// </summary>
	private static readonly ScanOctantFunc OCTANT_SCAN;

	/// <summary>
	/// The location where light cells are added.
	/// </summary>
	private readonly IDictionary<int, float> destination;

	/// <summary>
	/// The fallout to use when building the light.
	/// </summary>
	public float Falloff { get; set; }

	/// <summary>
	/// If false, uses the default game smoothing. If true, uses better smoothing.
	/// </summary>
	public bool SmoothLight { get; set; }

	/// <summary>
	/// The origin cell.
	/// </summary>
	public int SourceCell { get; }

	static OctantBuilder()
	{
		OCTANT_SCAN = typeof(DiscreteShadowCaster).Detour<ScanOctantFunc>("ScanOctant");
		if (OCTANT_SCAN == null)
		{
			PLightManager.LogLightingWarning("OctantBuilder cannot find default octant scanner!");
		}
	}

	/// <summary>
	/// Creates a new octant builder.
	/// </summary>
	/// <param name="destination">The location where the lit cells will be placed.</param>
	/// <param name="sourceCell">The origin cell of the light.</param>
	public OctantBuilder(IDictionary<int, float> destination, int sourceCell)
	{
		if (!Grid.IsValidCell(sourceCell))
		{
			throw new ArgumentOutOfRangeException("sourceCell");
		}
		this.destination = destination ?? throw new ArgumentNullException("destination");
		destination[sourceCell] = 1f;
		Falloff = 0.5f;
		SmoothLight = false;
		SourceCell = sourceCell;
	}

	public OctantBuilder AddOctant(int range, Octant octant)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		PooledList<int, OctantBuilder> val = ListPool<int, OctantBuilder>.Allocate();
		OCTANT_SCAN?.Invoke(Grid.CellToXY(SourceCell), range, 1, octant, 1.0, 0.0, (List<int>)(object)val);
		foreach (int item in (List<int>)(object)val)
		{
			float value = ((!SmoothLight) ? PLightManager.GetDefaultFalloff(Falloff, item, SourceCell) : PLightManager.GetSmoothFalloff(Falloff, item, SourceCell));
			destination[item] = value;
		}
		val.Recycle();
		return this;
	}

	public override string ToString()
	{
		return $"OctantBuilder[Cell {SourceCell:D}, {destination.Count:D} lit]";
	}
}
