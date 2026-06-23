using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FetchAreaChore : Chore<FetchAreaChore.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, FetchAreaChore, object>.GameInstance
	{
		public struct Delivery
		{
			private Action<FetchChore> onCancelled;

			private Action<Chore> onFetchChoreCleanup;

			public Storage destination { get; private set; }

			public float amount { get; private set; }

			public FetchChore chore { get; private set; }

			public Delivery(Precondition.Context context, float amount_to_be_fetched, Action<FetchChore> on_cancelled)
			{
				this = default(Delivery);
				chore = context.chore as FetchChore;
				amount = chore.originalAmount;
				destination = chore.destination;
				chore.SetOverrideTarget(context.consumerState.consumer);
				onCancelled = on_cancelled;
				onFetchChoreCleanup = OnFetchChoreCleanup;
				chore.FetchAreaBegin(context, amount_to_be_fetched);
				FetchChore fetchChore = chore;
				fetchChore.onCleanup = (Action<Chore>)Delegate.Combine(fetchChore.onCleanup, onFetchChoreCleanup);
			}

			public void Complete(List<Pickupable> deliverables)
			{
				if (destination == null || destination.IsEndOfLife())
				{
					return;
				}
				FetchChore fetchChore = chore;
				fetchChore.onCleanup = (Action<Chore>)Delegate.Remove(fetchChore.onCleanup, onFetchChoreCleanup);
				float num = amount;
				Pickupable pickupable = null;
				for (int i = 0; i < deliverables.Count; i++)
				{
					if (num <= 0f)
					{
						break;
					}
					if (deliverables[i] == null)
					{
						if (num < PICKUPABLETUNING.MINIMUM_PICKABLE_AMOUNT)
						{
							destination.ForceStore(chore.tagsFirst, num);
						}
						continue;
					}
					if (!IsPickupableStillValidForChore(deliverables[i], chore))
					{
						Debug.LogError($"Attempting to store {deliverables[i]} in a {destination} which did not request it");
						continue;
					}
					Pickupable pickupable2 = deliverables[i].Take(num);
					if (pickupable2 != null && pickupable2.FetchTotalAmount > 0f)
					{
						num -= pickupable2.FetchTotalAmount;
						destination.Store(pickupable2.gameObject);
						pickupable = pickupable2;
						if (pickupable2 == deliverables[i])
						{
							deliverables[i] = null;
						}
					}
				}
				if (chore.overrideTarget != null)
				{
					chore.FetchAreaEnd(chore.overrideTarget.GetComponent<ChoreDriver>(), pickupable, is_success: true);
				}
				chore = null;
			}

			private void OnFetchChoreCleanup(Chore chore)
			{
				if (onCancelled != null)
				{
					onCancelled(chore as FetchChore);
				}
			}

			public void Cleanup()
			{
				if (chore != null)
				{
					FetchChore fetchChore = chore;
					fetchChore.onCleanup = (Action<Chore>)Delegate.Remove(fetchChore.onCleanup, onFetchChoreCleanup);
					chore.FetchAreaEnd(null, null, is_success: false);
				}
			}
		}

		public struct Reservation
		{
			private int handle;

			public float amount { get; private set; }

			public Pickupable pickupable { get; private set; }

			public Reservation(ChoreConsumer consumer, Pickupable pickupable, float reservation_amount)
			{
				this = default(Reservation);
				if (reservation_amount <= 0f)
				{
					Debug.LogError("Invalid amount: " + reservation_amount);
				}
				amount = reservation_amount;
				this.pickupable = pickupable;
				handle = pickupable.Reserve("FetchAreaChore", consumer.GetComponent<KPrefabID>().InstanceID, reservation_amount);
			}

			public void Cleanup()
			{
				if (pickupable != null)
				{
					pickupable.Unreserve("FetchAreaChore", handle);
				}
			}
		}

		private List<FetchChore> chores = new List<FetchChore>();

		private List<Pickupable> fetchables = new List<Pickupable>();

		private List<Reservation> reservations = new List<Reservation>();

		private List<Pickupable> deliverables = new List<Pickupable>();

		public List<Delivery> deliveries = new List<Delivery>();

		private FetchChore rootChore;

		private Precondition.Context rootContext;

		private float fetchAmountRequested;

		public bool delivering;

		public bool pickingup;

		private static Tag[] s_transientDeliveryTags = new Tag[2]
		{
			GameTags.Garbage,
			GameTags.Creatures.Deliverable
		};

		public Tag RootChore_RequiredTag => rootChore.requiredTag;

		public bool RootChore_ValidateRequiredTagOnTagChange => rootChore.validateRequiredTagOnTagChange;

		public StatesInstance(FetchAreaChore master, Precondition.Context context)
			: base(master)
		{
			rootContext = context;
			rootChore = context.chore as FetchChore;
		}

		public void Begin(Precondition.Context context)
		{
			base.sm.fetcher.Set(context.consumerState.gameObject, base.smi);
			chores.Clear();
			chores.Add(rootChore);
			Grid.CellToXY(Grid.PosToCell(rootChore.destination.transform.GetPosition()), out var x, out var y);
			ListPool<Precondition.Context, FetchAreaChore>.PooledList succeeded_contexts = ListPool<Precondition.Context, FetchAreaChore>.Allocate();
			ListPool<Precondition.Context, FetchAreaChore>.PooledList pooledList = ListPool<Precondition.Context, FetchAreaChore>.Allocate();
			if (rootChore.allowMultifetch)
			{
				GatherNearbyFetchChores(rootChore, context, x, y, 3, succeeded_contexts, pooledList);
			}
			float max_carry_weight = Mathf.Max(1f, Db.Get().Attributes.CarryAmount.Lookup(context.consumerState.consumer).GetTotalValue());
			Pickupable root_fetchable = context.data as Pickupable;
			if (root_fetchable == null)
			{
				Debug.Assert(succeeded_contexts.Count > 0, "succeeded_contexts was empty");
				FetchChore fetchChore = (FetchChore)succeeded_contexts[0].chore;
				Debug.Assert(fetchChore != null, "fetch_chore was null");
				DebugUtil.LogWarningArgs("Missing root_fetchable for FetchAreaChore", fetchChore.destination, fetchChore.tagsFirst);
				root_fetchable = fetchChore.FindFetchTarget(context.consumerState);
			}
			Debug.Assert(root_fetchable != null, "root_fetchable was null");
			ListPool<Pickupable, FetchAreaChore>.PooledList potential_fetchables = ListPool<Pickupable, FetchAreaChore>.Allocate();
			potential_fetchables.Add(root_fetchable);
			float fetch_amount_available = root_fetchable.UnreservedFetchAmount;
			if (root_fetchable.MinTakeAmount)
			{
				max_carry_weight = (float)(int)(max_carry_weight / root_fetchable.PrimaryElement.MassPerUnit) * root_fetchable.PrimaryElement.MassPerUnit;
			}
			max_carry_weight = Mathf.Max(root_fetchable.PrimaryElement.MassPerUnit, max_carry_weight);
			int x2 = 0;
			int y2 = 0;
			Grid.CellToXY(Grid.PosToCell(root_fetchable.transform.GetPosition()), out x2, out y2);
			int num = 9;
			x2 -= 3;
			y2 -= 3;
			Tag root_fetchable_tag = root_fetchable.GetComponent<KPrefabID>().PrefabTag;
			Func<object, object, Util.IterationInstruction> visitor = delegate(object obj, object _)
			{
				if (fetch_amount_available > max_carry_weight)
				{
					return Util.IterationInstruction.Halt;
				}
				Pickupable pickupable2 = obj as Pickupable;
				KPrefabID kPrefabID = pickupable2.KPrefabID;
				if (pickupable2 == root_fetchable)
				{
					return Util.IterationInstruction.Continue;
				}
				if (kPrefabID.HasTag(GameTags.StoredPrivate))
				{
					return Util.IterationInstruction.Continue;
				}
				if (kPrefabID.PrefabTag != root_fetchable_tag)
				{
					return Util.IterationInstruction.Continue;
				}
				if (pickupable2.UnreservedFetchAmount <= 0f)
				{
					return Util.IterationInstruction.Continue;
				}
				if (rootChore.criteria == FetchChore.MatchCriteria.MatchID && !rootChore.tags.Contains(kPrefabID.PrefabTag))
				{
					return Util.IterationInstruction.Continue;
				}
				if (rootChore.criteria == FetchChore.MatchCriteria.MatchTags && !kPrefabID.HasTag(rootChore.tagsFirst))
				{
					return Util.IterationInstruction.Continue;
				}
				if (rootChore.requiredTag.IsValid && !kPrefabID.HasTag(rootChore.requiredTag))
				{
					return Util.IterationInstruction.Continue;
				}
				if (kPrefabID.HasAnyTags(rootChore.forbiddenTags))
				{
					return Util.IterationInstruction.Continue;
				}
				if (potential_fetchables.Contains(pickupable2))
				{
					return Util.IterationInstruction.Continue;
				}
				if (!rootContext.consumerState.consumer.CanReach(pickupable2))
				{
					return Util.IterationInstruction.Continue;
				}
				if (kPrefabID.HasTag(GameTags.MarkedForMove))
				{
					return Util.IterationInstruction.Continue;
				}
				Storage storage = pickupable2.storage;
				if (!storage.IsNullOrDestroyed())
				{
					bool flag = true;
					foreach (Precondition.Context item in succeeded_contexts)
					{
						FetchChore fetchChore3 = item.chore as FetchChore;
						if (!FetchManager.IsFetchablePickup(pickupable2, fetchChore3, fetchChore3.destination))
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						return Util.IterationInstruction.Continue;
					}
				}
				float unreservedFetchAmount = pickupable2.UnreservedFetchAmount;
				potential_fetchables.Add(pickupable2);
				fetch_amount_available += unreservedFetchAmount;
				return (potential_fetchables.Count >= 10) ? Util.IterationInstruction.Halt : Util.IterationInstruction.Continue;
			};
			GameScenePartitioner.Instance.ReadonlyVisitEntries(x2, y2, num, num, GameScenePartitioner.Instance.pickupablesLayer, visitor, null);
			GameScenePartitioner.Instance.ReadonlyVisitEntries(x2, y2, num, num, GameScenePartitioner.Instance.storedPickupablesLayer, visitor, null);
			fetch_amount_available = Mathf.Min(max_carry_weight, fetch_amount_available);
			deliveries.Clear();
			float num2 = Mathf.Min(rootChore.originalAmount, fetch_amount_available);
			deliveries.Add(new Delivery(rootContext, num2, OnFetchChoreCancelled));
			float num3 = num2;
			for (int num4 = 0; num4 < succeeded_contexts.Count; num4++)
			{
				if (num3 >= fetch_amount_available)
				{
					break;
				}
				Precondition.Context context2 = succeeded_contexts[num4];
				FetchChore fetchChore2 = context2.chore as FetchChore;
				if (fetchChore2 != rootChore && fetchChore2.overrideTarget == null && fetchChore2.driver == null && fetchChore2.tagsHash == rootChore.tagsHash && fetchChore2.requiredTag == rootChore.requiredTag && fetchChore2.forbidHash == rootChore.forbidHash)
				{
					num2 = Mathf.Min(fetchChore2.originalAmount, fetch_amount_available - num3);
					chores.Add(fetchChore2);
					deliveries.Add(new Delivery(context2, num2, OnFetchChoreCancelled));
					num3 += num2;
					if (deliveries.Count >= 10)
					{
						break;
					}
				}
			}
			num3 = Mathf.Min(num3, fetch_amount_available);
			float num5 = num3;
			fetchables.Clear();
			for (int num6 = 0; num6 < potential_fetchables.Count; num6++)
			{
				if (num5 <= 0f)
				{
					break;
				}
				Pickupable pickupable = potential_fetchables[num6];
				num5 -= pickupable.UnreservedFetchAmount;
				fetchables.Add(pickupable);
			}
			fetchAmountRequested = num3;
			reservations.Clear();
			succeeded_contexts.Recycle();
			pooledList.Recycle();
			potential_fetchables.Recycle();
		}

		public void End()
		{
			foreach (Delivery delivery in deliveries)
			{
				delivery.Cleanup();
			}
			deliveries.Clear();
		}

		public void SetupDelivery()
		{
			if (deliveries.Count == 0)
			{
				StopSM("FetchAreaChoreComplete");
				return;
			}
			Delivery delivery = deliveries[0];
			if (s_transientDeliveryTags.Contains(delivery.chore.requiredTag))
			{
				delivery.chore.requiredTag = Tag.Invalid;
			}
			Pickupable[] array = deliverables.ToArray();
			for (int num = deliverables.Count - 1; num >= 0; num--)
			{
				if (deliverables[num] == null || deliverables[num].FetchTotalAmount <= 0f || deliverables[num].KPrefabID.HasTag(GameTags.MarkedForMove))
				{
					deliverables.RemoveAtSwap(num);
				}
				else if (!IsPickupableStillValidForChore(deliverables[num], delivery.chore))
				{
					Debug.LogWarning($"Removing deliverable {deliverables[num]} for a delivery to {delivery.chore.destination} which did not request it");
					deliverables.RemoveAtSwap(num);
				}
			}
			if (deliverables.Count == 0)
			{
				StopSM("FetchAreaChoreComplete");
				return;
			}
			base.sm.deliveryDestination.Set(delivery.destination, base.smi);
			base.sm.deliveryObject.Set(deliverables[0], base.smi);
			if (delivery.destination != null)
			{
				if (rootContext.consumerState.hasSolidTransferArm)
				{
					if (rootContext.consumerState.consumer.IsWithinReach(deliveries[0].destination))
					{
						GoTo(base.sm.delivering.storing);
					}
					else
					{
						GoTo(base.sm.delivering.deliverfail);
					}
				}
				else
				{
					GoTo(base.sm.delivering.movetostorage);
				}
			}
			else
			{
				base.smi.GoTo(base.sm.delivering.deliverfail);
			}
		}

		public void SetupFetch()
		{
			if (reservations.Count > 0)
			{
				SetFetchTarget(reservations[0].pickupable);
				base.sm.fetchResultTarget.Set(null, base.smi);
				base.sm.fetchAmount.Set(reservations[0].amount, base.smi);
				if (reservations[0].pickupable != null)
				{
					if (rootContext.consumerState.hasSolidTransferArm)
					{
						if (rootContext.consumerState.consumer.IsWithinReach(reservations[0].pickupable))
						{
							GoTo(base.sm.fetching.pickup);
						}
						else
						{
							GoTo(base.sm.fetching.fetchfail);
						}
					}
					else
					{
						GoTo(base.sm.fetching.movetopickupable);
					}
				}
				else
				{
					GoTo(base.sm.fetching.fetchfail);
				}
			}
			else
			{
				GoTo(base.sm.delivering.next);
			}
		}

		public void SetFetchTarget(Pickupable fetching)
		{
			base.sm.fetchTarget.Set(fetching, base.smi);
			if (fetching != null)
			{
				fetching.Subscribe(1122777325, OnMarkForMove);
			}
		}

		public void DeliverFail()
		{
			if (deliveries.Count > 0)
			{
				deliveries[0].Cleanup();
				deliveries.RemoveAt(0);
			}
			GoTo(base.sm.delivering.next);
		}

		public void DeliverComplete()
		{
			Pickupable pickupable = base.sm.deliveryObject.Get<Pickupable>(base.smi);
			if (pickupable == null || pickupable.FetchTotalAmount <= 0f)
			{
				if (deliveries.Count > 0 && deliveries[0].chore.amount < PICKUPABLETUNING.MINIMUM_PICKABLE_AMOUNT)
				{
					Delivery delivery = deliveries[0];
					Chore chore = delivery.chore;
					delivery.Complete(deliverables);
					delivery.Cleanup();
					if (deliveries.Count > 0 && deliveries[0].chore == chore)
					{
						deliveries.RemoveAt(0);
					}
					GoTo(base.sm.delivering.next);
				}
				else
				{
					base.smi.GoTo(base.sm.delivering.deliverfail);
				}
				return;
			}
			if (deliveries.Count > 0)
			{
				Delivery delivery2 = deliveries[0];
				Chore chore2 = delivery2.chore;
				delivery2.Complete(deliverables);
				delivery2.Cleanup();
				if (deliveries.Count > 0 && deliveries[0].chore == chore2)
				{
					deliveries.RemoveAt(0);
				}
			}
			GoTo(base.sm.delivering.next);
		}

		public void FetchFail()
		{
			if (base.smi.sm.fetchTarget.Get(base.smi) != null)
			{
				base.smi.sm.fetchTarget.Get(base.smi).Unsubscribe(1122777325, OnMarkForMove);
			}
			reservations[0].Cleanup();
			reservations.RemoveAt(0);
			GoTo(base.sm.fetching.next);
		}

		public void FetchComplete()
		{
			reservations[0].Cleanup();
			reservations.RemoveAt(0);
			GoTo(base.sm.fetching.next);
		}

		public void SetupDeliverables()
		{
			foreach (GameObject item in base.sm.fetcher.Get<Storage>(base.smi).items)
			{
				if (item == null)
				{
					continue;
				}
				KPrefabID component = item.GetComponent<KPrefabID>();
				if (!(component == null) && !component.HasTag(GameTags.MarkedForMove))
				{
					Pickupable component2 = component.GetComponent<Pickupable>();
					if (component2 != null)
					{
						deliverables.Add(component2);
					}
				}
			}
		}

		public void ReservePickupables()
		{
			ChoreConsumer consumer = base.sm.fetcher.Get<ChoreConsumer>(base.smi);
			float num = fetchAmountRequested;
			foreach (Pickupable fetchable in fetchables)
			{
				if (num <= 0f)
				{
					break;
				}
				if (!fetchable.KPrefabID.HasTag(GameTags.MarkedForMove) && (!(fetchable.PrimaryElement.MassPerUnit > 1f) || !(num < fetchable.PrimaryElement.MassPerUnit)))
				{
					float num2 = Math.Min(num, fetchable.UnreservedFetchAmount);
					num -= num2;
					Reservation item = new Reservation(consumer, fetchable, num2);
					reservations.Add(item);
				}
			}
		}

		private void OnFetchChoreCancelled(FetchChore chore)
		{
			for (int i = 0; i < deliveries.Count; i++)
			{
				if (deliveries[i].chore == chore)
				{
					if (deliveries.Count == 1)
					{
						StopSM("AllDelivericesCancelled");
						break;
					}
					if (i == 0)
					{
						base.sm.currentdeliverycancelled.Trigger(this);
						break;
					}
					deliveries[i].Cleanup();
					deliveries.RemoveAt(i);
					break;
				}
			}
		}

		public void UnreservePickupables()
		{
			foreach (Reservation reservation in reservations)
			{
				reservation.Cleanup();
			}
			reservations.Clear();
		}

		public bool SameDestination(FetchChore fetch)
		{
			foreach (FetchChore chore in chores)
			{
				if (chore.destination == fetch.destination)
				{
					return true;
				}
			}
			return false;
		}

		public void OnMarkForMove(object data)
		{
			GameObject gameObject = base.smi.sm.fetchTarget.Get(base.smi);
			GameObject gameObject2 = data as GameObject;
			if (gameObject != null)
			{
				if (gameObject == gameObject2)
				{
					gameObject2.Unsubscribe(1122777325, OnMarkForMove);
					base.smi.sm.fetchTarget.Set(null, base.smi);
				}
				else
				{
					Debug.LogError("Listening for MarkForMove on the incorrect fetch target. Subscriptions did not update correctly.");
				}
			}
		}
	}

	public class States : GameStateMachine<States, StatesInstance, FetchAreaChore>
	{
		public class FetchStates : State
		{
			public State next;

			public ApproachSubState<Pickupable> movetopickupable;

			public State pickup;

			public State fetchfail;

			public State fetchcomplete;
		}

		public class DeliverStates : State
		{
			public State next;

			public ApproachSubState<Storage> movetostorage;

			public State storing;

			public State deliverfail;

			public State delivercomplete;
		}

		public FetchStates fetching;

		public DeliverStates delivering;

		public TargetParameter fetcher;

		public TargetParameter fetchTarget;

		public TargetParameter fetchResultTarget;

		public FloatParameter fetchAmount;

		public TargetParameter deliveryDestination;

		public TargetParameter deliveryObject;

		public FloatParameter deliveryAmount;

		public Signal currentdeliverycancelled;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = fetching;
			Target(fetcher);
			fetching.DefaultState(fetching.next).Enter("ReservePickupables", delegate(StatesInstance smi)
			{
				smi.ReservePickupables();
			}).Exit("UnreservePickupables", delegate(StatesInstance smi)
			{
				smi.UnreservePickupables();
			})
				.Enter("pickingup-on", delegate(StatesInstance smi)
				{
					smi.pickingup = true;
				})
				.Exit("pickingup-off", delegate(StatesInstance smi)
				{
					smi.pickingup = false;
				});
			fetching.next.Enter("SetupFetch", delegate(StatesInstance smi)
			{
				smi.SetupFetch();
			});
			fetching.movetopickupable.InitializeStates(GetNavTactic, fetcher, fetchTarget, fetching.pickup, fetching.fetchfail).Target(fetchTarget).EventHandlerTransition(GameHashes.TagsChanged, fetching.fetchfail, (StatesInstance smi, object obj) => smi.RootChore_ValidateRequiredTagOnTagChange && smi.RootChore_RequiredTag.IsValid && !fetchTarget.Get(smi).HasTag(smi.RootChore_RequiredTag))
				.Target(fetcher);
			fetching.pickup.DoPickup(fetchTarget, fetchResultTarget, fetchAmount, fetching.fetchcomplete, fetching.fetchfail).Exit(delegate(StatesInstance smi)
			{
				GameObject gameObject = smi.sm.fetchTarget.Get(smi);
				if (gameObject != null)
				{
					gameObject.Unsubscribe(1122777325, smi.OnMarkForMove);
				}
			});
			fetching.fetchcomplete.Enter(delegate(StatesInstance smi)
			{
				smi.FetchComplete();
			});
			fetching.fetchfail.Enter(delegate(StatesInstance smi)
			{
				smi.FetchFail();
			});
			delivering.DefaultState(delivering.next).OnSignal(currentdeliverycancelled, delivering.deliverfail).Enter("SetupDeliverables", delegate(StatesInstance smi)
			{
				smi.SetupDeliverables();
			})
				.Enter("delivering-on", delegate(StatesInstance smi)
				{
					smi.delivering = true;
				})
				.Exit("delivering-off", delegate(StatesInstance smi)
				{
					smi.delivering = false;
				});
			delivering.next.Enter("SetupDelivery", delegate(StatesInstance smi)
			{
				smi.SetupDelivery();
			});
			delivering.movetostorage.InitializeStates(GetNavTactic, fetcher, deliveryDestination, delivering.storing, delivering.deliverfail).Enter(delegate(StatesInstance smi)
			{
				if (deliveryObject.Get(smi) != null && deliveryObject.Get(smi).GetComponent<MinionIdentity>() != null)
				{
					deliveryObject.Get(smi).transform.SetLocalPosition(Vector3.zero);
					KBatchedAnimTracker component = deliveryObject.Get(smi).GetComponent<KBatchedAnimTracker>();
					component.symbol = new HashedString("snapTo_chest");
					component.offset = new Vector3(0f, 0f, 1f);
				}
			});
			delivering.storing.DoDelivery(fetcher, deliveryDestination, delivering.delivercomplete, delivering.deliverfail);
			delivering.deliverfail.Enter(delegate(StatesInstance smi)
			{
				smi.DeliverFail();
			});
			delivering.delivercomplete.Enter(delegate(StatesInstance smi)
			{
				smi.DeliverComplete();
			});
		}

		private NavTactic GetNavTactic(StatesInstance smi)
		{
			WorkerBase component = fetcher.Get(smi).GetComponent<WorkerBase>();
			if (component != null && component.IsFetchDrone())
			{
				return NavigationTactics.FetchDronePickup;
			}
			return NavigationTactics.ReduceTravelDistance;
		}
	}

	public bool IsFetching => base.smi.pickingup;

	public bool IsDelivering => base.smi.delivering;

	public GameObject GetFetchTarget => base.smi.sm.fetchTarget.Get(base.smi);

	public FetchAreaChore(Precondition.Context context)
		: base(context.chore.choreType, (IStateMachineTarget)context.consumerState.consumer, context.consumerState.choreProvider, run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, context.masterPriority.priority_class, context.masterPriority.priority_value, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		showAvailabilityInHoverText = false;
		base.smi = new StatesInstance(this, context);
	}

	public override void Cleanup()
	{
		base.Cleanup();
	}

	public override void Begin(Precondition.Context context)
	{
		base.smi.Begin(context);
		base.Begin(context);
	}

	protected override void End(string reason)
	{
		base.smi.End();
		base.End(reason);
	}

	private void OnTagsChanged(object data)
	{
		if (base.smi.sm.fetchTarget.Get(base.smi) != null)
		{
			Fail("Tags changed");
		}
	}

	private static bool IsPickupableStillValidForChore(Pickupable pickupable, FetchChore chore)
	{
		KPrefabID kPrefabID = pickupable.KPrefabID;
		if ((chore.criteria == FetchChore.MatchCriteria.MatchID && !chore.tags.Contains(kPrefabID.PrefabTag)) || (chore.criteria == FetchChore.MatchCriteria.MatchTags && !kPrefabID.HasTag(chore.tagsFirst)))
		{
			Debug.Log(string.Format("Pickupable {0} is not valid for chore because it is not or does not contain one of these tags: {1}", pickupable, string.Join(",", chore.tags)));
			return false;
		}
		if (chore.requiredTag.IsValid && !kPrefabID.HasTag(chore.requiredTag))
		{
			Debug.Log($"Pickupable {pickupable} is not valid for chore because it does not have the required tag: {chore.requiredTag}");
			return false;
		}
		if (kPrefabID.HasAnyTags(chore.forbiddenTags))
		{
			Debug.Log(string.Format("Pickupable {0} is not valid for chore because it has the forbidden tags: {1}", pickupable, string.Join(",", chore.forbiddenTags)));
			return false;
		}
		return pickupable.isChoreAllowedToPickup(chore.choreType);
	}

	private static Util.IterationInstruction gatherNearbyFetchChoresVisitor(object obj, ref (ChoreConsumerState, List<Precondition.Context>, List<Precondition.Context>) context)
	{
		Unsafe.As<FetchChore>(obj).CollectChoresFromGlobalChoreProvider(context.Item1, context.Item2, null, context.Item3, is_attempting_override: true);
		return Util.IterationInstruction.Continue;
	}

	public static void GatherNearbyFetchChores(FetchChore root_chore, Precondition.Context context, int x, int y, int radius, List<Precondition.Context> succeeded_contexts, List<Precondition.Context> failed_contexts)
	{
		(ChoreConsumerState, List<Precondition.Context>, List<Precondition.Context>) context2 = (context.consumerState, succeeded_contexts, failed_contexts);
		GameScenePartitioner.Instance.VisitEntries<(ChoreConsumerState, List<Precondition.Context>, List<Precondition.Context>)>(x - radius, y - radius, radius * 2 + 1, radius * 2 + 1, GameScenePartitioner.Instance.fetchChoreLayer, (GameScenePartitioner.VisitorRef<(ChoreConsumerState, List<Precondition.Context>, List<Precondition.Context>)>)gatherNearbyFetchChoresVisitor, ref context2);
	}
}
