using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class MissileLongRangeProjectile : GameStateMachine<MissileLongRangeProjectile, MissileLongRangeProjectile.StatesInstance, IStateMachineTarget, MissileLongRangeProjectile.Def>
{
	public class Def : BaseDef
	{
		public string starmapOverrideSymbol = "payload";

		public string missileName = "STRINGS.ITEMS.MISSILE_LONGRANGE.NAME";

		public string missileDesc = "STRINGS.ITEMS.MISSILE_LONGRANGE.DESC";
	}

	public class StatesInstance : GameInstance
	{
		public KBatchedAnimController animController;

		[Serialize]
		private float launchSpeed;

		public GameObject smokeTrailFX;

		private WorldContainer myWorld;

		[Serialize]
		private AxialI myLocation;

		[Serialize]
		private int myWorldId = -1;

		[Serialize]
		private Ref<KPrefabID> launchedTarget = new Ref<KPrefabID>();

		private Vector3 Position => base.transform.position + animController.Offset;

		public StatesInstance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			animController = GetComponent<KBatchedAnimController>();
		}

		public override void StartSM()
		{
			base.StartSM();
			if (launchedTarget.Get() != null)
			{
				base.sm.asteroidTarget.Set(launchedTarget.Get().gameObject, this);
				myWorld = ClusterManager.Instance.GetWorld(myWorldId);
			}
		}

		public void StartTakeoff()
		{
			if (GameComps.Fallers.Has(base.gameObject))
			{
				GameComps.Fallers.Remove(base.gameObject);
			}
			Pickupable component = GetComponent<Pickupable>();
			component.handleFallerComponents = false;
		}

		public void UpdateLaunch(float dt)
		{
			float rotation = MathUtil.AngleSigned(Vector3.up, Vector3.up, Vector3.forward);
			animController.Rotation = rotation;
			int cell = Grid.PosToCell(Position);
			Vector2I vector2I = Grid.CellToXY(cell);
			if (!Grid.IsValidCell(cell))
			{
				base.smi.sm.triggeroutofworld.Set(value: true, base.smi);
				return;
			}
			if (Grid.IsValidCellInWorld(Grid.PosToCell(Position), myWorldId) && (float)vector2I.y < myWorld.maximumBounds.y)
			{
				base.transform.SetPosition(base.transform.position + Vector3.up * (launchSpeed * dt));
			}
			else
			{
				animController.Offset += Vector3.up * (launchSpeed * dt);
			}
			ParticleSystem[] componentsInChildren = base.smi.smokeTrailFX.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren)
			{
				particleSystem.gameObject.transform.SetPositionAndRotation(Position, Quaternion.identity);
			}
		}

		public void PrepareLaunch(GameObject asteroid_target, float speed, Vector3 launchPos, float launchAngle)
		{
			base.gameObject.transform.SetParent(null);
			base.gameObject.layer = LayerMask.NameToLayer("Default");
			launchPos.z = Grid.GetLayerZ(Grid.SceneLayer.BuildingBack);
			base.gameObject.transform.SetLocalPosition(launchPos);
			animController.Rotation = launchAngle;
			animController.Offset = Vector3.back;
			animController.SetVisiblity(is_visible: true);
			base.gameObject.GetSMI<FetchableMonitor.Instance>()?.SetForceUnfetchable(is_unfetchable: true);
			base.sm.triggeroutofworld.Set(value: false, base.smi);
			base.sm.asteroidTarget.Set(asteroid_target, base.smi);
			launchedTarget = new Ref<KPrefabID>(asteroid_target.GetComponent<KPrefabID>());
			launchSpeed = speed;
			myWorld = base.gameObject.GetMyWorld();
			myWorldId = myWorld.id;
			ClusterGridEntity component = myWorld.GetComponent<ClusterGridEntity>();
			if (component != null)
			{
				myLocation = component.Location;
			}
		}

		public void ExitWorldEnterStarmap()
		{
			GameObject gameObject = base.sm.asteroidTarget.Get(base.smi);
			if (gameObject != null)
			{
				ClusterGridEntity component = gameObject.GetComponent<ClusterGridEntity>();
				if (component != null)
				{
					GameObject gameObject2 = GameUtil.KInstantiate(Assets.GetPrefab("ClusterMapLongRangeMissile"), Grid.SceneLayer.NoLayer);
					gameObject2.SetActive(value: true);
					ClusterMapLongRangeMissile.StatesInstance sMI = gameObject2.GetSMI<ClusterMapLongRangeMissile.StatesInstance>();
					sMI.Setup(myLocation, component, base.def);
				}
				else
				{
					gameObject.Trigger(-2056344675, (object)MissileLongRangeConfig.DamageEventPayload.sharedInstance);
				}
			}
			Util.KDestroyGameObject(base.gameObject);
		}
	}

	public State launch;

	public State leaveworld;

	public BoolParameter triggeroutofworld = new BoolParameter(default_value: false);

	public ObjectParameter<GameObject> asteroidTarget = new ObjectParameter<GameObject>();

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = root;
		root.ParamTransition(asteroidTarget, launch, (StatesInstance smi, GameObject target) => !target.IsNullOrDestroyed());
		launch.Update("Launch", delegate(StatesInstance smi, float dt)
		{
			smi.UpdateLaunch(dt);
		}, UpdateRate.SIM_EVERY_TICK).ParamTransition(triggeroutofworld, leaveworld, GameStateMachine<MissileLongRangeProjectile, StatesInstance, IStateMachineTarget, Def>.IsTrue).Enter(delegate(StatesInstance smi)
		{
			Vector3 position = smi.master.transform.GetPosition();
			position.z = Grid.GetLayerZ(Grid.SceneLayer.BuildingBack);
			smi.smokeTrailFX = Util.KInstantiate(EffectPrefabs.Instance.LongRangeMissileSmokeTrailFX, position);
			smi.smokeTrailFX.transform.SetParent(smi.master.transform);
			smi.smokeTrailFX.SetActive(value: true);
			smi.StartTakeoff();
			KFMOD.PlayOneShot(GlobalAssets.GetSound("MissileLauncher_Missile_ignite"), CameraController.Instance.GetVerticallyScaledPosition(position));
		});
		leaveworld.Enter(delegate(StatesInstance smi)
		{
			smi.ExitWorldEnterStarmap();
		});
	}
}
