using Database;

public class MonumentPartInfo : IBlueprintInfo, IHasDlcRestrictions
{
	private readonly PermitRarity rarity_;

	public string state;

	public string symbolName;

	public MonumentPartResource.Part part;

	public string[] requiredDlcIds;

	public string[] forbiddenDlcIds;

	public string id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public PermitRarity rarity => rarity_;

	public string animFile { get; set; }

	public MonumentPartInfo(string id, string name, string desc, PermitRarity rarity, string animFilename, string state, string symbolName, MonumentPartResource.Part part, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
	{
		this.id = id;
		this.name = name;
		this.desc = desc;
		rarity_ = rarity;
		animFile = animFilename;
		this.state = state;
		this.symbolName = symbolName;
		this.part = part;
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
