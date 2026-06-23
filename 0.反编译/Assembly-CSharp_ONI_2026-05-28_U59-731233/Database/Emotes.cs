using Klei.AI;

namespace Database;

public class Emotes : ResourceSet<Resource>
{
	public class MinionEmotes : ResourceSet<Emote>
	{
		private static EmoteStep[] DEFAULT_STEPS = new EmoteStep[1]
		{
			new EmoteStep
			{
				anim = "react"
			}
		};

		private static EmoteStep[] DEFAULT_IDLE_STEPS = new EmoteStep[3]
		{
			new EmoteStep
			{
				anim = "idle_pre"
			},
			new EmoteStep
			{
				anim = "idle_default"
			},
			new EmoteStep
			{
				anim = "idle_pst"
			}
		};

		public Emote ClapCheer = null;

		public Emote Cheer = null;

		public Emote ProductiveCheer = null;

		public Emote ResearchComplete = null;

		public Emote ThumbsUp = null;

		public Emote CloseCall_Fall = null;

		public Emote Cold = null;

		public Emote Cough = null;

		public Emote Cough_Small = null;

		public Emote FoodPoisoning = null;

		public Emote Hot = null;

		public Emote IritatedEyes = null;

		public Emote MorningStretch = null;

		public Emote Radiation_Glare = null;

		public Emote Radiation_Itch = null;

		public Emote Sick = null;

		public Emote Sneeze = null;

		public Emote WaterDamage = null;

		public Emote Sneeze_Short = null;

		public Emote GrindingGears = null;

		public Emote Concern = null;

		public Emote Cringe = null;

		public Emote Disappointed = null;

		public Emote Shock = null;

		public Emote Sing = null;

		public Emote FingerGuns = null;

		public Emote Wave = null;

		public Emote Wave_Shy = null;

		public MinionEmotes(ResourceSet parent)
			: base("Minion", parent)
		{
			InitializeCelebrations();
			InitializePhysicalStatus();
			InitializeEmotionalStatus();
			InitializeGreetings();
		}

		public void InitializeCelebrations()
		{
			ClapCheer = new Emote(this, "ClapCheer", new EmoteStep[3]
			{
				new EmoteStep
				{
					anim = "clapcheer_pre"
				},
				new EmoteStep
				{
					anim = "clapcheer_loop"
				},
				new EmoteStep
				{
					anim = "clapcheer_pst"
				}
			}, "anim_clapcheer_kanim", "anim_clapcheer_swim_kanim");
			Cheer = new Emote(this, "Cheer", new EmoteStep[3]
			{
				new EmoteStep
				{
					anim = "cheer_pre"
				},
				new EmoteStep
				{
					anim = "cheer_loop"
				},
				new EmoteStep
				{
					anim = "cheer_pst"
				}
			}, "anim_cheer_kanim");
			ProductiveCheer = new Emote(this, "Productive Cheer", new EmoteStep[1]
			{
				new EmoteStep
				{
					anim = "productive"
				}
			}, "anim_productive_kanim");
			ResearchComplete = new Emote(this, "ResearchComplete", DEFAULT_STEPS, "anim_react_research_complete_kanim");
			ThumbsUp = new Emote(this, "ThumbsUp", DEFAULT_STEPS, "anim_react_thumbsup_kanim");
		}

		private void InitializePhysicalStatus()
		{
			CloseCall_Fall = new Emote(this, "Near Fall", DEFAULT_STEPS, "anim_react_floor_missing_kanim");
			Cold = new Emote(this, "Cold", DEFAULT_IDLE_STEPS, "anim_idle_cold_kanim");
			Cough = new Emote(this, "Cough", DEFAULT_STEPS, "anim_slimelungcough_kanim", "anim_slimelungcough_swim_kanim");
			Cough_Small = new Emote(this, "Small Cough", new EmoteStep[1]
			{
				new EmoteStep
				{
					anim = "react_small"
				}
			}, "anim_slimelungcough_kanim");
			FoodPoisoning = new Emote(this, "Food Poisoning", DEFAULT_STEPS, "anim_react_contaminated_food_kanim");
			Hot = new Emote(this, "Hot", DEFAULT_IDLE_STEPS, "anim_idle_hot_kanim");
			IritatedEyes = new Emote(this, "Irritated Eyes", new EmoteStep[1]
			{
				new EmoteStep
				{
					anim = "irritated_eyes"
				}
			}, "anim_irritated_eyes_kanim", "anim_irritated_eyes_swim_kanim");
			MorningStretch = new Emote(this, "Morning Stretch", DEFAULT_STEPS, "anim_react_morning_stretch_kanim", "anim_react_morning_stretch_swim_kanim");
			Radiation_Glare = new Emote(this, "Radiation Glare", new EmoteStep[1]
			{
				new EmoteStep
				{
					anim = "react_radiation_glare"
				}
			}, "anim_react_radiation_kanim");
			Radiation_Itch = new Emote(this, "Radiation Itch", new EmoteStep[1]
			{
				new EmoteStep
				{
					anim = "react_radiation_itch"
				}
			}, "anim_react_radiation_kanim");
			Sick = new Emote(this, "Sick", DEFAULT_IDLE_STEPS, "anim_idle_sick_kanim", "anim_idle_sick_swim_kanim");
			Sneeze = new Emote(this, "Sneeze", new EmoteStep[2]
			{
				new EmoteStep
				{
					anim = "sneeze"
				},
				new EmoteStep
				{
					anim = "sneeze_pst"
				}
			}, "anim_sneeze_kanim", "anim_sneeze_swim_kanim");
			WaterDamage = new Emote(this, "WaterDamage", new EmoteStep[1]
			{
				new EmoteStep
				{
					anim = "zapped"
				}
			}, "anim_bionic_kanim");
			GrindingGears = new Emote(this, "GrindingGears", new EmoteStep[1]
			{
				new EmoteStep
				{
					anim = "react"
				}
			}, "anim_bionic_react_grinding_gears_kanim");
			Sneeze_Short = new Emote(this, "Short Sneeze", new EmoteStep[2]
			{
				new EmoteStep
				{
					anim = "sneeze_short"
				},
				new EmoteStep
				{
					anim = "sneeze_short_pst"
				}
			}, "anim_sneeze_kanim");
		}

		private void InitializeEmotionalStatus()
		{
			Concern = new Emote(this, "Concern", DEFAULT_STEPS, "anim_react_concern_kanim");
			Cringe = new Emote(this, "Cringe", new EmoteStep[3]
			{
				new EmoteStep
				{
					anim = "cringe_pre"
				},
				new EmoteStep
				{
					anim = "cringe_loop"
				},
				new EmoteStep
				{
					anim = "cringe_pst"
				}
			}, "anim_cringe_kanim");
			Disappointed = new Emote(this, "Disappointed", DEFAULT_STEPS, "anim_disappointed_kanim");
			Shock = new Emote(this, "Shock", DEFAULT_STEPS, "anim_react_shock_kanim");
			Sing = new Emote(this, "Sing", DEFAULT_STEPS, "anim_react_singer_kanim");
		}

		private void InitializeGreetings()
		{
			FingerGuns = new Emote(this, "Finger Guns", DEFAULT_STEPS, "anim_react_fingerguns_kanim");
			Wave = new Emote(this, "Wave", DEFAULT_STEPS, "anim_react_wave_kanim");
			Wave_Shy = new Emote(this, "Shy Wave", DEFAULT_STEPS, "anim_react_wave_shy_kanim");
		}
	}

	public class CritterEmotes : ResourceSet<Emote>
	{
		public Emote Positive = null;

		public Emote Negative = null;

		public Emote Roar = null;

		public Emote RaptorSignal = null;

		public CritterEmotes(ResourceSet parent)
			: base("Critter", parent)
		{
			InitializeEmotes();
		}

		private void InitializeEmotes()
		{
			Positive = new Emote(this, "Positive", new EmoteStep[1]
			{
				new EmoteStep
				{
					anim = "react_pos"
				}
			});
			Negative = new Emote(this, "Negative", new EmoteStep[1]
			{
				new EmoteStep
				{
					anim = "react_neg"
				}
			});
			Roar = new Emote(this, "Roar", new EmoteStep[1]
			{
				new EmoteStep
				{
					anim = "roar"
				}
			});
			RaptorSignal = new Emote(this, "Signal", new EmoteStep[1]
			{
				new EmoteStep
				{
					anim = "signal"
				}
			});
		}
	}

	public MinionEmotes Minion = null;

	public CritterEmotes Critter = null;

	public Emotes(ResourceSet parent)
		: base("Emotes", parent)
	{
		Minion = new MinionEmotes(this);
		Critter = new CritterEmotes(this);
	}

	public void ResetProblematicReferences()
	{
		for (int i = 0; i < Minion.resources.Count; i++)
		{
			Emote emote = Minion.resources[i];
			for (int j = 0; j < emote.StepCount; j++)
			{
				emote[j].UnregisterAllCallbacks();
			}
		}
		for (int k = 0; k < Critter.resources.Count; k++)
		{
			Emote emote2 = Critter.resources[k];
			for (int l = 0; l < emote2.StepCount; l++)
			{
				emote2[l].UnregisterAllCallbacks();
			}
		}
	}
}
