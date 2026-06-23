using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/LightSymbolTracker")]
public class LightSymbolTracker : KMonoBehaviour, IRenderEveryTick
{
	public HashedString targetSymbol;

	private KBatchedAnimController animController;

	private Light2D light2D;

	private Pickupable pickupable;

	protected override void OnSpawn()
	{
		animController = GetComponent<KBatchedAnimController>();
		light2D = GetComponent<Light2D>();
		pickupable = GetComponent<Pickupable>();
	}

	public bool IsEnableAndVisible()
	{
		return CameraController.Instance.VisibleArea.CurrentAreaExtended.Contains(pickupable.cachedCell) && base.enabled;
	}

	public void RenderEveryTick(float dt)
	{
		if (IsEnableAndVisible())
		{
			Vector3 zero = Vector3.zero;
			zero = (animController.GetTransformMatrix() * animController.GetSymbolLocalTransform(targetSymbol, out var _)).MultiplyPoint(Vector3.zero) - base.transform.position;
			light2D.Offset = zero;
		}
	}
}
