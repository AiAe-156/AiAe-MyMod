using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class BedDiagnostic : ColonyDiagnostic
{
	private List<MinionIdentity> minionsWithStamina;

	private const bool INCLUDE_CHILD_WORLDS = true;

	private readonly string NO_MINIONS_WITH_STAMINA;

	public BedDiagnostic(int worldID)
		: base(worldID, UI.COLONY_DIAGNOSTICS.BEDDIAGNOSTIC.ALL_NAME)
	{
		icon = "icon_action_region_bedroom";
		AddCriterion("CheckEnoughBeds", new DiagnosticCriterion(UI.COLONY_DIAGNOSTICS.BEDDIAGNOSTIC.CRITERIA.CHECKENOUGHBEDS, CheckEnoughBeds));
		AddCriterion("CheckReachability", new DiagnosticCriterion(UI.COLONY_DIAGNOSTICS.BEDDIAGNOSTIC.CRITERIA.CHECKREACHABILITY, CheckReachability));
		NO_MINIONS_WITH_STAMINA = (base.IsWorldModuleInterior ? UI.COLONY_DIAGNOSTICS.BEDDIAGNOSTIC.NO_MINIONS_ROCKET : UI.COLONY_DIAGNOSTICS.BEDDIAGNOSTIC.NO_MINIONS_PLANETOID);
	}

	private DiagnosticResult CheckEnoughBeds()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.GENERIC_CRITERIA_PASS);
		result.opinion = DiagnosticResult.Opinion.Normal;
		result.Message = UI.COLONY_DIAGNOSTICS.BEDDIAGNOSTIC.NORMAL;
		if (minionsWithStamina.Count == 0)
		{
			result.opinion = DiagnosticResult.Opinion.Normal;
			result.Message = NO_MINIONS_WITH_STAMINA;
		}
		else
		{
			int globalCount = Components.NormalBeds.GlobalCount;
			if (globalCount < minionsWithStamina.Count)
			{
				result.opinion = DiagnosticResult.Opinion.Concern;
				result.Message = UI.COLONY_DIAGNOSTICS.BEDDIAGNOSTIC.NOT_ENOUGH_BEDS;
			}
		}
		return result;
	}

	private DiagnosticResult CheckReachability()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.GENERIC_CRITERIA_PASS);
		result.opinion = DiagnosticResult.Opinion.Normal;
		result.Message = UI.COLONY_DIAGNOSTICS.BEDDIAGNOSTIC.NORMAL;
		if (minionsWithStamina.Count == 0)
		{
			result.opinion = DiagnosticResult.Opinion.Normal;
			result.Message = NO_MINIONS_WITH_STAMINA;
		}
		else
		{
			ListPool<Sleepable, BedDiagnostic>.PooledList pooledList = ListPool<Sleepable, BedDiagnostic>.Allocate();
			foreach (Sleepable item in Components.NormalBeds.WorldItemsEnumerate(base.worldID, checkChildWorlds: true))
			{
				if (item.assignable != null && !item.assignable.IsAssigned())
				{
					pooledList.Add(item);
				}
			}
			foreach (MinionIdentity item2 in minionsWithStamina)
			{
				Navigator component = item2.GetComponent<Navigator>();
				AssignableSlotInstance slot = item2.assignableProxy.Get().GetComponent<Ownables>().GetSlot(Db.Get().AssignableSlots.Bed);
				if (!slot.IsAssigned() && result.opinion == DiagnosticResult.Opinion.Normal)
				{
					Sleepable sleepable = null;
					foreach (Sleepable item3 in pooledList)
					{
						if (component.CanReach(item3.approachable) && item3.assignable.CanAutoAssignTo(item2))
						{
							sleepable = item3;
							break;
						}
					}
					if (sleepable != null)
					{
						pooledList.Remove(sleepable);
						continue;
					}
					result.opinion = DiagnosticResult.Opinion.Concern;
					result.Message = UI.COLONY_DIAGNOSTICS.BEDDIAGNOSTIC.MISSING_ASSIGNMENT;
					result.clickThroughTarget = new Tuple<Vector3, GameObject>(item2.gameObject.transform.position, item2.gameObject);
				}
				else if (slot.IsAssigned() && !component.CanReach(Grid.PosToCell(slot.assignable)))
				{
					result.opinion = DiagnosticResult.Opinion.Concern;
					result.Message = UI.COLONY_DIAGNOSTICS.BEDDIAGNOSTIC.CANT_REACH;
					result.clickThroughTarget = new Tuple<Vector3, GameObject>(item2.gameObject.transform.position, item2.gameObject);
					break;
				}
			}
			pooledList.Recycle();
		}
		return result;
	}

	public override DiagnosticResult Evaluate()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, NO_MINIONS_WITH_STAMINA);
		if (ColonyDiagnosticUtility.IgnoreRocketsWithNoCrewRequested(base.worldID, out result))
		{
			return result;
		}
		RefreshData();
		return base.Evaluate();
	}

	private void RefreshData()
	{
		minionsWithStamina = Components.LiveMinionIdentities.GetWorldItems(base.worldID, checkChildWorlds: true, MinionFilter);
	}

	private bool MinionFilter(MinionIdentity minion)
	{
		return minion.modifiers.amounts.Has(Db.Get().Amounts.Stamina);
	}

	public override string GetAverageValueString()
	{
		if (minionsWithStamina == null)
		{
			RefreshData();
		}
		return Components.NormalBeds.CountWorldItems(base.worldID, includeChildren: true) + "/" + minionsWithStamina.Count;
	}
}
