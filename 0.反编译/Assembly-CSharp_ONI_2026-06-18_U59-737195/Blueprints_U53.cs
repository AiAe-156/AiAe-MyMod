using Database;

public class Blueprints_U53 : BlueprintProvider
{
	public override void SetupBlueprints()
	{
		AddBuilding("LuxuryBed", PermitRarity.Loyalty, "permit_elegantbed_hatch", "elegantbed_hatch_kanim");
		AddBuilding("LuxuryBed", PermitRarity.Loyalty, "permit_elegantbed_pipsqueak", "elegantbed_pipsqueak_kanim");
	}
}
