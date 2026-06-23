using UnityEngine;

public class RunningWeightedAverage
{
	private struct Entry
	{
		public float time;

		public float value;

		public bool IsValid()
		{
			return time != float.NaN;
		}
	}

	private RingBuffer<Entry> samples;

	private float min;

	private float max;

	private bool ignoreZero = false;

	public float GetUnweightedAverage => GetAverageOfLastSeconds(4f);

	public RunningWeightedAverage(float minValue = float.MinValue, float maxValue = float.MaxValue, int sampleCount = 20, bool allowZero = true)
	{
		min = minValue;
		max = maxValue;
		ignoreZero = !allowZero;
		samples = new RingBuffer<Entry>(sampleCount, new Entry
		{
			time = float.NaN,
			value = float.NaN
		});
	}

	public void AddSample(float value, float timeOfRecord)
	{
		if (!ignoreZero || value != 0f)
		{
			if (value > max)
			{
				value = max;
			}
			if (value < min)
			{
				value = min;
			}
			samples.Add(new Entry
			{
				time = timeOfRecord,
				value = value
			});
		}
	}

	public int ValidRecordsInLastSeconds(float seconds)
	{
		int num = 0;
		float time = Time.time;
		for (int i = 0; i < samples.Count; i++)
		{
			Entry entry = samples[i];
			if (!float.IsNaN(entry.time) && time - entry.time <= seconds)
			{
				num++;
			}
		}
		return num;
	}

	private float GetAverageOfLastSeconds(float seconds)
	{
		float num = 0f;
		int num2 = 0;
		float time = Time.time;
		for (int i = 0; i < samples.Count; i++)
		{
			Entry entry = samples[i];
			if (!float.IsNaN(entry.time) && time - entry.time <= seconds)
			{
				num += entry.value;
				num2++;
			}
		}
		if (num2 == 0)
		{
			return 0f;
		}
		return num / (float)num2;
	}
}
