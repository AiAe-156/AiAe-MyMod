using System;
using UnityEngine;

public class DissolvingElementDiseaseEmitter : DiseaseEmitter
{
	protected SpawnFXHashes spawnFXHash = SpawnFXHashes.OxygenEmissionBubbles;

	protected float massDecayScale = 0.0001f;

	protected float massConversionRatio = 0.5f;

	protected const float massForMinEmission = 200f;

	protected const float massForMaxEmission = 1000f;

	private float minimumBubbleSize = 0.1f;

	private Guid statusItemGUID;

	protected float massDecayAccumulation = 0f;

	public SimHashes DissolveTargetElement { get; private set; }

	public float CurrentAverageDissolveRate { get; private set; } = 0f;

	public PrimaryElement PrimaryElement { get; private set; }

	public DissolvingElementDiseaseEmitter(SimHashes dissolveTargetElement)
	{
		DissolveTargetElement = dissolveTargetElement;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Init();
		UpdateStatusItem();
	}

	private void Init()
	{
		PrimaryElement = GetComponent<PrimaryElement>();
	}

	private void Update()
	{
		EvaluateEmissionCondition();
		MakeDiseaseAndBubbles();
	}

	protected virtual void EvaluateEmissionCondition()
	{
	}

	protected void UpdateStatusItem()
	{
		if (enableEmitter)
		{
			statusItemGUID = GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().BuildingStatusItems.DissolvingElementDissolving, this);
		}
		else
		{
			statusItemGUID = GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().BuildingStatusItems.DissolvingElementDormant, this);
		}
	}

	protected void MakeDiseaseAndBubbles()
	{
		if (Time.deltaTime == 0f || !enableEmitter)
		{
			return;
		}
		CurrentAverageDissolveRate = massDecayScale * Mathf.Clamp(PrimaryElement.Mass, 200f, 1000f);
		float num = CurrentAverageDissolveRate * Time.deltaTime;
		if (PrimaryElement.Mass > num)
		{
			massDecayAccumulation += num;
			if (massDecayAccumulation >= minimumBubbleSize)
			{
				float num2 = Mathf.Min(PrimaryElement.Mass, massDecayAccumulation);
				PrimaryElement.Mass -= num2;
				BubbleManager.instance.SpawnBubble(DissolveTargetElement, base.transform.position, num2 * massConversionRatio, PrimaryElement.Temperature, BubbleManager.Disease.None);
				massDecayAccumulation = 0f;
				SpawnVisualFX();
			}
		}
		else
		{
			SpawnVisualFX();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected void SpawnVisualFX()
	{
		if (spawnFXHash != SpawnFXHashes.None)
		{
			Vector3 position = base.transform.GetPosition();
			position.z = Grid.GetLayerZ(Grid.SceneLayer.Front);
			Game.Instance.SpawnFX(spawnFXHash, base.transform.GetPosition(), 0f);
		}
	}
}
