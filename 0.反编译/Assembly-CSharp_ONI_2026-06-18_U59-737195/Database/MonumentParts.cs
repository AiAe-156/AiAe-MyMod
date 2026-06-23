using System.Collections.Generic;

namespace Database;

public class MonumentParts : ResourceSet<MonumentPartResource>
{
	public MonumentParts(ResourceSet parent)
		: base("MonumentParts", parent)
	{
		Initialize();
		foreach (MonumentPartInfo monumentPart in Blueprints.Get().all.monumentParts)
		{
			Add(monumentPart.id, monumentPart.name, monumentPart.desc, monumentPart.rarity, monumentPart.animFile, monumentPart.state, monumentPart.symbolName, monumentPart.part, monumentPart.requiredDlcIds, monumentPart.forbiddenDlcIds);
		}
	}

	public void Add(string id, string name, string desc, PermitRarity rarity, string animFilename, string state, string symbolName, MonumentPartResource.Part part, string[] requiredDlcIds, string[] forbiddenDlcIds)
	{
		MonumentPartResource item = new MonumentPartResource(id, name, desc, rarity, animFilename, state, symbolName, part, requiredDlcIds, forbiddenDlcIds);
		resources.Add(item);
	}

	public List<MonumentPartResource> GetParts(MonumentPartResource.Part part)
	{
		return resources.FindAll((MonumentPartResource mpr) => mpr.part == part);
	}
}
