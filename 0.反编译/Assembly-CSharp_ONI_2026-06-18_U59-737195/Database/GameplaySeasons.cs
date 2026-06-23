using Klei.AI;

namespace Database;

public class GameplaySeasons : ResourceSet<GameplaySeason>
{
	public GameplaySeason NaturalRandomEvents;

	public GameplaySeason DupeRandomEvents;

	public GameplaySeason PrickleCropSeason;

	public GameplaySeason BonusEvents;

	public GameplaySeason MeteorShowers;

	public GameplaySeason TemporalTearMeteorShowers;

	public GameplaySeason SpacedOutStyleStartMeteorShowers;

	public GameplaySeason SpacedOutStyleRocketMeteorShowers;

	public GameplaySeason SpacedOutStyleWarpMeteorShowers;

	public GameplaySeason ClassicStyleStartMeteorShowers;

	public GameplaySeason ClassicStyleWarpMeteorShowers;

	public GameplaySeason TundraMoonletMeteorShowers;

	public GameplaySeason MarshyMoonletMeteorShowers;

	public GameplaySeason NiobiumMoonletMeteorShowers;

	public GameplaySeason WaterMoonletMeteorShowers;

	public GameplaySeason GassyMooteorShowers;

	public GameplaySeason RegolithMoonMeteorShowers;

	public GameplaySeason MiniMetallicSwampyMeteorShowers;

	public GameplaySeason MiniForestFrozenMeteorShowers;

	public GameplaySeason MiniBadlandsMeteorShowers;

	public GameplaySeason MiniFlippedMeteorShowers;

	public GameplaySeason MiniRadioactiveOceanMeteorShowers;

	public GameplaySeason MiniCeresStartShowers;

	public GameplaySeason CeresMeteorShowers;

	public GameplaySeason LargeImpactor;

	public GameplaySeason PrehistoricMeteorShowers;

	public GameplaySeasons(ResourceSet parent)
		: base("GameplaySeasons", parent)
	{
		VanillaSeasons();
		Expansion1Seasons();
		DLCSeasons();
		UnusedSeasons();
	}

	private void VanillaSeasons()
	{
		MeteorShowers = Add(new MeteorShowerSeason("MeteorShowers", GameplaySeason.Type.World, 14f, synchronizedToPeriod: false, -1f, startActive: true).AddEvent(Db.Get().GameplayEvents.MeteorShowerIronEvent).AddEvent(Db.Get().GameplayEvents.MeteorShowerGoldEvent).AddEvent(Db.Get().GameplayEvents.MeteorShowerCopperEvent));
	}

	private void Expansion1Seasons()
	{
		RegolithMoonMeteorShowers = Add(new MeteorShowerSeason("RegolithMoonMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1).AddEvent(Db.Get().GameplayEvents.MeteorShowerDustEvent).AddEvent(Db.Get().GameplayEvents.ClusterIronShower).AddEvent(Db.Get().GameplayEvents.ClusterIceShower));
		TemporalTearMeteorShowers = Add(new MeteorShowerSeason("TemporalTearMeteorShowers", GameplaySeason.Type.World, 1f, synchronizedToPeriod: false, 0f, startActive: false, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: false, -1f, DlcManager.EXPANSION1).AddEvent(Db.Get().GameplayEvents.MeteorShowerFullereneEvent));
		GassyMooteorShowers = Add(new MeteorShowerSeason("GassyMooteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: false, 6000f, DlcManager.EXPANSION1).AddEvent(Db.Get().GameplayEvents.GassyMooteorEvent));
		SpacedOutStyleStartMeteorShowers = Add(new MeteorShowerSeason("SpacedOutStyleStartMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1));
		SpacedOutStyleRocketMeteorShowers = Add(new MeteorShowerSeason("SpacedOutStyleRocketMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1).AddEvent(Db.Get().GameplayEvents.ClusterOxyliteShower));
		SpacedOutStyleWarpMeteorShowers = Add(new MeteorShowerSeason("SpacedOutStyleWarpMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1).AddEvent(Db.Get().GameplayEvents.ClusterCopperShower).AddEvent(Db.Get().GameplayEvents.ClusterIceShower).AddEvent(Db.Get().GameplayEvents.ClusterBiologicalShower));
		ClassicStyleStartMeteorShowers = Add(new MeteorShowerSeason("ClassicStyleStartMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1).AddEvent(Db.Get().GameplayEvents.ClusterCopperShower).AddEvent(Db.Get().GameplayEvents.ClusterIceShower).AddEvent(Db.Get().GameplayEvents.ClusterBiologicalShower));
		ClassicStyleWarpMeteorShowers = Add(new MeteorShowerSeason("ClassicStyleWarpMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1).AddEvent(Db.Get().GameplayEvents.ClusterGoldShower).AddEvent(Db.Get().GameplayEvents.ClusterIronShower));
		TundraMoonletMeteorShowers = Add(new MeteorShowerSeason("TundraMoonletMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1));
		MarshyMoonletMeteorShowers = Add(new MeteorShowerSeason("MarshyMoonletMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1));
		NiobiumMoonletMeteorShowers = Add(new MeteorShowerSeason("NiobiumMoonletMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1));
		WaterMoonletMeteorShowers = Add(new MeteorShowerSeason("WaterMoonletMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1));
		MiniMetallicSwampyMeteorShowers = Add(new MeteorShowerSeason("MiniMetallicSwampyMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1).AddEvent(Db.Get().GameplayEvents.ClusterBiologicalShower).AddEvent(Db.Get().GameplayEvents.ClusterGoldShower));
		MiniForestFrozenMeteorShowers = Add(new MeteorShowerSeason("MiniForestFrozenMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1).AddEvent(Db.Get().GameplayEvents.ClusterOxyliteShower));
		MiniBadlandsMeteorShowers = Add(new MeteorShowerSeason("MiniBadlandsMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1).AddEvent(Db.Get().GameplayEvents.ClusterIceShower));
		MiniFlippedMeteorShowers = Add(new MeteorShowerSeason("MiniFlippedMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1));
		MiniRadioactiveOceanMeteorShowers = Add(new MeteorShowerSeason("MiniRadioactiveOceanMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1).AddEvent(Db.Get().GameplayEvents.ClusterUraniumShower));
	}

	private void DLCSeasons()
	{
		CeresMeteorShowers = Add(new MeteorShowerSeason("CeresMeteorShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 10f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.DLC2).AddEvent(Db.Get().GameplayEvents.ClusterIceAndTreesShower));
		MiniCeresStartShowers = Add(new MeteorShowerSeason("MiniCeresStartShowers", GameplaySeason.Type.World, 20f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.EXPANSION1.Append(DlcManager.DLC2)).AddEvent(Db.Get().GameplayEvents.ClusterOxyliteShower).AddEvent(Db.Get().GameplayEvents.ClusterSnowShower));
		LargeImpactor = Add(new GameplaySeason("LargeImpactor", GameplaySeason.Type.World, 1f, synchronizedToPeriod: false, -1f, startActive: true, 1, 0f, float.PositiveInfinity, 1, DlcManager.DLC4).AddEvent(Db.Get().GameplayEvents.LargeImpactor));
		PrehistoricMeteorShowers = Add(new MeteorShowerSeason("PrehistoricMeteorShowers", GameplaySeason.Type.World, 50f, synchronizedToPeriod: false, -1f, startActive: true, -1, 0f, float.PositiveInfinity, 1, affectedByDifficultySettings: true, 6000f, DlcManager.DLC4).AddEvent(Db.Get().GameplayEvents.ClusterCopperShower).AddEvent(Db.Get().GameplayEvents.ClusterIronShower).AddEvent(Db.Get().GameplayEvents.ClusterGoldShower));
	}

	private void UnusedSeasons()
	{
	}
}
