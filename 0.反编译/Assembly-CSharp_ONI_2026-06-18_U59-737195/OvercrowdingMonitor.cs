using System;
using System.Collections.Generic;
using System.Text;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class OvercrowdingMonitor : GameStateMachine<OvercrowdingMonitor, OvercrowdingMonitor.Instance, IStateMachineTarget, OvercrowdingMonitor.Def>
{
	public class Def : BaseDef
	{
		public int spaceRequiredPerCreature;
	}

	public class Occupancy
	{
		public bool dirty = true;

		public Dictionary<Tag, int> CritterCounts { get; } = new Dictionary<Tag, int>();

		public int OccupiedCellCount { get; private set; }

		public int HatchedEggOccupiedCellCount { get; private set; }

		public int Generation { get; private set; }

		public void Analyze(List<KPrefabID> creatures, List<KPrefabID> eggs, bool excludeFish = false)
		{
			DebugUtil.DevAssert(dirty, "Only incur Analyze overhead when dirty");
			CritterCounts.EnsureCapacity(creatures.Count);
			CritterCounts.Clear();
			OccupiedCellCount = 0;
			HatchedEggOccupiedCellCount = 0;
			creatures.RemoveAll(Util.IsNullOrDestroyedPredicate);
			foreach (KPrefabID creature in creatures)
			{
				if (!excludeFish || !FetchIsFish(creature))
				{
					OccupiedCellCount += FetchCreaturePersonalSpace(creature);
					if (CritterCounts.TryGetValue(creature.PrefabTag, out var value))
					{
						CritterCounts[creature.PrefabTag] = value + 1;
					}
					else
					{
						CritterCounts[creature.PrefabTag] = 1;
					}
				}
			}
			eggs.RemoveAll(Util.IsNullOrDestroyedPredicate);
			foreach (KPrefabID egg in eggs)
			{
				if (!excludeFish || !FetchIsFishEgg(egg))
				{
					HatchedEggOccupiedCellCount += FetchEggPersonalSpace(egg);
				}
			}
			Generation++;
			dirty = false;
		}
	}

	public struct RegionAnalysis
	{
		private struct CritterOccupancy
		{
			public Tag critterType;

			public int overOccupancy;

			public bool canFix;
		}

		private Occupancy occupancy;

		private int occupancyGeneration;

		private readonly Instance smi;

		public readonly bool IsDirty
		{
			get
			{
				if (occupancy != null)
				{
					return occupancyGeneration != occupancy.Generation;
				}
				return true;
			}
		}

		public readonly int CellCount => smi.RegionSize;

		public readonly bool IsPond => smi.IsInPond;

		public readonly Dictionary<Tag, int> CritterCounts
		{
			get
			{
				if (occupancy == null)
				{
					return null;
				}
				return occupancy.CritterCounts;
			}
		}

		public readonly int OccupiedCellCount
		{
			get
			{
				if (occupancy == null)
				{
					return 0;
				}
				return occupancy.OccupiedCellCount;
			}
		}

		public readonly int HatchedEggOccupiedCellCount
		{
			get
			{
				if (occupancy == null)
				{
					return 0;
				}
				return occupancy.HatchedEggOccupiedCellCount;
			}
		}

		public readonly bool IsOvercrowded
		{
			get
			{
				if (!IsDegeneratePersonalSpace)
				{
					return UnoccupiedCellCount < 0;
				}
				return false;
			}
		}

		public readonly bool IsConfined
		{
			get
			{
				if (!ConfinementImmunity && !IsDegeneratePersonalSpace)
				{
					return CellCount < PersonalSpace;
				}
				return false;
			}
		}

		public readonly bool IsFutureOvercrowded
		{
			get
			{
				if (!IsDegeneratePersonalSpace && FutureUnoccupiedCellCount < 0)
				{
					return HatchedEggOccupiedCellCount > 0;
				}
				return false;
			}
		}

		public readonly int OvercrowdedModifier
		{
			get
			{
				if (!IsOvercrowded)
				{
					return 0;
				}
				return -OverOccupiedCritterCount;
			}
		}

		public readonly bool IsDegenerate => CellCount <= 0;

		private readonly int PersonalSpace => smi.def.spaceRequiredPerCreature;

		private readonly bool ConfinementImmunity => smi.kpid.HasAnyTags(CONFINEMENT_IMMUNITY_TAGS);

		private readonly int UnoccupiedCellCount => CellCount - OccupiedCellCount;

		private readonly int OverOccupiedCellCount => OccupiedCellCount - CellCount;

		private readonly int FutureUnoccupiedCellCount => UnoccupiedCellCount - HatchedEggOccupiedCellCount;

		private readonly bool IsDegeneratePersonalSpace => PersonalSpace == 0;

		private readonly int OverOccupiedCritterCount => ComputeOverOccupiedCritterCount(PersonalSpace);

		public RegionAnalysis(Instance smi)
		{
			this.smi = smi;
			occupancy = null;
			occupancyGeneration = -1;
		}

		public void SetOccupancy(Occupancy occupancy)
		{
			this.occupancy = occupancy;
			occupancyGeneration = occupancy?.Generation ?? (-1);
		}

		public void ForceDirty()
		{
			occupancyGeneration = -1;
		}

		public readonly string Substitute(string s)
		{
			LocString locString = ((!IsPond) ? ((CellCount == 0) ? CREATURES.MODIFIERS.OVERCROWDED.EXPLANATION.NO_CELLS : ((CellCount == 1) ? CREATURES.MODIFIERS.OVERCROWDED.EXPLANATION.SINGLE_CELL : CREATURES.MODIFIERS.OVERCROWDED.EXPLANATION.MULTIPLE_CELLS)) : ((CellCount == 0) ? CREATURES.MODIFIERS.OVERCROWDED.EXPLANATION_AQUATIC.NO_CELLS : ((CellCount == 1) ? CREATURES.MODIFIERS.OVERCROWDED.EXPLANATION_AQUATIC.SINGLE_CELL : CREATURES.MODIFIERS.OVERCROWDED.EXPLANATION_AQUATIC.MULTIPLE_CELLS)));
			StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
			stringBuilder.Append(s);
			stringBuilder.Replace("{explanation}", locString);
			stringBuilder.Replace("{contextCritterType}", smi.kpid.PrefabTag.ProperName());
			stringBuilder.Replace("{personalSpace}", PersonalSpace.ToString());
			stringBuilder.Replace("{cellCount}", CellCount.ToString());
			stringBuilder.Replace("{occupiedCellCount}", OccupiedCellCount.ToString());
			stringBuilder.Replace("{unoccupiedCellCount}", ((!IsOvercrowded) ? UnoccupiedCellCount : 0).ToString());
			stringBuilder.Replace("{overOccupiedCellCount}", (IsOvercrowded ? OverOccupiedCellCount : 0).ToString());
			stringBuilder.Replace("{bullets}", BuildCritterOccupancies());
			return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
		}

		private readonly int ComputeOverOccupiedCritterCount(int personalSpace)
		{
			if (personalSpace == 0)
			{
				return 0;
			}
			int result;
			return Math.DivRem(OverOccupiedCellCount, personalSpace, out result) + ((result != 0) ? 1 : 0);
		}

		private readonly string BuildCritterOccupancies()
		{
			if (CritterCounts == null)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
			ListPool<CritterOccupancy, RegionAnalysis>.PooledList pooledList = ListPool<CritterOccupancy, RegionAnalysis>.Allocate();
			pooledList.Capacity = CritterCounts.Count;
			foreach (Tag key in CritterCounts.Keys)
			{
				int personalSpace = personalSpaces[key];
				int num = ComputeOverOccupiedCritterCount(personalSpace);
				int num2 = CritterCounts[key];
				bool canFix = num <= num2;
				num = Math.Min(CritterCounts[key], num);
				pooledList.Add(new CritterOccupancy
				{
					critterType = key,
					overOccupancy = num,
					canFix = canFix
				});
			}
			Dictionary<Tag, int> capturedCritterCounts = CritterCounts;
			Dictionary<Tag, int> capturedPersonalSpaces = personalSpaces;
			pooledList.Sort(delegate(CritterOccupancy a, CritterOccupancy b)
			{
				int value = capturedCritterCounts[a.critterType] * capturedPersonalSpaces[a.critterType];
				return (capturedCritterCounts[b.critterType] * capturedPersonalSpaces[b.critterType]).CompareTo(value);
			});
			foreach (CritterOccupancy item in pooledList)
			{
				int num3 = CritterCounts[item.critterType];
				LocString locString = ((num3 == 1) ? (item.canFix ? CREATURES.MODIFIERS.OVERCROWDED.BULLET.CAN_FIX.SINGULAR : CREATURES.MODIFIERS.OVERCROWDED.BULLET.CANNOT_FIX.SINGULAR) : (item.canFix ? CREATURES.MODIFIERS.OVERCROWDED.BULLET.CAN_FIX.MULTIPLE : CREATURES.MODIFIERS.OVERCROWDED.BULLET.CANNOT_FIX.MULTIPLE));
				int num4 = personalSpaces[item.critterType];
				int num5 = OccupiedCellCount - item.overOccupancy * num4;
				StringBuilder stringBuilder2 = GlobalStringBuilderPool.Alloc();
				stringBuilder2.Append(locString);
				stringBuilder2.Replace("{critterType}", item.critterType.ProperName());
				stringBuilder2.Replace("{critterCount}", num3.ToString());
				stringBuilder2.Replace("{personalSpace}", num4.ToString());
				int overOccupancy = item.overOccupancy;
				stringBuilder2.Replace("{overOccupancy}", overOccupancy.ToString());
				stringBuilder2.Replace("{cellCountWithFix}", num5.ToString());
				stringBuilder.AppendLine(GlobalStringBuilderPool.ReturnAndFree(stringBuilder2));
			}
			if (CritterCounts.Count > 0)
			{
				stringBuilder.Length--;
			}
			pooledList.Recycle();
			return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
		}
	}

	public struct OvercrowdEffect
	{
		public EffectInstance instance;

		public Effect Effect { get; private set; }

		public AttributeModifier Modifier { get; private set; }

		public Func<string, object, string> Tooltip { get; private set; }

		public string TooltipText { get; private set; }

		public OvercrowdEffect(string id, string name, AttributeModifier modifier, Func<string> GenerateTooltip)
		{
			Effect = new Effect(id, name, string.Empty, 0f, show_in_ui: true, trigger_floating_text: false, is_bad: true);
			instance = null;
			Modifier = modifier;
			Tooltip = null;
			Effect.Add(modifier);
			TooltipText = null;
			OvercrowdEffect capturedThis = this;
			Tooltip = delegate(string _tooltip, object untypedEffectInstance)
			{
				if (capturedThis.TooltipText == null)
				{
					EffectInstance effectInstance = (EffectInstance)untypedEffectInstance;
					capturedThis.TooltipText = effectInstance.ResolveTooltip(GenerateTooltip(), untypedEffectInstance);
				}
				return capturedThis.TooltipText;
			};
		}

		public OvercrowdEffect(string id, string name, AttributeModifier modifier, string tooltipFormat)
			: this(id, name, modifier, () => tooltipFormat)
		{
		}

		public void ClearTooltip()
		{
			TooltipText = null;
		}
	}

	public new class Instance : GameInstance
	{
		public CavityInfo cavity;

		public FishOvercrowingManager.Pond pond;

		public bool isBaby;

		public OvercrowdEffect futureOvercrowded;

		public OvercrowdEffect overcrowded;

		public OvercrowdEffect confined;

		public RegionAnalysis regionAnalysis;

		[MyCmpReq]
		public KPrefabID kpid;

		[MyCmpReq]
		public Effects effects;

		private int onRoomUpdatedHandle = -1;

		public int RegionSize
		{
			get
			{
				if (!IsFish)
				{
					if (cavity == null)
					{
						return 0;
					}
					return cavity.NumCells;
				}
				if (pond == null)
				{
					return 0;
				}
				return pond.cellCount;
			}
		}

		public bool IsInPond => pond != null;

		public bool IsFish => FetchIsFish(kpid);

		public bool IsEgg => kpid.HasTag(GameTags.Egg);

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			BabyMonitor.Def def2 = master.gameObject.GetDef<BabyMonitor.Def>();
			isBaby = def2 != null;
			futureOvercrowded = new OvercrowdEffect("FutureOvercrowded", CREATURES.MODIFIERS.OVERCROWDED.CRAMPED.NAME, new AttributeModifier(Db.Get().Amounts.Fertility.deltaAttribute.Id, -1f, CREATURES.MODIFIERS.OVERCROWDED.CRAMPED.NAME, is_multiplier: true), FutureOvercrowdedTooltip);
			overcrowded = new OvercrowdEffect("Overcrowded", CREATURES.MODIFIERS.OVERCROWDED.CROWDED.NAME, new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, 0f, CREATURES.MODIFIERS.OVERCROWDED.CROWDED.NAME, is_multiplier: false, uiOnly: false, is_readonly: false), OvercrowdedTooltip);
			confined = new OvercrowdEffect("Confined", CREATURES.MODIFIERS.OVERCROWDED.CONFINED.NAME, new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, -10f, CREATURES.MODIFIERS.OVERCROWDED.CONFINED.NAME, is_multiplier: false, uiOnly: false, is_readonly: false), ConfinedTooltip);
			confined.Effect.Add(new AttributeModifier(Db.Get().Amounts.Fertility.deltaAttribute.Id, -1f, CREATURES.MODIFIERS.OVERCROWDED.CONFINED.NAME, is_multiplier: true));
			onRoomUpdatedHandle = Subscribe(144050788, OnRoomUpdated);
			regionAnalysis = new RegionAnalysis(this);
			UpdateState(this, 0f);
			string ConfinedTooltip()
			{
				return regionAnalysis.Substitute(regionAnalysis.IsDegenerate ? CREATURES.MODIFIERS.OVERCROWDED.CONFINED.TOOLTIP_NO_SUBSTITUTIONS : CREATURES.MODIFIERS.OVERCROWDED.CONFINED.TOOLTIP);
			}
			string FutureOvercrowdedTooltip()
			{
				return regionAnalysis.Substitute(IsFish ? CREATURES.MODIFIERS.OVERCROWDED.CRAMPED.FISHTOOLTIP : CREATURES.MODIFIERS.OVERCROWDED.CRAMPED.TOOLTIP);
			}
			string OvercrowdedTooltip()
			{
				return regionAnalysis.Substitute(IsFish ? CREATURES.MODIFIERS.OVERCROWDED.CROWDED.FISHTOOLTIP : CREATURES.MODIFIERS.OVERCROWDED.CROWDED.TOOLTIP);
			}
		}

		public void OnRegionAnalysisDirtied()
		{
			futureOvercrowded.ClearTooltip();
			overcrowded.ClearTooltip();
			confined.ClearTooltip();
		}

		public void AddToCavity()
		{
			if (IsEgg)
			{
				cavity.eggs.Add(kpid);
				if (FetchIsFishEgg(kpid))
				{
					cavity.fish_eggs.Add(kpid);
				}
			}
			else
			{
				cavity.creatures.Add(kpid);
				if (IsFish)
				{
					cavity.fishes.Add(kpid);
				}
			}
			cavity.occupancy.dirty = true;
		}

		public void RemoveFromCavity()
		{
			if (IsEgg)
			{
				cavity.RemoveFromCavity(kpid, cavity.eggs);
				if (FetchIsFishEgg(kpid))
				{
					cavity.RemoveFromCavity(kpid, cavity.fish_eggs);
				}
			}
			else
			{
				cavity.RemoveFromCavity(kpid, cavity.creatures);
				if (IsFish)
				{
					cavity.RemoveFromCavity(kpid, cavity.fishes);
				}
			}
			cavity.occupancy.dirty = true;
		}

		protected override void OnCleanUp()
		{
			Unsubscribe(ref onRoomUpdatedHandle);
			if (cavity != null)
			{
				RemoveFromCavity();
			}
		}

		public void OnRoomUpdated(object o)
		{
			if (o == null)
			{
				RoomRefreshUpdateCavity();
			}
		}

		public void RoomRefreshUpdateCavity()
		{
			UpdateState(this, 0f);
		}
	}

	public const float OVERCROWDED_FERTILITY_DEBUFF = -1f;

	public static readonly Tag[] CONFINEMENT_IMMUNITY_TAGS = new Tag[2]
	{
		GameTags.Creatures.Burrowed,
		GameTags.Creatures.Digger
	};

	private static readonly Dictionary<Tag, int> personalSpaces = new Dictionary<Tag, int>();

	private static readonly Dictionary<Tag, bool> isFish = new Dictionary<Tag, bool>();

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = root;
		root.Update(UpdateState, UpdateRate.SIM_1000ms, load_balance: true);
	}

	private static int FetchCreaturePersonalSpace(KPrefabID creature)
	{
		if (personalSpaces.TryGetValue(creature.PrefabTag, out var value))
		{
			return value;
		}
		Instance sMI = creature.GetSMI<Instance>();
		if (sMI != null)
		{
			value = sMI.def.spaceRequiredPerCreature;
		}
		personalSpaces[creature.PrefabTag] = value;
		return value;
	}

	private static int FetchEggPersonalSpace(KPrefabID egg)
	{
		if (personalSpaces.TryGetValue(egg.PrefabTag, out var value))
		{
			return value;
		}
		IncubationMonitor.Instance sMI = egg.GetSMI<IncubationMonitor.Instance>();
		if (personalSpaces.TryGetValue(sMI.def.spawnedCreature, out value))
		{
			personalSpaces[egg.PrefabTag] = value;
			return value;
		}
		GameObject prefab = Assets.GetPrefab(sMI.def.spawnedCreature);
		if (prefab != null)
		{
			value = prefab.GetDef<Def>().spaceRequiredPerCreature;
		}
		personalSpaces[egg.PrefabTag] = value;
		personalSpaces[sMI.def.spawnedCreature] = value;
		return value;
	}

	private static void UpdateState(Instance smi, float dt)
	{
		UpdateRegion(smi);
		if (smi.def.spaceRequiredPerCreature == 0)
		{
			return;
		}
		Occupancy occupancy = null;
		List<KPrefabID> creatures = null;
		List<KPrefabID> eggs = null;
		if (smi.IsFish)
		{
			if (smi.pond != null)
			{
				occupancy = smi.pond.occupancy;
				creatures = smi.pond.fishes;
				eggs = smi.pond.eggs;
			}
		}
		else if (smi.cavity != null)
		{
			occupancy = smi.cavity.occupancy;
			creatures = smi.cavity.creatures;
			eggs = smi.cavity.eggs;
		}
		if (occupancy != null && occupancy.dirty)
		{
			occupancy.Analyze(creatures, eggs, !smi.IsFish);
		}
		if (smi.regionAnalysis.IsDirty)
		{
			smi.regionAnalysis.SetOccupancy(occupancy);
			smi.OnRegionAnalysisDirtied();
		}
		AlignTagsAndEffects(smi);
	}

	private static void AlignTagsAndEffects(Instance smi)
	{
		bool isConfined = smi.regionAnalysis.IsConfined;
		bool isOvercrowded = smi.regionAnalysis.IsOvercrowded;
		int overcrowdedModifier = smi.regionAnalysis.OvercrowdedModifier;
		bool isFutureOvercrowded = smi.regionAnalysis.IsFutureOvercrowded;
		overcrowdedModifier = (isOvercrowded ? overcrowdedModifier : 0);
		if (isOvercrowded)
		{
			smi.overcrowded.Modifier.SetValue(overcrowdedModifier);
		}
		bool flag = smi.kpid.HasTag(GameTags.Creatures.Overcrowded);
		bool num = smi.kpid.HasTag(GameTags.Creatures.Expecting);
		bool flag2 = smi.kpid.HasTag(GameTags.Creatures.Confined);
		bool flag3 = smi.effects.HasEffect(smi.futureOvercrowded.Effect);
		if (flag != isOvercrowded)
		{
			smi.kpid.SetTag(GameTags.Creatures.Overcrowded, isOvercrowded);
		}
		bool flag4 = !smi.isBaby && !isFutureOvercrowded;
		if (num != flag4)
		{
			smi.kpid.SetTag(GameTags.Creatures.Expecting, flag4);
		}
		if (flag2 != isConfined)
		{
			smi.kpid.SetTag(GameTags.Creatures.Confined, isConfined);
		}
		bool flag5 = isConfined;
		bool flag6 = isOvercrowded && !flag5;
		bool flag7 = isFutureOvercrowded && !flag5;
		if (flag5 != flag2)
		{
			smi.confined.instance = SetEffect(smi, smi.confined.Effect, flag5, smi.confined.Tooltip);
		}
		if (flag6 != flag)
		{
			smi.overcrowded.instance = SetEffect(smi, smi.overcrowded.Effect, flag6, smi.overcrowded.Tooltip);
		}
		if (flag7 != flag3)
		{
			smi.futureOvercrowded.instance = SetEffect(smi, smi.futureOvercrowded.Effect, flag7, smi.futureOvercrowded.Tooltip);
		}
	}

	private static EffectInstance SetEffect(Instance smi, Effect effect, bool set, Func<string, object, string> tooltip)
	{
		if (set)
		{
			return smi.effects.Add(effect, should_save: false, tooltip);
		}
		smi.effects.Remove(effect);
		return null;
	}

	private static bool FetchIsFish(KPrefabID creature)
	{
		if (isFish.TryGetValue(creature.PrefabTag, out var value))
		{
			return value;
		}
		value = creature.GetDef<FishOvercrowdingMonitor.Def>() != null;
		isFish[creature.PrefabTag] = value;
		return value;
	}

	private static bool FetchIsFishEgg(KPrefabID egg)
	{
		if (isFish.TryGetValue(egg.PrefabTag, out var value))
		{
			return value;
		}
		IncubationMonitor.Instance sMI = egg.GetSMI<IncubationMonitor.Instance>();
		if (isFish.TryGetValue(sMI.def.spawnedCreature, out value))
		{
			isFish[egg.PrefabTag] = value;
			return value;
		}
		GameObject prefab = Assets.GetPrefab(sMI.def.spawnedCreature);
		if (prefab != null)
		{
			value = prefab.GetDef<FishOvercrowdingMonitor.Def>() != null;
		}
		isFish[egg.PrefabTag] = value;
		isFish[sMI.def.spawnedCreature] = value;
		return value;
	}

	private static void UpdateRegion(Instance smi)
	{
		int cell = Grid.PosToCell(smi);
		CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(cell);
		if (cavityForCell != smi.cavity)
		{
			if (smi.cavity != null)
			{
				smi.RemoveFromCavity();
				Game.Instance.roomProber.UpdateRoom(cavityForCell);
				smi.regionAnalysis.ForceDirty();
			}
			smi.cavity = cavityForCell;
			if (smi.cavity != null)
			{
				smi.AddToCavity();
				Game.Instance.roomProber.UpdateRoom(smi.cavity);
				smi.regionAnalysis.ForceDirty();
			}
		}
		if (!smi.IsFish)
		{
			return;
		}
		FishOvercrowingManager.Pond pond = FishOvercrowingManager.Instance.GetPond(cell);
		if (pond != smi.pond)
		{
			if (smi.pond != null)
			{
				smi.regionAnalysis.ForceDirty();
			}
			smi.pond = pond;
			if (smi.pond != null)
			{
				smi.regionAnalysis.ForceDirty();
			}
		}
	}
}
