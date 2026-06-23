using System;
using UnityEngine;

[Serializable]
public class AnimEvent
{
	[SerializeField]
	private KAnimHashedString fileHash;

	public bool OnExit;

	public string name { get; private set; }

	public string file { get; private set; }

	public int frame { get; private set; }

	public AnimEvent()
	{
	}

	public AnimEvent(string file, string name, int frame)
	{
		this.file = ((file == "") ? null : file);
		if (this.file != null)
		{
			fileHash = new KAnimHashedString(this.file);
		}
		this.name = name;
		this.frame = frame;
	}

	public void Play(AnimEventManager.EventPlayerData behaviour)
	{
		if (behaviour.previousFrame < behaviour.currentFrame)
		{
			if (behaviour.previousFrame < frame && behaviour.currentFrame >= frame)
			{
				OnPlay(behaviour);
			}
		}
		else if (behaviour.previousFrame > behaviour.currentFrame && (behaviour.previousFrame < frame || frame <= behaviour.currentFrame))
		{
			OnPlay(behaviour);
		}
	}

	private void DebugAnimEvent(string ev_name, AnimEventManager.EventPlayerData behaviour)
	{
	}

	public virtual void OnPlay(AnimEventManager.EventPlayerData behaviour)
	{
	}

	public virtual void OnUpdate(AnimEventManager.EventPlayerData behaviour)
	{
	}

	public virtual void Stop(AnimEventManager.EventPlayerData behaviour)
	{
	}
}
