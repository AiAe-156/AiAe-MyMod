public class HatListable : IListableOption
{
	public string name { get; private set; }

	public string hat { get; private set; }

	public HatListable(string name, string hat)
	{
		this.name = name;
		this.hat = hat;
	}

	public string GetProperName()
	{
		return name;
	}
}
