using UnityEngine;

public class UnderwaterSoundEvent : SoundEvent
{
	public const string POSTFIX = "_uw";

	private readonly string underwaterSound;

	public UnderwaterSoundEvent(string file_name, string sound_name, int frame, bool do_load, bool is_looping, float min_interval, bool is_dynamic)
		: base(file_name, sound_name, frame, do_load, is_looping, min_interval, is_dynamic)
	{
		underwaterSound = StringFormatter.Combine(base.sound, "_uw");
	}

	public static bool IsVisiblyInLiquid(Vector3 position)
	{
		int num = Grid.PosToCell(new Vector2(position.x, position.y - 0.05f));
		if (!Grid.IsValidCell(num))
		{
			return false;
		}
		if (!Grid.IsLiquid(num))
		{
			return false;
		}
		int cell = Grid.CellAbove(num);
		if (Grid.IsValidCell(cell) && Grid.IsLiquid(cell))
		{
			return true;
		}
		float num2 = Grid.Mass[num];
		float num3 = position.y - (float)(int)position.y;
		return num2 / 1000f >= num3;
	}

	private bool TryResolveMultitool(AnimEventManager.EventPlayerData behaviour, out string result)
	{
		KBatchedAnimEventToggler componentInParent = behaviour.controller.GetComponentInParent<KBatchedAnimEventToggler>();
		if (componentInParent != null && componentInParent.gameObject.name == "LaserEffect")
		{
			bool flag = IsVisiblyInLiquid(behaviour.controller.transform.GetPosition());
			result = (flag ? underwaterSound : base.sound);
			return true;
		}
		result = null;
		return false;
	}

	private string Resolve(AnimEventManager.EventPlayerData behaviour)
	{
		if (TryResolveMultitool(behaviour, out var result))
		{
			return result;
		}
		int cell = Grid.PosToCell(behaviour.position);
		if (Grid.IsNavigatableLiquid(cell))
		{
			return underwaterSound;
		}
		if (!behaviour.controller.transform.root.TryGetComponent<Navigator>(out var component))
		{
			return base.sound;
		}
		return (component.CurrentNavType == NavType.Swim) ? underwaterSound : base.sound;
	}

	public override void PlaySound(AnimEventManager.EventPlayerData behaviour)
	{
		PlaySound(behaviour, Resolve(behaviour));
	}

	public override void Stop(AnimEventManager.EventPlayerData behaviour)
	{
		if (base.looping && behaviour.controller.TryGetComponent<LoopingSounds>(out var component))
		{
			component.StopSound(base.sound);
			component.StopSound(underwaterSound);
		}
	}
}
