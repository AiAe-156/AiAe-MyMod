using System;
using System.Collections.Generic;
using KSerialization;
using Klei.AI;
using UnityEngine;

public class BeckoningMonitor : GameStateMachine<BeckoningMonitor, BeckoningMonitor.Instance, IStateMachineTarget, BeckoningMonitor.Def>
{
	[Serializable]
	public class SongChance
	{
		public Tag meteorID;

		public string singAnimPre;

		public string singAnimLoop;

		public string singAnimPst;

		public float weight;
	}

	public class Def : BaseDef
	{
		public List<SongChance> initialSongWeights;

		public float caloriesPerCycle;

		public string effectId = "MooWellFed";

		public override void Configure(GameObject prefab)
		{
			prefab.AddOrGet<Modifiers>().initialAmounts.Add(Db.Get().Amounts.Beckoning.Id);
		}
	}

	public new class Instance : GameInstance
	{
		private AmountInstance beckoning;

		[Serialize]
		public List<SongChance> songChances;

		[MyCmpGet]
		private Effects effects;

		[MyCmpGet]
		public KSelectable kselectable;

		private Guid beckoningBlockedHandle;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			beckoning = Db.Get().Amounts.Beckoning.Lookup(base.gameObject);
			InitializSongChances();
		}

		private void InitializSongChances()
		{
			songChances = new List<SongChance>();
			if (base.def.initialSongWeights == null)
			{
				return;
			}
			foreach (SongChance initialSongWeight in base.def.initialSongWeights)
			{
				songChances.Add(new SongChance
				{
					meteorID = initialSongWeight.meteorID,
					weight = initialSongWeight.weight,
					singAnimPre = initialSongWeight.singAnimPre,
					singAnimLoop = initialSongWeight.singAnimLoop,
					singAnimPst = initialSongWeight.singAnimPst
				});
				foreach (MooSongModifier item in Db.Get().MooSongModifiers.GetForTag(initialSongWeight.meteorID))
				{
					item.ApplyFunction(this, initialSongWeight.meteorID);
				}
			}
			NormalizeSongsChances();
		}

		public void AddSongChance(Tag type, float addedPercentChance)
		{
			foreach (SongChance songChance in songChances)
			{
				if (songChance.meteorID == type)
				{
					float num = Mathf.Min(1f - songChance.weight, Mathf.Max(0f - songChance.weight, addedPercentChance));
					songChance.weight += num;
				}
			}
			NormalizeSongsChances();
			base.master.Trigger(1105317911, songChances);
		}

		public void NormalizeSongsChances()
		{
			float num = 0f;
			foreach (SongChance songChance in songChances)
			{
				num += songChance.weight;
			}
			foreach (SongChance songChance2 in songChances)
			{
				songChance2.weight /= num;
			}
		}

		private bool IsSpaceVisible()
		{
			int num = Grid.PosToCell(this);
			if (Grid.IsValidCell(num))
			{
				return Grid.ExposedToSunlight[num] > 0;
			}
			return false;
		}

		private bool IsBeckoningAvailable()
		{
			return base.smi.beckoning.value >= base.smi.beckoning.GetMax();
		}

		public bool IsReadyToBeckon()
		{
			if (IsBeckoningAvailable())
			{
				return IsSpaceVisible();
			}
			return false;
		}

		public void UpdateBlockedStatusItem()
		{
			bool flag = IsSpaceVisible();
			if (!flag && IsBeckoningAvailable() && beckoningBlockedHandle == Guid.Empty)
			{
				beckoningBlockedHandle = kselectable.AddStatusItem(Db.Get().CreatureStatusItems.BeckoningBlocked);
			}
			else if (flag)
			{
				beckoningBlockedHandle = kselectable.RemoveStatusItem(beckoningBlockedHandle);
			}
		}

		public void OnCaloriesConsumed(object data)
		{
			CreatureCalorieMonitor.CaloriesConsumedEvent value = ((Boxed<CreatureCalorieMonitor.CaloriesConsumedEvent>)data).value;
			EffectInstance effectInstance = effects.Get(base.smi.def.effectId);
			if (effectInstance == null)
			{
				effectInstance = effects.Add(base.smi.def.effectId, should_save: true);
			}
			effectInstance.timeRemaining += value.calories / base.smi.def.caloriesPerCycle * 600f;
		}
	}

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = root;
		root.EventHandler(GameHashes.CaloriesConsumed, delegate(Instance smi, object data)
		{
			smi.OnCaloriesConsumed(data);
		}).ToggleBehaviour(GameTags.Creatures.WantsToBeckon, (Instance smi) => smi.IsReadyToBeckon()).Update(delegate(Instance smi, float dt)
		{
			smi.UpdateBlockedStatusItem();
		}, UpdateRate.SIM_1000ms);
	}
}
