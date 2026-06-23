using System;
using System.Collections.Generic;
using System.Linq;
using KSerialization;
using UnityEngine;

public class MissileLauncher : GameStateMachine<MissileLauncher, MissileLauncher.Instance, IStateMachineTarget, MissileLauncher.Def>
{
	public class Def : BaseDef
	{
		public static readonly CellOffset LaunchOffset = new CellOffset(0, 4);

		public float launchSpeed = 30f;

		public float rotationSpeed = 100f;

		public static readonly Vector2I launchRange = new Vector2I(16, 32);

		public float scanningAngle = 50f;

		public float maxAngle = 80f;
	}

	public new class Instance : GameInstance, IMissileSelectionInterface
	{
		[MyCmpReq]
		public Operational Operational;

		public Storage MissileStorage;

		public Storage LongRangeStorage;

		private Storage LoadingStorage;

		public ManualDeliveryKG ManualDeliveryMissile;

		public ManualDeliveryKG ManualDeliveryLongRange;

		[MyCmpReq]
		public KSelectable Selectable;

		[MyCmpReq]
		public FlatTagFilterable TargetFilter;

		private EntityClusterDestinationSelector clusterDestinationSelector;

		[Serialize]
		private Dictionary<Tag, bool> ammunitionPermissions = new Dictionary<Tag, bool> { { "MissileBasic", true } };

		private Vector3 launchPosition;

		private Vector2I launchXY;

		private float launchAnimTime;

		public KBatchedAnimController cannonAnimController;

		public GameObject cannonGameObject;

		public float cannonRotation;

		public float simpleAngle;

		private Tag missileElement;

		private MeterController meter;

		private MeterController longRangemeter;

		public State CooldownGoToState;

		private WorldContainer worldContainer;

		private static List<ChoreType> empty_chore_list = new List<ChoreType>();

		public WorldContainer myWorld
		{
			get
			{
				if (worldContainer == null)
				{
					worldContainer = this.GetMyWorld();
				}
				return worldContainer;
			}
		}

		public bool IsAnyCosmicBlastShotAllowed()
		{
			return MissileLauncherConfig.CosmicBlastShotTypes.Any((Tag match) => AmmunitionIsAllowed(match));
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			Components.MissileLaunchers.Add(this);
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			string name = component.name + ".cannon";
			base.smi.cannonGameObject = new GameObject(name);
			base.smi.cannonGameObject.SetActive(value: false);
			base.smi.cannonGameObject.transform.parent = component.transform;
			KPrefabID kPrefabID = base.smi.cannonGameObject.AddComponent<KPrefabID>();
			kPrefabID.PrefabTag = new Tag(name);
			base.smi.cannonAnimController = base.smi.cannonGameObject.AddComponent<KBatchedAnimController>();
			base.smi.cannonAnimController.AnimFiles = new KAnimFile[1] { component.AnimFiles[0] };
			base.smi.cannonAnimController.initialAnim = "Cannon_off";
			base.smi.cannonAnimController.isMovable = true;
			base.smi.cannonAnimController.SetSceneLayer(Grid.SceneLayer.Building);
			component.SetSymbolVisiblity("cannon_target", is_visible: false);
			bool symbolVisible;
			Vector4 column = component.GetSymbolTransform(new HashedString("cannon_target"), out symbolVisible).GetColumn(3);
			Vector3 position = column;
			position.z = Grid.GetLayerZ(Grid.SceneLayer.Building);
			base.smi.cannonGameObject.transform.SetPosition(position);
			launchPosition = position;
			Grid.PosToXY(launchPosition, out launchXY);
			base.smi.cannonGameObject.SetActive(value: true);
			base.smi.sm.cannonTarget.Set(base.smi.cannonGameObject, base.smi);
			KAnim.Anim anim = component.AnimFiles[0].GetData().GetAnim("Cannon_shooting_pre");
			if (anim != null)
			{
				launchAnimTime = anim.totalTime / 2f;
			}
			else
			{
				Debug.LogWarning("MissileLauncher anim data is missing");
				launchAnimTime = 1f;
			}
			meter = new MeterController(component, "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer);
			longRangemeter = new MeterController(component, "meter_target_longrange", "meter_longrange", Meter.Offset.Infront, Grid.SceneLayer.NoLayer);
			Subscribe(-1201923725, OnHighlight);
			Subscribe(-905833192, OnCopySettings);
			Storage[] components = base.smi.gameObject.GetComponents<Storage>();
			Storage[] array = components;
			foreach (Storage storage in array)
			{
				if (storage.storageID == "MissileBasic")
				{
					MissileStorage = storage;
				}
				else if (storage.storageID == "MissileLongRange")
				{
					LongRangeStorage = storage;
				}
				else if (storage.storageID == "CondiutStorage")
				{
					LoadingStorage = storage;
				}
			}
			Subscribe(-1697596308, OnStorage);
			FlatTagFilterable component2 = base.smi.master.GetComponent<FlatTagFilterable>();
			foreach (GameObject item in Assets.GetPrefabsWithTag(GameTags.Comet))
			{
				if (!item.HasTag(GameTags.DeprecatedContent))
				{
					if (!component2.tagOptions.Contains(item.PrefabID()))
					{
						component2.tagOptions.Add(item.PrefabID());
						component2.selectedTags.Add(item.PrefabID());
					}
					component2.selectedTags.Remove(GassyMooCometConfig.ID);
					component2.selectedTags.Remove(DieselMooCometConfig.ID);
				}
			}
			ManualDeliveryKG[] components2 = base.smi.gameObject.GetComponents<ManualDeliveryKG>();
			ManualDeliveryKG[] array2 = components2;
			foreach (ManualDeliveryKG manualDeliveryKG in array2)
			{
				if (manualDeliveryKG.RequestedItemTag == "MissileBasic")
				{
					ManualDeliveryMissile = manualDeliveryKG;
				}
				else
				{
					ManualDeliveryLongRange = manualDeliveryKG;
				}
			}
		}

		public override void StartSM()
		{
			base.StartSM();
			OnStorage(null);
			FlatTagFilterable component = base.smi.master.GetComponent<FlatTagFilterable>();
			component.currentlyUserAssignable = AmmunitionIsAllowed("MissileBasic");
			clusterDestinationSelector = base.smi.master.GetComponent<EntityClusterDestinationSelector>();
			if (clusterDestinationSelector != null)
			{
				clusterDestinationSelector.assignable = IsAnyCosmicBlastShotAllowed();
			}
			UpdateAmmunitionDelivery();
			UpdateMeterVisibility();
		}

		protected override void OnCleanUp()
		{
			Components.MissileLaunchers.Remove(this);
			Unsubscribe(-1201923725, OnHighlight);
			base.OnCleanUp();
		}

		private void OnHighlight(object _)
		{
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			base.smi.cannonAnimController.HighlightColour = component.HighlightColour;
		}

		private void OnCopySettings(object data)
		{
			GameObject gameObject = (GameObject)data;
			if (!(gameObject != null))
			{
				return;
			}
			Instance sMI = gameObject.GetSMI<Instance>();
			if (sMI == null)
			{
				return;
			}
			ammunitionPermissions.Clear();
			foreach (KeyValuePair<Tag, bool> ammunitionPermission in sMI.ammunitionPermissions)
			{
				ChangeAmmunition(ammunitionPermission.Key, sMI.AmmunitionIsAllowed(ammunitionPermission.Key));
			}
			FlatTagFilterable component = base.smi.master.GetComponent<FlatTagFilterable>();
			component.currentlyUserAssignable = AmmunitionIsAllowed("MissileBasic");
			clusterDestinationSelector = base.smi.master.GetComponent<EntityClusterDestinationSelector>();
			if (clusterDestinationSelector != null)
			{
				clusterDestinationSelector.assignable = IsAnyCosmicBlastShotAllowed();
			}
			if (sMI.sm.longRangeTarget != null)
			{
				base.sm.longRangeTarget.Set(sMI.sm.longRangeTarget.Get(sMI), this);
			}
		}

		private void OnStorage(object data)
		{
			if (LoadingStorage.items.Count > 0)
			{
				KPrefabID component = LoadingStorage.items[0].GetComponent<KPrefabID>();
				if (AmmunitionIsAllowed(component.PrefabTag))
				{
					Pickupable component2 = component.GetComponent<Pickupable>();
					Storage storage = null;
					if (component.PrefabTag == "MissileBasic")
					{
						storage = MissileStorage;
					}
					else if (MissileLauncherConfig.CosmicBlastShotTypes.Contains(component.PrefabTag))
					{
						storage = LongRangeStorage;
					}
					if (storage != null && storage.Capacity() - storage.MassStored() >= component2.PrimaryElement.Mass)
					{
						LoadingStorage.Transfer(component2.gameObject, storage, block_events: true, hide_popups: true);
					}
				}
			}
			meter.SetPositionPercent(Mathf.Clamp01(MissileStorage.MassStored() / MissileStorage.capacityKg));
			longRangemeter.SetPositionPercent(Mathf.Clamp01(LongRangeStorage.MassStored() / LongRangeStorage.capacityKg));
		}

		private void UpdateMeterVisibility()
		{
			meter.gameObject.SetActive(AmmunitionIsAllowed("MissileBasic"));
			longRangemeter.gameObject.SetActive(IsAnyCosmicBlastShotAllowed());
		}

		public void Searching(float dt)
		{
			if (!FindMeteor())
			{
				FindLongRangeTarget();
			}
			RotateCannon(dt, base.def.rotationSpeed / 2f);
			if (base.smi.sm.rotationComplete.Get(base.smi))
			{
				cannonRotation *= -1f;
				base.smi.sm.rotationComplete.Set(value: false, base.smi);
			}
		}

		private bool FindMeteor()
		{
			if (MissileStorage.items.Count > 0)
			{
				GameObject gameObject = ChooseClosestInterceptionPoint(myWorld.id);
				if (gameObject != null)
				{
					base.smi.sm.meteorTarget.Set(gameObject, base.smi);
					Comet component = gameObject.GetComponent<Comet>();
					component.Targeted = true;
					base.smi.cannonRotation = CalculateLaunchAngle(gameObject.transform.position);
					return true;
				}
			}
			return false;
		}

		private bool FindLongRangeTarget()
		{
			if (LongRangeStorage.items.Count > 0)
			{
				GameObject gameObject = null;
				if (clusterDestinationSelector != null)
				{
					if (clusterDestinationSelector.GetDestination() != myWorld.GetComponent<ClusterGridEntity>().Location)
					{
						ClusterGridEntity visibleEntityOfLayerAtCell = ClusterGrid.Instance.GetVisibleEntityOfLayerAtCell(clusterDestinationSelector.GetDestination(), EntityLayer.Meteor);
						gameObject = ((visibleEntityOfLayerAtCell != null) ? visibleEntityOfLayerAtCell.gameObject : null);
					}
				}
				else
				{
					GameplayEventInstance gameplayEventInstance = GameplayEventManager.Instance.GetGameplayEventInstance(Db.Get().GameplayEvents.LargeImpactor.IdHash);
					if (gameplayEventInstance != null)
					{
						LargeImpactorEvent.StatesInstance statesInstance = (LargeImpactorEvent.StatesInstance)gameplayEventInstance.smi;
						GameObject impactorInstance = statesInstance.impactorInstance;
						gameObject = ((impactorInstance != null) ? impactorInstance.gameObject : null);
					}
				}
				if (gameObject != null)
				{
					Vector3 position = base.transform.position;
					position.y += 50f;
					if (IsPathClear(launchPosition, position))
					{
						base.smi.sm.longRangeTarget.Set(gameObject, base.smi);
						base.smi.cannonRotation = CalculateLaunchAngle(position);
						return true;
					}
				}
			}
			return false;
		}

		private float CalculateLaunchAngle(Vector3 targetPosition)
		{
			Vector3 v = Vector3.Normalize(targetPosition - launchPosition);
			return MathUtil.AngleSigned(Vector3.up, v, Vector3.forward);
		}

		public void LaunchMissile()
		{
			GameObject gameObject = MissileStorage.FindFirst("MissileBasic");
			if (gameObject != null)
			{
				Pickupable pickupable = gameObject.GetComponent<Pickupable>();
				if (pickupable.TotalAmount <= 1f)
				{
					MissileStorage.Drop(pickupable.gameObject);
				}
				else
				{
					pickupable = EntitySplitter.Split(pickupable, 1f);
				}
				pickupable.allowedChoreTypes = empty_chore_list;
				SetMissileElement(gameObject);
				GameObject gameObject2 = base.smi.sm.meteorTarget.Get(base.smi);
				if (!gameObject2.IsNullOrDestroyed())
				{
					MissileProjectile.StatesInstance sMI = pickupable.GetSMI<MissileProjectile.StatesInstance>();
					sMI.PrepareLaunch(gameObject2.GetComponent<Comet>(), base.def.launchSpeed, launchPosition, base.smi.cannonRotation);
					CooldownGoToState = base.sm.Cooldown.basic;
				}
			}
		}

		public void LaunchLongRangeMissile()
		{
			GameObject gameObject = null;
			foreach (Tag cosmicBlastShotType in MissileLauncherConfig.CosmicBlastShotTypes)
			{
				gameObject = LongRangeStorage.FindFirst(cosmicBlastShotType);
				if (gameObject != null)
				{
					break;
				}
			}
			if (gameObject != null)
			{
				Pickupable pickupable = gameObject.GetComponent<Pickupable>();
				if (pickupable.TotalAmount <= 1f)
				{
					LongRangeStorage.Drop(pickupable.gameObject);
				}
				else
				{
					pickupable = EntitySplitter.Split(pickupable, 1f);
				}
				pickupable.allowedChoreTypes = empty_chore_list;
				SetMissileElement(gameObject);
				GameObject gameObject2 = base.smi.sm.longRangeTarget.Get(base.smi);
				if (!gameObject2.IsNullOrDestroyed())
				{
					MissileLongRangeProjectile.StatesInstance sMI = pickupable.GetSMI<MissileLongRangeProjectile.StatesInstance>();
					sMI.PrepareLaunch(gameObject2, base.def.launchSpeed, launchPosition, base.smi.cannonRotation);
					CooldownGoToState = base.sm.Cooldown.longrange;
					base.smi.sm.longRangeTarget.Set(null, base.smi);
				}
			}
		}

		private void SetMissileElement(GameObject missile)
		{
			missileElement = missile.GetComponent<PrimaryElement>().Element.tag;
			GameObject prefab = Assets.GetPrefab(missileElement);
			if (prefab == null)
			{
				Debug.LogWarning($"Missing element {missileElement} for missile launcher. Defaulting to IronOre");
				missileElement = GameTags.IronOre;
			}
		}

		public GameObject ChooseClosestInterceptionPoint(int world_id)
		{
			GameObject result = null;
			List<Comet> items = Components.Meteors.GetItems(world_id);
			float num = Def.launchRange.y;
			foreach (Comet item in items)
			{
				if (!item.IsNullOrDestroyed() && !item.Targeted && TargetFilter.selectedTags.Contains(item.typeID))
				{
					Vector3 targetPosition = item.TargetPosition;
					float timeToCollision;
					Vector3 vector = CalculateCollisionPoint(targetPosition, item.Velocity, out timeToCollision);
					int num2 = Grid.PosToCell(vector);
					float num3 = Vector3.Distance(vector, launchPosition);
					if (num3 < num && timeToCollision > launchAnimTime && IsMeteorInRange(vector) && IsPathClear(launchPosition, targetPosition))
					{
						result = item.gameObject;
						num = num3;
					}
				}
			}
			return result;
		}

		private bool IsMeteorInRange(Vector3 interception_point)
		{
			Grid.PosToXY(interception_point, out var xy);
			return Math.Abs(xy.X - launchXY.X) <= Def.launchRange.X && xy.Y - launchXY.Y > 0 && xy.Y - launchXY.Y <= Def.launchRange.Y;
		}

		public bool IsPathClear(Vector3 startPoint, Vector3 endPoint)
		{
			Vector2I vector2I = Grid.PosToXY(startPoint);
			Vector2I vector2I2 = Grid.PosToXY(endPoint);
			return Grid.TestLineOfSight(vector2I.x, vector2I.y, vector2I2.x, vector2I2.y, IsCellBlockedFromSky, blocking_tile_visible: false, allow_invalid_cells: true);
		}

		public bool IsCellBlockedFromSky(int cell)
		{
			if (Grid.IsValidCell(cell) && Grid.WorldIdx[cell] == myWorld.id)
			{
				return Grid.Solid[cell];
			}
			Grid.CellToXY(cell, out var _, out var y);
			return y <= launchXY.Y;
		}

		public Vector3 CalculateCollisionPoint(Vector3 targetPosition, Vector3 targetVelocity, out float timeToCollision)
		{
			Vector3 vector = targetVelocity - base.smi.def.launchSpeed * (targetPosition - launchPosition).normalized;
			timeToCollision = (targetPosition - launchPosition).magnitude / vector.magnitude;
			return targetPosition + targetVelocity * timeToCollision;
		}

		public void HasLineOfSight()
		{
			bool flag = false;
			bool flag2 = true;
			Building component = GetComponent<Building>();
			Extents extents = component.GetExtents();
			int val = launchXY.x - Def.launchRange.X;
			int val2 = launchXY.x + Def.launchRange.X;
			int y = extents.y + extents.height;
			int num = Grid.XYToCell(Math.Max((int)myWorld.minimumBounds.x, val), y);
			int num2 = Grid.XYToCell(Math.Min((int)myWorld.maximumBounds.x, val2), y);
			for (int i = num; i <= num2; i++)
			{
				flag = flag || Grid.ExposedToSunlight[i] <= 0;
				flag2 = flag2 && Grid.ExposedToSunlight[i] <= 0;
			}
			Selectable.ToggleStatusItem(PartiallyBlockedStatus, flag && !flag2);
			Selectable.ToggleStatusItem(NoSurfaceSight, flag2);
			base.smi.sm.fullyBlocked.Set(flag2, base.smi);
		}

		public bool MeteorDetected()
		{
			return Components.Meteors.GetItems(myWorld.id).Count > 0;
		}

		public void SetOreChunk()
		{
			if (!missileElement.IsValid)
			{
				Debug.LogWarning($"Missing element {missileElement} for missile launcher. Defaulting to IronOre");
				missileElement = GameTags.IronOre;
			}
			GameObject prefab = Assets.GetPrefab(missileElement);
			KAnim.Build.Symbol symbolByIndex = prefab.GetComponent<KBatchedAnimController>().AnimFiles[0].GetData().build.GetSymbolByIndex(0u);
			SymbolOverrideController component = base.gameObject.GetComponent<SymbolOverrideController>();
			component.AddSymbolOverride("Shell", symbolByIndex);
		}

		public void SpawnOre()
		{
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			bool symbolVisible;
			Vector4 column = component.GetSymbolTransform("Shell", out symbolVisible).GetColumn(3);
			Vector3 position = column;
			position.z = Grid.GetLayerZ(Grid.SceneLayer.Ore);
			GameObject prefab = Assets.GetPrefab(missileElement);
			PrimaryElement component2 = prefab.GetComponent<PrimaryElement>();
			GameObject gameObject = component2.Element.substance.SpawnResource(position, SHELL_MASS, SHELL_TEMPERATURE, byte.MaxValue, 0);
		}

		public void RotateCannon(float dt, float rotation_speed)
		{
			float num = cannonRotation - simpleAngle;
			if (num > 180f)
			{
				num -= 360f;
			}
			else if (num < -180f)
			{
				num += 360f;
			}
			float num2 = rotation_speed * dt;
			if (num > 0f && num2 < num)
			{
				simpleAngle += num2;
				cannonAnimController.Rotation = simpleAngle;
			}
			else if (num < 0f && 0f - num2 > num)
			{
				simpleAngle -= num2;
				cannonAnimController.Rotation = simpleAngle;
			}
			else
			{
				simpleAngle = cannonRotation;
				cannonAnimController.Rotation = simpleAngle;
				base.smi.sm.rotationComplete.Set(value: true, base.smi);
			}
		}

		public bool ShouldRotateToLongRange()
		{
			return !base.smi.sm.longRangeTarget.Get(base.smi).IsNullOrDestroyed() && LongRangeStorage.items.Count > 0 && IsPathClear(launchPosition, launchPosition + new Vector3(0f, 50f, 0f));
		}

		public void RotateToMeteor(float dt)
		{
			GameObject gameObject = base.sm.meteorTarget.Get(this);
			float num;
			if (!gameObject.IsNullOrDestroyed())
			{
				num = CalculateLaunchAngle(gameObject.transform.position);
			}
			else
			{
				if (!ShouldRotateToLongRange())
				{
					return;
				}
				Vector3 position = base.transform.position;
				position.y += 50f;
				num = CalculateLaunchAngle(position);
			}
			float num2 = num - simpleAngle;
			if (num2 > 180f)
			{
				num2 -= 360f;
			}
			else if (num2 < -180f)
			{
				num2 += 360f;
			}
			float num3 = base.def.rotationSpeed * dt;
			if (num2 > 0f && num3 < num2)
			{
				simpleAngle += num3;
				cannonAnimController.Rotation = simpleAngle;
			}
			else if (num2 < 0f && 0f - num3 > num2)
			{
				simpleAngle -= num3;
				cannonAnimController.Rotation = simpleAngle;
			}
			else
			{
				base.smi.sm.rotationComplete.Set(value: true, base.smi);
			}
		}

		public void ChangeAmmunition(Tag tag, bool allowed)
		{
			if (!ammunitionPermissions.ContainsKey(tag))
			{
				ammunitionPermissions.Add(tag, value: false);
			}
			ammunitionPermissions[tag] = allowed;
			UpdateAmmunitionDelivery();
			OnStorage(null);
			DropAmmunitionFromStorage(MissileStorage);
			DropAmmunitionFromStorage(LongRangeStorage);
			UpdateMeterVisibility();
		}

		public void OnRowToggleClick()
		{
			if (clusterDestinationSelector != null)
			{
				clusterDestinationSelector.assignable = IsAnyCosmicBlastShotAllowed();
			}
			GetComponent<FlatTagFilterable>().currentlyUserAssignable = AmmunitionIsAllowed("MissileBasic");
		}

		public List<Tag> GetValidAmmunitionTags()
		{
			List<Tag> list = new List<Tag> { "MissileBasic", "MissileLongRange" };
			GameplayEventInstance gameplayEventInstance = GameplayEventManager.Instance.GetGameplayEventInstance(Db.Get().GameplayEvents.LargeImpactor.IdHash);
			if (gameplayEventInstance == null)
			{
				list.Remove("MissileLongRange");
			}
			return list;
		}

		public bool AmmunitionIsAllowed(Tag tag)
		{
			if (!ammunitionPermissions.ContainsKey(tag))
			{
				return false;
			}
			return ammunitionPermissions[tag];
		}

		private void UpdateAmmunitionDelivery()
		{
			bool pause = false;
			bool flag = AmmunitionIsAllowed("MissileLongRange");
			if (flag)
			{
				ManualDeliveryLongRange.RequestedItemTag = GameTags.LongRangeMissile;
			}
			else if (flag)
			{
				ManualDeliveryLongRange.RequestedItemTag = "MissileLongRange";
			}
			else
			{
				pause = true;
			}
			ManualDeliveryLongRange.Pause(pause, "ammunitionnotallowed");
			ManualDeliveryMissile.Pause(!AmmunitionIsAllowed("MissileBasic"), "ammunitionnotallowed");
		}

		private void DropAmmunitionFromStorage(Storage targetStorage)
		{
			for (int num = targetStorage.items.Count - 1; num >= 0; num--)
			{
				GameObject gameObject = targetStorage.items[num];
				if (!(gameObject == null))
				{
					KPrefabID component = gameObject.GetComponent<KPrefabID>();
					if (!AmmunitionIsAllowed(component.PrefabTag))
					{
						targetStorage.Drop(gameObject);
					}
				}
			}
		}
	}

	public class OnState : State
	{
		public State searching;

		public State opening;

		public State shutdown;

		public State idle;
	}

	public class LaunchState : State
	{
		public State targeting;

		public State targetingLongRange;

		public State shoot;

		public State pst;
	}

	public class CooldownState : State
	{
		public State longrange;

		public State basic;
	}

	private static StatusItem NoSurfaceSight = new StatusItem("MissileLauncher_NoSurfaceSight", "BUILDING", "status_item_no_sky", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);

	private static StatusItem PartiallyBlockedStatus = new StatusItem("MissileLauncher_PartiallyBlocked", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);

	private static StatusItem LongRangeCooldown = new StatusItem("MissileLauncher_LongRangeCooldown", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);

	public float shutdownDuration = 50f;

	public float shootDelayDuration = 0.25f;

	public static float SHELL_MASS = 2.5f;

	public static float SHELL_TEMPERATURE = 353.15f;

	public BoolParameter rotationComplete;

	public ObjectParameter<GameObject> meteorTarget = new ObjectParameter<GameObject>();

	public TargetParameter cannonTarget;

	public BoolParameter fullyBlocked;

	public ObjectParameter<GameObject> longRangeTarget = new ObjectParameter<GameObject>();

	public static float longrangeCooldownTime = 10f;

	public State Off;

	public OnState On;

	public LaunchState Launch;

	public CooldownState Cooldown;

	public State Nosurfacesight;

	public State NoAmmo;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = Off;
		root.Update(delegate(Instance smi, float dt)
		{
			smi.HasLineOfSight();
		});
		Off.PlayAnim("inoperational").EventTransition(GameHashes.OperationalChanged, On, (Instance smi) => smi.Operational.IsOperational).Enter(delegate(Instance smi)
		{
			smi.Operational.SetActive(value: false);
		});
		On.DefaultState(On.opening).EventTransition(GameHashes.OperationalChanged, On.shutdown, (Instance smi) => !smi.Operational.IsOperational).ParamTransition(fullyBlocked, Nosurfacesight, GameStateMachine<MissileLauncher, Instance, IStateMachineTarget, Def>.IsTrue)
			.ScheduleGoTo(shutdownDuration, On.idle)
			.Enter(delegate(Instance smi)
			{
				smi.Operational.SetActive(smi.Operational.IsOperational);
			});
		On.opening.PlayAnim("working_pre").OnAnimQueueComplete(On.searching).Target(cannonTarget)
			.PlayAnim("Cannon_working_pre");
		On.searching.PlayAnim("on", KAnim.PlayMode.Loop).Enter(delegate(Instance smi)
		{
			smi.sm.rotationComplete.Set(value: false, smi);
			smi.sm.meteorTarget.Set(null, smi);
			smi.cannonRotation = smi.def.scanningAngle;
		}).Update("FindMeteor", delegate(Instance smi, float dt)
		{
			smi.Searching(dt);
		}, UpdateRate.SIM_EVERY_TICK)
			.EventTransition(GameHashes.OnStorageChange, NoAmmo, (Instance smi) => smi.MissileStorage.Count <= 0 && smi.LongRangeStorage.Count <= 0)
			.ParamTransition(meteorTarget, Launch.targeting, (Instance smi, GameObject meteor) => meteor != null)
			.ParamTransition(longRangeTarget, Launch.targetingLongRange, (Instance smi, GameObject longrange) => smi.ShouldRotateToLongRange())
			.Exit(delegate(Instance smi)
			{
				smi.sm.rotationComplete.Set(value: false, smi);
			});
		On.idle.Target(masterTarget).PlayAnim("idle", KAnim.PlayMode.Loop).UpdateTransition(On, (Instance smi, float dt) => smi.Operational.IsOperational && smi.MeteorDetected())
			.EventTransition(GameHashes.ClusterDestinationChanged, On.searching, (Instance smi) => smi.LongRangeStorage.Count > 0)
			.Target(cannonTarget)
			.PlayAnim("Cannon_working_pst");
		On.shutdown.Target(masterTarget).PlayAnim("working_pst").OnAnimQueueComplete(Off)
			.Target(cannonTarget)
			.PlayAnim("Cannon_working_pst");
		Launch.PlayAnim("target_detected", KAnim.PlayMode.Loop).Update("Rotate", delegate(Instance smi, float dt)
		{
			smi.RotateToMeteor(dt);
		}, UpdateRate.SIM_EVERY_TICK);
		Launch.targeting.Update("Targeting", delegate(Instance smi, float dt)
		{
			if (smi.sm.meteorTarget.Get(smi).IsNullOrDestroyed())
			{
				smi.GoTo(On.searching);
			}
			else if (smi.cannonAnimController.Rotation < smi.def.maxAngle * -1f || smi.cannonAnimController.Rotation > smi.def.maxAngle)
			{
				GameObject gameObject = smi.sm.meteorTarget.Get(smi);
				gameObject.GetComponent<Comet>().Targeted = false;
				smi.sm.meteorTarget.Set(null, smi);
				smi.GoTo(On.searching);
			}
		}, UpdateRate.SIM_EVERY_TICK).ParamTransition(rotationComplete, Launch.shoot, GameStateMachine<MissileLauncher, Instance, IStateMachineTarget, Def>.IsTrue);
		Launch.targetingLongRange.Update("TargetingLongRange", delegate
		{
		}, UpdateRate.SIM_EVERY_TICK).ParamTransition(rotationComplete, Launch.shoot, GameStateMachine<MissileLauncher, Instance, IStateMachineTarget, Def>.IsTrue);
		Launch.shoot.ScheduleGoTo(shootDelayDuration, Launch.pst).Exit("LaunchMissile", delegate(Instance smi)
		{
			if (smi.sm.meteorTarget.Get(smi) != null)
			{
				smi.LaunchMissile();
			}
			else if (smi.sm.longRangeTarget.Get(smi) != null)
			{
				smi.LaunchLongRangeMissile();
			}
			cannonTarget.Get(smi).GetComponent<KBatchedAnimController>().Play("Cannon_shooting_pre");
		});
		Launch.pst.Target(masterTarget).Enter(delegate(Instance smi)
		{
			smi.SetOreChunk();
			KAnimControllerBase component = smi.GetComponent<KAnimControllerBase>();
			if (smi.GetComponent<Storage>().Count <= 0)
			{
				component.Play("base_shooting_pst_last");
			}
			else
			{
				component.Play("base_shooting_pst");
			}
		}).Target(cannonTarget)
			.PlayAnim("Cannon_shooting_pst")
			.OnAnimQueueComplete(Cooldown);
		Cooldown.Exit(delegate(Instance smi)
		{
			smi.SpawnOre();
		}).Enter(delegate(Instance smi)
		{
			KAnimControllerBase component = smi.GetComponent<KAnimControllerBase>();
			if (smi.GetComponent<Storage>().Count <= 0)
			{
				component.Play("base_ejecting_last");
			}
			else
			{
				component.Play("base_ejecting");
			}
			smi.sm.rotationComplete.Set(value: false, smi);
			smi.sm.meteorTarget.Set(null, smi);
			smi.GoTo(smi.CooldownGoToState);
		});
		Cooldown.basic.Update("Rotate", delegate(Instance smi, float dt)
		{
			smi.RotateToMeteor(dt);
		}, UpdateRate.SIM_EVERY_TICK).OnAnimQueueComplete(On.searching);
		Cooldown.longrange.QueueAnim("cooldown", loop: true).ToggleStatusItem(LongRangeCooldown).Target(cannonTarget)
			.QueueAnim("cooldown_cannon_pre")
			.QueueAnim("cooldown_cannon", loop: true)
			.ScheduleGoTo(longrangeCooldownTime, On.searching)
			.Exit(delegate(Instance smi)
			{
				cannonTarget.Get(smi).GetComponent<KBatchedAnimController>().Play("cooldown_cannon_pst");
			});
		Nosurfacesight.Target(masterTarget).PlayAnim("working_pst").QueueAnim("error")
			.ParamTransition(fullyBlocked, On, GameStateMachine<MissileLauncher, Instance, IStateMachineTarget, Def>.IsFalse)
			.Target(cannonTarget)
			.PlayAnim("Cannon_working_pst")
			.Enter(delegate(Instance smi)
			{
				smi.Operational.SetActive(value: false);
			});
		NoAmmo.PlayAnim("off_open").EventTransition(GameHashes.OnStorageChange, On, (Instance smi) => smi.MissileStorage.Count > 0 || smi.LongRangeStorage.Count > 0).Enter(delegate(Instance smi)
		{
			smi.Operational.SetActive(value: false);
		})
			.Exit(delegate(Instance smi)
			{
				KAnimControllerBase component = smi.GetComponent<KAnimControllerBase>();
				component.Play("off_closing");
			})
			.Target(cannonTarget)
			.PlayAnim("Cannon_working_pst");
	}
}
