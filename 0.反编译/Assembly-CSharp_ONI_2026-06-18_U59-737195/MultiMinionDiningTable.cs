using System.Collections.Generic;
using KSerialization;
using Klei.AI;
using STRINGS;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class MultiMinionDiningTable : KMonoBehaviour, IGameObjectEffectDescriptor
{
	public class Seat : Assignable, IDiningSeat
	{
		private int index;

		private KPrefabID diner;

		private MultiMinionDiningTableConfig.Seat SeatConfig => MultiMinionDiningTableConfig.seats[index];

		public HashedString SaltSymbol => SeatConfig.SaltSymbol;

		public GameObject DiningTable => base.transform.parent.gameObject;

		public bool HasGarnish => DiningTable.GetComponent<MultiMinionDiningTable>().HasGarnish;

		public HashedString EatAnim => SeatConfig.EatAnim;

		public HashedString ReloadElectrobankAnim => SeatConfig.ReloadElectrobankAnim;

		public KPrefabID Diner
		{
			get
			{
				return diner;
			}
			set
			{
				KPrefabID prevDiner = diner;
				diner = value;
				DiningTable.GetComponent<MultiMinionDiningTable>().OnDinerChanged(prevDiner, diner, index);
			}
		}

		public bool HasDiner => Diner != null;

		public Storage FindStorage()
		{
			return DiningTable.GetComponent<Storage>();
		}

		public Operational FindOperational()
		{
			return DiningTable.GetComponent<Operational>();
		}

		public Seat()
		{
			slotID = Db.Get().AssignableSlots.MessStation.Id;
			canBePublic = true;
		}

		public void Initialize(int index)
		{
			this.index = index;
		}
	}

	private readonly struct Diner
	{
		private readonly KPrefabID kpid;

		private readonly int startTalkingHandler;

		private readonly int stopTalkingHandler;

		public Diner(MultiMinionDiningTable table, KPrefabID diner)
		{
			kpid = diner;
			diner.AddTag(GameTags.CommunalDining);
			diner.AddTag(GameTags.AlwaysConverse);
			startTalkingHandler = diner.Subscribe(-594200555, delegate(object eventData)
			{
				table.OnDinerStartTalking(diner, eventData);
			});
			stopTalkingHandler = diner.Subscribe(25860745, delegate(object eventData)
			{
				table.OnDinerStopTalking(diner, eventData);
			});
		}

		public void CleanUp()
		{
			kpid.RemoveTag(GameTags.CommunalDining);
			kpid.RemoveTag(GameTags.AlwaysConverse);
			kpid.Unsubscribe(startTalkingHandler);
			kpid.Unsubscribe(stopTalkingHandler);
		}
	}

	public const string SEAT_ID = "MultiMinionDiningSeat";

	[MyCmpGet]
	private readonly Storage storage;

	private static readonly HashedString ANIM = "salt";

	[MyCmpReq]
	private readonly KAnimControllerBase animController;

	private readonly Dictionary<GameObject, Diner> communalDiners = new Dictionary<GameObject, Diner>();

	private static readonly HashedString COMMUNAL_DINING_EFFECT = "CommunalDining";

	private const int MORALE_MODIFIER = 1;

	private static readonly Descriptor COMMUNAL_DINING_DESCRIPTOR = new Descriptor(string.Format(UI.BUILDINGEFFECTS.COMMUNAL_DINING, 1), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.COMMUNAL_DINING, 1));

	public static int SeatCount => MultiMinionDiningTableConfig.SeatCount;

	public bool HasGarnish => Garnish.HasAny(storage);

	private static GameObject SpawnSeat(MultiMinionDiningTable diningTable, int diningTableCell, int seatIndex)
	{
		GameObject obj = Util.KInstantiate(Assets.GetPrefab(ApproachableLocator.ID), diningTable.transform.gameObject, "MultiMinionDiningSeat");
		TransformExtensions.SetPosition(position: Grid.CellToPosCBC(Grid.OffsetCell(diningTableCell, MultiMinionDiningTableConfig.seats[seatIndex].TableRelativeLocation), Grid.SceneLayer.Move), transform: obj.transform);
		obj.SetActive(value: true);
		obj.AddOrGet<Seat>().Initialize(seatIndex);
		obj.AddOrGet<Reservable>();
		obj.GetComponent<KPrefabID>().CopyTags(diningTable.GetComponent<KPrefabID>());
		return obj;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		int diningTableCell = Grid.PosToCell(this);
		for (int i = 0; i < SeatCount; i++)
		{
			SpawnSeat(this, diningTableCell, i);
		}
		animController.Play(ANIM);
		UpdateGarnishVisibility();
		storage.Subscribe(-1697596308, delegate
		{
			UpdateGarnishVisibility();
		});
	}

	public void UpdateGarnishVisibility()
	{
		Garnish active = Garnish.GetActive(storage);
		bool flag = active != null;
		UpdateGarnishOverride(active);
		Seat[] componentsInChildren = base.gameObject.GetComponentsInChildren<Seat>();
		foreach (Seat seat in componentsInChildren)
		{
			bool is_visible = flag && !seat.HasDiner;
			animController.SetSymbolVisiblity(seat.SaltSymbol, is_visible);
		}
	}

	private void UpdateGarnishOverride(Garnish activeGarnish)
	{
		if (!TryGetComponent<SymbolOverrideController>(out var component))
		{
			return;
		}
		KAnim.Build.Symbol symbol = activeGarnish?.GetOverrideSymbol();
		Seat[] componentsInChildren = base.gameObject.GetComponentsInChildren<Seat>();
		foreach (Seat seat in componentsInChildren)
		{
			if (symbol != null)
			{
				component.AddSymbolOverride(seat.SaltSymbol, symbol);
			}
			else
			{
				component.RemoveSymbolOverride(seat.SaltSymbol);
			}
		}
	}

	private void RegisterCommunalDiner(KPrefabID diner)
	{
		if (diner.TryGetComponent<Effects>(out var component))
		{
			component.Add(COMMUNAL_DINING_EFFECT, should_save: true);
		}
		else
		{
			Debug.LogWarning("Diner has no Effects component");
		}
		communalDiners[diner.gameObject] = new Diner(this, diner);
	}

	private void UnregisterCommunalDiner(KPrefabID dinerKpid)
	{
		if (communalDiners.TryGetValue(dinerKpid.gameObject, out var value))
		{
			value.CleanUp();
			communalDiners.Remove(dinerKpid.gameObject);
		}
	}

	private void OnDinerStartTalking(KPrefabID diner, object untypedStartTalkingEvent)
	{
		if (untypedStartTalkingEvent is ConversationManager.StartedTalkingEvent startedTalkingEvent && startedTalkingEvent.talker.TryGetComponent<KPrefabID>(out var component) && !(component != diner))
		{
			diner.AddTag(GameTags.WantsToTalk);
			diner.AddTag(GameTags.DoNotInterruptMe);
		}
	}

	private void OnDinerStopTalking(KPrefabID diner, object untypedStoppedTalker)
	{
		if (untypedStoppedTalker is GameObject gameObject && gameObject.TryGetComponent<KPrefabID>(out var component) && !(component != diner))
		{
			diner.RemoveTag(GameTags.WantsToTalk);
		}
	}

	private void OnDinerChanged(KPrefabID prevDiner, KPrefabID newDiner, int seatIndex)
	{
		Seat[] componentsInChildren = base.gameObject.GetComponentsInChildren<Seat>();
		bool is_visible = newDiner == null && HasGarnish;
		animController.SetSymbolVisiblity(componentsInChildren[seatIndex].SaltSymbol, is_visible);
		if (prevDiner != null && communalDiners.ContainsKey(prevDiner.gameObject))
		{
			UnregisterCommunalDiner(prevDiner);
		}
		if (newDiner != null)
		{
			int num = 0;
			Seat[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].HasDiner)
				{
					num++;
					if (num > 1)
					{
						break;
					}
				}
			}
			if (num <= 1)
			{
				return;
			}
			array = componentsInChildren;
			foreach (Seat seat in array)
			{
				if (!(seat.Diner == null) && !communalDiners.ContainsKey(seat.Diner.gameObject))
				{
					RegisterCommunalDiner(seat.Diner);
				}
			}
		}
		else
		{
			if (communalDiners.Count != 1)
			{
				return;
			}
			Seat[] array = componentsInChildren;
			foreach (Seat seat2 in array)
			{
				if (!(seat2.Diner == null))
				{
					UnregisterCommunalDiner(seat2.Diner);
				}
			}
		}
	}

	List<Descriptor> IGameObjectEffectDescriptor.GetDescriptors(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor> { COMMUNAL_DINING_DESCRIPTOR };
		if (storage != null)
		{
			foreach (Garnish item in Garnish.All)
			{
				if (storage.Has(item.itemTag))
				{
					list.Add(item.descriptor);
				}
			}
		}
		return list;
	}
}
