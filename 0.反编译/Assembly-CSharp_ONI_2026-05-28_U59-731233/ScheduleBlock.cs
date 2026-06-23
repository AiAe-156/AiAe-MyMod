using System;
using System.Collections.Generic;
using KSerialization;

[Serializable]
public class ScheduleBlock
{
	[Serialize]
	public string name;

	[Serialize]
	private string _groupId;

	public List<ScheduleBlockType> allowed_types
	{
		get
		{
			Debug.Assert(!string.IsNullOrEmpty(_groupId));
			return Db.Get().ScheduleGroups.Get(_groupId).allowedTypes;
		}
	}

	public string GroupId
	{
		get
		{
			return _groupId;
		}
		set
		{
			_groupId = value;
		}
	}

	public ScheduleBlock(string name, string groupId)
	{
		this.name = name;
		_groupId = groupId;
	}

	public bool IsAllowed(ScheduleBlockType type)
	{
		if (allowed_types != null)
		{
			foreach (ScheduleBlockType allowed_type in allowed_types)
			{
				if (type.IdHash == allowed_type.IdHash)
				{
					return true;
				}
			}
		}
		return false;
	}
}
