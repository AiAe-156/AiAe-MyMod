using System;
using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class Deepfryer : ComplexFabricator, IGameObjectEffectDescriptor
{
	[SerializeField]
	private int diseaseCountKillRate = 100;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		choreType = Db.Get().ChoreTypes.Cook;
		fetchChoreTypeIdHash = Db.Get().ChoreTypes.CookFetch.IdHash;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		workable.requiredSkillPerk = Db.Get().SkillPerks.CanDeepFry.Id;
		workable.WorkerStatusItem = Db.Get().DuplicantStatusItems.Cooking;
		workable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_deepfryer_kanim") };
		workable.AttributeConverter = Db.Get().AttributeConverters.CookingSpeed;
		workable.AttributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.MOST_DAY_EXPERIENCE;
		workable.SkillExperienceSkillGroup = Db.Get().SkillGroups.Cooking.Id;
		workable.SkillExperienceMultiplier = SKILLS.MOST_DAY_EXPERIENCE;
		ComplexFabricatorWorkable complexFabricatorWorkable = workable;
		complexFabricatorWorkable.OnWorkTickActions = (Action<WorkerBase, float>)Delegate.Combine(complexFabricatorWorkable.OnWorkTickActions, (Action<WorkerBase, float>)delegate(WorkerBase worker, float dt)
		{
			Debug.Assert(worker != null, "How did we get a null worker?");
			if (diseaseCountKillRate > 0)
			{
				PrimaryElement component2 = GetComponent<PrimaryElement>();
				int num = Math.Max(1, (int)((float)diseaseCountKillRate * dt));
				component2.ModifyDiseaseCount(-num, "Deepfryer");
			}
		});
		ComplexFabricator component = GetComponent<ComplexFabricator>();
		component.workingStatusItem = Db.Get().BuildingStatusItems.ComplexFabricatorCooking;
	}

	protected override List<GameObject> SpawnOrderProduct(ComplexRecipe recipe)
	{
		List<GameObject> list = base.SpawnOrderProduct(recipe);
		foreach (GameObject item in list)
		{
			PrimaryElement component = item.GetComponent<PrimaryElement>();
			component.ModifyDiseaseCount(-component.DiseaseCount, "Deepfryer.CompleteOrder");
		}
		GetComponent<Operational>().SetActive(value: false);
		return list;
	}

	public override List<Descriptor> GetDescriptors(GameObject go)
	{
		List<Descriptor> descriptors = base.GetDescriptors(go);
		descriptors.Add(new Descriptor(UI.BUILDINGEFFECTS.REMOVES_DISEASE, UI.BUILDINGEFFECTS.TOOLTIPS.REMOVES_DISEASE));
		return descriptors;
	}
}
