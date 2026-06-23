using System.Collections.Generic;
using System.Runtime.Serialization;
using Database;
using KSerialization;
using Klei.AI;
using TUNING;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/Artable")]
public class Artable : Workable
{
	[Serialize]
	private string currentStage;

	[Serialize]
	private string userChosenTargetStage;

	private AttributeModifier artQualityDecorModifier;

	public const string defaultArtworkId = "Default";

	public string defaultAnimName;

	public bool onlyWorkableWhenOperational = true;

	private WorkChore<Artable> chore;

	public string CurrentStage => currentStage;

	protected Artable()
	{
		faceTargetWhenWorking = true;
		if (string.IsNullOrEmpty(requiredSkillPerk))
		{
			requiredSkillPerk = Db.Get().SkillPerks.CanArt.Id;
		}
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		workerStatusItem = Db.Get().DuplicantStatusItems.Arting;
		attributeConverter = Db.Get().AttributeConverters.ArtSpeed;
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.MOST_DAY_EXPERIENCE;
		skillExperienceSkillGroup = Db.Get().SkillGroups.Art.Id;
		skillExperienceMultiplier = SKILLS.MOST_DAY_EXPERIENCE;
		SetWorkTime(80f);
	}

	protected override void OnSpawn()
	{
		Tag tag = GetComponent<KPrefabID>().PrefabID();
		if (string.IsNullOrEmpty(currentStage) || currentStage == "Default")
		{
			SetDefault();
		}
		else
		{
			SetStage(currentStage, skip_effect: true);
		}
		shouldShowSkillPerkStatusItem = false;
		base.OnSpawn();
	}

	[OnDeserialized]
	public void OnDeserialized()
	{
		if (Db.GetArtableStages().TryGet(currentStage) == null && currentStage != "Default")
		{
			string id = $"{GetComponent<KPrefabID>().PrefabID().ToString()}_{currentStage}";
			if (Db.GetArtableStages().TryGet(id) == null)
			{
				Debug.LogWarning("Failed up to update " + currentStage + " to ArtableStages");
				currentStage = "Default";
			}
			else
			{
				currentStage = id;
			}
		}
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		if (string.IsNullOrEmpty(userChosenTargetStage))
		{
			Db db = Db.Get();
			Tag prefab_id = GetComponent<KPrefabID>().PrefabID();
			List<ArtableStage> prefabStages = Db.GetArtableStages().GetPrefabStages(prefab_id);
			ArtableStatusItem artist_skill = db.ArtableStatuses.LookingUgly;
			MinionResume component = worker.GetComponent<MinionResume>();
			if (component != null)
			{
				if (component.HasPerk(db.SkillPerks.CanArtGreat.Id))
				{
					artist_skill = db.ArtableStatuses.LookingGreat;
				}
				else if (component.HasPerk(db.SkillPerks.CanArtOkay.Id))
				{
					artist_skill = db.ArtableStatuses.LookingOkay;
				}
			}
			prefabStages.RemoveAll((ArtableStage stage) => stage.statusItem.StatusType > artist_skill.StatusType || stage.statusItem.StatusType == ArtableStatuses.ArtableStatusType.AwaitingArting);
			prefabStages.Sort((ArtableStage x, ArtableStage y) => y.statusItem.StatusType.CompareTo(x.statusItem.StatusType));
			ArtableStatuses.ArtableStatusType highest_type = prefabStages[0].statusItem.StatusType;
			prefabStages.RemoveAll((ArtableStage stage) => stage.statusItem.StatusType < highest_type);
			prefabStages.RemoveAll((ArtableStage stage) => !stage.IsUnlocked());
			prefabStages.Shuffle();
			SetStage(prefabStages[0].id, skip_effect: false);
			if (prefabStages[0].cheerOnComplete)
			{
				new EmoteChore(worker.GetComponent<ChoreProvider>(), db.ChoreTypes.EmoteHighPriority, db.Emotes.Minion.Cheer);
			}
			else
			{
				new EmoteChore(worker.GetComponent<ChoreProvider>(), db.ChoreTypes.EmoteHighPriority, db.Emotes.Minion.Disappointed);
			}
		}
		else
		{
			SetStage(userChosenTargetStage, skip_effect: false);
			userChosenTargetStage = null;
		}
		shouldShowSkillPerkStatusItem = false;
		UpdateStatusItem();
		Prioritizable.RemoveRef(base.gameObject);
	}

	public void SetDefault()
	{
		currentStage = "Default";
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		component.SwapAnims(GetComponent<Building>().Def.AnimFiles);
		GetComponent<KAnimControllerBase>().Play(defaultAnimName);
		KSelectable component2 = GetComponent<KSelectable>();
		BuildingDef def = GetComponent<Building>().Def;
		component2.SetName(def.Name);
		component2.SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().ArtableStatuses.AwaitingArting, this);
		this.GetAttributes().Remove(artQualityDecorModifier);
		shouldShowSkillPerkStatusItem = false;
		UpdateStatusItem();
		if (currentStage == "Default")
		{
			shouldShowSkillPerkStatusItem = true;
			Prioritizable.AddRef(base.gameObject);
			chore = new WorkChore<Artable>(Db.Get().ChoreTypes.Art, this, null, run_until_complete: true, null, null, null, allow_in_red_alert: true, null, ignore_schedule_block: false, onlyWorkableWhenOperational);
			chore.AddPrecondition(ChorePreconditions.instance.HasSkillPerk, requiredSkillPerk);
		}
		RefreshDecorTag();
		Trigger(111068960, (object)currentStage);
	}

	public virtual void SetStage(string stage_id, bool skip_effect)
	{
		ArtableStage artableStage = Db.GetArtableStages().Get(stage_id);
		if (artableStage == null)
		{
			Debug.LogError("Missing stage: " + stage_id);
			return;
		}
		currentStage = artableStage.id;
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		component.SwapAnims(new KAnimFile[1] { Assets.GetAnim(artableStage.animFile) });
		GetComponent<KAnimControllerBase>().Play(artableStage.anim);
		this.GetAttributes().Remove(artQualityDecorModifier);
		if (artableStage.decor != 0)
		{
			artQualityDecorModifier = new AttributeModifier(Db.Get().BuildingAttributes.Decor.Id, artableStage.decor, "Art Quality");
			this.GetAttributes().Add(artQualityDecorModifier);
		}
		KSelectable component2 = GetComponent<KSelectable>();
		component2.SetName(artableStage.Name);
		component2.SetStatusItem(Db.Get().StatusItemCategories.Main, artableStage.statusItem, this);
		BuildingComplete component3 = base.gameObject.GetComponent<BuildingComplete>();
		component3.SetDescriptionFlavour(artableStage.Description);
		shouldShowSkillPerkStatusItem = false;
		UpdateStatusItem();
		RefreshDecorTag();
		Trigger(111068960, (object)currentStage);
	}

	public void SetUserChosenTargetState(string stageID)
	{
		SetDefault();
		userChosenTargetStage = stageID;
	}

	public void RefreshDecorTag()
	{
		KPrefabID component = GetComponent<KPrefabID>();
		bool flag = component.HasTag(GameTags.Decoration);
		bool flag2 = CurrentStage != null && currentStage != "Default";
		if (flag2)
		{
			component.AddTag(GameTags.Decoration);
		}
		else
		{
			component.RemoveTag(GameTags.Decoration);
		}
		if (flag2 != flag)
		{
			Game.Instance.roomProber.TriggerBuildingChangedEvent(Grid.PosToCell(base.gameObject), component);
		}
	}
}
