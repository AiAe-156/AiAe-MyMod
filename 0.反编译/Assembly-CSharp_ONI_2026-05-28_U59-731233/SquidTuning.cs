using System.Collections.Generic;
using TUNING;

public static class SquidTuning
{
	public const float LIFESPAWN = 100f;

	public static List<FertilityMonitor.BreedingChance> EGG_CHANCES_BASE = new List<FertilityMonitor.BreedingChance>
	{
		new FertilityMonitor.BreedingChance
		{
			egg = "SquidEgg".ToTag(),
			weight = 1f
		}
	};

	public static float STANDARD_STARVE_CYCLES = 5f;

	public static float STANDARD_CALORIES_PER_CYCLE = 100000f;

	public static float STANDARD_STOMACH_SIZE = STANDARD_CALORIES_PER_CYCLE * STANDARD_STARVE_CYCLES;

	public static readonly float WELLFED_CALORIES_PER_CYCLE = STANDARD_CALORIES_PER_CYCLE * 0.9f;

	public static int PEN_SIZE_PER_CREATURE = CREATURES.SPACE_REQUIREMENTS.TIER3;

	public const float POOP_MASS_KG = 80f;

	public static Tag POOP_ELEMENT = SimHashes.Katairite.CreateTag();

	public static float EGG_MASS = 2f;

	public static float TUBE_WORM_PLANTS_PER_SQUID = 2f;

	public static float TUBE_WORM_GROWTH_EATEN_PER_CYCLE = 0.125f * TUBE_WORM_PLANTS_PER_SQUID;

	public static float CALORIES_PER_GROWTH_EATEN = STANDARD_CALORIES_PER_CYCLE / (TUBE_WORM_GROWTH_EATEN_PER_CYCLE * 8f);

	public static float GROWTH_TO_PRODUCT_EFFICIENCY = 10f;

	public static readonly float INK_PER_CYCLE = 50f;

	private static readonly float CYCLES_UNTIL_MILKING = 4f;

	public const float INKPERATTACK = 5f;

	public static readonly float INK_CAPACITY = INK_PER_CYCLE * CYCLES_UNTIL_MILKING;

	public static readonly float INK_AMOUNT_AT_MILKING = INK_PER_CYCLE * CYCLES_UNTIL_MILKING;

	public static readonly float INK_PRODUCTION_PERCENTAGE_PER_SECOND = 100f / (600f * CYCLES_UNTIL_MILKING);
}
