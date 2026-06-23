using System.Collections.Generic;
using FMOD.Studio;

internal class UpdateRocketLandingParameter : LoopingSoundParameterUpdater
{
	private struct Entry
	{
		public RocketModule rocketModule;

		public EventInstance ev;

		public PARAMETER_ID parameterId;
	}

	private List<Entry> entries = new List<Entry>();

	public UpdateRocketLandingParameter()
		: base("rocketLanding")
	{
	}

	public override void Add(Sound sound)
	{
		Entry item = new Entry
		{
			rocketModule = sound.transform.GetComponent<RocketModule>(),
			ev = sound.ev,
			parameterId = sound.description.GetParameterId(base.parameter)
		};
		entries.Add(item);
	}

	public override void Update(float dt)
	{
		foreach (Entry entry in entries)
		{
			if (entry.rocketModule == null)
			{
				continue;
			}
			LaunchConditionManager conditionManager = entry.rocketModule.conditionManager;
			if (conditionManager == null)
			{
				continue;
			}
			ILaunchableRocket component = conditionManager.GetComponent<ILaunchableRocket>();
			if (component != null)
			{
				if (component.isLanding)
				{
					EventInstance ev = entry.ev;
					ev.setParameterByID(entry.parameterId, 1f);
				}
				else
				{
					EventInstance ev = entry.ev;
					ev.setParameterByID(entry.parameterId, 0f);
				}
			}
		}
	}

	public override void Remove(Sound sound)
	{
		for (int i = 0; i < entries.Count; i++)
		{
			if (entries[i].ev.handle == sound.ev.handle)
			{
				entries.RemoveAt(i);
				break;
			}
		}
	}
}
