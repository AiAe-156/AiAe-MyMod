using System;
using System.Collections.Generic;
using STRINGS;

public class RemoteChore : WorkChore<RemoteWorkTerminal>
{
	private static Precondition RemoteTerminalHasDock = new Precondition
	{
		id = "RemoteDockAssigned",
		description = DUPLICANTS.CHORES.PRECONDITIONS.REMOTE_CHORE_NO_REMOTE_DOCK,
		fn = delegate(ref Precondition.Context context, object data)
		{
			return ((RemoteWorkTerminal)data).CurrentDock != null;
		},
		canExecuteOnAnyThread = true
	};

	private static Precondition RemoteDockOperational = new Precondition
	{
		id = "RemoteDockOperational",
		description = DUPLICANTS.CHORES.PRECONDITIONS.REMOTE_CHORE_DOCK_INOPERABLE,
		fn = delegate(ref Precondition.Context context, object data)
		{
			RemoteWorkTerminal remoteWorkTerminal = (RemoteWorkTerminal)data;
			return remoteWorkTerminal.CurrentDock != null && remoteWorkTerminal.CurrentDock.IsOperational;
		},
		canExecuteOnAnyThread = true
	};

	private static Precondition RemoteDockHasWorker = new Precondition
	{
		id = "RemoteDockHasAvailableWorker",
		description = DUPLICANTS.CHORES.PRECONDITIONS.REMOTE_CHORE_NO_REMOTE_WORKER,
		fn = delegate(ref Precondition.Context context, object data)
		{
			RemoteWorkerDock currentDock = ((RemoteWorkTerminal)data).CurrentDock;
			if (currentDock == null)
			{
				return false;
			}
			return currentDock.HasWorker() && currentDock.RemoteWorker.Available && !currentDock.RemoteWorker.RequiresMaintnence;
		},
		canExecuteOnAnyThread = true
	};

	private static Precondition RemoteDockAvailable = new Precondition
	{
		id = "RemoteDockAvailable",
		description = DUPLICANTS.CHORES.PRECONDITIONS.REMOTE_CHORE_DOCK_UNAVAILABLE,
		fn = delegate(ref Precondition.Context context, object data)
		{
			RemoteWorkTerminal remoteWorkTerminal = (RemoteWorkTerminal)data;
			RemoteWorkerDock currentDock = remoteWorkTerminal.CurrentDock;
			return !(currentDock == null) && currentDock.AvailableForWorkBy(remoteWorkTerminal);
		},
		canExecuteOnAnyThread = true
	};

	private static Precondition RemoteChoreSubchorePreconditions = new Precondition
	{
		id = "RemoteChorePreconditionsMet",
		description = DUPLICANTS.CHORES.PRECONDITIONS.REMOTE_CHORE_SUBCHORE_PRECONDITIONS,
		fn = delegate(ref Precondition.Context context, object data)
		{
			if (context.data == null)
			{
				return true;
			}
			Precondition.Context context2 = (Precondition.Context)context.data;
			if (context2.failedPreconditionId != -1)
			{
				return false;
			}
			context2.RunPreconditions();
			return context2.failedPreconditionId == -1;
		},
		canExecuteOnAnyThread = false
	};

	private RemoteWorkTerminal terminal;

	private Chore active_subchore;

	public RemoteChore(RemoteWorkTerminal terminal)
		: base(Db.Get().ChoreTypes.RemoteOperate, (IStateMachineTarget)terminal, (ChoreProvider)null, run_until_complete: true, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, allow_in_red_alert: true, (ScheduleBlockType)null, ignore_schedule_block: false, only_when_operational: true, (KAnimFile)null, is_preemptable: false, allow_in_context_menu: true, allow_prioritization: true, PriorityScreen.PriorityClass.basic, 5, ignore_building_assignment: false, add_to_daily_report: true)
	{
		this.terminal = terminal;
		AddPrecondition(RemoteTerminalHasDock, terminal);
		AddPrecondition(RemoteDockHasWorker, terminal);
		AddPrecondition(RemoteDockAvailable, terminal);
		AddPrecondition(RemoteChoreSubchorePreconditions, terminal);
		AddPrecondition(RemoteDockOperational, terminal);
	}

	public override void CollectChores(ChoreConsumerState duplicantState, List<Precondition.Context> succeeded_contexts, List<Precondition.Context> incomplete_contexts, List<Precondition.Context> failed_contexts, bool is_attempting_override)
	{
		Precondition.Context item = new Precondition.Context(this, duplicantState, is_attempting_override);
		item.RunPreconditions();
		if (!item.IsComplete())
		{
			ListPool<Precondition.Context, Precondition>.PooledList pooledList = ListPool<Precondition.Context, Precondition>.Allocate();
			ListPool<Precondition.Context, Precondition>.PooledList pooledList2 = ListPool<Precondition.Context, Precondition>.Allocate();
			ListPool<Precondition.Context, Precondition>.PooledList pooledList3 = ListPool<Precondition.Context, Precondition>.Allocate();
			terminal.CurrentDock?.CollectChores(duplicantState, pooledList, pooledList3, pooledList2, is_attempting_override);
			foreach (Precondition.Context item2 in pooledList)
			{
				item.data = item2;
				item.SetPriority(item2.chore);
				incomplete_contexts.Add(item);
			}
			foreach (Precondition.Context item3 in pooledList3)
			{
				item.data = item3;
				item.SetPriority(item3.chore);
				incomplete_contexts.Add(item);
			}
			List<PreconditionInstance> list = item.chore.GetPreconditions();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].condition.id == RemoteChoreSubchorePreconditions.id)
				{
					item.failedPreconditionId = i;
					break;
				}
			}
			foreach (Precondition.Context item4 in pooledList2)
			{
				item.data = item4;
				item.SetPriority(item4.chore);
				failed_contexts.Add(item);
			}
			pooledList.Recycle();
			pooledList2.Recycle();
			pooledList3.Recycle();
		}
		else if (item.IsSuccess())
		{
			ListPool<Precondition.Context, Precondition>.PooledList pooledList4 = ListPool<Precondition.Context, Precondition>.Allocate();
			ListPool<Precondition.Context, Precondition>.PooledList pooledList5 = ListPool<Precondition.Context, Precondition>.Allocate();
			terminal.CurrentDock?.CollectChores(duplicantState, pooledList4, null, pooledList5, is_attempting_override);
			foreach (Precondition.Context item5 in pooledList4)
			{
				item.data = item5;
				item.SetPriority(item5.chore);
				succeeded_contexts.Add(item);
			}
			foreach (Precondition.Context item6 in pooledList5)
			{
				item.data = item6;
				item.SetPriority(item6.chore);
				failed_contexts.Add(item);
			}
			pooledList4.Recycle();
			pooledList5.Recycle();
		}
		else
		{
			failed_contexts.Add(item);
		}
	}

	public override void PrepareChore(ref Precondition.Context context)
	{
		base.PrepareChore(ref context);
		DebugUtil.Assert(active_subchore == null);
		active_subchore = ((Precondition.Context)context.data).chore;
		terminal.CurrentDock?.SetNextChore(terminal, (Precondition.Context)context.data);
	}

	protected override void End(string reason)
	{
		if (active_subchore != null && active_subchore.driver != null && !active_subchore.driver.HasChore())
		{
			active_subchore.Reserve(null);
		}
		active_subchore = null;
		base.End(reason);
		if (terminal.worker != null)
		{
			terminal.StopWork(terminal.worker, aborted: true);
		}
	}
}
