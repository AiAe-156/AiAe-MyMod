using System.Collections.Generic;
using Klei.AI;
using TUNING;
using UnityEngine;

public class EquippableBalloonConfig : IEquipmentConfig
{
	public const string ID = "EquippableBalloon";

	public EquipmentDef CreateEquipmentDef()
	{
		List<AttributeModifier> attributeModifiers = new List<AttributeModifier>();
		EquipmentDef equipmentDef = EquipmentTemplates.CreateEquipmentDef("EquippableBalloon", EQUIPMENT.TOYS.SLOT, SimHashes.Carbon, EQUIPMENT.TOYS.BALLOON_MASS, EQUIPMENT.VESTS.WARM_VEST_ICON0, null, null, 0, attributeModifiers, null, IsBody: false, EntityTemplates.CollisionShape.RECTANGLE, 0.75f, 0.4f, new Tag[1] { GameTags.Clothes });
		equipmentDef.OnEquipCallBack = OnEquipBalloon;
		equipmentDef.OnUnequipCallBack = OnUnequipBalloon;
		return equipmentDef;
	}

	private void OnEquipBalloon(Equippable eq)
	{
		if (eq.IsNullOrDestroyed() || eq.assignee.IsNullOrDestroyed())
		{
			return;
		}
		Ownables soleOwner = eq.assignee.GetSoleOwner();
		if (!soleOwner.IsNullOrDestroyed())
		{
			MinionAssignablesProxy component = soleOwner.GetComponent<MinionAssignablesProxy>();
			KMonoBehaviour kMonoBehaviour = (KMonoBehaviour)component.target;
			Effects component2 = kMonoBehaviour.GetComponent<Effects>();
			KSelectable component3 = kMonoBehaviour.GetComponent<KSelectable>();
			if (!component2.IsNullOrDestroyed())
			{
				component2.Add("HasBalloon", should_save: false);
				EquippableBalloon component4 = eq.GetComponent<EquippableBalloon>();
				EquippableBalloon.StatesInstance data = (EquippableBalloon.StatesInstance)component4.GetSMI();
				component3.AddStatusItem(Db.Get().DuplicantStatusItems.JoyResponse_HasBalloon, data);
				SpawnFxInstanceFor(kMonoBehaviour);
				component4.ApplyBalloonOverrideToBalloonFx();
			}
		}
	}

	private void OnUnequipBalloon(Equippable eq)
	{
		if (!eq.IsNullOrDestroyed() && !eq.assignee.IsNullOrDestroyed())
		{
			Ownables soleOwner = eq.assignee.GetSoleOwner();
			if (soleOwner.IsNullOrDestroyed())
			{
				return;
			}
			MinionAssignablesProxy component = soleOwner.GetComponent<MinionAssignablesProxy>();
			if (!component.target.IsNullOrDestroyed())
			{
				KMonoBehaviour kMonoBehaviour = (KMonoBehaviour)component.target;
				Effects component2 = kMonoBehaviour.GetComponent<Effects>();
				KSelectable component3 = kMonoBehaviour.GetComponent<KSelectable>();
				if (!component2.IsNullOrDestroyed())
				{
					component2.Remove("HasBalloon");
					component3.RemoveStatusItem(Db.Get().DuplicantStatusItems.JoyResponse_HasBalloon);
					DestroyFxInstanceFor(kMonoBehaviour);
				}
			}
		}
		Util.KDestroyGameObject(eq.gameObject);
	}

	public void DoPostConfigure(GameObject go)
	{
		Equippable equippable = go.GetComponent<Equippable>();
		if (equippable.IsNullOrDestroyed())
		{
			equippable = go.AddComponent<Equippable>();
		}
		equippable.hideInCodex = true;
		equippable.unequippable = false;
		go.AddOrGet<EquippableBalloon>();
	}

	private void SpawnFxInstanceFor(KMonoBehaviour target)
	{
		BalloonFX.Instance instance = new BalloonFX.Instance(target.GetComponent<KMonoBehaviour>());
		instance.StartSM();
	}

	private void DestroyFxInstanceFor(KMonoBehaviour target)
	{
		BalloonFX.Instance sMI = target.GetSMI<BalloonFX.Instance>();
		sMI.StopSM("Unequipped");
	}
}
