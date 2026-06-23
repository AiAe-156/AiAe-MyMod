using System;

namespace Database;

public class EquippableFacades : ResourceSet<EquippableFacadeResource>
{
	public EquippableFacades(ResourceSet parent)
		: base("EquippableFacades", parent)
	{
		Initialize();
		foreach (EquippableFacadeInfo equippableFacade in Blueprints.Get().all.equippableFacades)
		{
			Add(equippableFacade.id, equippableFacade.name, equippableFacade.desc, equippableFacade.rarity, equippableFacade.defID, equippableFacade.buildOverride, equippableFacade.animFile, equippableFacade.GetRequiredDlcIds(), equippableFacade.GetForbiddenDlcIds());
		}
	}

	[Obsolete("Please use Add(...) with required forbidden")]
	public void Add(string id, string name, string desc, PermitRarity rarity, string defID, string buildOverride, string animFile)
	{
		Add(id, name, desc, rarity, defID, buildOverride, animFile, null, null);
	}

	[Obsolete("Please use Add(...) with required forbidden")]
	public void Add(string id, string name, string desc, PermitRarity rarity, string defID, string buildOverride, string animFile, string[] dlcIds)
	{
		DlcRestrictionsUtil.TemporaryHelperObject transientHelperObjectFromAllowList = DlcRestrictionsUtil.GetTransientHelperObjectFromAllowList(dlcIds);
		EquippableFacadeResource item = new EquippableFacadeResource(id, name, desc, rarity, buildOverride, defID, animFile, transientHelperObjectFromAllowList.GetRequiredDlcIds(), transientHelperObjectFromAllowList.GetForbiddenDlcIds());
		resources.Add(item);
	}

	public void Add(string id, string name, string desc, PermitRarity rarity, string defID, string buildOverride, string animFile, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
	{
		EquippableFacadeResource item = new EquippableFacadeResource(id, name, desc, rarity, buildOverride, defID, animFile, requiredDlcIds, forbiddenDlcIds);
		resources.Add(item);
	}
}
