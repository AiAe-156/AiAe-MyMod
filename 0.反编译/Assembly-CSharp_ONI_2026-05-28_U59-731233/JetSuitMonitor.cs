using System;
using UnityEngine;

public class JetSuitMonitor : GameStateMachine<JetSuitMonitor, JetSuitMonitor.Instance>
{
	public new class Instance : GameInstance
	{
		public HelmetController helmetController;

		public Navigator navigator;

		public JetSuitTank jet_suit_tank;

		public Instance(IStateMachineTarget master, GameObject owner)
			: base(master)
		{
			base.sm.owner.Set(owner, base.smi);
			helmetController = master.GetComponent<HelmetController>();
			navigator = owner.GetComponent<Navigator>();
			jet_suit_tank = master.GetComponent<JetSuitTank>();
		}
	}

	public State off;

	public State flying;

	public TargetParameter owner;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = off;
		Target(owner);
		off.EventTransition(GameHashes.PathAdvanced, flying, ShouldStartFlying);
		flying.Enter(StartFlying).Exit(StopFlying).EventTransition(GameHashes.PathAdvanced, off, ShouldStopFlying)
			.Update(Emit);
	}

	public static bool ShouldStartFlying(Instance smi)
	{
		return (bool)smi.navigator && smi.navigator.CurrentNavType == NavType.Hover;
	}

	public static bool ShouldStopFlying(Instance smi)
	{
		return !smi.navigator || smi.navigator.CurrentNavType != NavType.Hover;
	}

	public static void StartFlying(Instance smi)
	{
	}

	public static void StopFlying(Instance smi)
	{
	}

	public static void Emit(Instance smi, float dt)
	{
		if (!smi.navigator)
		{
			return;
		}
		GameObject gameObject = smi.sm.owner.Get(smi);
		if (!gameObject)
		{
			return;
		}
		int num = Grid.PosToCell(gameObject.transform.GetPosition());
		float a = 0.2f * dt;
		a = Mathf.Min(a, smi.jet_suit_tank.amount);
		smi.jet_suit_tank.amount -= a;
		float num2 = a * 0.25f;
		if (num2 > float.Epsilon)
		{
			Vector3 position = gameObject.transform.position;
			Vector3 position2 = position;
			Vector3 position3 = position;
			Vector3 vector = Vector3.down;
			if (smi.helmetController.jet_anim != null)
			{
				KBatchedAnimController jet_anim = smi.helmetController.jet_anim;
				bool symbolVisible;
				Matrix4x4 symbolTransform = jet_anim.GetSymbolTransform("left_fire", out symbolVisible);
				Matrix4x4 symbolTransform2 = jet_anim.GetSymbolTransform("right_fire", out symbolVisible);
				position2 = symbolTransform.GetColumn(3);
				position3 = symbolTransform2.GetColumn(3);
				float f = Quaternion.LookRotation((Vector3)symbolTransform.GetColumn(2), (Vector3)symbolTransform.GetColumn(1)).eulerAngles.z * (MathF.PI / 180f);
				vector = new Vector3(0f - Mathf.Sin(f), Mathf.Cos(f));
				position2 += vector.normalized * 0.6f;
				position3 += vector.normalized * 0.6f;
			}
			float mass = num2 / 2f;
			float num3 = 0.5f;
			int co2Cell = Grid.PosToCell(position);
			CO2Manager.instance.SpawnExhaust(position2, vector.normalized * num3, co2Cell, mass, 373.15f);
			CO2Manager.instance.SpawnExhaust(position3, vector.normalized * num3, co2Cell, mass, 373.15f);
		}
		if (smi.jet_suit_tank.amount == 0f)
		{
			smi.navigator.AddTag(GameTags.JetSuitOutOfFuel);
			smi.navigator.SetCurrentNavType(NavType.Floor);
		}
	}
}
