using Database;

public class EquippableFacadeInfo : IBlueprintInfo, IHasDlcRestrictions
{
	private readonly PermitRarity rarity_;

	public string buildOverride;

	public string defID;

	public string[] requiredDlcIds;

	public string[] forbiddenDlcIds;

	public string id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public PermitRarity rarity => rarity_;

	public string animFile { get; set; }

	public EquippableFacadeInfo(string id, string name, string desc, PermitRarity rarity, string defID, string buildOverride, string animFile, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
	{
		this.id = id;
		this.name = name;
		this.desc = desc;
		rarity_ = rarity;
		this.defID = defID;
		this.buildOverride = buildOverride;
		this.animFile = animFile;
		this.requiredDlcIds = requiredDlcIds;
		this.forbiddenDlcIds = forbiddenDlcIds;
	}

	public string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public string[] GetForbiddenDlcIds()
	{
		return forbiddenDlcIds;
	}
}
