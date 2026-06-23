using System;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class MissileProjectile : GameStateMachine<MissileProjectile, MissileProjectile.StatesInstance, IStateMachineTarget, MissileProjectile.Def>
{
	public class Def : BaseDef
	{
		public float MeteorDebrisMassModifier = 0.25f;

		public float ExplosionRange = 2f;

		public float debrisSpeed = 6f;

		public float debrisMaxAngle = 40f;

		public string explosionEffectAnim = "missile_explosion_kanim";
	}

	public class StatesInstance : GameInstance
	{
		public KBatchedAnimController animController;

		private float launchSpeed;

		public GameObject smokeTrailFX;

		private Vector3 Position => base.transform.position + animController.Offset;

		public StatesInstance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			animController = GetComponent<KBatchedAnimController>();
		}

		public void StartTakeoff()
		{
			if (GameComps.Fallers.Has(base.gameObject))
			{
				GameComps.Fallers.Remove(base.gameObject);
			}
		}

		public void UpdateLaunch(float dt)
		{
			int myWorldId = base.gameObject.GetMyWorldId();
			Comet comet = base.sm.meteorTarget.Get(base.smi);
			if (!comet.IsNullOrDestroyed())
			{
				Vector3 targetPosition = comet.TargetPosition;
				base.sm.triggerexplode.Set(InExplosionRange(targetPosition, Position), base.smi);
				Vector3 v = Vector3.Normalize(targetPosition - Position);
				Vector3 normalized = (targetPosition - Position).normalized;
				float rotation = MathUtil.AngleSigned(Vector3.up, v, Vector3.forward);
				animController.Rotation = rotation;
				if (Grid.IsValidCellInWorld(Grid.PosToCell(Position), myWorldId))
				{
					base.transform.SetPosition(base.transform.position + normalized * (launchSpeed * dt));
				}
				else
				{
					animController.Offset += normalized * (launchSpeed * dt);
				}
				ParticleSystem[] componentsInChildren = base.smi.smokeTrailFX.GetComponentsInChildren<ParticleSystem>();
				foreach (ParticleSystem particleSystem in componentsInChildren)
				{
					particleSystem.gameObject.transform.SetPositionAndRotation(Position, Quaternion.identity);
				}
			}
			else if (!base.sm.triggerexplode.Get(base.smi))
			{
				if (!base.smi.smokeTrailFX.IsNullOrDestroyed())
				{
					Util.KDestroyGameObject(base.smi.smokeTrailFX);
				}
				if (!GameComps.Fallers.Has(base.gameObject))
				{
					GameComps.Fallers.Add(base.gameObject, Vector2.down);
				}
				base.gameObject.GetComponent<KSelectable>().enabled = true;
				base.smi.GoTo("root");
			}
		}

		public void PrepareLaunch(Comet meteor_target, float speed, Vector3 launchPos, float launchAngle)
		{
			base.gameObject.transform.SetParent(null);
			base.gameObject.layer = LayerMask.NameToLayer("Default");
			launchPos.z = Grid.GetLayerZ(Grid.SceneLayer.BuildingBack);
			base.gameObject.transform.SetLocalPosition(launchPos);
			animController.Rotation = launchAngle;
			animController.Offset = Vector3.back;
			animController.SetVisiblity(is_visible: true);
			base.sm.triggerexplode.Set(value: false, base.smi);
			base.sm.meteorTarget.Set(meteor_target, base.smi);
			launchSpeed = speed;
		}

		public void TriggerExplosion()
		{
			if (!base.smi.sm.meteorTarget.IsNullOrDestroyed())
			{
				SpawnMeteorResources(base.smi.sm.meteorTarget.Get(base.smi));
				Util.KDestroyGameObject(base.smi.sm.meteorTarget.Get(base.smi));
			}
			Explode();
		}

		private void SpawnMeteorResources(Comet meteor)
		{
			PrimaryElement meteorPE = meteor.GetComponent<PrimaryElement>();
			Element element = meteorPE.Element;
			int num = meteor.GetMyWorldId();
			if (num == 255 || num == -1)
			{
				WorldContainer worldFromPosition = ClusterManager.Instance.GetWorldFromPosition(meteor.transform.GetPosition() - Vector3.down * Grid.CellSizeInMeters);
				num = ((worldFromPosition == null) ? num : worldFromPosition.id);
			}
			bool flag = Grid.IsValidCellInWorld(Grid.PosToCell(meteor.TargetPosition), num);
			float num2 = meteor.ExplosionMass * base.def.MeteorDebrisMassModifier;
			float num3 = meteor.AddTileMass * base.def.MeteorDebrisMassModifier;
			int num_nonTiles_ores = meteor.GetRandomNumOres();
			float arg = ((num_nonTiles_ores > 0) ? (num2 / (float)num_nonTiles_ores) : 1f);
			float temperature = meteor.GetRandomTemperatureForOres();
			int num_tile_ores = meteor.addTiles;
			float arg2 = ((num_tile_ores > 0) ? (num3 / (float)num_tile_ores) : 1f);
			Vector3 normalized = (meteor.TargetPosition - Position).normalized;
			Vector2 vector = new Vector2(normalized.x, normalized.y);
			Vector2 vector2 = new Vector2(vector.y, 0f - vector.x);
			Func<int, int, float, Vector3> func = delegate(int objectIndex, int objectCount, float maxAngleAllowed)
			{
				int num9 = ((objectCount % 2 == 0) ? objectCount : (objectCount - 1));
				float num10 = maxAngleAllowed * 2f / (float)num9;
				bool flag2 = objectIndex % 2 == 0;
				float num11 = num10 * (float)Mathf.CeilToInt((float)objectIndex / 2f) * (MathF.PI / 180f) * (float)(flag2 ? 1 : (-1));
				return new Vector3(Mathf.Cos(4.712389f + num11), Mathf.Sin(4.712389f + num11), 0f).normalized * base.def.debrisSpeed;
			};
			Action<Substance, float, Vector3> action = delegate(Substance substance2, float mass, Vector3 velocity)
			{
				Vector3 position2 = velocity.normalized * 0.75f;
				position2 += new Vector3(0f, 0.55f, 0f);
				position2 += Position;
				GameObject go = substance2.SpawnResource(position2, mass, temperature, meteorPE.DiseaseIdx, meteorPE.DiseaseCount / (num_nonTiles_ores + num_tile_ores));
				if (GameComps.Fallers.Has(go))
				{
					GameComps.Fallers.Remove(go);
				}
				GameComps.Fallers.Add(go, velocity);
			};
			Action<string, Vector3> action2 = delegate(string prefabName, Vector3 velocity)
			{
				Vector3 vector4 = velocity.normalized * 0.75f;
				vector4 += new Vector3(0f, 0.55f, 0f);
				vector4 += Position;
				GameObject gameObject2 = Scenario.SpawnPrefab(Grid.PosToCell(vector4), 0, 0, prefabName);
				gameObject2.SetActive(value: true);
				vector4.z = gameObject2.transform.position.z;
				gameObject2.transform.position = vector4;
				if (GameComps.Fallers.Has(gameObject2))
				{
					GameComps.Fallers.Remove(gameObject2);
				}
				GameComps.Fallers.Add(gameObject2, velocity);
			};
			Substance substance = element.substance;
			if (flag)
			{
				int arg3 = num_nonTiles_ores + num_tile_ores + ((meteor.lootOnDestroyedByMissile != null) ? meteor.lootOnDestroyedByMissile.Length : 0);
				for (int num4 = 0; num4 < num_nonTiles_ores; num4++)
				{
					Vector3 arg4 = func(num4, arg3, base.def.debrisMaxAngle);
					action(substance, arg, arg4);
				}
				for (int num5 = 0; num5 < num_tile_ores; num5++)
				{
					Vector3 arg5 = func(num_nonTiles_ores + num5, arg3, base.def.debrisMaxAngle);
					action(substance, arg2, arg5);
				}
				if (meteor.lootOnDestroyedByMissile != null)
				{
					for (int num6 = 0; num6 < meteor.lootOnDestroyedByMissile.Length; num6++)
					{
						Vector3 arg6 = func(num_nonTiles_ores + num_tile_ores + num6, arg3, base.def.debrisMaxAngle);
						string arg7 = meteor.lootOnDestroyedByMissile[num6];
						action2(arg7, arg6);
					}
				}
			}
			else
			{
				if (num == -1 || num == 255)
				{
					return;
				}
				int num7 = Grid.PosToCell(meteor.TargetPosition);
				Vector3 position = meteor.TargetPosition;
				Vector2 vector3 = meteor.GetMyWorld().WorldOffset;
				while (!Grid.IsValidCellInWorld(num7, num) && position.y > vector3.y)
				{
					num7 = Grid.CellBelow(num7);
					position = Grid.CellToPos(num7);
				}
				if (!(position.y > vector3.y))
				{
					return;
				}
				substance.SpawnResource(position, num2 + num3, temperature, meteorPE.DiseaseIdx, meteorPE.DiseaseCount);
				if (meteor.lootOnDestroyedByMissile != null)
				{
					for (int num8 = 0; num8 < meteor.lootOnDestroyedByMissile.Length; num8++)
					{
						string name = meteor.lootOnDestroyedByMissile[num8];
						GameObject gameObject = Scenario.SpawnPrefab(num7, 0, 0, name);
						gameObject.SetActive(value: true);
					}
				}
			}
		}

		private void Explode()
		{
			if (GameComps.Fallers.Has(base.gameObject))
			{
				GameComps.Fallers.Remove(base.gameObject);
			}
			Vector3 position = base.gameObject.transform.position;
			position.z = Grid.GetLayerZ(Grid.SceneLayer.FXFront2);
			SpawnExplosionFX(base.def.explosionEffectAnim, position, animController.Offset);
			animController.SetSymbolVisiblity("missile_body", is_visible: false);
			animController.SetSymbolVisiblity("missile_head", is_visible: false);
		}

		private bool InExplosionRange(Vector3 target_pos, Vector3 current_pos)
		{
			float num = Vector2.Distance((Vector2)target_pos, (Vector2)current_pos);
			return num <= base.def.ExplosionRange;
		}

		private void SpawnExplosionFX(string anim, Vector3 pos, Vector3 offset)
		{
			KBatchedAnimController kBatchedAnimController = FXHelpers.CreateEffect(anim, pos, base.gameObject.transform, update_looping_sounds_position: false, Grid.SceneLayer.FXFront2);
			kBatchedAnimController.Offset = offset;
			kBatchedAnimController.Play("idle");
			kBatchedAnimController.onAnimComplete += delegate
			{
				Util.KDestroyGameObject(base.gameObject);
			};
		}
	}

	public State launch;

	public State explode;

	public BoolParameter triggerexplode = new BoolParameter(default_value: false);

	public ObjectParameter<Comet> meteorTarget = new ObjectParameter<Comet>();

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = root;
		root.ParamTransition(meteorTarget, launch, (StatesInstance smi, Comet comet) => comet != null);
		launch.Update("Launch", delegate(StatesInstance smi, float dt)
		{
			smi.UpdateLaunch(dt);
		}, UpdateRate.SIM_EVERY_TICK).ParamTransition(triggerexplode, explode, GameStateMachine<MissileProjectile, StatesInstance, IStateMachineTarget, Def>.IsTrue).Enter(delegate(StatesInstance smi)
		{
			Vector3 position = smi.master.transform.GetPosition();
			position.z = Grid.GetLayerZ(Grid.SceneLayer.BuildingBack);
			smi.smokeTrailFX = Util.KInstantiate(EffectPrefabs.Instance.MissileSmokeTrailFX, position);
			smi.smokeTrailFX.transform.SetParent(smi.master.transform);
			smi.smokeTrailFX.SetActive(value: true);
			smi.StartTakeoff();
			KFMOD.PlayOneShot(GlobalAssets.GetSound("MissileLauncher_Missile_ignite"), CameraController.Instance.GetVerticallyScaledPosition(position));
		});
		explode.Enter(delegate(StatesInstance smi)
		{
			smi.TriggerExplosion();
			ParticleSystem[] componentsInChildren = smi.smokeTrailFX.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren)
			{
				ParticleSystem.EmissionModule emission = particleSystem.emission;
				emission.enabled = false;
			}
		});
	}
}
