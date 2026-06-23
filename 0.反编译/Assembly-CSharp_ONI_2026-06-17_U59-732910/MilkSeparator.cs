using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class MilkSeparator : GameStateMachine<MilkSeparator, MilkSeparator.Instance, IStateMachineTarget, MilkSeparator.Def>
{
	public class Def : BaseDef, IConverterByproduct
	{
		public float MILK_FAT_CAPACITY = 100f;

		public float CAVIAR_PRODUCTION_RATE;

		public Tag MILK_TAG;

		public Tag MILK_FAT_TAG;

		public Tag CAVIAR_TAG;

		public Tag FISHMILK_TAG;

		public Tag MILK_SEPARATED_LIQUID_OUTPUT_TAG;

		public Tag FISHMILK_SEPARATED_LIQUID_OUTPUT_TAG;

		public Tag ByproductAssociatedInputTag => FISHMILK_TAG;

		public Tag ByproductTag => CAVIAR_TAG;

		public float ByproductRate => CAVIAR_PRODUCTION_RATE;

		public bool ByproductIsContinuous => true;

		public Def()
		{
			MILK_FAT_TAG = ElementLoader.FindElementByHash(SimHashes.MilkFat).tag;
			MILK_TAG = ElementLoader.FindElementByHash(SimHashes.Milk).tag;
			FISHMILK_TAG = ElementLoader.FindElementByHash(SimHashes.FishMilk).tag;
			MILK_SEPARATED_LIQUID_OUTPUT_TAG = ElementLoader.FindElementByHash(SimHashes.Brine).tag;
			FISHMILK_SEPARATED_LIQUID_OUTPUT_TAG = ElementLoader.FindElementByHash(SimHashes.Mucus).tag;
			CAVIAR_TAG = new Tag("Caviar");
		}

		public void GetByproductDescriptors(GameObject obj, List<Descriptor> descriptors)
		{
			if (!(CAVIAR_PRODUCTION_RATE <= 0f))
			{
				string arg = CAVIAR_TAG.ProperName();
				string formattedMass = GameUtil.GetFormattedMass(CAVIAR_PRODUCTION_RATE, GameUtil.TimeSlice.PerSecond, GameUtil.MetricMassFormat.UseThreshold, includeSuffix: true, "{0:0.##}");
				descriptors.Add(new Descriptor(string.Format(UI.BUILDINGEFFECTS.ELEMENTEMITTED_INPUTTEMP, arg, formattedMass), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.ELEMENTEMITTED_INPUTTEMP, arg, formattedMass)));
			}
		}
	}

	public class WorkingStates : State
	{
		public State pre;

		public State work;

		public State post;
	}

	public class OperationalStates : State
	{
		public State idle;

		public WorkingStates working;

		public State full;

		public State emptyComplete;
	}

	public new class Instance : GameInstance
	{
		[MyCmpGet]
		public EmptyMilkSeparatorWorkable workable;

		[MyCmpGet]
		public Operational operational;

		[MyCmpGet]
		private Storage storage;

		private ElementConverter[] elementConverters;

		private SymbolOverrideController symbolOverrideController;

		private float caviarMassAccumulated;

		private KBatchedAnimController animController;

		private MeterController fatMeter;

		private Tag lastObjectDispensedTag = Tag.Invalid;

		public float SolidOutputStored => MilkFatStored + CaviarStored;

		public float CaviarStored => storage.GetMassAvailable(base.def.CAVIAR_TAG);

		public float MilkFatStored => storage.GetMassAvailable(base.def.MILK_FAT_TAG);

		public float MilkStored => storage.GetMassAvailable(base.def.MILK_TAG);

		public float FishMilkStored => storage.GetMassAvailable(base.def.FISHMILK_TAG);

		public float SolidOutputStoragePercentage => Mathf.Clamp(SolidOutputStored / base.def.MILK_FAT_CAPACITY, 0f, 1f);

		public bool MilkFatLimitReached => SolidOutputStored >= base.def.MILK_FAT_CAPACITY;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			animController = GetComponent<KBatchedAnimController>();
			fatMeter = new MeterController(animController, "meter_target_1", "meter_fat", Meter.Offset.Infront, Grid.SceneLayer.NoLayer, "meter_target_1");
			elementConverters = master.gameObject.GetComponents<ElementConverter>();
		}

		public override void StartSM()
		{
			base.StartSM();
			workable.OnWork_PST_Begins = Play_Empty_MeterAnimation;
			RefreshMeters();
		}

		private void Play_Empty_MeterAnimation()
		{
			fatMeter.SetPositionPercent(0f);
			fatMeter.meterController.Play("meter_fat_empty");
		}

		public bool HasEnoughMassToStartConverting()
		{
			for (int i = 0; i < elementConverters.Length; i++)
			{
				if (elementConverters[i].HasEnoughMassToStartConverting())
				{
					return true;
				}
			}
			return false;
		}

		public bool CanConvertAtAll()
		{
			for (int i = 0; i < elementConverters.Length; i++)
			{
				if (elementConverters[i].CanConvertAtAll())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsProducingCaviar()
		{
			if (base.def.CAVIAR_PRODUCTION_RATE > 0f)
			{
				return storage.GetAmountAvailable(base.def.FISHMILK_TAG) > 0f;
			}
			return false;
		}

		public void SpawnCaviar(float dt)
		{
			if (!(base.def.CAVIAR_PRODUCTION_RATE <= 0f) && !(storage.GetAmountAvailable(base.def.FISHMILK_TAG) <= 0f))
			{
				caviarMassAccumulated += base.def.CAVIAR_PRODUCTION_RATE * dt;
				if (caviarMassAccumulated >= 1f)
				{
					GameObject gameObject = GameUtil.KInstantiate(Assets.GetPrefab(base.def.CAVIAR_TAG), Grid.SceneLayer.Ore);
					gameObject.GetComponent<PrimaryElement>().Units = Mathf.FloorToInt(caviarMassAccumulated / 1f);
					gameObject.SetActive(value: true);
					storage.Store(gameObject, hide_popups: true);
					caviarMassAccumulated %= 1f;
				}
			}
		}

		public void DropMilkFat()
		{
			List<GameObject> list = new List<GameObject>();
			storage.Drop(base.def.MILK_FAT_TAG, list);
			storage.Drop(base.def.CAVIAR_TAG, list);
			Vector3 dropSpawnLocation = GetDropSpawnLocation();
			foreach (GameObject item in list)
			{
				item.transform.position = dropSpawnLocation;
			}
		}

		private Vector3 GetDropSpawnLocation()
		{
			bool symbolVisible;
			Vector3 vector = animController.GetSymbolTransform(new HashedString("object"), out symbolVisible).GetColumn(3);
			vector.z = Grid.GetLayerZ(Grid.SceneLayer.Ore);
			int num = Grid.PosToCell(vector);
			if (Grid.IsValidCell(num) && !Grid.Solid[num])
			{
				return vector;
			}
			return base.transform.GetPosition();
		}

		public void RefreshLastObjectDispensed(object o)
		{
			if (o != null)
			{
				PrimaryElement primaryElement = o as PrimaryElement;
				if (!(primaryElement == null))
				{
					lastObjectDispensedTag = primaryElement.Element.tag;
				}
			}
		}

		public Color GetFatColor()
		{
			bool flag = MilkFatStored >= CaviarStored;
			bool flag2 = MilkStored >= FishMilkStored;
			if (!((MilkFatStored <= 0f && CaviarStored <= 0f) ? flag2 : flag))
			{
				return CaviarTuning.COLOR;
			}
			return ElementLoader.FindElementByTag(base.def.MILK_FAT_TAG).substance.colour;
		}

		public void RefreshMeters()
		{
			if (fatMeter.meterController.currentAnim != "meter_fat")
			{
				fatMeter.meterController.Play("meter_fat", KAnim.PlayMode.Paused);
			}
			fatMeter.SetPositionPercent(SolidOutputStoragePercentage);
			_ = MilkFatStored;
			_ = CaviarStored;
			Tag tag = ((MilkStored >= FishMilkStored) ? base.def.MILK_TAG : base.def.FISHMILK_TAG);
			Tag tag2 = ((lastObjectDispensedTag != Tag.Invalid) ? lastObjectDispensedTag : ((tag == base.def.FISHMILK_TAG) ? base.def.FISHMILK_SEPARATED_LIQUID_OUTPUT_TAG : base.def.MILK_SEPARATED_LIQUID_OUTPUT_TAG));
			Color fatColor = GetFatColor();
			fatMeter.meterController.SetSymbolTint(new KAnimHashedString("meter_fat"), fatColor);
			animController.SetSymbolTint("fat", fatColor);
			GameUtil.TintLiquidSymbolOnBuilding("liquid_reservoir", animController, ElementLoader.FindElementByTag(tag));
			GameUtil.TintLiquidSymbolOnBuilding("meter_liquid_cycle", animController, ElementLoader.FindElementByTag(tag2));
		}
	}

	public const string FAT_IN_METER_COLOR_SYMBOL_NAME = "meter_fat";

	public const string FAT_COLOR_SYMBOL_NAME = "fat";

	public const string INPUT_LIQUID_SYMBOL_NAME = "liquid_reservoir";

	public const string OUTPUT_LIQUID_SYMBOL_NAME = "meter_liquid_cycle";

	public const string WORK_PRE_ANIM_NAME = "separating_pre";

	public const string WORK_ANIM_NAME = "separating_loop";

	public const string WORK_POST_ANIM_NAME = "separating_pst";

	public State noOperational;

	public OperationalStates operational;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = noOperational;
		base.serializable = SerializeType.ParamsOnly;
		root.EventHandler(GameHashes.OnConduitObjectDispensed, RefreshLastObjectDispensed).EventHandler(GameHashes.OnStorageChange, RefreshMeters);
		noOperational.TagTransition(GameTags.Operational, operational).PlayAnim("off");
		operational.TagTransition(GameTags.Operational, noOperational, on_remove: true).PlayAnim("on").DefaultState(operational.idle);
		operational.idle.EventTransition(GameHashes.OnStorageChange, operational.working.pre, CanBeginSeparate).EnterTransition(operational.full, RequiresEmptying);
		operational.working.pre.QueueAnim("separating_pre").OnAnimQueueComplete(operational.working.work);
		operational.working.work.Enter(BeginSeparation).PlayAnim("separating_loop", KAnim.PlayMode.Loop).Update(SpawnCaviar)
			.ToggleStatusItem((Instance smi) => (!smi.IsProducingCaviar()) ? null : Db.Get().BuildingStatusItems.MilkSeparatorProducingCaviar, (Instance smi) => smi)
			.EventTransition(GameHashes.OnStorageChange, operational.working.post, CanNOTKeepSeparating)
			.Exit(EndSeparation);
		operational.working.post.QueueAnim("separating_pst").OnAnimQueueComplete(operational.idle);
		operational.full.PlayAnim("ready").ToggleRecurringChore(CreateEmptyChore).WorkableCompleteTransition((Instance smi) => smi.workable, operational.emptyComplete)
			.ToggleStatusItem(Db.Get().BuildingStatusItems.MilkSeparatorNeedsEmptying);
		operational.emptyComplete.Enter(DropMilkFat).ScheduleActionNextFrame("AfterMilkFatDrop", delegate(Instance smi)
		{
			smi.GoTo(operational.idle);
		});
	}

	public static void SpawnCaviar(Instance smi, float dt)
	{
		smi.SpawnCaviar(dt);
	}

	public static void BeginSeparation(Instance smi)
	{
		smi.operational.SetActive(value: true);
	}

	public static void EndSeparation(Instance smi)
	{
		smi.operational.SetActive(value: false);
	}

	public static bool CanBeginSeparate(Instance smi)
	{
		if (!smi.MilkFatLimitReached)
		{
			return smi.HasEnoughMassToStartConverting();
		}
		return false;
	}

	public static bool CanKeepSeparating(Instance smi)
	{
		if (!smi.MilkFatLimitReached)
		{
			return smi.CanConvertAtAll();
		}
		return false;
	}

	public static bool CanNOTKeepSeparating(Instance smi)
	{
		return !CanKeepSeparating(smi);
	}

	public static bool RequiresEmptying(Instance smi)
	{
		return smi.MilkFatLimitReached;
	}

	public static bool ThereIsCapacityForMilkFat(Instance smi)
	{
		return !smi.MilkFatLimitReached;
	}

	public static void DropMilkFat(Instance smi)
	{
		smi.DropMilkFat();
	}

	public static void RefreshLastObjectDispensed(Instance smi, object o)
	{
		smi.RefreshLastObjectDispensed(o);
	}

	public static void RefreshMeters(Instance smi)
	{
		smi.RefreshMeters();
	}

	private static Chore CreateEmptyChore(Instance smi)
	{
		WorkChore<EmptyMilkSeparatorWorkable> workChore = new WorkChore<EmptyMilkSeparatorWorkable>(Db.Get().ChoreTypes.EmptyStorage, smi.workable);
		workChore.AddPrecondition(ChorePreconditions.instance.IsNotARobot);
		return workChore;
	}
}
