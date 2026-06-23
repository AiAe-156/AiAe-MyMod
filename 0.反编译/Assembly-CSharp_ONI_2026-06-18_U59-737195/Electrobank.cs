using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

public class Electrobank : KMonoBehaviour, ISim1000ms, ISim200ms, IConsumableUIItem, IGameObjectEffectDescriptor
{
	private static float capacity = 120000f;

	[Serialize]
	private float charge = capacity;

	private const float MAX_HEALTH = 10f;

	[Serialize]
	private float currentHealth = 10f;

	[Serialize]
	private float timeSincePowerDrawn = 0.5f;

	private const float RADIATION_EMITTER_TIMEOUT = 0.5f;

	public float radioactivityTuning;

	private RadiationEmitter radiationEmitter;

	private float lastDamageTime;

	public ProgressBar healthBar;

	public bool rechargeable;

	public bool keepEmpty;

	[MyCmpGet]
	private Pickupable pickupable;

	public string ID { get; private set; }

	public bool IsFullyCharged => charge == capacity;

	public float Charge => charge;

	public string ConsumableId => this.PrefabID().Name;

	public string ConsumableName => this.GetProperName();

	public int MajorOrder => 500;

	public int MinorOrder => 0;

	public bool Display => true;

	protected override void OnPrefabInit()
	{
		ID = base.gameObject.PrefabID().ToString();
		Subscribe(748399584, OnCraft);
		base.OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Subscribe(856640610, ClearHealthBar);
		Components.Electrobanks.Add(base.gameObject.GetMyWorldId(), this);
		radiationEmitter = GetComponent<RadiationEmitter>();
		UpdateRadiationEmitter();
	}

	private void OnCraft(object data)
	{
		WorldResourceAmountTracker<ElectrobankTracker>.Get().RegisterAmountProduced(Charge);
	}

	private void UpdateRadiationEmitter()
	{
		if (!(radiationEmitter == null))
		{
			bool flag = timeSincePowerDrawn < 0.5f;
			radiationEmitter.emitRads = (flag ? radioactivityTuning : 0f);
			radiationEmitter.Refresh();
		}
	}

	private static GameObject Replace(GameObject electrobank, Tag replacement, bool dropFromStorage = false)
	{
		Vector3 position = electrobank.transform.GetPosition();
		GameObject gameObject = Util.KInstantiate(Assets.GetPrefab(replacement), position);
		gameObject.GetComponent<PrimaryElement>().SetElement(electrobank.GetComponent<PrimaryElement>().Element.id);
		gameObject.SetActive(value: true);
		Storage storage = electrobank.GetComponent<Pickupable>().storage;
		if (storage != null)
		{
			storage.Remove(electrobank);
		}
		electrobank.DeleteObject();
		if (storage != null && !dropFromStorage)
		{
			storage.Store(gameObject);
		}
		return gameObject;
	}

	public static GameObject ReplaceEmptyWithCharged(GameObject EmptyElectrobank, bool dropFromStorage = false)
	{
		return Replace(EmptyElectrobank, "Electrobank", dropFromStorage);
	}

	public static GameObject ReplaceChargedWithEmpty(GameObject ChargedElectrobank, bool dropFromStorage = false)
	{
		return Replace(ChargedElectrobank, "EmptyElectrobank", dropFromStorage);
	}

	public static GameObject ReplaceEmptyWithGarbage(GameObject ChargedElectrobank, bool dropFromStorage = false)
	{
		return Replace(ChargedElectrobank, "GarbageElectrobank", dropFromStorage);
	}

	public float AddPower(float joules)
	{
		if (joules < 0f)
		{
			joules = 0f;
		}
		float num = Mathf.Min(joules, capacity - charge);
		charge += num;
		return num;
	}

	public float RemovePower(float joules, bool dropWhenEmpty)
	{
		float num = Mathf.Min(charge, joules);
		charge -= num;
		if (charge <= 0f)
		{
			OnEmpty(dropWhenEmpty);
		}
		if (num > 0f)
		{
			timeSincePowerDrawn = 0f;
		}
		return num;
	}

	protected virtual void OnEmpty(bool dropWhenEmpty)
	{
		if (rechargeable)
		{
			ReplaceChargedWithEmpty(base.gameObject, dropWhenEmpty);
		}
		else if (!keepEmpty)
		{
			if (pickupable.storage != null)
			{
				pickupable.storage.Remove(base.gameObject);
			}
			Util.KDestroyGameObject(base.gameObject);
		}
	}

	public void FullyCharge()
	{
		charge = capacity;
	}

	public virtual void Explode()
	{
		int num = Grid.PosToCell(base.gameObject.transform.position);
		float num2 = Grid.Temperature[num];
		num2 += charge / (Grid.Mass[num] * Grid.Element[num].specificHeatCapacity);
		num2 = Mathf.Clamp(num2, 1f, 9999f);
		SimMessages.ReplaceElement(num, Grid.Element[num].id, CellEventLogger.Instance.SandBoxTool, Grid.Mass[num], num2, Grid.DiseaseIdx[num], Grid.DiseaseCount[num]);
		Game.Instance.SpawnFX(SpawnFXHashes.MeteorImpactMetal, base.gameObject.transform.position, 0f);
		KFMOD.PlayOneShot(GlobalAssets.GetSound("Battery_explode"), base.gameObject.transform.position);
		if (rechargeable)
		{
			ReplaceEmptyWithGarbage(base.gameObject);
		}
		else
		{
			base.gameObject.DeleteObject();
		}
	}

	protected void LaunchNearbyStuff()
	{
		ListPool<ScenePartitionerEntry, Comet>.PooledList pooledList = ListPool<ScenePartitionerEntry, Comet>.Allocate();
		Vector3 position = base.transform.position;
		GameScenePartitioner.Instance.GatherEntries((int)position.x - 3, (int)position.y - 3, 6, 6, GameScenePartitioner.Instance.pickupablesLayer, pooledList);
		foreach (ScenePartitionerEntry item in pooledList)
		{
			GameObject gameObject = (item.obj as Pickupable).gameObject;
			if (!(gameObject.GetComponent<MinionIdentity>() != null) && !(gameObject.GetComponent<CreatureBrain>() != null) && gameObject.GetDef<RobotAi.Def>() == null)
			{
				Vector2 normalized = ((Vector2)(gameObject.transform.GetPosition() - position)).normalized;
				normalized *= (float)Random.Range(4, 6);
				normalized.y += Random.Range(2, 4);
				if (GameComps.Fallers.Has(gameObject))
				{
					GameComps.Fallers.Remove(gameObject);
				}
				if (GameComps.Gravities.Has(gameObject))
				{
					GameComps.Gravities.Remove(gameObject);
				}
				GameComps.Fallers.Add(gameObject, normalized);
			}
		}
		pooledList.Recycle();
	}

	public void Sim1000ms(float dt)
	{
		if (!pickupable.KPrefabID.HasTag(GameTags.Stored))
		{
			EvaluateWaterDamage(dt);
			UpdateHealthBar();
		}
	}

	public virtual void Sim200ms(float dt)
	{
		UpdateRadiationEmitter();
		timeSincePowerDrawn = Mathf.Min(timeSincePowerDrawn + dt, 10f);
	}

	private void EvaluateWaterDamage(float dt)
	{
		if (Grid.IsValidCell(pickupable.cachedCell) && Grid.Element[pickupable.cachedCell].HasTag(GameTags.AnyWater) && Random.Range(1, 101) > 75)
		{
			PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, UI.GAMEOBJECTEFFECTS.DAMAGE_POPS.POWER_BANK_WATER_DAMAGE, base.transform);
			Damage(Random.Range(0f, dt));
		}
	}

	public void Damage(float amount)
	{
		Game.Instance.SpawnFX(SpawnFXHashes.ElectrobankDamage, Grid.PosToCell(base.gameObject), 0f);
		KFMOD.PlayOneShot(GlobalAssets.GetSound("Battery_sparks_short"), base.gameObject.transform.position);
		currentHealth -= amount;
		if (healthBar == null)
		{
			CreateHealthBar();
		}
		healthBar.Update();
		lastDamageTime = Time.time;
		if (currentHealth <= 0f)
		{
			Explode();
		}
	}

	protected override void OnCleanUp()
	{
		ClearHealthBar();
		Components.Electrobanks.Remove(base.gameObject.GetMyWorldId(), this);
		base.OnCleanUp();
	}

	public void CreateHealthBar()
	{
		healthBar = ProgressBar.CreateProgressBar(base.gameObject, () => currentHealth / 10f);
		healthBar.SetVisibility(visible: true);
		healthBar.barColor = Util.ColorFromHex("CC3333");
	}

	public void UpdateHealthBar()
	{
		if (healthBar != null && Time.time - lastDamageTime > 5f)
		{
			ClearHealthBar();
		}
	}

	public void ClearHealthBar(object _ = null)
	{
		if (healthBar != null)
		{
			Util.KDestroyGameObject(healthBar);
			healthBar = null;
		}
	}

	public List<Descriptor> GetDescriptors(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor>();
		Descriptor item = default(Descriptor);
		item.SetupDescriptor(string.Format(UI.BUILDINGEFFECTS.ELECTROBANKS, GameUtil.GetFormattedJoules(Charge)), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.ELECTROBANKS, GameUtil.GetFormattedJoules(Charge)));
		list.Add(item);
		return list;
	}
}
