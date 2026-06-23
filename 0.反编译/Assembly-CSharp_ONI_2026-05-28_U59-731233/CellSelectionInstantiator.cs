using UnityEngine;

public class CellSelectionInstantiator : MonoBehaviour
{
	public GameObject CellSelectionPrefab;

	private void Awake()
	{
		GameObject gameObject = Util.KInstantiate(CellSelectionPrefab, null, "WorldSelectionCollider");
		GameObject gameObject2 = Util.KInstantiate(CellSelectionPrefab, null, "WorldSelectionCollider");
		CellSelectionObject component = gameObject.GetComponent<CellSelectionObject>();
		(component.alternateSelectionObject = gameObject2.GetComponent<CellSelectionObject>()).alternateSelectionObject = component;
		CreateProxySelectionObject<BackwallSelectionObject>("BackwallSelectionCollider");
	}

	private static void CreateProxySelectionObject<T>(string name) where T : KMonoBehaviour
	{
		GameObject gameObject = new GameObject(name);
		gameObject.SetActive(value: false);
		gameObject.AddComponent<T>();
		KSelectable kSelectable = gameObject.AddComponent<KSelectable>();
		kSelectable.DisableSelectMarker = true;
		gameObject.AddComponent<KBoxCollider2D>();
		GameObject gameObject2 = new GameObject("visualizer");
		gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
		SpriteRenderer spriteRenderer = gameObject2.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = Assets.GetSprite("cursorIcon");
		spriteRenderer.sortingOrder = 1;
		gameObject2.transform.localPosition = new Vector3(0f, 0f, -10f);
		gameObject2.transform.localScale = new Vector3(0.39f, 0.39f, 1f);
		gameObject.SetActive(value: true);
	}
}
