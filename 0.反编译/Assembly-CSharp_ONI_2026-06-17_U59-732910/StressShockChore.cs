using System;
using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class StressShockChore : Chore<StressShockChore.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, StressShockChore, object>.GameInstance
	{
		public Notification notification;

		[MySmiReq]
		public BionicBatteryMonitor.Instance batteryMonitor;

		public BionicBatteryMonitor.WattageModifier powerDrainModifier = new BionicBatteryMonitor.WattageModifier("StressShockChore", string.Format(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.STANDARD_ACTIVE_TEMPLATE, DUPLICANTS.TRAITS.STRESSSHOCKER.DRAIN_ATTRIBUTE, "<b>+</b>" + GameUtil.GetFormattedWattage(STRESS.SHOCKER.POWER_CONSUMPTION_RATE)), STRESS.SHOCKER.POWER_CONSUMPTION_RATE, STRESS.SHOCKER.POWER_CONSUMPTION_RATE);

		public StatesInstance(StressShockChore master, GameObject shocker, Notification notification)
			: base(master)
		{
			base.sm.shocker.Set(shocker, base.smi);
			this.notification = notification;
		}

		public void SetDrainModifierActiveState(bool draining)
		{
			if (draining)
			{
				batteryMonitor.AddOrUpdateModifier(powerDrainModifier);
			}
			else
			{
				batteryMonitor.RemoveModifier(powerDrainModifier.id);
			}
		}

		public void FindDestination()
		{
			int num = FindIdleCell();
			if (num != -1 && num != Grid.PosToCell(base.gameObject))
			{
				base.sm.targetMoveLocation.Set(num, base.smi);
				GoTo(base.sm.shocking.runAroundShockingStuff);
				return;
			}
			num = FindMinionTarget();
			if (num != -1 && num != Grid.PosToCell(base.gameObject))
			{
				base.sm.targetMoveLocation.Set(num, base.smi);
				GoTo(base.sm.shocking.runAroundShockingStuff);
			}
			else
			{
				base.sm.targetMoveLocation.Set(Grid.PosToCell(base.gameObject), base.smi);
				GoTo(base.sm.shocking.standStillShockingStuff);
			}
		}

		private int FindMinionTarget()
		{
			Navigator component = base.smi.gameObject.GetComponent<Navigator>();
			if (component == null)
			{
				return Grid.InvalidCell;
			}
			int num = int.MaxValue;
			int result = Grid.InvalidCell;
			List<MinionIdentity> worldItems = Components.LiveMinionIdentities.GetWorldItems(base.smi.gameObject.GetMyWorldId());
			for (int i = 0; i < worldItems.Count; i++)
			{
				if (worldItems[i].IsNullOrDestroyed() || worldItems[i].gameObject == base.gameObject)
				{
					continue;
				}
				int num2 = Grid.PosToCell(worldItems[i]);
				if (component.CanReach(num2))
				{
					int navigationCost = component.GetNavigationCost(num2);
					if (navigationCost < num)
					{
						num = navigationCost;
						result = num2;
					}
				}
			}
			return result;
		}

		private int FindIdleCell()
		{
			Navigator component = base.smi.master.GetComponent<Navigator>();
			MinionPathFinderAbilities minionPathFinderAbilities = (MinionPathFinderAbilities)component.GetCurrentAbilities();
			minionPathFinderAbilities.SetIdleNavMaskEnabled(enabled: true);
			IdleCellQuery idleCellQuery = PathFinderQueries.idleCellQuery.Reset(GetComponent<MinionBrain>(), UnityEngine.Random.Range(90, 180));
			component.RunQuery(idleCellQuery);
			if (idleCellQuery.GetResultCell() == Grid.PosToCell(base.gameObject))
			{
				idleCellQuery = PathFinderQueries.idleCellQuery.Reset(GetComponent<MinionBrain>(), UnityEngine.Random.Range(0, 90));
				component.RunQuery(idleCellQuery);
			}
			minionPathFinderAbilities.SetIdleNavMaskEnabled(enabled: false);
			return idleCellQuery.GetResultCell();
		}

		public void ShockUpdateRender(StatesInstance smi, float dt)
		{
			if (smi.sm.faceLightningFX.Get(smi) != null)
			{
				smi.sm.faceLightningFX.Get(smi).transform.SetPosition(smi.FaceOriginLocation());
			}
			if (smi.sm.beamTarget.Get(smi) != null)
			{
				Vector3 vector = smi.sm.beamTarget.Get(smi).transform.position + Vector3.up / 2f;
				if (smi.sm.beamFX.Get(smi) == null)
				{
					smi.MakeBeam();
				}
				if (!CheckBlocked(Grid.PosToCell(smi.sm.beamFX.Get(smi).transform.position), Grid.PosToCell(vector)))
				{
					smi.AimBeam(vector, 0);
				}
			}
		}

		public void ShockUpdate200(StatesInstance smi, float dt)
		{
			float num = dt * STRESS.SHOCKER.POWER_CONSUMPTION_RATE;
			smi.sm.powerConsumed.Delta(num, smi);
			smi.batteryMonitor.ConsumePower(num);
			if (!(smi.sm.beamTarget.Get(smi) != null))
			{
				return;
			}
			Health component = smi.sm.beamTarget.Get(smi).GetComponent<Health>();
			if (component != null)
			{
				component.Damage(dt * STRESS.SHOCKER.DAMAGE_RATE);
				return;
			}
			Electrobank component2 = smi.sm.beamTarget.Get(smi).GetComponent<Electrobank>();
			if (component2 != null)
			{
				component2.Damage(dt * STRESS.SHOCKER.DAMAGE_RATE);
			}
			else if (smi.sm.beamTarget.Get(smi).HasTag(GameTags.Wires))
			{
				BuildingHP component3 = smi.sm.beamTarget.Get(smi).GetComponent<BuildingHP>();
				if (component3 != null)
				{
					component3.DoDamage(Mathf.RoundToInt(dt * STRESS.SHOCKER.DAMAGE_RATE));
				}
			}
		}

		public void PickShockTarget(StatesInstance smi)
		{
			int num = Grid.PosToCell(smi.master.gameObject);
			int worldId = Grid.WorldIdx[num];
			List<GameObject> list = new List<GameObject>();
			float num2 = UnityEngine.Random.Range(0f, 2f);
			foreach (Health worldItem in Components.Health.GetWorldItems(worldId))
			{
				if (!worldItem.IsNullOrDestroyed() && !(worldItem.gameObject == smi.master.gameObject))
				{
					int num3 = Grid.PosToCell(worldItem);
					float num4 = Vector2.Distance((Vector2)Grid.CellToPos2D(num), (Vector2)Grid.CellToPos2D(num3));
					if (num4 <= (float)STRESS.SHOCKER.SHOCK_RADIUS && num4 > num2 && !CheckBlocked(num, num3))
					{
						list.Add(worldItem.gameObject);
					}
				}
			}
			if (list.Count == 0)
			{
				Vector2I vector2I = Grid.CellToXY(num);
				List<ScenePartitionerEntry> list2 = new List<ScenePartitionerEntry>();
				GameScenePartitioner.Instance.GatherEntries(vector2I.x - STRESS.SHOCKER.SHOCK_RADIUS, vector2I.y - STRESS.SHOCKER.SHOCK_RADIUS, STRESS.SHOCKER.SHOCK_RADIUS * 2, STRESS.SHOCKER.SHOCK_RADIUS * 2, GameScenePartitioner.Instance.completeBuildings, list2);
				foreach (ScenePartitionerEntry item in list2)
				{
					if (!CheckBlocked(num, Grid.PosToCell(new Vector2(item.x, item.y))))
					{
						BuildingComplete buildingComplete = item.obj as BuildingComplete;
						if (buildingComplete != null)
						{
							list.Add(buildingComplete.gameObject);
						}
					}
				}
			}
			if (list.Count == 0)
			{
				ClearBeam(clearFaceFX: false);
				return;
			}
			GameObject random = list.GetRandom();
			GameObject gameObject = random;
			float num5 = float.MaxValue;
			foreach (GameObject item2 in list)
			{
				if (list.Count <= 1 || !(item2 == base.sm.previousTarget.Get(smi)))
				{
					float num6 = Vector2.Distance((Vector2)base.transform.position, (Vector2)item2.transform.position);
					if (num6 < num5)
					{
						num5 = num6;
						gameObject = item2;
					}
				}
			}
			if (random != null && gameObject != null && UnityEngine.Random.Range(0, 100) > 50)
			{
				base.sm.beamTarget.Set(gameObject, smi);
			}
			else
			{
				base.sm.beamTarget.Set(gameObject, smi);
			}
		}

		public void MakeBeam()
		{
			GameObject gameObject = new GameObject("shockFX");
			gameObject.SetActive(value: false);
			KBatchedAnimController kBatchedAnimController = gameObject.AddComponent<KBatchedAnimController>();
			base.sm.beamFX.Set(kBatchedAnimController, base.smi);
			kBatchedAnimController.SwapAnims(new KAnimFile[1] { Assets.GetAnim("bionic_dupe_stress_beam_fx_kanim") });
			gameObject.SetActive(value: true);
			bool symbolVisible;
			Vector3 position = GetComponent<KBatchedAnimController>().GetSymbolTransform("snapTo_hat", out symbolVisible).GetColumn(3);
			position -= Vector3.up / 4f;
			position.z = base.transform.position.z + 0.01f;
			gameObject.transform.position = position;
			kBatchedAnimController.Play("beam1", KAnim.PlayMode.Loop);
			if (base.sm.faceLightningFX.Get(base.smi) != null)
			{
				Util.KDestroyGameObject(base.sm.faceLightningFX.Get(base.smi).gameObject);
				base.sm.faceLightningFX.Set(null, base.smi);
			}
			GameObject gameObject2 = new GameObject("faceLightningFX");
			gameObject2.SetActive(value: false);
			KBatchedAnimController kBatchedAnimController2 = gameObject2.AddComponent<KBatchedAnimController>();
			base.sm.faceLightningFX.Set(kBatchedAnimController2, base.smi);
			kBatchedAnimController2.SwapAnims(new KAnimFile[1] { Assets.GetAnim("bionic_dupe_stress_lightning_fx_kanim") });
			gameObject2.SetActive(value: true);
			gameObject2.transform.position = FaceOriginLocation();
			kBatchedAnimController2.Play("lightning", KAnim.PlayMode.Loop);
			GameObject gameObject3 = new GameObject("impactFX");
			gameObject3.SetActive(value: false);
			KBatchedAnimController kBatchedAnimController3 = gameObject3.AddComponent<KBatchedAnimController>();
			base.sm.impactFX.Set(kBatchedAnimController3, base.smi);
			kBatchedAnimController3.SwapAnims(new KAnimFile[1] { Assets.GetAnim("bionic_dupe_stress_beam_impact_fx_kanim") });
			gameObject3.SetActive(value: true);
			kBatchedAnimController3.Play("stress_beam_impact_fx", KAnim.PlayMode.Loop);
		}

		public Vector3 FaceOriginLocation()
		{
			bool symbolVisible;
			Vector3 result = GetComponent<KBatchedAnimController>().GetSymbolTransform("snapTo_hat", out symbolVisible).GetColumn(3);
			result -= Vector3.up / 4f;
			result.z = Grid.GetLayerZ(Grid.SceneLayer.FXFront);
			return result;
		}

		public void ClearBeam(bool clearFaceFX = true)
		{
			base.sm.previousTarget.Set(base.sm.beamTarget.Get(base.smi), base.smi);
			base.sm.beamTarget.Set(null, base.smi);
			if (base.sm.beamFX.Get(base.smi) != null)
			{
				Util.KDestroyGameObject(base.sm.beamFX.Get(base.smi).gameObject);
				base.sm.beamFX.Set(null, base.smi);
			}
			if (base.sm.impactFX.Get(base.smi) != null)
			{
				Util.KDestroyGameObject(base.sm.impactFX.Get(base.smi).gameObject);
				base.sm.impactFX.Set(null, base.smi);
			}
			if (clearFaceFX && base.sm.faceLightningFX.Get(base.smi) != null)
			{
				Util.KDestroyGameObject(base.sm.faceLightningFX.Get(base.smi).gameObject);
				base.sm.faceLightningFX.Set(null, base.smi);
			}
		}

		public void AimBeam(Vector3 targetPosition, int beamIdx)
		{
			Vector3 position = FaceOriginLocation();
			position.z = base.transform.position.z + 0.01f;
			base.smi.sm.beamFX.Get(base.smi).transform.SetPosition(position);
			Vector3 v = Vector3.Normalize(targetPosition - base.smi.sm.beamFX.Get(base.smi).transform.position);
			float rotation = MathUtil.AngleSigned(Vector3.up, v, Vector3.forward) + 90f;
			base.smi.sm.beamFX.Get(base.smi).Rotation = rotation;
			base.smi.sm.impactFX.Get(base.smi).transform.position = targetPosition;
			base.smi.sm.faceLightningFX.Get(base.smi).FlipX = targetPosition.x < base.smi.sm.faceLightningFX.Get(base.smi).transform.position.x;
			Vector3 position2 = base.smi.sm.beamFX.Get(base.smi).transform.position;
			position2.z = 0f;
			Vector3 b = targetPosition;
			b.z = 0f;
			float num = Vector3.Distance(position2, b);
			if (num > 3f)
			{
				if (base.smi.sm.beamFX.Get(base.smi).CurrentAnim == null || base.smi.sm.beamFX.Get(base.smi).CurrentAnim.name != "beam3")
				{
					base.smi.sm.beamFX.Get(base.smi).Play("beam3", KAnim.PlayMode.Loop);
				}
				base.smi.sm.beamFX.Get(base.smi).animWidth = num / 3f;
			}
			else if (num > 2f)
			{
				if (base.smi.sm.beamFX.Get(base.smi).CurrentAnim == null || base.smi.sm.beamFX.Get(base.smi).CurrentAnim.name != "beam2")
				{
					base.smi.sm.beamFX.Get(base.smi).Play("beam2", KAnim.PlayMode.Loop);
				}
				base.smi.sm.beamFX.Get(base.smi).animWidth = num / 2f;
			}
			else
			{
				if (base.smi.sm.beamFX.Get(base.smi).CurrentAnim == null || base.smi.sm.beamFX.Get(base.smi).CurrentAnim.name != "beam1")
				{
					base.smi.sm.beamFX.Get(base.smi).Play("beam1", KAnim.PlayMode.Loop);
				}
				base.smi.sm.beamFX.Get(base.smi).animWidth = num;
			}
		}

		public void ShowBeam(bool show)
		{
			if (base.smi.sm.impactFX.Get(base.smi) != null)
			{
				base.smi.sm.impactFX.Get(base.smi).enabled = show;
			}
			if (base.smi.sm.beamFX.Get(base.smi) != null)
			{
				base.smi.sm.beamFX.Get(base.smi).enabled = show;
			}
		}
	}

	public class States : GameStateMachine<States, StatesInstance, StressShockChore>
	{
		public class ShockStates : State
		{
			public State findDestination;

			public State runAroundShockingStuff;

			public State standStillShockingStuff;
		}

		public TargetParameter shocker;

		public ObjectParameter<KBatchedAnimController[]> cosmeticBeamFXs;

		public ObjectParameter<KBatchedAnimController> beamFX;

		public ObjectParameter<KBatchedAnimController> impactFX;

		public ObjectParameter<KBatchedAnimController> faceLightningFX;

		public ObjectParameter<GameObject> beamTarget;

		public ObjectParameter<GameObject> previousTarget;

		public IntParameter targetMoveLocation;

		public FloatParameter powerConsumed;

		public ShockStates shocking;

		public State delay;

		public State complete;

		public State offline;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = shocking.findDestination;
			base.serializable = SerializeType.Never;
			Target(shocker);
			shocking.EventTransition(GameHashes.BionicOffline, offline).DefaultState(shocking.findDestination).ToggleAnims("anim_loco_stressshocker_kanim")
				.ParamTransition(powerConsumed, complete, (StatesInstance smi, float p) => p >= STRESS.SHOCKER.MAX_POWER_USE)
				.Enter(delegate(StatesInstance smi)
				{
					smi.MakeBeam();
				})
				.Exit(delegate(StatesInstance smi)
				{
					smi.ClearBeam();
				});
			shocking.findDestination.Enter("FindDestination", delegate(StatesInstance smi)
			{
				smi.ShowBeam(show: false);
				smi.FindDestination();
			}).Update(delegate(StatesInstance smi, float dt)
			{
				float delta_value = dt * STRESS.SHOCKER.FAKE_POWER_CONSUMPTION_RATE;
				smi.sm.powerConsumed.Delta(delta_value, smi);
				smi.FindDestination();
			}, UpdateRate.SIM_1000ms);
			shocking.runAroundShockingStuff.MoveTo((StatesInstance smi) => smi.sm.targetMoveLocation.Get(smi), shocking.findDestination, delay).Toggle("BatteryDrain", AddBatteryDrainModifier, RemoveBatteryDrainModifier).Enter(delegate(StatesInstance smi)
			{
				smi.ShowBeam(show: true);
			})
				.Update(delegate(StatesInstance smi, float dt)
				{
					smi.PickShockTarget(smi);
					smi.ShockUpdate200(smi, dt);
				})
				.Update(delegate(StatesInstance smi, float dt)
				{
					smi.ShockUpdateRender(smi, dt);
				}, UpdateRate.RENDER_EVERY_TICK);
			shocking.standStillShockingStuff.Toggle("BatteryDrain", AddBatteryDrainModifier, RemoveBatteryDrainModifier).Enter(delegate(StatesInstance smi)
			{
				smi.ShowBeam(show: true);
			}).PlayAnim("interrupt_shocker", KAnim.PlayMode.Loop)
				.ScheduleGoTo(2f, delay)
				.Update(delegate(StatesInstance smi, float dt)
				{
					smi.PickShockTarget(smi);
					smi.ShockUpdate200(smi, dt);
				})
				.Update(delegate(StatesInstance smi, float dt)
				{
					smi.ShockUpdateRender(smi, dt);
				}, UpdateRate.RENDER_EVERY_TICK);
			delay.ScheduleGoTo(0.5f, shocking);
			complete.Enter(delegate(StatesInstance smi)
			{
				smi.StopSM("complete");
			});
			offline.Enter(ForceStressMonitorToTimeOut).ReturnSuccess();
		}
	}

	public const float FaceBeamZOffset = 0.01f;

	private static bool CheckBlocked(int sourceCell, int destinationCell)
	{
		HashSet<int> hashSet = new HashSet<int>();
		Grid.CollectCellsInLine(sourceCell, destinationCell, hashSet);
		bool result = false;
		foreach (int item in hashSet)
		{
			if (Grid.Solid[item])
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static void AddBatteryDrainModifier(StatesInstance smi)
	{
		smi.SetDrainModifierActiveState(draining: true);
	}

	public static void RemoveBatteryDrainModifier(StatesInstance smi)
	{
		smi.SetDrainModifierActiveState(draining: false);
	}

	public static void ForceStressMonitorToTimeOut(StatesInstance smi)
	{
		smi.GetSMI<StressBehaviourMonitor.Instance>()?.ManualSetStressTier2TimeCounter(150f);
	}

	public StressShockChore(ChoreType chore_type, IStateMachineTarget target, Notification notification, Action<Chore> on_complete = null)
		: base(Db.Get().ChoreTypes.StressShock, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, on_complete, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.compulsory, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new StatesInstance(this, target.gameObject, notification);
	}
}
