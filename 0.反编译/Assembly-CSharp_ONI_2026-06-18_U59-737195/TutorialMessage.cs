using KSerialization;

public class TutorialMessage : GenericMessage, IHasDlcRestrictions
{
	[Serialize]
	public Tutorial.TutorialMessages messageId;

	public string videoClipId;

	public string videoOverlayName;

	public string videoTitleText;

	public string icon;

	public string[] requiredDlcIds;

	public string[] forbiddenDlcIds;

	public string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public string[] GetForbiddenDlcIds()
	{
		return forbiddenDlcIds;
	}

	public TutorialMessage()
	{
	}

	public TutorialMessage(Tutorial.TutorialMessages messageId, string title, string body, string tooltip, string videoClipId = null, string videoOverlayName = null, string videoTitleText = null, string icon = "", string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
		: base(title, body, tooltip)
	{
		this.messageId = messageId;
		this.videoClipId = videoClipId;
		this.videoOverlayName = videoOverlayName;
		this.videoTitleText = videoTitleText;
		this.icon = icon;
		this.requiredDlcIds = requiredDlcIds;
		this.forbiddenDlcIds = forbiddenDlcIds;
	}
}
