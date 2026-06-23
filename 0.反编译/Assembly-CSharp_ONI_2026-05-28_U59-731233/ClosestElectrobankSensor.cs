using System.Collections.Generic;

public class ClosestElectrobankSensor : ClosestPickupableSensor<Electrobank>
{
	private Tag[] bionicIncompatiobleElectrobankTags;

	public ClosestElectrobankSensor(Sensors sensors, bool shouldStartActive)
		: base(sensors, GameTags.ChargedPortableBattery, shouldStartActive)
	{
		bionicIncompatiobleElectrobankTags = new Tag[GameTags.BionicIncompatibleBatteries.Count];
		GameTags.BionicIncompatibleBatteries.CopyTo(bionicIncompatiobleElectrobankTags, 0);
	}

	public override HashSet<Tag> GetForbbidenTags()
	{
		HashSet<Tag> forbbidenTags = base.GetForbbidenTags();
		if (bionicIncompatiobleElectrobankTags != null && bionicIncompatiobleElectrobankTags.Length != 0)
		{
			HashSet<Tag> hashSet = forbbidenTags;
			Tag[] array = bionicIncompatiobleElectrobankTags;
			foreach (Tag tag in array)
			{
				if (!forbbidenTags.Contains(tag))
				{
					hashSet.Add(tag);
				}
			}
			return hashSet;
		}
		return forbbidenTags;
	}
}
