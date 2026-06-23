using System;
using TUNING;
using UnityEngine;

public class EmptyMilkSeparatorWorkable : Workable
{
	public System.Action OnWork_PST_Begins = null;

	private static readonly HashedString DROPPED_SYMBOL_HASH = "object";

	private const string DROPPED_SYMBOL_NAME = "object";

	private const string FAT_ON_HAND_SYMBOL_NAME = "fat_goop";

	private KBatchedAnimController droppedItemController;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		workLayer = Grid.SceneLayer.BuildingFront;
		workerStatusItem = Db.Get().DuplicantStatusItems.Cleaning;
		workingStatusItem = Db.Get().MiscStatusItems.Cleaning;
		overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_milk_separator_kanim") };
		attributeConverter = Db.Get().AttributeConverters.TidyingSpeed;
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
		skillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
		SetWorkTime(15f);
		synchronizeAnims = true;
		SetupDroppedItemSymbol();
	}

	private void SetupDroppedItemSymbol()
	{
		KBatchedAnimController component = base.gameObject.GetComponent<KBatchedAnimController>();
		GameObject gameObject = Util.NewGameObject(base.gameObject, base.gameObject.name + ".dropped_item_symbol");
		gameObject.SetActive(value: false);
		bool symbolVisible;
		Vector4 column = component.GetSymbolTransform(DROPPED_SYMBOL_HASH, out symbolVisible).GetColumn(3);
		Vector3 position = column;
		position.z = component.transform.GetPosition().z - 0.05f;
		gameObject.transform.SetPosition(position);
		droppedItemController = gameObject.AddComponent<KBatchedAnimController>();
		droppedItemController.AnimFiles = new KAnimFile[1] { Assets.GetAnim("milkfat_kanim") };
		droppedItemController.initialAnim = "idle1";
		component.SetSymbolVisiblity(DROPPED_SYMBOL_HASH, is_visible: false);
		KBatchedAnimTracker kBatchedAnimTracker = gameObject.AddComponent<KBatchedAnimTracker>();
		kBatchedAnimTracker.symbol = DROPPED_SYMBOL_HASH;
		kBatchedAnimTracker.offset = Vector3.zero;
	}

	public override void OnPendingCompleteWork(WorkerBase worker)
	{
		OnWork_PST_Begins?.Invoke();
		ShowDroppedItemSymbol();
		TintDupeFatInHandSymbol(worker);
		base.OnPendingCompleteWork(worker);
	}

	protected override void OnStopWork(WorkerBase worker)
	{
		HideDroppedItemSymbol();
		ClearDupeFatInHandColor(worker);
	}

	private void ShowDroppedItemSymbol()
	{
		MilkSeparator.Instance sMI = base.gameObject.GetSMI<MilkSeparator.Instance>();
		if (sMI != null)
		{
			bool flag = sMI.MilkFatStored >= sMI.CaviarStored;
			HashedString hashedString = (flag ? "milkfat_kanim" : "caviar_kanim");
			KAnimFile[] anims = new KAnimFile[1] { Assets.GetAnim(hashedString) };
			droppedItemController.SwapAnims(anims);
			droppedItemController.gameObject.SetActive(value: true);
			droppedItemController.Play(flag ? "idle2" : "object", KAnim.PlayMode.Loop);
		}
	}

	private void HideDroppedItemSymbol()
	{
		droppedItemController.gameObject.SetActive(value: false);
	}

	private void TintDupeFatInHandSymbol(WorkerBase worker)
	{
		if (!(worker == null))
		{
			MilkSeparator.Instance sMI = base.gameObject.GetSMI<MilkSeparator.Instance>();
			if (sMI != null)
			{
				KBatchedAnimController component = worker.GetComponent<KBatchedAnimController>();
				component.SetSymbolTint("fat_goop", sMI.GetFatColor());
			}
		}
	}

	private void ClearDupeFatInHandColor(WorkerBase worker)
	{
		if (!(worker == null))
		{
			KBatchedAnimController component = worker.GetComponent<KBatchedAnimController>();
			if (component != null)
			{
				component.SetSymbolTint("fat_goop", Color.white);
			}
		}
	}
}
