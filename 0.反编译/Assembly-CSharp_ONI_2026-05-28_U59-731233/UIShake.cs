using UnityEngine;

public class UIShake : KMonoBehaviour, IRenderEveryTick
{
	public Vector2 MaxOffsets = Vector2.one;

	private float lastIntensity = 0f;

	private float intensity = 0f;

	private Vector2 initialLocalPosition;

	private new RectTransform transform;

	public float Intensity => intensity;

	public void RenderEveryTick(float dt)
	{
		if (intensity != 0f || lastIntensity != 0f)
		{
			lastIntensity = intensity;
			Vector2 vector = new Vector2(Random.Range(-1f, 1f) * MaxOffsets.x * intensity, Random.Range(-1f, 1f) * MaxOffsets.y * intensity);
			Vector2 anchoredPosition = initialLocalPosition + vector;
			transform.anchoredPosition = anchoredPosition;
		}
	}

	public void SetIntensity(float intensity)
	{
		this.intensity = intensity;
	}

	protected override void OnPrefabInit()
	{
		transform = base.transform as RectTransform;
		initialLocalPosition = transform.anchoredPosition;
	}
}
