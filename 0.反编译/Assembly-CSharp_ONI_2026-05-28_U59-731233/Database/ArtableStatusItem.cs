namespace Database;

public class ArtableStatusItem : StatusItem
{
	public ArtableStatuses.ArtableStatusType StatusType;

	public ArtableStatusItem(string id, ArtableStatuses.ArtableStatusType statusType)
		: base(id, "BUILDING", "", IconType.Info, (statusType == ArtableStatuses.ArtableStatusType.AwaitingArting) ? NotificationType.BadMinor : NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID)
	{
		StatusType = statusType;
	}
}
