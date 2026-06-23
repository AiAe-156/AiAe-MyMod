public interface IDiningSeat
{
	bool HasGarnish { get; }

	HashedString EatAnim { get; }

	HashedString ReloadElectrobankAnim { get; }

	KPrefabID Diner { get; set; }

	Storage FindStorage();

	Operational FindOperational();
}
