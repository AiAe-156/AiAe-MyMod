using System.Collections.Generic;
using FMOD.Studio;

internal class UpdatePercentCompleteParameter : LoopingSoundParameterUpdater
{
	private struct Entry
	{
		public WorkerBase worker;

		public EventInstance ev;

		public PARAMETER_ID parameterId;
	}

	private List<Entry> entries = new List<Entry>();

	public UpdatePercentCompleteParameter()
		: base("percentComplete")
	{
	}

	public override void Add(Sound sound)
	{
		Entry item = new Entry
		{
			worker = sound.transform.GetComponent<WorkerBase>(),
			ev = sound.ev,
			parameterId = sound.description.GetParameterId(base.parameter)
		};
		entries.Add(item);
	}

	public override void Update(float dt)
	{
		foreach (Entry entry in entries)
		{
			if (!(entry.worker == null))
			{
				Workable workable = entry.worker.GetWorkable();
				if (!(workable == null))
				{
					float percentComplete = workable.GetPercentComplete();
					EventInstance ev = entry.ev;
					ev.setParameterByID(entry.parameterId, percentComplete);
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
