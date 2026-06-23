public class Blueprints
{
	public BlueprintCollection all = new BlueprintCollection();

	public BlueprintCollection skinsRelease = new BlueprintCollection();

	public BlueprintProvider[] skinsReleaseProviders = new BlueprintProvider[8]
	{
		new Blueprints_U51AndBefore(),
		new Blueprints_DlcPack2(),
		new Blueprints_U53(),
		new Blueprints_DlcPack3(),
		new Blueprints_DlcPack4(),
		new Blueprints_U57(),
		new Blueprints_CosmeticPack1(),
		new Blueprints_DlcPack5()
	};

	private static Blueprints instance;

	public static Blueprints Get()
	{
		if (instance == null)
		{
			instance = new Blueprints();
			instance.all.AddBlueprintsFrom(new Blueprints_Default());
			BlueprintProvider[] array = instance.skinsReleaseProviders;
			foreach (BlueprintProvider provider in array)
			{
				instance.skinsRelease.AddBlueprintsFrom(provider);
			}
			instance.all.AddBlueprintsFrom(instance.skinsRelease);
			instance.skinsRelease.PostProcess();
			instance.all.PostProcess();
		}
		return instance;
	}
}
