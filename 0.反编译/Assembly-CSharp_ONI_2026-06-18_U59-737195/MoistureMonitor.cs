using System.Collections.Generic;
using System.Text;
using Database;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class MoistureMonitor : GameStateMachine<MoistureMonitor, MoistureMonitor.Instance, IStateMachineTarget, MoistureMonitor.Def>
{
	public class Def : BaseDef, IGameObjectEffectDescriptor
	{
		private const float DEFAULT_ON_DRY_LAND_MODIFIER = 0.05f;

		private const float DEFAULT_DRY_RATE = -0.05f;

		private const float DEFAULT_SOAK_RATE = 10f;

		public float onDryLandModifier = 0.05f;

		public SimHashes lubricant;

		public float lubricantTemperatureKelvin;

		[Tooltip("Stop producing more Mucus when this much is stored")]
		public float sufficientMoistureThreshold = 10f;

		[Tooltip("Decrease moisture at this rate while on dry land")]
		public float dryRate = -0.05f;

		[Tooltip("Increase moisture at this rate while inside liquid")]
		public float soakRate = 10f;

		public override void Configure(GameObject prefab)
		{
			Database.Amounts amounts = Db.Get().Amounts;
			List<string> initialAmounts = prefab.GetComponent<Modifiers>().initialAmounts;
			initialAmounts.Add(amounts.Moisture.Id);
			initialAmounts.Add(amounts.Mucus.Id);
		}

		private static void AppendMucusModifierTooltipBullet(StringBuilder tooltipBuilder, AttributeModifier modifier, bool showSign)
		{
			string text = (modifier.IsMultiplier ? GameUtil.GetFormattedPercent(modifier.Value * 100f) : GameUtil.GetFormattedMass(modifier.Value, GameUtil.TimeSlice.PerCycle));
			if (showSign)
			{
				text = GameUtil.AddPositiveSign(text, modifier.Value > 0f);
			}
			tooltipBuilder.AppendFormat(DUPLICANTS.ATTRIBUTES.MODIFIER_ENTRY, modifier.GetDescription(), text);
		}

		public float GetMaxModification()
		{
			return onDryLandModifier;
		}

		public List<Descriptor> GetDescriptors(GameObject obj)
		{
			string newValue = ElementLoader.FindElementByHash(lubricant).tag.ProperName();
			float delta = onDryLandModifier;
			AmountInstance amountInstance = Db.Get().Amounts.Mucus.Lookup(obj);
			if (amountInstance != null)
			{
				delta = amountInstance.GetDelta();
			}
			string formattedMass = GameUtil.GetFormattedMass(delta, GameUtil.TimeSlice.PerCycle);
			string txt = GlobalStringBuilderPool.ReturnAndFree(GlobalStringBuilderPool.Alloc().Append(UI.BUILDINGEFFECTS.MUCUS_SECRETION).Replace("{Item}", newValue)
				.Replace("{Rate}", formattedMass));
			StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
			stringBuilder.Append(UI.BUILDINGEFFECTS.TOOLTIPS.MUCUS_SECRETION);
			stringBuilder.Replace("{Item}", newValue);
			stringBuilder.Replace("{Rate}", formattedMass);
			if (amountInstance != null)
			{
				ArrayRef<AttributeModifier> modifiers = amountInstance.deltaAttribute.Modifiers;
				int num = -1;
				for (int i = 0; i < modifiers.Count; i++)
				{
					if (modifiers[i].GetDescription() == CREATURES.MODIFIERS.MUCUS.BASE_RATE)
					{
						num = i;
						break;
					}
				}
				if (num >= 0)
				{
					AppendMucusModifierTooltipBullet(stringBuilder, modifiers[num], showSign: false);
				}
				for (int j = 0; j < modifiers.Count; j++)
				{
					if (j != num)
					{
						AppendMucusModifierTooltipBullet(stringBuilder, modifiers[j], showSign: true);
					}
				}
			}
			string tooltip = GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
			return new List<Descriptor>
			{
				new Descriptor(txt, tooltip)
			};
		}
	}

	public new class Instance : GameInstance
	{
		private const float UNHAPPY_MUCUS_MODIFIER = -0.5f;

		private const float WILD_MUCUS_MODIFIER = -0.75f;

		[MyCmpReq]
		public Effects effects;

		public AmountInstance mucusAmount;

		public AttributeModifier onDryLandMucusModifier;

		public AttributeModifier wildMucusModifier;

		public AttributeModifier unhappyMucusModifier;

		public Klei.AI.Attributes attributes;

		public WildnessMonitor.Instance wildnessMonitor;

		public AmountInstance moisture;

		public AttributeModifier baseMoistureModifier;

		public AttributeModifier wetMoistureModifier;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			Database.Amounts amounts = Db.Get().Amounts;
			moisture = amounts.Moisture.Lookup(base.gameObject);
			moisture.value = moisture.GetMax();
			baseMoistureModifier = new AttributeModifier(moisture.amount.deltaAttribute.Id, def.dryRate, CREATURES.MODIFIERS.MOISTURE_LOSS_RATE.NAME);
			wetMoistureModifier = new AttributeModifier(moisture.amount.deltaAttribute.Id, def.soakRate, CREATURES.MODIFIERS.MOISTURE_GAIN_RATE.NAME);
			attributes = master.gameObject.GetAttributes();
			mucusAmount = amounts.Mucus.Lookup(base.gameObject);
			mucusAmount.value = mucusAmount.GetMax();
			onDryLandMucusModifier = new AttributeModifier(mucusAmount.amount.deltaAttribute.Id, def.onDryLandModifier, CREATURES.MODIFIERS.MUCUS.ON_DRY_LAND);
			unhappyMucusModifier = new AttributeModifier(mucusAmount.amount.deltaAttribute.Id, -0.5f, CREATURES.MODIFIERS.MUCUS.UNHAPPY, is_multiplier: true);
			wildMucusModifier = new AttributeModifier(mucusAmount.amount.deltaAttribute.Id, -0.75f, CREATURES.MODIFIERS.MUCUS.WILD, is_multiplier: true);
		}

		public void ProduceLubricant()
		{
			float value = mucusAmount.value;
			if (value > 0f)
			{
				BubbleManager.instance.SpawnBubble(base.def.lubricant, base.transform.GetPosition(), value, base.def.lubricantTemperatureKelvin, BubbleManager.Disease.None);
				Trigger(1151073968);
				effects.Add(RECENTLY_PRODUCED_LUBRICANT_EFFECT, should_save: true);
				mucusAmount.value = 0f;
			}
		}
	}

	private State onDryLand;

	private State inLiquid;

	private State secreting;

	public State dead;

	public static readonly HashedString RECENTLY_PRODUCED_LUBRICANT_EFFECT = "RecentlyProducedLubricant";

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = onDryLand;
		root.ToggleStateMachine((Instance smi) => new LubricatedMovementMonitor.Instance(smi.master)).EventHandler(GameHashes.Happy, delegate(Instance smi)
		{
			ToggleUnhappyModifier(smi, enabled: false);
		}).EventHandler(GameHashes.Unhappy, delegate(Instance smi)
		{
			ToggleUnhappyModifier(smi, enabled: true);
		})
			.EventHandler(GameHashes.TagsChanged, UpdateTags)
			.EventTransition(GameHashes.Died, dead)
			.Enter(delegate(Instance smi)
			{
				if (smi.HasTag(GameTags.Creatures.Wild))
				{
					smi.attributes.Add(smi.wildMucusModifier);
				}
			});
		onDryLand.UpdateTransition(inLiquid, IsInLiquid).ToggleAttributeModifier("dry", (Instance smi) => smi.baseMoistureModifier).ToggleAttributeModifier("mucus", (Instance smi) => smi.onDryLandMucusModifier)
			.UpdateTransition(secreting, IsMucusEnough);
		inLiquid.UpdateTransition(onDryLand, (Instance smi, float dt) => !IsInLiquid(smi, dt)).ToggleAttributeModifier("wet", (Instance smi) => smi.wetMoistureModifier);
		secreting.ToggleBehaviour(GameTags.Creatures.Behaviours.SecretingMucusBehavior, CanProduceLubricant, delegate(Instance smi)
		{
			smi.GoTo(onDryLand);
		});
		dead.DoNothing();
	}

	private static bool IsInLiquid(Instance smi, float _)
	{
		return Grid.IsSubstantialLiquid(Grid.PosToCell(smi), 0.02f);
	}

	private bool IsMucusEnough(Instance smi, float _)
	{
		return smi.mucusAmount.value >= (smi.HasTag(GameTags.Creatures.Dry) ? 2f : 10f);
	}

	private void UpdateTags(Instance smi, object data)
	{
		if (data is TagChangedEventData tagChangedEventData && tagChangedEventData.tag == GameTags.Creatures.Wild)
		{
			if (tagChangedEventData.added)
			{
				smi.attributes.Add(smi.wildMucusModifier);
			}
			else
			{
				smi.attributes.Remove(smi.wildMucusModifier);
			}
		}
	}

	private void ToggleUnhappyModifier(Instance smi, bool enabled)
	{
		if (enabled)
		{
			smi.attributes.Add(smi.unhappyMucusModifier);
		}
		else
		{
			smi.attributes.Remove(smi.unhappyMucusModifier);
		}
	}

	private static bool CanProduceLubricant(Instance smi)
	{
		if (smi.effects.HasEffect(RECENTLY_PRODUCED_LUBRICANT_EFFECT))
		{
			return false;
		}
		int cell = Grid.CellBelow(Grid.PosToCell(smi));
		if (Grid.IsValidCell(cell))
		{
			return Grid.IsSolidCell(cell);
		}
		return false;
	}
}
