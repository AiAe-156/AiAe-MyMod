using System;
using System.Collections.Generic;
using Database;
using STRINGS;

public static class RoomConstraints
{
	public static class ConstraintTags
	{
		public static List<Tag> AllTags = new List<Tag>();

		public static Tag BedType = AllTags.AddAndReturn("BedType".ToTag());

		public static Tag LuxuryBedType = AllTags.AddAndReturn("LuxuryBedType".ToTag());

		public static Tag ToiletType = AllTags.AddAndReturn("ToiletType".ToTag());

		public static Tag BionicUpkeepType = AllTags.AddAndReturn("BionicUpkeep".ToTag());

		public static Tag FlushToiletType = AllTags.AddAndReturn("FlushToiletType".ToTag());

		public static Tag MessTable = AllTags.AddAndReturn("MessTable".ToTag());

		public static Tag DiningTableType = AllTags.AddAndReturn("DiningTableType".ToTag());

		public static Tag Clinic = AllTags.AddAndReturn("Clinic".ToTag());

		public static Tag WashStation = AllTags.AddAndReturn("WashStation".ToTag());

		public static Tag AdvancedWashStation = AllTags.AddAndReturn("AdvancedWashStation".ToTag());

		public static Tag ScienceBuilding = AllTags.AddAndReturn("ScienceBuilding".ToTag());

		public static Tag MassageTable = AllTags.AddAndReturn("MassageTable".ToTag());

		public static Tag DeStressingBuilding = AllTags.AddAndReturn("DeStressingBuilding".ToTag());

		public static Tag IndustrialMachinery = AllTags.AddAndReturn("IndustrialMachinery".ToTag());

		public static Tag GeneratorType = AllTags.AddAndReturn("GeneratorType".ToTag());

		public static Tag HeavyDutyGeneratorType = AllTags.AddAndReturn("HeavyDutyGeneratorType".ToTag());

		public static Tag LightDutyGeneratorType = AllTags.AddAndReturn("LightDutyGeneratorType".ToTag());

		public static Tag PowerBuilding = AllTags.AddAndReturn("PowerBuilding".ToTag());

		public static Tag FarmStationType = AllTags.AddAndReturn("FarmStationType".ToTag());

		public static Tag RanchStationType = AllTags.AddAndReturn("RanchStationType".ToTag());

		public static Tag SpiceStation = AllTags.AddAndReturn("SpiceStation".ToTag());

		public static Tag CookTop = AllTags.AddAndReturn("CookTop".ToTag());

		public static Tag Refrigerator = AllTags.AddAndReturn("Refrigerator".ToTag());

		public static Tag RecBuilding = AllTags.AddAndReturn("RecBuilding".ToTag());

		public static Tag MachineShopType = AllTags.AddAndReturn("MachineShopType".ToTag());

		public static Tag Park = AllTags.AddAndReturn("Park".ToTag());

		public static Tag NatureReserve = AllTags.AddAndReturn("NatureReserve".ToTag());

		public static Tag RocketInterior = AllTags.AddAndReturn("RocketInterior".ToTag());

		public static Tag Decoration = AllTags.AddAndReturn(GameTags.Decoration);

		public static Tag Ornament = AllTags.AddAndReturn("Ornament".ToTag());

		public static Tag WarmingStation = AllTags.AddAndReturn("WarmingStation".ToTag());

		public static Tag Submersible = AllTags.AddAndReturn("Submersible".ToTag());

		[Obsolete("The light requirement constraint in rooms has been removed. Please update any references of RoomConstraints.LightSource to GameTags.Lightsource")]
		public static Tag LightSource = "LightSource".ToTag();

		public static Tag DecorFancy { get; internal set; }

		public static string GetRoomConstraintLabelText(Tag tag)
		{
			StringEntry result = null;
			string text = "STRINGS.ROOMS.CRITERIA." + tag.ToString().ToUpper() + ".NAME";
			return Strings.TryGet(new StringKey(text), out result) ? ((string)result) : ROOMS.CRITERIA.IN_CODE_ERROR.text.Replace("{0}", text);
		}
	}

	public class Constraint
	{
		public string name;

		public string description;

		public string conflictDescription;

		public int times_required = 1;

		public Func<Room, bool> room_criteria;

		public Func<KPrefabID, bool> building_criteria;

		public Func<KPrefabID, bool> creature_criteria;

		public List<Constraint> stomp_in_conflict;

		public Constraint(Func<KPrefabID, bool> building_criteria, Func<Room, bool> room_criteria, int times_required = 1, string name = "", string description = "", List<Constraint> stomp_in_conflict = null, string overrideConstraintConflictName = null)
			: this(null, building_criteria, room_criteria, times_required, name, description, stomp_in_conflict, overrideConstraintConflictName)
		{
		}

		public Constraint(Func<KPrefabID, bool> creature_criteria, Func<KPrefabID, bool> building_criteria, Func<Room, bool> room_criteria, int times_required = 1, string name = "", string description = "", List<Constraint> stomp_in_conflict = null, string overrideConstraintConflictName = null)
		{
			this.creature_criteria = creature_criteria;
			this.room_criteria = room_criteria;
			this.building_criteria = building_criteria;
			this.times_required = times_required;
			this.name = name;
			this.description = description;
			this.stomp_in_conflict = stomp_in_conflict;
			conflictDescription = ((overrideConstraintConflictName == null) ? name : overrideConstraintConflictName);
		}

		public bool isSatisfied(Room room)
		{
			int num = 0;
			if (room_criteria != null && !room_criteria(room))
			{
				return false;
			}
			if (building_criteria != null)
			{
				int num2 = 0;
				while (num < times_required && num2 < room.buildings.Count)
				{
					KPrefabID kPrefabID = room.buildings[num2];
					if (!(kPrefabID == null) && building_criteria(kPrefabID))
					{
						num++;
					}
					num2++;
				}
				int num3 = 0;
				while (num < times_required && num3 < room.plants.Count)
				{
					KPrefabID kPrefabID2 = room.plants[num3];
					if (!(kPrefabID2 == null) && building_criteria(kPrefabID2))
					{
						num++;
					}
					num3++;
				}
				return num >= times_required;
			}
			if (creature_criteria != null)
			{
			}
			return true;
		}
	}

	public static Constraint CEILING_HEIGHT_4 = new Constraint(null, (Room room) => 1 + room.cavity.maxY - room.cavity.minY >= 4, 1, string.Format(ROOMS.CRITERIA.CEILING_HEIGHT.NAME, "4"), string.Format(ROOMS.CRITERIA.CEILING_HEIGHT.DESCRIPTION, "4"));

	public static Constraint CEILING_HEIGHT_6 = new Constraint(null, (Room room) => 1 + room.cavity.maxY - room.cavity.minY >= 6, 1, string.Format(ROOMS.CRITERIA.CEILING_HEIGHT.NAME, "6"), string.Format(ROOMS.CRITERIA.CEILING_HEIGHT.DESCRIPTION, "6"));

	public static Constraint MINIMUM_SIZE_12 = new Constraint(null, (Room room) => room.cavity.NumCells >= 12, 1, string.Format(ROOMS.CRITERIA.MINIMUM_SIZE.NAME, "12"), string.Format(ROOMS.CRITERIA.MINIMUM_SIZE.DESCRIPTION, "12"));

	public static Constraint MINIMUM_SIZE_24 = new Constraint(null, (Room room) => room.cavity.NumCells >= 24, 1, string.Format(ROOMS.CRITERIA.MINIMUM_SIZE.NAME, "24"), string.Format(ROOMS.CRITERIA.MINIMUM_SIZE.DESCRIPTION, "24"));

	public static Constraint MINIMUM_SIZE_32 = new Constraint(null, (Room room) => room.cavity.NumCells >= 32, 1, string.Format(ROOMS.CRITERIA.MINIMUM_SIZE.NAME, "32"), string.Format(ROOMS.CRITERIA.MINIMUM_SIZE.DESCRIPTION, "32"));

	public static Constraint MAXIMUM_SIZE_64 = new Constraint(null, (Room room) => room.cavity.NumCells <= 64, 1, string.Format(ROOMS.CRITERIA.MAXIMUM_SIZE.NAME, "64"), string.Format(ROOMS.CRITERIA.MAXIMUM_SIZE.DESCRIPTION, "64"));

	public static Constraint MAXIMUM_SIZE_96 = new Constraint(null, (Room room) => room.cavity.NumCells <= 96, 1, string.Format(ROOMS.CRITERIA.MAXIMUM_SIZE.NAME, "96"), string.Format(ROOMS.CRITERIA.MAXIMUM_SIZE.DESCRIPTION, "96"));

	public static Constraint MAXIMUM_SIZE_120 = new Constraint(null, (Room room) => room.cavity.NumCells <= 120, 1, string.Format(ROOMS.CRITERIA.MAXIMUM_SIZE.NAME, "120"), string.Format(ROOMS.CRITERIA.MAXIMUM_SIZE.DESCRIPTION, "120"));

	public static Constraint NO_INDUSTRIAL_MACHINERY = new Constraint(null, delegate(Room room)
	{
		foreach (KPrefabID building in room.buildings)
		{
			if (building.HasTag(ConstraintTags.IndustrialMachinery))
			{
				return false;
			}
		}
		return true;
	}, 1, ROOMS.CRITERIA.NO_INDUSTRIAL_MACHINERY.NAME, ROOMS.CRITERIA.NO_INDUSTRIAL_MACHINERY.DESCRIPTION);

	public static Constraint NO_COTS = new Constraint(null, delegate(Room room)
	{
		foreach (KPrefabID building2 in room.buildings)
		{
			if (building2.HasTag(ConstraintTags.BedType) && !building2.HasTag(ConstraintTags.LuxuryBedType))
			{
				return false;
			}
		}
		return true;
	}, 1, ROOMS.CRITERIA.NO_COTS.NAME, ROOMS.CRITERIA.NO_COTS.DESCRIPTION);

	public static Constraint NO_LUXURY_BEDS = new Constraint(null, delegate(Room room)
	{
		foreach (KPrefabID building3 in room.buildings)
		{
			if (building3.HasTag(ConstraintTags.LuxuryBedType))
			{
				return false;
			}
		}
		return true;
	}, 1, ROOMS.CRITERIA.NO_COTS.NAME, ROOMS.CRITERIA.NO_COTS.DESCRIPTION);

	public static Constraint NO_OUTHOUSES = new Constraint(null, delegate(Room room)
	{
		foreach (KPrefabID building4 in room.buildings)
		{
			if (building4.HasTag(ConstraintTags.ToiletType) && !building4.HasTag(ConstraintTags.FlushToiletType))
			{
				return false;
			}
		}
		return true;
	}, 1, ROOMS.CRITERIA.NO_OUTHOUSES.NAME, ROOMS.CRITERIA.NO_OUTHOUSES.DESCRIPTION);

	public static Constraint NO_MESS_STATION = new Constraint(null, delegate(Room room)
	{
		bool flag = false;
		int num = 0;
		while (!flag && num < room.buildings.Count)
		{
			flag = room.buildings[num].HasTag(ConstraintTags.MessTable);
			num++;
		}
		return !flag;
	}, 1, ROOMS.CRITERIA.NO_MESS_STATION.NAME, ROOMS.CRITERIA.NO_MESS_STATION.DESCRIPTION);

	public static Constraint NO_BASIC_MESS_STATIONS = new Constraint(null, delegate(Room room)
	{
		bool flag = false;
		int num = 0;
		while (!flag && num < room.buildings.Count)
		{
			flag = room.buildings[num].PrefabID() == "DiningTable";
			if (flag)
			{
				break;
			}
			num++;
		}
		return !flag;
	}, 1, ROOMS.CRITERIA.NO_BASIC_MESS_STATIONS.NAME, ROOMS.CRITERIA.NO_BASIC_MESS_STATIONS.DESCRIPTION);

	public static Constraint HAS_LUXURY_BED = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.LuxuryBedType), null, 1, ROOMS.CRITERIA.HAS_LUXURY_BED.NAME, ROOMS.CRITERIA.HAS_LUXURY_BED.DESCRIPTION);

	public static Constraint HAS_BED = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.BedType) && !bc.HasTag(ConstraintTags.Clinic), null, 1, ROOMS.CRITERIA.HAS_BED.NAME, ROOMS.CRITERIA.HAS_BED.DESCRIPTION);

	public static Constraint SCIENCE_BUILDINGS = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.ScienceBuilding), null, 2, ROOMS.CRITERIA.SCIENCE_BUILDINGS.NAME, ROOMS.CRITERIA.SCIENCE_BUILDINGS.DESCRIPTION);

	public static Constraint BED_SINGLE = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.BedType) && !bc.HasTag(ConstraintTags.Clinic), delegate(Room room)
	{
		short num = 0;
		int num2 = 0;
		while (num < 2 && num2 < room.buildings.Count)
		{
			if (room.buildings[num2].HasTag(ConstraintTags.BedType))
			{
				num++;
			}
			num2++;
		}
		return num == 1;
	}, 1, ROOMS.CRITERIA.BED_SINGLE.NAME, ROOMS.CRITERIA.BED_SINGLE.DESCRIPTION);

	public static Constraint LUXURY_BED_SINGLE = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.LuxuryBedType), delegate(Room room)
	{
		short num = 0;
		int num2 = 0;
		while (num <= 2 && num2 < room.buildings.Count)
		{
			if (room.buildings[num2].HasTag(ConstraintTags.LuxuryBedType))
			{
				num++;
			}
			num2++;
		}
		return num == 1;
	}, 1, ROOMS.CRITERIA.LUXURYBEDTYPE.NAME, ROOMS.CRITERIA.LUXURYBEDTYPE.DESCRIPTION);

	public static Constraint BUILDING_DECOR_POSITIVE = new Constraint(delegate(KPrefabID bc)
	{
		DecorProvider component = bc.GetComponent<DecorProvider>();
		return (component != null && component.baseDecor > 0f) ? true : false;
	}, null, 1, ROOMS.CRITERIA.BUILDING_DECOR_POSITIVE.NAME, ROOMS.CRITERIA.BUILDING_DECOR_POSITIVE.DESCRIPTION);

	public static Constraint DECORATIVE_ITEM = new Constraint((KPrefabID bc) => bc.HasTag(GameTags.Decoration), null, 1, string.Format(ROOMS.CRITERIA.DECORATIVE_ITEM.NAME, 1), string.Format(ROOMS.CRITERIA.DECORATIVE_ITEM.DESCRIPTION, 1));

	public static Constraint DECORATIVE_ITEM_2 = new Constraint((KPrefabID bc) => bc.HasTag(GameTags.Decoration), null, 2, string.Format(ROOMS.CRITERIA.DECORATIVE_ITEM.NAME, 2), string.Format(ROOMS.CRITERIA.DECORATIVE_ITEM.DESCRIPTION, 2));

	public static Constraint ORNAMENTDISPLAYED = new Constraint(null, null, delegate(Room room)
	{
		for (int i = 0; i < room.buildings.Count; i++)
		{
			KPrefabID entityOrBuilding = room.buildings[i];
			if (CheckOrnament(entityOrBuilding))
			{
				return true;
			}
		}
		for (int j = 0; j < room.otherEntities.Count; j++)
		{
			KPrefabID entityOrBuilding2 = room.otherEntities[j];
			if (CheckOrnament(entityOrBuilding2))
			{
				return true;
			}
		}
		return false;
	}, 1, ROOMS.CRITERIA.ORNAMENT.NAME, ROOMS.CRITERIA.ORNAMENT.DESCRIPTION);

	public static Constraint POWER_STATION = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.HeavyDutyGeneratorType), delegate(Room room)
	{
		int num = 0;
		bool flag = false;
		foreach (KPrefabID building5 in room.buildings)
		{
			flag = flag || building5.HasTag(ConstraintTags.HeavyDutyGeneratorType);
			num += (building5.HasTag(ConstraintTags.PowerBuilding) ? 1 : 0);
		}
		return flag && num >= 2;
	}, 1, ROOMS.CRITERIA.POWERPLANT.NAME, ROOMS.CRITERIA.POWERPLANT.DESCRIPTION, null, ROOMS.CRITERIA.POWERPLANT.CONFLICT_DESCRIPTION);

	public static Constraint FARM_STATION = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.FarmStationType), null, 1, ROOMS.CRITERIA.FARMSTATIONTYPE.NAME, ROOMS.CRITERIA.FARMSTATIONTYPE.DESCRIPTION);

	public static Constraint RANCH_STATION = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.RanchStationType), null, 1, ROOMS.CRITERIA.RANCHSTATIONTYPE.NAME, ROOMS.CRITERIA.RANCHSTATIONTYPE.DESCRIPTION);

	public static Constraint SPICE_STATION = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.SpiceStation), null, 1, ROOMS.CRITERIA.SPICESTATION.NAME, ROOMS.CRITERIA.SPICESTATION.DESCRIPTION);

	public static Constraint COOK_TOP = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.CookTop), null, 1, ROOMS.CRITERIA.COOKTOP.NAME, ROOMS.CRITERIA.COOKTOP.DESCRIPTION);

	public static Constraint REFRIGERATOR = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.Refrigerator), null, 1, ROOMS.CRITERIA.REFRIGERATOR.NAME, ROOMS.CRITERIA.REFRIGERATOR.DESCRIPTION);

	public static Constraint REC_BUILDING = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.RecBuilding), null, 1, ROOMS.CRITERIA.RECBUILDING.NAME, ROOMS.CRITERIA.RECBUILDING.DESCRIPTION);

	public static Constraint MACHINE_SHOP = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.MachineShopType), null, 1, ROOMS.CRITERIA.MACHINESHOPTYPE.NAME, ROOMS.CRITERIA.MACHINESHOPTYPE.DESCRIPTION);

	[Obsolete("The light requirement constraint in rooms has been removed. This is retained solely to avoid breaking mods")]
	public static Constraint LIGHT = new Constraint(null, null, 1, ROOMS.CRITERIA.LIGHTSOURCE.NAME, ROOMS.CRITERIA.LIGHTSOURCE.DESCRIPTION);

	public static Constraint DESTRESSING_BUILDING = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.DeStressingBuilding), null, 1, ROOMS.CRITERIA.DESTRESSINGBUILDING.NAME, ROOMS.CRITERIA.DESTRESSINGBUILDING.DESCRIPTION);

	public static Constraint MASSAGE_TABLE = new Constraint((KPrefabID bc) => bc.IsPrefabID(ConstraintTags.MassageTable), null, 1, ROOMS.CRITERIA.MASSAGE_TABLE.NAME, ROOMS.CRITERIA.MASSAGE_TABLE.DESCRIPTION);

	public static Constraint DINING_TABLE = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.DiningTableType), null, 1, ROOMS.CRITERIA.DININGTABLETYPE.NAME, ROOMS.CRITERIA.DININGTABLETYPE.DESCRIPTION, new List<Constraint> { REC_BUILDING, MESS_STATION_SINGLE, MULTI_MINION_DINING_TABLE });

	public static Constraint MESS_STATION_SINGLE = new Constraint((KPrefabID bc) => bc.IsPrefabID("DiningTable"), null, 1, ROOMS.CRITERIA.DININGTABLETYPE.NAME, ROOMS.CRITERIA.DININGTABLETYPE.DESCRIPTION, new List<Constraint> { REC_BUILDING, DINING_TABLE });

	public static Constraint MULTI_MINION_DINING_TABLE = new Constraint((KPrefabID bc) => bc.IsPrefabID("MultiMinionDiningTable") || bc.gameObject.name == "MultiMinionDiningSeat", null, 1, ROOMS.CRITERIA.MULTI_MINION_DINING_TABLE.NAME, ROOMS.CRITERIA.MULTI_MINION_DINING_TABLE.DESCRIPTION, new List<Constraint> { REC_BUILDING, DINING_TABLE });

	public static Constraint TOILET = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.ToiletType), null, 1, ROOMS.CRITERIA.TOILETTYPE.NAME, ROOMS.CRITERIA.TOILETTYPE.DESCRIPTION);

	public static Constraint BIONICUPKEEP = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.BionicUpkeepType), null, 2, ROOMS.CRITERIA.BIONICUPKEEP.NAME, ROOMS.CRITERIA.BIONICUPKEEP.DESCRIPTION);

	public static Constraint BIONIC_LUBRICATION = new Constraint((KPrefabID bc) => bc.HasTag("OilChanger"), null, 1, ROOMS.CRITERIA.BIONIC_LUBRICATION.NAME, ROOMS.CRITERIA.BIONIC_LUBRICATION.DESCRIPTION);

	public static Constraint BIONIC_GUNKEMPTIER = new Constraint((KPrefabID bc) => bc.HasTag("GunkEmptier"), null, 1, ROOMS.CRITERIA.BIONIC_GUNKEMPTIER.NAME, ROOMS.CRITERIA.BIONIC_GUNKEMPTIER.DESCRIPTION);

	public static Constraint FLUSH_TOILET = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.FlushToiletType), null, 1, ROOMS.CRITERIA.FLUSHTOILETTYPE.NAME, ROOMS.CRITERIA.FLUSHTOILETTYPE.DESCRIPTION);

	public static Constraint WASH_STATION = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.WashStation), null, 1, ROOMS.CRITERIA.WASHSTATION.NAME, ROOMS.CRITERIA.WASHSTATION.DESCRIPTION);

	public static Constraint ADVANCEDWASHSTATION = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.AdvancedWashStation), null, 1, ROOMS.CRITERIA.ADVANCEDWASHSTATION.NAME, ROOMS.CRITERIA.ADVANCEDWASHSTATION.DESCRIPTION);

	public static Constraint CLINIC = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.Clinic), null, 1, ROOMS.CRITERIA.CLINIC.NAME, ROOMS.CRITERIA.CLINIC.DESCRIPTION, new List<Constraint> { TOILET, FLUSH_TOILET, MESS_STATION_SINGLE });

	public static Constraint PARK_BUILDING = new Constraint((KPrefabID bc) => bc.HasTag(ConstraintTags.Park), null, 1, ROOMS.CRITERIA.PARK.NAME, ROOMS.CRITERIA.PARK.DESCRIPTION);

	public static Constraint ORIGINALTILES = new Constraint(null, (Room room) => 1 + room.cavity.maxY - room.cavity.minY >= 4);

	public static Constraint IS_BACKWALLED = new Constraint(null, delegate(Room room)
	{
		bool flag = true;
		int num = (room.cavity.maxX - room.cavity.minX + 1) / 2 + 1;
		int num2 = 0;
		while (flag && num2 < num)
		{
			int x = room.cavity.minX + num2;
			int x2 = room.cavity.maxX - num2;
			int num3 = room.cavity.minY;
			while (flag && num3 <= room.cavity.maxY)
			{
				int cell = Grid.XYToCell(x, num3);
				int cell2 = Grid.XYToCell(x2, num3);
				if (Game.Instance.roomProber.GetCavityForCell(cell) == room.cavity)
				{
					bool flag2 = BackwallManager.HasBackwall(cell) || (Grid.Objects[cell, 2] != null && !Grid.Objects[cell, 2].HasTag(GameTags.UnderConstruction));
					flag = flag && flag2;
				}
				if (Game.Instance.roomProber.GetCavityForCell(cell2) == room.cavity)
				{
					bool flag3 = BackwallManager.HasBackwall(cell2) || (Grid.Objects[cell2, 2] != null && !Grid.Objects[cell2, 2].HasTag(GameTags.UnderConstruction));
					flag = flag && flag3;
				}
				if (!flag)
				{
					return false;
				}
				num3++;
			}
			num2++;
		}
		return flag;
	}, 1, ROOMS.CRITERIA.IS_BACKWALLED.NAME, ROOMS.CRITERIA.IS_BACKWALLED.DESCRIPTION);

	public static Constraint WILDANIMAL = new Constraint(null, delegate(Room room)
	{
		int num = room.cavity.creatures.Count + room.cavity.eggs.Count;
		return num > 0;
	}, 1, ROOMS.CRITERIA.WILDANIMAL.NAME, ROOMS.CRITERIA.WILDANIMAL.DESCRIPTION);

	public static Constraint WILDANIMALS = new Constraint(null, delegate(Room room)
	{
		int num = 0;
		foreach (KPrefabID creature in room.cavity.creatures)
		{
			if (creature.HasTag(GameTags.Creatures.Wild))
			{
				num++;
			}
		}
		return num >= 2;
	}, 1, ROOMS.CRITERIA.WILDANIMALS.NAME, ROOMS.CRITERIA.WILDANIMALS.DESCRIPTION);

	public static Constraint WILDPLANT = new Constraint(null, delegate(Room room)
	{
		int num = 0;
		foreach (KPrefabID plant in room.cavity.plants)
		{
			if (plant != null && !plant.HasTag(GameTags.PlantBranch))
			{
				BasicForagePlantPlanted component = plant.GetComponent<BasicForagePlantPlanted>();
				ReceptacleMonitor component2 = plant.GetComponent<ReceptacleMonitor>();
				if (component2 != null && !component2.Replanted)
				{
					num++;
				}
				else if (component != null)
				{
					num++;
				}
			}
		}
		return num >= 2;
	}, 1, ROOMS.CRITERIA.WILDPLANT.NAME, ROOMS.CRITERIA.WILDPLANT.DESCRIPTION);

	public static Constraint WILDPLANTS = new Constraint(null, delegate(Room room)
	{
		int num = 0;
		foreach (KPrefabID plant2 in room.cavity.plants)
		{
			if (plant2 != null && !plant2.HasTag(GameTags.PlantBranch))
			{
				BasicForagePlantPlanted component = plant2.GetComponent<BasicForagePlantPlanted>();
				ReceptacleMonitor component2 = plant2.GetComponent<ReceptacleMonitor>();
				if (component2 != null && !component2.Replanted)
				{
					num++;
				}
				else if (component != null)
				{
					num++;
				}
			}
		}
		return num >= 4;
	}, 1, ROOMS.CRITERIA.WILDPLANTS.NAME, ROOMS.CRITERIA.WILDPLANTS.DESCRIPTION);

	public static Tag AddAndReturn(this List<Tag> list, Tag tag)
	{
		list.Add(tag);
		return tag;
	}

	public static string RoomCriteriaString(Room room)
	{
		string text = "";
		RoomType roomType = room.roomType;
		if (roomType != Db.Get().RoomTypes.Neutral)
		{
			text = string.Concat(text, "<b>", ROOMS.CRITERIA.HEADER, "</b>");
			text = text + "\n    • " + roomType.primary_constraint.name;
			if (roomType.additional_constraints != null)
			{
				Constraint[] additional_constraints = roomType.additional_constraints;
				foreach (Constraint constraint in additional_constraints)
				{
					text = ((!constraint.isSatisfied(room)) ? (text + "\n<color=#F44A47FF>    • " + constraint.name + "</color>") : (text + "\n    • " + constraint.name));
				}
			}
			return text;
		}
		RoomTypes.RoomTypeQueryResult[] possibleRoomTypes = Db.Get().RoomTypes.GetPossibleRoomTypes(room);
		text += ((possibleRoomTypes.Length > 1) ? string.Concat("<b>", ROOMS.CRITERIA.POSSIBLE_TYPES_HEADER, "</b>") : "");
		RoomTypes.RoomTypeQueryResult[] array = possibleRoomTypes;
		for (int j = 0; j < array.Length; j++)
		{
			RoomTypes.RoomTypeQueryResult roomTypeQueryResult = array[j];
			RoomType type = roomTypeQueryResult.Type;
			if (type == Db.Get().RoomTypes.Neutral)
			{
				continue;
			}
			if (text != "")
			{
				text += "\n";
			}
			text = text + "<b><color=#BCBCBC>    • " + type.Name + "</b> (" + type.primary_constraint.conflictDescription + ")</color>";
			if (roomTypeQueryResult.SatisfactionRating == RoomType.RoomIdentificationResult.all_satisfied)
			{
				bool flag = false;
				RoomTypes.RoomTypeQueryResult[] array2 = possibleRoomTypes;
				for (int k = 0; k < array2.Length; k++)
				{
					RoomTypes.RoomTypeQueryResult roomTypeQueryResult2 = array2[k];
					RoomType type2 = roomTypeQueryResult2.Type;
					if (type2 != type && type2 != Db.Get().RoomTypes.Neutral && Db.Get().RoomTypes.HasAmbiguousRoomType(room, type, type2))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					text += string.Format("\n<color=#F44A47FF>{0}{1}{2}</color>", "    ", "    • ", ROOMS.CRITERIA.NO_TYPE_CONFLICTS);
				}
				continue;
			}
			Constraint[] additional_constraints2 = type.additional_constraints;
			foreach (Constraint constraint2 in additional_constraints2)
			{
				if (!constraint2.isSatisfied(room))
				{
					string empty = string.Empty;
					empty = ((constraint2.building_criteria == null) ? string.Format(ROOMS.CRITERIA.CRITERIA_FAILED.FAILED, constraint2.name) : string.Format(ROOMS.CRITERIA.CRITERIA_FAILED.MISSING_BUILDING, constraint2.name));
					text = text + "\n<color=#F44A47FF>        • " + empty + "</color>";
				}
			}
		}
		return text;
	}

	private static bool CheckOrnament(KPrefabID entityOrBuilding)
	{
		if (entityOrBuilding == null)
		{
			return false;
		}
		if (!entityOrBuilding.HasTag(GameTags.OrnamentDisplayer))
		{
			return false;
		}
		OrnamentReceptacle component = entityOrBuilding.GetComponent<OrnamentReceptacle>();
		if (component.Occupant != null && component.Occupant.HasTag(GameTags.Ornament) && (component.operational == null || component.operational.IsOperational))
		{
			return true;
		}
		return false;
	}
}
