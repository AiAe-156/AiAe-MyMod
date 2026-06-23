using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/ElectrobankTracker")]
public class ElectrobankTracker : WorldResourceAmountTracker<ElectrobankTracker>, ISaveLoadable
{
	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		ignoredTags = new Tag[GameTags.BionicIncompatibleBatteries.Count];
		GameTags.BionicIncompatibleBatteries.CopyTo(ignoredTags, 0);
		itemTag = GameTags.ChargedPortableBattery;
	}

	protected override ItemData GetItemData(Pickupable item)
	{
		Electrobank component = item.GetComponent<Electrobank>();
		return new ItemData
		{
			ID = component.ID,
			amountValue = component.Charge * item.PrimaryElement.Units,
			units = item.PrimaryElement.Units
		};
	}
}
