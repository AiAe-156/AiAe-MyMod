using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/UpdateElementConsumerPosition")]
public class UpdateElementConsumerPosition : KMonoBehaviour, ISim200ms
{
	private ElementConsumer consumer;

	protected override void OnSpawn()
	{
		consumer = GetComponent<ElementConsumer>();
	}

	public void Sim200ms(float dt)
	{
		consumer.GetComponent<ElementConsumer>().RefreshConsumptionRate();
	}
}
