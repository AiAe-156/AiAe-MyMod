using TUNING;
using UnityEngine;

public class AbsorbCellQuery : PathFinderQuery
{
	public enum AbsorbOxygenSafeCellFlags
	{
		IsNotTube = 1,
		IsNotRadiated = 2,
		IsBreathable = 4,
		IsNotScaldingTemperatures = 8,
		IsClear = 0x10,
		IsNotLiquidOnMyFace = 0x20,
		IsNotLiquid = 0x40
	}

	private MinionBrain brain;

	private float scaldingTreshold = -1f;

	private int targetCell;

	private int targetCost;

	private float targetOxygenScore;

	private bool criticalMode;

	private float bionicOxygenRemaining;

	private float breathPercentage;

	private float targetBreathableMassAvailable;

	public AbsorbOxygenSafeCellFlags targetCellSafetyFlags;

	public float targetCellBreathabilityScore;

	private int allowCellEvenIfReserved = -1;

	private SafetyChecker checker;

	private SafetyChecker.Context context;

	private bool isRecoveringFromSuffocation;

	public AbsorbCellQuery()
	{
		checker = Game.Instance.safetyConditions.AbsorbCellCellChecker;
	}

	public AbsorbCellQuery Reset(MinionBrain brain, bool criticalMode, float currentOxygenTankMass, float breathPercentage, int allowCellEvenIfReserved, bool isRecoveringFromSuffocation)
	{
		this.brain = brain;
		targetCell = PathFinder.InvalidCell;
		targetCost = int.MaxValue;
		targetOxygenScore = float.MinValue;
		targetCellSafetyFlags = (AbsorbOxygenSafeCellFlags)0;
		targetBreathableMassAvailable = 0f;
		this.criticalMode = criticalMode;
		bionicOxygenRemaining = currentOxygenTankMass;
		this.breathPercentage = breathPercentage;
		this.allowCellEvenIfReserved = allowCellEvenIfReserved;
		context = new SafetyChecker.Context(brain);
		scaldingTreshold = ((brain == null) ? null : brain.GetSMI<ScaldingMonitor.Instance>())?.GetScaldingThreshold() ?? (-1f);
		this.isRecoveringFromSuffocation = isRecoveringFromSuffocation;
		return this;
	}

	public static AbsorbOxygenSafeCellFlags GetAbsorbOxygenFlags(int cell, MinionBrain brain, float scaldingTreshold, out float totalBreathableMassAroundCell, out float breathableCellRatioInSample, int allowCellEvenIfReserved)
	{
		totalBreathableMassAroundCell = 0f;
		breathableCellRatioInSample = 0f;
		int num = Grid.CellAbove(cell);
		if (!Grid.IsValidCell(num))
		{
			return (AbsorbOxygenSafeCellFlags)0;
		}
		if (Grid.Solid[cell] || Grid.Solid[num])
		{
			return (AbsorbOxygenSafeCellFlags)0;
		}
		if (Grid.IsTileUnderConstruction[cell] || Grid.IsTileUnderConstruction[num])
		{
			return (AbsorbOxygenSafeCellFlags)0;
		}
		bool flag = cell == allowCellEvenIfReserved || brain.IsCellClear(cell);
		bool flag2 = !Grid.Element[cell].IsLiquid;
		bool flag3 = !Grid.Element[num].IsLiquid;
		bool flag4 = scaldingTreshold < 0f || Grid.Temperature[cell] < scaldingTreshold;
		bool flag5 = Grid.Radiation[cell] < 250f;
		bool flag6 = false;
		if (brain.OxygenBreather != null)
		{
			for (int i = 0; i < GasBreatherFromWorldProvider.DEFAULT_BREATHABLE_OFFSETS.Length; i++)
			{
				int num2 = Grid.OffsetCell(cell, GasBreatherFromWorldProvider.DEFAULT_BREATHABLE_OFFSETS[i]);
				if (Grid.IsValidCell(num2) && Grid.AreCellsInSameWorld(cell, num2) && Grid.Element[num2].HasTag(GameTags.Breathable))
				{
					breathableCellRatioInSample += 1f / (float)GasBreatherFromWorldProvider.DEFAULT_BREATHABLE_OFFSETS.Length;
				}
			}
			flag6 = GasBreatherFromWorldProvider.GetBestBreathableCellAroundSpecificCell(cell, GasBreatherFromWorldProvider.DEFAULT_BREATHABLE_OFFSETS, brain.OxygenBreather, out totalBreathableMassAroundCell).IsBreathable;
		}
		bool num3 = !brain.Navigator.NavGrid.NavTable.IsValid(cell, NavType.Tube);
		AbsorbOxygenSafeCellFlags absorbOxygenSafeCellFlags = (AbsorbOxygenSafeCellFlags)0;
		if (flag4)
		{
			absorbOxygenSafeCellFlags |= AbsorbOxygenSafeCellFlags.IsNotScaldingTemperatures;
		}
		if (flag5)
		{
			absorbOxygenSafeCellFlags |= AbsorbOxygenSafeCellFlags.IsNotRadiated;
		}
		if (flag6)
		{
			absorbOxygenSafeCellFlags |= AbsorbOxygenSafeCellFlags.IsBreathable;
		}
		if (flag)
		{
			absorbOxygenSafeCellFlags |= AbsorbOxygenSafeCellFlags.IsClear;
		}
		if (num3)
		{
			absorbOxygenSafeCellFlags |= AbsorbOxygenSafeCellFlags.IsNotTube;
		}
		if (flag2)
		{
			absorbOxygenSafeCellFlags |= AbsorbOxygenSafeCellFlags.IsNotLiquid;
		}
		if (flag3)
		{
			absorbOxygenSafeCellFlags |= AbsorbOxygenSafeCellFlags.IsNotLiquidOnMyFace;
		}
		return absorbOxygenSafeCellFlags;
	}

	public override bool IsMatch(int cell, int parent_cell, int cost)
	{
		float num = 0.1f * (float)GasBreatherFromWorldProvider.DEFAULT_BREATHABLE_OFFSETS.Length;
		float num2 = 2.5f * (float)GasBreatherFromWorldProvider.DEFAULT_BREATHABLE_OFFSETS.Length;
		float num3 = 54 / GasBreatherFromWorldProvider.DEFAULT_BREATHABLE_OFFSETS.Length;
		float num4 = num3 / 2.8f;
		float num5 = cost;
		checker.GetSafetyConditions(cell, cost, context, out var all_conditions_met);
		if (all_conditions_met)
		{
			float num6 = num5 / 10f;
			float num7 = 0.03f * num6;
			float totalBreathableMassAroundCell = 0f;
			float breathableCellRatioInSample = 0f;
			AbsorbOxygenSafeCellFlags absorbOxygenFlags = GetAbsorbOxygenFlags(cell, brain, scaldingTreshold, out totalBreathableMassAroundCell, out breathableCellRatioInSample, allowCellEvenIfReserved);
			totalBreathableMassAroundCell = Mathf.Clamp(totalBreathableMassAroundCell, 0f, num3);
			if (criticalMode)
			{
				if (!isRecoveringFromSuffocation && breathPercentage > DUPLICANTSTATS.BIONICS.Breath.SUFFOCATE_AMOUNT && totalBreathableMassAroundCell < num)
				{
					totalBreathableMassAroundCell = 0f;
				}
			}
			else if (totalBreathableMassAroundCell < num4)
			{
				totalBreathableMassAroundCell = 0f;
			}
			float num8 = (float)absorbOxygenFlags;
			float num9 = 10f * breathableCellRatioInSample;
			float num10 = totalBreathableMassAroundCell * num9 - num7;
			bool flag = false;
			if (targetCell == Grid.InvalidCell)
			{
				flag = true;
			}
			bool flag2 = targetBreathableMassAvailable > 0f;
			bool flag3 = num5 < (float)targetCost;
			bool flag4 = targetOxygenScore >= num2;
			bool flag5 = num8 >= (float)targetCellSafetyFlags || !flag2;
			float num11 = targetOxygenScore;
			if (criticalMode)
			{
				num11 = Mathf.Min(num2, num11);
			}
			if (num10 >= num11 && flag5)
			{
				if (criticalMode)
				{
					if (flag3 || !flag4)
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
			}
			if (flag && totalBreathableMassAroundCell > DUPLICANTSTATS.BIONICS.BaseStats.NO_OXYGEN_THRESHOLD)
			{
				targetBreathableMassAvailable = totalBreathableMassAroundCell;
				targetCellSafetyFlags = absorbOxygenFlags;
				targetCost = cost;
				targetCell = cell;
				targetOxygenScore = num10;
			}
		}
		return false;
	}

	public override int GetResultCell()
	{
		return targetCell;
	}
}
