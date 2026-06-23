using System.Collections.Generic;

public class GraveStorage : Storage
{
	public Dictionary<Tag, KAnimFile[]> workerTypeOverrideAnims = new Dictionary<Tag, KAnimFile[]>();

	public override AnimInfo GetAnim(WorkerBase worker)
	{
		KAnimFile[] value = null;
		if (workerTypeOverrideAnims.TryGetValue(worker.PrefabID(), out value))
		{
			overrideAnims = value;
		}
		return base.GetAnim(worker);
	}
}
