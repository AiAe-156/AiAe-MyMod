using Klei.AI;
using TUNING;
using UnityEngine;

public class BipedTransitionLayer : TransitionDriver.OverrideLayer
{
	private bool isWalking;

	private float floorSpeed;

	private float ladderSpeed;

	private float startTime;

	private bool isInLiquid;

	private float jetPackSpeed;

	private const float downPoleSpeed = 15f;

	private const float WATER_SPEED_PENALTY = 0.5f;

	private const float SUIT_SWIM_SPEED_PENALTY = 0.3f;

	private const float SUIT_SWIM_ANIM_PENALTY = 0.3f;

	private AttributeConverterInstance movementSpeed;

	private AttributeLevels attributeLevels;

	private Attributes attributes;

	public BipedTransitionLayer(Navigator navigator, float floor_speed, float ladder_speed)
		: base(navigator)
	{
		navigator.Subscribe(1773898642, delegate
		{
			isWalking = true;
		});
		navigator.Subscribe(1597112836, delegate
		{
			isWalking = false;
		});
		floorSpeed = floor_speed;
		ladderSpeed = ladder_speed;
		jetPackSpeed = 7f;
		movementSpeed = Db.Get().AttributeConverters.MovementSpeed.Lookup(navigator.gameObject);
		attributeLevels = navigator.GetComponent<AttributeLevels>();
		attributes = navigator.gameObject.GetAttributes();
	}

	public override void BeginTransition(Navigator navigator, Navigator.ActiveTransition transition)
	{
		base.BeginTransition(navigator, transition);
		float num = 1f;
		bool flag = (transition.start == NavType.Pole || transition.end == NavType.Pole) && transition.y < 0 && transition.x == 0;
		bool flag2 = transition.start == NavType.Tube || transition.end == NavType.Tube;
		bool flag3 = transition.start == NavType.Hover || transition.end == NavType.Hover;
		bool flag4 = transition.start == NavType.Swim || transition.end == NavType.Swim;
		bool flag5 = !flag && !flag2 && !flag3;
		int cell = Grid.PosToCell(navigator);
		isInLiquid = navigator.CurrentNavType == NavType.Swim || Grid.IsSubstantialLiquid(cell);
		if (flag5)
		{
			if (isWalking)
			{
				return;
			}
			num = GetMovementSpeedMultiplier();
		}
		float num2 = 1f;
		bool flag6 = (navigator.flags & PathFinder.PotentialPath.Flags.HasAtmoSuit) != 0;
		bool flag7 = (navigator.flags & PathFinder.PotentialPath.Flags.HasJetPack) != 0;
		bool flag8 = (navigator.flags & PathFinder.PotentialPath.Flags.HasLeadSuit) != 0;
		bool flag9 = flag7 || flag6 || flag8;
		if (!flag9 && !flag4 && Grid.IsSubstantialLiquid(cell))
		{
			num2 = 0.5f;
		}
		else if (flag9 && flag4)
		{
			num2 = 0.3f;
			transition.animSpeed = GetSwimmingInSuitAnimSpeed(transition);
		}
		num *= num2;
		if (transition.x == 0 && (transition.start == NavType.Ladder || transition.start == NavType.Pole) && transition.start == transition.end)
		{
			if (flag)
			{
				transition.speed = 15f * num;
			}
			else
			{
				transition.speed = ladderSpeed * num;
				GameObject gameObject = Grid.Objects[cell, 1];
				if (gameObject != null)
				{
					Ladder component = gameObject.GetComponent<Ladder>();
					if (component != null)
					{
						float num3 = component.upwardsMovementSpeedMultiplier;
						if (transition.y < 0)
						{
							num3 = component.downwardsMovementSpeedMultiplier;
						}
						transition.speed *= num3;
						transition.animSpeed *= num3;
					}
				}
			}
		}
		else if (flag2)
		{
			transition.speed = GetTubeTravellingSpeedMultiplier(navigator);
		}
		else if (flag3)
		{
			transition.speed = jetPackSpeed;
			if (transition.x == 0 && transition.y == -1)
			{
				transition.speed *= 0.75f;
			}
			transition.animSpeed = transition.speed;
		}
		else
		{
			transition.speed = floorSpeed * num;
		}
		float num4 = num - 1f;
		transition.animSpeed += transition.animSpeed * num4 / 2f;
		if (transition.start == NavType.Floor && transition.end == NavType.Floor)
		{
			int num5 = Grid.CellBelow(cell);
			if (Grid.Foundation[num5])
			{
				GameObject gameObject2 = Grid.Objects[num5, 1];
				if (gameObject2 != null)
				{
					SimCellOccupier component2 = gameObject2.GetComponent<SimCellOccupier>();
					if (component2 != null)
					{
						transition.speed *= component2.movementSpeedMultiplier;
						transition.animSpeed *= component2.movementSpeedMultiplier;
					}
				}
			}
		}
		startTime = Time.time;
	}

	public override void EndTransition(Navigator navigator, Navigator.ActiveTransition transition)
	{
		base.EndTransition(navigator, transition);
		bool flag = (transition.start == NavType.Pole || transition.end == NavType.Pole) && transition.y < 0 && transition.x == 0;
		bool flag2 = transition.start == NavType.Tube || transition.end == NavType.Tube;
		if (!isWalking && !flag && !flag2 && attributeLevels != null)
		{
			attributeLevels.AddExperience(Db.Get().Attributes.Athletics.Id, Time.time - startTime, DUPLICANTSTATS.ATTRIBUTE_LEVELING.ALL_DAY_EXPERIENCE);
		}
		int cell = Grid.OffsetCell(navigator.cachedCell, transition.x, transition.y);
		bool flag3 = transition.end == NavType.Swim || Grid.IsSubstantialLiquid(cell);
		if (flag3 == isInLiquid)
		{
		}
	}

	public float GetTubeTravellingSpeedMultiplier(Navigator navigator)
	{
		return Db.Get().Attributes.TransitTubeTravelSpeed.Lookup(navigator.gameObject)?.GetTotalValue() ?? DUPLICANTSTATS.STANDARD.BaseStats.TRANSIT_TUBE_TRAVEL_SPEED;
	}

	public static float GetMovementSpeedMultiplier(AttributeConverterInstance movementSpeed)
	{
		float num = 1f;
		if (movementSpeed != null)
		{
			num += movementSpeed.Evaluate();
		}
		return Mathf.Max(0.1f, num);
	}

	public static float GetSwimmingInSuitAnimSpeed(Navigator.ActiveTransition transition)
	{
		if (!transition.isLooping && transition.x != 0 && transition.y != 0 && transition.start == NavType.Swim && transition.end == NavType.Swim)
		{
			return 0.3f;
		}
		return transition.animSpeed;
	}

	public float GetMovementSpeedMultiplier()
	{
		return GetMovementSpeedMultiplier(movementSpeed);
	}
}
