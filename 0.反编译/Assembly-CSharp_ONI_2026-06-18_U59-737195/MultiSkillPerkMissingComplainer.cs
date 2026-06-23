using System;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/MultiSkillPerkMissingComplainer")]
public class MultiSkillPerkMissingComplainer : KMonoBehaviour
{
	public string[] requiredSkillPerks;

	private KSelectable selectable;

	private int skillUpdateHandle = -1;

	private Guid workStatusItemHandle;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		selectable = GetComponent<KSelectable>();
		if (requiredSkillPerks != null && requiredSkillPerks.Length > 1)
		{
			skillUpdateHandle = Game.Instance.Subscribe(-1523247426, UpdateStatusItem);
		}
		else
		{
			Debug.LogWarning("Use SkillPerkMissingComplainer on " + base.gameObject.name + " if multiple skill perks are not required. This should only be used if a single dupe requires more than one perk.");
		}
		UpdateStatusItem();
	}

	protected override void OnCleanUp()
	{
		if (skillUpdateHandle != -1)
		{
			Game.Instance.Unsubscribe(skillUpdateHandle);
		}
		base.OnCleanUp();
	}

	protected virtual void UpdateStatusItem(object data = null)
	{
		if (!(selectable == null) && requiredSkillPerks != null)
		{
			bool flag = MinionResume.AnyMinionHasAllPerks(requiredSkillPerks, this.GetMyWorldId());
			if (!flag && workStatusItemHandle == Guid.Empty)
			{
				workStatusItemHandle = selectable.AddStatusItem(Db.Get().BuildingStatusItems.ColonyLacksDupeWithMultiSkillPerk, requiredSkillPerks);
			}
			else if (flag && workStatusItemHandle != Guid.Empty)
			{
				selectable.RemoveStatusItem(workStatusItemHandle);
				workStatusItemHandle = Guid.Empty;
			}
		}
	}
}
