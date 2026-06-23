using Database;

public class Blueprints_DlcPack5 : BlueprintProvider
{
	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public override void SetupBlueprints()
	{
		AddBuilding("Headquarters", PermitRarity.Universal, "permit_hqbase_aquatic", "hqbase_aquatic_kanim");
	}
}
