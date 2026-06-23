using System;
using System.Runtime.Serialization;
using KSerialization;
using UnityEngine;

public class SelfChargingElectrobank : Electrobank
{
	[Serialize]
	private float lifetimeRemaining = 90000f;

	private KSelectable selectable;

	private Guid lifetimeStatus = Guid.Empty;

	public float LifetimeRemaining => lifetimeRemaining;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		selectable = GetComponent<KSelectable>();
		selectable.AddStatusItem(Db.Get().MiscStatusItems.ElectrobankSelfCharging, 60f);
		lifetimeStatus = selectable.AddStatusItem(Db.Get().MiscStatusItems.ElectrobankLifetimeRemaining, this);
		Components.SelfChargingElectrobanks.Add(base.gameObject.GetMyWorldId(), this);
		if (lifetimeRemaining <= 0f)
		{
			Delete();
		}
	}

	[OnDeserialized]
	private void OnDeserialized()
	{
		PrimaryElement component = GetComponent<PrimaryElement>();
		if (component != null)
		{
			component.Mass = 20f;
		}
	}

	public override void Sim200ms(float dt)
	{
		base.Sim200ms(dt);
		if (lifetimeRemaining > 0f)
		{
			AddPower(dt * 60f);
			lifetimeRemaining -= dt;
		}
		else
		{
			Explode();
		}
	}

	public override void Explode()
	{
		Game.Instance.SpawnFX(SpawnFXHashes.MeteorImpactMetal, base.gameObject.transform.position, 0f);
		KFMOD.PlayOneShot(GlobalAssets.GetSound("Battery_explode"), base.gameObject.transform.position);
		LaunchNearbyStuff();
		SimMessages.AddRemoveSubstance(Grid.PosToCell(base.transform.position), SimHashes.NuclearWaste, CellEventLogger.Instance.ElementEmitted, 20f, 3000f, Db.Get().Diseases.GetIndex(Db.Get().Diseases.RadiationPoisoning.Id), Mathf.RoundToInt(10000000f));
		if (base.transform.parent != null)
		{
			Storage component = base.transform.parent.GetComponent<Storage>();
			if (component != null)
			{
				Health component2 = component.GetComponent<Health>();
				if (component2 != null)
				{
					component2.Damage(500f);
				}
			}
		}
		Delete();
	}

	private void Delete()
	{
		if (!this.IsNullOrDestroyed() && !base.gameObject.IsNullOrDestroyed())
		{
			base.gameObject.DeleteObject();
		}
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		Components.SelfChargingElectrobanks.Remove(base.gameObject.GetMyWorldId(), this);
	}
}
