using System.Collections.Generic;
using Klei.AI;
using TUNING;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/MessStation")]
public class MessStation : Workable, IDiningSeat
{
	public class MessStationSM : GameStateMachine<MessStationSM, MessStationSM.Instance, MessStation>
	{
		public class SaltState : State
		{
			public State none;

			public State salty;
		}

		public new class Instance : GameInstance
		{
			private Storage garnishStorage;

			private Reservable reservable;

			private SymbolOverrideController symbolOverrideController;

			private static readonly HashedString SALT_SHAKER_SYMBOL = "saltshaker";

			public bool HasGarnish => GARNISHES.HasAnyGarnish(garnishStorage);

			public Instance(MessStation master)
				: base(master)
			{
				garnishStorage = master.GetComponent<Storage>();
				reservable = master.GetComponent<Reservable>();
				symbolOverrideController = master.GetComponent<SymbolOverrideController>();
				garnishStorage.Subscribe(-1697596308, delegate
				{
					UpdateGarnishOverride();
				});
				UpdateGarnishOverride();
			}

			public void UpdateGarnishOverride()
			{
				if (!(symbolOverrideController == null))
				{
					KAnim.Build.Symbol symbol = GARNISHES.GetActiveGarnish(garnishStorage)?.GetOverrideSymbol();
					if (symbol != null)
					{
						symbolOverrideController.AddSymbolOverride(SALT_SHAKER_SYMBOL, symbol);
					}
					else
					{
						symbolOverrideController.RemoveSymbolOverride(SALT_SHAKER_SYMBOL);
					}
				}
			}

			public bool IsEating()
			{
				if (reservable == null)
				{
					return false;
				}
				if (reservable.ReservedBy == null)
				{
					return false;
				}
				if (!reservable.ReservedBy.TryGetComponent<ChoreDriver>(out var component))
				{
					return false;
				}
				if (!component.HasChore())
				{
					return false;
				}
				if (component.GetCurrentChore() is ReloadElectrobankChore reloadElectrobankChore)
				{
					return reloadElectrobankChore.IsInstallingAtMessStation();
				}
				return component.GetCurrentChore().choreType.urge == Db.Get().Urges.Eat;
			}
		}

		public SaltState salt;

		public State eating;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = salt.none;
			salt.none.Transition(salt.salty, (Instance smi) => smi.HasGarnish).PlayAnim("off");
			salt.salty.Transition(salt.none, (Instance smi) => !smi.HasGarnish).PlayAnim("salt").EventTransition(GameHashes.EatStart, eating);
			eating.Transition(salt.salty, (Instance smi) => smi.HasGarnish && !smi.IsEating()).Transition(salt.none, (Instance smi) => !smi.HasGarnish && !smi.IsEating()).PlayAnim("off");
		}
	}

	[MyCmpGet]
	private Ownable ownable;

	private MessStationSM.Instance smi;

	public static readonly HashedString eatAnim = "anim_eat_table_kanim";

	public static readonly HashedString reloadElectrobankAnim = "anim_bionic_eat_table_kanim";

	public bool HasGarnish => smi.HasGarnish;

	public HashedString EatAnim => eatAnim;

	public HashedString ReloadElectrobankAnim => reloadElectrobankAnim;

	public KPrefabID Diner { get; set; }

	protected override void OnPrefabInit()
	{
		ownable.AddAssignPrecondition(HasCaloriesOwnablePrecondition);
		base.OnPrefabInit();
		overrideAnims = new KAnimFile[1] { Assets.GetAnim(eatAnim) };
	}

	public static bool CanBeAssignedTo(IAssignableIdentity assignee)
	{
		MinionAssignablesProxy minionAssignablesProxy = assignee as MinionAssignablesProxy;
		if (minionAssignablesProxy == null)
		{
			return false;
		}
		MinionIdentity minionIdentity = minionAssignablesProxy.target as MinionIdentity;
		if (minionIdentity == null)
		{
			return false;
		}
		AmountInstance amountInstance = Db.Get().Amounts.Calories.Lookup(minionIdentity);
		if (amountInstance != null)
		{
			return true;
		}
		if (Game.IsDlcActiveForCurrentSave("DLC3_ID"))
		{
			return minionIdentity.model == BionicMinionConfig.MODEL;
		}
		return false;
	}

	private bool HasCaloriesOwnablePrecondition(MinionAssignablesProxy worker)
	{
		return CanBeAssignedTo(worker);
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		worker.GetWorkable().GetComponent<Edible>().CompleteWork(worker);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		smi = new MessStationSM.Instance(this);
		smi.StartSM();
	}

	public override List<Descriptor> GetDescriptors(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor>();
		Storage component = go.GetComponent<Storage>();
		if (component != null)
		{
			foreach (GarnishInfo allGarnish in GARNISHES.AllGarnishes)
			{
				if (component.Has(allGarnish.itemTag))
				{
					list.Add(allGarnish.descriptor);
				}
			}
		}
		return list;
	}

	public Storage FindStorage()
	{
		return GetComponent<Storage>();
	}

	public Operational FindOperational()
	{
		return GetComponent<Operational>();
	}
}
