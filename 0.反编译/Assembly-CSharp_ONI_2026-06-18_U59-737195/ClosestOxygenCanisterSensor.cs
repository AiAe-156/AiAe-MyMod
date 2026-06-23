using System.Collections.Generic;

public class ClosestOxygenCanisterSensor : ClosestPickupableSensor<Pickupable>
{
	public static readonly Tag GenericBreathableGassesTankTag = new Tag("BreathableGasTank");

	private List<Element> BreathableGasses;

	public ClosestOxygenCanisterSensor(Sensors sensors, bool shouldStartActive)
		: base(sensors, GameTags.Gas, shouldStartActive)
	{
		requiredTags = new Tag[1] { GameTags.Breathable };
		BreathableGasses = ElementLoader.FindElements((Element element) => element.HasTag(GameTags.Breathable) && element.HasTag(GameTags.Gas));
	}

	public override HashSet<Tag> GetForbbidenTags()
	{
		if (consumableConsumer == null)
		{
			return new HashSet<Tag>(0);
		}
		HashSet<Tag> forbbidenTags = base.GetForbbidenTags();
		if (forbbidenTags == null || forbbidenTags.Count <= 0)
		{
			return forbbidenTags;
		}
		Tag[] array = new Tag[forbbidenTags.Count];
		base.GetForbbidenTags().CopyTo(array);
		HashSet<Tag> hashSet = new HashSet<Tag>();
		foreach (Tag tag in array)
		{
			if (tag == GenericBreathableGassesTankTag)
			{
				foreach (Element breathableGass in BreathableGasses)
				{
					hashSet.Add(breathableGass.id.ToString());
				}
			}
			else
			{
				hashSet.Add(tag);
			}
		}
		return hashSet;
	}
}
