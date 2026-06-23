using UnityEngine;

public class ArtifactModule : SingleEntityReceptacle, IRenderEveryTick, IHexCellCollector
{
	[MyCmpReq]
	private KBatchedAnimController animController;

	[MyCmpReq]
	private RocketModuleCluster module;

	private Clustercraft craft;

	protected override void OnSpawn()
	{
		craft = module.CraftInterface.GetComponent<Clustercraft>();
		if (craft.Status == Clustercraft.CraftStatus.InFlight && base.occupyingObject != null)
		{
			base.occupyingObject.SetActive(value: false);
		}
		base.OnSpawn();
		Subscribe(705820818, OnEnterSpace);
		Subscribe(-1165815793, OnExitSpace);
	}

	public void RenderEveryTick(float dt)
	{
		ArtifactTrackModulePosition();
	}

	private void ArtifactTrackModulePosition()
	{
		occupyingObjectRelativePosition = animController.Offset + Vector3.up * 0.5f + new Vector3(0f, 0f, -1f);
		if (base.occupyingObject != null)
		{
			PositionOccupyingObject();
		}
	}

	private void OnEnterSpace(object data)
	{
		if (base.occupyingObject != null)
		{
			base.occupyingObject.SetActive(value: false);
		}
	}

	private void OnExitSpace(object data)
	{
		if (base.occupyingObject != null)
		{
			base.occupyingObject.SetActive(value: true);
		}
	}

	public bool CheckIsCollecting()
	{
		return false;
	}

	public string GetProperName()
	{
		return GetComponent<RocketModuleCluster>().GetProperName();
	}

	public Sprite GetUISprite()
	{
		return Def.GetUISprite(base.gameObject.GetComponent<KPrefabID>().PrefabID()).first;
	}

	public float GetCapacity()
	{
		return 1f;
	}

	public float GetMassStored()
	{
		return storage.items.Count;
	}

	public float TimeInState()
	{
		return 0f;
	}

	public string GetCapacityBarText()
	{
		return $"{GetMassStored()} / {GetCapacity()}";
	}
}
