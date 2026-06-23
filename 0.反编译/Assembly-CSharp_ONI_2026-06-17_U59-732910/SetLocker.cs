using System;
using KSerialization;
using STRINGS;
using UnityEngine;

public class SetLocker : StateMachineComponent<SetLocker.StatesInstance>, ISidescreenButtonControl
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, SetLocker, object>.GameInstance
	{
		public StatesInstance(SetLocker master)
			: base(master)
		{
		}

		public override void StartSM()
		{
			base.StartSM();
			base.smi.Subscribe(-702296337, delegate
			{
				if (base.smi.master.dropOnDeconstruct && base.smi.IsInsideState(base.smi.sm.closed))
				{
					base.smi.master.DropContents();
				}
			});
		}
	}

	public class States : GameStateMachine<States, StatesInstance, SetLocker>
	{
		public State closed;

		public State being_worked;

		public State open;

		public State off;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = closed;
			base.serializable = SerializeType.Both_DEPRECATED;
			closed.PlayAnim("on").Enter(delegate(StatesInstance smi)
			{
				if (smi.master.machineSound != null)
				{
					LoopingSounds component = smi.master.GetComponent<LoopingSounds>();
					if (component != null)
					{
						component.StartSound(GlobalAssets.GetSound(smi.master.machineSound));
					}
				}
			});
			being_worked.DoNothing();
			open.PlayAnim("working_pre").QueueAnim("working_loop").QueueAnim("working_pst")
				.OnAnimQueueComplete(off)
				.Exit(delegate(StatesInstance smi)
				{
					smi.master.DropContents();
				});
			off.PlayAnim("off").Enter(delegate(StatesInstance smi)
			{
				if (smi.master.machineSound != null)
				{
					LoopingSounds component = smi.master.GetComponent<LoopingSounds>();
					if (component != null)
					{
						component.StopSound(GlobalAssets.GetSound(smi.master.machineSound));
					}
				}
			});
		}
	}

	[MyCmpAdd]
	private Prioritizable prioritizable;

	public string[][] possible_contents_ids;

	public string machineSound;

	public string overrideAnim;

	public Vector2I dropOffset = Vector2I.zero;

	public int[] numDataBanks;

	[Serialize]
	private string[] contents;

	public bool dropOnDeconstruct;

	public bool skipAnim;

	[Serialize]
	private bool pendingRummage;

	[Serialize]
	private bool used;

	private Chore chore;

	public string SidescreenButtonText
	{
		get
		{
			if (used)
			{
				return UI.USERMENUACTIONS.OPENPOI.ALREADY_RUMMAGED;
			}
			if (chore != null)
			{
				return UI.USERMENUACTIONS.OPENPOI.NAME_OFF;
			}
			return UI.USERMENUACTIONS.OPENPOI.NAME;
		}
	}

	public string SidescreenButtonTooltip
	{
		get
		{
			if (used)
			{
				return UI.USERMENUACTIONS.OPENPOI.TOOLTIP_ALREADYRUMMAGED;
			}
			if (chore != null)
			{
				return UI.USERMENUACTIONS.OPENPOI.TOOLTIP_OFF;
			}
			return UI.USERMENUACTIONS.OPENPOI.TOOLTIP;
		}
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
	}

	public void ChooseContents()
	{
		contents = possible_contents_ids[UnityEngine.Random.Range(0, possible_contents_ids.GetLength(0))];
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
		if (contents == null)
		{
			ChooseContents();
		}
		else
		{
			string[] array = contents;
			for (int i = 0; i < array.Length; i++)
			{
				if (Assets.GetPrefab(array[i]) == null)
				{
					ChooseContents();
					break;
				}
			}
		}
		if (pendingRummage)
		{
			ActivateChore();
		}
	}

	public void DropContents()
	{
		if (contents == null)
		{
			return;
		}
		if (DlcManager.IsExpansion1Active() && numDataBanks.Length >= 2)
		{
			int num = UnityEngine.Random.Range(numDataBanks[0], numDataBanks[1]);
			for (int i = 0; i <= num; i++)
			{
				Scenario.SpawnPrefab(Grid.PosToCell(base.gameObject), dropOffset.x, dropOffset.y, "OrbitalResearchDatabank", Grid.SceneLayer.Front).SetActive(value: true);
				PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, Assets.GetPrefab("OrbitalResearchDatabank".ToTag()).GetProperName(), base.smi.master.transform);
			}
		}
		for (int j = 0; j < contents.Length; j++)
		{
			GameObject gameObject = Scenario.SpawnPrefab(Grid.PosToCell(base.gameObject), dropOffset.x, dropOffset.y, contents[j], Grid.SceneLayer.Front);
			if (gameObject != null)
			{
				gameObject.SetActive(value: true);
				PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, Assets.GetPrefab(contents[j].ToTag()).GetProperName(), base.smi.master.transform);
			}
		}
		base.gameObject.Trigger(-372600542, (object)this);
	}

	private void OnClickOpen()
	{
		ActivateChore();
	}

	private void OnClickCancel()
	{
		CancelChore();
	}

	public void ActivateChore(object param = null)
	{
		if (chore == null)
		{
			Prioritizable.AddRef(base.gameObject);
			Trigger(1980521255);
			pendingRummage = true;
			GetComponent<Workable>().SetWorkTime(1.5f);
			chore = new WorkChore<Workable>(Db.Get().ChoreTypes.EmptyStorage, this, null, run_until_complete: true, delegate
			{
				CompleteChore();
			}, delegate
			{
				base.smi.GoTo(base.smi.sm.being_worked);
			}, delegate
			{
				OnChoreEnd();
			}, allow_in_red_alert: true, null, ignore_schedule_block: false, only_when_operational: true, Assets.GetAnim(overrideAnim), is_preemptable: false, allow_in_context_menu: true, allow_prioritization: true, PriorityScreen.PriorityClass.high);
		}
	}

	public void CancelChore(object param = null)
	{
		if (chore != null)
		{
			pendingRummage = false;
			Prioritizable.RemoveRef(base.gameObject);
			Trigger(1980521255);
			chore.Cancel("User cancelled");
			chore = null;
		}
	}

	private void OnChoreEnd()
	{
		if (skipAnim && chore != null)
		{
			base.smi.GoTo(base.smi.sm.closed);
		}
	}

	private void CompleteChore()
	{
		used = true;
		if (skipAnim)
		{
			DropContents();
			base.smi.GoTo(base.smi.sm.off);
		}
		else
		{
			base.smi.GoTo(base.smi.sm.open);
		}
		chore = null;
		pendingRummage = false;
		Game.Instance.userMenu.Refresh(base.gameObject);
		Prioritizable.RemoveRef(base.gameObject);
	}

	public bool SidescreenEnabled()
	{
		return true;
	}

	public int HorizontalGroupID()
	{
		return -1;
	}

	public void OnSidescreenButtonPressed()
	{
		if (chore == null)
		{
			OnClickOpen();
		}
		else
		{
			OnClickCancel();
		}
	}

	public bool SidescreenButtonInteractable()
	{
		return !used;
	}

	public int ButtonSideScreenSortOrder()
	{
		return 20;
	}

	public void SetButtonTextOverride(ButtonMenuTextOverride text)
	{
		throw new NotImplementedException();
	}
}
