using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/CO2Manager")]
public class CO2Manager : KMonoBehaviour, ISim33ms
{
	private const float CO2Lifetime = 3f;

	[SerializeField]
	private Vector3 acceleration;

	[SerializeField]
	private CO2 prefab;

	[SerializeField]
	private GameObject breathPrefab;

	[SerializeField]
	private GameObject exhaustPrefab;

	[SerializeField]
	private Color tintColour;

	private List<CO2> co2Items = new List<CO2>();

	private GameObjectPool breathPool;

	private GameObjectPool exhaustPool;

	private GameObjectPool co2Pool;

	public static CO2Manager instance;

	public static void DestroyInstance()
	{
		instance = null;
	}

	protected override void OnPrefabInit()
	{
		instance = this;
		prefab.gameObject.SetActive(value: false);
		breathPrefab.SetActive(value: false);
		exhaustPrefab.SetActive(value: false);
		co2Pool = new GameObjectPool(InstantiateCO2, Deactivate, 16);
		breathPool = new GameObjectPool(InstantiateBreath, Deactivate, 16);
		exhaustPool = new GameObjectPool(InstantiateExhaust, Deactivate, 16);
	}

	private GameObject InstantiateCO2()
	{
		GameObject gameObject = GameUtil.KInstantiate(prefab, Grid.SceneLayer.Front);
		gameObject.SetActive(value: false);
		return gameObject;
	}

	private static void Deactivate(GameObject _)
	{
	}

	private GameObject InstantiateBreath()
	{
		GameObject gameObject = GameUtil.KInstantiate(breathPrefab, Grid.SceneLayer.Front);
		gameObject.SetActive(value: false);
		return gameObject;
	}

	private GameObject InstantiateExhaust()
	{
		GameObject gameObject = GameUtil.KInstantiate(exhaustPrefab, Grid.SceneLayer.Front);
		gameObject.SetActive(value: false);
		return gameObject;
	}

	public void Sim33ms(float dt)
	{
		Vector2I xy = default(Vector2I);
		Vector2I xy2 = default(Vector2I);
		Vector3 vector = acceleration * dt;
		int num = co2Items.Count;
		for (int i = 0; i < num; i++)
		{
			CO2 cO = co2Items[i];
			cO.velocity += vector;
			cO.lifetimeRemaining -= dt;
			Grid.PosToXY(cO.transform.GetPosition(), out xy);
			cO.transform.SetPosition(cO.transform.GetPosition() + cO.velocity * dt);
			Grid.PosToXY(cO.transform.GetPosition(), out xy2);
			int num2 = Grid.XYToCell(xy.x, xy.y);
			int num3 = num2;
			for (int num4 = xy.y; num4 >= xy2.y; num4--)
			{
				int num5 = Grid.XYToCell(xy.x, num4);
				bool flag = !Grid.IsValidCell(num5) || cO.lifetimeRemaining <= 0f;
				if (!flag)
				{
					Element element = Grid.Element[num5];
					flag = element.IsLiquid || element.IsSolid || (Grid.Properties[num5] & 1) != 0;
				}
				if (flag)
				{
					int gameCell = num5;
					bool flag2 = false;
					if (num3 != num5)
					{
						gameCell = num3;
						flag2 = true;
					}
					else
					{
						bool flag3 = false;
						int num6 = -1;
						int num7 = -1;
						CellOffset[] dEFAULT_BREATHABLE_OFFSETS = GasBreatherFromWorldProvider.DEFAULT_BREATHABLE_OFFSETS;
						foreach (CellOffset offset in dEFAULT_BREATHABLE_OFFSETS)
						{
							int num8 = Grid.OffsetCell(num5, offset);
							if (Grid.IsValidCell(num8))
							{
								Element element2 = Grid.Element[num8];
								if (element2.id == SimHashes.CarbonDioxide || element2.HasTag(GameTags.Breathable))
								{
									num6 = num8;
									flag3 = true;
									flag2 = true;
									break;
								}
								if (element2.IsGas)
								{
									num7 = num8;
									flag2 = true;
								}
							}
						}
						if (flag2)
						{
							gameCell = ((!flag3) ? num7 : num6);
						}
					}
					if (flag2)
					{
						cO.TriggerDestroy();
						SimMessages.ModifyMass(gameCell, cO.mass, byte.MaxValue, 0, CellEventLogger.Instance.CO2ManagerFixedUpdate, cO.temperature, SimHashes.CarbonDioxide);
						num--;
						co2Items[i] = co2Items[num];
						co2Items.RemoveAt(num);
						break;
					}
				}
				num3 = num5;
			}
		}
	}

	public CO2 SpawnCO2(Vector3 position, float mass, float temperature, bool flip)
	{
		return SpawnCO2(position, mass, temperature, flip, 0f);
	}

	public CO2 SpawnCO2(Vector3 position, float mass, float temperature, bool flip, float rotation)
	{
		position.z = Grid.GetLayerZ(Grid.SceneLayer.Front);
		GameObject gameObject = co2Pool.GetInstance();
		gameObject.transform.SetPosition(position);
		gameObject.SetActive(value: true);
		CO2 component = gameObject.GetComponent<CO2>();
		component.mass = mass;
		component.temperature = temperature;
		component.velocity = Vector3.zero;
		component.lifetimeRemaining = 3f;
		KBatchedAnimController component2 = component.GetComponent<KBatchedAnimController>();
		component2.TintColour = tintColour;
		component2.onDestroySelf = OnDestroyCO2;
		component2.FlipX = flip;
		component.StartLoop();
		co2Items.Add(component);
		return component;
	}

	public void SpawnBreath(Vector3 position, float mass, float temperature, bool flip)
	{
		if (Grid.IsVisiblyInLiquid(position))
		{
			BubbleManager.instance.SpawnBubble(SimHashes.CarbonDioxide, position, mass, temperature, BubbleManager.Disease.None);
			return;
		}
		position.z = Grid.GetLayerZ(Grid.SceneLayer.Front);
		SpawnCO2(position, mass, temperature, flip);
		GameObject gameObject = breathPool.GetInstance();
		gameObject.transform.SetPosition(position);
		gameObject.SetActive(value: true);
		KBatchedAnimController component = gameObject.GetComponent<KBatchedAnimController>();
		component.TintColour = tintColour;
		component.onDestroySelf = OnDestroyBreath;
		component.FlipX = flip;
		component.Play("breath");
	}

	public void SpawnExhaust(Vector3 position, Vector3 velocity, int co2Cell, float mass, float temperature)
	{
		position.z = Grid.GetLayerZ(Grid.SceneLayer.Front);
		float rotation = Mathf.Repeat(Vector3.Angle(Vector3.down, velocity) * Mathf.Sign(velocity.x), 360f);
		SimMessages.ModifyMass(co2Cell, mass, byte.MaxValue, 0, CellEventLogger.Instance.CO2ManagerFixedUpdate, temperature, SimHashes.CarbonDioxide);
		GameObject gameObject = exhaustPool.GetInstance();
		gameObject.transform.SetPosition(position);
		gameObject.SetActive(value: true);
		CO2 cO = gameObject.AddOrGet<CO2>();
		cO.mass = mass;
		cO.temperature = temperature;
		cO.lifetimeRemaining = 3f;
		cO.affectedByGravity = false;
		KBatchedAnimController component = gameObject.GetComponent<KBatchedAnimController>();
		component.onDestroySelf = OnDestroyExhaust;
		component.Rotation = rotation;
		component.Play("smoke_particle");
	}

	private void OnDestroyCO2(GameObject co2_go)
	{
		co2_go.SetActive(value: false);
		co2Pool.ReleaseInstance(co2_go);
	}

	private void OnDestroyBreath(GameObject breath_go)
	{
		breath_go.SetActive(value: false);
		breathPool.ReleaseInstance(breath_go);
	}

	private void OnDestroyExhaust(GameObject go)
	{
		go.SetActive(value: false);
		exhaustPool.ReleaseInstance(go);
	}
}
