using KSerialization;
using TUNING;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/ToiletWorkableClean")]
public class ToiletWorkableClean : Workable
{
	[Serialize]
	public int timesCleaned = 0;

	private static readonly HashedString[] CLEAN_GUNK_ANIMS = new HashedString[2] { "degunk_pre", "degunk_loop" };

	private static readonly HashedString[] CLEAN_ANIMS = new HashedString[2] { "unclog_pre", "unclog_loop" };

	private static readonly HashedString[] PST_ANIM = new HashedString[1]
	{
		new HashedString("unclog_pst")
	};

	private static readonly HashedString[] PST_GUNK_ANIM = new HashedString[1]
	{
		new HashedString("degunk_pst")
	};

	public bool IsCloggedByGunk { get; private set; }

	public void SetIsCloggedByGunk(bool isIt)
	{
		IsCloggedByGunk = isIt;
		workAnims = (IsCloggedByGunk ? CLEAN_GUNK_ANIMS : CLEAN_ANIMS);
		workingPstComplete = (IsCloggedByGunk ? PST_GUNK_ANIM : PST_ANIM);
		workingPstFailed = (IsCloggedByGunk ? PST_GUNK_ANIM : PST_ANIM);
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		workerStatusItem = Db.Get().DuplicantStatusItems.Cleaning;
		workingStatusItem = Db.Get().MiscStatusItems.Cleaning;
		attributeConverter = Db.Get().AttributeConverters.TidyingSpeed;
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
		skillExperienceSkillGroup = Db.Get().SkillGroups.Basekeeping.Id;
		skillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		ToiletWorkableUse component = base.gameObject.GetComponent<ToiletWorkableUse>();
		if (component != null && IsCloggedByGunk && base.gameObject.GetComponent<FlushToilet>() == null)
		{
			LiquidSourceManager.Instance.CreateChunk(SimHashes.LiquidGunk, component.lastAmountOfWasteMassRemovedFromDupe, DUPLICANTSTATS.STANDARD.Temperature.Internal.IDEAL, byte.MaxValue, 0, Grid.CellToPos(Grid.PosToCell(base.gameObject), CellAlignment.Top, Grid.SceneLayer.Ore));
		}
		timesCleaned++;
		base.OnCompleteWork(worker);
	}
}
