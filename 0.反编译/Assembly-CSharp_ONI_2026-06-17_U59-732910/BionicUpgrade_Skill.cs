public class BionicUpgrade_Skill : GameStateMachine<BionicUpgrade_Skill, BionicUpgrade_Skill.Instance, IStateMachineTarget, BionicUpgrade_Skill.Def>
{
	public class Def : BaseDef
	{
		public string SKILL_ID;
	}

	public new class Instance : GameInstance
	{
		private MinionResume resume;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			resume = GetComponent<MinionResume>();
		}

		public void ApplySkill()
		{
			resume.GrantSkill(base.def.SKILL_ID);
		}

		public void RemoveSkill()
		{
			resume.UngrantSkill(base.def.SKILL_ID);
		}
	}

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = root;
		root.Enter(EnableEffect).Exit(DisableEffect);
	}

	public static void EnableEffect(Instance smi)
	{
		smi.ApplySkill();
	}

	public static void DisableEffect(Instance smi)
	{
		smi.RemoveSkill();
	}
}
