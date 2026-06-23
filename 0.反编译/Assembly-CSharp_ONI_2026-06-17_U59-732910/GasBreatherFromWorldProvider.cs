using System;

public class GasBreatherFromWorldProvider : OxygenBreather.IGasProvider
{
	public struct BreathableCellData
	{
		public int Cell;

		public SimHashes ElementID;

		public float Mass;

		public bool IsBreathable;
	}

	public static CellOffset[] DEFAULT_BREATHABLE_OFFSETS = new CellOffset[6]
	{
		new CellOffset(0, 0),
		new CellOffset(0, 1),
		new CellOffset(1, 1),
		new CellOffset(-1, 1),
		new CellOffset(1, 0),
		new CellOffset(-1, 0)
	};

	private OxygenBreather oxygenBreather;

	private Navigator nav;

	private static Action<Sim.MassConsumedCallback, object> OnSimConsumeCallbackAction = OnSimConsumeCallback;

	public BreathableCellData GetBestBreathableCellAtCurrentLocation()
	{
		return GetBestBreathableCellAroundSpecificCell(Grid.PosToCell(oxygenBreather), DEFAULT_BREATHABLE_OFFSETS, oxygenBreather);
	}

	public static BreathableCellData GetBestBreathableCellAroundSpecificCell(int theSpecificCell, CellOffset[] breathRange, OxygenBreather breather)
	{
		float totalBreathableMassAroundCell;
		return GetBestBreathableCellAroundSpecificCell(theSpecificCell, breathRange, breather, out totalBreathableMassAroundCell);
	}

	public static BreathableCellData GetBestBreathableCellAroundSpecificCell(int theSpecificCell, CellOffset[] breathRange, OxygenBreather breather, out float totalBreathableMassAroundCell)
	{
		if (breathRange == null)
		{
			breathRange = DEFAULT_BREATHABLE_OFFSETS;
		}
		float num = 0f;
		int cell = theSpecificCell;
		SimHashes simHashes = SimHashes.Vacuum;
		totalBreathableMassAroundCell = 0f;
		CellOffset[] array = breathRange;
		foreach (CellOffset offset in array)
		{
			int num2 = Grid.OffsetCell(theSpecificCell, offset);
			SimHashes elementID;
			float breathableCellMass = GetBreathableCellMass(num2, out elementID);
			totalBreathableMassAroundCell += breathableCellMass;
			if (breathableCellMass > num && breathableCellMass > breather.noOxygenThreshold)
			{
				num = breathableCellMass;
				cell = num2;
				simHashes = elementID;
			}
		}
		return new BreathableCellData
		{
			Cell = cell,
			ElementID = simHashes,
			Mass = num,
			IsBreathable = (simHashes != SimHashes.Vacuum)
		};
	}

	private static float GetBreathableCellMass(int cell, out SimHashes elementID)
	{
		elementID = SimHashes.Vacuum;
		if (Grid.IsValidCell(cell))
		{
			Element element = Grid.Element[cell];
			if (element.HasTag(GameTags.Breathable))
			{
				elementID = element.id;
				return Grid.Mass[cell];
			}
		}
		return 0f;
	}

	public void OnSetOxygenBreather(OxygenBreather oxygen_breather)
	{
		oxygenBreather = oxygen_breather;
		nav = oxygenBreather.GetComponent<Navigator>();
	}

	public void OnClearOxygenBreather(OxygenBreather oxygen_breather)
	{
	}

	public bool ShouldEmitCO2()
	{
		return nav.CurrentNavType != NavType.Tube;
	}

	public bool ShouldStoreCO2()
	{
		return false;
	}

	public bool IsLowOxygen()
	{
		BreathableCellData bestBreathableCellAtCurrentLocation = GetBestBreathableCellAtCurrentLocation();
		if (bestBreathableCellAtCurrentLocation.IsBreathable)
		{
			return bestBreathableCellAtCurrentLocation.Mass < oxygenBreather.lowOxygenThreshold;
		}
		return false;
	}

	public bool HasOxygen()
	{
		if (oxygenBreather.prefabID.HasTag(GameTags.RecoveringBreath) || oxygenBreather.prefabID.HasTag(GameTags.InTransitTube))
		{
			return true;
		}
		return GetBestBreathableCellAtCurrentLocation().IsBreathable;
	}

	public bool IsBlocked()
	{
		return oxygenBreather.HasTag(GameTags.HasSuitTank);
	}

	public bool ConsumeGas(OxygenBreather oxygen_breather, float mass_to_consume)
	{
		if (nav.CurrentNavType != NavType.Tube)
		{
			BreathableCellData bestBreathableCellAtCurrentLocation = GetBestBreathableCellAtCurrentLocation();
			if (!bestBreathableCellAtCurrentLocation.IsBreathable)
			{
				return false;
			}
			SimHashes elementID = bestBreathableCellAtCurrentLocation.ElementID;
			HandleVector<Game.ComplexCallbackInfo<Sim.MassConsumedCallback>>.Handle handle = Game.Instance.massConsumedCallbackManager.Add(OnSimConsumeCallbackAction, oxygen_breather, "GasBreatherFromWorldProvider");
			SimMessages.ConsumeMass(bestBreathableCellAtCurrentLocation.Cell, elementID, mass_to_consume, 3, handle.index);
		}
		return true;
	}

	private static void OnSimConsumeCallback(Sim.MassConsumedCallback mass_cb_info, object data)
	{
		SimHashes id = ElementLoader.elements[mass_cb_info.elemIdx].id;
		OxygenBreather.BreathableGasConsumed(data as OxygenBreather, id, mass_cb_info.mass, mass_cb_info.temperature, mass_cb_info.diseaseIdx, mass_cb_info.diseaseCount);
	}
}
