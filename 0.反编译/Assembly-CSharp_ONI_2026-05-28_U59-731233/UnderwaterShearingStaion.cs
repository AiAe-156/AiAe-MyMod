using UnityEngine;

public class UnderwaterShearingStaion : KMonoBehaviour
{
	private KBatchedAnimController symbolController;

	private MeterController meter;

	private static HashedString SYMBOL_HASH = "object_fg";

	private Storage storage;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		storage = GetComponent<Storage>();
		meter = new MeterController(this, Meter.Offset.Infront, Grid.SceneLayer.BuildingFront, "object_fg");
		UpdateMeter(null);
		SetupShearableSymbol();
	}

	public void UpdateMeter(object data)
	{
		meter.SetPositionPercent(0f);
	}

	public void UpdateShearableSymbol(Tag item_tag)
	{
		symbolController.gameObject.SetActive(value: true);
		GameObject prefab = Assets.GetPrefab(item_tag);
		symbolController.SwapAnims(prefab.GetComponent<KBatchedAnimController>().AnimFiles);
		symbolController.Play("idle1", KAnim.PlayMode.Loop);
	}

	public void HideShearableSymbol()
	{
		symbolController.gameObject.SetActive(value: false);
	}

	public void SetupShearableSymbol()
	{
		KBatchedAnimController component = base.gameObject.GetComponent<KBatchedAnimController>();
		KBatchedAnimController[] componentsInChildren = base.gameObject.GetComponentsInChildren<KBatchedAnimController>(includeInactive: true);
		GameObject gameObject = Util.NewGameObject(base.gameObject, base.gameObject.name + ".ore_symbol");
		gameObject.SetActive(value: false);
		bool symbolVisible;
		Vector4 column = component.GetSymbolTransform(SYMBOL_HASH, out symbolVisible).GetColumn(3);
		Vector3 position = column;
		position.z = component.transform.GetPosition().z - 0.05f;
		gameObject.transform.SetPosition(position);
		symbolController = gameObject.AddComponent<KBatchedAnimController>();
		symbolController.AnimFiles = new KAnimFile[1] { Assets.GetAnim("hematite_kanim") };
		symbolController.initialAnim = "idle1";
		component.SetSymbolVisiblity(SYMBOL_HASH, is_visible: false);
		KBatchedAnimController[] array = componentsInChildren;
		foreach (KBatchedAnimController kBatchedAnimController in array)
		{
			kBatchedAnimController.SetSymbolVisiblity(SYMBOL_HASH, is_visible: false);
		}
		KBatchedAnimTracker kBatchedAnimTracker = gameObject.AddComponent<KBatchedAnimTracker>();
		kBatchedAnimTracker.symbol = SYMBOL_HASH;
		kBatchedAnimTracker.offset = Vector3.zero;
	}
}
