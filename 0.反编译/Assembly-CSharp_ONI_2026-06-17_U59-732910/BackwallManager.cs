using UnityEngine;

public static class BackwallManager
{
	public readonly struct BackwallIndexer
	{
		public readonly int index;

		public unsafe Element Element
		{
			get
			{
				if (element_idx[index] == ushort.MaxValue)
				{
					return null;
				}
				return ElementLoader.elements[element_idx[index]];
			}
		}

		public unsafe float Mass => mass[index];

		public unsafe float Temperature => temperature[index];

		public BackwallIndexer(int index)
		{
			this.index = index;
		}
	}

	private unsafe static ushort* element_idx;

	private unsafe static float* mass;

	private unsafe static float* temperature;

	public unsafe static void UpdateFromSim(Sim.GameDataUpdate* data)
	{
		element_idx = data->backwallElement;
		mass = data->backwallMass;
		temperature = data->backwallTemperature;
		for (int i = 0; i < data->numBackwallShouldTransitionInfos; i++)
		{
			int gameCell = data->backwallShouldTransitionInfos[i].gameCell;
			Element element = At(gameCell).Element;
			if (element == null || element.IsVacuum)
			{
				continue;
			}
			ushort num = ushort.MaxValue;
			float num2 = 0f;
			bool flag = false;
			float num3;
			ushort idx;
			float num4;
			bool isSolid;
			if (At(gameCell).Temperature > element.highTemp)
			{
				num3 = At(gameCell).Temperature - 1.5f;
				idx = element.highTempTransition.idx;
				num4 = (1f - element.highTempTransitionOreMassConversion) * At(gameCell).Mass;
				isSolid = element.highTempTransition.IsSolid;
				if (element.highTempTransitionOreID != 0)
				{
					Element element2 = ElementLoader.FindElementByHash(element.highTempTransitionOreID);
					num = element2.idx;
					flag = element2.IsSolid;
					num2 = At(gameCell).Mass - num4;
				}
			}
			else
			{
				if (!(At(gameCell).Temperature < element.lowTemp))
				{
					continue;
				}
				num3 = At(gameCell).Temperature + 1.5f;
				idx = element.lowTempTransition.idx;
				num4 = (1f - element.lowTempTransitionOreMassConversion) * At(gameCell).Mass;
				isSolid = element.lowTempTransition.IsSolid;
				if (element.lowTempTransitionOreID != 0)
				{
					Element element3 = ElementLoader.FindElementByHash(element.lowTempTransitionOreID);
					num = element3.idx;
					flag = element3.IsSolid;
					num2 = At(gameCell).Mass - num4;
				}
			}
			if (isSolid)
			{
				SimMessages.SetBackwallData(gameCell, idx, num4, num3);
			}
			else
			{
				SimMessages.SetBackwallData(gameCell, ElementLoader.GetElementIndex(SimHashes.Vacuum), 0f, 0f);
				SimMessages.AddRemoveSubstance(gameCell, idx, CellEventLogger.Instance.OreMelted, num4, num3, byte.MaxValue, 0);
			}
			if (num2 > 0.001f)
			{
				if (flag)
				{
					Element element4 = ElementLoader.elements[num];
					GameObject obj = element4.substance.SpawnResource(Grid.CellToPos(gameCell), num2, num3, byte.MaxValue, 0, prevent_merge: true, forceTemperature: false, manual_activation: true);
					element4.substance.ActivateSubstanceGameObject(obj, byte.MaxValue, 0);
				}
				else
				{
					SimMessages.AddRemoveSubstance(gameCell, num, CellEventLogger.Instance.OreMelted, num2, num3, byte.MaxValue, 0);
				}
			}
		}
	}

	public static bool HasBackwall(int cell)
	{
		return At(cell).Element?.IsSolid ?? false;
	}

	public static BackwallIndexer At(int index)
	{
		return new BackwallIndexer(index);
	}

	public unsafe static void Clear()
	{
		element_idx = null;
		mass = null;
		temperature = null;
	}
}
