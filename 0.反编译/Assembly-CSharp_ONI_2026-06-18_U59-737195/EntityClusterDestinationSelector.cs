using KSerialization;
using UnityEngine;

public class EntityClusterDestinationSelector : ClusterDestinationSelector
{
	[Serialize]
	protected Ref<ClusterGridEntity> m_DestinationEntity = new Ref<ClusterGridEntity>();

	private ClusterGridEntity DestinationEntity
	{
		get
		{
			if (m_DestinationEntity == null)
			{
				return null;
			}
			return m_DestinationEntity.Get();
		}
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Debug.Assert(requiredEntityLayer != EntityLayer.None, "EnityClusterDestinationSelector must specify an EntityLayer");
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Subscribe(-905833192, OnCopySettings);
	}

	private void OnCopySettings(object data)
	{
		GameObject gameObject = (GameObject)data;
		if (gameObject != null)
		{
			EntityClusterDestinationSelector component = gameObject.GetComponent<EntityClusterDestinationSelector>();
			if (component != null && component.DestinationEntity != null)
			{
				m_DestinationEntity = new Ref<ClusterGridEntity>(component.DestinationEntity);
				SetDestination(m_DestinationEntity.Get().Location);
			}
		}
	}

	public override ClusterGridEntity GetClusterEntityTarget()
	{
		return DestinationEntity;
	}

	public override AxialI GetDestination()
	{
		if (DestinationEntity != null)
		{
			return DestinationEntity.Location;
		}
		return base.GetDestination();
	}

	public override void SetDestination(AxialI location)
	{
		ClusterGridEntity visibleEntityOfLayerAtCell = ClusterGrid.Instance.GetVisibleEntityOfLayerAtCell(location, requiredEntityLayer);
		m_DestinationEntity.Set(visibleEntityOfLayerAtCell);
		base.SetDestination(location);
	}
}
