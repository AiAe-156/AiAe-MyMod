public class ReefGeneratorPower : Generator
{
	public override void EnergySim200ms(float dt)
	{
		base.EnergySim200ms(dt);
		ushort circuitID = base.CircuitID;
		operational.SetFlag(Generator.wireConnectedFlag, circuitID != ushort.MaxValue);
		bool isOperational = operational.IsOperational;
		if (isOperational)
		{
			GenerateJoules(base.WattageRating * dt);
		}
		operational.SetActive(isOperational);
	}
}
