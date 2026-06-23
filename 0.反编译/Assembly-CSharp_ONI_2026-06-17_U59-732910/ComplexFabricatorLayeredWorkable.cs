public class ComplexFabricatorLayeredWorkable : ComplexFabricatorWorkable
{
	public Grid.SceneLayer foregroundLayer = Grid.SceneLayer.NoLayer;

	public override AnimInfo GetAnim(WorkerBase worker)
	{
		AnimInfo anim = base.GetAnim(worker);
		if (foregroundLayer != Grid.SceneLayer.NoLayer)
		{
			anim.smi = new SimpleLayeredAnimWork.Instance(base.gameObject, worker, foregroundLayer, synchronizeAnims, anim.overrideAnims);
		}
		return anim;
	}
}
