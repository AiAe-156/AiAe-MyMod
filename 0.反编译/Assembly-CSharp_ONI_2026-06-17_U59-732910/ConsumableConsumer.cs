using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using KSerialization;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/ConsumableConsumer")]
public class ConsumableConsumer : KMonoBehaviour
{
	[Obsolete("Deprecated, use forbiddenTagSet")]
	[Serialize]
	[HideInInspector]
	public Tag[] forbiddenTags;

	[Serialize]
	public HashSet<Tag> forbiddenTagSet;

	public HashSet<Tag> dietaryRestrictionTagSet;

	public System.Action consumableRulesChanged;

	[OnDeserialized]
	[Obsolete]
	private void OnDeserialized()
	{
		if (SaveLoader.Instance.GameInfo.IsVersionOlderThan(7, 29))
		{
			forbiddenTagSet = new HashSet<Tag>(forbiddenTags);
			forbiddenTags = null;
		}
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		if (ConsumerManager.instance != null)
		{
			forbiddenTagSet = new HashSet<Tag>(ConsumerManager.instance.DefaultForbiddenTagsList);
			SetModelDietaryRestrictions();
		}
		else
		{
			forbiddenTagSet = new HashSet<Tag>();
			dietaryRestrictionTagSet = new HashSet<Tag>();
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		SetModelDietaryRestrictions();
	}

	private void SetModelDietaryRestrictions()
	{
		if (this.HasTag(GameTags.Minions.Models.Standard))
		{
			dietaryRestrictionTagSet = new HashSet<Tag>(ConsumerManager.instance.StandardDuplicantDietaryRestrictions);
		}
		else if (this.HasTag(GameTags.Minions.Models.Bionic))
		{
			dietaryRestrictionTagSet = new HashSet<Tag>(ConsumerManager.instance.BionicDuplicantDietaryRestrictions);
		}
	}

	public bool IsPermitted(string consumable_id)
	{
		Tag item = new Tag(consumable_id);
		if (!forbiddenTagSet.Contains(item))
		{
			return !dietaryRestrictionTagSet.Contains(item);
		}
		return false;
	}

	public bool IsDietRestricted(string consumable_id)
	{
		Tag item = new Tag(consumable_id);
		return dietaryRestrictionTagSet.Contains(item);
	}

	public void SetPermitted(string consumable_id, bool is_allowed)
	{
		Tag item = new Tag(consumable_id);
		is_allowed = is_allowed && !dietaryRestrictionTagSet.Contains(consumable_id);
		if (is_allowed)
		{
			forbiddenTagSet.Remove(item);
		}
		else
		{
			forbiddenTagSet.Add(item);
		}
		consumableRulesChanged.Signal();
	}
}
