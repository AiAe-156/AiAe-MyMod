using System;
using UnityEngine;

internal class BeckonFromSpaceStates : GameStateMachine<BeckonFromSpaceStates, BeckonFromSpaceStates.Instance, IStateMachineTarget, BeckonFromSpaceStates.Def>
{
	public class Def : BaseDef
	{
		public Grid.SceneLayer sceneLayer;

		public HashedString[] choirAnims = new HashedString[1] { "reply_loop" };
	}

	public new class Instance : GameInstance
	{
		public BeckoningMonitor.SongChance ChosenSong;

		private BeckoningMonitor.Instance monitor;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.WantsToBeckon);
			monitor = base.gameObject.GetSMI<BeckoningMonitor.Instance>();
		}

		public void ChooseSong()
		{
			float num = UnityEngine.Random.value;
			BeckoningMonitor.SongChance chosenSong = null;
			foreach (BeckoningMonitor.SongChance songChance in monitor.songChances)
			{
				num -= songChance.weight;
				if (num <= 0f)
				{
					chosenSong = songChance;
					break;
				}
			}
			ChosenSong = chosenSong;
		}
	}

	public class BeckoningState : State
	{
		public State pre;

		public State loop;

		public State pst;
	}

	public BeckoningState beckoning;

	public State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = beckoning;
		beckoning.ToggleMainStatusItem(Db.Get().CreatureStatusItems.Beckoning).Enter(ChooseSong).DefaultState(beckoning.pre);
		beckoning.pre.PlayAnim((Func<Instance, string>)GetSongAnimPre, KAnim.PlayMode.Once).OnAnimQueueComplete(beckoning.loop);
		beckoning.loop.PlayAnim((Func<Instance, string>)GetSongAnimLoop, KAnim.PlayMode.Once).Enter(MooEchoFX).OnAnimQueueComplete(beckoning.pst);
		beckoning.pst.PlayAnim((Func<Instance, string>)GetSongAnimPst, KAnim.PlayMode.Once).OnAnimQueueComplete(behaviourcomplete);
		behaviourcomplete.PlayAnim("idle_loop", KAnim.PlayMode.Loop).Enter(DoBeckon).Enter(MooCheer)
			.BehaviourComplete(GameTags.Creatures.WantsToBeckon);
	}

	public static string GetSongAnimPre(Instance smi)
	{
		return smi.ChosenSong.singAnimPre;
	}

	public static string GetSongAnimLoop(Instance smi)
	{
		return smi.ChosenSong.singAnimLoop;
	}

	public static string GetSongAnimPst(Instance smi)
	{
		return smi.ChosenSong.singAnimPst;
	}

	private static void ChooseSong(Instance smi)
	{
		smi.ChooseSong();
	}

	private static void MooEchoFX(Instance smi)
	{
		KBatchedAnimController kBatchedAnimController = FXHelpers.CreateEffect("moo_call_fx_kanim", smi.master.transform.position);
		kBatchedAnimController.destroyOnAnimComplete = true;
		kBatchedAnimController.Play("moo_call");
	}

	private static Util.IterationInstruction mooCheerVisitor(object obj, Instance smi)
	{
		KPrefabID kPrefabID = (obj as Pickupable).KPrefabID;
		if (kPrefabID.gameObject == smi.gameObject)
		{
			return Util.IterationInstruction.Continue;
		}
		if (kPrefabID.HasTag("Moo"))
		{
			AnimInterruptMonitor.Instance sMI = kPrefabID.GetSMI<AnimInterruptMonitor.Instance>();
			if (sMI != null)
			{
				kPrefabID.GetSMI<AnimInterruptMonitor.Instance>().PlayAnimSequence(smi.def.choirAnims);
			}
		}
		return Util.IterationInstruction.Continue;
	}

	private static void MooCheer(Instance smi)
	{
		Vector3 position = smi.transform.GetPosition();
		Extents extents = new Extents((int)position.x, (int)position.y, 15);
		GameScenePartitioner.Instance.VisitEntries(extents.x, extents.y, extents.width, extents.height, GameScenePartitioner.Instance.pickupablesLayer, mooCheerVisitor, smi);
	}

	private static void DoBeckon(Instance smi)
	{
		Db.Get().Amounts.Beckoning.Lookup(smi.gameObject).value = 0f;
		WorldContainer myWorld = smi.GetMyWorld();
		Vector3 position = smi.transform.position;
		float num = myWorld.Height + myWorld.WorldOffset.y - 1;
		float layerZ = Grid.GetLayerZ(smi.def.sceneLayer);
		float num2 = num - position.y;
		float num3 = num2 * Mathf.Tan(MathF.PI / 12f);
		float num4 = position.x + (float)UnityEngine.Random.Range(-5, 5);
		float num5 = num4 - num3;
		float num6 = num4 + num3;
		float num7 = position.x;
		bool customInitialFlip = false;
		if (num5 > (float)myWorld.WorldOffset.x && num5 < (float)(myWorld.WorldOffset.x + myWorld.Width))
		{
			num7 = num5;
			customInitialFlip = false;
		}
		else if (num5 > (float)myWorld.WorldOffset.x && num5 < (float)(myWorld.WorldOffset.x + myWorld.Width))
		{
			num7 = num6;
			customInitialFlip = true;
		}
		DebugUtil.DevAssert(myWorld.ContainsPoint(new Vector2(num7, num)), "Gassy Moo spawned outside world bounds");
		GameObject gameObject = Util.KInstantiate(position: new Vector3(num7, num, layerZ), original: Assets.GetPrefab(smi.ChosenSong.meteorID), rotation: Quaternion.identity);
		GassyMooComet component = gameObject.GetComponent<GassyMooComet>();
		if (component != null)
		{
			component.spawnWithOffset = true;
			if (num7 != position.x)
			{
				component.SetCustomInitialFlip(customInitialFlip);
			}
		}
		gameObject.SetActive(value: true);
	}
}
