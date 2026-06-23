public class CreatureBrain : Brain
{
	public string symbolPrefix;

	public Tag species;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Navigator component = GetComponent<Navigator>();
		if (component != null)
		{
			if (GetComponent<KPrefabID>().HasTag(GameTags.Robots.Behaviours.HasDoorPermissions))
			{
				component.SetAbilities(new RobotPathFinderAbilities(component));
			}
			else
			{
				component.SetAbilities(new CreaturePathFinderAbilities(component));
			}
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.onPreUpdate += delegate
		{
			Navigator component = GetComponent<Navigator>();
			if (component != null)
			{
				component.UpdateProbe();
			}
		};
	}
}
