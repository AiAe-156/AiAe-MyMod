using TUNING;

namespace UtilLibs;

public static class BuildingUtil
{
	public static void AddToResearch(string ID, string tech)
	{
		if (!Util.IsNullOrWhiteSpace(tech))
		{
			((ResourceSet<Tech>)(object)Db.Get().Techs).Get(tech).unlockedItemIDs.Add(ID);
		}
	}

	public static BuildingDef CreateTileDef(string ID, string anim, float constructionMass, string material, EffectorValues decor, bool transparent)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return CreateTileDef(ID, anim, new float[1] { constructionMass }, new string[1] { material }, decor, transparent);
	}

	public static BuildingDef CreateTileDef(string ID, string anim, float[] constructionMass, string[] material, EffectorValues decor, bool transparent)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		BuildingDef val = BuildingTemplates.CreateBuildingDef(ID, 1, 1, anim, 30, 30f, constructionMass, material, 3200f, (BuildLocationRule)6, decor, NOISE_POLLUTION.NONE, 0.2f);
		BuildingTemplates.CreateFoundationTileDef(val);
		val.Floodable = false;
		val.Overheatable = false;
		val.Entombable = false;
		val.UseStructureTemperature = false;
		val.AudioCategory = "Glass";
		val.AudioSize = "small";
		val.BaseTimeUntilRepair = -1f;
		val.SceneLayer = (SceneLayer)(transparent ? 27 : 30);
		val.isKAnimTile = true;
		val.BlockTileIsTransparent = transparent;
		val.BlockTileMaterial = Assets.GetMaterial("tiles_solid");
		return val;
	}
}
