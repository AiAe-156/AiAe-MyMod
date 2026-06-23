using System;

public class NonEssentialEnergyConsumer : EnergyConsumer
{
	public Action<bool> PoweredStateChanged;

	private bool isPowered = false;

	public override bool IsPowered
	{
		get
		{
			return isPowered;
		}
		protected set
		{
			if (value != isPowered)
			{
				isPowered = value;
				PoweredStateChanged?.Invoke(isPowered);
			}
		}
	}
}
