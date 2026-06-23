using System.Collections.Generic;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class ArtifactPOIClusterGridEntity : ClusterGridEntity
{
	public string m_name;

	public string m_Anim;

	public override string Name => m_name;

	public override EntityLayer Layer => EntityLayer.POI;

	public override List<AnimConfig> AnimConfigs => new List<AnimConfig>
	{
		new AnimConfig
		{
			animFile = Assets.GetAnim("gravitas_space_poi_kanim"),
			initialAnim = (m_Anim.IsNullOrWhiteSpace() ? "station_1" : m_Anim)
		}
	};

	public override bool IsVisible => true;

	public override ClusterRevealLevel IsVisibleInFOW => ClusterRevealLevel.Peeked;

	public void Init(AxialI location)
	{
		base.Location = location;
	}

	public override Sprite GetUISprite()
	{
		Sprite uISpriteFromMultiObjectAnim = Def.GetUISpriteFromMultiObjectAnim(AnimConfigs[0].animFile, AnimConfigs[0].initialAnim);
		return (uISpriteFromMultiObjectAnim == null) ? base.GetUISprite() : uISpriteFromMultiObjectAnim;
	}
}
