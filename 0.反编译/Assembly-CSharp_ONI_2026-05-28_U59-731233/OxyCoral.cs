using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class OxyCoral : GameStateMachine<OxyCoral, OxyCoral.Instance, IStateMachineTarget, OxyCoral.Def>
{
	public class Def : BaseDef, IGameObjectEffectDescriptor
	{
		public float OxygenProductionRate;

		public float MinLuxRequired;

		public CellOffset[] OutputBubbleCells;

		public List<Descriptor> GetDescriptors(GameObject go)
		{
			return new List<Descriptor>
			{
				new Descriptor(string.Format(UI.BUILDINGEFFECTS.ELEMENTEMITTED_ENTITYTEMP, ElementLoader.FindElementByHash(SimHashes.Oxygen).name, GameUtil.GetFormattedMass(OxygenProductionRate, GameUtil.TimeSlice.PerSecond)), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.ELEMENTEMITTED_ENTITYTEMP, ElementLoader.FindElementByHash(SimHashes.Oxygen).name, GameUtil.GetFormattedMass(OxygenProductionRate, GameUtil.TimeSlice.PerSecond)))
			};
		}
	}

	public class NoProducing : State
	{
		public State noLight;

		public State wilted;

		public State dead;
	}

	public new class Instance : GameInstance, IPoopStation
	{
		private PrimaryElement primaryElement;

		private WiltCondition wiltCondition;

		private ReceptacleMonitor receptacleMonitor;

		private GameObject poopUser;

		public bool IsWild => !receptacleMonitor.Replanted;

		public bool IsWilted => wiltCondition == null || wiltCondition.IsWilting();

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			receptacleMonitor = GetComponent<ReceptacleMonitor>();
			primaryElement = GetComponent<PrimaryElement>();
			wiltCondition = GetComponent<WiltCondition>();
		}

		public override void StartSM()
		{
			RegisterPoopStation();
			base.StartSM();
		}

		public bool IsThereEnoughLight()
		{
			int num = 0;
			int cell = Grid.PosToCell(this);
			for (int i = 0; i < base.def.OutputBubbleCells.Length; i++)
			{
				int i2 = Grid.OffsetCell(cell, base.def.OutputBubbleCells[i]);
				float num2 = Grid.LightIntensity[i2];
				num = ((num2 > (float)num) ? ((int)num2) : num);
			}
			return (float)num >= base.def.MinLuxRequired;
		}

		public void ProduceOxygenUpdate(float dt)
		{
			int cell = Grid.PosToCell(this);
			int num = Random.Range(0, base.def.OutputBubbleCells.Length);
			int gameCell = Grid.OffsetCell(cell, base.def.OutputBubbleCells[num]);
			float num2 = base.def.OxygenProductionRate;
			if (IsWild)
			{
				num2 *= 0.25f;
			}
			float num3 = num2 * dt;
			if (num3 >= 1E-09f)
			{
				CreateOxygenBubble(gameCell, num3);
			}
		}

		private void CreateOxygenBubble(int gameCell, float mass)
		{
			Vector3 vector = Grid.CellToPosCCC(gameCell, Grid.SceneLayer.BuildingFront);
			BubbleManager.instance.SpawnBubble(SimHashes.Oxygen, vector, mass, primaryElement.Temperature, BubbleManager.Disease.None);
		}

		protected override void OnCleanUp()
		{
			UnregisterPoopStation();
			base.OnCleanUp();
		}

		public bool IsUserCompatibleWithPoopStation(KPrefabID userPrefabID)
		{
			return userPrefabID.HasTag("ParrotFish");
		}

		public GameObject GetPoopStationObject()
		{
			return base.gameObject;
		}

		public GameObject GetCurrentPoopStationUser()
		{
			return poopUser;
		}

		public bool IsPoopStationOperational()
		{
			return !base.smi.IsInsideState(base.smi.sm.noProducing.wilted) && !base.smi.IsInsideState(base.smi.sm.noProducing.dead);
		}

		public string[] GetPoopingAnimNames()
		{
			return null;
		}

		public void RegisterPoopStation()
		{
			Components.PoopStations.Add(base.gameObject.GetMyWorldId(), this);
		}

		public void UnregisterPoopStation()
		{
			Components.PoopStations.Remove(base.gameObject.GetMyWorldId(), this);
		}

		public PoopData GetPoopData()
		{
			return IsWild ? new PoopData(skipSpawningPoop: true, null, CREATURES.POOP.PLANT_POOP_STATION_WILD, global::Def.GetUISprite(base.gameObject).first) : new PoopData(skipSpawningPoop: false, receptacleMonitor.smi.ReceptacleObject.GetComponent<Storage>(), CREATURES.POOP.PLANT_POOP_STATION_WILD, global::Def.GetUISprite(base.gameObject).first);
		}

		public void PlayPoopStationAnim(string animName, KAnim.PlayMode playMode)
		{
		}

		public void ClearPoopStationUser(GameObject userRequestingClearing)
		{
			if (poopUser == userRequestingClearing)
			{
				poopUser = null;
				Trigger(-984476291);
			}
		}

		public bool AttemptToReservePoopStation(GameObject userRequestingReserve)
		{
			if (poopUser != null && poopUser != userRequestingReserve)
			{
				return false;
			}
			poopUser = userRequestingReserve;
			return true;
		}

		public void DestroySelf(object o)
		{
			CreatureHelpers.DeselectCreature(base.gameObject);
			Util.KDestroyGameObject(base.gameObject);
		}
	}

	private const float WILD_PLANTED_RATE_MODIFIER = 0.25f;

	private const string ANIM_NAME_IDLE = "idle_loop";

	private const string ANIM_NAME_PRODUCING_OXYGEN = "oxygen_idle_loop";

	private const string ANIM_NAME_WILTED = "wilt";

	public NoProducing noProducing;

	public State producing;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = noProducing;
		noProducing.DefaultState(noProducing.noLight);
		noProducing.noLight.EventTransition(GameHashes.Uprooted, noProducing.dead).EventTransition(GameHashes.Wilt, noProducing.wilted, IsWilted).PlayAnim("idle_loop", KAnim.PlayMode.Loop)
			.UpdateTransition(producing, IsThereEnoughtLight);
		noProducing.wilted.TriggerOnEnter(GameHashes.PoopStationUpdate).TriggerOnExit(GameHashes.PoopStationUpdate).EventTransition(GameHashes.Uprooted, noProducing.dead)
			.EventTransition(GameHashes.WiltRecover, noProducing.noLight, GameStateMachine<OxyCoral, Instance, IStateMachineTarget, Def>.Not(IsWilted))
			.PlayAnim("wilt");
		noProducing.dead.Enter(delegate(Instance smi)
		{
			GameUtil.KInstantiate(Assets.GetPrefab(EffectConfigs.PlantDeathId), smi.transform.GetPosition(), Grid.SceneLayer.FXFront).SetActive(value: true);
			smi.Trigger(1623392196);
			smi.DestroySelf(null);
		});
		producing.ToggleStatusItem(Db.Get().CreatureStatusItems.BubbleGasProduction, (Instance smi) => new Tuple<SimHashes, float>(SimHashes.Oxygen, smi.IsWild ? (smi.def.OxygenProductionRate * 0.25f) : smi.def.OxygenProductionRate)).EventTransition(GameHashes.Uprooted, noProducing.dead).EventTransition(GameHashes.Wilt, noProducing.wilted)
			.UpdateTransition(noProducing.noLight, LightLostUpdate)
			.PlayAnim("oxygen_idle_loop", KAnim.PlayMode.Loop)
			.Update(ProduceOxygenUpdate, UpdateRate.SIM_1000ms);
	}

	private static bool IsWilted(Instance smi)
	{
		return smi.IsWilted;
	}

	private static bool IsThereEnoughtLight(Instance smi, float dt)
	{
		return smi.IsThereEnoughLight();
	}

	private static bool LightLostUpdate(Instance smi, float dt)
	{
		return !smi.IsThereEnoughLight();
	}

	private static void ProduceOxygenUpdate(Instance smi, float dt)
	{
		smi.ProduceOxygenUpdate(dt);
	}
}
