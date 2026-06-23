using System.Collections.Generic;
using System.Linq;
using ProcGen;
using UnityEngine;

namespace FUtility;

public static class Extensions
{
	public static T GetWeightedRandom<T>(this IEnumerable<T> enumerator, SeededRandom rand = null) where T : IWeighted
	{
		if (enumerator == null || enumerator.Count() == 0)
		{
			return default(T);
		}
		float num = enumerator.Sum((T n) => ((IWeighted)n).weight);
		float num2 = ((rand == null) ? Random.value : rand.RandomValue());
		num2 *= num;
		float num3 = 0f;
		foreach (T item in enumerator)
		{
			num3 += ((IWeighted)item).weight;
			if (num3 > num2)
			{
				return item;
			}
		}
		return enumerator.GetEnumerator().Current;
	}
}
