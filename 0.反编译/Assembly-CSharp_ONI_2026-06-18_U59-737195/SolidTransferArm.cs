using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using FMODUnity;
using KSerialization;
using Klei.AI;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class SolidTransferArm : StateMachineComponent<SolidTransferArm.SMInstance>, ISim1000ms, IRenderEveryTick
{
	private enum ArmAnim
	{
		Idle,
		Pickup,
		Drop
	}

	public class SMInstance : GameStateMachine<States, SMInstance, SolidTransferArm, object>.GameInstance
	{
		public SMInstance(SolidTransferArm master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, SMInstance, SolidTransferArm>
	{
		public class ReadyStates : State
		{
			public State idle;

			public State working;
		}

		public BoolParameter transferring;

		public State off;

		public ReadyStates on;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = off;
			root.DoNothing();
			off.PlayAnim("off").EventTransition(GameHashes.OperationalChanged, on, (SMInstance smi) => smi.GetComponent<Operational>().IsOperational).Enter(delegate(SMInstance smi)
			{
				smi.master.StopRotateSound();
			});
			on.DefaultState(on.idle).EventTransition(GameHashes.OperationalChanged, off, (SMInstance smi) => !smi.GetComponent<Operational>().IsOperational);
			on.idle.PlayAnim("on").EventTransition(GameHashes.ActiveChanged, on.working, (SMInstance smi) => smi.GetComponent<Operational>().IsActive);
			on.working.PlayAnim("working").EventTransition(GameHashes.ActiveChanged, on.idle, (SMInstance smi) => !smi.GetComponent<Operational>().IsActive);
		}
	}

	private class SolidTransferArmBatchUpdater : WorkItemCollection<List<UpdateBucketWithUpdater<ISim1000ms>.Entry>>
	{
		private static readonly List<UpdateBucketWithUpdater<ISim1000ms>.Entry> EmptyList = new List<UpdateBucketWithUpdater<ISim1000ms>.Entry>();

		private const int kBatchSize = 8;

		private static SolidTransferArmBatchUpdater instance;

		public static SolidTransferArmBatchUpdater Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new SolidTransferArmBatchUpdater();
				}
				return instance;
			}
		}

		public void Reset(List<UpdateBucketWithUpdater<ISim1000ms>.Entry> entries)
		{
			sharedData = entries;
			count = (entries.Count + 8 - 1) / 8;
		}

		public override void RunItem(int item, ref List<UpdateBucketWithUpdater<ISim1000ms>.Entry> shared_data, int threadIndex)
		{
			int num = item * 8;
			int num2 = Math.Min(shared_data.Count, num + 8);
			for (int i = num; i < num2; i++)
			{
				SolidTransferArm solidTransferArm = (SolidTransferArm)shared_data[i].data;
				if (solidTransferArm.operational.IsOperational)
				{
					solidTransferArm.AsyncUpdate();
				}
			}
		}

		public void Finish()
		{
			foreach (UpdateBucketWithUpdater<ISim1000ms>.Entry sharedDatum in sharedData)
			{
				SolidTransferArm solidTransferArm = (SolidTransferArm)sharedDatum.data;
				if (solidTransferArm.operational.IsOperational)
				{
					solidTransferArm.Sim();
				}
			}
			Reset(EmptyList);
		}
	}

	public struct CachedPickupable
	{
		public Pickupable pickupable;

		public int storage_cell;
	}

	[MyCmpReq]
	private Operational operational;

	[MyCmpReq]
	private KPrefabID kPrefabID;

	[MyCmpAdd]
	private Storage storage;

	[MyCmpGet]
	private Rotatable rotatable;

	[MyCmpAdd]
	private StandardWorker worker;

	[MyCmpAdd]
	private ChoreConsumer choreConsumer;

	[MyCmpAdd]
	private ChoreDriver choreDriver;

	public int pickupRange = 4;

	private float max_carry_weight = 1000f;

	private List<Pickupable> pickupables = new List<Pickupable>();

	private KBatchedAnimController arm_anim_ctrl;

	private GameObject arm_go;

	private LoopingSounds looping_sounds;

	private bool rotateSoundPlaying;

	private string rotateSoundName = "TransferArm_rotate";

	private EventReference rotateSound;

	private KAnimLink link;

	private float arm_rot = 45f;

	private float turn_rate = 360f;

	private bool rotation_complete;

	private int gameCell;

	private ArmAnim arm_anim;

	private HashSet<int> reachableCells = new HashSet<int>();

	private static readonly EventSystem.IntraObjectHandler<SolidTransferArm> OnOperationalChangedDelegate = new EventSystem.IntraObjectHandler<SolidTransferArm>(delegate(SolidTransferArm component, object data)
	{
		component.OnOperationalChanged(data);
	});

	private static readonly EventSystem.IntraObjectHandler<SolidTransferArm> OnEndChoreDelegate = new EventSystem.IntraObjectHandler<SolidTransferArm>(delegate(SolidTransferArm component, object data)
	{
		component.OnEndChore(data);
	});

	private static Func<object, SolidTransferArm, Util.IterationInstruction> AsyncUpdateVisitor = delegate(object obj, SolidTransferArm arm)
	{
		Pickupable pickupable = obj as Pickupable;
		if (Grid.GetCellRange(arm.gameCell, pickupable.cachedCell) <= arm.pickupRange && arm.IsPickupableRelevantToMyInterests(pickupable.KPrefabID, pickupable.cachedCell) && pickupable.CouldBePickedUpByTransferArm(arm.kPrefabID.InstanceID))
		{
			arm.pickupables.Add(pickupable);
		}
		return Util.IterationInstruction.Continue;
	};

	private static HashedString HASH_ROTATION = "rotation";

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		choreConsumer.AddProvider(GlobalChoreProvider.Instance);
		choreConsumer.SetReach(pickupRange);
		Klei.AI.Attributes attributes = this.GetAttributes();
		if (attributes.Get(Db.Get().Attributes.CarryAmount) == null)
		{
			attributes.Add(Db.Get().Attributes.CarryAmount);
		}
		AttributeModifier modifier = new AttributeModifier(Db.Get().Attributes.CarryAmount.Id, max_carry_weight, base.gameObject.GetProperName());
		this.GetAttributes().Add(modifier);
		worker.usesMultiTool = false;
		storage.fxPrefix = Storage.FXPrefix.PickedUp;
		simRenderLoadBalance = false;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		string text = component.name + ".arm";
		arm_go = new GameObject(text);
		arm_go.SetActive(value: false);
		arm_go.transform.parent = component.transform;
		looping_sounds = arm_go.AddComponent<LoopingSounds>();
		string sound = GlobalAssets.GetSound(rotateSoundName);
		rotateSound = RuntimeManager.PathToEventReference(sound);
		arm_go.AddComponent<KPrefabID>().PrefabTag = new Tag(text);
		arm_anim_ctrl = arm_go.AddComponent<KBatchedAnimController>();
		arm_anim_ctrl.AnimFiles = new KAnimFile[1] { component.AnimFiles[0] };
		arm_anim_ctrl.initialAnim = "arm";
		arm_anim_ctrl.isMovable = true;
		arm_anim_ctrl.sceneLayer = Grid.SceneLayer.TransferArm;
		component.SetSymbolVisiblity("arm_target", is_visible: false);
		bool symbolVisible;
		Vector3 position = component.GetSymbolTransform(new HashedString("arm_target"), out symbolVisible).GetColumn(3);
		position.z = Grid.GetLayerZ(Grid.SceneLayer.TransferArm);
		arm_go.transform.SetPosition(position);
		arm_go.SetActive(value: true);
		gameCell = Grid.PosToCell(arm_go);
		link = new KAnimLink(component, arm_anim_ctrl);
		ChoreGroups choreGroups = Db.Get().ChoreGroups;
		for (int i = 0; i < choreGroups.Count; i++)
		{
			choreConsumer.SetPermittedByUser(choreGroups[i], is_allowed: true);
		}
		Subscribe(-592767678, OnOperationalChangedDelegate);
		Subscribe(1745615042, OnEndChoreDelegate);
		RotateArm(rotatable.GetRotatedOffset(Vector3.up), warp: true, 0f);
		DropLeftovers();
		component.enabled = false;
		component.enabled = true;
		base.smi.StartSM();
	}

	protected override void OnCleanUp()
	{
		MinionGroupProber.Get().Vacate(reachableCells.ToList());
		base.OnCleanUp();
	}

	public static void BatchUpdate(List<UpdateBucketWithUpdater<ISim1000ms>.Entry> solid_transfer_arms, float time_delta)
	{
		SolidTransferArmBatchUpdater.Instance.Reset(solid_transfer_arms);
		GlobalJobManager.Run(SolidTransferArmBatchUpdater.Instance);
		SolidTransferArmBatchUpdater.Instance.Finish();
	}

	private void Sim()
	{
		Chore.Precondition.Context out_context = default(Chore.Precondition.Context);
		if (choreConsumer.FindNextChore(ref out_context))
		{
			if (out_context.chore is FetchChore)
			{
				choreDriver.SetChore(out_context);
				FetchChore chore = out_context.chore as FetchChore;
				storage.DropUnlessMatching(chore);
				arm_anim_ctrl.enabled = false;
				arm_anim_ctrl.enabled = true;
			}
			else
			{
				Debug.Assert(condition: false, "I am but a lowly transfer arm. I should only acquire FetchChores: " + out_context.chore);
			}
		}
		operational.SetActive(choreDriver.HasChore());
	}

	public void Sim1000ms(float dt)
	{
	}

	private void UpdateArmAnim()
	{
		FetchAreaChore fetchAreaChore = choreDriver.GetCurrentChore() as FetchAreaChore;
		if ((bool)worker.GetWorkable() && fetchAreaChore != null && rotation_complete)
		{
			StopRotateSound();
			SetArmAnim((!fetchAreaChore.IsDelivering) ? ArmAnim.Pickup : ArmAnim.Drop);
		}
		else
		{
			SetArmAnim(ArmAnim.Idle);
		}
	}

	private bool AsyncUpdate()
	{
		Grid.CellToXY(gameCell, out var x, out var y);
		bool flag = false;
		for (int i = y - pickupRange; i < y + pickupRange + 1; i++)
		{
			for (int j = x - pickupRange; j < x + pickupRange + 1; j++)
			{
				int num = Grid.XYToCell(j, i);
				if (Grid.IsValidCell(num) && Grid.IsPhysicallyAccessible(x, y, j, i, blocking_tile_visible: true) != reachableCells.Contains(num))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			ListPool<int, SolidTransferArm>.PooledList pooledList = ListPool<int, SolidTransferArm>.Allocate();
			ListPool<int, SolidTransferArm>.PooledList pooledList2 = ListPool<int, SolidTransferArm>.Allocate();
			pooledList.AddRange(reachableCells);
			reachableCells.Clear();
			for (int k = y - pickupRange; k < y + pickupRange + 1; k++)
			{
				for (int l = x - pickupRange; l < x + pickupRange + 1; l++)
				{
					int num2 = Grid.XYToCell(l, k);
					if (Grid.IsValidCell(num2) && Grid.IsPhysicallyAccessible(x, y, l, k, blocking_tile_visible: true))
					{
						reachableCells.Add(num2);
						pooledList2.Add(num2);
					}
				}
			}
			MinionGroupProber.Get().Occupy(pooledList2);
			MinionGroupProber.Get().Vacate(pooledList);
			pooledList2.Recycle();
			pooledList.Recycle();
		}
		pickupables.Clear();
		GameScenePartitioner.Instance.ReadonlyVisitEntries(x - pickupRange, y - pickupRange, 2 * pickupRange + 1, 2 * pickupRange + 1, GameScenePartitioner.Instance.pickupablesLayer, AsyncUpdateVisitor, this);
		GameScenePartitioner.Instance.ReadonlyVisitEntries(x - pickupRange, y - pickupRange, 2 * pickupRange + 1, 2 * pickupRange + 1, GameScenePartitioner.Instance.storedPickupablesLayer, AsyncUpdateVisitor, this);
		return flag;
	}

	public bool IsCellReachable(int cell)
	{
		return reachableCells.Contains(cell);
	}

	private bool IsPickupableRelevantToMyInterests(KPrefabID prefabID, int storage_cell)
	{
		if (Assets.IsTagSolidTransferArmConveyable(prefabID.PrefabTag))
		{
			return IsCellReachable(storage_cell);
		}
		return false;
	}

	public Pickupable FindFetchTarget(Storage destination, FetchChore chore)
	{
		return FetchManager.FindFetchTarget(pickupables, destination, chore);
	}

	public void RenderEveryTick(float dt)
	{
		if ((bool)worker.GetWorkable())
		{
			Vector3 targetPoint = worker.GetWorkable().GetTargetPoint();
			targetPoint.z = 0f;
			Vector3 position = base.transform.GetPosition();
			position.z = 0f;
			Vector3 target_dir = Vector3.Normalize(targetPoint - position);
			RotateArm(target_dir, warp: false, dt);
		}
		UpdateArmAnim();
	}

	private void OnEndChore(object data)
	{
		DropLeftovers();
	}

	private void DropLeftovers()
	{
		if (!storage.IsEmpty() && !choreDriver.HasChore())
		{
			storage.DropAll();
		}
	}

	private void SetArmAnim(ArmAnim new_anim)
	{
		if (new_anim != arm_anim)
		{
			arm_anim = new_anim;
			switch (arm_anim)
			{
			case ArmAnim.Idle:
				arm_anim_ctrl.Play("arm", KAnim.PlayMode.Loop);
				break;
			case ArmAnim.Pickup:
				arm_anim_ctrl.Play("arm_pickup", KAnim.PlayMode.Loop);
				break;
			case ArmAnim.Drop:
				arm_anim_ctrl.Play("arm_drop", KAnim.PlayMode.Loop);
				break;
			}
		}
	}

	private void OnOperationalChanged(object data)
	{
		if (!((Boxed<bool>)data).value)
		{
			if (choreDriver.HasChore())
			{
				choreDriver.StopChore();
			}
			UpdateArmAnim();
		}
	}

	private void SetArmRotation(float rot)
	{
		arm_rot = rot;
		arm_go.transform.rotation = Quaternion.Euler(0f, 0f, arm_rot);
	}

	private void RotateArm(Vector3 target_dir, bool warp, float dt)
	{
		float num = MathUtil.AngleSigned(Vector3.up, target_dir, Vector3.forward) - arm_rot;
		if (num < -180f)
		{
			num += 360f;
		}
		if (num > 180f)
		{
			num -= 360f;
		}
		if (!warp)
		{
			num = Mathf.Clamp(num, (0f - turn_rate) * dt, turn_rate * dt);
		}
		arm_rot += num;
		SetArmRotation(arm_rot);
		rotation_complete = Mathf.Approximately(num, 0f);
		if (!warp && !rotation_complete)
		{
			if (!rotateSoundPlaying)
			{
				StartRotateSound();
			}
			SetRotateSoundParameter(arm_rot);
		}
		else
		{
			StopRotateSound();
		}
	}

	private void StartRotateSound()
	{
		if (!rotateSoundPlaying)
		{
			looping_sounds.StartSound(rotateSound);
			rotateSoundPlaying = true;
		}
	}

	private void SetRotateSoundParameter(float arm_rot)
	{
		if (rotateSoundPlaying)
		{
			looping_sounds.SetParameter(rotateSound, HASH_ROTATION, arm_rot);
		}
	}

	private void StopRotateSound()
	{
		if (rotateSoundPlaying)
		{
			looping_sounds.StopSound(rotateSound);
			rotateSoundPlaying = false;
		}
	}
}
