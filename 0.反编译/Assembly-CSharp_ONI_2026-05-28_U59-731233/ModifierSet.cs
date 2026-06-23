using System;
using System.Collections.Generic;
using Database;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class ModifierSet : ScriptableObject
{
	public class ModifierInfo : Resource
	{
		public string Type;

		public string Attribute;

		public float Value;

		public Units Units;

		public bool Multiplier;

		public float Duration;

		public bool ShowInUI;

		public string StompGroup;

		public int StompPriority;

		public bool IsBad;

		public string CustomIcon;

		public bool TriggerFloatingText;

		public string EmoteAnim;

		public float EmoteCooldown;
	}

	[Serializable]
	public class ModifierInfos : ResourceLoader<ModifierInfo>
	{
	}

	[Serializable]
	public class TraitSet : ResourceSet<Trait>
	{
	}

	[Serializable]
	public class TraitGroupSet : ResourceSet<TraitGroup>
	{
	}

	public TextAsset modifiersFile;

	public ModifierInfos modifierInfos;

	public TraitSet traits;

	public ResourceSet<Effect> effects;

	public TraitGroupSet traitGroups;

	public FertilityModifiers FertilityModifiers;

	public MooSongModifiers MooSongModifiers;

	public Database.Attributes Attributes;

	public BuildingAttributes BuildingAttributes;

	public CritterAttributes CritterAttributes;

	public PlantAttributes PlantAttributes;

	public Database.Amounts Amounts;

	public Database.AttributeConverters AttributeConverters;

	public ResourceSet Root;

	public List<Resource> ResourceTable;

	public virtual void Initialize()
	{
		ResourceTable = new List<Resource>();
		Root = new ResourceSet<Resource>("Root", null);
		modifierInfos = new ModifierInfos();
		modifierInfos.Load(modifiersFile);
		Attributes = new Database.Attributes(Root);
		BuildingAttributes = new BuildingAttributes(Root);
		CritterAttributes = new CritterAttributes(Root);
		PlantAttributes = new PlantAttributes(Root);
		effects = new ResourceSet<Effect>("Effects", Root);
		traits = new TraitSet();
		traitGroups = new TraitGroupSet();
		FertilityModifiers = new FertilityModifiers();
		MooSongModifiers = new MooSongModifiers();
		Amounts = new Database.Amounts();
		Amounts.Load();
		AttributeConverters = new Database.AttributeConverters();
		LoadEffects();
		LoadFertilityModifiers();
		LoadMooSongsModifiers();
	}

	public static float ConvertValue(float value, Units units)
	{
		if (Units.PerDay == units)
		{
			return value * 0.0016666667f;
		}
		return value;
	}

	private void LoadEffects()
	{
		foreach (ModifierInfo modifierInfo in modifierInfos)
		{
			if (effects.Exists(modifierInfo.Id) || (!(modifierInfo.Type == "Effect") && !(modifierInfo.Type == "Base") && !(modifierInfo.Type == "Need")))
			{
				continue;
			}
			string description = Strings.Get($"STRINGS.DUPLICANTS.MODIFIERS.{modifierInfo.Id.ToUpper()}.NAME");
			string description2 = Strings.Get($"STRINGS.DUPLICANTS.MODIFIERS.{modifierInfo.Id.ToUpper()}.TOOLTIP");
			Effect effect = new Effect(modifierInfo.Id, description, description2, modifierInfo.Duration * 600f, modifierInfo.ShowInUI && modifierInfo.Type != "Need", modifierInfo.TriggerFloatingText, modifierInfo.IsBad, modifierInfo.EmoteAnim, modifierInfo.EmoteCooldown, modifierInfo.StompGroup, modifierInfo.CustomIcon);
			effect.stompPriority = modifierInfo.StompPriority;
			foreach (ModifierInfo modifierInfo2 in modifierInfos)
			{
				if (modifierInfo2.Id == modifierInfo.Id)
				{
					effect.Add(new AttributeModifier(modifierInfo2.Attribute, ConvertValue(modifierInfo2.Value, modifierInfo2.Units), description, modifierInfo2.Multiplier));
				}
			}
			effects.Add(effect);
		}
		Reactable.ReactablePrecondition precon = delegate(GameObject go, Navigator.ActiveTransition n)
		{
			int cell = Grid.PosToCell(go);
			return Grid.IsValidCell(cell) && Grid.IsGas(cell);
		};
		Effect effect2 = effects.Get("WetFeet");
		effect2.AddEmotePrecondition(precon);
		Effect effect3 = effects.Get("SoakingWet");
		effect3.AddEmotePrecondition(precon);
		Effect effect4 = new Effect("PassedOutSleep", DUPLICANTS.MODIFIERS.PASSEDOUTSLEEP.NAME, DUPLICANTS.MODIFIERS.PASSEDOUTSLEEP.TOOLTIP, 0f, show_in_ui: true, trigger_floating_text: true, is_bad: true, null, 0f, null, showStatusInWorld: true, "status_item_exhausted");
		effect4.Add(new AttributeModifier(Db.Get().Amounts.Stamina.deltaAttribute.Id, 2f / 3f, DUPLICANTS.MODIFIERS.PASSEDOUTSLEEP.NAME));
		effect4.Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, -1f / 30f, DUPLICANTS.MODIFIERS.PASSEDOUTSLEEP.NAME));
		effects.Add(effect4);
		Effect resource = new Effect("WarmTouch", DUPLICANTS.MODIFIERS.WARMTOUCH.NAME, DUPLICANTS.MODIFIERS.WARMTOUCH.TOOLTIP, 120f, new string[1] { "WetFeet" }, show_in_ui: true, trigger_floating_text: true, is_bad: false, null, 0f, null, showStatusInWorld: false);
		effects.Add(resource);
		Effect resource2 = new Effect("WarmTouchFood", DUPLICANTS.MODIFIERS.WARMTOUCHFOOD.NAME, DUPLICANTS.MODIFIERS.WARMTOUCHFOOD.TOOLTIP, 600f, new string[1] { "WetFeet" }, show_in_ui: true, trigger_floating_text: true, is_bad: false, null, 0f, null, showStatusInWorld: false);
		effects.Add(resource2);
		Effect resource3 = new Effect("RefreshingTouch", DUPLICANTS.MODIFIERS.REFRESHINGTOUCH.NAME, DUPLICANTS.MODIFIERS.REFRESHINGTOUCH.TOOLTIP, 120f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effects.Add(resource3);
		Effect effect5 = new Effect("GunkSick", DUPLICANTS.MODIFIERS.GUNKSICK.NAME, DUPLICANTS.MODIFIERS.GUNKSICK.TOOLTIP, 0f, show_in_ui: true, trigger_floating_text: true, is_bad: true);
		effect5.Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 1f / 30f, DUPLICANTS.MODIFIERS.GUNKSICK.NAME));
		effects.Add(effect5);
		Effect effect6 = new Effect("ExpellingGunk", DUPLICANTS.MODIFIERS.EXPELLINGGUNK.NAME, DUPLICANTS.MODIFIERS.EXPELLINGGUNK.TOOLTIP, 0f, show_in_ui: true, trigger_floating_text: true, is_bad: true);
		effect6.Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 1f / 12f, DUPLICANTS.MODIFIERS.GUNKSICK.NAME));
		effects.Add(effect6);
		Effect effect7 = new Effect("GunkHungover", DUPLICANTS.MODIFIERS.GUNKHUNGOVER.NAME, DUPLICANTS.MODIFIERS.GUNKHUNGOVER.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: false, is_bad: true);
		effect7.Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 1f / 30f, DUPLICANTS.MODIFIERS.GUNKHUNGOVER.NAME));
		effects.Add(effect7);
		Effect effect8 = new Effect("NoLubricationMinor", DUPLICANTS.MODIFIERS.NOLUBRICATIONMINOR.NAME, DUPLICANTS.MODIFIERS.NOLUBRICATIONMINOR.TOOLTIP, 0f, show_in_ui: true, trigger_floating_text: true, is_bad: true);
		effect8.Add(new AttributeModifier(Db.Get().Attributes.Athletics.Id, -4f, DUPLICANTS.MODIFIERS.NOLUBRICATIONMINOR.NAME));
		effect8.Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 0.025f, DUPLICANTS.MODIFIERS.NOLUBRICATIONMINOR.NAME));
		effects.Add(effect8);
		Effect effect9 = new Effect("NoLubricationMajor", DUPLICANTS.MODIFIERS.NOLUBRICATIONMAJOR.NAME, DUPLICANTS.MODIFIERS.NOLUBRICATIONMAJOR.TOOLTIP, 0f, show_in_ui: true, trigger_floating_text: true, is_bad: true);
		effect9.Add(new AttributeModifier(Db.Get().Attributes.Athletics.Id, -8f, DUPLICANTS.MODIFIERS.NOLUBRICATIONMAJOR.NAME));
		effect9.Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 0.05f, DUPLICANTS.MODIFIERS.NOLUBRICATIONMINOR.NAME));
		effects.Add(effect9);
		Effect effect10 = new Effect("BionicOffline", DUPLICANTS.MODIFIERS.BIONICOFFLINE.NAME, DUPLICANTS.MODIFIERS.BIONICOFFLINE.TOOLTIP, 0f, show_in_ui: false, trigger_floating_text: true, is_bad: true);
		effect10.Add(new AttributeModifier(Db.Get().Amounts.BionicOil.deltaAttribute.Id, 0f, DUPLICANTS.MODIFIERS.BIONICOFFLINE.NAME));
		effects.Add(effect10);
		Effect effect11 = new Effect("BionicBedTimeEffect", DUPLICANTS.MODIFIERS.BIONICBEDTIMEEFFECT.NAME, DUPLICANTS.MODIFIERS.BIONICBEDTIMEEFFECT.TOOLTIP, 0f, show_in_ui: false, trigger_floating_text: false, is_bad: false);
		effect11.Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, -1f / 30f, DUPLICANTS.MODIFIERS.BIONICBEDTIMEEFFECT.NAME));
		effects.Add(effect11);
		Effect effect12 = new Effect("BionicWaterStress", DUPLICANTS.MODIFIERS.BIONICWATERSTRESS.NAME, DUPLICANTS.MODIFIERS.BIONICWATERSTRESS.TOOLTIP, 0f, show_in_ui: true, trigger_floating_text: true, is_bad: true);
		effect12.Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 1f / 3f, DUPLICANTS.MODIFIERS.BIONICWATERSTRESS.NAME));
		effects.Add(effect12);
		Effect resource4 = new Effect("RecentlySlippedTracker", DUPLICANTS.MODIFIERS.SLIPPED.NAME, DUPLICANTS.MODIFIERS.SLIPPED.TOOLTIP, 100f, show_in_ui: false, trigger_floating_text: false, is_bad: true);
		effects.Add(resource4);
		Effect effect13 = new Effect("DuplicantDrankInk", DUPLICANTS.MODIFIERS.DUPLICANTDRANKINK.NAME, DUPLICANTS.MODIFIERS.DUPLICANTDRANKINK.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect13.Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, -1f / 60f, DUPLICANTS.MODIFIERS.DUPLICANTDRANKINK.NAME));
		effect13.Add(new AttributeModifier(Db.Get().Attributes.QualityOfLife.Id, 1f, DUPLICANTS.MODIFIERS.DUPLICANTDRANKINK.NAME));
		effect13.Add(new AttributeModifier(Db.Get().Attributes.RadiationResistance.Id, 0.2f, DUPLICANTS.MODIFIERS.DUPLICANTDRANKINK.NAME));
		effects.Add(effect13);
		foreach (Effect value in BionicOilMonitor.LUBRICANT_TYPE_EFFECT.Values)
		{
			effects.Add(value);
		}
		CreateRoomEffects();
		CreateCritteEffects();
	}

	private void CreateRoomEffects()
	{
	}

	private void CreateMosquitoEffects()
	{
		Effect effect = new Effect("MosquitoFed", STRINGS.CREATURES.MODIFIERS.MOSQUITO_FED.NAME, STRINGS.CREATURES.MODIFIERS.MOSQUITO_FED.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: false, is_bad: false);
		float num = 0.4f;
		float value = 0.9f / num - 1f;
		effect.Add(new AttributeModifier(Db.Get().Amounts.Fertility.deltaAttribute.Id, value, effect.Name, is_multiplier: true));
		effects.Add(effect);
		Effect effect2 = new Effect("DupeMosquitoBite", STRINGS.CREATURES.MODIFIERS.DUPE_MOSQUITO_BITE.NAME, STRINGS.CREATURES.MODIFIERS.DUPE_MOSQUITO_BITE.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: true, is_bad: true);
		effect2.Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 1f / 60f, STRINGS.CREATURES.MODIFIERS.DUPE_MOSQUITO_BITE.NAME));
		effect2.Add(new AttributeModifier(Db.Get().Attributes.Sneezyness.Id, 5f, STRINGS.CREATURES.MODIFIERS.DUPE_MOSQUITO_BITE.NAME));
		effect2.Add(new AttributeModifier(Db.Get().Attributes.Athletics.Id, -1f, STRINGS.CREATURES.MODIFIERS.DUPE_MOSQUITO_BITE.NAME));
		effects.Add(effect2);
		Effect resource = new Effect("DupeMosquitoBiteSuppressed", STRINGS.CREATURES.MODIFIERS.DUPE_MOSQUITO_BITE_SUPPRESSED.NAME, STRINGS.CREATURES.MODIFIERS.DUPE_MOSQUITO_BITE_SUPPRESSED.TOOLTIP, 600f, show_in_ui: false, trigger_floating_text: false, is_bad: false);
		effects.Add(resource);
		Effect effect3 = new Effect("CritterMosquitoBite", STRINGS.CREATURES.MODIFIERS.CRITTER_MOSQUITO_BITE.NAME, STRINGS.CREATURES.MODIFIERS.CRITTER_MOSQUITO_BITE.TOOLTIP, 300f, show_in_ui: true, trigger_floating_text: true, is_bad: true);
		effect3.Add(new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, -1f, STRINGS.CREATURES.MODIFIERS.CRITTER_MOSQUITO_BITE.NAME));
		effects.Add(effect3);
		Effect resource2 = new Effect("CritterMosquitoBiteSuppressed", STRINGS.CREATURES.MODIFIERS.CRITTER_MOSQUITO_BITE_SUPPRESSED.NAME, STRINGS.CREATURES.MODIFIERS.CRITTER_MOSQUITO_BITE_SUPPRESSED.TOOLTIP, 300f, show_in_ui: false, trigger_floating_text: false, is_bad: false);
		effects.Add(resource2);
	}

	public void CreateCritteEffects()
	{
		Effect effect = new Effect("Ranched", STRINGS.CREATURES.MODIFIERS.RANCHED.NAME, STRINGS.CREATURES.MODIFIERS.RANCHED.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect.Add(new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, 5f, STRINGS.CREATURES.MODIFIERS.RANCHED.NAME));
		effect.Add(new AttributeModifier(Db.Get().Amounts.Wildness.deltaAttribute.Id, -11f / 120f, STRINGS.CREATURES.MODIFIERS.RANCHED.NAME));
		effects.Add(effect);
		Effect effect2 = new Effect("HadMilk", STRINGS.CREATURES.MODIFIERS.HADMILK.NAME, STRINGS.CREATURES.MODIFIERS.HADMILK.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect2.Add(new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, 5f, STRINGS.CREATURES.MODIFIERS.HADMILK.NAME));
		effects.Add(effect2);
		Effect effect3 = new Effect("HadInk", STRINGS.CREATURES.MODIFIERS.HADINK.NAME, STRINGS.CREATURES.MODIFIERS.HADINK.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect3.Add(new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, 3f, STRINGS.CREATURES.MODIFIERS.HADINK.NAME));
		effects.Add(effect3);
		Effect effect4 = new Effect("AteWellPreparedFishFood", STRINGS.CREATURES.MODIFIERS.FISHFOOD.NAME, STRINGS.CREATURES.MODIFIERS.FISHFOOD.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect4.Add(new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, 2f, STRINGS.CREATURES.MODIFIERS.FISHFOOD.NAME));
		effects.Add(effect4);
		Effect effect5 = new Effect("EggSong", STRINGS.CREATURES.MODIFIERS.INCUBATOR_SONG.NAME, STRINGS.CREATURES.MODIFIERS.INCUBATOR_SONG.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: false, is_bad: false);
		effect5.Add(new AttributeModifier(Db.Get().Amounts.Incubation.deltaAttribute.Id, 4f, STRINGS.CREATURES.MODIFIERS.INCUBATOR_SONG.NAME, is_multiplier: true));
		effects.Add(effect5);
		Effect effect6 = new Effect("EggHug", STRINGS.CREATURES.MODIFIERS.EGGHUG.NAME, STRINGS.CREATURES.MODIFIERS.EGGHUG.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect6.Add(new AttributeModifier(Db.Get().Amounts.Incubation.deltaAttribute.Id, 1f, STRINGS.CREATURES.MODIFIERS.EGGHUG.NAME, is_multiplier: true));
		effects.Add(effect6);
		Effect resource = new Effect("HuggingFrenzy", STRINGS.CREATURES.MODIFIERS.HUGGINGFRENZY.NAME, STRINGS.CREATURES.MODIFIERS.HUGGINGFRENZY.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: false, is_bad: false);
		effects.Add(resource);
		Effect effect7 = new Effect("DivergentCropTended", STRINGS.CREATURES.MODIFIERS.DIVERGENTPLANTTENDED.NAME, STRINGS.CREATURES.MODIFIERS.DIVERGENTPLANTTENDED.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect7.Add(new AttributeModifier(Db.Get().Amounts.Maturity.deltaAttribute.Id, 0.05f, STRINGS.CREATURES.MODIFIERS.DIVERGENTPLANTTENDED.NAME, is_multiplier: true));
		effect7.Add(new AttributeModifier(Db.Get().Amounts.Maturity2.deltaAttribute.Id, 0.05f, STRINGS.CREATURES.MODIFIERS.DIVERGENTPLANTTENDED.NAME, is_multiplier: true));
		effects.Add(effect7);
		Effect effect8 = new Effect("DivergentCropTendedWorm", STRINGS.CREATURES.MODIFIERS.DIVERGENTPLANTTENDEDWORM.NAME, STRINGS.CREATURES.MODIFIERS.DIVERGENTPLANTTENDEDWORM.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect8.Add(new AttributeModifier(Db.Get().Amounts.Maturity.deltaAttribute.Id, 0.5f, STRINGS.CREATURES.MODIFIERS.DIVERGENTPLANTTENDEDWORM.NAME, is_multiplier: true));
		effect8.Add(new AttributeModifier(Db.Get().Amounts.Maturity2.deltaAttribute.Id, 0.5f, STRINGS.CREATURES.MODIFIERS.DIVERGENTPLANTTENDEDWORM.NAME, is_multiplier: true));
		effects.Add(effect8);
		Effect effect9 = new Effect("MooWellFed", STRINGS.CREATURES.MODIFIERS.MOOWELLFED.NAME, STRINGS.CREATURES.MODIFIERS.MOOWELLFED.TOOLTIP, 1f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect9.Add(new AttributeModifier(Db.Get().Amounts.Beckoning.deltaAttribute.Id, MooTuning.WELLFED_EFFECT, STRINGS.CREATURES.MODIFIERS.MOOWELLFED.NAME));
		effect9.Add(new AttributeModifier(Db.Get().Amounts.MilkProduction.deltaAttribute.Id, MooTuning.MILK_PRODUCTION_PERCENTAGE_PER_SECOND, STRINGS.CREATURES.MODIFIERS.MOOWELLFED.NAME));
		effects.Add(effect9);
		Effect effect10 = new Effect("HuskyMooFed", STRINGS.CREATURES.MODIFIERS.HUSKYMOOFED.NAME, STRINGS.CREATURES.MODIFIERS.HUSKYMOOFED.TOOLTIP, 1f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect10.Add(new AttributeModifier(Db.Get().Amounts.Beckoning.deltaAttribute.Id, MooTuning.WELLFED_EFFECT, STRINGS.CREATURES.MODIFIERS.HUSKYMOOFED.NAME));
		effects.Add(effect10);
		Effect effect11 = new Effect("HuskyMooWellFed", STRINGS.CREATURES.MODIFIERS.HUSKYMOOWELLFED.NAME, STRINGS.CREATURES.MODIFIERS.HUSKYMOOWELLFED.TOOLTIP, 1f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect11.Add(new AttributeModifier(Db.Get().Amounts.MilkProduction.deltaAttribute.Id, MooTuning.MILK_PRODUCTION_PERCENTAGE_PER_SECOND, () => GameUtil.SafeStringFormat(STRINGS.CREATURES.STATS.MILKPRODUCTION.DISPLAYED_NAME, UI.StripLinkFormatting(ElementLoader.FindElementByHash(DieselMooConfig.MILK_ELEMENT).name)), () => STRINGS.CREATURES.MODIFIERS.HUSKYMOOWELLFED.NAME));
		effects.Add(effect11);
		Effect effect12 = new Effect("WoodDeerWellFed", STRINGS.CREATURES.MODIFIERS.DEERWELLFED.NAME, STRINGS.CREATURES.MODIFIERS.DEERWELLFED.TOOLTIP, 1f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect12.Add(new AttributeModifier(Db.Get().Amounts.ScaleGrowth.deltaAttribute.Id, 100f / (WoodDeerConfig.ANTLER_GROWTH_TIME_IN_CYCLES * 600f), () => GetScaleGrowthName("WoodDeer"), () => STRINGS.CREATURES.MODIFIERS.DEERWELLFED.NAME));
		effects.Add(effect12);
		Effect effect13 = new Effect("SquidWellFed", STRINGS.CREATURES.MODIFIERS.SQUIDWELLFED.NAME, STRINGS.CREATURES.MODIFIERS.SQUIDWELLFED.TOOLTIP, 1f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect13.Add(new AttributeModifier(Db.Get().Amounts.MilkProduction.deltaAttribute.Id, SquidTuning.INK_PRODUCTION_PERCENTAGE_PER_SECOND, STRINGS.CREATURES.MODIFIERS.SQUIDWELLFED.NAME));
		effects.Add(effect13);
		Effect effect14 = new Effect("GlassDeerWellFed", STRINGS.CREATURES.MODIFIERS.DEERWELLFED.NAME, STRINGS.CREATURES.MODIFIERS.DEERWELLFED.TOOLTIP, 1f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect14.Add(new AttributeModifier(Db.Get().Amounts.ScaleGrowth.deltaAttribute.Id, 1f / 36f, () => GetScaleGrowthName("GlassDeer"), () => STRINGS.CREATURES.MODIFIERS.DEERWELLFED.NAME));
		effects.Add(effect14);
		Effect effect15 = new Effect("IceBellyWellFed", STRINGS.CREATURES.MODIFIERS.ICEBELLYWELLFED.NAME, STRINGS.CREATURES.MODIFIERS.ICEBELLYWELLFED.TOOLTIP, 1f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect15.Add(new AttributeModifier(Db.Get().Amounts.ScaleGrowth.deltaAttribute.Id, 100f / (IceBellyConfig.SCALE_GROWTH_TIME_IN_CYCLES * 600f), () => GetScaleGrowthName("IceBelly"), () => STRINGS.CREATURES.MODIFIERS.ICEBELLYWELLFED.NAME));
		effects.Add(effect15);
		Effect effect16 = new Effect("GoldBellyWellFed", STRINGS.CREATURES.MODIFIERS.GOLDBELLYWELLFED.NAME, STRINGS.CREATURES.MODIFIERS.GOLDBELLYWELLFED.TOOLTIP, 1f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect16.Add(new AttributeModifier(Db.Get().Amounts.ScaleGrowth.deltaAttribute.Id, 1f / 60f, () => GetScaleGrowthName("GoldBelly"), () => STRINGS.CREATURES.MODIFIERS.GOLDBELLYWELLFED.NAME));
		effects.Add(effect16);
		Effect effect17 = new Effect("SeaTurtleWellFed", STRINGS.CREATURES.MODIFIERS.SEATURTLEWELLFED.NAME, STRINGS.CREATURES.MODIFIERS.SEATURTLEWELLFED.TOOLTIP, 1f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect17.Add(new AttributeModifier(Db.Get().Amounts.ScaleGrowth.deltaAttribute.Id, 100f / (SeaTurtleTuning.SCALE_GROWTH_TIME_IN_CYCLES * 600f), () => GetScaleGrowthName("SeaTurtle"), () => STRINGS.CREATURES.MODIFIERS.SEATURTLEWELLFED.NAME));
		effects.Add(effect17);
		Effect effect18 = new Effect("ButterflyPollinated", STRINGS.CREATURES.MODIFIERS.BUTTERFLYPOLLINATED.NAME, STRINGS.CREATURES.MODIFIERS.BUTTERFLYPOLLINATED.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect18.Add(new AttributeModifier(Db.Get().Amounts.Maturity.deltaAttribute.Id, 0.25f, STRINGS.CREATURES.MODIFIERS.BUTTERFLYPOLLINATED.NAME, is_multiplier: true));
		effect18.Add(new AttributeModifier(Db.Get().Amounts.Maturity2.deltaAttribute.Id, 0.25f, STRINGS.CREATURES.MODIFIERS.BUTTERFLYPOLLINATED.NAME, is_multiplier: true));
		effects.Add(effect18);
		Effect resource2 = new Effect(PollinationMonitor.INITIALLY_POLLINATED_EFFECT, STRINGS.CREATURES.MODIFIERS.INITIALLYPOLLINATED.NAME, STRINGS.CREATURES.MODIFIERS.INITIALLYPOLLINATED.TOOLTIP, 600f, show_in_ui: false, trigger_floating_text: false, is_bad: false);
		effects.Add(resource2);
		Effect effect19 = new Effect("RaptorWellFed", STRINGS.CREATURES.MODIFIERS.RAPTORWELLFED.NAME, STRINGS.CREATURES.MODIFIERS.RAPTORWELLFED.TOOLTIP, 1f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect19.Add(new AttributeModifier(Db.Get().Amounts.ScaleGrowth.deltaAttribute.Id, 100f / (RaptorConfig.SCALE_GROWTH_TIME_IN_CYCLES * 600f), () => GetScaleGrowthName("Raptor"), () => STRINGS.CREATURES.MODIFIERS.RAPTORWELLFED.NAME));
		effects.Add(effect19);
		Effect effect20 = new Effect("PredatorFailedHunt", STRINGS.CREATURES.MODIFIERS.HUNT_FAILED.NAME, STRINGS.CREATURES.MODIFIERS.HUNT_FAILED.TOOLTIP, 45f, show_in_ui: true, trigger_floating_text: false, is_bad: true);
		effect20.tag = GameTags.Creatures.SuppressedDiet;
		effects.Add(effect20);
		Effect resource3 = new Effect("PreyEvadedHunt", STRINGS.CREATURES.MODIFIERS.EVADED_HUNT.NAME, STRINGS.CREATURES.MODIFIERS.EVADED_HUNT.TOOLTIP, 10f, show_in_ui: true, trigger_floating_text: false, is_bad: false);
		effects.Add(resource3);
		Effect resource4 = new Effect("CaviarExtracted", STRINGS.CREATURES.MODIFIERS.CAVIAREXTRACTED.NAME, STRINGS.CREATURES.MODIFIERS.CAVIAREXTRACTED.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: false, is_bad: false);
		effects.Add(resource4);
		CreateMosquitoEffects();
		Effect resource5 = new Effect("RecentlyProducedLubricant", STRINGS.CREATURES.MODIFIERS.RECENTLYPRODUCEDLUBRICANT.NAME, STRINGS.CREATURES.MODIFIERS.RECENTLYPRODUCEDLUBRICANT.TOOLTIP, 10f, show_in_ui: false, trigger_floating_text: false, is_bad: false);
		effects.Add(resource5);
	}

	private static string GetScaleGrowthName(string prefabID)
	{
		if (STRINGS.CREATURES.STATS.SCALEGROWTH.GET_DISPLAYED_NAME().TryGetValue(prefabID, out var value))
		{
			return value;
		}
		return STRINGS.CREATURES.STATS.SCALEGROWTH.SCALE;
	}

	public Trait CreateTrait(string id, string name, string description, string group_name, bool should_save, ChoreGroup[] disabled_chore_groups, bool positive_trait, bool is_valid_starter_trait)
	{
		return CreateTrait(id, name, description, group_name, should_save, disabled_chore_groups, positive_trait, is_valid_starter_trait, null, null);
	}

	public Trait CreateTrait(string id, string name, string description, string group_name, bool should_save, ChoreGroup[] disabled_chore_groups, bool positive_trait, bool is_valid_starter_trait, string[] requiredDlcIds, string[] forbiddenDlcIds)
	{
		Trait trait = new Trait(id, name, description, 0f, should_save, disabled_chore_groups, positive_trait, is_valid_starter_trait, requiredDlcIds, forbiddenDlcIds);
		traits.Add(trait);
		if (group_name == "" || group_name == null)
		{
			group_name = "Default";
		}
		TraitGroup traitGroup = traitGroups.TryGet(group_name);
		if (traitGroup == null)
		{
			traitGroup = new TraitGroup(group_name, group_name, group_name != "Default");
			traitGroups.Add(traitGroup);
		}
		traitGroup.Add(trait);
		return trait;
	}

	public FertilityModifier CreateFertilityModifier(string id, Tag targetTag, string name, string description, Func<string, string> tooltipCB, FertilityModifier.FertilityModFn applyFunction)
	{
		FertilityModifier fertilityModifier = new FertilityModifier(id, targetTag, name, description, tooltipCB, applyFunction);
		FertilityModifiers.Add(fertilityModifier);
		return fertilityModifier;
	}

	public MooSongModifier CreateMooSongModifier(string id, Tag targetTag, string name, string description, Func<string, string> tooltipCB, MooSongModifier.MooSongModFn applyFunction)
	{
		MooSongModifier mooSongModifier = new MooSongModifier(id, targetTag, name, description, tooltipCB, applyFunction);
		MooSongModifiers.Add(mooSongModifier);
		return mooSongModifier;
	}

	protected void LoadTraits()
	{
		TRAITS.TRAIT_CREATORS.ForEach(delegate(System.Action action)
		{
			action();
		});
	}

	protected void LoadFertilityModifiers()
	{
		TUNING.CREATURES.EGG_CHANCE_MODIFIERS.MODIFIER_CREATORS.ForEach(delegate(System.Action action)
		{
			action();
		});
	}

	protected void LoadMooSongsModifiers()
	{
		TUNING.CREATURES.MOO_SONG_MODIFIERS.MODIFIER_CREATORS.ForEach(delegate(System.Action action)
		{
			action();
		});
	}
}
