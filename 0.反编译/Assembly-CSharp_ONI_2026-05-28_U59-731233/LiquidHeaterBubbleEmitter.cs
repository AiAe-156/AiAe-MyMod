using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class LiquidHeaterBubbleEmitter : KMonoBehaviour, ISim1000ms
{
	private enum EmitResult
	{
		None,
		InsufficientMass,
		Emitted
	}

	[MyCmpReq]
	private Building building;

	[MyCmpReq]
	private SpaceHeater spaceHeater;

	private const float MIN_BUBBLE_HEAT_FRACTION = 0.001f;

	private const float MAX_BUBBLE_HEAT_FRACTION = 0.01f;

	private const float MIN_EMIT_MASS = 0.0020000022f;

	public float BubblePowerThreshold;

	[Serialize]
	private float accruedEnergyKJ;

	private float BubbleHeatFraction => Mathf.Lerp(0.001f, 0.01f, spaceHeater.UserSliderSetting);

	public float DivertEnergy(float exhaustKW, float dt)
	{
		if (spaceHeater.CurrentPowerConsumption < BubblePowerThreshold)
		{
			accruedEnergyKJ = 0f;
			return 0f;
		}
		float num = exhaustKW * BubbleHeatFraction;
		accruedEnergyKJ += num * dt;
		return num;
	}

	public void Sim1000ms(float dt)
	{
		if (accruedEnergyKJ <= 0f)
		{
			return;
		}
		int num = building.PlacementCells.Length;
		float num2 = accruedEnergyKJ / (float)num;
		float num3 = 0f;
		int[] placementCells = building.PlacementCells;
		foreach (int num4 in placementCells)
		{
			Element element = Grid.Element[num4];
			if (!element.IsLiquid || !element.HasTransitionUp)
			{
				continue;
			}
			float num5 = Grid.Mass[num4];
			if (num5 <= 0f)
			{
				continue;
			}
			float num6 = Grid.Temperature[num4];
			float num7 = element.highTemp + 3f;
			float num8 = num7 - num6;
			if (num8 <= 0f)
			{
				continue;
			}
			float num9 = element.specificHeatCapacity * num8;
			float num10 = num2 / num9;
			if (!(num10 < 0.0020000022f))
			{
				float num11 = Mathf.Min(num10, num5);
				float bubbleMass = num11;
				byte b = Grid.DiseaseIdx[num4];
				DebugUtil.DevAssert(num5 > 0f, "Cell mass is zero or negative, cannot scale disease count.");
				int num12 = (int)((float)Grid.DiseaseCount[num4] * (num11 / num5));
				int bubbleDiseaseCount = num12;
				Vector2 vector = Grid.CellToPosCCC(num4, Grid.SceneLayer.Front);
				EmitResult emitResult = TryEmitOreByproduct(element, num11, ref bubbleMass, b, num12, ref bubbleDiseaseCount, num4, num6, vector);
				if (emitResult != EmitResult.InsufficientMass)
				{
					num3 += num11 * num9;
					SimMessages.AddRemoveSubstance(num4, element.id, CellEventLogger.Instance.ElementEmitted, 0f - num11, num6, b, -bubbleDiseaseCount);
					BubbleManager.Disease disease = new BubbleManager.Disease
					{
						Idx = b,
						Count = bubbleDiseaseCount
					};
					BubbleManager.instance.SpawnBubble(element.highTempTransitionTarget, vector, bubbleMass, num7, disease);
				}
			}
		}
		accruedEnergyKJ = Mathf.Max(accruedEnergyKJ - num3, 0f);
	}

	private EmitResult TryEmitOreByproduct(Element sourceElement, float boiledMass, ref float bubbleMass, byte diseaseIdx, int diseaseCount, ref int bubbleDiseaseCount, int cell, float cellTemp, Vector2 bubblePosition)
	{
		SimHashes highTempTransitionOreID = sourceElement.highTempTransitionOreID;
		if (highTempTransitionOreID == (SimHashes)0)
		{
			return EmitResult.None;
		}
		float highTempTransitionOreMassConversion = sourceElement.highTempTransitionOreMassConversion;
		if (highTempTransitionOreMassConversion <= 0f)
		{
			return EmitResult.None;
		}
		float num = boiledMass * highTempTransitionOreMassConversion;
		if (num < 0.001f)
		{
			return EmitResult.InsufficientMass;
		}
		int num2 = (int)((float)diseaseCount * highTempTransitionOreMassConversion);
		bubbleMass -= num;
		bubbleDiseaseCount -= num2;
		Element element = ElementLoader.FindElementByHash(highTempTransitionOreID);
		if (element.IsSolid)
		{
			element.substance.SpawnResource(bubblePosition, num, cellTemp, diseaseIdx, num2);
		}
		else
		{
			SimMessages.AddRemoveSubstance(cell, highTempTransitionOreID, CellEventLogger.Instance.ElementEmitted, num, cellTemp, diseaseIdx, num2);
		}
		return EmitResult.Emitted;
	}
}
