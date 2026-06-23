using System;

public class VistaParallax : KMonoBehaviour
{
	[Serializable]
	public class BgLayer
	{
		public KBatchedAnimController kbac;

		public float distance;
	}

	public BgLayer[] layers;

	public KBatchedAnimController maskKanim;

	private static readonly string maskAnimationName = "mask";

	protected override void OnPrefabInit()
	{
		maskKanim.Stop();
		maskKanim.AnimFiles = new KAnimFile[1] { Assets.GetAnim("beachbg_parallax_kanim") };
		maskKanim.Play(maskAnimationName, KAnim.PlayMode.Paused);
		base.transform.SetParent(GameScreenManager.Instance.worldSpaceCanvas.transform, worldPositionStays: true);
		SetLayer("beachbg_parallax_kanim", 0);
	}

	public void SetLayer(string animation, int layer)
	{
		BgLayer bgLayer = layers[layer];
		bgLayer.kbac.Stop();
		bgLayer.kbac.AnimFiles = new KAnimFile[1] { Assets.GetAnim("beachbg_parallax_kanim") };
		bgLayer.kbac.Play("layer" + layer);
	}
}
