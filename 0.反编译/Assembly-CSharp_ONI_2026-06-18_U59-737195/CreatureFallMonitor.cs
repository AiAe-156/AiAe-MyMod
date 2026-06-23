using UnityEngine;

public class CreatureFallMonitor : GameStateMachine<CreatureFallMonitor, CreatureFallMonitor.Instance, IStateMachineTarget, CreatureFallMonitor.Def>
{
	public class Def : BaseDef
	{
		public bool canSwim;
	}

	public new class Instance : GameInstance
	{
		public string anim = "fall";

		[MyCmpReq]
		private KPrefabID kprefabId;

		[MyCmpReq]
		private Navigator navigator;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
		}

		private Vector3 GetNavAnchor(Vector3 pos)
		{
			Vector3 vector = navigator.NavGrid.GetNavTypeData(navigator.CurrentNavType).animControllerOffset;
			return pos - vector;
		}

		public void SnapToGround()
		{
			Vector3 navAnchor = GetNavAnchor(base.smi.transform.GetPosition());
			Vector3 position = Grid.CellToPosCBC(Grid.PosToCell(navAnchor), Grid.SceneLayer.Creatures);
			position.x = navAnchor.x;
			base.smi.transform.SetPosition(position);
			NavType[] sNAP_NAV_TYPES = SNAP_NAV_TYPES;
			foreach (NavType navType in sNAP_NAV_TYPES)
			{
				if (navigator.IsValidNavType(navType))
				{
					navigator.SetCurrentNavType(navType);
					break;
				}
			}
		}

		public bool ShouldFall()
		{
			if (kprefabId.HasTag(GameTags.Stored))
			{
				return false;
			}
			Vector3 position = base.smi.transform.GetPosition();
			int num = Grid.PosToCell(position);
			if (Grid.IsValidCell(num) && Grid.Solid[num])
			{
				return false;
			}
			if (navigator.IsMoving())
			{
				return false;
			}
			if (ShouldSettleIntoSwim(position))
			{
				return false;
			}
			if (navigator.CurrentNavType != NavType.Swim)
			{
				if (navigator.NavGrid.NavTable.IsValid(num, navigator.CurrentNavType))
				{
					return false;
				}
				NavType[] wALL_CRAWLER_NAV_TYPES = WALL_CRAWLER_NAV_TYPES;
				foreach (NavType navType in wALL_CRAWLER_NAV_TYPES)
				{
					if (navigator.CurrentNavType == navType)
					{
						return true;
					}
				}
			}
			Vector3 pos = position;
			pos.y += FLOOR_DISTANCE;
			int num2 = Grid.PosToCell(pos);
			if (Grid.IsValidCell(num2) && Grid.Solid[num2])
			{
				return false;
			}
			return true;
		}

		public bool CanSwimAtCurrentLocation()
		{
			return CanSwimAtCell(Grid.PosToCell(base.transform.GetPosition()));
		}

		private bool CanSwimAtCell(int cell)
		{
			if (!base.def.canSwim)
			{
				return false;
			}
			if (!navigator.NavGrid.NavTable.IsValid(cell, NavType.Swim))
			{
				return false;
			}
			if (!GameComps.Gravities.Has(base.gameObject))
			{
				return true;
			}
			if (GameComps.Gravities.GetData(GameComps.Gravities.GetHandle(base.gameObject)).velocity.magnitude >= 2f)
			{
				return false;
			}
			return true;
		}

		public bool ShouldSettleIntoSwim()
		{
			return ShouldSettleIntoSwim(base.transform.GetPosition());
		}

		private bool ShouldSettleIntoSwim(Vector3 pos)
		{
			Vector3 navAnchor = GetNavAnchor(pos);
			int cell = Grid.PosToCell(navAnchor);
			if (!CanSwimAtCell(cell))
			{
				return false;
			}
			float y = Grid.CellToPosCBC(cell, Grid.SceneLayer.Creatures).y;
			return navAnchor.y <= y + 0.1f;
		}
	}

	public static float FLOOR_DISTANCE = -0.065f;

	private const float SWIM_SETTLE_EPSILON = 0.1f;

	private const float SWIM_MAX_FALL_SPEED = 2f;

	private static readonly NavType[] SNAP_NAV_TYPES = new NavType[3]
	{
		NavType.Floor,
		NavType.Hover,
		NavType.Swim
	};

	private static readonly NavType[] WALL_CRAWLER_NAV_TYPES = new NavType[3]
	{
		NavType.Ceiling,
		NavType.LeftWall,
		NavType.RightWall
	};

	public State grounded;

	public State falling;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = grounded;
		grounded.ToggleBehaviour(GameTags.Creatures.Falling, (Instance smi) => smi.ShouldFall());
	}
}
