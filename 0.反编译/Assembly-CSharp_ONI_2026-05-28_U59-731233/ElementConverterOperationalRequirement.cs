using KSerialization;

[SerializationConfig(MemberSerialization.OptIn)]
public class ElementConverterOperationalRequirement : KMonoBehaviour
{
	[MyCmpReq]
	private ElementConverter converter;

	[MyCmpReq]
	private Operational operational;

	private Operational.Flag.Type operationalReq = Operational.Flag.Type.Requirement;

	private Operational.Flag sufficientResources;

	private void onStorageChanged(object _)
	{
		operational.SetFlag(sufficientResources, converter.HasEnoughMassToStartConverting());
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		sufficientResources = new Operational.Flag("sufficientResources", operationalReq);
		Subscribe(-1697596308, onStorageChanged);
		onStorageChanged(null);
	}
}
