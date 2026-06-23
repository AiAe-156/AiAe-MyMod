namespace Database;

public class CritterEmotions : ResourceSet<Thought>
{
	public CritterEmotion Hungry;

	public CritterEmotion Hot;

	public CritterEmotion Cold;

	public CritterEmotion Cramped;

	public CritterEmotion Crowded;

	public CritterEmotion Suffocating;

	public CritterEmotion WellFed;

	public CritterEmotion Happy;

	public CritterEmotions(ResourceSet parent)
		: base("CritterEmotions", parent)
	{
		Hungry = new CritterEmotion("Hungry", isPositiveEmotion: false, Assets.GetSprite("crew_state_hungry"));
		Hot = new CritterEmotion("Hot", isPositiveEmotion: false, Assets.GetSprite("crew_state_temp_up"));
		Cold = new CritterEmotion("Cold", isPositiveEmotion: false, Assets.GetSprite("crew_state_temp_down"));
		Cramped = new CritterEmotion("Cramped", isPositiveEmotion: false, Assets.GetSprite("crew_state_stress"));
		Crowded = new CritterEmotion("Crowded", isPositiveEmotion: false, Assets.GetSprite("crew_state_stress"));
		Suffocating = new CritterEmotion("Suffocating", isPositiveEmotion: false, Assets.GetSprite("crew_state_cantbreathe"));
		WellFed = new CritterEmotion("WellFed", isPositiveEmotion: true, Assets.GetSprite("crew_state_binge_eat"));
		Happy = new CritterEmotion("Happy", isPositiveEmotion: true, Assets.GetSprite("crew_state_happy"));
	}
}
