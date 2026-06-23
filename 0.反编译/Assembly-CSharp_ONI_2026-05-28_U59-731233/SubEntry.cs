using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class SubEntry : IHasDlcRestrictions
{
	public ContentContainer lockedContentContainer;

	public Color iconColor = Color.white;

	public List<ContentContainer> contentContainers { get; set; }

	public string parentEntryID { get; set; }

	public string id { get; set; }

	public string name { get; set; }

	public string title { get; set; }

	public string subtitle { get; set; }

	public Sprite icon { get; set; }

	public int layoutPriority { get; set; }

	public bool disabled { get; set; }

	public string lockID { get; set; }

	public string[] requiredAtLeastOneDlcIds { get; set; }

	public string[] requiredDlcIds { get; set; }

	public string[] forbiddenDlcIds { get; set; }

	public string sortString { get; set; }

	public SubEntry()
	{
	}

	public SubEntry(string id, string parentEntryID, List<ContentContainer> contentContainers, string name)
	{
		this.id = id;
		this.parentEntryID = parentEntryID;
		this.name = name;
		this.contentContainers = contentContainers;
		if (!string.IsNullOrEmpty(lockID))
		{
			foreach (ContentContainer contentContainer in contentContainers)
			{
				contentContainer.lockID = lockID;
			}
		}
		if (string.IsNullOrEmpty(sortString))
		{
			if (!string.IsNullOrEmpty(title))
			{
				sortString = UI.StripLinkFormatting(title);
			}
			else
			{
				sortString = UI.StripLinkFormatting(name);
			}
		}
	}

	public string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public string[] GetForbiddenDlcIds()
	{
		return forbiddenDlcIds;
	}

	public string[] GetAnyRequiredDlcIds()
	{
		return requiredAtLeastOneDlcIds;
	}
}
