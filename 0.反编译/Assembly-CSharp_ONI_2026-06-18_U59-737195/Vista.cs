using UnityEngine;

public class Vista : KMonoBehaviour
{
	public string prefabName;

	public string audioName;

	public Grid.SceneLayer sceneLayer;

	public GameObject visualizer;

	public int width;

	public int height;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.transform.SetPosition(new Vector3(base.transform.position.x, base.transform.position.y, Grid.GetLayerZ(sceneLayer)));
		visualizer = Object.Instantiate(Assets.instance.vistasPrefabs.Find((GameObject p) => p.name == prefabName));
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y + (float)height / 2f, base.transform.position.z);
		visualizer.transform.position = position;
		visualizer.transform.SetParent(base.transform, worldPositionStays: true);
		visualizer.gameObject.SetActive(value: true);
		if (!string.IsNullOrEmpty(audioName))
		{
			LoopingSounds component = GetComponent<LoopingSounds>();
			if (component != null)
			{
				component.StartSound(GlobalAssets.GetSound(audioName));
			}
		}
	}
}
