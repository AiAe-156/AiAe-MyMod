using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/IridescenceEffect")]
public class IridescenceEffect : KMonoBehaviour
{
	public Color pearl1;

	public Color pearl2;

	private void Update()
	{
		if (!(World.Instance == null))
		{
			GroundRenderer groundRenderer = World.Instance.groundRenderer;
			if (groundRenderer != null)
			{
				Vector3 position = Camera.main.transform.position;
				float zoomFactor = CameraController.Instance.zoomFactor;
				float t = (Mathf.Cos(position.x / zoomFactor) + Mathf.Sin(position.y / zoomFactor)) / 4f + 0.5f;
				UpdatePearl(groundRenderer, t);
			}
		}
	}

	private void UpdatePearl(GroundRenderer renderer, float t)
	{
		Color edgeColor = Color.Lerp(pearl1, pearl2, t);
		Color centerColor = Color.Lerp(pearl2, pearl1, t);
		renderer.SetShineColors(SimHashes.Pearl, centerColor, edgeColor);
	}
}
