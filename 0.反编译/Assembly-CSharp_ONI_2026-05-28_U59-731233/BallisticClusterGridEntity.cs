using System.Collections.Generic;
using UnityEngine;

public class BallisticClusterGridEntity : ClusterGridEntity
{
	[MyCmpReq]
	private ClusterDestinationSelector m_destionationSelector;

	[MyCmpReq]
	private ClusterTraveler m_clusterTraveler;

	[SerializeField]
	public string clusterAnimName;

	[SerializeField]
	public StringKey nameKey;

	private string clusterAnimSymbolSwapTarget;

	private string clusterAnimSymbolSwapSymbol;

	public bool keepRotationWhenSpacingOutInHex = false;

	public override string Name => Strings.Get(nameKey);

	public override EntityLayer Layer => EntityLayer.Payload;

	public override List<AnimConfig> AnimConfigs => new List<AnimConfig>
	{
		new AnimConfig
		{
			animFile = Assets.GetAnim(clusterAnimName),
			initialAnim = "idle_loop",
			symbolSwapTarget = clusterAnimSymbolSwapTarget,
			symbolSwapSymbol = clusterAnimSymbolSwapSymbol
		}
	};

	public override bool IsVisible => !base.gameObject.HasTag(GameTags.ClusterEntityGrounded);

	public override ClusterRevealLevel IsVisibleInFOW => ClusterRevealLevel.Visible;

	public override bool KeepRotationWhenSpacingOutInHex()
	{
		return keepRotationWhenSpacingOutInHex;
	}

	public override bool SpaceOutInSameHex()
	{
		return true;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		m_clusterTraveler.getSpeedCB = GetSpeed;
		m_clusterTraveler.getCanTravelCB = CanTravel;
		m_clusterTraveler.onTravelCB = null;
	}

	private float GetSpeed()
	{
		return 10f;
	}

	private bool CanTravel(bool tryingToLand)
	{
		return this.HasTag(GameTags.EntityInSpace);
	}

	public void Configure(AxialI source, AxialI destination)
	{
		m_location = source;
		m_destionationSelector.SetDestination(destination);
	}

	public override bool ShowPath()
	{
		return m_selectable.IsSelected;
	}

	public override bool ShowProgressBar()
	{
		return m_selectable.IsSelected && m_clusterTraveler.IsTraveling();
	}

	public override float GetProgress()
	{
		return m_clusterTraveler.GetMoveProgress();
	}

	public void SwapSymbolFromSameAnim(string targetSymbolName, string swappedSymbolName)
	{
		clusterAnimSymbolSwapTarget = targetSymbolName;
		clusterAnimSymbolSwapSymbol = swappedSymbolName;
	}
}
