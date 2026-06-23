using System;
using System.Collections.Generic;
using TUNING;
using UnityEngine;

public class SushiBar : ComplexFabricator, IGameObjectEffectDescriptor
{
	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		choreType = Db.Get().ChoreTypes.Cook;
		fetchChoreTypeIdHash = Db.Get().ChoreTypes.CookFetch.IdHash;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		workable.requiredSkillPerk = Db.Get().SkillPerks.CanSushiBar.Id;
		workable.WorkerStatusItem = Db.Get().DuplicantStatusItems.Cooking;
		workable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interact_sushi_station_kanim") };
		workable.AttributeConverter = Db.Get().AttributeConverters.CookingSpeed;
		workable.AttributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.MOST_DAY_EXPERIENCE;
		workable.SkillExperienceSkillGroup = Db.Get().SkillGroups.Cooking.Id;
		workable.SkillExperienceMultiplier = SKILLS.MOST_DAY_EXPERIENCE;
		workable.workLayer = Grid.SceneLayer.BuildingUse;
		ComplexFabricatorWorkable complexFabricatorWorkable = workable;
		complexFabricatorWorkable.OnWorkTickActions = (Action<WorkerBase, float>)Delegate.Combine(complexFabricatorWorkable.OnWorkTickActions, (Action<WorkerBase, float>)delegate(WorkerBase worker, float dt)
		{
			Debug.Assert(worker != null, "How did we get a null worker?");
		});
		GetComponent<ComplexFabricator>().workingStatusItem = Db.Get().BuildingStatusItems.ComplexFabricatorCooking;
	}

	protected override List<GameObject> SpawnOrderProduct(ComplexRecipe recipe)
	{
		List<GameObject> result = base.SpawnOrderProduct(recipe);
		GetComponent<Operational>().SetActive(value: false);
		return result;
	}
}
