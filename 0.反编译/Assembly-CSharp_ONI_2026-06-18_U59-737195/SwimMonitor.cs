using System;
using System.Collections.Generic;
using Database;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class SwimMonitor : GameStateMachine<SwimMonitor, SwimMonitor.Instance, IStateMachineTarget>
{
	public new class Instance : GameInstance
	{
		[MyCmpReq]
		public MinionResume resume;

		public Navigator navigator;

		public KBatchedAnimController animController;

		public KSelectable selectable;

		public SkillPerk swimStaminaPerk;

		public SkillPerk swimAthleticPerk;

		public NavType previousNavType;

		public bool wasInLiquid;

		public Instance(IStateMachineTarget master)
			: base(master)
		{
			navigator = GetComponent<Navigator>();
			animController = GetComponent<KBatchedAnimController>();
			selectable = GetComponent<KSelectable>();
			previousNavType = navigator.CurrentNavType;
			swimStaminaPerk = Db.Get().SkillPerks.IncreaseSwimmerStaminaInLiquid;
			swimAthleticPerk = Db.Get().SkillPerks.IncreaseSwimmerAthleticsInLiquid;
			wasInLiquid = Grid.IsSubstantialLiquid(navigator.cachedCell);
			if (wasInLiquid)
			{
				ApplyAttributeModifier(inLiquidStaminaModifier, add: true);
			}
			CheckSwimSkill(this);
		}

		public bool CanSwim()
		{
			return base.sm.hasSwimSkill.Get(this);
		}

		public void ApplyAttributeModifier(AttributeModifier modifier, bool add)
		{
			Klei.AI.Attributes attributes = base.gameObject.GetAttributes();
			if (add)
			{
				attributes.Add(modifier);
			}
			else
			{
				attributes.Remove(modifier);
			}
		}
	}

	public static StatusItem HasSuitSwimPenalty = new StatusItem("HasSuitSwimPenalty", DUPLICANTS.STATUSITEMS.HASSUITSWIMPENALTY.NAME, DUPLICANTS.STATUSITEMS.HASSUITSWIMPENALTY.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);

	public static Dictionary<HashedString, HashedString> SurfaceSwimOverride = new Dictionary<HashedString, HashedString> { { "swim_swim_1_0_loop", "shallow_swim_1_0_loop" } };

	public static Dictionary<HashedString, Dictionary<HashedString, HashedString>> transitionAnims = new Dictionary<HashedString, Dictionary<HashedString, HashedString>>
	{
		{
			"treading_loop",
			new Dictionary<HashedString, HashedString>
			{
				{ "shallow_swim_1_0_loop", "treading_trans_shallow_swim_1_0" },
				{ "swim_swim_1_0_loop", "treading_trans_swim_1_0" },
				{ "swim_swim_1_-1_loop", "treading_trans_swim_1_-1" },
				{ "swim_swim_0_-1_loop", "treading_trans_swim_0_-1" }
			}
		},
		{
			"shallow_swim_1_0_loop",
			new Dictionary<HashedString, HashedString>
			{
				{ "swim_swim_1_0_loop", "shallow_trans_swim_1_0" },
				{ "swim_swim_1_-1_loop", "shallow_trans_swim_1_-1" },
				{ "swim_swim_0_-1_loop", "shallow_trans_swim_0_-1" }
			}
		},
		{
			"swim_swim_1_0_loop",
			new Dictionary<HashedString, HashedString>
			{
				{ "shallow_swim_1_0_loop", "horizontal_trans_shallow_swim_1_0" },
				{ "swim_swim_1_-1_loop", "horizontal_trans_swim_1_-1" },
				{ "swim_swim_0_-1_loop", "horizontal_trans_swim_0_-1" },
				{ "swim_swim_0_1_loop", "horizontal_trans_swim_0_1" },
				{ "swim_swim_1_1_loop", "horizontal_trans_swim_1_1" }
			}
		},
		{
			"swim_swim_0_1_loop",
			new Dictionary<HashedString, HashedString>
			{
				{ "swim_swim_1_-1_loop", "up_trans_swim_1_-1" },
				{ "swim_swim_0_-1_loop", "up_trans_swim_0_-1" },
				{ "swim_swim_0_1_loop", "up_trans_swim_1_0" },
				{ "swim_swim_1_1_loop", "up_trans_swim_1_1" }
			}
		},
		{
			"swim_swim_0_-1_loop",
			new Dictionary<HashedString, HashedString>
			{
				{ "swim_swim_1_0_loop", "down_trans_swim_1_0" },
				{ "swim_swim_0_1_loop", "down_trans_swim_0_1" },
				{ "swim_swim_1_-1_loop", "down_trans_swim_1_-1" },
				{ "swim_swim_1_1_loop", "down_trans_swim_1_1" }
			}
		},
		{
			"swim_swim_1_1_loop",
			new Dictionary<HashedString, HashedString>
			{
				{ "swim_swim_1_0_loop", "up_diagonal_trans_swim_1_0" },
				{ "swim_swim_0_1_loop", "up_diagonal_trans_swim_0_1" },
				{ "swim_swim_1_-1_loop", "up_diagonal_trans_swim_1_-1" },
				{ "swim_swim_0_-1_loop", "up_diagonal_trans_swim_0_-1" },
				{ "shallow_swim_1_0_loop", "up_diagonal_trans_shallow_swim_1_0" }
			}
		},
		{
			"swim_swim_1_-1_loop",
			new Dictionary<HashedString, HashedString>
			{
				{ "swim_swim_0_1_loop", "down_diagonal_trans_swim_0_1" },
				{ "swim_swim_1_0_loop", "down_diagonal_trans_swim_1_0" },
				{ "swim_swim_1_1_loop", "down_diagonal_trans_swim_1_1" },
				{ "swim_swim_0_-1_loop", "down_diagonal_trans_swim_0_-1" }
			}
		}
	};

	public static float OffsetEpsilon = 0.0001f;

	private static AttributeModifier swimmingStaminaModifier = new AttributeModifier(Db.Get().Amounts.Stamina.deltaAttribute.Id, 1f / 15f, DUPLICANTS.MODIFIERS.SWIMMINGSTAMINA.NAME);

	private static AttributeModifier swimmingAthleticsModifier = new AttributeModifier(TUNING.EQUIPMENT.ATTRIBUTE_MOD_IDS.ATHLETICS, 3f, DUPLICANTS.MODIFIERS.SWIMMINGATHLETICS.NAME);

	private static AttributeModifier inLiquidStaminaModifier = new AttributeModifier(Db.Get().Amounts.Stamina.deltaAttribute.Id, -1f / 15f, DUPLICANTS.MODIFIERS.INLIQUIDSTAMINA.NAME);

	public State cannotSwim;

	public State canSwim;

	public BoolParameter hasSwimSkill;

	public BoolParameter hasSwimSkill2;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = cannotSwim;
		root.EventHandler(GameHashes.RolesUpdated, (Instance smi) => Game.Instance, CheckSwimSkill).EventHandler(GameHashes.PathAdvanced, delegate(Instance smi, object data)
		{
			OnPathAdvanced(smi, data);
		});
		cannotSwim.Enter(delegate(Instance smi)
		{
			if (smi.navigator.CurrentNavType == NavType.Swim)
			{
				smi.navigator.SetCurrentNavType(NavType.Floor);
				smi.navigator.Stop();
			}
		}).ParamTransition(hasSwimSkill, canSwim, GameStateMachine<SwimMonitor, Instance, IStateMachineTarget, object>.IsTrue);
		canSwim.ParamTransition(hasSwimSkill, cannotSwim, GameStateMachine<SwimMonitor, Instance, IStateMachineTarget, object>.IsFalse).Enter(delegate(Instance smi)
		{
			bool add = smi.navigator.CurrentNavType == NavType.Swim;
			if (smi.resume.HasPerk(smi.swimStaminaPerk))
			{
				smi.ApplyAttributeModifier(swimmingStaminaModifier, add);
			}
			if (smi.resume.HasPerk(smi.swimAthleticPerk))
			{
				smi.ApplyAttributeModifier(swimmingAthleticsModifier, add);
			}
			smi.previousNavType = smi.navigator.CurrentNavType;
		}).ToggleAnims("anim_loco_swim_kanim")
			.Update(delegate(Instance smi, float dt)
			{
				UpdateSwimOffset(smi);
			});
	}

	private static void UpdateSwimOffset(Instance smi)
	{
		if (!smi.navigator.IsMoving() && smi.navigator.CurrentNavType == NavType.Swim)
		{
			SetSwimOffset(smi);
		}
	}

	private static void SetSwimOffset(Instance smi)
	{
		Vector3 offset = smi.animController.Offset;
		offset.y = ComputeSwimOffsetY(smi.navigator.cachedCell);
		if (MathF.Abs(offset.y - smi.animController.Offset.y) > OffsetEpsilon)
		{
			smi.animController.Offset = offset;
		}
	}

	public static void CheckSwimSkill(Instance smi)
	{
		smi.sm.hasSwimSkill.Set(smi.resume.HasPerk(Db.Get().SkillPerks.CanSwim), smi);
	}

	public static float ComputeSwimOffsetY(int cell)
	{
		return Mathf.Clamp((Grid.Mass[cell] / 1000f - 1f) * 0.5f, -0.6f, 0f);
	}

	public void OnPathAdvanced(Instance smi, object data)
	{
		bool num = smi.sm.hasSwimSkill.Get(smi);
		bool flag = smi.navigator.CurrentNavType == NavType.Swim;
		if (num)
		{
			bool flag2 = false;
			if (flag)
			{
				flag2 = (smi.navigator.flags & PathFinder.PotentialPath.Flags.HasAtmoSuit) != PathFinder.PotentialPath.Flags.None || (smi.navigator.flags & PathFinder.PotentialPath.Flags.HasJetPack) != PathFinder.PotentialPath.Flags.None || (smi.navigator.flags & PathFinder.PotentialPath.Flags.HasLeadSuit) != 0;
			}
			bool flag3 = smi.previousNavType == NavType.Swim;
			if (flag != flag3)
			{
				if (smi.resume.HasPerk(smi.swimStaminaPerk))
				{
					smi.ApplyAttributeModifier(swimmingStaminaModifier, flag);
				}
				if (smi.resume.HasPerk(smi.swimAthleticPerk))
				{
					smi.ApplyAttributeModifier(swimmingAthleticsModifier, flag);
				}
			}
			smi.previousNavType = smi.navigator.CurrentNavType;
			smi.selectable.ToggleStatusItem(HasSuitSwimPenalty, flag2);
		}
		bool flag4 = flag || Grid.IsSubstantialLiquid(smi.navigator.cachedCell);
		if (flag4 != smi.wasInLiquid)
		{
			smi.ApplyAttributeModifier(inLiquidStaminaModifier, flag4);
			smi.wasInLiquid = flag4;
		}
	}
}
