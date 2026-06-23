using System;

namespace Database;

public class SkillPerk : Resource, IHasDlcRestrictions
{
	public string[] requiredDlcIds;

	public Action<MinionResume> OnApply { get; protected set; }

	public Action<MinionResume> OnRemove { get; protected set; }

	public Action<MinionResume> OnMinionsChanged { get; protected set; }

	public bool affectAll { get; protected set; }

	public string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public static string GetDescription(string perkID)
	{
		string text = GameUtil.NamesOfBuildingsRequiringSkillPerk(perkID);
		if (text == null)
		{
			return Db.Get().SkillPerks.Get(perkID).Name;
		}
		return text;
	}

	public SkillPerk(string id_str, string description, Action<MinionResume> OnApply, Action<MinionResume> OnRemove, Action<MinionResume> OnMinionsChanged, bool affectAll = false)
		: base(id_str, description)
	{
		this.OnApply = OnApply;
		this.OnRemove = OnRemove;
		this.OnMinionsChanged = OnMinionsChanged;
		this.affectAll = affectAll;
	}

	public SkillPerk(string id_str, string description, Action<MinionResume> OnApply, Action<MinionResume> OnRemove, Action<MinionResume> OnMinionsChanged, string[] requiredDlcIds = null, bool affectAll = false)
		: base(id_str, description)
	{
		this.OnApply = OnApply;
		this.OnRemove = OnRemove;
		this.OnMinionsChanged = OnMinionsChanged;
		this.affectAll = affectAll;
		this.requiredDlcIds = requiredDlcIds;
	}
}
