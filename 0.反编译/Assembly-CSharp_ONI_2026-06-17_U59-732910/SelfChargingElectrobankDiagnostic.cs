using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class SelfChargingElectrobankDiagnostic : ColonyDiagnostic
{
	private float WARNING_LIFETIME = 600f;

	public SelfChargingElectrobankDiagnostic(int worldID)
		: base(worldID, UI.SELFCHARGINGBATTERYDIAGNOSTIC.ALL_NAME)
	{
		icon = "overlay_radiation";
		AddCriterion("CheckLifetime", new DiagnosticCriterion(UI.SELFCHARGINGBATTERYDIAGNOSTIC.CRITERIA.CHECKSELFCHARGINGBATTERYLIFE, CheckLifetime));
	}

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1.Concat(DlcManager.DLC3);
	}

	private DiagnosticResult CheckLifetime()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.SELFCHARGINGBATTERYDIAGNOSTIC.NORMAL);
		foreach (SelfChargingElectrobank item in Components.SelfChargingElectrobanks.GetItems(base.worldID))
		{
			if (!item.IsNullOrDestroyed() && item.LifetimeRemaining <= WARNING_LIFETIME)
			{
				result.opinion = DiagnosticResult.Opinion.Concern;
				if (result.clickThroughObjects == null)
				{
					result.clickThroughObjects = new List<GameObject>();
				}
				result.clickThroughObjects.Add(item.gameObject);
				result.Message = UI.SELFCHARGINGBATTERYDIAGNOSTIC.CRITERIA_BATTERYLIFE_WARNING;
			}
		}
		return result;
	}
}
