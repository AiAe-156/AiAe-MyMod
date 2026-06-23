namespace Database;

public class SimpleSkillPerk : SkillPerk
{
	public SimpleSkillPerk(string id, string description)
		: base(id, description, null, null, null, affectAll: false)
	{
	}

	public SimpleSkillPerk(string id, string description, string[] requiredDlcIds)
		: base(id, description, null, null, null, requiredDlcIds)
	{
	}
}
