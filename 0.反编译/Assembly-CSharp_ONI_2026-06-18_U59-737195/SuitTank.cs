using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/SuitTank")]
public class SuitTank : KMonoBehaviour, IGameObjectEffectDescriptor, OxygenBreather.IGasProvider
{
	public SafeCellQuery.SafeFlags SafeCellFlagsToIgnoreOnEquipped = (SafeCellQuery.SafeFlags)464;

	[Serialize]
	public string element;

	[Serialize]
	public float amount;

	public Tag elementTag;

	[MyCmpReq]
	public Storage storage;

	public float capacity;

	public const float REFILL_PERCENT = 0.25f;

	public bool underwaterSupport;

	private Equippable equippable;

	private static readonly EventSystem.IntraObjectHandler<SuitTank> OnEquippedDelegate = new EventSystem.IntraObjectHandler<SuitTank>(delegate(SuitTank component, object data)
	{
		component.OnEquipped(data);
	});

	private static readonly EventSystem.IntraObjectHandler<SuitTank> OnUnequippedDelegate = new EventSystem.IntraObjectHandler<SuitTank>(delegate(SuitTank component, object data)
	{
		component.OnUnequipped(data);
	});

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Subscribe(-1617557748, OnEquippedDelegate);
		Subscribe(-170173755, OnUnequippedDelegate);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (amount != 0f)
		{
			storage.AddGasChunk(SimHashes.Oxygen, amount, GetComponent<PrimaryElement>().Temperature, byte.MaxValue, 0, keep_zero_mass: false);
			amount = 0f;
		}
		equippable = GetComponent<Equippable>();
	}

	public float GetTankAmount()
	{
		if (storage == null)
		{
			storage = GetComponent<Storage>();
		}
		return storage.GetMassAvailable(elementTag);
	}

	public float PercentFull()
	{
		return GetTankAmount() / capacity;
	}

	public bool IsEmpty()
	{
		return GetTankAmount() <= 0f;
	}

	public bool IsFull()
	{
		return PercentFull() >= 1f;
	}

	public bool NeedsRecharging()
	{
		return PercentFull() < 0.25f;
	}

	public List<Descriptor> GetDescriptors(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor>();
		if (elementTag == GameTags.Breathable)
		{
			string text = (underwaterSupport ? string.Format(UI.UISIDESCREENS.FABRICATORSIDESCREEN.EFFECTS.OXYGEN_TANK_UNDERWATER, GameUtil.GetFormattedMass(GetTankAmount())) : string.Format(UI.UISIDESCREENS.FABRICATORSIDESCREEN.EFFECTS.OXYGEN_TANK, GameUtil.GetFormattedMass(GetTankAmount())));
			list.Add(new Descriptor(text, text));
		}
		return list;
	}

	private void OnEquipped(object data)
	{
		Equipment equipment = (Equipment)data;
		NameDisplayScreen.Instance.SetSuitTankDisplay(equipment.GetComponent<MinionAssignablesProxy>().GetTargetGameObject(), PercentFull, bVisible: true);
		GameObject targetGameObject = equipment.GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
		OxygenBreather component = targetGameObject.GetComponent<OxygenBreather>();
		if (component != null)
		{
			component.GetComponent<Sensors>().GetSensor<SafeCellSensor>().AddIgnoredFlagsSet("SuitTank", SafeCellFlagsToIgnoreOnEquipped);
			component.AddGasProvider(this);
		}
		targetGameObject.AddTag(GameTags.HasSuitTank);
	}

	private void OnUnequipped(object data)
	{
		Equipment equipment = (Equipment)data;
		if (!equipment.destroyed)
		{
			NameDisplayScreen.Instance.SetSuitTankDisplay(equipment.GetComponent<MinionAssignablesProxy>().GetTargetGameObject(), PercentFull, bVisible: false);
			GameObject targetGameObject = equipment.GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
			OxygenBreather component = targetGameObject.GetComponent<OxygenBreather>();
			if (component != null)
			{
				component.GetComponent<Sensors>().GetSensor<SafeCellSensor>().RemoveIgnoredFlagsSet("SuitTank");
				component.RemoveGasProvider(this);
			}
			targetGameObject.RemoveTag(GameTags.HasSuitTank);
		}
	}

	public void OnSetOxygenBreather(OxygenBreather oxygen_breather)
	{
	}

	public void OnClearOxygenBreather(OxygenBreather oxygen_breather)
	{
	}

	public bool ConsumeGas(OxygenBreather oxygen_breather, float amount)
	{
		if (IsEmpty())
		{
			return false;
		}
		float aggregate_temperature = 0f;
		SimHashes mostRelevantItemElement = SimHashes.Vacuum;
		storage.ConsumeAndGetDisease(elementTag, amount, out var amount_consumed, out var disease_info, out aggregate_temperature, out mostRelevantItemElement);
		OxygenBreather.BreathableGasConsumed(oxygen_breather, mostRelevantItemElement, amount_consumed, aggregate_temperature, disease_info.idx, disease_info.count);
		Trigger(608245985, (object)base.gameObject);
		return true;
	}

	public bool ShouldEmitCO2()
	{
		bool flag = GetComponent<KPrefabID>().HasTag(GameTags.AirtightSuit);
		if (flag)
		{
			return false;
		}
		bool flag2 = IsOwnerBionic();
		if (!flag)
		{
			return !flag2;
		}
		return false;
	}

	public bool ShouldStoreCO2()
	{
		bool flag = GetComponent<KPrefabID>().HasTag(GameTags.AirtightSuit);
		if (!flag)
		{
			return false;
		}
		bool flag2 = IsOwnerBionic();
		if (flag)
		{
			return !flag2;
		}
		return false;
	}

	public bool IsOwnerBionic()
	{
		bool result = false;
		if (equippable != null && equippable.IsAssigned() && equippable.isEquipped)
		{
			Ownables soleOwner = equippable.assignee.GetSoleOwner();
			if (soleOwner != null)
			{
				GameObject targetGameObject = soleOwner.GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
				if ((bool)targetGameObject)
				{
					result = targetGameObject.PrefabID() == BionicMinionConfig.ID;
				}
			}
		}
		return result;
	}

	public bool IsLowOxygen()
	{
		return NeedsRecharging();
	}

	[ContextMenu("SetToRefillAmount")]
	public void SetToRefillAmount()
	{
		float tankAmount = GetTankAmount();
		float num = 0.25f * capacity;
		if (tankAmount > num)
		{
			storage.ConsumeIgnoringDisease(elementTag, tankAmount - num);
		}
	}

	[ContextMenu("Empty")]
	public void Empty()
	{
		storage.ConsumeIgnoringDisease(elementTag, GetTankAmount());
	}

	[ContextMenu("Fill Tank")]
	public void FillTank()
	{
		Empty();
		storage.AddGasChunk(SimHashes.Oxygen, capacity, 15f, 0, 0, keep_zero_mass: false, do_disease_transfer: false);
	}

	public bool HasOxygen()
	{
		return !IsEmpty();
	}

	public bool IsBlocked()
	{
		return false;
	}
}
