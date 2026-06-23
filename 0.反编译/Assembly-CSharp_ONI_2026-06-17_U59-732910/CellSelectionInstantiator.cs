using UnityEngine;

public class CellSelectionInstantiator : MonoBehaviour
{
	public GameObject CellSelectionPrefab;

	private void Awake()
	{
		GameObject gameObject = Util.KInstantiate(CellSelectionPrefab, null, "WorldSelectionCollider");
		GameObject obj = Util.KInstantiate(CellSelectionPrefab, null, "WorldSelectionCollider");
		CellSelectionObject component = gameObject.GetComponent<CellSelectionObject>();
		(component.alternateSelectionObject = obj.GetComponent<CellSelectionObject>()).alternateSelectionObject = component;
		CreateBackwallSelectionProxy();
	}

	private static void CreateBackwallSelectionProxy()
	{
		GameObject obj = new GameObject("BackwallSelectionCollider");
		obj.SetActive(value: false);
		obj.AddComponent<BackwallSelectionObject>();
		obj.AddComponent<KSelectable>().DisableSelectMarker = true;
		obj.AddComponent<KBoxCollider2D>();
		obj.SetActive(value: true);
	}
}
