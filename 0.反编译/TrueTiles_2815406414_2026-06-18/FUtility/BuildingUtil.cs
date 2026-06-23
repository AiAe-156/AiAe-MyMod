using TUNING;

namespace FUtility;

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
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return CreateTileDef(ID, anim, new float[1] { constructionMass }, new string[1] { material }, decor, transparent);
	}

	public static BuildingDef CreateTileDef(string ID, string anim, float[] constructionMass, string[] material, EffectorValues decor, bool transparent)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		BuildingDef obj = BuildingTemplates.CreateBuildingDef(ID, 1, 1, anim + "_kanim", 30, 30f, constructionMass, material, 3200f, (BuildLocationRule)6, decor, NOISE_POLLUTION.NONE, 0.2f);
		BuildingTemplates.CreateFoundationTileDef(obj);
		obj.Floodable = false;
		obj.Overheatable = false;
		obj.Entombable = false;
		obj.UseStructureTemperature = false;
		obj.AudioCategory = "Glass";
		obj.AudioSize = "small";
		obj.BaseTimeUntilRepair = -1f;
		obj.SceneLayer = (SceneLayer)(transparent ? 27 : 30);
		obj.isKAnimTile = true;
		obj.BlockTileIsTransparent = transparent;
		obj.BlockTileMaterial = Assets.GetMaterial("tiles_solid");
		return obj;
	}
}
