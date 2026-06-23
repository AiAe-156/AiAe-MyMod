using KSerialization;
using STRINGS;

public class ClusterDestinationSelector : KMonoBehaviour
{
	[Serialize]
	protected AxialI m_destination;

	public bool assignable;

	public bool requireAsteroidDestination;

	[Serialize]
	public bool canNavigateFogOfWar;

	public bool dodgesHiddenAsteroids;

	public bool requireLaunchPadOnAsteroidDestination;

	public bool shouldPointTowardsPath;

	public string sidescreenTitleString = UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.TITLE;

	public string changeTargetButtonTooltipString = UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.CHANGE_DESTINATION_BUTTON_TOOLTIP;

	public string clearTargetButtonTooltipString = UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.CLEAR_DESTINATION_BUTTON_TOOLTIP;

	public EntityLayer requiredEntityLayer = EntityLayer.None;

	private EventSystem.IntraObjectHandler<ClusterDestinationSelector> OnClusterLocationChangedDelegate = new EventSystem.IntraObjectHandler<ClusterDestinationSelector>(delegate(ClusterDestinationSelector cmp, object data)
	{
		cmp.OnClusterLocationChanged(data);
	});

	protected override void OnPrefabInit()
	{
		Subscribe(-1298331547, OnClusterLocationChangedDelegate);
	}

	protected virtual void OnClusterLocationChanged(object data)
	{
		if (((ClusterLocationChangedEvent)data).newLocation == m_destination)
		{
			Trigger(1796608350, data);
		}
	}

	public int GetDestinationWorld()
	{
		return ClusterUtil.GetAsteroidWorldIdAtLocation(m_destination);
	}

	public virtual AxialI GetDestination()
	{
		return m_destination;
	}

	public virtual ClusterGridEntity GetClusterEntityTarget()
	{
		return null;
	}

	public virtual void SetDestination(AxialI location)
	{
		if (requireAsteroidDestination)
		{
			Debug.Assert(ClusterUtil.GetAsteroidWorldIdAtLocation(location) != -1, $"Cannot SetDestination to {location} as there is no world there");
		}
		m_destination = location;
		BoxingTrigger(543433792, location);
	}

	public bool HasAsteroidDestination()
	{
		return ClusterUtil.GetAsteroidWorldIdAtLocation(m_destination) != -1;
	}

	public virtual bool IsAtDestination()
	{
		return this.GetMyWorldLocation() == m_destination;
	}
}
