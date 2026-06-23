using System.Collections.Generic;

public class ClosestEdibleSensor : Sensor
{
	private Edible edible;

	private bool hasEdible;

	public bool edibleInReachButNotPermitted;

	public static Tag[] requiredSearchTags = new Tag[1] { GameTags.Edible };

	public ClosestEdibleSensor(Sensors sensors)
		: base(sensors)
	{
	}

	public override void Update()
	{
		HashSet<Tag> forbiddenTagSet = GetComponent<ConsumableConsumer>().forbiddenTagSet;
		Pickupable pickupable = Game.Instance.fetchManager.FindEdibleFetchTarget(GetComponent<Storage>(), forbiddenTagSet, requiredSearchTags);
		bool flag = edibleInReachButNotPermitted;
		Edible edible = null;
		bool flag2 = false;
		if (pickupable != null)
		{
			edible = pickupable.GetComponent<Edible>();
			flag2 = true;
			flag = false;
		}
		else
		{
			flag = Game.Instance.fetchManager.FindEdibleFetchTarget(GetComponent<Storage>(), new HashSet<Tag>(), requiredSearchTags) != null;
		}
		if (edible != this.edible || hasEdible != flag2)
		{
			this.edible = edible;
			hasEdible = flag2;
			edibleInReachButNotPermitted = flag;
			Trigger(86328522, this.edible);
		}
	}

	public Edible GetEdible()
	{
		return edible;
	}
}
