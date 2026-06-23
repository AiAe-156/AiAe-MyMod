using System.Collections.Generic;

public abstract class BionicColonyDiagnostic : ColonyDiagnostic
{
	protected const bool INCLUDE_CHILD_WORLDS = true;

	protected List<MinionIdentity> bionics;

	protected bool ignoreInIdleRockets = true;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC3;
	}

	public BionicColonyDiagnostic(int worldID, string name)
		: base(worldID, name)
	{
		RefreshData();
	}

	protected void RefreshData()
	{
		if (Components.LiveMinionIdentitiesByModel.TryGetValue(BionicMinionConfig.MODEL, out var value))
		{
			bionics = value.GetWorldItems(base.worldID, checkChildWorlds: true, MinionFilter);
		}
		else
		{
			bionics = new List<MinionIdentity>();
		}
	}

	protected virtual bool MinionFilter(MinionIdentity minion)
	{
		return true;
	}

	public override DiagnosticResult Evaluate()
	{
		if (ignoreInIdleRockets && ColonyDiagnosticUtility.IgnoreRocketsWithNoCrewRequested(base.worldID, out var result))
		{
			return result;
		}
		RefreshData();
		result = base.Evaluate();
		if (result.opinion == DiagnosticResult.Opinion.Normal)
		{
			result.Message = GetDefaultResultMessage();
		}
		return result;
	}

	protected abstract string GetDefaultResultMessage();
}
