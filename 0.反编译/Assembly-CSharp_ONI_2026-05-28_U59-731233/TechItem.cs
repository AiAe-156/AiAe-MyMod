using System;
using System.Collections.Generic;
using UnityEngine;

public class TechItem : Resource, IHasDlcRestrictions
{
	public string description;

	public Func<string, bool, Sprite> getUISprite;

	public string parentTechId;

	public bool isPOIUnlock;

	[Obsolete("Use required/forbidden instead")]
	public string[] dlcIds;

	public string[] requiredDlcIds;

	public string[] forbiddenDlcIds;

	public List<string> searchTerms = new List<string>();

	public Tech ParentTech => Db.Get().Techs.Get(parentTechId);

	public string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public string[] GetForbiddenDlcIds()
	{
		return forbiddenDlcIds;
	}

	[Obsolete("Use constructor with requiredDlcIds and forbiddenDlcIds")]
	public TechItem(string id, ResourceSet parent, string name, string description, Func<string, bool, Sprite> getUISprite, string parentTechId, string[] dlcIds, bool isPOIUnlock = false)
		: base(id, parent, name)
	{
		this.description = description;
		this.getUISprite = getUISprite;
		this.parentTechId = parentTechId;
		this.isPOIUnlock = isPOIUnlock;
		DlcManager.ConvertAvailableToRequireAndForbidden(dlcIds, out requiredDlcIds, out forbiddenDlcIds);
	}

	public TechItem(string id, ResourceSet parent, string name, string description, Func<string, bool, Sprite> getUISprite, string parentTechId, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null, bool isPOIUnlock = false)
		: base(id, parent, name)
	{
		this.description = description;
		this.getUISprite = getUISprite;
		this.parentTechId = parentTechId;
		this.isPOIUnlock = isPOIUnlock;
		this.requiredDlcIds = requiredDlcIds;
		this.forbiddenDlcIds = forbiddenDlcIds;
	}

	public Sprite UISprite()
	{
		return getUISprite("ui", arg2: false);
	}

	public bool IsComplete()
	{
		return ParentTech.IsComplete() || IsPOIUnlocked();
	}

	private bool IsPOIUnlocked()
	{
		if (isPOIUnlock)
		{
			TechInstance techInstance = Research.Instance.Get(ParentTech);
			if (techInstance != null)
			{
				return techInstance.UnlockedPOITechIds.Contains(Id);
			}
		}
		return false;
	}

	public void POIUnlocked()
	{
		DebugUtil.DevAssert(isPOIUnlock, "Trying to unlock tech item " + Id + " via POI and it's not marked as POI unlockable.");
		if (isPOIUnlock && !IsComplete())
		{
			TechInstance techInstance = Research.Instance.Get(ParentTech);
			techInstance.UnlockPOITech(Id);
		}
	}

	public void AddSearchTerms(List<string> newSearchTerms)
	{
		foreach (string newSearchTerm in newSearchTerms)
		{
			searchTerms.Add(newSearchTerm);
		}
	}

	public void AddSearchTerms(string newSearchTerms)
	{
		SearchUtil.AddCommaDelimitedSearchTerms(newSearchTerms, searchTerms);
	}
}
