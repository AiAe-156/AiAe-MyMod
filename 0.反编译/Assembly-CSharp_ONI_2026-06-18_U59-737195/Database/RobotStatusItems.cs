using UnityEngine;

namespace Database;

public class RobotStatusItems : StatusItems
{
	public StatusItem LowBattery;

	public StatusItem LowBatteryNoCharge;

	public StatusItem DeadBattery;

	public StatusItem DeadBatteryFlydo;

	public StatusItem CantReachStation;

	public StatusItem DustBinFull;

	public StatusItem Working;

	public StatusItem UnloadingStorage;

	public StatusItem ReactPositive;

	public StatusItem ReactNegative;

	public StatusItem MovingToChargeStation;

	public RobotStatusItems(ResourceSet parent)
		: base("RobotStatusItems", parent)
	{
		CreateStatusItems();
	}

	private void CreateStatusItems()
	{
		CantReachStation = new StatusItem("CantReachStation", "ROBOTS", "status_item_exclamation", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		CantReachStation.resolveStringCallback = delegate(string str, object data)
		{
			GameObject go = (GameObject)data;
			return str.Replace("{0}", go.GetProperName());
		};
		LowBattery = new StatusItem("LowBattery", "ROBOTS", "status_item_need_power", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		LowBattery.resolveStringCallback = delegate(string str, object data)
		{
			GameObject go = (GameObject)data;
			return str.Replace("{0}", go.GetProperName());
		};
		LowBatteryNoCharge = new StatusItem("LowBatteryNoCharge", "ROBOTS", "status_item_need_power", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		LowBatteryNoCharge.resolveStringCallback = delegate(string str, object data)
		{
			GameObject go = (GameObject)data;
			return str.Replace("{0}", go.GetProperName());
		};
		DeadBattery = new StatusItem("DeadBattery", "ROBOTS", "status_item_need_power", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		DeadBattery.resolveStringCallback = delegate(string str, object data)
		{
			GameObject go = (GameObject)data;
			return str.Replace("{0}", go.GetProperName());
		};
		DeadBatteryFlydo = new StatusItem("DeadBatteryFlydo", "ROBOTS", "status_item_need_power", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		DeadBatteryFlydo.resolveStringCallback = delegate(string str, object data)
		{
			GameObject go = (GameObject)data;
			return str.Replace("{0}", go.GetProperName());
		};
		DustBinFull = new StatusItem("DustBinFull", "ROBOTS", "status_item_pending_clear", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		DustBinFull.resolveStringCallback = delegate(string str, object data)
		{
			GameObject go = (GameObject)data;
			return str.Replace("{0}", go.GetProperName());
		};
		Working = new StatusItem("Working", "ROBOTS", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		Working.resolveStringCallback = delegate(string str, object data)
		{
			GameObject go = (GameObject)data;
			return str.Replace("{0}", go.GetProperName());
		};
		MovingToChargeStation = new StatusItem("MovingToChargeStation", "ROBOTS", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		MovingToChargeStation.resolveStringCallback = delegate(string str, object data)
		{
			GameObject go = (GameObject)data;
			return str.Replace("{0}", go.GetProperName());
		};
		UnloadingStorage = new StatusItem("UnloadingStorage", "ROBOTS", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		UnloadingStorage.resolveStringCallback = delegate(string str, object data)
		{
			GameObject go = (GameObject)data;
			return str.Replace("{0}", go.GetProperName());
		};
		ReactPositive = new StatusItem("ReactPositive", "ROBOTS", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		ReactPositive.resolveStringCallback = (string str, object data) => str;
		ReactNegative = new StatusItem("ReactNegative", "ROBOTS", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		ReactNegative.resolveStringCallback = (string str, object data) => str;
	}
}
