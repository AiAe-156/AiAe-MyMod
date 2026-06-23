using TUNING;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/RobotExhaustPipe")]
public class RobotExhaustPipe : KMonoBehaviour, ISim4000ms
{
	private float CO2_RATE = DUPLICANTSTATS.STANDARD.BaseStats.OXYGEN_USED_PER_SECOND * DUPLICANTSTATS.STANDARD.BaseStats.OXYGEN_TO_CO2_CONVERSION / 2f;

	public void Sim4000ms(float dt)
	{
		Facing component = GetComponent<Facing>();
		bool flip = false;
		if ((bool)component)
		{
			flip = component.GetFacing();
		}
		CO2Manager.instance.SpawnBreath(Grid.CellToPos(Grid.PosToCell(base.gameObject)), dt * CO2_RATE, 303.15f, flip);
	}
}
