public class RunningAverage
{
	private RingBuffer<float> samples;

	private float min;

	private float max;

	private bool ignoreZero;

	public float AverageValue => GetAverage();

	public RunningAverage(float minValue = float.MinValue, float maxValue = float.MaxValue, int sampleCount = 15, bool allowZero = true)
	{
		samples = new RingBuffer<float>(sampleCount, float.NaN);
		min = minValue;
		max = maxValue;
		ignoreZero = !allowZero;
	}

	public void AddSample(float value)
	{
		if (!(value < min) && !(value > max) && (!ignoreZero || value != 0f))
		{
			samples.Add(value);
		}
	}

	private float GetAverage()
	{
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < samples.Count; i++)
		{
			float num3 = samples[i];
			if (!float.IsNaN(num3))
			{
				num += num3;
				num2++;
			}
		}
		if (num2 == 0)
		{
			return float.NaN;
		}
		return num / (float)num2;
	}
}
