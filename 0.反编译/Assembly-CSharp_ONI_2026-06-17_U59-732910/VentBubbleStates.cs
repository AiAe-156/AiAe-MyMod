using STRINGS;
using UnityEngine;

public class VentBubbleStates : GameStateMachine<VentBubbleStates, VentBubbleStates.Instance, IStateMachineTarget, VentBubbleStates.Def>
{
	public class Def : BaseDef
	{
		public SimHashes element;

		public float emitMass = 1f;

		public KAnimFile[] dupebreathingAnimFiles;

		public HashedString[] dupebreathingAnims;

		public HashedString[] dupebreathingPst;
	}

	public new class Instance : GameInstance
	{
		[MyCmpGet]
		private Storage storage;

		private Tag elementTag;

		private static Chore.Precondition ShouldInflate = new Chore.Precondition
		{
			id = "ShouldInflate",
			description = "__ Blowter has no oxygen",
			fn = delegate(ref Chore.Precondition.Context context, object data)
			{
				if (context.consumerState.consumer == null)
				{
					return false;
				}
				return (!(context.consumerState.gameObject.GetComponent<Storage>().MassStored() <= 0f) || context.consumerState.consumer.RunBehaviourPrecondition(GameTags.Creatures.Poop)) ? true : false;
			}
		};

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ShouldInflate);
			elementTag = ElementLoader.FindElementByHash(def.element).tag;
		}

		protected override void OnCleanUp()
		{
			DisableBreathingLocation(this);
			base.OnCleanUp();
		}

		public void DrainStomachToStorage()
		{
			if (base.gameObject.GetSMI<CreatureCalorieMonitor.Instance>() != null)
			{
				base.gameObject.Trigger(-667597687, (object)new PoopData(skipSpawningPoop: false, storage));
			}
		}

		public bool HasStoredElement()
		{
			GameObject gameObject = storage.FindFirst(elementTag);
			if (gameObject == null)
			{
				return false;
			}
			return gameObject.GetComponent<PrimaryElement>().Mass > 0f;
		}

		public void EmitBubble(float min_mass, float y_offset = 0f)
		{
			GameObject gameObject = storage.FindFirst(elementTag);
			if (gameObject == null)
			{
				return;
			}
			PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
			float mass = component.Mass;
			if (!(mass <= 0f))
			{
				float num = Mathf.Min(mass, min_mass);
				component.Mass -= num;
				Vector3 position = base.master.transform.GetPosition();
				position.y += 0.75f;
				Facing component2 = GetComponent<Facing>();
				if (component2 != null)
				{
					position.x += (component2.GetFacing() ? (-0.5f) : 0.5f);
				}
				position.y += y_offset;
				BubbleManager.instance.SpawnBubble(base.def.element, position, num, component.Temperature, BubbleManager.Disease.None);
			}
		}
	}

	private const float INFLATED_DURATION = 30f;

	private const int INFLATE_POSITION_MIN_TILES_ABOVE_FLOOR = 3;

	public static StatusItem InflatedStatus = new StatusItem("InflatedStatus", CREATURES.STATUSITEMS.PUFFER_INFLATED.NAME, CREATURES.STATUSITEMS.PUFFER_INFLATED.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);

	public static StatusItem VentingStatus = new StatusItem("VentingStatus", CREATURES.STATUSITEMS.PUFFER_VENTING.NAME, CREATURES.STATUSITEMS.PUFFER_VENTING.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);

	public static StatusItem SharingAirStatus = new StatusItem("SharingAirStatus", CREATURES.STATUSITEMS.PUFFER_SHARING_AIR.NAME, CREATURES.STATUSITEMS.PUFFER_SHARING_AIR.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);

	public static StatusItem DupeConsumingAirStatus = new StatusItem("SharingAirStatus", DUPLICANTS.STATUSITEMS.PUFFER_SHARING_AIR.NAME, DUPLICANTS.STATUSITEMS.PUFFER_SHARING_AIR.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);

	public State position_above_floor;

	public State full;

	public State venting;

	public State venting_pst;

	public State empty;

	public State dupe_venting;

	public State dupe_venting_pst;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = position_above_floor;
		position_above_floor.MoveTo(GetCellAboveFloor, full, full);
		full.Enter(DrainStomachToStorage).Enter(EnableBreathingLocation).PlayAnim("full_pre", KAnim.PlayMode.Once)
			.QueueAnim("full_loop", loop: true)
			.ToggleMainStatusItem(InflatedStatus)
			.ScheduleGoTo(30f, venting)
			.WorkableStartTransition((Instance smi) => GetWorkable(smi), dupe_venting);
		dupe_venting.ToggleMainStatusItem(SharingAirStatus).Update(delegate(Instance smi, float dt)
		{
			smi.EmitBubble(smi.def.emitMass * 0.25f, 0.75f);
		}, UpdateRate.SIM_1000ms).WorkableStopTransition((Instance smi) => GetWorkable(smi), dupe_venting_pst)
			.Exit(DisableBreathingLocation);
		dupe_venting_pst.Enter(delegate(Instance smi)
		{
			if (smi.GetComponent<Storage>().IsEmpty())
			{
				smi.GoTo(empty);
			}
			else
			{
				smi.GoTo(full);
			}
		});
		venting.PlayAnim("deflate_loop", KAnim.PlayMode.Loop).Enter(DisableBreathingLocation).ToggleMainStatusItem(VentingStatus)
			.Update(EmitBubble, UpdateRate.SIM_1000ms)
			.Transition(venting_pst, GameStateMachine<VentBubbleStates, Instance, IStateMachineTarget, Def>.Not(HasStoredElement), UpdateRate.SIM_1000ms);
		venting_pst.PlayAnim("full_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(empty);
		empty.BehaviourComplete(GameTags.Creatures.Poop);
	}

	private static int GetCellAboveFloor(Instance smi)
	{
		int num = Grid.PosToCell(smi.transform.GetPosition());
		if (!Grid.IsValidCell(num))
		{
			return Grid.InvalidCell;
		}
		Grid.CellToXY(num, out var x, out var y);
		for (int i = 0; i <= 8; i++)
		{
			for (int j = -1; j <= 1; j += 2)
			{
				if (i == 0 && j == -1)
				{
					continue;
				}
				int num2 = Grid.XYToCell(x + i * j, y);
				if (!Grid.IsValidCell(num2) || Grid.Solid[num2])
				{
					continue;
				}
				int num3 = num2;
				int num4 = 0;
				while (Grid.IsValidCell(num3) && !Grid.Solid[num3] && num4 <= 32)
				{
					num3 = Grid.CellBelow(num3);
					num4++;
				}
				if (!Grid.IsValidCell(num3) || !Grid.Solid[num3])
				{
					continue;
				}
				if (num4 >= 3)
				{
					return AvoidLiquidSurface(num2);
				}
				int num5 = num3;
				bool flag = false;
				for (int k = 0; k < 3; k++)
				{
					num5 = Grid.CellAbove(num5);
					if (!Grid.IsValidCell(num5) || Grid.Solid[num5])
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return AvoidLiquidSurface(num5);
				}
			}
		}
		return num;
	}

	private static int AvoidLiquidSurface(int cell)
	{
		int num = Grid.CellAbove(cell);
		if (Grid.IsValidCell(num) && Grid.Element[cell].IsLiquid && !Grid.Element[num].IsLiquid)
		{
			int num2 = Grid.CellBelow(cell);
			if (Grid.IsValidCell(num2) && !Grid.Solid[num2])
			{
				return num2;
			}
		}
		return cell;
	}

	private static bool HasStoredElement(Instance smi)
	{
		return smi.HasStoredElement();
	}

	private static void EmitBubble(Instance smi, float dt)
	{
		smi.EmitBubble(smi.def.emitMass);
	}

	private static void DrainStomachToStorage(Instance smi)
	{
		smi.DrainStomachToStorage();
	}

	private Workable GetWorkable(Instance smi)
	{
		UnderwaterBreathingLocationWorkable underwaterBreathingLocationWorkable = smi.Get<UnderwaterBreathingLocationWorkable>();
		underwaterBreathingLocationWorkable.overrideAnims = smi.def.dupebreathingAnimFiles;
		underwaterBreathingLocationWorkable.workAnims = smi.def.dupebreathingAnims;
		underwaterBreathingLocationWorkable.workingPstComplete = smi.def.dupebreathingPst;
		underwaterBreathingLocationWorkable.workingPstFailed = smi.def.dupebreathingPst;
		underwaterBreathingLocationWorkable.synchronizeAnims = true;
		underwaterBreathingLocationWorkable.workLayer = Grid.SceneLayer.Move;
		underwaterBreathingLocationWorkable.SetWorkerStatusItem(DupeConsumingAirStatus);
		return smi.Get<UnderwaterBreathingLocationWorkable>();
	}

	private static void EnableBreathingLocation(Instance smi)
	{
		UnderwaterBreathingLocation component = smi.GetComponent<UnderwaterBreathingLocation>();
		if (component != null)
		{
			component.MarkCells();
		}
	}

	private static void DisableBreathingLocation(Instance smi)
	{
		UnderwaterBreathingLocation component = smi.GetComponent<UnderwaterBreathingLocation>();
		if (component != null)
		{
			component.UnmarkCells();
		}
	}
}
