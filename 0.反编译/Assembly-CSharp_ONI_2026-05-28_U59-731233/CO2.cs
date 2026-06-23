using System;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/CO2")]
public class CO2 : KMonoBehaviour
{
	[NonSerialized]
	[Serialize]
	public Vector3 velocity = Vector3.zero;

	[NonSerialized]
	[Serialize]
	public float mass;

	[NonSerialized]
	[Serialize]
	public float temperature;

	[NonSerialized]
	[Serialize]
	public float lifetimeRemaining;

	[NonSerialized]
	[Serialize]
	public string kAnimFileName = "exhale_kanim";

	[NonSerialized]
	[Serialize]
	public string anim_name_pre = "exhale_pre";

	[NonSerialized]
	[Serialize]
	public string anim_name_loop = "exhale_loop";

	[NonSerialized]
	[Serialize]
	public string anim_name_pst = "exhale_pst";

	[NonSerialized]
	[Serialize]
	public bool affectedByGravity = true;

	protected override void OnSpawn()
	{
		base.OnSpawn();
	}

	public void StartLoop()
	{
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		component.Play(anim_name_pre);
		component.Play(anim_name_loop, KAnim.PlayMode.Loop);
	}

	public void TriggerDestroy()
	{
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		component.Play(anim_name_pst);
	}
}
