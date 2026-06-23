using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/SimpleMassStatusItem")]
public class SimpleMassStatusItem : KMonoBehaviour
{
	public string symbolPrefix = "";

	protected override void OnSpawn()
	{
		base.OnSpawn();
		KSelectable component = GetComponent<KSelectable>();
		component.AddStatusItem(Db.Get().MiscStatusItems.OreMass, base.gameObject);
	}
}
